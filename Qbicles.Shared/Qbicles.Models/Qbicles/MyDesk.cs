using Qbicles.Models.Trader;
//using Qbicles.Models.Trader.MyDeskOrder;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_MyDesk")]
    public class MyDesk
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser Owner { get; set; }


        public virtual List<MyPin> Pins { get; set; } = new List<MyPin>();

        public virtual List<MyTag> Folders { get; set; } = new List<MyTag>();

        //public virtual List<MyDeskOrder> Orders { get; set; } = new List<MyDeskOrder>();

    }
}
