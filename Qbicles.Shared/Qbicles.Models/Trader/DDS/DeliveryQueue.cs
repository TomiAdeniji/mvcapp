using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.DDS
{
    /// <summary>
    /// This is the class for managing the delivery queue at a location
    /// </summary>
    [Table("dds_DeliveryQueue")]
    public class DeliveryQueue
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
        /// This is a collection of the Deliveries associated with the queue
        /// </summary>
        public virtual List<Delivery> Deliveries { get; set; }


        /// <summary>
        /// This is the user who created the Queue
        /// </summary>
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time when the Queue was created
        /// </summary>
        public DateTime CreatedDate { get; set; }



        /// <summary>
        /// An Order Queue must be associated with a Delivery Queue.
        /// The Orders from the Order Queue, 
        ///     where they are for Home delivery,
        ///     are available for adding to this Delivery Queue
        /// </summary>
        public virtual PrepQueue PrepQueue { get; set; }


    }
}
