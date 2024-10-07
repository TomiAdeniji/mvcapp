using Qbicles.Models.Trader.ODS;
using System;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.Trader.SalesChannel
{
    public class SalesChannelSettings
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
               
        public SalesChannelEnum SalesChannel { get; set; } = SalesChannelEnum.B2C;

        /// <summary>
        /// All sales channels must indicate what the defalut status of an order is when it is added to the prep queue
        /// </summary>
        public PrepQueueStatus OrderStatusWhenAddedToQueue { get; set; } = PrepQueueStatus.NotStarted;

    }
}
