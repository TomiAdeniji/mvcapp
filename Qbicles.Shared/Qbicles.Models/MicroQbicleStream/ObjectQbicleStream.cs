using Qbicles.Models.Highlight;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using static Qbicles.Models.Notification;

namespace Qbicles.Models.MicroQbicleStream
{
    public class BaseModel
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }

    public class SelectFilter : BaseModel
    {
        public bool Selected { get; set; }
    }
    public class BaseModelImage : BaseModel
    {
        public Uri ImageUri { get; set; }
    }


    public class HighlightDomainFollows
    {
        /// <summary>
        /// Domain Id ( use domain Key)
        /// </summary>
        public string DomainId { get; set; }
        public string Name { get; set; }
        public Uri ImageUri { get; set; }
        public List<string> Tags { get; set; }
    }


    public class HighlightParameter
    {
        public int DomainId { get; set; }
        public string Key { get; set; }
        public List<string> Tags { get; set; }
        public int PageIndex { get; set; }
        public HighlightPostType TypeShowed { get; set; }
        public ListingType TypeSearch { get; set; }
        public bool IsBookmarked { get; set; }
        public bool IsFlagged { get; set; }
        public string CountryName { get; set; }
        public string EventDateRange { get; set; }
        public string NewsPublishedDate { get; set; }
        public List<int> PropertyFacilities { get; set; }
        public int BedroomNumber { get; set; }
        public int BathroomNumber { get; set; }
        public List<int> PropertyTypes { get; set; }
        public int AreaId { get; set; }
    }


    public class MicroHighlightFilterOption
    {
        public List<HighlightCategoryFilter> Categories2Filter { get; set; } = new List<HighlightCategoryFilter>();
        public List<string> Tags2Filter { get; set; } = new List<string>();
        public List<BaseModel> BedRoom2Filter { get; set; } = new List<BaseModel>();
        public List<BaseModel> BathRoom2Filter { get; set; } = new List<BaseModel>();
        public List<BaseModel> PropertyType2Filter { get; set; } = new List<BaseModel>();
        public List<BaseModel> PropertyFacilities2Filter { get; set; } = new List<BaseModel>();
    }

    public class HighlightCategoryFilter
    {
        public string Name { get; set; }
        public HighlightPostType TypeShowed { get; set; }
        public ListingType TypeSearch { get; set; }
    }

    public class HighlightPropertyInfo : BaseModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Location { get; set; }
        public string Price { get; set; }
        public string PropertyType { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public List<BaseModelImage> Images { get; set; } = new List<BaseModelImage>();
        public List<BaseModel> IncludedProperties { get; set; } = new List<BaseModel>();
    }

    public class HighlightArticle : BaseModelImage
    {
        public List<string> Tags { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatedDate { get; set; }
    }

    public class HighlightPromotionShareParameter
    {
        public List<string> ContactIds { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// post Id
        /// </summary>
        public int Id { get; set; }
        public string PromotionKey { get; set; }
    }

    /// <summary>
    /// Model of Micro Qbicle Streams
    /// </summary>
    public class MicroQbicleStream
    {
        public List<MicroDatesQbicleStream> MicroActivities { get; set; }
        public int TotalCount { get; set; }
    }
    /// <summary>
    /// MicroDatesQbicleStream {Date, Activities,Posts}
    /// </summary>
    public class MicroDatesQbicleStream
    {
        public string Date { get; set; }
        public List<MicroActivitesStream> Activities { get; set; }
    }

    public class MicroActivitesStream : BaseModel
    {
        /// <summary>
        /// DiscussionType B2C Order - B2COrderCreation discussion
        /// </summary>
        public int TraderId { get; set; }
        /// <summary>
        /// DiscussionType B2C Order - B2COrderCreation.TraderOrder.Id
        /// </summary>
        public int TradeOrderId { get; set; }
        /// <summary>
        /// DiscussionType B2C Order - B2COrderCreation
        /// </summary>
        public string TraderKey { get; set; }
        public string TradeOrderKey { get; set; }

        public int StreamId { get; set; } = 0;
        public string StreamName { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public string ActivityCreatedBy { get; set; } = "";
        public string TimelineDate { get; set; } = "";
        public string CreatedDate { get; set; } = "";
        public Uri UserAvatar { get; set; }
        public Uri MediaUri { get; set; }
        public string AppIcon { get; set; } = "";
        public bool IsOwned { get; set; } = false;
        public string ActivityHref { get; set; } = "";
        //public string MediaHref { get; set; } = "";
        public string TopicId { get; set; } = "";
        public string TopicName { get; set; } = "";
        public string PostMessage { get; set; } = "";
        public int StatusId { get; set; } = 0;
        public string StatusName { get; set; } = "";
        public string StatusColor { get; set; } = "";
        public string AppTitle { get; set; } = "";
        public string AppTitleMsg { get; set; } = "";
        public string TraderUri { get; set; } = "";
        public string ConsumeCountTask { get; set; } = "";
        public bool Pinned { get; set; } = false;
        public string UpdateReason { get; set; } = "";
        public string ActivityEventDate { get; set; } = "";
        public string ActivityEventTime { get; set; } = "";
        public string Location { get; set; } = "";
        public string MediaInfo { get; set; } = "";
        public string MediaExtension { get; set; } = "";
        public string LinkUri { get; set; } = "";
        public string DiscussionBreadcrumb { get; set; } = "";
        public string DiscussionDetail { get; set; } = "";
        public string TaskType { get; set; } = "";
        public string TaskPriority { get; set; } = "";
        public string WorkgroupName { get; set; } = "";
        public string TaskCompliance { get; set; } = "";
        public string TaskForms { get; set; } = "";
        public string TaskComplianceTotal { get; set; } = "";
        public string TaskRecurring { get; set; } = "";
        public string CampaignName { get; set; } = "";
        public string CampaignTitle { get; set; } = "";
        public string CampaignContent { get; set; } = "";
        public int CatalogId { get; set; } = 0;
        public string CatalogName { get; set; } = "";
        public int CatalogItems { get; set; } = 0;
        public string DomainKey { get; set; }
        public string CoveringNote { get; set; }
        public SalesChannelEnum SalesChannel { get; set; }
    }

    public class NotificationDetail
    {
        public int ElementId { get; set; }
        public ApplicationPageName AppendToPageType { get; set; }
        public string AppendToPageName { get; set; }
        public int AppendToPageId { get; set; }
        public string CreatedById { get; set; }
        public string AssociatedById { get; set; }
        public int CurrentQbicleId { get; set; }
        public int CurrentDomainId { get; set; }
        public bool HasActionToHandle { get; set; }
        public NotificationEventEnum Event { get; set; }
        public string EventName { get; set; }
        public string HtmlNotification { get; set; }
        public MicroActivitesStream MicroNotification { get; set; }
        /// <summary>
        /// QBIC-3965: If the creator of the Activity or Post is the Customer in a B2CQBicle or B2COrder then this value must be set to true at the time the Activity/post is created
        /// This property should have the default value of false,
        /// </summary>
        public bool IsCreatorTheCustomer { get; set; } = false;
        public QbicleType CreatorTheQbcile { get; set; } = QbicleType.Qbicle;
        /// <summary>
        /// Show alert
        /// </summary>
        public bool IsAlertDisplay { get; set; }
    }
    /// <summary>
    /// 1 Qbicle, 2 B2C Qbicles, 3 C2C Qbicle
    /// </summary>
    public enum QbicleType
    {
        Qbicle = 1,
        B2CQbicle = 2,
        C2CQbicle = 3
    }

    public class MicroStreams
    {
        public List<MicroStream> Streams { get; set; } = new List<MicroStream>();
        public int TotalPage { get; set; } = 0;
    }

    public class MicroStream //: BaseModel
    {
        public int Id { get; set; }
        /// <summary>
        /// Key of stream id (StreamType enum)
        /// </summary>
        public string Key { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// DiscussionType B2C Order - B2COrderCreation discussion
        /// </summary>
        public int TraderId { get; set; }
        /// <summary>
        /// DiscussionType B2C Order - B2COrderCreation.TraderOrder.Id
        /// </summary>
        public int TradeOrderId { get; set; }
        /// <summary>
        /// DiscussionType B2C Order - B2COrderCreation
        /// </summary>
        public string TraderKey { get; set; }
        public string TradeOrderKey { get; set; }

        public int StreamId { get; set; } = 0;
        public string StreamName { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public string ActivityCreatedBy { get; set; } = "";
        public DateTime TimelineDate { get; set; }
        public string CreatedDate { get; set; } = "";
        public Uri UserAvatar { get; set; }
        public Uri MediaUri { get; set; }
        public string AppIcon { get; set; } = "";
        public bool IsOwned { get; set; } = false;
        //public string ActivityHref { get; set; } = "";
        //public string MediaHref { get; set; } = "";
        public string TopicId { get; set; } = "";
        public string TopicName { get; set; } = "";
        public string PostMessage { get; set; } = "";
        public int StatusId { get; set; } = 0;
        public string StatusName { get; set; } = "";
        public string AppTitle { get; set; } = "";
        public string AppTitleMsg { get; set; } = "";
        public string TraderUri { get; set; } = "";
        public string ConsumeCountTask { get; set; } = "";
        public bool Pinned { get; set; } = false;
        public string UpdateReason { get; set; } = "";
        public string ActivityEventDate { get; set; } = "";
        public string ActivityEventTime { get; set; } = "";
        public string Location { get; set; } = "";
        public string MediaInfo { get; set; } = "";
        public string MediaExtension { get; set; } = "";
        public string LinkUri { get; set; } = "";
        public string DiscussionBreadcrumb { get; set; } = "";
        public string DiscussionDetail { get; set; } = "";
        public string TaskType { get; set; } = "";
        public string TaskPriority { get; set; } = "";
        public string WorkgroupName { get; set; } = "";
        public string TaskCompliance { get; set; } = "";
        public string TaskForms { get; set; } = "";
        public string TaskComplianceTotal { get; set; } = "";
        public string TaskRecurring { get; set; } = "";
        public string CampaignName { get; set; } = "";
        public string CampaignTitle { get; set; } = "";
        public string CampaignContent { get; set; } = "";
        public int CatalogId { get; set; } = 0;
        public string CatalogName { get; set; } = "";
        public int CatalogItems { get; set; } = 0;
        public string DomainKey { get; set; }
        public string CoveringNote { get; set; }
        public SalesChannelEnum SalesChannel { get; set; }
        public object ObjectData { get; set; }
    }
}
