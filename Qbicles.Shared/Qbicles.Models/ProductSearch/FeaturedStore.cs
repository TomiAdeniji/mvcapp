using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.ProductSearch
{
    [Table("system_FeaturedStore")]
    public class FeaturedStore
    {
        [Key]
        [Required]
        public int Id { get; set; } = 0;

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

    }
}