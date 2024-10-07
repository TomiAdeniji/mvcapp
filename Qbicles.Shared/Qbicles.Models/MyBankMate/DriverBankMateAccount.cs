using Qbicles.Models.Trader;
using Qbicles.Models.Trader.DDS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.MyBankMate
{
    public class DriverBankmateAccount : TraderCashAccount
    {
        public DriverBankmateAccount()
        {
            BankmateType = BankmateAccountType.Driver;
        }
        public virtual Driver Driver { get; set; }
    }
}
