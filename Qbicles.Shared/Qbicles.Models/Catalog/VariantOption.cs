using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Catalogs
{
    [Table("pos_variantoptions")]
    public class VariantOption
    {
        public int Id { get; set; }


        /// <summary>
        /// The name of the Option to be associated with a variant property 
        /// e.g. if the property is Size, the options could be Small, Mediaum and Large
        /// </summary>
        [Required]
        public string Name { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The Property with which this option is associated
        /// </summary>
        [Required]
        public virtual VariantProperty VariantProperty { get; set; }


        /// <summary>
        /// This is list of the Variants with which this Option is associated
        /// </summary>
        public virtual List<Variant> Variants { get; set; } = new List<Variant>();
    }
}