using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Movement
{
    public class AlertConstraint
    {
        public int Id { get; set; }
        public DateTime BenchmarkStartDate { get; set; }
        public DateTime BenchmarkEndDate { get; set; }
        public int HangfireJobId { get; set; }
        public CheckEvent CheckEvent { get; set; }


        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        public bool IsEnabled { get; set; }

        public CheckType Type { get; set; }
    }

    public enum CheckEvent
    {
        [Description("Daily (11:59pm)")]
        Daily = 1,
        [Description("Weekly (Friday 11:59pm)")]
        Weekly = 2,
        [Description("Monthly (Last day 11:59pm)")]
        Month = 3
    }


    public enum CheckType
    {
        NoMovement = 1,
        MinMaxInventory= 2,
        InventoryAccumulation = 3
    }
}
