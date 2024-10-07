using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Spannered
{
    /// <summary>
    /// The SPTags class is he user can assign one or more Asset Tags to an Asset. 
    /// These Asset Tags are managed in Spannered’s App Config settings, similarly to Topics in Qbicles.
    /// </summary>
    [Table("sp_tags")]
    public class AssetTag
    {
        /// <summary>
        /// The unique ID to identify the spannered AssetTags in the database
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The Name of the AssetTags
        /// NOTE: The Name of the AssetTags MUST be unique within the Domain
        /// </summary>
        [StringLength(250)]
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// This is the Domain with which this asset is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        /// <summary>
        /// The Summary of the AssetTags
        /// </summary>
        [StringLength(500)]
        [Required]
        public string Summary { get; set; }
        /// <summary>
        /// The user from Qbicles who created the spannered AssetTags
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this spannered AssetTags was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the spannered AssetTags, 
        /// This is to be set each time the spannered AssetTags is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this spannered AssetTags was last edited.
        /// This is to be set each time the spannered AssetTags is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// This is a collection of the Asset associated with a AssetTags
        /// </summary>
        public virtual List<Asset> Assets { get; set; } = new List<Asset>();
    }
}
