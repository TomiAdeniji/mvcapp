using IdentityModel.Client;
using PayStackDotNetSDK.Methods.Plans;
using PayStackDotNetSDK.Methods.Refunds;
using PayStackDotNetSDK.Methods.Subscription;
using PayStackDotNetSDK.Methods.Transactions;
using PayStackDotNetSDK.Models;
using PayStackDotNetSDK.Models.Plans;
using PayStackDotNetSDK.Models.Refunds;
using PayStackDotNetSDK.Models.Subscription;
using PayStackDotNetSDK.Models.Transactions;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Model.PaystackWebhook;
using Qbicles.Models.Paystack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.BusinessRules.PayStack
{
	public class PayStackRules
	{
		private ApplicationDbContext dbContext;
		private string PAYSTACK_SECRET_KEY = ConfigurationManager.AppSettings["PayStackSecretKey"];
		private string PAYSTACK_PUBLIC_KEY = ConfigurationManager.AppSettings["PayStackPublicKey"];

		public PayStackRules()
		{
			dbContext = new ApplicationDbContext();
		}

		public PayStackRules(ApplicationDbContext context)
		{
			dbContext = context;
		}

		public async Task<PlanResponseModel> CreatePaystackPlan(string name, int amount, string interval)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, name, amount, interval);

				// Amount of paystack plan will be in cent, so it need to be multiple with 100 before sending to API
				amount = amount * 100;

				var paystackPlan = new PaystackPlan(PAYSTACK_SECRET_KEY);
				var planCreationResponse = await paystackPlan.CreatePlan(name, amount, interval);

				return planCreationResponse;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, amount, interval);
				return null;
			}
		}

		public async Task<PaymentInitalizationResponseModel> InitAuthVerificationTransaction(string planCode, decimal transactionAmount,
			string userEmail, string baseUrl, string currentUserId, string currency = "NGN")
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, planCode, transactionAmount, userEmail, currency);

				string[] channels = { "card" };

				var paystackTransaction = new PaystackTransaction(PAYSTACK_SECRET_KEY);

				// Amount of paystack plan will be in cent, so it need to be multiple with 100 before sending to API
				transactionAmount = transactionAmount * 100;

				var metadataObj = new
				{
					cancel_action = baseUrl + "/Paystack/DomainCreationTransactionCancelledCallback?planCode=" + planCode + "&userId=" + currentUserId
				};

				//var transactionInitRequest = new TransactionRequestModel()
				//{
				//    amount = (int)transactionAmount,
				//    email = userEmail,
				//    callback_url = baseUrl + "/Paystack/DomainCreationTransactionProcessCallback?planCode=" + planCode + "&userId=" + currentUserId,
				//    channels = channels
				//};
				//var transactionInitResponse = await paystackTransaction.InitializeTransaction(transactionInitRequest);

				// Initialize transaction
				var payStackURL = $"https://api.paystack.co/transaction/initialize";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

				//return await (await HttpConnection.CreateClient(secretKey).PostAsync(urlLink, new StringContent(content, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();
				//var requestBodyParams = new Dictionary<string, string>
				//{
				//    {"email", userEmail },
				//    {"amount", ((int)transactionAmount).ToString() },
				//    {"callback_url",  baseUrl + "/Paystack/DomainCreationTransactionProcessCallback?planCode=" + planCode + "&userId=" + currentUserId},
				//    {"metadata",  metadataObj.ToJson()},
				//    {"channels", channels.ToJson() }
				//};
				//var requestBody = new FormUrlEncodedContent(requestBodyParams);

				var requestBodyParams = new
				{
					email = userEmail,
					amount = (int)transactionAmount,
					callback_url = baseUrl + "/Paystack/DomainCreationTransactionProcessCallback?planCode=" + planCode + "&userId=" + currentUserId,
					metadata = metadataObj.ToJson(),
					channels = channels
				};

				var response = await httpClient.PostAsync(payStackURL, new StringContent(requestBodyParams.ToJson(), Encoding.UTF8, "application/json"));

				var responseJson = await response.Content.ReadAsStringAsync();
				var returnResponse = JsonHelper.ParseAs<PaymentInitalizationResponseModel>(responseJson);
				return returnResponse;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, planCode, transactionAmount, userEmail, currency);
				return null;
			}
		}

		public async Task<PaymentInitalizationResponseModel> InitSplitTransaction(decimal amount, string subAccountCode, string userEmail,
			string callBackUrl = "", string currency = "NGN")
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, amount, subAccountCode, userEmail, currency, callBackUrl);

				string[] channels = { "card" };

				var paystackTransaction = new PaystackTransaction(PAYSTACK_SECRET_KEY);

				// Amount of paystack plan will be in cent, so it need to be multiple with 100 before sending to API
				var transactionAmount = amount * 100;

				// Initialize transaction
				var payStackURL = $"https://api.paystack.co/transaction/initialize";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

				var requestBodyParams = new
				{
					email = userEmail,
					amount = (int)transactionAmount,
					callback_url = callBackUrl,
					channels = channels,
					subaccount = subAccountCode,
					currency = currency
				};

				var response = await httpClient.PostAsync(payStackURL, new StringContent(requestBodyParams.ToJson(), Encoding.UTF8, "application/json"));

				var responseJson = await response.Content.ReadAsStringAsync();
				var returnResponse = JsonHelper.ParseAs<PaymentInitalizationResponseModel>(responseJson);
				return returnResponse;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, amount, subAccountCode, userEmail, currency, callBackUrl);
				return null;
			}
		}

		public async Task<PayStackVerifyTransactionResponseModel> VerifyPaystackTransaction(string transactionReference)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, transactionReference);

				var paystackSubscription = new PaystackSubscription(PAYSTACK_SECRET_KEY);
				// Fetch data of the subscription from paystack
				var payStackURL = $"https://api.paystack.co/transaction/verify/" + transactionReference;
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				var response = await httpClient.GetAsync(payStackURL);

				var responseJson = await response.Content.ReadAsStringAsync();
				var returnResponse = JsonHelper.ParseAs<PayStackVerifyTransactionResponseModel>(responseJson);
				return returnResponse;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, transactionReference);
				return null;
			}
		}

		public async Task<PayStackSubscriptionModel> GetPaystackSubscription(string subCode = "")
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);

				var paystackSubscription = new PaystackSubscription(PAYSTACK_SECRET_KEY);
				// Fetch data of the subscription from paystack
				var payStackURL = $"https://api.paystack.co/subscription/" + subCode;
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				var response = await httpClient.GetAsync(payStackURL);

				var responseJson = await response.Content.ReadAsStringAsync();
				var subscriptionListResponse = JsonHelper.ParseAs<FetchPayStackSubscriptionResponse>(responseJson);

				// Update next paid date for the subscription
				var currentSub = dbContext.DomainSubscriptions.FirstOrDefault(p => p.PayStackSubscriptionCode == subCode);
				if (currentSub != null)
				{
					currentSub.NexPaymentDate = subscriptionListResponse?.data?.next_payment_date ?? DateTime.UtcNow;
					dbContext.Entry(currentSub).State = System.Data.Entity.EntityState.Modified;
					dbContext.SaveChanges();
				}

				return subscriptionListResponse.data;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
				return null;
			}
		}

		public async Task<SubscriptionModel> CreateSubscription(string customerEmailOrCode, string planCode, string authorization, DateTime? startDate = null)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, customerEmailOrCode, planCode, authorization, startDate);

				if (startDate == null)
				{
					startDate = DateTime.UtcNow;
				}
				// Convert time to ISO 8601 format
				var startDateString = ((DateTime)startDate).ToString("o");

				var subObj = new PaystackSubscription(PAYSTACK_SECRET_KEY);
				var createSubscriptionResponse = await subObj.CreateSubscription(customerEmailOrCode, planCode, authorization, startDateString);
				return createSubscriptionResponse;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, customerEmailOrCode, planCode, authorization, startDate);
				return null;
			}
		}

		public async Task<CreateRefundResponseModel> InitRefund(string transactionReference)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, transactionReference);

				var payStackRefundObj = new PaystackRefund(PAYSTACK_SECRET_KEY);
				var refundRequest = new CreateRefundRequestModel()
				{
					currency = "NGN",
					transaction = transactionReference
				};
				var creationResponse = await payStackRefundObj.CreateRefund(refundRequest);
				return creationResponse;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, transactionReference);
				return null;
			}
		}

		public async Task<PlanResponseModel> UpdatePlan(string planCode, decimal amount, string name, string interval = "monthly")
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, planCode, amount, name, interval);

				var payStackPlanObj = new PaystackPlan(PAYSTACK_SECRET_KEY);

				// Money amount needed to be multiple by 100 before send to Paystack
				var newAmount = amount * 100;

				var planRequest = new PlanRequestModel()
				{
					amount = (int)newAmount,
					name = name,
					interval = interval
				};
				var response = await payStackPlanObj.UpdatePlan(planCode, planRequest);
				return response;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, planCode, amount, name, interval);
				return null;
			}
		}

		public async Task<SubscriptionModel> DisableSubscription(string subscriptionCode, string emailToken)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, subscriptionCode, emailToken);

				var payStackSubscriptionObj = new PaystackSubscription(PAYSTACK_SECRET_KEY);
				var response = await payStackSubscriptionObj.DisableSubscription(subscriptionCode, emailToken);

				return response;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, subscriptionCode, emailToken);
				return null;
			}
		}

		public async Task<PaymentInitalizationResponseModel> InitGettingAuthorizationTransaction(decimal transactionAmount, string callbackUrl,
			string cancelUrl, string userEmail, string[] channels, string currency = "NGN")
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, transactionAmount, callbackUrl, cancelUrl, userEmail, channels, currency);

				var paystackTransaction = new PaystackTransaction(PAYSTACK_SECRET_KEY);

				// Amount of paystack plan will be in cent, so it need to be multiple with 100 before sending to API
				transactionAmount = transactionAmount * 100;

				var metadataObj = new
				{
					cancel_action = cancelUrl
				};

				// Initialize transaction
				var payStackURL = $"https://api.paystack.co/transaction/initialize";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

				var requestBodyParams = new
				{
					email = userEmail,
					amount = (int)transactionAmount,
					callback_url = callbackUrl,
					metadata = metadataObj.ToJson(),
					channels = channels
				};

				var response = await httpClient.PostAsync(payStackURL, new StringContent(requestBodyParams.ToJson(), Encoding.UTF8, "application/json"));

				var responseJson = await response.Content.ReadAsStringAsync();
				var returnResponse = JsonHelper.ParseAs<PaymentInitalizationResponseModel>(responseJson);
				return returnResponse;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, transactionAmount, callbackUrl, cancelUrl, userEmail, channels, currency);
				return null;
			}
		}

		public async Task<List<PaystackBankModel>> GetListPaystackBanks(string country = "nigeria")
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, country);

				// Fetch data of the subscription from paystack
				var payStackURL = $"https://api.paystack.co/bank?country={country}&currency=NGN&use_cursor=false";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				var response = await httpClient.GetAsync(payStackURL);

				var responseJson = await response.Content.ReadAsStringAsync();
				var subscriptionListResponse = JsonHelper.ParseAs<FetchingPaystackBankResponse>(responseJson);

				return subscriptionListResponse.data;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, country);
				return new List<PaystackBankModel>();
			}
		}

		public async Task<ReturnJsonModel> CreateSubAccount(string businessName, string bankCode, string accountNumber, decimal percentageCharge)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, businessName, bankCode, accountNumber, percentageCharge);

				var payStackURL = $"https://api.paystack.co/subaccount";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

				var requestBodyParams = new
				{
					business_name = businessName,
					settlement_bank = bankCode,
					account_number = accountNumber,
					percentage_charge = percentageCharge * 100
				};

				var response = await httpClient.PostAsync(payStackURL, new StringContent(requestBodyParams.ToJson(), Encoding.UTF8, "application/json"));

				var responseJson = await response.Content.ReadAsStringAsync();
				var returnResponse = JsonHelper.ParseAs<CreateSubAccountResponseModel>(responseJson);

				if (returnResponse == null || !returnResponse.status)
				{
					return new ReturnJsonModel()
					{
						result = false,
						msg = "Create SubAccount fails"
					};
				}

				return new ReturnJsonModel()
				{
					result = true,
					Object = returnResponse
				};
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, businessName, bankCode, accountNumber, percentageCharge);
				return new ReturnJsonModel()
				{
					result = false,
					msg = ResourcesManager._L("ERROR_MSG_5")
				};
			}
		}

		public async Task<SubAccountFetchingModel> GetSubAccountInformation(string accountCode)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, accountCode);

				// Fetch data of the subscription from paystack
				var payStackURL = $"https://api.paystack.co/subaccount/{accountCode}";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
				var response = await httpClient.GetAsync(payStackURL);

				var responseJson = await response.Content.ReadAsStringAsync();
				var subAccFetchingResponse = JsonHelper.ParseAs<FetchingSubAccountResponseModel>(responseJson);
				if (subAccFetchingResponse.status == false)
				{
					return null;
				}

				return subAccFetchingResponse.data;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountCode);
				return null;
			}
		}

		public async Task<ReturnJsonModel> UpdateSubAccountInformation(string accountCode,
			string businessName, string bankCode, string accountNumber, decimal percentageCharge)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, accountCode, businessName, bankCode, accountNumber, percentageCharge);
				//UpdateSubAccountResponseModel

				var payStackURL = $"https://api.paystack.co/subaccount/{accountCode}";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

				var requestBodyParams = new
				{
					business_name = businessName,
					settlement_bank = bankCode,
					account_number = accountNumber,
					percentage_charge = percentageCharge * 100
				};

				var response = await httpClient.PutAsync(payStackURL, new StringContent(requestBodyParams.ToJson(), Encoding.UTF8, "application/json"));

				var responseJson = await response.Content.ReadAsStringAsync();
				var returnResponse = JsonHelper.ParseAs<UpdateSubAccountResponseModel>(responseJson);

				if (returnResponse == null || !returnResponse.status)
				{
					return new ReturnJsonModel()
					{
						result = false,
						msg = "Update SubAccount fails"
					};
				}

				return new ReturnJsonModel()
				{
					result = true,
					Object = returnResponse.data
				};
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountCode, businessName, bankCode, accountNumber, percentageCharge);
				return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
			}
		}

		public async Task<ReturnJsonModel> CreatePaystackCustomerAccount(string email, string firstName, string lastName, string phone = "", object metaDataObj = null)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, email, firstName, lastName, phone, metaDataObj);

				var payStackURL = $"https://api.paystack.co/customer";
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + PAYSTACK_SECRET_KEY);
				httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

				var requestBodyParams = new
				{
					email = email,
					first_name = firstName,
					last_name = lastName,
					phone = phone
				};

				var response = await httpClient.PostAsync(payStackURL, new StringContent(requestBodyParams.ToJson(), Encoding.UTF8, "application/json"));

				var responseJson = await response.Content.ReadAsStringAsync();
				var returnResponse = JsonHelper.ParseAs<CustomerAccountCreationResponseModel>(responseJson);
				if (returnResponse == null || !returnResponse.status)
				{
					return new ReturnJsonModel()
					{
						result = false,
						msg = "Create Paystack account fails"
					};
				}
				return new ReturnJsonModel()
				{
					result = true,
					Object = returnResponse
				};
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email, firstName, lastName, phone, metaDataObj);
				return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
			}
		}

		// Webhook area
		public bool SavePaystackPaymentFromWebhook(WebhookInvoiceUpdatedRequestModel data)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet)
					LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, data);

				var paymentData = data.data;
				var associatedSub = dbContext.DomainSubscriptions.FirstOrDefault(p => p.PayStackSubscriptionCode == paymentData.subscription.subscription_code);
				var paymentObj = new PaystackSubscriptionPayment()
				{
					EventName = paymentData.ToJson(),
					Amount = paymentData.amount,
					Description = paymentData.description,
					CreatedAt = paymentData.created_at,
					InvoiceCode = paymentData.invoice_code,
					PaidDate = paymentData.paid_at,
					PeriodStart = paymentData.period_start,
					PeriodEnd = paymentData.period_end,
					TransactionAmount = paymentData.transaction?.amount ?? 0,
					TransactionCurrency = paymentData.transaction?.currency ?? "",
					TransactionReference = paymentData.transaction?.reference ?? "",
					TransactionStatus = paymentData.transaction?.status ?? "",
					AssociatedSubscription = associatedSub
				};

				dbContext.PaystackSubscriptionPayments.Add(paymentObj);
				dbContext.Entry(paymentObj).State = System.Data.Entity.EntityState.Added;
				dbContext.SaveChanges();

				return true;
			}
			catch (Exception ex)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, data);
				return false;
			}
		}

		//END: Webhook area

		/// <summary>
		/// Implements simple InitializeTransaction with basic parameters
		/// </summary>
		public async Task<string> InitializeTransaction(UserSetting userInfo, int amount)
		{
			string url = "https://dev1-web.qbicles.com/Commerce/BusinessProfileTrading?tab=general-locations";

            // Amount of paystack plan will be in cent, so it need to be multiple with 100 before sending to API
            var transactionAmount = amount * 100;

            var requestModel = new TransactionRequestModel()
			{
				firstName = userInfo.DisplayName,
				lastName = userInfo.DisplayName,
				amount = transactionAmount,
				currency = PayStackDotNetSDK.Helpers.Constants.Currency.Naira,
				email = userInfo.Email,
				//transaction_charge = 4000,
				callback_url = url
			};

			var connectionInstance = new PaystackTransaction(PAYSTACK_SECRET_KEY);
			var response = await connectionInstance.InitializeTransaction(requestModel);

			if (response.status)
			{
				url = response.data.authorization_url;

				//Response.AddHeader("Access-Control-Allow-Origin", "*");
				//Response.AppendHeader("Access-Control-Allow-Origin", "*");
				//Response.Redirect(response.data.authorization_url); //Redirects your browser to the secure URL
			}

			return url;
		}

		public static int Generate()
		{
			Random rand = new Random((int)DateTime.Now.Ticks);
			return rand.Next(100000000, 999999999);
		}
	}
}