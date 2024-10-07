using Qbicles.Models.Operator.Team;
using Qbicles.Models.Qbicles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator
{
    [Table("op_locations")]
    public class OperatorLocation
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string AddressLine2 { get; set; }

        [Required]
        [StringLength(200)]
        public string City { get; set; }

        [StringLength(200)]
        public string Postcode { get; set; }

        [StringLength(200)]
        public string State { get; set; }

        public virtual Country Country { get; set; }

        [StringLength(350)]
        public string Email { get; set; }

        [StringLength(25)]
        public string Telephone { get; set; }
        
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsHide { get; set; }

        public virtual List<TeamPerson> Teams { get; set; }

        public virtual List<OperatorWorkGroup> Workgroups { get; set; }
    }
}
