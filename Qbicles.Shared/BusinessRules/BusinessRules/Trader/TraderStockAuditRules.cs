using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderStockAuditRules
    {
        private ApplicationDbContext dbContext;

        public TraderStockAuditRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public StockAudit GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.StockAudits.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new StockAudit();
            }
        }
        public List<StockAudit> GetByLocationId(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                return dbContext.StockAudits.Where(e => e.Location != null && e.Location.Id == locationId && e.Domain.Id == domainId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, domainId);
                return new List<StockAudit>();
            }
        }
        public bool ExistsName(StockAudit stockAudit)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, stockAudit);
                var stockaudit = dbContext.StockAudits.FirstOrDefault(q => q.Domain.Id == stockAudit.Domain.Id && q.Location.Id == stockAudit.Location.Id && q.Name.ToLower() == stockAudit.Name.ToLower() && q.Id != stockAudit.Id);
                return stockaudit != null;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, stockAudit);
                return false;
            }
        }

        public ReturnJsonModel SaveStockAudit(StockAudit stockAudit, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, stockAudit);

                var user = new UserRules(dbContext).GetById(userId);

                if (stockAudit.Id == 0)
                    stockAudit.CreatedBy = user;

                stockAudit.Status = ShiftAuditStatus.Pending;
                stockAudit.StartedDate = DateTime.UtcNow;
                stockAudit.StartedBy = user;
                stockAudit.IsStarted = true;

                if (stockAudit.WorkGroup != null && stockAudit.WorkGroup.Id > 0)
                {
                    stockAudit.WorkGroup = dbContext.WorkGroups.Find(stockAudit.WorkGroup.Id);
                }
                if (stockAudit.Location != null && stockAudit.Location.Id > 0)
                {
                    stockAudit.Location = dbContext.TraderLocations.Find(stockAudit.Location.Id);
                }
                else
                {
                    result.msg = ResourcesManager._L("ERROR_MSG_630");
                    return result;
                }
                stockAudit.CreatedDate = DateTime.UtcNow;
                if (stockAudit.ProductList != null && stockAudit.ProductList.Count > 0)
                {
                    foreach (var item in stockAudit.ProductList)
                    {
                        item.Id = 0;
                        item.Product = dbContext.TraderItems.Find(item.Product.Id);
                        item.StockAudit = stockAudit;
                        item.AuditUnit = dbContext.ProductUnits.Find(item.AuditUnit.Id);
                        item.CreatedBy = stockAudit.CreatedBy;
                        item.CreatedDate = DateTime.UtcNow;
                        if (item.Product?.InventoryDetails != null && item.Product.InventoryDetails.Count > 0)
                        {
                            item.InventoryDetail = item.Product.InventoryDetails.FirstOrDefault(q => q.Location.Id == stockAudit.Location.Id);
                        }
                    }
                }
                if (stockAudit.Id == 0)
                {
                    dbContext.Entry(stockAudit).State = EntityState.Added;
                    dbContext.StockAudits.Add(stockAudit);
                    dbContext.SaveChanges();

                    result.msgId = stockAudit.Id.ToString();
                    result.actionVal = 1;
                }
                else
                {
                    var stockAuditEdited = dbContext.StockAudits.Find(stockAudit.Id);
                    if (stockAuditEdited != null)
                    {
                        dbContext.StockAuditItems.RemoveRange(stockAuditEdited.ProductList);
                        stockAuditEdited.ProductList.Clear();
                        dbContext.SaveChanges();
                        stockAuditEdited.Name = stockAudit.Name;
                        stockAuditEdited.Status = stockAudit.Status;
                        stockAuditEdited.Notes = stockAudit.Notes;
                        stockAuditEdited.Location = stockAudit.Location;
                        stockAuditEdited.WorkGroup = stockAudit.WorkGroup;
                        stockAuditEdited.IsStarted = stockAudit.IsStarted;
                        stockAuditEdited.StartedBy = stockAudit.StartedBy;
                        stockAuditEdited.StartedDate = stockAudit.StartedDate;
                        stockAuditEdited.ProductList = stockAudit.ProductList;
                        if (stockAuditEdited.ProductList != null)
                            foreach (var item in stockAuditEdited.ProductList)
                            {
                                item.Id = 0;
                                item.CreatedBy = stockAuditEdited.CreatedBy;
                                item.StockAudit = stockAuditEdited;
                            }

                        dbContext.Entry(stockAuditEdited).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        result.msgId = stockAuditEdited.Id.ToString();
                    }

                    result.actionVal = 2;

                }

                var stockAuditDb = dbContext.StockAudits.Find(stockAudit.Id);

                if (stockAuditDb?.StockAuditApproval != null)
                    return result;

                if (stockAuditDb == null || stockAuditDb.Status != ShiftAuditStatus.Pending) return result;
                stockAuditDb.WorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var appDef =
                    dbContext.ShiftAuditApprovalDefinitions.FirstOrDefault(w => w.ShiftAuditWorkGroup.Id == stockAuditDb.WorkGroup.Id);

                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    StockAudits = new List<StockAudit> { stockAuditDb },
                    Name = $"Shift  Audit: {stockAuditDb.Name}",
                    Qbicle = stockAuditDb.WorkGroup.Qbicle,
                    Topic = stockAuditDb.WorkGroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    ApprovalReqHistories = new List<ApprovalReqHistory>(),
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader
                };
                approval.ApprovalReqHistories.Add(new ApprovalReqHistory()
                {
                    ApprovalReq = approval,
                    CreatedDate = DateTime.UtcNow,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    UpdatedBy = user
                });
                approval.ActivityMembers.AddRange(stockAuditDb.WorkGroup.Members);
                stockAuditDb.StockAuditApproval = approval;
                stockAuditDb.StockAuditApproval.ApprovalRequestDefinition = appDef;
                dbContext.Entry(stockAuditDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = Notification.NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, stockAudit);
                result.result = false;
                result.actionVal = 3;
                result.msg = e.Message;
                //result.msg = e.Message;
            }


            return result;

        }
        public List<ApprovalStatusTimeline> ShiftAuditApprovalStatusTimeline(int id, string timeZone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var stockAudit = dbContext.StockAudits.Find(id);
                string status = "", icon = "";

                if (stockAudit == null) return timeline;

                foreach (var log in stockAudit.StockAuditApproval.ApprovalReqHistories)
                {
                    switch (log.RequestStatus)
                    {
                        case ApprovalReq.RequestStatusEnum.Pending:
                            status = "Pending Review";
                            icon = "fa fa-info bg-aqua";
                            break;
                        case ApprovalReq.RequestStatusEnum.Reviewed:
                            status = "Pending Approval";
                            icon = "fa fa-truck bg-yellow";
                            break;
                        case ApprovalReq.RequestStatusEnum.Approved:
                            status = "Approved";
                            icon = "fa fa-check bg-green";
                            break;
                        case ApprovalReq.RequestStatusEnum.Denied:
                            status = "Denied";
                            icon = "fa fa-warning bg-red";
                            break;
                        case ApprovalReq.RequestStatusEnum.Discarded:
                            status = "Discarded";
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = status,
                            Icon = icon
                        }
                    );
                }

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timeZone);
            }
            return timeline;
        }

        public ReturnJsonModel UpdateStockAuditItem(int id, decimal closingCount)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, closingCount);
                var stockAuditItem = dbContext.StockAuditItems.Find(id);
                if (stockAuditItem != null)
                {
                    stockAuditItem.ClosingCount = closingCount;
                    stockAuditItem.Variance = stockAuditItem.OpeningCount - stockAuditItem.ClosingCount;
                    dbContext.SaveChanges();
                    result.actionVal = 2;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, closingCount);
                result.actionVal = 3;
                result.msg = ex.Message;
            }

            return result;
        }
        public void ShiftAuditApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var shiftAudit = approval.StockAudits.FirstOrDefault();
                if (shiftAudit == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        shiftAudit.Status = ShiftAuditStatus.Pending;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        shiftAudit.Status = ShiftAuditStatus.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        shiftAudit.Status = ShiftAuditStatus.Approved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        shiftAudit.Status = ShiftAuditStatus.Denied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        shiftAudit.Status = ShiftAuditStatus.Discarded;
                        break;
                }
                dbContext.Entry(shiftAudit).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }

        }

        public ReturnJsonModel FinishStockAudit(StockAudit stockAudit, string status, int domainId, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, stockAudit, status);
                if (stockAudit.Id == 0)
                {
                    result.msg = "Error stock audit do not exists.";
                    return result;
                }

                var user = new UserRules(dbContext).GetById(userId);
                if (stockAudit.Id == 0)
                    stockAudit.CreatedBy = user;
                stockAudit.IsFinished = true;
                stockAudit.FinishedBy = user;
                stockAudit.FinishedDate = DateTime.UtcNow;
                stockAudit.Domain = dbContext.Domains.Find(domainId);

                var stockAuditEdited = dbContext.StockAudits.Find(stockAudit.Id);
                if (stockAuditEdited != null)
                {
                    if (stockAudit.ProductList == null) stockAudit.ProductList = new List<StockAuditItem>();
                    if (!string.IsNullOrEmpty(status))
                        switch (status.Split('_')[0].ToLower())
                        {
                            case "reviewed":
                            case "approved":
                                stockAuditEdited.FinishedBy = stockAudit.FinishedBy;
                                stockAuditEdited.FinishedDate = stockAudit.FinishedDate;
                                stockAuditEdited.IsFinished = true;
                                break;
                            case "discarded":
                                foreach (var stockAuditItem in stockAuditEdited.ProductList)
                                {
                                    stockAuditItem.ClosingCount = 0;
                                }

                                if (status.Split('_').Length > 1)
                                {
                                    stockAuditEdited.FinishedBy = null;
                                    stockAuditEdited.FinishedDate = DateTime.MinValue;
                                    stockAuditEdited.IsFinished = false;
                                }
                                else
                                {
                                    stockAuditEdited.FinishedBy = stockAudit.FinishedBy;
                                    stockAuditEdited.FinishedDate = stockAudit.FinishedDate;
                                    stockAuditEdited.IsFinished = true;
                                }
                                break;
                            case "pending":
                                foreach (var stockAuditItem in stockAuditEdited.ProductList)
                                {
                                    stockAuditItem.ClosingCount = 0;
                                }
                                stockAuditEdited.FinishedBy = null;
                                stockAuditEdited.FinishedDate = DateTime.MinValue;
                                stockAuditEdited.IsFinished = false;
                                break;

                        }
                    if (stockAudit.ProductList.Count > 0)
                    {
                        foreach (var item in stockAudit.ProductList)
                        {
                            if (item.Id <= 0) continue;
                            foreach (var pl in stockAuditEdited.ProductList)
                            {
                                if (pl.Id != item.Id) continue;
                                pl.ClosingCount = item.ClosingCount;
                                pl.Variance = item.Variance;
                            }
                        }
                    }

                    dbContext.Entry(stockAuditEdited).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    result.msgId = stockAuditEdited.Id.ToString();
                }

                result.actionVal = 2;


            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, stockAudit, status);
                result.msg = e.Message;
            }


            return result;

        }

        public StockAuditModel GetStockAuditModel(int id, UserSetting user, int domainId, StockAudit sAudit = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                if (sAudit == null)
                    sAudit = dbContext.StockAudits.FirstOrDefault(e => e.Id == id);

                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                var stockAudit = new StockAuditModel
                {
                    Id = sAudit.Id,
                    Name = sAudit.Name,
                    StartedDate = sAudit.StartedDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateFormat + " hh:mmtt"),
                    FinishedDate = sAudit.FinishedDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateFormat + " hh:mmtt"),
                    WorkGroup = new Models.MicroQbicleStream.BaseModel { Id = sAudit.WorkGroup.Id, Name = sAudit.WorkGroup.Name },
                    IsFinished = sAudit.IsFinished,
                    Status = sAudit.Status,
                    ProductList = new List<StockAuditItemModel>()
                };

                var startDate = sAudit.StartedDate;
                var endDate = sAudit.FinishedDate;
                if (sAudit.IsFinished)
                {
                    startDate = sAudit.StartedDate;
                    endDate = DateTime.UtcNow;
                }

                sAudit.ProductList.ForEach(item =>
                {
                    var sItem = new StockAuditItemModel
                    {
                        Id = item.Id,
                        ProductId = item.Product.Id,
                        Name = item.Product.Name,
                        SKU = item.Product.SKU,
                        Unit = new Models.MicroQbicleStream.BaseModel { Id = item.AuditUnit.Id, Name = item.AuditUnit.Name },
                        OpeningCount = item.OpeningCount.ToDecimalPlace(currencySetting),//Observed Opening
                        ClosingCount = item.ClosingCount.ToDecimalPlace(currencySetting),//Observed Closing
                        //Variance = item.Variance.ToDecimalPlace(currencySetting)
                    };

                    decimal periodIn = 0; decimal periodOut = 0;
                    if (item.InventoryDetail != null)
                    {
                        periodIn = item.InventoryDetail.InventoryBatches.
                                    Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate && q.Direction == BatchDirection.In)
                                    .Sum(q => q.OriginalQuantity);
                        periodOut = item.InventoryDetail.InventoryBatches.
                                    Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate && q.Direction == BatchDirection.Out)
                                    .Sum(q => q.OriginalQuantity);
                    }
                    sItem.PeriodIn = periodIn.ToDecimalPlace(currencySetting);
                    sItem.PeriodOut = periodOut.ToDecimalPlace(currencySetting);

                    //ExpectedClosing = Observed_Opening - (Movement_in_Period.Out - Movement_in_Period.In)
                    var expectedClosing = item.OpeningCount - (periodOut - periodIn);
                    sItem.ExpectedClosing = expectedClosing.ToDecimalPlace(currencySetting);

                    //Variance =  Observed_Closing - Expected_Closing
                    sItem.Variance = (item.ClosingCount - expectedClosing).ToDecimalPlace(currencySetting);

                    stockAudit.ProductList.Add(sItem);
                });

                return stockAudit;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new StockAuditModel();
            }
        }


        public string GetTraderItem2StockAudit(int domainId, int locationId, int workgroupId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, locationId);

                var items = (from item in dbContext.TraderItems
                             where item.Group.Domain.Id == domainId && item.Group.WorkGroupCategories.Any(w => w.Id == workgroupId)
                             && item.Locations.Any(l => l.Id == locationId) || item.IsActiveInAllLocations
                             select item).Select(i => $"<option value='" + i.Id + "|" + i.Name + "|" + i.SKU + "'>" + i.Name + "</option>").ToList();

                var optionStr = string.Join("", items);

                return optionStr;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, locationId);
                return "";
            }
        }



        public object GetStockAuditServerSide(IDataTablesRequest requestModel, string keyword, int workgroupId, int locationId, UserSetting dateTimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, keyword, locationId, workgroupId);



                var stockAudits = dbContext.StockAudits.Where(e => e.Location != null && e.Location.Id == locationId);
                if (workgroupId > 0)
                    stockAudits = stockAudits.Where(e => e.WorkGroup.Id == workgroupId);

                if (!string.IsNullOrEmpty(keyword))
                {
                    stockAudits = stockAudits.Where(e => e.Name.ToLower().Contains(keyword) || e.ProductList.Any(p=> p.Product.Name.ToLower().Contains(keyword)));

                }


                var totalStockAudits = stockAudits.Count();

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
                        case "StartedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "StartedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "FinishedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "FinishedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "WorkGroup":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "WorkGroup.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Items":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "ProductList.Count" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "StartedDate desc";
                            break;
                    }
                }

                stockAudits = stockAudits.OrderBy(orderByString == string.Empty ? "StartedDate desc" : orderByString);
                #endregion

                #region Paging
                var list = stockAudits.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Name,
                    StartedDate = q.StartedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat + " " + dateTimeFormat.TimeFormat),
                    FinishedDate = q.FinishedDate.Year == 0001 ? null : q.FinishedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat + " " + dateTimeFormat.TimeFormat),
                    Workgroup = q.WorkGroup.Name,
                    q.Status,
                    StatusName = q.Status.GetDescription(),
                    Items = q.ProductList.Count,
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalStockAudits, totalStockAudits);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return null;
            }
        }

    }
}
