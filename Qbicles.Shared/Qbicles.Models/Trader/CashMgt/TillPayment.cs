using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;
namespace Qbicles.Models.Trader.CashMgt
{
    [Table("trad_tillpayment")]
    public class TillPayment: DataModelBase
    {

        [Required]
        public TillPaymentDirection Direction { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public virtual Till AssociatedTill { get; set; }

        [Required]
        public virtual Safe AssociatedSafe { get; set; }

        [Required]
        public virtual WorkGroup WorkGroup { get; set; }

        public virtual QbicleDiscussion Discussion { get; set; }

        public virtual ApprovalReq TillPaymentApprovalProcess { get; set; }
        
        public TraderTillPaymentStatusEnum Status { get; set; } = TraderTillPaymentStatusEnum.PendingReview;

        public enum TraderTillPaymentStatusEnum
        {
            [Description("Awaiting review")]
            PendingReview = 1,
            [Description("Awaiting approval")]
            PendingApproval = 2,
            [Description("Denied")]
            Denied = 3,
            [Description("Approved")]
            Approved = 4,
            [Description("Discarded")]
            Discarded = 5
        }

        public enum TillPaymentDirection
        {
            [Description("In to Till")]
            InToTill = 1,
            [Description("Out of Till")]
            OutOfTill = 2
        }
    }
}
