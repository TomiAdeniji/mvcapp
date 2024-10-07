using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.Models.Trader.CashMgt.TillPayment;

namespace Qbicles.Models.Trader.CashMgt
{
    [Table("trad_tillpaymentprocesslog")]
    public class TillPaymentProcessLog
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This property records the status of the Associated Till Payment at the time the Log is created
        /// </summary>
        [Required]
        public TraderTillPaymentStatusEnum TillPaymentStatus { get; set; }

        /// <summary>
        /// This property records the TillPayment with which the log is associated
        /// </summary>
        [Required]
        public virtual TillPayment AssociatedTillPayment { get; set; }

        /// <summary>
        /// This property associates the Sale Log that was created when the TillPayment was updated
        /// </summary>
        [Required]
        public virtual TillPaymentLog AssociatedTillPaymentLog { get; set; }

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
