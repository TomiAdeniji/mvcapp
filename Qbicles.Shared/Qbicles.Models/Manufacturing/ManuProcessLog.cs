using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Qbicles.Models.Manufacturing
{
    /// <summary>
    /// This class inherits from the ApprovalReqHistory log because this log is used to record the extra infomation associated with
    /// processing a Manufacturing Job through its stages.
    /// </summary>
    [Table("trad_manuprocesslog")]
    public class ManuProcessLog
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated Manufacturing Job at the time the Log is created
        /// </summary>
        [Required]
        public ManuJobStatus ManuJobStatus { get; set; }

        /// <summary>
        /// This property records the Manufacturing Job with which the log is associated
        /// </summary>
        [Required]
        public virtual ManuJob AssociatedManuJob { get; set; }


        /// <summary>
        /// This property associates the Transfer Log that was created when the Transfer was updated
        /// </summary>
        [Required]
        public virtual ManufacturingLog AssociatedManuLog { get; set; }


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

