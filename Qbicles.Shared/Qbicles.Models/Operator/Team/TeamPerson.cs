using Qbicles.Models.Operator.TimeAttendance;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator.Team
{
    [Table("op_teampersons")]
    public class TeamPerson
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// The user from Qbicles who created the Operator TeamPerson
        /// </summary>
        [Required]
        public virtual ApplicationUser User { get; set; }

        public virtual MediaFolder ResourceFolder { get; set; }

        public virtual List<OperatorRole> Roles { get; set; } = new List<OperatorRole>();

        public virtual List<OperatorLocation> Locations { get; set; } = new List<OperatorLocation>();
        public virtual bool IsHide { get; set; }
    }
}
