using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.ODS;

namespace Qbicles.Models.Trader.DDS
{
    /// <summary>
    /// This is the archive Queue where old Deliveries are moved after a certain time period
    /// </summary>
    [Table("ods_DeliveryQueueArchive")]
    public class DeliveryQueueArchive
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
        /// This is a collection of the Deliveries with the queue
        /// </summary>
        public virtual List<Delivery> Deliveries { get; set; } = new List<Delivery>();


        /// <summary>
        /// This is the user who created the Queue
        /// </summary>
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time when the Queue was created
        /// </summary>
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is the Delivery Queue from which the Deliveries are queued when they are archived
        /// </summary>
        public virtual DeliveryQueue ParentDeliveryQueue { get; set; }


        /// <summary>
        /// An Order Queue must be associated with a Delivery Queue.
        /// The Orders from the Order Queue, 
        ///     where they are for Home delivery,
        ///     are available for adding to this Delivery Queue
        /// </summary>
        public virtual PrepQueue PrepQueue { get; set; }
    }
}
