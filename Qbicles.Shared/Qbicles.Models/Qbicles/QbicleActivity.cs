using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    /// <summary>
    /// Baseclass for a qbiclke activity
    /// </summary>
    [Table("qb_qbicleactivities")]
    public abstract class QbicleActivity : DataModelBase
    {
        public QbicleActivity()
        {
            UpdateReason = ActivityUpdateReasonEnum.NoUpdates;
            IsVisibleInQbicleDashboard = true;
        }

        // enum defining the activity type.  Used when serialising/deserialisng entity to/from JSON
        public enum ActivityTypeEnum
        {
            [Description("Discussion Activity")]
            DiscussionActivity = 1,

            [Description("Task Activity")]
            TaskActivity = 2,

            [Description("Alert Activity")]
            AlertActivity = 3,

            [Description("Event Activity")]
            EventActivity = 4,

            [Description("Media Activity")]
            MediaActivity = 5,

            [Description("Post Activity")]
            PostActivity = 6,

            [Description("Qbicle")]
            QbicleActivity = 7,

            [Description("Approval Request")]
            ApprovalRequest = 8,

            [Description("Approval Activity")]
            ApprovalActivity = 9,

            [Description("Domain")]
            Domain = 10,

            [Description("Approval Request Application")]
            ApprovalRequestApp = 11,

            [Description("Link")]
            Link = 12,

            [Description("Remove Queue")]
            RemoveQueue = 13,

            [Description("Shared Highlight Post")]
            SharedHLPost = 14,

            [Description("Shared Loyalty Promotion")]
            SharedPromotion = 15,

            [Description("Order Cancellation Discussion")]
            OrderCancellation = 16,

            [Description("Order Print Check Discussion")]
            OrderPrintCheck = 18,
        };

        public enum ActivityStateEnum
        {
            [Description("Open")]
            Open = 1,

            [Description("Closed")]
            Closed = 2
        };

        [Required]
        public ActivityApp App { get; set; }

        public enum ActivityApp
        {
            [Description("Qbicles")]
            Qbicles = 0,

            [Description("Trader")]
            Trader = 1,

            [Description("Bookkepping")]
            Bookkeeping = 2,

            [Description("Sale and Marketing")]
            SalesAndMarketing = 3,

            [Description("Clean books")]
            CleanBooks = 4,

            [Description("Spannered")]
            Spannered = 5,

            [Description("Operator")]
            Operator = 6,

            [Description("Bankmate")]
            Bankmate = 7
        }

        public enum ActivityUpdateReasonEnum
        {
            NoUpdates = 0,

            [Description("New comment(s)")]
            NewComments = 1,

            [Description("New file(s)")]
            NewFiles = 2,

            [Description("Task completed")]
            ClosedTask = 3,

            [Description("Attendance changed")]
            EventWithdrawl = 4,

            [Description("Media version updated")]
            AddMediaVersion = 5,

            [Description("Media version deleted")]
            DelMediaVersion = 6,

            [Description("Reviewed")]
            ApprovalReviewed = 7,

            [Description("Approved")]
            ApprovalApproved = 8,

            [Description("Denied")]
            ApprovalDenied = 9
        };

        public string Name { get; set; }

        [Required]
        public DateTime TimeLineDate { get; set; }

        [Required]
        public virtual ActivityTypeEnum ActivityType { get; set; }

        public virtual ActivityStateEnum State { get; set; }

        public virtual ActivityUpdateReasonEnum UpdateReason { get; set; }

        [Required]
        public DateTime StartedDate { get; set; }

        [Required]
        public virtual ApplicationUser StartedBy { get; set; }

        public DateTime? ClosedDate { get; set; }

        public virtual ApplicationUser ClosedBy { get; set; }

        public DateTime? ProgrammedStart { get; set; }

        public DateTime? ProgrammedEnd { get; set; }

        public DateTime? ActualStart { get; set; }

        public DateTime? ActualEnd { get; set; }

        [Column(TypeName = "bit")]
        public bool isComplete { get; set; }

        public virtual Qbicle Qbicle { get; set; }

        public virtual List<QbicleMedia> Media { get; set; } = new List<QbicleMedia>();

        public virtual List<ApplicationUser> ActivityMembers { get; set; } = new List<ApplicationUser>();

        public virtual List<QbicleActivity> SubActivities { get; set; } = new List<QbicleActivity>();
        public virtual List<MyTag> Folders { get; set; } = new List<MyTag>();

        public virtual List<QbiclePost> Posts { get; set; } = new List<QbiclePost>();

        //[Required]
        public virtual Topic Topic { get; set; }

        [Column(TypeName = "bit")]
        public bool IsVisibleInQbicleDashboard { get; set; }

        public virtual ICollection<QbicleJob> Jobs { get; set; } = new List<QbicleJob>();

        public virtual QbicleSet AssociatedSet { get; set; }

        public virtual ICollection<QbicleRelated> Relateds { get; set; } = new List<QbicleRelated>();

        /// <summary>
        /// QBIC-3965: If the creator of the Activity or Post is the Customer in a B2CQBicle or B2COrder then this value must be set to true at the time the Activity/post is created
        /// This property should have the default value of false,
        /// </summary>
        public bool IsCreatorTheCustomer { get; set; } = false;

        /// <summary>
        /// Hangfire job id
        /// QBIC-3384: Event & Task notification points
        /// </summary>
        public string JobId { get; set; }
    }
}