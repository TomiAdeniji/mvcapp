using System.Configuration;
using System.IO;

namespace Qbicles.BusinessRules.Helper
{
    public static class ConfigManager
    {
        public static string DocumentsApi => ConfigurationManager.AppSettings["DocumentsAPI"];
        public static string TraderApi => ConfigurationManager.AppSettings["TraderAPI"];
        public static string QbiclesJobApi => ConfigurationManager.AppSettings["QbiclesJobAPI"];
        public static string SignalRApi => ConfigurationManager.AppSettings["SignalRAPI"];
        public static string DefaultUserUrlGuid => ConfigurationManager.AppSettings["DefaultUserUrlGuid"];
        public static string QbiclesUrl => ConfigurationManager.AppSettings["QbiclesUrl"];
        public static string AuthHost => ConfigurationManager.AppSettings["AuthHost"];
        public static string ClientSecret => ConfigurationManager.AppSettings["ClientSecret"];

        /// <summary>
        /// service_connect
        /// </summary>
        public static string ClientId => "service_connect";

        /// <summary>
        /// service
        /// </summary>
        public static string ClientScope => "service";

        public static string DefaultProductPlaceholderImageUrl => ConfigurationManager.AppSettings["DefaultProductPlaceholderImageUrl"];
        public static string ItemProductImportTemplate => ConfigurationManager.AppSettings["ItemProductImportTemplate"];
        public static string GoogleMapApiKey => ConfigurationManager.AppSettings["GoogleMapApiKey"];
        public static string GoogleShortlinkApiKey => ConfigurationManager.AppSettings["GoogleShortlinkApiKey"];

        public static string ApiGetDocumentUri => DocumentsApi + @"/retriever/getdocument?file=";
        public static string ApiGetVideoUri => DocumentsApi + @"/retriever/getvideomediaplay?file={0}&type={1}";
        public static string ApiGetVideoScreenshot => DocumentsApi + @"/retriever/GetVideoMediaScreenshot?file=";
        public static string HubUrl => ConfigurationManager.AppSettings["HubUrl"];
        public static string HubName => "BroadcastUpdate";
        public static string HubInvokeMethod => "NotificationBroadcast";

        public static string ThumbnailImageWidth => ConfigurationManager.AppSettings["ThumbnailImageWidth"];
        public static string SmallImageWidth => ConfigurationManager.AppSettings["SmallImageWidth"];
        public static string MediumImageWidth => ConfigurationManager.AppSettings["MediumImageWidth"];
        public static string MaximumInvitesJoinQbiclesPerDay => ConfigurationManager.AppSettings["MaximumInvitesJoinQbiclesPerDay"];

        public static string UploadTransaction => ConfigurationManager.AppSettings["uploadTransaction"];

        public static string SQLitePOSMenuName => ConfigurationManager.AppSettings["SQLitePOSMenuName"];

        #region Azure Storage

        public static string AzureStorageConnectionString => ConfigurationManager.AppSettings["StorageConnectionString"];
        public static string AzureBlobContainerName => ConfigurationManager.AppSettings["BlobContainerName"];
        public static string AzureShareName => ConfigurationManager.AppSettings["ShareName"];

        #endregion Azure Storage

        //AWS Configuration
        public static string SecretKey => ConfigurationManager.AppSettings["AWSS3SecretKey"];

        public static string AccessKey => ConfigurationManager.AppSettings["AWSS3AccessKey"];
        public static string BucketRegion => ConfigurationManager.AppSettings["AWSS3BucketRegion"];
        public static string BucketName => ConfigurationManager.AppSettings["AWSS3BucketName"];
        public static string IdentityPoolId => ConfigurationManager.AppSettings["AWSS3IdentityPoolId"];

        public static string SESSecretKey => ConfigurationManager.AppSettings["SESSecretKey"];
        public static string SESAccessKey => ConfigurationManager.AppSettings["SESAccessKey"];

        public static string HighlightBannerNew => ConfigurationManager.AppSettings["HighlightBannerNew"];
        public static string HighlightBannerEvent => ConfigurationManager.AppSettings["HighlightBannerEvent"];
        public static string HighlightBannerKnowledge => ConfigurationManager.AppSettings["HighlightBannerKnowledge"];
        public static string HighlightBannerListing => ConfigurationManager.AppSettings["HighlightBannerListing"];
        public static string CommunityBlocked => ConfigurationManager.AppSettings["CommunityBlocked"];
        public static string CommunityLestTalk => ConfigurationManager.AppSettings["CommunityLestTalk"];
        public static string CommunityPendingadd => ConfigurationManager.AppSettings["CommunityPendingadd"];
        public static string Communitybuysell => ConfigurationManager.AppSettings["Communitybuysell"];
        public static string CommunityShop => ConfigurationManager.AppSettings["CommunityShop"];
        public static string B2BBuySell => ConfigurationManager.AppSettings["B2BBuySell"];
        public static string CatalogDefaultImage => ConfigurationManager.AppSettings["CatalogDefaultImage"];
        public static string SESIdentityVerificationTemplateNamePrefix => ConfigurationManager.AppSettings["SESIdentityVerificationTemplateNamePrefix"];
        public static string SQLiteConnectionString { get; set; }
        // public static string SQLiteRepository => ConfigurationManager.AppSettings["SQLiteRepository"];

        //Logging
        public static string LogInstance => ConfigurationManager.AppSettings["LogInstance"];

        public static string LogEnvironment => ConfigurationManager.AppSettings["LogEnvironment"];
        public static string LogApplication => ConfigurationManager.AppSettings["LogApplication"];

        //firebase
        public static string FirebaseServerKey => ConfigurationManager.AppSettings["FirebaseServerKey"];

        public static string FirebaseSenderId => ConfigurationManager.AppSettings["FirebaseSenderId"];
        public static string FirebaseRequestUri => ConfigurationManager.AppSettings["FirebaseRequestUri"];

        public static string TempPathRepository
        {
            get
            {
                var tempPath = Path.GetTempPath();
                DirectoryInfo info = new DirectoryInfo(tempPath);
                if (!info.Exists)
                    info.Create();
                return tempPath;
            }
        }

        //public static string SQLiteDbName => $"Data Source={ConfigurationManager.AppSettings["SQLiteRepository"]}/{{0}};Version=3;";

        public static bool LoggingDebugSet
        {
            get { return ConfigurationManager.AppSettings["LoggingDebugSet"] == "true"; }
        }
    }
}