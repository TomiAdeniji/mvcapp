using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Spannered
{
    /// <summary>
    /// The Asset class is an entirely user-created entity that will ultimately have its own Tasks, Inventory, Related Purchases and Resources.
    /// </summary>
    [Table("sp_asset")]
    public class Asset
    {
        /// <summary>
        /// The unique ID to identify the spannered Asset in the database
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The title of the Asset
        /// NOTE: The Title of the Asset MUST be unique within the Domain
        /// </summary>
        [StringLength(250)]
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// The Free version will provide 2 options, 
        /// “Edit” to open the Edit Asset modal, and Hide/Show to hide the Asset from display (or show it if it’s hidden)
        /// </summary>
        public OptionsEnum Options { get; set; }
        /// <summary>
        /// The Identification is an alphanumeric value, for example a barcode number
        /// </summary>
        [StringLength(250)]
        public string Identification { get; set; }
        /// <summary>
        /// This is the image that is associated with the Asset and shown in the UI for the Asset.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        [StringLength(250)]
        public string FeaturedImageUri { get; set; }
        /// <summary>
        /// This is the Description from the Asset ui
        /// </summary>
        /// </summary>
        [StringLength(500)]
        [Required]
        public string Description { get; set; }
        /// <summary>
        /// This is the Domain with which this Asset is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        /// <summary>
        /// This is the Trader Location with this Asset is associated.
        /// </summary>
        public virtual TraderLocation Location { get; set; }
        /// <summary>
        /// The user from Qbicles who created the spannered Asset
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this spannered Asset was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the spannered Asset, 
        /// This is to be set each time the spannered Asset is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this spannered Asset was last edited.
        /// This is to be set each time the spannered Asset is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// This is the Media Folder in the Settings.SourceQbicle in all images, docs and videos
        /// associated with this Asset are stored.
        /// NOTE: If the user choses to create a Folder, it must be unique within the Qbicle.
        /// </summary>
        [Required]
        public virtual MediaFolder ResourceFolder { get; set; }
        [Required]
        public virtual SpanneredWorkgroup Workgroup { get; set; }
        /// <summary>
        /// This is a Link to other Assets of the Asset
        /// </summary>
        public virtual List<Asset> OtherAssets { get; set; } = new List<Asset>();
        /// <summary>
        /// This is a collection of the Meters associated with a Asset
        /// </summary>
        public virtual List<Meter> Meters { get; set; } = new List<Meter>();
        /// <summary>
        /// This is a collection of the AssetTags associated with a Asset
        /// </summary>
        public virtual List<AssetTag> Tags { get; set; } = new List<AssetTag>();
        /// <summary>
        /// This is a collection of the QbicleTasks associated with a Asset
        /// </summary>
        public virtual List<QbicleTask> Tasks { get; set; } = new List<QbicleTask>();
        /// <summary>
        /// This is the TraderTransfer with this Asset is associated.
        /// </summary>
         public virtual List<TraderTransfer> Transfers { get; set; } = new List<TraderTransfer>();
        /// <summary>
        /// This is the TraderItem with this Asset is associated.
        /// Optionally link this Asset to an existing item in your Trader inventory at the current location. This will allow finer control over the physical asset, including the ability to move it between locations.
        /// </summary>
        public virtual TraderItem AssociatedTraderItem { get; set; }
        /// <summary>
        /// This will display a list of all Central Inventory items, which can be optionally assigned to an Asset during its creation. 
        /// Doing so will bind these items to the Asset Inventory. 
        /// The process surrounding this is detailed earlier in this story.
        /// </summary>
        public virtual List<AssetInventory> AssetInventories { get; set; } = new List<AssetInventory>();
        public virtual List<TraderPurchase> AssetTraderPurchases { get; set; } = new List<TraderPurchase>();
        public enum OptionsEnum
        {
            Edit = 1,
            Hide = 2,
            Show = 3
        }
    }
}
