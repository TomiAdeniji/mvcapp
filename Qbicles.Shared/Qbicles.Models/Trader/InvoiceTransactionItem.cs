using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_InvoiceTransactionItems")]
    public class InvoiceTransactionItems
    {
        public int Id { get; set; }

        public virtual TraderTransactionItem TransactionItem { get; set; }

        public decimal InvoiceValue { get; set; }
        /// <summary>
        /// Total tax of the item with the quantity as 1
        /// </summary>
        public decimal? InvoiceTaxValue { get; set; }
        public decimal InvoiceDiscountValue { get; set; }
        public decimal InvoiceItemQuantity { get; set; }
    }
}
