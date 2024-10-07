using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;
namespace Qbicles.Models.Trader.Payments
{
    [Table("trad_creditnote")]
    public class CreditNote: DataModelBase
    {
        /// <summary>
        /// The user from Qbicles who created the credit note
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this credit note was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the credit note, this is to be set each time the credit note is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this credit note was last edited.
        /// This is to be set each time the credit note is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the Contact with which this credit note is associated
        /// </summary>
        [Required]
        public virtual TraderContact Contact { get; set; }


        /// <summary>
        /// The notes of the credit note.
        /// </summary>
        public string Notes { get; set; }


        /// <summary>
        /// This is the value of the Credit Note
        /// The Value may be positive or negative
        /// </summary>
        public decimal Value { get; set; }


        /// <summary>
        /// The Credit note MAY be associated with a Invoice or Bill
        /// </summary>
        public virtual Invoice Invoice { get; set; }


        /// <summary>
        /// The credit note MAY be associated with a sale
        /// </summary>
        public virtual TraderSale Sale { get; set; }


        /// <summary>
        /// The credit note MAY be associated with a purchase
        /// </summary>
        public virtual TraderPurchase  Purchase { get; set; }



        /// <summary>
        /// This indicates the reeason for the note based on the enum
        /// </summary>
        public CreditNoteReason Reason { get; set; }


        /// <summary>
        /// This is the date and time at which the credit note  was finalised.
        /// </summary>
        public DateTime FinalisedDate { get; set; }


        /// <summary>
        /// The status of the Credit Note
        /// </summary>
        public CreditNoteStatus Status { get; set; }



        /// <summary>
        /// The approval process for the Credit Note
        /// </summary>
        public virtual ApprovalReq ApprovalProcess { get; set; }



        /// <summary>
        /// This is the reference created for the Credit Note
        /// IMPORTANT: There are different references for the Credit Notes and Debit Notes
        /// Credit Notes and Debit notes are distinguished by CreditNOte reason as follows:
        /// Reasons for Credit Note
        /// • Credit Note(Related to an Invoice)
        /// • Discount
        /// • Price Decrease
        /// • Voucher
        /// Reasons for Debit Note
        /// • Debit Note(Relates to a Bill)
        /// • Price Increase
        /// </summary>
        public virtual TraderReference Reference { get; set; }

        /// <summary>
        /// This is the Workgroup associated with the CreditNote.
        /// </summary>
        [Required]
        public virtual WorkGroup WorkGroup { get; set; }

    }


    public enum CreditNoteStatus
    {
        [Description("Draft")]
        Draft = 1, 
        [Description("Pending Review")]
        PendingReview = 2,
        [Description("Reviewed")]
        Reviewed = 3,
        [Description("Approved")]
        Approved = 4,
        [Description("Denied")]
        Denied = 5,
        [Description("Discarded")]
        Discarded = 6

    }


    /// <summary>
    ///  Debit Note and Credit Note are used while the return of goods is made between two businesses. 
    ///  Debit Note is issued by the purchaser, at the time of returning the goods to the vendor, 
    ///  and the vendor issues a Credit Note to inform that he/she has received the returned goods.
    /// </summary>
    public enum CreditNoteReason
    {
        CreditNote = 1,
        DebitNote = 2,
        Discount = 3,
        Voucher = 4,
        PriceIncrease = 5,
        PriceDecrease = 6
    }
}
