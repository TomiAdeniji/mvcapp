using Qbicles.Models.Base;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_OrderPrintCheck")]
    public class PosOrderPrintCheck : DataModelBase
    {
        [Required]
        public virtual PosDevice POS { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        [Required]
        public SalesChannelEnum SalesChannel { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// as Till Manager
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser TillUser { get; set; }
        /// <summary>
        /// Reference Qbicles.Models.TraderApi.Order
        /// </summary>
        [Required]
        public string OrderJson { get; set; }

        public virtual QbicleDiscussion Discussion { get; set; }
    }
}
