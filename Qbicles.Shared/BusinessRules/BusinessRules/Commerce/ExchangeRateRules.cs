using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.B2B;
using System;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.BusinessRules.Commerce
{
    public class ExchangeRateRules
    {
        ApplicationDbContext dbContext;
        public ExchangeRateRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ExchangeRate GetExchangeRateByOrderId(int b2bOrderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, b2bOrderId);

                var exchangeRate = dbContext.ExchangeRates.FirstOrDefault(s => s.Order.Id == b2bOrderId);
                if(exchangeRate!=null)
                {
                    return exchangeRate;
                }else
                {
                    var order = dbContext.B2BTradeOrders.Find(b2bOrderId);
                    if (order == null)
                        return null;
                    CreateExchangeRateByOrder(order);
                    exchangeRate= dbContext.ExchangeRates.FirstOrDefault(s => s.Order.Id == b2bOrderId);
                    return exchangeRate;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, b2bOrderId);
                return null;
            }
        }
        public ReturnJsonModel CreateExchangeRateByOrder(TradeOrderB2B b2BOrder)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, b2BOrder);

                if (b2BOrder == null)
                    return returnModel;
                var currencyRule = new CurrencySettingRules(dbContext);
                var sellingCurrency = currencyRule.GetCurrencySettingByDomain(b2BOrder.SellingDomain.Id);
                var buyingCurrency = currencyRule.GetCurrencySettingByDomain(b2BOrder.BuyingDomain.Id);
               var  exchangeRate = new ExchangeRate();
                exchangeRate.BuyingDomainCurrencySymbol = buyingCurrency.CurrencySymbol;
                exchangeRate.AmountBuyerCurrency = 1;
                exchangeRate.SellingDomainCurrencySymbol = sellingCurrency.CurrencySymbol;
                exchangeRate.AmountSellerCurrency = 1;
                exchangeRate.Order = b2BOrder;
                exchangeRate.CreatedDate = DateTime.UtcNow;
                exchangeRate.CreatedBy = b2BOrder.CreatedBy;
                exchangeRate.LastUpdatedDate = exchangeRate.CreatedDate;
                exchangeRate.LastUpdatedBy = b2BOrder.CreatedBy;
                exchangeRate.ExchangeRateValue = 1;
                dbContext.ExchangeRates.Add(exchangeRate);
                returnModel.result = dbContext.SaveChanges()>0;
                return returnModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, b2BOrder);
                return returnModel;
            }
        }
        public ReturnJsonModel UpdateExchangeRateById(int id, decimal amountSeller,decimal amountBuyer)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id, amountSeller, amountBuyer);
                var exchangeRate = dbContext.ExchangeRates.Find(id);
                if (exchangeRate == null)
                    return returnModel;
                exchangeRate.AmountSellerCurrency = amountSeller;
                exchangeRate.AmountBuyerCurrency = amountBuyer;
                exchangeRate.ExchangeRateValue = amountBuyer/amountSeller;
                returnModel.result = dbContext.SaveChanges() > 0;
                returnModel.Object = exchangeRate.ExchangeRateValue;
                return returnModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, amountSeller, amountBuyer);
                return returnModel;
            }
        }
    }
}
