using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_paymentprocesslog")]
    public class PaymentProcessLog
    {

        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated Payment at the time the Log is created
        /// </summary>
        [Required]
        public TraderPaymentStatusEnum PaymentStatus { get; set; }

        /// <summary>
        /// This property records the CashAccountTransaction  with which the log is associated
        /// </summary>
        [Required]
        public virtual CashAccountTransaction AssociatedTransaction {get; set;}

        /// <summary>
        /// This property associates the CashAccountTransaction Log that was created when the CashAccountTransaction was updated
        /// </summary>
        [Required]
        public virtual CashAccountTransactionLog AssociatedCashAccountTransactionLog { get; set; }


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
