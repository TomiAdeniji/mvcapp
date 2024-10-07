using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.Reorder;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static Qbicles.BusinessRules.Model.TB_Column;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderInventoryRules
    {
        private ApplicationDbContext dbContext;

        public TraderInventoryRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public InventoryDetail GetInventoryDetail(int itemId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId, locationId);
                return dbContext.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId && q.Item.Id == itemId);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, itemId, locationId);
                return new InventoryDetail();
            }
        }

        public decimal GetAverageCost(int itemId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId, locationId);

                return GetInventoryDetail(itemId, locationId)?.AverageCost ?? 0;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, itemId, locationId);
                return 0M;
            }
        }

        /// <summary>
        /// This routine gets the AverageCost and Latest costs based on the supplied InventoryDetail reference
        /// </summary>
        /// <param name="inventoryDetail"></param>
        /// <param name="averageCost"></param>
        /// <param name="lastCost"></param>
        public void GetCostsFromInventory(InventoryDetail inventoryDetail, ref decimal averageCost, ref decimal lastCost)
        {
            decimal totalValue = 0;
            decimal totalQuantity = 0;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, inventoryDetail, averageCost, lastCost);
                if (inventoryDetail.InventoryBatches != null && inventoryDetail.InventoryBatches.Count > 0)
                {
                    inventoryDetail.InventoryBatches.ForEach(batch =>
                    {
                        if (batch.UnusedQuantity > 0 && batch.Direction == BatchDirection.In)
                        {
                            totalValue += batch.CurrentBatchValue;
                            totalQuantity += batch.UnusedQuantity;
                        }
                    });

                    averageCost = totalValue / (totalQuantity == 0 ? 1 : totalQuantity);

                    lastCost = inventoryDetail.InventoryBatches.OrderByDescending(d => d.CreatedDate).FirstOrDefault()?.CostPerUnit ?? 0;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, inventoryDetail, averageCost, lastCost);
            }
        }

        /// <summary>
        /// calculate and update the iventoryDetail.AverageCost &  inventoryDetail.LatestCost
        /// </summary>
        /// <param name="inventoryDetail"></param>
        public void UpdateCosts(InventoryDetail inventoryDetail)
        {
            //initialise the costs
            decimal averageCost = 0;
            decimal lastCost = 0;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, inventoryDetail);
                //get the costs
                if (inventoryDetail != null)
                    GetCostsFromInventory(inventoryDetail, ref averageCost, ref lastCost);

                //set the costs
                inventoryDetail.AverageCost = averageCost;
                inventoryDetail.LatestCost = lastCost;

                //update the database
                dbContext.SaveChanges();

                inventoryDetail.UpdatingCatalogPricingBasedOnLatestCostChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, inventoryDetail);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="locationId"></param>
        /// <param name="userId"></param>
        /// <param name="keySearch"></param>
        /// <param name="inventoryBasis"></param>
        /// <param name="maxDayToLast"></param>
        /// <param name="days2Last"></param>
        /// <param name="dayToLastOperator"></param>
        /// <returns></returns>
        public DataTablesResponse GetInventoryServerSide(DataTablesRequest requestModel, int locationId,
            UserSetting user, List<ItemUnitChangeModel> lstUnitsChanged,
            string keySearch = "", string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1, bool hasSymbol = true)
        {
            // hasSymbol : Show the symbol on each line in Inventory tab
            var dateFormat = string.IsNullOrEmpty(user.DateFormat) ? "dd/MM/yyyy" : user.DateFormat;
            keySearch = string.IsNullOrEmpty(keySearch) ? requestModel.Search.Value.ToLower() : keySearch.ToLower();
            var days2LastFrom = DateTime.UtcNow;
            var days2LastTo = DateTime.UtcNow;
            switch (dayToLastOperator)
            {
                case 1:
                    days2LastFrom = days2LastTo.AddDays(-7);
                    break;

                case 2:
                    days2LastTo = days2LastFrom.AddMonths(1);
                    break;

                case 3:
                    if (!string.IsNullOrEmpty(days2Last.Trim()))
                    {
                        if (!days2Last.Contains('-'))
                        {
                            days2Last += "-";
                        }

                        days2Last.ConvertDaterangeFormat(dateFormat, user.Timezone, out days2LastFrom, out days2LastTo);
                        days2LastFrom.AddTicks(1);
                        days2LastTo.AddDays(1).AddTicks(-1);
                    }
                    break;
            }

            var numberOfDay = (decimal)(days2LastTo - days2LastFrom).TotalDays;//Number of days in date range

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, requestModel, locationId, user,
                        user.Timezone, keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);

                var itemIds = new List<int>();
                var unitIds = new List<int>();

                if (lstUnitsChanged != null && lstUnitsChanged.Any())
                {
                    if (string.IsNullOrEmpty(keySearch))
                    {
                        itemIds = lstUnitsChanged.Select(u => u.ItemId).ToList();
                        unitIds = lstUnitsChanged.Select(u => u.UnitBaseId).ToList();
                    }
                    else
                    {
                        itemIds = lstUnitsChanged.Where(n => n.UnitName.ToLower().Contains(keySearch))
                            .Select(u => u.ItemId).ToList();
                        unitIds = lstUnitsChanged.Where(n => n.UnitName.ToLower().Contains(keySearch))
                            .Select(u => u.UnitBaseId).ToList();
                    }
                }

                IQueryable<ProductUnit> productUnits;
                if (lstUnitsChanged == null || !lstUnitsChanged.Any())
                {
                    productUnits = dbContext.ProductUnits.Where(unit => unit.IsBase && unit.Item.Locations.Any(l => l.Id == locationId));
                }
                else
                {
                    //exclude units has changed - get with is base
                    productUnits = dbContext.ProductUnits.Where(unit => unit.IsBase && unit.Item.Locations.Any(l => l.Id == locationId) && !itemIds.Contains(unit.Item.Id));
                    // get unit has change from stored id
                    var unitItemsChanged = dbContext.ProductUnits.Where(unit => unitIds.Contains(unit.Id));

                    productUnits = productUnits.Concat(unitItemsChanged);
                }

                if (!string.IsNullOrEmpty(keySearch))
                    productUnits = productUnits.Where(unit => unit.Item.Name.ToLower().Contains(keySearch) || unit.Name.ToLower().Contains(keySearch));
                //Fix bug issue https://atomsinteractive.atlassian.net/browse/QBIC-2415
                productUnits = productUnits.Where(unit => unit.Item.InventoryDetails.Any(e => e.Location.Id == locationId));
                //end fix
                var totalRecords = productUnits.Count();

                if (requestModel.Columns != null)
                {
                    var sortedOrderColumns = requestModel.Columns.GetSortedColumns();
                    productUnits = productUnits.SortOrderItemUnitDetails(sortedOrderColumns);
                }
                else
                    productUnits = productUnits.OrderBy("Item".GenerateSortItemExp<string>());

                var tradItemsResultList = productUnits.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                var inventories = new List<InventoryModel>();

                string unitName;
                decimal quantityOfBaseUnit;
                decimal quantityOut;
                var nOfDay = numberOfDay == 0 ? 1 : numberOfDay;

                var domainId = dbContext.TraderLocations.AsNoTracking().Where(e => e.Id == locationId).FirstOrDefault().Domain.Id;

                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                tradItemsResultList.ForEach(unit =>
                {
                    unitName = unit.Name;

                    quantityOfBaseUnit = unit.QuantityOfBaseunit;

                    var inventoryDetail = unit.Item.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId);
                    if (inventoryDetail == null)
                        return;
                    int associatedCount = 0;
                    var isCompoundProduct = true;
                    if (inventoryDetail.CurrentRecipe != null && inventoryDetail.CurrentRecipe.Ingredients.Any() && unit.Item.IsCompoundProduct)
                    {
                        associatedCount = inventoryDetail.CurrentRecipe.Ingredients.Count;
                    }
                    else
                    {
                        isCompoundProduct = false;
                        associatedCount = dbContext.InventoryDetails
                        .Where(e => e.Location.Id == locationId && e.CurrentRecipe.Ingredients.Any(i => i.SubItem.Id == unit.Item.Id))
                        .Count();
                    }

                    quantityOut = inventoryDetail.InventoryBatches.Where(e => e.Direction == BatchDirection.Out
                                                                        && e.CreatedDate <= days2LastTo
                                                                        && e.CreatedDate >= days2LastFrom).Sum(u => u.UnusedQuantity) / nOfDay;
                    if (quantityOut == 0) quantityOut = 1;
                    var dayToLast = (inventoryDetail.CurrentInventoryLevel / quantityOut);
                    var model = new InventoryModel
                    {
                        Id = unit.Item.Id,
                        UnitId = unit.Id,
                        Icon = unit.Item.ImageUri.ToUriString(),
                        Item = unit.Item.Name,
                        Barcode = unit.Item.Barcode,
                        SKU = unit.Item.SKU,

                        isBought = unit.Item.IsBought,
                        DaysToLast = dayToLast.ToString("N0"), //13

                        MinInventory = $"{(inventoryDetail.MinInventorylLevel / quantityOfBaseUnit):F2} {unitName}",//14
                        MaxInventory = $"{(inventoryDetail.MaxInventoryLevel / quantityOfBaseUnit):F2} {unitName}",//15

                        DaysToLastHighlighted = dayToLast < maxDayToLast,
                        Description = unit.Item.Description,
                        Unit = unitName,
                        EditType = unit.Item.IsBought ? 1 : 2,
                        Associated = $"{associatedCount} item(s)",//17,
                        IsCompoundProduct = isCompoundProduct,
                        ListUnitHtmlString = ""
                    };
                    //remove symbol
                    if (!hasSymbol)
                    {
                        model.AverageCost = CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.AverageCost * quantityOfBaseUnit, currencySetting);
                        model.LatestCost = CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.LatestCost * quantityOfBaseUnit, currencySetting);
                        model.CurrentInventory = CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.CurrentInventoryLevel / quantityOfBaseUnit, currencySetting);
                        model.InventoryTotal = inventoryBasis == "average"
                            ? CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.CurrentInventoryLevel * inventoryDetail.AverageCost, currencySetting)
                            : CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.CurrentInventoryLevel * inventoryDetail.LatestCost, currencySetting);
                    }
                    else
                    {
                        model.AverageCost = (inventoryDetail.AverageCost * quantityOfBaseUnit).ToCurrencySymbol(currencySetting);
                        model.LatestCost = (inventoryDetail.LatestCost * quantityOfBaseUnit).ToCurrencySymbol(currencySetting);
                        model.CurrentInventory = (inventoryDetail.CurrentInventoryLevel / quantityOfBaseUnit).ToCurrencySymbol(currencySetting);
                        model.InventoryTotal = inventoryBasis == "average"
                            ? (inventoryDetail.CurrentInventoryLevel * inventoryDetail.AverageCost).ToCurrencySymbol(currencySetting)
                            : (inventoryDetail.CurrentInventoryLevel * inventoryDetail.LatestCost).ToCurrencySymbol(currencySetting);
                    }

                    model.ListUnitHtmlString += "<select name=\"unit\" id='" + unit.Item.Id + "-unit' onchange='UpdateInventoryUnit(" + unit.Item.Id + ")' class=\"form-control select2 select2-hidden-accessible\" style=\"width: 100%;\" tabindex=\"-1\" aria-hidden=\"true\">";
                    foreach (var unitItem in unit.Item.Units)
                    {
                        var isSelected = unitItem.Id == unit.Id ? "selected" : "";
                        model.ListUnitHtmlString += "<option value=' " + unitItem.Id + " '" + isSelected + ">" + unitItem.Name + "</option>";
                    }
                    model.ListUnitHtmlString += "</select>";

                    model.Units = unit.Item.Units.Select(e => new Select2Model { Id = e.Id, Text = e.Name, Selected = e.Name == unitName }).ToList();

                    inventories.Add(model);
                });

                return new DataTablesResponse(requestModel.Draw, inventories, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, requestModel, locationId, user, keySearch, inventoryBasis,
                    inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);
            }

            return new DataTablesResponse(requestModel.Draw, new List<InventoryModel>(), 0, 0);
        }

        public ItemsAssociated ShowIngredientsItemAssociated(int itemId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId, locationId);

                var itemsAssociated = new ItemsAssociated();

                var item = dbContext.TraderItems.Find(itemId);
                if (item == null)
                    return itemsAssociated;

                var inventoryDetail = item.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId);
                if (inventoryDetail == null)
                    return itemsAssociated;

                itemsAssociated.ItemName = item.Name;

                if (inventoryDetail.CurrentRecipe != null && inventoryDetail.CurrentRecipe.Ingredients.Any() && item.IsCompoundProduct)
                {
                    itemsAssociated.Items = inventoryDetail.CurrentRecipe.Ingredients;
                    itemsAssociated.IsCompoundProduct = true;

                    return itemsAssociated;
                }

                var inventoryDetails = dbContext.InventoryDetails.Where(e => e.Location.Id == locationId && e.CurrentRecipe.Ingredients.Any(i => i.SubItem.Id == itemId)).ToList();

                var compound = inventoryDetails.Select(i => new Ingredient
                {
                    SubItem = i.Item,
                    ParentRecipe = i.Item.AssociatedRecipes.FirstOrDefault(),
                    Quantity = i.Item.AssociatedRecipes.FirstOrDefault()?.Ingredients?.FirstOrDefault(ing => ing.SubItem.Id == itemId)?.Quantity ?? 0,
                    Unit = i.Item.AssociatedRecipes.FirstOrDefault()?.Ingredients?.FirstOrDefault(si => si.SubItem.Id == itemId)?.Unit ?? null,
                });
                itemsAssociated.Items = compound.ToList();
                itemsAssociated.IsCompoundProduct = false;
                return itemsAssociated;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, itemId, locationId);
                return new ItemsAssociated();
            }
        }

        public List<ProductUnit> ShowChangeItemUnits(int itemId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId);
                return dbContext.TraderItems.FirstOrDefault(e => e.Id == itemId)?.Units;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<ProductUnit>();
            }
        }

        public ReturnJsonModel UpdateChangeItemUnit(string userId, int unitId, int itemId, int locationId, List<ItemUnitChangeModel> unitsChange,
        string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1, bool hasSymbol = true)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, userId, unitId, itemId, locationId,
                        inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);

                #region Update

                var item = dbContext.TraderItems.Find(itemId);
                if (item == null)
                    return refModel;

                var days2LastFrom = DateTime.UtcNow;
                var days2LastTo = DateTime.UtcNow;
                switch (dayToLastOperator)
                {
                    case 1:
                        days2LastFrom = days2LastTo.AddDays(-7);
                        break;

                    case 2:
                        days2LastTo = days2LastFrom.AddMonths(1);
                        break;

                    case 3:
                        if (!string.IsNullOrEmpty(days2Last.Trim()))
                        {
                            if (!days2Last.Contains('-'))
                            {
                                days2Last += "-";
                            }

                            days2Last.ConvertDaterangeFormat("dd/MM/yyyy", "", out days2LastFrom, out days2LastTo);
                            days2LastFrom.AddTicks(1);
                            days2LastTo.AddDays(1).AddTicks(-1);
                        }
                        break;
                }

                var numberOfDay = (decimal)(days2LastTo - days2LastFrom).TotalDays;//Number of days in date range
                var nOfDay = numberOfDay == 0 ? 1 : numberOfDay;
                var inventoryDetail = item.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId);
                if (inventoryDetail == null)
                    return refModel;
                var unitBase = item.Units.FirstOrDefault(e => e.Id == unitId);
                if (unitBase == null)
                    return refModel;

                var quantityOfBaseUnit = unitBase.QuantityOfBaseunit;
                var quantityOut = inventoryDetail.InventoryBatches.Where(e => e.Direction == BatchDirection.Out && e.CreatedDate <= days2LastTo && e.CreatedDate >= days2LastFrom)
                               .Sum(u => u.UnusedQuantity) / nOfDay;
                if (quantityOut == 0)
                    quantityOut = 1;

                var domainId = dbContext.TraderLocations.AsNoTracking().Where(e => e.Id == locationId).FirstOrDefault().Domain.Id;

                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                var model = new InventoryModel
                {
                    Id = inventoryDetail.Item.Id,

                    DaysToLast = (inventoryDetail.CurrentInventoryLevel / quantityOut).ToString("N0"), //13 ????
                    MinInventory = $"{(inventoryDetail.MinInventorylLevel / quantityOfBaseUnit):N0} {unitBase.Name}",//14
                    MaxInventory = $"{(inventoryDetail.MaxInventoryLevel / quantityOfBaseUnit):N0} {unitBase.Name}",//15
                    DaysToLastHighlighted = (inventoryDetail.InventoryBatches.Where(e => e.Direction == BatchDirection.Out && e.CreatedDate <= days2LastTo && e.CreatedDate >= days2LastFrom).Sum(u => u.UnusedQuantity) / nOfDay) < maxDayToLast,
                    Unit = unitBase.Name,
                    UnitId = unitBase.Id,
                    Units = unitBase.Item.Units.Select(e => new Select2Model { Id = e.Id, Text = e.Name, Selected = e.Name == unitBase.Name }).ToList()
                };

                if (!hasSymbol)
                {
                    model.AverageCost = CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.AverageCost * quantityOfBaseUnit, currencySetting);
                    model.LatestCost = CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.LatestCost * quantityOfBaseUnit, currencySetting);
                    model.CurrentInventory = CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.CurrentInventoryLevel / quantityOfBaseUnit, currencySetting);
                    model.InventoryTotal = inventoryBasis == "average" ? CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.CurrentInventoryLevel * inventoryDetail.AverageCost, currencySetting)
                    : CurrencyUserConfiguration.ToCurrencyWithoutSymbol(inventoryDetail.CurrentInventoryLevel * inventoryDetail.LatestCost, currencySetting);
                }
                else
                {
                    model.AverageCost = (inventoryDetail.AverageCost * quantityOfBaseUnit).ToCurrencySymbol(currencySetting);
                    model.LatestCost = (inventoryDetail.LatestCost * quantityOfBaseUnit).ToCurrencySymbol(currencySetting);
                    model.CurrentInventory = (inventoryDetail.CurrentInventoryLevel / quantityOfBaseUnit).ToCurrencySymbol(currencySetting);
                    model.InventoryTotal = inventoryBasis == "average" ? (inventoryDetail.CurrentInventoryLevel * inventoryDetail.AverageCost).ToCurrencySymbol(currencySetting)
                    : (inventoryDetail.CurrentInventoryLevel * inventoryDetail.LatestCost).ToCurrencySymbol(currencySetting);
                }
                // verify if the item has changed unit then remove the item and add with new base Unit again
                if (unitsChange == null) unitsChange = new List<ItemUnitChangeModel>();

                var itemChangeAgain = unitsChange.FirstOrDefault(i => i.ItemId == itemId);
                if (itemChangeAgain != null)
                    unitsChange.Remove(itemChangeAgain);

                unitsChange.Add(new ItemUnitChangeModel
                {
                    ItemId = itemId,
                    UnitBaseId = unitId,
                    UnitName = unitBase.Name
                });

                refModel.Object = model;
                refModel.Object2 = unitsChange;
                refModel.result = true;

                #endregion Update
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, userId, unitId, itemId, locationId, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public async Task InitialInventory(ApplicationUser user, TraderItem traderItem, TraderLocation location, decimal quantity, decimal unitCost)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, user, traderItem, location, quantity, unitCost);

                var transfer = new TraderTransfer
                {
                    DestinationLocation = location,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Status = TransferStatus.Delivered,
                    Reason = TransferReasonEnum.InventoryCreation,
                    TransferItems = new List<TraderTransferItem>()
                };

                var itemBaseUnit = traderItem.Units.FirstOrDefault(s => s.IsBase && s.IsActive) ?? traderItem.Units.FirstOrDefault(s => s.IsActive);
                var inventoryItem = new TraderTransferItem
                {
                    TraderItem = traderItem,
                    QuantityAtDelivery = quantity,
                    QuantityAtPickup = 0,
                    Unit = itemBaseUnit
                };

                transfer.TransferItems.Add(inventoryItem);

                var transferLog = new TransferLog
                {
                    Address = transfer.Address,
                    AssociatedTransfer = transfer,
                    Contact = transfer.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Purchase = transfer.Purchase,
                    Sale = transfer.Sale,
                    TransferApprovalProcess = null,
                    Status = transfer.Status,
                    UpdatedBy = user,
                    AssociatedShipment = transfer.AssociatedShipment,
                    DestinationLocation = transfer.DestinationLocation,
                    OriginatingLocation = transfer.OriginatingLocation,
                    TransferItems = transfer.TransferItems,
                    Workgroup = transfer.Workgroup
                };

                var transferProcessLog = new TransferProcessLog
                {
                    AssociatedTransfer = transfer,
                    AssociatedTransferLog = transferLog,
                    TransferStatus = transfer.Status,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = null
                };

                dbContext.TraderTransferProcessLogs.Add(transferProcessLog);
                dbContext.Entry(transferProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();
                //Incoming Inventory
                await new TraderTransfersRules(dbContext).IncomingInventory(transfer, user, null, unitCost);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, user.Id, user, traderItem, location, quantity, unitCost);
            }
        }

        public ReturnJsonModel TriggeringReorderProcess(int domainId, int locationId, UserSetting user,
            string keySearch = "", string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, locationId, user,
                             keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);
                    var dateFormat = string.IsNullOrEmpty(user.DateFormat) ? "dd/MM/yyyy" : user.DateFormat;
                    keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();
                    var days2LastFrom = DateTime.UtcNow;
                    var days2LastTo = DateTime.UtcNow;
                    switch (dayToLastOperator)
                    {
                        case 1:
                            days2LastFrom = days2LastTo.AddDays(-7);
                            break;

                        case 2:
                            days2LastTo = days2LastFrom.AddMonths(1);
                            break;

                        case 3:
                            if (!string.IsNullOrEmpty(days2Last.Trim()))
                            {
                                if (!days2Last.Contains('-'))
                                {
                                    days2Last += "-";
                                }

                                days2Last.ConvertDaterangeFormat(dateFormat, user.Timezone, out days2LastFrom, out days2LastTo);
                                days2LastFrom.AddTicks(1);
                                days2LastTo.AddDays(1).AddTicks(-1);
                            }
                            break;
                    }
                    decimal quantityOut;
                    var numberOfDay = (decimal)(days2LastTo - days2LastFrom).TotalDays;//Number of days in date range
                    var nOfDay = numberOfDay == 0 ? 1 : numberOfDay;
                    var inventoryDetails = dbContext.InventoryDetails.Where(e => e.Location.Id == locationId && e.Item.IsBought && e.Item.Name.ToLower().Contains(keySearch)).ToList();
                    if (inventoryDetails != null)
                    {
                        #region Create Reorder

                        Reorder reorder = new Reorder
                        {
                            Reference = new TraderReferenceRules(dbContext).GetNewReference(domainId, TraderReferenceType.Reorder),
                            Location = dbContext.TraderLocations.Find(locationId),
                            Status = Reorder.StatusEnum.InComlete,
                            DateComplete = DateTime.UtcNow,
                            CreatedBy = dbContext.QbicleUser.Find(user.Id),
                            CreatedDate = DateTime.UtcNow,
                            Total = 0
                        };

                        #endregion Create Reorder

                        #region Create Reorder Item Group

                        ReorderItemGroup reorderItemGroup = new ReorderItemGroup();
                        reorderItemGroup.Reorder = reorder;
                        reorderItemGroup.DaysToLast = maxDayToLast;
                        reorderItemGroup.PrimaryContact = null;
                        reorder.ReorderItemGroups.Add(reorderItemGroup);

                        #endregion Create Reorder Item Group

                        foreach (var item in inventoryDetails)
                        {
                            quantityOut = item.InventoryBatches.Where(e => e.Direction == BatchDirection.Out
                                                                            && e.CreatedDate <= days2LastTo
                                                                            && e.CreatedDate >= days2LastFrom).Sum(u => u.UnusedQuantity) / nOfDay;
                            if (quantityOut == 0) quantityOut = 1;
                            var dayToLast = (item.CurrentInventoryLevel / quantityOut);
                            if (dayToLast < maxDayToLast)
                            {
                                ReorderItem reorderItem = new ReorderItem();
                                reorderItem.Item = item.Item;
                                reorderItem.IsDisabled = false;
                                reorderItem.Unit = item.Item.Units.FirstOrDefault(s => s.IsBase);
                                reorderItem.CostPerUnit = item.AverageCost;
                                reorderItem.Total = 0;
                                reorderItem.PurchaseItem = null;
                                //Canculate On Order
                                decimal quantityItemsPurchaseAssociated = 0;
                                decimal quantitytranferItem = 0;
                                QuantityItemsPurchaseByItem(locationId, reorderItem.Item.Id, out quantityItemsPurchaseAssociated, out quantitytranferItem);
                                reorderItem.OnOrder = quantityItemsPurchaseAssociated - quantitytranferItem;
                                //end calculate
                                var primarycontact = item.Item.VendorsPerLocation.FirstOrDefault(s => s.IsPrimaryVendor);
                                if (primarycontact == null)//Add group Unallocated
                                {
                                    reorderItem.ReorderItemGroup = reorderItemGroup;
                                    reorderItem.PrimaryContact = null;
                                    reorderItem.IsForReorder = false;
                                }
                                else//Add Primary contact group
                                {
                                    var primaryContactGroup = reorder.ReorderItemGroups.FirstOrDefault(s => s.PrimaryContact != null && s.PrimaryContact.Id == primarycontact.Vendor.Id);
                                    if (primaryContactGroup == null)
                                    {
                                        primaryContactGroup = new ReorderItemGroup();
                                        primaryContactGroup.Reorder = reorder;
                                        primaryContactGroup.DaysToLast = maxDayToLast;
                                        primaryContactGroup.PrimaryContact = primarycontact.Vendor;
                                        reorder.ReorderItemGroups.Add(primaryContactGroup);
                                    }
                                    reorderItem.ReorderItemGroup = primaryContactGroup;
                                    reorderItem.PrimaryContact = primarycontact.Vendor;
                                    reorderItem.IsForReorder = true;
                                }
                                dbContext.Entry(reorderItem).State = EntityState.Added;
                                dbContext.ReorderItems.Add(reorderItem);
                            }
                        }
                        //Calculate all
                        foreach (var group in reorder.ReorderItemGroups)
                        {
                            group.Total = group.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);
                        }
                        dbContext.Entry(reorder).State = EntityState.Added;
                        dbContext.Reorders.Add(reorder);
                        returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                        returnJson.Object = new { id = reorder.Id };
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, user, keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }

        public DataTablesResponse SearchReorders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int locationId, string keyword, int status, string daterange, string timezone, string dateformat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, keyword, status, daterange, timezone, dateformat);
                var query = dbContext.Reorders.Where(s => s.Location.Id == locationId);
                int totalRecords = 0;

                #region Filter

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Reference.FullRef.Contains(keyword));
                }
                if (status != 2)
                {
                    query = query.Where(s => (int)s.Status == status);
                }
                if (!string.IsNullOrEmpty(daterange.Trim()))
                {
                    var fromDate = DateTime.UtcNow;
                    var toDate = DateTime.UtcNow;
                    if (daterange.Contains('-'))
                    {
                        daterange.ConvertDaterangeFormat(dateformat, timezone, out fromDate, out toDate, HelperClass.endDateAddedType.day);
                    }
                    query = query.Where(s => s.DateComplete >= fromDate && s.DateComplete < toDate);
                }
                totalRecords = query.Count();

                #endregion Filter

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Reference":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DateComplete" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Total":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Total" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Reference = q.Reference?.FullRef ?? q.Id.ToString(),
                    Date = q.DateComplete.ConvertTimeFromUtc(timezone).ToString(dateformat),
                    Items = dbContext.ReorderItems.Count(s => s.ReorderItemGroup.Reorder.Id == q.Id && s.IsForReorder),
                    Total = q.Total,
                    Status = q.Status
                }).ToList();

                #endregion Paging

                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword, status, daterange, timezone, dateformat);
            }
            return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
        }

        public int CountReorderItems(int locationId, UserSetting user,
            string keySearch = "", string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, locationId, user,
                        keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);
                var dateFormat = string.IsNullOrEmpty(user.DateFormat) ? "dd/MM/yyyy" : user.DateFormat;
                keySearch = string.IsNullOrEmpty(keySearch) ? "" : keySearch.ToLower();
                var days2LastFrom = DateTime.UtcNow;
                var days2LastTo = DateTime.UtcNow;
                switch (dayToLastOperator)
                {
                    case 1:
                        days2LastFrom = days2LastTo.AddDays(-7);
                        break;

                    case 2:
                        days2LastTo = days2LastFrom.AddMonths(1);
                        break;

                    case 3:
                        if (!string.IsNullOrEmpty(days2Last.Trim()))
                        {
                            if (!days2Last.Contains('-'))
                            {
                                days2Last += "-";
                            }

                            days2Last.ConvertDaterangeFormat(dateFormat, user.Timezone, out days2LastFrom, out days2LastTo);
                            days2LastFrom.AddTicks(1);
                            days2LastTo.AddDays(1).AddTicks(-1);
                        }
                        break;
                }
                decimal quantityOut;
                var numberOfDay = (decimal)(days2LastTo - days2LastFrom).TotalDays;//Number of days in date range
                var nOfDay = numberOfDay == 0 ? 1 : numberOfDay;
                var inventoryDetails = dbContext.InventoryDetails.Where(e => e.Location.Id == locationId && e.Item.IsBought && e.Item.Name.ToLower().Contains(keySearch)).ToList();
                int countItems = 0;
                if (inventoryDetails != null)
                {
                    foreach (var item in inventoryDetails)
                    {
                        quantityOut = item.InventoryBatches.Where(e => e.Direction == BatchDirection.Out
                                                                        && e.CreatedDate <= days2LastTo
                                                                        && e.CreatedDate >= days2LastFrom).Sum(u => u.UnusedQuantity) / nOfDay;
                        if (quantityOut == 0) quantityOut = 1;
                        var dayToLast = (item.CurrentInventoryLevel / quantityOut);
                        if (dayToLast < maxDayToLast)
                        {
                            countItems += 1;
                        }
                    }
                }
                return countItems;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, user, keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);
                return 0;
            }
        }

        public Reorder GetReorderById(int id)
        {
            try
            {
                return dbContext.Reorders.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new Reorder();
            }
        }

        public ReorderItemGroup GetReorderGroupById(int id)
        {
            try
            {
                return dbContext.ReorderItemGroups.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new ReorderItemGroup();
            }
        }

        public ReturnJsonModel CreateGroupReorder(ReorderItemGroupCustomModel model, int locationId, string timezone, string dateformat)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, model);
                    var days2LastFrom = DateTime.UtcNow;
                    var days2LastTo = DateTime.UtcNow;
                    switch (model.DaysToLastBasis)
                    {
                        case 1:
                            days2LastFrom = days2LastTo.AddDays(-7);
                            break;

                        case 2:
                            days2LastTo = days2LastFrom.AddMonths(1);
                            break;

                        case 3:
                            if (!string.IsNullOrEmpty(model.Days2Last.Trim()))
                            {
                                if (!model.Days2Last.Contains('-'))
                                {
                                    model.Days2Last += "-";
                                }
                                model.Days2Last.ConvertDaterangeFormat(dateformat, timezone, out days2LastFrom, out days2LastTo);
                                days2LastFrom.AddTicks(1);
                                days2LastTo.AddDays(1).AddTicks(-1);
                            }
                            break;
                    }
                    var numberOfDay = (decimal)(days2LastTo - days2LastFrom).TotalDays;//Number of days in date range
                    var nOfDay = numberOfDay == 0 ? 1 : numberOfDay;
                    decimal movementRate = 0;
                    var reorder = dbContext.Reorders.Find(model.ReorderId);
                    if (reorder != null)
                    {
                        var group = reorder.ReorderItemGroups.FirstOrDefault(s => s.PrimaryContact != null && s.PrimaryContact.Id == model.PrimaryContactId);
                        if (group == null)
                        {
                            group = new ReorderItemGroup();
                            group.PrimaryContact = dbContext.TraderContacts.Find(model.PrimaryContactId);
                            group.DeliveryMethod = model.DeliveryMethod;
                            group.DaysToLastBasis = model.DaysToLastBasis;
                            group.DaysToLast = model.DaysToLast;
                            group.Days2Last = model.Days2Last;
                        }
                        var index = 1;
                        foreach (var item in model.Items)
                        {
                            if (item.Dimensions == null || item.Dimensions.Count == 0)
                            {
                                returnJson.msg = ResourcesManager._L("ERROR_MSG_54", index);
                                return returnJson;
                            }
                            index++;
                            var reorderitem = dbContext.ReorderItems.Find(item.Id);
                            if (reorderitem != null)
                            {
                                var inventoryDetail = reorderitem.Item.InventoryDetails.FirstOrDefault(s => s.Location.Id == locationId);
                                if (inventoryDetail != null)
                                {
                                    var unit = dbContext.ProductUnits.Find(item.UnitId);
                                    movementRate = inventoryDetail.InventoryBatches.Where(e => e.Direction == BatchDirection.Out
                                                                                    && e.CreatedDate <= days2LastTo
                                                                                    && e.CreatedDate >= days2LastFrom).Sum(u => u.OriginalQuantity) / nOfDay;
                                    var reorderQuantity = (movementRate * model.DaysToLast) - inventoryDetail.CurrentInventoryLevel;
                                    var displayQuantity = reorderQuantity * unit.QuantityOfBaseunit;
                                    reorderitem.PrimaryContact = dbContext.TraderContacts.Find(item.PrimaryContactId);
                                    reorderitem.Unit = unit;
                                    if (item.CostPerUnit > 0)//If the CostPerUnit value entered by the user
                                        reorderitem.CostPerUnit = item.CostPerUnit;
                                    else
                                        reorderitem.CostPerUnit = inventoryDetail.AverageCost * unit.QuantityOfBaseunit;
                                    if (item.Quantity > 0)//If the Quantity value entered by the user
                                        reorderitem.Quantity = item.Quantity;
                                    else
                                        reorderitem.Quantity = displayQuantity < 0 ? 0 : displayQuantity;
                                    reorderitem.Discount = item.Discount;
                                    reorderitem.Total = item.CostPerUnit * reorderitem.Quantity * (1 - (reorderitem.Discount / 100)) * (1 + reorderitem.Item.SumTaxRates(false));
                                    reorderitem.IsForReorder = true;
                                    reorderitem.ReorderItemGroup = group;
                                    //add Dimensions
                                    foreach (var dimemsion in item.Dimensions)
                                    {
                                        var dm = dbContext.TransactionDimensions.Find(dimemsion);
                                        if (dm != null)
                                            reorderitem.Dimensions.Add(dm);
                                    }
                                    //Calculate Inventory
                                    var unitbase = reorderitem.Item.Units.FirstOrDefault(s => s.IsBase);
                                    reorderitem.InInventory = (inventoryDetail.CurrentInventoryLevel / unitbase.QuantityOfBaseunit);
                                    //End calculate
                                    //Canculate On Order
                                    decimal quantityItemsPurchaseAssociated = 0;
                                    decimal quantitytranferItem = 0;
                                    QuantityItemsPurchaseByItem(locationId, reorderitem.Item.Id, out quantityItemsPurchaseAssociated, out quantitytranferItem);
                                    reorderitem.OnOrder = quantityItemsPurchaseAssociated - quantitytranferItem;
                                    //end calculate
                                }

                                #region Update Primary Contact for Trader Item

                                var traderItem = reorderitem.Item;
                                if (traderItem.VendorsPerLocation == null || !traderItem.VendorsPerLocation.Any(s => s.Vendor.Id == model.PrimaryContactId))
                                {
                                    TraderItemVendor vendor = new TraderItemVendor();
                                    vendor.Location = dbContext.TraderLocations.Find(locationId);
                                    vendor.Item = traderItem;
                                    vendor.IsPrimaryVendor = true;
                                    vendor.Vendor = group.PrimaryContact;
                                    dbContext.Entry(vendor).State = EntityState.Added;
                                    traderItem.VendorsPerLocation.Add(vendor);
                                }

                                #endregion Update Primary Contact for Trader Item

                                if (dbContext.Entry(reorderitem).State == EntityState.Detached)
                                    dbContext.ReorderItems.Attach(reorderitem);
                                dbContext.Entry(reorderitem).State = EntityState.Modified;
                            }
                        }
                        //Calculate Total Group
                        group.Total = group.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);
                        if (group.Id > 0)
                        {
                            if (dbContext.Entry(group).State == EntityState.Detached)
                                dbContext.ReorderItemGroups.Attach(group);
                            dbContext.Entry(group).State = EntityState.Modified;
                        }
                        else
                        {
                            dbContext.Entry(group).State = EntityState.Added;
                            reorder.ReorderItemGroups.Add(group);
                        }

                        //Calculate Total reorder
                        reorder.Total = reorder.ReorderItemGroups.Sum(s => s.Total);
                        if (dbContext.Entry(reorder).State == EntityState.Detached)
                            dbContext.Reorders.Attach(reorder);
                        dbContext.Entry(reorder).State = EntityState.Modified;
                        returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }

        public ReturnJsonModel CalculateQuantities(int locationId, ReorderItemGroupCustomModel model, string timezone, string dateformat)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, model);
                var days2LastFrom = DateTime.UtcNow;
                var days2LastTo = DateTime.UtcNow;
                switch (model.DaysToLastBasis)
                {
                    case 1:
                        days2LastFrom = days2LastTo.AddDays(-7);
                        break;

                    case 2:
                        days2LastTo = days2LastFrom.AddMonths(1);
                        break;

                    case 3:
                        if (!string.IsNullOrEmpty(model.Days2Last.Trim()))
                        {
                            if (!model.Days2Last.Contains('-'))
                            {
                                model.Days2Last += "-";
                            }
                            model.Days2Last.ConvertDaterangeFormat(dateformat, timezone, out days2LastFrom, out days2LastTo);
                            days2LastFrom.AddTicks(1);
                            days2LastTo.AddDays(1).AddTicks(-1);
                        }
                        break;
                }
                var numberOfDay = (decimal)(days2LastTo - days2LastFrom).TotalDays;//Number of days in date range
                var nOfDay = numberOfDay == 0 ? 1 : numberOfDay;
                decimal movementRate = 0;
                foreach (var item in model.Items)
                {
                    var reorderitem = dbContext.ReorderItems.Find(item.Id);
                    if (reorderitem != null)
                    {
                        var inventoryDetail = reorderitem.Item.InventoryDetails.FirstOrDefault(s => s.Location.Id == locationId);
                        if (inventoryDetail != null && item.IsForReorder)
                        {
                            var unit = dbContext.ProductUnits.Find(item.UnitId);
                            movementRate = inventoryDetail.InventoryBatches.Where(e => e.Direction == BatchDirection.Out
                                                                            && e.CreatedDate <= days2LastTo
                                                                            && e.CreatedDate >= days2LastFrom).Sum(u => u.OriginalQuantity) / nOfDay;
                            var reorderQuantity = (movementRate * model.DaysToLast) - inventoryDetail.CurrentInventoryLevel;
                            var displayQuantity = reorderQuantity * unit.QuantityOfBaseunit;
                            reorderitem.IsForReorder = item.IsForReorder;
                            if (item.Quantity > 0)//If the Quantity value entered by the user
                                reorderitem.Quantity = item.Quantity;
                            else
                                reorderitem.Quantity = displayQuantity < 0 ? 0 : displayQuantity;
                            reorderitem.Unit = unit;
                            reorderitem.Discount = item.Discount;
                            if (item.CostPerUnit > 0)//If the CostPerUnit value entered by the user
                                reorderitem.CostPerUnit = item.CostPerUnit;
                            else
                                reorderitem.CostPerUnit = inventoryDetail.AverageCost;
                            reorderitem.Total = reorderitem.CostPerUnit * unit.QuantityOfBaseunit * reorderitem.Quantity * (1 - (reorderitem.Discount / 100)) * (1 + reorderitem.Item.SumTaxRates(false));
                            //Calculate Inventory
                            reorderitem.InInventory = (inventoryDetail.CurrentInventoryLevel / unit.QuantityOfBaseunit);
                            //End calculate
                            //Canculate On Order
                            decimal quantityItemsPurchaseAssociated = 0;
                            decimal quantitytranferItem = 0;
                            QuantityItemsPurchaseByItem(locationId, reorderitem.Item.Id, out quantityItemsPurchaseAssociated, out quantitytranferItem);
                            reorderitem.OnOrder = quantityItemsPurchaseAssociated - quantitytranferItem;
                            //end calculate
                            //add Dimensions
                            foreach (var dimemsion in item.Dimensions)
                            {
                                var dm = dbContext.TransactionDimensions.Find(dimemsion);
                                if (dm != null)
                                    reorderitem.Dimensions.Add(dm);
                            }
                            if (dbContext.Entry(reorderitem).State == EntityState.Detached)
                                dbContext.ReorderItems.Attach(reorderitem);
                            dbContext.Entry(reorderitem).State = EntityState.Modified;
                        }
                        else
                        {
                            reorderitem.IsDisabled = item.IsDisabled;
                            reorderitem.IsForReorder = false;
                            if (dbContext.Entry(reorderitem).State == EntityState.Detached)
                                dbContext.ReorderItems.Attach(reorderitem);
                            dbContext.Entry(reorderitem).State = EntityState.Modified;
                        }
                    }
                }
                //Calculate Total Group
                var group = dbContext.ReorderItemGroups.Find(model.Id);
                if (group != null)
                {
                    group.DaysToLastBasis = model.DaysToLastBasis;
                    group.Days2Last = model.Days2Last;
                    group.DaysToLast = model.DaysToLast;
                    group.DeliveryMethod = model.DeliveryMethod.HasValue ? model.DeliveryMethod.Value : DeliveryMethodEnum.None;
                    group.Total = group.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);
                    if (dbContext.Entry(group).State == EntityState.Detached)
                        dbContext.ReorderItemGroups.Attach(group);
                    dbContext.Entry(group).State = EntityState.Modified;
                }
                //Calculate Total reorder
                var reorder = dbContext.Reorders.Find(model.ReorderId);
                if (reorder != null)
                {
                    reorder.Total = reorder.ReorderItemGroups.Sum(s => s.Total);
                    if (dbContext.Entry(reorder).State == EntityState.Detached)
                        dbContext.Reorders.Attach(reorder);
                    dbContext.Entry(reorder).State = EntityState.Modified;
                }
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(model.DomainId); ;
                var profiGroups = reorder.ReorderItemGroups.Where(s => s.PrimaryContact != null).ToList();
                var countRedorderItems = profiGroups.Sum(s => s.ReorderItems.Where(i => i.IsForReorder).Count());
                returnJson.Object = new { countRedorderItems, total = (reorder != null ? reorder.Total : 0).ToCurrencySymbol(currencySettings) };
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, model);
                return returnJson;
            }
        }

        public ReturnJsonModel ReorderFinish(ReorderCustomModel model, int domainid, string userId, string originatingConnectionId = "")
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, model);
                    var reorder = dbContext.Reorders.Find(model.Id);
                    if (reorder != null)
                    {
                        var user = dbContext.QbicleUser.Find(userId);
                        reorder.Workgroup = dbContext.WorkGroups.Find(model.WorkgroupId);
                        reorder.ExcludeProductGroup = model.ExcludeGroupId.HasValue ? dbContext.TraderGroups.Find(model.ExcludeGroupId) : null;
                        reorder.DeliveryMethod = model.Delivery;
                        if (model.TypeSubmit == "finish")
                        {
                            reorder.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                            List<ActivityNotification> notifications = new List<ActivityNotification>();
                            var groups = reorder.ReorderItemGroups.Where(s => s.PrimaryContact != null).ToList();
                            foreach (var item in groups)
                            {
                                TraderPurchase purchase = new TraderPurchase();
                                purchase.Location = reorder.Location;
                                purchase.Status = TraderPurchaseStatusEnum.PendingReview;
                                purchase.Reference = new TraderReferenceRules(dbContext).GetNewReference(domainid, TraderReferenceType.Purchase);
                                purchase.DeliveryMethod = item.DeliveryMethod.HasValue ? item.DeliveryMethod.Value : DeliveryMethodEnum.None;
                                purchase.CreatedDate = DateTime.UtcNow;
                                purchase.CreatedBy = user;
                                purchase.Vendor = item.PrimaryContact;
                                purchase.Vendor.InUsed = true;
                                purchase.Workgroup = reorder.Workgroup;
                                foreach (var reit in item.ReorderItems)
                                {
                                    if (reit.IsForReorder && reit.Quantity > 0)
                                    {
                                        TraderTransactionItem transItem = new TraderTransactionItem();
                                        transItem.TraderItem = reit.Item;
                                        transItem.Discount = reit.Discount;
                                        transItem.Quantity = reit.Quantity;
                                        transItem.Unit = reit.Unit;
                                        transItem.CostPerUnit = reit.CostPerUnit;
                                        transItem.Cost = reit.Total;
                                        transItem.CreatedBy = user;
                                        transItem.CreatedDate = DateTime.UtcNow;
                                        transItem.Dimensions = reit.Dimensions;
                                        foreach (var tax in reit.Item.TaxRates.Where(s => s.IsPurchaseTax).ToList())
                                        {
                                            var staticTaxRate = new TaxRateRules(dbContext).CloneStaticTaxRateById(tax.Id);
                                            OrderTax orderTax = new OrderTax
                                            {
                                                StaticTaxRate = staticTaxRate,
                                                TaxRate = tax,
                                                Value = transItem.CostPerUnit * transItem.Unit.QuantityOfBaseunit * transItem.Quantity * (1 - (transItem.Discount / 100)) * (tax.Rate / 100)
                                            };
                                            transItem.Taxes.Add(orderTax);
                                            dbContext.OrderTaxs.Add(orderTax);
                                            dbContext.Entry(orderTax).State = EntityState.Added;
                                        }
                                        purchase.PurchaseItems.Add(transItem);

                                        #region Transaction item logs

                                        TransactionItemLog transItemLog = new TransactionItemLog
                                        {
                                            Unit = transItem.Unit,
                                            AssociatedTransactionItem = transItem,
                                            Cost = transItem.Cost,
                                            CostPerUnit = transItem.CostPerUnit,
                                            Dimensions = transItem.Dimensions,
                                            Discount = transItem.Discount,
                                            Quantity = transItem.Quantity,
                                            SalePricePerUnit = transItem.SalePricePerUnit,
                                            TraderItem = transItem.TraderItem,
                                            TransferItems = transItem.TransferItems
                                        };
                                        dbContext.Entry(transItemLog).State = EntityState.Added;
                                        dbContext.TraderTransactionItemLogs.Add(transItemLog);

                                        #endregion Transaction item logs

                                        reit.PurchaseItem = transItem;
                                        if (dbContext.Entry(reit).State == EntityState.Detached)
                                            dbContext.ReorderItems.Attach(reit);
                                        dbContext.Entry(reit).State = EntityState.Modified;
                                    }
                                }

                                #region Approvals and Notifications

                                var refFull = purchase.Reference == null ? "" : purchase.Reference.FullRef;
                                var approval = new ApprovalReq
                                {
                                    ApprovalRequestDefinition = dbContext.PurchaseApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == purchase.Workgroup.Id),
                                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                                    Purchase = new List<TraderPurchase> { purchase },
                                    Name = $"Trader Approval for Purchase #{refFull}",
                                    Qbicle = purchase.Workgroup.Qbicle,
                                    Topic = purchase.Workgroup.Topic,
                                    State = QbicleActivity.ActivityStateEnum.Open,
                                    StartedBy = purchase.CreatedBy,
                                    StartedDate = DateTime.UtcNow,
                                    TimeLineDate = DateTime.UtcNow,
                                    Notes = "",
                                    App = QbicleActivity.ActivityApp.Trader
                                };

                                approval.ActivityMembers.AddRange(purchase.Workgroup.Members);
                                dbContext.ApprovalReqs.Add(approval);
                                dbContext.Entry(approval).State = EntityState.Added;
                                purchase.PurchaseApprovalProcess = approval;
                                var purchaseLog = new PurchaseLog
                                {
                                    AssociatedPurchase = purchase,
                                    CreatedBy = user,
                                    CreatedDate = DateTime.UtcNow,
                                    DeliveryMethod = purchase.DeliveryMethod,
                                    Invoices = purchase.Invoices,
                                    IsInHouse = false,
                                    Location = purchase.Location,
                                    PurchaseApprovalProcess = approval,
                                    PurchaseItems = purchase.PurchaseItems,
                                    PurchaseOrder = purchase.PurchaseOrder,
                                    PurchaseTotal = purchase.PurchaseTotal,
                                    Status = purchase.Status,
                                    Transfer = null,
                                    Vendor = purchase.Vendor,
                                    Workgroup = purchase.Workgroup
                                };

                                var purchaseProcessLog = new PurchaseProcessLog
                                {
                                    AssociatedPurchase = purchase,
                                    AssociatedPurchaseLog = purchaseLog,
                                    PurchaseStatus = purchase.Status,
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = purchase.CreatedBy,
                                    ApprovalReqHistory = new ApprovalReqHistory
                                    {
                                        ApprovalReq = approval,
                                        UpdatedBy = user,
                                        CreatedDate = DateTime.UtcNow,
                                        RequestStatus = approval.RequestStatus
                                    }
                                };

                                dbContext.TraderPurchaseProcessLogs.Add(purchaseProcessLog);
                                dbContext.Entry(purchaseProcessLog).State = EntityState.Added;
                                var activityNotification = new ActivityNotification
                                {
                                    OriginatingConnectionId = originatingConnectionId,
                                    Id = approval.Id,
                                    EventNotify = NotificationEventEnum.ApprovalCreation,
                                    AppendToPageName = ApplicationPageName.Activities,
                                    AppendToPageId = 0,
                                    CreatedById = userId,
                                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                                    ReminderMinutes = 0
                                };
                                notifications.Add(activityNotification);

                                #endregion Approvals and Notifications

                                purchase.PurchaseTotal = purchase.PurchaseItems.Sum(s => s.Cost);
                                dbContext.TraderPurchases.Add(purchase);
                                dbContext.Entry(purchase).State = EntityState.Added;
                                item.Purchase = purchase;
                                if (dbContext.Entry(item).State == EntityState.Detached)
                                    dbContext.ReorderItemGroups.Attach(item);
                                dbContext.Entry(item).State = EntityState.Modified;
                                dbContext.SaveChanges();
                            }
                            reorder.DateComplete = DateTime.UtcNow;
                            reorder.Status = Reorder.StatusEnum.Complete;
                            returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                            var nRule = new NotificationRules(dbContext);
                            foreach (var item in notifications)
                            {
                                nRule.Notification2Activity(item);
                            }
                        }
                        else
                        {
                            reorder.DateComplete = DateTime.UtcNow;
                            reorder.Status = Reorder.StatusEnum.InComlete;
                            returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }

        public ReturnJsonModel ExcludeReorderItems(int productGroupId, int reorderId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, productGroupId, reorderId);
                    var reorder = dbContext.Reorders.Find(reorderId);
                    if (reorder != null)
                    {
                        var items = dbContext.ReorderItems.Where(s => s.ReorderItemGroup.Reorder.Id == reorderId).ToList();
                        foreach (var item in items)
                        {
                            if (item.Item.Group.Id == productGroupId)
                            {
                                item.IsDisabled = true;
                                item.IsForReorder = false;
                            }
                            else
                            {
                                item.IsDisabled = false;
                            }

                            if (dbContext.Entry(item).State == EntityState.Detached)
                                dbContext.ReorderItems.Attach(item);
                            dbContext.Entry(item).State = EntityState.Modified;
                        }
                        reorder.ExcludeProductGroup = dbContext.TraderGroups.Find(productGroupId);
                        //Calculate Total Group
                        foreach (var item in reorder.ReorderItemGroups.Where(s => s.PrimaryContact != null).ToList())
                        {
                            item.Total = item.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);
                            if (dbContext.Entry(item).State == EntityState.Detached)
                                dbContext.ReorderItemGroups.Attach(item);
                            dbContext.Entry(item).State = EntityState.Modified;
                        }
                        //Calculate Total reorder
                        reorder.Total = reorder.ReorderItemGroups.Sum(s => s.Total);
                    }

                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, productGroupId, reorderId);
                    transaction.Rollback();
                }
                return returnJson;
            }
        }

        public ReturnJsonModel UncheckAllReorder(int groupid)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, groupid);

                //Calculate Total Group
                var group = dbContext.ReorderItemGroups.Find(groupid);
                if (group != null)
                {
                    foreach (var item in group.ReorderItems)
                    {
                        item.IsForReorder = false;
                        if (dbContext.Entry(item).State == EntityState.Detached)
                            dbContext.ReorderItems.Attach(item);
                        dbContext.Entry(item).State = EntityState.Modified;
                    }
                    group.Total = group.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);
                    if (dbContext.Entry(group).State == EntityState.Detached)
                        dbContext.ReorderItemGroups.Attach(group);
                    dbContext.Entry(group).State = EntityState.Modified;
                }
                //Calculate Total reorder
                var reorder = dbContext.Reorders.Find(group.Reorder.Id);
                if (reorder != null)
                {
                    reorder.Total = reorder.ReorderItemGroups.Sum(s => s.Total);
                    if (dbContext.Entry(reorder).State == EntityState.Detached)
                        dbContext.Reorders.Attach(reorder);
                    dbContext.Entry(reorder).State = EntityState.Modified;
                }
                var profiGroups = reorder.ReorderItemGroups.Where(s => s.PrimaryContact != null).ToList();
                var countRedorderItems = profiGroups != null ? profiGroups.Sum(s => s.ReorderItems.Where(i => i.IsForReorder).Count()) : 0;
                returnJson.Object = new { countRedorderItems, groupPriceTotal = group.Total, total = reorder.Total };
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupid);
                return returnJson;
            }
        }

        public ReturnJsonModel ChangeContact(int groupid, int PrimaryContactId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, groupid, PrimaryContactId);
                    var group = dbContext.ReorderItemGroups.Find(groupid);
                    if (group != null)
                    {
                        var exist = dbContext.ReorderItemGroups.Where(s => s.Reorder.Id == group.Reorder.Id && s.PrimaryContact.Id == PrimaryContactId).Any();
                        if (!exist)
                        {
                            group.PrimaryContact = dbContext.TraderContacts.Find(PrimaryContactId);
                            foreach (var item in group.ReorderItems)
                            {
                                item.PrimaryContact = group.PrimaryContact;
                                if (dbContext.Entry(item).State == EntityState.Detached)
                                    dbContext.ReorderItems.Attach(item);
                                dbContext.Entry(item).State = EntityState.Modified;
                            }
                            if (dbContext.Entry(group).State == EntityState.Detached)
                                dbContext.ReorderItemGroups.Attach(group);
                            dbContext.Entry(group).State = EntityState.Modified;
                            returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                            transaction.Commit();
                        }
                        else
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_818");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupid, PrimaryContactId);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }

        public ReturnJsonModel MoveContacts(ReorderItemGroupCustomModel model)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, model);
                    var reorder = dbContext.Reorders.Find(model.ReorderId);
                    var group = dbContext.ReorderItemGroups.Find(model.Id);
                    if (group != null)
                    {
                        foreach (var item in model.Items)
                        {
                            var contact = dbContext.TraderContacts.Find(item.PrimaryContactId);

                            if (contact != group.PrimaryContact)
                            {
                                var movegroup = reorder.ReorderItemGroups.FirstOrDefault(s => s.PrimaryContact != null && s.PrimaryContact.Id == contact.Id);
                                var reitem = dbContext.ReorderItems.Find(item.Id);
                                if (movegroup != null && reitem != null)
                                {
                                    reitem.ReorderItemGroup = movegroup;
                                    reitem.PrimaryContact = contact;
                                    if (dbContext.Entry(reitem).State == EntityState.Detached)
                                        dbContext.ReorderItems.Attach(reitem);
                                    dbContext.Entry(reitem).State = EntityState.Modified;
                                }
                                else if (movegroup == null && reitem != null)
                                {
                                    movegroup = new ReorderItemGroup();
                                    movegroup.PrimaryContact = contact;
                                    movegroup.DeliveryMethod = group.DeliveryMethod;
                                    movegroup.DaysToLastBasis = group.DaysToLastBasis;
                                    movegroup.DaysToLast = group.DaysToLast;
                                    movegroup.Days2Last = group.Days2Last;
                                    movegroup.Reorder = reorder;
                                    movegroup.ReorderItems.Add(reitem);
                                    dbContext.ReorderItemGroups.Add(movegroup);
                                    dbContext.Entry(movegroup).State = EntityState.Added;
                                    reorder.ReorderItemGroups.Add(movegroup);
                                    reitem.PrimaryContact = contact;
                                    if (dbContext.Entry(reitem).State == EntityState.Detached)
                                        dbContext.ReorderItems.Attach(reitem);
                                    dbContext.Entry(reitem).State = EntityState.Modified;
                                }
                                movegroup.Total = movegroup.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);

                                #region Update Primary Contact for Trader Item

                                var traderItem = reitem?.Item ?? null;
                                if (traderItem != null && traderItem.VendorsPerLocation == null || !traderItem.VendorsPerLocation.Any(s => s.Vendor.Id == contact.Id))
                                {
                                    TraderItemVendor vendor = new TraderItemVendor();
                                    vendor.Location = reorder.Location;
                                    vendor.Item = traderItem;
                                    vendor.IsPrimaryVendor = true;
                                    vendor.Vendor = contact;
                                    dbContext.Entry(vendor).State = EntityState.Added;
                                    traderItem.VendorsPerLocation.Add(vendor);
                                }

                                #endregion Update Primary Contact for Trader Item
                            }
                        }
                        //dbContext.SaveChanges();
                        group.Total = group.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);
                        if (dbContext.Entry(group).State == EntityState.Detached)
                            dbContext.ReorderItemGroups.Attach(group);
                        dbContext.Entry(group).State = EntityState.Modified;
                        returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }

        public ReturnJsonModel SetIsReOrderForItem(int itemId, bool isReOrder)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId, isReOrder);
                var reitem = dbContext.ReorderItems.Find(itemId);
                if (reitem != null)
                {
                    reitem.IsForReorder = isReOrder;
                    var group = reitem.ReorderItemGroup;
                    group.Total = group.ReorderItems.Where(s => s.IsForReorder).Sum(s => s.Total);
                    var reorder = reitem.ReorderItemGroup.Reorder;
                    reorder.Total = reorder.ReorderItemGroups.Sum(s => s.Total);
                    var profiGroups = reorder.ReorderItemGroups.Where(s => s.PrimaryContact != null).ToList();
                    var countRedorderItems = profiGroups.Sum(s => s.ReorderItems.Where(i => i.IsForReorder).Count());
                    var countGroupItems = reitem.ReorderItemGroup.ReorderItems.Where(s => s.IsForReorder).Count();
                    returnJson.Object = new { countRedorderItems, countGroupItems, groupPriceTotal = group.Total, total = reorder.Total };
                    returnJson.result = dbContext.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, itemId, isReOrder);
            }
            return returnJson;
        }

        public ReturnJsonModel ChangeDelivery(int reorderid, DeliveryMethodEnum DeliveryMethod)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, reorderid, DeliveryMethod);

                    var reorder = dbContext.Reorders.Find(reorderid);
                    if (reorder != null)
                    {
                        reorder.DeliveryMethod = DeliveryMethod;
                        foreach (var item in reorder.ReorderItemGroups)
                        {
                            item.DeliveryMethod = DeliveryMethod;
                            if (dbContext.Entry(item).State == EntityState.Detached)
                                dbContext.ReorderItemGroups.Attach(item);
                            dbContext.Entry(item).State = EntityState.Modified;
                        }
                        if (dbContext.Entry(reorder).State == EntityState.Detached)
                            dbContext.Reorders.Attach(reorder);
                        dbContext.Entry(reorder).State = EntityState.Modified;
                        returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, reorderid, DeliveryMethod);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }

        private void QuantityItemsPurchaseByItem(int locationId, int itemId, out decimal quantityItemsPurchaseAssociated, out decimal quantitytranferItem)
        {
            var query = dbContext.TraderPurchases.Where(
            s => s.Location.Id == locationId
            && (s.Status == TraderPurchaseStatusEnum.PurchaseApproved || s.Status == TraderPurchaseStatusEnum.PurchaseOrderIssued)
            && s.PurchaseItems.Any(p => p.TraderItem.Id == itemId));
            var purchaseItems = query.SelectMany(s => s.PurchaseItems).ToList();
            quantityItemsPurchaseAssociated = purchaseItems.Sum(s => s.Quantity);
            var tranfer = query.SelectMany(s => s.Transfer).Where(s => s.Status == TransferStatus.Delivered).ToList();
            quantitytranferItem = tranfer != null ? tranfer.SelectMany(s => s.TransferItems).Sum(s => s.QuantityAtPickup) : 0;
        }
    }
}