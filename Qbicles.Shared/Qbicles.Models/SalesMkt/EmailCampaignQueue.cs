using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_emailcampaignqueue")]
    public class EmailCampaignQueue
    {

        public int Id { get; set; }

        /// <summary>
        /// This is the Domain with which the Email is associated.
        /// This will allow us to quickly filter the queue on a company(Domain) by company(Domain) basis
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// The user from Qbicles who added the Email to the Queue
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the Email was added to the Queue
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is the Email that is to be sent from the Queue
        /// </summary>
        [Required]
        public virtual CampaignEmail Email { get; set; }

        /// <summary>
        /// The date and time at which the Email is to be 'sent' to the targeted networks
        /// </summary>
        [Required]
        public DateTime PostingDate { get; set; }


        /// <summary>
        /// The status of the Email in the Queue.
        /// The initial status is Scheduled.
        /// </summary>
        [Required]
        public CampaignEmailQueueStatus Status { get; set; }

        public int CountErrors { get; set; }

        [Column(TypeName = "bit")]
        public bool isNotifyWhenSent { get; set; }

        public string JobId { get; set; }
    }

    public enum CampaignEmailQueueStatus
    {
        Scheduled = 1,
        Sent = 2,
        Error = 3
    }
}
