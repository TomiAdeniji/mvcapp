using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{

    [Table("trad_recipe")]
    public class Recipe
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual TraderItem ParentItem { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();


        [Column(TypeName = "bit")]
        public bool IsActive { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        [Column(TypeName = "bit")]
        public bool IsCurrent { get; set; }
    }
}
