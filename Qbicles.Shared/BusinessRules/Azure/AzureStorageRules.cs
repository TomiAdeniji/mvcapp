using Amazon.S3.Model;
using Azure.Storage.Blobs.Models;
using FFMpegSharp;
using FFMpegSharp.Enums;
using FFMpegSharp.FFMPEG;
using FFMpegSharp.FFMPEG.Enums;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.AWS.EXIF;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.FileStorage;
using Storage.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WebPWrapper;

namespace Qbicles.BusinessRules.Azure
{
    public class AzureStorageRules
    {
        private ApplicationDbContext dbContext;

        /// <summary>
        /// Initialise constructor
        /// </summary>
        /// <param name="context"></param>
        public AzureStorageRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        private async Task<QbicleJobResult> VerifyHangfireJobState(string jobId)
        {
            try
            {
                var job = new QbicleJobParameter
                {
                    EndPointName = "verifyhangfirejobstate",
                    JobId = jobId
                };
                //execute SignalR2Activity
                var jobExecuted = await new QbiclesJob().HangFireExcecuteAsync(job);

                return jobExecuted;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, jobId);
                return null;
            }
        }

        /// <summary>
        /// Process media upload with hangfire
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="isPublic"></param>
        /// <param name="reminderMinutes"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangfireProcessMediaUpload(string objectKey, bool isPublic, double reminderMinutes = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(objectKey))
                    return null;

                var activityNotify = new ActivityNotification
                {
                    S3ObjectUploadedItem = new AwsS3ObjectItem
                    {
                        ObjectKey = objectKey,
                        IsPublic = isPublic
                    }
                };

                var job = new QbicleJobParameter
                {
                    EndPointName = "awss3fileuploadprocess",
                    ActivityNotification = activityNotify,
                    ReminderMinutes = reminderMinutes
                };

                //execute SignalR2Activity
                var jobExecuted = await new QbiclesJob().HangFireExcecuteAsync(job);

                return jobExecuted;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, objectKey);
                return null;
            }
        }

        /// <summary>
        /// Get media from s3 and process( convert, resize)
        /// Save to Storage
        /// </summary>
        /// <param name="job"></param>
        public async Task AwsS3FileUploadProcessAsync(QbicleJobParameter job)
        {
            try
            {
                var fileUploaded = job.ActivityNotification.S3ObjectUploadedItem;

                //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileUploaded.ObjectKey);

                //if (dbContext.StorageFiles.Any(e => e.Id == s3Object.ObjectKey))
                //    return;
                if ((await dbContext.StorageFiles.FirstOrDefaultAsync(e => e.Id == s3Object.ObjectKey)) != null)
                    return;

                s3Object.IsPublic = fileUploaded.IsPublic;

                // Store a record of the file for later use in retrieving the document
                var storageOrigin = new StorageFile
                {
                    Id = s3Object.ObjectKey,
                    Name = s3Object.ObjectName ?? s3Object.ObjectKey,
                    Size = s3Object.ObjectSize,
                    Path = s3Object.ObjectKey,
                    IsPublic = s3Object.IsPublic
                };

                dbContext.Entry(storageOrigin).State = System.Data.Entity.EntityState.Added;

                var fileType = s3Object.ObjectType ?? s3Object.ObjectContentType;
                var imageTypes = new List<string> { "octet-stream", "webp", "bmp", "gif", "png", "jpg", "tiff", "tif", "jpeg" };

                if (imageTypes.Contains(s3Object.Extension))
                    await ImageProcessAsync(s3Object, storageOrigin);
                else if (fileType.StartsWith("video"))
                    await VideoProcessAsync(s3Object);
                else
                    dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, "hangfire", job);
            }
        }

        //Upload qbicles doc use S3

        public async Task ImageProcessAsync(S3ObjectModel s3Object, StorageFile storageOrigin)
        {
            //byte[] imageByte = null;

            var originalWidth = 0;
            var originalHeight = 0;
            var tempPathRepository = ConfigManager.TempPathRepository;

            var fullPathOrigin = Path.Combine(tempPathRepository, s3Object.ObjectKey);

            using (FileStream outputFileStream = new FileStream(fullPathOrigin, FileMode.Create))
            {
                await s3Object.ObjectStream.CopyToAsync(outputFileStream);
            }

            if (s3Object.Extension != "webp")
            {
                //rotale origin image and upload replace
                var imageByte = RotatedImage.OrientExifRotatedImage(s3Object, fullPathOrigin);
                //save image to webp format
                using (WebP webp = new WebP())
                using (var ms = new MemoryStream(imageByte))
                    webp.Save(new Bitmap(ms), fullPathOrigin, 100);
            }

            //get image width/height

            using (WebP webp = new WebP())
            {
                var imageBitmap = webp.Load(fullPathOrigin);
                originalWidth = imageBitmap.Width;
                originalHeight = imageBitmap.Height;
            }

            var mediaProcess = new MediaProcess
            {
                FileName = s3Object.ObjectName,
                ObjectKey = s3Object.ObjectKey,
                FilePath = fullPathOrigin
            };

            await UploadMediaFromPathByHangfireAsync(mediaProcess);

            //process and create 3 files MST
            var fileDetailList = new List<StorageFileDetail>();

            //if originalWidth < medium image width then not process resize image
            //not upload new file MST to S3, use path of origin image
            if (originalWidth <= int.Parse(ConfigManager.MediumImageWidth))
            {
                fileDetailList = new List<StorageFileDetail>
                {
                    new StorageFileDetail()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = storageOrigin.Name,
                        Size = storageOrigin.Size,
                        Path = storageOrigin.Path,
                        Extension = "T",
                        StorageFile = storageOrigin.Id
                    },
                    new StorageFileDetail()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = storageOrigin.Name,
                        Size = storageOrigin.Size,
                        Path = storageOrigin.Path,
                        Extension = "S",
                        StorageFile = storageOrigin.Id
                    },
                    new StorageFileDetail()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = storageOrigin.Name,
                        Size = storageOrigin.Size,
                        Path = storageOrigin.Path,
                        Extension = "M",
                        StorageFile = storageOrigin.Id
                    }
                };
                dbContext.StorageFileDetails.AddRange(fileDetailList);

                dbContext.SaveChanges();

                return;
            }

            //process files create 3 file MST

            var targetWidthT = int.Parse(ConfigManager.ThumbnailImageWidth);
            var targetWidthS = int.Parse(ConfigManager.SmallImageWidth);
            var targetWidthM = int.Parse(ConfigManager.MediumImageWidth);

            // file T
            var targetHeight = originalHeight * targetWidthT / originalWidth;
            var objectKey = Guid.NewGuid().ToString();
            ResizeImage(objectKey, targetWidthT, targetHeight, fullPathOrigin, tempPathRepository);

            mediaProcess = new MediaProcess
            {
                FileName = s3Object.ObjectName,
                ObjectKey = objectKey,
                FilePath = Path.Combine(tempPathRepository, objectKey)
            };
            var contentLength = await UploadMediaFromPathByHangfireAsync(mediaProcess);
            File.Delete(mediaProcess.FilePath);

            fileDetailList.Add(new StorageFileDetail
            {
                Id = objectKey,
                Name = s3Object.ObjectName,
                Size = HelperClass.FileSize((int)contentLength),
                Path = objectKey,
                Extension = "T",
                StorageFile = s3Object.ObjectKey
            });

            // file S
            targetHeight = originalHeight * targetWidthS / originalWidth;
            objectKey = Guid.NewGuid().ToString();
            ResizeImage(objectKey, targetWidthS, targetHeight, fullPathOrigin, tempPathRepository);

            mediaProcess = new MediaProcess
            {
                FileName = s3Object.ObjectName,
                ObjectKey = objectKey,
                FilePath = Path.Combine(tempPathRepository, objectKey)
            };
            contentLength = await UploadMediaFromPathByHangfireAsync(mediaProcess);
            File.Delete(mediaProcess.FilePath);

            fileDetailList.Add(new StorageFileDetail
            {
                Id = objectKey,
                Name = s3Object.ObjectName,
                Size = HelperClass.FileSize((int)contentLength),
                Path = objectKey,
                Extension = "S",
                StorageFile = s3Object.ObjectKey
            });

            // file M
            targetHeight = originalHeight * targetWidthM / originalWidth;
            objectKey = Guid.NewGuid().ToString();
            ResizeImage(objectKey, targetWidthM, targetHeight, fullPathOrigin, tempPathRepository);

            mediaProcess = new MediaProcess
            {
                FileName = s3Object.ObjectName,
                ObjectKey = objectKey,
                FilePath = Path.Combine(tempPathRepository, objectKey)
            };
            contentLength = await UploadMediaFromPathByHangfireAsync(mediaProcess);
            File.Delete(mediaProcess.FilePath);

            fileDetailList.Add(new StorageFileDetail
            {
                Id = objectKey,
                Name = s3Object.ObjectName,
                Size = HelperClass.FileSize((int)contentLength),
                Path = objectKey,
                Extension = "M",
                StorageFile = s3Object.ObjectKey
            });

            File.Delete(fullPathOrigin);
            dbContext.StorageFileDetails.AddRange(fileDetailList);

            dbContext.SaveChanges();
        }

        /// <summary>
        /// Resize image
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="filePathSource"></param>
        /// <param name="tempPathRepository"></param>
        private static void ResizeImage(string objectKey, int width, int height, string filePathSource, string tempPathRepository)//, string extension)
        {
            byte[] rawWebP = File.ReadAllBytes(filePathSource);
            using (WebP webp = new WebP())
            {
                var bmp = webp.GetThumbnailQuality(rawWebP, width, height);
                string lossyFileName = Path.Combine(tempPathRepository, objectKey);
                webp.Save(bmp, lossyFileName, 100);
            }
        }

        private async Task VideoProcessAsync(S3ObjectModel s3Object)
        {
            var fileDetailList = new List<StorageFileDetail>();
            //write file to temp folder
            var tempPathRepository = ConfigManager.TempPathRepository;

            string fullPathWithGuid = Path.Combine(tempPathRepository, s3Object.ObjectKey);
            using (FileStream outputFileStream = new FileStream(fullPathWithGuid, FileMode.Create))
            {
                await s3Object.ObjectStream.CopyToAsync(outputFileStream);
            }

            //process
            var encoder = new FFMpeg();

            var video = VideoInfo.FromPath(fullPathWithGuid);
            //thumb image
            var fileGuidDetail = Guid.NewGuid().ToString();
            fullPathWithGuid = $@"{tempPathRepository}{fileGuidDetail}.png";
            var outputThumb = new FileInfo(fullPathWithGuid);
            var fileNameOnly = Path.GetFileNameWithoutExtension(s3Object.ObjectName).ToLower();

            #region Create and save file Thumb

            encoder
                .Snapshot(
                    video,
                    outputThumb,
                    new Size(600, 375),
                    TimeSpan.FromSeconds(1)
                );
            //upload file to s3 with file path
            var mediaProcess = new MediaProcess
            {
                FileName = $"{fileNameOnly}.png",
                ObjectKey = fileGuidDetail,
                FilePath = fullPathWithGuid,
            };
            var contentLength = await UploadMediaFromPathByHangfireAsync(mediaProcess);

            //delete file uploaded
            File.Delete(fullPathWithGuid);

            fileDetailList.Add(new StorageFileDetail()
            {
                Id = fileGuidDetail,
                Name = s3Object.ObjectName,
                Size = HelperClass.FileSize((int)contentLength),
                Path = fileGuidDetail,
                Extension = "png",
                StorageFile = s3Object.ObjectKey
            });

            #endregion Create and save file Thumb

            #region MP4 conversion

            fileGuidDetail = Guid.NewGuid().ToString();
            fullPathWithGuid = $@"{tempPathRepository}{fileGuidDetail}.mp4";
            var outputFile = new FileInfo(fullPathWithGuid);
            encoder.Convert(
                video,
                outputFile,
                VideoType.Mp4,
                Speed.UltraFast,
                VideoSize.Original,
                AudioQuality.Hd,
                true
            );

            // upload file to s3 with file path
            mediaProcess = new MediaProcess
            {
                FileName = $"{fileNameOnly}.mp4",
                ObjectKey = fileGuidDetail,
                FilePath = fullPathWithGuid,
            };
            contentLength = await UploadMediaFromPathByHangfireAsync(mediaProcess);

            //delete file uploaded
            File.Delete(fullPathWithGuid);

            fileDetailList.Add(new StorageFileDetail()
            {
                Id = fileGuidDetail,
                Name = s3Object.ObjectName,
                Size = HelperClass.FileSize((int)contentLength),
                Path = fileGuidDetail,
                Extension = "mp4",
                StorageFile = s3Object.ObjectKey
            });

            #endregion MP4 conversion

            #region WEBM conversion

            fileGuidDetail = Guid.NewGuid().ToString();
            fullPathWithGuid = $@"{tempPathRepository}{fileGuidDetail}.webm";
            outputFile = new FileInfo(fullPathWithGuid);
            encoder.Convert(
                video,
                outputFile,
                VideoType.WebM,
                Speed.UltraFast,
                VideoSize.Original,
                AudioQuality.Hd,
                true
            );

            // upload file to s3 with file path
            mediaProcess = new MediaProcess
            {
                FileName = $"{fileNameOnly}.webm",
                ObjectKey = fileGuidDetail,
                FilePath = fullPathWithGuid,
            };
            contentLength = await UploadMediaFromPathByHangfireAsync(mediaProcess);

            //delete file uploaded
            File.Delete(fullPathWithGuid);

            fileDetailList.Add(new StorageFileDetail()
            {
                Id = fileGuidDetail,
                Name = s3Object.ObjectName,
                Size = HelperClass.FileSize((int)contentLength),
                Path = fileGuidDetail,
                Extension = "webm",
                StorageFile = s3Object.ObjectKey
            });

            #endregion WEBM conversion

            #region OGV conversion

            fileGuidDetail = Guid.NewGuid().ToString();
            fullPathWithGuid = $@"{tempPathRepository}{fileGuidDetail}.ogv";
            outputFile = new FileInfo(fullPathWithGuid);
            encoder.Convert(
                video,
                outputFile,
                VideoType.Ogv,
                Speed.UltraFast,
                VideoSize.Original,
                AudioQuality.Hd,
                true
            );
            // upload file to s3 with file path
            mediaProcess = new MediaProcess
            {
                FileName = $"{fileNameOnly}.ogv",
                ObjectKey = fileGuidDetail,
                FilePath = fullPathWithGuid,
            };
            contentLength = await UploadMediaFromPathByHangfireAsync(mediaProcess);

            //delete file uploaded
            File.Delete(fullPathWithGuid);

            fileDetailList.Add(new StorageFileDetail()
            {
                Id = fileGuidDetail,
                Name = s3Object.ObjectName,
                Size = HelperClass.FileSize((int)contentLength),
                Path = fileGuidDetail,
                Extension = "ovg",
                StorageFile = s3Object.ObjectKey
            });

            dbContext.StorageFileDetails.AddRange(fileDetailList);

            dbContext.SaveChanges();

            #endregion OGV conversion
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public async Task<long> UploadMediaFromPathByHangfireAsync(MediaProcess media)
        {
            //Initalize the storage settings
            var blobRepository = new BlobStorageRepository(AzureStorageHelper.GetBlobStorageSettings());

            // Get a reference to the blob
            var blockBlobClient = blobRepository.GetBlockBlobClient(media.ObjectKey);

            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            var contentType = System.Web.MimeMapping.GetMimeMapping(media.FileName);

            // Set metadata for the uploaded file
            var metadata = new Dictionary<string, string>
            {
                { "file-name", media.FileName },
                { "file-type", contentType.Split('/')[0] },
                { "file-extension", contentType.Split('/')[1] }
            };

            //verify upload file or file path to use path or byte etc...
            // 2. Upload video in small Parts of 5 MB.
            var fileInfo = new FileInfo(media.FilePath);
            long contentLength = fileInfo.Length;
            int partSize = 5 * (int)Math.Pow(2, 20); // 5 MB

            // List to keep track of block IDs
            List<string> blockIds = new List<string>();

            try
            {
                long filePosition = 0;
                for (int index = 1; filePosition < contentLength; index++)
                {
                    // Read file in chunks and upload each block
                    byte[] buffer = new byte[partSize];

                    // Read a chunk of the file
                    int bytesRead = await media.MemoryStream.ReadAsync(buffer, 0, partSize);

                    // Generate a unique block ID (Base64 encoded)
                    string blockId = Convert.ToBase64String(BitConverter.GetBytes(index));
                    blockIds.Add(blockId);

                    // Upload the block
                    using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
                    {
                        await blockBlobClient.StageBlockAsync(blockId, ms);
                    }

                    //Increment
                    filePosition += partSize;
                }

                // Step 3: Complete - commit the blocks to finalize the upload
                await blockBlobClient.CommitBlockListAsync(blockIds, new BlobHttpHeaders { ContentType = contentType }, metadata);

                return contentLength;
            }
            catch (Exception exception)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), exception, null, media);

                // Delete the blob if there is an error during the upload process
                await blockBlobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                return 0;
            }
        }

        /// <summary>
        /// Upload a media file to Azure Blob storage using FileStream for file handling.
        /// </summary>
        /// <param name="media"></param>
        /// <param name="applicationFile"></param>
        /// <returns></returns>
        public async Task<long> UploadMediaFromPathByQbicleAsync(MediaProcess media, bool applicationFile = false)
        {
            //Initalize the storage settings
            var blobRepository = new BlobStorageRepository(AzureStorageHelper.GetBlobStorageSettings());

            // Get a reference to the blob
            var blockBlobClient = blobRepository.GetBlockBlobClient(media.ObjectKey);

            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            var contentType = System.Web.MimeMapping.GetMimeMapping(media.FileName);

            // List to keep track of block IDs
            List<string> blockIds = new List<string>();

            // track the current block number as the code iterates through the file
            int blockIndex = 0;

            // Set metadata for the uploaded file
            var metadata = new Dictionary<string, string>
            {
                { "fileName", media.FileName },
                { "fileType", contentType?.Split('/')[0] }
            };

            if (!applicationFile)
            {
                metadata.Add("fileExtension", contentType.Split('/')[1]);
            }
            else
            {
                metadata.Add("fileExtension", Path.GetExtension(media.FileName).Replace(".", ""));
            }

            //verify upload file or file path to use path or byte etc...
            // 2. Upload video in small Parts of 5 MB.
            var fileInfo = new FileInfo(media.FilePath);
            long contentLength = fileInfo.Length;
            int partSize = 5 * (int)Math.Pow(2, 20); // 5 MB

            try
            {
                long totalBytesRead = 0;

                // Open the file using a FileStream to handle large files efficiently
                using (FileStream fileStream = new FileStream(media.FilePath, FileMode.Open, FileAccess.Read))
                {
                    // Create a buffer to store file chunks
                    byte[] buffer = new byte[partSize];

                    // local variable to track the current number of bytes read into buffer
                    int bytesRead;

                    // Read file in chunks and upload each block
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // Generate a unique block ID (Base64 encoded)
                        string blockId = Convert.ToBase64String(BitConverter.GetBytes(blockIndex));
                        blockIds.Add(blockId);

                        // Upload the block using a MemoryStream created from the buffer
                        using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
                        {
                            await blockBlobClient.StageBlockAsync(blockId, ms);
                        }

                        // Update counters
                        totalBytesRead += bytesRead;
                        blockIndex++;
                    }
                }

                // Commit the blocks to finalize the upload
                await blockBlobClient.CommitBlockListAsync(blockIds, new BlobHttpHeaders { ContentType = contentType }, metadata);

                // Read file in chunks and upload each block
                //byte[] buffer = new byte[partSize];

                //while (true)
                //{
                //    // Read a chunk of the file
                //    bytesRead = await media.MemoryStream.ReadAsync(buffer, 0, partSize);
                //    if (bytesRead == 0) break; // Exit loop when all bytes are read

                //    // Generate a unique block ID (Base64 encoded)
                //    string blockId = Convert.ToBase64String(BitConverter.GetBytes(blockIndex));
                //    blockIds.Add(blockId);

                //    // Upload the block
                //    using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
                //    {
                //        await blockBlobClient.StageBlockAsync(blockId, ms);
                //    }

                //    //Increment
                //    blockIndex++;
                //}

                // Commit the blocks to finalize the upload
                //await blockBlobClient.CommitBlockListAsync(blockIds, new BlobHttpHeaders { ContentType = contentType }, metadata);

                // Notify after upload completion
                var activityNotify = new ActivityNotification
                {
                    S3ObjectUploadedItem = new AwsS3ObjectItem
                    {
                        ObjectKey = media.ObjectKey,
                        IsPublic = media.IsPublic
                    }
                };

                //
                var job = new QbicleJobParameter
                {
                    ActivityNotification = activityNotify
                };

                //
                await AwsS3FileUploadProcessAsync(job);

                return totalBytesRead;
            }
            catch (Exception exception)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), exception, null, media);

                // Delete the blob if there is an error during the upload process
                await blockBlobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                return 0;
            }
        }

        /// <summary>
        /// Processing media
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="isPublic">Default is false</param>
        public void ProcessingMediaS3(string objectKey, bool isPublic = false)
        {
            if (!string.IsNullOrEmpty(objectKey))
            {
                Task tskHangfire = new Task(async () =>
                {
                    await HangfireProcessMediaUpload(objectKey, isPublic);
                });
                tskHangfire.Start();
            }
        }
    }
}