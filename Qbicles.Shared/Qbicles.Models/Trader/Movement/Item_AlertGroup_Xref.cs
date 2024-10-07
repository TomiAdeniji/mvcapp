using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement
{
    [Table("trad_itemalertgroupxref")]
    public class Item_AlertGroup_Xref
    {
        public int id { get; set; }

        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        public virtual TraderItem Item { get; set; }
        public virtual AlertGroup  Group { get; set; }

        
        public virtual int NoMovementOutDaysThreshold { get; set; }
        public virtual int NoMovementInDaysThreshold { get; set; }


        public decimal MinInventoryThreshold{ get; set; }
        public decimal MaxInventoryThreshold { get; set; }


        public decimal AccumulationTreshold { get; set; }

    }
}
