using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Movement
{
    public class ReportEntry
    {
        public int id { get; set; }

        public virtual AlertGroup AlertGroup {get;set;}


        public virtual TraderItem Item { get; set; }

        public virtual TraderGroup ProductGroup { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}
