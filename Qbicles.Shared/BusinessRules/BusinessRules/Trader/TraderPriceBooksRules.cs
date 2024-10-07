using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderPriceBooksRules
    {
        private ApplicationDbContext _db;
        private ReturnJsonModel refModel = new ReturnJsonModel();
        public TraderPriceBooksRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();
        //Pricebook
        public List<PriceBook> GetPriceBooks(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);
                return DbContext.TraderPriceBooks.Where(e => e.Location.Id == locationId)
                    .ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId);
                return new List<PriceBook>();
            }
        }

        public List<TraderGroup> ProductGroupByChannel(int locationId, SalesChannelEnum channel, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, channel, domainId);
                var prices = DbContext.TraderPriceBooks.Where(e => e.Location.Id == locationId && e.SalesChannel == channel && e.Domain.Id == domainId).ToList();
                var groupExists = new List<int>();
                prices.ForEach(price =>
                {
                    price.AssociatedProductGroups.ForEach(group =>
                    {
                        groupExists.Add(group.Id);
                    });
                });
                var groups = new TraderGroupRules(DbContext).GetNotInIds(groupExists, domainId);
                return groups;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, channel, domainId);
                return new List<TraderGroup>();
            }
        }

        public PriceBook GetPriceBookById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderPriceBooks.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new PriceBook();
            }
        }

        public ReturnJsonModel CheckExistName(int pricebookId, string pricebookName, int domainId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, pricebookId, pricebookName, domainId);
                refModel.result = pricebookId > 0 ? DbContext.TraderPriceBooks.Any(x => x.Id != pricebookId && x.Domain.Id == domainId && x.Location.Id == locationId && x.Name == pricebookName) : DbContext.TraderPriceBooks.Any(x => x.Domain.Id == domainId && x.Location.Id == locationId && x.Name == pricebookName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pricebookId, pricebookName, domainId);
                refModel.result = false;
            }

            return refModel;
        }

        public ReturnJsonModel SavePriceBook(PriceBook priceBook, string userId)
        {
            try
            {
                //if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePriceBook - priceBook", userId, null, priceBook);

                var location = new TraderLocationRules(DbContext).GetById(priceBook.Location.Id);
                //if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePriceBook - Location", userId, location, priceBook.Location);

                var productGroupIds = priceBook.AssociatedProductGroups.Select(e => e.Id);
                //if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePriceBook - productGroupIds", userId, productGroupIds, priceBook.AssociatedProductGroups);

                var productGroups = new TraderGroupRules(DbContext).GetByIds(productGroupIds);
                //if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePriceBook - productGroupIds", userId, productGroups, productGroupIds);


                if (priceBook.Id == 0)
                {
                    var price = new PriceBook
                    {
                        Name = priceBook.Name,
                        AssociatedProductGroups = productGroups,
                        CreatedBy = DbContext.QbicleUser.Find(userId),
                        CreatedDate = DateTime.UtcNow,
                        Description = priceBook.Description,
                        Domain = location.Domain,
                        Location = location,
                        SalesChannel = priceBook.SalesChannel,
                        Versions = new List<PriceBookVersion>()
                    };
                    refModel.actionVal = 1;
                    DbContext.Entry(price).State = EntityState.Added;
                    DbContext.TraderPriceBooks.Add(price);
                    DbContext.SaveChanges();
                    refModel.msgId = price.Id.ToString();
                }
                else
                {
                    var priceDb = GetPriceBookById(priceBook.Id);
                    //location,domain,created not change
                    priceDb.Name = priceBook.Name;
                    priceDb.Description = priceBook.Description;
                    priceDb.SalesChannel = priceBook.SalesChannel;
                    priceDb.AssociatedProductGroups.Clear();
                    priceDb.AssociatedProductGroups = productGroups;
                    //isopen,version continue here

                    refModel.actionVal = 2;
                    if (DbContext.Entry(priceDb).State == EntityState.Detached)
                        DbContext.TraderPriceBooks.Attach(priceDb);
                    DbContext.Entry(priceDb).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }

                refModel.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        //Pricebook version
        public List<PriceBookVersion> GetPricebookVersionsByPricebookId(int pricebookId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, pricebookId);
                return DbContext.PriceBookVersions.Where(p => p.ParentPriceBook.Id == pricebookId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, pricebookId);
                return new List<PriceBookVersion>();
            }
        }

        public List<PriceBookInstance> GetPriceBookInstanceByVersionId(int versionId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, versionId);
                return DbContext.TraderPriceBookInstances.Where(e => e.ParentPriceBookVersion.Id == versionId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, versionId);
                return new List<PriceBookInstance>();
            }

        }

        public PriceBookInstance GetPriceBookInstanceById(int instanceId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, instanceId);
                return DbContext.TraderPriceBookInstances.FirstOrDefault(e => e.Id == instanceId);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, instanceId);
                return new PriceBookInstance();
            }
        }

        private ProductGroupPriceDefaults GetProductGroupPriceDefaultId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderProductGroupPriceDefaults.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new ProductGroupPriceDefaults();
            }
        }
        private PriceBookPrice GetPriceBookPriceId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderPriceBookPrices.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new PriceBookPrice();
            }
        }
        /// <summary>
        /// Get collection price books by instance
        /// </summary>
        /// <param name="instanceId">required value</param>
        /// <param name="groupId">if GroupId # 0 then filter items by group id</param>
        /// <returns></returns>
        public List<PriceBookPrice> GetPriceBookPricesByInstance(int instanceId, int groupId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, instanceId, groupId);
                return groupId == 0 ?
                    DbContext.TraderPriceBookPrices.Where(i => i.ParentPriceBookInstance.Id == instanceId).ToList() :
                    DbContext.TraderPriceBookPrices.Where(i => i.ParentPriceBookInstance.Id == instanceId && i.Item.Group.Id == groupId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, instanceId, groupId);
                return new List<PriceBookPrice>();
            }
        }

        public MarkupDiscountModel GetMarkupDiscountModel(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var info = DbContext.TraderProductGroupPriceDefaults.FirstOrDefault(e => e.Id == id);
                var domainId = info.ProductGroup != null && info.ProductGroup.Domain != null ? info.ProductGroup.Domain.Id : 0;
                var currencySettings = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
                return new MarkupDiscountModel
                {
                    Id = info.Id,
                    Discount = info.Discount,
                    MarkUp = info.MarkUp,
                    DiscountPercentage = info.IsDiscountPercentage ? "%" : currencySettings.CurrencySymbol,
                    MarkupPercentage = info.IsMarkupPercentage ? "%" : currencySettings.CurrencySymbol
                };
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new MarkupDiscountModel();
            }
        }

        public ReturnJsonModel ApplyMarkupDiscount(MarkupDiscountModel markupDiscount)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, markupDiscount);
                var info = GetProductGroupPriceDefaultId(markupDiscount.Id);
                var priceBookPrices =
                    info.ParentInstance.PriceBookPrices.Where(i => i.Item.Group.Id == info.ProductGroup.Id).ToList();

                priceBookPrices.ForEach(priceBookPrice =>
                {
                    switch (markupDiscount.ApplyType)
                    {
                        case MarkupDiscountApply.Apply:
                            //Markup
                            if (priceBookPrice.IsMarkupManuallyUpdated)
                                break;
                            //In the collection of associated PriceBookPrices, WHERE the PriceBookPrice.IsMarkupManuallyUpdated is set to False
                            priceBookPrice.MarkUp = markupDiscount.MarkUp;
                            priceBookPrice.IsMarkupPercentage = markupDiscount.MarkupPercentage == "%";

                            //Discount
                            if (priceBookPrice.IsDiscountManuallyUpdated)
                                break;
                            //In the collection of associated PriceBookPrices,WHERE the PriceBookPrice.IsDiscountManuallyUpdated is set to False
                            priceBookPrice.IsMarkupManuallyUpdated = false;
                            priceBookPrice.Discount = markupDiscount.Discount;
                            priceBookPrice.IsDiscountPercentage = markupDiscount.DiscountPercentage == "%";
                            break;
                        case MarkupDiscountApply.ApplyOverwrite:
                            //Markup: In the collection of associated PriceBookPrices
                            priceBookPrice.MarkUp = markupDiscount.MarkUp;
                            priceBookPrice.IsMarkupPercentage = markupDiscount.MarkupPercentage == "%";
                            priceBookPrice.IsMarkupManuallyUpdated = false;

                            //Discount: In the collection of associated PriceBookPrices
                            priceBookPrice.Discount = markupDiscount.Discount;
                            priceBookPrice.IsDiscountPercentage = markupDiscount.DiscountPercentage == "%";
                            priceBookPrice.IsDiscountManuallyUpdated = false;

                            break;
                    }
                    CalculatePriceBookPrice(priceBookPrice, priceBookPrice.ParentPriceBookInstance.IsPriceCalWithAvgCost, false);
                });

                info.MarkUp = markupDiscount.MarkUp;
                info.Discount = markupDiscount.Discount;
                info.IsDiscountPercentage = markupDiscount.DiscountPercentage == "%";
                info.IsMarkupPercentage = markupDiscount.MarkupPercentage == "%";

                DbContext.SaveChanges();
                refModel.result = true;
                refModel.actionVal = 1;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, markupDiscount);
                refModel.result = false;
                refModel.msg = e.Message;
                refModel.actionVal = 3;
            }
            return refModel;
        }

        public List<PriceBookPrice> RecalculatePrices(int instanceId, int groupId, RecalculatePricesType type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, instanceId, groupId, type);
                var priceBookPrices = GetPriceBookPricesByInstance(instanceId, groupId);
                foreach (var priceBookPrice in priceBookPrices)
                {
                    CalculatePriceBookPrice(priceBookPrice, type == RecalculatePricesType.AverageCost ? true : false, false);
                }

                DbContext.SaveChanges();

                var instance = GetPriceBookInstanceById(instanceId);
                switch (type)
                {
                    case RecalculatePricesType.AverageCost:
                        instance.IsPriceCalWithAvgCost = true;
                        break;
                    case RecalculatePricesType.LatestCost:
                        instance.IsPriceCalWithAvgCost = false;
                        break;
                }
                if (DbContext.Entry(instance).State == EntityState.Detached)
                    DbContext.TraderPriceBookInstances.Attach(instance);
                DbContext.Entry(instance).State = EntityState.Modified;
                DbContext.SaveChanges();
                return priceBookPrices;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, instanceId, groupId, type);
                return new List<PriceBookPrice>();
            }
        }

        public ReturnJsonModel SavePriceBookPrices(List<PriceBookPrice> priceBookPrices, string status,
            string versionName, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, priceBookPrices, status, versionName);
                var priceBookId = 0;
                if (priceBookPrices != null)
                {
                    var currentUser = DbContext.QbicleUser.Find(userId);
                    foreach (var priceBookPrice in priceBookPrices)
                    {
                        var priceBookPriceDb = GetPriceBookPriceId(priceBookPrice.Id);
                        priceBookId = priceBookPriceDb.ParentPriceBookInstance.ParentPriceBookVersion.ParentPriceBook.Id;

                        priceBookPriceDb.IsPriceManuallyUpdated = priceBookPrice.Price != 0;

                        if (priceBookPriceDb.MarkUp != priceBookPrice.MarkUp)
                            priceBookPriceDb.IsMarkupManuallyUpdated = true;
                        priceBookPriceDb.MarkUp = priceBookPrice.MarkUp;
                        priceBookPriceDb.IsMarkupPercentage = priceBookPrice.IsMarkupPercentage;


                        if (priceBookPriceDb.Discount != priceBookPrice.Discount)
                            priceBookPriceDb.IsDiscountManuallyUpdated = true;
                        priceBookPriceDb.Discount = priceBookPrice.Discount;
                        priceBookPriceDb.IsDiscountPercentage = priceBookPrice.IsDiscountPercentage;

                        priceBookPriceDb.Price = priceBookPrice.Price;
                        priceBookPriceDb.LastUpdatedBy = currentUser;
                        priceBookPriceDb.LastUpdateDate = DateTime.UtcNow;
                        if (status.Equals("apply", StringComparison.CurrentCultureIgnoreCase))
                        {
                            priceBookPriceDb.ParentPriceBookInstance.IsDraft = false;


                            var priceValue = !priceBookPrice.IsPriceManuallyUpdated ? priceBookPriceDb.Price : priceBookPriceDb.CalculatedPrice;
                            var priceBookDb = priceBookPriceDb.ParentPriceBookInstance.ParentPriceBookVersion.ParentPriceBook;


                            /*
                             IF, a Price already exists for the Item at the location of the PriceBook, for the SalesChannel of the PriceBook,a Price is to be created for the Item,
                             it MUST be OVERWRITTEN.
                             */
                            new TraderPriceRules(DbContext).CreatePriceByLocationIdItemId(priceBookDb.Location, priceBookDb.SalesChannel, priceValue, priceBookPrice.Price, priceBookPriceDb.Item, currentUser.Id);
                        }
                        if (!string.IsNullOrEmpty(versionName))
                        {
                            priceBookPriceDb.ParentPriceBookInstance.ParentPriceBookVersion.VersionName = versionName;
                        }
                    }
                }


                DbContext.SaveChanges();

                var priceBook = GetPriceBookById(priceBookId);
                if (DbContext.Entry(priceBook).State == EntityState.Detached)
                    DbContext.TraderPriceBooks.Attach(priceBook);
                DbContext.Entry(priceBook).State = EntityState.Modified;
                DbContext.SaveChanges();

                refModel.result = true;
                refModel.actionVal = 1;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, priceBookPrices, status, versionName);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }
            return refModel;
        }

        public ReturnJsonModel ApplyPriceBookPrices(int instanceId, string versionName, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, instanceId, versionName);
                var instance = DbContext.TraderPriceBookInstances.Find(instanceId);
                if (instance != null)
                {
                    var currentUser = DbContext.QbicleUser.Find(userId);

                    foreach (var priceBookPrice in instance.PriceBookPrices)
                    {
                        var priceBookPriceDb = GetPriceBookPriceId(priceBookPrice.Id);

                        priceBookPriceDb.IsPriceManuallyUpdated = priceBookPrice.Price != 0;

                        if (priceBookPriceDb.MarkUp != priceBookPrice.MarkUp)
                            priceBookPriceDb.IsMarkupManuallyUpdated = true;
                        priceBookPriceDb.MarkUp = priceBookPrice.MarkUp;
                        priceBookPriceDb.IsMarkupPercentage = priceBookPrice.IsMarkupPercentage;


                        if (priceBookPriceDb.Discount != priceBookPrice.Discount)
                            priceBookPriceDb.IsDiscountManuallyUpdated = true;
                        priceBookPriceDb.Discount = priceBookPrice.Discount;
                        priceBookPriceDb.IsDiscountPercentage = priceBookPrice.IsDiscountPercentage;

                        priceBookPriceDb.Price = priceBookPrice.Price;
                        priceBookPriceDb.LastUpdatedBy = currentUser;
                        priceBookPriceDb.LastUpdateDate = DateTime.UtcNow;
                        //if (status.Equals("apply", StringComparison.CurrentCultureIgnoreCase))
                        //{
                        priceBookPriceDb.ParentPriceBookInstance.IsDraft = false;


                        var priceValue = !priceBookPrice.IsPriceManuallyUpdated ? priceBookPriceDb.Price : priceBookPriceDb.CalculatedPrice;
                        var priceBookDb = priceBookPriceDb.ParentPriceBookInstance.ParentPriceBookVersion.ParentPriceBook;


                        /*
                         IF, a Price already exists for the Item at the location of the PriceBook, for the SalesChannel of the PriceBook,a Price is to be created for the Item,
                         it MUST be OVERWRITTEN.
                         */
                        new TraderPriceRules(DbContext).CreatePriceByLocationIdItemId(priceBookDb.Location, priceBookDb.SalesChannel, priceValue, priceBookPrice.Price, priceBookPriceDb.Item, currentUser.Id);
                        if (!string.IsNullOrEmpty(versionName))
                        {
                            priceBookPriceDb.ParentPriceBookInstance.ParentPriceBookVersion.VersionName = versionName;
                        }

                        //priceBookPriceDb.


                        if (DbContext.Entry(priceBookPriceDb).State == EntityState.Detached)
                            DbContext.TraderPriceBookPrices.Attach(priceBookPriceDb);
                        DbContext.Entry(priceBookPriceDb).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }
                }

                refModel.result = true;
                refModel.actionVal = 1;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, instanceId, versionName);
                refModel.actionVal = 3;
                refModel.msg = e.Message;
                refModel.result = false;
            }
            return refModel;
        }

        public PriceBookInstance CopyPriceBookVersion(int instanceId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, instanceId);
                var instanceDb = GetPriceBookInstanceById(instanceId);
                var user = DbContext.QbicleUser.Find(userId);

                var priceBookVersion = instanceDb.ParentPriceBookVersion;
                var priceBookPrices = instanceDb.PriceBookPrices;
                var priceDefaults = instanceDb.ProductGroupInfo;
                var priceBookInstance = new PriceBookInstance
                {
                    IsDraft = true,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    InstanceVersion = instanceDb.InstanceVersion + 1,
                    PriceBookPrices = new List<PriceBookPrice>(),
                    PricesCreatedDate = DateTime.UtcNow,
                    ProductGroupInfo = new List<ProductGroupPriceDefaults>()
                };


                foreach (var info in priceDefaults)
                {
                    var productGroupPriceDefault = new ProductGroupPriceDefaults
                    {
                        //ParentInstance = priceBookInstance,
                        ProductGroup = info.ProductGroup,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Discount = info.Discount,
                        IsDiscountPercentage = info.IsDiscountPercentage,
                        IsMarkupPercentage = info.IsMarkupPercentage,
                        MarkUp = info.MarkUp

                    };
                    priceBookInstance.ProductGroupInfo.Add(productGroupPriceDefault);
                }



                foreach (var pbPrice in priceBookPrices)
                {
                    var priceBookPrice = new PriceBookPrice
                    {
                        Item = pbPrice.Item,
                        AverageCost = pbPrice.AverageCost,
                        LatestCost = pbPrice.LatestCost,
                        CalculatedPrice = pbPrice.CalculatedPrice,
                        FullPrice = pbPrice.FullPrice,

                        IsDiscountManuallyUpdated = pbPrice.IsDiscountManuallyUpdated,
                        IsDiscountPercentage = pbPrice.IsDiscountPercentage,
                        Discount = pbPrice.Discount,

                        IsMarkupManuallyUpdated = pbPrice.IsMarkupManuallyUpdated,
                        IsMarkupPercentage = pbPrice.IsMarkupPercentage,
                        MarkUp = pbPrice.MarkUp,

                        IsPriceManuallyUpdated = pbPrice.IsPriceManuallyUpdated,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        Price = pbPrice.Price,
                        TaxValue = pbPrice.TaxValue
                    };
                    priceBookInstance.PriceBookPrices.Add(priceBookPrice);
                }

                priceBookInstance.ParentPriceBookVersion = priceBookVersion;

                refModel.actionVal = 1;
                DbContext.Entry(priceBookInstance).State = EntityState.Added;
                DbContext.TraderPriceBookInstances.Add(priceBookInstance);
                DbContext.SaveChanges();

                return priceBookInstance;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, instanceId);
                return new PriceBookInstance();
            }
        }

        public ReturnJsonModel ReCalculatePriceRow(PriceBookPrice priceBookPrice1, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, priceBookPrice1);
                var pbookPrice = GetPriceBookPriceId(priceBookPrice1.Id);
                if (pbookPrice == null)
                {
                    refModel.result = false;
                    refModel.actionVal = 3;
                    refModel.msg = "Price book price is null!";
                    return refModel;
                }
                var instance = pbookPrice.ParentPriceBookInstance;

                pbookPrice.IsDiscountPercentage = priceBookPrice1.IsDiscountPercentage;
                pbookPrice.Discount = priceBookPrice1.Discount;

                pbookPrice.IsMarkupPercentage = priceBookPrice1.IsMarkupPercentage;
                pbookPrice.MarkUp = priceBookPrice1.MarkUp;

                pbookPrice.Price = priceBookPrice1.Price;
                if (priceBookPrice1.FullPrice != 0)
                    pbookPrice.FullPrice = priceBookPrice1.FullPrice;
                pbookPrice.IsPriceManuallyUpdated = (pbookPrice.Price != 0 || priceBookPrice1.FullPrice != 0);

                CalculatePriceBookPrice(pbookPrice, instance.IsPriceCalWithAvgCost, priceBookPrice1.FullPrice != 0 ? true : false);

                if (DbContext.Entry(pbookPrice).State == EntityState.Detached)
                    DbContext.TraderPriceBookPrices.Attach(pbookPrice);
                DbContext.Entry(pbookPrice).State = EntityState.Modified;

                DbContext.SaveChanges();

                refModel.result = true;
                refModel.actionVal = 1;


                refModel.Object = new
                {
                    FullPrice = pbookPrice.FullPrice,
                    CalculatedPrice = pbookPrice.CalculatedPrice.ToString("N2"),
                    TaxValue = pbookPrice.TaxValue.ToString("N2"),
                    Price = (pbookPrice.IsPriceManuallyUpdated ? pbookPrice.Price : 0)
                };

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, priceBookPrice1);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public ReturnJsonModel SavePriceBookVersion(PriceBookVersion version, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, version);
                var priceBook = GetPriceBookById(version.ParentPriceBook.Id);
                var priceBookVersion = new PriceBookVersion
                {
                    VersionName = version.VersionName,
                    ParentPriceBook = priceBook,
                    AssociatedInstances = new List<PriceBookInstance>()
                };
                var user = DbContext.QbicleUser.Find(userId);
                var priceBookInstance = new PriceBookInstance
                {
                    IsDraft = true,
                    //ParentPriceBookVersion = priceBookVersion,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    InstanceVersion = 1,
                    PriceBookPrices = new List<PriceBookPrice>(),
                    PricesCreatedDate = DateTime.UtcNow,
                    ProductGroupInfo = new List<ProductGroupPriceDefaults>()
                };
                foreach (var group in priceBook.AssociatedProductGroups)
                {
                    AddProductGroupIntoPriceBookInstance(priceBookInstance, group, user, priceBook.Location.Id);
                }


                priceBookVersion.AssociatedInstances.Add(priceBookInstance);

                refModel.actionVal = 1;
                DbContext.Entry(priceBookVersion).State = EntityState.Added;
                DbContext.PriceBookVersions.Add(priceBookVersion);
                DbContext.SaveChanges();
                refModel.msgId = priceBookVersion.Id.ToString();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, version);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }
            return refModel;
        }
        public PriceBookInstance CheckProducts(int instanceId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, instanceId);
                var pricebookinstance = DbContext.TraderPriceBookInstances.Find(instanceId);
                if (pricebookinstance != null)
                {
                    var user = DbContext.QbicleUser.Find(userId);
                    #region Add and Remove groups
                    var pricebook = pricebookinstance.ParentPriceBookVersion.ParentPriceBook;
                    var locationId = pricebook.Location.Id;
                    var groups = pricebook.AssociatedProductGroups;
                    var currentgroups = pricebookinstance.ProductGroupInfo;
                    foreach (var group in groups)
                    {
                        if (!currentgroups.Any(s => s.ProductGroup.Id == group.Id))
                        {
                            AddProductGroupIntoPriceBookInstance(pricebookinstance, group, user, locationId);
                        }
                    }
                    List<ProductGroupPriceDefaults> removeGroups = new List<ProductGroupPriceDefaults>();
                    foreach (var group in currentgroups)
                    {
                        if (!groups.Any(s => s.Id == group.ProductGroup.Id))
                        {
                            removeGroups.Add(group);
                        }
                    }
                    DbContext.TraderProductGroupPriceDefaults.RemoveRange(removeGroups);
                    #endregion
                    #region Add and Remove TraderItems
                    var items = groups.SelectMany(s => s.Items).ToList();
                    var currentitems = pricebookinstance.PriceBookPrices.ToList();
                    if (items != null)
                        foreach (var item in items)
                        {
                            if (!currentitems.Any(s => s.Item.Id == item.Id))
                            {
                                AddProductItemIntoPriceBookInstance(pricebookinstance, item, user, locationId);
                            }
                        }
                    if (currentitems != null)
                    {
                        List<PriceBookPrice> removeprices = new List<PriceBookPrice>();
                        foreach (var item in currentitems)
                        {
                            if (!items.Any(s => s.Id == item.Item.Id))
                            {
                                removeprices.Add(item);
                            }
                        }
                        DbContext.TraderPriceBookPrices.RemoveRange(removeprices);
                    }
                    else
                    {
                        pricebookinstance.PriceBookPrices.Clear();
                    }
                    #endregion
                    DbContext.SaveChanges();

                }
                return pricebookinstance;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, instanceId);
                return null;
            }
        }
        private void AddProductGroupIntoPriceBookInstance(PriceBookInstance instance, TraderGroup group, ApplicationUser currentUser, int locationId)
        {
            var productGroupPriceDefaults = new ProductGroupPriceDefaults
            {
                //ParentInstance = priceBookInstance,
                ProductGroup = group,
                CreatedBy = currentUser,
                CreatedDate = DateTime.UtcNow,
                Discount = 0,
                IsDiscountPercentage = false,
                IsMarkupPercentage = false,
                MarkUp = 0
            };
            instance.ProductGroupInfo.Add(productGroupPriceDefaults);
            foreach (var item in group.Items)
            {
                AddProductItemIntoPriceBookInstance(instance, item, currentUser, locationId);
            }
        }
        private void AddProductItemIntoPriceBookInstance(PriceBookInstance instance, TraderItem item, ApplicationUser currentUser, int locationId)
        {
            //Since this is a pricebook for Items I sell, the pricebook must be updated so that only those items that ‘I sell’ are included in the pricebook. 
            if (item.IsSold)
            {
                var inventoryDetail = item.InventoryDetails.FirstOrDefault(l => l.Location.Id == locationId);

                decimal avgCost = 0;
                decimal lastCost = 0;

                if (inventoryDetail != null)
                {
                    // make the call to get the costs based on the inventory detail
                    // this call takes care of both simple and compound items
                    new TraderInventoryRules(DbContext).GetCostsFromInventory(inventoryDetail, ref avgCost, ref lastCost);
                }
                var priceBookPrice = new PriceBookPrice
                {
                    Item = item,
                    AverageCost = avgCost,
                    LatestCost = lastCost,

                    IsDiscountManuallyUpdated = false,
                    IsDiscountPercentage = false,
                    Discount = 0,

                    IsMarkupManuallyUpdated = false,
                    IsMarkupPercentage = false,
                    MarkUp = 0,

                    IsPriceManuallyUpdated = false,
                    LastUpdateDate = DateTime.UtcNow,
                    LastUpdatedBy = currentUser,
                    Price = 0
                };
                // Work out which cost to use
                switch (instance.IsPriceCalWithAvgCost)
                {
                    case true:
                        priceBookPrice.CalculatedPrice = avgCost;
                        break;
                    default:
                        priceBookPrice.CalculatedPrice = lastCost;
                        break;
                }
                priceBookPrice.TaxValue = priceBookPrice.CalculatedPrice * item.SumTaxRates(true);
                priceBookPrice.FullPrice = priceBookPrice.CalculatedPrice + priceBookPrice.TaxValue;
                instance.PriceBookPrices.Add(priceBookPrice);
            }
        }
        private void CalculatePriceBookPrice(PriceBookPrice price, bool isPriceCalWithAvgCost, bool calFromFullPrice)
        {
            // Work out which cost to use
            decimal cost = 0;
            switch (isPriceCalWithAvgCost)
            {
                case true:
                    cost = price.AverageCost;
                    break;
                default:
                    cost = price.LatestCost;
                    break;
            }

            // Work out the Markup
            decimal calculatedMarkup;

            if (!price.IsMarkupPercentage)
                calculatedMarkup = price.MarkUp;
            else
                calculatedMarkup = cost * (price.MarkUp / 100);


            // Work out the Discount
            decimal calculatedDiscount;

            if (!price.IsDiscountPercentage)
                calculatedDiscount = price.Discount;
            else
                calculatedDiscount = cost * (price.Discount / 100);


            // Work out the calculated price
            price.CalculatedPrice = cost + calculatedMarkup - calculatedDiscount;
            //If the Price has been manually set Price.Value = PriceBookPrice.Price
            //If the Price has not been manually upfated Price.Value = PriceBookPrice.CalculatedPrice
            var itemTaxValue = price.Item.SumTaxRates(true);
            if (!price.IsPriceManuallyUpdated)
            {
                // Work out the tax
                price.TaxValue = price.CalculatedPrice * itemTaxValue;
                // Set the Full Price
                price.FullPrice = price.CalculatedPrice + price.TaxValue;
                price.Price = 0;
            }
            else
            {
                if (calFromFullPrice)
                {
                    // Set the Full Price
                    price.Price = price.FullPrice / (1 + itemTaxValue);
                    price.TaxValue = price.Price * itemTaxValue;
                }
                else
                {
                    // Work out the tax
                    price.TaxValue = price.Price * itemTaxValue;
                    // Set the Full Price
                    price.FullPrice = price.Price + price.TaxValue;
                }
            }
        }
    }
}
