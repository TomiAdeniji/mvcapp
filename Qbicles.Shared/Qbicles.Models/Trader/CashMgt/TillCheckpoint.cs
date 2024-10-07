using Qbicles.Models.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.CashMgt
{
    [Table("trad_checkpoint")]
    public class Checkpoint
    {
        public int Id { get; set; }

        public virtual Till VirtualTill { get; set; } = null;

        public virtual Safe VirtualSafe { get; set; } = null;

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime CheckpointDate { get; set; }

        /// <summary>
        /// This is the amount when the check is carried out
        /// It is a required field
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public virtual WorkGroup WorkGroup { get; set; }

        public virtual QbicleDiscussion Discussion { get; set; }

    }
}
