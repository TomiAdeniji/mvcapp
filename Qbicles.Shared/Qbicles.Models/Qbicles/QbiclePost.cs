using Qbicles.Models.Base;
using Qbicles.Models.Bookkeeping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Qbicles.Models
{
    /// <summary>
    /// Concrete class for Qbicle post
    /// </summary>
    /// 
    [Table("qb_qbiclepost")]
    public class QbiclePost : DataModelBase
    {

        [Required]
        public string Message { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime StartedDate { get; set; }
        public DateTime TimeLineDate { get; set; }
        public virtual QbicleSet Set { get; set; }

        public virtual Topic Topic { get; set; }

        public virtual List<QbicleMedia> Media { get; set; } = new List<QbicleMedia>();

        public virtual List<MyTag> Folders { get; set; } = new List<MyTag>();

        public virtual JournalEntry JournalEntry { get; set; }
        public virtual BKTransaction BKTransaction { get; set; }
        /// <summary>
        /// QBIC-3965: If the creator of the Activity or Post is the Customer in a B2CQBicle or B2COrder then this value must be set to true at the time the Activity/post is created
        /// This property should have the default value of false,
        /// </summary>
        public bool IsCreatorTheCustomer { get; set; } = false;

    }
}