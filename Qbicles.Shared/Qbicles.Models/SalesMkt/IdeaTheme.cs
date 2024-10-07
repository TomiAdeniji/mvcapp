using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_IdeaTheme")]
    public class IdeaTheme
    {
        /// <summary>
        /// The unique ID to identify the Theme/Idea in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is a boolean to indicate of the Theme/Idea ias active. 
        /// If it is active it can be selected for inclusion in a Social Campaign
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsActive { get; set; }


        /// <summary>
        /// The user from Qbicles who created the Theme/Idea
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Theme/Idea was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the Theme/Idea, this is to be set each time the Theme/Idea is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Theme/Idea was last edited.
        /// This is to be set each time the Theme/Idea is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the Domain with which this Theme/Idea is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The name of the Theme/Idea.
        /// NOTE: The name of the Theme/Idea MUST be unique within the Domain
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This is the Media Folder in the Settings.SourceQbicle in which all images, docs and videos
        /// associated with this Theme/Idea are stored.
        /// NOTE: If the user choses to create a Folder, it must be unique within the Qbicle.
        /// </summary>
        [Required]
        public virtual MediaFolder ResourceFolder { get; set; }


        /// <summary>
        /// This is the image that is associated with the Theme/Idea and shown in the UI for the Theme/Idea.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        public string FeaturedImageUri { get; set; }


        /// <summary>
        /// This is the explanation for the Theme/Idea
        /// </summary>
        /// 
        [Required]
        public string Explanation { get; set; }

        // <summary>
        /// Status of Theme
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// THis is the collection of links (urls)
        /// </summary>
        public virtual List<IdeaThemeLink> Links { get; set; } = new List<IdeaThemeLink>();



        /// <summary>
        /// This is a collection of the SocialCampaigns with which this Idea/Theme is associated.
        /// The Idea/Theme can be associated with one or more SocialCampaigns
        /// </summary>
        public virtual List<SocialCampaign> Campaigns { get; set; } = new List<SocialCampaign>();

        /// <summary>
        /// This is the discussion, in the Qbicle, for discussing this theme
        /// </summary>
        public virtual QbicleDiscussion Discussion { get; set; }


        /// <summary>
        /// This is the kind of theme
        /// </summary>
        public virtual IdeaThemeType Type { get; set; }

    }
}
