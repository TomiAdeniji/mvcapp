using Qbicles.Models.Attributes;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
    [Table("trad_tradertransactionitem")]
    public class TraderTransactionItem
    {
        public int Id { get; set; }

        public virtual List<TransactionItemLog> Logs { get; set; } = new List<TransactionItemLog>();

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public virtual TraderItem TraderItem { get; set; }

        /// <summary>
        /// %
        /// This is the discount applied to the item
        /// It is a percentage i.e. a value of 10 means 10%
        /// It is a record of the discount i.e. the discount has already 
        /// been applied to the GrossPrice, 
        /// this field is simply recording what discount has been applied
        /// </summary>
        [DecimalPrecision(10, 7)]
        public decimal Discount { get; set; }

        public virtual List<TransactionDimension> Dimensions { get; set; } = new List<TransactionDimension>();

        public decimal Quantity { get; set; }

        public virtual ProductUnit Unit { get; set; }
        [DecimalPrecision(18, 7)]
        public decimal CostPerUnit { get; set; }
        [DecimalPrecision(18, 7)]
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
        [DecimalPrecision(18, 7)]
        public decimal PriceBookPriceValue { get; set; }


        /// <summary>
        /// The SalePricePerUnit is the actual price that is set for the sale.
        /// This value defaults to the PriceBookPriceValue when the PriceBookPrice 
        /// is attached to the TraderTransactionItem, but can be overwritten by the user 
        /// when adding or editing a Sale.
        /// This is the price per unit of the item
        /// </summary>
        [DecimalPrecision(18, 7)]
        public decimal SalePricePerUnit { get; set; }



        /// <summary>
        /// This the total Price for the Quantity of items in the sale
        /// i.e. Quantity * SalePricePerUnit
        /// </summary>
        [DecimalPrecision(18, 7)]
        public decimal Price { get; set; }



        public virtual List<OrderTax> Taxes { get; set; } = new List<OrderTax>();

        public string StringTaxRates(CurrencySetting setting)
        {
            if (this.Taxes == null || !this.Taxes.Any())
                return 0.ToString("N" + (int)setting.DecimalPlace);

            var stringTaxes = "";
            this.Taxes.ForEach(s =>
            {
                var taxValueNumber = s.Value * this.Quantity;
                var taxValue = $"{taxValueNumber.ToString("N" + (int)setting.DecimalPlace)}";
                stringTaxes += $"{taxValue} ({s.TaxRate.Name}){Environment.NewLine}";
            });

            return stringTaxes;




            //return !this.Taxes.Any() ? 0.ToString("N" + (int)setting.DecimalPlace) : string.Join(Environment.NewLine, this.Taxes.Select(s => $"{s.Value.ToString("N" + (int)setting.DecimalPlace)} ({s.StaticTaxRate?.Name ?? "Tax free"})"));
        }

        public decimal SumTaxRates()
        {
            return this.Taxes == null ? 0 : (this.Taxes.Sum(s => s.StaticTaxRate?.Rate ?? 0) / 100);
        }
        public decimal SumTaxRatesPercent()
        {
            return this.Taxes == null ? 0 : this.Taxes.Sum(s => s.StaticTaxRate?.Rate ?? 0);
        }
    }
}
