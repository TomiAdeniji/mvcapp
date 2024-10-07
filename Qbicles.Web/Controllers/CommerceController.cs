using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.BusinessRules.Commerce;
using Qbicles.BusinessRules.BusinessRules.PayStack;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.ProfilePages;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Resources;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
	[Authorize]
	public class CommerceController : BaseController
	{
		// GET: Commerce
		public ActionResult Index()
		{
			var domainId = CurrentDomainId();
			if (domainId == 0)
				return Redirect("~/");
			var currentUserId = CurrentUser().Id;

			if (!CanAccessBusiness() && !new B2CRules(dbContext).CheckHasAccessB2B(domainId, currentUserId))
				return View("ErrorAccessPage");
			var relationships = new B2BRelationshipRules(dbContext).GetRelationships(domainId, "", currentUserId);
			ViewBag.Relationships = relationships;
			var b2bcurrentqbicleid = CurrentQbicleId();
			if (b2bcurrentqbicleid <= 0 || !relationships.Any(s => s.RelationshipHub.Id == b2bcurrentqbicleid || s.Partnerships.Any(q => q.CommunicationQbicle != null && q.CommunicationQbicle.Id == b2bcurrentqbicleid)))
			{
				b2bcurrentqbicleid = relationships.FirstOrDefault()?.RelationshipHub.Id ?? 0;
				SetCurrentQbicleIdCookies(b2bcurrentqbicleid);
				ViewBag.CurrentQbicleId = b2bcurrentqbicleid;
			}
			ViewBag.CurrentPage = "Commerce"; SetCurrentPage("Commerce");
			SetCurrentLocationManage();
			this.SetCreatorTheCustomer(SystemPageConst.B2B);
			return View();
		}

		public ActionResult Promotion(string key)
		{
			var promotion = new PromotionRules(dbContext).GetPromotionByKey(key);
			ViewBag.currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(promotion.LoyaltyPromotion.Domain.Id);
			if (promotion.LoyaltyPromotion.VoucherInfo.Type == VoucherType.ItemDiscount)
			{
				var iVoucher = promotion.LoyaltyPromotion.VoucherInfo as ItemDiscountVoucherInfo;
				ViewBag.Item = dbContext.TraderItems.FirstOrDefault(e => e.SKU == iVoucher.ItemSKU);
			}

			return View(promotion);
		}

		public ActionResult PromotionVoucches([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
			string key, string search = "", string status = "", string dates = "")
		{
			var result = new CommerceRules(dbContext).PromotionVoucches(requestModel, key, search, status, dates, CurrentUser());

			if (result != null)
				return Json(result, JsonRequestBehavior.AllowGet);
			else
				return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
		}

		public ActionResult DiscussionOrder(string disKey)
		{
			var disId = 0;
			if (!string.IsNullOrEmpty(disKey?.Trim()))
			{
				disId = disKey.Decrypt2Int();
			}
			var discussion = new DiscussionsRules(dbContext).GetDiscussionByB2BOrderById(disId);
			if (discussion == null)
				return View("Error");
			ValidateCurrentDomain(discussion.Qbicle.Domain, discussion.Qbicle.Id);

			SetCurrentDiscussionIdCookies(disId);
			ViewBag.CurrentPage = SystemPageConst.SOCIALPOSTDISCUSSION;
			SetCurrentPage(SystemPageConst.SOCIALPOSTDISCUSSION);
			var currentDomainId = CurrentDomainId();
			ViewBag.currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(discussion.TradeOrder.SellingDomain.Id);
			if (discussion.TradeOrder.SellingDomain.Id == currentDomainId)
			{
				this.SetCreatorTheCustomer(SystemPageConst.B2C);
				return View("_ProviderDiscussionOrder", discussion);
			}
			else
			{
				this.SetCreatorTheCustomer(SystemPageConst.C2C);
				ViewBag.ExchangeRate = new ExchangeRateRules(dbContext).GetExchangeRateByOrderId(discussion.TradeOrder.Id);
				return View("_ConsumerDiscussionOrder", discussion);
			}
		}

		public ActionResult BusinessProfile(string key = "")
		{
			this.SetCookieGoBackPage("BusinessProfile");
			SetCurrentLocationManage();
			var domainId = 0;
			if (string.IsNullOrEmpty(key))
				domainId = CurrentDomainId();
			else
				domainId = int.Parse(key.Decrypt());

			if (domainId == 0)
				return Redirect("~/");

			if (!CanAccessBusiness())
				return View("ErrorAccessPage");

			var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
			ViewBag.CurrentDomainPlan = currentDomainPlan;

			ViewBag.CurrentPage = "BusinessProfile";
			ViewBag.UserRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
			ViewBag.BusinessCategories = new AdminListingRules(dbContext).GetAllBusinessCategories() ?? new List<BusinessCategory>();
			var profile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId);
			return View(profile != null ? profile : new B2BProfile());
		}

		public ActionResult BusinessProfileTrading()
		{
			var domainId = CurrentDomainId();
			var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
			var currentDomainPlanLevel = currentDomainPlan?.Level?.Level ?? BusinessDomainLevelEnum.Free;

			var profile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId);
			if (profile == null)
				return Redirect("~/Commerce/BusinessProfile");
			this.SetCookieGoBackPage("BusinessProfile");
			SetCurrentLocationManage();
			if (domainId == 0)
				return Redirect("~/");

			var currentDomain = dbContext.Domains.Find(domainId);
			if (currentDomain == null)
			{
				return View("ErrorAccessPage");
			}

			if (!CanAccessBusiness())
				return View("ErrorAccessPage");

			ViewBag.CurrentDomainPlan = currentDomainPlan;

			ViewBag.CurrentPage = "BusinessProfile";
			ViewBag.UserRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);

			var storepointRules = new StorePointConversionRules(dbContext);
			var activePaymentConversion = storepointRules.GetActiveConversion(domainId, OrderToPointsConversionType.Payment);
			ViewBag.ActivePaymentConversion = activePaymentConversion;

			ViewBag.CurrencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

			var activeSystemSettings = storepointRules.GetActiveSystemSettings();
			ViewBag.ActiveSysSetting = activeSystemSettings;
			var creditProcessWorkGroup = dbContext.WorkGroups.Where(p => p.Domain.Id == domainId && p.Processes.Any(e => e.Name == TraderProcessName.CreditNotes)).ToList();
			ViewBag.CreditProcessWorkGroup = creditProcessWorkGroup;
			var domainLoyaltySetting = storepointRules.GetOrCreateDomainLoyaltySetting(domainId, CurrentUser().Id);
			ViewBag.DomainLoyaltySetting = domainLoyaltySetting;

			return View(profile);
		}

		public ActionResult B2BCatalogueDistributor(int catalogId, int relationshipId)
		{
			ViewBag.relationshipId = relationshipId;
			return View(new PosMenuRules(dbContext).GetById(catalogId));
		}

		public ActionResult SearchCatalogItemsNotTaxes(B2COrderItemsRequestModel request, string scatids)
		{
			request.bdomainId = 0;
			if (!string.IsNullOrEmpty(request.bdomainKey?.Trim()))
			{
				request.bdomainId = request.bdomainKey.Decrypt2Int();
			}

			request.CatIds = JsonConvert.DeserializeObject<List<int>>(scatids);
			return Json(new PosMenuRules(dbContext).LoadOrderMenuItemNotTaxes(request), JsonRequestBehavior.AllowGet);
		}

		public ActionResult B2BOrderItemContentShow(int itemId, string domainKey)
		{
			var domainId = 0;
			if (!string.IsNullOrEmpty(domainKey?.Trim()))
			{
				domainId = domainKey.Decrypt2Int();
			}

			var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
			ViewBag.CurrencySetting = currencySetting;

			var categoryItem = new PosMenuRules(dbContext).GetPosCategoryItemById(itemId);

			var isSale = categoryItem.Category.Menu.Type == CatalogType.Sales;
			if (!isSale)
			{
				var consumingDomain = new DomainRules(dbContext).GetDomainById(domainId);
				ViewBag.ConsumingBusinessName = consumingDomain.Id.BusinesProfile()?.BusinessName ?? consumingDomain.Name;
				ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItem(CurrentDomainId());
				return PartialView("_CatalogueItemContent", categoryItem);
			}

			ViewBag.isAllowAdd = false;
			ViewBag.IsShopping = false;
			return PartialView(@"~\Views\B2C\_B2COrderItemContent.cshtml", new PosMenuRules(dbContext).GetPosCategoryItemWithTaxesById(itemId));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult ImportingItemfromDistributorCatalog(CloneDistributorCatalogueItem model)
		{
			model.CurrentUserId = CurrentUser().Id;
			model.destinationDomainId = CurrentDomainId();
			return Json(new CommerceRules(dbContext).CloneTradingItemFromCatalogueItemId(model));
		}

		public ActionResult GetBusinessLocations([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
		{
			return Json(new TraderLocationRules(dbContext).GetBusinessLocations(requestModel, CurrentDomainId()), JsonRequestBehavior.AllowGet);
		}

		public ActionResult SetLocationIncludeInProfile(int locationId, bool isIncludeInProfile)
		{
			return Json(new TraderLocationRules(dbContext).SetLocationIncludeInProfile(CurrentDomainId(), locationId, isIncludeInProfile));
		}

		public ActionResult SetCatalogIncludeInProfile(int catId, bool isIncludeInProfile)
		{
			return Json(new PosMenuRules(dbContext).SetCatalogIncludeInProfile(CurrentDomainId(), catId, isIncludeInProfile));
		}

		public ActionResult SetDefaultAddress(int locationId)
		{
			return Json(new CommerceRules(dbContext).SetDefaultAddress(CurrentDomainId(), locationId));
		}

		public ActionResult DiscussionPartner(int rlid)
		{
			var b2BRelationshipRule = new B2BRelationshipRules(dbContext);
			var relationship = b2BRelationshipRule.GetRelationship(rlid);
			if (relationship == null)
				return View("NotFoundPage");
			var discussion = new DiscussionsRules(dbContext).GetDiscussionByB2BRelationshipId(rlid);
			ViewBag.Discussion = discussion;
			ValidateCurrentDomain(discussion.Qbicle.Domain, discussion.Qbicle.Id);

			SetCurrentDiscussionIdCookies(discussion.Id);
			SetCurrentQbicleIdCookies(discussion.Qbicle.Id);
			ViewBag.CurrentPage = SystemPageConst.SOCIALPOSTDISCUSSION;
			SetCurrentPage(SystemPageConst.SOCIALPOSTDISCUSSION);
			ViewBag.CurrentGoBackPage = SystemPageConst.COMMERCE;
			SetCookieGoBackPage(SystemPageConst.COMMERCE);
			var currentDomainId = CurrentDomainId();
			List<int> lids = new TraderLocationRules(dbContext).GetTraderLocation(currentDomainId).Select(s => s.Id).ToList();
			ViewBag.Catalogs = new PosMenuRules(dbContext).FiltersCatalog(lids, "", true, (int)SalesChannelEnum.B2B, (int)CatalogType.Sales);
			ViewBag.Accounts = new TraderCashBankRules(dbContext).GetTraderCashAccounts(currentDomainId, false); //dbContext.TraderCashAccounts.Where(p =>p.Domain.Id== b2cqbicle.Business.Id).ToList();
			var currentLogisticsPartnership = b2BRelationshipRule.CurrentLogisticsAgreement(rlid);
			ViewBag.CurrentLogisticsAgreement = currentLogisticsPartnership;
			ViewBag.ProviderLocations = currentLogisticsPartnership != null ? new TraderLocationRules(dbContext).GetTraderLocation(currentLogisticsPartnership.ProviderDomain.Id) : new List<TraderLocation>();
			ViewBag.PartnerDefaultLocation = new CommerceRules(dbContext).GetDefaultLocationOfDomain(relationship.Domain1.Id == currentDomainId ? relationship.Domain2.Id : relationship.Domain1.Id);
			ViewBag.ContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(currentDomainId);
			var workGroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(currentDomainId);
			ViewBag.Workgroups = workGroups.Where(s => s.Processes.Any(x => x.Name == TraderProcessesConst.Contact)).OrderBy(n => n.Name).ToList();
			return View(relationship);
		}

		public ActionResult SetContactForB2BRelationship(int relationshipId, int contactId, int groupId, int workgroupId)
		{
			return Json(new CommerceRules(dbContext).SetContactForB2BRelationship(relationshipId, CurrentDomainId(), contactId, groupId, workgroupId, CurrentUser().Id));
		}

		public ActionResult LoadPartnershipsContent(int rlid)
		{
			var b2BRelationshipRule = new B2BRelationshipRules(dbContext);
			var relationship = b2BRelationshipRule.GetRelationship(rlid);
			if (relationship == null)
				return View("NotFoundPage");
			ViewBag.CurrentLogisticsAgreement = b2BRelationshipRule.CurrentLogisticsAgreement(rlid);
			return PartialView("_LoadPartnershipsContent", relationship);
		}

		public ActionResult LoadLogisticsPartnershipContent(int pid)
		{
			var ruleRetopnship = new B2BRelationshipRules(dbContext);
			var logisticsPartnership = ruleRetopnship.GetLogisticsPartnership(pid);
			if (logisticsPartnership == null)
				return View("NotFoundPage");
			ViewBag.ConsumerLocations = new TraderLocationRules(dbContext).GetTraderLocation(logisticsPartnership.ConsumerDomain.Id);
			//ViewBag.ProviderLocations = traderLocationRules.GetTraderLocation(logisticsPartnership.ProviderDomain.Id);
			return PartialView("_LoadLogisticsPartnershipContent", logisticsPartnership);
		}

		public ActionResult ProviderPriceList(int pid)
		{
			var logisticsPartnership = new B2BRelationshipRules(dbContext).GetLogisticsPartnership(pid);
			if (logisticsPartnership == null)
				return View("NotFoundPage");
			return PartialView("_ProviderPriceList", logisticsPartnership);
		}

		public ActionResult LogisticsPartnershipModals(int pid)
		{
			var logisticsPartnership = new B2BRelationshipRules(dbContext).GetLogisticsPartnership(pid);
			if (logisticsPartnership == null)
				return null;
			ViewBag.ProviderLocations = new TraderLocationRules(dbContext).GetTraderLocation(logisticsPartnership.ProviderDomain.Id);
			return PartialView("_LogisticsPartnershipModals", logisticsPartnership);
		}

		public ActionResult ChargeListModal(int logisticsAgreementId)
		{
			var LogisticsAgreement = new B2BRelationshipRules(dbContext).GetLogisticsAgreement(logisticsAgreementId);
			if (LogisticsAgreement == null)
				return null;
			return PartialView("_ChargeListModal", LogisticsAgreement);
		}

		public ActionResult LogisticsPartnershipButton(int pid)
		{
			var logisticsPartnership = new B2BRelationshipRules(dbContext).GetLogisticsPartnership(pid);
			if (logisticsPartnership == null)
				return null;
			return PartialView("_LogisticsPartnershipButton", logisticsPartnership);
		}

		public ActionResult LoadActiveContent(int pid)
		{
			var logisticsPartnership = new B2BRelationshipRules(dbContext).GetLogisticsPartnership(pid);
			if (logisticsPartnership == null)
				return null;
			return PartialView("_LoadActiveContent", logisticsPartnership);
		}

		public ActionResult LoadArchiveContent(int pid)
		{
			var logisticsPartnership = new B2BRelationshipRules(dbContext).GetLogisticsPartnership(pid);
			if (logisticsPartnership == null)
				return null;
			return PartialView("_LoadArchiveContent", logisticsPartnership);
		}

		public ActionResult PublishBusinessProfile(int id, bool isDomainId = false)
		{
			B2BProfile profile;
			if (isDomainId)
				profile = new CommerceRules(dbContext).GetB2bProfileByDomainId(id);
			else
				profile = new CommerceRules(dbContext).GetB2bProfileById(id);

			//ShowCatalog - QBIC-4831
			ViewBag.IsShowCatalog = false;
			if (profile.IsDisplayedInB2CListings
				&& profile.DefaultB2CRelationshipManagers.Any()
				&& profile.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
				&& profile.BusinessCatalogues.Any()
				&& profile.BusinessCatalogues.Any(e => e.IsPublished)
				)
			{
				ViewBag.IsShowCatalog = true;
			}
			ViewBag.CurrentPage = "C2C";
			SetCurrentPage("C2C");
			ViewBag.IsPreview = true;//Enable Preview BlockTemplate
			ViewBag.BusinessPages = new BusinessPageRules(dbContext).GetBusinessPages(profile?.Domain?.Id ?? 0);
			ViewBag.CurrentUserId = CurrentUser().Id;
			return View(profile != null ? profile : new B2BProfile());
		}

		public ActionResult UpdateSocialLinks(List<B2BSocialLink> socialLinks)
		{
			return Json(new CommerceRules(dbContext).UpdateSocialLinks(socialLinks));
		}

		public ActionResult UpdateTags(List<string> tags, int profileId)
		{
			return Json(new CommerceRules(dbContext).UpdateTags(tags, profileId));
		}

		public ActionResult UpdateCategories(List<int> categories, int profileId)
		{
			return Json(new CommerceRules(dbContext).UpdateCategories(categories, profileId));
		}

		public ActionResult B2BProfilefriend(int id)
		{
			var currentDomainId = CurrentDomainId();
			var currentUserId = CurrentUser().Id;
			var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, currentDomainId);
			if (userRoleRights.All(r => r != RightPermissions.CommerceAccess)/*|| !new B2BWorkgroupRules(dbContext).GetCheckPermission(currentDomainId,currentUserId, B2bProcessesConst.Partnerships)*/)
				return View("ErrorAccessPage");
			var commerceRule = new CommerceRules(dbContext);
			var profileConsumer = commerceRule.GetB2bProfileById(id);
			var profileProvider = commerceRule.GetB2bProfileByDomainId(currentDomainId);
			var relationship = new B2BRelationshipRules(dbContext).GetRelationshipByDomainId(profileConsumer.Domain?.Id ?? 0, currentDomainId);
			var traderRule = new TraderLocationRules(dbContext);
			ViewBag.Relationship = relationship;
			ViewBag.ProviderConfig = commerceRule.GetB2BConfigByLocationId(profileConsumer.Domain?.Id ?? 0);
			ViewBag.LocationsProvider = traderRule.GetTraderLocation(profileConsumer.Domain?.Id ?? 0);
			ViewBag.LocationsConsumer = traderRule.GetTraderLocation(currentDomainId);
			ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItem(currentDomainId);
			ViewBag.ParnershipGroups = new TraderGroupRules(dbContext).GetTraderGroupItem(profileConsumer.Domain?.Id ?? 0);
			ViewBag.ProfileProvider = profileProvider;
			ViewBag.ShowMenuSellToOther = new B2BTradingItemRules(dbContext).CheckingSellToOther(relationship?.Id ?? 0, currentDomainId);
			return View(profileConsumer != null ? profileConsumer : new B2BProfile());
		}

		public ActionResult SaveProfileB2B(B2bProfileModel model)
		{
			model.Domain = CurrentDomain();
			return Json(new CommerceRules(dbContext).SaveProfile(model, CurrentUser().Id));
		}

		public ActionResult getLocationsOfCurrentDomain()
		{
			var locations = new TraderLocationRules(dbContext).GetTraderLocation(CurrentDomainId());
			return Json(locations.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }), JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateDefaultManagers(B2bProfileModel model)
		{
			return Json(new CommerceRules(dbContext).UpdateDefaultManagers(model, CurrentUser().Id));
		}

		public ActionResult LoadDefaultManagers(int profileId)
		{
			var profile = new CommerceRules(dbContext).GetB2bProfileById(profileId);
			if (profile == null)
				return Json(new { defaultb2bmanagers = new List<string>(), defaultb2cmanagers = new List<string>() }, JsonRequestBehavior.AllowGet);
			return Json(new
			{
				defaultb2bmanagers = profile.DefaultB2BRelationshipManagers.Select(s => s.Id).ToList(),
				defaultb2cmanagers = profile.DefaultB2CRelationshipManagers.Select(s => s.Id).ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		public ActionResult SavePostB2B(B2bPostModel model)
		{
			return Json(new CommerceRules(dbContext).SavePost(model, CurrentUser().Id));
		}

		public ActionResult GetPostById(int id)
		{
			return Json(new CommerceRules(dbContext).GetB2bPostById(id), JsonRequestBehavior.AllowGet);
		}

		public ActionResult DeletePostById(int id)
		{
			return Json(new CommerceRules(dbContext).DeleteB2bPostById(id));
		}

		public ActionResult LoadPostsContent(int profileId, bool isfeatured, string search)
		{
			ViewBag.IsFeatured = isfeatured;
			var posts = new CommerceRules(dbContext).GetB2BPosts(profileId, isfeatured, search);
			return PartialView("_LoadPostsContent", posts);
		}

		public ActionResult LoadRelationshipQbicles(string keyword)
		{
			var relationships = new B2BRelationshipRules(dbContext).GetRelationships(CurrentDomainId(), keyword, CurrentUser().Id);
			ViewBag.Relationships = relationships;
			var b2bcurrentqbicleid = CurrentQbicleId();
			if (b2bcurrentqbicleid <= 0 || !relationships.Any(s => s.RelationshipHub.Id == b2bcurrentqbicleid || s.Partnerships.Any(q => q.CommunicationQbicle?.Id == b2bcurrentqbicleid)))
			{
				b2bcurrentqbicleid = relationships.FirstOrDefault()?.RelationshipHub.Id ?? 0;
				SetCurrentQbicleIdCookies(b2bcurrentqbicleid);
				ViewBag.CurrentQbicleId = b2bcurrentqbicleid;
			}
			ViewBag.IsIndexRedirect = false;
			return PartialView("_LoadRelationshipQbicles");
		}

		public ActionResult LoadBusinessesContent(FindBusinesesRequest request)
		{
			request.currentDomainId = CurrentDomainId();
			return Json(new CommerceRules(dbContext).GetBusinesses(request), JsonRequestBehavior.AllowGet);
		}

		public ActionResult ConnectB2B(int partnerDomainId)
		{
			var rs = new B2BRelationshipRules(dbContext).CreateRelationship(CurrentDomainId(), partnerDomainId, CurrentUser().Id);
			if (rs.result)
				SetCurrentQbicleIdCookies((int)rs.Object);//set current B2b Qbicle
			return Json(rs, JsonRequestBehavior.AllowGet);
		}

		public ActionResult SettingsPartnership(int partnershipId, List<string> members, List<int> menus, int accountId, string type, bool status)
		{
			return Json(new B2BRelationshipRules(dbContext).SettingsPartnership(partnershipId, CurrentDomainId(), members, menus, accountId, type.ToLower(), status, CurrentUser().Id));
		}

		public ActionResult HaltAllPartnerships(int relationshipId)
		{
			return Json(new B2BRelationshipRules(dbContext).HaltAllPartnerships(relationshipId, CurrentDomainId(), false, CurrentUser().Id));
		}

		public ActionResult HaltPartnership(int partnershipId)
		{
			return Json(new B2BRelationshipRules(dbContext).HaltPartnership(partnershipId, CurrentDomainId(), false, CurrentUser().Id));
		}

		public ActionResult CheckLogisticsAgreement(int relationshipId, int partnershipId)
		{
			return Json(new B2BRelationshipRules(dbContext).CheckLogisticsAgreement(relationshipId, partnershipId));
		}

		public ActionResult UpdateLocationsPartnership(int partnershipId, List<int> locids)
		{
			return Json(new B2BRelationshipRules(dbContext).UpdateLocationsPartnership(partnershipId, CurrentDomainId(), locids, CurrentUser().Id));
		}

		public ActionResult AddPriceListLogisticsAgreement(int partnershipId, int priceListId)
		{
			return Json(new B2BRelationshipRules(dbContext).AddPriceListLogisticsAgreement(partnershipId, priceListId, CurrentUser().Id));
		}

		public ActionResult DeletePriceListLogisticsAgreement(int partnershipId)
		{
			return Json(new B2BRelationshipRules(dbContext).DeletePriceListLogisticsAgreement(partnershipId, CurrentUser().Id));
		}

		public ActionResult UpdateProviderChargeFrameworks(int partnershipId, List<B2BProviderChargeFramework> chargeFrameworks)
		{
			return Json(new B2BRelationshipRules(dbContext).UpdateProviderChargeFrameworks(partnershipId, chargeFrameworks, CurrentUser().Id));
		}

		public ActionResult GetPriceLists(int lid)
		{
			var prices = new B2BPricelistRules(dbContext).SearchPricelist("", lid);
			return Json(prices.Where(s => s.ChargeFrameworks.Any()).Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }), JsonRequestBehavior.AllowGet);
		}

		public ActionResult AgreeTerms(int partnershipId)
		{
			return Json(new B2BRelationshipRules(dbContext).AgreeTerms(partnershipId, CurrentDomainId(), CurrentUser().Id));
		}

		public ActionResult FinaliseAgreement(int partnershipId)
		{
			return Json(new B2BRelationshipRules(dbContext).FinaliseAgreement(partnershipId, CurrentDomainId(), CurrentUser().Id));
		}

		public ActionResult UpdateMembersRelationship(string key, List<string> members)
		{
			var qbicleId = int.Parse(key.Decrypt());
			return Json(new B2BRelationshipRules(dbContext).UpdateMembersRelationship(qbicleId, CurrentDomainId(), members, CurrentUser().Id));
		}

		public ActionResult LoadModalActivities(string qbicleKey)
		{
			var qbicleId = int.Parse(qbicleKey.Decrypt());
			var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
			ViewBag.qbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(qbicleId, CurrentUser().Id);
			var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
			ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);
			return PartialView("_ModalActivities", qbicle);
		}

		public JsonResult LoadMoreActivities(QbicleFillterModel fillterModel)
		{
			try
			{
				var user = CurrentUser();
				fillterModel.QbicleId = int.Parse(fillterModel.Key.Decrypt());
				if (CurrentQbicleId() != fillterModel.QbicleId)
				{
					SetCurrentQbicleIdCookies(fillterModel.QbicleId);//B2b Qbicle
					ViewBag.CurrentQbicleId = fillterModel.QbicleId;
				}
				var qbRule = new QbicleRules(dbContext);
				fillterModel.UserId = user.Id;
				var model = qbRule.GetQbicleStreams(fillterModel, user.Timezone, user.DateFormat);
				if (model != null)
				{
					//var modelString = RenderLoadNextViewToString("~/Views/Qbicles/_Dashboard.cshtml", model);

					var result = Json(new { ModelString = ActivityPostHtmlTemplateRules.getQbicleStreamsHtml(model, user.Id, user.Timezone, user.DateFormat), ModelCount = model.TotalCount },
						JsonRequestBehavior.AllowGet);
					result.MaxJsonLength = int.MaxValue;
					return result;
				}

				return Json(model);
			}
			catch (Exception ex)
			{
				LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
				return null;
			}
		}

		public ActionResult GetPricelistByLocationId(int lid)
		{
			var list = new B2BPricelistRules(dbContext).SearchPricelist("", lid);
			return Json(list.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }), JsonRequestBehavior.AllowGet);
		}

		public ActionResult AddCatalogueItem(B2BCatalogItem model, string tradingName, bool IsShown)
		{
			model.Domain = CurrentDomain();
			return Json(new B2BCatalogueItemRules(dbContext).AddCatalogueItem(model, tradingName, IsShown, CurrentUser().Id));
		}

		public ActionResult Select2TraderItemsByDomainId(int page, string keyword, int domainId = 0, bool isSell = true)
		{
			var result = new TraderItemRules(dbContext).Select2TraderItemsByLocationId(domainId == 0 ? CurrentDomainId() : domainId, keyword, 0, isSell);
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Select2GroupsByDomainId()
		{
			var groups = new TraderGroupRules(dbContext).GetTraderGroupItem(CurrentDomainId());
			var selectGroups = groups != null && groups.Any() ? groups.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }).ToList() : null;
			return Json(selectGroups, JsonRequestBehavior.AllowGet);
		}

		public ActionResult ItemSelectedById(int id)
		{
			var result = new TraderItemRules(dbContext).ItemSelectedById(id);
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetCatalogueItems([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int itemgroupId)
		{
			return Json(new B2BCatalogueItemRules(dbContext).GetCatalogueItems(requestModel, CurrentDomainId(), keyword, itemgroupId), JsonRequestBehavior.AllowGet);
		}

		//public ActionResult RemoveCatalogueItemById(int id)
		//{
		//    var currentDomainId = CurrentDomainId();
		//    var currentUserId = CurrentUser().Id;
		//    var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, currentDomainId);
		//    if (userRoleRights.All(r => r != RightPermissions.CommerceAccess))
		//        return Json(new ReturnJsonModel { result = false, msg = "ERROR_MSG_28" });
		//    return Json(new B2BCatalogueItemRules(dbContext).RemoveCatalogueItemById(id));
		//}
		public ActionResult GetTradingItems([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int relationshipId, string keyword, string groupIds, int status)
		{
			var groups = string.IsNullOrEmpty(groupIds) ? null : JsonConvert.DeserializeObject<List<int>>(groupIds);
			return Json(new B2BTradingItemRules(dbContext).GetTradingItems(requestModel, relationshipId, keyword, groups, status, CurrentDomainId()), JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateTradingName(int id, string tradingName)
		{
			var currentDomainId = CurrentDomainId();
			var currentUserId = CurrentUser().Id;
			var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, currentDomainId);
			if (userRoleRights.All(r => r != RightPermissions.CommerceAccess))
				return Json(new ReturnJsonModel { result = false, msg = "ERROR_MSG_28" });
			return Json(new B2BTradingItemRules(dbContext).UpdateTradingName(id, tradingName));
		}

		public ActionResult UpdateTradingItemStatus(int id, bool isShown)
		{
			var currentDomainId = CurrentDomainId();
			var currentUserId = CurrentUser().Id;
			var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, currentDomainId);
			if (userRoleRights.All(r => r != RightPermissions.CommerceAccess))
				return Json(new ReturnJsonModel { result = false, msg = "ERROR_MSG_28" });
			return Json(new B2BTradingItemRules(dbContext).UpdateTradingItemStatus(id, isShown));
		}

		public ActionResult PublishCatalogue(int relationshipId, bool isPublish)
		{
			var currentDomainId = CurrentDomainId();
			var currentUserId = CurrentUser().Id;
			var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, currentDomainId);
			if (userRoleRights.All(r => r != RightPermissions.CommerceAccess))
				return Json(new ReturnJsonModel { result = false, msg = "ERROR_MSG_28" });
			return Json(new B2BRelationshipRules(dbContext).PublishCatalogue(relationshipId/*,isPublish*/, currentUserId));
		}

		public ActionResult GetTradingItemsPartnership(int relationshipId, string keyword, List<int> groupIds, int domainParnershipId, string orderByString)
		{
			return PartialView("_TradingItemPartnershipContent", new B2BTradingItemRules(dbContext).GetTradingItemsPartnership(relationshipId, keyword, groupIds, domainParnershipId, orderByString));
		}

		public ActionResult LoadProductMoreModal(int tradingItemId)
		{
			var currentDomainId = CurrentDomainId();
			ViewBag.profileProvider = new CommerceRules(dbContext).GetB2bProfileByDomainId(currentDomainId);
			ViewBag.locations = new TraderLocationRules(dbContext).GetTraderLocation(currentDomainId);
			return PartialView("_ModalProductMore", new B2BTradingItemRules(dbContext).GetTradingItemById(tradingItemId));
		}

		public ActionResult SaveLinkConsumerItem(B2bLinkConsumerItem linkConsumerItem)
		{
			var currentDomainId = CurrentDomainId();
			var currentUserId = CurrentUser().Id;
			var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, currentDomainId);
			if (userRoleRights.All(r => r != RightPermissions.CommerceAccess))
				return Json(new ReturnJsonModel { result = false, msg = "ERROR_MSG_28" });
			return Json(new B2BTradingItemRules(dbContext).SaveLinkConsumerItem(linkConsumerItem));
		}

		public ActionResult GetTradingItemsOfRelationship([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int relationshipId, string keyword, int isLinked)
		{
			return Json(new B2BTradingItemRules(dbContext).GetTradingItemsOfRelationship(requestModel, relationshipId, keyword, isLinked), JsonRequestBehavior.AllowGet);
		}

		public ActionResult LoadBusinessProfileTab(string tab, bool reload = false)
		{
			try
			{
				var domainId = CurrentDomainId();
				var userId = CurrentUser().Id;
				var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, domainId);
				ViewBag.UserRoleRights = userRoleRights;
				var workGroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(domainId);
				var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
				switch (tab)
				{
					//case "settings":
					//    return PartialView("_TraderSettingsConfig", new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId));
					case "general-workgroups":
						ViewBag.CurrentDomainPlan = currentDomainPlan;
						if (!reload)
						{
							ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
							ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
							ViewBag.Process = new TraderProcessRules(dbContext).GetAll();
							ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
							ViewBag.DomainRoles = new DomainRolesRules(dbContext).GetDomainRolesDomainId(domainId);
							return PartialView("~/Views/Commerce/TradingSettings/_WorkgroupsConfigContent.cshtml", workGroups);
						}
						return PartialView("~/Views/Commerce/TradingSettings/_TblWorkgroupContent.cshtml", workGroups);

					case "general-locations":
						ViewBag.CurrentDomainPlan = currentDomainPlan;
						var location = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
						return PartialView("~/Views/Commerce/TradingSettings/_LocationConfigContent.cshtml", location);

					case "general-candb":
						ViewBag.CurrentDomainPlan = currentDomainPlan;
						ViewBag.WorkgroupPayment = workGroups.Where(q => q.Processes.Any(p => p.Name == TraderProcessName.TraderPaymentProcessName)
																	&& q.Members.Select(u => u.Id).Contains(userId)).OrderBy(n => n.Name).ToList();
						ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
						ViewBag.CurrentDomainId = domainId;
						return PartialView("~/Views/Commerce/TradingSettings/_CashBankConfigContent.cshtml");

					case "general-currency":
						var taxRates = new TaxRateRules(dbContext).GetTaxRateByDomainId(domainId).ToList();
						if (!reload)
						{
							ViewBag.Taxrates = taxRates;
							return PartialView("~/Views/Commerce/TradingSettings/_TaxesCurrencyConfigContent.cshtml", new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId));
						}
						return PartialView("~/Views/Commerce/TradingSettings/_TblTaxratesContent.cshtml", taxRates);

					case "tab-groups-items":
						ViewBag.LstGroupItems = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
						return PartialView("~/Views/Commerce/TradingSettings/_GroupProductConfigContent.cshtml");

					case "tab-groups-contacts":
						ViewBag.LstGroupItems = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
						ViewBag.LstContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(domainId);
						return PartialView("~/Views/Commerce/TradingSettings/_GroupContactConfigContent.cshtml");

					case "general-groups":
						ViewBag.CurrentDomainPlan = currentDomainPlan;
						ViewBag.LstGroupItems = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
						return PartialView("~/Views/Commerce/TradingSettings/_GroupsConfigContent.cshtml");

					case "items":
						ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
						ViewBag.LstGroupItems = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
						return PartialView("~/Views/Commerce/TradingSettings/_TradingItemContent.cshtml");

					case "item-resources":
						return PartialView("~/Views/Commerce/TradingSettings/_TradingItemResources.cshtml");

					case "general-contacts":
						ViewBag.CurrentDomainPlan = currentDomainPlan;
						ViewBag.WorkGroups = workGroups;
						var contactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(domainId);
						ViewBag.ContactGroups = contactGroups;
						ViewBag.ContactGroupFilter = contactGroups;
						ViewBag.Countries = new CountriesRules().GetAllCountries();
						return PartialView("~/Views/Commerce/TradingSettings/_ContactConfigContent.cshtml");

					case "items-pricing":
						var groupRule = new TraderGroupRules(dbContext);
						MasterSetupModel masterSetup = new MasterSetupModel();
						masterSetup.MasterSetups = groupRule.GetMasterSetup(domainId);
						masterSetup.TraderGroups = groupRule.GetTraderGroupItem(domainId);
						//var queryTaxRates = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId());
						//masterSetup.PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).ToList();
						//masterSetup.SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).ToList();
						ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
						ViewBag.SalesChannels = Enum.GetValues(typeof(SalesChannelEnum)).Cast<SalesChannelEnum>().ToList();
						return PartialView("~/Views/Commerce/TradingSettings/_TradingPriceContent.cshtml", masterSetup);

					case "items-catalogues":
					case "items-catalogues-distribution":
						if (tab == "items-catalogues-distribution")
							ViewBag.IsDistribution = true;
						else
							ViewBag.IsDistribution = false;
						ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
						var dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domainId);
						ViewBag.Dimensions = dimensions == null ? new List<TransactionDimension>() : dimensions;

						return PartialView("~/Views/Commerce/TradingSettings/_SalesCatalogContent.cshtml");

					case "general-order-defaults":
						ViewBag.CurrentDomainPlan = currentDomainPlan;
						var commerceRule = new CommerceRules(dbContext);
						ViewBag.B2BOrderSetting = commerceRule.GetB2BOrderSettingDefault(domainId, userId);
						ViewBag.B2COrderSetting = commerceRule.GetB2COrderSettingDefault(domainId, userId);
						ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
						return PartialView(@"~/Views/Commerce/TradingSettings/_OrderDefaults.cshtml");

					case "items-import":
						ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
						ViewBag.ListFileType = string.Join(",.", new FileTypeRules(dbContext).GetExtension("Excel File"));
						ViewBag.Template = ConfigManager.ItemProductImportTemplate.ToDocumentUri(Enums.FileTypeEnum.Document);
						return PartialView(@"~/Views/TraderItemImport/_ItemsImport.cshtml");
				}
			}
			catch (Exception e)
			{
				LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), e, CurrentUser().Id);
				return null;
			}
			return null;
		}

		public ActionResult LoadTradingItemAdd(string type, bool isProduct)
		{
			var domainId = CurrentDomainId();
			ViewBag.TradingType = type;
			ViewBag.IsProduct = isProduct;
			var queryTaxRates = new TaxRateRules(dbContext).GetByDomainId(domainId);
			ViewBag.PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).ToList();
			ViewBag.SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).ToList();
			ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
			ViewBag.TraderItems = new TraderItemRules(dbContext).GetAll(domainId);
			ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);

			var lstProductTags = dbContext.AdditionalInfos.AsNoTracking().Where(p => p.Type == AdditionalInfoType.ProductTag || p.Type == AdditionalInfoType.Brand).ToList();

			ViewBag.ListProductTags = lstProductTags.Where(p => p.Type == AdditionalInfoType.ProductTag).OrderBy(e => e.Name).ToList();

			ViewBag.ListBrands = lstProductTags.Where(p => p.Type == AdditionalInfoType.Brand).OrderBy(e => e.Name).ToList();
			return PartialView("~/Views/Commerce/TradingSettings/_AddTradingItemType.cshtml");
		}

		public ActionResult LoadGroupConfigTab(int groupid)
		{
			var domainId = CurrentDomainId();
			var grouprule = new TraderGroupRules(dbContext);
			TabGroupConfigModel model = new TabGroupConfigModel();
			model.TraderGroup = grouprule.GetById(groupid);
			model.MasterSetup = grouprule.GetMasterSetupByGroupId(groupid, domainId);
			if (model.MasterSetup == null)
			{
				grouprule.SaveMasterSetup(groupid, domainId, CurrentUser().Id);
			}

			model.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
			model.SalesChannels = Enum.GetValues(typeof(SalesChannelEnum)).Cast<SalesChannelEnum>().ToList();
			return PartialView("~/Views/Commerce/TradingSettings/_PricingGroupsContent.cshtml", model);
		}

		public ActionResult LoadPartnerCatalogs(string domainKey, int relationshipId)
		{
			var domainId = 0;
			if (!string.IsNullOrEmpty(domainKey?.Trim()))
			{
				domainId = domainKey.Decrypt2Int();
			}
			List<Catalog> catalogs = new PosMenuRules(dbContext).GetCatalogsByDomainId(domainId, SalesChannelEnum.B2B).Where(e => e.Type == CatalogType.Distribution).ToList();
			var relationship = new B2BRelationshipRules(dbContext).GetRelationship(relationshipId);
			if (relationship == null)
			{
				catalogs = null;
			}
			else
			{
				var partnerDomain = relationship.Partnerships.Where(s => s.Type == B2BService.Products && s.ProviderDomain.Id == domainId).FirstOrDefault() as PurchaseSalesPartnership;
				var listSalesCatalogs = partnerDomain.Catalogs;
				if (listSalesCatalogs.Count() > 0)
				{
					catalogs.AddRange(listSalesCatalogs);
				}
			}

			return PartialView("_CatalogueItemForB2B", catalogs);
		}

		public string RenderLoadNextViewToString(string viewName, object model)
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

		public ActionResult GetArchivedPromotions([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int type)
		{
			var userinfo = CurrentUser();
			PromotionFilterModel filterModel = new PromotionFilterModel();
			filterModel.keyword = keyword;
			filterModel.daterange = daterange;
			filterModel.type = type;
			filterModel.dateformat = userinfo.DateFormat;
			filterModel.timezone = userinfo.Timezone;
			filterModel.currentDomainId = CurrentDomainId();
			return Json(new PromotionRules(dbContext).GetArchivedPromotions(requestModel, filterModel), JsonRequestBehavior.AllowGet);
		}

		public ActionResult LoadActivePromotions(PromotionFilterModel filterModel)
		{
			var userinfo = CurrentUser();
			filterModel.dateformat = userinfo.DateFormat;
			filterModel.timezone = userinfo.Timezone;
			filterModel.currentDomainId = CurrentDomainId();
			var promotions = new PromotionRules(dbContext).GetActivePromotions(filterModel);
			return PartialView("~/Views/Commerce/TradingSettings/_ActivePromotionsContent.cshtml", promotions);
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
			var viewModel = new PromotionRules(dbContext).GetPromotionByKey(promotionKey);
			viewModel.TraderGeoLocations = new TraderLocationRules(dbContext).GetTraderGeoLocation(CurrentDomainId());
			return PartialView("~/Views/Commerce/TradingSettings/_ModalPromotionAddEdit.cshtml", viewModel);
		}

		public ActionResult StopStartPromotion(string promotionKey, bool isStop, string message)
		{
			return Json(new PromotionRules(dbContext).StopStartPromotion(promotionKey, isStop, message, CurrentUser().Id, CurrentDomainId()));
		}

		public ActionResult ArchivePromotion(string promotionKey)
		{
			return Json(new PromotionRules(dbContext).ArchivePromotion(promotionKey));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <param name="itemDiscountVoucherInfo"></param>
		/// <param name="orderDiscountVoucherInfo"></param>
		/// <param name="featuredImageUri"></param>
		/// <returns></returns>
		public async Task<ActionResult> SavePromotion(PromotionModel model, ItemDiscountVoucherInfo itemDiscountVoucherInfo, OrderDiscountVoucherInfo orderDiscountVoucherInfo, string featuredImageUri)
		{
			var userinfo = CurrentUser();
			model.Timezone = userinfo.Timezone;
			model.DateFormat = userinfo.DateTimeFormat;
			model.CurrentUserId = userinfo.Id;
			model.CurrentDomainId = CurrentDomainId();
            return Json(await new PromotionRules(dbContext).SavePromotion(model, itemDiscountVoucherInfo, orderDiscountVoucherInfo, featuredImageUri), JsonRequestBehavior.AllowGet);					
		}


        /// <summary>
		/// 
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="status"></param>
		/// <returns></returns>
        public async Task<ActionResult> UpdatePromotionPayment(string reference, string status)
        {
            return Json(await new PromotionRules(dbContext).UpdatePromotionPayment(reference, status), JsonRequestBehavior.AllowGet);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="promotionKey"></param>
		/// <returns></returns>
        public async Task<ActionResult> RetryPromotionPayment(string promotionKey)
        {
            var userinfo = CurrentUser();
            return Json(await new PromotionRules(dbContext).RetryPromotionPayment(promotionKey, userinfo.Id), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="itemDiscountVoucherInfo"></param>
        /// <param name="orderDiscountVoucherInfo"></param>
        /// <param name="featuredImageUri"></param>
        /// <returns></returns>
        public async Task<ActionResult> SavePromotionNew(PromotionModel model, ItemDiscountVoucherInfo itemDiscountVoucherInfo, OrderDiscountVoucherInfo orderDiscountVoucherInfo, string featuredImageUri)
        {
            var userinfo = CurrentUser();
            model.Timezone = userinfo.Timezone;
            model.DateFormat = userinfo.DateTimeFormat;
            model.CurrentUserId = userinfo.Id;
            model.CurrentDomainId = CurrentDomainId();

            //TODO:
            //Save promotion to DB and do not activate promotion
            //Process payment 
            //Confirm payment 

            var res = new PromotionRules(dbContext).SavePromotion(model, itemDiscountVoucherInfo, orderDiscountVoucherInfo, featuredImageUri);

            //Skip Payment if promotion is a draft or a free promo
            if (!model.IsDraft && model.PlanType.Id > 1)
            {
                var currentType = await dbContext.PromotionTypes.FindAsync(model.PlanType.Id);
                var paystackResponse = await new PayStackRules(dbContext).InitializeTransaction(userinfo, (int)currentType.Price);

                if (!string.IsNullOrEmpty(paystackResponse))
                {
                    //res.msgName = paystackResponse;
                }
            }

            return Json(res);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sku"></param>
        /// <returns></returns>
        public ActionResult LoadModalFindTraderItem(string sku)
		{
			//var userinfo = CurrentUser();
			var currentDomainId = CurrentDomainId();
			ViewBag.ItemGroups = new TraderGroupRules(dbContext).GetTraderGroupItem(currentDomainId);
			ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(currentDomainId);
			ViewBag.SKU = sku;
			return PartialView("~/Views/Commerce/TradingSettings/_FindTraderItemContent.cshtml");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="endDate"></param>
		/// <param name="startDate"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult GetCalculateDatesForPromotions(string endDate, string startDate)
		{
			try
			{
				string dayofweek = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";
				string formatDate = CurrentUser().DateTimeFormat;
				var dtLastOccurenceDate =
					DateTime.ParseExact(endDate, formatDate, CultureInfo.InvariantCulture);
				var dtCalculator =
					DateTime.ParseExact(startDate, formatDate, CultureInfo.InvariantCulture);
				var lstDay = Utility.GetListDayToTable(dtCalculator, dayofweek, dtLastOccurenceDate);
				return Json(lstDay.Where(s => s.StartDate <= dtLastOccurenceDate).ToList(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
				return null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="partnershipKey"></param>
		/// <returns></returns>
		public JsonResult InitDataB2bForCreateOrder(string partnershipKey)
		{
			var partnershipId = 0;
			if (!string.IsNullOrEmpty(partnershipKey?.Trim()))
			{
				partnershipId = partnershipKey.Decrypt2Int();
			}
			var parnership = new B2BPartnershipRules(dbContext).GetPartnershipById(partnershipId);
			var traderReference = new TraderReferenceRules(dbContext).GetNewReference(parnership?.ProviderDomain.Id ?? 0, TraderReferenceType.Order);
			var response = new
			{
				reference = new { id = traderReference?.Id ?? 0, orderref = traderReference?.FullRef },
				catalogs = (parnership?.Catalogs.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }).ToList() ?? null)
			};
			return Json(response, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Create an order from B2B
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public ActionResult SaveDiscussionForOrderCreation(B2BOrderCreationDiscussionModel model)
		{
			model.CurrentUserId = CurrentUser().Id;
			if (!string.IsNullOrEmpty(model.Partnershipkey?.Trim()))
			{
				model.PartnershipId = model.Partnershipkey.Decrypt2Int();
			}
			return Json(new DiscussionsRules(dbContext).SaveB2BDiscussionForOrderCreation(model, GetOriginatingConnectionIdFromCookies()));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="scatids"></param>
		/// <returns></returns>
		public ActionResult SearchB2bOrderItems(B2COrderItemsRequestModel request, string scatids)
		{
			request.bdomainId = 0;
			if (!string.IsNullOrEmpty(request.bdomainKey))
			{
				request.bdomainId = request.bdomainKey.Decrypt2Int();
			}

			request.CatIds = JsonConvert.DeserializeObject<List<int>>(scatids);
			return Json(new PosMenuRules(dbContext).LoadOrderMenuItem(request), JsonRequestBehavior.AllowGet);
		}

		public ActionResult LoadProductMoreContent(int itemId, string orderKey)
		{
			var orderId = 0;
			if (!string.IsNullOrEmpty(orderKey))
			{
				orderId = orderKey.Decrypt2Int();
			}
			var tradeOrder = new B2CRules(dbContext).GetTradeOrderById(orderId);
			var isAllowAdd = true;
			if (tradeOrder != null)
				isAllowAdd = tradeOrder.IsAgreedByCustomer && tradeOrder.IsAgreedByBusiness ? false : true;
			ViewBag.isAllowAdd = isAllowAdd;
			ViewBag.CurrencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(tradeOrder.SellingDomain.Id);
			ViewBag.SellingBusinessName = tradeOrder.SellingDomain.Id.BusinesProfile()?.BusinessName ?? tradeOrder.SellingDomain.Name;
			ViewBag.ConsumerBusinessName = tradeOrder.BuyingDomain.Id.BusinesProfile()?.BusinessName ?? tradeOrder.BuyingDomain.Name;

			var catalogItem = new PosMenuRules(dbContext).GetPosCategoryItemWithTaxesById(itemId);

			// B2B QBIC-2723: The call to the following method is looking for the default TraderItem in the SellingDomain
			// The purpose of the Default TraderItem is to enable the correct ReaderItem in the CONSUMER Domain to be associated with the Variant
			// So, instead of checking in the 'SellingDomain' the method should check in the 'tradeOrder.BuyingDomain'

			var itemSku = catalogItem.PosVariants?.FirstOrDefault(s => s.IsDefault)?.TraderItem.SKU ?? "";
			var itemBarcode = catalogItem.PosVariants?.FirstOrDefault(s => s.IsDefault)?.TraderItem.Barcode ?? "";

			ViewBag.ExchangeRate = new ExchangeRateRules(dbContext).GetExchangeRateByOrderId(tradeOrder.Id);

			// QBIC-3827: The buyers item should be found by checking if there is a TraderItem in the buyer’s Domain that has the same SKU as the item selected from the seller’s catalog.
			// If the item is not in the buyer’s Domain so nothing should be shown.
			//var itemAssociated = new CommerceRules(dbContext).GetDefaultAssociatedTraderItem(tradeOrder.SellingDomain.Id, tradeOrder.BuyingDomain.Id, itemSku);

			//QBIC-4283: The buyers item should be found by checking if there is a TradeItem in the buyer's Domain that has the same BARCODE (if not, it has the same SKU) as the item selected from the seller's catalog
			var itemAssociated = new CommerceRules(dbContext).FindAssociatedTraderItem(tradeOrder.SellingDomain.Id, tradeOrder.BuyingDomain.Id, itemBarcode, itemSku);

			ViewBag.DefaultAssociatedTraderItem = itemAssociated;

			//QBIC-4283: View from Buyers and Sellers
			if (CurrentDomainId() == tradeOrder.SellingDomain.Id)
			{
				return PartialView("_B2BSellerAddItem", catalogItem);
			}
			return PartialView("_ProductMoreB2border", catalogItem);
		}

		public ActionResult B2BMatchSellerAndPurchaserTaxes(int traderItemId, int variantId)
		{
			return Json(new CommerceRules(dbContext).B2BMatchSellerAndPurchaserTaxes(traderItemId, variantId), JsonRequestBehavior.AllowGet);
		}

		public ActionResult AddItemToB2BOrder(B2BOrderItemModel model)
		{
			// B2B QBIC-2723: The model.AssociatedItemId looks to me to be from the ProviderDomain, it should be from the ConsumingDomain
			// This leads me to belivere that the ID being pushed to the Browser fo rthe user to select is ProviderDomain TraderItem ID, it should be a ConsumerDomain TraderItem Id

			model.DiscussionId = string.IsNullOrEmpty(model.DisKey) ? 0 : int.Parse(model.DisKey.Decrypt());
			model.OrderId = string.IsNullOrEmpty(model.OrderKey) ? 0 : int.Parse(model.OrderKey.Decrypt());
			var currentUser = CurrentUser();
			model.CurrentUserId = currentUser.Id;
			var result = new CommerceRules(dbContext).AddItemToB2BOrder(model);
			if (!result.result)
				return Json(result);
			//QBIC-4283: Provider also can Add/edit/remove item, so we need to know which domain updates order
			var DomainUpdatesOrder = CurrentDomainId().BusinesProfile().BusinessName;
			result.msgName = DomainUpdatesOrder;
			new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_CHANGED_ORDER", result.msgName), model.DiscussionId, currentUser.Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());

			return Json(result);
		}

		public ActionResult ReCalculateTaxes(string orderKey, decimal discount, int itemId)
		{
			var orderId = 0;
			if (!string.IsNullOrEmpty(orderKey))
			{
				orderId = orderKey.Decrypt2Int();
			}
			var calculationResult = new B2CRules(dbContext).ReCalculateTax(orderId, discount, itemId);
			return Json(calculationResult, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult LoadB2BOrderItems([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string orderKey, string searchKey, List<int> catIds, int start, int length, int draw, bool isViewBuy)
		{
			try
			{
				var orderId = string.IsNullOrEmpty(orderKey) ? 0 : int.Parse(orderKey.Decrypt());
				var totalRecord = 0;
				List<B2BCartItemModel> lstResult = new CommerceRules(dbContext).GetB2BOrderItemsPagination(orderId, searchKey, catIds, requestModel, isViewBuy, ref totalRecord, start, length);
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
				return Json(new DataTableModel() { draw = draw, data = new List<B2COrderItemModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
			}
		}

		public ActionResult GetListTraderItem(string search, int itemId = 0)
		{
			return Json(new CommerceRules(dbContext).GetListTraderItem(CurrentDomainId(), search, itemId), JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateQuantityOrderItem(string disKey, string orderKey, Item updatedItem)
		{
			var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
			var orderId = string.IsNullOrEmpty(orderKey) ? 0 : int.Parse(orderKey.Decrypt());
			var updateResult = new CommerceRules(dbContext).UpdateQuantityOrderItem(orderId, updatedItem);
			//QBIC-4283: Provider also can Add/edit/remove item, so we need to know which domain updates order
			var DomainUpdatesOrder = CurrentDomainId().BusinesProfile().BusinessName;
			updateResult.msgName = DomainUpdatesOrder;
			if (updateResult.result && updateResult.actionVal != -1)
			{
				new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_UPDATED_ORDER", updateResult.msgName), disId, CurrentUser().Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());
			}
			return Json(updateResult, JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateDiscountOrderItem(string disKey, string orderKey, Item updatedItem)
		{
			var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
			var orderId = string.IsNullOrEmpty(orderKey) ? 0 : int.Parse(orderKey.Decrypt());
			var updateResult = new CommerceRules(dbContext).UpdateDiscountOrderItem(orderId, updatedItem);
			//QBIC-4283: Provider also can Add/edit/remove item, so we need to know which domain updates order
			var DomainUpdatesOrder = CurrentDomainId().BusinesProfile().BusinessName;
			updateResult.msgName = DomainUpdatesOrder;
			if (updateResult.result && updateResult.actionVal != -1)
				new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_UPDATED_ORDER", updateResult.msgName), disId, CurrentUser().Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());

			return Json(updateResult, JsonRequestBehavior.AllowGet);
		}

		public ActionResult RemoveItemFromB2BOrder(string disKey, int itemId)
		{
			var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
			var removeResult = new CommerceRules(dbContext).RemoveItemFromOrder(disId, itemId);
			if (removeResult.actionVal == -1)
				return Json(removeResult, JsonRequestBehavior.AllowGet);

			var currentUser = CurrentUser();
			//QBIC-4283: Provider also can Add/edit/remove item, so we need to know which domain updates order
			var DomainUpdatesOrder = CurrentDomainId().BusinesProfile().BusinessName;
			removeResult.msgName = DomainUpdatesOrder;
			new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_UPDATED_ORDER", removeResult.msgName), disId, currentUser.Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());

			return Json(removeResult, JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateAssociatedItem(string orderKey, int itemId, int unitId, int variantId)
		{
			var orderId = string.IsNullOrEmpty(orderKey) ? 0 : int.Parse(orderKey.Decrypt());
			var result = new CommerceRules(dbContext).UpdateAssociatedItem(orderId, itemId, unitId, variantId);
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public ActionResult BuyingDomainSubmitProposal(B2BSubmitProposal proposal)
		{
			proposal.discussionId = string.IsNullOrEmpty(proposal.disKey) ? 0 : int.Parse(proposal.disKey.Decrypt());
			proposal.tradeOrderId = string.IsNullOrEmpty(proposal.orderKey) ? 0 : int.Parse(proposal.orderKey.Decrypt());

			proposal.CurrentUserId = CurrentUser().Id;
			var returnJson = new CommerceRules(dbContext).BuyingDomainSubmitProposal(proposal);

			new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_AGREED_ORDER", returnJson.msgName), proposal.discussionId, CurrentUser().Id, CurrentQbicleId(), "");

			return Json(returnJson, JsonRequestBehavior.AllowGet);
		}

		public ActionResult SellingDomainSubmitProposal(string disKey)
		{
			var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
			var returnJson = new CommerceRules(dbContext).SellingDomainSubmitProposal(disId);

			new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_AGREED_ORDER", returnJson.msgName), disId, CurrentUser().Id, CurrentQbicleId(), "");

			return Json(returnJson, JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetDeliveryDetail(int locationId)
		{
			var location = new TraderLocationRules(dbContext).GetById(locationId);
			return Json(new { locationName = location.Name, address = location.TraderLocationToAddress() }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult LoadOrderSubmit(string orderKey)
		{
			var orderId = string.IsNullOrEmpty(orderKey) ? 0 : int.Parse(orderKey.Decrypt());
			return PartialView("_OrderButtonSubmit", new CommerceRules(dbContext).GetTradeOrderById(orderId));
		}

		public ActionResult SellingChooseWGModal(string orderKey)
		{
			var orderId = string.IsNullOrEmpty(orderKey) ? 0 : int.Parse(orderKey.Decrypt());
			var order = new CommerceRules(dbContext).GetTradeOrderById(orderId);

			var location = order.ProductMenu.Location;
			var listPaymentAccount = new TraderCashBankRules(dbContext).GetTraderCashAccounts(order.SellingDomain.Id);
			var listWorkgroup = new TraderWorkGroupsRules(dbContext).GetWorkGroupsByLocationId(location.Id);
			ViewBag.OrderDefaultSettings = new CommerceRules(dbContext).GetB2BOrderSettingDefault(order.SellingDomain.Id, CurrentUser().Id, location.Id);
			ViewBag.lstPaymentAcc = listPaymentAccount;
			ViewBag.lstWorkgroup = listWorkgroup;
			ViewBag.Location = location;

			return PartialView("_SellingChooseWG", order);
		}

		public ActionResult BuyingChooseWGModal(string orderKey, int locationId)
		{
			var orderId = orderKey.Decrypt2Int();
			var order = new CommerceRules(dbContext).GetTradeOrderById(orderId);

			var location = new TraderLocationRules(dbContext).GetById(locationId);
			var listPaymentAccount = new TraderCashBankRules(dbContext).GetTraderCashAccounts(order.BuyingDomain.Id);
			var listWorkgroup = new TraderWorkGroupsRules(dbContext).GetWorkGroupsByLocationId(location.Id);
			ViewBag.OrderDefaultSettings = new CommerceRules(dbContext).GetB2BOrderSettingDefault(order.BuyingDomain.Id, CurrentUser().Id, location.Id);
			ViewBag.lstPaymentAcc = listPaymentAccount;
			ViewBag.lstWorkgroup = listWorkgroup;
			ViewBag.Location = location;

			return PartialView("_BuyingChooseWG", order);
		}

		public ActionResult ProcessB2BOrder(B2BSubmitProposal proposal)
		{
			proposal.discussionId = proposal.disKey.Decrypt2Int();
			proposal.tradeOrderId = proposal.orderKey.Decrypt2Int();
			proposal.CurrentUserId = CurrentUser().Id;
			var returnJson = new CommerceRules(dbContext).ProcessB2BOrder(proposal);

			if (returnJson.result)
				new B2CRules(dbContext).B2BDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_PROCESSED_ORDER"), proposal.discussionId, CurrentUser().Id, CurrentQbicleId(), "", true);

			return Json(returnJson, JsonRequestBehavior.AllowGet);
		}

		public bool CheckPartnershipPortalAccessibility(string partnerDomainKey)
		{
			try
			{
				var partnerProfileId = string.IsNullOrEmpty(partnerDomainKey) ? 0 : int.Parse(partnerDomainKey.Decrypt());
				var partnerProfile = dbContext.B2BProfiles.Find(partnerProfileId);
				var partnerDomain = partnerProfile?.Domain ?? null;
				var currentDomain = dbContext.Domains.Find(CurrentDomainId());
				var isPartnershipAvailableToDomains = new CommerceRules(dbContext).IsPartnershipAvailableToDomains(currentDomain, partnerDomain);
				var currentB2BProfile = currentDomain.Id.BusinesProfile();
				var canAccessPortal = false;
				if (partnerDomain != null && currentDomain != null && currentB2BProfile != null && partnerProfile != null)
				{
					canAccessPortal = isPartnershipAvailableToDomains
									&& currentB2BProfile.IsB2BServicesProvided && partnerProfile.IsB2BServicesProvided;
				}

				return canAccessPortal;
			}
			catch (Exception ex)
			{
				LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, partnerDomainKey);
				return false;
			}
		}

		public ActionResult ChangeB2BServiceAcessibilityStatus(int profileId, bool isActive = false)
		{
			var result = new CommerceRules(dbContext).ChangeB2BProfileUsabilityStatus(profileId, isActive);
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdateExchangeRateById(int id, decimal amountSeller, decimal amountBuyer)
		{
			var result = new ExchangeRateRules(dbContext).UpdateExchangeRateById(id, amountSeller, amountBuyer);
			return Json(result);
		}

		public ActionResult CatalogDiscussion(string disKey)
		{
			var discussionId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
			var discussion = dbContext.B2BCatalogDiscussions.FirstOrDefault(p => p.Id == discussionId);
			var currentUserId = CurrentUser().Id;

			var relationship = dbContext.B2BRelationships
				.Where(s => (s.Domain1.Id == discussion.SharedByDomain.Id || s.Domain2.Id == discussion.SharedByDomain.Id)
						&& (s.Domain1.Id == discussion.SharedWithDomain.Id || s.Domain2.Id == discussion.SharedWithDomain.Id)
						&& s.CommunicationQbicle.Members.Any(m => m.Id == currentUserId)).FirstOrDefault();
			ViewBag.ProviderPartnershipKey = relationship.Partnerships.Where(s => s.IsConsumerConfirmed
								&& s.IsProviderConfirmed && s.CommunicationQbicle != null
								&& s.CommunicationQbicle.Members.Any(m => m.Id == currentUserId))
				.FirstOrDefault(p => p.ProviderDomain.Id == CurrentDomainId() && p.Type == Qbicles.Models.B2B.B2BService.Products);

			ViewBag.ConsumerPartnershipKey = relationship.Partnerships.Where(s => s.IsConsumerConfirmed
								&& s.IsProviderConfirmed && s.CommunicationQbicle != null
								&& s.CommunicationQbicle.Members.Any(m => m.Id == currentUserId))
				.FirstOrDefault(p => p.ConsumerDomain.Id == CurrentDomainId() && p.Type == Qbicles.Models.B2B.B2BService.Products);
			return View(discussion);
		}

		public ActionResult SearchMenuItems(B2BCatalogItemsRequestModel request, string scatids)
		{
			request.CatIds = JsonConvert.DeserializeObject<List<int>>(scatids);
			return Json(new PosMenuRules(dbContext).LoadB2BCatalogDiscussionItem(request), JsonRequestBehavior.AllowGet);
		}

		public ActionResult PublishCatalogInProfile(int catId, bool isPublish)
		{
			return Json(new PosMenuRules(dbContext).PublishCatalogInProfile(catId, isPublish));
		}

		public ActionResult GetProductGroups()
		{
			var productGroups = new TraderGroupRules(dbContext).GetTraderGroupItem(CurrentDomainId());
			var simpleDataProductGroups = productGroups.Select(e => new
			{
				id = e.Id,
				text = e.Name
			});
			return Json(simpleDataProductGroups, JsonRequestBehavior.AllowGet);
		}

		#region Server-side datatable getting data functions

		// Location tab
		public ActionResult GetLocationTableData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
			string keySearch = "", bool? isGeoLocated = null)
		{
			return Json(new TraderLocationRules(dbContext).GetLocationDataTableData(requestModel, keySearch, isGeoLocated, CurrentDomainId()));
		}

		/// <summary>
		/// Return domain trader address with Geo location
		/// </summary>
		/// <returns></returns>
		public ActionResult GetTraderGeoLocation()
		{
			return Json(new TraderLocationRules(dbContext).GetTraderGeoLocation(CurrentDomainId()));
		}

		// Workgroups tab
		public ActionResult GetWorkgroupTableData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
			string keySearch, List<int> lstProcessIds)
		{
			return Json(new TraderWorkGroupsRules(dbContext).GetWorkgroupDataTableData(requestModel,
				keySearch, lstProcessIds, CurrentDomainId()), JsonRequestBehavior.AllowGet);
		}

		// Cash & bank accounts
		//public ActionResult GetMyOrders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int channel = 0, bool isHideCompleted = true)
		//{
		//    return Json(new C2CRules(dbContext).GetMyOrders(requestModel, CurrentUser().Id, keyword, daterange, channel, isHideCompleted), JsonRequestBehavior.AllowGet);
		//}

		// Taxes & currency
		//public ActionResult GetMyOrders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int channel = 0, bool isHideCompleted = true)
		//{
		//    return Json(new C2CRules(dbContext).GetMyOrders(requestModel, CurrentUser().Id, keyword, daterange, channel, isHideCompleted), JsonRequestBehavior.AllowGet);
		//}

		// Groups
		//public ActionResult GetMyOrders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int channel = 0, bool isHideCompleted = true)
		//{
		//    return Json(new C2CRules(dbContext).GetMyOrders(requestModel, CurrentUser().Id, keyword, daterange, channel, isHideCompleted), JsonRequestBehavior.AllowGet);
		//}

		// Contacts
		//public ActionResult GetMyOrders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int channel = 0, bool isHideCompleted = true)
		//{
		//    return Json(new C2CRules(dbContext).GetMyOrders(requestModel, CurrentUser().Id, keyword, daterange, channel, isHideCompleted), JsonRequestBehavior.AllowGet);
		//}

		// Order defaults
		//public ActionResult GetMyOrders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int channel = 0, bool isHideCompleted = true)
		//{
		//    return Json(new C2CRules(dbContext).GetMyOrders(requestModel, CurrentUser().Id, keyword, daterange, channel, isHideCompleted), JsonRequestBehavior.AllowGet);
		//}

		#endregion Server-side datatable getting data functions

		public ActionResult OpenOrderContextFlyout()
		{
			return PartialView("~/Views/Commerce/_OrderContextFlyout.cshtml");
		}

		public ActionResult GetOrderContextFlyoutB2B([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string qbicleKey, string type,
	string keyword = "", string daterange = "", List<int> status = null, int orderBy = 0)
		{
			var currentUser = CurrentUser();
			var refModel = new CommerceRules(dbContext).GetOrderContextFlyoutB2B(requestModel, qbicleKey, type, currentUser.DateFormat, currentUser.Timezone, keyword, daterange, status, orderBy);
			return Json(refModel, JsonRequestBehavior.AllowGet);
		}

		public ActionResult CheckExistB2BOrders(string qbicleKey)
		{
			var result = new CommerceRules(dbContext).CheckExistB2BOrders(qbicleKey);
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public ActionResult CheckStatusB2BOrders(string DiscussionOrderKey)
		{
			var result = new CommerceRules(dbContext).CheckStatusB2BOrders(DiscussionOrderKey);
			return Json(result, JsonRequestBehavior.AllowGet);
		}
	}
}