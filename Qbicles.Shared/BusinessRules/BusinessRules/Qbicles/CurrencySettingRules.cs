using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class CurrencySettingRules
    {
        ApplicationDbContext dbContext;
        public CurrencySettingRules()
        {
        }
        public CurrencySettingRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public CurrencySetting GetCurrencySettingByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get currency setting by domain", null, null, domainId);

                var currency = dbContext.CurrencySettings.AsNoTracking().Where(s => s.Domain.Id == domainId).FirstOrDefault();
                if (currency == null)
                {
                    using (dbContext)
                    {
                        currency = new CurrencySetting
                        {
                            CurrencySymbol = "₦",
                            SymbolDisplay = CurrencySetting.SymbolDisplayEnum.Prefixed,
                            DecimalPlace = CurrencySetting.DecimalPlaceEnum.Two,
                            Domain = dbContext.Domains.Find(domainId)
                        };
                        dbContext.CurrencySettings.Add(currency);
                        dbContext.SaveChanges();
                    }                    
                }
                return currency;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }
        public ReturnJsonModel SaveCurrencyConfiguration(CurrencySetting currencySetting)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save currency configuration", null, null, currencySetting);

                var dbcurrency = dbContext.CurrencySettings.FirstOrDefault(s => s.Domain.Id == currencySetting.Domain.Id);
                if (dbcurrency != null)
                {
                    dbcurrency.CurrencySymbol = currencySetting.CurrencySymbol;
                    dbcurrency.SymbolDisplay = currencySetting.SymbolDisplay;
                    dbcurrency.DecimalPlace = currencySetting.DecimalPlace;
                    if (dbContext.Entry(dbcurrency).State == EntityState.Detached)
                        dbContext.CurrencySettings.Attach(dbcurrency);
                    dbContext.Entry(dbcurrency).State = EntityState.Modified;
                }
                else
                {
                    dbcurrency = new CurrencySetting
                    {
                        CurrencySymbol = currencySetting.CurrencySymbol,
                        SymbolDisplay = currencySetting.SymbolDisplay,
                        DecimalPlace = currencySetting.DecimalPlace,
                        Domain = currencySetting.Domain
                    };
                    dbContext.CurrencySettings.Add(dbcurrency);
                    dbContext.Entry(dbcurrency).State = EntityState.Added;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currencySetting);
            }
            return refModel;
        }
    }
}
