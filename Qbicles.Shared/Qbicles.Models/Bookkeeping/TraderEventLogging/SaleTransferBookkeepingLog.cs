using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    public class SaleTransferBookkeepingLog : TraderJournalEntryLog
    {
        public SaleTransferBookkeepingLog()
        {
            this.Type = TraderJournalEntryLogType.SaleTransfer;
        }

        public virtual TraderTransfer SaleTransfer { get; set; }
        
        public string SaleTransferType { get; set; }

    }
}
