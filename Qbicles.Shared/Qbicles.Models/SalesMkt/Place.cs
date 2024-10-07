using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    /// <summary>
    /// The Place is an actual place in the world with which a contact can be associated
    /// </summary>
    [Table("sm_Place")]
    public class Place
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing Place in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is the Domain with which this Place is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The user from Qbicles who created the sales and marketing Place
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing Place was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the sales and marketing Place, 
        /// This is to be set each time the sales and marketing Place is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing Place was last edited.
        /// This is to be set each time the sales and marketing Place is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the name of the Place
        /// </summary>
        [Required(ErrorMessage = "Place name is required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Name should be minimum 4 characters and a maximum of 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }


        /// <summary>
        /// This is the number of Prospects (contacts) that can be associated with a Place
        /// </summary>
        public int Prospects { get; set; }


        /// <summary>
        /// This is the summary associated with the place.
        /// </summary>
        [StringLength(200)]
        [DataType(DataType.Text)]
        public string Summary { get; set; }


        /// <summary>
        /// This is the image for the place
        /// </summary>
        [Required(ErrorMessage = "Place image is required")]
        [DataType(DataType.Text)]
        public string FeaturedImageUri { get; set; }

        // <summary>
        /// Status of Place
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// The collection of contacts that have been associated with theis Contacts
        /// This is part of a many-to-many relationship
        /// </summary>
        public virtual List<SMContact> Contacts { get; set; } = new List<SMContact>();


        /// <summary>
        /// This is a collection of the Areas with which the place is associated
        /// This is part of a many-to-many relationship
        /// </summary>
        public virtual List<Area> Areas { get; set; } = new List<Area>();


        /// <summary>
        /// This is a collection of the Visits associated with the place
        /// </summary>
        public virtual List<Visit> Visits { get; set; } = new List<Visit>();


        /// <summary>
        /// This is a collection of the QbicleTasks that have been created to schedule 
        /// Visits or other activities for this place.
        /// </summary>
        public virtual List<QbicleTask> Tasks { get; set; } = new List<QbicleTask>();


        /// <summary>
        /// This is a collection of the PLaceActivities that have been created to record 
        /// activities for this place.
        /// </summary>
        public virtual List<PlaceActivity> PlaceActivities { get; set; } = new List<PlaceActivity>();


        /// <summary>
        /// A discussion where this place can be discussed
        /// </summary>
        public virtual QbicleDiscussion Discussion { get; set; }

    }
}
