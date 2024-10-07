using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    /// <summary>
    /// This class inherits from the ApprovalReqHistory log because this log is used to record the extra infomation associated with
    /// processing a Purchase through its stages.
    /// </summary>
    [Table("trad_purchaseprocesslog")]
    public class PurchaseProcessLog //: ApprovalReqHistory
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated Transfer at the time the Log is created
        /// </summary>
        [Required]
        public TraderPurchaseStatusEnum PurchaseStatus { get; set; }

        /// <summary>
        /// This property records the Purchase with which the log is associated
        /// </summary>
        [Required]
        public virtual TraderPurchase AssociatedPurchase { get; set; }


        /// <summary>
        /// This property associates the Purchase Log that was created when the Purchase was updated
        /// </summary>
        [Required]
        public virtual PurchaseLog AssociatedPurchaseLog { get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the update occurred.
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual ApprovalReqHistory ApprovalReqHistory { get; set; }
    }
}
