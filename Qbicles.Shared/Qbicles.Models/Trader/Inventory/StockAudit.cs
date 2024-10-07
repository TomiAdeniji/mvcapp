using System;
using Qbicles.Models.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Inventory
{

    [Table("trad_StockAudit")]
    public class StockAudit : DataModelBase
    {

        [Required]
        public string Name { get; set; }

        public string Notes { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        public virtual List<StockAuditItem> ProductList { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        [Column(TypeName = "bit")]
        public bool IsStarted{ get; set; }

        public virtual ApplicationUser StartedBy { get; set; }

        public DateTime StartedDate { get; set; }



        [Column(TypeName = "bit")]
        public bool IsFinished { get; set; }

        public virtual ApplicationUser FinishedBy { get; set; }

        public DateTime FinishedDate { get; set; }

        [Required]
        public virtual WorkGroup WorkGroup { get; set; }


        public virtual ApprovalReq StockAuditApproval { get; set; }


        public ShiftAuditStatus Status { get; set; }
    }



    public enum ShiftAuditStatus
    {
        Draft = 0,
        Pending = 1,
        Reviewed = 2,
        Approved = 3,
        Discarded = 4,
        Denied = 5
    }
}
