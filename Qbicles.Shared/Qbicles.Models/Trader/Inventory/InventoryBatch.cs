using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Inventory
{
    [Table("trad_inventorybatch")]
    public class Batch
    {
        public int Id { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public decimal OriginalQuantity { get; set; }

        public decimal CostPerUnit { get; set; }

        public decimal CurrentBatchValue { get; set; }

        public decimal UnusedQuantity { get; set; }

        public virtual InventoryDetail InventoryDetail { get; set; }

        public virtual TraderTransferItem ParentTransferItem { get; set; }

        public virtual List<BatchLog> Logs { get; set; }

        public BatchDirection Direction { get; set; }


        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        [Required]
        public DateTime LastUpdatedDate { get; set; }


        /// <summary>
        /// This flag indicates if the batch has been added because the 'raeal-world' nventory was found to be 
        /// lower than the database values
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsInvented { get; set; } = false;


    }


   

    public enum BatchDirection
    {
        In = 0, 
        Out = 1
    }
}