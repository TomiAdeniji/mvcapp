
using Microsoft.Ajax.Utilities;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class PointOfSaleMenuController : BaseController
    {
        public ActionResult GetById(int id)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                var menu = new PosMenuRules(dbContext).GetById(id);
                result.Object = new
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    SalesChannel = menu.SalesChannel.ToString(),
                    Dimensions = (menu.OrderItemDimensions != null ? menu.OrderItemDimensions.Select(q => q.Id).ToList() : new List<int>()),
                    Description = menu.Description,
                    LocationId = menu.Location?.Id ?? 0,
                    Type = menu.Type
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPriceValue(int traderid, int locationid, SalesChannelEnum saleChannel)
        {
            var result = new PosMenuRules(dbContext).GetPriceValue(traderid, locationid, saleChannel);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPriceValueBySKU(string sku, int locationid, SalesChannelEnum saleChannel)
        {
            var result = new PosMenuRules(dbContext).GetPriceValueBySKU(sku, locationid, saleChannel);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTraderItemBySku(string sku, int locationId)
        {
            var rs = new ReturnJsonModel();
            var currentDomainId = CurrentDomainId();
            var traderItems = new TraderItemRules(dbContext).GetTraderItemsDomainSkuLocation(sku, locationId, currentDomainId);
            var item = dbContext.TraderItems.FirstOrDefault(p => p.SKU == sku && p.Domain.Id == currentDomainId);
            if (item == null)
            {
                rs.result = false;
                rs.msg = "Can not find item with the given SKU";
            }
            else
            {
                rs.result = true;
                rs.Object = new
                {
                    Id = item.Id,
                    SKU = item.SKU,
                    Items = traderItems.Select(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Units = x.Units.Select(p => new { p.Id, p.Name, p.QuantityOfBaseunit, p.IsBase }).ToList()
                    })
                };
            };

            return Json(rs, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreatePosMenu(Catalog posMenu, bool isLocationFromLocal = false)
        {
            if (!isLocationFromLocal)
                posMenu.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            else
                posMenu.Location = new TraderLocationRules(dbContext).GetById(posMenu.Location.Id);
            posMenu.Domain = CurrentDomain();
            var result = new PosMenuRules(dbContext).CreatePosMenu(posMenu, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePosMenu(Catalog posMenu)
        {
            posMenu.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            posMenu.Type = new PosMenuRules(dbContext).GetById(posMenu.Id).Type;
            var result = new PosMenuRules(dbContext).UpdatePosMenu(posMenu);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateExtrasPosCategoryItem(Extra extras)
        {
            var result = new PosMenuRules(dbContext).UpdatePosExtra(extras);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ClonePosMenu(Catalog posMenu)
        {
            posMenu.Location = new TraderLocationRules(dbContext).GetById(posMenu.Location.Id);
            posMenu.Domain = CurrentDomain();
            var result = new PosMenuRules(dbContext).ClonePosMenu(posMenu, CurrentUser(), CurrentDomainId());

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePosCategoryItem(CategoryItem posCategoryItem, decimal? itemPrice = null)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 2,
                msgId = posCategoryItem.Id.ToString()
            };

            var itemCategory = new PosMenuRules(dbContext).GetCategoryItemByName(posCategoryItem.Name, posCategoryItem.Category.Menu.Id);
            if (itemCategory != null && itemCategory.Id != posCategoryItem.Id)
            {
                result.msg = ResourcesManager._L("ERROR_DATA_EXISTED", posCategoryItem.Name);
                result.actionVal = 3;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (posCategoryItem.Category.Id == 0)
            {
                var category = posCategoryItem.Category.Id == 0 ? new PosMenuRules(dbContext).GetCategoryByName(posCategoryItem.Category.Name, posCategoryItem.Category.Menu.Id)
                                                : dbContext.PosCategories.Find(posCategoryItem.Category.Id);
                if (category != null)
                {
                    result.msg = "\"" + posCategoryItem.Category.Name + "\" already exists!";
                    result.actionVal = 3;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    posCategoryItem.Category.IsVisible = true;
                }
            }
            result = new PosMenuRules(dbContext).UpdateNameCategoryOfCategoryItem(posCategoryItem, CurrentUser().Id, itemPrice);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult SaveCategoryItem(CategoryItem posCategoryItem)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0,
                msgId = posCategoryItem.Id.ToString()
            };

            var itemCategory = new PosMenuRules(dbContext).GetCategoryItemByName(posCategoryItem.Name, posCategoryItem.Category.Menu.Id);
            if (itemCategory != null && itemCategory.Id != posCategoryItem.Id)
            {
                result.msg = ResourcesManager._L("ERROR_DATA_EXISTED", posCategoryItem.Name);

                result.actionVal = 3;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (posCategoryItem.Category.Id == 0)
            {
                var category = posCategoryItem.Category.Id == 0 ? new PosMenuRules(dbContext).GetCategoryByName(posCategoryItem.Category.Name, posCategoryItem.Category.Menu.Id)
                                                : dbContext.PosCategories.Find(posCategoryItem.Category.Id);
                if (category != null)
                {
                    result.msg = ResourcesManager._L("ERROR_DATA_EXISTED", posCategoryItem.Category.Name);
                    result.actionVal = 3;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            var user = new UserRules(dbContext).GetById(CurrentUser().Id);
            posCategoryItem.Category.CreatedBy = user;
            posCategoryItem.Category.CreatedDate = DateTime.UtcNow;
            if (posCategoryItem.PosVariants != null && posCategoryItem.PosVariants.Count > 0)
            {
                foreach (var item in posCategoryItem.PosVariants)
                {
                    item.CreatedBy = user;
                    item.CreatedDate = DateTime.UtcNow;
                    item.ImageUri = posCategoryItem.ImageUri;
                }
            }
            posCategoryItem.CreatedBy = user;
            posCategoryItem.CreatedDate = DateTime.UtcNow;
            result = new PosMenuRules(dbContext).SaveCategoryItem(posCategoryItem);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult SaveVariant(Variant variant)
        {

            var result = new PosMenuRules(dbContext).SaveVariant(variant, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePosCategoryItemImage(int idItem, string imageurl)
        {
            var result = new PosMenuRules(dbContext).UpdatePosCategoryItemImageUrl(idItem, imageurl);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveExtras(Extra extras, int locationid)
        {

            var result = new PosMenuRules(dbContext).SavePosExtras(extras, locationid, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult DeleteExtras(int id)
        {
            var result = new PosMenuRules(dbContext).DeleteExtras(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult DeleteProperty(int id)
        {
            var result = new PosMenuRules(dbContext).DeleteProperty(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateVariantOptions(VariantProperty posVariantProperty)
        {

            var result = new PosMenuRules(dbContext).UpdatePosVariantProperty(posVariantProperty, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetCategoryByMenu(int idMenu)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                var menu = new PosMenuRules(dbContext).GetById(idMenu);
                result.Object = new Catalog
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Description = menu.Description,
                    Categories = menu.Categories.Select(q => new Category() { Id = q.Id, Name = q.Name, IsVisible = q.IsVisible, Description = q.Description }).OrderBy(q => q.Name).ToList()
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                result.result = false;
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddCategory(Category category)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            if (category.Id == 0)
            {
                var categoryItem = new PosMenuRules(dbContext).GetCategoryByName(category.Name, category.Menu.Id);
                if (categoryItem != null)
                {
                    ResourcesManager._L("ERROR_DATA_EXISTED", category.Name);
                    result.actionVal = 3;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            result = new PosMenuRules(dbContext).AddPosCategory(category, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult UpdateCategory(Category category)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            var categoryItem = new PosMenuRules(dbContext).GetCategoryByName(category.Name, category.Menu.Id);
            if (categoryItem != null && category.Id != categoryItem.Id)
            {
                ResourcesManager._L("ERROR_DATA_EXISTED", category.Name);
                result.actionVal = 3;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            result = new PosMenuRules(dbContext).UpdatePosCategory(category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult DeleteCategory(int id)
        {
            var result = new PosMenuRules(dbContext).DeletePosCategory(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult DeleteCategoryItem(int id)
        {
            var result = new PosMenuRules(dbContext).DeletePosCategoryItem(id);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetPosCategoryItemView(int id)
        {
            var posCategoryItem = new PosMenuRules(dbContext).GetPosCategoryItemById(id);
            var variants = new PosMenuRules(dbContext).GetPosVariantsOfProperties(id);
            ViewBag.Variants = variants;
            return PartialView("_PosCategoryItemView", posCategoryItem);

        }

        public ActionResult GetPosCategories(int idmenu)
        {
            try
            {
                var menu = new PosMenuRules(dbContext).GetById(idmenu);
                return PartialView("_PosCategoriesTable", menu);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, this.CurrentUser().Id);
                return null;
            }

        }

        public ActionResult GetPosCategoryItemPosExtras(int iditem)
        {
            var categoryItem = new PosMenuRules(dbContext).GetPosCategoryItemById(iditem);
            return PartialView("_PosExtrasTable", categoryItem.PosExtras);
        }

        public ActionResult GetPosCategoryItemPosProperties(int iditem)
        {
            var categoryItem = new PosMenuRules(dbContext).GetPosCategoryItemById(iditem);
            return PartialView("_PosPropertyTable", categoryItem.VariantProperties);
        }

        public ActionResult GetCategoryByMenuId(int idMenu)
        {
            try
            {
                var menu = new PosMenuRules(dbContext).GetById(idMenu);
                if (menu.Categories == null)
                {
                    menu.Categories = new List<Category>();
                }
                return PartialView("_PosCategoryTable", menu.Categories.OrderBy(q => q.Name).ToList());
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, this.CurrentUser().Id);
                return null;
            }


        }

        public ActionResult GetVariantsByProperty(int categoryItemId)
        {
            try
            {
                var variants = new PosMenuRules(dbContext).GetPosVariantsOfProperties(categoryItemId);
                return PartialView("_PosVariantsTableByProperty", variants);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, this.CurrentUser().Id);
                return null;
            }


        }

        public ActionResult DeleteMenu(int id)
        {
            var result = new PosMenuRules(dbContext).DeleteMenu(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RefreshPrices(List<int> categoryIds)
        {
            var result = new PosMenuRules(dbContext).RefreshPrices(categoryIds);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PushPricesToPricingPool(List<int> categoryIds)
        {
            var result = new PosMenuRules(dbContext).PushPricesToPricingPool(categoryIds);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePosMenuProduct(int menuId)
        {
            var result = new PosMenuRules(dbContext).UpdatePosMenuProduct(menuId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyHangfireState(string jobId)
        {
            var status = new PosMenuRules(dbContext).VerifyHangfireState(jobId);
            return Json(new { result = status }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyCatalogStatus(int catalogId, int locationId = 0, string type = "QuickMode")
        {
            var status = new PosMenuRules(dbContext).VerifyCatalogStatus(catalogId, locationId, type, CurrentUser().Timezone);
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This is funtions return Posmenus content
        /// </summary>
        /// <param name="locationId">TraderLocationId</param>
        /// <param name="keyword">keyword</param>
        /// <param name="status">All,Activated,Inactive</param>
        /// <returns></returns>
        public ActionResult LoadPosMenu(List<int> locationIds, string keyword, int catalogSearchType = -1, int salesChannel = 0)
        {
            var domainId = CurrentDomainId();
            ViewBag.isDisplayLocation = locationIds == null ? false : true;
            if (locationIds == null)
            {
                locationIds = new List<int>();
                locationIds.Add(CurrentLocationManage());
            }
            ViewBag.Menu = new PosMenuRules(dbContext).FiltersCatalog(locationIds, keyword, true, salesChannel, catalogSearchType, domainId);

            ViewBag.listB2BPartnerships = new PosMenuRules(dbContext).ListPartnershipInDomain(domainId);

            return PartialView("~/Views/PointOfSale/_PosMenuContent.cshtml");
        }

        public ActionResult GetListPartnershipInCatalog(int catalogId, string keysearch="")
        {
            var domainId = CurrentDomainId();
            var listPartner = new PosMenuRules(dbContext).ListPartnershipInCatalog(domainId, catalogId, keysearch);
            return PartialView("~/Views/PointOfSale/_ConsumerBusinesses.cshtml", listPartner);
        }

        public ActionResult SaveCatalogueQuickMode(Catalog catalogue, List<int> productGroupIds, int locationId, List<int> filterIds)
        {
            if (filterIds == null)
            {
                filterIds = new List<int>();
            }
            catalogue.Domain = CurrentDomain();
            var saveResult = new PosMenuRules(dbContext).ProcessQuickCatalogueCreattion(catalogue, productGroupIds,
                locationId, filterIds, CurrentUser());
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenderCatalogUI(int catalogId, int locationId = 0)
        {
            var saveResult = new PosMenuRules(dbContext).RenderCatalogUI(catalogId, locationId, CurrentUser().Timezone);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Type of action while selected an item
        /// 1- Add new item, 2 - add extra, 3 - add variant
        /// </summary>
        /// <param name="search"></param>
        /// <param name="locationId"></param>
        /// <param name="selectItemType"></param>
        /// <param name="categoryItemId"></param>
        /// <returns></returns>
        public ActionResult ShowPosItemModal(string search, int locationId, int selectItemType, int idposition = 0)
        {
            try
            {
                ViewBag.Search = search;
                ViewBag.LocationId = locationId;
                ViewBag.Position = idposition;
                ViewBag.SelectItemType = selectItemType;
                ViewBag.ItemGroups = new TraderGroupRules(dbContext).GetTraderGroupItemOnly(CurrentDomainId());

                return PartialView("_PosItemModal");
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, this.CurrentUser().Id);
                return null;
            }
        }

        public ActionResult FindItemServerside([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            string search, int locationId, bool nonInventory, int productGroupId)
        {
            var result = new PosMenuRules(dbContext).FindItemServerside(requestModel, CurrentDomainId(), locationId, nonInventory, search, productGroupId);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// //Referenced from Trader App => Configuration Master => Setup Power
        /// </summary>
        /// <param name="catalogId">The id of the menu id</param>
        /// <param name="id">The id of the price</param>
        /// <param name="isTaxIncluded">True - the value param is the NEW GrossPrice, False - the value param is the NEW NetPrice</param>
        /// <param name="value">New Gross or Net Price</param>
        /// <returns></returns>
        public ActionResult UpdateCatalogPriceItem(int catalogId, int id, bool isTaxIncluded, decimal value)
        {
            var domainId = CurrentDomainId();
            var currentUserId = CurrentUser().Id;
            var updateResult = new PosMenuRules(dbContext).UpdateCatalogItemPrice(catalogId, id, isTaxIncluded, value, domainId, currentUserId);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AppplyBulkMargin(decimal marginValue = 0, decimal discountValue = 0,
            bool isAppliedToAverageCost = true, int unitType = 1, string catalogKey = "",
            bool isMarginUpdated = true, List<int> lstVariantIds = null, List<int> lstExtraIds = null)
        {
            var catalogId = string.IsNullOrEmpty(catalogKey) ? 0 : int.Parse(catalogKey.Decrypt());
            lstVariantIds = lstVariantIds == null ? new List<int>() : lstVariantIds;
            lstExtraIds = lstExtraIds == null ? new List<int>() : lstExtraIds;
            var updateResult = new PosMenuRules(dbContext).ApplyConfigPriceByGroup(marginValue, discountValue,
                isAppliedToAverageCost, unitType, catalogId, isMarginUpdated, lstExtraIds, lstVariantIds);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListAffectedPriceItem([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            List<int> lstVariants, List<int> lstExtras, string catalogKey, string keySearch = "", int locationId = 0)
        {
            var catalogId = string.IsNullOrEmpty(catalogKey) ? 0 : int.Parse(catalogKey.Decrypt());
            lstVariants = lstVariants.Distinct().ToList();
            lstExtras = lstExtras.Distinct().ToList();
            var result = new PosMenuRules(dbContext)
                .GetAffectedPrices(requestModel, lstVariants, lstExtras, keySearch, catalogId, locationId, CurrentDomainId());
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetFilteredPriceIds(string catalogKey,
            string traderItemSKU = "", List<int> lstCategoryId = null, string categoryItemNameKeySearch = "", string variantOrExtraName = "")
        {
            var catalogId = string.IsNullOrEmpty(catalogKey) ? 0 : int.Parse(catalogKey.Decrypt());
            var result = new PosMenuRules(dbContext)
                .GetFilteredCatalogPriceIds(catalogId, traderItemSKU, lstCategoryId, categoryItemNameKeySearch, variantOrExtraName);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateCatalogItemVariantPrice(int variantId, decimal variantGrossPrice)
        {
            var updateResult = new PosMenuRules(dbContext).UpdateCatalogItemVariantPrice(variantId, variantGrossPrice);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTraderItemDescriptionById(int id)
        {
            var result = new ReturnJsonModel
            {
                result = true
            };
            try
            {
                var item = new TraderItemRules(dbContext).GetById(id);
                result.Object = item.Description;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                result.msg = ex.Message;
                result.result = false;
                result.Object = "";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult OpenPriceAdvance(int menuId, string type)
        {
            var model = dbContext.PosCategories.AsNoTracking().Where(e => e.Menu.Id == menuId).ToList();
            var catalog = dbContext.PosMenus.AsNoTracking().Where(e => e.Id == menuId).FirstOrDefault();
            if (type == "refresh")
                return PartialView("_PriceModalRefresh", model);
            if (type == "import")
                return PartialView("_ImportProduct", catalog);
            return PartialView("_PriceModalPush", model);
           
        }

        public ActionResult GetCategoryByCatalog(int idMenu)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                var menu = new PosMenuRules(dbContext).GetById(idMenu);
                result.Object = new Catalog
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Description = menu.Description,
                    Categories = menu.Categories.Select(q => new Qbicles.Models.Catalogs.Category() { Id = q.Id, Name = q.Name, IsVisible = q.IsVisible, Description = q.Description }).OrderBy(q => q.Id).ToList()
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                result.result = false;
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MappingProductGroupsWithCategories(List<ListProductGroupsWithCategory> listProductGroups, int menuId, int locationId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, listProductGroups, menuId, locationId);
            foreach (var item in listProductGroups)
            {
                List<int> lst = new List<int>(item.ProductGroupsId);
                if (String.IsNullOrEmpty(item.CategoryId.ToString()))
                {
                    return Json("one of Categories is null", JsonRequestBehavior.AllowGet);
                }
                UpdateCategoryWithProductGroupIds(lst, item.CategoryId, locationId, menuId);
            }
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        public void UpdateCategoryWithProductGroupIds(List<int> productGroupIds, int categoryId, int locationId, int catalogId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, productGroupIds, categoryId, locationId);

                var user = dbContext.QbicleUser.Find(CurrentUser().Id);
                var location = dbContext.TraderLocations.Find(locationId);
                var productGroups = dbContext.TraderGroups.Where(g => productGroupIds.Contains(g.Id)).ToList();
                var listTraderItem = new List<TraderItem>();

                //data will be savechanges
                var posCategory = new List<Category>();
                var posCategoryItems = new List<CategoryItem>();
                var posVariants = new List<Variant>();

                var categoryTarget = dbContext.PosCategories.Find(categoryId);
                // select all categoryItem in targetCategory without PosExtra
                var listTraderItemFromCategory = categoryTarget.PosCategoryItems.Where(item => item.PosVariants.Count == 1 && item.PosExtras.Count == 0).SelectMany(e => e.PosVariants).Select(p => p.TraderItem).ToList();
                //For each selected Product Group ...
                new PosMenuRules(dbContext).ProcessCategoryItem(productGroups, categoryId, catalogId, locationId, CurrentUser().Id, out posCategory, out posCategoryItems, out posVariants);

                dbContext.PosVariants.AddRange(posVariants);
                categoryTarget.PosCategoryItems.AddRange(posCategoryItems);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, productGroupIds, categoryId, locationId);
            }
        }

        public ActionResult ViewMenuDevices(int menuId)
        {
            var menu = new PosMenuRules(dbContext).GetById(menuId);

            return PartialView("_ViewMenuDevicesModal", menu);
        }

    }
}