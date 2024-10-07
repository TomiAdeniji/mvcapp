using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Highlight
{
    [Table("hl_PropertyType")]
    public class PropertyType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
