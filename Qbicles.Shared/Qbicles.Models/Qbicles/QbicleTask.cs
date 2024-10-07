using CleanBooksData;
using Qbicles.Models.Form;
using Qbicles.Models.Operator.Compliance;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Spannered;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    /// <summary>
    /// Concrete model for QbicleTask.  Note this implementation does not allow any user configuration of the object
    /// </summary>
    /// 
    [Table("qb_qbicletask")]
    public class QbicleTask : QbicleActivity
    {

        public virtual task task { get; set; }

        public virtual QbicleMedia AttachedFile { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "bit")]
        public bool isSteps { get; set; }

        [Column(TypeName = "bit")]
        public bool isRecurs { get; set; }

        public TaskDurationUnitEnum DurationUnit { get; set; }

        public short Duration { get; set; }

        /// <summary>
        /// Use for Asset Tasks in the Spannered APP take a meter threshold value
        /// </summary>
        public int? MeterThreshold { get; set; }

        /// <summary>
        /// Asset Tasks are created from, and associated with an Asset in Spannered
        /// </summary>
        public virtual Asset Asset { get; set; }
        public virtual SpanneredWorkgroup Workgroup { get; set; }
        public virtual List<FormDefinition> FormDefinitions { get; set; }

        public TaskPriorityEnum Priority { get; set; }  

        public DateTime? DueDate { get; set; }

        public TaskRepeatEnum Repeat { get; set; }
        public virtual PipelineContact PipelineContact { get; set; }
        public virtual Place Place { get; set; }

        public virtual ICollection<QbicleStep> QSteps { get; set; }
        public virtual ICollection<QbicleTimeSpent> QTimeSpents { get; set; }
        public virtual ICollection<QbiclePerformance> QPerformances { get; set; }
        public virtual ICollection<QbicleStepInstance> QStepinstances { get; set; }
        public virtual ICollection<ConsumablesPartServiceItem> ConsumableItems { get; set; }

        /// <summary>
        /// This is the ComplianceTask from which this QbiclesTask has been created (assuming it is created from a ComplianceTask)
        /// </summary>
        public virtual ComplianceTask ComplianceTask { get; set; }

        public enum TaskPriorityEnum
        {
            [Description("Priority 1 (critical)")]
            Critical = 1,
            [Description("Priority 2 (general)")]
            General = 2,
            [Description("Priority 3 (low)")]
            Low = 3
        };

        public enum TaskRepeatEnum
        {
            [Description("No")]
            No = 1,
            [Description("Daily")]
            Daily = 2,
            [Description("Weekly")]
            Weekly = 3,
            [Description("Monthly")]
            Monthly = 4
        };

        public enum TaskDurationUnitEnum
        {
            Hours= 0,
            Days=1,
            Weeks=2
        }
        public QbicleTask ()
        {
            ActivityType = ActivityTypeEnum.TaskActivity;
            this.App = ActivityApp.Qbicles;
            Priority = TaskPriorityEnum.General;
            State = ActivityStateEnum.Open;
            QSteps = new HashSet<QbicleStep>();
            QTimeSpents = new HashSet<QbicleTimeSpent>();
            QPerformances = new HashSet<QbiclePerformance>();
            QStepinstances = new HashSet<QbicleStepInstance>();
            ConsumableItems = new HashSet<ConsumablesPartServiceItem>();
        }
    }
}