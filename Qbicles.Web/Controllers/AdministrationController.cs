using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.BusinessRules.Community;
using Qbicles.BusinessRules.BusinessRules.PayStack;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.MyBankMate;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Community;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Resources;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.Web.Controllers
{
    public class AdministrationController : BaseController
    {
        private ReturnJsonModel refModel;

        public async Task<ActionResult> AdminPermissions()
        {
            SetCurrentPage("AdminPermissions");
            ViewBag.CurrentPage = "AdminPermissions";
            ViewBag.PageTitle = "Domain Administration - Apps";
            var userSetting = CurrentUser();
            var curUser = dbContext.QbicleUser.Find(userSetting.Id);
            var domainId = CurrentDomainId();
            var currentDomain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
            if (domainId == 0 && !SystemRoleValidation(userSetting.Id, SystemRoles.SystemAdministrator) && !curUser.DomainAdministrators.Any(e => e.Id == domainId))
            {
                return View("Error");
            }

            // Subscription plan info
            var domainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain != null && p.Domain.Id == domainId && p.IsArchived == false);
            var domainPlanId = domainPlan?.Id ?? 0;
            var subscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan != null && p.Plan.Id == domainPlanId);
            ViewBag.domainPlan = domainPlan;
            ViewBag.domainSubscription = subscription;

            var paystackSubscriptionObj = await new PayStackRules(dbContext).GetPaystackSubscription(subscription?.PayStackSubscriptionCode ?? "");
            var nextBillingDateStr = paystackSubscriptionObj?.next_payment_date == null
                ? "--" : HelperClass.DatetimeToOrdinal((DateTime)paystackSubscriptionObj.next_payment_date);
            ViewBag.NextBillingDateStr = nextBillingDateStr;
            // END: Subscription plan info

            // SubAccount info
            var subAccCode = currentDomain?.SubAccountCode ?? "";
            if (!string.IsNullOrEmpty(subAccCode))
            {
                var subAccountInfo = await new PayStackRules(dbContext).GetSubAccountInformation(subAccCode);
                ViewBag.SubAccountInfo = subAccountInfo;
            }
            var lstNigeriaBanks = await new PayStackRules(dbContext).GetListPaystackBanks("nigeria");
            ViewBag.ListBanks = lstNigeriaBanks.Where(p => p.active == true && p.is_deleted == false).Distinct().ToList();
            var b2cOrderCharge = dbContext.B2COrderPaymentCharges.FirstOrDefault();
            ViewBag.B2COrderCharge = b2cOrderCharge;
            // END: SubAccoutn info

            var domainRoleRule = new DomainRolesRules(dbContext);
            var roles = domainRoleRule.GetDomainRolesDomainId(domainId);
            //Domain roles
            ViewBag.Roles = roles;
            //Applications
            ViewBag.Applications = new DomainRules(dbContext).GetDomainAvailableApps(domainId);
            ViewBag.UserRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, domainId);
            ViewBag.UsersNotExistInRole = domainRoleRule.GetUsersNotExistInRole(domainId, roles.FirstOrDefault()?.Id ?? 0);

            // Account Tab
            ViewBag.Packages = new AccountPackageRules(dbContext).GetAllAccountPackage();
            var aList = new AccountRules(dbContext).ListAdministrator(CurrentDomain())
                .GroupBy(x => new { x.Id, x.Name, x.Avatar }).Select(y => new AdministratorViewModal
                {
                    Id = y.Key.Id,
                    Name = y.Key.Name,
                    Domains = y.Select(d => d.Domain).ToList(),
                    Avatar = y.Key.Avatar,
                    Levels = y.Select(z => new KeyValuePair<string, int>(z.Level,
                            z.Level == AdministratorViewModal.AccountOwner
                                ? 1
                                : z.Level == AdministratorViewModal.AccountAdministrators
                                    ? 2
                                    : z.Level == AdministratorViewModal.DomainAdministrator
                                        ? 3
                                        : 999))
                          .Distinct().ToList()
                }).ToList();
            ViewBag.ListAdministrator = aList;
            //End Account Tab

            //Check for request to upgrade to Premium Domain Ability
            var upgradeRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.ExistingDomain != null
                                    && p.DomainType == QbicleDomain.DomainTypeEnum.Premium
                                    && p.ExistingDomain.Id == domainId && p.Status == DomainRequestStatus.Pending);

            ViewBag.HasUpgradeRequest = upgradeRequest != null;
            //End Check for request to upgrade to Premium Domain Ability

            //Extension Request Tab
            var extensionRules = new DomainExtensionRules(dbContext);
            ViewBag.ArticleRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Articles);
            ViewBag.EventRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Events);
            ViewBag.JobRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Jobs);
            ViewBag.KnowledgeRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Knowledge);
            ViewBag.NewsRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.News);
            ViewBag.RealEstateRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.RealEstate);

            ViewBag.LstExtensionRequestCreators = extensionRules.GetListExtensionRequestCreators(domainId);
            //End Extension Request Tab
            return View();
        }

        public ActionResult AdminSysManage(string tabActive = "sysadmin")
        {
            SetCurrentPage("Administration");
            ViewBag.CurrentPage = "Administration";

            if (!SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator) && !SystemRoleValidation(CurrentUser().Id, SystemRoles.QbiclesBankManager))
            {
                ViewBag.PageTitle = "Administrator";
                return View("Error");
            }
            switch (tabActive.ToLower())
            {
                case "domains":
                    ViewBag.Title = "Administration - Domains";
                    break;

                case "bankmatetrans":
                    ViewBag.Title = "Administration - Bankmate transactions";
                    break;

                case "hlsetup":
                    ViewBag.Title = "Administration - Categories & lists";
                    break;

                case "domainrequest":
                    ViewBag.Title = "Administration - Domain request";
                    break;

                case "extensionrequest":
                    ViewBag.Title = "Administration - Extension request";
                    break;

                case "waitlistrequest":
                    ViewBag.Title = "Administration - Waitlist";
                    break;

                case "communityfeature":
                    ViewBag.Title = "Administration - Community features";
                    break;

                case "monibackpromotion":
                    ViewBag.Title = "Administration - Moniback promotions";
                    break;

                case "bulkdeal":
                    ViewBag.Title = "Administration - Bulk Deal promotions";
                    break;

                default:
                    ViewBag.Title = "Administration - System";
                    break;
            }

            return View("Manage");
        }

        public ActionResult NavigationAdministrationPartial()
        {
            ViewBag.CurrentPage = CurrentPage();
            return PartialView("_NavigationAdministrationPartial");
        }

        public ActionResult AdminSystemsTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                {
                    ViewBag.DomainRoles = dbContext.Roles.Select(r => new DomainRoleModel { Id = r.Id, Name = r.Name }).ToList();
                    ViewBag.SystemSettings = dbContext.LoyaltySystemSettings.FirstOrDefault(p => !p.IsArchived);
                    return PartialView("_AdminSystemsTab");
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminDomainRequestTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                {
                    var lstRequestCreator = new DomainRules(dbContext).getDomainRequestCreator();
                    ViewBag.lstRequestCreator = lstRequestCreator;
                    return PartialView("_AdminDomainRequest");
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminWaitlistRequestTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (!SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                    return View("Error");

                ViewBag.Countries = new CountriesRules().GetCountries();
                ViewBag.BusinessCategories = dbContext.BusinessCategories.OrderBy(e => e.Name).AsNoTracking().Select(e => new Select2CustomeModel { id = e.Id, text = e.Name }).ToList();

                return PartialView("_AdminWaitlistRequest");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminExtensionRequestTabContent()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                {
                    var lstRequestCreator = new DomainExtensionRules(dbContext).GetListExtensionRequestCreators(0);
                    ViewBag.lstRequestCreator = lstRequestCreator;
                    return PartialView("_AdminExtensionRequestTab");
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminCommunityFeatureTabContent()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                {
                    return PartialView("_CommunityFeatureTab");
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// Admin view for MonibackPromotion
        /// </summary>
        /// <returns></returns>
        public ActionResult AdminMonibackPromotionTabContent()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                {
                    return PartialView("_AdminMonibackPromotionTab");
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// Admin view for Bulk Deal Promotion
        /// </summary>
        /// <returns></returns>
        public ActionResult BulkDealTabContent(BulkDealSearchParameter bulkDealParameter)
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                {
                    ViewBag.currentTimeZone = CurrentUser().Timezone;
                    ViewBag.CurrentDateFormat = CurrentUser().DateFormat;
                    ViewBag.DocRetrievalUrl = ConfigManager.ApiGetDocumentUri;
                    ViewBag.BulkDealPromotions = new PromotionRules(dbContext).GetBulkDealPromotions(bulkDealParameter);
                    return PartialView("_BulkDealTab");
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ApplyFilterToBulkDealPromotions(BulkDealSearchParameter bulkDealParameter)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var userinfo = CurrentUser();
                bulkDealParameter.DateFormat = userinfo.DateFormat;
                bulkDealParameter.Timezone = userinfo.Timezone;
                var bulkDeals = new PromotionRules(dbContext).GetBulkDealPromotions(bulkDealParameter);
                return PartialView("~/Views/Administration/_BulkDealListList.cshtml", bulkDeals);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public ActionResult LoadModalPromotionAddEdit(string promotionKey)
        {
            var currentDomainId = CurrentDomainId();
            ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(currentDomainId);
            ViewBag.CurrencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);
            return PartialView("~/Views/Administration/_ModalPromotionAddEdit.cshtml", new PromotionRules(dbContext).GetPromotionTypeByKey(promotionKey));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public ActionResult LoadModalPromotionView(string promotionKey)
        {
            var currentDomainId = CurrentDomainId();
            ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(currentDomainId);
            ViewBag.CurrencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);
            return PartialView("~/Views/Administration/_ModalPromotionView.cshtml", new PromotionRules(dbContext).GetPromotionTypeByKey(promotionKey));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadModalRankPromotion()
        {
            return PartialView("~/Views/Administration/_ModalRankPromotion.cshtml");
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadModalPromotionsUsedBy(string promotionKey)
        {
            return PartialView("~/Views/Administration/_ModalPromotionUsedBy.cshtml", new PromotionRules(dbContext).GetPromotionTypeByKey(promotionKey));
        }

        public ActionResult GetItemOverviewItemProductForSearchAndAddBulkDeal1(string keysearch, int start, int take)
        {
            var result = new PromotionRules(dbContext).GetItemOverviewItemProductForSearchAndAddBulkDeal(keysearch, start, take);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItemOverviewItemProductForSearchAndAddBulkDeal(int draw, int start, int length, string searchValue)
        {
            var result = new PromotionRules(dbContext).GetItemOverviewItemProductForSearchAndAddBulkDeal(draw, start, length, searchValue);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="keyword"></param>
        /// <param name="daterange"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult GetPromotionTypes([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int type)
        {
            var userinfo = CurrentUser();
            PromotionFilterModel filterModel = new PromotionFilterModel();
            filterModel.keyword = keyword;
            filterModel.daterange = daterange;
            filterModel.type = type;
            filterModel.dateformat = userinfo.DateFormat;
            filterModel.timezone = userinfo.Timezone;
            filterModel.currentDomainId = CurrentDomainId();
            return Json(new PromotionRules(dbContext).GetPromotionTypes(requestModel, filterModel), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public ActionResult GetParticipatingDomainsInPromotionType([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string promotionKey)
        {
            return Json(new PromotionRules(dbContext).GetParticipatingDomainsInPromotionType(requestModel, promotionKey), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRankPromotionTypes([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var userinfo = CurrentUser();
            PromotionFilterModel filterModel = new PromotionFilterModel();
            filterModel.dateformat = userinfo.DateFormat;
            filterModel.timezone = userinfo.Timezone;
            filterModel.currentDomainId = CurrentDomainId();
            return Json(new PromotionRules(dbContext).GetRankPromotionTypes(requestModel, filterModel), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SavePromotionType(PromotionTypeModel model)
        {
            //Get current user info
            var userinfo = CurrentUser();

            //Update model entity
            model.CreatedBy = userinfo.Id;
            model.LastModifiedBy = userinfo.Id;

            return Json(new PromotionRules(dbContext).SavePromotionType(model));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveRankPromotionOrder(List<PromotionTypeModel> model)
        {
            //Get current user info
            var userinfo = CurrentUser();

            //Return
            return Json(new PromotionRules(dbContext).SaveRankPromotionOrder(model, userinfo.Id));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult ManagePromotionTypeStatus(string key)
        {
            var id = Int32.Parse(EncryptionService.Decrypt(key));
            var res = new PromotionRules(dbContext).ManagePromotionTypeStatusById(id, CurrentUser().Id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadBulkDealPromotions(PromotionFilterModel filterModel)
        {
            var userinfo = CurrentUser();
            filterModel.dateformat = userinfo.DateFormat;
            filterModel.timezone = userinfo.Timezone;
            filterModel.currentDomainId = CurrentDomainId();
            var promotions = new PromotionRules(dbContext).GetActivePromotions(filterModel);
            return PartialView("~/Views/Administration/_BulkDealPromotionsContent.cshtml", promotions);
        }

        public ActionResult loadSearchItemsForBulkDealPromotions(PromotionFilterModel filterModel)
        {
            var userinfo = CurrentUser();
            filterModel.dateformat = userinfo.DateFormat;
            filterModel.timezone = userinfo.Timezone;
            filterModel.currentDomainId = CurrentDomainId();
            var promotions = new PromotionRules(dbContext).GetActivePromotions(filterModel);
            return PartialView("~/Views/Administration/_BulkDealPromotionsContent.cshtml", promotions);
        }

        public ActionResult loadAddEditBulkDeal(string promotionKey)
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.ProxyCreationEnabled = false;
                var currentDomainId = CurrentDomainId();
                ViewBag.CurrencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);
                return PartialView("~/Views/Administration/_ModalBulkDealAddEdit.cshtml", new PromotionRules(dbContext).GetBulkDealPromotionByKey(promotionKey));
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

        public ActionResult loadAddToBulkDeal()
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.ProxyCreationEnabled = false;
                return PartialView("~/Views/Administration/_ModalAddToBulkDealPromotion.cshtml", new PromotionRules(dbContext).GetBulkDealCreation());
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

        public ActionResult SaveBulkDealPromotion(PromotionModel model, ItemDiscountBulkDealVoucherInfo itemDiscountVoucherInfo, string featuredImageUri)
        {
            var userinfo = CurrentUser();
            model.Timezone = userinfo.Timezone;
            model.DateFormat = userinfo.DateTimeFormat;
            model.CurrentUserId = userinfo.Id;
            model.CurrentDomainId = CurrentDomainId();
            var res = new PromotionRules(dbContext).SaveBulkDealPromotion(model, itemDiscountVoucherInfo, featuredImageUri);
            return Json(res);
        }

        public ActionResult ApplicationAccess()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                    return PartialView("_ApplicationAccess");

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminHighlightSetup()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                    return PartialView("_HLSetupTab");

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminDomainsTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                var domainId = CurrentDomainId();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.SystemAdministrator))
                    return PartialView("_AdminDomainsTab");

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminBankmatetransTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                if (SystemRoleValidation(CurrentUser().Id, SystemRoles.QbiclesBankManager))
                {
                    ViewBag.PendingFilterModal = new MyBankMateRules(dbContext).GetParmatersForFilter();
                    ViewBag.HistoryFilterModal = new MyBankMateRules(dbContext).GetParmatersForFilter(false);
                    return PartialView("_AdminBankmateTransactions");
                }
                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminDataRecoveryTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                return PartialView("_AdminDataRecoveryTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminAccountStatementTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                return PartialView("_AdminAccountStatementTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AdminSkillsTab()
        {
            try
            {
                ViewBag.CurrentPage = CurrentPage();
                return PartialView("_AdminSkillsTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SysadminAppAddRemoveModalShow(bool add, bool all)
        {
            var applications = new QbicleApplicationsRules(dbContext).GetQbicleApplicationsNotCore();
            ViewBag.IsAdd = add;
            ViewBag.IsAll = all;
            return PartialView("_SysadminAppAddRemoveModal", applications);
        }

        public ActionResult SaveAddRemoveAppsDomains(bool add, bool all, string[] domainKeys, string[] appIds)
        {
            var refModel = new QbicleApplicationsRules(dbContext).SaveAddRemoveAppsDomains(add, all, domainKeys, appIds);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SysadminAppAssignModalShow(string domainkey)
        {
            var domainId = Int32.Parse(EncryptionService.Decrypt(domainkey));
            var applications = new QbicleApplicationsRules(dbContext).GetDomainAppAssign(domainId);
            ViewBag.DomainKey = domainkey;
            return PartialView("_SysadminAppAssignModal", applications);
        }

        public ActionResult SaveAppAssignModal(string domainKey, string[] addAppIds = null)
        {
            var refModel = new QbicleApplicationsRules(dbContext).SaveAppAssignModal(domainKey, addAppIds);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RevokeAllApps(bool all, string[] domainIds)
        {
            var refModel = new QbicleApplicationsRules(dbContext).RevokeAllApps(all, domainIds, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUsersNotExistInRole(int domainId, int roleId)
        {
            var usersNotExistInRole = new DomainRolesRules(dbContext).GetUsersNotExistInRole(domainId, roleId);
            return PartialView("_UsersNotExistInRoleModal", usersNotExistInRole);
        }

        // GET: Administration
        public ActionResult UserProfile(string userId = "")
        {
            try
            {
                var notificationMethod =
                    EnumModel.ConvertEnumToList<Notification.NotificationSendMethodEnum>();
                var notificationSound =
                    EnumModel.ConvertEnumToList<Notification.NotificationSound>();
                var userRule = new UserRules(dbContext);
                var user = userRule.GetUser(userId == "" ? CurrentUser().Id : userId, 0);
                //get avata template

                var path = WebConfigurationManager.AppSettings["AvataTemp"].Replace("~", "") + "/";
                var fullPath = Server.MapPath(WebConfigurationManager.AppSettings["AvataTemp"]);

                var avatas = new DirectoryInfo(fullPath).GetFiles().Select(o => path + o.Name).ToArray();

                ViewBag.user = user;
                ViewBag.currentAvatar = userRule.CheckAvatarExist(user.ProfilePic);
                ViewBag.notificationMethod = notificationMethod;
                ViewBag.notificationSound = notificationSound;
                ViewBag.avataTemplate = avatas;
                ViewBag.CurrentPage = "Profile"; SetCurrentPage("Profile");
                ViewBag.PageTitle = "Profile overview";
                ViewBag.ListFileType = new FileTypeRules(dbContext).GetExtension();
                if (CurrentDomainId() > 0)
                {
                    var domainAdmin = CurrentDomain().Administrators.Any(u => u.Id == user.Id) ? AdminLevel.Administrators.GetDescription() : "";
                    ViewBag.DomainAdministrator = domainAdmin;
                    ViewBag.ProfileLabel = domainAdmin;
                }
                else
                {
                    ViewBag.IsGuestQbicles = "GuestQbicles";
                }

                var tzs = TimeZoneInfo.GetSystemTimeZones();
                var timezoneList = tzs.Select(tz => new SelectListItem
                {
                    Text = tz.DisplayName,
                    Value = tz.Id
                }).ToList();
                ViewBag.TimezoneList = timezoneList;
                ViewBag.Interests = new AdminListingRules(dbContext).GetAllBusinessCategories();
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DuplicateUserName(string userId, string UserName)
        {
            refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new UserRules().ValidDuplicateUserName(userId, UserName);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult loadPreferredQbicles(string domainKey)
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey.Trim()))
            {
                domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
            }
            return Json(new UserRules(dbContext).loadPreferredQbicles(domainId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DuplicateEmail(string email)
        {
            refModel = new ReturnJsonModel();
            try
            {
                refModel = new UserRules().ValidDuplicateEmail(email);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveProfile(MyProfileModel user)
        {
            //var media = new MediaModel();
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                var userRules = new UserRules();
                user.Id = CurrentUser().Id;
                refModel = userRules.SaveUser(user);

                if (refModel.result)
                {
                    SetUserSettingsCookie((ApplicationUser)refModel.Object);
                    refModel.Object = null;
                    // Reset Date store UI Settings My Desk
                    new MyDesksRules(dbContext).MyDeskResetDateStoreUiSetting("MyDesk", CurrentUser().Id);
                }
            }
            catch (Exception ex)
            {
                refModel.Object = ex;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel);
            }
            return Json(refModel);
        }

        public ActionResult SaveGeneralProfile(MyProfileModel user)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                var userRules = new UserRules();
                user.Id = CurrentUser().Id;
                refModel = userRules.SaveUser(user);

                if (refModel.result)
                {
                    SetUserSettingsCookie((ApplicationUser)refModel.Object);
                    refModel.Object = null;
                    // Reset Date store UI Settings My Desk
                    new MyDesksRules(dbContext).MyDeskResetDateStoreUiSetting("MyDesk", CurrentUser().Id);
                }
            }
            catch (Exception ex)
            {
                refModel.Object = ex;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveUserSettings(MyProfileModel user)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                user.PreferredDomain_Id = 0;
                if (!string.IsNullOrEmpty(user.PreferredDomain_Key))
                {
                    user.PreferredDomain_Id = Int32.Parse(EncryptionService.Decrypt(user.PreferredDomain_Key));
                }

                var userRules = new UserRules();
                user.Id = CurrentUser().Id;
                refModel = userRules.SaveUserSetting(user);

                if (refModel.result)
                {
                    SetUserSettingsCookie((ApplicationUser)refModel.Object);
                    refModel.Object = null;
                    // Reset Date store UI Settings My Desk
                    new MyDesksRules(dbContext).MyDeskResetDateStoreUiSetting("MyDesk", CurrentUser().Id);
                }
            }
            catch (Exception ex)
            {
                refModel.Object = ex;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveUserProfileWizard(MyProfileModel user)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                var userRules = new UserRules();
                user.Id = CurrentUser().Id;
                refModel = userRules.SaveUserWizardProfile(user);

                if (refModel.result)
                {
                    SetUserSettingsCookie((ApplicationUser)refModel.Object);
                    refModel.Object = null;
                    // Reset Date store UI Settings My Desk
                    new MyDesksRules(dbContext).MyDeskResetDateStoreUiSetting("MyDesk", user.Id);
                }
            }
            catch (Exception ex)
            {
                refModel.Object = ex;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AvatarUpload(string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                var media = new MediaModel
                {
                    Name = mediaObjectName,
                    UrlGuid = mediaObjectKey
                };
                var userRule = new UserRules(dbContext);
                var result = userRule.SaveAvatar(media, false, CurrentUser().Id);
                refModel.Object = media.UrlGuid;
                refModel.result = true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_MSG_131");
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
            }
            return Json(refModel);
        }

        [HttpPost]
        public async Task<ActionResult> SetAvatar(string userAvatar)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (!string.IsNullOrEmpty(userAvatar))
                {
                    var userRule = new UserRules(dbContext);
                    var avatar = userRule.CheckAvatarExist(Path.GetFileName(userAvatar));
                    if (avatar == null)
                    {
                        var fileName = Path.GetFileName(userAvatar);

                        var media = new MediaModel
                        {
                            Name = fileName,
                            UrlGuid = await UploadMediaFromPath(fileName, Server.MapPath(userAvatar))
                        };
                        var result = userRule.SaveAvatar(media, true, CurrentUser().Id);

                        refModel.Object = media.UrlGuid;
                    }
                    else
                    {
                        userRule.SetAvatar(CurrentUser().Id, avatar.URI);
                        refModel.Object = avatar.URI;
                        refModel.result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                refModel.result = true;
                refModel.msg = ResourcesManager._L("ERROR_MSG_131");
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
            }
            return Json(refModel);
        }

        [HttpPost]
        public ActionResult ChangePassword(string newpass)
        {
            var logRule = new QbicleLogRules(dbContext);
            QbicleLog log = new QbicleLog(QbicleLogType.PasswordReset, CurrentUser().Id);
            logRule.SaveQbicleLog(log);

            refModel = new ReturnJsonModel() { result = false };
            if (!string.IsNullOrEmpty(newpass))
                refModel.result = ResetPassProfile(CurrentUser().Id, newpass);
            else
            {
                refModel.msg = ResourcesManager._L("ERROR_MSG_387");
            }
            return Json(refModel);
        }

        public ActionResult SetPrivacyOptions(MyProfilePrivacyOptionsModel optionsModel)
        {
            refModel = new ReturnJsonModel() { result = false };
            refModel.result = new UserRules(dbContext).SetPrivacyOptions(CurrentUser().Id, optionsModel);
            return Json(refModel);
        }

        [HttpPost]
        public ActionResult SaveEmploymentHistory(EmploymentModel employment)
        {
            var user = new UserRules(dbContext).GetUser(CurrentUser().Id, 0);
            var userProfile = new UserProfilePage
            {
                AssociatedUser = user,
                ProfileText = user.Profile,
                StoredLogoName = user.ProfilePic,
                StoredFeaturedImageName = user.ProfilePic,
                StrapLine = user.DisplayUserName,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = user,
                PageType = CommunityPageTypeEnum.UserProfile
            };
            employment.User = user;
            employment.CurrentTimeZone = CurrentUser().Timezone;
            var result = new UserRules(dbContext).SaveEmloymentHistory(employment, userProfile);
            return Json(result);
        }

        public ActionResult SaleUserMyFile(MyfileUploadModal myfileUpload, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            refModel = new ReturnJsonModel() { result = false };

            var media = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };

            myfileUpload.media = media;
            refModel = new UserRules(dbContext).SaleUserMyFile(myfileUpload, CurrentUser().Id);

            return Json(refModel);
        }

        public ActionResult GetEmploymentHistory()
        {
            var data = new UserRules(dbContext).GetEmploymentsByUserId(CurrentUser().Id, CurrentUser().Timezone);
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserAddresses()
        {
            var userSetting = CurrentUser();
            var user = dbContext.QbicleUser.Find(userSetting.Id);
            var addressList = user.TraderAddresses;
            var data = new List<TraderAddressCustomModel>();
            foreach (var addr in addressList)
            {
                var customItem = new TraderAddressCustomModel()
                {
                    Email = addr.Email,
                    Key = addr.Key,
                    IsDefault = addr.IsDefault,
                    Latitude = addr.Latitude,
                    Longitude = addr.Longitude,
                    Phone = addr.Phone
                };

                var lstStringAddr = new List<string>();
                if (!String.IsNullOrEmpty(addr.AddressLine1))
                {
                    lstStringAddr.Add(addr.AddressLine1);
                }

                if (!String.IsNullOrEmpty(addr.AddressLine2))
                {
                    lstStringAddr.Add(addr.AddressLine2);
                }

                if (!String.IsNullOrEmpty(addr.City))
                {
                    lstStringAddr.Add(addr.City);
                }

                if (!String.IsNullOrEmpty(addr.State))
                {
                    lstStringAddr.Add(addr.State);
                }

                if (!String.IsNullOrEmpty(addr.Country?.CommonName))
                {
                    lstStringAddr.Add(addr.Country?.CommonName);
                }

                if (!String.IsNullOrEmpty(addr.PostCode))
                {
                    lstStringAddr.Add(addr.PostCode);
                }

                customItem.AddressFull = string.Join(", ", lstStringAddr);
                data.Add(customItem);
            }

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GeMyfiles()
        {
            var data = new UserRules(dbContext).GetMyFilesByUserId(CurrentUser().Id, CurrentUser().Timezone);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteEmploymentById(int id)
        {
            refModel = new ReturnJsonModel() { result = false };
            refModel.result = new UserRules(dbContext).DeleteEmploymentById(id);
            return Json(refModel);
        }

        [HttpPost]
        public ActionResult DeleteProFileById(int id)
        {
            refModel = new ReturnJsonModel() { result = false };
            refModel.result = new UserRules(dbContext).DeleteProFileById(id);
            return Json(refModel);
        }

        public ActionResult getEmploymentById(int id)
        {
            return Json(new UserRules(dbContext).GetEmploymentById(id, CurrentUser().Timezone), JsonRequestBehavior.AllowGet);
        }

        private bool IsValidPath(string path, bool exactPath = true)
        {
            var isValid = true;

            try
            {
                if (exactPath)
                {
                    var root = Path.GetPathRoot(path);
                    isValid = string.IsNullOrEmpty(root.Trim('\\', '/')) == false;
                }
                else
                {
                    isValid = Path.IsPathRooted(path);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                isValid = false;
            }

            return isValid;
        }

        public ActionResult ManageFormEditors()
        {
            try
            {
                var aList = new AccountRules(dbContext).ListAdministrator(CurrentDomain(), CurrentUser().Id)
                    .GroupBy(x => new { x.Id, x.Name, x.Avatar }).Select(y => new AdministratorViewModal
                    {
                        Id = y.Key.Id,
                        Name = y.Key.Name,
                        Levels = y.Select(z => new KeyValuePair<string, int>(z.Level,
                                z.Level == AdministratorViewModal.AccountOwner
                                    ? 1
                                    : (z.Level == AdministratorViewModal.AccountAdministrators
                                        ? 2
                                        : (z.Level == AdministratorViewModal.DomainAdministrator ? 3 : 999))))
                              .Distinct()
                              .ToList()
                    }).FirstOrDefault();
                var permissions = aList.Levels.ToDictionary(k => k.Key);

                if (permissions.ContainsKey(AdministratorViewModal.DomainAdministrator))
                {
                    var user = new UserRules().GetUser(CurrentUser().Id, 0);

                    ViewBag.Domains = user.Domains;

                    ViewBag.CurrentPage = "ManageFormEditors"; SetCurrentPage("ManageFormEditors");
                    ViewBag.PageTitle = "Manage Task Form Rights";
                    return View();
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ManageTaskForms()
        {
            try
            {
                ClearAllCurrentActivities();

                if (!new AppRightRules(dbContext).CheckTaskFormPermission(CurrentUser().Id, CurrentDomainId()))
                    return View("Error");

                ViewBag.Domains = CurrentDomain();

                ViewBag.CurrentPage = "ManageFormEditors"; SetCurrentPage("ManageTaskForms");
                ViewBag.PageTitle = "Manage Task Forms";
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult CreateTaskForm()
        {
            try
            {
                var domain = CurrentDomain();
                var isManageTaskForm =
                    new FormManagerRules(dbContext).IsManageTaskForm(CurrentUser().Id, domain);
                var isTaskFormEditor =
                    new FormManagerRules(dbContext).IsManageTaskForm(CurrentUser().Id, domain);
                var aList = new AccountRules(dbContext).ListAdministrator(domain, CurrentUser().Id)
                    .GroupBy(x => new { x.Id, x.Name, x.Avatar }).Select(y => new AdministratorViewModal
                    {
                        Id = y.Key.Id,
                        Name = y.Key.Name,
                        Levels = y.Select(z => new KeyValuePair<string, int>(z.Level,
                                z.Level == AdministratorViewModal.AccountOwner
                                    ? 1
                                    : (z.Level == AdministratorViewModal.AccountAdministrators
                                        ? 2
                                        : (z.Level == AdministratorViewModal.DomainAdministrator ? 3 : 999))))
                              .Distinct()
                              .ToList()
                    }).FirstOrDefault();

                ViewBag.CurrentDomain = domain;

                var permissions = aList.Levels.ToDictionary(k => k.Key);
                if (permissions.ContainsKey(AdministratorViewModal.DomainAdministrator) || isManageTaskForm ||
                    isTaskFormEditor)
                {
                    ViewBag.CurrentPage = "CreateTaskForm"; SetCurrentPage("CreateTaskForm");
                    ViewBag.PageTitle = "Create Task Form";
                    return View();
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult QueryTaskForm()
        {
            try
            {
                var domain = CurrentDomain();
                var userSetting = CurrentUser();
                var isQueryOrReport = new FormManagerRules(dbContext).isQueryOrReport(userSetting.Id, domain);
                var aList = new AccountRules(dbContext).ListAdministrator(domain, userSetting.Id)
                    .GroupBy(x => new { x.Id, x.Name, x.Avatar }).Select(y => new AdministratorViewModal
                    {
                        Id = y.Key.Id,
                        Name = y.Key.Name,
                        Levels = y.Select(z => new KeyValuePair<string, int>(z.Level,
                                z.Level == AdministratorViewModal.AccountOwner
                                    ? 1
                                    : (z.Level == AdministratorViewModal.AccountAdministrators
                                        ? 2
                                        : (z.Level == AdministratorViewModal.DomainAdministrator ? 3 : 999))))
                              .Distinct()
                              .ToList()
                    }).FirstOrDefault();
                var permissions = aList.Levels.ToDictionary(k => k.Key);

                if (permissions.ContainsKey(AdministratorViewModal.DomainAdministrator) || isQueryOrReport)
                {
                    ViewBag.Domains = dbContext.QbicleUser.Find(userSetting.Id)?.Domains;

                    ViewBag.CurrentPage = "QueryTaskForm"; SetCurrentPage("QueryTaskForm");
                    ViewBag.PageTitle = "Query Task Form";
                    return View();
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        [ChildActionOnly]
        public ActionResult GeneratesAdministrationPartial()
        {
            try
            {
                var domain = CurrentDomain();
                var isMemberOrOwnerDomain =
                    new DomainRules(dbContext).IsMemberOrOwnerDomain(CurrentUser().Id, domain);
                var isManageTaskForm =
                    new FormManagerRules(dbContext).IsManageTaskForm(CurrentUser().Id, domain);
                var isQueryOrReport = new FormManagerRules(dbContext).isQueryOrReport(CurrentUser().Id, domain);
                var aList = new AccountRules(dbContext).ListAdministrator(domain, CurrentUser().Id)
                    .GroupBy(x => new { x.Id, x.Name, x.Avatar }).Select(y => new AdministratorViewModal
                    {
                        Id = y.Key.Id,
                        Name = y.Key.Name,
                        Levels = y.Select(z => new KeyValuePair<string, int>(z.Level,
                                z.Level == AdministratorViewModal.AccountOwner
                                    ? 1
                                    : (z.Level == AdministratorViewModal.AccountAdministrators
                                        ? 2
                                        : (z.Level == AdministratorViewModal.DomainAdministrator ? 3 : 999))))
                              .Distinct()
                              .ToList()
                    });
                ViewBag.Administrator = aList.FirstOrDefault();

                ViewBag.isMemberOrOwnerDomain = isMemberOrOwnerDomain;
                ViewBag.isManageTaskFormRights = isManageTaskForm;
                ViewBag.isQueryOrReport = isQueryOrReport;
                return PartialView("_Administration");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ViewTaskForm(int id)
        {
            try
            {
                var domain = CurrentDomain();
                ViewBag.CurrentDomain = domain;
                ViewBag.CurrentPage = "ViewTaskForm"; SetCurrentPage("ViewTaskForm");
                var isMemberOrOwnerDomain =
                    new DomainRules(dbContext).IsOwnerOrDomainAdmin(CurrentUser().Id, domain);
                var isManageTaskFormRights =
                    new FormManagerRules(dbContext).IsManageTaskForm(CurrentUser().Id, domain);
                if (isMemberOrOwnerDomain || isManageTaskFormRights)
                {
                    var formDefinition = new FormDefinitionRules(dbContext).GetFormDefinitionById(id);
                    ViewBag.IsUsed = new TaskFormDefinitionRefRules(dbContext)
                        .CheckFormDefinitionIsExistsInTaskFormDefinitionRef(id);
                    return View(formDefinition);
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ProfileAdmin(string userId = "", string userType = "")
        {
            try
            {
                if (userId == "" || userType == "")
                    return View("Error");
                var domain = CurrentDomain();
                var userRule = new UserRules(dbContext);
                var userProfile = userRule.GetUser(userId, 0);
                if (userType == AdminLevel.Users.GetDescription() || userType == AdminLevel.Administrators.GetDescription())
                {
                    var domainAdmin = domain.Administrators.Any(u => u.Id == CurrentUser().Id);
                    ViewBag.domainAdmin = domainAdmin;
                    //check user profile as domain user/domain admin
                    var userPrivilege = domain.Administrators.Any(u => u.Id == userId);
                    ViewBag.userPrivilege = userPrivilege;
                }
                else
                {
                    ViewBag.currentDomainAdmin = null;
                    ViewBag.userPrivilege = null;
                    ViewBag.domainAdmin = null;
                }

                ViewBag.user = userProfile;
                ViewBag.CurrentPage = "ProfileAdmin"; SetCurrentPage("ProfileAdmin");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ChangeUserPrivilege(string userId, string privilegeType)
        {
            try
            {
                refModel = new ReturnJsonModel
                {
                    result = new UserRules(dbContext).ChangeUserPrivilege(userId, privilegeType, CurrentDomain())
                };
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult QueryTasksForm()
        {
            try
            {
                var domain = CurrentDomain();
                var isQueryOrReport = new FormManagerRules(dbContext).isQueryOrReport(CurrentUser().Id, domain);
                var aList = new AccountRules(dbContext).ListAdministrator(domain, CurrentUser().Id)
                    .GroupBy(x => new { x.Id, x.Name, x.Avatar }).Select(y => new AdministratorViewModal
                    {
                        Id = y.Key.Id,
                        Name = y.Key.Name,
                        Levels = y.Select(z => new KeyValuePair<string, int>(z.Level,
                                z.Level == AdministratorViewModal.AccountOwner
                                    ? 1
                                    : (z.Level == AdministratorViewModal.AccountAdministrators
                                        ? 2
                                        : (z.Level == AdministratorViewModal.DomainAdministrator ? 3 : 999))))
                              .Distinct()
                              .ToList()
                    }).FirstOrDefault();
                var permissions = aList.Levels.ToDictionary(k => k.Key);

                if (permissions.ContainsKey(AdministratorViewModal.DomainAdministrator) || isQueryOrReport)
                {
                    ViewBag.taskPrioritys = EnumModel.GetEnumValuesAndDescriptions<QbicleTask.TaskPriorityEnum>();
                    ViewBag.taskRepeats = EnumModel.GetEnumValuesAndDescriptions<QbicleTask.TaskRepeatEnum>();

                    ViewBag.CurrentPage = "QueryTaskForm"; SetCurrentPage("QueryTaskForm");
                    ViewBag.PageTitle = "Query Task Form";
                    return View();
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadLogFilter()
        {
            var dr = new DomainRules(dbContext);
            var users = dr.GetUser(CurrentUser().Id);
            ViewBag.ListDomains = users.Domains.OrderBy(d => d.Name).ToList();
            return PartialView("_LogsFilter");
        }

        [HttpPost]
        public ActionResult LoadLogs([Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderBy,
           string searchAll, string dateRangeLog, string sessionId, string domainKey, int appType, int action, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                var domainId = 0;
                if (!string.IsNullOrEmpty(domainKey.Trim()))
                {
                    domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
                }
                List<LogModel> lstResult = new QbicleLogRules(dbContext).GetLogs(searchAll, dateRangeLog, sessionId, domainId, appType, action, column, orderBy, start, length, ref totalRecord, CurrentUser().DateFormat, CurrentUser().DateTimeFormat);
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
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<LogModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public void ExportToCSV(string searchAll, string dateRangeLog, string sessionId, string domainKey, int appType, int act)
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey.Trim()))
            {
                domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
            }

            var sb = new StringBuilder();
            var totalRecord = 0;
            List<LogModel> lstResult = new QbicleLogRules(dbContext).GetLogs(searchAll, dateRangeLog, sessionId, domainId, appType, act, 0, "desc", 0, 0, ref totalRecord, CurrentUser().DateFormat, CurrentUser().DateTimeFormat);
            sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}", "Date/time", "Session ID", "Domain", "Qbicle", "User", "IP address", "App", "Action");
            sb.Append(Environment.NewLine);
            foreach (var item in lstResult)
            {
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}", item.StrCreatedDate, item.SessionID, item.Domain, item.Qbicle, item.User, item.IPAddress, item.App, item.Action);
                sb.Append(Environment.NewLine);
            }
            //Get Current Response
            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=Logs.CSV");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();
        }

        public ActionResult UpdateSystemUserRoles(string userId, string[] roles = null)
        {
            return Json(new DomainRolesRules(dbContext).UpdateSystemUserRoles(userId, roles), JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerificationNewEmail(string newEmailAddress)
        {
            try
            {
                var callbackUrl = Url.Action("UserProfile", "Administration", new { checkverify = true }, Request.Url.Scheme);
                var returnJson = new UserRules(dbContext).ChangeNewEmailAddress(CurrentUser().Id, newEmailAddress, callbackUrl);
                return Json(returnJson);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Json(new { result = false });
            }
        }

        public ActionResult CheckPinNewEmailAvailable(string pin)
        {
            return Json(new UserRules(dbContext).CheckPINNewEmail(pin, CurrentUser().Id));
        }

        public ActionResult LoadCheckVerify()
        {
            var userId = CurrentUser().Id;
            var tempEmailAddress = new UserRules(dbContext).GetPendingVerifyNewEmail(userId);
            return PartialView("_CheckVerify", tempEmailAddress);
        }

        public async Task<ActionResult> UpdateSubscriptionDetail()
        {
            var currentDomain = CurrentDomain();
            var domainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomain.Id && p.IsArchived == false);
            var subscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == domainPlan.Id);

            ViewBag.DomainPlan = domainPlan;
            ViewBag.CurrentDomain = currentDomain;
            ViewBag.DocRetrievalUrl = ConfigManager.ApiGetDocumentUri;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomain.Id);
            ViewBag.CurrencySettings = currencySettings;

            var paystackSubscriptionObj = await new PayStackRules(dbContext).GetPaystackSubscription(subscription?.PayStackSubscriptionCode ?? "");
            ViewBag.NextBillingDate = paystackSubscriptionObj?.next_payment_date ?? null;

            var lstBusinessDomainLevel = dbContext.BusinessDomainLevels.ToList();
            ViewBag.LstBusinessDomainLevel = lstBusinessDomainLevel;

            ViewBag.HasError = false;
            ViewBag.ErrorMessage = "";

            return View();
        }

        // Update Domain plan type
        public ActionResult ValidateOnChangingDomainPlanLevel(int newDomainLevelId)
        {
            var domainId = CurrentDomain().Id;
            var validationResult = new DomainRules(dbContext).ValidateChangingPlanLevel(domainId, newDomainLevelId);

            return Json(validationResult, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ChangeDomainPlanLevel(int newDomainLevelId)
        {
            var domainId = CurrentDomain().Id;
            var currentUserId = CurrentUser().Id;
            var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
            var changingResult = await new DomainRules(dbContext).ChangeDomainPlanLevel(domainId, newDomainLevelId, currentUserId, baseUrl);

            return Json(changingResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowChangeDomainPlanLevelErrorPopup(int newDomainLevelId)
        {
            var domainId = CurrentDomain().Id;
            var currentDomain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
            var newDomainLevel = dbContext.BusinessDomainLevels.FirstOrDefault(p => p.Id == newDomainLevelId);
            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
            ViewBag.CurrentDomainPlan = currentDomainPlan;
            ViewBag.NewDomainPlanLevel = newDomainLevel;
            ViewBag.CurrentDomain = currentDomain;
            return PartialView("_ChangeDomainPlanLevelErrorPopup");
        }

        public async Task<ActionResult> ShowChangeDomainPlanLevelConfirmationPopup(int newDomainLevelId)
        {
            var domainId = CurrentDomain().Id;
            var currentDomain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
            var newDomainLevel = dbContext.BusinessDomainLevels.FirstOrDefault(p => p.Id == newDomainLevelId);
            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
            var currentSubscriptionCode = dbContext.DomainSubscriptions
                .Where(p => p.Plan.Id == currentDomainPlan.Id).Select(p => p.PayStackSubscriptionCode).FirstOrDefault();

            if (!string.IsNullOrEmpty(currentSubscriptionCode))
            {
                await new PayStackRules(dbContext).GetPaystackSubscription(currentSubscriptionCode);
            }
            var currentSub = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == currentDomainPlan.Id);
            if (currentDomainPlan.Level.Level == BusinessDomainLevelEnum.Free && currentSub.NexPaymentDate < DateTime.UtcNow)
                currentSub.NexPaymentDate = DateTime.UtcNow;

            ViewBag.CurrentDomainPlan = currentDomainPlan;
            ViewBag.NewDomainPlanLevel = newDomainLevel;
            ViewBag.CurrentDomain = currentDomain;
            ViewBag.CurrentSubscription = currentSub;

            // Calculate the amount needed to pay for new domain plan level
            decimal amount = 0;
            amount += newDomainLevel.Cost ?? 0;
            amount += currentDomainPlan.NumberOfExtraUsers * (newDomainLevel.CostPerAdditionalUser ?? 0);
            ViewBag.NewDomainPlanCost = amount;
            return PartialView("_ChangeDomainPlanLevelConfirmationPopup");
        }

        // Product specific Tabs
        public ActionResult ShowBrandsTab()
        {
            var lstDomain = dbContext.Domains.ToList();
            ViewBag.ListDomains = lstDomain;
            ViewBag.ListBrands = dbContext.AdditionalInfos.AsNoTracking().Where(p => p.Type == AdditionalInfoType.Brand).OrderBy(e => e.Name).ToList();
            return PartialView("_ItemBrands");
        }

        public ActionResult ShowProductTagsTab()
        {
            var lstDomain = dbContext.Domains.ToList();
            ViewBag.ListDomains = lstDomain;
            ViewBag.ListProductTags = dbContext.AdditionalInfos.AsNoTracking().Where(p => p.Type == AdditionalInfoType.ProductTag).OrderBy(e => e.Name).ToList();
            return PartialView("_ItemProductTag");
        }

        public ActionResult GetAdditionalInforDTData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, AdditionalInfoType type, string keySearch = "", string domainKey = "")
        {
            var domainId = string.IsNullOrEmpty(domainKey) ? 0 : domainKey.Decrypt2Int();
            return Json(new TraderResourceRules(dbContext)
                .GetAdditionalInforDTData(requestModel, keySearch, domainId, type));
        }

        public ActionResult AdminAddItemBrand(int brandId, string name, int domainId)
        {
            var currentUserId = CurrentUser().Id;

            var additionalInfoItem = new AdditionalInfo()
            {
                Id = brandId,
                Domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId),
                Name = name,
                Type = AdditionalInfoType.Brand
            };

            var addOrUpdateResult = new TraderResourceRules(dbContext).SaveAdditionalInfo(additionalInfoItem, currentUserId);
            return Json(addOrUpdateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AdminAddItemProductTag(int productTagId, string name, int domainId)
        {
            var currentUserId = CurrentUser().Id;

            var additionalInfoItem = new AdditionalInfo()
            {
                Id = productTagId,
                Domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId),
                Name = name,
                Type = AdditionalInfoType.ProductTag
            };

            var addOrUpdateResult = new TraderResourceRules(dbContext).SaveAdditionalInfo(additionalInfoItem, currentUserId);
            return Json(addOrUpdateResult, JsonRequestBehavior.AllowGet);
        }

        // Community Feature Tabs
        public ActionResult ShowAddEditFeaturedProductModal(int productId)
        {
            // Get domain from catalog. There some catalog hasn't domain ID
            var lstDomainHasCatalog = dbContext.PosMenus.Where(p => p.Domain != null & !p.IsDeleted
                                    && p.SalesChannel == SalesChannelEnum.B2C
                                    && p.Type == CatalogType.Sales
                                    && p.IsPublished).Select(e => e.Domain).AsNoTracking().Distinct().OrderBy(e => e.Name).ToList();

            ViewBag.ListDomain = lstDomainHasCatalog;

            var productItem = dbContext.FeaturedProducts.FirstOrDefault(p => p.Id == productId);

            var lstCatalogs = new List<Catalog>();
            if (productItem != null)
            {
                var productDomainId = productItem.Domain.Id;
                lstCatalogs = dbContext.PosMenus.Where(p => (p.Domain.Id == productDomainId || p.Location.Domain.Id == productDomainId)
                                    && !p.IsDeleted
                                    && p.SalesChannel == SalesChannelEnum.B2C
                                    && p.Type == CatalogType.Sales
                                    && p.IsPublished).OrderBy(e => e.Name).ToList();
            }
            ViewBag.ListCatalog = lstCatalogs;

            return PartialView("_AddEditFeaturedProductModal", productItem);
        }

        public ActionResult ShowAddEditFeaturedStoreModal(int storeId)
        {
            var storeItem = dbContext.FeaturedStores.FirstOrDefault(p => p.Id == storeId);
            ViewBag.ListDomain = dbContext.Domains.OrderBy(e => e.Name).ToList();
            return PartialView("_AddEditFeaturedStoreModal", storeItem);
        }

        public ActionResult SaveFeaturedProduct(TraderItem item, int productId, string domainKey, int catalogId, string itemSKU)
        {
            var domainId = domainKey.Decrypt2Int();
            var saveResult = new AdminCommunityFeatureRules(dbContext).SaveFeaturedProduct(item, productId, domainId, catalogId, itemSKU);

            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveFeaturedImage(int imageId, string domainKey, string imageUrl, S3ObjectUploadModel uploadModel = null)
        {
            var domainId = domainKey.Decrypt2Int();
            var saveResult = new AdminCommunityFeatureRules(dbContext).SaveFeaturedImage(imageId, domainId, imageUrl, uploadModel, CurrentUser().Id);

            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SwapFeaturedProductOrder(int productId1, int productId2)
        {
            var updateResult = true;
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveFeaturedStore(int featuredStoredId, string domainKey)
        {
            var domainId = domainKey.Decrypt2Int();
            var saveReult = new AdminCommunityFeatureRules(dbContext).SaveFeaturedStore(featuredStoredId, domainId);
            return Json(saveReult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListCatalogByDomain(string domainKey)
        {
            var domainId = domainKey.Decrypt2Int();
            try
            {
                var lstCatalogs = dbContext.PosMenus
                    .Where(p => (p.Domain.Id == domainId || p.Location.Domain.Id == domainId)
                                    && !p.IsDeleted
                                    && p.SalesChannel == SalesChannelEnum.B2C
                                    && p.Type == CatalogType.Sales
                                    && p.IsPublished)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Text = p.Name
                    }).OrderBy(p => p.Text).ToList();
                return Json(lstCatalogs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<Catalog>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ShowProductItemFilteringModal(int catalogId, string keySearch = "")
        {
            ViewBag.CatalogId = catalogId;
            var catalog = dbContext.PosMenus.FirstOrDefault(p => p.Id == catalogId);
            ViewBag.ItemGroups = new TraderGroupRules(dbContext).GetTraderGroupItemOnly(catalog?.Domain?.Id ?? catalog.Location.Domain?.Id ?? 0);
            ViewBag.InitialKeySearch = keySearch;
            return PartialView("_TraderItemFilteringModal");
        }

        public ActionResult ShowProductItemBrandTagModal(int itemId)
        {
            var item = dbContext.TraderItems.FirstOrDefault(p => p.Id == itemId);

            ViewBag.AdditionalInfos = new TraderResourceRules(dbContext).GetListAdditionalInfos();
            return PartialView("_ProductItemBrandTagModal", item);
        }

        public ActionResult FilterTraderItemServerSide([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string search, int catalogId, bool nonInventory, int productGroupId)
        {
            var catalog = dbContext.PosMenus.FirstOrDefault(p => p.Id == catalogId);
            var locationId = catalog.Location?.Id ?? 0;
            var domainId = catalog.Domain?.Id ?? 0;
            //var result = new PosMenuRules(dbContext)
            //    .FindItemServerside(requestModel, catalog.Domain.Id, locationId, nonInventory, search, productGroupId);
            var result = new PosMenuRules(dbContext)
                .FindItemInCatalogServerside(requestModel, catalogId, domainId, locationId, nonInventory, search, productGroupId);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult FeaturedProductDTData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var result = new AdminCommunityFeatureRules(dbContext).GetFeaturedProductDTTable(requestModel);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult FeaturedStoreDTData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var result = new AdminCommunityFeatureRules(dbContext).GetFeaturedStoreDTTable(requestModel);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, null, 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletedFeaturedStore(int storeId)
        {
            var removeResult = new AdminCommunityFeatureRules(dbContext).DeleteFeaturedStore(storeId);
            return Json(removeResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFeaturedProduct(int productId)
        {
            var removeResult = new AdminCommunityFeatureRules(dbContext).DeleteFeaturedProduct(productId);
            return Json(removeResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateStoreDisplayOrder(List<FeaturedStoreDTItem> lstStores)
        {
            var updateResult = new AdminCommunityFeatureRules(dbContext).UpdateStoreDisplayOrder(lstStores);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateFeaturedProductDisplayOrder(List<FeaturedProductDTItem> lstProduct)
        {
            var updateResult = new AdminCommunityFeatureRules(dbContext).UpdateFeaturedProductDisplayOrder(lstProduct);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }
    }
}