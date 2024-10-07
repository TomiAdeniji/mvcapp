using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Paystack
{
    [Table("ps_subscriptionpayment")]
    public class PaystackSubscriptionPayment : DataModelBase
    {
        public string EventName { get; set; }
        public string InvoiceCode { get; set; }
        public decimal Amount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime PaidDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TransactionReference { get; set; }
        public string TransactionStatus { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionCurrency { get; set; }
        public Subscription AssociatedSubscription { get; set; }
    }
}
