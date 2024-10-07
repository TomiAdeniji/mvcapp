//using Qbicles.Models.Trader.ODS;
//using Qbicles.Models.Trader.PoS;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Qbicles.Models.Trader.SalesChannel
//{
//    [Table("settings_storefront")]
//    public class StoreFrontSettings : SalesChannelSettings
//    {
//        public StoreFrontSettings()
//        {
//            this.OrderHandling = PrepQueueStatus.NotStarted;
//            this.OrderStatusWhenAddedToQueue = PrepQueueStatus.NotStarted;
//            this.SalesChannel = SalesChannelEnum.Storefront;
//        }
//        public virtual WorkGroup DefaultWorkGroup { get; set; }

//        /// <summary>
//        /// This is the currency symbol that will be used for displaying money 
//        /// on the PoS. It can be null
//        /// </summary>
//        public string MoneyCurrency { get; set; }

//        /// <summary>
//        /// The number of decimal places to be used on the PoS to display money
//        /// </summary>
//        public int MoneyDecimalPlaces { get; set; }

//        public virtual PosOrderType DeliveryOrderType { get; set; }

//        public virtual PosOrderType CustomerPickUpOrderType { get; set; }

//        public PrepQueueStatus OrderHandling { get; set; } = PrepQueueStatus.NotStarted;

//    }

//}
