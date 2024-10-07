using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_topic")]
    public class Topic
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [StringLength(350)]
        public string Summary { get; set; }
        public DateTime? CreatedDate { get; set; }
        public ApplicationUser Creator { get; set; }

        [Required]
        public virtual Qbicle Qbicle { get; set; }
    }
}
