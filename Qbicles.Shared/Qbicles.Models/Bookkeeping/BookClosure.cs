using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    /// <summary>
    /// This class is used to record a point-in-time
    /// when no more transactions older than the point-in-time would
    /// be added to the associated BKAccount 
    /// </summary>
    [Table("bk_BookClosure")]
    public class BookClosure
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the user who created the Book Closure
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        /// <summary>
        /// This is the UTC time when BookClosure was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This the date and time, specified by the user, before which it
        /// will not be possible to add Transactions to the BKAccount
        /// </summary>
        [Required]
        public DateTime ClosureDate { get; set; }


        /// <summary>
        /// This is the Domain with which the books closure is associated
        /// </summary>
        public virtual QbicleDomain Domain { get; set; }


    }
}
 