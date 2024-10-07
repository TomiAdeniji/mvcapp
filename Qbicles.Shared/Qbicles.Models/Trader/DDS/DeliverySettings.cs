using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.DDS
{
    [Table("dds_DeliverySettings")]
    public class DeliverySettings
    {
        [Required]
        public int Id { get; set; }

        public virtual TraderItem DeliveryService { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        public DateTime LastUpdatedDate { get; set; }
    }
}
