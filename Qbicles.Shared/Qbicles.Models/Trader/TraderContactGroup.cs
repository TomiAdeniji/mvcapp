using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_tradercontactgroup")]
    public class TraderContactGroup
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual QbicleDomain Domain {get; set;}
        
        public virtual List<TraderContact> Contacts { get; set; } = new List<TraderContact>();
        
        public virtual ApplicationUser Creator { get; set; }

        public DateTime CreatedDate { get; set; }

        public SalesChannelContactGroup saleChannelGroup { get; set; }
    }

    public enum SalesChannelContactGroup
    {
        [Description("Unassigned")]
        Unassigned = 0,
        [Description("MyDeskOrdering")]
        MyDeskOrdering = 1,
        [Description("POS")]
        POS = 2,
        [Description("B2C")]
        B2C = 3,
        [Description("Trader")]
        Trader = 4
    };
}
