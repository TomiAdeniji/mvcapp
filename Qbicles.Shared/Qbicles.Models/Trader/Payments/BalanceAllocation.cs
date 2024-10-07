using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Payments
{
    [Table("trad_balanceallocation")]
    public class BalanceAllocation
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the contact with which the allocation is associated
        /// </summary>
        [Required]
        public virtual TraderContact Contact { get; set; }


        /// <summary>
        /// This is the invoice to which the balance is allocated
        /// </summary>
        public virtual Invoice Invoice { get; set; }


        /// <summary>
        /// This is the value of the allocation
        /// </summary>
        [Required]
        public Decimal Value { get; set; }

        /// <summary>
        /// This is the user who created the allocation
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the allocation was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The user from Qbicles who last updated the allocation, this is to be set each time the allocation is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this allocation was last edited.
        /// This is to be set each time the credit note is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the value of the Contact's balance before the allocation was created
        /// </summary>
        [Required]
        public decimal ContactBalanceBefore { get; set; }


        /// <summary>
        /// A user can enter a description for an allocation
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// This is the date and time at which the allocation was made.
        /// </summary>
        public DateTime AllocatedDate { get; set; }

        /// <summary>
        /// This is the reference created for the Allocation
        /// </summary>
        public virtual TraderReference Reference { get; set; }

    }
}
