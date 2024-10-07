using Amazon.S3;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Storage.Repositories;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Azure
{
    public static class AzureStorageHelper
    {
        /// <summary>
        /// Get Azure storage settings for both blob and file
        /// </summary>
        /// <returns></returns>
        private static AzureStorageSettings GetAzureStorageSettings()
        {
            return new AzureStorageSettings()
            {
                ConnectionString = ConfigManager.AzureStorageConnectionString,
                ContainerName = ConfigManager.AzureBlobContainerName,
                ShareName = ConfigManager.AzureShareName,
            };
        }

        /// <summary>
        /// Get Azure storage settings for blob storage
        /// </summary>
        /// <returns></returns>
        public static BlobStorageSettings GetBlobStorageSettings()
        {
            return new BlobStorageSettings()
            {
                ConnectionString = ConfigManager.AzureStorageConnectionString,
                ContainerName = ConfigManager.AzureBlobContainerName,
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        public static bool CheckExistedObject(string objectKey)
        {
            var blobRepository = new BlobStorageRepository(GetBlobStorageSettings());
            return blobRepository.Exists(objectKey);
        }

        /// <summary>
        /// Get URI of keyName, if not found return URI of origin file
        /// Use in the case get URI by file size - case get URI file size but hangfire processing file not completed yet
        /// </summary>
        /// <param name="objectKey">object key to get uri</param>
        /// <returns></returns>
        public static string SignedUrl(string objectKey, string filename = "")
        {
            //Initalize the storage settings
            var blobStorageSettings = GetBlobStorageSettings();

            //Initalize the blob repository
            var blobRepository = new BlobStorageRepository(blobStorageSettings);

            //Create blob object
            var blockBlob = blobRepository.CreateClient(objectKey);

            //Init blob pre-signed url request
            var builder = new BlobSasBuilder
            {
                Resource = "b",
                BlobContainerName = blobStorageSettings.ContainerName,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(24),
                BlobName = objectKey,
            };

            //Set the permission
            builder.SetPermissions(BlobAccountSasPermissions.Read);

            //Init the blob client
            var uri = blockBlob.GenerateSasUri(builder);

            return uri.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objKey"></param>
        /// <returns></returns>
        public static async Task<Stream> ReadSqLiteFileAsync(string objKey)
        {
            try
            {
                //Initalize the storage settings
                var blobRepository = new BlobStorageRepository(GetBlobStorageSettings());

                //Create blob object
                var blockBlob = blobRepository.CreateClient(objKey);

                //Get blob file stream
                using (var responseStream = await blockBlob.OpenReadAsync())
                {
                    var memoryStream = new MemoryStream();
                    responseStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }
            catch (Exception exception)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), exception);
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objKey"></param>
        /// <returns></returns>
        public static async Task<S3ObjectModel> ReadObjectDataAsync(string objKey)
        {
            try
            {
                //Initalize the storage settings
                var blobRepository = new BlobStorageRepository(GetBlobStorageSettings());

                // Get a reference to the blob
                var blobClient = blobRepository.CreateClient(objKey);

                //Get blob file stream
                using (var responseStream = await blobClient.OpenReadAsync())
                {
                    var memoryStream = new MemoryStream();
                    await responseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    //return memoryStream;

                    // Fetch the blob properties
                    BlobProperties properties = await blobClient.GetPropertiesAsync();                                      
                    string fileExtension = string.Empty;
                    string fileType = string.Empty;

                    // Extract the required properties
                    string fileName = blobClient.Name; // Gets the file name with extension
                    var fileSize = HelperClass.FileSize(properties.ContentLength); // Gets the file size
                    string contentType = properties?.ContentType; // Content type, e.g., "image/png"
                    properties?.Metadata.TryGetValue("fileType", out fileType); // File type, e.g., "PNG"
                    properties?.Metadata.TryGetValue("fileExtension", out fileExtension); // Gets the extension (e.g., ".png")


                    if (fileType == null)
                    {
                        try
                        {
                            fileType = contentType.Split('/')[0];
                            fileExtension = contentType.Split('/')[1];
                        }
                        catch
                        {
                            fileType = "application";
                        }
                    }

                    return new S3ObjectModel
                    {
                        ObjectKey = objKey,
                        ObjectName = fileName,
                        ObjectType = fileType,
                        ObjectSize = fileSize,
                        ObjectContentType = contentType,
                        ObjectStream = memoryStream,
                        Extension = fileExtension
                    };
                }
            }
            catch (Exception exception)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), exception);
                return new S3ObjectModel
                {
                    ObjectName = "",
                    ObjectType = "",
                    ObjectContentType = "",
                    ObjectStream = null
                };
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objKey"></param>
        /// <param name="awsS3"></param>
        /// <returns></returns>
        public static async Task<S3ObjectModel> ReadS3ObjectDataAsync(string objKey, AwsS3Bucket awsS3)
        {
            try
            {
                //Initalize the storage settings
                var blobRepository = new BlobStorageRepository(GetBlobStorageSettings());

                // Get a reference to the blob
                var blobClient = blobRepository.CreateClient(objKey);

                //Get blob file stream
                using (var responseStream = await blobClient.OpenReadAsync())
                {
                    var memoryStream = new MemoryStream();
                    await responseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    //return memoryStream;

                    // Fetch the blob properties
                    var properties = await blobClient.GetPropertiesAsync();

                    // Extract the required properties
                    string fileName = Path.GetFileName(blobClient.Name); // Gets the file name with extension
                    string fileExtension = Path.GetExtension(blobClient.Name); // Gets the extension (e.g., ".png")
                    string fileType = fileExtension?.TrimStart('.').ToUpper(); // File type, e.g., "PNG"
                    string contentType = properties.Value.ContentType; // Content type, e.g., "image/png"

                    //string fileName = response.Metadata["x-amz-meta-file-name"]; // have "file-name" as medata added to the object.
                    //string fileType = response.Metadata["x-amz-meta-file-type"]; // have "file-type" as medata added to the object.
                    //string fileExtension = response.Metadata["x-amz-meta-file-extension"]; // have "file-extension" as medata added to the object.
                    //string contentType = response.Headers["Content-Type"];

                    if (fileType == null)
                    {
                        try
                        {
                            fileType = contentType.Split('/')[0];
                            fileExtension = contentType.Split('/')[1];
                        }
                        catch
                        {
                            fileType = "application";
                        }
                    }

                    return new S3ObjectModel
                    {
                        ObjectName = fileName,
                        ObjectType = fileType,
                        ObjectContentType = contentType,
                        ObjectStream = memoryStream,
                        Extension = fileExtension
                    };
                }
            }
            catch (Exception exception)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), exception);
                return new S3ObjectModel
                {
                    ObjectName = "",
                    ObjectType = "",
                    ObjectContentType = "",
                    ObjectStream = null
                };
            }
        }

        /// <summary>
        /// Upload to S3 and Return Object Key
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string> UploadMediaFromPath(string fileName, string filePath, bool applicationFile = false)
        {
            var mediaProcess = new MediaProcess
            {
                FileName = fileName,
                ObjectKey = Guid.NewGuid().ToString(),
                FilePath = filePath,
                IsPublic = false
            };

            if (applicationFile)
                mediaProcess.ObjectKey = fileName;

            await (new AzureStorageRules(new ApplicationDbContext()).UploadMediaFromPathByQbicleAsync(mediaProcess, applicationFile));
            return mediaProcess.ObjectKey;
        }

        /// <summary>
        /// Todo:
        ///     This was retained since it's possibly been used anywhere with two params
        ///     It should be later removed since the awsS3 is useless
        /// </summary>
        /// <param name="objKey"></param>
        /// <param name="awsS3"></param>
        public static void DeleteS3Object(string objKey, AwsS3Bucket awsS3)
        {
            try
            {
                //Initalize the storage settings
                var blobRepository = new BlobStorageRepository(GetBlobStorageSettings());

                //Delete blob object
                blobRepository.Delete(objKey);
            }
            catch (AmazonS3Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, objKey);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, objKey);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objKey"></param>
        public static void DeleteObject(string objKey)
        {
            try
            {
                //Initalize the storage settings
                var blobRepository = new BlobStorageRepository(GetBlobStorageSettings());

                //Delete blob object
                blobRepository.Delete(objKey);
            }
            catch (AmazonS3Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, objKey);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, objKey);
            }
        }
    }
}