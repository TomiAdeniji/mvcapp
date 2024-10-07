using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2B
{
    [Table("b2b_areasofoperation")]
    public class AreaOfOperation
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual B2BProfile Profile { get; set; }
    }
}
