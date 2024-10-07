using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    public class SaleInvoiceBookkeepingLog : TraderJournalEntryLog
    {
        public SaleInvoiceBookkeepingLog()
        {
            this.Type = TraderJournalEntryLogType.SaleInvoice;
        }

        public virtual Invoice Invoice { get; set; }

    }
}
