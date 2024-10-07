using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2C_C2C
{

    /// <summary>
    /// This class is used to log the state of the relationship with customers.
    /// It is associated with the Qbicles that are customer related
    /// </summary>
    [Table("qb_CustomerRelationshipLog")]
    public class CustomerRelationshipLog
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the user id of the Application User who was responsible for the creation of the log.
        /// It is a GUID
        /// </summary>
        public string UserId { get; set; }


        /// <summary>
        /// This is the date and time at which the log was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is the status of the B2C or C2C Qbicle at the time the log was created
        /// </summary>
        [Required]
        public CommsStatus Status { get; set; }


        /// <summary>
        /// This is the ID of the QBicle with which the log is associated
        /// </summary>
        [Required]
        public int QbicleId { get; set; }



        /// The following two properties are only set if the the Status is Blocked
        /// <summary>
        /// This records the Id of the ApplicationUser if the relationship is blocked by a Customer
        /// It is a GUID
        /// </summary>
        public string BlockedByUserId { get; set; }

        /// <summary>
        /// This records the Id of the Domain if the relationship is blocked by a Business
        /// </summary>
        public int? BlockedByDomainId { get; set; }

    }
}
