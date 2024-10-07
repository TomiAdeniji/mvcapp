using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Catalogs
{
    [Table("pos_category")]
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// This is the link to the menu with which the category is associated
        /// </summary>
        [Required]
        public virtual Catalog Menu { get; set; }

        /// <summary>
        /// This is  a list of the items that will appear in a particular category on a menu
        /// </summary>
        public virtual List<CategoryItem> PosCategoryItems {get; set;}

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This indicates whether the particular Variant is visible on the Pos.
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsVisible { get; set; }
    }
}
