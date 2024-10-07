using Qbicles.Models.Operator.Team;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator
{
    [Table("op_roles")]
    public class OperatorRole
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// This is a description of the Operator Role
        /// </summary>
        [StringLength(500)]
        public string Summary { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool Status { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsHide { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual List<TeamPerson> Teams { get; set; }
    }
}
