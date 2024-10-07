using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderSpotCountRules
    {
        private ApplicationDbContext dbContext;

        public TraderSpotCountRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public SpotCount GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.SpotCounts.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new SpotCount();
            }
        }

        public DataTablesResponse GetByLocationPagination(int locationId, int domainId, IDataTablesRequest requestModel, UserSetting user, string keyword, string datetime, int[] workgroups, SpotCountStatus[] status)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId, workgroups, requestModel);
                var query = dbContext.SpotCounts.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId);
                int totalSpot = 0;

                #region Filter

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => q.Name.Contains(keyword) || q.Description.Contains(keyword));

                if (workgroups.Count() > 0 && workgroups.Any(w => w != -1))
                    query = query.Where(q => workgroups.Contains(q.Workgroup.Id));

                if (status.Count() > 0 && Enum.IsDefined(typeof(SpotCountStatus), status[0]))
                    query = query.Where(q => status.Contains(q.Status));

                //Filter by dates
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);

                    if (!string.IsNullOrEmpty(datetime.Trim()))
                    {
                        datetime.ConvertDaterangeFormat(user.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                        startDate = startDate.ConvertTimeToUtc(tz);
                        endDate = endDate.ConvertTimeToUtc(tz);
                    }
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);
                }
                totalSpot = query.Count();

                #endregion Filter

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "WorkgroupName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Workgroup.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Description":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Description " + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "CreatedBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.ForeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.SurName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                #endregion Paging

                var dataJson = list.Select(q => new SpotCountModel
                {
                    Id = q.Id,
                    Name = q.Name,
                    Date = q.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString($"{user.DateFormat} {user.TimeFormat}"),
                    WorkgroupName = q.Workgroup?.Name,
                    ItemsCount = q.ProductList != null ? q.ProductList.Count : 0,
                    Description = q.Description,
                    Status = q.Status,
                    CreatedBy = q.CreatedBy.GetFullName()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSpot, totalSpot);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, domainId, workgroups, requestModel);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        public async Task SpotCountApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var spotCount = approval.SpotCounts.FirstOrDefault();
                if (spotCount == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        spotCount.Status = SpotCountStatus.CountStarted;
                        foreach (var item in spotCount.ProductList)
                        {
                            item.Adjustment = 0;
                        }
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        spotCount.Status = SpotCountStatus.CountCompleted;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        spotCount.Status = SpotCountStatus.StockAdjusted;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        spotCount.Status = SpotCountStatus.Discarded;
                        foreach (var item in spotCount.ProductList)
                        {
                            item.Adjustment = 0;
                        }
                        break;
                }
                spotCount.LastUpdatedDate = DateTime.UtcNow;
                spotCount.LastUpdatedBy = approval.ApprovedOrDeniedAppBy;
                dbContext.Entry(spotCount).State = EntityState.Modified;
                dbContext.SaveChanges();

                var spotCountLog = new SpotCountLog
                {
                    AssociatedSpotCount = spotCount,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    Description = spotCount.Description,
                    Domain = spotCount.Domain,
                    LastUpdatedBy = spotCount.LastUpdatedBy,
                    LastUpdatedDate = spotCount.LastUpdatedDate,
                    Location = spotCount.Location,
                    ProductList = spotCount.ProductList,
                    SpotCountApprovalProcess = spotCount.SpotCountApprovalProcess,
                    Status = spotCount.Status,
                    Workgroup = spotCount.Workgroup
                };

                var spotCountProcessLog = new SpotCountProcessLog
                {
                    AssociatedSpotCount = spotCount,
                    AssociatedSpotCountLog = spotCountLog,
                    SpotCountStatus = spotCount.Status,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = approval.ApprovedOrDeniedAppBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.SpotCountProcessLogs.Add(spotCountProcessLog);
                dbContext.Entry(spotCountProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();

                //Create new Transfer and Outgoing when Approved -- SpotCount
                if (spotCount.Status != SpotCountStatus.StockAdjusted) return;
                //Transfer process
                //var transferItems = new List<TraderTransferItem>();
                var negativeAdjustment = new List<TraderTransferItem>();
                var positiveAdjustment = new List<TraderTransferItem>();

                foreach (var item in spotCount.ProductList)
                {
                    item.Adjustment = item.SpotCountValue - item.SavedInventoryCount;
                    if (item.Adjustment > 0)
                    {
                        var transferItem = new TraderTransferItem
                        {
                            Unit = item.CountUnit,
                            QuantityAtPickup = item.Adjustment,
                            QuantityAtDelivery = item.Adjustment,
                            TransactionItem = null,// For a Point-to-Point transfer there is no Tansaction Item
                            TraderItem = item.Product
                        };

                        positiveAdjustment.Add(transferItem);
                    }
                    else if (item.Adjustment < 0)
                    {
                        //Make the quantities positive, but the items are decremented from incentory
                        var transferItem = new TraderTransferItem
                        {
                            Unit = item.CountUnit,
                            QuantityAtPickup = item.Adjustment * -1,
                            QuantityAtDelivery = item.Adjustment * -1,
                            TransactionItem = null,// For a Point-to-Point transfer there is no Tansaction Item
                            TraderItem = item.Product
                        };

                        negativeAdjustment.Add(transferItem);
                    }
                }

                var tradTransfer = new TraderTransfer
                {
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    DestinationLocation = spotCount.Location,
                    OriginatingLocation = spotCount.Location,
                    Workgroup = spotCount.Workgroup,
                    Address = null,
                    Contact = null,
                    Status = TransferStatus.Delivered,
                    Reason = TransferReasonEnum.SpotCountAdjustment,
                    SpotCount = spotCount,
                    TransferItems = new List<TraderTransferItem>()
                };

                tradTransfer.Reference = new TraderReferenceRules(dbContext).GetNewReference(tradTransfer.Workgroup.Domain.Id, TraderReferenceType.Transfer);

                tradTransfer.TransferItems.AddRange(positiveAdjustment);
                tradTransfer.TransferItems.AddRange(negativeAdjustment);
                //tradTransfer.TransferItems.AddRange(transferItems);

                dbContext.TraderTransfers.Add(tradTransfer);
                dbContext.Entry(tradTransfer).State = EntityState.Added;

                dbContext.SaveChanges();

                /*
                TransferOut and TransferIn
                 */
                var transferRule = new TraderTransfersRules(dbContext);
                //Call TransferOut where TransferItems negative
                if (negativeAdjustment.Count > 0)
                    await transferRule.OutgoingInventory(tradTransfer, approval.ApprovedOrDeniedAppBy);

                if (positiveAdjustment.Count > 0)
                {
                    //Each item that causes an increase in inventory must be handled separetly.
                    //THis is required because there must be an individual cost for each item

                    foreach (var transferItem in positiveAdjustment)
                    {
                        //Calculate the cost to be applied
                        var inventoryDetail = transferItem.TraderItem.InventoryDetails.FirstOrDefault(e => e.Location.Id == tradTransfer.DestinationLocation.Id);
                        var avgCost = inventoryDetail.AverageCost;
                        var listSingle = new List<TraderTransferItem>();
                        listSingle.Add(transferItem);

                        //Call TransferIn where TransferItems positive
                        await transferRule.IncomingInventory(tradTransfer, approval.ApprovedOrDeniedAppBy, listSingle, avgCost);
                    }
                }

                tradTransfer.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var transferLog = new TransferLog
                {
                    Address = tradTransfer.Address,
                    AssociatedTransfer = tradTransfer,
                    Contact = tradTransfer.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Sale = tradTransfer.Sale,
                    Status = tradTransfer.Status,
                    UpdatedBy = approval.ApprovedOrDeniedAppBy,
                    AssociatedShipment = tradTransfer.AssociatedShipment,
                    DestinationLocation = tradTransfer.DestinationLocation,
                    OriginatingLocation = tradTransfer.OriginatingLocation,
                    TransferItems = tradTransfer.TransferItems,
                    Workgroup = tradTransfer.Workgroup
                };

                var transferProcessLog = new TransferProcessLog
                {
                    AssociatedTransfer = tradTransfer,
                    AssociatedTransferLog = transferLog,
                    TransferStatus = tradTransfer.Status,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.TraderTransferProcessLogs.Add(transferProcessLog);
                dbContext.Entry(transferProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public ReturnJsonModel SaveSpotCount(SpotCount spotCount, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, spotCount);
                if (spotCount.Workgroup != null && spotCount.Workgroup.Id > 0)
                {
                    spotCount.Workgroup = dbContext.WorkGroups.Find(spotCount.Workgroup.Id);
                }

                var currentUser = dbContext.QbicleUser.Find(userId);

                if (spotCount.Id == 0)
                {
                    spotCount.CreatedBy = currentUser;
                    spotCount.CreatedDate = DateTime.UtcNow;
                    spotCount.LastUpdatedBy = currentUser;
                    spotCount.LastUpdatedDate = spotCount.CreatedDate;
                    spotCount.Description = spotCount.Description.Trim();
                    if (spotCount.ProductList != null && spotCount.ProductList.Count > 0)
                    {
                        foreach (var item in spotCount.ProductList)
                        {
                            item.CreatedBy = spotCount.CreatedBy;
                            item.CreatedDate = spotCount.CreatedDate;
                            item.Product = new TraderItemRules(dbContext).GetById(item.Product.Id);
                            item.CountUnit = dbContext.ProductUnits.Find(item.CountUnit.Id);
                            item.SpotCount = spotCount;
                            item.LastUpdatedDate = spotCount.LastUpdatedDate;
                            item.LastUpdatedBy = spotCount.LastUpdatedBy;
                        }
                    }

                    dbContext.Entry(spotCount).State = EntityState.Added;
                    dbContext.SpotCounts.Add(spotCount);
                    dbContext.SaveChanges();

                    result.msgId = spotCount.Id.ToString();
                    result.actionVal = 1;
                }
                else
                {
                    var spotCountUpdate = dbContext.SpotCounts.Find(spotCount.Id);
                    if (spotCountUpdate != null)
                    {
                        spotCountUpdate.LastUpdatedBy = currentUser;
                        spotCountUpdate.LastUpdatedDate = DateTime.UtcNow;
                        spotCountUpdate.Name = spotCount.Name;
                        spotCountUpdate.Description = spotCount.Description.Trim();
                        if (spotCountUpdate.ProductList != null && spotCountUpdate.ProductList.Count > 0)
                        {
                            //DbContext.SpotCountItems.RemoveRange(spotCountUpdate.ProductList);
                            spotCountUpdate.ProductList.Clear();
                            dbContext.SaveChanges();
                        }

                        if (spotCount.ProductList != null && spotCount.ProductList.Count > 0)
                        {
                            foreach (var item in spotCount.ProductList)
                            {
                                item.CreatedBy = currentUser;
                                item.CreatedDate = spotCount.CreatedDate;
                                item.Product = new TraderItemRules(dbContext).GetById(item.Product.Id);
                                item.CountUnit = dbContext.ProductUnits.Find(item.CountUnit.Id);
                                item.SpotCount = spotCountUpdate;
                                item.LastUpdatedDate = spotCountUpdate.LastUpdatedDate;
                                item.LastUpdatedBy = spotCountUpdate.LastUpdatedBy;
                            }
                        }

                        spotCountUpdate.ProductList = spotCount.ProductList;
                        spotCountUpdate.Status = spotCount.Status;
                        spotCountUpdate.Workgroup = spotCount.Workgroup;
                        dbContext.Entry(spotCountUpdate).State = EntityState.Modified;
                    }

                    dbContext.SaveChanges();
                    result.msgId = spotCount.Id.ToString();
                    result.actionVal = 2;
                }

                var spotCountDb = dbContext.SpotCounts.Find(spotCount.Id);

                if (spotCountDb?.SpotCountApprovalProcess != null)
                    return result;

                if (spotCountDb == null || spotCountDb.Status != SpotCountStatus.CountStarted) return result;
                spotCountDb.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var appDef =
                    dbContext.SpotCountApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == spotCountDb.Workgroup.Id);
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    SpotCounts = new List<SpotCount> { spotCountDb },
                    Name = $"Spot Count: {spotCountDb.Name}",
                    Qbicle = spotCountDb.Workgroup.Qbicle,
                    Topic = spotCountDb.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = spotCountDb.CreatedBy,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader
                };
                spotCountDb.SpotCountApprovalProcess = approval;
                spotCountDb.SpotCountApprovalProcess.ApprovalRequestDefinition = appDef;
                approval.ActivityMembers.AddRange(spotCountDb.Workgroup.Members);
                dbContext.Entry(spotCountDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                var spotCountLog = new SpotCountLog
                {
                    AssociatedSpotCount = spotCountDb,
                    CreatedBy = spotCountDb.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    Description = spotCountDb.Description,
                    Domain = spotCountDb.Domain,
                    LastUpdatedBy = spotCountDb.CreatedBy,
                    LastUpdatedDate = spotCountDb.LastUpdatedDate,
                    Location = spotCountDb.Location,
                    ProductList = spotCountDb.ProductList,
                    SpotCountApprovalProcess = spotCountDb.SpotCountApprovalProcess,
                    Status = spotCountDb.Status,
                    Workgroup = spotCountDb.Workgroup
                };

                var spotCountProcessLog = new SpotCountProcessLog
                {
                    AssociatedSpotCount = spotCountDb,
                    AssociatedSpotCountLog = spotCountLog,
                    SpotCountStatus = spotCountDb.Status,
                    CreatedBy = spotCountDb.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = spotCountDb.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.SpotCountProcessLogs.Add(spotCountProcessLog);
                dbContext.Entry(spotCountProcessLog).State = EntityState.Added;
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
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, spotCount);
                result.msg = e.Message;
            }

            return result;
        }

        public ReturnJsonModel UpdateSportCountItems(SpotCount spotCount, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, spotCount);

                spotCount.LastUpdatedBy = dbContext.QbicleUser.Find(userId);

                var spotCountUpdate = dbContext.SpotCounts.Find(spotCount.Id);
                if (spotCountUpdate?.ProductList == null || spotCountUpdate.ProductList.Count == 0) return result;
                foreach (var item in spotCountUpdate.ProductList)
                {
                    var itemUpdate = spotCount.ProductList.FirstOrDefault(e => e.Id == item.Id);
                    if (itemUpdate == null) continue;
                    item.CountUnit = new TraderConversionUnitRules(dbContext).GetById(itemUpdate.CountUnit.Id);
                    item.Adjustment = itemUpdate.Adjustment;
                    item.SpotCountValue = itemUpdate.SpotCountValue;
                    item.Notes = itemUpdate.Notes;
                    //item.SavedInventoryCount = itemUpdate.SavedInventoryCount;
                }
                spotCountUpdate.LastUpdatedDate = DateTime.UtcNow;
                spotCountUpdate.LastUpdatedBy = spotCount.LastUpdatedBy;

                dbContext.Entry(spotCountUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();
                result.actionVal = 2;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, spotCount);
                result.msg = e.Message;
            }

            return result;
        }

        public ReturnJsonModel UpdateDescription(SpotCount spotCount)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, spotCount);
                var spotCountUpdate = dbContext.SpotCounts.Find(spotCount.Id);
                if (spotCountUpdate == null) return result;
                spotCountUpdate.Description = spotCount.Description.Trim();
                dbContext.Entry(spotCountUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();
                result.actionVal = 2;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, spotCount);
                result.msg = e.Message;
            }

            return result;
        }

        public List<ApprovalStatusTimeline> SpotCountApprovalStatusTimeline(int id, string timeZone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);

                var logs = dbContext.SpotCountProcessLogs.Where(e => e.AssociatedSpotCount.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string icon = "";

                foreach (var log in logs)
                {
                    switch (log.SpotCountStatus)
                    {
                        case SpotCountStatus.CountStarted:
                            icon = "fa fa-info bg-aqua";
                            break;

                        case SpotCountStatus.CountCompleted:
                            icon = "fa fa-truck bg-yellow";
                            break;

                        case SpotCountStatus.StockAdjusted:
                            icon = "fa fa-check bg-green";
                            break;

                        case SpotCountStatus.Draft:
                            icon = "fa fa-warning bg-yellow";
                            break;

                        case SpotCountStatus.Denied:
                            icon = "fa fa-warning bg-red";
                            break;

                        case SpotCountStatus.Discarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = log.SpotCountStatus.GetDescription(),
                            Icon = icon,
                            UserAvatar = log.CreatedBy.ProfilePic
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


        /// <summary>
        /// Using for case add/edit/remove item in a SpotCount existed
        /// </summary>
        /// <param name="spotCountItem"></param>
        /// <param name="userId"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateSpotCountProduct(SpotCountItem spotCountItem, string userId, bool isDelete)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, "", null, spotCountItem, isDelete);

                var user = new UserRules(dbContext).GetById(userId);

                var spotCountDb = dbContext.SpotCounts.Find(spotCountItem.SpotCount.Id);
                var spotCountItemDb = spotCountDb.ProductList.FirstOrDefault(e => e.Id == spotCountItem.Id);
                if (spotCountItemDb != null)
                {
                    if (isDelete)
                    {
                        spotCountDb.ProductList.Remove(spotCountItemDb);
                    }
                    else
                    {
                        spotCountItemDb.Notes = spotCountItem.Notes;
                        spotCountItemDb.CountUnit = dbContext.ProductUnits.Find(spotCountItem.CountUnit.Id);
                    }
                }
                else
                {
                    var sItem = new SpotCountItem
                    {
                        Id = spotCountItem.Id,
                        CountUnit = dbContext.ProductUnits.Find(spotCountItem.CountUnit.Id),
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Product = new TraderItemRules(dbContext).GetById(spotCountItem.Product.Id),
                        SpotCount = spotCountDb,
                        LastUpdatedDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        Notes = spotCountItem.Notes,
                        SavedInventoryCount = spotCountItem.SavedInventoryCount,
                        SpotCountValue = spotCountItem.SpotCountValue
                    };
                    spotCountDb.ProductList.Add(sItem);                    
                }

                dbContext.Entry(spotCountDb).State = EntityState.Modified;
                dbContext.SaveChanges();



            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, spotCountItem, userId);
                result.msg = e.Message;
                result.result = false;
            }
            result.result = true;

            return result;

        }

        public DataTablesResponse GetSpotCountItem(IDataTablesRequest requestModel,
            int spotCountId, int locationId, int domainId, UserSetting user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, spotCountId, domainId);

                var spotCount = new TraderSpotCountRules(dbContext).GetById(spotCountId);

                var totalItems = spotCount.ProductList.Count();
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Product.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SKU":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Product.SKU" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Notes":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Notes" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CurrentStock":
                            break;
                        default:
                            orderByString = "Product.Name asc";
                            break;
                    }
                }

                var items = spotCount.ProductList.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);


                //Paging
                var list = items.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                var dataJson = new List<WasteReportItemModel>();

                list.ForEach(item =>
                {
                    var spot = new WasteReportItemModel
                    {
                        Id = item.Id,
                        ProductId = item.Product?.Id ?? 0,
                        Name = $"<span>{item.Product?.Name}</span><input type='hidden' value='{item.Id}' class='spot_id' />",
                        Unit = InitSpotCountItemUnit(item),
                        SKU = $"<span>{item.Product.SKU}</span><input type='hidden' value='{item.Product.Id}' class='spot_item_id' />",
                        Notes = $"<input type='text' name='item-1-notes' onchange='UpdateSpotCountProduct({item.Product.Id},false)' value='{item.Notes}' class='form-control' style='width: 100%;'>",
                        Button = $"<button class='btn btn-danger' onclick='removeItem({item.Product.Id})'><i class='fa fa-trash'></i></button>"
                    };
                    if (spotCount.Status == SpotCountStatus.CountCompleted || spotCount.Status == SpotCountStatus.StockAdjusted)
                    {
                        spot.CurrentStock = $"<span class='demo'>{item.SavedInventoryCount}</span>";
                    }
                    else
                    {
                        var inventory = item.Product.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId);
                        if (inventory == null)
                        {
                            inventory = new InventoryDetail();
                        }
                        spot.CurrentStock = $"<span class='demo'>{inventory.CurrentInventoryLevel}</span>";
                    }
                    dataJson.Add(spot);

                });

                return new DataTablesResponse(requestModel.Draw, dataJson, totalItems, totalItems);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel, spotCountId, domainId);
            }

            return null;
        }

        private string InitSpotCountItemUnit(SpotCountItem item)
        {
            string selectUnit = "";
            selectUnit += $"<select id='{Guid.NewGuid().ToString().Replace("-", "")}'";
            selectUnit += $"onchange='UpdateSpotCountProduct({item.Product.Id},false)' data-placeholder='Please select' class='form-control unit-select2' style='width: 100%;'>";
            foreach (var unit in item.Product.Units)
            {
                if (unit.Id == item.CountUnit?.Id)
                    selectUnit += $"<option selected value='{unit.Id}'>{unit.Name}</option>";
                else
                    selectUnit += $"<option value='{unit.Id}'>{unit.Name}</option>";
            }
            selectUnit += $"</select>";
            return selectUnit;
        }
    }
}