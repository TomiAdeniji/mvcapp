using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_currencysettings")]
    public class CurrencySetting
    {
        /// <summary>
        /// The unique ID to identify the spannered CurrencySetting in the database
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// This is the Domain with which this CurrencySetting is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        /// <summary>
        /// Eg:₦ (NGN),€ (EUR),£ (GBP),$ (USD)
        /// </summary>
        public string CurrencySymbol { get; set; }

        [NotMapped]
        public string CurrencyName
        {
            get
            {
                switch (CurrencySymbol)
                {
                    case "₦":
                        return "NGN";
                    case "€":
                        return "EUR";
                    case "£":
                        return "GBP";
                    case "$":
                        return "USD";
                }
                return CurrencySymbol;
            }
        }

        public SymbolDisplayEnum SymbolDisplay { get; set; }
        public DecimalPlaceEnum DecimalPlace { get; set; }
        /// <summary>
        /// Eg: Prefixed (₦1,000),Suffixed (1,000₦)
        /// </summary>
        public enum SymbolDisplayEnum
        {
            Prefixed = 0,
            Suffixed = 1
        }
        /// <summary>
        /// Eg: None, 1 (1,000.0), 2 (1,000.00)
        /// </summary>
        public enum DecimalPlaceEnum
        {
            None = 0,
            One = 1,
            Two = 2,
            Three = 3,
        }
    }
}
