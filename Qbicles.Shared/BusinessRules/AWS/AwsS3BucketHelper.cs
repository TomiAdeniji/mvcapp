//using Amazon;
//using Amazon.S3;
//using Amazon.S3.Model;
//using Qbicles.BusinessRules.Helper;
//using Qbicles.BusinessRules.Model;
//using System;
//using System.IO;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace Qbicles.BusinessRules.AWS
//{
//    public static class AwsS3BucketHelper
//    {
//        public static AwsS3Bucket AwsS3Client()
//        {
//            var awsS3Bucket = new AwsS3Bucket
//            {
//                AccessKey = ConfigManager.AccessKey,
//                BucketName = ConfigManager.BucketName,
//                BucketRegion = ConfigManager.BucketRegion,
//                IdentityPoolId = ConfigManager.IdentityPoolId,
//                SecretKey = ConfigManager.SecretKey
//            };

//            var bucketRegion = RegionEndpoint.GetBySystemName(awsS3Bucket.BucketRegion);
//            awsS3Bucket.AwsS3Client = new AmazonS3Client(awsS3Bucket.AccessKey, awsS3Bucket.SecretKey, bucketRegion);

//            return awsS3Bucket;
//        }

//        public static string SignedUrl(AwsS3Bucket awsS3Bucket, string keyName, string filename = "")
//        {
//            var request = new GetPreSignedUrlRequest
//            {
//                BucketName = awsS3Bucket.BucketName,
//                Key = keyName,
//                Expires = DateTime.Now.AddDays(1),
//                Protocol = Protocol.HTTPS
//            };

//            if (!string.IsNullOrEmpty(filename))
//                request.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename={filename}";

//            return awsS3Bucket.AwsS3Client.GetPreSignedURL(request);
//        }

//        public static bool CheckExistedObject(string objectKey)
//        {
//            var s3Client = AwsS3Client();
//            var s3FileInfo = new Amazon.S3.IO.S3FileInfo(s3Client.AwsS3Client, s3Client.BucketName, objectKey);
//            return s3FileInfo.Exists;
//        }

//        /// <summary>
//        /// Get URI of keyName, if not found return URI of origin file
//        /// Use in the case get URI by file size - case get URI file size but hangfire processing file not completed yet
//        /// </summary>
//        /// <param name="objectKey">object key to get uri</param>
//        /// <returns></returns>
//        public static string SignedUrl(string objectKey, string filename = "")
//        {
//            var awsS3Bucket = AwsS3Client();

//            var request = new GetPreSignedUrlRequest
//            {
//                BucketName = awsS3Bucket.BucketName,
//                Key = objectKey,
//                Expires = DateTime.UtcNow.AddMinutes(10)
//            };

//            if (!string.IsNullOrEmpty(filename))
//                request.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename={filename}";

//            return awsS3Bucket.AwsS3Client.GetPreSignedURL(request);
//        }

//        public static async Task<Stream> ReadSqLiteFileAsync(string objKey)
//        {
//            try
//            {
//                var awsS3Bucket = AwsS3Client();
//                var request = new GetObjectRequest
//                {
//                    BucketName = awsS3Bucket.BucketName,
//                    Key = objKey
//                };

//                using (var response = await awsS3Bucket.AwsS3Client.GetObjectAsync(request))
//                using (var responseStream = response.ResponseStream)
//                {
//                    var memoryStream = new MemoryStream();
//                    responseStream.CopyTo(memoryStream);
//                    memoryStream.Position = 0;
//                    return memoryStream;
//                }
//            }
//            catch (Exception exception)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), exception);
//                return null;
//            }
//        }

//        public static async Task<S3ObjectModel> ReadS3ObjectDataAsync(string objKey)
//        {
//            try
//            {
//                var awsS3Bucket = AwsS3Client();
//                var request = new GetObjectRequest
//                {
//                    BucketName = awsS3Bucket.BucketName,
//                    Key = objKey
//                };

//                using (var response = awsS3Bucket.AwsS3Client.GetObject(request))
//                using (var responseStream = response.ResponseStream)
//                {
//                    var memoryStream = new MemoryStream();
//                    await responseStream.CopyToAsync(memoryStream);
//                    memoryStream.Position = 0;
//                    //return memoryStream;

//                    string fileName = response.Metadata["x-amz-meta-file-name"]; // have "file-name" as medata added to the object.
//                    string fileType = response.Metadata["x-amz-meta-file-type"]; // have "file-type" as medata added to the object.
//                    string fileExtension = response.Metadata["x-amz-meta-file-extension"]; // have "file-extension" as medata added to the object.
//                    string contentType = response.Headers["Content-Type"];

//                    if (fileType == null)
//                    {
//                        try
//                        {
//                            fileType = contentType.Split('/')[0];
//                            fileExtension = contentType.Split('/')[1];
//                        }
//                        catch
//                        {
//                            fileType = "application";
//                        }
//                    }

//                    return new S3ObjectModel
//                    {
//                        ObjectName = fileName,
//                        ObjectType = fileType,
//                        ObjectContentType = contentType,
//                        ObjectStream = memoryStream,
//                        Extension = fileExtension
//                    };
//                }
//            }
//            catch (Exception exception)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), exception);
//                return new S3ObjectModel
//                {
//                    ObjectName = "",
//                    ObjectType = "",
//                    ObjectContentType = "",
//                    ObjectStream = null
//                };
//            }
//        }

//        public static async Task<S3ObjectModel> ReadS3ObjectDataAsync(string objKey, AwsS3Bucket awsS3)
//        {
//            try
//            {
//                var request = new GetObjectRequest
//                {
//                    BucketName = awsS3.BucketName,
//                    Key = objKey
//                };

//                using (var response = await awsS3.AwsS3Client.GetObjectAsync(request))
//                using (var responseStream = response.ResponseStream)
//                {
//                    var memoryStream = new MemoryStream();
//                    await responseStream.CopyToAsync(memoryStream);
//                    memoryStream.Position = 0;
//                    //return memoryStream;

//                    string fileName = response.Metadata["x-amz-meta-file-name"]; // have "file-name" as medata added to the object.
//                    string fileType = response.Metadata["x-amz-meta-file-type"]; // have "file-type" as medata added to the object.
//                    string fileExtension = response.Metadata["x-amz-meta-file-extension"]; // have "file-extension" as metadata added to the object.
//                    string contentType = response.Headers["Content-Type"];

//                    if (fileType == null)
//                    {
//                        try
//                        {
//                            fileType = contentType.Split('/')[0];
//                            fileExtension = contentType.Split('/')[1];
//                        }
//                        catch
//                        {
//                            fileType = "application";
//                        }
//                    }

//                    return new S3ObjectModel
//                    {
//                        ObjectKey = objKey,
//                        ObjectName = fileName ?? objKey,
//                        ObjectType = fileType,
//                        ObjectContentType = contentType,
//                        ObjectSize = HelperClass.FileSize((int)memoryStream.Length),
//                        ObjectStream = memoryStream,
//                        Extension = fileExtension
//                    };
//                }
//            }
//            catch (Exception exception)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), exception);
//                return new S3ObjectModel
//                {
//                    ObjectName = "",
//                    ObjectType = "",
//                    ObjectContentType = "",
//                    ObjectStream = null,
//                    Extension = ""
//                };
//            }
//        }

//        /// <summary>
//        /// Upload to S3 and Return Object Key
//        /// </summary>
//        /// <param name="fileName"></param>
//        /// <param name="filePath"></param>
//        /// <returns></returns>
//        public static async Task<string> UploadMediaFromPath(string fileName, string filePath, bool applicationFile = false)
//        {
//            var awsS3Bucket = AwsS3Client();
//            var mediaProcess = new MediaProcess
//            {
//                FileName = fileName,
//                ObjectKey = Guid.NewGuid().ToString(),
//                FilePath = filePath,
//                IsPublic = false
//            };
//            if (applicationFile)
//                mediaProcess.ObjectKey = fileName;

//            await (new AzureStorageRules(new ApplicationDbContext()).UploadMediaFromPathByQbicleAsync(awsS3Bucket, mediaProcess, applicationFile));
//            return mediaProcess.ObjectKey;
//        }

//        public static void DeleteS3Object(string objKey, AwsS3Bucket awsS3)
//        {
//            try
//            {
//                var deleteObjectRequest = new DeleteObjectRequest
//                {
//                    BucketName = awsS3.BucketName,
//                    Key = objKey
//                };

//                awsS3.AwsS3Client.DeleteObjectAsync(deleteObjectRequest);
//            }
//            catch (AmazonS3Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, objKey);
//            }
//            catch (Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, objKey);
//            }
//        }
//    }
//}