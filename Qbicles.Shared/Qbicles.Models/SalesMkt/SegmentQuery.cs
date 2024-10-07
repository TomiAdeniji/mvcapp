using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_SegmentQuery")]
    public class SegmentQuery
    {
        /// <summary>
        /// The unique ID to identify the SegmentQuery in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who SegmentQuery the attribute
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this SegmentQuery was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the SegmentQuery, this is to be set each time the SegmentQuery is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this SegmentQuery was last edited.
        /// This is to be set each time the SegmentQuery is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// The Domain with which the Segmentn is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }



    }
}
