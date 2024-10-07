using Qbicles.Models.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    /// <summary>
    /// 
    /// </summary>

    [Table("qb_qbiclealert")]
    public class QbicleAlert : QbicleActivity
    {
        public AlertTypeEnum Type { get; set; }

        public AlertPriorityEnum Priority { get; set; }

        public String Content { get; set; }

        public virtual QbicleMedia AttachedFile { get; set; }
        public enum AlertTypeEnum
        {
            General = 1,
            Logistics = 2,
            Staffing = 3
        };


        public enum AlertPriorityEnum
        {
            [Description("Priority 1 (critical)")]
            Critical = 1,
            [Description("Priority 2 (general)")]
            General = 2,
            [Description("Priority 3 (low)")]
            Low = 3
        };

        public QbicleAlert()
        {
            this.ActivityType = ActivityTypeEnum.AlertActivity;
            this.App = ActivityApp.Qbicles;
        }
    }

}
