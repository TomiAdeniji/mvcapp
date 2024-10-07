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
    [Table("sp_consumptionreportprocesslogs")]
    public class ConsumptionReportProcessLog
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated Consumption Report at the time the Log is created
        /// </summary>
        [Required]
        public ConsumptionReportStatusEnum ConsumptionReportStatus { get; set; }

        /// <summary>
        /// This property records the WasteReport with which the log is associated
        /// </summary>
        [Required]
        public virtual ConsumptionReport ConsumptionReport { get; set; }


        /// <summary>
        /// This property associates the Consumption Report Log that was created when the Consumption Report was updated
        /// </summary>
        [Required]
        public virtual ConsumptionReportLog ConsumptionReportLog { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the update occurred.
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual ApprovalReqHistory ApprovalReqHistory { get; set; }
    }
}
