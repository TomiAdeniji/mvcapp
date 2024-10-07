using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;
namespace Qbicles.Models.SalesMkt
{
    [Table("sm_SocialCampaignPost")]
    public class SocialCampaignPost: DataModelBase
    {
        /// <summary>
        /// The user from Qbicles who created the campaign post
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this campaign post was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The user from Qbicles who last updated the campaign post, this is to be set each time the campaign post is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this campaign post was last edited.
        /// This is to be set each time the campaign post is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// This is the campaign with which this post is associated
        /// </summary>
        [Required]
        public virtual SocialCampaign AssociatedCampaign { get; set; }

        /// <summary>
        /// This is the title of the Post
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        /// This is a list of the Social Network Accounts that the Post is shared with
        /// </summary>
        public virtual List<SocialNetworkAccount> SharingAccount { get; set; } = new List<SocialNetworkAccount>(); 


        /// <summary>
        /// This is the content of the post
        /// </summary>
        [Required]
        public virtual string Content { get; set; }

        /// <summary>
        /// This is a video or image that is selected from the resources in the Media Folder associated with the 
        /// Campaign. It can be selected froma list or uploaded at the time the Post is created
        /// </summary>
        public virtual QbicleMedia ImageOrVideo { get; set; }
        /// <summary>
        /// This is the Reminder (one or zero) associated with the Post
        /// </summary>
        public virtual ReminderQueue Reminder { get; set; }

        /// <summary>
        /// This is a list of the Social Networks that the Post is associated with
        /// </summary>
        public virtual List<NetworkType> Networks { get; set; } = new List<NetworkType>();
    }
}
