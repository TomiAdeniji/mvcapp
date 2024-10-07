using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_qbiclediscussion")]
    public class QbicleDiscussion : QbicleActivity
    {
        public QbicleDiscussion()
        {
            this.ActivityType = ActivityTypeEnum.DiscussionActivity;
            this.App = ActivityApp.Qbicles;
        }
        [StringLength(36)]
        public string FeaturedImageUri { get; set; }
        public string Summary { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public DiscussionTypeEnum DiscussionType { get; set; }
        public enum DiscussionTypeEnum
        {
            [Description("Idea Discussion")]
            IdeaDiscussion = 1,
            [Description("Place Discussion")]
            PlaceDiscussion = 2,
            [Description("Qbicle Discussion")]
            QbicleDiscussion = 3,
            [Description("PoS Order Discussion")]
            PosOrderDiscussion = 4,
            [Description("Goal Discussion")]
            GoalDiscussion = 5,
            [Description("Performance Discussion")]
            PerformanceDiscussion = 6,
            [Description("ComplianceTask Discussion")]
            ComplianceTaskDiscussion = 7,
            [Description("CashManagement")]
            CashManagement = 8,
            [Description("B2C Product Menu Discussion")]
            B2CProductMenu = 9,
            [Description("B2C Order Discussion")]
            B2COrder = 10,
            [Description("B2B Partnership Discussion")]
            B2BPartnershipDiscussion = 11,
            [Description("B2B Order Discussion")]
            B2BOrder = 12,
            [Description("Order Cancellation Discussion")]
            OrderCancellation = 13,
            [Description("B2B Catalog Discussion")]
            B2BCatalogDiscussion = 14,
            [Description("Order Print Check Discussion")]
            OrderPrintCheck = 15,
        }
    }
}