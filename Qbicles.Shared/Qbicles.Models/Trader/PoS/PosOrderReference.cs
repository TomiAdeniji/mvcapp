using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_OrderReference")]
    public class PosOrderReference
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public long ReferenceNumber { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy{ get; set; }

    }
}
