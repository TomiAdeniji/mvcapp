using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Inventory
{
    [Table("trad_StockAuditItem")]
    public class StockAuditItem
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public StockAudit StockAudit { get; set; }

        [Required]
        public virtual TraderItem Product { get; set; }

        [Required]
        public virtual ProductUnit AuditUnit { get; set; }


        public decimal OpeningCount { get; set; }

        public decimal ClosingCount { get; set; }

        public List<Batch> RelevantBatches { get; set; } = new List<Batch>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual InventoryDetail InventoryDetail { get; set; }

        public decimal Variance { get; set; }

    }
}

