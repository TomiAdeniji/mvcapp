using Qbicles.Models.Bookkeeping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_cashaccounttransactionlog")]
    public class CashAccountTransactionLog
    {

        public int Id { get; set; }

        public virtual CashAccountTransaction AssociatedTransaction { get; set; }

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
}
