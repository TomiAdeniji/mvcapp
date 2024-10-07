using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Form;

namespace Qbicles.Models
{
    [Table("qb_ApprovalRequestDefinition")]
    public class ApprovalRequestDefinition
    {
        public int Id { get; set; }

        public virtual ApprovalGroup Group { get; set; }

        public RequestTypeEnum Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual List<ApplicationUser> Initiators { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Reviewers { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Approvers { get; set; } = new List<ApplicationUser>();
        public virtual List<ApprovalReq> ApprovalReqs { get; set; } = new List<ApprovalReq>();

        public virtual List<FormDefinition> Forms { get; set; } = new List<FormDefinition>();

        public virtual List<ApprovalDocument> Documents { get; set; } = new List<ApprovalDocument>();

        //public virtual List<BKAppSettings> BKAppSettings { get; set; } = new List<BKAppSettings>();

        public string ApprovalImage { get; set; }

        [Column(TypeName = "bit")]
        [DefaultValue(false)]
        public bool IsViewOnly { get; set; } = false;

        public enum RequestTypeEnum
        {
            [Description("General")]
            General = 1,
            [Description("Procurement")]
            Procurement = 2,
            [Description("Payment")]
            Payment = 3,
            [Description("Journal Entry")]
            Bookkeeping = 4,
            [Description("Trader Process")]
            Trader = 5,

        };
    }


}