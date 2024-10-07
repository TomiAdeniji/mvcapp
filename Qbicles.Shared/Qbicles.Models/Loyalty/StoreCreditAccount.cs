using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_StoreCreditAccount")]
    public class StoreCreditAccount
    {
        public int Id { get; set; }

        public decimal CurrentBalance { get; set; }

        [Required]
        public virtual TraderContact Contact { get; set; }

        public virtual List<StoreCreditTransaction> Transactions { get; set; } = new List<StoreCreditTransaction>();

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public virtual ApplicationUser CreatedBy { get; set; }
    }
}