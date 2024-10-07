using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// The class on which the Spot Counts rae based
    /// </summary>
    [Table("trad_spotcount")]
    public class SpotCount : DataModelBase
    {

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        public virtual List<SpotCountItem> ProductList { get; set; }

        public virtual ApprovalReq SpotCountApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual SpotCountStatus Status { get; set; } = SpotCountStatus.Draft;

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the spot count is first created the LastUpdateDate = CreatedDate
        /// </summary>
        [Required]
        public DateTime LastUpdatedDate { get; set; }


        /// <summary>
        /// When the spot count is first created the LastUpdatedBy = CreatedBy
        /// </summary>
        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }



        public virtual List<SpotCountLog> Logs { get; set; } = new List<SpotCountLog>();



    }


    public enum SpotCountStatus
    {
        [Description("Draft")]
        Draft = 1,
        [Description("Count Started")]
        CountStarted = 2,    //=> RequestStatusEnum.Pending
        [Description("Count Completed")]
        CountCompleted = 3,  //=> RequestStatusEnum.Reviewed
        [Description("Stock Adjusted")]
        StockAdjusted = 4,   //=> RequestStatusEnum.Approved
        [Description("Discarded")]
        Discarded = 5,        //=> RequestStatusEnum.Discarded
        [Description("Denied")]
        Denied = 6       //=> RequestStatusEnum.Denied
    }

}
