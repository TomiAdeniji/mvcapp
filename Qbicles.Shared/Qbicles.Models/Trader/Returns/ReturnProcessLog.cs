using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.Returns;

namespace Qbicles.Models.Trader.Movement
{
    /// <summary>
    /// This class inherits from the ApprovalReqHistory log because this log is used to record the extra infomation associated with
    /// processing a Sale through its stages.
    /// </summary>
    [Table("trad_returnprocesslog")]
    public class ReturnProcessLog
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated Transfer at the time the Log is created
        /// </summary>
        [Required]
        public TraderReturnStatusEnum ReturnStatus { get; set; }

        /// <summary>
        /// This property records the Sale with which the log is associated
        /// </summary>
        [Required]
        public virtual TraderReturn AssociatedReturn { get; set; }


        /// <summary>
        /// This property associates the Sale Log that was created when the Sale was updated
        /// </summary>
        [Required]
        public virtual ReturnLog AssociatedReturnLog { get; set; }
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
