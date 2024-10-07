using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Resources
{
    /// <summary>
    /// Addition Info is split into  
    ///  - Brands, 
    ///  - Tags, 
    ///  - Needs and 
    ///  - Ratings
    /// </summary>
    [Table("trad_ResourceAdditionalInfo")]
    public class AdditionalInfo
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// The name of the Additional Info
        /// It MUST be unique with a Domain PER TYPE
        /// E.G. In one Domain there can be a Brand called Apple and a Tag called apple but,
        /// there cannot be two brands called Apple
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// The user from Qbicles who created the Additional Info
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Additional Info was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is a list of the products associated with Additional Info
        /// </summary>
        public virtual List<TraderItem> TraderItems { get; set; } = new List<TraderItem>();


        /// <summary>
        /// The Domain with which this Additional Info is associated.
        /// If this is created by a System Administrator then the Domain will NOT be set
        /// </summary>
        //[Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// This is the type of the additional info.
        /// It MUST be set
        /// </summary>
        [Required]
        public AdditionalInfoType Type { get; set; }

        
    }


    public enum AdditionalInfoType
    {
        [Description("Brand")]
        Brand = 1, 
        [Description("Need")]
        Need = 2,
        [Description("Quality rating")]
        QualityRating = 3,
        [Description("Product tag")]
        ProductTag = 4
    }
}
