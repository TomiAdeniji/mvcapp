using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_SocialNetworkAccounts")]
    public abstract class SocialNetworkAccount
    {
        /// <summary>
        /// The unique ID to identify the social network in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who created the social network
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this social network was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The user from Qbicles who last updated the social network
        /// This is to be set each time the social network is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this social network was last edited.
        /// This is to be set each time the social network is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This indicates which of the Social networks this is associated with
        /// </summary>
        [Required]
        public virtual NetworkType Type { get; set; }



        /// <summary>
        /// This indictaes which Sales and Marketing settings the Social Network is associated with
        /// </summary>
        [Required]
        public virtual SalesMkt.Settings Settings { get; set; }

        /// <summary>
        /// This bool is to indicate if the SocialNetwork is available for selection when creating or editing a Social Campaign.
        /// It is ONLY for that purpose, i.e. making it available to a Social Campaign.
        /// If it has been selected as an account for a Post, changing the IsDisabled setting HAS NO EFFECT
        /// </summary>

        [Column(TypeName = "bit")]
        public bool IsDisabled { get; set; }

        public virtual List<SocialCampaignPost> Posts { get; set; }

    }

   
}
