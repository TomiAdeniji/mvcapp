using Qbicles.BusinessRules;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class C2CController : BaseController
    {
        // Community: C2C#comms-activities
        public ActionResult Index()
        {
            int allNum = 0;
            int favouriteNum = 0;
            int requestNum = 0;
            int sentNum = 0;
            int blockedNum = 0;
            var uiSettings = new QbicleRules(dbContext).LoadUiSettings(SystemPageConst.C2C, CurrentUser().Id);
            var sContactType = uiSettings.FirstOrDefault(s => s.Key == C2CStoreUiSettingsConst.CONTACTTYPE)?.Value ?? "0";
            var c2cRules = new C2CRules(dbContext);
            c2cRules.GetCommunityTalkNumByType(CurrentUser().Id, "", int.Parse(sContactType), ref allNum, ref favouriteNum, ref requestNum, ref sentNum, ref blockedNum);

            ViewBag.AllTalkNum = allNum;
            ViewBag.FavouriteNum = favouriteNum;
            ViewBag.RequestNum = requestNum;
            ViewBag.SentNum = sentNum;
            ViewBag.BlockedNum = blockedNum;

            ViewBag.CurrentPage = SystemPageConst.C2C;
            this.SetCreatorTheCustomer(SystemPageConst.C2C);
            ViewBag.UiSetting = uiSettings;
            //Country
            ViewBag.AreaOfOperations = c2cRules.GetAreaOfOperations();
            ViewBag.ShopCategories = c2cRules.GetBusinessCategoriesProfile();
            ViewBag.FeatureStore = c2cRules.GetFeaturedStores();
            ViewBag.BrandsMaster = c2cRules.GetBrandsMaster();
            ViewBag.FeatureProductListProduct = c2cRules.GetProductsList();
            ViewBag.FeatureProductImageList = c2cRules.GetProductsImageList();
            ViewBag.FillterObject = c2cRules.CustomObject();
            ViewBag.ProductTags = c2cRules.GetProductTagTagify();
            //View
            return View();
        }
        public ActionResult MyOrders()
        {
            ViewBag.CurrentPage = SystemPageConst.MYORDERS;
            return View();
        }
        public ActionResult ContactStore(int orderId)
        {
            var b2CRules = new B2CRules(dbContext);
            var tradeOrder = b2CRules.GetTradeOrderById(orderId);
            if (tradeOrder == null)
            {
                var qbicle = b2CRules.Get2CQbicleByBusinessIdAndCustomerId(tradeOrder.SellingDomain?.Id ?? 0, tradeOrder.Customer.Id);
                if (qbicle == null)
                {
                    qbicle = b2CRules.CreateB2CQbicleForChannel(tradeOrder.SellingDomain?.Id ?? 0, tradeOrder.Customer.Id, CurrentUser().Id);
                }
                if (CurrentQbicleId() != qbicle.Id)
                {
                    SetCurrentQbicleIdCookies(qbicle.Id);
                    ViewBag.CurrentQbicleId = qbicle.Id;
                }
            }
            return Redirect("~/C2C");
        }
        public ActionResult TalkBusiness(string businessKey)
        {
            var currentUserInfo = CurrentUser();
            var domainId = int.Parse(businessKey.Decrypt());
            var qbicle = new B2CRules(dbContext).CreateB2CQbicleForChannel(domainId, currentUserInfo.Id, currentUserInfo.Id);
            if (CurrentQbicleId() != qbicle.Id)
            {
                SetCurrentQbicleIdCookies(qbicle.Id);
                ViewBag.CurrentQbicleId = qbicle.Id;
            }
            return Redirect("~/C2C");
        }
        public ActionResult FindPeople(FindPeopleRequest request)
        {
            request.CurrentUserId = CurrentUser().Id;
            request.CurrentBusinessId = CurrentDomainId();
            return Json(new C2CRules(dbContext).FindPeople(request), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadShoppingProductListDT([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keySearch, string countryName, List<int> lstBrandIds, List<string> lstProductTags, int start, int length, int draw)
        {
            var totalRecords = 0;
            var dtData = new C2CRules(dbContext).FindProducts(requestModel,
                keySearch, countryName, lstBrandIds, lstProductTags, ref totalRecords, start, length);
            return Json(dtData, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GetMyOrders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int channel = 0, bool isHideCompleted = true)
        {
            return Json(new C2CRules(dbContext).GetMyOrders(requestModel, CurrentUser().Id, keyword, daterange, channel, isHideCompleted), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// This is function create a associate of  B2C or C2C
        /// </summary>
        /// <param name="linkId">- Type=1 => linkId is a the b2bProfileId, Type=2 => linkId is a the UserId </param>
        /// <param name="Type">1: Businesses, 2: Individual</param>
        /// <returns></returns>
        public ActionResult ConnectC2C(string linkId, int type)
        {
            var responseJson = new C2CRules(dbContext).ConnectC2C(CurrentUser().Id, linkId, type);
            if (responseJson.result && responseJson.Object != null && CurrentQbicleId() != (int)responseJson.Object)
            {
                var qbicleId = (int)responseJson.Object;
                SetCurrentQbicleIdCookies(qbicleId);//B2c Qbicle
                responseJson.Object = qbicleId.Encrypt();//Encrypt qbicleId
            }
            return Json(responseJson, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveAddressForCustomer(TraderAddress address, string country)
        {
            address.Country = new CountriesRules().GetCountryByName(country);
            return Json(new TraderLocationRules(dbContext).SaveAddressForUser(address, CurrentUser().Id));
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
        /// 5: Order by date added
        /// </param>
        /// <param name="contactType">
        /// 0: Show all contact types(Default)
        /// 1: Only individuals
        /// 2: Only pending
        /// </param>
        /// <param name="onlyShowFavourites">Only show favourites</param>
        /// <param name="includeBlocked">Include blocked users</param>
        /// <param name="c2cQbiceActiveId">This is the Id of c2cqbicle set selected. It default is 0 </param>
        /// <returns>PartialView _C2CQbiclesContent</returns>
        public ActionResult LoadC2CQbiclesContent(string keyword, int orderby, int contactType,
            bool isAllShown = true, bool isFavouriteShown = false, bool isRequestShown = false,
            bool isSentShown = false, bool isBlockedShown = false, string c2cQbicleKey = "0",
            int PageIndex = 0)
        {
            var userid = CurrentUser().Id;
            var c2cRule = new C2CRules(dbContext);
            var totalPage = 0;
            var parameter = new CommunityParameter
            {
                UserId = userid,
                Keyword = keyword,
                OrderBy = orderby,
                ContactType = contactType,
                ShownAll = isAllShown,
                ShownFavourite = isFavouriteShown,
                ShownRequest = isRequestShown,
                ShownSent = isSentShown,
                ShownBlocked = isBlockedShown,
                PageIndex = PageIndex
            };
            var listC2CQbicle = c2cRule.GetC2CQbicles(out totalPage, parameter);
            if (c2cQbicleKey == "0")
            {
                var c2cqbicle = new QbicleRules(dbContext).GetQbicleById(CurrentQbicleId());
                if (c2cqbicle != null)
                    c2cQbicleKey = c2cqbicle.Key;
                else
                    c2cQbicleKey = listC2CQbicle.FirstOrDefault()?.QbicleId.Encrypt() ?? "0";
            }
            var qbicleId = int.Parse(c2cQbicleKey.Decrypt());
            SetCurrentQbicleIdCookies(qbicleId);//B2C Qbicle
            ViewBag.C2CQbiceSelectedId = qbicleId;
            //store ui settings

            c2cRule.C2CUiSetting(SystemPageConst.C2C, userid, new C2CSearchQbicleModel
            {
                Orderby = orderby,
                ContactType = contactType,
                OnlyShowFavourites = false,
                IncludeBlocked = false
            });
            var modelString = RenderLoadNextViewToString("~/Views/C2C/_C2CQbiclesContent.cshtml", listC2CQbicle);
            var result = Json(new { ModelString = modelString, ModelCount = totalPage },
                        JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qId"></param>
        /// <param name="linkId"></param>
        /// <param name="type">1: Businesses, 2: Individual</param>
        /// <param name="isFavorite"></param>
        /// <returns></returns>
        public ActionResult SetLikeBy(int qId, string linkId, int type, bool isFavorite = false)
        {
            var ogrConnection = GetOriginatingConnectionIdFromCookies();
            return Json(new C2CRules(dbContext).SetLikeBy(qId, CurrentUser().Id, linkId, type, isFavorite , ogrConnection, ogrConnection));
        }
        public ActionResult LoadModalActivities()
        {
            ViewBag.qbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(CurrentQbicleId(), CurrentUser().Id);
            var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
            ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);
            return PartialView("_ModalActivities");
        }
        public ActionResult LoadC2CQbicleStatusInfo(string key)
        {
            var c2cQbicleId = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            return PartialView("_C2CQbicleStatusContent", new C2CRules(dbContext).GetC2CQbicleById(c2cQbicleId));
        }
        /// <summary>
        /// This is function update status of the C2C
        /// </summary>
        /// <param name="qId"></param>
        /// <param name="status"></param>
        /// <param name="type">1: Businesses, 2: Individual</param>
        /// <returns></returns>
        public ActionResult SetStatusBy(string key, CommsStatus status, int type)
        {
            var qId = int.Parse(key.Decrypt());
            if (type == 1) //For B2C
                return Json(new B2CRules(dbContext).SetStatusByCustomer(qId, CurrentUser().Id, status));
            //For C2C
            return Json(new C2CRules(dbContext).SetStatusBy(qId, CurrentUser().Id, status, GetOriginatingConnectionIdFromCookies()));
        }
        public ActionResult RemoveC2CQbicleById(string key)
        {
            var qbicleId = int.Parse(key.Decrypt());
            return Json(new C2CRules(dbContext).RemoveC2CQbicleById(qbicleId, CurrentUser().Id));
        }
        /// <summary>
        /// This is funtion load activities 
        /// </summary>
        /// <param name="fillterModel">object QbicleFillterModel</param>
        /// <param name="status">CommsStatus</param>
        /// <param name="type">1: Businesses, 2: Individual</param>
        /// <returns></returns>
        public JsonResult LoadMoreC2CActivities(QbicleFillterModel fillterModel, int type = 2)
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
                fillterModel.UserId = CurrentUser().Id;
                C2CQbicle c2cqbicle = null;
                B2CQbicle b2cqbicle = null;
                if (type == 2)
                {
                    c2cqbicle = new C2CRules(dbContext).GetC2CQbicleById(fillterModel.QbicleId);
                    if (c2cqbicle == null)
                        return Json(new { ModelString = "", ModelCount = 0, isHidden = true });
                }
                else if (type == 1)
                {
                    b2cqbicle = new B2CRules(dbContext).GetB2CQbicleById(fillterModel.QbicleId);
                    if (b2cqbicle == null)
                        return Json(new { ModelString = "", ModelCount = 0, isHidden = true });
                }

                var model = qbRule.GetQbicleStreams(fillterModel, user.Timezone, user.DateFormat);
                if (model != null)
                {
                    var isHidden = false;
                    var modelString = "";
                    if (type == 2 && c2cqbicle.Status == CommsStatus.Blocked && fillterModel.Size == 0)
                    {
                        if (c2cqbicle != null)
                            isHidden = c2cqbicle.IsHidden;
                        modelString = RenderLoadNextViewToString("~/Views/C2C/_C2CQbicleStatusContent.cshtml", c2cqbicle) + modelString;
                    }
                    else if (type == 1 && b2cqbicle.Status == CommsStatus.Blocked && fillterModel.Size == 0)
                    {
                        b2cqbicle = new B2CRules(dbContext).GetB2CQbicleById(fillterModel.QbicleId);
                        if (b2cqbicle != null)
                            isHidden = b2cqbicle.IsHidden;
                        ViewBag.B2BProfile = new CommerceRules(dbContext).GetB2bProfileByDomainId(b2cqbicle != null && b2cqbicle.Business != null ? b2cqbicle.Business.Id : 0);
                        modelString = RenderLoadNextViewToString("~/Views/C2C/_BusinessBlockedByCustomer.cshtml", b2cqbicle) + modelString;
                    }
                    else if (type == 1 && model.TotalCount == 0)
                    {
                        ViewBag.BusinessName = new CommerceRules(dbContext).GetB2bProfileByDomainId(b2cqbicle != null && b2cqbicle.Business != null ? b2cqbicle.Business.Id : 0)?.BusinessName;
                        modelString = RenderLoadNextViewToString("~/Views/C2C/_LetsTalk.cshtml", null);
                    }
                    else
                        modelString = ActivityPostHtmlTemplateRules.getQbicleStreamsHtml(model, user.Id, user.Timezone, user.DateFormat);

                    //var ajaxModelCount = data.ModelCount - (loadCountActivity * qbiclePageSize);
                    var result = Json(new { ModelString = modelString, ModelCount = model.TotalCount, isHidden },
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

        public ActionResult GetCommunityTalkNum(string keyword, int contactType)
        {
            var currentUserId = CurrentUser().Id;

            int allNum = 0;
            int favouriteNum = 0;
            int requestNum = 0;
            int sentNum = 0;
            int blockedNum = 0;
            new C2CRules(dbContext).GetCommunityTalkNumByType(CurrentUser().Id, keyword, contactType, ref allNum, ref favouriteNum, ref requestNum, ref sentNum, ref blockedNum);

            var rsObj = new
            {
                allNum,
                favouriteNum,
                requestNum,
                sentNum,
                blockedNum
            };

            return Json(rsObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type">1-Business, 2-Individual</param>
        /// <param name="isViewed"></param>
        /// <returns></returns>
        public ActionResult SetC2CConnectionViewedStatusAndGetShoppingAbility(string key, int type, bool isViewed = true)
        {
            var qId = int.Parse(key.Decrypt());
            return Json(new C2CRules(dbContext).UpdateC2CCommunityTalkViewedStatus(qId, type, CurrentUser().Id, isViewed));
        }

        public ActionResult RemoveQbicleFromUser(string qKey)
        {
            var qId = string.IsNullOrEmpty(qKey) ? 0 : int.Parse(qKey.Decrypt());
            var currentUserId = CurrentUser().Id;

            var updatedResult = new C2CRules(dbContext).RemoveQbicleForUser(currentUserId, qId);
            return Json(updatedResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFeaturedProductForShoppingTab()
        {
            var lstFeaturedProduct = new C2CRules(dbContext).GetCustomizedCommunityFeaturedProduct().Select(e=> new CommunityFeturedProductModel
            {
                BusinessLogo = e.BusinessLogo,
                BusinessName = e.BusinessName,
                CategoryItemName = e.CategoryItemName,
                Id = e.Id,
                LogoImageUri = e.LogoImageUri,
                Price = e.Price,
                PriceStr = e.PriceStr,
                Type = e.Type,
                TypeName = e.TypeName,
                AssociatedCurrencySetting = e.AssociatedCurrencySetting,
                AssociatedCatalog = e.AssociatedCatalog,
                AssociatedCatalogKey = e.AssociatedCatalogKey,
                FeaturedImageURL = e.FeaturedImageURL,
                DisplayOrder = e.DisplayOrder
            });
            return Json(lstFeaturedProduct, JsonRequestBehavior.AllowGet);
        }
    }
}