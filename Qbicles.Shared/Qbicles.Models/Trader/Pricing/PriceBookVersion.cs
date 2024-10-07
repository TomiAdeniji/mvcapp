using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Pricing
{
    [Table("trad_pricebookversion")]
    public class PriceBookVersion
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string VersionName { get; set; }

        [Required]
        public virtual PriceBook ParentPriceBook { get; set; }

        public virtual List<PriceBookInstance> AssociatedInstances { get; set; } = new List<PriceBookInstance>();


    }
}
