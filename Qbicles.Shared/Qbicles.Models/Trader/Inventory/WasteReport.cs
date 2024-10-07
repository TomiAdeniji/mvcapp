using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qbicles.Models.Base;
namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// The class on which the Waste Reports are based
    /// </summary>
    [Table("trad_wastereport")]
    public class WasteReport : DataModelBase
    {

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        public virtual List<WasteItem> ProductList { get; set; }

        public virtual ApprovalReq WasteApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual WasteReportStatus Status { get; set; } = WasteReportStatus.Draft;

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }

        /// <summary>
        /// This is the collection of Logs of the Waste Report (copies of this record)
        /// </summary>
        public virtual List<WasteReportLog> Logs { get; set; } = new List<WasteReportLog>();


        /// <summary>
        /// This is the collection of ProcessLogs i.e. steps through the approval process, associated with the waster report
        /// </summary>
        public virtual List<WasteReportProcessLog> ProcessLogs { get; set; } = new List<WasteReportProcessLog>();

            


    }


    public enum WasteReportStatus
    {
        [Description("Draft")]
        Draft = 1,
        [Description("Started")]
        Started = 2,    //=> RequestStatusEnum.Pending
        [Description("Completed")]
        Completed = 3,  //=> RequestStatusEnum.Reviewed
        [Description("Stock Adjusted")]
        StockAdjusted = 4,   //=> RequestStatusEnum.Approved
        [Description("Discarded")]
        Discarded = 5        //=> RequestStatusEnum.Discarded

    }

}
