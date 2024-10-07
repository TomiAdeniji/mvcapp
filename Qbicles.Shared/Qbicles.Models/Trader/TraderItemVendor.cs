using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_itemvendor")]
    public class TraderItemVendor
    {
        public int Id { get; set; }

        [Required]
        public virtual TraderContact Vendor { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual TraderItem Item { get; set; }

        [Column(TypeName = "bit")]
        public bool IsPrimaryVendor { get; set; }
    }
}
