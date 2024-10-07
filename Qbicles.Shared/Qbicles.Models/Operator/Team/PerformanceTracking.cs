using Qbicles.Models.Operator.Goals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Operator.Team
{
    [Table("op_performancetrackings")]
    public class PerformanceTracking
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual OperatorWorkGroup WorkGroup { get; set; }

        public virtual TeamPerson Team { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public bool IsHide { get; set; }

        /// <summary>
        /// A discussion where this place can be discussed
        /// </summary>
        public virtual QbicleDiscussion Discussion { get; set; }

        public virtual List<TrackingMeasure> TrackingMeasures { get; set; } = new List<TrackingMeasure>();
    }
}
