using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_InvoiceProcessLog")]
    public class InvoiceProcessLog
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated Transfer at the time the Log is created
        /// </summary>
        [Required]
        public TraderInvoiceStatusEnum InvoiceStatus { get; set; }

        /// <summary>
        /// This property records the Sale with which the log is associated
        /// </summary>
        [Required]
        public virtual Invoice AssociatedInvoice { get; set; }


        /// <summary>
        /// This property associates the Sale Log that was created when the Sale was updated
        /// </summary>
        [Required]
        public virtual InvoiceLog AssociatedInvoiceLog { get; set; }
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
