using Amazon.S3;
using Amazon.S3.Model;
using FFMpegSharp;
using FFMpegSharp.Enums;
using FFMpegSharp.FFMPEG;
using FFMpegSharp.FFMPEG.Enums;
using Qbicles.BusinessRules.AWS.EXIF;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.FileStorage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebPWrapper;

namespace Qbicles.BusinessRules.AWS
{
    public class AwsS3BucketRules
    {
        private ApplicationDbContext dbContext;

        public AwsS3BucketRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

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

                var awsS3Bucket = AwsS3BucketHelper.AwsS3Client();
                var s3Object = await AwsS3BucketHelper.ReadS3ObjectDataAsync(fileUploaded.ObjectKey, awsS3Bucket);

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
                    await ImageProcessAsync(awsS3Bucket, s3Object, storageOrigin);
                else if (fileType.StartsWith("video"))
                    await VideoProcessAsync(awsS3Bucket, s3Object);
                else
                    dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, "hangfire", job);
            }
        }

        //Upload qbicles doc use S3

        public async Task ImageProcessAsync(AwsS3Bucket awsS3Bucket, S3ObjectModel s3Object, StorageFile storageOrigin)
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
            await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);

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
            var contentLength = await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);
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
            contentLength = await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);
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
            contentLength = await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);
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

        private async Task VideoProcessAsync(AwsS3Bucket awsS3Bucket, S3ObjectModel s3Object)
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
            var contentLength = await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);

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
            contentLength = await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);

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
            contentLength = await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);

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
            contentLength = await UploadMediaFromPathByHangfireAsync(awsS3Bucket, mediaProcess);

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

        public async Task<long> UploadMediaFromPathByHangfireAsync(AwsS3Bucket awsS3Bucket, MediaProcess media)
        {
            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            var contentType = System.Web.MimeMapping.GetMimeMapping(media.FileName);

            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = awsS3Bucket.BucketName,
                Key = media.ObjectKey,
                ContentType = contentType,
                CannedACL = S3CannedACL.NoACL
            };
            initiateRequest.Metadata.Add("file-name", media.FileName);
            initiateRequest.Metadata.Add("file-type", contentType.Split('/')[0]);
            initiateRequest.Metadata.Add("file-extension", contentType.Split('/')[1]);

            InitiateMultipartUploadResponse initResponse = await awsS3Bucket.AwsS3Client.InitiateMultipartUploadAsync(initiateRequest);

            //verify upload file or file path to use path or byte etc...
            // 2. Upload video in small Parts of 5 MB.
            var fileInfo = new FileInfo(media.FilePath);
            long contentLength = fileInfo.Length;
            long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB

            try
            {
                long filePosition = 0;
                for (int index = 1; filePosition < contentLength; index++)
                {
                    UploadPartRequest uploadRequest = new UploadPartRequest
                    {
                        BucketName = awsS3Bucket.BucketName,
                        Key = media.ObjectKey,
                        UploadId = initResponse.UploadId,
                        PartNumber = index,
                        PartSize = partSize,
                        FilePosition = filePosition,
                        FilePath = media.FilePath
                    };
                    // Upload part and add response to our list.
                    uploadResponses.Add(await awsS3Bucket.AwsS3Client.UploadPartAsync(uploadRequest));

                    filePosition += partSize;
                }

                // Step 3: complete.
                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = awsS3Bucket.BucketName,
                    Key = media.ObjectKey,
                    UploadId = initResponse.UploadId,
                    //PartETags = new List<PartETag>(uploadResponses)
                };
                completeRequest.AddPartETags(uploadResponses);

                var completeUploadResponse = await awsS3Bucket.AwsS3Client.CompleteMultipartUploadAsync(completeRequest);
                return contentLength;
            }
            catch (Exception exception)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), exception, null, awsS3Bucket, media);
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = awsS3Bucket.BucketName,
                    Key = media.ObjectKey,
                    UploadId = initResponse.UploadId
                };
                await awsS3Bucket.AwsS3Client.AbortMultipartUploadAsync(abortMPURequest);
                return 0;
            }
        }

        public async Task<long> UploadMediaFromPathByQbicleAsync(AwsS3Bucket awsS3Bucket, MediaProcess media, bool applicationFile = false)
        {
            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            var contentType = System.Web.MimeMapping.GetMimeMapping(media.FileName);
            //var filePath = "";
            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = awsS3Bucket.BucketName,
                Key = media.ObjectKey,
                ContentType = contentType,
                CannedACL = S3CannedACL.NoACL
            };
            initiateRequest.Metadata.Add("file-name", media.FileName);
            initiateRequest.Metadata.Add("file-type", contentType.Split('/')[0]);
            if (!applicationFile)
            {
                initiateRequest.Metadata.Add("file-extension", contentType.Split('/')[1]);
                //filePath = $"{media.FilePath}.{Path.GetExtension(media.FilePath)}";
            }
            else
            {
                initiateRequest.Metadata.Add("file-extension", Path.GetExtension(media.FileName).Replace(".", ""));
                //filePath = media.FilePath;
            }
            InitiateMultipartUploadResponse initResponse = await awsS3Bucket.AwsS3Client.InitiateMultipartUploadAsync(initiateRequest);

            //verify upload file or file path to use path or byte etc...
            // 2. Upload video in small Parts of 5 MB.
            var fileInfo = new FileInfo(media.FilePath);
            long contentLength = fileInfo.Length;
            long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB

            try
            {
                long filePosition = 0;
                for (int index = 1; filePosition < contentLength; index++)
                {
                    UploadPartRequest uploadRequest = new UploadPartRequest
                    {
                        BucketName = awsS3Bucket.BucketName,
                        Key = media.ObjectKey,
                        UploadId = initResponse.UploadId,
                        PartNumber = index,
                        PartSize = partSize,
                        FilePosition = filePosition,
                        FilePath = media.FilePath
                    };
                    // Upload part and add response to our list.
                    uploadResponses.Add(await awsS3Bucket.AwsS3Client.UploadPartAsync(uploadRequest));

                    filePosition += partSize;
                }

                // Step 3: complete.
                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = awsS3Bucket.BucketName,
                    Key = media.ObjectKey,
                    UploadId = initResponse.UploadId,
                    //PartETags = new List<PartETag>(uploadResponses)
                };
                completeRequest.AddPartETags(uploadResponses);

                var completeUploadResponse = await awsS3Bucket.AwsS3Client.CompleteMultipartUploadAsync(completeRequest);

                var activityNotify = new ActivityNotification
                {
                    S3ObjectUploadedItem = new AwsS3ObjectItem
                    {
                        ObjectKey = media.ObjectKey,
                        IsPublic = media.IsPublic
                    }
                };
                var job = new QbicleJobParameter
                {
                    ActivityNotification = activityNotify
                };
                await AwsS3FileUploadProcessAsync(job);

                return contentLength;
            }
            catch (Exception exception)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), exception, null, awsS3Bucket, media);
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = awsS3Bucket.BucketName,
                    Key = media.ObjectKey,
                    UploadId = initResponse.UploadId
                };
                await awsS3Bucket.AwsS3Client.AbortMultipartUploadAsync(abortMPURequest);
                return 0;
            }
        }

        /// <summary>
        ///
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