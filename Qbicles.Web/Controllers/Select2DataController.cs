using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class Select2DataController : BaseController
    {
        public ActionResult GetTraderContact(string keySearch)
        {
            var lstContacts = new TraderContactRules(dbContext).GetTraderContactsDataForSelect2(CurrentDomainId(), keySearch);
            return Json(lstContacts, JsonRequestBehavior.AllowGet);
        }

        //Purchases only show (isBought = true) Items
        //Sales only show (isSold = true) Items
        public ActionResult GetTraderItemsByWorkgroup(int workGroupId = 0, int locationId = 0, string keySearch = "", bool isSold = true, bool isBought = true)
        {
            var result = new ReturnJsonModel();
            result.result = true;
            try
            {
                ViewBag.locationId = locationId;
                var traderItems = dbContext.TraderItems.
                    Where(q => q.Locations.Any(z => z.Id == locationId)
                    && ((isSold && q.IsSold) || (isBought && q.IsBought))
                    && q.Group.WorkGroupCategories.Select(w => w.Id).Contains(workGroupId)
                    && q.Name.ToLower().Contains(keySearch.ToLower())
                    ).OrderBy(n => n.Name).Distinct().Take(10).ToList().Select(i => new TraderItemModel
                    {
                        Id = i.Id,
                        Name = i.Name,
                        ImageUri = i.ImageUri,
                        CostUnit = 0,
                        TaxRateName = i.StringItemTaxRates(isSold),
                        TaxRateValue = i.SumTaxRatesPercent(isSold)
                    }).ToList();
                result.Object = traderItems;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                result.Object = null;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSpotCountWasteItemsByWorkgroup(int workGroupId = 0, int locationId = 0, string keySearch = "", string itemIds = "")
        {
            var result = new ReturnJsonModel();
            result.result = true;
            try
            {
                var iIds = new List<int>();
                if (!string.IsNullOrEmpty(itemIds))
                    iIds = itemIds.Split(',').Select(int.Parse).ToList();

                var domainId = CurrentDomainId();

                ViewBag.locationId = locationId;
                var traderItems = dbContext.TraderItems.
                    Where(q => q.Domain.Id == domainId && q.Locations.Any(z => z.Id == locationId) && !iIds.Contains(q.Id)
                    && q.Group.WorkGroupCategories.Select(w => w.Id).Contains(workGroupId)
                    && q.Name.ToLower().Contains(keySearch.ToLower())
                    && (q.IsSold || q.IsBought)
                    ).OrderBy(n => n.Name).Distinct().Take(10).ToList().Select(i => new
                    {
                        id = i.InventoryDetails.Count == 0
                            ? $"{i.Id}:{i.ImageUri}:{i.Name}:{i.SKU}:0"
                            : $"{i.Id}:{i.ImageUri}:{i.Name}:{i.SKU}:{i.InventoryDetails.FirstOrDefault(l => l.Location.Id == locationId)?.Id ?? 0}",
                        text = i.Name
                    }).ToList();
                result.Object = traderItems;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                result.Object = null;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSpotCountItemsById(int spotCountId, int workGroupId = 0, int locationId = 0, string keySearch = "")
        {
            var result = new ReturnJsonModel();
            result.result = true;
            try
            {
                var domainId = CurrentDomainId();
                var itemIds = new TraderSpotCountRules(dbContext).GetById(spotCountId)?.ProductList.Select(e => e.Product).Select(i => i.Id).ToList() ?? new List<int>();

                ViewBag.locationId = locationId;
                var traderItems = dbContext.TraderItems.
                    Where(q => q.Domain.Id == domainId && q.Locations.Any(z => z.Id == locationId) && !itemIds.Contains(q.Id)
                    && q.Group.WorkGroupCategories.Select(w => w.Id).Contains(workGroupId)
                    && q.Name.ToLower().Contains(keySearch.ToLower())
                    && (q.IsSold || q.IsBought)
                    ).OrderBy(n => n.Name).Distinct().Take(10).ToList().Select(i => new
                    {
                        id = i.InventoryDetails.Count == 0
                            ? $"{i.Id}:{i.ImageUri}:{i.Name}:{i.SKU}:0"
                            : $"{i.Id}:{i.ImageUri}:{i.Name}:{i.SKU}:{i.InventoryDetails.FirstOrDefault(l => l.Location.Id == locationId)?.Id ?? 0}",
                        text = i.Name
                    }).ToList();
                result.Object = traderItems;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                result.Object = null;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetWasteReportItemsById(int wasteReportId, int workGroupId = 0, int locationId = 0, string keySearch = "")
        {
            var result = new ReturnJsonModel();
            result.result = true;
            try
            {
                var domainId = CurrentDomainId();
                var itemIds = new TraderWasteReportRules(dbContext).GetById(wasteReportId)?.ProductList.Select(e => e.Product).Select(i => i.Id).ToList()?? new List<int>();

                ViewBag.locationId = locationId;
                var traderItems = dbContext.TraderItems.
                    Where(q => q.Domain.Id == domainId && q.Locations.Any(z => z.Id == locationId) && !itemIds.Contains(q.Id)
                    && q.Group.WorkGroupCategories.Select(w => w.Id).Contains(workGroupId)
                    && q.Name.ToLower().Contains(keySearch.ToLower())
                    && (q.IsSold || q.IsBought)
                    ).OrderBy(n => n.Name).Distinct().Take(10).ToList().Select(i => new
                    {
                        id = i.InventoryDetails.Count == 0
                            ? $"{i.Id}:{i.ImageUri}:{i.Name}:{i.SKU}:0"
                            : $"{i.Id}:{i.ImageUri}:{i.Name}:{i.SKU}:{i.InventoryDetails.FirstOrDefault(l => l.Location.Id == locationId)?.Id ?? 0}",
                        text = i.Name
                    }).ToList();
                result.Object = traderItems;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                result.Object = null;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetUnusedInventoryQuantity(int inventoryId)
        {
            var currentInventory = dbContext.UnusedInventoriesView.FirstOrDefault(e => e.Id == inventoryId)?.CurrentInventory ?? 0;

            var inventoryDetail = dbContext.InventoryDetails.FirstOrDefault(e => e.Id == inventoryId);
            if (inventoryDetail != null)
            {
                inventoryDetail.CurrentInventoryLevel = currentInventory;
                dbContext.SaveChanges();
            }
            return Json(currentInventory, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStockAuditItemsByWorkgroup(int workGroupId = 0, int locationId = 0, string keySearch = "")
        {
            var result = new ReturnJsonModel();
            result.result = true;
            try
            {
                var domainId = CurrentDomainId();
                var traderItems = (from item in dbContext.TraderItems
                                   where item.Group.Domain.Id == domainId
                                   && item.Group.WorkGroupCategories.Any(w => w.Id == workGroupId)
                                   && item.Locations.Any(l => (l.Id == locationId) || item.IsActiveInAllLocations)
                                   && (string.IsNullOrEmpty(keySearch) || item.Name.ToLower().Contains(keySearch.ToLower()))
                                   && (item.IsSold || item.IsBought)
                                   select item).OrderBy(p => p.Name).Distinct().Take(10).Select(i => new
                                   {
                                       id = i.Id + " | " + i.Name + " | " + i.SKU,
                                       text = i.Name
                                   });
                result.Object = traderItems;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                result.Object = null;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGroupedTraderItemsByWG(int workGroupId = 0, int locationId = 0, string keySearch = "")
        {
            var result = new ReturnJsonModel();
            result.result = true;
            try
            {
                var domainId = CurrentDomainId();
                var groups = dbContext.TraderGroups.Where(d => d.Domain.Id == domainId);

                var productGroups = groups.Where(x => x.Items.Any(i => i.IsCompoundProduct == true
                    && (i.Locations.Select(l => l.Id).Contains(locationId) || i.IsActiveInAllLocations)
                    && i.Name.ToLower().Contains(keySearch.ToLower())
                    && (i.IsSold || i.IsBought))
                    && x.WorkGroupCategories.Select(w => w.Id).Contains(workGroupId))
                    .Distinct().OrderBy(x => x.Name).Take(10).ToList().Select(c => new TraderGroup()
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Items = c.Items.Where(item =>
                                                item.IsCompoundProduct == true
                                                && (item.Locations.Select(lc => lc.Id).Contains(locationId) || item.IsActiveInAllLocations)
                                                && item.Name.ToLower().Contains(keySearch.ToLower())
                                                && (item.IsSold || item.IsBought)).ToList()
                    });

                var resultList = new List<Select2GroupedCustomModel>();
                productGroups.ToList().ForEach(pgItem =>
                {
                    var item = new Select2GroupedCustomModel()
                    {
                        text = pgItem.Name,
                        children = pgItem.Items.Select(itemSl2 => new Select2CustomModel()
                        {
                            id = itemSl2.Id,
                            text = itemSl2.Name
                        }).ToList()
                    };
                    resultList.Add(item);
                });

                result.Object = resultList;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                result.Object = null;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPoint2PointTransferItemsByWorkgroup(int workGroupId = 0, int locationId = 0, string keySearch = "")
        {
            var result = new ReturnJsonModel();
            result.result = true;
            try
            {
                var currentLocationId = CurrentLocationManage();
                var domainId = CurrentDomainId();
                var traderItems = dbContext.TraderItems.Where(q => (q.Locations.Any(z => z.Id == currentLocationId || z.Id == locationId))
                                                            && q.Group != null && q.Group.WorkGroupCategories.Select(w => w.Id).Contains(workGroupId)
                                                            && (q.Name.ToLower().Contains(keySearch.ToLower()))
                                                            && (q.InventoryDetails.Any(d => d.Location != null
                                                                && (d.Location.Id == currentLocationId || d.Location.Id == locationId))))
                                                        .OrderBy(n => n.Name).Take(10).ToList().Select(i => new TraderItemModel
                                                        {
                                                            Id = i.Id,
                                                            Name = i.Name,
                                                            ImageUri = i.ImageUri,
                                                            CostUnit = 0,
                                                            TaxRateName = i.StringItemTaxRates(false),
                                                            TaxRateValue = i.SumTaxRatesPercent(false),
                                                            WgIds = i.Group.WorkGroupCategories.Select(g => g.Id).ToList()
                                                        }).ToList();
                result.Object = traderItems;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                result.Object = null;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}