using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_usersession")]
    public class PosUserSession
    {
        
        public int Id { get; set; }

        public PosDevice AssociatedPoS { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual List<TraderSale> AssociatedSales { get; set; } = new List<TraderSale>();

    }
}
