using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Operator.Goals
{
    [Table("op_goals")]
    public class Goal
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Summary { get; set; }

        /// <summary>
        /// A discussion where this place can be discussed
        /// </summary>
        public virtual QbicleDiscussion Discussion { get; set; }

        [Required]
        [StringLength(36)]
        public string FeaturedImageUri { get; set; }
        /// <summary>
        /// This is the collection of the Tags with which the Goal is associated
        /// It is part of a many-to-many relationship
        /// </summary>
        [Required]
        public virtual List<OperatorTag> Tags { get; set; } = new List<OperatorTag>();

        public virtual List<GoalMeasure> GoalMeasures { get; set; } = new List<GoalMeasure>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the Goal, 
        /// This is to be set each time the Goal is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        /// <summary>
        /// This is the date and time on which this Goal was last edited.
        /// This is to be set each time the Goal is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool isHide { get; set; }
    }
}
