using Qbicles.Models.Manufacturing;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    public class ManufacturingBookkeepingLog : TraderJournalEntryLog
    {
        public ManufacturingBookkeepingLog()
        {
            this.Type = TraderJournalEntryLogType.Manufacturing;
        }

        public virtual TraderTransfer TransferIn { get; set; }

        public virtual TraderTransfer TransferOut { get; set; }

        public virtual TraderSale TraderSale { get; set; }

        public virtual ManuJob ManufacturingJob { get; set; }



       
    }
}
