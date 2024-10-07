using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.MyBankMate
{
    public class ExternalBankAccount : TraderCashAccount
    {
        public ExternalBankAccount()
        {
            BankmateType = BankmateAccountType.ExternalBank;
        }
        public string NUBAN { get; set; }

        public string IBAN { get; set; }

        public virtual TraderAddress Address { get; set; }

        public string PhoneNumber { get; set; }

        public virtual Bank Bank { get; set; }
    }
}
