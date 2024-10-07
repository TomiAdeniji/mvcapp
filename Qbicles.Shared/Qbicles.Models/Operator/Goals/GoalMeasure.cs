using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Operator.Goals
{
    [Table("op_goalmeasures")]
    public class GoalMeasure
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual Goal Goal { get; set; }

        /// <summary>
        /// This is the GoalMeasure with which this Measure is associated
        /// </summary>
        [Required]
        public virtual Measure Measure { get; set; }

        /// <summary>
        /// The combined weight of all of your measures should total 100%. The higher this measure's percentage, the higher its potential performance gains.
        /// </summary>
        [Required]
        public int Weight { get; set; }

        [Required]
        public GoalMeasureTypeEnum Type { get; set; }
    }
    public enum GoalMeasureTypeEnum
    {
        LeadingIndicator=0,
        GoalMeasure=1
    }
}
