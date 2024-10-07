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
    /// This class is used to define a clsue of the filter that is used to find
    /// those contacts to include in a segment
    /// </summary>
    [Table("sm_SegmentClause")]
    public class SegmentQueryClause
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the Criteria definition with which the clause is associated
        /// </summary>
        public virtual CustomCriteriaDefinition CriteriaDefinition { get; set; }
        

        /// <summary>
        /// This is the list of options to be used in this clause
        /// </summary>
        public virtual List<CustomOption> Options { get; set; } = new List<CustomOption>();


        /// <summary>
        /// The clause can be associated with only one Segment
        /// </summary>
        [Required]
        public virtual Segment Segment { get; set; }



    }


   
}
