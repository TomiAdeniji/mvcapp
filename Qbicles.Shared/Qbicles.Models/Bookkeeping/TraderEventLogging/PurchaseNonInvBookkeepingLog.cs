using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    public class PurchaseNonInvBookkeepingLog : TraderJournalEntryLog
    {
        public PurchaseNonInvBookkeepingLog()
        {
            this.Type = TraderJournalEntryLogType.PurchaseNoninventory;
        }

        public virtual Invoice Bill { get; set; }

    }
}
