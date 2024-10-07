using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Qbicles.BusinessRules.Helper;
using Qbicles.Web.App_Start;
using Qbicles.BusinessRules.BusinessRules.PayStack;
using static Qbicles.Models.Notification;

namespace Qbicles.Web.Controllers.Paystack
{
    //[AuthorizeIPAddress]
    public class PaystackController : BaseController
    {
        //public ApplicationDbContext dbContext = new ApplicationDbContext();

        // GET: Paystack
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> DomainCreationTransactionProcessCallback(string planCode, string userId)
        {
            var currentUser = dbContext.QbicleUser.Find(userId);
            var transactionRef = new DomainRules(dbContext).GetTransactionReferenceByPlanCode(planCode, currentUser.Email);
            var verifyResult = await new DomainRules(dbContext).VerifyDomainCreationTransaction(transactionRef);
            var isTransactionSuccess = verifyResult.status && verifyResult.data.status == "success";
            var domainId = 0;
            var domainRules = new DomainRules(dbContext);
            var hasError = true;
            var errorMessage = "";

            // Get the authorization information from Transaction Verification response
            if (isTransactionSuccess)
            {
                // Create Customer
                // As a customer can have multiple cards info, so need to create a new Customer anytime a domain is created
                var currentPaystackCustomer = new Customer()
                {
                    PaystackCustomerId = verifyResult.data.customer.id,
                    PaystackEmail = verifyResult.data.customer.email,
                    PaystackFirstName = verifyResult.data.customer.first_name,
                    PaystackLastName = verifyResult.data.customer.last_name,
                    User = currentUser,
                    CustomerCode = verifyResult.data.customer.customer_code,
                    Subscriptions = new List<Subscription>()
                };
                dbContext.PaystackCustomers.Add(currentPaystackCustomer);
                dbContext.Entry(currentPaystackCustomer).State = System.Data.Entity.EntityState.Added;
                dbContext.SaveChanges();

                // 1. Cash back - Refund
                var refundCreationResponse = await domainRules.CreatePayStackRefund(transactionRef);
                if (refundCreationResponse.result == true)
                {
                    // 2. Create a Subscription
                    var subscriptionCreationResponse = await domainRules
                        .CreatePayStackSubscription(verifyResult.data.authorization, verifyResult.data.customer, planCode);
                    if (subscriptionCreationResponse.result == true)
                    {
                        var newSub = subscriptionCreationResponse.Object2 as Subscription;
                        // Save subscription to the Customer
                        newSub = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Id == newSub.Id);
                        if (newSub != null)
                        {
                            currentPaystackCustomer.Subscriptions.Add(newSub);
                            dbContext.Entry(currentPaystackCustomer).State = System.Data.Entity.EntityState.Modified;
                            dbContext.SaveChanges();
                        }

                        // 3. Process the Domain creation
                        var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.DomainPlan.PayStackPlanCode == planCode);
                        var domainCreationProcessResult =
                                    await new DomainRules(dbContext)
                                    .ProcessDomainRequest(domainRequest.Id, DomainRequestStatus.Approved, userId, Server.MapPath("~"));
                        if (domainCreationProcessResult.result == true)
                        {
                            domainId = ((QbicleDomain)domainCreationProcessResult.Object).Id;
                            hasError = false;

                            var domain = dbContext.Domains.Find(domainId);

                            if (domainRequest.DomainPlan.Level.Level > BusinessDomainLevelEnum.Free)
                            {
                                var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domain.Id && p.IsArchived == false);
                                // Scheduling the trial expiring notification
                                // Create Notification
                                var trialTimeEndingNotification = new ActivityNotification
                                {
                                    OriginatingConnectionId = "",
                                    DomainId = domain.Id,
                                    QbicleId = 0,
                                    AppendToPageName = ApplicationPageName.Domain,
                                    EventNotify = NotificationEventEnum.DomainSubTrialEnd,
                                    CreatedByName = HelperClass.GetFullName(currentUser),
                                    ObjectById = currentDomainPlan.Id.ToString(),
                                    ReminderMinutes = 0,
                                    CreatedById = domain.CreatedBy.Id,
                                    Id = newSub.Id
                                };
                                new NotificationRules(dbContext).NotifyDomainAdminOnTrialTimeEnd(trialTimeEndingNotification);

                                // Scheduling the payment date notification
                                // Create Notification
                                var paymentDateReminderNotification = new ActivityNotification
                                {
                                    OriginatingConnectionId = "",
                                    DomainId = domain.Id,
                                    QbicleId = 0,
                                    AppendToPageName = ApplicationPageName.Domain,
                                    EventNotify = NotificationEventEnum.DomainSubNextPaymentDate,
                                    CreatedByName = HelperClass.GetFullName(currentUser),
                                    ObjectById = currentDomainPlan.Id.ToString(),
                                    ReminderMinutes = 0,
                                    CreatedById = domain.CreatedBy.Id,
                                    Id = newSub.Id
                                };
                                new NotificationRules(dbContext).NotifyDomainAdminOnPaymentDate(paymentDateReminderNotification);
                            }

                        }
                        else
                        {
                            domainId = domainCreationProcessResult.Object == null ? 0 : ((Qbicles.Models.Base.DataModelBase)domainCreationProcessResult.Object).Id;
                            if (domainId == 0)
                                hasError = true;
                            else
                                hasError = false;
                            errorMessage = domainCreationProcessResult.msg;
                        }
                    }
                }
            }

            var lstDomainPlanLevel = dbContext.BusinessDomainLevels.ToList();
            ViewBag.ListPlanLevels = lstDomainPlanLevel;
            ViewBag.IsTransactionCallBack = true;
            ViewBag.Domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
            ViewBag.HasError = hasError;
            ViewBag.IsTransactionCancelled = false;
            ViewBag.ErrorMsg = errorMessage;
            return View("~/Views/Domain/CreateDomain.cshtml");
        }


        /// <summary>
        /// The call back of the successful transaction
        /// After a minium amount of money is paid, we can get Authorization information. After that:
        /// 1. Refund the paid amount of money
        /// 2. Create a Subscription to the created Plan, with the start_date will be day the Trial time ends
        /// 3. Process the domain Creation in the Qbicles System
        /// </summary>
        /// <param name="planCode">The code of the created paystack plan</param>
        /// <returns></returns>
        public async Task<ActionResult> ProcessSubscriptionAndDomainCreation(string planCode, string userId)
        {
            var currentUser = dbContext.QbicleUser.Find(userId);
            var transactionRef = new DomainRules(dbContext).GetTransactionReferenceByPlanCode(planCode, currentUser.Email);
            var verifyResult = await new DomainRules(dbContext).VerifyDomainCreationTransaction(transactionRef);
            var isTransactionSuccess = verifyResult.status && verifyResult.data.status == "success";
            var domainId = 0;
            var domainRules = new DomainRules(dbContext);


            // Get the authorization information from Transaction Verification response
            if (isTransactionSuccess)
            {
                // Create Customer
                // A Customer can have multiple cards info - So always create new Customer when a domain is created
                var currentPaystackCustomer = new Customer()
                {
                    PaystackCustomerId = verifyResult.data.customer.id,
                    PaystackEmail = verifyResult.data.customer.email,
                    PaystackFirstName = verifyResult.data.customer.first_name,
                    PaystackLastName = verifyResult.data.customer.last_name,
                    User = currentUser,
                    CustomerCode = verifyResult.data.customer.customer_code,
                    Subscriptions = new List<Subscription>()
                };
                dbContext.PaystackCustomers.Add(currentPaystackCustomer);
                dbContext.Entry(currentPaystackCustomer).State = System.Data.Entity.EntityState.Added;
                dbContext.SaveChanges();

                // 1. Cash back - Refund
                var refundCreationResponse = await domainRules.CreatePayStackRefund(transactionRef);
                if (refundCreationResponse.result == true)
                {
                    // 2. Create a Subscription
                    var subscriptionCreationResponse = await domainRules
                        .CreatePayStackSubscription(verifyResult.data.authorization, verifyResult.data.customer, planCode);
                    if (subscriptionCreationResponse.result == true)
                    {
                        var newSub = subscriptionCreationResponse.Object2 as Subscription;
                        // Save subscription to the Customer
                        newSub = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Id == newSub.Id);
                        if (newSub != null)
                        {
                            currentPaystackCustomer.Subscriptions.Add(newSub);
                            dbContext.Entry(currentPaystackCustomer).State = System.Data.Entity.EntityState.Modified;
                            dbContext.SaveChanges();
                        }

                        // 3. Process the Domain creation
                        var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.DomainPlan.PayStackPlanCode == planCode);
                        var domainCreationProcessResult =
                                    await new DomainRules(dbContext)
                                    .ProcessDomainRequest(domainRequest.Id, DomainRequestStatus.Approved, userId, Server.MapPath("~"));
                        if (domainCreationProcessResult.result == true)
                        {
                            domainId = (int)domainCreationProcessResult.Object;
                        }
                    }
                }
            }

            var result = new ReturnJsonModel()
            {
                result = true,
                Object = domainId.Encrypt()
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Callback urls
        public ActionResult DomainCreationTransactionCancelledCallback(string planCode, string userId)
        {
            var domainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.PayStackPlanCode == planCode);
            var currentUser = dbContext.QbicleUser.Find(userId);
            var domainPlanId = domainPlan?.Id ?? 0;
            var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.DomainPlan.Id == domainPlanId);

            ViewBag.DomainPlan = domainPlan;
            ViewBag.DomainRequest = domainRequest;

            var lstDomainPlanLevel = dbContext.BusinessDomainLevels.ToList();
            ViewBag.ListPlanLevels = lstDomainPlanLevel;
            ViewBag.IsTransactionCallBack = true;
            ViewBag.IsTransactionCancelled = true;
            ViewBag.HasError = false;
            return View("~/Views/Domain/CreateDomain.cshtml");
        }

        public async Task<ActionResult> UpdateDomainPlanLevelTransactionCallback(int domainId, int newDomainLevelId)
        {
            var paystackRules = new PayStackRules(dbContext);

            var domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
            var newDomainLevel = dbContext.BusinessDomainLevels.FirstOrDefault(p => p.Id == newDomainLevelId);
            var currentUserId = CurrentUser().Id;
            var currentUser = dbContext.QbicleUser.Find(currentUserId);
            var domainRules = new DomainRules(dbContext);

            if (currentDomainPlan?.Level?.Id == newDomainLevelId)
            {
                ViewBag.HasError = false;
                ViewBag.ErrorMessage = "";
            }
            else
            {
                // 1. Verify transaction status
                var transactionRef = new DomainRules(dbContext).GetTransactionRefByDomainPlan(currentDomainPlan.Id);
                var verifyResult = await new DomainRules(dbContext).VerifyDomainCreationTransaction(transactionRef);
                var isTransactionSuccess = verifyResult.status && verifyResult.data.status == "success";

                if (!isTransactionSuccess)
                {
                    ViewBag.HasError = true;
                    ViewBag.ErrorMessage = "Transaction failed";
                }
                else
                {

                    // 2. Get Authorization data
                    // As a customer can have multiple cards info, alway create a new Customer to save new Authorization data
                    var currentPaystackCustomer = new Customer()
                    {
                        PaystackCustomerId = verifyResult.data.customer.id,
                        PaystackEmail = verifyResult.data.customer.email,
                        PaystackFirstName = verifyResult.data.customer.first_name,
                        PaystackLastName = verifyResult.data.customer.last_name,
                        User = currentUser,
                        CustomerCode = verifyResult.data.customer.customer_code,
                        Subscriptions = new List<Subscription>()
                    };
                    dbContext.PaystackCustomers.Add(currentPaystackCustomer);
                    dbContext.Entry(currentPaystackCustomer).State = System.Data.Entity.EntityState.Added;
                    dbContext.SaveChanges();

                    // SAVE authorization data to the subscription
                    var currentSub = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == currentDomainPlan.Id);
                    currentSub.PayStackAuthorization = verifyResult.data.authorization.ToJson();
                    dbContext.Entry(currentSub).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    // 3. Refund
                    var refundCreationResponse = await domainRules.CreatePayStackRefund(transactionRef);
                    if (refundCreationResponse.result != true)
                    {
                        ViewBag.HasError = true;
                        ViewBag.ErrorMessage = "Refund failed. Please contact to the administrator.";
                    }
                    else
                    {
                        var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                        // 4. Update Domain plan level 
                        var domainPlanLevelResponse = await new DomainRules(dbContext)
                            .ChangeDomainPlanLevel(domainId, newDomainLevelId, currentUserId, baseUrl, currentPaystackCustomer.Id);

                        if (domainPlanLevelResponse.result == true)
                        {
                            ViewBag.HasError = false;
                            ViewBag.ErrorMessage = "";
                        }
                        else
                        {
                            ViewBag.HasError = true;
                            ViewBag.ErrorMessage = domainPlanLevelResponse.msg;
                        }
                    }
                }
            }

            var newestPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
            ViewBag.DomainPlan = newestPlan;
            ViewBag.CurrentDomain = domain;
            ViewBag.DocRetrievalUrl = ConfigManager.ApiGetDocumentUri;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            ViewBag.CurrencySettings = currencySettings;

            var newestSubscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == newestPlan.Id && p.IsActive == true);
            var paystackSubscriptionObj = await new PayStackRules(dbContext).GetPaystackSubscription(newestSubscription?.PayStackSubscriptionCode ?? "");
            ViewBag.NextBillingDate = paystackSubscriptionObj?.next_payment_date ?? null;

            var lstBusinessDomainLevel = dbContext.BusinessDomainLevels.ToList();
            ViewBag.LstBusinessDomainLevel = lstBusinessDomainLevel;

            return View("~/Views/Administration/UpdateSubscriptionDetail.cshtml");
        }


        #region 

        //TODO: remove or optimise

        // Callback urls
        public ActionResult UpdatePromotionTransactionCancelledCallback(string planCode, string userId)
        {
            var domainPlan = dbContext.PromotionTypes.FirstOrDefault(p => p.Icon == planCode);
            var currentUser = dbContext.QbicleUser.Find(userId);
            var domainPlanId = domainPlan?.Id ?? 0;
            var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.DomainPlan.Id == domainPlanId);

            ViewBag.DomainPlan = domainPlan;
            ViewBag.DomainRequest = domainRequest;

            var lstDomainPlanLevel = dbContext.BusinessDomainLevels.ToList();
            ViewBag.ListPlanLevels = lstDomainPlanLevel;
            ViewBag.IsTransactionCallBack = true;
            ViewBag.IsTransactionCancelled = true;
            ViewBag.HasError = false;
            return View("~/Views/Domain/CreateDomain.cshtml");
        }

        #endregion

    }
}