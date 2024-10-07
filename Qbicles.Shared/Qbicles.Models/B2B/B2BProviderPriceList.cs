using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    /// <summary>
    /// the entire Pricelist needs to be copied - so the ChargeFramework would be copied as part of that.
    /// That way, any changes to the ChargeFramework would be specific to the Partnership
    /// </summary>
    [Table("b2b_providerpricelist")]
    public class B2BProviderPriceList
    {
        [Required]
        public int Id { get; set; }
        public virtual PriceList PriceList { get; set; }
        public virtual List<B2BProviderChargeFramework> ChargeFrameworks { get; set; } = new List<B2BProviderChargeFramework>();
    }
}
