using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader;

namespace Qbicles.Models.ProductSearch
{
    public class Product : FeaturedProduct
    {
        /// <summary>
        /// POS Menu
        /// </summary>
        public virtual Catalog Catalog { get; set; }

        public virtual TraderItem TraderItem { get; set; }
    }
}