using Qbicles.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.UserInformation
{
    [Table("user_experience")]
    public class Experience : DataModelBase
    {
        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public virtual ApplicationUser AssociatedUser { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public string Summary { get; set; }

        public ExperienceType Type { get; set; }

    }

    public enum ExperienceType
    {
        [Description("Work")]
        WorkExperience = 0,
        [Description("Experience")]
        EducationExperience = 1
    }
}