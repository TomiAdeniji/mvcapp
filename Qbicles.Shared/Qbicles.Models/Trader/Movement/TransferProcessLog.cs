using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement
{
    /// <summary>
    /// This class inherits from the ApprovalReqHistory log because this log is used to record the extra infomation associated with
    /// processing a Transfer through its stages.
    /// </summary>
    [Table("trad_transferprocesslog")]
    public class TransferProcessLog
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated Transfer at the time the Log is created
        /// </summary>
        [Required]
        public TransferStatus TransferStatus { get; set; }

        /// <summary>
        /// This property records the Transfer with which the log is associated
        /// </summary>
        [Required]
        public virtual TraderTransfer AssociatedTransfer { get; set; }


        /// <summary>
        /// This property associates the Transfer Log that was created when the Transfer was updated
        /// </summary>
        [Required]
        public virtual TransferLog AssociatedTransferLog { get; set; }

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
