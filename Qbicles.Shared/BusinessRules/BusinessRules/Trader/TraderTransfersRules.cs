using Qbicles.BusinessRules.Hangfire;
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
    public class TraderTransfersRules
    {
        private readonly ApplicationDbContext dbContext;

        public TraderTransfersRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task ScheduleIncomingInventory(IncomingInventoryJobParamenter incoming)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ScheduleIncomingInventory", null, null, incoming);

                var user = await dbContext.QbicleUser.FindAsync(incoming.UserId);
                var transfer = await dbContext.TraderTransfers.FindAsync(incoming.TransferId);

                var transferItems = dbContext.TraderTransferItems.Where(e => incoming.TraderTransferItemIds.Contains(e.Id)).ToList();
                await IncomingInventory(transfer, user, transferItems, incoming.OverrideUnitCost, false);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, incoming);
            }
        }

        public async Task ScheduleOutgoingInventory(OutgoingInventoryJobParamenter outgoing)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ScheduleOutgoingInventory", null, null, outgoing);

                var user = await dbContext.QbicleUser.FindAsync(outgoing.UserId);
                var transfer = await dbContext.TraderTransfers.FindAsync(outgoing.TransferId);
                if (outgoing.OutgoingItems != null && outgoing.OutgoingItems.Count > 0)
                {
                    var negativeAdjustmentItems = transfer.TransferItems.Where(e => outgoing.OutgoingItems.Contains(e.Id)).ToList();
                    await OutgoingInventory(transfer, user, negativeAdjustmentItems, false);
                }
                else
                    await OutgoingInventory(transfer, user, null, false);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, outgoing);
            }
        }

        //Transfers
        public List<WorkGroup> GetWorkGroups(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);
                return dbContext.TraderTransfers.Where(d => (d.DestinationLocation.Id == locationId || d.OriginatingLocation.Id == locationId)).Select(q => q.Workgroup).Distinct().OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId);
                return new List<WorkGroup>();
            }
        }

        public DataTablesResponse TraderTransfersSearch(IDataTablesRequest requestModel, int locationId, int domainId,
            UserSetting user,
            string keyword, string route, int wgId, string date)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, requestModel, locationId, domainId, user,
                        keyword, route, wgId, date);
                var dateFormat = string.IsNullOrEmpty(user.DateFormat) ? "dd/MM/yyyy" : user.DateFormat;
                var workGroupIds = dbContext.WorkGroups.Where(q =>
                    q.Location.Id == locationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).Select(q => q.Id).ToList();

                var transfers = dbContext.TraderTransfers.Where(q => (q.DestinationLocation.Id == locationId || q.OriginatingLocation.Id == locationId));
                int totalTransfer;

                #region Filter

                keyword = string.IsNullOrEmpty(keyword) ? requestModel.Search.Value : keyword;
                route = route.ToLower().Trim();
                date = date.ToLower().Trim();

                if (!string.IsNullOrEmpty(keyword))
                    transfers = transfers.Where(q =>
                        (q.Workgroup != null && q.Workgroup.Name.ToLower().Contains(keyword))
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword))
                        || ((q.CreatedDate.Day < 10 ? ("0" + q.CreatedDate.Day.ToString()) : q.CreatedDate.Day.ToString()) + "/" + (q.CreatedDate.Month < 10 ? ("0" + q.CreatedDate.Month.ToString()) : q.CreatedDate.Month.ToString()) + "/" + q.CreatedDate.Year).Contains(keyword)
                        || (q.Sale != null && q.Sale.Purchaser != null && q.Sale.Purchaser.Name.ToLower().Contains(keyword))
                        || (q.OriginatingLocation != null && q.OriginatingLocation.Name.ToLower().Contains(keyword))
                        || (q.Purchase != null && q.Purchase.Vendor != null && q.Purchase.Vendor.Name.ToLower().Contains(keyword))
                        || (q.DestinationLocation != null && q.DestinationLocation.Name.ToLower().Contains(keyword))
                    );
                if (wgId > 0)
                {
                    transfers = transfers.Where(q => q.Workgroup.Id == wgId);
                }
                if (!string.IsNullOrEmpty(route.Trim()))
                {
                    if (route == "inbound")
                    {
                        transfers = transfers.Where(q => (route == "inbound" && q.DestinationLocation.Id == locationId));
                    }
                    else if (route == "outbound")
                    {
                        transfers = transfers.Where(q => (route == "outbound" && q.OriginatingLocation.Id == locationId));
                    }
                }
                if (!string.IsNullOrEmpty(date.Trim()))
                {
                    if (!date.Contains('-'))
                    {
                        date += "-";
                    }
                    //var sDate = DateTime.Parse(date.Split('-')[0].Trim());
                    //var endDate = DateTime.Parse(date.Split('-')[1].Trim());

                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    date.ConvertDaterangeFormat(dateFormat, user.Timezone, out startDate, out endDate);
                    startDate = startDate.AddTicks(1);
                    endDate = endDate.AddDays(1).AddTicks(-1);

                    transfers = transfers.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                }
                totalTransfer = transfers.Count();

                #endregion Filter

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "WorkGroupName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Workgroup.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Reason":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reason" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }

                transfers = transfers.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = transfers.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = new List<TransferCustom>();
                var routeStr = "";
                foreach (var item in list)
                {
                    string transferFrom;
                    string transferTo;
                    if (item.Sale != null)
                    {
                        transferFrom = item.OriginatingLocation.Name;
                        transferTo = item.Sale.Purchaser.Name;
                    }
                    else if (item.Purchase != null)
                    {
                        transferFrom = item.Purchase.Vendor.Name;
                        transferTo = item.DestinationLocation.Name;
                    }
                    else if (item.OriginatingLocation != null && item.DestinationLocation != null)
                    {
                        transferFrom = item.OriginatingLocation.Name;
                        transferTo = item.DestinationLocation.Name;
                    }
                    else
                    {
                        transferFrom = item.OriginatingLocation?.Name;
                        transferTo = item.DestinationLocation?.Name;
                    }

                    if (item.DestinationLocation != null && item.OriginatingLocation != null
                                                         && item.DestinationLocation.Id == locationId && item.OriginatingLocation.Id == locationId)
                    {
                        routeStr = "InBound, OutBound";
                    }
                    else if (item.DestinationLocation != null && item.DestinationLocation.Id == locationId)
                    {
                        routeStr = "InBound";
                    }
                    else if (item.OriginatingLocation != null && item.OriginatingLocation.Id == locationId)
                    {
                        routeStr = "OutBound";
                    }
                    dataJson.Add(new TransferCustom()
                    {
                        Id = item.Id,
                        Key = item.Key,
                        FullRef = item.Reference?.FullRef ?? "",
                        Date = item.CreatedDate.ToString(dateFormat),
                        To = transferTo,
                        From = transferFrom,
                        WorkGroupName = item.Workgroup?.Name,
                        Route = routeStr,
                        Reason = item.Reason.GetDescription(),
                        Status = item.Status.ToString(),
                        Purchase = item.Purchase != null ? new { item.Purchase.Id } : null,
                        Sale = item.Sale != null ? new { item.Sale.Id, item.Sale.Key } : null,
                        AllowEdit = workGroupIds.Contains(item.Workgroup?.Id ?? 0)
                    });
                }
                return new DataTablesResponse(requestModel.Draw, dataJson, totalTransfer, totalTransfer);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, requestModel, locationId, domainId, user, keyword,
                    route, wgId, date);
                return null;
            }
        }

        public int SaveTraderTransfer(TraderTransfer transfer, string userId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, transfer, userId);

                if (transfer.Reference != null)
                    transfer.Reference = new TraderReferenceRules(dbContext).GetById(transfer.Reference.Id);

                var transferItemRule = new TraderItemRules(dbContext);
                var actionVal = 3;
                if (transfer == null)
                    return actionVal;
                int transferId;

                var user = dbContext.QbicleUser.Find(userId);

                var locationRule = new TraderLocationRules(dbContext);
                var transferType = TransferType.P2P;

                if (transfer.Id > 0)
                {
                    transferId = transfer.Id;
                    var trDb = GetTraderTransferId(transfer.Id);
                    trDb.Status = transfer.Status;
                    while (trDb.TransferItems.Count > 0)
                    {
                        dbContext.TraderTransferItems.Remove(trDb.TransferItems[0]);
                        dbContext.SaveChanges();
                    }

                    if (transfer.Sale != null || transfer.Purchase != null)
                    {
                        if (transfer.Sale?.Id > 0)
                        {
                            transferType = TransferType.Sale;
                            var sale = new TraderSaleRules(dbContext).GetById(transfer.Sale.Id);
                            transfer.Sale = sale;
                            transfer.DestinationLocation = null;
                            transfer.OriginatingLocation = sale.Location;
                            transfer.Address = sale.DeliveryAddress;
                            transfer.Contact = sale.Purchaser;
                            transfer.Reason = TransferReasonEnum.Sale;
                        }
                        else if (transfer.Purchase?.Id > 0)
                        {
                            transferType = TransferType.Purchase;
                            var purchase = new TraderPurchaseRules(dbContext).GetById(transfer.Purchase.Id);
                            transfer.Purchase = purchase;
                            transfer.DestinationLocation = purchase.Location;
                            transfer.OriginatingLocation = null;
                            transfer.Address = purchase.Vendor.Address;
                            transfer.Contact = purchase.Vendor;
                            transfer.Reason = TransferReasonEnum.Purchase;
                        }
                    }
                    else
                    {
                        transfer.Reason = TransferReasonEnum.PointToPoint;
                        transferType = TransferType.P2P;
                        trDb.DestinationLocation = locationRule.GetById(transfer.DestinationLocation?.Id ?? 0);
                        trDb.OriginatingLocation = locationRule.GetById(transfer.OriginatingLocation?.Id ?? 0);
                        trDb.Workgroup = new TraderWorkGroupsRules(dbContext).GetById(transfer.Workgroup.Id);
                    }
                    var transferItems = new List<TraderTransferItem>();
                    foreach (var item in transfer.TransferItems)
                    {
                        //var transferUnit = dbContext.ProductUnits.Find(item.Unit?.Id);
                        var transactionItem = transferType == TransferType.P2P ? null : dbContext.TraderSaleItems.Find(item.TransactionItem?.Id);
                        //transactionItem.Unit = transferUnit;

                        transferItems.Add(new TraderTransferItem
                        {
                            //Unit = transferUnit,
                            Unit = dbContext.ProductUnits.Find(item.Unit?.Id),
                            QuantityAtPickup = item.QuantityAtPickup,//By default the Quantity at Delivery is to be set to the Quantity at Pickup
                                                                     //It will be up to the person indicating that delivery has occurred to indicate that the quantities were not the same.
                            QuantityAtDelivery = item.QuantityAtPickup,
                            TransactionItem = transactionItem,
                            TraderItem = transferItemRule.GetById(item.TraderItem?.Id ?? 0)
                        });
                    }

                    #region Compress TraderTransferItems

                    var compressTraderTransferItems = CompressTraderTransferItems(transferItems);
                    foreach (var item in compressTraderTransferItems)
                    {
                        var unit = item.TraderItem?.Units?.FirstOrDefault(s => s.IsBase);
                        var transItem = new TraderTransferItem
                        {
                            //Unit = item.TraderItem.Units.FirstOrDefault(s => s.IsBase),
                            Unit = unit,
                            QuantityAtPickup = item.TotalQuantity,//By default the Quantity at Delivery is to be set to the Quantity at Pickup
                                                                  //It will be up to the person indicating that delivery has occurred to indicate that the quantities were not the same.
                            QuantityAtDelivery = item.TotalQuantity,
                            TransactionItem = item.TransactionItem,
                            TraderItem = item.TraderItem
                        };
                        trDb.TransferItems.Add(transItem);
                    }

                    #endregion Compress TraderTransferItems

                    trDb.Reference = transfer.Reference;
                    if (dbContext.Entry(trDb).State == EntityState.Detached)
                        dbContext.TraderTransfers.Attach(trDb);
                    dbContext.Entry(trDb).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    actionVal = 2;
                }
                else
                {
                    var traderTransfer = new TraderTransfer
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        DestinationLocation = locationRule.GetById(transfer.DestinationLocation?.Id ?? 0),
                        OriginatingLocation = locationRule.GetById(transfer.OriginatingLocation?.Id ?? 0),
                        Workgroup = new TraderWorkGroupsRules(dbContext).GetById(transfer.Workgroup.Id),
                        Status = transfer.Status,
                        Reference = transfer.Reference,
                        Reason = TransferReasonEnum.PointToPoint
                    };

                    if (transfer.Sale?.Id > 0)
                    {
                        transferType = TransferType.Sale;
                        var sale = new TraderSaleRules(dbContext).GetById(transfer.Sale.Id);
                        traderTransfer.Sale = sale;
                        traderTransfer.DestinationLocation = null;
                        traderTransfer.OriginatingLocation = sale.Location;
                        traderTransfer.Address = sale.DeliveryAddress;
                        traderTransfer.Contact = sale.Purchaser;
                        traderTransfer.Reason = TransferReasonEnum.Sale;
                    }
                    else if (transfer.Purchase?.Id > 0)
                    {
                        transferType = TransferType.Purchase;
                        var purchase = new TraderPurchaseRules(dbContext).GetById(transfer.Purchase.Id);
                        traderTransfer.Purchase = purchase;
                        traderTransfer.OriginatingLocation = null;
                        traderTransfer.DestinationLocation = purchase.Location;
                        traderTransfer.Address = purchase.Vendor.Address;
                        traderTransfer.Contact = purchase.Vendor;
                        traderTransfer.Reason = TransferReasonEnum.Purchase;
                    }

                    var transferItems = new List<TraderTransferItem>();
                    foreach (var item in transfer.TransferItems)
                    {
                        //var transferUnit = dbContext.ProductUnits.Find(item.Unit?.Id);
                        var transactionItem = transferType == TransferType.P2P ? null : dbContext.TraderSaleItems.Find(item.TransactionItem?.Id);
                        //transactionItem.Unit = transferUnit;

                        transferItems.Add(new TraderTransferItem
                        {
                            Unit = dbContext.ProductUnits.Find(item.Unit?.Id),
                            //Unit = transferUnit,
                            QuantityAtPickup = item.QuantityAtPickup,//By default the Quantity at Delivery is to be set to the Quantity at Pickup
                                                                     //It will be up to the person indicating that delivery has occurred to indicate that the quantities were not the same.
                            QuantityAtDelivery = item.QuantityAtPickup,
                            TransactionItem = transactionItem,
                            TraderItem = transferItemRule.GetById(item.TraderItem?.Id ?? 0)
                        });
                    }

                    #region Compress TraderTransferItems

                    var compressTraderTransferItems = CompressTraderTransferItems(transferItems);
                    foreach (var item in compressTraderTransferItems)
                    {
                        var unit = item.TraderItem.Units.FirstOrDefault(s => s.IsBase);
                        var transItem = new TraderTransferItem
                        {
                            //Unit = item.TraderItem.Units.FirstOrDefault(s => s.IsBase),
                            Unit = unit,
                            QuantityAtPickup = item.TotalQuantity,//By default the Quantity at Delivery is to be set to the Quantity at Pickup
                                                                  //It will be up to the person indicating that delivery has occurred to indicate that the quantities were not the same.
                            QuantityAtDelivery = item.TotalQuantity,
                            TransactionItem = item.TransactionItem,
                            TraderItem = item.TraderItem
                        };
                        traderTransfer.TransferItems.Add(transItem);
                    }

                    #endregion Compress TraderTransferItems

                    dbContext.TraderTransfers.Add(traderTransfer);
                    dbContext.Entry(traderTransfer).State = EntityState.Added;
                    actionVal = 1;
                    dbContext.SaveChanges();
                    transferId = traderTransfer.Id;
                }

                var tradTransfer = dbContext.TraderTransfers.Find(transferId);

                if (tradTransfer?.TransferApprovalProcess != null)
                    return actionVal;

                if (tradTransfer == null || tradTransfer.Status != TransferStatus.PendingPickup)
                    return actionVal;
                tradTransfer.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var appDef =
                    dbContext.TransferApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == tradTransfer.Workgroup.Id);
                var refFull = tradTransfer.Reference == null ? "" : tradTransfer.Reference.FullRef;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = tradTransfer.Workgroup.Qbicle,
                    Topic = tradTransfer.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Trader Transfer Process #{refFull}",
                    Transfer = new List<TraderTransfer> { tradTransfer },
                    //Sale = new List<TraderSale> { appSale },
                    //Purchase = new List<TraderPurchase> { appPurchase }
                };
                tradTransfer.TransferApprovalProcess = approval;
                approval.ActivityMembers.AddRange(tradTransfer.Workgroup.Members);
                dbContext.Entry(tradTransfer).State = EntityState.Modified;

                dbContext.SaveChanges();

                var transferLog = new TransferLog
                {
                    Address = tradTransfer.Address,
                    AssociatedTransfer = tradTransfer,
                    Contact = tradTransfer.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Purchase = tradTransfer.Purchase,
                    Sale = tradTransfer.Sale,
                    TransferApprovalProcess = approval,
                    Status = tradTransfer.Status,
                    UpdatedBy = user,
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
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.TraderTransferProcessLogs.Add(transferProcessLog);
                dbContext.Entry(transferProcessLog).State = EntityState.Added;
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

                return actionVal;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, transfer, userId);
                return 0;
            }
        }

        private TraderTransfer GetTraderTransferId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderTransfers.FirstOrDefault(t => t.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderTransfer();
            }
        }

        public bool DeleteTraderTransfer(int id)
        {
            var transfer = dbContext.TraderTransfers.FirstOrDefault(t => t.Id == id) ?? new TraderTransfer();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                dbContext.TraderTransfers.Remove(transfer);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return true;
        }

        public TraderTransfer GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderTransfers.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderTransfer();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">Transfer Id</param>
        /// <param name="timezone"></param>
        /// <returns></returns>
        public List<ApprovalStatusTimeline> TransferApprovalStatusTimeline(int id, string timezone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timezone);
                var logs = dbContext.TraderTransferProcessLogs.Where(e => e.AssociatedTransfer.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string status = "", icon = "";

                foreach (var log in logs.ToList())
                {
                    switch (log.TransferStatus)
                    {
                        case TransferStatus.Initiated:
                            status = "Initiated";
                            icon = "fa fa-info bg-aqua";
                            break;

                        case TransferStatus.PendingPickup:
                            status = "Pending Pickup";
                            icon = "fa fa-info bg-aqua";
                            break;

                        case TransferStatus.PickedUp:
                            status = "Picked Up";
                            icon = "fa fa-truck bg-yellow";
                            break;

                        case TransferStatus.Delivered:
                            status = "Delivered";
                            icon = "fa fa-check bg-green";
                            break;

                        case TransferStatus.Draft:
                            status = "Draft";
                            icon = "fa fa-warning bg-yellow";
                            break;

                        case TransferStatus.Denied:
                            status = "Denied";
                            icon = "fa fa-warning bg-red";
                            break;

                        case TransferStatus.Discarded:
                            status = "Discarded";
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timezone).ToShortTimeString(),
                            Status = status,
                            Icon = icon,
                            UserAvatar = log.CreatedBy != null ? log.CreatedBy.ProfilePic : ""
                        }
                    );
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timezone);
            }

            return timeline;
        }

        public void TransferApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var transfer = approval.Transfer.FirstOrDefault();
                if (transfer == null) return;

                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        transfer.Status = TransferStatus.PendingPickup;
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        transfer.Status = TransferStatus.PickedUp;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        transfer.Status = TransferStatus.Delivered;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                        transfer.Status = TransferStatus.Denied;
                        break;

                    case ApprovalReq.RequestStatusEnum.Discarded:
                        transfer.Status = TransferStatus.Discarded;
                        break;
                }
                dbContext.Entry(transfer).State = EntityState.Modified;
                dbContext.SaveChanges();

                /*
                1. The Transfer (and TransferItems) are created in a Purchase or Sale.
                2. The Costs of the individual TRansferItems are set at that they are created.
                As a cost specified by the user in terms of a Purchase or as a price specified by the user in terms of a Sale
                3. When a Transfer OUT occurs (a Sale), after the Batches (out) have been created, that is when we have to calculate the costs for the inverntorydetail OUT of which the batches are leaving
                4. When a Transfer IN occurs (a Purchase), after the Batches (in) have been created, that is when we have to calculate the costs for the inverntorydetail INTO which the batches are coming
                 */
                switch (transfer.Status)
                {
                    case TransferStatus.Delivered:
                        IncomingInventory(transfer, approval.ApprovedOrDeniedAppBy);
                        ////The Journal Entries for Purchases are to be created after the Transfers for Purchases have been saved to the database when the status of the Transfer is 'Delivered'
                        //if (transfer.Purchase != null)
                        //    new BookkeepingIntegrationRules(dbContext).AddPurchaseInventoryJournalEntry(approval.ApprovedOrDeniedAppBy, transfer);
                        break;

                    case TransferStatus.PickedUp:
                        OutgoingInventory(transfer, approval.ApprovedOrDeniedAppBy);
                        break;
                }

                var transferLog = new TransferLog
                {
                    Address = transfer.Address,
                    AssociatedTransfer = transfer,
                    Contact = transfer.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Purchase = transfer.Purchase,
                    Sale = transfer.Sale,
                    TransferApprovalProcess = approval,
                    Status = transfer.Status,
                    UpdatedBy = approval.ApprovedOrDeniedAppBy,
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

                dbContext.TraderTransferProcessLogs.Add(transferProcessLog);
                dbContext.Entry(transferProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public async Task IncomingInventory(TraderTransfer transfer, ApplicationUser user, List<TraderTransferItem> transferItems = null,
                                      decimal? overrideUnitCost = null, bool sendToQueue = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, transfer, user, transferItems, overrideUnitCost);
                // Check if the Destination location is NULL
                // If it is null then we cannot make batches, this is a Transfer out of the system
                if (transfer.DestinationLocation == null)
                {
                    await Task.CompletedTask; return;
                }

                var tranItems = transferItems ?? transfer.TransferItems.ToList();

                if (sendToQueue)
                {
                    var job = new IncomingInventoryJobParamenter
                    {
                        EndPointName = "scheduleonincominginventory",
                        SendToQueue = false,
                        UserId = user.Id,
                        TransferId = transfer.Id,
                        OverrideUnitCost = overrideUnitCost,
                        TraderTransferItemIds = transferItems == null ? tranItems.Select(e => e.Id).ToList() : transferItems.Select(e => e.Id).ToList()
                    };

                    await new QbiclesJob().HangFireExcecuteAsync(job);
                    await Task.CompletedTask;
                    return;
                }

                foreach (var transferItem in tranItems)
                {
                    // Get the correct InventoryDetail
                    // This is Incoming => the code must INCREASE the Inventory at the Location INTO which the Transfer is coming
                    var inventoryDetail = transferItem.TraderItem.InventoryDetails.FirstOrDefault(e => e.Location.Id == transfer.DestinationLocation.Id);

                    // If the InventoryDetail for the Item is not at the location then I think we should create the InventoryDetail
                    if (inventoryDetail == null) continue;

                    if (transferItem.InventoryBatches != null && transferItem.InventoryBatches.Count > 0) //This is a Transfer from another Location.
                    {
                        var batches = new List<Batch>();
                        var ivDetailLogs = new List<InventoryDetailLog>();
                        var batchLogs = new List<BatchLog>();
                        transferItem.InventoryBatches.ForEach(batch =>
                        {
                            var newBatch = new Batch { OriginalQuantity = batch.OriginalQuantity };
                            newBatch.UnusedQuantity = newBatch.OriginalQuantity;

                            //Override the unit cost if the value has been supplied
                            if (overrideUnitCost != null)
                            {
                                newBatch.CostPerUnit = (decimal)overrideUnitCost;
                            }
                            else
                            {
                                newBatch.CostPerUnit = batch.CostPerUnit;
                            }

                            newBatch.CreatedDate = DateTime.UtcNow;
                            newBatch.CreatedBy = user;
                            newBatch.LastUpdatedDate = newBatch.CreatedDate;
                            newBatch.LastUpdatedBy = user;
                            newBatch.CurrentBatchValue = newBatch.UnusedQuantity * newBatch.CostPerUnit;
                            newBatch.Direction = BatchDirection.In;
                            newBatch.ParentTransferItem = transferItem;

                            inventoryDetail.InventoryBatches.Add(newBatch);
                            inventoryDetail.LastUpdatedDate = DateTime.UtcNow;
                            inventoryDetail.LastUpdatedBy = user;

                            //Update InventoryDetail (Explanation below in section (7) InventoryDetail)
                            inventoryDetail.CurrentInventoryLevel += batch.OriginalQuantity;

                            //Add an InventoryDetail log Explanation below in section (9) Inventory Detail Log)
                            var newInventoryDetailLog = new InventoryDetailLog
                            {
                                CreatedBy = user,
                                CreatedDate = DateTime.UtcNow,
                                CurrentInventoryLevel = inventoryDetail.CurrentInventoryLevel,
                                InventoryBatches = new List<Batch>(),
                                AverageCost = inventoryDetail.AverageCost,
                                CurrentRecipe = inventoryDetail.CurrentRecipe,
                                Item = inventoryDetail.Item,
                                LastUpdatedBy = user,
                                LastUpdatedDate = DateTime.UtcNow,
                                LatestCost = inventoryDetail.LatestCost,
                                Location = inventoryDetail.Location,
                                Logs = new List<InventoryDetailLog>(),
                                MaxInventoryLevel = inventoryDetail.MaxInventoryLevel,
                                MinInventorylLevel = inventoryDetail.MinInventorylLevel,
                                ParentInventoryDetail = inventoryDetail,
                                ReorderLevel = 0,
                                ReorderUnit = transferItem.Unit,
                                UnitCost = 0
                            };

                            //Add a batch log (Explanation below in section (8) BatchLog)
                            var newBatchLog = new BatchLog
                            {
                                CreatedBy = user,
                                CreatedDate = DateTime.UtcNow,
                                CurrentBatchValue = newBatch.CurrentBatchValue,
                                Direction = BatchDirection.In,
                                InventoryDetail = inventoryDetail,
                                LasteUpdatedBy = user,
                                LasteUpdatedDate = DateTime.UtcNow,
                                LastUpdatedDate = DateTime.UtcNow,
                                CostPerUnit = batch.CostPerUnit,
                                OriginalQuantity = newBatch.OriginalQuantity,
                                ParentBatch = batch,
                                ParentTransferItem = transferItem,
                                Reason = BatchLogReason.TransferIn,
                                UnusedQuantity = newBatch.UnusedQuantity
                            };

                            batches.Add(newBatch);
                            ivDetailLogs.Add(newInventoryDetailLog);
                            batchLogs.Add(newBatchLog);
                        });

                        dbContext.InventoryBatches.AddRange(batches);
                        dbContext.InventoryDetailLogs.AddRange(ivDetailLogs);
                        dbContext.InventoryBatchLogs.AddRange(batchLogs);
                    }
                    else if (transferItem.InventoryBatches == null || transferItem.InventoryBatches.Count == 0) //This is a Transfer from outside the Domain, i.e. a purchase from an external supplier.
                    {
                        var newBatch = new Batch
                        {
                            OriginalQuantity = transferItem.QuantityAtDelivery * transferItem.Unit?.QuantityOfBaseunit ?? 0
                        };
                        newBatch.UnusedQuantity = newBatch.OriginalQuantity;

                        //Override the unit cost if the value has been supplied
                        if (overrideUnitCost != null)
                        {
                            newBatch.CostPerUnit = (decimal)overrideUnitCost;
                        }
                        else
                        {
                            newBatch.CostPerUnit = (transferItem.TransactionItem?.CostPerUnit ?? 0) / (transferItem.TransactionItem?.Unit?.QuantityOfBaseunit ?? 0);
                        }

                        newBatch.CreatedDate = DateTime.UtcNow;
                        newBatch.CreatedBy = user;
                        newBatch.LastUpdatedDate = newBatch.CreatedDate;
                        newBatch.LastUpdatedBy = user;
                        newBatch.CurrentBatchValue = newBatch.UnusedQuantity * newBatch.CostPerUnit;
                        newBatch.Direction = BatchDirection.In;
                        newBatch.ParentTransferItem = transferItem;
                        inventoryDetail.InventoryBatches.Add(newBatch);
                        inventoryDetail.LastUpdatedDate = DateTime.UtcNow;
                        inventoryDetail.LastUpdatedBy = user;

                        //Update InventoryDetail (Explanation below in section (7) InventoryDetail)
                        inventoryDetail.CurrentInventoryLevel += newBatch.OriginalQuantity;

                        //Add an InventoryDetail log Explanation below in section (9) Inventory Detail Log)
                        var newInventoryDetailLog = new InventoryDetailLog
                        {
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            CurrentInventoryLevel = inventoryDetail.CurrentInventoryLevel,
                            InventoryBatches = new List<Batch>(),
                            AverageCost = inventoryDetail.AverageCost,
                            CurrentRecipe = inventoryDetail.CurrentRecipe,
                            Item = inventoryDetail.Item,
                            LastUpdatedBy = user,
                            LastUpdatedDate = DateTime.UtcNow,
                            LatestCost = inventoryDetail.LatestCost,
                            Location = inventoryDetail.Location,
                            Logs = new List<InventoryDetailLog>(),
                            MaxInventoryLevel = inventoryDetail.MaxInventoryLevel,
                            MinInventorylLevel = inventoryDetail.MinInventorylLevel,
                            ParentInventoryDetail = inventoryDetail,
                            ReorderLevel = 0,
                            ReorderUnit = transferItem.Unit,
                            UnitCost = 0
                        };

                        //Add a batch log (Explanation below in section (8) BatchLog)
                        var newBatchLog = new BatchLog
                        {
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            CurrentBatchValue = newBatch.CurrentBatchValue,
                            Direction = BatchDirection.In,
                            InventoryDetail = inventoryDetail,
                            LasteUpdatedBy = user,
                            LasteUpdatedDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow,
                            CostPerUnit = newBatch.CostPerUnit,
                            OriginalQuantity = newBatch.OriginalQuantity,
                            ParentBatch = newBatch,
                            ParentTransferItem = transferItem,
                            Reason = BatchLogReason.TransferIn,
                            UnusedQuantity = newBatch.UnusedQuantity
                        };

                        dbContext.InventoryBatches.Add(newBatch);
                        dbContext.InventoryDetailLogs.Add(newInventoryDetailLog);
                        dbContext.InventoryBatchLogs.Add(newBatchLog);
                    }

                    // I thought we could create the log at line 409 above
                    // then update the log here ?
                    // NOT just create it here
                    var newInventoryUpdateLog = new InventoryUpdateLog
                    {
                        AssociatedInventoryDetail = inventoryDetail,
                        AssociatedItem = inventoryDetail.Item,
                        AssociatedLocation = inventoryDetail.Location,
                        AssociatedTransfer = transfer,
                        CompletedDate = DateTime.UtcNow,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Domain = inventoryDetail.Location.Domain,
                        IsComplete = true,
                        IsTransferIn = true //false a Transfer Out is causing the update, tru => Transfer In
                    };
                    dbContext.InventoryUpdateLogs.Add(newInventoryUpdateLog);
                    dbContext.SaveChanges();

                    // AFTER the batches have been created or updated -> calculate avgCost, lastCost for inventoryDetail
                    var inventoryRules = new TraderInventoryRules(dbContext);
                    inventoryRules.UpdateCosts(inventoryDetail);
                }

                //The Journal Entries for Purchases are to be created after the Transfers for Purchases have been saved to the database when the status of the Transfer is 'Delivered'
                if (transfer.Purchase != null)
                    new BookkeepingIntegrationRules(dbContext).AddPurchaseInventoryJournalEntry(transfer.TransferApprovalProcess.ApprovedOrDeniedAppBy, transfer);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, user.Id, transfer, user, transferItems, overrideUnitCost);
            }
        }

        public async Task OutgoingInventory(TraderTransfer transfer, ApplicationUser user, List<TraderTransferItem> outgoingItems = null, bool sendToQueue = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, transfer, user);

                // Check if the Originating location is NULL
                // If it is null then we cannot make batches, this is a Transfer into the system
                if (transfer.OriginatingLocation == null)
                    return;

                if (sendToQueue)
                {
                    var job = new OutgoingInventoryJobParamenter
                    {
                        EndPointName = "scheduleonoutgoinginventory",
                        SendToQueue = false,
                        UserId = user.Id,
                        TransferId = transfer.Id
                    };
                    if (outgoingItems != null)
                        job.OutgoingItems = outgoingItems.Select(e => e.Id).ToList(); ;

                    Task tskHangfire = new Task(async () =>
                    {
                        await new QbiclesJob().HangFireExcecuteAsync(job);
                    });
                    tskHangfire.Start();
                    return;
                }

                var listManufactureProductModel = new List<ManufactureProductModel>();

                var tranItems = outgoingItems ?? transfer.TransferItems.ToList();

                var transferOriginatingLocation = transfer.OriginatingLocation;
                foreach (var transferItem in tranItems)
                {
                    var quantityRequired = transferItem.QuantityAtPickup * transferItem.Unit.QuantityOfBaseunit;
                    var inventoryDetail = transferItem.TraderItem.InventoryDetails.FirstOrDefault(e => e.Location.Id == transferOriginatingLocation.Id);
                    if (inventoryDetail != null)
                    {
                        await FindAllNeededInventory(transferItem, transferItem.TraderItem, inventoryDetail, quantityRequired, transferOriginatingLocation, user, transfer.Sale, listManufactureProductModel);
                    }
                }
                //System.Diagnostics.Debug.WriteLine("FindAllNeededInventory End:" + DateTime.Now.ToString());

                #region Save All into database

                dbContext.SaveChanges();

                #endregion Save All into database

                foreach (var item in listManufactureProductModel)
                {
                    var transferRule = new TraderTransfersRules(dbContext);

                    await transferRule.IncomingInventory(item.tradTransferIn, user, null, item.compoundItemUnitCost, sendToQueue = false);

                    //Call Bookkeeping Integration for Manufacturing
                    var bkIntegrationRule = new BookkeepingIntegrationRules(dbContext);
                    bkIntegrationRule.AddManufacturingJournalEntry(user, transfer.OriginatingLocation.Domain, item.tradTransferIn, item.tradTransferOut, transfer.Sale, null);
                    //Add ALL the ingredients for decremented
                    tranItems.AddRange(item.tradTransferOut.TransferItems);
                }

                foreach (var transferItem in tranItems)
                {
                    // Get the correct InventoryDetail
                    // This is Outgoing =>  DECREASE the Inventory at the Location the Transfer is leaving
                    var inventoryDetail = transferItem.TraderItem.InventoryDetails.FirstOrDefault(e => e.Location.Id == transfer.OriginatingLocation.Id);

                    if (inventoryDetail == null) continue;

                    //Get the Quantity of items, in Base units, specified in the TraderTransferItem.Quantity
                    var totalBatchQuantityRequired = transferItem.QuantityAtPickup * transferItem.Unit.QuantityOfBaseunit;

                    DecrementInventoryDetail(inventoryDetail, totalBatchQuantityRequired, user, transferItem);
                }

                #region Save All DecrementInventoryDetail into database

                dbContext.SaveChanges();

                #endregion Save All DecrementInventoryDetail into database

                //Add referencs to manufacturing transfers
                var domainId = transfer.OriginatingLocation.Domain.Id;
                foreach (var item in listManufactureProductModel)
                {
                    item.tradTransferOut.Reference = new TraderReferenceRules(dbContext).GetNewReference(domainId, TraderReferenceType.Transfer);
                    item.tradTransferIn.Reference = new TraderReferenceRules(dbContext).GetNewReference(domainId, TraderReferenceType.Transfer);
                }
                dbContext.SaveChanges();

                // Do the bookkeeping
                //The Journal Entries for Sales Transfers are to be created after the Transfers for Sales have been saved to the database when the status of the Transfer is 'Picked Up'
                if (transfer.Sale != null)
                    new BookkeepingIntegrationRules(dbContext).AddSaleTransferJournalEntry(user, transfer, transfer.Sale.SalesChannel.GetDescription());
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, user.Id, transfer, user);
            }
        }

        private void DecrementInventoryDetail(InventoryDetail inventoryDetail,
                                       decimal totalBatchQuantityRequired,
                                       ApplicationUser user,
                                       TraderTransferItem transferItem)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, inventoryDetail, totalBatchQuantityRequired, user, transferItem);

                #region Logging

                // Start Log
                var newInventoryUpdateLog = new InventoryUpdateLog
                {
                    AssociatedInventoryDetail = inventoryDetail,
                    AssociatedItem = inventoryDetail.Item,
                    AssociatedLocation = inventoryDetail.Location,
                    AssociatedTransfer = transferItem.AssociatedTransfer,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Domain = inventoryDetail.Location.Domain,
                    IsComplete = false,
                    IsTransferIn = false//false a Transfer Out is causing the update
                };

                #endregion Logging

                // Get a collection of the Ids for the Unused batches associated with the Inventory Detail
                var unusedBatchIds = dbContext.UnusedBatchesView.Where(e => e.InventoryId == inventoryDetail.Id).Select(b => b.BatchId).ToList();

                // Set up the flag to stop emptying the batches
                var canStop = false;
                var remainder = totalBatchQuantityRequired;

                // Go through each unused batch
                // use up the batches until th equantity required has been fulfilled
                foreach (var unusedBatchId in unusedBatchIds)
                {
                    var inventoryBatch = dbContext.InventoryBatches.Find(unusedBatchId);
                    var newBatch = new Batch();
                    newBatch.UnusedQuantity = newBatch.OriginalQuantity;
                    newBatch.CostPerUnit = inventoryBatch.CostPerUnit;
                    newBatch.CreatedDate = DateTime.UtcNow;
                    newBatch.CreatedBy = user;
                    newBatch.LastUpdatedDate = newBatch.CreatedDate;
                    newBatch.LastUpdatedBy = user;
                    newBatch.CurrentBatchValue = newBatch.UnusedQuantity * newBatch.CostPerUnit;
                    newBatch.Direction = BatchDirection.Out;
                    newBatch.InventoryDetail = inventoryDetail;
                    newBatch.ParentTransferItem = transferItem;

                    if (remainder <= inventoryBatch.UnusedQuantity)
                    {
                        //We have found the last inventory batch needed to fulfill the transfer
                        newBatch.OriginalQuantity = remainder;
                        inventoryBatch.UnusedQuantity = inventoryBatch.UnusedQuantity - remainder;
                        inventoryBatch.CurrentBatchValue = inventoryBatch.UnusedQuantity * inventoryBatch.CostPerUnit;
                        inventoryBatch.LastUpdatedDate = DateTime.UtcNow;
                        inventoryBatch.LastUpdatedBy = user;
                        canStop = true;
                    }
                    else
                    {
                        newBatch.OriginalQuantity = inventoryBatch.UnusedQuantity;
                        inventoryBatch.UnusedQuantity = 0;
                        inventoryBatch.CurrentBatchValue = inventoryBatch.UnusedQuantity * inventoryBatch.CostPerUnit;
                        inventoryBatch.LastUpdatedDate = DateTime.UtcNow;
                        inventoryBatch.LastUpdatedBy = user;
                        remainder -= newBatch.OriginalQuantity;
                    }

                    // Set the InventoryDetail property of the InventoryBatch
                    // DO NOT ADD THE INVENTORYBATCH TO THE INVENTORYDETAIL.INVENTORYBATCHES - REALLY, REALLY INEFFICIENT
                    inventoryBatch.InventoryDetail = inventoryDetail;

                    //Update InventoryDetail (Explanation below in section (7) InventoryDetail)
                    inventoryDetail.CurrentInventoryLevel -= newBatch.OriginalQuantity;
                    inventoryDetail.LastUpdatedDate = DateTime.UtcNow;
                    inventoryDetail.LastUpdatedBy = user;

                    #region Logging

                    //Add an InventoryDetail log Explanation below in section (9) Inventory Detail Log)
                    var newInventoryDetailLog = new InventoryDetailLog
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        CurrentInventoryLevel = inventoryDetail.CurrentInventoryLevel,
                        InventoryBatches = new List<Batch>(),
                        AverageCost = inventoryDetail.AverageCost,
                        CurrentRecipe = inventoryDetail.CurrentRecipe,
                        Item = inventoryDetail.Item,
                        LastUpdatedBy = user,
                        LastUpdatedDate = DateTime.UtcNow,
                        LatestCost = inventoryDetail.LatestCost,
                        Location = inventoryDetail.Location,
                        Logs = new List<InventoryDetailLog>(),
                        MaxInventoryLevel = inventoryDetail.MaxInventoryLevel,
                        MinInventorylLevel = inventoryDetail.MinInventorylLevel,
                        ParentInventoryDetail = inventoryDetail,
                        ReorderLevel = 0,
                        ReorderUnit = transferItem.Unit,
                        UnitCost = 0
                    };

                    //Add a batch log (Explanation below in section (8) BatchLog)
                    var newBatchLog = new BatchLog
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        CurrentBatchValue = newBatch.CurrentBatchValue,
                        Direction = BatchDirection.Out,
                        InventoryDetail = inventoryDetail,
                        LasteUpdatedBy = user,
                        LasteUpdatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow,
                        CostPerUnit = newBatch.CostPerUnit,
                        OriginalQuantity = newBatch.OriginalQuantity,
                        ParentBatch = inventoryBatch,
                        ParentTransferItem = transferItem,
                        Reason = BatchLogReason.TransferOut,
                        UnusedQuantity = newBatch.UnusedQuantity
                    };

                    #endregion Logging

                    dbContext.InventoryBatches.Add(newBatch);
                    dbContext.InventoryDetailLogs.Add(newInventoryDetailLog);
                    dbContext.InventoryBatchLogs.Add(newBatchLog);

                    //Add the outgoing batch to the TraderTransferItem
                    if (transferItem.InventoryBatches == null)
                        transferItem.InventoryBatches = new List<Batch>();
                    transferItem.InventoryBatches.Add(newBatch);

                    if (canStop)
                        break;
                }

                #region Logging

                // Update Log
                newInventoryUpdateLog.CompletedDate = DateTime.UtcNow;
                newInventoryUpdateLog.IsComplete = true;

                #endregion Logging

                dbContext.InventoryUpdateLogs.Add(newInventoryUpdateLog);

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, inventoryDetail, totalBatchQuantityRequired, user, transferItem);
            }
        }

        private void CreateInventedInventoryBatch(InventoryDetail inventoryDetail,
                                                  decimal inventoryRequiredCount,
                                                  ApplicationUser user,
                                                  TraderTransferItem transferItem)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, inventoryDetail, inventoryRequiredCount, user, transferItem);
                var inventedInventoryBatch = new Batch();
                inventedInventoryBatch.CreatedDate = DateTime.UtcNow;
                inventedInventoryBatch.LastUpdatedDate = inventedInventoryBatch.CreatedDate;
                inventedInventoryBatch.CreatedBy = user;
                inventedInventoryBatch.LastUpdatedBy = user;
                inventedInventoryBatch.InventoryDetail = inventoryDetail;
                inventedInventoryBatch.IsInvented = true;
                inventedInventoryBatch.Direction = BatchDirection.In;
                inventedInventoryBatch.OriginalQuantity = inventoryRequiredCount;
                inventedInventoryBatch.UnusedQuantity = inventoryRequiredCount;
                inventedInventoryBatch.CostPerUnit = inventoryDetail.AverageCost;
                inventedInventoryBatch.CurrentBatchValue = inventedInventoryBatch.CostPerUnit * inventedInventoryBatch.UnusedQuantity;

                // Update the inventory count
                inventoryDetail.CurrentInventoryLevel += inventoryRequiredCount;

                // Add the batch
                dbContext.InventoryBatches.Add(inventedInventoryBatch);

                #region Logging

                // Start Log
                var newInventoryUpdateLog = new InventoryUpdateLog
                {
                    AssociatedInventoryDetail = inventoryDetail,
                    AssociatedItem = inventoryDetail.Item,
                    AssociatedLocation = inventoryDetail.Location,
                    AssociatedTransfer = transferItem.AssociatedTransfer,
                    CreatedBy = user,
                    CreatedDate = inventedInventoryBatch.CreatedDate,
                    Domain = inventoryDetail.Location.Domain,
                    IsComplete = true,
                    IsTransferIn = true,
                    CompletedDate = inventedInventoryBatch.CreatedDate
                };

                //Add an InventoryDetail log
                var newInventoryDetailLog = new InventoryDetailLog
                {
                    CreatedBy = user,
                    CreatedDate = inventedInventoryBatch.CreatedDate,
                    CurrentInventoryLevel = inventoryDetail.CurrentInventoryLevel,
                    InventoryBatches = new List<Batch>(),
                    AverageCost = inventoryDetail.AverageCost,
                    CurrentRecipe = inventoryDetail.CurrentRecipe,
                    Item = inventoryDetail.Item,
                    LastUpdatedBy = user,
                    LastUpdatedDate = inventedInventoryBatch.CreatedDate,
                    LatestCost = inventoryDetail.LatestCost,
                    Location = inventoryDetail.Location,
                    Logs = new List<InventoryDetailLog>(),
                    MaxInventoryLevel = inventoryDetail.MaxInventoryLevel,
                    MinInventorylLevel = inventoryDetail.MinInventorylLevel,
                    ParentInventoryDetail = inventoryDetail,
                    ReorderLevel = 0,
                    ReorderUnit = transferItem.Unit,
                    UnitCost = 0
                };

                //Add a batch log
                var newBatchLog = new BatchLog
                {
                    CreatedBy = user,
                    CreatedDate = inventedInventoryBatch.CreatedDate,
                    CurrentBatchValue = inventedInventoryBatch.CurrentBatchValue,
                    Direction = inventedInventoryBatch.Direction,
                    InventoryDetail = inventoryDetail,
                    LasteUpdatedBy = user,
                    LasteUpdatedDate = inventedInventoryBatch.CreatedDate,
                    LastUpdatedDate = inventedInventoryBatch.CreatedDate,
                    CostPerUnit = inventedInventoryBatch.CostPerUnit,
                    OriginalQuantity = inventedInventoryBatch.OriginalQuantity,
                    ParentBatch = inventedInventoryBatch,
                    ParentTransferItem = transferItem,
                    Reason = BatchLogReason.StockAdjustmentUp,
                    UnusedQuantity = inventedInventoryBatch.UnusedQuantity
                };

                dbContext.InventoryBatches.Add(inventedInventoryBatch);
                dbContext.InventoryDetailLogs.Add(newInventoryDetailLog);
                dbContext.InventoryBatchLogs.Add(newBatchLog);

                #endregion Logging

                //Save the changes to the database
                //dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, user.Id, inventoryDetail, inventoryRequiredCount, user, transferItem);
            }
        }

        private async Task FindAllNeededInventory(TraderTransferItem referenceTransferItem,
                                            TraderItem traderItem,
                                            InventoryDetail inventoryDetail,
                                            decimal quantityRequired,
                                            TraderLocation transferOriginatingLocation,
                                            ApplicationUser user,
                                            TraderSale sale,
                                            List<ManufactureProductModel> manufactureProducts
                                            )
        {
            //Check if inventory is required and then if there is sufficient inventory
            if (inventoryDetail == null) return;
            var _unusedQuantity = await GetUnusedInventoryQuantity(inventoryDetail.Id);

            #region In Inventory if Quantity Required > _unusedQuantity

            if (quantityRequired > _unusedQuantity)
            {
                // Inventory MUST BE CREATED
                // Calculate how much inventory MUST BE CREATED
                var inventoryQuantityToBeCreated = quantityRequired - _unusedQuantity;

                // Check if the item is a compound item
                // If it is then the inventory of its ingredients must be checked
                if (traderItem.IsCompoundProduct)
                {
                    var currentRecipe = inventoryDetail?.CurrentRecipe ?? new Recipe();
                    var itemIngredients = currentRecipe.Ingredients;

                    ManufactureProductModel manufactureProduct = new ManufactureProductModel();

                    #region Transfer out,In for the ingredients

                    manufactureProduct.tradTransferOut = new TraderTransfer
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        DestinationLocation = null,
                        OriginatingLocation = transferOriginatingLocation,
                        ManufacturingJob = null,
                        Workgroup = null,
                        Address = null,
                        Contact = null,
                        Status = TransferStatus.Delivered,
                        Reason = TransferReasonEnum.ManufacturingJobAdjustment
                    };
                    manufactureProduct.tradTransferIn = new TraderTransfer
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        DestinationLocation = transferOriginatingLocation,
                        OriginatingLocation = null,
                        ManufacturingJob = null,
                        Workgroup = null,
                        Address = null,
                        Contact = null,
                        Status = TransferStatus.Delivered,
                        Reason = TransferReasonEnum.ManufacturingJobAdjustment
                    };
                    var tradTransferItemIn = new TraderTransferItem
                    {
                        Unit = traderItem.Units.FirstOrDefault(q => q.IsBase),
                        QuantityAtPickup = inventoryQuantityToBeCreated,
                        QuantityAtDelivery = inventoryQuantityToBeCreated,
                        TransactionItem = null, //Not related to Sale or Purchase
                        TraderItem = traderItem,
                        AssociatedTransfer = manufactureProduct.tradTransferIn
                    };
                    manufactureProduct.tradTransferIn.TransferItems.Add(tradTransferItemIn);

                    #endregion Transfer out,In for the ingredients

                    //Get a list of the ingredients that are not compound items and sort out their inventory
                    var itemsNoCompoundProduct = itemIngredients.Where(s => !s.SubItem.IsCompoundProduct).ToList();

                    foreach (var ingredientItem in itemsNoCompoundProduct)
                    {
                        //Calculate the quantity of inventry required for the ingredient
                        var ingredientInventoryDetail = ingredientItem.SubItem.InventoryDetails.FirstOrDefault(e => e.Location.Id == transferOriginatingLocation.Id);
                        var ingredientInventoryQuantityRequired = inventoryQuantityToBeCreated * (ingredientItem.Quantity * ingredientItem.Unit.QuantityOfBaseunit);

                        //Calculate the unused quantity based on the unused batches NOT the CurrentInventoryLevel
                        var ingredientUnusedQuantity = await GetUnusedInventoryQuantity(ingredientInventoryDetail.Id);

                        //Check if there is enough quantity of the ingredient
                        // If there isn't, invent the necessary inventory
                        if (ingredientInventoryQuantityRequired > ingredientUnusedQuantity)
                        {
                            // The following line of code comment is left in the code for clarity
                            // var ingredientInventoryQuantityToBeCreated = ingredientInventoryQuantityRequired - ingredientInventoryDetail.CurrentInventoryLevel;
                            var ingredientInventoryQuantityToBeCreated = ingredientInventoryQuantityRequired - ingredientUnusedQuantity;

                            #region Add Batch into List the batches

                            var inventedInventoryBatch = new Batch();
                            inventedInventoryBatch.CreatedDate = DateTime.UtcNow;
                            inventedInventoryBatch.LastUpdatedDate = inventedInventoryBatch.CreatedDate;
                            inventedInventoryBatch.CreatedBy = user;
                            inventedInventoryBatch.LastUpdatedBy = user;
                            inventedInventoryBatch.InventoryDetail = ingredientInventoryDetail;
                            inventedInventoryBatch.IsInvented = true;
                            inventedInventoryBatch.Direction = BatchDirection.In;
                            inventedInventoryBatch.OriginalQuantity = ingredientInventoryQuantityToBeCreated;
                            inventedInventoryBatch.UnusedQuantity = ingredientInventoryQuantityToBeCreated;
                            inventedInventoryBatch.CostPerUnit = ingredientInventoryDetail.AverageCost;
                            inventedInventoryBatch.CurrentBatchValue = inventedInventoryBatch.CostPerUnit * inventedInventoryBatch.UnusedQuantity;

                            // Update the inventory count
                            ingredientInventoryDetail.CurrentInventoryLevel += ingredientInventoryQuantityToBeCreated;

                            // Start Log
                            var newInventoryUpdateLog = new InventoryUpdateLog
                            {
                                AssociatedInventoryDetail = inventoryDetail,
                                AssociatedItem = inventoryDetail.Item,
                                AssociatedLocation = inventoryDetail.Location,
                                AssociatedTransfer = referenceTransferItem.AssociatedTransfer,
                                CreatedBy = user,
                                CreatedDate = inventedInventoryBatch.CreatedDate,
                                Domain = inventoryDetail.Location.Domain,
                                IsComplete = true,
                                IsTransferIn = true,
                                CompletedDate = inventedInventoryBatch.CreatedDate
                            };

                            //Add an InventoryDetail log
                            var newInventoryDetailLog = new InventoryDetailLog
                            {
                                CreatedBy = user,
                                CreatedDate = inventedInventoryBatch.CreatedDate,
                                CurrentInventoryLevel = inventoryDetail.CurrentInventoryLevel,
                                InventoryBatches = new List<Batch>(),
                                AverageCost = inventoryDetail.AverageCost,
                                CurrentRecipe = inventoryDetail.CurrentRecipe,
                                Item = inventoryDetail.Item,
                                LastUpdatedBy = user,
                                LastUpdatedDate = inventedInventoryBatch.CreatedDate,
                                LatestCost = inventoryDetail.LatestCost,
                                Location = inventoryDetail.Location,
                                Logs = new List<InventoryDetailLog>(),
                                MaxInventoryLevel = inventoryDetail.MaxInventoryLevel,
                                MinInventorylLevel = inventoryDetail.MinInventorylLevel,
                                ParentInventoryDetail = inventoryDetail,
                                ReorderLevel = 0,
                                ReorderUnit = referenceTransferItem.Unit,
                                UnitCost = 0
                            };

                            //Add a batch log
                            var newBatchLog = new BatchLog
                            {
                                CreatedBy = user,
                                CreatedDate = inventedInventoryBatch.CreatedDate,
                                CurrentBatchValue = inventedInventoryBatch.CurrentBatchValue,
                                Direction = inventedInventoryBatch.Direction,
                                InventoryDetail = inventoryDetail,
                                LasteUpdatedBy = user,
                                LasteUpdatedDate = inventedInventoryBatch.CreatedDate,
                                LastUpdatedDate = inventedInventoryBatch.CreatedDate,
                                CostPerUnit = inventedInventoryBatch.CostPerUnit,
                                OriginalQuantity = inventedInventoryBatch.OriginalQuantity,
                                ParentBatch = inventedInventoryBatch,
                                ParentTransferItem = referenceTransferItem,
                                Reason = BatchLogReason.StockAdjustmentUp,
                                UnusedQuantity = inventedInventoryBatch.UnusedQuantity
                            };

                            dbContext.InventoryBatches.Add(inventedInventoryBatch);
                            dbContext.InventoryDetailLogs.Add(newInventoryDetailLog);
                            dbContext.InventoryBatchLogs.Add(newBatchLog);
                            dbContext.InventoryUpdateLogs.Add(newInventoryUpdateLog);

                            #endregion Add Batch into List the batches
                        }

                        #region Tranfer Out Items of itemsNoCompoundProduct

                        var ingredientTransferItem = new TraderTransferItem
                        {
                            Unit = ingredientItem.SubItem.Units.FirstOrDefault(s => s.IsBase),
                            QuantityAtPickup = ingredientInventoryQuantityRequired,
                            QuantityAtDelivery = ingredientInventoryQuantityRequired,
                            TransactionItem = null,
                            TraderItem = ingredientItem.SubItem,
                            AssociatedTransfer = manufactureProduct.tradTransferOut
                        };
                        manufactureProduct.tradTransferOut.TransferItems.Add(ingredientTransferItem);

                        manufactureProduct.compoundItemUnitCost += (ingredientInventoryDetail.AverageCost * (ingredientItem.Quantity * ingredientItem.Unit.QuantityOfBaseunit));

                        #endregion Tranfer Out Items of itemsNoCompoundProduct
                    }

                    // Get a list of the ingredients that are compound items and sort out their inventory
                    //For any item that IsCompoundProduct
                    var lstCompoundItems = itemIngredients.Where(s => s.SubItem.IsCompoundProduct).ToList();

                    if (lstCompoundItems != null && lstCompoundItems.Any())
                    {
                        foreach (var ingredientItem in lstCompoundItems)
                        {
                            //Calculate the quantity of inventry required for the ingredient
                            var ingredientInventoryDetail = ingredientItem.SubItem.InventoryDetails.FirstOrDefault(e => e.Location.Id == transferOriginatingLocation.Id);
                            var ingredientInventoryQuantityRequired = inventoryQuantityToBeCreated * (ingredientItem.Quantity * ingredientItem.Unit.QuantityOfBaseunit);

                            //Calculate the unused quantity based on the unused batches NOT the CurrentInventoryLevel
                            var ingredientUnusedQuantity = await GetUnusedInventoryQuantity(ingredientInventoryDetail.Id);

                            // The following line of code comment is left in the code for clarity
                            // var ingredientInventoryQuantityToBeCreated = ingredientInventoryQuantityRequired - ingredientInventoryDetail.CurrentInventoryLevel;
                            var ingredientInventoryQuantityToBeCreated = ingredientInventoryQuantityRequired - ingredientUnusedQuantity;

                            //Add result collection to ingredients
                            await FindAllNeededInventory(referenceTransferItem, ingredientItem.SubItem, ingredientInventoryDetail, ingredientInventoryQuantityToBeCreated, transferOriginatingLocation, user, sale, manufactureProducts);

                            #region Tranfer Out Items of lstCompoundItems

                            var ingredientTransferItem = new TraderTransferItem
                            {
                                Unit = ingredientItem.SubItem.Units.FirstOrDefault(s => s.IsBase),
                                QuantityAtPickup = ingredientInventoryQuantityRequired,
                                QuantityAtDelivery = ingredientInventoryQuantityRequired,
                                TransactionItem = null,
                                TraderItem = ingredientItem.SubItem,
                                AssociatedTransfer = manufactureProduct.tradTransferOut
                            };
                            manufactureProduct.tradTransferOut.TransferItems.Add(ingredientTransferItem);

                            manufactureProduct.compoundItemUnitCost += (ingredientInventoryDetail.AverageCost * (ingredientItem.Quantity * ingredientItem.Unit.QuantityOfBaseunit));

                            #endregion Tranfer Out Items of lstCompoundItems
                        }
                    }

                    #region Logging TransferLog and TransferProcessLog

                    var transferLogIn = new TransferLog
                    {
                        Address = null,
                        AssociatedTransfer = manufactureProduct.tradTransferIn,
                        Contact = null,
                        CreatedDate = DateTime.UtcNow,
                        Sale = sale,
                        Status = manufactureProduct.tradTransferIn.Status,
                        UpdatedBy = user,
                        AssociatedShipment = null,
                        DestinationLocation = manufactureProduct.tradTransferIn.DestinationLocation,
                        OriginatingLocation = manufactureProduct.tradTransferIn.OriginatingLocation,
                        TransferItems = manufactureProduct.tradTransferIn.TransferItems,
                        Workgroup = null
                    };

                    var transferProcessLogIn = new TransferProcessLog
                    {
                        AssociatedTransfer = manufactureProduct.tradTransferIn,
                        AssociatedTransferLog = transferLogIn,
                        TransferStatus = manufactureProduct.tradTransferIn.Status,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow
                    };

                    var transferLogOut = new TransferLog
                    {
                        Address = null,
                        AssociatedTransfer = manufactureProduct.tradTransferOut,
                        Contact = null,
                        CreatedDate = DateTime.UtcNow,
                        Sale = sale,
                        Status = manufactureProduct.tradTransferOut.Status,
                        UpdatedBy = user,
                        AssociatedShipment = null,
                        DestinationLocation = manufactureProduct.tradTransferOut.DestinationLocation,
                        OriginatingLocation = manufactureProduct.tradTransferOut.OriginatingLocation,
                        TransferItems = manufactureProduct.tradTransferOut.TransferItems,
                        Workgroup = null
                    };

                    var transferProcessLogOut = new TransferProcessLog
                    {
                        AssociatedTransfer = manufactureProduct.tradTransferOut,
                        AssociatedTransferLog = transferLogOut,
                        TransferStatus = manufactureProduct.tradTransferOut.Status,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow
                    };

                    dbContext.TraderTransferProcessLogs.Add(transferProcessLogIn);
                    dbContext.Entry(transferProcessLogIn).State = EntityState.Added;

                    dbContext.TraderTransferProcessLogs.Add(transferProcessLogOut);
                    dbContext.Entry(transferProcessLogOut).State = EntityState.Added;

                    #endregion Logging TransferLog and TransferProcessLog

                    #region add ManufactureProductModel to List<ManufactureProductModel>

                    //Transfer Out
                    dbContext.TraderTransfers.Add(manufactureProduct.tradTransferOut);
                    dbContext.Entry(manufactureProduct.tradTransferOut).State = EntityState.Added;
                    //Transfer In
                    dbContext.TraderTransfers.Add(manufactureProduct.tradTransferIn);
                    dbContext.Entry(manufactureProduct.tradTransferIn).State = EntityState.Added;
                    manufactureProducts.Add(manufactureProduct);

                    #endregion add ManufactureProductModel to List<ManufactureProductModel>
                }
                else
                {
                    #region Add Batch into List the batches

                    var inventedInventoryBatch = new Batch();
                    inventedInventoryBatch.CreatedDate = DateTime.UtcNow;
                    inventedInventoryBatch.LastUpdatedDate = inventedInventoryBatch.CreatedDate;
                    inventedInventoryBatch.CreatedBy = user;
                    inventedInventoryBatch.LastUpdatedBy = user;
                    inventedInventoryBatch.InventoryDetail = inventoryDetail;
                    inventedInventoryBatch.IsInvented = true;
                    inventedInventoryBatch.Direction = BatchDirection.In;
                    inventedInventoryBatch.OriginalQuantity = inventoryQuantityToBeCreated;
                    inventedInventoryBatch.UnusedQuantity = inventoryQuantityToBeCreated;
                    inventedInventoryBatch.CostPerUnit = inventoryDetail.AverageCost;
                    inventedInventoryBatch.ParentTransferItem = referenceTransferItem;
                    inventedInventoryBatch.CurrentBatchValue = inventedInventoryBatch.CostPerUnit * inventedInventoryBatch.UnusedQuantity;

                    // Update the inventory count
                    inventoryDetail.CurrentInventoryLevel += inventoryQuantityToBeCreated;
                    // Start Log
                    var newInventoryUpdateLog = new InventoryUpdateLog
                    {
                        AssociatedInventoryDetail = inventoryDetail,
                        AssociatedItem = inventoryDetail.Item,
                        AssociatedLocation = inventoryDetail.Location,
                        AssociatedTransfer = referenceTransferItem.AssociatedTransfer,
                        CreatedBy = user,
                        CreatedDate = inventedInventoryBatch.CreatedDate,
                        Domain = inventoryDetail.Location.Domain,
                        IsComplete = true,
                        IsTransferIn = true,
                        CompletedDate = inventedInventoryBatch.CreatedDate
                    };

                    //Add an InventoryDetail log
                    var newInventoryDetailLog = new InventoryDetailLog
                    {
                        CreatedBy = user,
                        CreatedDate = inventedInventoryBatch.CreatedDate,
                        CurrentInventoryLevel = inventoryDetail.CurrentInventoryLevel,
                        InventoryBatches = new List<Batch>(),
                        AverageCost = inventoryDetail.AverageCost,
                        CurrentRecipe = inventoryDetail.CurrentRecipe,
                        Item = inventoryDetail.Item,
                        LastUpdatedBy = user,
                        LastUpdatedDate = inventedInventoryBatch.CreatedDate,
                        LatestCost = inventoryDetail.LatestCost,
                        Location = inventoryDetail.Location,
                        Logs = new List<InventoryDetailLog>(),
                        MaxInventoryLevel = inventoryDetail.MaxInventoryLevel,
                        MinInventorylLevel = inventoryDetail.MinInventorylLevel,
                        ParentInventoryDetail = inventoryDetail,
                        ReorderLevel = 0,
                        ReorderUnit = referenceTransferItem.Unit,
                        UnitCost = 0
                    };

                    //Add a batch log
                    var newBatchLog = new BatchLog
                    {
                        CreatedBy = user,
                        CreatedDate = inventedInventoryBatch.CreatedDate,
                        CurrentBatchValue = inventedInventoryBatch.CurrentBatchValue,
                        Direction = inventedInventoryBatch.Direction,
                        InventoryDetail = inventoryDetail,
                        LasteUpdatedBy = user,
                        LasteUpdatedDate = inventedInventoryBatch.CreatedDate,
                        LastUpdatedDate = inventedInventoryBatch.CreatedDate,
                        CostPerUnit = inventedInventoryBatch.CostPerUnit,
                        OriginalQuantity = inventedInventoryBatch.OriginalQuantity,
                        ParentBatch = inventedInventoryBatch,
                        ParentTransferItem = referenceTransferItem,
                        Reason = BatchLogReason.StockAdjustmentUp,
                        UnusedQuantity = inventedInventoryBatch.UnusedQuantity
                    };

                    dbContext.InventoryBatches.Add(inventedInventoryBatch);
                    dbContext.InventoryDetailLogs.Add(newInventoryDetailLog);
                    dbContext.InventoryBatchLogs.Add(newBatchLog);
                    dbContext.InventoryUpdateLogs.Add(newInventoryUpdateLog);

                    #endregion Add Batch into List the batches
                }
            }

            #endregion In Inventory if Quantity Required > _unusedQuantity
        }

        private async Task<decimal> GetUnusedInventoryQuantity(int inventoryId)
        {
            try
            {
                return (await dbContext.UnusedInventoriesView.FirstOrDefaultAsync(e => e.Id == inventoryId))?.CurrentInventory ?? 0;
            }
            catch
            {
                return 0M;
            }
        }

        private List<CompressTraderTransferItem> CompressTraderTransferItems(List<TraderTransferItem> transferItems)
        {
            return transferItems.GroupBy(s => s.TraderItem, (key, g) =>
            new CompressTraderTransferItem
            {
                TraderItem = key,
                TotalQuantity = g.Sum(p => p.QuantityAtPickup * p.Unit.QuantityOfBaseunit),
                TransactionItem = g.FirstOrDefault()?.TransactionItem
            }).ToList();

            //new CompressTraderTransferItem { TraderItem = key, TotalQuantity = g.Sum(p => p.QuantityAtPickup * p.Unit.QuantityOfBaseunit),
            //    TransactionItem = g.FirstOrDefault()?.TransactionItem }).ToList();
        }

        public DataTablesResponse GetReportTransfers(IDataTablesRequest requestModel, string keyword,
            string datetime, UserSetting dateTimeFormat, string currentUserId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, keyword, datetime, dateTimeFormat, domainId);

                var query = dbContext.TraderTransfers.Where(q => (q.DestinationLocation.Domain.Id == domainId || q.OriginatingLocation.Domain.Id == domainId));
                int totalRecords;

                #region Filter

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q =>
                        (q.Workgroup != null && q.Workgroup.Name.ToLower().Contains(keyword))
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword))
                        //|| (q.Sale != null && q.Sale.Purchaser != null && q.Sale.Purchaser.Name.ToLower().Contains(keyword))
                        || (q.OriginatingLocation != null && q.OriginatingLocation.Name.ToLower().Contains(keyword))
                        || (q.Purchase != null && q.Purchase.Vendor != null && q.Purchase.Vendor.Name.ToLower().Contains(keyword))
                        || (q.DestinationLocation != null && q.DestinationLocation.Name.ToLower().Contains(keyword))
                    );

                if (!string.IsNullOrEmpty(datetime))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(dateTimeFormat.Timezone);
                    datetime.ConvertDaterangeFormat(dateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }
                totalRecords = query.Count();

                #endregion Filter

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "FullRef":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Reason":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reason" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                var dataJson = new List<TransferCustom>();

                foreach (var item in list)
                {
                    var routeStr = "";
                    string transferFrom;
                    string transferTo;
                    if (item.Sale != null)
                    {
                        transferFrom = item.OriginatingLocation.Name;
                        transferTo = item.Sale.Purchaser?.Name;
                        routeStr = item.Sale.Purchaser?.QbicleUser?.Id ?? "";
                    }
                    else if (item.Purchase != null)
                    {
                        transferFrom = item.Purchase.Vendor.Name;
                        transferTo = item.DestinationLocation.Name;
                    }
                    else if (item.OriginatingLocation != null && item.DestinationLocation != null)
                    {
                        transferFrom = item.OriginatingLocation.Name;
                        transferTo = item.DestinationLocation.Name;
                    }
                    else
                    {
                        transferFrom = item.OriginatingLocation?.Name;
                        transferTo = item.DestinationLocation?.Name;
                    }

                    dataJson.Add(new TransferCustom()
                    {
                        Id = item.Id,
                        Key = item.Key,
                        FullRef = item.Reference?.FullRef ?? item.Id.ToString(),
                        Date = item.CreatedDate.ToString(dateTimeFormat.DateFormat),
                        To = transferTo,
                        From = transferFrom,
                        Route = routeStr,
                        Reason = item.Reason.GetDescription(),
                        Status = item.Status.ToString(),
                        AllowEdit = (item.Workgroup != null
                        && item.Workgroup.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                        && item.Workgroup.Members.Any(q => q.Id == currentUserId)
                        )
                    });
                }
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword, datetime, dateTimeFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }
    }
}