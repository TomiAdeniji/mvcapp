using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Invitation;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Qbicles;
using Qbicles.Models.UserInformation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleDomain;
using static Qbicles.Models.QbicleEvent;
using static Qbicles.Models.QbicleTask;
namespace Qbicles.BusinessRules.Micro.Model
{
    public class MicroContext
    {
        public ApplicationDbContext Context { get; set; }
        public string UserId { get; set; }
    }

    public class MicroDomain : BaseModel
    {
        public string DomainKey { get; set; }
        public string CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public string OwnedBy { get; set; }
        public Uri LogoUri { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DomainStatusEnum Status { get; set; } = DomainStatusEnum.Open;

        public string DomainRole { get; set; }
        public int DomainRoleType { get; set; }

    }

    public enum DomainRoleEnum
    {
        [Description("My Domain")]
        MyDomain = 1,
        [Description("Admin")]
        Admin = 2,
        [Description("Manager")]
        Manager = 3
    }

    public class MicroApproverInvitation
    {
        public int Id { get; set; }
        public int DomainId { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
    }

    public class MicroInvitation
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public InvitationStatusEnum Status { get; set; }
        public string DomainName { get; set; }
        public Uri DomainLogoUri { get; set; }
        public string InviteBy { get; set; }
        public int DomainId { get; set; }
        public string UserId { get; set; }
    }

    public class MicroQbicle : BaseModel
    {
        public int DomainId { get; set; }
        public string Description { get; set; }
        public string StartedBy { get; set; }
        public string StartedDate { get; set; }
        public string OwnedBy { get; set; }
        public Uri LogoUri { get; set; }
        /// <summary>
        /// Object key from S3 to process file upload
        /// </summary>
        public string LogoKey { get; set; }
        public string ClosedBy { get; set; }
        public DateTime? ClosedDate { get; set; }
        public bool IsUsingApprovals { get; set; } = false;
        /// <summary>
        /// Get value is User Full name
        /// POST value is User Id
        /// </summary>
        public List<MicroUser> Managers { get; set; }
        public List<MicroUser> Members { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsHidden { get; set; }
        public bool IsClosed { get; set; }
        public string Manager { get; set; }
        public string[] QbicleUsers { get; set; }
    }

    public class MicroTopic : BaseModel
    {
        public string Summary { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Creator { get; set; }
        public string Apps { get; set; }
        public int Instances { get; set; }
        public int QbicleId { get; set; }
        public bool CanDelete { get; set; } = true;
    }

    public class MicroMediaFolder : BaseModel
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<MicroMedia> Medias { get; set; } = new List<MicroMedia>();
    }

    public class MicroMedia
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TopicId { get; set; }
        public string Topic { get; set; }
        /// <summary>
        /// QbicleFileType model
        /// </summary>
        public MicroFileType FileType { get; set; }

        public MicroVersionedFile VersionedFile { get; set; } = new MicroVersionedFile();

        public int MediaFolderId { get; set; }
        public bool IsPublic { get; set; }
        /// <summary>
        /// ActivityTypeEnum.GetDescription
        /// </summary>
        public string ActivityType { get; set; }
        /// <summary>
        /// ActivityApp.GetDescription
        /// </summary>
        public string App { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int QbicleId { get; set; }

    }

    public class MicroVersionedFile
    {
        public int Id { get; set; }
        public Uri Uri { get; set; }
        public string FileSize { get; set; }
        public string FileKey { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public int MediaId { get; set; }
        public MicroFileType FileType { get; set; }
    }

    public class MicroFileType
    {
        public string Extension { get; set; }
        /// <summary>
        /// If Type === "Image File" then display VersionedFile.URI
        /// else display icon by other Type value ( Word Document, Powerpoint Presentation, Excel File...) or Extenssion value ( doc, docx,...)
        /// </summary>
        public string Type { get; set; }
        public string Size { get; set; }
    }

    public class MicroMediaUpload : BaseModel
    {
        public string Description { get; set; }
        public string TopicName { get; set; }
        public int FolderId { get; set; }
        public int QbicleId { get; set; }
        public int ActivityId { get; set; }
        public StreamType ActivityType { get; set; }
        public S3ObjectUploadModel Detail { get; set; }
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; }
    }

    public class MicroPostParameter
    {
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; }
        //public int Id { get; set; }
        public string TopicName { get; set; } = "General";
        public string Message { get; set; }
        public string ToppicId { get; set; }
        public int QbicleId { get; set; }
        public int ActivityId { get; set; }
        public StreamType ActivityType { get; set; }
        /// <summary>
        /// QBIC-3965: If the creator of the Activity or Post is the Customer in a B2CQBicle or B2COrder then this value must be set to true at the time the Activity/post is created
        /// This property should have the default value of false,
        /// </summary>
        public bool IsCreatorTheCustomer { get; set; }
    }

    public class StreamFilterOption
    {
        public List<BaseModel> ActivityTypes { get; set; } = new List<BaseModel>();
        public List<string> AppTypes { get; set; } = new List<string>();
        public List<BaseModel> Topics { get; set; } = new List<BaseModel>();
    }

    public class StreamCalendarFilterOption
    {
        public List<BaseModel> ActivityTypes { get; set; } = new List<BaseModel>();
        public List<string> CalendarTypes { get; set; } = new List<string>();
        public List<string> AppTypes { get; set; } = new List<string>();
        public Dictionary<string, string> Orderby { get; set; } = new Dictionary<string, string>();
        public List<BaseModel> Topics { get; set; } = new List<BaseModel>();
        public List<MicroUserBase> Users { get; set; } = new List<MicroUserBase>();
    }

    public class MicroLinkQbicleModel : BaseModel
    {
        public string Url { get; set; }
        public string Description { get; set; }
        public int TopicId { get; set; }
        public int QbicleId { get; set; }
        public S3ObjectUploadModel FeaturedImage { get; set; }
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; }
    }

    public class MicroEventQbicleModel : BaseModel
    {
        public DateTime Start { get; set; }
        public string Location { get; set; }
        public EventTypeEnum EventType { get; set; }
        public EventDurationUnitEnum DurationUnit { get; set; }
        public short Duration { get; set; }
        public string Description { get; set; }
        public bool IsRecurs { get; set; } = false;
        public string[] Invites { get; set; }
        public int TopicId { get; set; }
        public int QbicleId { get; set; }

        public S3ObjectUploadModel Image { get; set; }
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; }
    }

    public class MicroTaskQbicleModel : BaseModel
    {
        public TaskDurationUnitEnum DurationUnit { get; set; }
        public short Duration { get; set; }
        public string Description { get; set; }
        public TaskPriorityEnum Priority { get; set; }
        public DateTime? ProgrammedStart { get; set; }
        public string Assignee { get; set; }
        public string[] Watchers { get; set; }
        public int TopicId { get; set; }
        public int QbicleId { get; set; }
        public S3ObjectUploadModel Image { get; set; }
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; }
    }

    public class MicroActivityComment
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string DateCreated { get; set; }
        public Uri Image { get; set; }
    }

    public class MicroActivityComments
    {
        public List<MicroActivityComment> Comments { get; set; } = new List<MicroActivityComment>();
        public int Total { get; set; } = 0;
        public int CommentTotal { get; set; } = 0;
    }

    public class MicroActivityMedia
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public Uri CreatedByImage { get; set; }
        public string CreatedDate { get; set; }
        public string LastUpdate { get; set; }
        public Uri MediaUri { get; set; }
        public string Description { get; set; }
        public MicroFileType FileType { get; set; }
        public string Topic { get; set; }
        public string Folder { get; set; }
    }

    public class MicroMediaView : MicroActivityMedia
    {
        public List<MicroVersionedFile> VersionedFiles { get; set; } = new List<MicroVersionedFile>();
        public List<BaseModel> Foldedrs { get; set; } = new List<BaseModel>();
        public List<BaseModel> Topics { get; set; } = new List<BaseModel>();
        public List<BaseModel> VersionsOption { get; set; } = new List<BaseModel>();
    }

    public class MicroActivityMedias
    {
        public List<MicroActivityMedia> Medias { get; set; } = new List<MicroActivityMedia>();
        public int Total { get; set; } = 0;
    }

    public class MicroActivityBase
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri FeaturedImage { get; set; }
        public string Domain { get; set; }
        public int QbicleId { get; set; }
        public int TopicId { get; set; }
        public string Topic { get; set; }
        public List<MicroUserBase> Attendees { get; set; } = new List<MicroUserBase>();
        public List<MicroUserBase> Participants { get; set; } = new List<MicroUserBase>();
        public List<MicroUserBase> QbicleMembers { get; set; } = new List<MicroUserBase>();
        public MicroActivityComments Comments { get; set; } = new MicroActivityComments();
        public MicroActivityMedias Medias { get; set; } = new MicroActivityMedias();
        public string CreatedBy { get; set; }
        public Uri UserAvatar { get; set; }
    }

    public class MicroDiscussionActivity : MicroActivityBase
    {
        public string Summary { get; set; }
        public string StartedDate { get; set; }
        public string ExpireDateString { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsExpiry { get; set; }
    }

    public class MicroLinkActivity : MicroActivityBase
    {
        public string Url { get; set; }
    }

    public class MicroEventActivity : MicroActivityBase
    {
        public string Where { get; set; }
        public string When { get; set; }
        public string Duration { get; set; }
        public string EventType { get; set; }
        public string Present { get; set; }
        public bool? Attending { get; set; }
        public int? AttendingId { get; set; }
    }
    public class MicroTaskActivity : MicroActivityBase
    {
        public string Deadline { get; set; }
        public string Duration { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public bool Started { get; set; }
        public bool Completed { get; set; }
        public string Assignee { get; set; }
        public string AssigneeId { get; set; }
        public bool CanAddWorklog { get; set; }
        public TaskPriorityEnum PriorityId { get; set; }
        public string PriorityName { get; set; }
        public string TimeBegin { get; set; }
        public List<MicroUserBase> Watchers { get; set; } = new List<MicroUserBase>();
        public List<TimeLogging> TimeLoggings { get; set; } = new List<TimeLogging>();
    }

    public class TimeLogging
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        public string DateTime { get; set; }

        public short Days { get; set; }

        public short Hours { get; set; }

        public short Minutes { get; set; }

        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; }
    }

    public struct MicroNotification
    {
        /// <summary>
        /// Key of Notification
        /// </summary>
        public string NotificationId { get; set; }
        public bool IsRead { get; set; }
        // public string Name { get; set; }
        public string Title { get; set; }
        public int? DomainId { get; set; }
        //public string DomainName { get; set; }
        public int? QbicleId { get; set; }
        //public string Qbiclename { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedDateString { get; set; }
        public DateTime CreatedDate { get; set; }
        public Uri CreatedByImage { get; set; }
        /// <summary>
        /// Event notification ( create qbicle, discusion,.. Approved,...)
        /// </summary>
        public NotificationEventEnum EventId { get; set; }
        //public string EventName { get; set; }
        /// <summary>
        /// Use for open approval page ( sale/purchase... review, approval, journal), and activity detail ( alert, discussion...)
        /// if empty then don't open detail page
        /// </summary>
        //public string ActivityDetail { get; set; } = "";
        //public int? ApprovalId { get; set; }
        /// <summary>
        /// key of activity, use to open page detail
        /// </summary>
        public int? ActivityKey { get; set; }
        //public string DomainKey { get; set; }
        //public string QbicleKey { get; set; }
        //public int? ActivityId { get; set; }
    }

    public class MicroUserBase
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public Uri ProfilePic { get; set; }
        public string Role { get; set; }
    }

    public class MicroUser : MicroUserBase
    {
        public string Email { get; set; }
        public string Timezone { get; set; }
        public string DateFormat { set; get; }
        public string TimeFormat { set; get; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Profile { get; set; }
        public bool IsOwner { get; set; }
        public bool IsQbicleManager { get; set; }
        public bool IsQbicleMemeber { get; set; }
        public int DomainRoleLevelId { get; set; }
        public int DomainRoleLevel { get; set; }
    }


    public class MicroUserProfile
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public Uri ProfilePic { get; set; }
        public string Profile { get; set; }
        public string UserName { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Timezone { get; set; }

        public string DateFormat { set; get; }
        public string TimeFormat { set; get; }

        public string TagLine { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string Tell { get; set; }
        public bool IsShareEmail { get; set; }
        public bool IsShareCompany { get; set; }
        public bool IsShareJobTitle { get; set; }
        public bool IsAlwaysLimitMyContact { get; set; }

        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string LinkedlnLink { get; set; }
        public string TwitterLink { get; set; }
        public string WhatsAppLink { get; set; }

        public BaseModel PreferredDomain { get; set; }

        public BaseModel PreferredQbicle { get; set; }

        public string DisplayUserName { get; set; }

        //Notification Settings
        public NotificationSendMethodEnum ChosenNotificationMethod { get; set; } = NotificationSendMethodEnum.Email;
        /// <summary>
        /// Play sound notification
        /// True: Play
        /// False: No
        /// </summary>
        public NotificationSound NotificationSound { get; set; } = NotificationSound.No;

        public bool ShowConnect { get; set; }
        public string Connect { get; set; }
        public UserConnectStatus ConnectStatus { get; set; }
        public bool MyProfile { get; set; } = false;
    }

    public enum UserConnectStatus
    {
        [Description("Connect")]
        None = 0,
        [Description("Pending connection")]
        Pending = 1,
        [Description("Connected")]
        Connected = 2
    }

    public class MicroConnect
    {
        public List<MicroCommunityConnect> Connectes { get; set; } = new List<MicroCommunityConnect>();
        public int TotalPage { get; set; }
    }

    public class MicroCommunityConnect
    {
        public string DomainKey { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public Uri Image { get; set; }
        [JsonIgnore]
        public string AvatarUri { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
    }

    public class MicroUserShowcase : BaseModel
    {
        public string Title { get; set; }
        public string Caption { get; set; }
        public Uri ImageUri { get; set; }
    }

    public class MicroUserSkill : BaseModel
    {
        public int Proficiency { get; set; }
    }

    /// <summary>
    /// Work and Education
    /// </summary>
    public class MicroUserExperience : BaseModel
    {
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public string StartDateString { get; set; }

        public string EndDateString { get; set; }

        public string Summary { get; set; }
        public string Company { get; set; }

        public string Role { get; set; }
        public string Institution { get; set; }

        public string Course { get; set; }
        public ExperienceType Type { get; set; }
    }

    public class MicroUserFileShare : BaseModel
    {
        public string Title { get; set; }
        public string StoredFileName { get; set; }

        public string Description { get; set; }
        public virtual MicroFileType FileType { get; set; }
    }

    public class MicroUserSharedQbicles : BaseModel
    {
        public string Domain { get; set; }
        public bool IsMyDomain { get; set; }
        public Uri LogoUri { get; set; }
    }

    public class MicroUserAddress
    {

        public int Id { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public MicroCountry Country { get; set; }

        public string PostCode { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }

        public bool IsDefault { get; set; } = false;
    }

    public class MicroCountry
    {
        public CountryCode? CountryCode { get; set; }
        public string CommonName { get; set; }
    }


    public class MyOrderFilter
    {
        public int PageIndex { get; set; }
        public string Keyword { get; set; }
        public string Daterange { get; set; }
        public int SaleChannel { get; set; }
        public int OrderStatus { get; set; }
    }

    public class ShoppingFilter
    {
        public string Keyword { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; } = 0;
        public string LocationId { get; set; }
        public List<int> CategoryIds { get; set; }
        public bool LimitMyConnections { get; set; }
    }


    public class MicroShop
    {
        public int DomainId { get; set; }
        public string DomainKey { get; set; }
        public string LogoUri { get; set; }
        public string BusinessName { get; set; }
        public string BusinessSummary { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public string[] Categories { get; set; }
        public string[] Tags { get; set; }
    }

    public class MicroMyOrderDetail
    {
        public Uri BusinessLogo { get; set; }
        public string BusinessName { get; set; }
        public string Placed { get; set; }
        public string Ref { get; set; }
        public string StatusName { get; set; }
        public int Status { get; set; }
        public List<MicroMyOrderItem> Items { get; set; }
        public string Total { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingNote { get; set; }
    }


    public class MicroMyOrderItem
    {
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string Discount { get; set; }
        public string Taxes { get; set; }
        public string Total { get; set; }
        public string TotalWithoutDiscount { get; set; }
        public MicroMyOrderItemVariantExtra Variant { get; set; }
        public List<MicroMyOrderItemVariantExtra> Extras { get; set; }
    }
    public class MicroMyOrderItemVariantExtra
    {
        public string Name { get; set; }
        public string Price { get; set; }
    }


    public class MicroCommunities
    {
        public List<MicroCommunity> Communities { get; set; }
        public int TotalPage { get; set; } = 0;
    }

    public class MicroCommunity : BaseModel
    {
        public string DomainKey { get; set; }
        public string DomainSubAccountCode { get; set; } = "";
        public CommsStatus Status { get; set; }
        public string StatusName { get; set; }
        public string Surname { get; set; }
        /// <summary>
        /// 1.LinkUsers
        /// 2.LinkBusiness
        /// 3. pending
        /// </summary>
        public int ContactType { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int QbicleId { get; set; }
        public string LinkId { get; set; }
        public Uri Image { get; set; }
        public bool Favourited { get; set; }
        public bool CanCreateDiscussion { get; set; } = false;
        public bool CanCreateAddFile { get; set; } = false;
        public bool CanCreateAddTask { get; set; } = false;
        public string LastUpdatedString { get; set; }
        public DateTime LastUpdated { get; set; }
        public int NewActivities { get; set; }
        public string ComnunityEmail { get; set; }
        public List<string> Actions { get; set; }
    }
    public class MicroCommunityDefault
    {
        public Uri Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedDate { get; set; }
        public List<string> Action { get; set; } = new List<string>();
    }


    public class MicroMonibackMyStore
    {
        public string BusinessProfileId { get; set; }
        public string ContactKey { get; set; }
        public string Name { get; set; }
        public Uri LogoUri { get; set; }
        public string DomainId { get; set; }
    }

    public class MicroMonibackMyStoreInfo
    {
        public string DomainId { get; set; }
        public int QbicleId { get; set; }
        public string BusinessProfileId { get; set; }
        public string ContactKey { get; set; }
        public string Name { get; set; }
        public Uri LogoUri { get; set; }
        public decimal AccountBalance { get; set; }
        public string AccountBalanceString { get; set; }
        public int Points { get; set; }
        public decimal StoreCredit { get; set; }
        public string StoreCreditString { get; set; }
        public int ValidVouchersCount { get; set; }
    }

    public class VoucherClaimParameter
    {
        public string PromotionKey { get; set; }
        public string BusinessKey { get; set; }
    }

    public class MicroBusinessProfile
    {
        public string DomainKey { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Summary { get; set; }
        public Uri ImageUri { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<string> Tags { get; set; }

        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Linkedln { get; set; }
        public string Twitter { get; set; }
        public string Youtube { get; set; }

        public IEnumerable<MicroBusinessPost> Posts { get; set; }
        public IEnumerable<MicroBusinessAddress> Locations { get; set; } 
        /// <summary>
        /// business profile id = 0 has connect, > 0 not connect
        /// </summary>
        public string ConnectId { get; set; } = "0";
        public int QbicleId { get; set; } = 0;
        public IEnumerable<MicroBusinessShop> Shops { get; set; }
    }


    public class MicroBusinessPost
    {
        public string Key { get; set; }
        public Uri ImageUri { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class MicroBusinessAddress
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class MicroBusinessShop
    {
        /// <summary>
        /// Catalog key
        /// </summary>
        public string Key { get; set; }
        public int CatalogId { get; set; }
        /// <summary>
        /// use to open business detail
        /// </summary>
        public string QbicleKey { get; set; }
        public string Title { get; set; }
        public Uri ImageUri { get; set; }
        public string Summary { get; set; }
    }

    public class MicroContact
    {
        /// <summary>
        /// using while connect
        /// </summary>
        public string Id { get; set; }
        public Uri AvatarUri { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class BusinessPageModel
    {
        public string PageTitle { get; set; }
        public string PageContent { get; set; }
    }

    public class BusinessPageParameter
    {
        public string DomainKey { get; set; }
        public string Token { get; set; }
    }


    public class MicroContactImport
    {
        public string InviteUrl { get; set; }
        public List<MicroContact> Potentials { get; set; } = new List<MicroContact>();
        public List<MicroContact> InviteAsNews { get; set; } = new List<MicroContact>();
    }

    public class LoginSplashInterstitial
    {
        public Uri Avatar { get; set; }
        public string Forename { get; set; }
        public string DomainTitle { get; set; }
        public LoginSplashInterstitialAction DomainAction { get; set; } = LoginSplashInterstitialAction.AddDomain;
        public BaseModelImage Domain { get; set; } = new BaseModelImage();
        public int? NewMessage { get; set; } = 0;
    }

    public enum LoginSplashInterstitialAction
    {
        AddDomain = 1,
        GoToListDomain = 2,
        GoToQbicle = 3,
        UserProfileWizard = 4
    }

    public enum MicroFirstLaunched
    {
        ImportWizard = 0,
        UserProfileWizard = 1,
        BusinessProfileWizard = 2,
        Splash = 3,
    }

    public enum MicroUserWizardStep
    {
        [Description("Basics")]
        Basics = 1,//GeneralSettingsStep = 1,
        [Description("Contact")]
        Contact = 2,//AddressAndPhoneSettingStep = 2,
        [Description("Showcase")]
        Showcase = 3,//ShowcaseSettingStep = 3,
        [Description("Done")]
        Done = 4,
        //[Description("Step 5")]
        //InterestSettingsStep = 5,
        //[Description("Step 6")]
        //BusinessesConnectStep = 6

        /*
         UserWizardStep

        [Description("Step 1")]
        GeneralSettingsStep = 1,
        [Description("Step 2")]
        AddressAndPhoneSettingStep = 2,
        [Description("Step 3")]
        ShowcaseSettingStep = 3,
        [Description("Step 4")]
        Settings = 4,
        [Description("Step 5")]
        InterestSettingsStep = 5,
        [Description("Step 6")]
        BusinessesConnectStep = 6

         */
    }

    public class UserProfileWizard
    {
        /// <summary>
        /// [Description("Basics")]
        /// Basics = 1,//GeneralSettingsStep = 1,
        /// [Description("Contact")]
        /// Contact = 2,//AddressAndPhoneSettingStep = 2,
        /// [Description("Showcase")]
        /// Showcase = 3,//ShowcaseSettingStep = 3,
        /// [Description("Done")]
        /// Done = 4,
        /// </summary>
        public int CurrentStepId { get; set; }
        public MicroUserWizardStep CurrentStep { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public Uri Avatar { get; set; }
        public string TagLine { get; set; }
        public string BrieflyDescribeYourself { get; set; }
        public string Phone { get; set; }
        public List<MicroUserAddress> Contacts { get; set; }
        public List<MicroUserShowcase> Showcases { get; set; }
    }
    public class CommunityParameter
    {
        public string Keyword { get; set; }
        /// <summary>
        /// 1.Order by forename A-Z
        /// 2.Order by forename Z-A
        /// 3.Order by surname A-Z
        /// 4.Order by surname Z-A
        /// 5.Order by date added
        /// 6.Order by latest activity
        /// </summary>
        public int OrderBy { get; set; }
        /// <summary>
        /// 1.LinkBusiness
        /// 2.LinkUsers
        /// 3. pending
        /// </summary>
        public int ContactType { get; set; }
        //public string c2cQbicleKey { get; set; }
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// Manual set value on rules
        /// </summary>
        public bool ShownAll { get; set; } = false;
        /// <summary>
        /// Manual set value on rules
        /// </summary>
        public bool ShownFavourite { get; set; } = false;
        /// <summary>
        /// Manual set value on rules
        /// </summary>
        public bool ShownRequest { get; set; } = false;
        /// <summary>
        /// Manual set value on rules
        /// </summary>
        public bool ShownSent { get; set; } = false;
        /// <summary>
        /// Manual set value on rules
        /// </summary>
        public bool ShownBlocked { get; set; } = false;
        /// <summary>
        /// Current user, need update value before filter
        /// </summary>
        public string UserId { get; set; }
    }
}
