using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.Pricing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    /// <summary>
    /// A TraderTransactionItem is the line item for a product in a Sale or Purchase order.
    /// It describes the item in terms of Cost, QuantityAtPickup, Units
    /// E.G.
    ///     A Sale might have a line item of 
    ///         10(QuantityAtPickup) Cans(Unit) of Soda, costing $2 per can (Unit Cost), with a total cost $20 (Cost)
    ///     These properties make up the TraderTransactionItem
    ///         
    /// </summary>
    [Table("trad_transactionitemlog")]
    public class TransactionItemLog
    {
        public int Id { get; set; }

        public virtual TraderTransactionItem AssociatedTransactionItem { get; set; }

        public virtual TraderItem TraderItem { get; set; }

        public decimal Discount { get; set; }

        public virtual List<TransactionDimension> Dimensions { get; set; } = new List<TransactionDimension>();

        public decimal Quantity { get; set; }

        public virtual ProductUnit Unit { get; set; }

        public decimal CostPerUnit { get; set; }

        public decimal Cost { get; set; }


        /// <summary>
        /// The TraderTransactionItem must be transferred, i.e. it must be shipped between point A and Point B.
        /// Normally the total quantity of a TraderTransactionItem would be shipped in 'one go'.
        /// In that case the TransferItem would have one element which would be a link to the one TraderTransferItem in the one TraderTransfer
        /// However, the transfer of a TraderTransactionItem might have to be split up into multiple transfers.
        /// In that case, each element in the TransferItems, would refer to one TraderTransferItem in one TraderTransfer, BUT there would be multiple 
        /// Transfers.
        /// </summary>
        public virtual List<TraderTransferItem> TransferItems { get; set; } = new List<TraderTransferItem>();



        /// ONLY FOR A SALE!!!!!!!!!!!!!!!!!!!!!!!!!

        /// The following properties are associated with a TraderTransactionItem for a SALE.

        /// <summary>
        /// The PriceBookPrice is the Price Object from the PriceBook, 
        /// associated with the Item,
        /// at the OriginatingLocation of the Sale,
        /// associated with the SalesChannel of the Sale
        /// </summary>
        public virtual Price PriceBookPrice { get; set; }


        /// <summary>
        /// The PriceBookPriceValue is the Price.Value at the time the PriceBookPrice is 
        /// attached to the TraderTransactionItem
        /// </summary>
        public decimal PriceBookPriceValue { get; set; }


        /// <summary>
        /// The SalePricePerUnit is the actual price that is set for the sale.
        /// This value defaults to the PriceBookPriceValue when the PriceBookPrice 
        /// is attached to the TraderTransactionItem, but can be overwritten by the user 
        /// when adding or editing a Sale.
        /// This is the price per unit of the item
        /// </summary>
        public decimal SalePricePerUnit { get; set; }



        /// <summary>
        /// This the total Price for the Quantity of items in the sale
        /// i.e. Quantity * SalePricePerUnit
        /// </summary>
        public decimal Price { get; set; }





    }
}
