using Qbicles.Models.Form;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator.Compliance
{
    /// <summary>
    /// The taskInstance is used to link a ComplianceTask to the Forms and QbiclesTasks
    /// This is required because if a ComplianceTask.Type == Repeatable => the same QbicleTask could be used 
    /// multiple times, each time with new forms for a user to fill in.
    /// In order to keep track of this, each time a QbicleTask is reused (for a ComplianceTask) a new
    /// TaskInstance is created.
    /// In the case where the ComplianceTask is Fixed, a TaskInstance will be created for each QbicleTask
    /// created
    /// </summary>
    [Table("compliance_taskinstance")]
    public class TaskInstance
    {
       
        public int Id { get; set; }


        /// <summary>
        /// This is the ComplianceTask with which the taskInstance is associated
        /// </summary>
        [Required]
        public virtual ComplianceTask ParentComplianceTask { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the QbicleTask with which the TaskInstance is associated
        /// </summary>
        [Required]
        public virtual QbicleTask AssociatedQbicleTask { get; set; }

        /// <summary>
        /// This is a collection of the FormInstances that is asssociated with the TaskInstance
        /// Each Form instance is effectively the data gathered in a Form
        /// </summary>
        public virtual List<FormInstance> FormInstances { get; set; }
    }
}