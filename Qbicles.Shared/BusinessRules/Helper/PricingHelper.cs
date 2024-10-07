using Qbicles.BusinessRules.Model;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader.Pricing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Helper
{
    public class PricingHelper
    {
        private readonly ApplicationDbContext _dbContext;
        public PricingHelper(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void UpdateGrossPriceOfVariantPrice(int variantId, decimal grossPrice)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, variantId, grossPrice);

                var variantInDb = _dbContext.PosVariants.FirstOrDefault(p => p.Id == variantId);
                if(variantInDb != null)
                {
                    // Update the GrossPrice, NetPrice and
                    if (variantInDb.Price == null)
                    {
                        variantInDb.Price = new CatalogPrice()
                        {
                            GrossPrice = 0,
                            NetPrice = 0,
                            TotalTaxAmount = 0,
                            Taxes = new List<PriceTax>()
                        };
                        _dbContext.Entry(variantInDb.Price).State = EntityState.Added;
                        _dbContext.CatalogPrices.Add(variantInDb.Price);
                        _dbContext.SaveChanges();
                    }
                    variantInDb.Price.GrossPrice = grossPrice;
                    var totalTaxPercentage = variantInDb.Price.Taxes.Sum(p => p.Rate);
                    variantInDb.Price.NetPrice = variantInDb.Price.GrossPrice / ((100 + totalTaxPercentage) / 100);
                    variantInDb.Price.TotalTaxAmount = variantInDb.Price.GrossPrice - variantInDb.Price.NetPrice;
                    variantInDb.Price.Taxes.ForEach(taxItem =>
                    {
                        taxItem.Amount = variantInDb.Price.NetPrice * (taxItem.Rate / 100);
                    });
                    _dbContext.Entry(variantInDb.Price).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, variantId, grossPrice);
                return;
            }
        }

        public void UpdateGrossPriceOfExtraPrice(int extraId, decimal grossPrice)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, extraId, grossPrice);

                var extraInDb = _dbContext.PosExtras.FirstOrDefault(p => p.Id == extraId);
                if (extraInDb != null)
                {
                    // Update the GrossPrice, NetPrice and
                    if (extraInDb.Price == null)
                    {
                        extraInDb.Price = new CatalogPrice()
                        {
                            GrossPrice = 0,
                            NetPrice = 0,
                            TotalTaxAmount = 0,
                            Taxes = new List<PriceTax>()
                        };
                        _dbContext.Entry(extraInDb.Price).State = EntityState.Added;
                        _dbContext.CatalogPrices.Add(extraInDb.Price);
                        _dbContext.SaveChanges();
                    }
                    extraInDb.Price.GrossPrice = grossPrice;
                    var totalTaxPercentage = extraInDb.Price.Taxes.Sum(p => p.Rate);
                    extraInDb.Price.NetPrice = extraInDb.Price.GrossPrice / ((100 + totalTaxPercentage) / 100);
                    extraInDb.Price.TotalTaxAmount = extraInDb.Price.GrossPrice - extraInDb.Price.NetPrice;
                    extraInDb.Price.Taxes.ForEach(taxItem =>
                    {
                        taxItem.Amount = extraInDb.Price.NetPrice * (taxItem.Rate / 100);
                    });
                    _dbContext.Entry(extraInDb.Price).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, extraId, grossPrice);
                return;
            }
        }
    }
}
