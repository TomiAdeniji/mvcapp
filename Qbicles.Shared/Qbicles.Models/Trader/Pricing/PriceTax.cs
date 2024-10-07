using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Pricing
{
    [Table("trad_pricetax")]
    public class PriceTax
    {
        public int Id { get; set; }

        public string TaxName { get; set; }

        public decimal Rate { get; set; }

        /// <summary>
        /// price.NetPrice * (taxitem.Rate / 100)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// This property is used to hold a STATIC copy of the TaxRate that is used in the creation of the PriceTax
        /// </summary>
        public virtual TaxRate TaxRate { get; set; }
    }
}
