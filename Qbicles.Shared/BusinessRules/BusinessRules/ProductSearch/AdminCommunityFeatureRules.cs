using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.ProductSearch;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.BusinessRules.Community
{
    public class AdminCommunityFeatureRules
    {
        private ApplicationDbContext _db;

        public AdminCommunityFeatureRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }


        internal int GetNewFeaturedProductDisplayOrderNumber()
        {
            var largestOrderNumber = 0;
            if (DbContext.FeaturedProducts.Any())
            {
                largestOrderNumber = DbContext.FeaturedProducts.Max(p => p.DisplayOrder);
            }
            return largestOrderNumber + 1;
        }

        internal int GetNewFeaturedStoreDisplayOrderNumber()
        {
            var largestOrderNumber = 0;
            if (DbContext.FeaturedStores.Any())
            {
                largestOrderNumber = DbContext.FeaturedStores.Max(p => p.DisplayOrder);
            }
            return largestOrderNumber + 1;
        }


        /// <summary>
        /// Save Featured Product with the type = Product
        /// </summary>
        /// <returns></returns>
        public ReturnJsonModel SaveFeaturedProduct(TraderItem item, int productId, int domainId, int catalogId, string itemSKU)
        {
            var responseResult = new ReturnJsonModel() { result = false };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, productId, domainId, catalogId, itemSKU);
                }

                var domain = DbContext.Domains.FirstOrDefault(p => p.Id == domainId);
                if (domain == null)
                {
                    responseResult.result = false;
                    responseResult.msg = "Cannot find the Associated Domain!";
                    return responseResult;
                }
                var catalog = DbContext.PosMenus.FirstOrDefault(p => p.Id == catalogId);
                if (catalog == null)
                {
                    responseResult.result = false;
                    responseResult.msg = "Cannot find the Associated Catalog!";
                    return responseResult;
                }

                var variantSelected = DbContext.PosVariants
                    .Where(p => p.IsDefault 
                                && p.IsActive 
                                && p.TraderItem != null 
                                && p.TraderItem.SKU == itemSKU
                                && p.CategoryItem != null
                                && p.CategoryItem.Category != null
                                && p.CategoryItem.Category.Menu != null
                                && p.CategoryItem.Category.Menu.Id == catalog.Id)
                    .FirstOrDefault();

                var traderItem = variantSelected.TraderItem;
                if (traderItem == null)
                {
                    responseResult.result = false;
                    responseResult.msg = "Cannot find the Associated Trader Item with the given Sku!";
                    return responseResult;
                }

                if (productId > 0)
                {
                    var featuredProduct = DbContext.FPProducts.FirstOrDefault(p => p.Id == productId);
                    if (featuredProduct == null)
                    {
                        responseResult.result = false;
                        responseResult.msg = "Cannot find the Product!";
                        return responseResult;
                    }

                    featuredProduct.Domain = domain;
                    featuredProduct.Catalog = catalog;
                    featuredProduct.TraderItem = traderItem;

                    DbContext.Entry(featuredProduct).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }
                else
                {
                    var productItem = new Product()
                    {
                        Domain = domain,
                        Catalog = catalog,
                        DisplayOrder = GetNewFeaturedProductDisplayOrderNumber(),
                        Type = FeaturedType.Product,
                        TraderItem = traderItem
                    };
                    DbContext.FPProducts.Add(productItem);
                    DbContext.Entry(productItem).State = EntityState.Added;
                    DbContext.SaveChanges();
                }

                if (item.AdditionalInfos.Any())
                {
                    var additionalInfos = new List<AdditionalInfo>();
                    foreach (var a in item.AdditionalInfos.Where(q => q.Id > 0))
                    {
                        var ai1 = DbContext.AdditionalInfos.Find(a.Id);
                        if (ai1 != null)
                            additionalInfos.Add(ai1);
                    }
                    foreach (var a in item.AdditionalInfos.Where(q => q.Id == 0))
                    {
                        var ai = DbContext.AdditionalInfos.FirstOrDefault(e => e.Name == a.Name && e.Type == a.Type);
                        if (ai != null)
                            additionalInfos.Add(ai);
                    }
                    if(additionalInfos.Count > 0)
                    {
                        traderItem.AdditionalInfos.Clear();
                        traderItem.AdditionalInfos = additionalInfos;

                        DbContext.Entry(traderItem).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }


                }

                responseResult.result = true;
                return responseResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, productId, domainId, catalogId, itemSKU);
                responseResult.result = false;
                responseResult.msg = ResourcesManager._L("ERROR_MSG_5");
                return responseResult;
            }
        }

        /// <summary>
        /// Save Featured Product with the type = Image
        /// </summary>
        /// <param name="featuredImageId"></param>
        /// <param name="domainId"></param>
        /// <param name="imageUrl"></param>
        /// <param name="uploadModel"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveFeaturedImage(int featuredImageId, int domainId, string imageUrl, S3ObjectUploadModel uploadModel, string currentUserId)
        {
            var user = DbContext.QbicleUser.Find(currentUserId);
            var returnResult = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, featuredImageId, domainId, imageUrl, uploadModel);

                var domain = DbContext.Domains.FirstOrDefault(p => p.Id == domainId);

                //Start: Processing with file uploaded
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(DbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                var featImage = DbContext.FPImages.FirstOrDefault(p => p.Id == featuredImageId);
                if (featImage != null)
                {
                    featImage.Domain = domain;

                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        featImage.FeaturedImageUri = uploadModel.FileKey ?? "";
                    }

                    featImage.URL = imageUrl;
                    DbContext.Entry(featImage).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    returnResult.result = true;
                    return returnResult;
                }
                else
                {
                    var displayOrder = GetNewFeaturedProductDisplayOrderNumber();
                    var fImageItem = new FeaturedProductImage()
                    {
                        DisplayOrder = displayOrder,
                        Domain = domain,
                        FeaturedImageUri = uploadModel.FileKey ?? "",
                        URL = imageUrl,
                        Type = FeaturedType.Image
                    };
                    DbContext.FPImages.Add(fImageItem);
                    DbContext.Entry(fImageItem).State = EntityState.Added;
                    DbContext.SaveChanges();
                    returnResult.result = true;
                    return returnResult;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, featuredImageId, domainId, imageUrl, uploadModel);
                returnResult.result = false;
                returnResult.msg = ResourcesManager._L("ERROR_MSG_5");
                return returnResult;
            }
        }


        public ReturnJsonModel SaveFeaturedStore(int storeId, int domainId)
        {
            var returnResult = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, storeId, domainId);


                var isDuplicated = DbContext.FeaturedStores.Any(p => p.Domain.Id == domainId && p.Id != storeId);
                if (isDuplicated)
                {
                    returnResult.result = false;
                    returnResult.msg = "A store with the domain already existed!";
                    return returnResult;
                }

                var domain = DbContext.Domains.FirstOrDefault(p => p.Id == domainId);
                if (domain == null)
                {
                    returnResult.result = false;
                    returnResult.msg = "Cannot find the associated domain with the given Id!";
                    return returnResult;
                }

                if (storeId > 0)
                {
                    var featuredStore = DbContext.FeaturedStores.FirstOrDefault(p => p.Id == storeId);
                    if (featuredStore == null)
                    {
                        returnResult.result = false;
                        returnResult.msg = "Cannot find the featured Store";
                        return returnResult;
                    }

                    featuredStore.Domain = domain;
                    DbContext.Entry(featuredStore).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }
                else
                {
                    var featuredStore = new FeaturedStore();
                    featuredStore.Domain = domain;
                    featuredStore.DisplayOrder = GetNewFeaturedStoreDisplayOrderNumber();
                    DbContext.FeaturedStores.Add(featuredStore);
                    DbContext.Entry(featuredStore).State = EntityState.Added;
                    DbContext.SaveChanges();
                }

                returnResult.result = true;
                return returnResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, storeId, domainId);
                returnResult.result = false;
                returnResult.msg = ResourcesManager._L("ERROR_MSG_5");
                return returnResult;
            }
        }

        public DataTablesResponse GetFeaturedProductDTTable(IDataTablesRequest requestModel)
        {
            var lstCashAccountTransactionModel = new List<CashAccountTransactionModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel);

                var apiBaseUrl = ConfigManager.ApiGetDocumentUri;
                var lstFPProducts = from product in DbContext.FPProducts
                                    join catalogItem in DbContext.PosCategoryItems on product.Catalog.Id equals catalogItem.Category.Menu.Id into associatedCatalogItem
                                    from business in DbContext.B2BProfiles where product.Domain.Id == business.Domain.Id
                                    from cataItem in associatedCatalogItem.DefaultIfEmpty()
                                    where cataItem.PosVariants.Any(p => p.TraderItem.Id == product.TraderItem.Id)
                                    select new FeaturedProductDTItem()
                                    {
                                        ProductId = product.Id,
                                        DisplayOrder = product.DisplayOrder,
                                        ProductTypeLabelName = "Product feature",
                                        ItemImageUri = cataItem == null ? "" : apiBaseUrl + cataItem.ImageUri,
                                        BusinessLogUri = apiBaseUrl + business.LogoUri,
                                        BusinessName = business.BusinessName,
                                        CatalogItemName = cataItem == null ? "" : cataItem.Name,
                                        Link = product.Catalog.Name,
                                        DomainName = product.Domain.Name
                                    };
                var lstFPProductWithoutCategoryItem = from product in DbContext.FPProducts
                                                      from business in DbContext.B2BProfiles
                                                      where product.Domain.Id == business.Domain.Id
                                                      && !lstFPProducts.Any(p => p.ProductId == product.Id)
                                                      select new FeaturedProductDTItem()
                                                      {
                                                          ProductId = product.Id,
                                                          DisplayOrder = product.DisplayOrder,
                                                          ProductTypeLabelName = "Product feature",
                                                          ItemImageUri = "",
                                                          BusinessLogUri = apiBaseUrl + business.LogoUri,
                                                          BusinessName = business.BusinessName,
                                                          CatalogItemName = "",
                                                          Link = product.Catalog.Name,
                                                          DomainName = product.Domain.Name
                                                      };
                var lstFPImages = from image in DbContext.FPImages
                                  select new FeaturedProductDTItem()
                                  {
                                      ProductId = image.Id,
                                      DisplayOrder = image.DisplayOrder,
                                      ProductTypeLabelName = "Image",
                                      ItemImageUri = apiBaseUrl + image.FeaturedImageUri,
                                      BusinessLogUri = "",
                                      BusinessName = "",
                                      CatalogItemName = "",
                                      Link = image.URL ?? "",
                                      DomainName = image.Domain.Name
                                  };

                var lstFeaturedProduct = (lstFPProducts).Union(lstFPImages).Union(lstFPProductWithoutCategoryItem);

                var totalItem = lstFeaturedProduct.Count();
                #region Sorting
                lstFeaturedProduct = lstFeaturedProduct.OrderBy(p => p.DisplayOrder);
                #endregion

                // No need for pagination for this table - As the drag & drop events to change the display order is required


                return new DataTablesResponse(requestModel.Draw, lstFeaturedProduct.ToList(), totalItem, totalItem);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel);
            }

            return null;
        }

        public DataTablesResponse GetFeaturedStoreDTTable(IDataTablesRequest requestModel)
        {
            var lstCashAccountTransactionModel = new List<CashAccountTransactionModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel);

                var apiBaseUrl = ConfigManager.ApiGetDocumentUri;
                var lstFeaturedStore = from store in DbContext.FeaturedStores
                                       join domainPlan in DbContext.DomainPlans
                                            on store.Domain.Id equals domainPlan.Domain.Id into dmp
                                       from st in dmp.DefaultIfEmpty()
                                       join business in DbContext.B2BProfiles
                                            on store.Domain.Id equals business.Domain.Id into businessquery
                                       from bu in businessquery.DefaultIfEmpty()
                                       where st == null || st.IsArchived == false
                                       select new FeaturedStoreDTItem
                                       {
                                           StoreId = store.Id,
                                           DisplayOrder = store.DisplayOrder,
                                           DomainName = store.Domain.Name,
                                           DomainImageUri = apiBaseUrl + bu.LogoUri,
                                           DomainPlanLevel = st.Level.Level
                                       };

                var totalItem = lstFeaturedStore.Count();
                var total = DbContext.FeaturedStores.Count();
                #region Sorting
                lstFeaturedStore = lstFeaturedStore.OrderBy(p => p.DisplayOrder);
                #endregion

                #region Paging

                #endregion

                var lstStores = lstFeaturedStore.ToList();
                lstStores.ForEach(p => p.DomainPlanLevelName = p.DomainPlanLevel.GetDescription());

                return new DataTablesResponse(requestModel.Draw, lstStores, totalItem, totalItem);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel);
            }

            return null;
        }

        public ReturnJsonModel DeleteFeaturedStore(int storeId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, storeId);

                var store = DbContext.FeaturedStores.FirstOrDefault(p => p.Id == storeId);
                if(store != null)
                {
                    DbContext.FeaturedStores.Remove(store);

                    // Update the display order of the other Stores
                    var lstStores = DbContext.FeaturedStores
                        .Where(x => x.Id != storeId).OrderBy(x => x.DisplayOrder).ToList();
                    for(int i = 1; i <= lstStores.Count(); i++)
                    {
                        lstStores.ElementAt(i - 1).DisplayOrder = i;
                        DbContext.Entry(lstStores.ElementAt(i - 1)).State = EntityState.Modified;
                    }

                    DbContext.SaveChanges();
                }

                returnJson.result = true;
                return returnJson;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, storeId);
                returnJson.result = false;
                return returnJson;
            }
        }

        public ReturnJsonModel DeleteFeaturedProduct(int productId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, productId);

                var product = DbContext.FeaturedProducts.FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    DbContext.FeaturedProducts.Remove(product);

                    // Update the display order of the other Stores
                    var lstProducts = DbContext.FeaturedProducts
                        .Where(x => x.Id != productId).OrderBy(x => x.DisplayOrder).ToList();
                    for (int i = 1; i <= lstProducts.Count(); i++)
                    {
                        lstProducts.ElementAt(i - 1).DisplayOrder = i;
                        DbContext.Entry(lstProducts.ElementAt(i - 1)).State = EntityState.Modified;
                    }

                    DbContext.SaveChanges();
                }

                returnJson.result = true;
                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, productId);
                returnJson.result = false;
                return returnJson;
            }
        }

        public ReturnJsonModel UpdateStoreDisplayOrder(List<FeaturedStoreDTItem> lstStores)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, lstStores);

                if (lstStores == null || lstStores.Count == 0)
                {
                    returnJson.result = true;
                    return returnJson;
                }

                foreach (var store in lstStores)
                {
                    var storeItemInDb = DbContext.FeaturedStores.FirstOrDefault(p => p.Id == store.StoreId);
                    storeItemInDb.DisplayOrder = store.DisplayOrder;
                    DbContext.Entry(storeItemInDb).State = EntityState.Modified;
                }
                DbContext.SaveChanges();
                returnJson.result = true;
                return returnJson;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstStores);
                returnJson.result = false;
                returnJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return returnJson;
            }
        }

        public ReturnJsonModel UpdateFeaturedProductDisplayOrder(List<FeaturedProductDTItem> lstProducts)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, lstProducts);

                if (lstProducts == null  || lstProducts.Count == 0)
                {
                    returnJson.result = true;
                    return returnJson;
                }

                foreach (var product in lstProducts)
                {
                    var productItemInDb = DbContext.FeaturedProducts.FirstOrDefault(p => p.Id == product.ProductId);
                    productItemInDb.DisplayOrder = product.DisplayOrder;
                    DbContext.Entry(productItemInDb).State = EntityState.Modified;
                }
                DbContext.SaveChanges();
                returnJson.result = true;
                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstProducts);
                returnJson.result = false;
                returnJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return returnJson;
            }
        }
    }
}
