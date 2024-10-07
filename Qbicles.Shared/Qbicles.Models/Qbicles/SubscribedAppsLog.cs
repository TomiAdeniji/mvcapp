using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_SubscribedAppsLog")]
    public class SubscribedAppsLog
    {
        public int Id { get; set; }

        [Required]
        public virtual QbicleApplication Application { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public SubscriptionStatus Status { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

    }


    public enum SubscriptionStatus
    {
        Subscribed = 1,
        Unsubscribed = 2
    }
}
