using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_attribute")]
    public class Attribute
    {

        /// <summary>
        /// The unique ID to identify the attribute in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who created the attribute
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this attribute was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the attribute, this is to be set each time the attribute is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this attribute was last edited.
        /// This is to be set each time the attribute is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the Brand with which this attribute is associated
        /// </summary>
        [Required]
        public virtual Brand Brand { get; set; }


        /// <summary>
        /// The name of the value proposition.
        /// NOTE: The name of the attribute MUST be unique within the Brand
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// The summary of the attribute.
        /// </summary>
        public string Summary { get; set; }


        /// <summary>
        /// This is the image that is associated with the attribute and shown in the UI for the attributr.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        public string Icon { get; set; }

        /// <summary>
        /// Status of Attribute
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// This is a collection of the Social Campaigns with which the BrandProduct is associated
        /// This completes a many to many link between the SocialCampaigns and the BrandProducts.
        /// </summary>
        public virtual List<SocialCampaign> SocialCampaigns { get; set; } = new List<SocialCampaign>();
    }
}
