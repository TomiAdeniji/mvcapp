using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_EmailPostApproval")]
    public class EmailPostApproval
    {
        public int Id { get; set; }

        public virtual ApprovalReq Activity { get; set; }

        public SalesMktApprovalStatusEnum ApprovalStatus { get; set; }

        public virtual CampaignEmail CampaignEmail { get; set; }

        public virtual SalesMarketingWorkGroup WorkGroup { get; set; }

        public DateTime? ApprovedDate { get; set; }
    }

}
