using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{

    /// <summary>
    /// This is the archive Queue where old Orders are moved after a certain time period
    /// </summary>
    [Table("ods_PrepQueueArchive")]
    public class PrepQueueArchive
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This is the name of the Queue
        /// The name must be unique within the scope a Location
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// This is the Location with which the Queue is associated
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// This is a collection of the Orders with the queue
        /// </summary>
        public virtual List<QueueOrder> QueueOrders { get; set; }


        /// <summary>
        /// This is the user who created the Queue
        /// </summary>
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time when the Queue was created
        /// </summary>
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is the PrepQueue from which the QueueOrders are queued when they are archived
        /// </summary>
        public virtual PrepQueue ParentPrepQueue { get; set; }
    }
}
