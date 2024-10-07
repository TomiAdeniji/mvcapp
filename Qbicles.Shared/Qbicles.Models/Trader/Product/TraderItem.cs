using Qbicles.Models.Base;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader.Product;
using Qbicles.Models.Trader.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Models.Trader
{
    [Table("trad_item")]
    public class TraderItem: DataModelBase
    {
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderGroup Group { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        //[StringLength(50, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        public virtual List<ProductUnit> Units { get; set; } = new List<ProductUnit>();


        [Required(ErrorMessage = "Item description is required")]
        //[StringLength(200, MinimumLength = 4, ErrorMessage = "Description should be minimum 4 characters and a maximum of 200 characters")]
        [DataType(DataType.Text)]
        [AllowHtml]
        public string Description { get; set; }
        /// <summary>
        /// If description length > 150 characters:Truncate at 150 chars, and add an ellipsis               
        /// </summary>
        public string DescriptionText { get; set; }


        //This is the URI for the image after it has been saved through Qbicles.Doc 
        [Required(ErrorMessage = "Item image is required")]
        [DataType(DataType.Text)]
        public string ImageUri { get; set; }
        /// <summary>
        /// Item I buy
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsBought { get; set; }
        /// <summary>
        /// Item I sale
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsSold { get; set; }

        [Column(TypeName = "bit")]
        public bool IsCommunityProduct { get; set; }

        public virtual BKAccount PurchaseAccount { get; set; }

        public virtual BKAccount SalesAccount { get; set; }

        public virtual List<TaxRate> TaxRates { get; set; }

        public virtual BKAccount InventoryAccount { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "bit")]
        public bool IsCompoundProduct { get; set; }

        public virtual List<Recipe> AssociatedRecipes { get; set; } = new List<Recipe>();

        public virtual List<InventoryDetail> InventoryDetails { get; set; } = new List<InventoryDetail>();

        public virtual List<TraderItemVendor> VendorsPerLocation { get; set; } = new List<TraderItemVendor>();

        [Column(TypeName = "bit")]
        public bool IsActiveInAllLocations { get; set; }

        public virtual List<TraderLocation> Locations { get; set; } = new List<TraderLocation>();


        public string Barcode { get; set; }

        public string SKU { get; set; }

        /// <summary>
        /// Addition Info is split into  
        ///  - Brands, 
        ///  - Tags, 
        ///  - Needs and 
        ///  - Ratings
        /// </summary>
        public virtual List<AdditionalInfo> AdditionalInfos { get; set; } = new List<AdditionalInfo>();

        public virtual List<ResourceDocument> ResourceDocuments { get; set; } = new List<ResourceDocument>();
        /// <summary>
        /// Return string TaxRates
        /// </summary>
        /// <param name="isSale">true: is Sale TaxRates,false: is Purchase TaxRates</param>
        /// <returns></returns>
        public string StringTaxRates(bool isSale)
        {
            if(isSale)
                return !this.TaxRates.Any() ? "" : string.Join(",", this.TaxRates.Where(s=>!s.IsPurchaseTax).Select(s => $"{s.Name}"));
            else
                return !this.TaxRates.Any() ? "" : string.Join(",", this.TaxRates.Where(s => s.IsPurchaseTax).Select(s => $"{s.Name}"));
        }
        /// <summary>
        /// Return string TaxRates
        /// </summary>
        /// <param name="isSale">true: is Sale TaxRates,false: is Purchase TaxRates</param>
        /// <returns></returns>
        public string StringItemTaxRates(bool isSale)
        {
            if (isSale)
                return !this.TaxRates.Any() ? "" : string.Join(",", this.TaxRates.Where(s => !s.IsPurchaseTax).Select(s => $"{s.Rate}-{s.Name}"));
            else
                return !this.TaxRates.Any() ? "" : string.Join(",", this.TaxRates.Where(s => s.IsPurchaseTax).Select(s => $"{s.Rate}-{s.Name}"));
        }
        /// <summary>
        /// Return string TaxRates Value
        /// </summary>
        /// <param name="isSale">true: is Sale TaxRates,false: is Purchase TaxRates</param>
        /// <returns></returns>
        public string StringTaxRatesValue(bool isSale)
        {
            if(isSale)
                return this.TaxRates == null ? "" : string.Join(";", this.TaxRates.Where(s => !s.IsPurchaseTax).Select(s => $"{s.Id}-{s.Rate}"));
            else
                return this.TaxRates == null ? "" : string.Join(";", this.TaxRates.Where(s => s.IsPurchaseTax).Select(s => $"{s.Id}-{s.Rate}"));
        }
        /// <summary>
        /// Return sum rate TaxRates
        /// </summary>
        /// <param name="isSale">true: is Sale TaxRates,false: is Purchase TaxRates</param>
        /// <returns></returns>
        public decimal SumTaxRates(bool isSale)
        {
            if(isSale)
                return this.TaxRates == null ? 0 : (this.TaxRates.Where(s => !s.IsPurchaseTax).Sum(s=>s.Rate) / 100);
            else
                return this.TaxRates == null ? 0 : (this.TaxRates.Where(s => s.IsPurchaseTax).Sum(s => s.Rate) / 100);
        }
        /// <summary>
        /// Return sum rate percent TaxRates
        /// </summary>
        /// <param name="isSale">true: is Sale TaxRates,false: is Purchase TaxRates</param>
        /// <returns></returns>
        public decimal SumTaxRatesPercent(bool isSale)
        {
            if (isSale)
                return this.TaxRates == null ? 0 : this.TaxRates.Where(s => !s.IsPurchaseTax).Sum(s => s.Rate);
            else
                return this.TaxRates == null ? 0 : this.TaxRates.Where(s => s.IsPurchaseTax).Sum(s => s.Rate);
        }



        /// <summary>
        /// This is the collection of GalleryItems associated with the TraderItem
        /// </summary>
        public virtual List<ProductGalleryItem> GalleryItems { get; set; } = new List<ProductGalleryItem>();
    }
}