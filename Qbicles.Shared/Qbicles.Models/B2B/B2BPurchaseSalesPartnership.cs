using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qbicles.Models.Catalogs;

namespace Qbicles.Models.B2B
{
    public class PurchaseSalesPartnership : Partnership
    {
        public PurchaseSalesPartnership()
        {
            this.Type = B2BService.Products;
        }
        public virtual List<Catalog> Catalogs { get; set; } = new List<Catalog>();
    }
}
