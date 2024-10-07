using Qbicles.BusinessRules.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Loyalty;
using Qbicles.Models.MicroQbicleStream;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroMoniBackRules : MicroRulesBase
    {
        public MicroMoniBackRules(MicroContext microContext) : base(microContext)
        {
        }

        public object GetMyStores(int pageIndex, string search)
        {
            var total = 0;
            var data = new StoreRules(dbContext).MicroGetMonibacConnectedBusinesses(CurrentUser.Id, ref total, search, pageIndex);

            return new
            {
                TotalPage = total,
                Monibacks = data.ToMonibackMyStore()
            };
        }

        public MicroMonibackMyStoreInfo GetMyStoreInfo(string key)
        {
            var userId = CurrentUser.Id;
            var contact = new TraderContactRules(dbContext).GetTraderContactByKey(key);
            var domainId = contact?.ContactGroup?.Domain?.Id ?? 0;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            var infoModel = new StoreRules(dbContext).GetMonibacBusinessInfor(domainId, userId, currencySettings, contact);

            return infoModel.ToMonibackMyStoreInfo();
        }

        public StoreCreditExchangeModel GetStoreCreditBalanceInfo(string key)
        {
            var contact = new TraderContactRules(dbContext).GetTraderContactByKey(key);
            var domainId = contact.ContactGroup.Domain.Id;
            var exchangeModel = new StoreCreditRules(dbContext).GetAccountBalanceExchangeInfo(key, CurrentUser.Id, new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId));

            return exchangeModel;
        }

        public StoreCreditExchangeModel GetStoreCreditPointInfo(string key)
        {
            var contact = new TraderContactRules(dbContext).GetTraderContactByKey(key);
            var domainId = contact.ContactGroup.Domain.Id;
            var exchangeModel = new StoreCreditRules(dbContext).GetStoreCreditPointExchangeInfo(key, CurrentUser.Id, new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId));

            return exchangeModel;
        }

        public StoreCreditExchangeModel Balance2Credit(StoreCreditExchangeModel balance)
        {
            var currentSetting = new Models.CurrencySetting
            {
                CurrencySymbol = balance.CurrencySymbol,
                SymbolDisplay = balance.SymbolDisplay,
                DecimalPlace = balance.DecimalPlace
            };
            var accBalance = balance.AccountBalance;
            var inputBalance = balance.ConvertValue;
            if (inputBalance > accBalance)
            {
                inputBalance = accBalance;
            };
            var balanceReceived = inputBalance;
            var storePointAccountBalance = balance.StoreCredit;
            var newCreditBalance = balanceReceived + storePointAccountBalance;

            balance.StoreCredit = newCreditBalance;
            balance.StoreCreditString = newCreditBalance.ToCurrencySymbol(currentSetting);
            balance.CreditReceived = balanceReceived.ToCurrencySymbol(currentSetting);

            return balance;
        }

        public StoreCreditExchangeModel Point2Credit(StoreCreditExchangeModel point)
        {
            var currentSetting = new Models.CurrencySetting
            {
                CurrencySymbol = point.CurrencySymbol,
                SymbolDisplay = point.SymbolDisplay,
                DecimalPlace = point.DecimalPlace
            };
            var exchangeRate = point.ExchangeRate;
            var pointBalance = point.Point;
            var inputPoint = point.ConvertValue;
            if (inputPoint > pointBalance)
            {
                point.ConvertValue = pointBalance;
            };
            var pointReceived = inputPoint * exchangeRate;
            var storePointAccountBalance = point.StoreCredit;
            var newCreditBalance = pointReceived + storePointAccountBalance;
            point.StoreCredit = newCreditBalance;
            point.StoreCreditString = newCreditBalance.ToCurrencySymbol(currentSetting);
            point.CreditReceived = pointReceived.ToCurrencySymbol(currentSetting);

            return point;
        }

        public ReturnJsonModel ConvertBalance2Credit(StoreCreditExchangeModel balance)
        {
            return new StoreCreditRules(dbContext).GenerateCreditFromContactBalance(balance.ContactKey, balance.ConvertValue, CurrentUser.Id);
        }

        public ReturnJsonModel ConvertPoint2Credit(StoreCreditExchangeModel point)
        {
            return new StoreCreditRules(dbContext).GenerateCreditFromStorePoint(point.ContactKey, point.ConvertValue, CurrentUser.Id);
        }

        public ReturnJsonModel GenerateStoreCreditSecurePIN()
        {
            return new StoreCreditRules(dbContext).GenerateStoreCreditPIN(CurrentUser.Id, StoreCreditTransactionReason.GeneratedFromUser);
        }

        public object GetVouchers(VoucherByUserAndShopModel filterModel)
        {
            filterModel.isRedeemed = filterModel.isExpired;
            filterModel.isCountRecords = true;
            filterModel.pageSize = filterModel.pageNumber * 15;
            filterModel.pageNumber = 15;
            filterModel.currentUserId = CurrentUser.Id;
            filterModel.timezone = CurrentUser.Timezone;
            var voucher = new PromotionRules(dbContext).GetVouchersByUserAndShop(filterModel);
            var totalePages = voucher.totalNumber / 15 == 0 ? 1 : voucher.totalNumber / 15;
            return new { totalePages, vouchers = voucher.items, totalVouchers = voucher.totalNumber };
        }

        public string GetStoreCreditSecurePIN()
        {
            return new StoreCreditRules(dbContext).GetActivePIN(CurrentUser.Id)?.PIN ?? "0000";
        }

        public object GetDeals(PromotionPublishFilterModel filterModel)
        {
            filterModel.currentUserId = CurrentUser.Id;
            filterModel.timezone = CurrentUser.Timezone;
            filterModel.dateformat = CurrentUser.DateFormat;
            filterModel.isLoadTotalRecord = true;
            filterModel.pageSize = filterModel.pageNumber * 15;
            filterModel.pageNumber = 15;

            var promotions = new PromotionRules(dbContext).GetPublishPromotions(filterModel);
            var totalePages = promotions.totalNumber / 15 == 0 ? 1 : promotions.totalNumber / 15;
            return new { totalePages, promotions = promotions.items };
        }

        /// <summary>
        /// Get nearby deals using user geolocations
        /// </summary>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public object GetNearByDeals(PromotionPublishFilterModel filterModel, string longitude, string latitude)
        {
            filterModel.currentUserId = CurrentUser.Id;
            filterModel.timezone = CurrentUser.Timezone;
            filterModel.dateformat = CurrentUser.DateFormat;
            filterModel.isLoadTotalRecord = true;
            filterModel.pageSize = filterModel.pageNumber * 15;
            filterModel.pageNumber = 15;

            //TODO: use the longitude and lat to fetch promotions

            var promotions = new PromotionRules(dbContext).GetPublishPromotions(filterModel);
            var totalePages = promotions.totalNumber / 15 == 0 ? 1 : promotions.totalNumber / 15;
            return new { totalePages, promotions = promotions.items };
        }

        public ReturnJsonModel VoucherClaim(VoucherClaimParameter voucher)
        {
            return new PromotionRules(dbContext).ClaimPromotion(CurrentUser.Id, voucher.PromotionKey, voucher.BusinessKey);
        }

        public ReturnJsonModel VoucherLike(string promotionKey, bool like)
        {
            return new PromotionRules(dbContext).SetLikingUser(CurrentUser.Id, promotionKey, like);
        }

        public ReturnJsonModel VoucherBookmark(string promotionKey, bool mark)
        {
            return new PromotionRules(dbContext).MarkLikePromotion(CurrentUser.Id, promotionKey, mark);
        }

        public List<MicroContact> GetContactsForPromotionShare()
        {
            var c2cContactUsers = new PromotionRules(dbContext).GetContactsForPromotionShare(CurrentUser.Id);
            return c2cContactUsers.ToMicroUser();
        }

        public async Task<ReturnJsonModel> SharePromotionContact(HighlightPromotionShareParameter share)
        {
            return await new PromotionRules(dbContext).SharePromotion(1, "", share.PromotionKey, share.ContactIds.ToJson(), CurrentUser.Id);
        }

        public async Task<ReturnJsonModel> SharePromotionEmail(HighlightPromotionShareParameter share)
        {
            return await (new PromotionRules(dbContext).SharePromotion(2, share.Email, share.PromotionKey, "", CurrentUser.Id));
        }

        public object GetVoucherInfo(int id)
        {
            var vourcher = new PromotionRules(dbContext).GetVoucherByKey(id.Encrypt());

            return new
            {
                vourcher.Id,
                vourcher.Key,
                vourcher.Promotion.Name,
                FeaturedImage = vourcher.Promotion.FeaturedImageUri.ToUri(),
                Type = vourcher.Promotion.VoucherInfo.Type.GetDescription()
            };
        }

        public ReturnJsonModel AddToMyStores(string businessKey)
        {
            return new PromotionRules(dbContext).AddToMyStores(CurrentUser.Id, businessKey);
        }
    }
}