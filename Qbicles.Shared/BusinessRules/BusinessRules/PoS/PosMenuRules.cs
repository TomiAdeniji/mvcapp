using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.PoS
{
    public class PosMenuRules
    {
        private ApplicationDbContext dbContext;

        public PosMenuRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ReturnJsonModel CreatePosMenu(Catalog posMenu, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreatePosMenu", null, null, posMenu);

                var isValid = dbContext.PosMenus.Any(x => x.Name == posMenu.Name && !x.IsDeleted);

                if (posMenu.Type == CatalogType.Sales)
                {
                    isValid = dbContext.PosMenus.Any(x =>
                    x.Name == posMenu.Name && x.Location.Id == posMenu.Location.Id && !x.IsDeleted);
                }

                if (isValid)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_POS_MENU_EXISTED");
                    refModel.actionVal = 9;
                    refModel.result = false;
                }

                //Process with upload model
                if (posMenu != null)
                {
                    if (!string.IsNullOrEmpty(posMenu.Image))
                    {
                        var awsRules = new AzureStorageRules(dbContext);
                        awsRules.ProcessingMediaS3(posMenu.Image);
                    }
                }

                if (posMenu.OrderItemDimensions != null && posMenu.OrderItemDimensions.Any())
                {
                    for (int i = 0; i < posMenu.OrderItemDimensions.Count; i++)
                    {
                        posMenu.OrderItemDimensions[i] = dbContext.TransactionDimensions.Find(posMenu.OrderItemDimensions[i].Id);
                    }
                }

                if (posMenu.SalesChannel == SalesChannelEnum.POS)
                {
                    posMenu.Type = CatalogType.Sales;
                }

                posMenu.CreatedBy = dbContext.QbicleUser.Find(userId);
                posMenu.CreatedDate = DateTime.UtcNow;
                dbContext.PosMenus.Add(posMenu);
                dbContext.Entry(posMenu).State = EntityState.Added;
                dbContext.SaveChanges();
                refModel.msgId = posMenu.Id.ToString();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posMenu);
                refModel.actionVal = 9;
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public List<Catalog> GetByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "POS Menu GetByLocation", null, null, locationId);

                return dbContext.PosMenus.Where(e => e.Location.Id == locationId && !e.IsDeleted && e.SalesChannel == SalesChannelEnum.POS).OrderBy(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return new List<Catalog>();
            }
        }

        /// <summary>
        /// This is funtions return List Catalog
        /// </summary>
        /// <param name="locationId">TraderLocationId</param>
        /// <param name="keyword">keyword</param>
        /// <param name="status">All,Activated,Inactive</param>
        /// <param name="isLoadAll">true get all, false get where IsPublished = true</param>

        public List<Catalog> FiltersCatalog(List<int> locationIds, string keyword, bool isLoadAll, int salesChannel = 0, int catalogSearchType = -1, int domainId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "POS Menu GetByLocation", null, null, locationIds);

                IQueryable<Catalog> query = dbContext.PosMenus.Where(e => locationIds.Contains(e.Location.Id) && !e.IsDeleted);

                if (!isLoadAll)
                    query = query.Where(e => e.IsPublished);

                //If is a CatalogType.Distribution then ignore all locations
                if (catalogSearchType == (int)CatalogType.Distribution)
                {
                    query = dbContext.PosMenus.Where(e => e.Domain.Id == domainId && !e.IsDeleted);
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Name.Contains(keyword) || s.Description.Contains(keyword));
                }
                //if (!string.IsNullOrEmpty(status) && status != "All" && salesChannel == (int)SalesChannelEnum.POS)
                //{
                //    if (status == "Activated")
                //        query = query.Where(s => (s.Devices.Any(e => !e.Archived)));
                //    else
                //        query = query.Where(s => !(s.Devices.Any(e => !e.Archived)));
                //}
                if (salesChannel > 0)
                    query = query.Where(s => s.SalesChannel == (SalesChannelEnum)salesChannel);
                if (catalogSearchType >= 0)
                    query = query.Where(s => s.Type == (CatalogType)catalogSearchType);
                query.ForEach(catalog =>
                {
                    var catalogItemImage = string.IsNullOrEmpty(catalog.Image) ? ConfigManager.CatalogDefaultImage : catalog.Image;
                    catalog.Image = HelperClass.ToDocumentUri(catalogItemImage).ToString();
                });
                return query.OrderByDescending(d => d.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationIds);
                return new List<Catalog>();
            }
        }

        public CategoryItem GetPosCategoryItemById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosCategoryItemById", null, null, id);
                return dbContext.PosCategoryItems.FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new CategoryItem();
            }
        }

        public CategoryItem GetPosCategoryItemWithTaxesById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosCategoryItemById", null, null, id);

                var posCategoryItem = dbContext.PosCategoryItems.FirstOrDefault(s => s.Id == id);

                return posCategoryItem;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new CategoryItem();
            }
        }

        public List<Variant> GetPosVariantsOfProperties(int categoryItemId)
        {
            var variants = new List<Variant>();

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosVariantsOfProperties", null, null, categoryItemId);
                var posCategoryItem = dbContext.PosCategoryItems.Find(categoryItemId);
                if (posCategoryItem != null)
                {
                    var properties = posCategoryItem.VariantProperties
                        .Where(q => q.VariantOptions != null && q.VariantOptions.Any()).OrderBy(q => q.Name).ToList();
                    if (!properties.Any())
                    {
                        return variants;
                    }

                    var lstNameVariants = GetPosVariantsByOption(properties);
                    foreach (var item in lstNameVariants)
                    {
                        var variant = new Variant()
                        {
                            Name = item.Name,
                            IsActive = true,
                            VariantOptions = item.Options
                        };

                        var optIds = item.Options.Select(o => o.Id).OrderBy(e => e).ToList();

                        posCategoryItem.PosVariants.ForEach(v =>
                        {
                            var vOptions = v.VariantOptions.Select(o => o.Id).OrderBy(e => e).ToList();
                            if (vOptions.SequenceEqual(optIds))
                            {
                                variant = v;
                            }
                        });
                        variants.Add(variant);
                    }
                }

                variants = variants.OrderBy(q => q.Name).ToList();
                if (variants.Any() && !variants.Any(q => q.IsDefault))
                {
                    variants[0].IsDefault = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, categoryItemId);
            }

            return variants;
        }

        public List<VariantOptionModel> GetPosVariantsByOption(List<VariantProperty> properties)
        {
            var lstName = new List<VariantOptionModel>();

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosVariantsByOption", null, null, properties);

                if (properties.Count == 0)
                    return lstName;

                var property = properties[0];
                if (properties.Count > 1)
                {
                    properties.RemoveAt(0);
                    var newLstName = GetPosVariantsByOption(properties);
                    foreach (var item in property.VariantOptions)
                    {
                        foreach (var name in newLstName)
                        {
                            var ops = new List<VariantOption>();
                            ops.Add(new VariantOption { Id = item.Id });
                            ops.AddRange(name.Options);
                            lstName.Add(new VariantOptionModel { Options = ops, Name = item.Name + "/" + name.Name });
                        }
                    }
                }
                else
                {
                    foreach (var item in property.VariantOptions)
                    {
                        lstName.Add(new VariantOptionModel { Options = new List<VariantOption> { new VariantOption { Id = item.Id } }, Name = item.Name });
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, properties);
            }

            return lstName;
        }

        public Catalog GetById(int menuId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "POS MENU GetById", null, null, menuId);

                return dbContext.PosMenus.Find(menuId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, menuId);
                return null;
            }
        }

        public List<CategoryCustomItem> GetCatalogItemsPagination(int catalogId, string searchKey, int categoryIdSearch, ref int totalRecord, IDataTablesRequest requestModel,
            int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, searchKey, categoryIdSearch, totalRecord, requestModel, start, length);
                }

                var categoryItems = dbContext.PosMenus.Where(p => p.Id == catalogId).SelectMany(x => x.Categories).SelectMany(x => x.PosCategoryItems).Distinct();
                if (categoryIdSearch != 0)
                {
                    categoryItems = categoryItems.Where(p => p.Category.Id == categoryIdSearch);
                }

                var customItems = new List<CategoryCustomItem>();
                categoryItems.ToList().ForEach(p =>
                {
                    var variant = p.PosVariants.Where(x => x.IsDefault && x.IsActive).FirstOrDefault();
                    if (variant == null)
                    {
                        variant = p.PosVariants.Where(x => x.IsActive).OrderByDescending(v => v.Price.GrossPrice).FirstOrDefault();
                    }
                    var inventoryDetail = variant?.TraderItem?.InventoryDetails.FirstOrDefault();

                    if (!string.IsNullOrEmpty(searchKey.Trim()))
                    {
                        searchKey = searchKey.ToLower();
                        if (p.Name.ToLower().Contains(searchKey) || (variant?.TraderItem?.SKU.ToLower() ?? "").Contains(searchKey))
                        {
                            var customItem = new CategoryCustomItem
                            {
                                Name = p.Name,
                                SKU = variant?.TraderItem?.SKU ?? "",
                                CategoryName = p.Category?.Name ?? "",
                                Price = variant?.Price?.GrossPrice ?? 0,
                                Level = inventoryDetail?.CurrentInventoryLevel ?? 0,
                                Id = p.Id,
                                InStockLabel = "",
                                ImageUri = p.ImageUri.ToUriString()
                            };

                            if ((inventoryDetail?.CurrentInventoryLevel ?? 0) <= (inventoryDetail?.MinInventorylLevel ?? 0))
                            {
                                customItem.InStockLabel = @" <span class='label label-lg label-danger'>Low stock</span>";
                            }
                            customItems.Add(customItem);
                        }
                    }
                    else
                    {
                        var customItem = new CategoryCustomItem
                        {
                            Name = p.Name,
                            SKU = variant?.TraderItem?.SKU ?? "",
                            CategoryName = p.Category?.Name ?? "",
                            Price = variant?.Price?.GrossPrice ?? 0,
                            Level = inventoryDetail?.CurrentInventoryLevel ?? 0,
                            Id = p.Id,
                            InStockLabel = "",
                            ImageUri = p.ImageUri.ToUriString()
                        };

                        if ((inventoryDetail?.CurrentInventoryLevel ?? 0) <= (inventoryDetail?.MinInventorylLevel ?? 0))
                        {
                            customItem.InStockLabel = @" <span class='label label-lg label-danger'>Low stock</span>";
                        }
                        customItems.Add(customItem);
                    }
                });

                var columns = requestModel.Columns;
                var orderByString = string.Empty;

                foreach (var column in columns.GetSortedColumns())
                {
                    switch (column.Data)
                    {
                        case "Name":
                            if (column.SortDirection == TB_Column.OrderDirection.Descendant)
                            {
                                customItems = customItems.OrderByDescending(x => x.Name.Trim()).ToList();
                            }
                            else
                            {
                                customItems = customItems.OrderBy(x => x.Name.Trim()).ToList();
                            }
                            break;

                        case "SKU":
                            if (column.SortDirection == TB_Column.OrderDirection.Descendant)
                            {
                                customItems = customItems.OrderByDescending(x => x.SKU).ToList();
                            }
                            else
                            {
                                customItems = customItems.OrderBy(x => x.SKU).ToList();
                            }
                            break;

                        case "CategoryName":
                            if (column.SortDirection == TB_Column.OrderDirection.Descendant)
                            {
                                customItems = customItems.OrderByDescending(x => x.CategoryName).ToList();
                            }
                            else
                            {
                                customItems = customItems.OrderBy(x => x.CategoryName).ToList();
                            }
                            break;

                        case "Price":
                            if (column.SortDirection == TB_Column.OrderDirection.Descendant)
                            {
                                customItems = customItems.OrderByDescending(x => x.Price).ToList();
                            }
                            else
                            {
                                customItems = customItems.OrderBy(x => x.Price).ToList();
                            }
                            break;

                        case "Level":
                            if (column.SortDirection == TB_Column.OrderDirection.Descendant)
                            {
                                customItems = customItems.OrderByDescending(x => x.Level).ToList();
                            }
                            else
                            {
                                customItems = customItems.OrderBy(x => x.Level).ToList();
                            }
                            break;

                        default:
                            customItems = customItems.OrderBy(x => x.Name).ToList();
                            break;
                    };
                }

                totalRecord = customItems.Count();

                var lstItems = customItems.Skip(start).Take(length).ToList();

                return lstItems;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, searchKey, categoryIdSearch, totalRecord, requestModel, start, length);
                return new List<CategoryCustomItem>();
            }
        }

        public ReturnJsonModel UpdatePosCategoryItemImageUrl(int itemId, string imageUrl)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosCategoryItemImageUrl", null, null, itemId, imageUrl);

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                s3Rules.ProcessingMediaS3(imageUrl);

                var item = dbContext.PosCategoryItems.Find(itemId);

                item.ImageUri = imageUrl;
                if (item.PosVariants.Count > 0)
                {
                    item.PosVariants[0].ImageUri = imageUrl;
                }

                dbContext.SaveChanges();
                return new ReturnJsonModel
                {
                    result = true,
                    actionVal = 2,
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, itemId, imageUrl);
                return new ReturnJsonModel
                {
                    result = false,
                    actionVal = 3,
                    msg = ex.Message
                };
            }
        }

        public ReturnJsonModel GetPriceValue(int traderItemId, int locationId, SalesChannelEnum saleChannel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPriceValue", null, null, traderItemId, locationId);

                var item = dbContext.TraderItems.FirstOrDefault(p => p.Id == traderItemId);
                if (item == null)
                {
                    return new ReturnJsonModel
                    {
                        result = false,
                        actionVal = 1,
                        msg = "Can not find any Item with the given SKU"
                    };
                }

                var price = dbContext.TraderPrices.FirstOrDefault(q => q.Item.Id == traderItemId && q.Location.Id == locationId && q.SalesChannel == saleChannel);
                return new ReturnJsonModel
                {
                    result = true,
                    actionVal = 1,
                    Object = new
                    {
                        Id = (price?.Id ?? 0),
                        GrossPrice = price?.GrossPrice ?? 0
                    },
                    Object2 = item.Units.Select(s => $"<option value='{s.Id}|{s.QuantityOfBaseunit}'>{s.Name}</option>")
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderItemId, locationId);
                return new ReturnJsonModel
                {
                    result = false,
                    actionVal = -1,
                    Object = new { Id = 0, Value = 0 },
                    msg = ex.Message
                };
            }
        }

        public ReturnJsonModel GetPriceValueBySKU(string sku, int locationId, SalesChannelEnum saleChannel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPriceValue", null, null, sku, locationId);

                var item = dbContext.TraderItems.FirstOrDefault(p => p.SKU == sku);
                if (item == null)
                {
                    return new ReturnJsonModel
                    {
                        result = false,
                        actionVal = 1,
                        msg = "Can not find any Item with the given SKU"
                    };
                }

                var price = dbContext.TraderPrices.FirstOrDefault(q => q.Item.Id == item.Id && q.Location.Id == locationId && q.SalesChannel == saleChannel);

                return new ReturnJsonModel
                {
                    result = true,
                    actionVal = 1,
                    Object = new { Id = price?.Id ?? 0, GrossPrice = price?.GrossPrice ?? 0 }
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sku, locationId);
                return new ReturnJsonModel
                {
                    result = false,
                    actionVal = -1,
                    Object = new { Id = 0, GrossPrice = 0 },
                    msg = ex.Message
                };
            }
        }

        public CategoryItem GetCategoryItemByName(string name, int menuId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetCategoryItemByName", null, null, name, menuId);

                var categories = dbContext.PosMenus.Find(menuId)?.Categories.Select(i => i.Id).ToList();

                return dbContext.PosCategoryItems.FirstOrDefault(q => string.Equals(q.Name, name, StringComparison.CurrentCultureIgnoreCase) && categories.Contains(q.Category.Id));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, menuId);
                return null;
            }
        }

        public Category GetCategoryByName(string name, int idmenu)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetCategoryByName", null, null, name, idmenu);

                return dbContext.PosCategories.FirstOrDefault(q => q.Name.ToLower() == name.ToLower() && q.Menu.Id == idmenu);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, idmenu);

                return null;
            }
        }

        public ReturnJsonModel AddPosCategory(Category posCategory, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "AddPosCategory", null, null, posCategory);
                posCategory.CreatedBy = dbContext.QbicleUser.Find(userId);
                posCategory.CreatedDate = DateTime.UtcNow;
                posCategory.Menu = dbContext.PosMenus.Find(posCategory.Menu.Id);
                posCategory.IsVisible = true;
                dbContext.PosCategories.Add(posCategory);
                dbContext.Entry(posCategory).State = EntityState.Added;
                dbContext.SaveChanges();
                refModel.msgId = posCategory.Id.ToString();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posCategory);
                refModel.actionVal = 3;
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }
            return refModel;
        }

        public ReturnJsonModel UpdatePosCategory(Category posCategory)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosCategory", null, null, posCategory);

                var category = dbContext.PosCategories.Find(posCategory.Id);
                if (category == null) return null;
                category.IsVisible = posCategory.IsVisible;
                category.Name = posCategory.Name;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posCategory);
                result.actionVal = 3;
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return result;
        }

        public ReturnJsonModel DeletePosCategory(int id)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeletePosCategory", null, null, id);

                var category = dbContext.PosCategories.Find(id);

                dbContext.PosCategories.Remove(category);
                dbContext.Entry(category).State = EntityState.Deleted;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                result.actionVal = 3;
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return result;
        }

        public ReturnJsonModel DeletePosCategoryItem(int id)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeletePosCategoryItem", null, null, id);

                var categoryItem = dbContext.PosCategoryItems.FirstOrDefault(e => e.Id == id);
                var category = categoryItem.Category;
                categoryItem.Category = null;
                category = categoryItem.Category;
                dbContext.Entry(categoryItem).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                result.actionVal = 3;
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return result;
        }

        public ReturnJsonModel UpdatePosMenu(Catalog posMenu)
        {
            var refModel = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosMenu", null, null, posMenu);

                //Process with upload model
                if (posMenu != null)
                {
                    if (!string.IsNullOrEmpty(posMenu.Image))
                    {
                        var awsRules = new AzureStorageRules(dbContext);
                        awsRules.ProcessingMediaS3(posMenu.Image);
                    }
                }

                var menuUpdate = dbContext.PosMenus.Find(posMenu.Id);

                if (!string.IsNullOrEmpty(posMenu.Image))
                {
                    menuUpdate.Image = posMenu.Image;
                }

                if (menuUpdate.SalesChannel == SalesChannelEnum.POS)
                {
                    menuUpdate.Type = CatalogType.Sales;
                };
                if (posMenu.Location == null)
                    posMenu.Location = menuUpdate.Location;

                var isValid = dbContext.PosMenus.Any(x =>
                x.Name == posMenu.Name && x.Id != posMenu.Id);

                if (posMenu.Type != CatalogType.Distribution)
                {
                    isValid = dbContext.PosMenus.Any(x =>
                            x.Name == posMenu.Name && x.Location.Id == posMenu.Location.Id && x.Id != posMenu.Id);
                }
                if (isValid)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_POS_MENU_EXISTED");
                    refModel.actionVal = 9;
                    refModel.result = false;
                    return refModel;
                }
                if (posMenu.OrderItemDimensions != null && posMenu.OrderItemDimensions.Any())
                {
                    for (int i = 0; i < posMenu.OrderItemDimensions.Count; i++)
                    {
                        posMenu.OrderItemDimensions[i] = dbContext.TransactionDimensions.Find(posMenu.OrderItemDimensions[i].Id);
                    }
                }

                if (menuUpdate == null) return refModel;
                menuUpdate.OrderItemDimensions.Clear();
                dbContext.SaveChanges();
                menuUpdate.OrderItemDimensions = posMenu.OrderItemDimensions;
                menuUpdate.Name = posMenu.Name;
                //menuUpdate.SalesChannel = posMenu.SalesChannel;
                menuUpdate.Description = posMenu.Description;

                if (dbContext.Entry(menuUpdate).State == EntityState.Detached)
                    dbContext.PosMenus.Attach(menuUpdate);
                dbContext.Entry(menuUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.msgId = posMenu.Id.ToString();
                refModel.Object = new
                {
                    menuUpdate.Name,
                    menuUpdate.Description,
                    SalesChannel = menuUpdate.SalesChannel.ToString(),
                    Dimensions = menuUpdate.OrderItemDimensions != null
                        ? string.Join(", ", menuUpdate.OrderItemDimensions.Select(q => q.Name))
                        : ""
                };
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posMenu);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public ReturnJsonModel UpdatePosExtra(Extra posExtra)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosExtra", null, null, posExtra);
                //valid name
                var item = dbContext.PosCategoryItems.Find(posExtra.CategoryItem.Id);
                if (item != null && item.PosExtras.Count > 0)
                {
                    var updatedExtra = item.PosExtras.FirstOrDefault(p => p.Id == posExtra.Id);
                    var newUnitId = posExtra?.Unit.Id ?? 0;
                    var newUnit = dbContext.ProductUnits.FirstOrDefault(p => p.Id == newUnitId);

                    // Update the GrossPrice, NetPrice
                    new PricingHelper(dbContext).UpdateGrossPriceOfExtraPrice(updatedExtra.Id, posExtra.Price?.GrossPrice ?? 0);

                    if (updatedExtra != null && newUnit != null)
                    {
                        var oldUnit = updatedExtra.Unit;
                        var oldUnitBaseQuantity = oldUnit?.QuantityOfBaseunit ?? 1;
                        updatedExtra.Unit = newUnit;
                        if (updatedExtra.Unit != null && updatedExtra.Price != null)
                        {
                            updatedExtra.Price.GrossPrice = (updatedExtra.Price.GrossPrice / oldUnitBaseQuantity) * updatedExtra.Unit.QuantityOfBaseunit;
                            updatedExtra.Price.NetPrice = (updatedExtra.Price.NetPrice / oldUnitBaseQuantity) * updatedExtra.Unit.QuantityOfBaseunit;
                            updatedExtra.Price.TotalTaxAmount = (updatedExtra.Price.TotalTaxAmount / oldUnitBaseQuantity) * updatedExtra.Unit.QuantityOfBaseunit;
                            updatedExtra.Price.Taxes.ForEach(tx =>
                            {
                                tx.Amount = (tx.Amount / oldUnitBaseQuantity) * updatedExtra.Unit.QuantityOfBaseunit;
                            });
                            dbContext.Entry(updatedExtra.Price).State = EntityState.Modified;
                        }
                        refModel.Object = new
                        {
                            GrossPrice = (updatedExtra?.Price?.GrossPrice ?? 0).ToString(CultureInfo.InvariantCulture),
                            Quantity = newUnit.QuantityOfBaseunit.ToString(CultureInfo.InvariantCulture)
                        };
                    }
                }

                if (item != null && dbContext.Entry(item).State == EntityState.Detached)
                    dbContext.PosCategoryItems.Attach(item);
                dbContext.Entry(item).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posExtra);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public ReturnJsonModel ClonePosMenu(Catalog posMenu, UserSetting user, int domainId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete catalog", null, null, posMenu);

            try
            {
                //Process with upload model
                if (posMenu != null)
                {
                    if (!string.IsNullOrEmpty(posMenu.Image))
                    {
                        var awsRules = new AzureStorageRules(dbContext);
                        awsRules.ProcessingMediaS3(posMenu.Image);
                    }
                }

                var checkingPosPermission = false;
                if (posMenu.Type != CatalogType.Distribution)
                {
                    checkingPosPermission = dbContext.WorkGroups.Any(q =>
                        q.Location.Id == posMenu.Location.Id
                        && q.Processes.Any(p => p.Name.Equals(TraderProcessName.POS))
                        && q.Members.Select(u => u.Id).Contains(user.Id));
                }
                else
                {
                    checkingPosPermission = dbContext.WorkGroups.Any(q =>
                        q.Location.Domain.Id == domainId
                        && q.Processes.Any(p => p.Name.Equals(TraderProcessName.POS))
                        && q.Members.Select(u => u.Id).Contains(user.Id));
                }

                if (!checkingPosPermission)
                    return new ReturnJsonModel { msg = ResourcesManager._L("ERROR_MSG_NOT_PROCESS_POS", posMenu.Location.Name), result = false, actionVal = 8 };

                var isValid = dbContext.PosMenus.Any(x =>
                        x.Name == posMenu.Name && !x.IsDeleted);
                if (posMenu.Type != CatalogType.Distribution)
                {
                    isValid = dbContext.PosMenus.Any(x =>
                        x.Name == posMenu.Name && x.Location.Id == posMenu.Location.Id && !x.IsDeleted);
                }

                if (isValid)
                    return new ReturnJsonModel { msg = ResourcesManager._L("ERROR_MSG_POS_MENU_EXISTED", posMenu.Location.Name), result = false, actionVal = 9 };

                if (posMenu.OrderItemDimensions != null && posMenu.OrderItemDimensions.Any())
                {
                    for (int i = 0; i < posMenu.OrderItemDimensions.Count; i++)
                    {
                        posMenu.OrderItemDimensions[i] = dbContext.TransactionDimensions.Find(posMenu.OrderItemDimensions[i].Id);
                    }
                }
                var newMenu = new Catalog
                {
                    Name = posMenu.Name,
                    SalesChannel = posMenu.SalesChannel,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = dbContext.QbicleUser.Find(user.Id),
                    Description = posMenu.Description,
                    Location = posMenu.Location,
                    OrderItemDimensions = posMenu.OrderItemDimensions,
                    Categories = new List<Category>(),
                    Devices = new List<PosDevice>(),
                    IsBeingQuickModeProcessed = true,
                    Type = posMenu.Type,
                    Image = posMenu.Image,
                };

                dbContext.PosMenus.Add(newMenu);
                dbContext.Entry(newMenu).State = EntityState.Added;
                dbContext.SaveChanges();

                var job = new CatalogJobParameter
                {
                    CatalogId = posMenu.Id,
                    NewMenuId = newMenu.Id,
                    EndPointName = "clonecatalog"
                };

                Task tskHangfire = new Task(async () =>
                {
                    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();

                return new ReturnJsonModel
                {
                    result = true,
                    msgId = newMenu.Id.ToString(),
                    msgName = newMenu.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString("dd/MM/yyyy hh:mm:ss")
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posMenu);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }
        }

        public ReturnJsonModel UpdateNameCategoryOfCategoryItem(CategoryItem posCategoryItem, string userId, decimal? updatedPrice = null)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2,
                msgId = posCategoryItem.Category.Id.ToString()
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateNameCategoryOfCategoryItem", null, null, posCategoryItem, updatedPrice);

                var itemCategory = dbContext.PosCategoryItems.Find(posCategoryItem.Id);
                if (itemCategory == null) return refModel;
                itemCategory.Name = posCategoryItem.Name;
                itemCategory.Description = posCategoryItem.Description;
                if (itemCategory?.PosVariants.Count > 0 && updatedPrice != null)
                {
                    // Recalculate the NetPrice, Taxes
                    new PricingHelper(dbContext).UpdateGrossPriceOfVariantPrice(itemCategory.PosVariants[0].Id, (decimal)updatedPrice);
                }
                if (posCategoryItem.Category.Id > 0)
                {
                    itemCategory.Category = dbContext.PosCategories.Find(posCategoryItem.Category.Id);
                }
                else
                {
                    posCategoryItem.Category.Menu = dbContext.PosMenus.Find(posCategoryItem.Category.Menu.Id);
                    posCategoryItem.Category.PosCategoryItems = new List<CategoryItem>();

                    itemCategory.Category.PosCategoryItems.Remove(itemCategory);
                    posCategoryItem.Category.PosCategoryItems.Add(itemCategory);
                    itemCategory.Category = posCategoryItem.Category;
                    itemCategory.Category.CreatedBy = dbContext.QbicleUser.Find(userId);
                    itemCategory.Category.CreatedDate = DateTime.UtcNow;
                }

                dbContext.SaveChanges();
                if (itemCategory.Category != null) refModel.msgId = itemCategory.Category.Id.ToString();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posCategoryItem, updatedPrice);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        private void GetBaseUnitPrice(Catalog menu, Variant variant = null, Extra extra = null)
        {
            if (variant != null)
            {
                variant.BaseUnitPrice = dbContext.TraderPrices.Find(variant.BaseUnitPrice?.Id ?? 0);
                if (variant.BaseUnitPrice == null)
                {
                    variant.BaseUnitPrice = dbContext.TraderPrices.FirstOrDefault(q => q.Item.Id == variant.TraderItem.Id && q.Location.Id == menu.Location.Id && q.SalesChannel == menu.SalesChannel);
                }
                if (variant.BaseUnitPrice == null)
                {
                    var baseUnitPrice = new Price
                    {
                        Item = variant.TraderItem,
                        SalesChannel = menu.SalesChannel,
                        CreatedBy = menu.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        GrossPrice = 0,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = menu.CreatedBy,
                        Location = menu.Location,
                        NetPrice = 0,
                        TotalTaxAmount = 0,
                        Taxes = new List<PriceTax>(),
                    };
                    variant.TraderItem.TaxRates.Where(e => !e.IsPurchaseTax).ForEach(taxRate =>
                    {
                        var staticTaxItem = new TaxRateRules(dbContext).CloneStaticTaxRateById(taxRate.Id);
                        var priceTaxItem = new PriceTax()
                        {
                            Amount = 0,
                            Rate = taxRate.Rate,
                            TaxName = taxRate.Name,
                            TaxRate = staticTaxItem
                        };
                        baseUnitPrice.Taxes.Add(priceTaxItem);
                    });
                    variant.BaseUnitPrice = baseUnitPrice;
                    dbContext.TraderPrices.Add(baseUnitPrice);
                    dbContext.Entry(baseUnitPrice).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
            }
            if (extra != null)
            {
                extra.BaseUnitPrice = dbContext.TraderPrices.FirstOrDefault(q => q.Item.Id == extra.TraderItem.Id && q.Location.Id == menu.Location.Id && q.SalesChannel == menu.SalesChannel);
                if (extra.BaseUnitPrice == null)
                {
                    var baseUnitPrice = new Price
                    {
                        Item = extra.TraderItem,
                        SalesChannel = menu.SalesChannel,
                        CreatedBy = menu.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        GrossPrice = 0,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = menu.CreatedBy,
                        Location = menu.Location,
                        NetPrice = 0,
                        TotalTaxAmount = 0,
                        Taxes = new List<PriceTax>(),
                    };
                    extra.TraderItem.TaxRates.Where(e => !e.IsPurchaseTax).ForEach(taxRate =>
                    {
                        var staticTaxItem = new TaxRateRules(dbContext).CloneStaticTaxRateById(taxRate.Id);
                        var priceTaxItem = new PriceTax()
                        {
                            Amount = 0,
                            Rate = taxRate.Rate,
                            TaxName = taxRate.Name,
                            TaxRate = staticTaxItem
                        };
                        baseUnitPrice.Taxes.Add(priceTaxItem);
                    });
                    extra.BaseUnitPrice = baseUnitPrice;
                    dbContext.TraderPrices.Add(baseUnitPrice);
                    dbContext.Entry(baseUnitPrice).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
            }
        }

        public ReturnJsonModel SaveCategoryItem(CategoryItem posCategoryItem)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1,
                msgId = posCategoryItem.Id.ToString()
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveCategoryItem", null, null, posCategoryItem);
                var s3Rules = new Azure.AzureStorageRules(dbContext);
                var getPosMenu = dbContext.PosMenus.Find(posCategoryItem.Category.Menu.Id);
                if (posCategoryItem.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(posCategoryItem.ImageUri);
                }
                else
                {
                    var mediaValid = dbContext.PosCategoryItems.Find(posCategoryItem.Id);
                    if (mediaValid.ImageUri != posCategoryItem.ImageUri)
                        s3Rules.ProcessingMediaS3(posCategoryItem.ImageUri);
                }
                //  Variants
                //posCategoryItem.PosVariants.Clear();
                foreach (var variant in posCategoryItem.PosVariants)
                {
                    variant.CreatedBy = posCategoryItem.CreatedBy;
                    variant.CreatedDate = posCategoryItem.CreatedDate;
                    variant.CategoryItem = posCategoryItem;
                    variant.TraderItem = dbContext.TraderItems.Find(variant.TraderItem.Id);
                    variant.Unit = dbContext.ProductUnits.Find(variant.Unit.Id);
                    // BaseUnitPrice null incase add item for the first time or expert catalog
                    GetBaseUnitPrice(getPosMenu, variant);

                    variant.IsActive = true;
                    variant.IsDefault = true;

                    if (variant.Price == null)
                    {
                        variant.Price = new CatalogPrice()
                        {
                            GrossPrice = 0,
                            NetPrice = 0,
                            Taxes = new List<PriceTax>(),
                            TotalTaxAmount = 0
                        };
                    }

                    if (variant.Price.Id == 0)
                    {
                        dbContext.CatalogPrices.Add(variant.Price);
                        dbContext.Entry(variant.Price).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }

                    if (variant.BaseUnitPrice != null && variant.BaseUnitPrice.Taxes != null && variant.BaseUnitPrice.Taxes.Count > 0)
                    {
                        var totalTaxPercentage = variant.BaseUnitPrice.Taxes.Sum(tax => tax.Rate);
                        variant.Price.NetPrice = variant.Price.GrossPrice / ((100 + totalTaxPercentage) / 100);
                        variant.Price.TotalTaxAmount = variant.Price.GrossPrice - variant.Price.NetPrice;
                        foreach (var taxItem in variant.BaseUnitPrice.Taxes)
                        {
                            var catalogTaxItem = new PriceTax()
                            {
                                Rate = taxItem.Rate,
                                TaxName = taxItem.TaxName,
                                TaxRate = taxItem.TaxRate
                            };

                            // Tax amount calculation
                            catalogTaxItem.Amount = variant.Price.NetPrice * (catalogTaxItem.Rate / 100);
                            variant.Price.Taxes.Add(catalogTaxItem);
                        }
                    }
                    else
                    {
                        variant.Price.NetPrice = variant.Price.GrossPrice;
                    }
                }
                // VariantProperties
                foreach (var item in posCategoryItem.VariantProperties)
                {
                    item.Name = posCategoryItem.Name;
                    item.CreatedBy = posCategoryItem.CreatedBy;
                    item.CreatedDate = posCategoryItem.CreatedDate;
                    item.CategoryItem = posCategoryItem;
                    foreach (var option in item.VariantOptions)
                    {
                        option.Name = item.Name;
                        option.CreatedDate = item.CreatedDate;
                        option.CreatedBy = item.CreatedBy;
                        option.VariantProperty = item;
                        option.Variants = posCategoryItem.PosVariants;
                    }
                }
                // category
                if (posCategoryItem.Category.Id > 0)
                {
                    posCategoryItem.Category = dbContext.PosCategories.Find(posCategoryItem.Category.Id);
                    if (posCategoryItem.Category != null && posCategoryItem.Category.PosCategoryItems == null)
                    {
                        posCategoryItem.Category.PosCategoryItems = new List<CategoryItem>();
                    }

                    posCategoryItem.Category?.PosCategoryItems.Add(posCategoryItem);
                }
                else
                {
                    posCategoryItem.Category.Menu = dbContext.PosMenus.Find(posCategoryItem.Category.Menu.Id);
                    posCategoryItem.Category.IsVisible = true;
                    posCategoryItem.Category.PosCategoryItems = new List<CategoryItem>();
                    posCategoryItem.Category.PosCategoryItems.Add(posCategoryItem);
                }
                if (posCategoryItem.Id == 0)
                {
                    dbContext.PosCategoryItems.Add(posCategoryItem);
                    dbContext.Entry(posCategoryItem).State = EntityState.Added;
                    dbContext.SaveChanges();
                    refModel.msgId = posCategoryItem.Id.ToString();
                }
                else
                {
                    var dposCategoryItem = dbContext.PosCategoryItems.Find(posCategoryItem.Id);
                    if (posCategoryItem.PosVariants[0].TraderItem != null)
                    {
                        dposCategoryItem?.PosVariants.Add(posCategoryItem.PosVariants[0]);
                    }

                    if (dposCategoryItem != null)
                    {
                        dposCategoryItem.Name = posCategoryItem.Name;
                        dposCategoryItem.Description = posCategoryItem.Description;
                        dposCategoryItem.ImageUri = posCategoryItem.ImageUri;
                    }

                    dbContext.SaveChanges();
                    refModel.actionVal = 2;
                    refModel.msgId = posCategoryItem.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posCategoryItem);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
                refModel.actionVal = -1;
            }

            return refModel;
        }

        public ReturnJsonModel SaveVariant(Variant variant, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1,
                msgId = variant.Id.ToString()
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveVariant", null, null, variant);

                variant.TraderItem = dbContext.TraderItems.Find(variant.TraderItem.Id);
                variant.Unit = dbContext.ProductUnits.Find(variant.Unit.Id);

                var posMenu = dbContext.PosCategoryItems.FirstOrDefault(e => e.Id == variant.CategoryItem.Id)?.Category?.Menu;
                GetBaseUnitPrice(posMenu, variant);

                var imageUri = string.IsNullOrEmpty(variant.ImageUri) ? variant.TraderItem.ImageUri : variant.ImageUri;

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                if (variant.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(imageUri);
                    variant.ImageUri = imageUri;
                }
                else
                {
                    var mediaValid = dbContext.PosVariants.Find(variant.Id);
                    if (mediaValid.ImageUri != variant.ImageUri && !string.IsNullOrEmpty(variant.ImageUri))
                        s3Rules.ProcessingMediaS3(variant.ImageUri);
                }

                var opts = variant.VariantOptions.Select(e => e.Id).ToList();
                var options = dbContext.PosVariantOptions.Where(e => opts.Contains(e.Id)).ToList();

                variant.VariantOptions = options;

                if (variant.Id == 0)
                {
                    variant.Price = new CatalogPrice()
                    {
                        GrossPrice = (variant?.BaseUnitPrice?.GrossPrice ?? 0) * (variant.Unit?.QuantityOfBaseunit ?? 1),
                        NetPrice = (variant?.BaseUnitPrice?.NetPrice ?? 0) * (variant.Unit?.QuantityOfBaseunit ?? 1),
                        TotalTaxAmount = (variant?.BaseUnitPrice?.TotalTaxAmount ?? 0) * (variant.Unit?.QuantityOfBaseunit ?? 1),
                        Taxes = new List<PriceTax>()
                    };
                    dbContext.Entry(variant.Price).State = EntityState.Added;
                    dbContext.CatalogPrices.Add(variant.Price);
                    if (variant?.BaseUnitPrice?.Taxes != null && variant?.BaseUnitPrice?.Taxes.Count > 0)
                    {
                        variant.BaseUnitPrice.Taxes.ForEach(p =>
                        {
                            var priceTaxItem = new PriceTax()
                            {
                                Amount = p.Amount * (variant.Unit?.QuantityOfBaseunit ?? 1),
                                Rate = p.Rate,
                                TaxName = p.TaxName,
                                TaxRate = p.TaxRate
                            };
                            dbContext.TraderPriceTaxes.Add(priceTaxItem);
                            variant.Price.Taxes.Add(priceTaxItem);
                        });
                    }

                    var dcategoryitem = dbContext.PosCategoryItems.Find(variant.CategoryItem.Id);
                    if (dcategoryitem != null && !dcategoryitem.PosVariants.Any(q => q.IsDefault))
                    {
                        variant.IsDefault = true;
                        variant.IsActive = true;
                    }
                    variant.CreatedBy = dbContext.QbicleUser.Find(userId);
                    variant.CreatedDate = DateTime.UtcNow;
                    variant.CategoryItem = dcategoryitem;
                    dcategoryitem?.PosVariants.Add(variant);
                    dbContext.SaveChanges();
                    refModel.msgId = variant.Id.ToString();
                }
                else
                {
                    var variantInDb = dbContext.PosVariants.Find(variant.Id);
                    if (variantInDb != null)
                    {
                        if (variantInDb.Price == null)
                        {
                            var priceItem = new CatalogPrice()
                            {
                                GrossPrice = 0,
                                NetPrice = 0,
                                TotalTaxAmount = 0,
                                // ? ? ? ? ? ?
                                Taxes = new List<PriceTax>()
                            };
                            dbContext.Entry(priceItem).State = EntityState.Added;
                            dbContext.CatalogPrices.Add(priceItem);
                            variantInDb.Price = priceItem;
                        }

                        // Update prices of the variant
                        new PricingHelper(dbContext).UpdateGrossPriceOfVariantPrice(variantInDb.Id, variant.Price?.GrossPrice ?? 0);

                        variantInDb.VariantOptions.Clear();
                        dbContext.SaveChanges();
                        variantInDb.VariantOptions = options;
                        variantInDb.Unit = variant.Unit;
                        variantInDb.BaseUnitPrice = variant.BaseUnitPrice;
                        variantInDb.TraderItem = variant.TraderItem;
                        variantInDb.ImageUri = variant.TraderItem.ImageUri;
                        variantInDb.IsActive = variant.IsActive;
                        variantInDb.IsDefault = variant.IsDefault;
                        dbContext.SaveChanges();
                        refModel.actionVal = 2;
                        refModel.msgId = variantInDb.Id.ToString();
                    }
                }

                if (!variant.IsDefault) return refModel;
                if (variant.CategoryItem != null)
                {
                    var categoryItem = dbContext.PosCategoryItems.Find(variant.CategoryItem.Id);
                    if (categoryItem != null)
                        foreach (var posVariant in categoryItem.PosVariants)
                        {
                            posVariant.IsDefault = variant.Id == posVariant.Id;
                        }
                }

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, variant);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public ReturnJsonModel SavePosExtras(Extra extras, int locationid, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePosExtras", null, null, extras, locationid);

                var categoryitem = dbContext.PosCategoryItems.Find(extras.CategoryItem.Id);

                if (extras.Price == null)
                {
                    extras.Price = new CatalogPrice();
                    dbContext.Entry(extras.Price).State = EntityState.Added;
                    dbContext.CatalogPrices.Add(extras.Price);
                }
                extras.TraderItem = dbContext.TraderItems.Find(extras.TraderItem.Id);
                GetBaseUnitPrice(categoryitem.Category.Menu, null, extras);

                extras.Name = extras.TraderItem?.Name;
                extras.Price.GrossPrice = extras.BaseUnitPrice?.GrossPrice ?? 0;
                extras.Price.NetPrice = extras.BaseUnitPrice?.NetPrice ?? 0;
                extras.Price.Taxes = new List<PriceTax>();
                foreach (var taxItem in (extras.BaseUnitPrice?.Taxes ?? new List<PriceTax>()))
                {
                    var catalogTaxItem = new PriceTax()
                    {
                        Amount = taxItem.Amount,
                        Rate = taxItem.Rate,
                        TaxName = taxItem.TaxName,
                        TaxRate = taxItem.TaxRate
                    };
                    extras.Price.Taxes.Add(catalogTaxItem);
                }
                extras.Price.TotalTaxAmount = extras.BaseUnitPrice?.TotalTaxAmount ?? 0;
                if (extras.TraderItem != null && extras.TraderItem.Units.Count > 0)
                {
                    extras.Unit = extras.TraderItem.Units.FirstOrDefault(q => q.IsBase);
                    if (extras.Unit != null)
                    {
                        extras.Price.GrossPrice = extras.Unit.QuantityOfBaseunit * (extras.BaseUnitPrice?.GrossPrice ?? 0);
                        extras.Price.NetPrice = extras.Unit.QuantityOfBaseunit * (extras.BaseUnitPrice?.NetPrice ?? 0);
                        extras.Price?.Taxes?.ForEach(ti =>
                        {
                            ti.Amount = extras.Unit.QuantityOfBaseunit * ti.Amount;
                        });
                        extras.Price.TotalTaxAmount = extras.Unit.QuantityOfBaseunit * (extras.BaseUnitPrice?.TotalTaxAmount ?? 0);
                    }
                }
                extras.CreatedBy = dbContext.QbicleUser.Find(userId);
                extras.CreatedDate = DateTime.UtcNow;

                extras.CategoryItem = categoryitem;
                categoryitem?.PosExtras.Add(extras);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, extras, locationid);
                refModel.actionVal = 0;
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public ReturnJsonModel SaveDimension(Catalog posMenu)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2,
                msgId = ""
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDimension", null, null, posMenu);

                if (posMenu.OrderItemDimensions != null && posMenu.OrderItemDimensions.Count > 0)
                {
                    var dPosMenu = dbContext.PosMenus.Find(posMenu.Id);
                    if (dPosMenu?.OrderItemDimensions != null && dPosMenu.OrderItemDimensions.Count > 0)
                    {
                        foreach (var t in dPosMenu.OrderItemDimensions)
                        {
                            t.PosMenus.Remove(dPosMenu);
                        }
                        dPosMenu.OrderItemDimensions.Clear();
                        dbContext.SaveChanges();
                    }
                    for (var i = 0; i < posMenu.OrderItemDimensions.Count; i++)
                    {
                        posMenu.OrderItemDimensions[i] = dbContext.TransactionDimensions.Find(posMenu.OrderItemDimensions[i].Id);
                        posMenu.OrderItemDimensions[i].PosMenus.Add(dPosMenu);
                    }

                    if (dPosMenu == null) return refModel;
                    dPosMenu.OrderItemDimensions = new List<TransactionDimension>();
                    dPosMenu.OrderItemDimensions.AddRange(posMenu.OrderItemDimensions);
                    dbContext.SaveChanges();
                    if (dPosMenu.OrderItemDimensions != null && dPosMenu.OrderItemDimensions.Count > 0)
                    {
                        refModel.msgId = string.Join(", ", dPosMenu.OrderItemDimensions.Select(q => q.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posMenu);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public ReturnJsonModel UpdatePosVariantProperty(VariantProperty posVariantProperty, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2,
                msgId = posVariantProperty.Id.ToString()
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosVariantProperty", null, null, posVariantProperty);
                if (posVariantProperty.Id > 0)
                {
                    //var lstItemDelete = new List<string>();
                    var dPosVariantProperty = dbContext.PosVariantProperties.Find(posVariantProperty.Id);
                    if (dPosVariantProperty?.VariantOptions != null)
                    {
                        //foreach (var item in dPosVariantProperty?.VariantOptions)
                        //{
                        //    if (posVariantProperty.VariantOptions.All(q => q.Name != item.Name))
                        //    {
                        //        lstItemDelete.Add(item.Name);
                        //    }
                        //}

                        dbContext.PosVariantOptions.RemoveRange(dPosVariantProperty.VariantOptions);
                        dPosVariantProperty.VariantOptions.Clear();
                        dbContext.SaveChanges();
                        foreach (var item in posVariantProperty.VariantOptions)
                        {
                            item.CreatedBy = dPosVariantProperty.CreatedBy;
                            item.CreatedDate = dPosVariantProperty.CreatedDate;
                            item.VariantProperty = dPosVariantProperty;
                            dPosVariantProperty.VariantOptions.Add(item);
                        }
                    }

                    dbContext.SaveChanges();

                    var posCategoryItem = dbContext.PosCategoryItems.Find(dPosVariantProperty.CategoryItem.Id);
                    if (posCategoryItem != null)
                    {
                        dbContext.PosVariants.RemoveRange(posCategoryItem.PosVariants);
                        posCategoryItem.PosVariants.Clear();
                    }

                    dbContext.SaveChanges();

                    //if (lstItemDelete.Any())
                    //{
                    //    if (dPosVariantProperty != null)
                    //    {
                    //        var posCategoryItem = dbContext.PosCategoryItems.Find(dPosVariantProperty.CategoryItem.Id);
                    //        foreach (var item in lstItemDelete)
                    //        {
                    //            if (posCategoryItem == null) continue;
                    //            var lstRemove = posCategoryItem.PosVariants.Where(q => q.Name.Contains(item)).ToList();
                    //            if (!lstRemove.Any()) continue;
                    //            foreach (var itemVar in lstRemove)
                    //            {
                    //                posCategoryItem.PosVariants.Remove(itemVar);
                    //            }
                    //        }
                    //    }

                    //    dbContext.SaveChanges();
                    //}

                    //if (dPosVariantProperty == null) return refModel;
                    //{
                    //    var dcategoryitem = dbContext.PosCategoryItems.Find(dPosVariantProperty.CategoryItem.Id);
                    //    if (dcategoryitem == null && dcategoryitem.PosVariants.Any(q => q.IsDefault)) return refModel;
                    //    if (dcategoryitem.PosVariants.Any())
                    //        dcategoryitem.PosVariants[0].IsDefault = true;
                    //    dbContext.SaveChanges();
                    //}
                }
                else
                {
                    posVariantProperty.CreatedBy = dbContext.QbicleUser.Find(userId);
                    posVariantProperty.CreatedDate = DateTime.UtcNow;
                    var categoryitem = dbContext.PosCategoryItems.Find(posVariantProperty.CategoryItem.Id);
                    foreach (var item in posVariantProperty.VariantOptions)
                    {
                        item.CreatedBy = posVariantProperty.CreatedBy;
                        item.CreatedDate = posVariantProperty.CreatedDate;
                        item.VariantProperty = posVariantProperty;
                    }
                    posVariantProperty.CategoryItem = categoryitem;
                    categoryitem?.VariantProperties.Add(posVariantProperty);
                    dbContext.SaveChanges();
                    refModel.actionVal = 1;
                    refModel.msgId = posVariantProperty.Id.ToString();
                    if (posVariantProperty.CategoryItem != null)
                    {
                        var dcategoryitem = dbContext.PosCategoryItems.Find(posVariantProperty.CategoryItem.Id);
                        if (dcategoryitem != null)
                        {
                            dbContext.PosVariants.RemoveRange(dcategoryitem.PosVariants);
                            dcategoryitem.PosVariants.Clear();
                        }
                    }

                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posVariantProperty);
                refModel.actionVal = 3;
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public ReturnJsonModel DeleteExtras(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteExtras", null, null, id);

                var extras = dbContext.PosExtras.Find(id);
                if (extras == null) return refModel;

                var item = dbContext.PosCategoryItems.Find(extras.CategoryItem.Id);
                extras.CategoryItem = null;
                item?.PosExtras.Remove(extras);

                dbContext.PosExtras.Remove(extras);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }
            return refModel;
        }

        public ReturnJsonModel DeleteProperty(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteProperty", null, null, id);

                var property = dbContext.PosVariantProperties.Find(id);
                if (property == null) return refModel;
                var item = dbContext.PosCategoryItems.Find(property.CategoryItem.Id);
                if (item == null) return refModel;
                property.CategoryItem = null;
                item.VariantProperties.Remove(property);
                dbContext.PosVariantProperties.Remove(property);
                dbContext.PosVariants.RemoveRange(item.PosVariants);
                item.PosVariants.Clear();
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }
            return refModel;
        }

        public ReturnJsonModel DeleteMenu(int catalogId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete catalog", null, null, catalogId);

            try
            {
                var job = new CatalogJobParameter
                {
                    CatalogId = catalogId,
                    EndPointName = "deletecatalog"
                };

                Task tskHangfire = new Task(async () =>
                {
                    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
                });

                tskHangfire.Start();
                return new ReturnJsonModel
                {
                    result = true,
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogId);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }
        }

        public ReturnJsonModel RefreshPrices(List<int> categoryIds)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Refresh Prices", null, null, categoryIds);
            var category = dbContext.PosCategories.Find(categoryIds.FirstOrDefault());

            try
            {
                category.Menu.IsRefreshPricesDbBeingProcessed = true;
                dbContext.SaveChanges();

                var job = new CatalogJobParameter
                {
                    CategoryIds = categoryIds,
                    EndPointName = "processrefreshprices"
                };

                Task tskHangfire = new Task(async () =>
                {
                    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();

                return new ReturnJsonModel
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                category.Menu.IsRefreshPricesDbBeingProcessed = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, categoryIds);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }
        }

        public ReturnJsonModel PushPricesToPricingPool(List<int> categoryIds)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Push Prices To Pricing Pool", null, null, categoryIds);

            try
            {
                var job = new CatalogJobParameter
                {
                    CategoryIds = categoryIds,
                    EndPointName = "pushpricestopricingpool"
                };

                Task tskHangfire = new Task(async () =>
                {
                    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();

                return new ReturnJsonModel
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, categoryIds);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }
        }

        public ReturnJsonModel UpdatePosMenuProduct(int catalogId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Update Catalog Product", null, null, catalogId);

            try
            {
                var catalog = dbContext.PosMenus.Find(catalogId);
                catalog.IsPOSSqliteDbBeingProcessed = true;
                dbContext.SaveChanges();

                MoveUpdateCatalogProductSqliteToHangfire(catalogId);

                return new ReturnJsonModel
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogId);

                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }
        }

        /// <summary>
        /// Load catalog Items for B2C Catalog discussion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PaginationResponse LoadMenuItems(B2CMenuItemsRequestModel request, bool isApiCall = false)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request);

                var currency = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(request.bdomainId);

                //QBIC-3927 Customers now can see the non-inventory items (additional service items)
                //var query = from citem in dbContext.PosCategoryItems
                //            where request.CatIds.Contains(citem.Category.Id)
                //             && citem.PosVariants.Any()
                //select citem;

                var query = from citem in dbContext.PosCategoryItems
                            where request.CatIds.Contains(citem.Category.Id) && citem.Category.IsVisible
                            && citem.PosVariants.Count > 0
                            select citem;

                //var categoryId = request.CatIds.FirstOrDefault();
                //var menuId = dbContext.PosCategories.FirstOrDefault(e => e.Id == categoryId)?.Menu.Id ?? 0;

                //var query = dbContext.PosMenus.Where(p => p.Id == menuId).SelectMany(x => x.Categories).Where(c => c.IsVisible).SelectMany(x => x.PosCategoryItems).Distinct();

                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(n => n.Name.Contains(request.keyword));

                response.totalNumber = query.Count();
                if (isApiCall)
                    request.pageNumber++;

                var items = query.OrderBy(s => s.Name).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();

                var menuitems = new List<Models.TraderApi.CategoryItemModel>();
                items.ForEach(item =>
                {
                    var storefront = GetStoreFrontByItem(item, currency);
                    if (storefront != null)
                    {
                        menuitems.Add(storefront);
                    }
                });
                response.items = menuitems;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                response.items = new List<Models.TraderApi.CategoryItemModel>();
            }
            return response;
        }

        /// <summary>
        /// Load catalog Items for B2B / Commerce discussion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PaginationResponse LoadB2BCatalogDiscussionItem(B2BCatalogItemsRequestModel request)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request);

                var currency = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(request.bdomainId);
                var query = dbContext.PosCategoryItems.Where(s => request.CatIds.Contains(s.Category.Id));

                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(n => n.Name.Contains(request.keyword));

                response.totalNumber = query.Count();
                var items = query.OrderBy(s => s.Name).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();

                var menuitems = new List<Models.TraderApi.CategoryItemModel>();
                items.ForEach(item =>
                {
                    var storefront = GetStoreFrontByItem(item, currency);
                    if (storefront != null)
                    {
                        menuitems.Add(storefront);
                    }
                });
                response.items = menuitems;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                response.items = new List<Models.TraderApi.CategoryItemModel>();
            }
            return response;
        }

        public Models.TraderApi.CategoryItemModel GetStoreFrontByItem(CategoryItem item, CurrencySetting currency)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, item, currency);
                var imageUri = item.ImageUri;
                if (string.IsNullOrEmpty(imageUri))
                    imageUri = item.PosVariants.FirstOrDefault(v => v.IsDefault)?.ImageUri;
                if (string.IsNullOrEmpty(imageUri))
                    imageUri = "https://www.placehold.it/300x250/EFEFEF/AAAAAA&text=no+image+selected";
                else
                    imageUri = imageUri.ToUriString();

                //var vPrice = item.PosVariants.Where(p => p.Price != null);
                var galleryItems = item.PosVariants.FirstOrDefault(v => v.IsDefault)?.TraderItem.GalleryItems.OrderBy(e => e.Order).ToList() ?? new List<Models.Trader.Product.ProductGalleryItem>();

                var galleries = galleryItems.Select(e => new Models.TraderApi.ItemGalery
                {
                    FileUri = e.FileUri,
                    Order = e.Order,
                    Small = e.FileUri.ToUri(Enums.FileTypeEnum.Image, "T"),
                    Large = e.FileUri.ToUri()
                }).ToList();
                var minPrice = item.PosVariants.Where(p => p.Price != null).OrderBy(g => g.Price.GrossPrice).FirstOrDefault()?.Price;

                if (minPrice == null)
                {
                    decimal price = 0;
                    string priceText = price.ToCurrencySymbol(currency);
                    return new Models.TraderApi.CategoryItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        ImageUri = imageUri,
                        CategoryId = item.Category.Id,
                        CategoryName = item.Category.Name,
                        Price = priceText,
                        PriceVal = price,
                        NetValue = price,
                        NetValueText = priceText,
                        GrossValue = price,
                        GrossValueText = priceText,
                        TaxAmount = price,
                        TaxAmountText = priceText,
                        Description = item.Description,
                        ItemsGaleries = galleries
                    };
                }
                return new Models.TraderApi.CategoryItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    ImageUri = imageUri,
                    CategoryId = item.Category.Id,
                    CategoryName = item.Category.Name,
                    Price = minPrice.GrossPrice.ToCurrencySymbol(currency),
                    PriceVal = minPrice.GrossPrice,
                    NetValue = minPrice.NetPrice,
                    NetValueText = minPrice.NetPrice.ToCurrencySymbol(currency),
                    GrossValue = minPrice.GrossPrice,
                    GrossValueText = minPrice.GrossPrice.ToCurrencySymbol(currency),
                    TaxAmount = minPrice.TotalTaxAmount,
                    TaxAmountText = minPrice.TotalTaxAmount.ToCurrencySymbol(currency),
                    Description = item.Description,
                    ItemsGaleries = galleries
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, item, currency);
            }
            return null;
        }

        public Models.TraderApi.CategoryItemModel GetStoreFrontByItemNotTaxes(CategoryItem item, CurrencySetting currency)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, item, currency);
                var imageUri = item.ImageUri;
                if (string.IsNullOrEmpty(imageUri))
                    imageUri = item.PosVariants.FirstOrDefault(v => v.IsDefault)?.ImageUri;
                if (string.IsNullOrEmpty(imageUri))
                    imageUri = "https://www.placehold.it/300x250/EFEFEF/AAAAAA&text=no+image+selected";
                else
                    imageUri = imageUri.ToUriString();

                var minPrice = item.PosVariants.Where(p => p.Price != null).OrderBy(g => g.Price.GrossPrice).FirstOrDefault()?.Price?.GrossPrice ?? 0;

                return new Models.TraderApi.CategoryItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    ImageUri = imageUri,
                    CategoryId = item.Category.Id,
                    CategoryName = item.Category.Name,
                    Price = $"{currency.CurrencySymbol}{minPrice.ToDecimalPlace(currency)}",
                    Description = item.Description
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, item, currency);
            }
            return null;
        }

        public PaginationResponse LoadOrderMenuItem(B2COrderItemsRequestModel request)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request);

                var currency = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(request.bdomainId);
                //QBIC-3927 Customers now can see the non-inventory items (additional service items)
                var query = from citem in dbContext.PosCategoryItems
                            where request.CatIds.Contains(citem.Category.Id) && citem.Category.IsVisible
                            && citem.PosVariants.Count > 0
                            select citem;

                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(n => n.Name.Contains(request.keyword));

                response.totalNumber = query.Count();
                response.totalPage = response.totalNumber / request.pageSize;
                var items = query.OrderBy(s => s.Name).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();

                var menuitems = new List<Models.TraderApi.CategoryItemModel>();
                items.ForEach(item =>
                {
                    var storefront = GetStoreFrontByItem(item, currency);
                    if (storefront != null)
                    {
                        menuitems.Add(storefront);
                    }
                });
                response.items = menuitems;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                response.items = new List<Models.TraderApi.CategoryItemModel>();
            }
            return response;
        }

        /// <summary>
        /// get catalogue and items
        /// response items in a caregoty
        /// </summary>
        /// <returns></returns>
        public PaginationResponse GetCategoryItems(B2COrderItemsRequestModel request)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request);

                var currency = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(request.bdomainId);
                //QBIC-3927 Customers now can see the non-inventory items (additional service items)
                var query = from citem in dbContext.PosCategoryItems
                            where request.CatIds.Contains(citem.Category.Id) && citem.Category.IsVisible
                            && citem.PosVariants.Count > 0
                            select citem;

                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(n => n.Name.Contains(request.keyword));

                response.totalNumber = query.Count();
                response.totalPage = response.totalNumber / request.pageSize;
                var items = query.OrderBy(s => s.Name).Skip((request.pageNumber) * request.pageSize).Take(request.pageSize).ToList();

                var menuitems = new List<Models.TraderApi.CategoryItemModel>();
                items.ForEach(item =>
                {
                    var storefront = GetStoreFrontByItem(item, currency);
                    if (storefront != null)
                    {
                        menuitems.Add(storefront);
                    }
                });
                response.items = menuitems;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                response.items = new List<Models.TraderApi.CategoryItemModel>();
            }
            return response;
        }

        public PaginationResponse LoadOrderMenuItemNotTaxes(B2COrderItemsRequestModel request)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request);

                var currency = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(request.bdomainId);

                //QBIC-3927 Customers now can see the non-inventory items (additional service items)
                var query = from citem in dbContext.PosCategoryItems
                            where request.CatIds.Contains(citem.Category.Id) && citem.Category.IsVisible
                            && citem.PosVariants.Count > 0
                            select citem;

                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(n => n.Name.Contains(request.keyword));

                response.totalNumber = query.Count();
                var items = query.OrderBy(s => s.Name).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();

                var menuitems = new List<Models.TraderApi.CategoryItemModel>();
                items.ForEach(item =>
                {
                    var storefront = GetStoreFrontByItemNotTaxes(item, currency);
                    if (storefront != null)
                    {
                        menuitems.Add(storefront);
                    }
                });
                response.items = menuitems;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                response.items = new List<Models.TraderApi.CategoryItemModel>();
            }
            return response;
        }

        //Catalogue stands for Catalog
        public ReturnJsonModel ProcessQuickCatalogueCreattion(Catalog catalogue, List<int> productGroupIds,
            int locationId, List<int> filterIds, UserSetting user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, catalogue,
                        productGroupIds, locationId, user.Id, filterIds);

                //Process with upload model
                if (catalogue != null)
                {
                    if (!string.IsNullOrEmpty(catalogue.Image))
                    {
                        var awsRules = new AzureStorageRules(dbContext);
                        awsRules.ProcessingMediaS3(catalogue.Image);
                    }
                }

                //Check catalogue name must be unique within location
                if (dbContext.PosMenus.Any(p => p.Location.Id == locationId && p.Name == catalogue.Name))
                    return new ReturnJsonModel { result = false, msg = "Catalogue name existed in the location." };
                catalogue.IsBeingQuickModeProcessed = true;
                catalogue.Location = dbContext.TraderLocations.Find(locationId);
                catalogue.CreatedBy = dbContext.QbicleUser.FirstOrDefault(p => p.Id == user.Id); ;
                catalogue.CreatedDate = DateTime.UtcNow;
                //Get dimensions info
                catalogue.OrderItemDimensions = dbContext.TransactionDimensions.Where(d => filterIds.Contains(d.Id)).ToList();

                dbContext.PosMenus.Add(catalogue);
                dbContext.Entry(catalogue).State = EntityState.Added;
                dbContext.SaveChanges();

                var job = new CatalogJobParameter
                {
                    CatalogId = catalogue.Id,
                    LocationId = locationId,
                    UserId = user.Id,
                    ProductGroupIds = productGroupIds,
                    EndPointName = "processcatalogquickmode"
                };
                Task tskHangfire = new Task(async () =>
                {
                    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();

                return new ReturnJsonModel
                {
                    result = true,
                    msgId = catalogue.Id.ToString(),
                    msgName = catalogue.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString("dd/MM/yyyy hh:mm:ss")
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogue, productGroupIds, locationId, user.Id, filterIds);
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public List<TraderGroup> GetListTraderGroupByLocation(int locationId)
        {
            var locationItemIds = dbContext.TraderLocations.Find(locationId).Items.Where(item => item.IsSold == true).Select(item => item.Id);
            var listGroups = new List<TraderGroup>();
            listGroups = dbContext.TraderGroups.Where(p => p.Items
            .Any(item => item.Locations.Any(l => l.Id == locationId) && locationItemIds.Contains(item.Id))).OrderBy(p => p.Name).ToList();
            return listGroups;
        }

        public List<TraderGroup> GetListTraderGroupByDomain(int domainId)
        {
            var domainItemIds = from dm in dbContext.Domains
                                where dm.Id == domainId
                                from lc in dm.TraderLocations
                                from item in lc.Items
                                where item.IsSold == true
                                select item.Id;
            var listGroups = new List<TraderGroup>();
            listGroups = dbContext.TraderGroups
                .Where(p => p.Items
                .Any(item => item.Locations.Any(l => l.Domain.Id == domainId) && domainItemIds.Contains(item.Id))).OrderBy(p => p.Name).ToList();
            return listGroups;
        }

        /// <summary>
        /// QBIC-4636 Refresh prices
        /// With this collection of Categories the application must
        ///for each Category
        ///find the associated collections of Variant AND Extras
        ///for each collection of Variants and Extras
        ///for each Variant/Extra
        ///find the associated Pricing Pool price
        ///Variant/Extra.BaseUnitPrice
        ///If a Price cannot be found do nothing
        ///Update the Catalog Price from the CatalogPrice based on the mapping shown in Figure 5
        /// </summary>
        /// <param name="categoryIds">list of category Id</param>
        public void ProcessRefreshPrices(List<int> categoryIds)
        {
            try
            {
                var posCategories = dbContext.PosCategories.Where(m => categoryIds.Contains(m.Id)).ToList();

                posCategories.ForEach(category =>
                {
                    category.PosCategoryItems.ForEach(item =>
                    {
                        item.PosVariants.ForEach(variant =>
                        {
                            if (variant.Price == null)
                            {
                                var priceItem = new CatalogPrice()
                                {
                                    GrossPrice = 0,
                                    NetPrice = 0,
                                    TotalTaxAmount = 0,
                                    Taxes = new List<PriceTax>()
                                };
                                dbContext.Entry(priceItem).State = EntityState.Added;
                                dbContext.CatalogPrices.Add(priceItem);
                                variant.Price = priceItem;
                            }

                            if (variant.BaseUnitPrice != null)
                            {
                                variant.Price.GrossPrice = variant.BaseUnitPrice?.GrossPrice ?? 0;
                                variant.Price.NetPrice = variant.BaseUnitPrice?.NetPrice ?? 0;
                                variant.Price.TotalTaxAmount = variant.BaseUnitPrice?.TotalTaxAmount ?? 0;

                                dbContext.TraderPriceTaxes.RemoveRange(variant.Price.Taxes);
                                if (variant?.BaseUnitPrice?.Taxes != null)
                                {
                                    variant.BaseUnitPrice.Taxes.ForEach(priceTaxSource =>
                                    {
                                        var priceTaxItem = new PriceTax()
                                        {
                                            Amount = priceTaxSource.Amount,
                                            Rate = priceTaxSource.Rate,
                                            TaxName = priceTaxSource.TaxName,
                                            TaxRate = priceTaxSource.TaxRate
                                        };
                                        dbContext.TraderPriceTaxes.Add(priceTaxItem);
                                        variant.Price.Taxes.Add(priceTaxItem);
                                    });
                                }
                            }
                            dbContext.SaveChanges();
                        });

                        item.PosExtras.ForEach(extra =>
                        {
                            if (extra.Price == null)
                            {
                                var priceItem = new CatalogPrice()
                                {
                                    GrossPrice = 0,
                                    NetPrice = 0,
                                    TotalTaxAmount = 0,
                                    Taxes = new List<PriceTax>()
                                };
                                dbContext.Entry(priceItem).State = EntityState.Added;
                                dbContext.CatalogPrices.Add(priceItem);
                                extra.Price = priceItem;
                            };

                            if (extra.BaseUnitPrice != null)
                            {
                                extra.Price.GrossPrice = extra?.BaseUnitPrice?.GrossPrice ?? 0;
                                extra.Price.NetPrice = extra?.BaseUnitPrice?.NetPrice ?? 0;
                                extra.Price.TotalTaxAmount = extra?.BaseUnitPrice?.TotalTaxAmount ?? 0;

                                dbContext.TraderPriceTaxes.RemoveRange(extra.Price.Taxes);
                                if (extra?.BaseUnitPrice?.Taxes != null)
                                {
                                    extra.BaseUnitPrice.Taxes.ForEach(priceTaxSource =>
                                    {
                                        var priceTaxItem = new PriceTax()
                                        {
                                            Amount = priceTaxSource.Amount,
                                            Rate = priceTaxSource.Rate,
                                            TaxName = priceTaxSource.TaxName,
                                            TaxRate = priceTaxSource.TaxRate
                                        };
                                        dbContext.TraderPriceTaxes.Add(priceTaxItem);
                                        extra.Price.Taxes.Add(priceTaxItem);
                                    });
                                }
                            }
                            dbContext.SaveChanges();
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, categoryIds);
            }
            finally
            {
                //Ensure that if an error occurs in any of the product catalog hangfire processes that the appropriate menu boolean is resent to false to ensure that if there is a problem the menu can be deleted.
                var category = dbContext.PosCategories.Find(categoryIds.FirstOrDefault());
                var catalog = category.Menu;
                catalog.IsRefreshPricesDbBeingProcessed = false;
                catalog.Devices.ForEach(device =>
                {
                    device.LastUpdatedDate = DateTime.UtcNow;
                });
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// QBIC-4636 Update Pricing Pool from Catalog
        /// for each Category
        ///find the associated collections of Variant AND Extras
        ///for each collection of Variants and Extras
        ///for each Variant/Extra
        ///find the associated Pricing Pool price
        ///Variant/Extra.BaseUnitPrice
        ///If the Price cannot be found using BaseUnitPrice then find the Pricing Pool price by searching(Price) with
        ///Variant/Extra.TraderItem
        ///Catalog.Location
        ///Catalog.SalesChannel
        ///If a Price cannot be found using by either of the two methods above, create a Price
        ///Update the Price from the CatalogPrice based on the mapping shown in Figure 5
        ///Update the Price.LastUpdateDate and Price.LastUpdatedBy
        ///If creating a Price
        ///Set Price.Location = Catalog.Location , Price.SalesChannel = Catalog.SalesChannel
        ///Update the Price.CreatedBy and Price.CreatedDate
        /// </summary>
        /// <param name="categoryIds">list of category Id</param>
        public void ProcessPushPricesToPricingPool(List<int> categoryIds)
        {
            try
            {
                var posCategories = dbContext.PosCategories.Where(m => categoryIds.Contains(m.Id)).ToList();

                posCategories.ForEach(category =>
                {
                    category.PosCategoryItems.ForEach(item =>
                    {
                        item.PosVariants.ForEach(variant =>
                        {
                            PushPrice(variant.BaseUnitPrice, variant.TraderItem, variant.CategoryItem.Category.Menu, variant.Price);
                        });

                        item.PosExtras.ForEach(extra =>
                        {
                            PushPrice(extra.BaseUnitPrice, extra.TraderItem, extra.CategoryItem.Category.Menu, extra.Price);
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, categoryIds);
            }
            finally
            {
            }
        }

        private void PushPrice(Price baseUnitPrice, TraderItem traderItem, Catalog menu, CatalogPrice catalogPrice)
        {
            if (baseUnitPrice == null)
                baseUnitPrice = dbContext.TraderPrices.FirstOrDefault(e => e.Item.Id == traderItem.Id
                                                && e.Location.Id == menu.Location.Id
                                                && e.SalesChannel == menu.SalesChannel);
            if (baseUnitPrice == null)
            {
                baseUnitPrice = new Price
                {
                    Item = traderItem,
                    SalesChannel = menu.SalesChannel,
                    CreatedBy = menu.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    GrossPrice = catalogPrice?.GrossPrice ?? 0,
                    LastUpdateDate = DateTime.UtcNow,
                    LastUpdatedBy = menu.CreatedBy,
                    Location = menu.Location,
                    NetPrice = catalogPrice?.NetPrice ?? 0,
                    TotalTaxAmount = catalogPrice?.TotalTaxAmount ?? 0,
                    Taxes = catalogPrice?.Taxes,
                };
                dbContext.TraderPrices.Add(baseUnitPrice);
                dbContext.Entry(baseUnitPrice).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            else
            {
                baseUnitPrice.GrossPrice = catalogPrice?.GrossPrice ?? 0;
                baseUnitPrice.NetPrice = catalogPrice?.NetPrice ?? 0;
                baseUnitPrice.TotalTaxAmount = catalogPrice?.TotalTaxAmount ?? 0;

                dbContext.SaveChanges();
            }
        }

        public QbicleJobResult MoveUpdateCatalogProductSqliteToHangfire(int catalogId)
        {
            var job = new CatalogJobParameter
            {
                CatalogId = catalogId,
                EndPointName = "processupdatecatalogproductsqlite"
            };

            Task tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
            return new QbicleJobResult { Status = System.Net.HttpStatusCode.OK };
        }

        public void MoveUpdateCatalogProductSqliteToHangfireAsync(int catalogId)
        {
            var job = new CatalogJobParameter
            {
                CatalogId = catalogId,
                EndPointName = "processupdatecatalogproductsqlite"
            };
            Task tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });

            tskHangfire.Start();
        }

        public void VerifyHangfireStateAsync(string jobId)
        {
            string stateJob;
            do
            {
                stateJob = new Hangfire.HangfireState().VerifyState(jobId).Message;
                if (stateJob == "Succeeded") break;
                System.Threading.Thread.Sleep(1000);
            } while (stateJob != "Succeeded");
        }

        public bool VerifyHangfireState(string jobId)
        {
            return new Hangfire.HangfireState().VerifyState(jobId).Message == "Succeeded";
        }

        public ReturnJsonModel VerifyCatalogStatus(int catalogId, int locationId, string type, string timezone)
        {
            var catalog = GetById(catalogId);
            if (type == "Price")
                return new ReturnJsonModel { result = catalog.IsRefreshPricesDbBeingProcessed };
            if (type == "Database")
                return new ReturnJsonModel { result = catalog.IsPOSSqliteDbBeingProcessed };
            else
                return new ReturnJsonModel { result = catalog.IsBeingQuickModeProcessed, msg = RenderCatalogUI(catalogId, locationId, timezone, catalog).msg };
        }

        public void ProcessDeleteCatalog(int catalogId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Hangfire DeleteMenu", null, null, catalogId);
                var menu = GetById(catalogId);

                var posExtras = menu.Categories.SelectMany(q => q.PosCategoryItems.SelectMany(p => p.PosExtras.Select(po => po))).ToList();
                var itemPosVariants = menu.Categories.SelectMany(q => q.PosCategoryItems.SelectMany(pi => pi.PosVariants)).ToList();
                var postExtrasIds = posExtras.Select(q => q.Id).ToList();
                var posVariantIds = itemPosVariants.Select(q => q.Id).ToList();
                var existsQueueOrderItemRl = dbContext.QueueOrderItems.Any(q => q.Variant != null && posVariantIds.Contains(q.Variant.Id));
                var existsExtrasRl = dbContext.QueueExtras.Any(q => q.Extra != null && postExtrasIds.Contains(q.Extra.Id));
                if (existsExtrasRl || existsQueueOrderItemRl)
                {
                    menu.IsDeleted = true;
                    menu.Name += "_isDel";
                }
                else
                {
                    var variantProperties = menu.Categories.SelectMany(q =>
                        q.PosCategoryItems.SelectMany(pi =>
                            pi.VariantProperties.SelectMany(va => va.VariantOptions.Select(vp => vp.VariantProperty))));

                    var variantOptions = menu.Categories.SelectMany(q =>
                        q.PosCategoryItems.SelectMany(pi => pi.VariantProperties.SelectMany(va => va.VariantOptions)));

                    var categoryItems = menu.Categories.SelectMany(q => q.PosCategoryItems);
                    dbContext.PosExtras.RemoveRange(posExtras);
                    dbContext.PosVariantProperties.RemoveRange(variantProperties);
                    dbContext.PosVariantOptions.RemoveRange(variantOptions);
                    dbContext.PosVariants.RemoveRange(itemPosVariants);
                    dbContext.PosCategoryItems.RemoveRange(categoryItems);
                    dbContext.PosCategories.RemoveRange(menu.Categories);
                    menu.Categories.Clear();
                    //dbContext.SaveChanges();
                    menu.OrderItemDimensions.Clear();
                    dbContext.PosMenus.Remove(menu);
                }
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogId);
            }
        }

        /// <summary>
        /// Clone from catalogId to menuid
        /// </summary>
        /// <param name="catalogId"></param>
        /// <param name="menuId"></param>
        public void ProcessCloneCatalog(int catalogId, int menuId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ClonePosMenu", null, null, catalogId);
                var traderItemRules = new TraderItemRules(dbContext);
                var traderPriceRules = new TraderPriceRules(dbContext);
                var menuSource = GetById(catalogId);
                var menuDestination = GetById(menuId);
                var saleChannelDestination = menuDestination.SalesChannel;

                if (saleChannelDestination == SalesChannelEnum.B2B)
                {
                    foreach (var categorySource in menuSource.Categories)
                    {
                        var categoryNew = new Category
                        {
                            Name = categorySource.Name,
                            Description = categorySource.Description,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = menuDestination.CreatedBy,
                            Menu = menuDestination,
                            IsVisible = categorySource.IsVisible,
                            PosCategoryItems = new List<CategoryItem>()
                        };

                        foreach (var categoryItemSource in categorySource.PosCategoryItems.Where(e => !e.PosExtras.Any() && !e.VariantProperties.Any() && e.PosVariants.Count == 1))
                        {
                            var categoryItemNew = new CategoryItem
                            {
                                Name = categoryItemSource.Name,
                                Description = categoryItemSource.Description,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = menuDestination.CreatedBy,
                                ImageUri = categoryItemSource.ImageUri,
                                PosExtras = new List<Extra>(),
                                PosVariants = new List<Variant>(),
                                VariantProperties = new List<VariantProperty>(),
                                Category = categoryNew
                            };

                            foreach (var variantSource in categoryItemSource.PosVariants)
                            {
                                var price = traderPriceRules.GetPrice(menuDestination.Location, menuDestination.SalesChannel, variantSource.TraderItem, menuDestination.CreatedBy, variantSource.Price.GrossPrice);

                                var clonedVariant = new Variant
                                {
                                    Id = variantSource.Id,
                                    Unit = variantSource.Unit,
                                    Name = variantSource.Name,
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = menuDestination.CreatedBy,
                                    BaseUnitPrice = price,
                                    ImageUri = variantSource.ImageUri,
                                    IsActive = variantSource.IsActive,
                                    TraderItem = variantSource.TraderItem,
                                    VariantOptions = new List<VariantOption>(),
                                    IsDefault = variantSource.IsDefault
                                };
                                var variantPrice = new CatalogPrice()
                                {
                                    GrossPrice = 0,
                                    NetPrice = 0,
                                    TotalTaxAmount = 0,
                                    Taxes = new List<PriceTax>()
                                };
                                dbContext.Entry(variantPrice).State = EntityState.Added;
                                dbContext.CatalogPrices.Add(variantPrice);
                                clonedVariant.Price = variantPrice;

                                if (variantSource.Price != null)
                                {
                                    variantPrice.GrossPrice = variantSource.Price.GrossPrice;
                                    variantPrice.NetPrice = variantSource.Price.NetPrice;
                                    variantPrice.TotalTaxAmount = variantSource.Price.TotalTaxAmount;
                                    variantSource.Price.Taxes.ForEach(taxItemSource =>
                                    {
                                        var clonedTaxItem = new PriceTax()
                                        {
                                            Amount = taxItemSource.Amount,
                                            Rate = taxItemSource.Rate,
                                            TaxName = taxItemSource.TaxName,
                                            TaxRate = taxItemSource.TaxRate
                                        };
                                        dbContext.TraderPriceTaxes.Add(clonedTaxItem);
                                        variantPrice.Taxes.Add(clonedTaxItem);
                                    });
                                }
                                categoryItemNew.PosVariants.Add(clonedVariant);
                                traderItemRules.CheckingProcessTraderItemByLocationValid(variantSource.TraderItem, menuDestination.Location);
                            }

                            categoryNew.PosCategoryItems.Add(categoryItemNew);
                        }
                        menuDestination.Categories.Add(categoryNew);
                    }
                }
                else
                {
                    foreach (var categorySource in menuSource.Categories)
                    {
                        var categoryNew = new Category
                        {
                            Name = categorySource.Name,
                            Description = categorySource.Description,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = menuDestination.CreatedBy,
                            Menu = menuDestination,
                            IsVisible = categorySource.IsVisible,
                            PosCategoryItems = new List<CategoryItem>()
                        };

                        foreach (var categoryItemSource in categorySource.PosCategoryItems)
                        {
                            var categoryItemNew = new CategoryItem
                            {
                                Name = categoryItemSource.Name,
                                Description = categoryItemSource.Description,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = menuDestination.CreatedBy,
                                ImageUri = categoryItemSource.ImageUri,
                                PosExtras = new List<Extra>(),
                                PosVariants = new List<Variant>(),
                                VariantProperties = new List<VariantProperty>(),
                                Category = categoryNew
                            };

                            foreach (var extraSource in categoryItemSource.PosExtras)
                            {
                                var price = new TraderPriceRules(dbContext).GetPrice(menuDestination.Location, menuDestination.SalesChannel, extraSource.TraderItem, menuDestination.CreatedBy);

                                var clonedExtraItem = new Extra
                                {
                                    Unit = extraSource.Unit,
                                    Name = extraSource.Name,
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = menuDestination.CreatedBy,
                                    BaseUnitPrice = price,
                                    TraderItem = extraSource.TraderItem,
                                };
                                //if(clonedExtraItem.BaseUnitPrice != null)
                                //{
                                //    clonedExtraItem.BaseUnitPrice.Id = 0;
                                //    dbContext.TraderPrices.Add(clonedExtraItem.BaseUnitPrice);
                                //}
                                //Clone Price for extras

                                var extraPrice = new CatalogPrice()
                                {
                                    GrossPrice = 0,
                                    NetPrice = 0,
                                    TotalTaxAmount = 0,
                                    Taxes = new List<PriceTax>()
                                };
                                dbContext.Entry(extraPrice).State = EntityState.Added;
                                dbContext.CatalogPrices.Add(extraPrice);
                                clonedExtraItem.Price = extraPrice;

                                if (extraSource.Price != null)
                                {
                                    extraPrice.GrossPrice = extraSource.Price.GrossPrice;
                                    extraPrice.NetPrice = extraSource.Price.NetPrice;
                                    extraPrice.TotalTaxAmount = extraSource.Price.TotalTaxAmount;
                                    extraSource.Price.Taxes.ForEach(sourceTaxItem =>
                                    {
                                        var clonedTaxItem = new PriceTax()
                                        {
                                            Amount = sourceTaxItem.Amount,
                                            Rate = sourceTaxItem.Rate,
                                            TaxName = sourceTaxItem.TaxName,
                                            TaxRate = sourceTaxItem.TaxRate
                                        };
                                        dbContext.TraderPriceTaxes.Add(clonedTaxItem);
                                        extraPrice.Taxes.Add(clonedTaxItem);
                                    });
                                }

                                categoryItemNew.PosExtras.Add(clonedExtraItem);

                                traderItemRules.CheckingProcessTraderItemByLocationValid(extraSource.TraderItem, menuDestination.Location);
                            }

                            foreach (var variantSource in categoryItemSource.PosVariants)
                            {
                                var price = traderPriceRules.GetPrice(menuDestination.Location, menuDestination.SalesChannel, variantSource.TraderItem, menuDestination.CreatedBy, variantSource.Price.GrossPrice);

                                var clonedVariant = new Variant
                                {
                                    Id = variantSource.Id,
                                    Unit = variantSource.Unit,
                                    Name = variantSource.Name,
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = menuDestination.CreatedBy,
                                    BaseUnitPrice = price,
                                    //CategoryItem = categoryItemNew,
                                    ImageUri = variantSource.ImageUri,
                                    IsActive = variantSource.IsActive,
                                    TraderItem = variantSource.TraderItem,
                                    VariantOptions = new List<VariantOption>(),
                                    IsDefault = variantSource.IsDefault
                                };
                                //if(clonedVariant.BaseUnitPrice != null)
                                //{
                                //    clonedVariant.BaseUnitPrice.Id = 0;
                                //    dbContext.TraderPrices.Add(clonedVariant.BaseUnitPrice);
                                //}
                                //Clone Price for variant
                                var variantPrice = new CatalogPrice()
                                {
                                    GrossPrice = 0,
                                    NetPrice = 0,
                                    TotalTaxAmount = 0,
                                    Taxes = new List<PriceTax>()
                                };
                                dbContext.Entry(variantPrice).State = EntityState.Added;
                                dbContext.CatalogPrices.Add(variantPrice);
                                clonedVariant.Price = variantPrice;

                                if (variantSource.Price != null)
                                {
                                    variantPrice.GrossPrice = variantSource.Price.GrossPrice;
                                    variantPrice.NetPrice = variantSource.Price.NetPrice;
                                    variantPrice.TotalTaxAmount = variantSource.Price.TotalTaxAmount;
                                    variantSource.Price.Taxes.ForEach(taxItemSource =>
                                    {
                                        var clonedTaxItem = new PriceTax()
                                        {
                                            Amount = taxItemSource.Amount,
                                            Rate = taxItemSource.Rate,
                                            TaxName = taxItemSource.TaxName,
                                            TaxRate = taxItemSource.TaxRate
                                        };
                                        dbContext.TraderPriceTaxes.Add(clonedTaxItem);
                                        variantPrice.Taxes.Add(clonedTaxItem);
                                    });
                                }

                                categoryItemNew.PosVariants.Add(clonedVariant);
                                traderItemRules.CheckingProcessTraderItemByLocationValid(variantSource.TraderItem, menuDestination.Location);
                            }

                            foreach (var propertySource in categoryItemSource.VariantProperties)
                            {
                                var propertyNew = new VariantProperty
                                {
                                    Name = propertySource.Name,
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = menuDestination.CreatedBy,
                                    CategoryItem = categoryItemNew,
                                    VariantOptions = new List<VariantOption>()
                                };
                                foreach (var optionSource in propertySource.VariantOptions)
                                {
                                    var optionNew = new VariantOption
                                    {
                                        CreatedBy = menuDestination.CreatedBy,
                                        CreatedDate = DateTime.UtcNow,
                                        Name = optionSource.Name,
                                        VariantProperty = propertyNew,
                                        Variants = new List<Variant>()
                                    };

                                    var variants = optionSource.Variants.Select(v => v.Id).ToList();
                                    var variantOptionDestinations = categoryItemNew.PosVariants.Where(q => variants.Contains(q.Id));
                                    variantOptionDestinations.ForEach(v =>
                                    {
                                        v.VariantOptions.Add(optionNew);
                                        optionNew.Variants.Add(v);
                                    });
                                    propertyNew.VariantOptions.Add(optionNew);
                                }
                                categoryItemNew.VariantProperties.Add(propertyNew);
                            }
                            categoryItemNew.PosVariants.ForEach(v => v.Id = 0);
                            categoryItemNew.VariantProperties.ForEach(propertyNew =>
                            {
                                propertyNew.VariantOptions.ForEach(optionNew =>
                                {
                                    optionNew.Variants.ForEach(v => v.Id = 0);
                                });
                            });

                            categoryNew.PosCategoryItems.Add(categoryItemNew);
                        }
                        menuDestination.Categories.Add(categoryNew);
                    }
                }

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogId);
            }
            finally
            {
                //Ensure that if an error occurs in any of the product catalog hangfire processes that the appropriate menu boolean is resent to false to ensure that if there is a problem the menu can be deleted.
                var newMenu = GetById(menuId);
                if (newMenu != null)
                {
                    newMenu.IsBeingQuickModeProcessed = false;
                }
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// call from hangfire to create quick mode catalog
        /// </summary>
        /// <param name="productGroupIds"></param>
        /// <param name="catalogId"></param>
        /// <param name="locationId"></param>
        /// <param name="userId"></param>
        public void ProcessCategoryWithProductGroupIds(List<int> productGroupIds, int catalogId, int locationId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, productGroupIds, catalogId, locationId, userId);

                var productGroups = dbContext.TraderGroups.Where(g => productGroupIds.Contains(g.Id)).ToList();

                //data will be savechanges
                var posCategories = new List<Category>();
                var posCategoryItems = new List<CategoryItem>();
                var posVariants = new List<Variant>();

                //For each selected Product Group ...
                ProcessCategoryItem(productGroups, 0, catalogId, locationId, userId, out posCategories, out posCategoryItems, out posVariants);

                //dbContext.Configuration.AutoDetectChangesEnabled = false;

                dbContext.PosCategories.AddRange(posCategories);
                dbContext.PosVariants.AddRange(posVariants);
                dbContext.PosCategoryItems.AddRange(posCategoryItems);

                dbContext.SaveChanges();

                //dbContext.Configuration.AutoDetectChangesEnabled = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, productGroupIds, catalogId, locationId, userId);
            }
            finally
            {
                dbContext.Configuration.AutoDetectChangesEnabled = true;
                //Ensure that if an error occurs in any of the product catalog hangfire processes that the appropriate menu boolean is resent to false to ensure that if there is a problem the menu can be deleted.
                var catalogInDb = dbContext.PosMenus.Find(catalogId);
                catalogInDb.IsBeingQuickModeProcessed = false;
                dbContext.SaveChanges();
            }
        }

        public void ProcessCategoryItem(List<TraderGroup> productGroups, int IdCategoryTarget, int catalogId, int locationId, string userId, out List<Category> posCategories, out List<CategoryItem> posCategoryItems, out List<Variant> posVariants)
        {
            //for QuickMode - IdCategoryTarget = 0
            //for import item from ProductGrous, IdCategoryTarget require to remove any exist item

            var user = dbContext.QbicleUser.Find(userId);
            var catalogInDb = dbContext.PosMenus.Find(catalogId);
            var location = dbContext.TraderLocations.Find(locationId);
            var itemsLocation = location.Items.Where(i => i.IsSold).Select(i => i.Id);
            var itemsCount = 0;

            //
            posCategories = new List<Category>();
            posCategoryItems = new List<CategoryItem>();
            posVariants = new List<Variant>();

            foreach (var groupItem in productGroups)
            {
                var categoryItem = new Category
                {
                    Name = groupItem.Name,
                    Description = groupItem.Name,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    IsVisible = true,
                    Menu = catalogInDb
                };

                //for each active TraderItem in the ProductGroup...
                var activeItems = groupItem.Items.Where(item => item.IsSold &&
                                                    item.Locations.Any(lc =>
                                                        ((lc.Id == locationId && catalogInDb.Type != CatalogType.Distribution)
                                                        || (lc.Domain.Id == catalogInDb.Location.Domain.Id && catalogInDb.Type == CatalogType.Distribution))
                                                        && itemsLocation.Any(i => i == item.Id))
                                                    ).ToList();

                if (IdCategoryTarget > 0)
                {
                    categoryItem = dbContext.PosCategories.Find(IdCategoryTarget);
                    var itemFromCategories = categoryItem.PosCategoryItems.Where(item => item.PosVariants.Count == 1 && item.PosExtras.Count == 0).SelectMany(e => e.PosVariants).Select(p => p.TraderItem).ToList();
                    activeItems = activeItems.Where(item => !itemFromCategories.Contains(item)).ToList();
                }
                else
                {
                    //create a Category in the Catalog with the same name as the ProductGroup if there aren't selected Categories
                    posCategories.Add(categoryItem);
                }

                //dbContext.PosCategories.AddRange()
                itemsCount += activeItems.Count;
                foreach (var item in activeItems)
                {
                    //each CategoryItem will contain one Variant based on the TraderItem
                    var itemUnit = item.Units.FirstOrDefault(u => u.IsBase);
                    if (itemUnit == null)
                        continue;
                    var variantItem = new Variant
                    {
                        Unit = itemUnit,
                        Name = item.Name,
                        TraderItem = item,
                        ImageUri = item.ImageUri,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDefault = true,
                        BaseUnitPrice = dbContext.TraderPrices.FirstOrDefault(p =>
                                            (((p.Location.Id == locationId && catalogInDb.Type != CatalogType.Distribution)
                                            || (p.Location.Domain.Id == catalogInDb.Location.Domain.Id && catalogInDb.Type == CatalogType.Distribution)) && p.SalesChannel == catalogInDb.SalesChannel)
                                            && p.Item.Id == item.Id),
                        VariantOptions = new List<VariantOption>(),
                    };

                    if (variantItem.Price == null)
                        variantItem.Price = new CatalogPrice();
                    variantItem.Price.GrossPrice = variantItem?.BaseUnitPrice?.GrossPrice ?? 0;
                    variantItem.Price.NetPrice = variantItem?.BaseUnitPrice?.NetPrice ?? 0;
                    variantItem.Price.Taxes = new List<PriceTax>();
                    if (variantItem.BaseUnitPrice?.Taxes != null)
                    {
                        variantItem.BaseUnitPrice.Taxes.ForEach(priceTaxSource =>
                        {
                            var priceTaxItem = new PriceTax()
                            {
                                Amount = priceTaxSource.Amount,
                                Rate = priceTaxSource.Rate,
                                TaxName = priceTaxSource.TaxName,
                                TaxRate = priceTaxSource.TaxRate
                            };
                            dbContext.TraderPriceTaxes.Add(priceTaxItem);
                            variantItem.Price.Taxes.Add(priceTaxItem);
                        });
                    }
                    variantItem.Price.TotalTaxAmount = variantItem.BaseUnitPrice?.TotalTaxAmount ?? 0;
                    //create a CategoryItem in the Category
                    var cateItem = new CategoryItem
                    {
                        Name = item.Name,
                        Description = item.Description,
                        ImageUri = item.ImageUri,
                        Category = categoryItem,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        PosVariants = new List<Variant>() { variantItem }
                    };
                    variantItem.CategoryItem = cateItem;

                    posCategoryItems.Add(cateItem);

                    posVariants.Add(variantItem);
                }
            }
        }

        public ReturnJsonModel RenderCatalogUI(int catalogId, int locationId, string timezone, Catalog catalog = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, catalogId);
                //var isDisplayLocation = locationIds == null ? false : true;
                Catalog menu;
                if (catalog == null)
                    menu = GetById(catalogId);
                else
                    menu = catalog;
                var menuStatus = (menu.Devices.Any(e => !e.Archived));

                var catalogHtml = $"<article id='pos-menu-{menu.Id}' class='col'>";
                catalogHtml += $"<input id='input_{menu.Id}' value='{menu.Name}' type='hidden' />";

                catalogHtml += $"<div class='cat-alerts animated bounceIn' style='top:8px;'>";

                if (menu.FlaggedForTaxUpdate)
                {
                    catalogHtml += $"<span class='alert-tax' data-tooltip='Tax changes have affected prices in this catalogue' data-tooltip-stickto='right' data-tooltip-color='teal'><i class='fa fa-briefcase'></i></span>";
                }
                if (menu.FlaggedForLatestCostUpdate)
                {
                    catalogHtml += $"<span class='alert-tax' data-tooltip='Latest cost changes have affected prices in this catalogue' data-tooltip-stickto='right' data-tooltip-color='teal'><i class='fa fa-dollar'></i></span>";
                }
                catalogHtml += $"</div>";

                catalogHtml += $"<div class='qbicle-opts dropdown'>";
                catalogHtml += $"<a href='javascript:' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>";
                catalogHtml += $"<i class='fa fa-cog'></i></a>";
                catalogHtml += $"<ul class='dropdown-menu primary dropdown-menu-right' style='right: 0;'>";

                if (!menuStatus)
                {
                    catalogHtml += $"<li><a href='javascript:' onclick='UpdateMenu({menu.Id})'>Edit</a></li>";
                    catalogHtml += $"<li><a href='javascript:' onclick='ConfirmDeleteMenu({menu.Id})' data-toggle='modal' data-target='#confirm-delete'>Delete</a></li>";
                }
                catalogHtml += $"<li><a href='javascript:' onclick='CloneMenu({menu.Id})'>Clone</a></li>";

                if (!menu.IsPOSSqliteDbBeingProcessed && menu.SalesChannel == SalesChannelEnum.POS)
                {
                    catalogHtml += $"<li><a href='javascript:' onclick='SetMenuId({menu.Id})' data-toggle='modal' data-target='#pos-menu-updadte-pos-menu-modal'>Update POS Menu</a></li>";
                }
                else if (menu.IsPOSSqliteDbBeingProcessed && menu.SalesChannel == SalesChannelEnum.POS)
                {
                    catalogHtml += $"<li><a class='disabled' href='javascript:'>Update POS Menu processing</a></li>";
                }
                if (menu.SalesChannel == SalesChannelEnum.POS)
                {
                    catalogHtml += $"<li>";
                    if (menu.Devices.Count() <= 0)
                    {
                        catalogHtml += $"<a href='javascript:'>Devices using this (0)</a>";
                    }
                    else
                    {
                        catalogHtml += $"<a href='javascript:' onclick='ViewMenuDevices({menu.Id})'>Devices using this ({menu.Devices.Count()})</a>";
                    }
                    catalogHtml += $"</li>";
                }
                catalogHtml += $"</ul></div>";

                catalogHtml += $"<a href='/PointOfSale/PoSMenu?id={menu.Id}'>";
                catalogHtml += $"<div class='avatar' style='background-image: url('{menu.Image}&size=S');'>&nbsp;</div>";
                catalogHtml += $"<h1 style='color: #333;' class='txt-menuname{menu.Id}'>{menu.Name}</h1>";

                if (locationId == 0)
                {
                    catalogHtml += $"<span class='label label-lg label-soft'>{menu.Location?.Name}</span></a><br /> <br />";
                }

                catalogHtml += $"<p class='qbicle-detail txt-menudesc{menu.Id}' style='white-space: pre-wrap !important;'>{menu.Description}</p><br />";
                catalogHtml += $"<table class='table table-condensed table-striped table-borderless tidytable'>";
                catalogHtml += $"<tr>";
                catalogHtml += $"<td>Menu type</td>";
                catalogHtml += $"<td>{menu.SalesChannel.GetDescription()}</td>";
                catalogHtml += $"</tr>";
                catalogHtml += $"<tr>";
                catalogHtml += $"<td>Added</td>";
                catalogHtml += $"<td>{menu.CreatedDate.ConvertTimeFromUtc(timezone).ToString(menu.CreatedBy.DateFormat + " hh:mmtt").ToLower()}</td>";
                catalogHtml += $"</tr>";
                catalogHtml += $"<tr>";
                if (menu.SalesChannel == SalesChannelEnum.POS)
                {
                    catalogHtml += $"<td>Linked devices</td>";
                    catalogHtml += $"<td>";
                    catalogHtml += $"{menu.Devices.Count()} &nbsp;";
                    if (menu.Devices.Count() > 0)
                    {
                        catalogHtml += $"<a href='javascript:' onclick='ViewMenuDevices({menu.Id})'>View list</a>";
                    }
                    catalogHtml += $"</td>";
                }

                catalogHtml += $"</tr>";
                catalogHtml += $"</table>";
                catalogHtml += $"<a href='/PointOfSale/PoSMenu?id={menu.Id}' class='btn btn-primary community-button'>Manage</a>";
                catalogHtml += $"</article>";

                return new ReturnJsonModel { result = true, msg = catalogHtml };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogId);
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel SetCatalogIncludeInProfile(int domainId, int catId, bool isIncludeInProfile)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, catId, isIncludeInProfile);

                var businessProfile = dbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == domainId);
                if (businessProfile == null)
                {
                    refModel.msg = ResourcesManager._L("WARNING_MSG_SAVEBUSINESSPROFILE");
                    return refModel;
                }
                var catalog = dbContext.PosMenus.Find(catId);
                if (isIncludeInProfile && !businessProfile.BusinessCatalogues.Any(s => s.Id == catId))
                {
                    businessProfile.BusinessCatalogues.Add(catalog);
                    catalog.IsPublished = true;
                }
                else if (!isIncludeInProfile && businessProfile.BusinessCatalogues.Any(s => s.Id == catId))
                {
                    businessProfile.BusinessCatalogues.Remove(catalog);
                    catalog.IsPublished = false;
                }

                refModel.result = dbContext.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, catId, isIncludeInProfile);
                refModel.result = false;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public List<Catalog> GetCatalogsByDomainId(int domainId, SalesChannelEnum SalesChannel = SalesChannelEnum.B2B)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return dbContext.PosMenus.Where(s => s.SalesChannel == SalesChannel && s.Location.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<Catalog>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="domainId"></param>
        /// <param name="locationId"></param>
        /// <param name="nonInventory"></param>
        /// <param name="search"></param>
        /// <param name="productGroupId"></param>
        /// <param name="categoryitemId"></param>
        /// <returns></returns>
        public DataTablesResponse FindItemServerside(IDataTablesRequest requestModel, int domainId, int locationId, bool nonInventory, string search, int productGroupId = 0)
        {
            var lstCashAccountTransactionModel = new List<CashAccountTransactionModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, search, locationId);

                var lstItems = dbContext.TraderItems.Where(q => q.IsSold && q.Domain.Id == domainId
                    && (q.Locations.Select(l => l.Id).Contains(locationId) || locationId == 0));

                if (!nonInventory)
                    lstItems = lstItems.Where(e => e.InventoryDetails.Any(x => x.Location.Id == locationId || (locationId == 0 && x.Location.Domain.Id == domainId)));
                else
                    lstItems = lstItems.Where(e => !e.InventoryDetails.Any(x => x.Location.Id == locationId || (locationId == 0 && x.Location.Domain.Id == domainId)));

                if (!string.IsNullOrEmpty(search))
                    lstItems = lstItems.Where(q => q.SKU.ToLower().Contains(search.ToLower().Trim()) || q.Name.ToLower().Contains(search.ToLower().Trim()));

                if (productGroupId > 0)
                    lstItems = lstItems.Where(q => q.Group.Id == productGroupId);

                var totalItem = lstItems.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "SKU":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SKU" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Group":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Group.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "Name";
                            break;
                    }
                }

                lstItems = lstItems.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = lstItems.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new ProductItemModel
                {
                    Id = q.Id,
                    Name = q.Name,
                    SKU = q.SKU,
                    Group = q.Group?.Name,
                    ImageUri = q.ImageUri
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalItem, totalItem);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel, search, locationId);
            }

            return null;
        }

        public DataTablesResponse FindItemInCatalogServerside(IDataTablesRequest requestModel, int catalogId, int domainId, int locationId, bool nonInventory, string search, int productGroupId)
        {
            var lstCashAccountTransactionModel = new List<CashAccountTransactionModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, search);
                var lstItems = dbContext.PosMenus.Where(p => p.Id == catalogId).SelectMany(x => x.Categories).SelectMany(x => x.PosCategoryItems);

                if (!nonInventory)
                    lstItems = lstItems.Where(e => e.PosVariants.Any(va => va.IsDefault && va.TraderItem.InventoryDetails.Any(x => x.Location.Id == locationId || (locationId == 0 && x.Location.Domain.Id == domainId))));
                else
                    lstItems = lstItems.Where(e => e.PosVariants == null || !e.PosVariants.Any(va => va.IsDefault && va.TraderItem.InventoryDetails.Any(x => x.Location.Id == locationId || (locationId == 0 && x.Location.Domain.Id == domainId))));

                if (!string.IsNullOrEmpty(search))
                {
                    lstItems = lstItems.Where(q => q.PosVariants.Any(va => va.TraderItem.SKU.ToLower().Contains(search.ToLower().Trim())) || q.Name.ToLower().Contains(search.ToLower().Trim()));
                }
                if (productGroupId > 0)
                    lstItems = lstItems.Where(q => q.PosVariants.FirstOrDefault().TraderItem.Group.Id == productGroupId);
                //show total when finishing filter
                var totalItem = lstItems.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "SKU":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SKU" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Group":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Group.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "Name";
                            break;
                    }
                }

                lstItems = lstItems.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = lstItems.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new ProductItemModel
                {
                    Id = q.PosVariants?.FirstOrDefault()?.TraderItem.Id ?? 0,
                    Name = q.Name,
                    SKU = q.PosVariants?.FirstOrDefault()?.TraderItem.SKU,
                    Group = q.PosVariants?.FirstOrDefault()?.TraderItem.Group?.Name,
                    ImageUri = q.ImageUri
                }).ToList();

                return new DataTablesResponse(requestModel.Draw, dataJson, totalItem, totalItem);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel, search);
            }

            return null;
        }

        public DataTablesResponse GetPriceItemTableData(IDataTablesRequest requestModel, int catalogId = 0, string traderItemSKU = "",
            List<int> lstCategoryId = null, string categoryItemNameKeySearch = "", string variantOrExtraNameKeySearch = "", int locationId = 0, int taxupdate = 0, int lastcostupdate = 0, int domainId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, traderItemSKU,
                        lstCategoryId, categoryItemNameKeySearch, variantOrExtraNameKeySearch, locationId);

                #region Filterting

                var lstCategories = dbContext.PosMenus.Where(p => p.Id == catalogId).SelectMany(p => p.Categories);

                if (lstCategoryId != null)
                {
                    lstCategories = lstCategories.Where(p => lstCategoryId.Any(x => x == p.Id));
                }

                var lstCategoryItems = lstCategories.SelectMany(p => p.PosCategoryItems).AsQueryable();
                if (!string.IsNullOrEmpty(categoryItemNameKeySearch))
                {
                    lstCategoryItems = lstCategoryItems.Where(p => p.Name.ToLower().Contains(categoryItemNameKeySearch.ToLower()));
                }

                var lstVariantsQuery = lstCategoryItems.SelectMany(p => p.PosVariants).Where(p => p.Price != null).AsQueryable();
                if (taxupdate == 1)
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.Price.FlaggedForTaxUpdate == true);
                else if (taxupdate == 2)
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.Price.FlaggedForTaxUpdate == false);
                if (lastcostupdate == 1)
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.Price.FlaggedForLatestCostUpdate == true);
                else if (lastcostupdate == 2)
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.Price.FlaggedForLatestCostUpdate == false);

                var lstExtrasQuery = lstCategoryItems.SelectMany(p => p.PosExtras).Where(p => p.Price != null).AsQueryable();
                if (taxupdate == 1)
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.Price.FlaggedForTaxUpdate == true);
                else if (taxupdate == 2)
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.Price.FlaggedForTaxUpdate == false);
                if (lastcostupdate == 1)
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.Price.FlaggedForLatestCostUpdate == true);
                else if (lastcostupdate == 2)
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.Price.FlaggedForLatestCostUpdate == false);

                if (!string.IsNullOrEmpty(traderItemSKU))
                {
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.TraderItem.SKU != null && p.TraderItem.SKU.ToLower().Contains(traderItemSKU.ToLower()));
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.TraderItem.SKU != null && p.TraderItem.SKU.ToLower().Contains(traderItemSKU.ToLower()));
                }
                if (!string.IsNullOrEmpty(variantOrExtraNameKeySearch))
                {
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.Name.ToLower().Contains(variantOrExtraNameKeySearch.ToLower()));
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.Name.ToLower().Contains(variantOrExtraNameKeySearch.ToLower()));
                }
                var lstVariants = lstVariantsQuery;
                var lstExtras = lstExtrasQuery;
                var lstResult = new List<CatalogVariantExtraPrice>();

                //The rows must be displayed grouped by Category, Category Item then Variants above Extras.
                var lstVariantCustomModel = lstVariants.Select(p => new CatalogVariantExtraPrice()
                {
                    Id = p.Id,
                    IsVariant = true,
                    CategoryId = p.CategoryItem.Category.Id,
                    CategoryItemId = p.CategoryItem.Id
                });

                var lstExtraCustomModel = lstExtras.Select(p => new CatalogVariantExtraPrice()
                {
                    Id = p.Id,
                    IsVariant = false,
                    CategoryId = p.CategoryItem.Category.Id,
                    CategoryItemId = p.CategoryItem.Id
                });

                var lstExtraVariantItemQuery = lstVariantCustomModel.Union(lstExtraCustomModel).AsQueryable();
                var lstResultQuery = new List<CatalogVariantExtraPrice>();

                //Grouping by 3 fields - CatalogId, CatalogItemId and IsVariant
                var totalItem = 0;
                totalItem = lstVariantCustomModel.Count() + lstExtraCustomModel.Count();
                var groupedQuery = from c in lstExtraVariantItemQuery
                                   group c by new
                                   {
                                       c.CategoryId,
                                       c.CategoryItemId
                                   } into gcs
                                   orderby gcs.Key.CategoryItemId
                                   select gcs;

                //lstResultQuery = groupedQuery.SelectMany(c => c);

                groupedQuery.ForEach(x => lstResultQuery.AddRange(x.OrderByDescending(k => k.IsVariant)));

                #endregion Filterting

                #region Paging

                lstResult = lstResultQuery.ToList().Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                lstResult.ForEach(p =>
                {
                    if (p.IsVariant)
                    {
                        var variant_item = dbContext.PosVariants.FirstOrDefault(x => x.Id == p.Id);
                        var averageCost = variant_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.AverageCost ?? 0;
                        var latestCost = variant_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.LatestCost ?? 0;
                        var netPrice = variant_item.Price.NetPrice;

                        p.ItemSKU = variant_item.TraderItem.SKU;
                        p.CategoryName = variant_item.CategoryItem.Category.Name;
                        p.CategoryItemName = variant_item.CategoryItem.Name;
                        p.Name = variant_item.Name;
                        p.AverageAndLastestCost = $"{averageCost.ToDecimalPlace(currencySetting)}/ {latestCost.ToDecimalPlace(currencySetting)}";
                        p.PriceId = variant_item.Price.Id;
                        p.NetPrice = netPrice;
                        p.GrossPrice = variant_item.Price.GrossPrice;
                        p.TraderItemId = variant_item.TraderItem.Id;
                        p.ListTaxes = variant_item.Price.Taxes.Select(m => new CustomizedPricingTax
                        {
                            Id = m.Id,
                            Amount = m.Amount,
                            TaxName = m.TaxName
                        }).ToList();
                        p.FlaggedForTaxUpdate = variant_item.Price.FlaggedForTaxUpdate;
                        p.FlaggedForLatestCostUpdate = variant_item.Price.FlaggedForLatestCostUpdate;
                        decimal percentage = 100;
                        if (netPrice > 0)
                            percentage = p.ListTaxes.Sum(a => a.Amount) / netPrice;
                        var netPriceDivide = netPrice == 0 ? 1 : netPrice;
                        p.MarginLatestCost = $"{(netPrice - latestCost).ToDecimalPlace(currencySetting)}/ {(((netPrice - latestCost) / netPriceDivide) * 100).ToDecimalPlace(currencySetting)}%";
                        p.MarginAverageCost = $"{(netPrice - averageCost).ToDecimalPlace(currencySetting)}/ {(((netPrice - averageCost) / netPriceDivide) * 100).ToDecimalPlace(currencySetting)}%";

                        p.CatalogId = catalogId;
                    }
                    else
                    {
                        var extra_item = dbContext.PosExtras.FirstOrDefault(x => x.Id == p.Id);
                        var averageCost = extra_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.AverageCost ?? 0;
                        var latestCost = extra_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.LatestCost ?? 0;
                        var netPrice = extra_item.Price.NetPrice;

                        p.ItemSKU = extra_item.TraderItem?.SKU ?? "";
                        p.CategoryName = extra_item.CategoryItem?.Category?.Name ?? "";
                        p.CategoryItemName = extra_item.CategoryItem?.Name ?? "";
                        p.Name = extra_item.Name;
                        p.AverageAndLastestCost = $"{averageCost.ToDecimalPlace(currencySetting)}/ {latestCost.ToDecimalPlace(currencySetting)}";
                        p.PriceId = extra_item.Price.Id;
                        p.NetPrice = extra_item.Price.NetPrice;
                        p.GrossPrice = extra_item.Price.GrossPrice;
                        p.TraderItemId = extra_item.TraderItem.Id;
                        p.ListTaxes = extra_item.Price.Taxes.Select(m => new CustomizedPricingTax
                        {
                            Id = m.Id,
                            Amount = m.Amount,
                            TaxName = m.TaxName
                        }).ToList();

                        p.FlaggedForTaxUpdate = extra_item.Price.FlaggedForTaxUpdate;
                        p.FlaggedForLatestCostUpdate = extra_item.Price.FlaggedForLatestCostUpdate;

                        decimal percentage = 100;
                        if (netPrice > 0)
                            percentage = p.ListTaxes.Sum(a => a.Amount) / netPrice;

                        var netPriceDivide = netPrice == 0 ? 1 : netPrice;

                        p.MarginLatestCost = $"{(netPrice - latestCost).ToDecimalPlace(currencySetting)}/ {(((netPrice - latestCost) / netPriceDivide) * 100).ToDecimalPlace(currencySetting)}%";
                        p.MarginAverageCost = $"{(netPrice - averageCost).ToDecimalPlace(currencySetting)}/ {(((netPrice - averageCost) / netPriceDivide) * 100).ToDecimalPlace(currencySetting)}%";

                        p.CatalogId = catalogId;
                    }
                });
                return new DataTablesResponse(requestModel.Draw, lstResult, totalItem, totalItem);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, traderItemSKU, lstCategoryId,
                    categoryItemNameKeySearch, variantOrExtraNameKeySearch, locationId);
                return new DataTablesResponse(requestModel.Draw, new List<CatalogVariantExtraPrice>(), 0, 0);
            }
        }

        /// <summary>
        /// UpdateCatalogItemPrice
        /// </summary>
        /// <param name="catalogId"> menu id</param>
        /// <param name="id">Catalog Price Id</param>
        /// <param name="isInclusiveTax"></param>
        /// <param name="value"></param>
        /// <param name="domainId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateCatalogItemPrice(int catalogId, int id, bool isInclusiveTax, decimal value, int domainId, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, isInclusiveTax, value);

                var currentUser = dbContext.QbicleUser.Find(userId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var price = dbContext.CatalogPrices.Find(id);
                if (price != null)
                {
                    if (isInclusiveTax)
                    {
                        price.NetPrice = value / (1 + (price.Taxes.Sum(x => x.Rate) / 100));
                        price.GrossPrice = value;
                    }
                    else
                    {
                        price.NetPrice = value;
                        price.GrossPrice = value * (1 + (price.Taxes.Sum(x => x.Rate) / 100));
                    }
                    price.FlaggedForLatestCostUpdate = false;
                    price.FlaggedForTaxUpdate = false;

                    /*
                        If there are NO CatalogPrices associated with the catalog that have CatalogPrice.FlaggedForTaxUpdate = true then Catalog.FlaggedForTaxUpdate is set to false.
                        If there are NO CatalogPrices associated with the catalog that have CatalogPrice.FlaggedForLatestCostUpdate = true then Catalog.FlaggedForLatestCostUpdate is set to false.
                     */

                    //catalog.FlaggedForTaxUpdate = true;
                    var catalog = dbContext.PosMenus.FirstOrDefault(p => p.Id == catalogId);
                    var lstCategories = catalog.Categories;
                    var categoryItems = lstCategories.SelectMany(e => e.PosCategoryItems);
                    var variants = categoryItems.SelectMany(e => e.PosVariants);
                    var extras = categoryItems.SelectMany(e => e.PosExtras);

                    var catalogPrices = variants.Select(e => e.Price);
                    catalogPrices.Union(extras.Select(e => e.Price));

                    if (catalogPrices.All(e => e.FlaggedForTaxUpdate == false))
                        catalog.FlaggedForTaxUpdate = false;

                    if (catalogPrices.All(e => e.FlaggedForLatestCostUpdate == false))
                        catalog.FlaggedForLatestCostUpdate = false;
                    //var catalog = dbContext.memu

                    //Calculate taxes
                    if (price.Taxes != null && price.Taxes.Count > 0)
                    {
                        foreach (var taxitem in price.Taxes)
                        {
                            taxitem.Amount = price.NetPrice * (taxitem.Rate / 100);
                            dbContext.Entry(taxitem).State = EntityState.Modified;
                        }
                    }
                    dbContext.SaveChanges();
                    returnJson.Object = new { price.Id, PriceExcTax = price.NetPrice.ToInputNumberFormat(currencySettings) };
                }

                returnJson.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, isInclusiveTax, value);
                returnJson.result = false;
                returnJson.msg = ResourcesManager._L("ERROR_MSG_5");
            }
            return returnJson;
        }

        /// <summary>
        /// Bulk Update for Catalog Detail page => Prices tab
        /// </summary>
        /// <param name="marginValue">Input Value for margin updating</param>
        /// <param name="discountValue">Input Value for discount updating</param>
        /// <param name="isAppliedToAverageCost">true - Average Cost is selected, false - Latest Cost is selected</param>
        /// <param name="unitType">1- Percentage (%), 2- Number</param>
        /// <param name="lstItemIds">List ids of the items will be updated</param>
        /// <returns></returns>
        public ReturnJsonModel ApplyConfigPriceByGroup(decimal marginValue, decimal discountValue,
            bool isAppliedToAverageCost, int unitType, int catalogId, bool isMarginUpdated, List<int> lstExtraIds, List<int> lstVariantIds)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, marginValue, discountValue,
                        isAppliedToAverageCost, unitType, catalogId, isMarginUpdated, lstExtraIds, lstVariantIds);

                // Validate input data
                if ((unitType != 1 && unitType != 2))
                {
                    returnJson.result = true;
                    return returnJson;
                }

                var lstExtraDistinct = lstExtraIds.Distinct().ToList();
                var lstVariantDistinct = lstVariantIds.Distinct().ToList();
                var catalog = dbContext.PosMenus.FirstOrDefault(p => p.Id == catalogId);

                decimal priceValue = 0;
                decimal calculatedMargin = 0;
                decimal calculatedDiscount = 0;

                foreach (var extraIdItem in lstExtraDistinct)
                {
                    var extraItem = dbContext.PosExtras.FirstOrDefault(ex => ex.Id == extraIdItem);
                    if (extraItem == null)
                    {
                        continue;
                    }
                    var traderItem = extraItem.TraderItem;
                    var inventoryDetail = traderItem.InventoryDetails.FirstOrDefault(p => p.Location.Id == catalog.Location.Id);

                    var cost = (inventoryDetail != null ? (isAppliedToAverageCost ? inventoryDetail.AverageCost : inventoryDetail.LatestCost) : 0);
                    calculatedMargin = unitType == 1 ? (cost * (marginValue / 100)) : marginValue;/*1=%; 2=value*/
                    calculatedDiscount = unitType == 1 ? (cost * (discountValue / 100)) : discountValue;/*1=%; 2=value*/

                    if (inventoryDetail != null)
                    {
                        if (isMarginUpdated)
                        {
                            priceValue = cost + calculatedMargin;
                        }
                        else
                        {
                            priceValue = cost - calculatedDiscount;
                            if (priceValue < 0)
                            {
                                priceValue = 0;
                            }
                        }

                        //Update Price for extra and variant
                        extraItem.Price.NetPrice = priceValue;
                        extraItem.Price.GrossPrice = priceValue * (1 + (extraItem.Price.Taxes.Sum(tx => tx.Rate) / 100));
                        extraItem.Price.TotalTaxAmount = priceValue * (extraItem.Price.Taxes.Sum(tx => tx.Rate) / 100);
                        extraItem.Price.Taxes.ForEach(tx =>
                        {
                            tx.Amount = priceValue * (tx.Rate / 100);
                            dbContext.Entry(tx).State = EntityState.Modified;
                        });

                        dbContext.Entry(extraItem).State = EntityState.Modified;
                        dbContext.Entry(inventoryDetail).State = EntityState.Modified;
                    }
                }

                foreach (var variantIdItem in lstVariantDistinct)
                {
                    var variantItem = dbContext.PosVariants.FirstOrDefault(p => p.Id == variantIdItem);
                    if (variantItem == null)
                        continue;

                    var traderItem = variantItem.TraderItem;
                    var inventoryDetail = traderItem.InventoryDetails.FirstOrDefault(p => p.Location.Id == catalog.Location.Id);

                    var cost = (inventoryDetail != null ? (isAppliedToAverageCost ? inventoryDetail.AverageCost : inventoryDetail.LatestCost) : 0);
                    calculatedMargin = unitType == 1 ? (cost * (marginValue / 100)) : marginValue;/*1=%; 2=value*/
                    calculatedDiscount = unitType == 1 ? (cost * (discountValue / 100)) : discountValue;/*1=%; 2=value*/

                    if (inventoryDetail != null)
                    {
                        if (isMarginUpdated)
                        {
                            priceValue = cost + calculatedMargin;
                        }
                        else
                        {
                            priceValue = cost - calculatedDiscount;
                            if (priceValue < 0)
                            {
                                priceValue = 0;
                            }
                        }

                        //Update Price for extra and variant
                        variantItem.Price.NetPrice = priceValue;
                        variantItem.Price.GrossPrice = priceValue * (1 + (variantItem.Price.Taxes.Sum(tx => tx.Rate) / 100));
                        variantItem.Price.TotalTaxAmount = priceValue * (variantItem.Price.Taxes.Sum(tx => tx.Rate) / 100);
                        variantItem.Price.Taxes.ForEach(tx =>
                        {
                            tx.Amount = priceValue * (tx.Rate / 100);
                            dbContext.Entry(tx).State = EntityState.Modified;
                        });

                        dbContext.Entry(variantItem).State = EntityState.Modified;
                        dbContext.Entry(inventoryDetail).State = EntityState.Modified;
                    }
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, marginValue, discountValue,
                        isAppliedToAverageCost, unitType, catalogId, isMarginUpdated, lstExtraIds, lstVariantIds);
                returnJson.result = false;
                returnJson.msg = ResourcesManager._L("ERROR_MSG_5");
            }
            return returnJson;
        }

        public DataTablesResponse GetAffectedPrices(IDataTablesRequest requestModel,
            List<int> lstVariantIds, List<int> lstExtraIds, string keySearch, int catalogId, int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, lstVariantIds, lstExtraIds, catalogId);

                var lstVariantsQuery = dbContext.PosVariants.Where(p => lstVariantIds.Contains(p.Id));
                var lstExtrasQuery = dbContext.PosExtras.Where(p => lstExtraIds.Contains(p.Id));
                var catalog = dbContext.PosMenus.FirstOrDefault(p => p.Id == catalogId);
                if (catalog == null)
                {
                    return null;
                }

                var totalItem = 0;
                totalItem = lstVariantsQuery.Count() + lstExtrasQuery.Count();

                #region Filterting

                if (!string.IsNullOrEmpty(keySearch))
                {
                    lstVariantsQuery = lstVariantsQuery
                        .Where(p => p.Name.ToLower().Contains(keySearch.ToLower())
                                    || p.CategoryItem.Name.ToLower().Contains(keySearch.ToLower())
                                    || p.CategoryItem.Category.Name.ToLower().Contains(keySearch.ToLower()));
                    lstExtrasQuery = lstExtrasQuery
                        .Where(p => p.Name.ToLower().Contains(keySearch.ToLower())
                                    || p.CategoryItem.Name.ToLower().Contains(keySearch.ToLower())
                                    || p.CategoryItem.Category.Name.ToLower().Contains(keySearch.ToLower()));
                }

                var lstVariants = lstVariantsQuery.ToList();
                var lstExtras = lstExtrasQuery.ToList();
                var lstResult = new List<CatalogVariantExtraPrice>();

                //The rows must be displayed grouped by Category, Category Item then Variants above Extras.
                var lstVariantCustomModel = lstVariants.Select(p => new CatalogVariantExtraPrice()
                {
                    Id = p.Id,
                    IsVariant = true,
                    CategoryId = p.CategoryItem.Category.Id,
                    CategoryItemId = p.CategoryItem.Id,
                });

                var lstExtraCustomModel = lstExtras.Select(p => new CatalogVariantExtraPrice()
                {
                    Id = p.Id,
                    IsVariant = false,
                    CategoryId = p.CategoryItem.Category.Id,
                    CategoryItemId = p.CategoryItem.Id,
                });

                var lstExtraVariantItemQuery = lstVariantCustomModel.Union(lstExtraCustomModel).AsQueryable();
                var lstResultQuery = new List<CatalogVariantExtraPrice>();

                //Grouping by 3 fields - CatalogId, CatalogItemId and IsVariant
                var groupedQuery = from c in lstExtraVariantItemQuery
                                   group c by new
                                   {
                                       c.CategoryId,
                                       c.CategoryItemId
                                   } into gcs
                                   orderby gcs.Key.CategoryItemId
                                   select gcs;

                groupedQuery.ForEach(x => lstResultQuery.AddRange(x.OrderByDescending(k => k.IsVariant)));

                #endregion Filterting

                #region Paging

                lstResult = lstResultQuery.ToList().Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                lstResult.ForEach(p =>
                {
                    if (p.IsVariant)
                    {
                        var variant_item = dbContext.PosVariants.FirstOrDefault(x => x.Id == p.Id);
                        var averageCost = variant_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.AverageCost ?? 0;
                        var latestCost = variant_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.LatestCost ?? 0;
                        var netPrice = variant_item.Price.NetPrice;

                        p.ItemSKU = variant_item.TraderItem.SKU;
                        p.CategoryName = variant_item.CategoryItem.Category.Name;
                        p.CategoryItemName = variant_item.CategoryItem.Name;
                        p.Name = variant_item.Name;
                        p.AverageAndLastestCost = $"{averageCost.ToDecimalPlace(currencySetting)}/ {latestCost.ToDecimalPlace(currencySetting)}";
                        p.PriceId = variant_item.Price.Id;
                        p.NetPrice = variant_item.Price.NetPrice;
                        p.GrossPrice = variant_item.Price.GrossPrice;
                        p.TraderItemId = variant_item.TraderItem.Id;
                        p.ListTaxes = variant_item.Price.Taxes.Select(m => new CustomizedPricingTax
                        {
                            Id = m.Id,
                            Amount = m.Amount,
                            TaxName = m.TaxName
                        }).ToList();
                        p.FlaggedForTaxUpdate = variant_item.Price.FlaggedForTaxUpdate;
                        p.FlaggedForLatestCostUpdate = variant_item.Price.FlaggedForLatestCostUpdate;

                        decimal percentage = 100;
                        if (netPrice > 0)
                            percentage = p.ListTaxes.Sum(a => a.Amount) / netPrice;
                        p.MarginLatestCost = $"{(netPrice - latestCost).ToDecimalPlace(currencySetting)}/ {percentage.ToDecimalPlace(currencySetting)} %";
                        p.MarginAverageCost = $"{(netPrice - averageCost).ToDecimalPlace(currencySetting)}/ {percentage.ToDecimalPlace(currencySetting)}%";
                    }
                    else
                    {
                        var extra_item = dbContext.PosExtras.FirstOrDefault(x => x.Id == p.Id);
                        var averageCost = extra_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.AverageCost ?? 0;
                        var latestCost = extra_item.TraderItem?.InventoryDetails?.FirstOrDefault(x => x.Location.Id == locationId)?.LatestCost ?? 0;
                        var netPrice = extra_item.Price.NetPrice;

                        p.ItemSKU = extra_item.TraderItem.SKU;
                        p.CategoryName = extra_item.CategoryItem.Category.Name;
                        p.CategoryItemName = extra_item.CategoryItem.Name;
                        p.Name = extra_item.Name;
                        p.AverageAndLastestCost = $"{averageCost.ToDecimalPlace(currencySetting)}/ {latestCost.ToDecimalPlace(currencySetting)}";
                        p.PriceId = extra_item.Price.Id;
                        p.NetPrice = extra_item.Price.NetPrice;
                        p.GrossPrice = extra_item.Price.GrossPrice;
                        p.TraderItemId = extra_item.TraderItem.Id;
                        p.ListTaxes = extra_item.Price.Taxes.Select(m => new CustomizedPricingTax
                        {
                            Id = m.Id,
                            Amount = m.Amount,
                            TaxName = m.TaxName
                        }).ToList();

                        p.FlaggedForTaxUpdate = extra_item.Price.FlaggedForTaxUpdate;
                        p.FlaggedForLatestCostUpdate = extra_item.Price.FlaggedForLatestCostUpdate;

                        decimal percentage = 100;
                        if (netPrice > 0)
                            percentage = p.ListTaxes.Sum(a => a.Amount) / netPrice;
                        p.MarginLatestCost = $"{(netPrice - latestCost).ToDecimalPlace(currencySetting)}/ {percentage.ToDecimalPlace(currencySetting)} %";
                        p.MarginAverageCost = $"{(netPrice - averageCost).ToDecimalPlace(currencySetting)}/ {percentage.ToDecimalPlace(currencySetting)}%";
                    }
                });

                return new DataTablesResponse(requestModel.Draw, lstResult, totalItem, totalItem);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, lstVariantIds, lstExtraIds, catalogId);
                return new DataTablesResponse(requestModel.Draw, new List<CatalogVariantExtraPrice>(), 0, 0);
            }
        }

        // Get all Ids of Filtered Prices of the catalog
        public ReturnJsonModel GetFilteredCatalogPriceIds(int catalogId = 0, string traderItemSKU = "",
            List<int> lstCategoryId = null, string categoryItemNameKeySearch = "", string variantOrExtraNameKeySearch = "")
        {
            var result = new ReturnJsonModel()
            {
                result = false,
                Object = null
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderItemSKU,
                        lstCategoryId, categoryItemNameKeySearch, variantOrExtraNameKeySearch);

                #region Filterting

                var lstCategories = dbContext.PosMenus.Where(p => p.Id == catalogId).SelectMany(p => p.Categories);
                if (lstCategoryId != null)
                {
                    lstCategories = lstCategories.Where(p => lstCategoryId.Any(x => x == p.Id));
                }

                var lstCategoryItems = lstCategories.SelectMany(p => p.PosCategoryItems).Distinct().AsQueryable();
                if (!string.IsNullOrEmpty(categoryItemNameKeySearch))
                {
                    lstCategoryItems = lstCategoryItems.Where(p => p.Name.ToLower().Contains(categoryItemNameKeySearch.ToLower()));
                }

                var lstVariantsQuery = lstCategoryItems.SelectMany(p => p.PosVariants).AsQueryable();
                var lstExtrasQuery = lstCategoryItems.SelectMany(p => p.PosExtras).AsQueryable();
                if (!string.IsNullOrEmpty(traderItemSKU))
                {
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.TraderItem.SKU != null && p.TraderItem.SKU.ToLower().Contains(traderItemSKU.ToLower()));
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.TraderItem.SKU != null && p.TraderItem.SKU.ToLower().Contains(traderItemSKU.ToLower()));
                }
                if (!string.IsNullOrEmpty(variantOrExtraNameKeySearch))
                {
                    lstVariantsQuery = lstVariantsQuery.Where(p => p.Name.ToLower().Contains(variantOrExtraNameKeySearch.ToLower()));
                    lstExtrasQuery = lstExtrasQuery.Where(p => p.Name.ToLower().Contains(variantOrExtraNameKeySearch.ToLower()));
                }

                #endregion Filterting

                var lstVariants = lstVariantsQuery;
                var lstExtras = lstExtrasQuery;
                var lstResult = new List<CatalogVariantExtraPrice>();

                // The rows must be displayed grouped by Category, Category Item then Variants above Extras.
                // NOTE: format of the return object must match
                //      the format used in pos.menu.js file - for lst_selected_trader_item_ids field
                var lstVariantCustomModel = lstVariants.Select(p => new
                {
                    extra_id = 0,
                    item_id = p.TraderItem.Id,
                    price_id = p.Price.Id,
                    variant_id = p.Id
                });

                var lstExtraCustomModel = lstExtras.Select(p => new
                {
                    extra_id = p.Id,
                    item_id = p.TraderItem.Id,
                    price_id = p.Price.Id,
                    variant_id = 0
                });

                var lstIds = lstVariantCustomModel.Union(lstExtraCustomModel).ToList();
                result.result = true;
                result.Object = lstIds;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderItemSKU, lstCategoryId,
                    categoryItemNameKeySearch, variantOrExtraNameKeySearch);
                result.result = false;
                result.Object = null;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel UpdateCatalogItemVariantPrice(int variantId, decimal variantGrossPrice)
        {
            var result = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, variantId, variantGrossPrice);
                }
                var variantInDb = dbContext.PosVariants.FirstOrDefault(p => p.Id == variantId);
                if (variantInDb == null)
                {
                    result.result = false;
                    result.msg = "The variant does not exist";
                    return result;
                }

                if (variantInDb.Price == null)
                {
                    variantInDb.Price = new CatalogPrice()
                    {
                        GrossPrice = 0,
                        NetPrice = 0,
                        TotalTaxAmount = 0,
                        Taxes = new List<PriceTax>()
                    };
                    dbContext.Entry(variantInDb.Price).State = EntityState.Added;
                    dbContext.CatalogPrices.Add(variantInDb.Price);
                    dbContext.SaveChanges();
                }

                variantInDb.Price.GrossPrice = variantGrossPrice;
                var totalTaxRatePercentage = variantInDb.Price.Taxes.Sum(p => p.Rate);
                variantInDb.Price.NetPrice = variantInDb.Price.GrossPrice / ((100 + totalTaxRatePercentage) / 100);
                variantInDb.Price.TotalTaxAmount = variantInDb.Price.GrossPrice - variantInDb.Price.NetPrice;
                variantInDb.Price.Taxes.ForEach(taxItem =>
                {
                    taxItem.Amount = variantInDb.Price.NetPrice * (taxItem.Rate / 100);
                });

                dbContext.Entry(variantInDb.Price).State = EntityState.Modified;
                dbContext.SaveChanges();

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, variantId, variantGrossPrice);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel PublishCatalogInProfile(int catId, bool isPublish)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, catId, isPublish);

                var catalog = dbContext.PosMenus.Find(catId);
                catalog.IsPublished = isPublish;
                refModel.result = dbContext.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, catId, isPublish);
                refModel.result = false;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public List<listB2BPartnershipInCatalog> ListPartnershipInDomain(int domainId)
        {
            var apiBaseUrl = ConfigManager.ApiGetDocumentUri;
            var catalogsB2B = GetCatalogsByDomainId(domainId, SalesChannelEnum.B2B);
            var b2bprofiles = dbContext.B2BProfiles;
            var listB2BPartnerships = catalogsB2B.Where(y => y.PurchaseSalesPartnerships.Any(t => t.ConsumerDomain != null)).Select(e => new listB2BPartnershipInCatalog
            {
                id = e.Id,
                consumerDomain = e.PurchaseSalesPartnerships.Select(t => new simpleDomainInfo
                {
                    ConsumerDomainId = t.ConsumerDomain.Id,
                    Name = b2bprofiles.Where(b => b.Domain.Id == t.ConsumerDomain.Id).FirstOrDefault()?.BusinessName ?? t.ConsumerDomain?.Name ?? "",
                    LogoUriDomain = apiBaseUrl + t.ConsumerDomain.LogoUri,
                    DateCoOp = t.CreatedDate,
                    BusinessSummary = b2bprofiles.Where(b => b.Domain.Id == t.ConsumerDomain.Id).FirstOrDefault()?.BusinessSummary ?? "",
                    BusinessMail = b2bprofiles.Where(b => b.Domain.Id == t.ConsumerDomain.Id).FirstOrDefault()?.BusinessEmail ?? ""
                }).ToList()
            }).ToList();
            return listB2BPartnerships;
        }

        public listB2BPartnershipInCatalog ListPartnershipInCatalog(int domainId, int catalogId, string keysearch = "")
        {
            var apiBaseUrl = ConfigManager.ApiGetDocumentUri;
            var catalogsB2B = GetCatalogsByDomainId(domainId, SalesChannelEnum.B2B).Where(e => e.Id == catalogId);
            var b2bprofiles = dbContext.B2BProfiles;
            if (!keysearch.IsNullOrEmpty()) keysearch = keysearch.ToLower();
            var listB2BPartnerships = catalogsB2B.Where(y => y.PurchaseSalesPartnerships.Any(t => t.ConsumerDomain != null)).Select(e => new listB2BPartnershipInCatalog
            {
                id = e.Id,
                consumerDomain = e.PurchaseSalesPartnerships.Where(z => z.ConsumerDomain.Name.ToLower().Contains(keysearch)).Select(t => new simpleDomainInfo
                {
                    ConsumerDomainId = t.ConsumerDomain.Id,
                    Name = b2bprofiles.Where(b => b.Domain.Id == t.ConsumerDomain.Id).FirstOrDefault()?.BusinessName ?? t.ConsumerDomain?.Name ?? "",
                    LogoUriDomain = apiBaseUrl + t.ConsumerDomain.LogoUri,
                    DateCoOp = t.CreatedDate,
                    BusinessSummary = b2bprofiles.Where(b => b.Domain.Id == t.ConsumerDomain.Id).FirstOrDefault()?.BusinessSummary ?? "",
                    BusinessMail = b2bprofiles.Where(b => b.Domain.Id == t.ConsumerDomain.Id).FirstOrDefault()?.BusinessEmail ?? "",
                    Phone = b2bprofiles.Where(b => b.Domain.Id == t.ConsumerDomain.Id).FirstOrDefault()?.BusinessLocations.Where(z => z.IsDefaultAddress).FirstOrDefault()?.Address?.Phone ?? ""
                }).ToList()
            }).FirstOrDefault();
            return listB2BPartnerships;
        }
    }
}