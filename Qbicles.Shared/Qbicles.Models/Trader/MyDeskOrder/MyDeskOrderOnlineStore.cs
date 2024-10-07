//using Qbicles.Models.Attributes;
//using Qbicles.Models.Trader.PoS;
//using Qbicles.Models.Trader.Pricing;
//using System;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using Qbicles.Models.Catalogs;

//namespace Qbicles.Models.Trader.MyDeskOrder
//{
//    /// <summary>
//    /// MyDesk Store
//    /// </summary>
//    [Table("mydesk_onlinestore")]
//    public class OnlineStore
//    {
//        [Required]
//        public int Id { get; set; }
//        /// <summary>
//        /// The name of the store MUST be unique with the Qbicles System
//        /// </summary>
//        [Required]
//        [StringLength(40)]
//        public string Name { get; set; }


//        /// <summary>
//        /// Each store is associated with one, and only one, Location
//        /// </summary>
//        [Required]
//        public virtual TraderLocation Location { get; set; }


//        /// <summary>
//        /// This is so that a user can search for 
//        /// </summary>
//        [Required]
//        public StoreType Category { get; set; }


//        /// <summary>
//        /// A summary description of the store
//        /// </summary>
//        [StringLength(500)]
//        public string Summary { get; set; }


//        /// <summary>
//        /// A boolean to indicate if the store is in operation.
//        /// If enabled it can be displayed on a users MyDesk ordering screen
//        /// </summary>
//        [Column(TypeName = "bit")]
//        public bool IsEnabled { get; set; }


//        /// <summary>
//        /// This is a pointer to the menu that is to be used for the store
//        /// </summary>
//        public virtual Catalog Menu { get; set; }


//        /// <summary>
//        /// This is the image that will be displayed on the store in the user's mydesk
//        /// </summary>
//        [Required]
//        public string Logo { get; set; }


//        /// <summary>
//        /// This is the image that will be displayed on the product menu in the user's mydesk
//        /// </summary>
//        [Required]
//        public string HeroImage { get; set; }

//        /// <summary>
//        /// A slogan to appear on the product menu when it is displayed ina user's mydeskof the store
//        /// </summary>
//        [Required]
//        [StringLength(50)]
//        public string Slogan { get; set; }

//        public ApplicationUser CreatedBy { get; set; }
//        public DateTime CreatedDate { get; set; }

//        //This charge is a charge from the store to the customer for the delivery of goods.
//        //It is not a changer from the logistics partner to the store for carrying out the delivery.
//        [DecimalPrecision(10, 7)]
//        public decimal DeliveryChargePrice { get; set; }
//        public virtual TraderItem DeliveryChargeItem { get; set; }

//        public virtual WorkGroup AssociatedWorkGroup { get; set; }
//    }


//    public enum StoreType
//    {
//        [Description("Fast food")]
//        FastFood = 1,
//        [Description("Creative Services")]
//        CreativeServices = 2,
//        [Description("Household")]
//        Household = 3
//    }
//}
