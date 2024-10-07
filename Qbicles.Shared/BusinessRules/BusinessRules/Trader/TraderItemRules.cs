using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Product;
using Qbicles.Models.Trader.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Tweetinvi.Logic.Model;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderItemRules
    {
        private ApplicationDbContext _db;

        public TraderItemRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }

        public ReturnJsonModel DeleteVendorContact(int domainId, int id)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, id);
                if (id > 0)
                {
                    var contact = DbContext.TraderContacts.Find(id);
                    contact.AssociatedItems = new List<TraderItemVendor>();
                    DbContext.Entry(contact).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    DbContext.Entry(contact).State = EntityState.Deleted;
                    DbContext.TraderContacts.Remove(contact);
                    DbContext.SaveChanges();
                }
                else
                {
                    result.actionVal = 3;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, id);
                result.msg = ex.Message;
                result.actionVal = 3;
            }

            if (DbContext.TraderContacts.Any(q => q.ContactGroup != null && q.ContactGroup.Domain.Id == domainId))
                result.result = true;
            return result;
        }

        public ReturnJsonModel SaveContact(TraderContact contact, string country = "", string userId = "")
        {
            var result = new ReturnJsonModel { result = false, actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contact, country);
                contact.QbicleUser = DbContext.QbicleUser.Find(userId);
                var s3Rules = new Azure.AzureStorageRules(DbContext);
                if (contact.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(contact.AvatarUri);
                }
                else
                {
                    var mediaValid = DbContext.TraderContacts.Find(contact.Id);
                    if (mediaValid.AvatarUri != contact.AvatarUri)
                        s3Rules.ProcessingMediaS3(contact.AvatarUri);
                }

                if (contact.Address == null)
                    contact.Address = new TraderAddress();
                if (contact.Address.Id > 0)
                {
                    var address = DbContext.TraderAddress.Find(contact.Address.Id);
                    address.AddressLine1 = contact.Address.AddressLine1;
                    address.AddressLine2 = contact.Address.AddressLine2;
                    address.City = contact.Address.City;
                    if (country != "") address.Country = new CountriesRules().GetCountryByName(country);
                    address.State = contact.Address.State;
                    address.PostCode = contact.Address.PostCode;
                    contact.Address = address;
                }
                else
                {
                    if (country != "") contact.Address.Country = new CountriesRules().GetCountryByName(country);
                }

                if (contact.AssociatedItems.Count > 0)
                    for (var i = 0; i < contact.AssociatedItems.Count; i++)
                        contact.AssociatedItems[i] = DbContext.TraderItemVendors.Find(contact.AssociatedItems[i].Id);

                contact.ContactGroup = DbContext.TraderContactGroups.Find(contact.ContactGroup.Id);
                contact.CustomerAccount = DbContext.BKAccounts.Find(contact.CustomerAccount.Id);
                if (contact.Id > 0)
                {
                    var contactItem = DbContext.TraderContacts.Find(contact.Id);
                    contactItem.Email = contact.Email;
                    contactItem.Address = contact.Address;
                    contactItem.AssociatedItems = contact.AssociatedItems;
                    contactItem.AvatarUri = contact.AvatarUri;
                    contactItem.CompanyName = contact.CompanyName;
                    contactItem.ContactGroup = contact.ContactGroup;
                    contactItem.CustomerAccount = contact.CustomerAccount;
                    contactItem.JobTitle = contact.JobTitle;
                    contactItem.Name = contact.Name;
                    contactItem.PhoneNumber = contact.PhoneNumber;
                    contactItem.QbicleUser = contact.QbicleUser;
                    DbContext.Entry(contactItem).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.actionVal = 2;
                }
                else
                {
                    DbContext.Entry(contact).State = EntityState.Added;
                    DbContext.TraderContacts.Add(contact);
                    DbContext.SaveChanges();
                    result.actionVal = 1;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contact, country);
                result.actionVal = 3;
                result.msg = ex.Message;
            }

            return result;
        }

        public TraderTransactionItem GetSaleItemById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderSaleItems.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderTransactionItem();
            }
        }

        public ReturnJsonModel CheckBookkeepingConnected(int domainId)
        {
            var result = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var setting = DbContext.TraderSettings.FirstOrDefault(q => q.Domain.Id == domainId);
                if (setting != null) result.result = setting.IsQbiclesBookkeepingEnabled;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                result.msg = ex.Message;
                result.result = false;
            }

            return result;
        }

        public DataTablesResponse GetItemOverviewItemProduct(IDataTablesRequest requestModel, int domainId, int locationId, string keysearch,
            string groupids, string types, string brands, string needs, string rating, string tags, int activeInLocation)
        {
            var totalRecords = 0;
            var response = new DataTablesResponse(requestModel.Draw, new List<ItemOverview>(), 0, 0);
            if (domainId == 0)
                return response;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, locationId, keysearch,
                        groupids, types, brands, needs, rating, tags);
                var traderItems = DbContext.TraderItems.Include(x=>x.Locations).Include(x=>x.InventoryAccount).Include(x=>x.InventoryDetails).Include(x=>x.VendorsPerLocation).Where(q => q.Domain.Id == domainId);

                if (traderItems.Any())
                {
                    if (!string.IsNullOrEmpty(keysearch))
                    {
                        keysearch = keysearch.ToLower().Trim();
                        traderItems = traderItems.Where(q => q.Name.ToLower().Contains(keysearch) || q.SKU.ToLower().Contains(keysearch)
                                                                                                  || q.Barcode.ToLower().Contains(keysearch) || q.Description.ToLower().Contains(keysearch)
                                                                                                  || (q.Group != null && q.Group.Name.ToLower().Contains(keysearch)));
                    }
                    if (!string.IsNullOrEmpty(groupids))
                    {
                        var lstGroupIds = groupids.Split(',').ToList().Select(s => int.Parse(s)).ToList();
                        traderItems = traderItems.Where(q => lstGroupIds.Contains(q.Group.Id));
                    }
                    if (!string.IsNullOrEmpty(types))
                    {
                        var lstTypes = types.Split(',');
                        if (!(lstTypes.Contains("2") && lstTypes.Contains("1") && lstTypes.Contains("0")))
                        {
                            if (lstTypes.Contains("2") && lstTypes.Contains("1"))
                            {
                                traderItems = traderItems.Where(q => (q.IsBought && q.IsSold) || (!q.IsBought && q.IsSold));
                            }
                            else if (lstTypes.Contains("2") && lstTypes.Contains("0"))
                            {
                                traderItems = traderItems.Where(q => (q.IsBought && q.IsSold) || (q.IsBought && !q.IsSold));
                            }
                            else if (lstTypes.Contains("1") && lstTypes.Contains("0"))
                            {
                                traderItems = traderItems.Where(q => (q.IsBought && !q.IsSold) || (!q.IsBought && q.IsSold));
                            }
                            else if (lstTypes.Contains("2"))
                            {
                                traderItems = traderItems.Where(q => (q.IsBought && q.IsSold));
                            }
                            else if (lstTypes.Contains("1"))
                            {
                                traderItems = traderItems.Where(q => (!q.IsBought && q.IsSold));
                            }
                            else if (lstTypes.Contains("0"))
                            {
                                traderItems = traderItems.Where(q => (q.IsBought && !q.IsSold));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(brands))
                    {
                        var lstBrand = brands.Split(',').Select(s => int.Parse(s)).ToList();
                        traderItems = traderItems.Where(q => q.AdditionalInfos.Where(a => a.Type == AdditionalInfoType.Brand).Any() && lstBrand.Contains(q.AdditionalInfos.FirstOrDefault(f => f.Type == AdditionalInfoType.Brand).Id));
                    }
                    if (!string.IsNullOrEmpty(needs))
                    {
                        var lstNeed = needs.Split(',').Select(s => int.Parse(s)).ToList();
                        traderItems = traderItems.Where(q => q.AdditionalInfos.Where(a => a.Type == AdditionalInfoType.Need && lstNeed.Contains(a.Id)).Any());
                    }
                    if (!string.IsNullOrEmpty(rating))
                    {
                        var lstRating = rating.Split(',').Select(s => int.Parse(s)).ToList();
                        traderItems = traderItems.Where(q => q.AdditionalInfos.Where(a => a.Type == AdditionalInfoType.QualityRating).Any() && lstRating.Contains(q.AdditionalInfos.FirstOrDefault(f => f.Type == AdditionalInfoType.QualityRating).Id));
                    }
                    if (!string.IsNullOrEmpty(tags))
                    {
                        var lstTag = tags.Split(',').Select(s => int.Parse(s)).ToList();
                        traderItems = traderItems.Where(q => q.AdditionalInfos.Where(a => a.Type == AdditionalInfoType.ProductTag && lstTag.Contains(a.Id)).Any());
                    }
                    if (activeInLocation == 2)
                    {
                        traderItems = traderItems.Where(q => q.IsActiveInAllLocations || q.Locations.Any(s => s.Id == locationId));
                    }
                    else if (activeInLocation == 3)
                    {
                        traderItems = traderItems.Where(q => !q.IsActiveInAllLocations && !q.Locations.Any(s => s.Id == locationId));
                    }
                    if (traderItems.Any())
                    {
                        totalRecords = traderItems.Count();
                        var sortedColumns = requestModel.Columns.GetSortedColumns();
                        var orderByString = string.Empty;
                        foreach (var column in sortedColumns)
                        {
                            switch (column.Name)
                            {
                                case "ItemName":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;

                                case "Description":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "DescriptionText" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;

                                case "SKU":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "SKU" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;

                                case "Barcode":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "Barcode" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;

                                case "GroupName":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "Group.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;

                                case "IsActive":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "IsActiveInAllLocations" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    if (locationId > 0)
                                        orderByString += ",Locations.Any(Id==" + locationId + ")" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;

                                default:
                                    orderByString = "Id asc";
                                    break;
                            }
                        }
                        traderItems = traderItems.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                        var lstItems = traderItems.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                        var returnListItem = lstItems.Select(q => new ItemOverview()
                        {
                            Id = q.Id,
                            GroupName = q.Group != null ? q.Group.Name : "",
                            ImageUri = q.ImageUri,
                            IsSold = q.IsSold,
                            IsBought = q.IsBought,
                            IsCompoundProduct = q.IsCompoundProduct,
                            IsActive = q.IsActiveInAllLocations || q.Locations.Select(s => s.Id).Contains(locationId),
                            Description = string.IsNullOrEmpty(q.DescriptionText) ? "" : q.DescriptionText,
                            SKU = q.SKU,
                            Barcode = q.Barcode,
                            //Unit = q.Units.FirstOrDefault(x => x.IsBase).Name,
                           // Inventory = q.InventoryAccount != null ? q.InventoryAccount.Transactions.Count : 0,
                           // SellingPrice = DbContext.TraderPriceBookPrices.FirstOrDefault(x => x.Item.Id == q.Id)?.FullPrice ?? 0,
                            //Cost = q.InventoryDetails.FirstOrDefault()?.AverageCost ?? 0,
                            ItemName = q.Name,
                            Vendor = (q.VendorsPerLocation.Count > 0 ? string.Join("<br/>", q.VendorsPerLocation.Where(p => p.IsPrimaryVendor && p.Location.Id == locationId).Select(s => s.Vendor.Name).ToArray()) : "N/A")
                        }).ToList();

                        return new DataTablesResponse(requestModel.Draw, returnListItem, totalRecords, totalRecords);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel,
                    domainId, locationId, keysearch, groupids, types, brands, needs, rating, tags);
            }

            return response;
        }

        public DataTablesResponse GetTraderItemsByDateRange(IDataTablesRequest requestModel, int domainId, int locationId, string timeZone,
      string keysearch, string dateFormat, string datestring = "")
        {
            var totalRecords = 0;
            var response = new DataTablesResponse(requestModel.Draw, new List<InventoryBatchCustom>(), 0, 0);
            if (domainId == 0)
                return response;

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, locationId,
                        timeZone, keysearch, dateFormat, datestring);

                var currency = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
                var startDate = DateTime.MinValue;
                var endDate = DateTime.UtcNow;
                if (datestring != null && !string.IsNullOrEmpty(datestring.Trim()))
                {
                    if (!datestring.Contains('-'))
                    {
                        datestring += "-";
                    }

                    datestring.ConvertDaterangeFormat(dateFormat, timeZone, out startDate, out endDate);
                    startDate.AddTicks(1);
                    endDate.AddDays(1).AddTicks(-1);
                }

                var traderItems = from it in DbContext.TraderItems
                                  join iv in DbContext.InventoryDetails on it.Id equals iv.Item.Id
                                  join ig in DbContext.TraderGroups on it.Group.Id equals ig.Id
                                  join pu in DbContext.ProductUnits on it.Id equals pu.Item.Id
                                  where
                                  it.Domain.Id == domainId
                                  && iv.Location.Id == locationId
                                  && iv.InventoryBatches.Any(b => b.CreatedDate >= startDate && b.CreatedDate <= endDate)
                                  select it;

                if (!string.IsNullOrEmpty(keysearch))
                {
                    keysearch = keysearch.ToLower();
                    traderItems = traderItems.Where(q => q.Name.ToLower().Contains(keysearch));
                }

                totalRecords = traderItems.Count();
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "ItemName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }
                traderItems = traderItems.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                //include
                //traderItems = traderItems.Include(x => x.Group);
                //traderItems = traderItems.Include(x => x.Group);

                var lstItems = traderItems.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var returnListItem = lstItems.Select(q => new InventoryBatchCustom()
                {
                    Id = q.Id,
                    In = CalculateInventoryBatchIn(q.InventoryDetails, locationId, q.Units.FirstOrDefault(uI => uI.IsBase), startDate, endDate, currency),
                    Out = CalculateInventoryBatchOut(q.InventoryDetails, locationId, q.Units.FirstOrDefault(uI => uI.IsBase), startDate, endDate, currency),
                    Difference = CalculateInventoryBatchDifference(q.InventoryDetails, locationId, q.Units.FirstOrDefault(uI => uI.IsBase), startDate, endDate, currency),
                    BaseUnitId = q.Units.Any(u => u.IsBase) ? q.Units.FirstOrDefault(u => u.IsBase)?.Id ?? 0 : 0,
                    ImageUri = q.ImageUri,
                    ItemName = q.Name,
                    //Added for report
                    ItemType = q.Name,
                    ProductGroup = q.Group.Name,
                    SKU = q.SKU,
                    Unit = q.ImageUri,
                    Cost = q.Name,
                    PoolPrice = q.Name,
                    QuantitySold = q.Name,
                    PurchaseQuantity = q.Name,
                    TransferInQuantity = q.Name,
                    TransferOutQuantity = q.Name,
                    ManufacturedQuantity = q.Name,
                    GeneratedInventory = q.Name,
                    SpotCountQuantity = q.Name,
                    WasteQuantity = q.Name,
                    OnHandQuantity = q.Name,
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, returnListItem, totalRecords, totalRecords);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel, domainId, locationId, timeZone,
                    keysearch, dateFormat, datestring);
            }

            return response;
        }

        public InventoryBatchCustom ChangeUnitItemInMovement(int traderItemId, int unitId, int domainId, int locationId, string timeZone, string dateFormat, string datestring = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderItemId, locationId, timeZone, dateFormat, datestring);
                var startDate = DateTime.MinValue;
                var endDate = DateTime.UtcNow;
                if (datestring != null && !string.IsNullOrEmpty(datestring.Trim()))
                {
                    if (!datestring.Contains('-'))
                    {
                        datestring += "-";
                    }

                    datestring.ConvertDaterangeFormat(dateFormat, timeZone, out startDate, out endDate);
                    startDate.AddTicks(1);
                    endDate.AddDays(1).AddTicks(-1);
                }
                var traderitem = DbContext.TraderItems.Find(traderItemId);
                var unit = traderitem.Units.FirstOrDefault(s => s.Id == unitId);
                var currency = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
                InventoryBatchCustom inventoryBatch = new InventoryBatchCustom
                {
                    Id = traderitem.Id,
                    BaseUnitId = unit.Id,
                    In = CalculateInventoryBatchIn(traderitem.InventoryDetails, locationId, unit, startDate, endDate, currency),
                    Out = CalculateInventoryBatchOut(traderitem.InventoryDetails, locationId, unit, startDate, endDate, currency),
                    Difference = CalculateInventoryBatchDifference(traderitem.InventoryDetails, locationId, unit, startDate, endDate, currency),
                    ImageUri = traderitem.ImageUri,
                    ItemName = traderitem.Name
                };
                return inventoryBatch;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderItemId, locationId, timeZone, dateFormat, datestring);
                return null;
            }
        }

        private string CalculateInventoryBatchIn(List<InventoryDetail> details, int locationId, ProductUnit unit, DateTime startDate, DateTime endDate, CurrencySetting currency)
        {
            var inventoryBatchesIn = details.Where(s => s.Location.Id == locationId).SelectMany(sm =>
                sm.InventoryBatches.Where(ib => ib.Direction == BatchDirection.In && ib.CreatedDate >= startDate && ib.CreatedDate <= endDate));

            var orgQty = $"{inventoryBatchesIn.Sum(ibs => ibs.OriginalQuantity / (unit?.QuantityOfBaseunit ?? 1)).ToDecimalPlace(currency)} {unit?.Name}";
            var orgPer = $"{inventoryBatchesIn.Sum(ibs => ibs.OriginalQuantity * ibs.CostPerUnit).ToCurrencySymbol(currency)}";

            return $"{orgQty} - {orgPer}";
        }

        private string CalculateInventoryBatchOut(List<InventoryDetail> details, int locationId, ProductUnit unit, DateTime startDate, DateTime endDate, CurrencySetting currency)
        {
            var inventoryBatchesOut = details.Where(s => s.Location.Id == locationId).SelectMany(sm =>
                sm.InventoryBatches.Where(ib => ib.Direction == BatchDirection.Out && ib.CreatedDate >= startDate && ib.CreatedDate <= endDate));

            var orgQty = $"{inventoryBatchesOut.Sum(ibs => ibs.OriginalQuantity / (unit?.QuantityOfBaseunit ?? 1)).ToDecimalPlace(currency)} {unit?.Name}";
            var orgPer = $"{inventoryBatchesOut.Sum(ibs => ibs.OriginalQuantity * ibs.CostPerUnit).ToCurrencySymbol(currency)}";

            return $"{orgQty} - {orgPer}";
        }

        private string CalculateInventoryBatchDifference(List<InventoryDetail> details, int locationId, ProductUnit unit, DateTime startDate, DateTime endDate, CurrencySetting currency)
        {
            details = details.Where(s => s.Location.Id == locationId).ToList();
            var inventoryBatchesIn = details.SelectMany(sm => sm.InventoryBatches.Where(ib =>
                ib.Direction == BatchDirection.In && ib.CreatedDate >= startDate && ib.CreatedDate <= endDate));

            var inventoryBatchesOut = details.SelectMany(sm => sm.InventoryBatches.Where(ib =>
                ib.Direction == BatchDirection.Out && ib.CreatedDate >= startDate && ib.CreatedDate <= endDate));

            var value = Math.Abs(inventoryBatchesIn.Sum(ibs =>
                ibs.OriginalQuantity / (unit?.QuantityOfBaseunit ?? 1)) - inventoryBatchesOut.Sum(ibs => ibs.OriginalQuantity / (unit?.QuantityOfBaseunit ?? 1)));

            var valuePerUnit = Math.Abs(
                inventoryBatchesIn.Sum(ibs => ibs.OriginalQuantity * ibs.CostPerUnit) -
                inventoryBatchesOut.Sum(ibs => ibs.OriginalQuantity * ibs.CostPerUnit));

            return $"{value.ToDecimalPlace(currency)} {unit?.Name} - {valuePerUnit.ToCurrencySymbol(currency)}";
        }

        public DataTablesResponse GetTraderItemsByDateRangeServer(IDataTablesRequest requestModel, int traderItemId, int domainId, int locationId, string timeZone,
            string datetimeFormat, string datestring = "", bool isShowInvented = false, int unitId = 0)
        {
            var newUnit = new ProductUnit();
            var totalBatches = 0;
            newUnit.QuantityOfBaseunit = 1;
            var traderItem = new TraderItem();
            var timeZoneUser = TimeZoneInfo.GetSystemTimeZones().Where(e => e.Id.Equals(timeZone)).FirstOrDefault();
            var currency = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
            var decimalPlace = (int)currency.DecimalPlace;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, traderItemId, locationId,
                        timeZone, datetimeFormat, datestring);

                #region config

                var startDate = DateTime.MinValue;
                var endDate = DateTime.UtcNow;

                if (datestring != null && !string.IsNullOrEmpty(datestring.Trim()))
                {
                    if (!datestring.Contains('-'))
                    {
                        datestring += "-";
                    }

                    datestring.ConvertDaterangeFormat(datetimeFormat, timeZone, out startDate, out endDate);
                    startDate.AddTicks(1);
                    endDate.AddDays(1).AddTicks(-1);
                }

                #endregion config

                var IquerylistBatches = from iv in DbContext.InventoryDetails
                                        from bath in iv.InventoryBatches
                                        where iv.Item.Id == traderItemId
                                       && iv.Location.Id == locationId
                                       && (bath.CreatedDate >= startDate && bath.CreatedDate <= endDate)
                                        // && bath.ParentTransferItem != null
                                        select bath;

                #region isInvented - filter

                if (!isShowInvented)
                {
                    IquerylistBatches = IquerylistBatches.Where(e => e.IsInvented == false);
                }

                #endregion isInvented - filter

                #region Pagination

                var listBatches = IquerylistBatches.OrderByDescending(e => e.CreatedDate).Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Pagination

                #region afterFilter & unit division

                totalBatches = IquerylistBatches.Count();
                if (totalBatches > 0)
                {
                    traderItem = GetById(traderItemId);
                    newUnit = traderItem.Units.FirstOrDefault(q => q.Id == unitId);
                    if (newUnit != null)
                    {
                        if (newUnit.QuantityOfBaseunit <= 0) newUnit.QuantityOfBaseunit = 1;

                        foreach (var batch in listBatches)
                        {
                            batch.OriginalQuantity = batch.OriginalQuantity / newUnit.QuantityOfBaseunit;
                        }
                    }
                }

                #endregion afterFilter & unit division

                var returnItem = listBatches.Select(e => new
                {
                    date = TimeZoneInfo.ConvertTimeFromUtc(e.CreatedDate, timeZoneUser).ToString("dd/MM/yyyy HH:mm"),
                    trigger = e.IsInvented ? "System Generated" : e.ParentTransferItem?.AssociatedTransfer.Reason.GetDescription() ?? string.Empty,
                    quantityIn = Math.Round((e.Direction == BatchDirection.In) ? e.OriginalQuantity : 0, decimalPlace),
                    quantityOut = Math.Round((e.Direction == BatchDirection.Out) ? e.OriginalQuantity : 0, decimalPlace),
                    valueIn = Math.Round(((e.Direction == BatchDirection.In) ? e.OriginalQuantity : 0) * e.CostPerUnit * newUnit.QuantityOfBaseunit, decimalPlace),
                    valueOut = Math.Round(((e.Direction == BatchDirection.Out) ? e.OriginalQuantity : 0) * e.CostPerUnit * newUnit.QuantityOfBaseunit, decimalPlace),
                    absoluteValue = Math.Round(Math.Abs((((e.Direction == BatchDirection.In) ? e.OriginalQuantity : 0) * e.CostPerUnit * newUnit.QuantityOfBaseunit) - (((e.Direction == BatchDirection.Out) ? e.OriginalQuantity : 0) * e.CostPerUnit * newUnit.QuantityOfBaseunit)), decimalPlace),
                    absoluteQuantity = Math.Round(Math.Abs(((e.Direction == BatchDirection.In) ? e.OriginalQuantity : 0) - ((e.Direction == BatchDirection.Out) ? e.OriginalQuantity : 0)), decimalPlace),
                    transferKey = e.ParentTransferItem?.AssociatedTransfer.Key ?? "#",
                    saleKey = e.ParentTransferItem?.AssociatedTransfer.Sale?.Key ?? "#",
                    purchaseKey = e.ParentTransferItem?.AssociatedTransfer.Purchase?.Key ?? "#",
                    wasteKey = e.ParentTransferItem?.AssociatedTransfer.WasteReport?.Key ?? "#",
                    spotKey = e.ParentTransferItem?.AssociatedTransfer.SpotCount?.Key ?? "#",
                    manufacturingKey = e.ParentTransferItem?.AssociatedTransfer.ManufacturingJob?.Key ?? "#",
                    decimalPlace = decimalPlace
                }).ToList();

                return new DataTablesResponse(requestModel.Draw, returnItem, totalBatches, totalBatches);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel, traderItemId, locationId, timeZone,
                    datetimeFormat, datestring);
                return new DataTablesResponse(requestModel.Draw, new List<InventoryBatchCustom>(), 0, 0);
            }
        }

        public decimal GetCurrentOnHandInventoryDetails(int traderItemId, int locationId, int unitId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderItemId, locationId);
            var onHandInventory = (decimal)0;
            var traderItem = GetById(traderItemId);
            var newUnit = new ProductUnit
            {
                QuantityOfBaseunit = 1
            };

            if (traderItemId > 0)
            {
                var IqueryCurrentInventoryDetails = from iv in DbContext.InventoryDetails
                                                    where iv.Item.Id == traderItemId
                                                    && iv.Location.Id == locationId
                                                    select iv.CurrentInventoryLevel;
                onHandInventory = IqueryCurrentInventoryDetails.FirstOrDefault();

                newUnit = traderItem.Units.FirstOrDefault(q => q.Id == unitId);
                if (newUnit != null)
                {
                    if (newUnit.QuantityOfBaseunit <= 0) newUnit.QuantityOfBaseunit = 1;
                }
                onHandInventory = onHandInventory / newUnit.QuantityOfBaseunit;
            }
            else
            {
                onHandInventory = 0;
            }
            return onHandInventory;
        }

        public TraderItem GetTraderItemByDateRange(int traderItemId, int locationId, string timeZone, string datetimeFormat, string datestring)
        {
            var traderItem = new TraderItem();
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderItemId, locationId, timeZone, datestring);
                if (datestring != null && !string.IsNullOrEmpty(datestring.Trim()))
                {
                    if (!datestring.Contains('-'))
                    {
                        datestring += "-";
                    }
                    datestring.ConvertDaterangeFormat(datetimeFormat, timeZone, out startDate, out endDate);
                    startDate.AddTicks(1);
                    endDate.AddDays(1).AddTicks(-1);
                }

                if (traderItemId > 0)
                {
                    traderItem = GetById(traderItemId);
                    traderItem.InventoryDetails = traderItem.InventoryDetails.Where(q => q.Location != null && q.Location.Id == locationId).ToList();
                    if (traderItem.InventoryDetails.Count > 0)
                    {
                        foreach (var inv in traderItem.InventoryDetails)
                        {
                            if (!string.IsNullOrEmpty(datestring))
                                inv.InventoryBatches = inv.InventoryBatches.Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate).ToList();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, traderItemId, locationId, timeZone, datestring);
            }

            return traderItem;
        }

        public TraderItem GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderItems.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderItem();
            }
        }

        public List<TraderItem> GetTraderItemsDomainSkuLocation(string sku, int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, sku, locationId, domainId);
                var lstItems = DbContext.TraderItems.Where(q =>
                    q.IsSold &&
                    q.Domain.Id == domainId
                    && (q.Locations.Select(l => l.Id).Contains(locationId) || locationId == 0)
                    && q.SKU.ToLower().Contains(sku.ToLower().Trim())).ToList();

                return lstItems;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, sku, locationId, domainId);
                return new List<TraderItem>();
            }
        }

        public DataTablesResponse GetTraderItemsFindSkuLocaion(IDataTablesRequest requestModel, string sku, int locationId, int groupId, int domainId, bool isNoneInventory)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, sku, locationId, domainId);
                var query = DbContext.TraderItems.Where(s => s.Domain.Id == domainId);
                int totalRecords;

                #region Filters

                if (!string.IsNullOrEmpty(sku))
                {
                    query = query.Where(s => s.SKU.Contains(sku) || s.Name.Contains(sku));
                }
                if (groupId > 0)
                {
                    query = query.Where(s => s.Group.Id == groupId);
                }
                if (locationId > 0)
                {
                    if (isNoneInventory)
                    {
                        query = query.Where(s => !s.InventoryDetails.Any(x => x.Location.Id == locationId));
                    }
                    else
                    {
                        query = query.Where(s => s.InventoryDetails.Any(x => x.Location.Id == locationId));
                    }
                }
                totalRecords = query.Count();

                #endregion Filters

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
                            orderByString = "Id asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(s => new
                {
                    s.Id,
                    s.SKU,
                    ImageUri = s.ImageUri.ToUriString(Enums.FileTypeEnum.Image, "T"),
                    s.Name,
                    Group = s.Group?.Name ?? ""
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, sku, locationId, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        public List<ProductUnit> GetUnitByTraderItem(int itemId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId);
                var lstUnits = DbContext.ProductUnits.Where(q => q.Item.Id == itemId).ToList();
                return lstUnits;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, itemId);
                return new List<ProductUnit>();
            }
        }

        public void UpdateTraderItemLocations(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var items = DbContext.TraderItems.Where(q => q.Domain.Id == domainId).ToList();
                var locations = DbContext.TraderLocations.Where(q => q.Domain.Id == domainId).ToList();
                if (items.Count <= 0) return;
                foreach (var item in items)
                {
                    if (!item.IsActiveInAllLocations) continue;
                    foreach (var locationItem in locations)
                    {
                        if (!item.IsActiveInAllLocations || item.Locations.IndexOf(locationItem) != -1) continue;
                        item.Locations.Add(locationItem);
                        locationItem.Items.Add(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        //UpdateLocationTraderItem(locationItem.Id, item.Id);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
            }
        }

        public List<TraderItem> GetAll(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.TraderItems.Where(q => q.Domain.Id == domainId).OrderBy(s => s.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderItem>();
            }
        }

        public ReturnJsonModel UpdateLocationTraderItem(int locationId, int itemId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, itemId);
                var traderItem = GetById(itemId);
                var traderLocation = new TraderLocationRules(DbContext).GetById(locationId);
                if (traderItem.Locations.IndexOf(traderLocation) >= 0)
                {
                    traderItem.Locations.Remove(traderLocation);
                    traderItem.IsActiveInAllLocations = false;
                    DbContext.Entry(traderItem).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }
                else
                {
                    traderItem.Locations.Add(traderLocation);
                    if (traderItem.Locations.Count == traderItem.Domain.TraderLocations.Count)
                        traderItem.IsActiveInAllLocations = true;
                    DbContext.Entry(traderItem).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }

                result.Object = new TraderItem { IsActiveInAllLocations = traderItem.IsActiveInAllLocations };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, itemId);
                result.msg = ex.Message;
                result.actionVal = 3;
            }

            return result;
        }

        public ReturnJsonModel UpdateLocationVendorTraderItem(int vendorId, int itemId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, vendorId, itemId);
                var traderItem = GetById(itemId);
                if (traderItem.VendorsPerLocation.Any(q => q.Id == vendorId))
                {
                    var vendor = traderItem.VendorsPerLocation.FirstOrDefault(q => q.Id == vendorId);
                    traderItem.VendorsPerLocation.Remove(vendor);
                    vendor.Item = null;
                    DbContext.Entry(vendor).State = EntityState.Deleted;
                    DbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, vendorId, itemId);
                result.msg = ex.Message;
                result.actionVal = 3;
            }

            return result;
        }

        public ReturnJsonModel LocationVendorPrimaryChange(int itemId, int vendorId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId, vendorId);
                var traderItem = GetById(itemId);
                foreach (var item in traderItem.VendorsPerLocation)
                    if (item.Id == vendorId)
                        item.IsPrimaryVendor = true;
                    else
                        item.IsPrimaryVendor = false;
                DbContext.Entry(traderItem).State = EntityState.Modified;
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, itemId, vendorId);
                result.msg = ex.Message;
                result.actionVal = 3;
            }

            return result;
        }

        public void ValidateUniqueSkuAndBarcode(int traderId, int domainId, string sku, string barCode, ref bool existSku, ref bool existBarcode)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderId, domainId, sku, barCode, existSku, existBarcode);
                if (traderId > 0)
                {
                    IQueryable<TraderItem> query = DbContext.TraderItems;
                    if (!string.IsNullOrEmpty(sku))
                        existSku = query.Any(s => s.Id != traderId && s.Domain.Id == domainId && s.SKU == sku);
                    if (!string.IsNullOrEmpty(barCode))
                        existBarcode = query.Any(s => s.Id != traderId && s.Domain.Id == domainId && s.Barcode == barCode);
                }
                else
                {
                    IQueryable<TraderItem> query = DbContext.TraderItems;
                    if (!string.IsNullOrEmpty(sku))
                        existSku = query.Any(s => s.Domain.Id == domainId && s.SKU == sku);
                    if (!string.IsNullOrEmpty(barCode))
                        existBarcode = query.Any(s => s.Domain.Id == domainId && s.Barcode == barCode);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderId, domainId, sku, barCode, existSku, existBarcode);
                existSku = true;
                existBarcode = true;
            }
        }

        public async Task<ReturnJsonModel> SaveTraderItem(TraderItem item, CreateInventoryCustom createInventory,
            int currentLocationId, bool isCurrentLocation, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, item, createInventory, currentLocationId, isCurrentLocation);
                bool existSKU = false, existBarcode = false;
                ValidateUniqueSkuAndBarcode(item.Id, item.Domain.Id, item.SKU, item.Barcode, ref existSKU, ref existBarcode);
                if (existSKU || existBarcode)
                {
                    result.actionVal = 3;
                    result.msg = ResourcesManager._L("ERROR_MSG_627");
                    return result;
                }
                if (item.Id > 0)
                    result.actionVal = 2;
                var s3Rules = new Azure.AzureStorageRules(DbContext);
                if (item.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(item.ImageUri);
                }
                else
                {
                    var traderItemImageValid = DbContext.TraderItems.Find(item.Id);
                    if (traderItemImageValid.ImageUri != item.ImageUri)
                        s3Rules.ProcessingMediaS3(item.ImageUri);
                }
                var createdBy = DbContext.QbicleUser.Find(userId);

                //QBIC-4921
                if (item.DescriptionText.IsNullOrEmpty())
                {
                    item.DescriptionText = " ";
                }
                item.DescriptionText = item.DescriptionText.TruncateForDisplay(150);

                using (DbContextTransaction transaction = DbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (item.PurchaseAccount != null && item.PurchaseAccount.Id > 0)
                            item.PurchaseAccount = new BKCoANodesRule(DbContext).GetAccountById(item.PurchaseAccount.Id);
                        else item.PurchaseAccount = null;
                        if (item.InventoryAccount != null && item.InventoryAccount.Id > 0)
                            item.InventoryAccount = new BKCoANodesRule(DbContext).GetAccountById(item.InventoryAccount.Id);
                        else item.InventoryAccount = null;
                        if (item.SalesAccount != null && item.SalesAccount.Id > 0)
                            item.SalesAccount = new BKCoANodesRule(DbContext).GetAccountById(item.SalesAccount.Id);
                        else item.SalesAccount = null;
                        if (item.TaxRates != null && item.TaxRates.Where(q => q.Id > 0).Any())
                        {
                            item.TaxRates = item.TaxRates.Where(q => q.Id > 0).ToList();
                            for (int i = 0; i < item.TaxRates.Count; i++)
                            {
                                item.TaxRates[i] = DbContext.TaxRates.Find(item.TaxRates[i].Id);
                            }
                        }
                        else item.TaxRates = null;
                        item.Group = new TraderGroupRules(DbContext).GetById(item.Group.Id);
                        // additional info
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
                            item.AdditionalInfos = additionalInfos;
                        }

                        // resource document
                        if (item.ResourceDocuments.Where(q => q.Id > 0).Any())
                        {
                            item.ResourceDocuments = item.ResourceDocuments.Where(q => q.Id > 0).ToList();
                            for (int i = 0; i < item.ResourceDocuments.Count; i++)
                            {
                                item.ResourceDocuments[i] = DbContext.ResourceDocuments.Find(item.ResourceDocuments[i].Id);
                            }
                        }
                        // location
                        item.Locations = new List<TraderLocation>();
                        if (item.IsActiveInAllLocations) item.Locations = item.Domain.TraderLocations;
                        if (!isCurrentLocation && item.IsActiveInAllLocations && currentLocationId > 0)
                        {
                            item.IsActiveInAllLocations = false;
                            var itemLocation = item.Locations.FirstOrDefault(q => q.Id == currentLocationId);
                            item.Locations.Remove(itemLocation);
                        }
                        else if (isCurrentLocation && !item.IsActiveInAllLocations && currentLocationId > 0)
                        {
                            item.Locations.Add(new TraderLocationRules(DbContext).GetById(currentLocationId));
                        }

                        //the slider is turned on
                        if (item.InventoryDetails.Count > 0)
                        {
                            foreach (var t in item.InventoryDetails)
                            {
                                t.CreatedBy = createdBy;
                                t.CreatedDate = DateTime.UtcNow;
                                t.LastUpdatedBy = createdBy;
                                t.LastUpdatedDate = DateTime.UtcNow;
                                t.Location = DbContext.TraderLocations.Find(t.Location.Id);
                                t.Item = item;
                            }

                            #region create InventoryDetail for all traderlocations in Domain

                            foreach (var location in item.Domain.TraderLocations)
                            {
                                if (!item.InventoryDetails.Any(s => s.Location.Id == location.Id))
                                {
                                    item.InventoryDetails.Add(new InventoryDetail
                                    {
                                        MinInventorylLevel = 0,
                                        MaxInventoryLevel = 0,
                                        CurrentInventoryLevel = 0,
                                        Location = location,
                                        CreatedBy = createdBy,
                                        CreatedDate = DateTime.UtcNow,
                                        LastUpdatedBy = createdBy,
                                        LastUpdatedDate = DateTime.UtcNow,
                                        Item = item
                                    });
                                }
                            }

                            #endregion create InventoryDetail for all traderlocations in Domain
                        }
                        var inventoryD = new List<ProductUnit>();
                        inventoryD.AddRange(item.Units);
                        item.Units.Clear();
                        var recipes = new List<Recipe>();
                        recipes.AddRange(item.AssociatedRecipes);
                        item.AssociatedRecipes.Clear();
                        bool isEdit = false;

                        if (item.Id == 0) // add new trader item
                        {
                            for (int i = 0; i < item.GalleryItems.Count; i++)
                            {
                                var iGalery = new ProductGalleryItem
                                {
                                    CreatedBy = createdBy,
                                    CreatedDate = DateTime.UtcNow,
                                    FileUri = item.GalleryItems[i].FileUri,
                                    Order = item.GalleryItems[i].Order,
                                    //TraderItem = item
                                };
                                DbContext.Entry(iGalery).State = EntityState.Added;
                                item.GalleryItems[i] = iGalery;
                                s3Rules.ProcessingMediaS3(iGalery.FileUri);
                            }

                            item.CreatedBy = createdBy;
                            item.CreatedDate = DateTime.UtcNow;

                            if (item.VendorsPerLocation.Count > 0)
                                for (var i = 0; i < item.VendorsPerLocation.Count; i++)
                                {
                                    item.VendorsPerLocation[i].Vendor =
                                        DbContext.TraderContacts.Find(item.VendorsPerLocation[i].Vendor.Id);
                                    if (item.VendorsPerLocation[i].Vendor != null)
                                        item.VendorsPerLocation[i].Vendor.InUsed = true;
                                    item.VendorsPerLocation[i].Location =
                                        DbContext.TraderLocations.Find(item.VendorsPerLocation[i].Location.Id);
                                    item.VendorsPerLocation[i].Item = item;
                                }
                            if (item.IsCompoundProduct)
                            {
                                var inventorydetail = item.InventoryDetails.Any()
                                    ? item.InventoryDetails.FirstOrDefault()
                                    : new InventoryDetail();
                                for (int i = 0; i < item.Locations.Count; i++)
                                {
                                    if (item.InventoryDetails.Any(q => q.Location.Id == item.Locations[i].Id) == false)
                                    {
                                        item.InventoryDetails.Add(new InventoryDetail()
                                        {
                                            Id = 0,
                                            AverageCost = inventorydetail.AverageCost,
                                            CreatedDate = DateTime.UtcNow,
                                            CreatedBy = createdBy,
                                            LastUpdatedBy = createdBy,
                                            LastUpdatedDate = DateTime.UtcNow,
                                            CurrentInventoryLevel = inventorydetail.CurrentInventoryLevel,
                                            Item = item,
                                            LatestCost = inventorydetail.LatestCost,
                                            Location = item.Locations[i],
                                            MaxInventoryLevel = inventorydetail.MaxInventoryLevel,
                                            MinInventorylLevel = inventorydetail.MinInventorylLevel,
                                            StorageLocationRef = inventorydetail.StorageLocationRef
                                        });
                                    }
                                }
                            }
                            DbContext.Entry(item).State = EntityState.Added;
                            DbContext.TraderItems.Add(item);
                            DbContext.SaveChanges();
                        }
                        else // edit trader item
                        {
                            isEdit = true;
                            var traderItem = DbContext.TraderItems.Find(item.Id);
                            traderItem.GalleryItems.Clear();
                            traderItem.ResourceDocuments.Clear();
                            traderItem.AdditionalInfos.Clear();
                            DbContext.SaveChanges();
                            traderItem.ResourceDocuments = item.ResourceDocuments;
                            traderItem.AdditionalInfos = item.AdditionalInfos;
                            DbContext.SaveChanges();

                            var galleryItems = new List<ProductGalleryItem>();
                            item.GalleryItems.ForEach(image =>
                            {
                                var iGalery = new ProductGalleryItem
                                {
                                    CreatedBy = createdBy,
                                    CreatedDate = DateTime.UtcNow,
                                    FileUri = image.FileUri,
                                    Order = image.Order,
                                    TraderItem = traderItem
                                };
                                DbContext.Entry(iGalery).State = EntityState.Added;
                                traderItem.GalleryItems.Add(iGalery);

                                s3Rules.ProcessingMediaS3(iGalery.FileUri);
                            });
                            if (item.VendorsPerLocation.Count > 0)
                            {
                                for (var i = 0; i < item.VendorsPerLocation.Count; i++)
                                {
                                    var vendor = DbContext.TraderContacts.Find(item.VendorsPerLocation[i].Vendor.Id);
                                    if (vendor != null) vendor.InUsed = true;
                                    traderItem.VendorsPerLocation.Add(new TraderItemVendor
                                    {
                                        Id = 0,
                                        IsPrimaryVendor = item.VendorsPerLocation[i].IsPrimaryVendor,
                                        Item = traderItem,
                                        Vendor = vendor,
                                        Location = DbContext.TraderLocations.Find(item.VendorsPerLocation[i].Location.Id)
                                    });
                                }
                                DbContext.SaveChanges();
                            }
                            traderItem.Barcode = item.Barcode;
                            traderItem.SKU = item.SKU;
                            if (traderItem.TaxRates != null && traderItem.TaxRates.Any())
                                traderItem.TaxRates.Clear();
                            traderItem.TaxRates = item.TaxRates;
                            traderItem.PurchaseAccount = item.PurchaseAccount;
                            traderItem.SalesAccount = item.SalesAccount;
                            traderItem.InventoryAccount = item.InventoryAccount;
                            traderItem.IsCommunityProduct = item.IsCommunityProduct;
                            traderItem.IsCompoundProduct = item.IsCompoundProduct;
                            traderItem.ImageUri = item.ImageUri;
                            traderItem.IsActiveInAllLocations = item.IsActiveInAllLocations;
                            if (traderItem.IsActiveInAllLocations)
                            {
                                traderItem.Locations.Clear();
                                DbContext.SaveChanges();
                                traderItem.Locations = traderItem.Domain.TraderLocations;
                                DbContext.Entry(traderItem).State = EntityState.Modified;
                                DbContext.SaveChanges();
                            }

                            if (!isCurrentLocation && traderItem.Locations.Any(q => q.Id == currentLocationId) &&
                                currentLocationId > 0)
                            {
                                var locationItem = traderItem.Locations.FirstOrDefault(q => q.Id == currentLocationId);
                                traderItem.Locations.Remove(locationItem);
                            }
                            else if (isCurrentLocation && traderItem.Locations.All(q => q.Id != currentLocationId) &&
                                     currentLocationId > 0)
                            {
                                traderItem.Locations.Add(new TraderLocationRules(DbContext).GetById(currentLocationId));
                            }

                            if (traderItem.IsCompoundProduct)
                            {
                                var inventorydetail = traderItem.InventoryDetails.Any()
                                    ? traderItem.InventoryDetails.FirstOrDefault()
                                    : new InventoryDetail();
                                for (int i = 0; i < traderItem.Locations.Count; i++)
                                {
                                    if (traderItem.InventoryDetails.Any(q => q.Location.Id == traderItem.Locations[i].Id) == false)
                                    {
                                        traderItem.InventoryDetails.Add(new InventoryDetail()
                                        {
                                            Id = 0,
                                            AverageCost = inventorydetail.AverageCost,
                                            CreatedDate = DateTime.UtcNow,
                                            CreatedBy = traderItem.CreatedBy,
                                            LastUpdatedBy = traderItem.CreatedBy,
                                            LastUpdatedDate = DateTime.UtcNow,
                                            CurrentInventoryLevel = inventorydetail.CurrentInventoryLevel,
                                            Item = traderItem,
                                            LatestCost = inventorydetail.LatestCost,
                                            Location = traderItem.Locations[i],
                                            MaxInventoryLevel = inventorydetail.MaxInventoryLevel,
                                            MinInventorylLevel = inventorydetail.MinInventorylLevel,
                                            StorageLocationRef = inventorydetail.StorageLocationRef
                                        });
                                    }
                                }
                            }
                            // If the slider is turned off, then all InventoryDetails for the item at every location are deleted
                            if (item.InventoryDetails.Count() == 0)
                            {
                                List<InventoryDetail> removeInvDetails = new List<InventoryDetail>();
                                foreach (var location in traderItem.Locations)
                                {
                                    if (traderItem.InventoryDetails.Any(i => i.Location.Id == location.Id))
                                    {
                                        var removedInventoryDetail = traderItem.InventoryDetails.FirstOrDefault(i => i.Location.Id == currentLocationId);
                                        if (removedInventoryDetail != null && !removedInventoryDetail.InventoryBatches.Any())
                                            removeInvDetails.Add(removedInventoryDetail);
                                    }
                                }
                                DbContext.InventoryDetails.RemoveRange(removeInvDetails);
                                DbContext.SaveChanges();
                            }

                            if (traderItem.InventoryDetails.Any(q =>
                                q.Location != null && item.InventoryDetails.Count > 0 &&
                                q.Location.Id == item.InventoryDetails[0].Location.Id))
                            {
                                for (var i = 0; i < traderItem.InventoryDetails.Count; i++)
                                    if (traderItem.InventoryDetails[i].Location != null &&
                                        traderItem.InventoryDetails[i].Location.Id == item.InventoryDetails[0].Location.Id)
                                    {
                                        traderItem.InventoryDetails[i].MaxInventoryLevel = item.InventoryDetails[0].MaxInventoryLevel;
                                        traderItem.InventoryDetails[i].MinInventorylLevel = item.InventoryDetails[0].MinInventorylLevel;
                                    }
                            }
                            else if (item.InventoryDetails.Count > 0)
                            {
                                item.InventoryDetails[0].Item = traderItem;
                                DbContext.InventoryDetails.Add(item.InventoryDetails[0]);
                                DbContext.Entry(item.InventoryDetails[0]).State = EntityState.Added;
                                DbContext.SaveChanges();
                            }
                            traderItem.IsCompoundProduct = item.IsCompoundProduct;
                            traderItem.Name = item.Name;
                            traderItem.Description = item.Description;
                            traderItem.DescriptionText = item.DescriptionText;
                            traderItem.IsActiveInAllLocations = item.IsActiveInAllLocations;
                            traderItem.IsBought = item.IsBought;
                            traderItem.IsSold = item.IsSold;
                            traderItem.Group = item.Group;
                            DbContext.Entry(traderItem).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            if (traderItem.InventoryDetails.Count > 0 && !traderItem.IsCompoundProduct)
                            {
                                for (int i = 0; i < traderItem.InventoryDetails.Count; i++)
                                {
                                    traderItem.InventoryDetails[i].CurrentRecipe = null;
                                }
                            }
                            else if (traderItem.InventoryDetails.Count > 0 && traderItem.IsCompoundProduct && traderItem.AssociatedRecipes.Count > 0)
                            {
                                for (int i = 0; i < traderItem.InventoryDetails.Count; i++)
                                {
                                    traderItem.InventoryDetails[i].CurrentRecipe = traderItem.AssociatedRecipes.FirstOrDefault(q => q.IsCurrent);
                                }
                            }
                            DbContext.SaveChanges();
                        }

                        var traderItemLast = DbContext.TraderItems.Find(item.Id);

                        if (inventoryD.Count > 0)
                        {
                            var units = inventoryD.OrderByDescending(q => q.Id).ToList();
                            for (var i = 0; i < units.Count; i++)
                                if (units[i].Id.ToString().StartsWith("99999"))
                                {
                                    var id = units[i].Id;
                                    units[i].Id = 0;
                                    units[i].CreatedDate = DateTime.UtcNow;
                                    units[i].CreatedBy = createdBy;
                                    if (units[i].ParentUnit != null &&
                                        !units[i].ParentUnit.Id.ToString().StartsWith("99999"))
                                        units[i].ParentUnit = DbContext.ProductUnits.Find(units[i].ParentUnit.Id);
                                    else
                                        units[i].ParentUnit = null;
                                    units[i].Item = traderItemLast;
                                    DbContext.Entry(units[i]).State = EntityState.Added;
                                    DbContext.ProductUnits.Add(units[i]);
                                    DbContext.SaveChanges();
                                    for (var j = 0; j < units.Count; j++)
                                        if (i != j && units[j].ParentUnit != null && units[j].ParentUnit.Id == id)
                                            units[j].ParentUnit = units[i];
                                    if (recipes.Count > 0)
                                    {
                                        for (int k = 0; k < recipes.Count; k++)
                                        {
                                            if (recipes[k].Ingredients.Count > 0)
                                            {
                                                for (int l = 0; l < recipes[k].Ingredients.Count; l++)
                                                {
                                                    if (recipes[k].Ingredients[l].Unit.Id == id)
                                                        recipes[k].Ingredients[l].Unit = units[i];
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (units[i].Id > 0)
                                {
                                    var unitUpdate = DbContext.ProductUnits.Find(units[i].Id);
                                    if (units[i].ParentUnit != null &&
                                        !units[i].ParentUnit.Id.ToString().StartsWith("99999"))
                                        unitUpdate.ParentUnit = DbContext.ProductUnits.Find(units[i].ParentUnit.Id);
                                    else unitUpdate.ParentUnit = null;
                                    unitUpdate.IsActive = units[i].IsActive;
                                    unitUpdate.IsPrimary = units[i].IsPrimary;
                                    unitUpdate.MeasurementType = units[i].MeasurementType;
                                    DbContext.Entry(unitUpdate).State = EntityState.Modified;
                                    DbContext.SaveChanges();
                                }

                            DbContext.Entry(traderItemLast).State = EntityState.Modified;
                            DbContext.SaveChanges();

                            //Init InitialInventory by localtion
                            if (createInventory.LocationId > 0 && createInventory.Quantity > 0 && createInventory.Unitcost > 0)
                            {
                                var inventoryLocation = DbContext.TraderLocations.FirstOrDefault(s => s.Id == createInventory.LocationId);
                                await new TraderInventoryRules(DbContext).InitialInventory(
                                    traderItemLast.CreatedBy, traderItemLast, inventoryLocation, createInventory.Quantity, createInventory.Unitcost);
                            }
                        }

                        // recipes
                        if (recipes.Count > 0)
                        {
                            foreach (var traderItemAssociatedRecipe in traderItemLast.AssociatedRecipes)
                            {
                                DbContext.Ingredients.RemoveRange(traderItemAssociatedRecipe.Ingredients);
                                traderItemAssociatedRecipe.Ingredients.Clear();
                                DbContext.SaveChanges();
                            }
                            for (var i = 0; i < recipes.Count; i++)
                            {
                                //recipes[i].Id = 0;
                                recipes[i].ParentItem = traderItemLast;
                                recipes[i].CreatedBy = createdBy;
                                var resultRec = (Recipe)SaveRecipe(recipes[i], traderItemLast.Id, userId).Object;
                                if (resultRec != null && resultRec.Id > 0)
                                    recipes[i] = new TraderRecipeRules(DbContext).GetById(recipes[i].Id);
                            }
                        }
                        // apply  inventory detail
                        if (recipes.Count > 0 && recipes.FirstOrDefault(q => q.IsCurrent) == null)
                        {
                            var recipeCurrent = DbContext.Recipes.Find(recipes[0].Id);
                            recipeCurrent.IsCurrent = true;
                            DbContext.SaveChanges();
                        }

                        if (traderItemLast.AssociatedRecipes.Any(q => q.IsCurrent) && traderItemLast.InventoryDetails.Any(q => q.Location.Id == currentLocationId) && isEdit)
                        {
                            var inventoryCurrent = traderItemLast.InventoryDetails.FirstOrDefault(q => q.Location.Id == currentLocationId);
                            inventoryCurrent.CurrentRecipe = traderItemLast.AssociatedRecipes.FirstOrDefault(q => q.IsCurrent);
                            DbContext.SaveChanges();
                        }
                        else if (traderItemLast.AssociatedRecipes.Count > 0 && traderItemLast.InventoryDetails.Count > 0 && !isEdit)
                        {
                            for (int i = 0; i < traderItemLast.InventoryDetails.Count; i++)
                            {
                                traderItemLast.InventoryDetails[i].CurrentRecipe =
                                    traderItemLast.AssociatedRecipes.FirstOrDefault(q => q.IsCurrent);
                            }
                        }

                        result.msgId = item.Id.ToString();
                        transaction.Commit();

                        //if (traderItemLast.IsSold)
                        traderItemLast.UpdateCatalogPricingBasedOnTaxEvents();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.actionVal = 3;
                        LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                        result.msg = ResourcesManager._L("ERROR_MSG_EXCEPTION_SYSTEM");
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, item, createInventory, currentLocationId, isCurrentLocation);
                result.result = false;
                result.actionVal = 3;
                result.msg = e.Message;
            }

            return result;
        }

        public ReturnJsonModel SaveRecipe(Recipe recipe, int traderItemId, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, recipe, traderItemId);

                var traderItem = GetById(traderItemId);
                var inventoryDetail = traderItem.InventoryDetails.FirstOrDefault();

                if (recipe.Ingredients != null && recipe.Ingredients.Count > 0)
                    for (var i = 0; i < recipe.Ingredients.Count; i++)
                    {
                        recipe.Ingredients[i].Id = 0;
                        if (recipe.Ingredients[i].Id > 0 && !recipe.Ingredients[i].Id.ToString().StartsWith("99999"))
                        {
                            var ing = DbContext.Ingredients.Find(recipe.Ingredients[i].Id);
                            if (ing == null) continue;

                            ing.Quantity = recipe.Ingredients[i].Quantity;
                            recipe.Ingredients[i] = ing;
                        }
                        else
                        {
                            recipe.Ingredients[i].Id = 0;
                            if (recipe.Ingredients[i].SubItem != null && recipe.Ingredients[i].SubItem.Id > 0)
                                recipe.Ingredients[i].SubItem =
                                    DbContext.TraderItems.Find(recipe.Ingredients[i].SubItem.Id);
                            else recipe.Ingredients[i].SubItem = traderItem;
                        }

                        recipe.Ingredients[i].ParentRecipe = recipe;
                        recipe.Ingredients[i].Unit = DbContext.ProductUnits.Find(recipe.Ingredients[i].Unit.Id);
                    }

                if (recipe.Id > 0 && !recipe.Id.ToString().StartsWith("99999"))
                {
                    var rec = DbContext.Recipes.Find(recipe.Id);
                    if (rec != null)
                    {
                        rec.Ingredients = recipe.Ingredients;
                        if (rec.Ingredients == null) rec.Ingredients = new List<Ingredient>();
                        for (int i = 0; i < rec.Ingredients.Count; i++)
                        {
                            rec.Ingredients[i].ParentRecipe = rec;
                        }
                        rec.IsActive = recipe.IsActive;
                        rec.IsCurrent = recipe.IsCurrent;
                        rec.Name = recipe.Name;
                    }
                    DbContext.SaveChanges();
                    recipe = rec;
                }
                else
                {
                    recipe.CreatedBy = DbContext.QbicleUser.Find(userId);
                    recipe.Id = 0;
                    recipe.CreatedDate = DateTime.UtcNow;
                    recipe.ParentItem = traderItem;
                    traderItem.AssociatedRecipes.Add(recipe);
                    DbContext.SaveChanges();
                }
                if (inventoryDetail != null && recipe != null && recipe.IsCurrent)
                {
                    inventoryDetail.CurrentRecipe = recipe;
                    DbContext.SaveChanges();
                }
                result.Object = new Recipe
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    IsActive = recipe.IsActive,
                    Ingredients = (recipe.Ingredients ?? new List<Ingredient>()).Select(q => new Ingredient
                    {
                        Id = q.Id,

                        Quantity = q.Quantity,
                        SubItem = new TraderItem { Id = q.SubItem.Id }
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, recipe, traderItemId);
                result.msg = ex.Message;
                result.actionVal = 3;
            }

            return result;
        }

        public ReturnJsonModel ChangeUnitName(int unitId, string unitNewName, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, unitId, unitNewName, userId);

                var unitItem = DbContext.ProductUnits.FirstOrDefault(p => p.Id == unitId);
                var _item = unitItem.Item;
                if (_item.Units.Any(p => p.Name.Equals(unitNewName)))
                {
                    result.result = false;
                    result.msg = "The Unit name already exists for this item.";
                    return result;
                }

                if (unitItem == null)
                {
                    result.result = false;
                    result.msg = "Cannot find the Unit to edit.";
                }
                else
                {
                    unitItem.Name = unitNewName;
                    DbContext.Entry(unitItem).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    result.result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, unitId, unitNewName);
                result.result = false;
                result.msg = ex.ToString();
                return result;
            }
        }

        public Select2GroupedModel Select2TraderItemsByLocationId(int domainId, string keyword, int locationId = 0, bool iSell = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, locationId, keyword);
                IQueryable<TraderItem> query = null;
                if (iSell)
                    query = DbContext.TraderItems.Where(q => q.Domain.Id == domainId && (locationId == 0 || (locationId > 0 && q.Locations.Any(s => s.Id == locationId))) && q.IsSold);
                else
                    query = DbContext.TraderItems.Where(q => q.Domain.Id == domainId && (locationId == 0 || (locationId > 0 && q.Locations.Any(s => s.Id == locationId))) && q.IsBought);
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Name.Contains(keyword) || s.Barcode.Contains(keyword) || s.SKU.Contains(keyword));
                }
                var items = query.OrderBy(s => s.Name).ToList();
                var returnItems = items.GroupBy(g => g.Group).Select(
                    s => new Select2GroupedOption
                    {
                        text = s.Key.Name,
                        children = s.Select(op => new Select2Option { id = op.Id.ToString(), text = op.Name }).ToList()
                    }).OrderBy(n => n.text).ToList();
                var select2data = new Select2GroupedModel();
                select2data.results = returnItems;
                select2data.pagination = new Select2Pagination { more = false };
                return select2data;
            }
            catch (Exception ex)
            {
                LogManager.Debug(MethodBase.GetCurrentMethod(), ex.Message, null, null, domainId, locationId, keyword);
                return new Select2GroupedModel();
            }
        }

        public B2bItemDetailModel ItemSelectedById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var traderItem = DbContext.TraderItems.Find(id);
                if (traderItem == null)
                    return new B2bItemDetailModel();
                B2bItemDetailModel b2BItem = new B2bItemDetailModel();
                b2BItem.ItemId = traderItem.Id;
                b2BItem.ItemName = traderItem.Name;
                b2BItem.SKU = traderItem.SKU;
                b2BItem.Units = traderItem.Units.Select(u => new Select2Option { id = u.Id.ToString(), text = u.Name, selected = u.IsBase }).ToList();
                b2BItem.Locations = traderItem.Locations.Select(u => new Select2Option { id = u.Id.ToString(), text = u.Name }).ToList();
                b2BItem.StringTaxRates = traderItem.TaxRates.Select(t => t.Name).ToList();
                return b2BItem;
            }
            catch (Exception ex)
            {
                LogManager.Debug(MethodBase.GetCurrentMethod(), ex.Message, null, null, id);
                return new B2bItemDetailModel();
            }
        }

        /// <summary>
        /// The process must ensure that there is an InventoryDetail for item at the Location.
        /// If the InventoryDetail does not exist, the process must create it.
        /// The Item may not be Active at the Location. If they are not active they must be made active
        /// </summary>
        /// <param name="item">TraderItem</param>
        /// <param name="locationId">locationId</param>
        /// <returns></returns>
        public bool CheckingProcessTraderItemByLocationValid(TraderItem item, TraderLocation location)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, item, location);

                if (!item.Locations.Any(s => s.Id == location.Id))
                {
                    item.Locations.Add(location);
                }
                if (!item.InventoryDetails.Any(s => s.Location.Id == location.Id))
                {
                    item.InventoryDetails.Add(new InventoryDetail
                    {
                        MinInventorylLevel = 0,
                        MaxInventoryLevel = 0,
                        CurrentInventoryLevel = 0,
                        Location = location,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedBy = item.CreatedBy,
                        LastUpdatedDate = DateTime.UtcNow,
                        Item = item
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Debug(MethodBase.GetCurrentMethod(), ex.Message, null, null, item, location);
                return false;
            }
        }

        public List<TraderItem> GetListDeliveryChargeItem(int locationId)
        {
            var lstDeliveryChargeItem = new List<TraderItem>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);

                var _location = DbContext.TraderLocations.Find(locationId);
                if (_location == null)
                    return new List<TraderItem>();

                var temp = DbContext.TraderItems.Where(t => t.IsSold && !t.IsBought).ToList();
                lstDeliveryChargeItem = temp.Where(t => t.Locations.Contains(_location)
                                            && (t.InventoryDetails == null || t.InventoryDetails.Count <= 0)).ToList();
                return lstDeliveryChargeItem;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return new List<TraderItem>();
            }
        }

        public TraderItem GetItemByDomainIdAndSku(int domainId, string sku)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, sku, domainId);
                return DbContext.TraderItems.FirstOrDefault(s => s.Domain.Id == domainId && s.SKU == sku);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, sku, domainId);
                return new TraderItem();
            }
        }

        private Expression<Func<DateTime, int>> GetWeekNumberExpress =
            d => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(d, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);

        public object GetDataChartViewTrend(int domainId, int itemId = 0, int unitId = 0, int locationId = 0, bool isGenSystem = true, string datestring = "", string timeZone = "",
            string datetimeFormat = "")
        {
            var data = new object();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.UtcNow;
            Func<DateTime, int> GetWeekNumber = GetWeekNumberExpress.Compile();
            try
            {
                if (datestring != null && !string.IsNullOrEmpty(datestring.Trim()))
                {
                    if (!datestring.Contains('-'))
                    {
                        datestring += "-";
                    }

                    datestring.ConvertDaterangeFormat(datetimeFormat, timeZone, out startDate, out endDate);
                    startDate.AddTicks(1);
                    endDate.AddDays(1).AddTicks(-1);
                }
                var rangeDate = endDate - startDate;
                var rangeYearDate = startDate.AddYears(1) - startDate;
                var rangeMonthDate = startDate.AddMonths(1) - startDate;

                #region config unit

                var scaleUnit = (decimal)1;
                var traderItem = GetById(itemId);
                var newUnit = traderItem.Units.FirstOrDefault(q => q.Id == unitId);
                if (newUnit != null)
                {
                    if (newUnit.QuantityOfBaseunit <= 0) newUnit.QuantityOfBaseunit = 1;
                    scaleUnit = newUnit.QuantityOfBaseunit;
                }

                var currency = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
                var decimalPlace = (int)currency.DecimalPlace;

                #endregion config unit

                #region pre-config query

                var query = from tradBatch in DbContext.InventoryBatches
                            join
                                tradDetail in (from tradDetail in DbContext.InventoryDetails
                                               join
                                               tradItem in DbContext.TraderItems
                                               on tradDetail.Item.Id equals tradItem.Id into ab
                                               from subTradItem in ab.DefaultIfEmpty()
                                               select tradDetail)
                            on tradBatch.InventoryDetail.Id equals tradDetail.Id into ac
                            from subTradBatch in ac.DefaultIfEmpty()
                            where (
                                (!tradBatch.IsInvented || isGenSystem)
                                && tradBatch.OriginalQuantity > 0
                                && subTradBatch.Item.Id == itemId
                                && tradBatch.CreatedDate >= startDate
                                && tradBatch.CreatedDate <= endDate
                                && tradBatch.ParentTransferItem != null
                                && tradBatch.InventoryDetail.Location.Id == locationId
                            )
                            select new
                            {
                                Id = subTradBatch.Item.Id,
                                CreateDate = tradBatch.CreatedDate,
                                OriginalQuantity = tradBatch.OriginalQuantity / scaleUnit,
                                Direction = tradBatch.Direction,
                                IsInvented = tradBatch.IsInvented,
                                Value = tradBatch.OriginalQuantity * tradBatch.CostPerUnit
                            };

                var queryin = from movement in query
                              where movement.Direction == BatchDirection.In
                              select movement;

                var queryout = from movement in query
                               where movement.Direction == BatchDirection.Out
                               select movement;

                #endregion pre-config query

                #region date-range config query

                if (rangeDate.CompareTo(rangeYearDate) > 0)
                {
                    var exIn = from movement in queryin
                               group movement by new
                               {
                                   movement.CreateDate.Year,
                                   movement.CreateDate.Month
                               } into m
                               select new
                               {
                                   ItemId = m.FirstOrDefault().Id,
                                   DateByYear = m.Key.Year,
                                   DateByMonth = m.Key.Month,
                                   AvgIn = m.Average(s => s.Value),
                                   AvgInQuantity = m.Average(s => s.OriginalQuantity)
                               };
                    var exOut = from movement in queryout
                                group movement by new
                                {
                                    movement.CreateDate.Year,
                                    movement.CreateDate.Month
                                } into m
                                select new
                                {
                                    ItemId = m.FirstOrDefault().Id,
                                    DateByYear = m.Key.Year,
                                    DateByMonth = m.Key.Month,
                                    AvgOut = m.Average(s => s.Value),
                                    AvgOutQuantity = m.Average(s => s.OriginalQuantity)
                                };
                    var mergeleft = from movementIn in exIn
                                    join movementOut in exOut
                                    on new { movementIn.DateByYear, movementIn.DateByMonth } equals new { movementOut.DateByYear, movementOut.DateByMonth }
                                    into detail
                                    from d in detail.DefaultIfEmpty()
                                    select new
                                    {
                                        movementIn.ItemId,
                                        movementIn.DateByYear,
                                        movementIn.DateByMonth,
                                        DateByWeek = "",
                                        DateByDay = "",
                                        movementIn.AvgIn,
                                        movementIn.AvgInQuantity,
                                        AvgOut = d.AvgOut == null ? 0 : d.AvgOut,
                                        AvgOutQuantity = d.AvgOutQuantity == null ? 0 : d.AvgOutQuantity
                                    };
                    var mergeright = from movementOut in exOut
                                     join movementIn in exIn
                                     on new { movementOut.DateByYear, movementOut.DateByMonth } equals new { movementIn.DateByYear, movementIn.DateByMonth }
                                     into detail
                                     from d in detail.DefaultIfEmpty()
                                     select new
                                     {
                                         movementOut.ItemId,
                                         movementOut.DateByYear,
                                         movementOut.DateByMonth,
                                         DateByWeek = "",
                                         DateByDay = "",
                                         AvgIn = d.AvgIn == null ? 0 : d.AvgIn,
                                         AvgInQuantity = d.AvgInQuantity == null ? 0 : d.AvgInQuantity,
                                         movementOut.AvgOut,
                                         movementOut.AvgOutQuantity
                                     };
                    var merge = mergeleft.Union(mergeright);

                    data = merge.OrderBy(m => new { m.DateByYear, m.DateByMonth })
                        .Select(e =>
                        new
                        {
                            e.ItemId,
                            e.DateByYear,
                            e.DateByMonth,
                            e.DateByWeek,
                            e.DateByDay,
                            AvgIn = Math.Round(e.AvgIn, decimalPlace),
                            AvgOut = Math.Round(e.AvgOut, decimalPlace),
                            AvgInQuantity = Math.Round(e.AvgInQuantity, decimalPlace),
                            AvgOutQuantity = Math.Round(e.AvgOutQuantity, decimalPlace),
                            Deff = Math.Round(Math.Abs(e.AvgIn - e.AvgOut), decimalPlace),
                            DeffQuantity = Math.Round(Math.Abs(e.AvgOutQuantity - e.AvgInQuantity), decimalPlace)
                        })
                        .ToList();
                }
                else if (rangeDate.CompareTo(rangeMonthDate) > 0)
                {
                    var exIn = queryin.ToList().GroupBy(e => new { e.CreateDate.Year, e.CreateDate.Month, WeekGroup = GetWeekNumber(e.CreateDate) }).Select(m => new
                    {
                        ItemId = m.FirstOrDefault().Id,
                        DateByYear = m.Key.Year,
                        DateByMonth = m.Key.Month,
                        DateByWeek = m.Key.WeekGroup,
                        AvgIn = m.Average(s => s.Value),
                        AvgInQuantity = m.Average(s => s.OriginalQuantity)
                    });
                    var exOut = queryout.ToList().GroupBy(e => new { e.CreateDate.Year, e.CreateDate.Month, WeekGroup = GetWeekNumber(e.CreateDate) }).Select(m => new
                    {
                        ItemId = m.FirstOrDefault().Id,
                        DateByYear = m.Key.Year,
                        DateByMonth = m.Key.Month,
                        DateByWeek = m.Key.WeekGroup,
                        AvgOut = m.Average(s => s.Value),
                        AvgOutQuantity = m.Average(s => s.OriginalQuantity)
                    });
                    var mergeleft = from movementIn in exIn
                                    join movementOut in exOut
                                    on new { movementIn.DateByYear, movementIn.DateByWeek } equals new { movementOut.DateByYear, movementOut.DateByWeek }
                                    into detail
                                    from d in detail.DefaultIfEmpty()
                                    select new
                                    {
                                        movementIn.ItemId,
                                        movementIn.DateByYear,
                                        movementIn.DateByMonth,
                                        movementIn.DateByWeek,
                                        DateByDay = "",
                                        movementIn.AvgIn,
                                        movementIn.AvgInQuantity,
                                        AvgOut = d?.AvgOut ?? 0,
                                        AvgOutQuantity = d?.AvgOutQuantity ?? 0
                                    };
                    var mergeright = from movementOut in exOut
                                     join movementIn in exIn
                                     on new { movementOut.DateByYear, movementOut.DateByWeek } equals new { movementIn.DateByYear, movementIn.DateByWeek }
                                     into detail
                                     from d in detail.DefaultIfEmpty()
                                     select new
                                     {
                                         movementOut.ItemId,
                                         movementOut.DateByYear,
                                         movementOut.DateByMonth,
                                         movementOut.DateByWeek,
                                         DateByDay = "",
                                         AvgIn = d?.AvgIn ?? 0,
                                         AvgInQuantity = d?.AvgInQuantity ?? 0,
                                         movementOut.AvgOut,
                                         movementOut.AvgOutQuantity
                                     };

                    var merge = mergeleft.Union(mergeright);
                    data = merge.OrderBy(m => m.DateByYear).ThenBy(m => m.DateByMonth).ThenBy(m => m.DateByWeek).
                        Select(e =>
                        new
                        {
                            e.ItemId,
                            e.DateByYear,
                            e.DateByMonth,
                            e.DateByWeek,
                            e.DateByDay,
                            AvgIn = Math.Round(e.AvgIn, decimalPlace),
                            AvgOut = Math.Round(e.AvgOut, decimalPlace),
                            AvgInQuantity = Math.Round(e.AvgInQuantity, decimalPlace),
                            AvgOutQuantity = Math.Round(e.AvgOutQuantity, decimalPlace),
                            Deff = Math.Round(Math.Abs(e.AvgIn - e.AvgOut), decimalPlace),
                            DeffQuantity = Math.Round(Math.Abs(e.AvgOutQuantity - e.AvgInQuantity), decimalPlace)
                        })
                        .ToList();
                }
                else
                {
                    var exIn = from movement in queryin
                               group movement by new
                               {
                                   movement.CreateDate.Year,
                                   movement.CreateDate.Month,
                                   movement.CreateDate.Day
                               } into m
                               select new
                               {
                                   ItemId = m.FirstOrDefault().Id,
                                   DateByYear = m.Key.Year,
                                   DateByMonth = m.Key.Month,
                                   DateByDay = m.Key.Day,
                                   AvgIn = m.Average(s => s.Value),
                                   AvgInQuantity = m.Average(s => s.OriginalQuantity)
                               };
                    var exOut = from movement in queryout
                                group movement by new
                                {
                                    movement.CreateDate.Year,
                                    movement.CreateDate.Month,
                                    movement.CreateDate.Day
                                } into m
                                select new
                                {
                                    ItemId = m.FirstOrDefault().Id,
                                    DateByYear = m.Key.Year,
                                    DateByMonth = m.Key.Month,
                                    DateByDay = m.Key.Day,
                                    AvgOut = m.Average(s => s.Value),
                                    AvgOutQuantity = m.Average(s => s.OriginalQuantity)
                                };
                    var mergeleft = from movementIn in exIn
                                    join movementOut in exOut
                                    on new { movementIn.DateByYear, movementIn.DateByMonth, movementIn.DateByDay } equals new { movementOut.DateByYear, movementOut.DateByMonth, movementOut.DateByDay }
                                    into detail
                                    from d in detail.DefaultIfEmpty()
                                    select new
                                    {
                                        movementIn.ItemId,
                                        movementIn.DateByYear,
                                        movementIn.DateByMonth,
                                        DateByWeek = "",
                                        movementIn.DateByDay,
                                        movementIn.AvgIn,
                                        movementIn.AvgInQuantity,
                                        AvgOut = d.AvgOut == null ? 0 : d.AvgOut,
                                        AvgOutQuantity = d.AvgOutQuantity == null ? 0 : d.AvgOutQuantity
                                    };
                    var mergeright = from movementOut in exOut
                                     join movementIn in exIn
                                     on new { movementOut.DateByYear, movementOut.DateByMonth, movementOut.DateByDay } equals new { movementIn.DateByYear, movementIn.DateByMonth, movementIn.DateByDay }
                                     into detail
                                     from d in detail.DefaultIfEmpty()
                                     select new
                                     {
                                         movementOut.ItemId,
                                         movementOut.DateByYear,
                                         movementOut.DateByMonth,
                                         DateByWeek = "",
                                         movementOut.DateByDay,
                                         AvgIn = d.AvgIn == null ? 0 : d.AvgIn,
                                         AvgInQuantity = d.AvgInQuantity == null ? 0 : d.AvgInQuantity,
                                         movementOut.AvgOut,
                                         movementOut.AvgOutQuantity
                                     };
                    var merge = mergeleft.Union(mergeright);
                    data = merge.OrderBy(m => new { m.DateByYear, m.DateByMonth, m.DateByDay }).
                        Select(e =>
                        new
                        {
                            e.ItemId,
                            e.DateByYear,
                            e.DateByMonth,
                            e.DateByWeek,
                            e.DateByDay,
                            AvgIn = Math.Round(e.AvgIn, decimalPlace),
                            AvgOut = Math.Round(e.AvgOut, decimalPlace),
                            AvgInQuantity = Math.Round(e.AvgInQuantity, decimalPlace),
                            AvgOutQuantity = Math.Round(e.AvgOutQuantity, decimalPlace),
                            Deff = Math.Round(Math.Abs(e.AvgIn - e.AvgOut), decimalPlace),
                            DeffQuantity = Math.Round(Math.Abs(e.AvgOutQuantity - e.AvgInQuantity), decimalPlace)
                        })
                        .ToList();
                }

                #endregion date-range config query

                return data;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}