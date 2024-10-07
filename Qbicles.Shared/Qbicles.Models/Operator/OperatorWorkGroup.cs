using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator
{
    [Table("op_workgroups")]
    public class OperatorWorkGroup
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public WorkGroupTypeEnum Type { get; set; }

        [Required]
        public virtual OperatorLocation Location { get; set; }

        [Required]
        public virtual Qbicle SourceQbicle { get; set; }

        [Required]
        public virtual Topic DefaultTopic { get; set; }

        /// <summary>
        /// List of members when WorkGroupType = Tasks
        /// </summary>
        public virtual List<WorkGroupTaskMember> TaskMembers { get; set; } = new List<WorkGroupTaskMember>();
        /// <summary>
        /// List of members when WorkGroupType = Team
        /// </summary>
        public virtual List<WorkGroupTeamMember> TeamMembers { get; set; } = new List<WorkGroupTeamMember>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the OperatorWorkGroup, 
        /// This is to be set each time the OperatorWorkGroup is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        /// <summary>
        /// This is the date and time on which this OperatorWorkGroup was last edited.
        /// This is to be set each time the OperatorWorkGroup is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }
        public bool IsHide { get; set; }
    }
    public enum WorkGroupTypeEnum
    {
        Tasks = 0,
        Team = 1
    }
}
