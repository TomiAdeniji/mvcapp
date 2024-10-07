using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.BusinessRules.Trader
{
    public class TraderMovementRules
    {
        private ApplicationDbContext _db;
        public TraderMovementRules(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
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

        public DataTablesResponse GetMovementAlertSettingDTContent(IDataTablesRequest requestModel, int domainId, int locationId, string timeZone,
            string keysearch, string dateFormat, AlertGroupStatusShown statusShown, string datestring = "")
        {
            var totalRecord = 0;
            var response = new DataTablesResponse(requestModel.Draw, new List<AlertGroup>(), 0, 0);

            if (domainId == 0)
                return response;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, domainId, locationId, timeZone,
                        keysearch, dateFormat, statusShown, datestring);

                var startDate = DateTime.MinValue;
                var endDate = DateTime.UtcNow;
                if (datestring != null && !string.IsNullOrEmpty(datestring.Trim()))
                {
                    if (!datestring.Contains('-'))
                        datestring += "-";

                    HelperClass.ConvertDaterangeFormat(datestring, dateFormat, timeZone, out startDate, out endDate);
                }

                var listAlertGroup = DbContext.AlertGroups.Where(p => p.Location.Id == locationId
                    && p.LastUpdateDate >= startDate && p.LastUpdateDate <= endDate).ToList();

                if (!String.IsNullOrEmpty(keysearch))
                {
                    keysearch = keysearch.ToLower();
                    listAlertGroup = listAlertGroup.Where(p => p.Reference != null && p.Reference.FullRef.Contains(keysearch)).ToList();
                }

                if (statusShown == AlertGroupStatusShown.Enabled)
                {
                    listAlertGroup = listAlertGroup.Where(p => p.AlertConstraints.Any(c => c.IsEnabled)).ToList();
                }
                else if (statusShown == AlertGroupStatusShown.Disabled)
                {
                    listAlertGroup = listAlertGroup.Where(p => p.AlertConstraints.Any(c => !c.IsEnabled)).ToList();
                }

                totalRecord = listAlertGroup.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderString = string.Empty;
                foreach (var sortedColumnItem in sortedColumns)
                {
                    switch (sortedColumnItem.Name)
                    {
                        case "Reference":
                            orderString += orderString != string.Empty ? "," : "";
                            orderString += "Reference.FullRef" + (sortedColumnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderString += orderString != string.Empty ? "," : "";
                            orderString += "LastUpdateDate" + (sortedColumnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderString = "LastUpdateDate asc";
                            break;
                    }
                }

                listAlertGroup = listAlertGroup.OrderBy(orderString == string.Empty ? "Reference asc" : orderString).ToList();
                var lstAlertGroupsShown = listAlertGroup.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var returnListAlertGroups = lstAlertGroupsShown.Select(p => new AlertSettingCustom()
                {
                    alertGroupId = p.Id,
                    Reference = p.Reference?.FullRef ?? "",
                    Date = p.LastUpdateDate.ToString(dateFormat),
                    NoMvnAlert = p.AlertConstraints.FirstOrDefault(q => q.Type == CheckType.NoMovement)?.IsEnabled == true,
                    AccumulationAlert = p.AlertConstraints.FirstOrDefault(q => q.Type == CheckType.MinMaxInventory)?.IsEnabled == true,
                    MinMaxAlert = p.AlertConstraints.FirstOrDefault(q => q.Type == CheckType.InventoryAccumulation)?.IsEnabled == true
                }).ToList();

                //var returnListAlertGroups = new List<AlertSettingCustom>();
                //returnListAlertGroups.Add(new AlertSettingCustom()
                //{
                //    Ref = "#1",
                //    Date = "12/12/2020",
                //    NoMvnAlert = true,
                //    MinMaxAlert = true,
                //    AccumulationAlert = true
                //});
                //returnListAlertGroups.Add(new AlertSettingCustom()
                //{
                //    Ref = "#2",
                //    Date = "03/05/2021",
                //    AccumulationAlert = false,
                //    MinMaxAlert = false,
                //    NoMvnAlert = false
                //});

                return new DataTablesResponse(requestModel.Draw, returnListAlertGroups, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, locationId, timeZone, keysearch, dateFormat, statusShown, datestring);
                return response;
            }

        }

        public AlertGroup GetAlertGroupById(int groupId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, groupId);

                var _alertGroup = DbContext.AlertGroups.Find(groupId);
                if (_alertGroup != null)
                    return _alertGroup;
                return new AlertGroup();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupId);
                return new AlertGroup();
            }
        }

        public List<AlertGroup> GetAlertGroupByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);

                var _lstAlertGroup = DbContext.AlertGroups.Where(p => p.Location.Id == locationId).ToList();
                return _lstAlertGroup;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return new List<AlertGroup>();
            }
        }

        public ReturnJsonModel SetAlertProductGroup(int alertGroupId, List<int> lstTraderGroupIds, string userId)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, lstTraderGroupIds, userId);

                var context = ((IObjectContextAdapter)DbContext).ObjectContext;
                var refreshObj = DbContext.ChangeTracker.Entries().Select(c => c.Entity).ToList();
                context.Refresh(RefreshMode.StoreWins, refreshObj);

                var alertGroup = DbContext.AlertGroups.FirstOrDefault(p => p.Id == alertGroupId);

                if (alertGroup == null)
                {
                    result.result = false;
                    result.msg = "The Alert group does not exist.";
                    return result;
                }
                else
                {
                    var _lstProductGroups = new List<TraderGroup>();
                    foreach (var traderGroupItemId in lstTraderGroupIds)
                    {
                        var traderGroupItem = DbContext.TraderGroups.FirstOrDefault(p => p.Id == traderGroupItemId);
                        if (traderGroupItem != null)
                            _lstProductGroups.Add(traderGroupItem);
                    }
                    alertGroup.ProductGroups.RemoveRange(0, alertGroup.ProductGroups.Count);
                    alertGroup.ProductGroups = _lstProductGroups;
                    alertGroup.LastUpdateDate = DateTime.UtcNow;
                    alertGroup.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                    if (alertGroup.Reference == null)
                        alertGroup.Reference = new TraderReferenceRules(DbContext).GetNewReference(alertGroup.Location.Domain.Id, TraderReferenceType.AlertGroup);
                    DbContext.Entry(alertGroup).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.result = true;
                    result.Object = alertGroup.Id;
                    return result;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, lstTraderGroupIds, userId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                result.Object = ex;
                return result;
            }
        }

        public ReturnJsonModel CreateNewAlertGroup(string userId, int locationId)
        {
            var result = new ReturnJsonModel() { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, locationId);

                var location = DbContext.TraderLocations.Find(locationId);
                if (location == null)
                {
                    result.result = false;
                    result.msg = "The TraderLocation does not exist.";
                    return result;
                }

                var newAlertGroup = new AlertGroup
                {
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = DbContext.QbicleUser.Find(userId),
                    Location = location,
                    Reference = new TraderReferenceRules(DbContext).GetNewReference(location.Domain.Id, TraderReferenceType.AlertGroup)
                };

                DbContext.AlertGroups.Add(newAlertGroup);
                DbContext.Entry(newAlertGroup).State = EntityState.Added;
                DbContext.SaveChanges();

                result.result = true;
                result.Object = newAlertGroup.Id;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, locationId);
                result.result = false;
                result.Object = 0;
                return result;
            }
        }

        public ReturnJsonModel SetNoMovementThresholds(int alertGroupId, string daterangeString, UserSetting userSetting)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, alertGroupId, daterangeString, userSetting);

                var _alertGroup = DbContext.AlertGroups.Find(alertGroupId);
                if (_alertGroup == null)
                {
                    result.result = false;
                    result.msg = "The Alert Group does not exist.";
                    return result;
                }

                var _dateRangeList = daterangeString.Split('-');
                var _startDate = _dateRangeList[0].Trim().ConvertDateFormat(userSetting.DateFormat);
                var _endDate = _dateRangeList[1].Trim().ConvertDateFormat(userSetting.DateFormat);
                _startDate = new DateTime(_startDate.Year, _startDate.Month, _startDate.Day, 0, 0, 0);
                _endDate = new DateTime(_endDate.Year, _endDate.Month, _endDate.Day, 0, 0, 0).AddDays(1).AddTicks(-1);
                daterangeString = _startDate.ToString(userSetting.DateTimeFormat) + " - " + _endDate.ToString(userSetting.DateTimeFormat);

                var startDate = new DateTime();
                var endDate = new DateTime();
                HelperClass.ConvertDaterangeFormat(daterangeString, userSetting.DateTimeFormat, userSetting.Timezone, out startDate, out endDate);
                var user = DbContext.QbicleUser.Find(userSetting.Id);
                var _noMovementConstrain = _alertGroup.AlertConstraints.FirstOrDefault(p => p.Type == CheckType.NoMovement);
                if (_noMovementConstrain == null)
                {
                    _noMovementConstrain = new AlertConstraint
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        BenchmarkStartDate = startDate,
                        BenchmarkEndDate = endDate,
                        Type = CheckType.NoMovement
                    };
                    _alertGroup.AlertConstraints.Add(_noMovementConstrain);
                }
                else
                {
                    _noMovementConstrain.LastUpdateDate = DateTime.UtcNow;
                    _noMovementConstrain.LastUpdatedBy = user;
                    _noMovementConstrain.BenchmarkStartDate = startDate;
                    _noMovementConstrain.BenchmarkEndDate = endDate;
                }
                DbContext.Entry(_alertGroup).State = EntityState.Modified;
                DbContext.SaveChanges();

                var _lstProductGroups = _alertGroup.ProductGroups;
                var _lstItemInventories = DbContext.InventoryDetails.Where(p => p.Location.Id == _alertGroup.Location.Id
                    && p.CreatedDate >= startDate && p.CreatedDate <= endDate).ToList();
                foreach (var productGroupItem in _lstProductGroups)
                {
                    var _lstTraderItems = productGroupItem.Items;
                    foreach (var traderItem in _lstTraderItems)
                    {
                        var hasMovementIn = false;
                        var hasMovementOut = false;
                        decimal gapInNum = 0;
                        decimal gapOutNum = 0;
                        decimal noMovementInDayAmount = 0;
                        decimal noMovementOutDayAmount = 0;
                        var _itemInventory = _lstItemInventories.Where(p => p.Item.Id == traderItem.Id).ToList();
                        for (DateTime dayCheck = startDate; dayCheck <= endDate; dayCheck = dayCheck.AddDays(1))
                        {
                            var _dayBegin = new DateTime(dayCheck.Year, dayCheck.Month, dayCheck.Day, 0, 0, 0);
                            var _dayEnd = _dayBegin.AddDays(1).AddTicks(-1);

                            if (!_itemInventory.Any(p => p.CreatedDate >= _dayBegin && p.CreatedDate <= _dayEnd && p.InventoryBatches.Any(b => b.Direction == BatchDirection.In)))
                            {
                                noMovementInDayAmount++;
                                if ((dayCheck == startDate || (dayCheck != startDate && !hasMovementIn)))
                                    gapInNum++;
                                hasMovementIn = false;
                            }
                            else
                            {
                                hasMovementIn = true;
                            }

                            if (!_itemInventory.Any(p => p.CreatedDate >= _dayBegin && p.CreatedDate <= _dayEnd && p.InventoryBatches.Any(b => b.Direction == BatchDirection.Out)))
                            {
                                noMovementOutDayAmount++;
                                if (dayCheck == startDate || (dayCheck != startDate && !hasMovementOut))
                                    gapOutNum++;
                                hasMovementOut = false;
                            }
                            else
                            {
                                hasMovementOut = true;
                            }
                        }
                        var _averageIn = gapInNum != 0 ? (int)Math.Ceiling(noMovementInDayAmount / gapInNum) : 0;
                        var _averageOut = gapOutNum != 0 ? (int)Math.Ceiling(noMovementOutDayAmount / gapOutNum) : 0;

                        //find or add xref
                        var _itemAlertGroupXref = DbContext.Item_AlertGroup_Xrefs.FirstOrDefault(p => p.Item.Id == traderItem.Id && p.Group.Id == _alertGroup.Id);
                        if (_itemAlertGroupXref == null)
                        {
                            _itemAlertGroupXref = new Item_AlertGroup_Xref
                            {
                                Item = traderItem,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = user,
                                LastUpdateDate = DateTime.UtcNow,
                                LastUpdatedBy = user,
                                Group = _alertGroup,
                                NoMovementInDaysThreshold = _averageIn,
                                NoMovementOutDaysThreshold = _averageOut
                            };

                            DbContext.Item_AlertGroup_Xrefs.Add(_itemAlertGroupXref);
                            DbContext.Entry(_itemAlertGroupXref).State = EntityState.Added;
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            _itemAlertGroupXref.LastUpdateDate = DateTime.UtcNow;
                            _itemAlertGroupXref.LastUpdatedBy = user;
                            _itemAlertGroupXref.NoMovementInDaysThreshold = _averageIn;
                            _itemAlertGroupXref.NoMovementOutDaysThreshold = _averageOut;

                            DbContext.Entry(_itemAlertGroupXref).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                }
                result.result = true;
                return result;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, daterangeString, userSetting);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel SetMinMaxThresholds(int alertGroupId, string daterangeString,
            UserSetting userSetting)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, daterangeString, userSetting);

                var _alertGroup = DbContext.AlertGroups.FirstOrDefault(p => p.Id == alertGroupId);
                if (_alertGroup == null)
                {
                    result.result = false;
                    result.msg = "The Alert Group does not exist.";
                    return result;
                }
                var _lstProductGroups = _alertGroup.ProductGroups;

                var _dateRangeList = daterangeString.Split('-');
                var _startDate = _dateRangeList[0].Trim().ConvertDateFormat(userSetting.DateFormat);
                var _endDate = _dateRangeList[1].Trim().ConvertDateFormat(userSetting.DateFormat);
                _startDate = new DateTime(_startDate.Year, _startDate.Month, _startDate.Day, 0, 0, 0);
                _endDate = new DateTime(_endDate.Year, _endDate.Month, _endDate.Day, 0, 0, 0).AddDays(1).AddTicks(-1);
                daterangeString = _startDate.ToString(userSetting.DateTimeFormat) + " - " + _endDate.ToString(userSetting.DateTimeFormat);

                var startDate = new DateTime();
                var endDate = new DateTime();
                HelperClass.ConvertDaterangeFormat(daterangeString, userSetting.DateTimeFormat, userSetting.Timezone, out startDate, out endDate);
                var user = DbContext.QbicleUser.Find(userSetting.Id);
                var _minMaxConstrain = _alertGroup.AlertConstraints.FirstOrDefault(p => p.Type == CheckType.MinMaxInventory);
                if (_minMaxConstrain == null)
                {
                    _minMaxConstrain = new AlertConstraint();
                    _minMaxConstrain.CreatedBy = user;
                    _minMaxConstrain.CreatedDate = DateTime.UtcNow;
                    _minMaxConstrain.LastUpdateDate = DateTime.UtcNow;
                    _minMaxConstrain.LastUpdatedBy = user;
                    _minMaxConstrain.BenchmarkStartDate = startDate;
                    _minMaxConstrain.BenchmarkEndDate = endDate;
                    _minMaxConstrain.Type = CheckType.MinMaxInventory;
                    _alertGroup.AlertConstraints.Add(_minMaxConstrain);
                }
                else
                {
                    _minMaxConstrain.LastUpdateDate = DateTime.UtcNow;
                    _minMaxConstrain.LastUpdatedBy = user;
                    _minMaxConstrain.BenchmarkStartDate = startDate;
                    _minMaxConstrain.BenchmarkEndDate = endDate;
                }
                DbContext.Entry(_alertGroup).State = EntityState.Modified;
                DbContext.SaveChanges();

                var _listInventoryLog = DbContext.InventoryDetailLogs.Where(p => p.Location.Id == _alertGroup.Location.Id
                    && p.CreatedDate >= startDate && p.CreatedDate <= endDate).ToList();

                foreach (var productGroupItem in _lstProductGroups)
                {
                    var _lstTraderItems = productGroupItem.Items;
                    foreach (var traderItem in _lstTraderItems)
                    {
                        var _lstItemInventoryLog = _listInventoryLog.Where(p => p.Item.Id == traderItem.Id).ToList();
                        decimal minThresholds = 0;
                        decimal maxThresholds = 0;
                        if (_lstItemInventoryLog.Count > 0)
                        {
                            minThresholds = _lstItemInventoryLog.Min(p => p.CurrentInventoryLevel);
                            maxThresholds = _lstItemInventoryLog.Max(p => p.CurrentInventoryLevel);
                        }

                        // Find or add xref
                        var _itemAlertGroupXref = DbContext.Item_AlertGroup_Xrefs.FirstOrDefault(p => p.Item.Id == traderItem.Id && p.Group.Id == _alertGroup.Id);
                        if (_itemAlertGroupXref == null)
                        {
                            _itemAlertGroupXref = new Item_AlertGroup_Xref();
                            _itemAlertGroupXref.Item = traderItem;
                            _itemAlertGroupXref.CreatedDate = DateTime.UtcNow;
                            _itemAlertGroupXref.CreatedBy = user;
                            _itemAlertGroupXref.LastUpdateDate = DateTime.UtcNow;
                            _itemAlertGroupXref.LastUpdatedBy = user;
                            _itemAlertGroupXref.Group = _alertGroup;
                            _itemAlertGroupXref.MinInventoryThreshold = minThresholds;
                            _itemAlertGroupXref.MaxInventoryThreshold = maxThresholds;

                            DbContext.Item_AlertGroup_Xrefs.Add(_itemAlertGroupXref);
                            DbContext.Entry(_itemAlertGroupXref).State = EntityState.Added;
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            _itemAlertGroupXref.LastUpdateDate = DateTime.UtcNow;
                            _itemAlertGroupXref.LastUpdatedBy = user;
                            _itemAlertGroupXref.MinInventoryThreshold = minThresholds;
                            _itemAlertGroupXref.MaxInventoryThreshold = maxThresholds;

                            DbContext.Entry(_itemAlertGroupXref).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                }

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userSetting.Id, daterangeString, userSetting, alertGroupId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel SetAccumulationThresholds(int alertGroupId, string daterangeString, CheckEvent checkperiod, UserSetting userSetting)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, daterangeString, checkperiod, userSetting);

                var _alertGroup = DbContext.AlertGroups.FirstOrDefault(p => p.Id == alertGroupId);
                if (_alertGroup == null)
                {
                    result.result = false;
                    result.msg = "The Alert Group does not exist.";
                    return result;
                }
                var _lstProductGroups = _alertGroup.ProductGroups;

                var _dateRangeList = daterangeString.Split('-');
                var _firstDay = _dateRangeList[0].Trim().ConvertDateFormat(userSetting.DateFormat);
                var _lastDay = _dateRangeList[1].Trim().ConvertDateFormat(userSetting.DateFormat);
                _firstDay = new DateTime(_firstDay.Year, _firstDay.Month, _firstDay.Day, 0, 0, 0);
                _lastDay = new DateTime(_lastDay.Year, _lastDay.Month, _lastDay.Day, 0, 0, 0).AddDays(1).AddTicks(-1);
                daterangeString = _firstDay.ToString(userSetting.DateTimeFormat) + " - " + _lastDay.ToString(userSetting.DateTimeFormat);

                var startDate = new DateTime();
                var endDate = new DateTime();
                HelperClass.ConvertDaterangeFormat(daterangeString, userSetting.DateTimeFormat, userSetting.Timezone, out startDate, out endDate);
                var user = DbContext.QbicleUser.Find(userSetting.Id);
                var _accumulationConstrain = _alertGroup.AlertConstraints.FirstOrDefault(p => p.Type == CheckType.InventoryAccumulation);
                if (_accumulationConstrain == null)
                {
                    _accumulationConstrain = new AlertConstraint
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        BenchmarkStartDate = startDate,
                        BenchmarkEndDate = endDate,
                        CheckEvent = checkperiod,
                        Type = CheckType.InventoryAccumulation
                    };
                    _alertGroup.AlertConstraints.Add(_accumulationConstrain);
                }
                else
                {
                    _accumulationConstrain.LastUpdateDate = DateTime.UtcNow;
                    _accumulationConstrain.LastUpdatedBy = user;
                    _accumulationConstrain.BenchmarkStartDate = startDate;
                    _accumulationConstrain.BenchmarkEndDate = endDate;
                    _accumulationConstrain.CheckEvent = checkperiod;
                }
                DbContext.Entry(_alertGroup).State = EntityState.Modified;
                DbContext.SaveChanges();

                var _lstInventoryDetails = DbContext.InventoryDetails.Where(p => p.Location.Id == _alertGroup.Location.Id
                    && p.CreatedDate >= startDate && p.CreatedDate <= endDate).ToList();
                foreach (var productGroupItem in _lstProductGroups)
                {
                    var _lstTraderItem = productGroupItem.Items;

                    foreach (var traderItem in _lstTraderItem)
                    {
                        var _inventoryDetail = _lstInventoryDetails.Where(p => p.Item.Id == traderItem.Id).ToList();
                        decimal inventoryAccumulation = Decimal.MinValue;
                        // Calculate InventoryAccumulation and get the maximum value
                        if (checkperiod == CheckEvent.Daily)
                        {
                            for (var _startDate = startDate; _startDate <= endDate; _startDate = _startDate.AddDays(1))
                            {
                                var _endDate = _startDate.AddDays(1).AddTicks(-1);
                                var _dailyInventoryDetail = _inventoryDetail.Where(p => p.CreatedDate >= _startDate && p.CreatedDate <= _endDate).ToList();
                                var _batches = new List<Batch>();
                                foreach (var inventoryItem in _dailyInventoryDetail)
                                    _batches.AddRange(inventoryItem.InventoryBatches);

                                var _inBatches = _batches.Where(p => p.Direction == BatchDirection.In).ToList();
                                var _outBatches = _batches.Where(p => p.Direction == BatchDirection.Out).ToList();

                                var _sumIn = _inBatches.Sum(p => p.OriginalQuantity);
                                var _sumOut = _outBatches.Sum(p => p.OriginalQuantity);
                                var _currentThresholds = _sumIn - _sumOut;
                                if (_currentThresholds > inventoryAccumulation)
                                    inventoryAccumulation = _currentThresholds;
                            }
                        }
                        else if (checkperiod == CheckEvent.Weekly)
                        {
                            for (var firstDayCheck = startDate; firstDayCheck <= endDate; firstDayCheck = firstDayCheck.AddDays(1))
                            {
                                var lastDayCheck = firstDayCheck.AddDays(1).AddTicks(-1);
                                var _dailyInventoryDetail = _inventoryDetail.Where(p => p.CreatedDate >= firstDayCheck && p.CreatedDate <= lastDayCheck).ToList();
                                var _batches = new List<Batch>();
                                foreach (var inventoryItem in _dailyInventoryDetail)
                                    _batches.AddRange(inventoryItem.InventoryBatches);

                                var _inBatches = _batches.Where(p => p.Direction == BatchDirection.In).ToList();
                                var _outBatches = _batches.Where(p => p.Direction == BatchDirection.Out).ToList();

                                var _sumIn = _inBatches.Sum(p => p.OriginalQuantity);
                                var _sumOut = _outBatches.Sum(p => p.OriginalQuantity);
                                var _currentThresholds = _sumIn - _sumOut;
                                if (_currentThresholds > inventoryAccumulation)
                                    inventoryAccumulation = _currentThresholds;
                            }
                        }
                        else if (checkperiod == CheckEvent.Month)
                        {
                            var totalMonth = endDate.Month - startDate.Month;
                            for (int monthToAdd = 0; monthToAdd <= totalMonth; monthToAdd++)
                            {
                                var firstDayCheck = startDate.AddMonths(monthToAdd);
                                var lastDayCheck = startDate.AddMonths(monthToAdd + 1).AddTicks(-1);
                                var _dailyInventoryDetail = _inventoryDetail.Where(p => p.CreatedDate >= firstDayCheck && p.CreatedDate <= lastDayCheck).ToList();
                                var _batches = new List<Batch>();
                                foreach (var inventoryItem in _dailyInventoryDetail)
                                    _batches.AddRange(inventoryItem.InventoryBatches);

                                var _inBatches = _batches.Where(p => p.Direction == BatchDirection.In).ToList();
                                var _outBatches = _batches.Where(p => p.Direction == BatchDirection.Out).ToList();

                                var _sumIn = _inBatches.Sum(p => p.OriginalQuantity);
                                var _sumOut = _outBatches.Sum(p => p.OriginalQuantity);
                                var _currentThresholds = _sumIn - _sumOut;
                                if (_currentThresholds > inventoryAccumulation)
                                    inventoryAccumulation = _currentThresholds;
                            }
                        }

                        // Find or add xref
                        var _itemAlertGroupXref = DbContext.Item_AlertGroup_Xrefs.FirstOrDefault(p => p.Item.Id == traderItem.Id && p.Group.Id == _alertGroup.Id);
                        if (_itemAlertGroupXref == null)
                        {
                            _itemAlertGroupXref = new Item_AlertGroup_Xref();
                            _itemAlertGroupXref.Item = traderItem;
                            _itemAlertGroupXref.CreatedDate = DateTime.UtcNow;
                            _itemAlertGroupXref.CreatedBy = user;
                            _itemAlertGroupXref.LastUpdateDate = DateTime.UtcNow;
                            _itemAlertGroupXref.LastUpdatedBy = user;
                            _itemAlertGroupXref.Group = _alertGroup;
                            _itemAlertGroupXref.AccumulationTreshold = inventoryAccumulation;

                            DbContext.Item_AlertGroup_Xrefs.Add(_itemAlertGroupXref);
                            DbContext.Entry(_itemAlertGroupXref).State = EntityState.Added;
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            _itemAlertGroupXref.LastUpdateDate = DateTime.UtcNow;
                            _itemAlertGroupXref.LastUpdatedBy = user;
                            _itemAlertGroupXref.AccumulationTreshold = inventoryAccumulation;

                            DbContext.Entry(_itemAlertGroupXref).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }

                    }
                }
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, daterangeString, checkperiod, userSetting);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        //Check for creating reports
        public void CheckForNoMovementReport(int alertGroupId, DateTime startCheckingTime)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, startCheckingTime);

                var alertGroup = DbContext.AlertGroups.FirstOrDefault(p => p.Id == alertGroupId);
                if (alertGroup != null)
                {
                    var noMovementConstraints = alertGroup.AlertConstraints.FirstOrDefault(p => p.Type == CheckType.NoMovement);
                    var lstProductGroups = alertGroup.ProductGroups ?? new List<TraderGroup>();
                    if (noMovementConstraints != null)
                    {

                        //Create a Movement Report to collect any Report Entries
                        var movementReport = new Report();

                        foreach (var productGroupItem in lstProductGroups)
                        {
                            var lstTraderItems = productGroupItem.Items;
                            foreach (var traderItem in lstTraderItems)
                            {
                                var item_AlertGroup_Xref = DbContext.Item_AlertGroup_Xrefs.FirstOrDefault(p => p.Group.Id == alertGroup.Id && p.Item.Id == traderItem.Id);
                                var inThresholds = item_AlertGroup_Xref.NoMovementInDaysThreshold;
                                var outThresholds = item_AlertGroup_Xref.NoMovementOutDaysThreshold;

                                var _startTime = startCheckingTime;
                                var _endTime = startCheckingTime;
                                if (noMovementConstraints.CheckEvent == CheckEvent.Daily)
                                {
                                    _startTime = new DateTime(startCheckingTime.Year, startCheckingTime.Month, startCheckingTime.Day, 0, 0, 0);
                                    _endTime = _startTime.AddDays(1).AddTicks(-1);


                                }
                                else if (noMovementConstraints.CheckEvent == CheckEvent.Weekly)
                                {
                                    _startTime = startCheckingTime.AddDays(-6);
                                    _startTime = new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, 0, 0, 0);
                                    _endTime = startCheckingTime;

                                }
                                else if (noMovementConstraints.CheckEvent == CheckEvent.Month)
                                {
                                    _startTime = new DateTime(startCheckingTime.Year, startCheckingTime.Month, 1, 0, 0, 0);
                                    _endTime = startCheckingTime;
                                }

                                var noMovementReportEntry = new NoMovementReportEntry()
                                {
                                    Item = traderItem,
                                    AlertGroup = alertGroup,
                                    CreatedDate = DateTime.UtcNow,
                                    ProductGroup = productGroupItem
                                };
                                noMovementReportEntry.NoMovementInDaysThreshold = inThresholds;
                                noMovementReportEntry.NoMovementOutDaysThreshold = outThresholds;

                                var _lstInventoryDetails = DbContext.InventoryDetails.Where(p => p.Item.Id == traderItem.Id).ToList();

                                for (var checkTime = _startTime; checkTime <= _endTime; checkTime = checkTime.AddDays(1))
                                {
                                    var _checkMovementInBegin = new DateTime(checkTime.AddDays(-(inThresholds)).Year, checkTime.AddDays(-(inThresholds)).Month, checkTime.AddDays(-(inThresholds)).Day, 0, 0, 0);
                                    var _checkMovementInEnd = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, 0, 0, 0).AddDays(1).AddTicks(-1);
                                    var _checkMovementOutBegin = new DateTime(checkTime.AddDays(-(outThresholds)).Year, checkTime.AddDays(-(outThresholds)).Month, checkTime.AddDays(-(outThresholds)).Day, 0, 0, 0);
                                    var _checkMovementOutEnd = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, 0, 0, 0).AddDays(1).AddTicks(-1);

                                    // Check to create NoMovement IN Report
                                    if (_lstInventoryDetails.Any(p => p.InventoryBatches.Any(b => b.CreatedDate >= _checkMovementInBegin && b.CreatedDate <= _checkMovementInEnd
                                        && b.Direction == BatchDirection.In)))
                                    {
                                        noMovementReportEntry.IsNoMovementInDaysThresholdOK = true;
                                    }
                                    else
                                    {
                                        noMovementReportEntry.IsNoMovementInDaysThresholdOK = false;
                                        noMovementReportEntry.NoMovementInDateRanges.Add(new Models.Trader.Movement.DateRange()
                                        {
                                            StartDate = _checkMovementInBegin,
                                            EndDate = _checkMovementInEnd
                                        });
                                    }

                                    // Check to create NoMovement OUT Report
                                    if (_lstInventoryDetails.Any(p => p.InventoryBatches.Any(b => b.CreatedDate >= _checkMovementOutBegin && b.CreatedDate <= _checkMovementOutEnd
                                        && b.Direction == BatchDirection.Out)))
                                    {
                                        noMovementReportEntry.IsNoMovementOutDaysThresholdOK = true;
                                    }
                                    else
                                    {
                                        noMovementReportEntry.IsNoMovementOutDaysThresholdOK = false;
                                        noMovementReportEntry.NoMovementOutDateRanges.Add(new Models.Trader.Movement.DateRange()
                                        {
                                            StartDate = _checkMovementOutBegin,
                                            EndDate = _checkMovementOutEnd
                                        });
                                    }

                                    // If no thresholds are exceeded then the NoMovementReportEntry is not saved to the database.
                                    if (!noMovementReportEntry.IsNoMovementInDaysThresholdOK || !noMovementReportEntry.IsNoMovementOutDaysThresholdOK)
                                    {
                                        if (noMovementReportEntry.id > 0)
                                        {
                                            DbContext.Entry(noMovementReportEntry).State = EntityState.Modified;
                                            DbContext.SaveChanges();
                                        }
                                        else
                                        {
                                            DbContext.NoMovementReportEntries.Add(noMovementReportEntry);
                                            DbContext.Entry(noMovementReportEntry).State = EntityState.Added;
                                            DbContext.SaveChanges();
                                        }

                                        movementReport.ReportEntries.Add(noMovementReportEntry);
                                    }
                                }
                            }
                        }

                        // If there are Report Entries finish off setting up the Movement Report
                        if (!movementReport.ReportEntries.IsEmpty())
                        {
                            movementReport.Reference = new TraderReferenceRules(DbContext).GetNewReference(alertGroup.Location.Domain.Id, TraderReferenceType.AlertReport);
                            movementReport.Executiondate = DateTime.UtcNow;
                            movementReport.AlertGroup = alertGroup;
                            DbContext.Reports.Add(movementReport);
                            DbContext.Entry(movementReport).State = EntityState.Added;
                            DbContext.SaveChanges();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, startCheckingTime);
            }
        }

        public void CheckForMinMaxReport(int alertGroupId, DateTime startCheckingTime)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, startCheckingTime);

                var alertGroup = DbContext.AlertGroups.FirstOrDefault(p => p.Id == alertGroupId);
                var lstProductGroups = alertGroup?.ProductGroups ?? new List<TraderGroup>();
                var minMaxConstrains = alertGroup?.AlertConstraints.FirstOrDefault(p => p.Type == CheckType.MinMaxInventory) ?? null;
                if (alertGroup != null && minMaxConstrains != null)
                {
                    // Setup the movement report
                    var movementReport = new Report();

                    foreach (var productGroupItem in lstProductGroups)
                    {
                        var lstTraderItems = productGroupItem.Items;
                        foreach (var traderItem in lstTraderItems)
                        {
                            var item_AlertGroup_Xref = DbContext.Item_AlertGroup_Xrefs.FirstOrDefault(p => p.Group.Id == alertGroup.Id && p.Item.Id == traderItem.Id);
                            var minThresholds = item_AlertGroup_Xref?.MinInventoryThreshold ?? Decimal.MinValue;
                            var maxThresholds = item_AlertGroup_Xref?.MaxInventoryThreshold ?? Decimal.MaxValue;

                            var _startTime = startCheckingTime;
                            var _endTime = startCheckingTime;
                            if (minMaxConstrains.CheckEvent == CheckEvent.Daily)
                            {
                                _startTime = new DateTime(startCheckingTime.Year, startCheckingTime.Month, startCheckingTime.Day, 0, 0, 0);
                                _endTime = _startTime.AddDays(1).AddTicks(-1);


                            }
                            else if (minMaxConstrains.CheckEvent == CheckEvent.Weekly)
                            {
                                _startTime = startCheckingTime.AddDays(-6);
                                _startTime = new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, 0, 0, 0);
                                _endTime = startCheckingTime;

                            }
                            else if (minMaxConstrains.CheckEvent == CheckEvent.Month)
                            {
                                _startTime = new DateTime(startCheckingTime.Year, startCheckingTime.Month, 1, 0, 0, 0);
                                _endTime = startCheckingTime;
                            }

                            for (var checkTime = _startTime; checkTime <= _endTime; checkTime = checkTime.AddDays(1))
                            {
                                var _checkBegin = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, 0, 0, 0);
                                var _checkEnd = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, 0, 0, 0).AddDays(1).AddTicks(-1);

                                var minMaxMovementReportEntry = new MinMaxInventoryReportEntry()
                                {
                                    Item = traderItem,
                                    ProductGroup = productGroupItem,
                                    AlertGroup = alertGroup,
                                    CreatedDate = DateTime.UtcNow,
                                    MaxInventoryThreshold = maxThresholds,
                                    MinInventoryThreshold = minThresholds,
                                    IsMinInventoryThresholdOK = true,
                                    IsMaxInventoryThresholdOK = true
                                };

                                var _lstDetailLogs = DbContext.InventoryDetailLogs.Where(p => p.Location.Id == alertGroup.Location.Id
                                    && p.Item.Id == traderItem.Id
                                    && p.CreatedDate >= _checkBegin && p.CreatedDate <= _checkEnd).ToList();

                                if (_lstDetailLogs.Any(p => p.CurrentInventoryLevel < minThresholds))
                                {
                                    minMaxMovementReportEntry.IsMinInventoryThresholdOK = false;
                                    minMaxMovementReportEntry.MinInventoryThresholdDate = checkTime;
                                }

                                if (maxThresholds != minThresholds && _lstDetailLogs.Any(p => p.CurrentInventoryLevel > maxThresholds))
                                {
                                    minMaxMovementReportEntry.IsMaxInventoryThresholdOK = false;
                                    minMaxMovementReportEntry.MaxInventoryThresholdDate = new DateTime();  //?????????? What is this
                                }

                                if (!minMaxMovementReportEntry.IsMaxInventoryThresholdOK || !minMaxMovementReportEntry.IsMinInventoryThresholdOK)
                                {
                                    DbContext.MinMaxInventoryReportEntries.Add(minMaxMovementReportEntry);
                                    DbContext.Entry(minMaxMovementReportEntry).State = EntityState.Added;
                                    DbContext.SaveChanges();

                                    //Add the report entry to the report
                                    movementReport.ReportEntries.Add(minMaxMovementReportEntry);
                                }
                            }
                        }
                    }


                    // If there are Report Entries finish off setting up the Movement Report
                    if (!movementReport.ReportEntries.IsEmpty())
                    {
                        movementReport.Reference = new TraderReferenceRules(DbContext).GetNewReference(alertGroup.Location.Domain.Id, TraderReferenceType.AlertReport);
                        movementReport.Executiondate = DateTime.UtcNow;
                        movementReport.AlertGroup = alertGroup;
                        DbContext.Reports.Add(movementReport);
                        DbContext.Entry(movementReport).State = EntityState.Added;
                        DbContext.SaveChanges();
                    }

                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, startCheckingTime, DateTime.UtcNow);
            }
        }

        public void CheckForAccumulationReport(int alertGroupId, DateTime startCheckingTime)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, startCheckingTime);

                var alertGroup = DbContext.AlertGroups.FirstOrDefault(p => p.Id == alertGroupId);
                var lstProductGroups = alertGroup?.ProductGroups ?? new List<TraderGroup>();
                var accumulationConstrains = alertGroup?.AlertConstraints.FirstOrDefault(p => p.Type == CheckType.InventoryAccumulation) ?? null;
                if (alertGroup != null && accumulationConstrains != null)
                {
                    var movementReport = new Report();

                    foreach (var productGroupItem in lstProductGroups)
                    {
                        var lstTraderItems = productGroupItem.Items;
                        foreach (var traderItem in lstTraderItems)
                        {
                            var item_AlertGroup_Xref = DbContext.Item_AlertGroup_Xrefs.FirstOrDefault(p => p.Group.Id == alertGroup.Id && p.Item.Id == traderItem.Id);
                            var acculationThresholds = item_AlertGroup_Xref?.AccumulationTreshold ?? Decimal.MaxValue;

                            var _startTime = startCheckingTime;
                            var _endTime = startCheckingTime;
                            if (accumulationConstrains.CheckEvent == CheckEvent.Daily)
                            {
                                _startTime = new DateTime(startCheckingTime.Year, startCheckingTime.Month, startCheckingTime.Day, 0, 0, 0);
                                _endTime = _startTime.AddDays(1).AddTicks(-1);


                            }
                            else if (accumulationConstrains.CheckEvent == CheckEvent.Weekly)
                            {
                                _startTime = startCheckingTime.AddDays(-6);
                                _startTime = new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, 0, 0, 0);
                                _endTime = startCheckingTime;

                            }
                            else if (accumulationConstrains.CheckEvent == CheckEvent.Month)
                            {
                                _startTime = new DateTime(startCheckingTime.Year, startCheckingTime.Month, 1, 0, 0, 0);
                                _endTime = startCheckingTime;
                            }

                            var accumulationReportEntry = new AccumulationEntryReport()
                            {
                                Item = traderItem,
                                AlertGroup = alertGroup,
                                AccumulationThreshold = acculationThresholds,
                                CreatedDate = DateTime.UtcNow,
                                IsAccumulationThresholdOK = true,
                                ProductGroup = productGroupItem
                            };

                            var lstInventoryDetails = DbContext.InventoryDetails.Where(p => p.Item.Id == traderItem.Id
                                && p.Location.Id == alertGroup.Location.Id && p.CreatedDate >= _startTime && p.CreatedDate <= _endTime).ToList();
                            var lstInventoryBatches = new List<Batch>();

                            foreach (var inventoryItem in lstInventoryDetails)
                            {
                                lstInventoryBatches.AddRange(inventoryItem.InventoryBatches);
                            }
                            var lstInBatches = lstInventoryBatches.Where(p => p.Direction == BatchDirection.In);
                            var lstOutBatches = lstInventoryBatches.Where(p => p.Direction == BatchDirection.Out);
                            var inQuantity = lstInBatches.Sum(p => p.OriginalQuantity);
                            var outQuantity = lstOutBatches.Sum(p => p.OriginalQuantity);

                            var currentAccumulation = inQuantity - outQuantity;
                            if (currentAccumulation > acculationThresholds)
                            {
                                accumulationReportEntry.IsAccumulationThresholdOK = false;
                                accumulationReportEntry.AccumulationDateRanges = new List<Models.Trader.Movement.DateRange>();
                                accumulationReportEntry.AccumulationDateRanges.Add(new Models.Trader.Movement.DateRange()
                                {
                                    StartDate = _startTime,
                                    EndDate = _endTime
                                });
                                DbContext.AccumulationEntryReports.Add(accumulationReportEntry);
                                DbContext.Entry(accumulationReportEntry).State = EntityState.Added;
                                DbContext.SaveChanges();

                                // Add the accumulation report entry to the movement report
                                movementReport.ReportEntries.Add(accumulationReportEntry);
                            }
                        }
                    }

                    // If there are Report Entries finish off setting up the Movement Report
                    if (!movementReport.ReportEntries.IsEmpty())
                    {
                        movementReport.Reference = new TraderReferenceRules(DbContext).GetNewReference(alertGroup.Location.Domain.Id, TraderReferenceType.AlertReport);
                        movementReport.Executiondate = DateTime.UtcNow;
                        movementReport.AlertGroup = alertGroup;
                        DbContext.Reports.Add(movementReport);
                        DbContext.Entry(movementReport).State = EntityState.Added;
                        DbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, startCheckingTime);
            }
        }

        //Methods for Schedule Job
        public ReturnJsonModel ScheduleNoMovementCheck(int alertGroupId, string userId, CheckEvent checkPeriod = CheckEvent.Daily, bool sendToQueue = true)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, userId, checkPeriod, sendToQueue);

                var checkingTime = DateTime.UtcNow;
                var alertGroup = DbContext.AlertGroups.Find(alertGroupId);
                var alertConstraint = alertGroup?.AlertConstraints?.FirstOrDefault(p => p.Type == CheckType.NoMovement) ?? null;
                if (alertGroup == null || alertConstraint == null)
                {
                    result.result = false;
                    result.msg = "AlertGroup or AlertConstraint does not exist.";
                    return result;
                }

                if (sendToQueue)
                {

                    Task tskHangfire = new Task(async () =>
                    {
                        var job = new TraderMovementJobParamenter()
                        {
                            AlertConstraintId = alertConstraint.Id,
                            EndPointName = "schedulecheckingnomovementalert",
                            SendToQueue = false,
                            UserId = userId
                        };
                        await new QbiclesJob().HangFireExcecuteAsync(job);
                    });
                    tskHangfire.Start();

                    alertConstraint.CheckEvent = checkPeriod;
                    alertConstraint.HangfireJobId = alertConstraint.Id;
                    alertConstraint.IsEnabled = true;
                    alertConstraint.LastUpdateDate = DateTime.UtcNow;
                    alertConstraint.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                    DbContext.Entry(alertConstraint).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    result.Object = alertConstraint.Id;
                    result.result = true;
                    return result;
                }

                CheckForNoMovementReport(alertGroup.Id, checkingTime);

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, userId, checkPeriod, sendToQueue);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel ScheduleMinMaxCheck(int alertGroupId, string userId, CheckEvent checkPeriod = CheckEvent.Daily, bool sendToQueue = true)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, userId, checkPeriod, sendToQueue);

                var checkingTime = DateTime.UtcNow;
                var alertGroup = DbContext.AlertGroups.Find(alertGroupId);
                var alertConstraint = alertGroup?.AlertConstraints?.FirstOrDefault(p => p.Type == CheckType.MinMaxInventory);
                if (alertGroup == null || alertConstraint == null)
                {
                    result.result = false;
                    result.msg = "AlertGroup or AlertConstrain does not exist.";
                    return result;
                }

                if (sendToQueue)
                {
                    Task tskHangfire = new Task(async () =>
                    {
                        var job = new TraderMovementJobParamenter()
                        {
                            AlertConstraintId = alertConstraint.Id,
                            EndPointName = "schedulecheckingminmaxalert",
                            SendToQueue = false,
                            UserId = userId
                        };
                        await new QbiclesJob().HangFireExcecuteAsync(job);
                    });
                    tskHangfire.Start();

                    alertConstraint.CheckEvent = checkPeriod;
                    alertConstraint.HangfireJobId = alertConstraint.Id;
                    alertConstraint.IsEnabled = true;
                    alertConstraint.LastUpdateDate = DateTime.UtcNow;
                    alertConstraint.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                    DbContext.Entry(alertConstraint).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    result.Object = alertConstraint.Id;
                    result.result = true;
                    return result;
                }

                CheckForMinMaxReport(alertGroup?.Id ?? 0, checkingTime);

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, userId, checkPeriod, sendToQueue);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel ScheduleAccumulationCheck(int alertGroupId, string userId, CheckEvent checkPeriod = CheckEvent.Daily, bool sendToQueue = true)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertGroupId, userId, checkPeriod, sendToQueue);

                var checkingTime = DateTime.UtcNow;
                var alertGroup = DbContext.AlertGroups.Find(alertGroupId);
                var alertConstraint = alertGroup?.AlertConstraints?.FirstOrDefault(p => p.Type == CheckType.InventoryAccumulation);
                if (alertGroup == null || alertConstraint == null)
                {
                    result.result = false;
                    result.msg = "AlertGroup or AlertConstraint does not exist.";
                    return result;
                }

                if (sendToQueue)
                {
                    Task tskHangfire = new Task(async () =>
                    {
                        var job = new TraderMovementJobParamenter()
                        {
                            AlertConstraintId = alertConstraint.Id,
                            EndPointName = "schedulecheckingaccumulationalert",
                            SendToQueue = false,
                            UserId = userId
                        };
                        await new QbiclesJob().HangFireExcecuteAsync(job);
                    });
                    tskHangfire.Start();

                    alertConstraint.CheckEvent = checkPeriod;
                    alertConstraint.HangfireJobId = alertConstraint.Id;
                    alertConstraint.IsEnabled = true;
                    alertConstraint.LastUpdateDate = DateTime.UtcNow;
                    alertConstraint.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                    DbContext.Entry(alertConstraint).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    result.Object = alertConstraint.Id;
                    result.result = true;
                    return result;
                }

                CheckForAccumulationReport(alertGroup?.Id ?? 0, checkingTime);

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertGroupId, userId, checkPeriod, sendToQueue);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel RemoveScheduleCheck(int alertConstraintId, string userId)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, alertConstraintId, userId);

                var alertConstraint = DbContext.AlertConstraints.Find(alertConstraintId);
                if (alertConstraint == null)
                {
                    result.result = false;
                    result.msg = "AlertConstraint does not exist.";
                    return result;
                }


                Task tskHangfire = new Task(async () =>
                {
                    var job = new TraderMovementJobParamenter()
                    {
                        AlertConstraintId = alertConstraintId,
                        EndPointName = "removeschedulejob",
                        SendToQueue = false,
                        UserId = userId
                    };
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();


                alertConstraint.HangfireJobId = alertConstraintId;
                alertConstraint.IsEnabled = false;
                alertConstraint.LastUpdateDate = DateTime.UtcNow;
                alertConstraint.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                DbContext.Entry(alertConstraint).State = EntityState.Modified;
                DbContext.SaveChanges();

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertConstraintId, userId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel UpdateItemAlertGroup(Item_AlertGroup_Xref item)
        {
            var result = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, item);

                var alertItem = DbContext.Item_AlertGroup_Xrefs.FirstOrDefault(e => e.id == item.id);
                alertItem.NoMovementInDaysThreshold = item.NoMovementInDaysThreshold;
                alertItem.NoMovementOutDaysThreshold = item.NoMovementOutDaysThreshold;
                alertItem.MinInventoryThreshold = item.MinInventoryThreshold;
                alertItem.MaxInventoryThreshold = item.MaxInventoryThreshold;
                alertItem.AccumulationTreshold = item.AccumulationTreshold;
                DbContext.Entry(alertItem).State = EntityState.Modified;
                DbContext.SaveChanges();
                result.result = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, item);
            }

            return result;
        }
    }
}
