using Qbicles.Models.Operator.Compliance;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Form
{
    /// <summary>
    /// The FormInstance is used to record the use of a form
    /// The data is collected from the form definition and stored in the Elementdata collection
    /// In addition the level of compliance for the instance of the form is recorded in ComplianceLevel
    /// </summary>
    [Table("form_instance")]
    public class FormInstance
    {
        public int Id { get; set; }


        public decimal ComplianceLevel { get; set; }

        /// <summary>
        /// This is the FormDefinition from which this instance was created
        /// </summary>
        [Required]
        public virtual FormDefinition ParentDefinition { get; set; }

        /// <summary>
        /// This is the collection od user data entered on the FromDefinition's FormElements
        /// </summary>
        public virtual List<FormElementData> ElementData { get; set; }

        /// <summary>
        /// This is the TaskInstance with whihc the FormInstance is associated
        /// </summary>
        public virtual TaskInstance ComplianceTaskInstance { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is to indicate whether the FormInstance has been submitted.
        /// Once this is set to true the user can no longer edit the data for the FormInstance
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsSubmitted { get; set; }

    }
}
