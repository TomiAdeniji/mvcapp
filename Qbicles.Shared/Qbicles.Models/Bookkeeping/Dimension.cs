using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.Reorder;
using Qbicles.Models.Catalogs;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_dimensions")]
    public class TransactionDimension
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        //public virtual List<BKAccount> DefaultDimensionAccounts { get; set; } = new List<BKAccount>();
        public virtual List<BKTransaction> DefaultDimensionTransactions { get; set; } = new List<BKTransaction>();
        public virtual List<TraderTransactionItem> TraderTransactionItems { get; set; } = new List<TraderTransactionItem>();
        public virtual List<TransactionItemLog> TransactionItemLogs { get; set; } = new List<TransactionItemLog>();
        public virtual List<Catalog> PosMenus { get; set; } = new List<Catalog>();
        public virtual List<ReorderItem> ReorderItems { get; set; } = new List<ReorderItem>(); 

    }
}