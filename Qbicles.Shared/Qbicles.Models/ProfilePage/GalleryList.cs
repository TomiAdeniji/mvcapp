using System.Collections.Generic;

namespace Qbicles.Models.ProfilePage
{
    public class GalleryList : Block
    {
        public GalleryList()
        {
            this.Type = BusinessPageBlockType.Gallery;
        }
        public virtual List<GalleryItem> GalleryItems { get; set; } = new List<GalleryItem>();
    }
}
