using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Model.PaystackWebhook
{
    public class WebhookInvoiceUpdatedRequestModel
    {
        public string @event { get; set; }
        public InvoiceUpdatedDataModel data { get; set; }

    }

    public class InvoiceUpdatedDataModel
    {
        public string domain { get; set; }
        public string invoice_code { get; set; }
        public decimal amount { get; set; }
        public DateTime period_start { get; set; }
        public DateTime period_end { get; set; }
        public string status { get; set; }
        public bool paid { get; set; }
        public DateTime paid_at { get; set; }
        public string description { get; set; }
        public WebhookAuthorizationModel authorization { get; set; }
        public WebhookSubscriptionModel subscription { get; set; }
        public WebhookCustomerModel customer { get; set; }
        public WebhookTransactionModel transaction { get; set; }
        public DateTime created_at { get; set; }
    }

    public class WebhookAuthorizationModel
    {
        public string authorization_code { get; set; }
        public string bin { get; set; }
        public string last4 { get; set; }
        public string exp_month { get; set; }
        public string exp_year { get; set; }
        public string card_type { get; set; }
        public string bank { get; set; }
        public string country_code { get; set; }
        public string brand { get; set; }
        public string account_name { get; set; }
    }

    public class WebhookSubscriptionModel
    {
        public string status { get; set; }
        public string subscription_code { get; set; }
        public decimal amount { get; set; }
        public string cron_expression { get; set; }
        public DateTime? next_payment_date { get; set; }
        public string open_invoice { get; set; }
    }

    public class WebhookCustomerModel
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string customer_code { get; set; }
        public string phone { get; set; }
        public string risk_action { get; set; }
    }

    public class WebhookTransactionModel
    {
        public string reference { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
    }
}
