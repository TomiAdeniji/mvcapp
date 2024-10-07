using Qbicles.Models.Form;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Operator.Goals
{
    /// <summary>
    /// The measure will be used in the amalysisi part of Operator
    /// </summary>
    [Table("op_measures")]
    public class Measure
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the name of the measure.
        /// The name of the measure must be unique within a domain
        /// </summary>
        [Required]
        [StringLength(250)]
        public string Name { get; set; }


        /// <summary>
        /// This is a summary of what the measure is to be used for
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Summary { get; set; }

        /// <summary>
        /// This is the Domain with which the measure is associated
        /// </summary>
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// This is a collection of the FormElements with which the Measure is associated 
        /// </summary>
        public virtual List<FormElement> FormElements { get; set; }

        /// <summary>
        /// This is a collection of the GoalMeasures with which the Measure is associated 
        /// </summary>
        public virtual List<GoalMeasure> GoalMeasures { get; set; }

        /// <summary>
        /// This is a collection of the TrackingMeasures with which the Measure is associated 
        /// </summary>
        public virtual List<TrackingMeasure> TrackingMeasures { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
