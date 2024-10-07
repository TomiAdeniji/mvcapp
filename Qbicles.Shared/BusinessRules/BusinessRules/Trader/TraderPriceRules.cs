using System;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader.Pricing;
using System.Linq;
using System.Reflection;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models.Trader;
using Qbicles.Models;
using System.Data.Entity;
using Qbicles.Models.Trader.SalesChannel;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderPriceRules
    {
        private ApplicationDbContext _db;
        private ReturnJsonModel refModel = new ReturnJsonModel();
        public TraderPriceRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();

        public Price GetPriceByLocationIdItemId(int locationid, int itemid, SalesChannelEnum saleChannel) {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationid, itemid, saleChannel);
                return DbContext.TraderPrices.FirstOrDefault(q => q.Location.Id == locationid && q.Item.Id == itemid && q.SalesChannel == saleChannel) ?? (new Price());
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationid, itemid, saleChannel);
                return  new Price();
            }
        }


        public List<Price> GetPriceByLocationIdItemId(int locationid, int itemid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationid, itemid);
                return DbContext.TraderPrices.Where(q => q.Location.Id == locationid && q.Item.Id == itemid).ToList() ;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationid, itemid);
                return new List<Price>();
            }
        }

        /// <summary>
        /// IF, a Price already exists for the Item at the location of the PriceBook, for the SalesChannel of the PriceBook,a Price is to be created for the Item,
        /// it MUST be OVERWRITTEN.
        /// </summary>
        public void CreatePriceByLocationIdItemId(TraderLocation location, SalesChannelEnum saleChannel, decimal priceValue, decimal priceValueUpdate, TraderItem item,string userId, bool isExstingOverwritten = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, location, saleChannel, item, userId);

                var currentUser = DbContext.QbicleUser.Find(userId);
                var priceOverwrite= DbContext.TraderPrices.FirstOrDefault(q => q.Location.Id == location.Id && q.Item.Id == item.Id && q.SalesChannel == saleChannel);
                if (priceOverwrite == null)
                {
                    var price = new Price
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Item = item,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = currentUser,
                        Location = location,
                        SalesChannel = saleChannel,
                        NetPrice = priceValue //priceBookPriceDb.FullPrice
                    };

                    //Calculate Price Taxes and Gross Price for price object
                    price.GrossPrice = price.NetPrice * (1 + item.SumTaxRates(true));
                    var lstTaxes = item.TaxRates.Where(s => !s.IsPurchaseTax).ToList();
                    if (lstTaxes != null && lstTaxes.Count > 0)
                    {
                        foreach (var taxitem in lstTaxes)
                        {
                            var priceTaxItem = price.Taxes.FirstOrDefault(p => p.TaxName == taxitem.Name);
                            if (priceTaxItem == null || priceTaxItem.Id == 0)
                            {
                                var staticTaxItem = new TaxRateRules(DbContext).CloneStaticTaxRateById(taxitem.Id);
                                priceTaxItem = new PriceTax
                                {
                                    TaxName = taxitem.Name,
                                    Rate = taxitem.Rate,
                                    TaxRate = staticTaxItem
                                };
                                priceTaxItem.Amount = price.NetPrice * (taxitem.Rate / 100);
                                DbContext.Entry(priceTaxItem).State = EntityState.Added;
                                DbContext.TraderPriceTaxes.Add(priceTaxItem);
                                price.Taxes.Add(priceTaxItem);
                            }
                            else
                            {
                                priceTaxItem.Amount = price.NetPrice * (taxitem.Rate / 100);
                                DbContext.Entry(priceTaxItem).State = EntityState.Modified;
                            }
                        }
                        DbContext.SaveChanges();
                    }
                    price.Taxes.RemoveAll(p => !lstTaxes.Any(x => x.Name == p.TaxName));
                    price.TotalTaxAmount = price.Taxes.Sum(p => p.Amount);

                    var priceLog = new PriceLog
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Item = item,
                        Location = price.Location,
                        ParentPrice = price,
                        Value = priceValue,
                        GrossPrice = price.GrossPrice,
                        Taxes = price.Taxes
                    };
                    DbContext.TraderPrices.Add(price);
                    DbContext.TraderPriceLogs.Add(priceLog);
                }
                else if(isExstingOverwritten)
                {
                    priceOverwrite.NetPrice = priceValueUpdate;
                    priceOverwrite.LastUpdateDate = DateTime.UtcNow;
                    priceOverwrite.LastUpdatedBy = currentUser;
                    priceOverwrite.GrossPrice = priceOverwrite.NetPrice * (1 + item.SumTaxRates(true));

                    //Calculate price taxes value
                    var lstTaxes = item.TaxRates.Where(s => !s.IsPurchaseTax).ToList();
                    if (lstTaxes != null && lstTaxes.Count > 0)
                    {
                        foreach (var taxitem in lstTaxes)
                        {
                            var priceTaxItem = priceOverwrite.Taxes.FirstOrDefault(p => p.TaxName == taxitem.Name);
                            if (priceTaxItem == null || priceTaxItem.Id == 0)
                            {
                                var staticTaxItem = new TaxRateRules(DbContext).CloneStaticTaxRateById(taxitem.Id);
                                priceTaxItem = new PriceTax
                                {
                                    TaxName = taxitem.Name,
                                    Rate = taxitem.Rate,
                                    TaxRate = staticTaxItem
                                };
                                priceTaxItem.Amount = priceOverwrite.NetPrice * (taxitem.Rate / 100);
                                DbContext.Entry(priceTaxItem).State = EntityState.Added;
                                DbContext.TraderPriceTaxes.Add(priceTaxItem);
                                priceOverwrite.Taxes.Add(priceTaxItem);
                            }
                            else
                            {
                                priceTaxItem.Amount = priceOverwrite.NetPrice * (taxitem.Rate / 100);
                                DbContext.Entry(priceTaxItem).State = EntityState.Modified;
                            }
                        }
                        DbContext.SaveChanges();
                    }
                    //Remove all tax prices that the Price does not contain
                    priceOverwrite.Taxes.RemoveAll(p => !lstTaxes.Any(x => x.Name == p.TaxName));
                    priceOverwrite.TotalTaxAmount = priceOverwrite.Taxes.Sum(p => p.Amount);

                    if (DbContext.Entry(priceOverwrite).State == EntityState.Detached)
                        DbContext.TraderPrices.Attach(priceOverwrite);
                    DbContext.Entry(priceOverwrite).State = EntityState.Modified;


                    var priceLog = new PriceLog
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Item = item,
                        Location = priceOverwrite.Location,
                        ParentPrice = priceOverwrite,
                        Value = priceOverwrite.NetPrice
                    };
                    DbContext.TraderPriceLogs.Add(priceLog);
                }
                DbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, location, saleChannel, item, userId);
            }
        }
        /// <summary>
        /// Get TraderPrice by Location, SalesChannel, TraderItem if the TraderPrice not exist then create a new TraderPrice
        /// </summary>
        /// <param name="location">TraderLocation</param>
        /// <param name="saleChannel">SalesChannelEnum</param>
        /// <param name="item">TraderItem</param>
        /// <param name="currentUser">currentUser</param>
        /// <param name="initPriceValue">This is value init when create a new TraderPrice</param>
        /// <returns></returns>
        public Price GetPrice(TraderLocation location, SalesChannelEnum saleChannel, TraderItem item, ApplicationUser currentUser, decimal initPriceValue=0)
        {
            var price = DbContext.TraderPrices.FirstOrDefault(q => q.Item.Id == item.Id && q.Location.Id == location.Id && q.SalesChannel == saleChannel);
            if (price != null)
                return price;
            else
            {
                decimal priceValue = 0;
                if (initPriceValue > 0)
                    priceValue = initPriceValue;
                else
                {
                    var InventoryDetail = item.InventoryDetails.FirstOrDefault(s => s.Location.Id == location.Id);
                    priceValue = InventoryDetail != null ? InventoryDetail.AverageCost : 0;
                }
                price = new Price
                {
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    Item = item,
                    LastUpdateDate = DateTime.UtcNow,
                    LastUpdatedBy = currentUser,
                    Location = location,
                    SalesChannel = saleChannel,
                    NetPrice = priceValue
                };
                var priceLog = new PriceLog
                {
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    Item = item,
                    Location = price.Location,
                    ParentPrice = price,
                    Value = priceValue
                };
                DbContext.TraderPrices.Add(price);
                DbContext.TraderPriceLogs.Add(priceLog);
                return price;
            }
        }
    }
}
