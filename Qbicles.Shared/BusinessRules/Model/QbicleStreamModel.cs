using System;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class QbicleStreamModel
    {
        /// <summary>
        /// List Date Range Activities and Posts
        /// </summary>
        public List<DatesQbicleStream> Dates { get; set; }
        /// <summary>
        /// List QbicleActivity is Pinned
        /// </summary>
        public List<int> Pinneds { get; set; }
        public int TotalCount { get; set; }
        public bool IsFilterDiscussionOrder { get; set; } = false;
    }
    public class DatesQbicleStream
    {
        public DateTime Date { get; set; }
        /// <summary>
        /// List Posts by Date Range  and Fillters
        /// </summary>
        //public List<QbiclePost> Posts { get; set; }
        //public List<QbicleActivity> Activities { get; set; }
        public List<dynamic> Activities { get; set; }

    }
    public class QbicleFillterModel
    {
        public string Key { get; set; }
        public int QbicleId { get; set; }
        public int Size { get; set; }
        public List<int> TopicIds { get; set; }
        public List<string> ActivityTypes { get; set; }
        public List<string> Apps { get; set; }
        public string UserId { get; set; }
        public string Daterange { get; set; }
        /// <summary>
        /// 1: Businesses, 2: Individual
        /// </summary>
        public int Type { get; set; }
    }

    public class CalendarFilterModel
    {
        public int QbicleId { get; set; }
        public string Type { get; set; }
        public string Day { get; set; }
        public string Keyword { get; set; }
        public string Orderby { get; set; }
        public short[] Types { get; set; }
        public int[] Topics { get; set; }
        public string[] Peoples { get; set; }
        public string[] Apps { get; set; }
        public int PageSize { get; set; } = HelperClass.myDeskPageSize;
        public int PageIndex { get; set; }
    }

    public class DefaultMedia
    {
        public string Id { get; set; }
        public string FilePath { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
    }
    public class ClientInfoModel
    {
        public string CurrentUserId { get; set; }
        public int CurrentTaskId { get; set; }
        public int CurrentAlertId { get; set; }
        public int CurrentEventId { get; set; }
        public int CurrentMediaId { get; set; }
        public int CurrentLinkId { get; set; }
        public int CurrentApprovalId { get; set; }
        public int CurrentDiscussionId { get; set; }
        public int CurrentJournalEntryId { get; set; }
    }

    public class CommunityPaggingModel
    {
        public string Key { get; set; }
        public int QbicleId { get; set; }
        public int PageIndex { get; set; } = 0;
        /// <summary>
        /// 1: Businesses, 2: Individual
        /// </summary>
        public int Type { get; set; } = 0;
        /// <summary>
        /// value for filter rule, do not include it to parameter
        /// </summary>
        public string UserId { get; set; }
    }

    public class MicroStreamParameter
    {
        /// <summary>
        /// Qbicle key
        /// </summary>
        public string Key { get; set; }
        public int QbicleId { get; set; }
        public int PageIndex { get; set; }
        public List<int> TopicIds { get; set; }
        public List<string> ActivityTypes { get; set; }
        public List<string> Apps { get; set; }
        public string UserId { get; set; }
        public string Daterange { get; set; }
        /// <summary>
        /// 1: Businesses, 2: Individual
        /// </summary>
        public int Type { get; set; }
    }

    /// <summary>
    /// Alert notification object stored in cookies, and use for get notification data
    /// use when received new notification ad id to list
    /// </summary>
    public class AlertNotificationModel
    {
        /// <summary>
        /// Notification Id
        /// </summary>
        public List<int> Ids { get; set; } = new List<int>();
        public bool IsShowAlertCustomer { get; set; } = true;
        public bool IsShowAlertBusiness { get; set; } = true;
        /// <summary>
        /// Last of notification style - from business or cutomer
        /// info/ business
        /// </summary>
        public string NotifyCircleClass { get; set; } = "info";
    } 
}
