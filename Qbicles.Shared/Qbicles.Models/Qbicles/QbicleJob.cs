using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Qbicles.Models
{
    [Table("qb_job")]
    public class QbicleJob
    {
        public int Id { get; set; }

        [Required]
        [StringLength(45)]
        public string JobId { get; set; }

        [Required]
        [StringLength(45)]
        public string Type { get; set; }

        public int ActivityId { get; set; }

        public virtual QbicleActivity Activity { get; set; }
    }
}