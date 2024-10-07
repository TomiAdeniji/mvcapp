using System;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class BasePaystackResponseModel
    {
        public bool status { get; set; }
        public string message { get; set; }
    }

    public class FetchPayStackSubscriptionResponse : BasePaystackResponseModel
    {
        public virtual PayStackSubscriptionModel data { get; set; }
    }

    public class PayStackSubscriptionModel
    {
        public decimal id { get; set; }
        public string domain { get; set; }
        public string status { get; set; }
        public decimal? start { get; set; }
        public string subscription_code { get; set; }
        public string email_token { get; set; }
        public decimal amount { get; set; }
        public DateTime? next_payment_date { get; set; }
        public PayStackAuthorizationModel authorization { get; set; }
        public PayStackInvoiceModel most_recent_invoice { get; set; }
    }

    public class PayStackInvoiceModel
    {
        public decimal subscription { get; set; }
        public decimal integration { get; set; }
        public string domain { get; set; }
        public string invoice_code { get; set; }
        public decimal customer { get; set; }
        public decimal transaction { get; set; }
        public decimal amount { get; set; }
        public DateTime? period_start { get; set; }
        public DateTime? period_end { get; set; }
        public string status { get; set; }
        public bool paid { get; set; }
        public int retries { get; set; }
        public decimal authorization { get; set; }
        public DateTime? paid_at { get; set; }
    }

    public class PayStackAuthorizationModel
    {
        public string authorization_code { get; set; }
        public string bin { get; set; }
        public string last4 { get; set; }
        public string exp_month { get; set; }
        public string exp_year { get; set; }
        public string channel { get; set; }
        public string card_type { get; set; }
        public string bank { get; set; }
        public string country_code { get; set; }
        public string brand { get; set; }
        public string signature { get; set; }
    }

    public class PayStackVerifyTransactionResponseModel : BasePaystackResponseModel
    {
        public PayStackVerifyTransactionDataModel data { get; set; }

    }

    public class PayStackVerifyTransactionDataModel
    {
        public decimal id { get; set; }
        public string status { get; set; }
        public PayStackAuthorizationModel authorization { get; set; }
        public PayStackCustomerModel customer { get; set; }
    }

    public class PayStackCustomerModel
    {
        public decimal id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string customer_code { get; set; }
        public string phone { get; set; }
        public PayStackCustomerMetaDataModel metadata { get; set; }
        public string risk_action { get; set; }
        public string international_format_phone { get; set; }
    }

    public class PayStackCustomerMetaDataModel
    {
        public string calling_code { get; set; }
    }

    public class FetchingPaystackBankResponse : BasePaystackResponseModel
    {
        public List<PaystackBankModel> data { get; set; }
    }

    public class PaystackBankModel
    {
        public string name { get; set; }
        public string slug { get; set; }
        public string code { get; set; }
        public bool active { get; set; }
        public bool is_deleted { get; set; }
        public string country { get; set; }
        public string currency { get; set; }
        public string type { get; set; }
        public int id { get; set; }
    }


    public class CreateSubAccountResponseModel : BasePaystackResponseModel
    {
        public SubAccountCreatedModel data { get; set; }
    }

    public class SubAccountCreatedModel
    {
        public decimal integration { get; set; }
        public string domain { get; set; }
        public string subaccount_code { get; set; }
        public string business_name { get; set; }
        public string description { get; set; }
        public string primary_contact_name { get; set; }
        public string primary_contact_email { get; set; }
        public string primary_contact_phone { get; set; }
        public decimal percentage_charge { get; set; }
        public bool is_verified { get; set; }
        public string settlement_bank { get; set; }
        public string account_number { get; set; }
        public string settlement_schedule { get; set; }
        public bool active { get; set; }
        public bool migrate { get; set; }
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }


    public class FetchingSubAccountResponseModel : BasePaystackResponseModel
    {
        public SubAccountFetchingModel data { get; set; }
    }

    public class SubAccountFetchingModel
    {
        public decimal integration { get; set; }
        public string domain { get; set; }
        public string subaccount_code { get; set; }
        public string business_name { get; set; }
        public string description { get; set; }
        public string primary_contact_name { get; set; }
        public string primary_contact_email { get; set; }
        public string primary_contact_phone { get; set; }
        public decimal percentage_charge { get; set; }
        public bool is_verified { get; set; }
        public string settlement_bank { get; set; }
        public string account_number { get; set; }
        public string settlement_schedule { get; set; }
        public bool active { get; set; }
        public bool migrate { get; set; }
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class UpdateSubAccountResponseModel : BasePaystackResponseModel
    {
        public SubAccountFetchingModel data { get; set; }
    }

    public class SubAccountUpdatingModel
    {
        public decimal integration { get; set; }
        public string domain { get; set; }
        public string subaccount_code { get; set; }
        public string business_name { get; set; }
        public string description { get; set; }
        public string primary_contact_name { get; set; }
        public string primary_contact_email { get; set; }
        public string primary_contact_phone { get; set; }
        public decimal percentage_charge { get; set; }
        public bool is_verified { get; set; }
        public string settlement_bank { get; set; }
        public string account_number { get; set; }
        public string settlement_schedule { get; set; }
        public bool active { get; set; }
        public bool migrate { get; set; }
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }


    // Paystack Customer account creation response model
    public class CustomerAccountCreationResponseModel : BasePaystackResponseModel
    {
        public CustomerAccountCreationDataModel data { get; set; }
    }

    public class CustomerAccountCreationDataModel
    {
        public string email { get; set; }
        public decimal integration { get; set; }
        public string domain { get; set; }
        public string customer_code { get; set; }
        public decimal id { get; set; }
        public bool identified { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}

