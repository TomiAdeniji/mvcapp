using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_SocialCampaign")]
    public class SocialCampaign
    {
        /// <summary>
        /// The unique ID to identify the campaign in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
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
        public string Details { get; set; }

        /// <summary>
        /// A list of the social networks that will be targeted by the campaign
        /// </summary>
        public virtual List<NetworkType> TargetNetworks { get; set; } = new List<NetworkType>();

        /// <summary>
        /// A collection of the posts associated with the campaign
        /// </summary>
        public virtual List<SocialCampaignPost> Posts { get; set; } = new List<SocialCampaignPost>();


        /// <summary>
        /// This is the Sales and Marketing WorkGroup with which the Social Campaign is associated
        /// The WorkGroup provides
        ///     The ApprovalRequestDefinition which will be used in the Social Post approval process
        ///     The list of users (SalesMarketingWorkGroup.Members) who can create and work on a Social Campaign
        /// </summary>
        public virtual SalesMarketingWorkGroup WorkGroup { get; set; }
        /// <summary>
        /// This is the one and only Brand that is associated with the Campaign.
        /// The user selects one Brand to associate it with the SocialCampaign.
        /// Then the application will display the BrandProducts associated with the Brand for possible selection by the user.
        /// It is not required
        /// </summary>
        public virtual Brand Brand { get; set; }

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


        /// <summary>
        /// This indicates which type of campaign this is 
        /// Automated or Manual
        /// NOTE: A campaign CANNOT use CampaignType.Both
        /// </summary>
        [Required]
        public CampaignType CampaignType { get; set; }
    }
}
