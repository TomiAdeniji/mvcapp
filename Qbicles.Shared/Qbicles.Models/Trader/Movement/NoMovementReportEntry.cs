using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Movement
{
    public class NoMovementReportEntry: ReportEntry
    {

        public int NoMovementOutDaysThreshold { get; set; }

        public int NoMovementInDaysThreshold { get; set; }

        [Column(TypeName = "bit")]
        public bool IsNoMovementInDaysThresholdOK { get; set; }

        [Column(TypeName = "bit")]
        public bool IsNoMovementOutDaysThresholdOK { get; set; }

        public virtual List<DateRange> NoMovementOutDateRanges { get; set; } = new List<DateRange>();

        public virtual List<DateRange> NoMovementInDateRanges { get; set; } = new List<DateRange>();

    }
}
