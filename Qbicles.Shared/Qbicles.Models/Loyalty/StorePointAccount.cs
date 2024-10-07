using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_StorePointAccount")]
    public class StorePointAccount
    {
        public int Id { get; set; }

        public decimal CurrentBalance { get; set; }
        
        [Required]
        public virtual TraderContact Contact { get; set; }

        public virtual List<StorePointTransaction> Transactions { get; set; } = new List<StorePointTransaction>();

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser CreatedBy { get; set; }
    }

}
