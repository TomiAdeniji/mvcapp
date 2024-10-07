using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.Models.CurrencySetting;

namespace Qbicles.BusinessRules.Model
{
    public class LoyaltyConversionCustomModel
    {
        public int RuleId { get; set; }
        public string ArchievedDate { get; set; }
        public string ArchievedBy { get; set; }
        public int Points { get; set; }
        public string Amount { get; set; }
    }

    public class MonibacBusinessModel
    {
        public int BusinessProfileId { get; set; }
        public string ContactKey { get; set; }
        public TraderContact Contact { get; set; }
        public string BusinessName { get; set; }
        public string BusinessLogoUri { get; set; }
        public decimal AccountBalance { get; set; }
        public string AccountBalanceString { get; set; }
        public decimal Points { get; set; }
        public decimal StoreCreditBalance { get; set; }
        public int ValidVouchersCount { get; set; }
        public string StoreCreditBalanceString { get; set; }
        public int DomainId { get; set; }
        public int QbicleId { get; set; }
    }

    public class StoreCreditExchangeModel
    {
        public string ContactKey { get; set; }
        public int StoreCreditAccountId { get; set; }
        public Uri LogoUri { get; set; }
        public string Name { get; set; }
        public decimal AccountBalance { get; set; }
        public string AccountBalanceString { get; set; }
        public decimal Point { get; set; }
        public decimal StoreCredit { get; set; }
        public string StoreCreditString { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal ConvertValue { get; set; }
        public string CreditReceived { get; set; }
        public string CurrencySymbol { get; set; }
        public DecimalPlaceEnum DecimalPlace { get; set; }
        public SymbolDisplayEnum SymbolDisplay { get; set; }
    }
}
