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
    /// The Area class is effectivel used to group Places together.
    /// An ARe can have many places and a Place can be in many Areas
    /// </summary>
    [Table("sm_Area")]
    public class Area
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing Area in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is the Domain with which this Area is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The user from Qbicles who created the sales and marketing Area
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing Area was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the sales and marketing Area, 
        /// This is to be set each time the sales and marketing Area is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing Area was last edited.
        /// This is to be set each time the sales and marketing Area is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the name of the Area
        /// </summary>
        [Required(ErrorMessage = "Area name is required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Name should be minimum 4 characters and a maximum of 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }


        /// <summary>
        /// This is the image for the Area
        /// </summary>
        [Required(ErrorMessage = "Area image is required")]
        [DataType(DataType.Text)]
        public string FeaturedImageUri { get; set; }

        // <summary>
        /// Status of Area
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// This is a collection of the Places with which the Area is associated
        /// This is part of a many-to-many relationship
        /// </summary>
        public virtual List<Place> Places { get; set; } = new List<Place>();

        /// <summary>
        /// This is a list of the Segmenst with which this Area has been associated.
        /// It is part of a many-to-many relationship
        /// </summary>
        public virtual List<Segment> Segments { get; set; } = new List<Segment>();
    }
}
