using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    public class PaymentBookkeepingLog : TraderJournalEntryLog
    {
        public PaymentBookkeepingLog()
        {
            this.Type = TraderJournalEntryLogType.Payment;
        }

        public virtual CashAccountTransaction Payment { get; set; }
    }
}
