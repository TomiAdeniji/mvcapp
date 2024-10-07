using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Form;

namespace Qbicles.Models
{
    [Table("qb_approvalreqformref")]
    public class ApprovalReqFormRef
    {
        public int Id { get; set; }

        /// <summary>
        ///  a propertylink to the Task ApprovalReq will use the form definition
        /// </summary>
        /// 
        [Required]
        public virtual ApprovalReq ApprovalReq { get; set; }


        /// <summary>
        /// a property to link to the FormDefinition used for the form
        /// </summary>
        //[Required]
        public virtual FormDefinition FormDefinition { get; set; }

        /// <summary>
        /// a property for the date and time at which the form data was last filled in
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// a property to link to the ApplicationUser who last edited data in the form.
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This only values Json entered
        /// This is to be either an xml or json field that will hold the FormData
        /// This field will be blank until a user fills in data on the Task Form.
        /// </summary>
        public string FormData { get; set; }


        /// <summary>
        /// This is template task form and value entered
        /// This is to be either an xml or json field that will hold the FormData
        /// This field will be blank until a user fills in data on the Task Form.
        /// </summary>
        public string FormBuilder { get; set; }
    }
}