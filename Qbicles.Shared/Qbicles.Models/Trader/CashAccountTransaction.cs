using Qbicles.Models.Bookkeeping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;
namespace Qbicles.Models.Trader
{
    [Table("trad_cashaccounttransaction")]
    public class CashAccountTransaction : DataModelBase
    {
        public CashAccountTransactionTypeEnum Type { get; set; }

        public decimal Amount { get; set; }

        public decimal Charges { get; set; }

        public virtual TraderCashAccount DestinationAccount { get; set; }

        public virtual TraderCashAccount OriginatingAccount { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Description { get; set; }

        public virtual BKTransaction AssociatedBKTransaction { get; set; }

        public virtual TraderSale AssociatedSale { get; set; }

        public virtual TraderPurchase AssociatedPurchase { get; set; }

        public virtual Invoice AssociatedInvoice { get; set; }

        public virtual List<QbicleMedia> AssociatedFiles { get; set; } = new List<QbicleMedia>();

        public virtual TraderContact Contact { get; set; }

        public virtual TraderPaymentStatusEnum Status { get; set; } = TraderPaymentStatusEnum.Draft;

        public virtual ApprovalReq PaymentApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual PaymentMethod PaymentMethod { get; set; }

        public string Reference { get; set; }
    }

    public class PaymentReferenceConst
    {
        public const string PosCashPaymentReferenceString = "POS payment";
        public const string B2CCashPaymentReferenceString = "Payment for B2C Order";
        public const string B2BCashPaymentReferenceString = "Payment for B2B Order";
    }

    public enum CashAccountTransactionTypeEnum
    {
        /// <summary>
        /// Sale
        /// </summary>
        PaymentIn = 1,
        /// <summary>
        /// Purchase
        /// </summary>
        PaymentOut = 2,
        /// <summary>
        /// Transfer
        /// </summary>
        Transfer = 3
    }


    public enum TraderPaymentStatusEnum
    {
        Draft = 0,
        [Description("Awaiting Review")]
        PendingReview = 1,
        [Description("Awaiting Approval")]
        PendingApproval = 2,
        [Description("Denied")]
        PaymentDenied = 3,
        [Description("Approved")]
        PaymentApproved = 4,
        [Description("Discarded")]
        PaymentDiscarded = 5

    }
}
