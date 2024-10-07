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
    /// A Segment defines a group of contacts.
    /// A filter is created that can be executed against the collection of Contacts
    /// in a Domain based on the properties and Custom Criteria associated with 
    /// a contact to create the collection.
    /// </summary>
    [Table("sm_segment")]
    public class Segment
    {
        
        public int Id { get; set; }

        /// <summary>
        /// The user from Qbicles who created the segment
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this segment was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the segment, this is to be set each time the segment is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this segment was last edited.
        /// This is to be set each time the segment is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// The Domain with which the Segmentn is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// This is the name of the segment and it must be unique within a Domain
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This is the summary of the segment 
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///  This is an indication of how the segment will be created
        /// </summary>
        public SegmentType Type { get; set; }

        /// <summary>
        /// This is the image that is associated with the Segment and shown in the UI for the Segment.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        public string FeaturedImageUri { get; set; }

        // <summary>
        /// Status of segment
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// This is a list of the contacts associated with the segment
        /// /// It forms part of a many - to - many realtionship
        /// </summary>
        public virtual List<SMContact> Contacts { get; set; } = new List<SMContact>();


        /// <summary>
        /// This is the collection of clauses associated with the Segment
        /// </summary>
        public virtual List<SegmentQueryClause> Clauses { get; set; } = new List<SegmentQueryClause>();

        /// <summary>
        /// This is a list of the Area associated with the Segment.
        /// Îf there are Areas in the list then the Criteria for finding Contacts
        /// must take into account the Areas that contain the Places associated with a Contact.
        /// It is part of a many-to-many relationship
        /// </summary>
        public virtual List<Area> Areas { get; set; } = new List<Area>();

        /// <summary>
        /// This is a list of the CampaignEmail associated with the segment
        /// /// It forms part of a many - to - many realtionship
        /// </summary>
        public virtual List<CampaignEmail> CampaignEmails { get; set; } = new List<CampaignEmail>();

        /// <summary>
        /// This is a list of the EmailCampaign associated with the segment
        /// /// It forms part of a many - to - many realtionship
        /// </summary>
        public virtual List<EmailCampaign> EmailCampaigns { get; set; } = new List<EmailCampaign>();
        public virtual List<ValueProposition> ValuePropositions { get; set; } = new List<ValueProposition>();

    }


    public enum SegmentType
    {
        Behavioural = 1,
        Demographic = 2,
        Geographic = 3,
        Psychographic = 4
    }
}
