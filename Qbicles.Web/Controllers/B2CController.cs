using CleanBooksData;
using Newtonsoft.Json;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.BusinessRules.TraderApi;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class B2CController : BaseController
    {
        // GET: B2C
        public ActionResult Index()
        {
            var currentUserId = CurrentUser().Id;
            var currentDomainId = CurrentDomainId();

            if (!CanAccessBusiness() && !new B2CRules(dbContext).CheckHasAccessB2C(currentDomainId, currentUserId))
                return View("ErrorAccessPage");

            ViewBag.CurrentPage = SystemPageConst.B2C;
            this.SetCreatorTheCustomer(SystemPageConst.B2C);
            var b2bProfille = new CommerceRules(dbContext).GetB2bProfileByDomainId(currentDomainId);

            var uiSettings = new QbicleRules(dbContext).LoadUiSettings(SystemPageConst.B2C, currentUserId);

            ViewBag.UiSetting = uiSettings;

            var domain = dbContext.Domains.Find(CurrentDomainId());
            var isDomainAdmin = domain?.Administrators.Any(p => p.Id == currentUserId) ?? false;
            var query = (from b2c in dbContext.B2CQbicles
                         where !b2c.IsHidden
                         && b2c.Business.Id == currentDomainId
                         && (isDomainAdmin || b2c.Members.Any(s => s.Id == currentUserId))
                         select b2c).ToList();

            var connections = 0;
            query.Where(p => p.Status != CommsStatus.Blocked).ForEach(q =>
            {
                if (q.RemovedForUsers.Count == 0)
                    connections++;
                else if (q.RemovedForUsers.Any(r => r.Id != q.Customer.Id))
                    connections++;
            });

            ViewBag.ExcludeConnectionNum = connections;
            ViewBag.NewConnectionNum = query.Where(p => p.IsNewContact == true).Count();
            ViewBag.BlockedConnectionNum = query.Where(p => p.Status == CommsStatus.Blocked).Count();

            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomainId && p.IsArchived == false);
            ViewBag.CurrentDomainPlan = currentDomainPlan;

            return View(b2bProfille);
        }
        public ActionResult DiscussionMenu(string disKey)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var discussion = new DiscussionsRules(dbContext).GetDiscussionProductMenuById(disId);
            if (discussion == null)
                return View("Error");
            ValidateCurrentDomain(discussion.Qbicle.Domain, discussion.Qbicle.Id);
            ViewBag.CurrentPage = SystemPageConst.SOCIALPOSTDISCUSSION;
            SetCurrentPage(SystemPageConst.SOCIALPOSTDISCUSSION);
            SetCurrentDiscussionIdCookies(disId);
            ViewBag.CurrentPage = CurrentGoBackPage();

            return View(discussion);
        }
        public ActionResult ManageOrder(int id)
        {
            var b2cOrder = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(id);
            if (b2cOrder == null)
                return View("Error");
            return Redirect($"~/B2C/DiscussionOrder?disKey={b2cOrder.Key}");
        }
        public ActionResult DiscussionOrder(string disKey, string discussionType = "")
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var currentUser = CurrentUser();

            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(disId);
            if (discussion == null)
                return View("Error");
            ValidateCurrentDomain(discussion.Qbicle.Domain, discussion.Qbicle.Id);

            SetCurrentDiscussionIdCookies(disId);

            var b2cqbicle = discussion.Qbicle as B2CQbicle;
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(b2cqbicle.Business.Id);

            ViewBag.currencySetting = currencySetting;

            if (string.IsNullOrEmpty(discussionType))
                discussionType = GetCreatorTheCustomer();
            var go2Discussion = new B2CRules(dbContext).DiscussionOrderBy(disId, currentUser, discussionType, discussion);

            var tradeOrder = discussion.TradeOrder;

            ViewBag.BusinessProfileLogo = dbContext.B2BProfiles.AsNoTracking().Where(s => s.Domain.Id == tradeOrder.SellingDomain.Id).FirstOrDefault()?.LogoUri.ToUriString();

            var locationId = tradeOrder.ProductMenu.Location.Id;
            ViewBag.UseDefaultWorkgroupSettings = dbContext.B2CSettings.FirstOrDefault(c => c.Location.Id == locationId)?.UseDefaultWorkgroupSettings ?? false;

            ViewBag.PaymentMethods = dbContext.PaymentMethods.Select(e => new Qbicles.Models.MicroQbicleStream.BaseModel { Id = e.Id, Name = e.Name }).ToList();

            var _order = new Order() { Items = new List<Item>() };
            if (tradeOrder != null && tradeOrder.OrderJson != null)
                _order = JsonHelper.ParseAs<Order>(tradeOrder.OrderJson);

            ViewBag.TotalPriceStr = _order.AmountInclTax.ToCurrencySymbol(currencySetting);

            if (go2Discussion.Type == DiscusionOrderBy.Business)
            {
                var voucher = dbContext.Vouchers.FirstOrDefault(e => e.Id == _order.VoucherId) ?? new Qbicles.Models.Loyalty.Voucher();
                ViewBag.Voucher = new VoucherModel { Id = voucher.Id, Name = voucher.Promotion?.Name, Key = voucher.Promotion?.Key, Code = voucher.Code };

                this.SetCreatorTheCustomer(SystemPageConst.B2C);
                this.SetCookieGoBackPage(SystemPageConst.B2C);
                ViewBag.CurrentGoBackPage = SystemPageConst.B2C;

                ViewBag.CurrentPage = SystemPageConst.SOCIALPOSTDISCUSSION;
                SetCurrentPage(SystemPageConst.SOCIALPOSTDISCUSSION);

                return View(discussion);
            }
            else if (go2Discussion.Type == DiscusionOrderBy.Customer)
            {
                ViewBag.Vouchers = new TraderContactApiRules(dbContext).GetContactVouchers(currentUser.Id, currentUser.Id, tradeOrder.SellingDomain.Id);

                this.SetCreatorTheCustomer(SystemPageConst.C2C);
                this.SetCookieGoBackPage(SystemPageConst.C2C);
                ViewBag.CurrentGoBackPage = SystemPageConst.C2C;

                ViewBag.CurrentPage = SystemPageConst.SOCIALPOSTDISCUSSION;
                SetCurrentPage(SystemPageConst.SOCIALPOSTDISCUSSION);
                SetCurrentB2COrderIdCookies(tradeOrder.Id);
                return View("CustomerDiscussionOrder", discussion);
            }
            else
            {
                return View("Error");
            }
        }


        public ActionResult StorefrontOrder(int disId)
        {
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(disId);
            if (discussion == null)
                return View("Error");
            var currentUser = CurrentUser();
            ValidateCurrentDomain(discussion.Qbicle.Domain, discussion.Qbicle.Id);
            SetCurrentDiscussionIdCookies(disId);
            ViewBag.CurrentPage = SystemPageConst.SOCIALPOSTDISCUSSION;
            SetCurrentPage(SystemPageConst.SOCIALPOSTDISCUSSION);
            ViewBag.Vouchers = new TraderContactApiRules(dbContext).GetContactVouchers(currentUser.Id, currentUser.Id, discussion.TradeOrder.SellingDomain.Id);
            return View("StorefrontOrder", discussion);
        }
        public ActionResult ActiveInteraction(int disId)
        {
            return Json(new DiscussionsRules(dbContext).ActiveInteraction(disId));
        }
        public ActionResult SearchMenuItems(B2CMenuItemsRequestModel request, string scatids)
        {
            request.CatIds = JsonConvert.DeserializeObject<List<int>>(scatids);
            return Json(new PosMenuRules(dbContext).LoadMenuItems(request), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadProductMoreMenuContent(int menuId, int businessDomainId)
        {
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(businessDomainId);
            ViewBag.CurrencySetting = currencySetting;
            ViewBag.isAllowAdd = false;
            ViewBag.IsShopping = false;
            return PartialView("_B2COrderItemContent", new PosMenuRules(dbContext).GetPosCategoryItemById(menuId));

        }
        /// <summary>
        /// Get all c2c qbicle by current user
        /// </summary>
        /// <param name="keyword">Search contacts...</param>
        /// <param name="orderby">
        /// 0: Order by latest activity(Default)
        /// 1: Order by forename A-Z
        /// 2: Order by forename Z-A
        /// 3: Order by surname A-Z
        /// 4: Order by surname Z-A
        /// </param>
        /// <param name="includeBlocked">Include blocked users</param>
        /// <param name="b2cQbiceActiveId">This is the Id of b2cqbicle set selected. It default is 0 </param>
        /// <param name="typeShown">1 - show all - excluded blocked connections, 2 - show new connections, 3 - show blocked connections</param>
        /// <returns>PartialView _B2CQbiclesContent</returns>
        public ActionResult LoadB2CQbiclesContent(string keyword, int orderby, string b2cQbiceKey = "0", int typeShown = 1)
        {
            var b2cRule = new B2CRules(dbContext);
            var listB2cQbicle = b2cRule.GetB2CQbicles(CurrentDomainId(), CurrentUser().Id, keyword, orderby, typeShown);
            if (b2cQbiceKey == "0")
            {
                var b2cqbicle = b2cRule.GetB2CQbicleById(CurrentQbicleId());
                if (b2cqbicle != null)
                    b2cQbiceKey = b2cqbicle.Key;
                else
                    b2cQbiceKey = listB2cQbicle.FirstOrDefault()?.Key ?? "0";
            }
            var qbicleId = int.Parse(b2cQbiceKey.Decrypt());
            SetCurrentQbicleIdCookies(qbicleId);//B2C Qbicle
            ViewBag.B2CQbiceSelectedId = qbicleId;
            var cPage = SystemPageConst.B2C;

            //store ui settings
            b2cRule.B2CUiSetting(cPage, CurrentUser().Id, new B2CSearchQbicleModel { Orderby = orderby, ShownType = typeShown });
            return PartialView("_B2CQbiclesContent", listB2cQbicle);
        }
        public ActionResult LoadModalActivities()
        {
            var qbicleId = CurrentQbicleId();
            var b2cQbicle = new B2CRules(dbContext).GetB2CQbicleById(qbicleId);
            ViewBag.qbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(qbicleId, CurrentUser().Id);
            var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
            ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);
            return PartialView("_ModalActivities", b2cQbicle);
        }
        public ActionResult LoadB2CQbicleStatusInfo(string key)
        {
            var b2cQbicleId = int.Parse(key.Decrypt());
            return PartialView("_B2CQbicleStatusContent", new B2CRules(dbContext).GetB2CQbicleById(b2cQbicleId));
        }
        /// <summary>
        /// This is function update status of the C2C
        /// </summary>
        /// <param name="qId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult SetStatusBy(string key, CommsStatus status)
        {
            var qId = int.Parse(key.Decrypt());
            return Json(new B2CRules(dbContext).SetStatusByBusiness(qId, CurrentDomainId(), CurrentUser().Id, status));
        }
        public ActionResult UpdateRelationshipManagers(string key, List<string> UserIds)
        {
            var qbicleId = int.Parse(key.Decrypt());
            return Json(new B2CRules(dbContext).UpdateRelationshipManagers(qbicleId, CurrentUser().Id, UserIds));
        }
        public ActionResult SetB2BConnectionViewedStatus(string key, bool isViewed = true)
        {
            var qId = int.Parse(key.Decrypt());
            return Json(new B2CRules(dbContext).SetViewedConnection(qId, isViewed));
        }
        public ActionResult RemoveB2CQbicleById(string key)
        {
            var qbicleId = int.Parse(key.Decrypt());
            return Json(new B2CRules(dbContext).RemoveB2CQbicleById(qbicleId, CurrentUser().Id));
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
                var isHidden = false;
                var b2cqbicle = new B2CRules(dbContext).GetB2CQbicleById(fillterModel.QbicleId);
                if (b2cqbicle != null)
                {
                    var model = qbRule.GetQbicleStreams(fillterModel, user.Timezone, user.DateFormat);
                    if (model != null)
                    {
                        //var modelString = RenderLoadNextViewToString("~/Views/Qbicles/_Dashboard.cshtml", model);
                        var modelString = ActivityPostHtmlTemplateRules.getQbicleStreamsHtml(model, user.Id, user.Timezone, user.DateFormat);
                        if (fillterModel.Size == 0 && b2cqbicle.Status == CommsStatus.Blocked)
                        {
                            isHidden = b2cqbicle.IsHidden;
                            modelString = RenderLoadNextViewToString("~/Views/B2C/_B2CQbicleStatusContent.cshtml", b2cqbicle) + modelString;
                        }
                        var result = Json(new { ModelString = modelString, ModelCount = model.TotalCount, isHidden },
                            JsonRequestBehavior.AllowGet);
                        result.MaxJsonLength = int.MaxValue;
                        return result;
                    }
                    return Json(new { ModelString = "", ModelCount = 0, isHidden = true });
                }
                else
                {
                    return Json(new { ModelString = "", ModelCount = 0, isHidden = true });
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return null;
            }
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
        public ActionResult SaveDiscussionForProductMenu(B2CProductMenuDiscussionModel model)
        {
            model.QbicleId = CurrentQbicleId();
            return Json(new DiscussionsRules(dbContext).SaveDiscussionForProductMenu(model, CurrentUser().Id, GetOriginatingConnectionIdFromCookies(), AppType.Web));
        }



        public ActionResult LoadOrderCreationContent()
        {
            var domain = CurrentDomain();
            var traderReferenceForSale = new TraderReferenceRules(dbContext).GetNewReference(domain.Id, TraderReferenceType.Order);
            var b2OrderCreation = new B2OrderCreationDiscussionModel
            {
                OrderReference = traderReferenceForSale.FullRef,
                OrderReferenceId = traderReferenceForSale.Id,
                //ProductMenus = new B2CRules(dbContext).GetPosMenusByB2CQbicleId(CurrentQbicleId())
            };
            List<int> lids = domain.TraderLocations.Select(s => s.Id).ToList();
            b2OrderCreation.ProductMenus = new PosMenuRules(dbContext).FiltersCatalog(lids, "", true, (int)SalesChannelEnum.B2C);
            return PartialView("_B2COrderAddContent", b2OrderCreation);
        }

        public ActionResult ConnectBusiness(string linkId)
        {
            var connectResult = new C2CRules(dbContext).ConnectC2C(CurrentUser().Id, linkId, 1);
            if (connectResult.result)
            {
                SetCurrentQbicleIdCookies((int)connectResult.Object);
            }
            return Json(connectResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConnectB2C(string userId, string profileId)
        {
            var connectResult = new C2CRules(dbContext).ConnectC2C(userId, profileId, 1);
            if (connectResult.result)
            {
                SetCurrentQbicleIdCookies((int)connectResult.Object);
            }
            return Json(connectResult, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult ConnectB2CFromListing(string message, int listingId, string topic)
        //{
        //    var result = new B2CRules(dbContext).ConnectB2CFromListing(CurrentUserId(), message, listingId, topic, CurrentQbicleId());
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        #region B2C Order Discussion
        public ActionResult SearchB2COrderItems(B2COrderItemsRequestModel request, string scatids)
        {
            request.bdomainId = 0;
            if (!string.IsNullOrEmpty(request.bdomainKey))
            {
                request.bdomainId = Int32.Parse(EncryptionService.Decrypt(request.bdomainKey));
            }

            request.CatIds = JsonConvert.DeserializeObject<List<int>>(scatids);
            return Json(new PosMenuRules(dbContext).LoadOrderMenuItem(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadB2COrderItems([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string discussionKey, string searchKey, int start, int length, int draw)
        {
            try
            {
                var discussionId = string.IsNullOrEmpty(discussionKey) ? 0 : int.Parse(discussionKey.Decrypt());
                var totalRecord = 0;
                List<B2COrderItemModel> lstResult = new B2CRules(dbContext).GetB2COrderItemsPagination(discussionId, searchKey, requestModel, ref totalRecord, start, length);
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

        public ActionResult B2COrderItemContentShow(int itemId, string domainKey, int orderId)
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey))
            {
                domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
            }
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            ViewBag.CurrencySetting = currencySetting;
            var tradeOrder = new B2CRules(dbContext).GetTradeOrderById(orderId);
            var isAllowAdd = true;
            if (tradeOrder != null)
                isAllowAdd = !tradeOrder.IsAgreedByCustomer || !tradeOrder.IsAgreedByBusiness;
            ViewBag.isAllowAdd = isAllowAdd;
            ViewBag.IsShopping = true;
            return PartialView("_B2COrderItemContent", new PosMenuRules(dbContext).GetPosCategoryItemWithTaxesById(itemId));
        }

        public ActionResult B2COrderCartShow(string disKey, string domainKey)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey))
            {
                domainId = int.Parse(domainKey.Decrypt());
            }

            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            ViewBag.CurrencySetting = currencySetting;
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(disId);

            //ViewBag.CurrentUser = new UserRules(dbContext).GetUser(CurrentUser().Id, 0);

            var order = JsonHelper.ParseAs<Order>(discussion.TradeOrder.OrderJson) ?? new Order();
            ViewBag.Order = order;
            var voucher = dbContext.Vouchers.FirstOrDefault(e => e.Id == order.VoucherId) ?? new Qbicles.Models.Loyalty.Voucher();
            ViewBag.Voucher = new VoucherModel { Id = voucher.Id, Name = voucher.Promotion?.Name, Key = voucher.Promotion?.Key, Code = voucher.Code };

            ViewBag.IsInvoiceApproved = discussion.TradeOrder.Invoice != null ? "true" : "false";

            var invoiceTotal = discussion.TradeOrder.Invoice?.TotalInvoiceAmount ?? 0;
            var paymentTotal = discussion.TradeOrder.Payments.Sum(e => e.Amount);//.ToCurrencySymbol(currencySettings);
            var paymentRemain = invoiceTotal - paymentTotal;

            ViewBag.InvoiceTotal = invoiceTotal;
            ViewBag.InvoiceTotalTxt = invoiceTotal.ToCurrencySymbol(currencySetting);
            ViewBag.PaymentRemain = paymentRemain;
            ViewBag.PaymentRemainTxt = paymentRemain.ToCurrencySymbol(currencySetting);

            return PartialView("_B2COrderCart", discussion.TradeOrder);
        }
        public ActionResult B2COrderInfoModal(int id)
        {

            var tradeOrder = new B2CRules(dbContext).GetTradeOrderById(id);
            if (tradeOrder == null)
                return PartialView("Error");
            ViewBag.CurrencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(tradeOrder.SellingDomain.Id);
            return PartialView("_B2COrderInfo", tradeOrder);
        }

        public ActionResult AddItemToB2COrder(string disKey, TradeOrder b2cOrder,
            Qbicles.Models.Catalogs.Variant variant, List<Extra> extras, int quantity, decimal includedTaxAmount, int posItemId, int voucherId = 0)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var currentUser = CurrentUser();
            var result = new B2CRules(dbContext).AddItemToB2COrder(b2cOrder.Id, variant, extras, quantity, includedTaxAmount, currentUser.Id, posItemId, voucherId);
            if (!result.result)
                return Json(result);

            var discussion = new B2CRules(dbContext).DiscussionOrderBy(disId, currentUser, GetCreatorTheCustomer());

            new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_CHANGED_ORDER", discussion.DisplayName), disId, currentUser.Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());

            return Json(result);
        }

        public ActionResult RemoveItemFromB2COrder(string disKey, int itemId)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var removeResult = new B2CRules(dbContext).RemoveItemFromOrder(disId, itemId);
            if (removeResult.actionVal == -1)
                return Json(removeResult, JsonRequestBehavior.AllowGet);

            var currentUser = CurrentUser();
            var discussion = new B2CRules(dbContext).DiscussionOrderBy(disId, currentUser, GetCreatorTheCustomer());
            new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_UPDATED_ORDER", discussion.DisplayName), disId, currentUser.Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());

            return Json(removeResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeOrderItemQuantity(string disKey, TradeOrder b2cOrder, int itemId, int newQuantity)
        {
            if (newQuantity <= 0)
                newQuantity = 1;

            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var result = new B2CRules(dbContext).UpdateB2COrderItemQuantity(b2cOrder, itemId, newQuantity, CurrentUser().Id);
            if (result.actionVal == -1)
                return Json(result, JsonRequestBehavior.AllowGet);


            var currentUser = CurrentUser();
            var discussion = new B2CRules(dbContext).DiscussionOrderBy(disId, currentUser, GetCreatorTheCustomer());
            new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_UPDATED_ORDER", discussion.DisplayName), disId, currentUser.Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult setB2COrderAcceptedByCustomer(B2CCustomerAcceptedInfo acceptedInfo)
        {
            acceptedInfo.disId = string.IsNullOrEmpty(acceptedInfo.disKey) ? 0 : int.Parse(acceptedInfo.disKey.Decrypt());
            var returnJson = new B2CRules(dbContext).SetOrderAcceptedByCustomer(acceptedInfo);

            if (!string.IsNullOrEmpty(acceptedInfo.note))
            {
                new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), acceptedInfo.note, acceptedInfo.disId, CurrentUser().Id, CurrentQbicleId(), "");
                return Json(returnJson, JsonRequestBehavior.AllowGet);
            }

            var discussion = new B2CRules(dbContext).DiscussionOrderBy(acceptedInfo.disId, CurrentUser(), GetCreatorTheCustomer());
            new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_AGREED_ORDER", discussion.DisplayName), acceptedInfo.disId, CurrentUser().Id, CurrentQbicleId(), "");
            return Json(returnJson, JsonRequestBehavior.AllowGet);
        }

        public ActionResult setB2COrderAcceptedByBusiness(int disId)
        {
            var returnJson = new B2CRules(dbContext).SetOrderAcceptedByBusiness(disId);
            if (!returnJson.result)
                return Json(returnJson, JsonRequestBehavior.AllowGet);

            new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_PROCESSED_ORDER"), disId, CurrentUser().Id, CurrentQbicleId(), "", true);

            return Json(returnJson, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Re-calculation Item Taxes while business update change discount
        /// </summary>
        /// <param name="tradeOrderId"></param>
        /// <param name="discount"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ActionResult ReCalculateTax(int tradeOrderId, decimal discount, int itemId)
        {
            var calculationResult = new B2CRules(dbContext).ReCalculateTax(tradeOrderId, discount, itemId);
            return Json(calculationResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// update item when business change discount or price
        /// </summary>
        /// <param name="disKey"></param>
        /// <param name="tradeOrderId"></param>
        /// <param name="updatedItem"></param>
        /// <returns></returns>
        public ActionResult UpdateItemInfor(string disKey, int tradeOrderId, Item updatedItem)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var updateResult = new B2CRules(dbContext).UpdateItemInfor(tradeOrderId, updatedItem);

            if (updateResult.result && updateResult.actionVal != -1)
            {

                var currentUser = CurrentUser();
                var discussion = new B2CRules(dbContext).DiscussionOrderBy(disId, currentUser, GetCreatorTheCustomer());

                new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_UPDATED_ORDER", discussion.DisplayName), disId, CurrentUser().Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());
            }


            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OrderProcessingModal(string disKey)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(disId);

            var location = discussion.TradeOrder.ProductMenu.Location;
            var b2cqbicle = discussion.Qbicle as B2CQbicle;
            var listPaymentAccount = new TraderCashBankRules(dbContext).GetTraderCashAccounts(b2cqbicle.Business.Id);
            var listWorkgroup = new TraderWorkGroupsRules(dbContext).GetWorkGroupsByLocationId(location.Id);

            ViewBag.OrderDefaultSettings = new CommerceRules(dbContext).GetB2COrderSettingDefault(b2cqbicle.Business.Id, CurrentUser().Id, location.Id);

            ViewBag.lstPaymentAcc = listPaymentAccount;
            ViewBag.lstWorkgroup = listWorkgroup;
            ViewBag.Location = location;

            return PartialView("_OrderProcessingModal", discussion.TradeOrder);
        }

        public ActionResult ProcessB2COrder(string disKey, int tradeOrderId, int PaymentAccId, int SaleWGId, int InvoiceWGId, int PaymentWGId, int TransferWGId)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(disId);
            var qbicle = discussion.Qbicle as B2CQbicle;
            var result = new B2CRules(dbContext).ProcessB2COrder(tradeOrderId, PaymentAccId, SaleWGId, InvoiceWGId, PaymentWGId, TransferWGId, qbicle.Customer.Id, CurrentUser().Id);

            if (result.result)
                new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_PROCESSED_ORDER"), disId, CurrentUser().Id, CurrentQbicleId(), "", true);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ProcessB2COrderUseDefaultWorkgroupSettings(string disKey, int tradeOrderId)
        {

            var disId = disKey.Decrypt2Int();
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(disId);
            var qbicle = discussion.Qbicle as B2CQbicle;

            var orderDefaultSettings = new CommerceRules(dbContext).GetB2COrderSettingDefault(qbicle.Id, CurrentUser().Id, discussion.TradeOrder.ProductMenu.Location.Id);


            if (orderDefaultSettings.SaveSettings
                && orderDefaultSettings.DefaultSaleWorkGroupId > 0 && orderDefaultSettings.DefaultInvoiceWorkGroupId > 0
                && orderDefaultSettings.DefaultPaymentWorkGroupId > 0 && orderDefaultSettings.DefaultTransferWorkGroupId > 0
                && orderDefaultSettings.DefaultPaymentAccountId > 0
                )
            {
                var result = new B2CRules(dbContext).ProcessB2COrder(tradeOrderId, orderDefaultSettings.DefaultPaymentAccountId, orderDefaultSettings.DefaultSaleWorkGroupId,
               orderDefaultSettings.DefaultInvoiceWorkGroupId, orderDefaultSettings.DefaultPaymentWorkGroupId, orderDefaultSettings.DefaultTransferWorkGroupId, qbicle.Customer.Id, CurrentUser().Id);

                if (result.result)
                    new B2CRules(dbContext).B2CDicussionOrderSendMessage(IsCreatorTheCustomer(), ResourcesManager._L("B2C_PROCESSED_ORDER"), disId, CurrentUser().Id, CurrentQbicleId(), "", true);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new ReturnJsonModel { result = false }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetIsAgreedByCustomer(int oid)
        {
            var tradeOrder = new B2CRules(dbContext).GetTradeOrderById(oid);
            var isAgreed = false;
            var modelString = "";
            if (tradeOrder != null && tradeOrder.IsAgreedByCustomer && !tradeOrder.IsAgreedByBusiness)
            {
                isAgreed = true;
                modelString = RenderLoadNextViewToString("~/Views/B2C/_CustomerInfo.cshtml", tradeOrder);
            }

            var isAgreedByBoth = false;
            if (tradeOrder != null && tradeOrder.IsAgreedByCustomer && tradeOrder.IsAgreedByBusiness)
                isAgreedByBoth = true;


            return Json(new { isAgreedByCustomer = isAgreed, isAgreedByBoth, customerInfo = modelString, orderStatus = tradeOrder.GetDescription(), orderStatusCss = tradeOrder.GetClass() }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult GetBusinessStores(FindBusinessStoresRequest request)
        {
            request.currentUserId = CurrentUser().Id;
            return Json(new B2CRules(dbContext).GetFeaturedBusinessStores(request), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="isLoadAll"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public ActionResult LoadB2CBusinessCatalogues(string domainKey, bool isLoadAll, string view = "_BusinessCatalogues")
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey.Trim()))
            {
                domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
            }
            ViewBag.BusinessProfile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId);
            ViewBag.Catalogues = new B2CRules(dbContext).GetListCatalogViewModelByDomainId(domainId, isLoadAll);
            return PartialView(view);
        }
        public ActionResult MapQbicleKeyIsUpdated(string currentKey, List<string> listQbicleKey)
        {
            var qbicleMapKey = "";
            var currentId = int.Parse(currentKey.Decrypt());
            if (listQbicleKey != null && listQbicleKey.Any())
            {
                foreach (var item in listQbicleKey)
                {
                    var id = int.Parse(item.Decrypt());
                    if (currentId == id)
                    {
                        qbicleMapKey = item;
                        break;
                    }

                }
            }
            return Json(qbicleMapKey, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetB2CConnectionNumberByTypes()
        {
            return Json(new B2CRules(dbContext).GetB2CConnectionTypeNum(CurrentUser().Id, CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateB2CPayment(string tradeOrderId, bool isCutomer, CashAccountTransaction payment)
        {
            var refModel = new B2CRules(dbContext).CreateB2CPayment(tradeOrderId, payment, isCutomer, CurrentUser().Id);

            refModel.Object = null;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Apply or remove voucher from B2C Order
        /// </summary>
        /// <param name="discussionKey"></param>
        /// <param name="tradeOrderId"></param>
        /// <param name="voucherId"></param>
        /// <returns></returns>
        public ActionResult ApplyVoucherOrderB2C(string discussionKey, string tradeOrderId, int voucherId)
        {

            var refModel = new B2CRules(dbContext).ApplyVoucherOrderB2C(discussionKey, tradeOrderId, voucherId, CurrentUser(), GetCreatorTheCustomer(), CurrentQbicleId(), GetOriginatingConnectionIdFromCookies());

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadB2CBusinessCatalogDetail(string catalogKey)
        {
            var catalogId = int.Parse(catalogKey.Decrypt());

            var model = dbContext.PosMenus.FirstOrDefault(e => e.Id == catalogId);

            var currentDomainId = model.Location?.Domain?.Id ?? 0;
            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomainId && p.IsArchived == false);
            ViewBag.CurrentDomainPlan = currentDomainPlan;

            var b2cProfile = new CommerceRules(dbContext).GetB2bProfileByDomainId(currentDomainId);

            ViewBag.IsShowCatalog = false;
            if (b2cProfile != null
                && b2cProfile.IsDisplayedInB2CListings
                && b2cProfile.DefaultB2CRelationshipManagers.Any()
                && b2cProfile.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                && model.IsPublished
                && !model.IsDeleted
                )
            {
                ViewBag.IsShowCatalog = true;
            }

            return PartialView("_BusinessCatalogDetail", model);
        }

        public ActionResult OrderProcessedCheck(string disKey)
        {
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(disKey.Decrypt2Int());

            object refModel;
            if (discussion.TradeOrder?.Invoice != null)
            {
                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(discussion.TradeOrder.SellingDomain.Id);
                var invoiceTotal = discussion.TradeOrder.Invoice?.TotalInvoiceAmount ?? 0;
                var paymentTotal = discussion.TradeOrder.Payments.Sum(e => e.Amount);//.ToCurrencySymbol(currencySettings);
                var paymentRemain = invoiceTotal - paymentTotal;

                ViewBag.InvoiceTotal = invoiceTotal;
                ViewBag.InvoiceTotalTxt = invoiceTotal.ToCurrencySymbol(currencySetting);
                ViewBag.PaymentRemain = paymentRemain;
                ViewBag.PaymentRemainTxt = paymentRemain.ToCurrencySymbol(currencySetting);

                refModel = new
                {
                    Processed = true,
                    InvoiceKey = discussion.TradeOrder.Invoice.Key,
                    SaleTotal = discussion.TradeOrder.Sale.SaleTotal,
                    PaymentFull = discussion.TradeOrder.Invoice.TotalInvoiceAmount == discussion.TradeOrder.Payments.Sum(e => e.Amount),
                    InvoiceTotalTxt = invoiceTotal.ToCurrencySymbol(currencySetting),
                    PaymentRemain = paymentRemain,
                    PaymentRemainTxt = paymentRemain.ToCurrencySymbol(currencySetting)
                    //PayTotal= discussion.TradeOrder?.Payments.Sum(e => e.Amount)
                };
            }
            else
                refModel = new { Processed = false };
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }





        /// <summary>
        /// Create an order from business (B2C)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveDiscussionForOrderCreation(B2OrderCreationDiscussionModel model)
        {
            model.QbicleId = CurrentQbicleId();
            return Json(new DiscussionsRules(dbContext).SaveDiscussionForOrderCreation(model, CurrentUser().Id, IsCreatorTheCustomer(), GetOriginatingConnectionIdFromCookies(), AppType.Web), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetMenusByLocationId(int lid)
        {
            var lids = new List<int>
            {
                lid
            };
            var list = new PosMenuRules(dbContext).FiltersCatalog(lids, "", true, (int)SalesChannelEnum.B2C);
            return Json(list.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Create an order from Customer( c2c community) or B2B
        /// </summary>
        /// <param name="businessDomainKey"></param>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        public ActionResult CreateB2COrderDiscussion(string businessDomainKey, int catalogId)
        {
            var businessDomainId = 0;
            if (!string.IsNullOrEmpty(businessDomainKey.Trim()))
            {
                businessDomainId = int.Parse(EncryptionService.Decrypt(businessDomainKey));
            }
            var refModel = new DiscussionsRules(dbContext).SaveB2CDiscussionForStore(businessDomainId, catalogId, CurrentUser().Id, IsCreatorTheCustomer());
            refModel.Object2 = null;
            return Json(refModel);
        }

        /// <summary>
        /// Create b2b order from Discussion menu
        /// </summary>
        /// <param name="catalogDiscussionKey"></param>
        /// <returns></returns>
        public ActionResult CreateB2COrderDiscussionFromCatalogDiscussion(string catalogDiscussionKey)
        {
            var disId = string.IsNullOrEmpty(catalogDiscussionKey) ? 0 : int.Parse(catalogDiscussionKey.Decrypt());
            var catalogDiscussion = new DiscussionsRules(dbContext).GetDiscussionProductMenuById(disId);

            var businessDomainId = catalogDiscussion?.ProductMenu?.Location?.Domain?.Id ?? 0;
            var customerId = (catalogDiscussion?.Qbicle as B2CQbicle)?.Customer?.Id ?? "";
            var catalogId = catalogDiscussion?.ProductMenu?.Id ?? 0;
            var refModel = new DiscussionsRules(dbContext).SaveB2CDiscussionForStore(businessDomainId, catalogId, customerId, IsCreatorTheCustomer());
            refModel.Object2 = null;
            return Json(refModel);
        }


        public async Task<ActionResult> GetB2COrerPaymentPaystackPopupData(string tradeOrderKey)
        {
            var tradeOrderId = string.IsNullOrEmpty(tradeOrderKey) ? 0 : int.Parse(tradeOrderKey.Decrypt());
            var transactionCreationResponse = await new DiscussionsRules(dbContext).GetB2COrerPaymentPaystackPopupData(tradeOrderId);
            return Json(transactionCreationResponse, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Load B2C Catalogues display on modal
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public ActionResult LoadB2CCatalogues(string domainKey)
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey.Trim()))
            {
                domainId = domainKey.Decrypt2Int();
            }
            ViewBag.BusinessProfile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId);
            ViewBag.Catalogues = new B2CRules(dbContext).GetListCatalogViewModelByDomainId(domainId, false);
            return PartialView("_B2CCatalogues");
        }

        public ActionResult GetVariantBySelectedOption(List<int> listVariantOptionIds, int categoryItemId, int quantity)
        {
            var result = new B2CRules(dbContext).GetVariantBySelectedOptions(listVariantOptionIds, categoryItemId, quantity);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult OpenOrderContextFlyout()
        {
            return PartialView("_OrderContextFlyout");
        }
        public ActionResult GetOrderContextFlyout([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string qbicleKey, string type,
            string keyword = "", string daterange = "", List<int> status = null, int orderBy = 0)
        {
            var currentUser = CurrentUser();
            var refModel = new B2CRules(dbContext).GetOrderContextFlyout(requestModel, qbicleKey, type, currentUser.DateFormat, currentUser.Timezone, keyword, daterange, status, orderBy);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult CheckExistB2cOrders(string qbicleKey)
        {
            var result = new B2CRules(dbContext).CheckExistB2cOrders(qbicleKey);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListTopic(int qbicleId)
        {
            var result = new TopicRules(dbContext).GetTopicByQbicle(qbicleId, CurrentUser().Id);
            return Json(result.Select(e => new
            {
                id = e.Id,
                text = e.Name
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}