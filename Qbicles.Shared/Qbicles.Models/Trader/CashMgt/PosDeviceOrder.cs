using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.CashMgt
{
    [Table("pos_DeviceOrderXref")]
    public class PosDeviceOrderXref
    {
        public int Id { get; set; }
        [Required]
        public virtual PosDevice PosDevice { get; set; }

        [Required]
        public virtual QueueOrder Order { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual Till Till { get; set; }
    }
}
