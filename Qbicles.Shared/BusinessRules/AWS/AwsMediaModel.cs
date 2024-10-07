using System.Drawing;
using System.IO;

namespace Qbicles.BusinessRules.AWS
{

    public class AwsS3ObjectItem
    {
        public string ObjectKey { get; set; }
        public bool IsPublic { get; set; }
    }

    public class S3ObjectUploadModel
    {
        public string FileKey { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string FileType { get; set; }
    }

    public class MediaProcess
    {
        public string ObjectKey { get; set; }
        /// <summary>
        /// Upload media to Bucket's sub folder 
        /// </summary>
        public string FolderUpload { get; set; }
        public string FileName { get; set; }
        public int SourceWidth { get; set; }
        public int SourceHeight { get; set; }
        public int TargetHeight { get; set; }
        public int TargetWidth { get; set; }
        public MemoryStream MemoryStream { get; set; }
        public string FilePath { get; set; }
        public bool IsPublic { get; set; }
    }
}
