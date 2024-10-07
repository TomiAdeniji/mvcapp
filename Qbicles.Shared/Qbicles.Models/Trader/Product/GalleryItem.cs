using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Product
{
    [Table("trad_galleryitem")]
    public class ProductGalleryItem: DataModelBase
    {       
        /// <summary>
        /// The order in which this particular item appears in the collection of GalleryItems associated with a TraderItem
        /// </summary>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// The TraderItem with which the GalleryItem is associated
        /// </summary>
        //[Required]
        public virtual TraderItem TraderItem { get; set; }  


        /// <summary>
        /// The user from Qbicles who created the GalleryItem
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this GalleryItem was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the S3 GUID of the 'master' file that was uploaded for this GalleryItem
        /// All files when uploaded must have their Thumbnail, Small, Medium sizes created ans stored in S3
        /// </summary>
        [Required]
        public string FileUri { get; set; }
    }
}
