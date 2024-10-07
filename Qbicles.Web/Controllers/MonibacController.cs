using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Loyalty;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class MonibacController : BaseController
    {
        // GET: Monibac
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MonibacManage()
        {
            var currentUserId = CurrentUser().Id;

            SetCurrentPage(SystemPageConst.MONIBAC);
            ViewBag.CurrentPage = SystemPageConst.MONIBAC;
            ViewBag.CurrentPIN = new StoreCreditRules(dbContext).GetActivePIN(currentUserId);
            ViewBag.ListBusiness = new PromotionRules(dbContext).getAllBusinessHasPromotion();
            return View();
        }

        public ActionResult ShowListConnectedBusiness(int totalLoad, string keysearch)
        {
            var total = 0;
            var lstBusiness = new StoreRules(dbContext).GetMonibacConnectedBusinesses(CurrentUser().Id, ref total, keysearch, totalLoad);
            ViewBag.TotalRecords = total;
            return PartialView("_MonibacConnectedBusiness", lstBusiness);
        }

        public ActionResult GetMonibacInfor(string contactKey)
        {
            var userId = CurrentUser().Id;
            var contact = new TraderContactRules(dbContext).GetTraderContactByKey(contactKey);
            var domainId = contact?.ContactGroup?.Domain?.Id ?? 0;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            var infoModel = new StoreRules(dbContext).GetMonibacBusinessInfor(domainId, userId, currencySettings, contact);
            return PartialView("_MonibacStoreInfoPartial", infoModel);
        }

        public ActionResult ShowPointExchangePartialView(string contactKey)
        {
            var contact = new TraderContactRules(dbContext).GetTraderContactByKey(contactKey);
            var domainId = contact.ContactGroup.Domain.Id;
            var exchangeModel = new StoreCreditRules(dbContext).GetStoreCreditPointExchangeInfo(contactKey, CurrentUser().Id, new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId));
            
            return PartialView("_MonibacStorePointExchangePartial", exchangeModel);
        }

        public ActionResult ShowAccountBalanceExchangePartialView(string contactKey)
        {
            var contact = new TraderContactRules(dbContext).GetTraderContactByKey(contactKey);
            var domainId = contact.ContactGroup.Domain.Id;
            var exchangeModel = new StoreCreditRules(dbContext).GetAccountBalanceExchangeInfo(contactKey, CurrentUser().Id, new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId));
            
            return PartialView("_MonibacAccountBalanceExchangePartial", exchangeModel);
        }
        public ActionResult ShowWidgetCodesPartialView(string businessKey)
        {
            var profileId = int.Parse(businessKey.Decrypt());
            return PartialView("_MonibacWidgetCodes",new CommerceRules(dbContext).GetB2bProfileById(profileId) );
        }

        public ActionResult GenerateStoreCreditFromAccountBalance(string contactKey, decimal exchangeBalance)
        {
            var userId = CurrentUser().Id;
            var updateResult = new StoreCreditRules(dbContext).GenerateCreditFromContactBalance(contactKey, exchangeBalance, userId);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateStoreCreditFromStorePoint(string contactKey, decimal exchangePoint)
        {
            var userId = CurrentUser().Id;
            var updateResult = new StoreCreditRules(dbContext).GenerateCreditFromStorePoint(contactKey, exchangePoint, userId);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateStoreCreditPIN(StoreCreditTransactionReason createdReason, string contactKey = "")
        {
            var generatingResult = new StoreCreditRules(dbContext).GenerateStoreCreditPIN(CurrentUser().Id, createdReason, contactKey);
            return Json(generatingResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadPublishPromotions(PromotionPublishFilterModel filterModel)
        {
            var currentUserInfo = CurrentUser();
            filterModel.currentUserId = currentUserInfo.Id;
            filterModel.timezone = currentUserInfo.Timezone;
            filterModel.dateformat = currentUserInfo.DateFormat;
            var promotions = new PromotionRules(dbContext).GetPublishPromotions(filterModel);
            return Json(new { totalRecords=promotions.totalNumber,htmlContent= RenderLoadNextViewToString ("~/Views/Monibac/_PublishPromotionContent.cshtml",promotions.items) });
        }
        public ActionResult LoadLocationsByBusinessKey(string businessKey)
        {
            var domainId =string.IsNullOrEmpty(businessKey) ? 0 : int.Parse(businessKey.Decrypt());
            var locations= new TraderLocationRules(dbContext).GetTraderLocation(domainId);
            return Json(locations.Select(s => new { label = s.Name, value = s.Id.ToString(), selected=true }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVouchersByUserAndShop(VoucherByUserAndShopModel filterModel)
        {
            var userinfo = CurrentUser();
            filterModel.currentUserId = userinfo.Id;
            filterModel.timezone = userinfo.Timezone;
            var vouchers = new PromotionRules(dbContext).GetVouchersByUserAndShop(filterModel);
            return Json(new { totalRecords = vouchers.totalNumber, htmlContent = RenderLoadNextViewToString("~/Views/Monibac/_MobibacMyVourchers.cshtml", vouchers.items) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVoucherItemMore(string voucherKey)
        {
            var vourcher = new PromotionRules(dbContext).GetVoucherByKey(voucherKey);
            if (voucherKey == null)
                return View("Error");
            var domainId = vourcher.Promotion.Domain.Id;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            ViewBag.CurrencySettings = currencySettings;
            if (vourcher.Promotion.VoucherInfo.Type == VoucherType.ItemDiscount)
            {
                var itemvoucher = vourcher.Promotion.VoucherInfo as ItemDiscountVoucherInfo;
                ViewBag.Item = new TraderItemRules(dbContext).GetItemByDomainIdAndSku(domainId, itemvoucher.ItemSKU);
            }
                
            return PartialView("_MonibacVoucherItemMore", vourcher);
        }
        public ActionResult RemoveVoucher(string voucherKey)
        {
            return Json(new PromotionRules(dbContext).RemoveVoucher(voucherKey));
        }
        public ActionResult ClaimPromotion(string promotionKey, string businessKey)
        {
            return Json(new PromotionRules(dbContext).ClaimPromotion(CurrentUser().Id, promotionKey, businessKey));
        }
        public ActionResult SetLikingUser(string promotionKey, bool isLiked)
        {
            return Json(new PromotionRules(dbContext).SetLikingUser(CurrentUser().Id, promotionKey, isLiked));
        }
        public ActionResult MarkLikePromotion(string promotionKey, bool isLiked)
        {
            return Json(new PromotionRules(dbContext).MarkLikePromotion(CurrentUser().Id, promotionKey, isLiked));
        }
        private string RenderLoadNextViewToString(string viewName, object model)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public ActionResult PromotionDetailView(string promotionKey)
        {
            ViewBag.CurrentPage = SystemPageConst.MONIBAC;
            var promotionId = string.IsNullOrEmpty(promotionKey) ? 0 : int.Parse(promotionKey.Decrypt());
            var promotion = dbContext.Promotions.Where(p => p.Id == promotionId).Include(p => p.PlanType).FirstOrDefault();
            var currentUserId = CurrentUser().Id;
            var currentUserSetting = CurrentUser();
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.AllowClaimNow = promotion.CheckAllowClaimNow(currentUserId, DateTime.UtcNow);

            var businessKey = promotion.Domain.Id.BusinesProfile()?.Key ?? promotion.Domain.Key;
            var businessProfileId = promotion.Domain.Id.BusinesProfile()?.Id ?? promotion.Domain.Id;
            ViewBag.BusinessKey = businessKey;
            ViewBag.BusinessProfileId = businessProfileId;

            ViewBag.RemainHtmlInfo = promotion.CalRemainPromotionInfo(currentUserSetting.Timezone, currentUserSetting.DateFormat, DateTime.UtcNow);
            
            return View(promotion);
        }

        public ActionResult PromotionSharePartialView(string promotionKey)
        {
            ViewBag.SharedPromotionKey = promotionKey;
            var c2cContactUsers = new PromotionRules(dbContext).GetContactsForPromotionShare(CurrentUser().Id);
            return PartialView("_PromotionSharePartial", c2cContactUsers);
        }

        public ActionResult SharePromotion(string promotionKey, int type, string email, string sharedUserIds)
        {            
            var result = new PromotionRules(dbContext).SharePromotion(type, email, promotionKey, sharedUserIds, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}