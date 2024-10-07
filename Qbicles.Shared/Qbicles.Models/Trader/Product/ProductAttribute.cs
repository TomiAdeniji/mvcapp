using System.Collections.Generic;

namespace Qbicles.Models.Trader.Product
{
    public class ProductAttribute
    {
        public TraderItem Item { get; set; }

        public List<AttributeProperty> Properties
        {
            get; set;
        }
    }
}
