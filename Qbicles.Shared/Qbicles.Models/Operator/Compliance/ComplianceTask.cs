using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator.Compliance
{
    /// <summary>
    /// The compliance task is the definition of a task in Operator.
    /// It defines a list of Forms (this.OrderedForms) that can be used in creating one or more QbicleTasks (this.Tasks)
    /// The collection of OrderedForms references the FormDefinitions in the order in which they are selected
    ///         so that, when the forms are displayed to the user in a QbicleTask they are in the correct order.
    /// The collection of QbicleTasks references
    ///         the ONE QbicleTask created if the ComplianceTask is a Repeatable Task (this.Type=Repeatable)
    ///         a collection of one or more QbicleTasks created if the ComplianceTask is Fixed (there could be more than one QbicleTask if recurrence is used for the QbicleTask)
    /// </summary>
    [Table("compliance_task")]
    public class ComplianceTask
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The name of the compliance task.
        /// The Name must be unique WITHIN the Domain.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// A list of Forms (this.OrderedForms) that can be used in creating one or more QbicleTasks (this.Tasks)
        /// </summary>
        public virtual List<OrderedForm> OrderedForms { get; set; } = new List<OrderedForm>();

        /// <summary>
        /// This is the Domain with which the ComplianceTask is associated.
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// This is a description of the Compliance Task.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is the WorkGroup with which the ComplianceTask is associated
        /// </summary>
        public virtual OperatorWorkGroup WorkGroup { get; set; }

        /// <summary>
        /// This indicates whether the Compliance task is Fixed or Repeatable.
        /// A Repeatable Compliance Task can create a 'special' QbicleTask
        ///                 that QBicleTask can be 'Restarted' with new blank FormInstances
        /// A Fixed Compliance Task can create a QbiclesTask,
        ///                 that can be a 'normal' QbicleTask or can create multiple QbicleTasks if recurrence is used.
        ///                 in either case the QBiclesTasks cannot be repeated.
        /// </summary>
        public TaskType Type { get; set; }

        /// <summary>
        /// This is the collection of QbicleTasks that are created from the ComplianceTask.
        /// If the ComplianceTask.Type=Repeatable then the collection will contain only one QbicleTask.
        /// If the ComplianceTask.Type=Fixed then the collection can contain one QbicleTask (if there is no Recurrence) or multiple QbicleTasks, one for each recurrence.
        /// </summary>
        public virtual List<QbicleTask> Tasks { get; set; } = new List<QbicleTask>();

        public virtual List<TaskInstance> TaskInstances { get; set; } = new List<TaskInstance>();

        public DateTime? ExpectedEnd { get; set; }

        [Required]
        public virtual ApplicationUser Assignee { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// A discussion where this place can be discussed
        /// </summary>
        public virtual QbicleDiscussion Discussion { get; set; }
    }

    public enum TaskType
    {
        Fixed = 0,
        Repeatable = 1
    }
}