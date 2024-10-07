using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_ReminderQueue")]
    public class ReminderQueue
    {
        [Key, ForeignKey("SocialCampaignPost")]
        public int Id { get; set; }


        /// <summary>
        /// This is the content of the reminder to be sent.
        /// </summary>
        [Required]
        public string Content { get; set; }


        /// <summary>
        /// The date and time at which the reminder is to be 'sent'
        /// </summary>
        [Required]
        public DateTime ReminderDate { get; set; }


        /// <summary>
        /// This value will be filled in when the Job is queued in Qbicles.HangFire
        /// </summary>
        public string JobId { get; set; }


        /// <summary>
        /// This is the Domain with which the Reminder is associated.
        /// This will allow us to quickly filter the reminders on a company(Domain) by company(Domain) basis
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// The user from Qbicles who added the Reminder to the Queue,
        /// and in the initial pahse is the user who gets reminded
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the Reminder was added to the Queue
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The date and time at which the reminder is to be 'sent' 
        /// </summary>
        [Required]
        public DateTime PostingDate { get; set; }

        /// <summary>
        /// The status of the reminder in the Queue.
        /// The initial status is Scheduled.
        /// </summary>
        [Required]
        public ReminderQueueStatus Status { get; set; }


        /// <summary>
        /// This is the Post with which the reminder is associated
        /// </summary>
        public virtual SocialCampaignPost SocialCampaignPost { get; set; }

    }


    public enum ReminderQueueStatus
    {
        Scheduled = 1,
        Sent = 2,
        Error = 3
    }
}
