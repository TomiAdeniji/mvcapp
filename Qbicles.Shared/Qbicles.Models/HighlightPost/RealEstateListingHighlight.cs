using Qbicles.Models.FileStorage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Highlight
{
    public class RealEstateListingHighlight : ListingHighlight
    {
        public virtual PropertyType PropType { get; set; }
        public int BedRoomNum { get; set; }
        public int BathRoomNum { get; set; }
        public string PricingInfo { get; set; }
        public virtual List<PropertyExtras> IncludedProperties { get; set; } = new List<PropertyExtras>();
        public virtual List<RealEstateImage> RealEstateListImgs { get; set; } = new List<RealEstateImage>();
    }

    public class RealEstateImage
    {
        public string Id { get; set; }
        public int Order { get; set; }
        public string FileUri { get; set; }
        public string Name { get; set; }
        public virtual QbicleFileType FileType { get; set; }
    }
}
