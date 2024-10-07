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
    /// The Visit class allows the user to record a non-scheduled visit to a Place.
    /// When a Scheduled Task (QbicleTask) is created to instruct someone visit a place
    /// or carry out some other activity at a place then a Visit will be created that is
    /// associated with the QBicleTask
    /// </summary>
    [Table("sm_Visit")]
    public class Visit
    {
        /// <summary>
        /// The unique ID to identify the visit in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is the Place with which this visit is associated
        /// </summary>
        [Required]
        public virtual Place Place { get; set; }

        /// <summary>
        /// The user from Qbicles who created the visit
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        /// <summary>
        /// This is the date and time on which this vidit was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the place, this is to be set each time the viit is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        /// <summary>
        /// This is the date and time on which this place was last edited.
        /// This is to be set each time the visit is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// The date and time at which the visit is to occur
        /// </summary>
        public DateTime VisitDate { get; set; }

        /// <summary>
        /// A string to indicate the purpose of the visit
        /// </summary>
        public VisitReason Reason { get; set; }

        /// <summary>
        /// A task that can be associated with this visit
        /// </summary>
        public virtual QbicleTask AssociatedTask { get; set; }

        /// <summary>
        /// This is the number of leads generated from the visit
        /// </summary>
        public int NumberOfGeneratedLeads { get; set; }


        /// <summary>
        /// These are the notes recorded from the visit
        /// </summary>
        public string Notes { get; set; }


        /// <summary>
        /// This is the user who carries out the visit
        /// </summary>
        [Required]
        public virtual ApplicationUser Agent { get; set; }
    }

    public enum VisitReason
    {
        AssignedTask = 1,
        AdvertisingStall = 2,
        ColdCalling = 3, 
        LeafletDistribution =4,
        Other = 5
            
    }
}
