using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.ODS;

namespace Qbicles.Models.Trader.SalesChannel
{
    [Table("settings_pos")]
    public class PosSettings : SalesChannelSettings
    {
        public PosSettings()
        {
            this.SalesChannel = SalesChannelEnum.POS;
            this.OrderStatusWhenAddedToQueue = PrepQueueStatus.NotStarted;
        }

        public virtual WorkGroup DefaultWorkGroup { get; set; }

        public virtual TraderContact DefaultWalkinCustomer { get; set; }
        public int MaxContactResult { get; set; }

        [Required]
        public TimeSpan RolloverTime { get; set; } = new TimeSpan(04, 00, 00);


        /// <summary>
        /// This is the currency symbol that will be used for displaying money 
        /// on the PoS. It can be null
        /// </summary>
        public string MoneyCurrency { get; set; }

        /// <summary>
        /// The number of decimal places to be used on the PoS to display money
        /// </summary>
        public int  MoneyDecimalPlaces { get; set; }

        /// <summary>
        /// The header for the receipt displayed on the receipts generated on the PoS
        /// </summary>
        [Required]
        public string ReceiptHeader { get; set; }

        /// <summary>
        /// The footer for the receipt displayed on the receipts generated on the PoS
        /// </summary>
        [Required]
        public string ReceiptFooter { get; set; }


        /// <summary>
        /// The refresh time in minutes for the order display device
        /// </summary>
        public int OrderDisplayRefreshInterval { get; set; }

        /// <summary>
        /// The refresh time in minutes for the delivery display device
        /// </summary>
        public int DeliveryDisplayRefreshInterval { get; set; }

        public string ProductPlaceholderImage { get; set; }

        public int LingerTime { get; set; }

        public SpeedDistance SpeedDistance { get; set; }
        /// <summary>
        /// Time to call Google Directions API (GDA) ( Minute)
        /// </summary>
        public int APICallThresholdTimeInterval { get; set; } = 60;
        
    }

    public enum SpeedDistance
    {
        [Description("mph / miles")]
        Miles =1,
        [Description("km/h / km")]
        Km =2
    }
}
