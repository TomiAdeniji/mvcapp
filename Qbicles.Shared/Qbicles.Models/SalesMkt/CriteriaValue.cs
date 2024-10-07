using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    /// <summary>
    /// When a Custom Criteria is created and a vlue is entered for the Criteria for a 
    /// particular Contact, a CriteriaValue must be created to indicated which value 
    /// of the CustomCriteria has been set for that contact.
    /// </summary>
    [Table("sm_SegmentCriteriaValue")]
    public partial class CriteriaValue
    {
        /// <summary>
        /// This is a unique ID for the CriteriaValue in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is the definition of the Custom Criteria on which this value is based
        /// </summary>
        [Required]
        public virtual CustomCriteriaDefinition Criteria { get; set; }


        /// <summary>
        /// This is the custom option from th eoption list selected by the user for the associated contact
        /// A CustomCriteriaDefinition is effectively an option list,
        /// this value indicates which of the options has been chose for the selected contact.
        /// </summary>
        [Required]
        public virtual CustomOption Option { get; set; }


        /// <summary>
        /// This is the contact with which this value is associated
        /// </summary>
        [Required]
        public virtual SMContact Contact { get; set; }




    }





}
