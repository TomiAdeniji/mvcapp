using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Operator
{
    [Table("op_wgteammembers")]
    public class WorkGroupTeamMember
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual OperatorWorkGroup WorkGroup { get; set; }

        [Required]
        public virtual ApplicationUser Member { get; set; }

        [Required]
        public TeamPermissionTypeEnum TeamPermission { get; set; }
    }
    public enum TeamPermissionTypeEnum
    {
        /// <summary>
        /// Can add Team activities (for example Performance Tracking and Attendance), and interact with all Team members within the Workgroup. The Manager inherits all permissions below their level.
        /// </summary>
        Manager = 0,
        /// <summary>
        /// Supervisors will not have the ability to add Team activities, but will have access to the entire Team’s profiles, performance tracking etc.
        /// </summary>
        Supervisor = 1,
        /// <summary>
        /// The lowest level permission, this will grant an individual access to only their own information.
        /// </summary>
        Member = 2,
    }
}
