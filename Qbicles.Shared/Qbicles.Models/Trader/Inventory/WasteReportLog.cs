using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// The class on for logging changes to Waste Reports 
    /// </summary>
    [Table("trad_wastereportlog")]
    public class WasteReportLog
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// This is the waste report for which this is a log
        /// </summary>
        [Required]
        public WasteReport WasteReport { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// For the lot of waste items we do not refer to the 
        /// public virtual List<WasteItem> ProductList { get; set; }
        /// 
        /// Instead we refer to the WasteItemLogs collection
        /// </summary>
        public virtual List<WasteItemLog> WasteItemLogs { get; set; } = new List<WasteItemLog>();

        
        public virtual ApprovalReq WasteApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual WasteReportStatus Status { get; set; } = WasteReportStatus.Draft;


        /// <summary>
        /// This is the current user when the log was created
        /// </summary>
        [Required]
        public virtual ApplicationUser UpdatedBy { get; set; }

        /// <summary>
        /// This is the date on which the LOG is created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }

        


    }




}
