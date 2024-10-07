using System;
using Qbicles.Models.Bookkeeping;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;

namespace Qbicles.Models.Trader
{
    [Table("trad_contact")]
    public class TraderContact : DataModelBase
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(71, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 71 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        public string AvatarUri { get; set; }

        public string CompanyName { get; set; }

        public string JobTitle { get; set; }

        public virtual TraderAddress Address { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public virtual BKAccount CustomerAccount {get; set;}

        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public virtual ApplicationUser QbicleUser { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual TraderContactGroup ContactGroup { get; set; }

        public virtual List<TraderItemVendor> AssociatedItems { get; set; } = new List<TraderItemVendor>();

        [Column(TypeName = "bit")]
        public bool InUsed { get; set; }

        public virtual TraderContactStatusEnum Status { get; set; } = TraderContactStatusEnum.Draft;

        public virtual ApprovalReq ContactApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }
        public virtual TraderContactRef ContactRef { get; set; }

    }

    public enum TraderContactStatusEnum
    {
        Draft = 0,
        PendingReview = 1,
        PendingApproval = 2,
        ContactDenied = 3,
        ContactApproved = 4,
        ContactDiscarded = 5

    }
}
