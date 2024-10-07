using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Resources;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderItemController : BaseController
    {
        public async Task<ActionResult> SaveItemProduct(TraderItem item, CreateInventoryCustom createInventory, int currentLocationId = 0, bool isCurrentLocation = false)
        {
            item.Domain = CurrentDomain();
            var result = await new TraderItemRules(dbContext).SaveTraderItem(item, createInventory, currentLocationId, isCurrentLocation, CurrentUser().Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult validateUniqueSKUandBarcode(int traderId, string SKU, string Barcode)
        {
            bool existSKU = false, existBarcode = false;
            new TraderItemRules(dbContext).ValidateUniqueSkuAndBarcode(traderId, CurrentDomainId(), SKU, Barcode, ref existSKU, ref existBarcode);
            return Json(new { sku = existSKU, barcode = existBarcode }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveContact(TraderContact contact, string country = "")
        {
            var result = new TraderItemRules(dbContext).SaveContact(contact, country, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTraderItem(int id)
        {
            var item = new TraderItemRules(dbContext).GetById(id);
            return PartialView("_TraderItemAdditional", item);
        }

        public ActionResult GetTraderItemDescription(int id)
        {
            var item = new TraderItemRules(dbContext).GetById(id);
            return PartialView("_TraderItemDescription", item);
        }

        [HttpDelete]
        public ActionResult DeleteVendorContact(int id)
        {
            var traderItem = new TraderItemRules(dbContext);
            var result = traderItem.DeleteVendorContact(CurrentDomainId(), id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddRemoveLocation(int locationId, int traderItemId)
        {
            var result = new TraderItemRules(dbContext).UpdateLocationTraderItem(locationId, traderItemId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveLocationVendor(int vendorId, int traderItemId)
        {
            var result = new TraderItemRules(dbContext).UpdateLocationVendorTraderItem(vendorId, traderItemId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LocationVendorPrimaryChange(int traderItemId, int locationvendorId)
        {
            var result = new TraderItemRules(dbContext).LocationVendorPrimaryChange(traderItemId, locationvendorId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteUnitConversion(int unitId)
        {
            var result = new TraderConversionUnitRules(dbContext).DeleteByUnit(unitId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetUnitByTraderItem(int itemId = 0)
        {
            var productUnits = new TraderItemRules(dbContext).GetUnitByTraderItem(itemId).Select(c => new
            {
                Id = c.Id,
                Name = c.Name,
                QuantityOfBaseunit = c.QuantityOfBaseunit
            }).ToList();
            return Json(productUnits, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetListIngredient(List<Ingredient> lstIng, int currentLocationId = 0)
        {
            if (currentLocationId == 0) currentLocationId = CurrentLocationManage();
            foreach (var item in lstIng)
            {
                if (item.SubItem != null && item.SubItem.Id > 0)
                {
                    var subItem = new TraderItemRules(dbContext).GetById(item.SubItem.Id);
                    item.SubItem = new TraderItem()
                    {
                        Id = subItem.Id,
                        InventoryDetails = subItem.InventoryDetails.Where(p => p.Location.Id == currentLocationId).Select(c =>
                        new InventoryDetail()
                        {
                            AverageCost = c.AverageCost,
                            LatestCost = c.LatestCost
                        }
                    ).ToList()
                    };
                    var testInventoryDetails = subItem.InventoryDetails.Where(p => p.Location.Id == currentLocationId).ToList();
                }
            }
            var lstIngObject = lstIng.Select(q => new
            {
                q.Id,
                q.ParentRecipe,
                q.Quantity,
                q.SubItem,
                q.Unit,
                Units = new TraderItemRules(dbContext).GetUnitByTraderItem(q.SubItem != null ? q.SubItem.Id : 0).Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.QuantityOfBaseunit
                }).ToList()
            }).ToList();

            return Json(lstIngObject, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRecipeById(int id)
        {
            var result = new ReturnJsonModel();
            var recipe = new TraderRecipeRules(dbContext).GetById(id);
            result.Object = new
            {
                recipe.Id,
                recipe.Name,
                recipe.IsActive,
                Ingredients = recipe.Ingredients.Select(c => new
                {
                    c.Id,
                    SubItem = new { Id = c.SubItem.Id },
                    c.Quantity,
                    Unit = new { c.Unit.Id },
                    AverageCost = c.SubItem.InventoryDetails.Any() ? c.SubItem.InventoryDetails[0].AverageCost : 0,
                    LatestCost = c.SubItem.InventoryDetails.Any() ? c.SubItem.InventoryDetails[0].LatestCost : 0
                }).ToList()
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckBookkeepingConnected()
        {
            var result = new TraderItemRules(dbContext).CheckBookkeepingConnected(CurrentDomainId());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveRecipe(Recipe recipe, int traderItemId)
        {
            var result = new TraderItemRules(dbContext).SaveRecipe(recipe, traderItemId, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeActiveRecipe(int recipeId, bool value)
        {
            var result = new TraderRecipeRules(dbContext).ChangeActive(recipeId, value);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get buy item I sell (IsSold = true)
        /// </summary>
        /// <param name="workGroupId"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public ActionResult GetItemProductByWorkgroup(int workGroupId, int locationId)
        {
            var result = new ReturnJsonModel();
            result.result = true;
            ViewBag.locationId = locationId;
            var traderItems = CurrentDomain().TraderItems.Where(q => q.Locations.Any(z => z.Id == locationId) && q.IsSold && q.Group.WorkGroupCategories.Select(w => w.Id).Contains(workGroupId)).Select(i => new TraderItemModel
            {
                Id = i.Id,
                Name = i.Name,
                ImageUri = i.ImageUri,
                CostUnit = 0,
                TaxRateName = i.StringItemTaxRates(true),
                TaxRateValue = i.SumTaxRatesPercent(true)
            }).OrderBy(n => n.Name).ToList();
            result.Object = traderItems;
            return Json(result, JsonRequestBehavior.AllowGet);
            //return PartialView("_TraderItemsSelectControl", traderItems);
        }

        public ActionResult AdjustStockTab(int locationId = 0)
        {
            return PartialView("_TraderItemAdjustStockTab");
        }

        public ActionResult GetListRreourceImage()
        {
            ViewBag.ResourceCategorys = new TraderResourceRules(dbContext).GetListResourceCategory(CurrentDomainId(), ResourceCategoryType.Image);
            var resourceImages = new TraderResourceRules(dbContext).GetListResourceImages(CurrentDomainId());
            return PartialView("_TraderItemSelectImageResources", resourceImages);
        }

        public ActionResult GetListRreourceDocument()
        {
            ViewBag.ResourceCategorys = new TraderResourceRules(dbContext).GetListResourceCategory(CurrentDomainId(), ResourceCategoryType.Document);
            var resourceDocuemnts = new TraderResourceRules(dbContext).GetListResourceDocuments(CurrentDomainId());
            return PartialView("_TraderItemSelectDocumentResources", resourceDocuemnts);
        }

        [HttpGet]
        public ActionResult GetItemsSource(int locationId, int wgid)
        {
            ReturnJsonModel result = new ReturnJsonModel();
            result.result = true;
            var groups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), locationId);
            groups = groups.Where(q => q.WorkGroupCategories.Select(x => x.Id).Contains(wgid)).ToList();

            var lstTraderItems = new List<TraderItem>();

            foreach (var item in groups)
            {
                lstTraderItems.AddRange(item.Items.Select(q => new TraderItem()
                {
                    Id = q.Id,
                    Name = q.Name,
                    SKU = q.SKU,
                    InventoryDetails = (q.InventoryDetails.Where(x => x.Location.Id == locationId).ToList()),
                    ImageUri = q.ImageUri,
                }).OrderBy(n => n.Name).ToList());
            }
            result.Object = lstTraderItems.Select(x => new
            {
                x.Id,
                x.Name,
                x.SKU,
                x.ImageUri,
                LevelCount = x.InventoryDetails.Count > 0 ? x.InventoryDetails[0].CurrentInventoryLevel : 0
            }).OrderBy(n => n.Name).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeUnitName(int unitId, string unitNewName)
        {
            var result = new TraderItemRules(dbContext).ChangeUnitName(unitId, unitNewName, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Select2TraderItemsByLocationId(int page, string keyword, int locationId = 0)
        {
            var result = new TraderItemRules(dbContext).Select2TraderItemsByLocationId(CurrentDomainId(), keyword, (locationId == 0 ? CurrentLocationManage() : locationId));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCurrentInventory(int locationId = 0, int itemId = 0)
        {
            var inventory = new TraderInventoryRules(dbContext).GetInventoryDetail(itemId, locationId);
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
            if (inventory != null && inventory.Id > 0)
                return Json(new
                {
                    id = inventory.Id,
                    itemId,
                    currentInventory = inventory.CurrentInventoryLevel.ToInputNumberFormat(currencySettings),
                    minInventorylLevel = inventory.MinInventorylLevel,
                    maxInventoryLevel = inventory.MaxInventoryLevel,
                    averageCost = inventory.AverageCost.ToCurrencySymbol(currencySettings),
                    latestCost = inventory.LatestCost.ToCurrencySymbol(currencySettings)
                }, JsonRequestBehavior.AllowGet);
            else
                return Json(new
                {
                    id = 0,
                    itemId = 0,
                    currentInventory = ((decimal)0).ToInputNumberFormat(currencySettings),
                    minInventorylLevel = 0,
                    maxInventoryLevel = 0,
                    averageCost = ((decimal)0).ToCurrencySymbol(currencySettings),
                    latestCost = ((decimal)0).ToCurrencySymbol(currencySettings)
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTradeItemInventory([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string sku, int locationId, int groupId, bool isNoneInventory)
        {
            var currentDomainId = CurrentDomainId();
            return Json(new TraderItemRules(dbContext).GetTraderItemsFindSkuLocaion(requestModel, sku, locationId, groupId, currentDomainId, isNoneInventory), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnitsByItemId(int itemId)
        {
            var units = new TraderConversionUnitRules(dbContext).GetUnitsByItemId(itemId);
            return Json(units, JsonRequestBehavior.AllowGet);
        }
    }
}