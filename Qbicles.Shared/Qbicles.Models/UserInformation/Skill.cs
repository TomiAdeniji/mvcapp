using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.UserInformation
{
    [Table("user_skill")]
    public class Skill : DataModelBase
    {
        [Required]
        [StringLength(50, ErrorMessage = "The Skill must have a maximum of 50 characters")]
        public string Name { get; set; }

        [Range(1, 100, ErrorMessage = "Proficiency must be in the range 1 to 100")]
        public int Proficiency { get; set; }

        [Required]
        public virtual ApplicationUser AssociatedUser { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

    }
}