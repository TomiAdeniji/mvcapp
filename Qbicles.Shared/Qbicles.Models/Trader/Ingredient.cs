using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Attributes;

namespace Qbicles.Models.Trader
{
    [Table("trad_ingredient")]
    public class Ingredient
    {
        [Required]
        public int Id { get; set; }

        public virtual TraderItem SubItem { get; set; }

        [DecimalPrecision(10, 3)]
        public decimal Quantity { get; set; }


        /// <summary>
        /// This property indicates what ProductUnit associated with the SubItem is used to indicate
        /// the quantity of the Ingredient in the ParentRecipe
        /// </summary>
        public virtual ProductUnit Unit { get; set; }

 
        public virtual Recipe ParentRecipe { get; set; }

    }
}
