using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_CampaignPostApproval")]
    public class CampaignPostApproval
    {
        public int Id { get; set; }

        public virtual ApprovalReq Activity { get; set; }

        public SalesMktApprovalStatusEnum ApprovalStatus { get; set; }

        public virtual SocialCampaignPost CampaignPost { get; set; }
        
        public virtual SalesMarketingWorkGroup WorkGroup { get; set; }

        public DateTime? ApprovedDate { get; set; }
    }

    public enum SalesMktApprovalStatusEnum
    {
        [Description("Awaiting Approval")]
        InReview = 1,
        [Description("Approved")]
        Approved = 2,
        [Description("Denied")]
        Denied = 3,
        [Description("Queued")]
        Queued=4
    }
}
