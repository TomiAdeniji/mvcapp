//using Qbicles.Models.TraderApi;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Helpers;

//namespace Qbicles.Models.Trader.MyDeskOrder
//{
//    [Table("mydesk_order")]
//    public class MyDeskOrder
//    {
//        public int Id { get; set; }

//        public DateTime CreatedDate { get; set; }

//        public virtual OnlineStore OnlineStore { get; set; }

//        /// <summary>
//        /// This is a string for storing the JSON of the order based on Qbicles.Models.TraderApi.Order
//        /// </summary>
//        public string OrderJson { get; set; }


//        /// <summary>
//        /// Property to get and set the order and have it converted to the OrderJson for storing in the database
//        /// </summary>
//        [NotMapped]
//        public Order Order { get; set; }

//        public MyDeskOrderStatus Status { get; set; }

//        public virtual TraderAddress DeliveryAddress { get; set; }

//        public PickupInformation PickupInformation { get; set; }

//        public virtual PaymentMethod PaymentMethod { get; set; }

//        public virtual ApplicationUser CreatedBy { get; set; }
//    }

//    public enum MyDeskOrderStatus
//    {
//        Draft = 1,
//        Complete = 2
//    }

//    //public enum ReceiveMethod
//    //{
//    //    Delivery = 1,
//    //    Pickup = 2
//    //}

//    public class PickupInformation
//    {
//        public DateTime? PickingDate { get; set; }
//        public DateTime? PickingTime { get; set; }
//        public String Note { get; set; }
//    }
//}
