using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.Models.Spannered.ConsumptionReport;

namespace Qbicles.Models.Spannered
{
    [Table("sp_consumptionreportlogs")]
    public class ConsumptionReportLog
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public virtual ConsumptionReport ConsumptionReport { get; set; }
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
        public virtual QbicleTask AssociatedTask { get; set; }
        [Required]
        public virtual SpanneredWorkgroup Workgroup { get; set; }
        public virtual ApprovalReq ConsumptionApprovalProcess { get; set; }
        [Required]
        public ConsumptionReportStatusEnum Status { get; set; }
        public virtual List<ConsumptionItemLog> ConsumptionItemLogs { get; set; } = new List<ConsumptionItemLog>();
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public virtual ApplicationUser LastUpdatedBy { get; set; }
    }
}
