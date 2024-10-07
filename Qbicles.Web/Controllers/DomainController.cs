using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.BusinessRules.PayStack;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleDomain;

namespace Qbicles.Web.Controllers
{

    [Authorize]
    public class DomainController : BaseController
    {
        ReturnJsonModel refModel;

        public ActionResult Index()
        {
            try
            {
                var dr = new DomainRules(dbContext);
                var currentUserId = CurrentUser().Id;
                var user = dr.GetUser(currentUserId);
                if (user == null)
                    return RedirectToAction("Login", "Account");
                if (user != null && !user.IsUserProfileWizardRun)
                {
                    return RedirectToAction("UserProfileWizard", "UserInformation");
                }


                SetCurrentPage("Domain");
                ViewBag.CurrentPage = "Domain";
                #region Order by Domain default Recently Update
                var lstDomain = user.Domains;
                var lstQbicNotNull = lstDomain.Where(p => p.Qbicles.Count > 0).ToList();
                if (lstQbicNotNull.Any())
                {
                    lstDomain = lstDomain.Where(p => !lstQbicNotNull.Any(a => a.Id == p.Id)).ToList();
                    lstQbicNotNull = lstQbicNotNull.OrderByDescending(o => o.Qbicles.OrderByDescending(a => a.LastUpdated).FirstOrDefault().LastUpdated).ToList();
                    lstQbicNotNull.AddRange(lstDomain);
                    lstDomain = lstQbicNotNull;
                }


                ViewBag.Domains = lstDomain.ToList();

                ViewBag.WaitlistRequestRights = new WaitListRules(dbContext).CheckApprovalWaitlist(currentUserId);

                #endregion
                if (ViewBag.CurrentDomainId > 0 || CurrentDomain() != null)
                {
                    var currentDomain = CurrentDomain();
                    SetCurrentDomainIdCookies(currentDomain != null ? currentDomain.Id : 0);

                    if (currentDomain != null)
                        ViewBag.UserCurrentDomain = currentDomain.Users;
                    else
                    {
                        currentDomain = new QbicleDomain
                        {
                            Name = "All Domains",
                            Id = 0
                        };
                    }
                    ViewBag.InvitationCount = new OurPeopleRules(dbContext).NotificationCountPendingByUser(user.Email);
                    ViewBag.PageTitle = "Domain Management";

                    return View(currentDomain);
                }
                else
                {
                    ViewBag.PageTitle = "Domain Management";
                    ViewBag.InvitationCount = new OurPeopleRules(dbContext).NotificationCountPendingByUser(user.Email);
                    var domain = new QbicleDomain
                    {
                        Name = "Guest Domains",
                        Id = -1
                    };
                    return View(domain);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return RedirectToAction("Login", "Account");
            }
        }

        /// <summary>
        /// //get list users associated with the Domain has selected
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUsersByDomain()
        {
            refModel = new ReturnJsonModel();
            try
            {
                var users = CurrentDomain().Users;
                StringBuilder str = new StringBuilder();
                foreach (var item in users)
                {
                    var fullName = (string.IsNullOrEmpty(item.Forename) || string.IsNullOrEmpty(item.Surname)) ? item.UserName : item.Forename + " " + item.Surname;
                    str.AppendFormat("<option value='{0}' LogoUri='{1}' api='{2}'>{3}</option>", item.Id, item.ProfilePic, ConfigManager.ApiGetDocumentUri, fullName);
                }
                refModel.Object = str.ToString();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUsersByDomainId(int domainId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var dRules = new DomainRules(dbContext);
                var users = dRules.GetUsersByDomainId(domainId);

                StringBuilder str = new StringBuilder();
                foreach (var item in users)
                {
                    var fullName = (string.IsNullOrEmpty(item.Forename) || string.IsNullOrEmpty(item.Surname)) ? item.UserName : item.Forename + " " + item.Surname;
                    str.AppendFormat("<option value='{0}'>{1}</option>", item.Id, fullName);
                }
                refModel.Object = str.ToString();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllDomain()
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.ProxyCreationEnabled = false;
                return Json(new DomainRules(dbContext).GetAllDomain().BusinessMapping(CurrentUser().Timezone), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
            finally
            {
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }

        }

        /// <summary>
        /// Create user for Domain
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateUser(string email, string originatingConnectionId = "")
        {
            try
            {
                refModel = new ReturnJsonModel();

                var userRule = new UserRules(dbContext);

                //Checks to see if the email address exists in the application
                var userNew = userRule.GetUserByEmail(email);

                var domain = CurrentDomain();
                var user = new UserRules(dbContext).GetById(CurrentUser().Id);

                var eventNotify = NotificationEventEnum.InvitedMember;
                // If the application finds that the email address does NOT exist in the system
                if (userNew == null)
                {
                    //adds the user
                    var newUser = userRule.CreateUserInvitedByEmail(email);
                    domain.Users.Add(newUser);
                    dbContext.SaveChanges();
                    //sends the user the 'Token' email to tell them that they have been invited to join Qbicles.
                    //send here
                    string callbackUrl = GenerateUrlToken(newUser.Id, domain.Id,
                        QbicleActivity.ActivityTypeEnum.Domain, user.Email);
                    new EmailRules(dbContext).SendEmailInvitedGuest(user.Id, newUser.Email,
                        callbackUrl, QbicleActivity.ActivityTypeEnum.Domain, domain.Name, "");

                    eventNotify = NotificationEventEnum.CreateMember;

                }
                else
                {

                    //. If the user is available in Qbicles then a link is made between the user and the current Domain using the 'domainsusersxref' cross - reference.
                    domain.Users.Add(userNew);
                    dbContext.SaveChanges();
                    //The application will send an email to the user to tell them that they are now a full domain member
                    //send here
                    eventNotify = NotificationEventEnum.InvitedMember;

                }

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    DomainId = domain.Id,
                    QbicleId = CurrentQbicleId(),
                    EventNotify = eventNotify,
                    AppendToPageName = ApplicationPageName.Domain,
                    CreatedByName = user.GetFullName(),
                    CreatedById = user.Id,
                    ObjectById = userNew.Id,
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2UserCreateRemoveFromDomain(activityNotification);

                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult GetDomainsByCurrentUser()
        {
            try
            {
                var listDomains = new DomainRules(dbContext).GetDomainsByUserId(CurrentUser().Id).BusinessMapping(CurrentUser().Timezone);
                var listDommainsForCombobox = listDomains.Select(x => new { text = x.Name, id = x.Id }).ToList();
                return Json(listDommainsForCombobox, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public async Task<ActionResult> UpdateOrInsertDomain(QbicleDomain domain, Qbicle initQbicle = null, string domainKey = "")
        {
            refModel = new ReturnJsonModel { result = false, msg = "An error" };
            domain.Id = 0;
            if (!string.IsNullOrEmpty(domainKey.Trim()))
            {
                domain.Id = Int32.Parse(EncryptionService.Decrypt(domainKey));
            }

            refModel = await new DomainRules(dbContext).UpdateOrInsertDomain(domain, CurrentUser().Id, Server.MapPath("~"), initQbicle);
            var domainId = ((QbicleDomain)refModel.Object).Id;
            refModel.Object = domainId;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ProcessDomainRequest(int domainRequestId, DomainRequestStatus status)
        {
            refModel = new ReturnJsonModel { result = false, msg = "An error occured" };
            refModel = await new DomainRules(dbContext).ProcessDomainRequest(domainRequestId, status, CurrentUser().Id, Server.MapPath("~"));
            //parse Domain object to json cause circular reference ( user - domain - user - domain ....)
            //Ajax from frontend don't use result.object too.
            //QBIC-4960
            refModel.Object = null;

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ProcessMultipleDomainRequests(List<int> lstRequestId, DomainRequestStatus status)
        {
            refModel = new ReturnJsonModel { result = false, msg = "An error occured" };
            try
            {
                foreach (var requestId in lstRequestId)
                {
                    await new DomainRules(dbContext).ProcessDomainRequest(requestId, status, CurrentUser().Id, Server.MapPath("~"));
                }
                refModel.result = true;
                refModel.msg = "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstRequestId, status);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetListDomainByAccount([Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderBy,
           string name, string dateRange, int status, string type, int start, int length, int draw)
        {
            var lstDomains = new DomainRules(dbContext).GetAllDomain(column, orderBy, name, dateRange, status, start, length, type,
                CurrentUser().DateFormat, CurrentUser().Timezone, CurrentUser().Id, draw, CurrentUser().DateTimeFormat);

            return Json(lstDomains, JsonRequestBehavior.AllowGet);
        }
        public ActionResult IsDuplicateDomainNameCheck(string domainKey, string domainName)
        {
            try
            {
                var domainId = 0;
                if (!string.IsNullOrEmpty(domainKey.Trim()))
                {
                    domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
                }
                return Json(new DomainRules(dbContext).IsDuplicateDomainNameCheck(domainId, domainName), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult OpenOrCloseDomainById(string domainKey)
        {
            try
            {
                var domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
                var res = new DomainRules(dbContext).OpenOrCloseDomainById(domainId, CurrentUser().Id);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult SearchDomain(string title, int order)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                var dr = new DomainRules(dbContext);
                var users = dr.GetUser(CurrentUser().Id);
                var lstDomain = users.Domains.BusinessMapping(CurrentUser().Timezone);
                if (!string.IsNullOrEmpty(title))
                    lstDomain = lstDomain.Where(p => p.Name.ToLower().Contains(title.ToLower())).ToList();
                if (order == 0)
                {
                    var lstQbicNotNull = lstDomain.Where(p => p.Qbicles.Count > 0).ToList();
                    if (lstQbicNotNull.Any())
                    {
                        lstDomain = lstDomain.Where(p => !lstQbicNotNull.Any(a => a.Id == p.Id)).ToList();
                        lstQbicNotNull = lstQbicNotNull.OrderByDescending(o => o.Qbicles.OrderByDescending(a => a.LastUpdated).FirstOrDefault().LastUpdated).ToList();
                        lstQbicNotNull.AddRange(lstDomain);
                        lstDomain = lstQbicNotNull;
                    }

                }
                else if (order == 1)
                {
                    lstDomain = lstDomain.OrderBy(o => o.Name).ToList();
                }
                else
                {
                    lstDomain = lstDomain.OrderByDescending(o => o.Name).ToList();
                }
                var partialView = RenderViewToString("~/Views/Domain/_ListDomainPartial.cshtml", lstDomain);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                refModel.Object = ex;
                refModel.result = false;
                refModel.msg = ex.Message;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
        [HandleExceptionAjax(System.Net.HttpStatusCode.MethodNotAllowed)]
        [System.Diagnostics.DebuggerStepThrough]
        public ActionResult AccessUserDenied()
        {
            IAuthenticationManager AuthenticationManager = HttpContext.GetOwinContext().Authentication;
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignOut();
            throw new Exception("Your account has been locked!");
        }
        [System.Diagnostics.DebuggerStepThrough]
        [HandleExceptionAjax(System.Net.HttpStatusCode.MethodNotAllowed)]
        public ActionResult AccessDomainDenied()
        {
            throw new Exception("You have been denied domain access!");
        }


        [HttpPost]
        public ActionResult GetListDomainApplicationAccess([Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderBy,
           string name, int status, int start, int length, int draw)
        {
            var lstDomains = new DomainRules(dbContext).GetListDomainApplicationAccess(column, orderBy, name, status, start, length, draw);

            return Json(lstDomains, JsonRequestBehavior.AllowGet);
        }


        public ActionResult InvitationApproverByUser()
        {
            var applications = new OurPeopleRules(dbContext).GetInvitationApproverByUser(CurrentUser().Email);
            return PartialView("_DomainInvites", applications);
        }

        public ActionResult UpdateDateDomainSetting(string domainKey, string name, string image)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(domainKey?.Trim()))
            {
                id = Int32.Parse(EncryptionService.Decrypt(domainKey));
            }
            var refModal = new DomainRules(dbContext).UpdateDateDomainSetting(id, name, image);
            return Json(refModal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddQbicleDomainRequest(DomainRequest domain, InitialQbicleRequest initialQbicle, DomainTypeEnum domainType)
        {
            var userId = CurrentUser().Id;
            var saveResult = new DomainRules(dbContext).CreateDomainRequest(domain, initialQbicle, domainType, userId);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpgradeBusinessDomain(string domainKey)
        {
            var domainId = string.IsNullOrEmpty(domainKey) ? 0 : int.Parse(domainKey.Decrypt());
            var currentUserId = CurrentUser().Id;
            var updateResult = new DomainRules(dbContext).CreateUpgradedDomainRequest(domainId, currentUserId, DomainTypeEnum.Premium);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadDomainRequestTableContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keySearch, string dateRange, string createdUserIdSearch, DomainTypeEnum domainTypeSearch,
            List<DomainRequestStatus> lstRequestStatusSearch, int start, int length, int draw, bool isSearchingForCurrentUser = false)
        {
            try
            {
                var userSettings = CurrentUser();
                var dateTimeFormat = userSettings.DateTimeFormat;
                var dateFormat = userSettings.DateFormat;
                var timeZone = userSettings.Timezone;
                var totalRecord = 0;
                if (isSearchingForCurrentUser)
                {
                    createdUserIdSearch = userSettings.Id;
                }
                List<QbicleDomainRequestCustomModel> lstResult = new DomainRules(dbContext).GetListDomainRequestPagination(requestModel, keySearch, dateRange, createdUserIdSearch, domainTypeSearch, lstRequestStatusSearch, dateTimeFormat, dateFormat, timeZone, ref totalRecord, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return Json(new DataTableModel() { draw = draw, data = new List<QbicleDomainRequestCustomModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public int GetDomainRequestCount(DomainRequestStatus status)
        {
            var lstRequests = new DomainRules(dbContext).GetListDomainRequestByStatus(status);
            return lstRequests.Count();
        }

        public ActionResult CheckPendingRequest()
        {
            var pendingDomainRequestNum = dbContext.QbicleDomainRequests.AsNoTracking().Count(p => p.Status == DomainRequestStatus.Pending);
            var pendingExtensionRequestNum = dbContext.DomainExtensionRequests.AsNoTracking().Count(p => p.Status == ExtensionRequestStatus.Pending);
            //Thomas: Add check exist verify new email
            var pendingVerifyNewEmail = new UserRules(dbContext).CheckPendingVerifyNewEmail(CurrentUser().Id);
            return Json(new { pendingDomainRequestNum, pendingExtensionRequestNum, pendingVerifyNewEmail }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateCurrentDomain(string domainKey)
        {
            try
            {
                SetCurrentDomainIdCookies(domainKey);
                return Json(new ReturnJsonModel() { result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return Json(new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_5") }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult HeartBeat()
        {
            return Json(1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DomainBusinessProfileWirad(string key)
        {
            refModel = new ReturnJsonModel { result = true };
            refModel.msgId = new DomainRules(dbContext).GetBusinessProfileUnwizard(key, false);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Wizard(string key)
        {
            try
            {
                var domainId = int.Parse(key.Decrypt());
                var profile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId) ?? new Qbicles.Models.B2B.B2BProfile();
                ViewBag.Domain = profile.Domain ?? dbContext.Domains.FirstOrDefault(e => e.Id == domainId);
                ViewBag.BusinessCategories = new AdminListingRules(dbContext).GetAllBusinessCategories() ?? new List<BusinessCategory>();
                var userSlots = HelperClass.GetDomainUsersAllowed(domainId);
                ViewBag.CanSendInvite = userSlots.ActualMembers < userSlots.UsersAllowed;
                return View(profile);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return RedirectToAction("Login", "Account");
            }
        }

        public async Task<ActionResult> CreateDomain()
        {
            // Domain Creation Plan Levels
            var lstDomainPlanLevel = dbContext.BusinessDomainLevels.ToList();
            ViewBag.ListPlanLevels = lstDomainPlanLevel;
            ViewBag.IsTransactionCallBack = false;

            // Get list banks available in Nigeria
            var countryName = "nigeria";
            var lstPaystackBanks = await new PayStackRules(dbContext).GetListPaystackBanks(countryName);
            ViewBag.ListBanks = lstPaystackBanks.Where(p => p.is_deleted == false && p.active == true).ToList();

            var b2cOrderCharge = dbContext.B2COrderPaymentCharges.FirstOrDefault();
            ViewBag.B2COrderCharge = b2cOrderCharge;

            return View();
        }


        public ActionResult ReserveDomainRequestName(string requestedName, int requestId = 0)
        {
            var saveResult = new DomainRules(dbContext).ReserveDomainRequestName(requestedName, requestId, CurrentUser().Id);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> CreateDomainRequestPlan(int domainRequestId, int userNumber, BusinessDomainLevelEnum level)
        {
            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
            var creationResult = await new DomainRules(dbContext)
                .CreatePaystackPlanForDomainRequest(domainRequestId, userNumber, level, CurrentUser().Id, baseUrl);
            return Json(creationResult, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SaveDomainRequestSpecifics(string description, string uploadKey, int requestId)
        {
            var updateResult = new DomainRules(dbContext).SaveDomainSpecifics(description, uploadKey, requestId, CurrentUser().Id, Server.MapPath("~"));

            var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.Id == requestId);

            // If the request type is Free - no payment needed, then this step will create the Domain right away
            if (domainRequest.DomainPlan != null && domainRequest.DomainPlan.Level.Level == BusinessDomainLevelEnum.Free && updateResult != null && updateResult.result)
            {
                var domainCreationProcessResult =
                    await new DomainRules(dbContext).ProcessDomainRequest(requestId, DomainRequestStatus.Approved, CurrentUser().Id, Server.MapPath("~"));
                //parse Domain object to json cause circular reference ( user - domain - user - domain ....)
                //Ajax from frontend don't use result.object too.
                //QBIC-4960
                domainCreationProcessResult.Object = null;
                return Json(domainCreationProcessResult, JsonRequestBehavior.AllowGet);
            }

            return Json(updateResult, JsonRequestBehavior.DenyGet);
        }

        public async Task<ActionResult> UpdateDomainSlotNumber(int newTotalSlotNumber)
        {
            var currentDomainId = CurrentDomainId();
            var updateResponse = await new DomainRules(dbContext).UpdateSlotNumberOfPayStackPlan(currentDomainId, newTotalSlotNumber, CurrentUser().Id);
            return Json(updateResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveSubAccountInfoToDomainRequest(string businessName, string bankCode, string accountNumber, int domainRequestId)
        {
            var savingResult = new DomainRules(dbContext).SaveSubAccountInfoToDomainRequest(businessName, bankCode, accountNumber, domainRequestId);
            return Json(savingResult, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> UpdatePaystackSubAccountInformation(string businessName, string bankCode, string accountNumber)
        {
            var currentDomainId = CurrentDomain().Id;
            var currentUserId = CurrentUser().Id;
            var updateResult = await new DomainRules(dbContext).UpdatePaystackSubAccount(currentDomainId, businessName, bankCode, accountNumber, currentUserId);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ShowCreateDomainCustomModal()
        {
            return PartialView("_CreateDomainCustomModal");
        }

        public async Task<ActionResult> ProcessCustomDomainCreate(DomainRequest domainRequest)
        {
            refModel = new ReturnJsonModel { result = false, msg = "An error occured" };
            refModel = await new DomainRules(dbContext).ProcessCustomDomainCreate(CurrentUser().Id, Server.MapPath("~"), domainRequest);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowJoinWaitlistModal()
        {
            ViewBag.Countries = new CountriesRules().GetCountries();
            ViewBag.BusinessCategories = dbContext.BusinessCategories.OrderBy(e => e.Name).AsNoTracking().Select(e => new Select2CustomeModel { id = e.Id, text = e.Name }).ToList();
            return PartialView("_JoinWaitlistModal");
        }
    }
}