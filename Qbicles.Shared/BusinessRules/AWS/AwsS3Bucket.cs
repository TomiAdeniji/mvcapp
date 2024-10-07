using System.IO;
using Amazon.S3;

namespace Qbicles.BusinessRules.AWS
{
    public class AwsS3Bucket
    {
        public string BucketName { get; set; }
        public string BucketRegion { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string IdentityPoolId { get; set; }

        public AmazonS3Client AwsS3Client { get; set; }
    }

    public class S3ObjectModel
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
