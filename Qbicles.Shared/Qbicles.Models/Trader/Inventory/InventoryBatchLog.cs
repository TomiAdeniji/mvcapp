using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Inventory
{
    [Table("trad_inventorybatchlog")]
    public class BatchLog
    {
        public int Id { get; set; }

        [Required]
        public virtual Batch ParentBatch { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public decimal OriginalQuantity { get; set; }

        public decimal CostPerUnit { get; set; }

        public decimal CurrentBatchValue { get; set; }

        public decimal UnusedQuantity { get; set; }

        public virtual InventoryDetail InventoryDetail { get; set; }

        public virtual Movement.TraderTransferItem ParentTransferItem { get; set; }


        public BatchDirection Direction { get; set; }


        [Required]
        public virtual ApplicationUser LasteUpdatedBy { get; set; }


        [Required]
        public DateTime LasteUpdatedDate { get; set; }


        public BatchLogReason Reason { get; set; }


    }

    public enum BatchLogReason
    {
        TransferIn = 1,
        TransferOut = 2,
        StockAdjustmentUp = 3,
        StockAdjustmentDown = 4 

    }
}
