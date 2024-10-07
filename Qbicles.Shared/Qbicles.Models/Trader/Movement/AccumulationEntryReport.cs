using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Movement
{
    public class AccumulationEntryReport: ReportEntry
    {
        public decimal AccumulationThreshold { get; set; }

        [Column(TypeName = "bit")]
        public bool IsAccumulationThresholdOK { get; set; }

        public virtual List<DateRange> AccumulationDateRanges { get; set; } = new List<DateRange>();
    }
}
