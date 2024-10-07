using System;
using System.Collections.Generic;

namespace Qbicles.Models.Trader.Movement
{
    public class Report
    {
        public int id { get; set; }

        public virtual AlertGroup AlertGroup {get; set;}

        public virtual List<ReportEntry> ReportEntries { get; set; } = new List<ReportEntry>();

        public DateTime Executiondate { get; set; }
        public virtual TraderReference Reference { get; set; }
    }
}
