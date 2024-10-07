using Qbicles.Models.B2B;
using Qbicles.Models.Base;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Catalogs
{
    /// <summary>
    /// POS Menu
    /// </summary>
    [Table("pos_menu")]
    public class Catalog : DataModelBase
    {

        [Required]
        public string Name { get; set; }

        public string Image { get; set; }

        public string Description { get; set; }

        [Required]
        public SalesChannelEnum SalesChannel { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        //[Required]
        public virtual TraderLocation Location { get; set; }

        public virtual QbicleDomain Domain { get; set; }

        public virtual List<Category> Categories { get; set; } = new List<Category>();

        public virtual List<PosDevice> Devices { get; set; } = new List<PosDevice>();
        [Column(TypeName = "bit")]
        public bool IsDeleted { set; get; }


        /// <summary>
        /// This is a list of the dimensions that are to be associated with each Transaction Item,
        /// when the order is created and sent back to Trader from PoS
        /// </summary>
        public virtual List<TransactionDimension> OrderItemDimensions { get; set; } = new List<TransactionDimension>();


        [Column(TypeName = "bit")]
        public bool IsPOSSqliteDbBeingProcessed { get; set; } = false;
        [Column(TypeName = "bit")]
        public bool IsBeingQuickModeProcessed { get; set; } = false;
        [Column(TypeName = "bit")]
        public bool IsRefreshPricesDbBeingProcessed { get; set; } = false;
    
        public virtual List<PurchaseSalesPartnership> PurchaseSalesPartnerships { get; set; } = new List<PurchaseSalesPartnership>();

        /// <summary>
        /// This setting indicates whether this catalog can be used in a sales situation (Sales) or is only used as a marketing tool (Distribution)
        /// </summary>
        public virtual CatalogType Type { get; set; } = CatalogType.Sales;


        /// <summary>
        /// This is used to indicate if the menu can be accessed by other busineses viewingg the Catalog associated with the publishing business
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsPublished { set; get; } = false;

        public virtual List<B2BProfile> B2BProfiles { get; set; }

        public string ProductFile { get; set; }
        /// <summary>
        /// This is a bool to indicate that there are prices in the catalog that have been updated
        /// because of a tax change to the underlying item's taxes
        /// </summary>
        [Column(TypeName = "bit")]
        public bool FlaggedForTaxUpdate { get; set; } = false;

        /// <summary>
        /// This is a bool to indicate that there are prices in the catalog that have been updated
        /// because of a change to the underlying item's latest cost at the location at which the catalog is based
        /// </summary>
        [Column(TypeName = "bit")]
        public bool FlaggedForLatestCostUpdate { get; set; } = false;
    }
}

public enum CatalogType
{

    [Description("Sales Catalog")]
    Sales = 0,

    [Description("Distribution Catalog")]
    Distribution = 1
}
