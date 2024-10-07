using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.UserInformation
{
    public class Showcase : DataModelBase
    {
        [Required]
        [StringLength(50, ErrorMessage = "The Title must have a maximum of 50 characters")]
        public string Title { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The Caption must have a maximum of 100 characters")]
        public string Caption { get; set; }

        [Required]
        public virtual ApplicationUser AssociatedUser { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(256)]
        public string ImageUri { get; set; }

    }
}