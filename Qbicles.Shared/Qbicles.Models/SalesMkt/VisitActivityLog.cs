using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_VisitActivityLog")]
    public class VisitActivityLog
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing VisitActivityLog in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is the Domain with which this VisitActivityLog is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The user from Qbicles who created the sales and marketing VisitActivityLog
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing VisitActivityLog was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the sales and marketing VisitActivityLog, 
        /// This is to be set each time the sales and marketing VisitActivityLog is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing VisitActivityLog was last edited.
        /// This is to be set each time the sales and marketing VisitActivityLog is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the start date and time of the activity
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }


        /// <summary>
        /// This is the end date and time of the activity
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// This i sthe number of visits
        /// </summary>
        [Required]
        public int NumberOfVisits { get; set; }


        /// <summary>
        /// These are the notes recorded from the activity
        /// </summary>
        public string Notes { get; set; }

    }
}
