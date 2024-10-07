using Qbicles.Models.Trader.Inventory;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement
{
    /// <summary>
    /// The TraderTransferItem is used to model the transfer a quantity of products or items from one location to another.
    /// In a Sale there will be a QuantityAtPickup of items to transfer between locations.
    /// That is modeled by the TraderTransactionItem, i.e. there could be 200 cans of Soda to transfer for x Cost etc
    /// However, it might not be possible to transfer the entire TraderTransactionItem in 'one go'.
    /// So, for each partial transfer of items, a TraderTransferItem is created. The cost etc is all based on the original TraderTransactionItem.
    /// </summary>
    [Table("trad_tradertransferitem")]
    public class TraderTransferItem
    {
 
        public int Id { get; set; }

        public virtual TraderItem TraderItem { get; set; }

        public decimal QuantityAtPickup { get; set; }

        public decimal QuantityAtDelivery { get; set; }

        public virtual ProductUnit Unit { get; set; }

        public virtual TraderTransactionItem TransactionItem { get; set; }

        public virtual TraderTransfer AssociatedTransfer { get; set; }

        public virtual List<Batch> InventoryBatches { get; set; }
    }
}