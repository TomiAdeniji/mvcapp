using Qbicles.Models.Bookkeeping;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    /// <summary>
    /// This class is used to represent the taxes associated with either a Variant or Extra
    /// </summary>
    [Table("ods_OrderTax")]
    public class OrderTax
    {
        public int Id { get; set; }

        /// <summary>
        /// This the TaxRate definition in the system with which the OrderTax is associated.
        /// The ID for this TaxRate is provided as part of the Order submitted by the PoS
        /// </summary>
        public virtual TaxRate TaxRate { get; set; }

        public virtual TaxRate StaticTaxRate { get; set; }
        /// <summary>
        /// This is the value (amount in money) of the tax
        /// </summary>
        public decimal Value { get; set; }
    }
}
