using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.ProductSearch
{
    [Table("system_FeaturedProducts")]
    public class FeaturedProduct
    {
        [Key]
        [Required]
        public int Id { get; set; } = 0;

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        public FeaturedType Type { get; set; } = FeaturedType.Product;
    }

    public enum FeaturedType
    {
        Image = 1,
        Product = 2
    }
}
