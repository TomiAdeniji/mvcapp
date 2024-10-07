using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_CustomerSegment")]
    public class CustomerSegment
    {
        /// <summary>
        /// The unique ID to identify the CustomerSegment in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who created the CustomerSegment
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this CustomerSegment was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the CustomerSegment, this is to be set each time the CustomerSegment is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this CustomerSegment was last edited.
        /// This is to be set each time the CustomerSegment is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the Domain with which this CustomerSegment is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The name of the CustomerSegment.
        /// NOTE: The name of the CustomerSegment MUST be unique within the Domain
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// This indicates that a Customer segment can be associated with more than one value proposition
        /// It completes the Man to Manz relationship with ValuePropositions
        /// </summary>
        public virtual List<ValueProposition> ValuePropositions { get; set; } = new List<ValueProposition>();
    }
}
