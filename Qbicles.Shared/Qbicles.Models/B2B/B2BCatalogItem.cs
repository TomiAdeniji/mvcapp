using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    [Table("b2b_catalogitems")]
    public class B2BCatalogItem
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual TraderItem Item { get; set; }
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        public virtual List<TraderLocation> ProviderLocations { get; set; } = new List<TraderLocation>();
        [Required]
        public virtual ProductUnit ProviderUnit { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
