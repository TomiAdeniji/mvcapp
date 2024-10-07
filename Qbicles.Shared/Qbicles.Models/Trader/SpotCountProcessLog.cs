using Qbicles.Models.Trader.Inventory;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement
{
    /// <summary>
    /// This class inherits from the ApprovalReqHistory log because this log is used to record the extra infomation associated with
    /// processing a SpotCount through its stages.
    /// </summary>
    [Table("trad_spotcountprocesslog")]
    public class SpotCountProcessLog
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated SpotCount at the time the Log is created
        /// </summary>
        [Required]
        public SpotCountStatus SpotCountStatus { get; set; }

        /// <summary>
        /// This property records the Sale with which the log is associated
        /// </summary>
        [Required]
        public virtual SpotCount AssociatedSpotCount { get; set; }


        /// <summary>
        /// This property associates the Sale Log that was created when the SpotCount was updated
        /// </summary>
        [Required]
        public virtual SpotCountLog AssociatedSpotCountLog { get; set; }
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
