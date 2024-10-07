using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement
{
    [Table("trad_alertgroup")]
    public class AlertGroup
    {
        public int Id { get; set; }

        public virtual TraderLocation Location { get; set; }
        public virtual List<TraderGroup> ProductGroups { get; set; } = new List<TraderGroup>();

        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        //public int ReferenceNumber { get; set;}
        //public string Reference { get; set; }

        public virtual TraderReference Reference { get; set; }

        public virtual List<AlertConstraint> AlertConstraints { get; set; } = new List<AlertConstraint>();

    }

    public enum AlertGroupStatusShown
    {
        Enabled = 1,
        Disabled = 2,
        ShowAll = 3
    }
}
