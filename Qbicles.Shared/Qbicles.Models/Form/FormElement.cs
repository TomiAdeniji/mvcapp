using Qbicles.Models.Operator.Compliance;
using Qbicles.Models.Operator.Goals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Form
{
    /// <summary>
    /// This is the input control in which the user will enter a specific piece of information on a form.
    /// The FromDefinition brings together a collection of FromElements which are displayed base don their DisplayOrder.
    /// When a user looks at a QbicleTask that is baed on a ComplianceTask, they will see a tab for each FormDefintion and
    /// on each tab will be the collection of FromElements of the FormDefintion.
    /// When teh user enters data in a FromElement, the information entered by the user is stored in a FormElement
    /// </summary>
    [Table("form_element")]
    public class FormElement
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This is the order in which the FormElement will appear on the FromDefiniotn when dispalyed to a user for 
        /// creatting/editing a form definition and
        ///  when a user sees the form on a QBicles Task
        /// </summary>
        [Required]
        public int DisplayOrder { get; set; }


        /// <summary>
        /// This is the FormDefinition with which the FormElemet is dispalyed
        /// </summary>
        [Required]
        public virtual FormDefinition FormDefintion { get; set; }


        /// <summary>
        /// Only specific types of information can be captured based on the FormElemetType enum
        /// this is the type of information that will be captured by this Form Element
        /// </summary>
        [Required]
        public virtual FormElementType Type { get; set; }


        /// <summary>
        /// This is the label that will appear on the element when it is
        /// displayed for a user to enter data
        /// </summary>
        [Required]
        public string Label { get; set; }


        /// <summary>
        /// The user who defines the form is able to indicate whether Photos/Images can be captured and associated with the FormElementData
        /// </summary>
        [Column(TypeName = "bit")]
        public bool AllowPhotos { get; set; }


        /// <summary>
        /// The user who defines the form is able to indicate whether Docs can be captured and associated with the FormElementData
        /// </summary>
        [Column(TypeName = "bit")]
        public bool AllowDocs { get; set; }


        /// <summary>
        /// The user who defines the form is able to indicate whether Notes can be captured and associated with the FormElementData
        /// </summary>
        [Column(TypeName = "bit")]
        public bool AllowNotes { get; set; }


        /// <summary>
        /// The user who defines the form is able to indicate whether a score can be captured and associated with the FormElementData
        /// </summary>
        [Column(TypeName = "bit")]
        public bool AllowScore { get; set; }

        /// <summary>
        /// This is the measure that will be used for analysis of the data associated with ethe FormElement
        /// The measure will only be selected if the user is this.AllowNotes == True
        /// </summary>
        public virtual Measure AssociatedMeasure { get; set; }


        /// <summary>
        /// This is a collection of the data that is entered by users into a FormElement
        /// </summary>
        public virtual List<FormElementData> FormElementDatas { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }


    public enum FormElementType
    {
        TrueOrFalse = 1,
        Number = 2,
        Date = 3, 
        FreeText = 4
    }

}
