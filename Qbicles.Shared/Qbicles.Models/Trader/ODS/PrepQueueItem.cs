using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.PoS;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_PrepQueue")]
    public class PrepQueue
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



        ///// <summary>
        ///// An Order Queue must be associated with a Delivery Queue.
        ///// The Orders from the Order Queue, 
        /////     where they are for Home delivery,
        /////     are available for adding to the Delivery Queue 
        ///// </summary>
        //public virtual DeliveryQueue DeliveryQueue { get; set; } 



        /// <summary>
        /// This is a list of the PoS Devices that are associated with this PrepQueue
        /// </summary>
        public virtual List<PosDevice> AssociatedPosDevices { get; set; } = new List<PosDevice>();


    }
}
