using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_inventorydetailLog")]
    public class InventoryDetailLog
    {
        [Required]
        public int Id { get; set; }


        [Required]
        public virtual InventoryDetail ParentInventoryDetail { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual TraderItem Item { get; set; }


        [Required(ErrorMessage = "Unit cost is required")]
        public decimal UnitCost { get; set; }



        public int MinInventorylLevel { get; set; }

        public int MaxInventoryLevel { get; set; }

        public decimal CurrentInventoryLevel { get; set; }




        public virtual Recipe CurrentRecipe { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        public virtual List<Batch> InventoryBatches { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        [Required]
        public DateTime LastUpdatedDate { get; set; }


        public virtual List<InventoryDetailLog> Logs { get; set; }

        /// <summary>
        /// This is the ProductUnit used to reorder the associated item, for this inventory at this location.
        /// </summary>
        public virtual ProductUnit ReorderUnit { get; set; }


        /// <summary>
        /// This is the quantity of the item, measured in ReorderUnits, below which an alert must be generated 
        /// to indicate that the item must be ordered for this inventory at this location
        /// </summary>
        public decimal ReorderLevel { get; set; }


        /// <summary>
        /// This is the average cost per unit of the current inventory for this item at this location.
        /// It takes into account the different costs per item in the associated batches and the quantity remaining in each 
        /// batch that has an UnusedQuantity greater than 0.
        /// 
        /// It is intended that this value will be calculated after a TransferIn or TransferOut has occurred
        /// </summary>
        public decimal AverageCost { get; set; }


        /// <summary>
        /// This is the cost per unit of the latest batch associated with the inventory detail
        /// 
        /// It is intended that this value will be set after a TransferIn or TransferOut has occurred
        /// </summary>
        public decimal LatestCost { get; set; }

    }
}