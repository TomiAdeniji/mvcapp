using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Qbicles.Models
{
    [Table("qb_step")]
    public class QbicleStep
    {
        public QbicleStep()
        {
            StepInstance = new HashSet<QbicleStepInstance>();
        }

        public int Id { get; set; }

        public int ActivityId { get; set; }

        [Required]
        [StringLength(450)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public short Weight { get; set; }

        public short? Order { get; set; }

        public virtual QbicleTask Task { get; set; }


        public virtual ICollection<QbicleStepInstance> StepInstance { get; set; }
    }
}