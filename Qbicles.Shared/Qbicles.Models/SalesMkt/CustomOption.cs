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
    /// A CustomCustomCriteria definition is effective a list of options.
    /// This class is used to define each of the options in a Custom Criteria
    /// </summary>
    [Table("sm_CustomOption")]
    public class CustomOption
    {
        /// <summary>
        /// This is the unique ID for the CustomOption in the database
        /// This is used as the value of the option when it is displayed in a drop-down list
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// This is the option label which will be displayed in a drop-down list when the option
        /// is displayed for selection
        /// </summary>
        [Required]
        public string Label { get; set; }


        /// <summary>
        /// This field is used to determin in whch order the items are displayed 
        /// in the drop-down list in which they appear
        /// </summary>
        [Required]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// This is the CustomCriteriaDefinition with which this option is associated
        /// A Custom can only be associated with one CustomCriterialDefintion
        /// </summary>
        [Required]
        public virtual CustomCriteriaDefinition CustomCriteriaDefinition { get; set;}



        /// <summary>
        /// An option can be associated with more than one SegmentQueryClass.
        /// The Option could be used in the filters for two different segments
        /// </summary>
        public virtual List<SegmentQueryClause> SegmentQueryClauses { get; set; }

    }
}
