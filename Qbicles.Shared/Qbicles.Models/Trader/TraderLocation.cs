using Qbicles.Models.B2B;
using Qbicles.Models.Base;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_location")]
    public class TraderLocation : DataModelBase
    {
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<TraderItem> Items { get; set; } = new List<TraderItem>();

        public virtual List<InventoryDetail> Inventory { get; set; } = new List<InventoryDetail>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public bool IsDefaultAddress { get; set; }

        public virtual List<TraderItemVendor> VendorsAndItems { get; set; } = new List<TraderItemVendor>();

        public virtual List<TraderSale> Sales { get; set; } = new List<TraderSale>();

        public virtual List<TraderPurchase> Purchases { get; set; } = new List<TraderPurchase>();

        public virtual TraderAddress Address { get; set; }

        public virtual List<PriceBookInstance> PriceBooks { get; set; }

        public virtual List<Driver> Drivers { get; set; } = new List<Driver>();

        public virtual List<Catalog> PosMenus { get; set; } = new List<Catalog>();
        public virtual List<B2BTradingItem> B2BTradingItems { get; set; }
        public virtual List<B2BCatalogItem> B2BCatalogItems { get; set; }
        public virtual List<B2BLogisticsAgreement> B2BLogisticsAgreements { get; set; }
        public virtual List<POSTable> Tables { get; set; } = new List<POSTable>();
        public virtual List<B2BProfile> B2BProfiles { get; set; }
        public virtual List<VoucherInfo> VoucherInfos { get; set; }
    }
}
