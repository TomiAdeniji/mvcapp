using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_administrator")]
    public class PosAdministrator
    {
        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }

        public virtual List<PosDevice> Devices { get; set; } = new List<PosDevice>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }
    }
}
