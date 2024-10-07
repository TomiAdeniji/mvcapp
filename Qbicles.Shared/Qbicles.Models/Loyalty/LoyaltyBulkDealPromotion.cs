using Qbicles.Models.Base;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_bulkdeal_Promotion")]
    public class LoyaltyBulkDealPromotion : DataModelBase
    {
        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public ApplicationUser CreatedBy { get; set; }

         /// <summary>
        /// This is the name of the promotion
        /// </summary>
        [Required]
        public string Name { get; set; } // Length??

        /// <summary>
        /// THis is the GUID to associate with the image file uploaded to S3
        /// </summary>
        [Required]
        public string FeaturedImageUri { get; set; }


        /// <summary>
        /// The description of the promotion
        /// </summary>
        [Required]
        public string Description { get; set; } // Length??


        /// <summary>
        /// This is the date and time at which the Promotion will be displayed in the 
        /// Community MoniBac list.
        /// This shold be earlier or equal to the StartDate
        /// This should default to the StartDate unless explicity set
        /// </summary>
        [Required]
        public DateTime DisplayDate { get; set; }

        /// <summary>
        /// The date and time at which it becomes possible to claim the vouchers
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }


        /// <summary>
        /// <summary>
        /// The date and time at which it stops being possible to claim the vouchers
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// This is the information directly related to the voucher creation associated with this promotion
        /// </summary>
        public virtual BulkDealVoucherInfo BulkDealVoucherInfo { get; set; }

        /// <summary>
        /// This is a boolean that can be set by a user to halt or stop a promotion
        /// When/If this value is set to true No vouchers associated with the Promotion can be claimed OR redeemed 
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHalted { get; set; } = false;

        /// <summary>
        /// A description of why the Promotion was halted
        /// </summary>
        public string HaltReason { get; set; }

        /// <summary>
        /// The Opt-In vouchers for participating businesses
        /// </summary>
        public virtual List<BulkDealVoucher> OptInVouchers { get; set; } = new List<BulkDealVoucher>();

        /// <summary>
        /// This indicates that the promotion has been archived
        /// this occurs when the end date of the promotion is reached
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// This indicates that the bulk deal promotion can be draft
        /// this occurs when the end date of the promotion is reached
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsDraft { get; set; } = true;
        /// <summary>
        /// This is the date on which the Promotion was archived
        /// </summary>
        public DateTime ArchivedDate { get; set; }
    }
}
