using Qbicles.Models.Trader.Movement;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Inventory
{

    /// <summary>
    /// This class is used to record a log of the updates to Inventory.
    /// It will eventually used as a means to queue updates to a particular Inventory at a Location, for an Item
    /// </summary>
    [Table("trad_inventoryupdatelog")]
    public class InventoryUpdateLog
    {
        /// <summary>
        /// A unique identfier for the log
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The date in which the log was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// THe curren tuser at the time the log was created
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// The Transfer, for which the status update caused the Inventory update
        /// </summary>
        [Required]
        public virtual TraderTransfer AssociatedTransfer { get; set; }


        /// <summary>
        /// The Domain in which this process is taking place
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// The InventoryDetail that is associated with the update
        /// </summary>
        [Required]
        public virtual InventoryDetail AssociatedInventoryDetail { get; set; }


        /// <summary>
        /// The Loctaio at which the update is occurring
        /// </summary>
        [Required]
        public virtual TraderLocation AssociatedLocation { get; set; }


        /// <summary>
        /// This boolean indicates, if true, that a Transfer In is causing the update, if false a Transfer Out is causing the update.
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsTransferIn { get; set; }


        /// <summary>
        /// This is the Item for which the update is taking place
        /// </summary>
        [Required]
        public virtual TraderItem AssociatedItem { get; set; }


        /// <summary>
        /// This boolean is set to false when the log entry is initiall made, then set to true when it is completed
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// This is the DateTime at which the record is marked complete.
        /// </summary>
        [Required]
        public DateTime CompletedDate { get; set; }

    }
}