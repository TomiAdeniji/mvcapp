using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Azure
{
    public class AzureStorage
    {
        public string BucketName { get; set; }
        public string BucketRegion { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string IdentityPoolId { get; set; }

        public AmazonS3Client AzureStorageClient { get; set; }
    }

    public class AzureObjectModel
    {
        public string ObjectKey { get; set; }
        public string ObjectName { get; set; }
        public string Extension { get; set; }
        public string ObjectType { get; set; }
        public string ObjectSize { get; set; }
        public string ObjectContentType { get; set; }
        public bool IsPublic { get; set; }
        public Stream ObjectStream { get; set; }
    }
}
