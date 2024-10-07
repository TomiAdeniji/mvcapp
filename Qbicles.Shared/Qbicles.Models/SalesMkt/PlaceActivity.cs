using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    /// <summary>
    /// The PLaceActivity allows the user to record an activity that has taken place at a place.
    /// It is important to recognise that the Activity here is NOT a QBicleActivity
    /// </summary>
    [Table("sm_PlaceActivity")]
    public class PlaceActivity
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing PlaceActivity in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is the Place with which this PlaceActivity is associated
        /// </summary>
        [Required]
        public virtual Place Place { get; set; }


        /// <summary>
        /// The user from Qbicles who created the sales and marketing PlaceActivity
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing PlaceActivity was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This the the Date AND Time at which the activity started
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// This the the Date AND Time at which the activity ended
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }


        /// <summary>
        /// This is the number of people counted as part of the activity
        /// </summary>
        public int RecordedFootfall { get; set; }


        /// <summary>
        /// These are notes taken as part of the activity
        /// </summary>
        public string Notes { get; set; }
    }
}
