using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Catalogs
{
    [Table("pos_variantproperty")]
    public class VariantProperty
    {
        public int Id { get; set; }


        /// <summary>
        /// The name of the property to be associated with a variant e.g. Size, Colour, Pattern etc
        /// </summary>
        [Required]
        public string Name { get; set; }


        public virtual List<VariantOption> VariantOptions { get; set; } = new List<VariantOption>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual CategoryItem CategoryItem { get; set; }

    }
}
