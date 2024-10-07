using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_employment")]
    public class Employment
    {
        [Required]
        public int Id { get; set; }

        public string Employer { get; set; }

        public string Role { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public virtual UserProfilePage AssociatedProfile { get; set; }

        public int DisplayOrder { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public string Summary { get; set; }

    }
}
