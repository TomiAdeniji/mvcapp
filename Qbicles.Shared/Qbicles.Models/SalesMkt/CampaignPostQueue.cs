using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_socialcampaignqueue")]
    public class SocialCampaignQueue
    {

        public int Id { get; set; }

        /// <summary>
        /// This is the Domain with which the Post is associated.
        /// This will allow us to quickly filter the queue on a company(Domain) by company(Domain) basis
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        /// <summary>
        /// The user from Qbicles who added the Post to the Queue
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the Post was added to the Queue
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is the post that is to be sent from the Queue
        /// </summary>
        [Required]
        public virtual SocialCampaignPost Post { get; set; }

        /// <summary>
        /// The date and time at whihc the post is to be 'sent' to the targeted networks
        /// </summary>
        [Required]
        public DateTime PostingDate { get; set; }


        /// <summary>
        /// The status of the Post in the Queue.
        /// The initial status is Scheduled.
        /// </summary>
        [Required]
        public CampaignPostQueueStatus Status { get; set; }

        public int CountErrors { get; set; }

        [Column(TypeName = "bit")]
        public bool isNotifyWhenSent { get; set; }

        public string JobId { get; set; }
    }

    public enum CampaignPostQueueStatus
    {
        Scheduled = 1,
        Sent = 2,
        Error = 3
    }
}
