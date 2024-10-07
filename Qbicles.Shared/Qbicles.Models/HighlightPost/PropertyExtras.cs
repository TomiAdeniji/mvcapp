using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Highlight
{
    [Table("hl_PropertyExtra")]
    public class PropertyExtras
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public virtual List<RealEstateListingHighlight> RealEstates { get; set; } = new List<RealEstateListingHighlight>();
    }
}
