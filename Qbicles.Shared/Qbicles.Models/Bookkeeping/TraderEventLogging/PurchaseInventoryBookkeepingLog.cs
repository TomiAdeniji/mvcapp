using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    public class PurchaseInventoryBookkeepingLog : TraderJournalEntryLog
    {
        public PurchaseInventoryBookkeepingLog()
        {
            this.Type = TraderJournalEntryLogType.PurchaseInventory;
        }

        public virtual TraderTransfer PurchaseTransfer { get; set; }

    }
}
