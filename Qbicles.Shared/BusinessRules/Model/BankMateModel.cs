using Qbicles.Models;
using Qbicles.Models.MyBankMate;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Model
{
    public class BankMateModel
    {
        public QbicleDomain Domain { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public int bankId { get; set; }
        public string address { get; set; }
        public string countryCode { get; set; }
        public string phone { get; set; }
        public string accountName { get; set; }
        public string IBAN { get; set; }
        public string NUBAN { get; set; }
    }
    public class BankmateTransactionsFilterModel
    {
        public List<ApplicationUser> Creators { get; set; } = new List<ApplicationUser>();
        public List<TraderCashAccount> ExternalBanks { get; set; } = new List<TraderCashAccount>();
    }
}
