using Qbicles.Models.Trader;
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
    /// This is the main class for contacts in Sales and marketing
    /// </summary>
    [Table("sm_Contact")]
    public class SMContact
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing contact in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// This is the Domain with which this Contact is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The user from Qbicles who created the sales and marketing contact
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing contact was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the sales and marketing contact, this is to be set each time the sales and marketing contact is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this sales and marketing contact was last edited.
        /// This is to be set each time the sales and marketing contact is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the name of the Contact
        /// </summary>
        [Required(ErrorMessage = "Contact name is required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Name should be minimum 4 characters and a maximum of 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// This is the phone number for the contach
        /// </summary>
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }


        /// <summary>
        /// This is the email for the contact.
        /// This is a critical piece of information as it will be used to make links between this contact and others in the system
        /// where the email addresses match
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        /// <summary>
        /// This is the birth day for the user and is to be entered if available.
        /// The time portion of the date is not important
        /// </summary>
        public DateTime BirthDay { get; set; }

        /// <summary>
        /// This is the Avatar Uri for the contact
        /// </summary>
        public string AvatarUri { get; set; }
        /// <summary>
        /// This is a TraderContacts - FROM THE SAME DOMAIN - to which this contact MAY be linked
        /// </summary>
        public virtual TraderContact TraderContact { get; set; }


        /// <summary>
        /// This is to indicate where the details for this customwer have come from
        /// </summary>
        [Required]
        public ContactSourceEnum Source { get; set; }

        /// <summary>
        /// This is to indicate whether contact is subscribed to receive email
        /// </summary>
        [Required]
        public bool IsSubscribed { get; set; } = true;

        /// <summary>
        /// The collection of Places with which the contact is associated
        /// This is part of a many-to-many relationship
        /// </summary>
        public virtual List<Place> Places { get; set; } = new List<Place>();

        /// <summary>
        /// This is the list of Segmenst with which the Contact is associated 
        /// It forms part of a many - to - many realtionship
        /// </summary>
        public virtual List<Segment> Segments { get; set; } = new List<Segment>();


        /// <summary>
        /// This is a list of the custom cruterial values associated with a particular contact
        /// </summary>
        public virtual List<CriteriaValue> CriteriaValues { get; set; } = new List<CriteriaValue>();

        public string SourceDescription { get; set; }

    }


    public enum ContactSourceEnum
    {
        Customer = 0,
        Trader = 1,
        EnquiryForm = 2,
        SalesCall = 3,
        Other = 4
    }
}
