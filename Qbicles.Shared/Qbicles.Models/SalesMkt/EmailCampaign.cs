using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_EmailCampaign")]
    public class EmailCampaign
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// This is the Reply Email  that will be used by default in sending an email from the campaign, 
        /// unless overridden when the user creates a specific email
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string DefaultReplyEmail { get; set; }


        /// <summary>
        /// This is the FROM NAME that will be used by default in sending an email from the campaign, 
        /// unless overridden when the user creates a specific email
        /// </summary>
        [Required]
        public string DefaultFromName { get; set; }

        /// <summary>
        /// This is the FROM EMAIL ADDRESS that will be used by default in sending an email from the campaign, 
        /// unless overridden when the user creates a specific email
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string DefaultFromEmail { get; set; }

        /// <summary>
        /// This is is the workgroup that controls access to the Email Campaign and identifies who
        /// can Review/Approve it
        /// </summary>
        public virtual SalesMarketingWorkGroup WorkGroup { get; set; }


        /// <summary>
        /// This is the collection of Segments that the Email Campaign will contact
        /// </summary>
        public virtual List<Segment> Segments { get; set; } = new List<Segment>();


        /// <summary>
        /// This is the Domain with which this campaign is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The name of the campaign.
        /// NOTE: The name of the campaign MUST be unique within the Domain
        /// </summary>
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// This is the one and only Brand that is associated with the Campaign.
        /// The user selects one Brand to associate it with the Campaign.
        /// Then the application will display the BrandProducts associated with the Brand for possible selection by the user.
        /// It is not required
        /// </summary>
        public virtual Brand Brand { get; set; }


        /// <summary>
        /// This is the Media Folder in the Settings.SourceQbicle in all images, docs and videos
        /// associated with this campaign are stored.
        /// NOTE: If the user choses to create a Folder, it must be unique within the Qbicle.
        /// </summary>
        [Required]
        public virtual MediaFolder ResourceFolder { get; set; }


        /// <summary>
        /// This is the image that is associated with the campaign and shown in the UI for the campaign.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        public string FeaturedImageUri { get; set; }


        /// <summary>
        /// Some detailed information about the campaign
        /// </summary>
        public string Summary { get; set; }


        /// <summary>
        /// From the associated Brand the user can select one or more BrandProducts to use in the SocialCampaign
        /// Each selected Brand Product is linked to the Social Campaign.
        /// </summary>
        public virtual List<BrandProduct> BrandProducts { get; set; } = new List<BrandProduct>();


        /// <summary>
        /// From the associated Brand the user can select one or more Attributes to use in the SocialCampaign
        /// Each selected Attribute is linked to the Social Campaign.
        /// </summary>
        public virtual List<Attribute> Attributes { get; set; } = new List<Attribute>();

        /// <summary>
        /// From the associated Brand the user can select one or more ValueProposition to use in the SocialCampaign
        /// Each selected ValuePropositon is linked to the Social Campaign.
        /// </summary>
        public virtual List<ValueProposition> ValuePropositons { get; set; } = new List<ValueProposition>();


        /// <summary>
        /// This is the one and only Theme/Idea that is associated with the Campaign.
        /// The user selects one Theme/Idea  to associate it with the SocialCampaign.
        /// </summary>
        public virtual IdeaTheme IdeaTheme { get; set; }


        // <summary>
        /// The user from Qbicles who created the campaign
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this campaign was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The user from Qbicles who last updated the campaign, this is to be set each time the campaign is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this campaign was last edited.
        /// This is to be set each time the campaign is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is a list of the emails associated with the campaign
        /// </summary>
        public virtual List<CampaignEmail> CampaignEmails { get; set; } = new List<CampaignEmail>();

    }
}
