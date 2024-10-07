using Qbicles.Models.Operator;
using Qbicles.Models.Operator.Compliance;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Qbicles.Models.Form
{
    /// <summary>
    /// This is the definition of the form.
    /// It provides collections 
    ///         - to identify the Elements (parts of the form) that are added to the form
    ///         - to identify Instances of use of the form definition, i.e. identify which task have used the form
    /// </summary>
    [Table("form_definition")]
    public class FormDefinition
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The title of the form
        /// </summary>
        [Required]
        public string Title { get; set; }

        public int ComplianceForms_Id { get; set; }
        /// <summary>
        /// A link to the parent ComplianceForm
        /// </summary>
        [Required]
        [ForeignKey("ComplianceForms_Id")]
        public virtual ComplianceForms ComplianceForm { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is to indicate whether the From is available for a user to use.
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsDraft { get; set; }

        /// <summary>
        /// This is a collection of the parts of the forms i.e. text inout, numbers etc
        /// </summary>
        public virtual List<FormElement> Elements { get; set; }

        /// <summary>
        /// This is a collection of the uses of the Form defintion
        /// </summary>
        public virtual List<FormInstance> Instances { get; set; }

        /// <summary>
        /// This is a description of the form
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// DJN: I think that this property can be removed.
        /// The Definition is a throwback to the old form definition capabilities.
        /// I have not removed it from the model because methods making use of the property.
        /// Please check that the methods using this property are no longer needed, remove them and then remove this property.
        /// </summary>
        public string Definition { get; set; }


        public virtual List<QbicleTask> Tasks { get; set; } = new List<QbicleTask>();

        /// <summary>
        /// This is a collection of the Tags associated with the Form
        /// The tags are used in searching for a Form Definition
        /// It is part of a many-to-many relationship
        /// </summary>
        public virtual List<OperatorTag> Tags { get; set; } = new List<OperatorTag>();

        /// <summary>
        /// The time, in minutes, estimated to complete the form
        /// </summary>
        public int EstimatedTime { get; set; }

        [Column(TypeName = "bit")]
        [Required]
        public bool IsHide { get; set; }
    }
}
