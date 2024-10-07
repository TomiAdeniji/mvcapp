using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Web.UI;

namespace Qbicles.Models.Form
{
    /// <summary>
    /// THis is the data that is entered into a FromElemenet by a user
    /// </summary>
    [Table("form_elementdata")]
    public class FormElementData
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This is the FormInstance associated with the element data
        /// </summary>
        [Required]
        public virtual FormInstance ParentInstance { get; set; }


        /// <summary>
        /// This is the value that the user enters into the FormElement
        /// THis item can be from
        ///     public enum FormElementType
        ///     {
        ///         TrueOrFalse = 1,
        ///         Number = 2,
        ///         Date = 3, 
        ///         FreeText = 4
        ///     }
        /// </summary>
        /// 
        /// Because the value can be any of these types I can only think that it must 
        /// be saved as a string, possibly as a JSON. I need developer direction on what is the best solution here.
        [Required]
        public string Value { get; set; }

        /// <summary>
        /// This is the FromElement that was used by the user to enter this data
        /// </summary>
        [Required]
        public virtual FormElement ParentElement { get; set; }


        /// <summary>
        /// This is a collection of the Attachments associated that could be associated with teh form element
        /// The Attachments collection will contains Notes, Photos & Documents
        /// </summary>
        public virtual System.Collections.Generic.List<Attachment> Attachments { get; set; }


        /// <summary>
        /// This is the score that will be set for the ElementData, if the FormElement allows a score to be set.
        /// </summary>
        public decimal Score { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

    }
}
