using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Resources
{
    [Table("trad_ResourceCategory")]
    public class ResourceCategory
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }


        /// <summary>
        /// The Domain with which this category is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The user from Qbicles who created the category
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        public virtual List<ResourceDocument> AssociatedDocuments { get; set; } = new List<ResourceDocument>();
        public virtual List<ResourceImage> AssociatedImages { get; set; } = new List<ResourceImage>();
        /// <summary>
        /// This is the date and time on which this category was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is to indicate whether the category is used for Images or for Documents
        /// </summary>
        [Required]
        public ResourceCategoryType Type { get; set; }

    }

    public enum ResourceCategoryType  
    {
        Image = 1,
        Document = 2
    }
}
