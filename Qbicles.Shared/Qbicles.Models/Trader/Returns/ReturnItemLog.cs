using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Returns
{
    [Table("trad_returnitemlog")]
    public class ReturnItemLog
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the Return Item of which this is a log
        /// </summary>
        [Required]
        public virtual ReturnItem ParentReturnItem { get; set; }


        /// <summary>
        /// This is the Return TraderReturn with which this return item is associated
        /// </summary>
        [Required]
        public virtual TraderReturn Return { get; set; }


        /// <summary>
        /// This is the Sale TraderTransactionItem with which this return item is associated
        /// </summary>
        [Required]
        public virtual TraderTransactionItem SaleItem { get; set; }


        /// <summary>
        /// The Quantity of the item returned
        /// This Quantity must be less than or equal to the Quantity of the Associated TraderTransactionItem
        /// </summary>
        public decimal ReturnQuantity { get; set; }


        /// <summary>
        /// The Credit to be given for the item returned
        /// </summary>
        public decimal Credit { get; set; }


        /// <summary>
        /// A boolean to indicate whether the Quantity of item is returned to inventory or not
        /// If it is not returned to Inventory, there is a Quantity to Return AND there is an InventoryDetail for the item then the item is 'Wasted'
        /// </summary>
        [Required]
        [Column(TypeName = "bit")]
        public bool IsReturnedToInventory { get; set; }

        /// <summary>
        /// The UTC date and time on which the Return Item LOG was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The Application user who created the return item LOG
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        /// <summary>
        /// The date on which the ReturnItem was last updated
        /// </summary>
        [Required]
        public DateTime LastUpdatedDate { get; set; }


        /// <summary>
        /// The Application user who last updated the return item
        /// </summary>
        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

    }
}
