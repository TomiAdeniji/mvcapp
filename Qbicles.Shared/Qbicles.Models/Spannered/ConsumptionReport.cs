using Qbicles.Models.Base;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Spannered
{
    [Table("sp_consumptionreports")]
    public class ConsumptionReport : DataModelBase
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        [Required]
        public virtual TraderLocation Location { get; set; }
        [Required]
        public virtual SpanneredWorkgroup Workgroup { get; set; }
        public virtual QbicleTask AssociatedTask { get; set; }
        public virtual List<ConsumptionItem> ConsumptionItems { get; set; } = new List<ConsumptionItem>();
        public virtual ApprovalReq ConsumptionApprovalProcess { get; set; }
        /// <summary>
        /// This is the collection of Logs of the Waste Report (copies of this record)
        /// </summary>
        public virtual List<ConsumptionReportLog> Logs { get; set; } = new List<ConsumptionReportLog>();
        /// <summary>
        /// This is the collection of ProcessLogs i.e. steps through the approval process, associated with the waster report
        /// </summary>
        public virtual List<ConsumptionReportProcessLog> ProcessLogs { get; set; } = new List<ConsumptionReportProcessLog>();
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public virtual ApplicationUser LastUpdatedBy { get; set; }
        [Required]
        public ConsumptionReportStatusEnum Status { get; set; }
        public enum ConsumptionReportStatusEnum
        {
            [Description("Draft")]
            Draft = 1,
            [Description("Awaiting Review")]
            Pending =2,
            [Description("Awaiting Approval")]
            Reviewed,
            [Description("Approved")]
            Approved,
            [Description("Denied")]
            Denied,
            [Description("Discarded")]
            Discarded
        }
    }
}
