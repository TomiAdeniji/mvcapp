using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_brand")]
    public class Brand
    {
        /// <summary>
        /// The unique ID to identify the brand in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who created the brand
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this brand was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the brand, this is to be set each time the brand is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this brand was last edited.
        /// This is to be set each time the brand is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the Domain with which this brand is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The name of the brand.
        /// NOTE: The name of the brand MUST be unique within the Domain
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This is the Media Folder in the Settings.SourceQbicle in which all images, docs and videos
        /// associated with this brand are stored.
        /// NOTE: If the user choses to create a Folder, it must be unique within the Qbicle.
        /// </summary>
        [Required]
        public virtual MediaFolder ResourceFolder { get; set; }


        /// <summary>
        /// This is the image that is associated with the brand and shown in the UI for the brand.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        public string FeaturedImageUri { get; set; }


        /// <summary>
        /// This is the Category/Position from the brand ui
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Status of brand
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// This is a collection of the products associated with the brand
        /// </summary>
        public virtual List<BrandProduct> Products { get; set; } = new List<BrandProduct>();


        /// <summary>
        /// This is a collection of the SocialCampaigns with which this brand is associated.
        /// The brand can be associated with one or more SocialCampaigns
        /// </summary>
        public virtual List<SocialCampaign> Campaigns { get; set; } = new List<SocialCampaign>();


        /// <summary>
        /// This is a collection of the Attributes associated with a Brand
        /// </summary>
        public virtual List<Attribute> Attributes { get; set; } = new List<Attribute>();


        /// <summary>
        /// This is a collection of the AttributeGroups associated with a a Brand
        /// </summary>
        public virtual List<AttributeGroup> AttributeGroups { get; set; } = new List<AttributeGroup>();


        /// <summary>
        /// This is a collection of the ValuePropositions associated with a Brand
        /// </summary>
        public virtual List<ValueProposition> ValuePropositions { get; set; } = new List<ValueProposition>();
    }
}
