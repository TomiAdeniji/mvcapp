using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Web.UI;
using Qbicles.Models.Form;

namespace Qbicles.Models.Operator.Compliance
{
    /// <summary>
    /// This class provided a means to control the order of display of the Forms when they are attached to the 
    /// ComplianceTask and, through the ComplianceTask, the QbicleTask i.e. the order in which the tabs for the forms will be displayed
    /// </summary>
    [Table("compliance_orderedform")]
    public class OrderedForm
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This is the order in which the tabs will be displayed on the 'page' for QbicleTask
        /// </summary>
        [Required]
        public int DisplayOrder { get; set; }


        /// <summary>
        /// The form definition whose order is being controlled.
        /// </summary>
        [Required]
        public virtual FormDefinition FormDefinition { get; set; }


        /// <summary>
        /// This is the Compliance task with which the ordererd form is associated
        /// </summary>
        [Required]
        public virtual ComplianceTask ParentComplianceTask { get; set; }
    }
}
