using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Qbicles.Models
{
    [Table("qb_performance")]
    public class QbiclePerformance
    {
        public int Id { get; set; }

        public short Rating { get; set; }

        [Required]
        [StringLength(128)]
        public string RatedBy { get; set; }

        public DateTime RatedDateTime { get; set; }

        public virtual QbicleTask Task { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}