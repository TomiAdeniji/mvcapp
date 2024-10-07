using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    public class ExchangeRate
    {
        [Required]
        public int Id { get; set; }
        public virtual TradeOrderB2B Order { get; set; }
        [Required]
        public string SellingDomainCurrencySymbol { get; set; }
        [Required]
        public string BuyingDomainCurrencySymbol { get; set; }
        //Amount in the seller's currency
        [Required]
        public decimal AmountSellerCurrency { get; set; }
        //Equivalent amount in your desired currency
        [Required]
        public decimal AmountBuyerCurrency { get; set; }
        [Required]
        public decimal ExchangeRateValue { get; set; }
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
