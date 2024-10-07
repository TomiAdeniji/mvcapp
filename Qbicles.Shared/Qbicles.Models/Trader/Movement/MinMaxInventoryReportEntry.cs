using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Movement
{
    public class MinMaxInventoryReportEntry: ReportEntry
    {

        public decimal MinInventoryThreshold { get; set; }

        public decimal MaxInventoryThreshold { get; set; }

        [Column(TypeName = "bit")]
        public bool IsMinInventoryThresholdOK { get; set; }

        [Column(TypeName = "bit")]
        public bool IsMaxInventoryThresholdOK { get; set; }

        public DateTime MinInventoryThresholdDate { get; set; } 

        public DateTime MaxInventoryThresholdDate { get; set; } 

    }
}