using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_location")]
    public class Location
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public virtual DomainProfile DomainProfile { get; set; }

        [Required]
        public string Address { get; set; }

        public int DisplayOrder{ get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime EditedDate { get; set; }
    }
}
