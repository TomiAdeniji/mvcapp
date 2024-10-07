using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.Returns;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.BusinessRules.Trader
{
    public class TraderSalesReturnRules
    {
        ApplicationDbContext dbContext;

        public TraderSalesReturnRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<TraderReturn> GetByLocation(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                return dbContext.TraderReturns.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId)
                    .ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, domainId);
                return new List<TraderReturn>();
            }
        }


        public TraderReturn GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderReturns.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderReturn();
            }
        }


        public DataTablesResponse TraderSaleReturnSearch(IDataTablesRequest requestModel, UserSetting user, int locationId,
            string keyword, int wgId, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, requestModel, user, locationId,
                        keyword, wgId, timeZone);
                //Get the filtered sales, get all sales not just the approved sales
                string dateFormat = string.IsNullOrEmpty(user.DateFormat) ? "dd/MM/yyyy" : user.DateFormat;
                var sales = FilteredSalesReturn(locationId, keyword, wgId, isFilteredByApproved: false);

                if (sales == null)
                {
                    return null;
                }

                var totalSale = sales.Count();


                var workGroupIds = dbContext.WorkGroups.Where(q =>
                    q.Location.Id == locationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderSaleProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).Select(q => q.Id).ToList();



                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "FullRef":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" +
                                             (column.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? " asc"
                                                 : " desc");
                            break;
                        case "WorkgroupName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Workgroup.Name" +
                                             (column.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? " asc"
                                                 : " desc");
                            break;
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? " asc"
                                                 : " desc");
                            break;
                        case "SalesRef":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SalesChannel" +
                                             (column.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? " asc"
                                                 : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? " asc"
                                                 : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                sales = sales.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion

                #region Paging

                var list = sales.Skip(requestModel.Start).Take(requestModel.Length).ToList();


                #endregion

                var dataJson = list.Select(q => new TraderSaleReturnCustom
                {
                    Id = q.Id,
                    FullRef = q.Reference?.FullRef,
                    WorkgroupName = q.Workgroup?.Name,
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateFormat),
                    SalesRef = q.Sale.Reference?.FullRef,
                    SaleRefId = q.Sale.Id,
                    SaleRefKey = q.Sale?.Key ?? "",
                    Status = q.Status.ToString(),
                    AllowEdit = q.Workgroup != null && workGroupIds.Contains(q.Workgroup.Id)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSale, totalSale);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, requestModel, user, locationId, keyword, wgId, timeZone);
                return null;
            }

        }


        private IQueryable<TraderReturn> FilteredSalesReturn(int locationId, string keyword = "", int workGroupId = 0,
            bool isFilteredByApproved = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, keyword, workGroupId, isFilteredByApproved);
                //Create sales and filter by location
                var sales = dbContext.TraderReturns.Where(l => l.Location.Id == locationId);
                //Filter by workgroup
                if (workGroupId > 0)
                    sales = sales.Where(q => q.Workgroup.Id == workGroupId);
                //If the sales must be filered by Approved
                if (isFilteredByApproved)
                {
                    sales = sales.Where(q => q.Status == TraderReturnStatusEnum.Approved);
                }


                //Filter by keyword
                keyword = keyword.ToLower().Trim();

                if (!string.IsNullOrEmpty(keyword))
                    sales = sales.Where(q =>
                        (q.Workgroup != null && q.Workgroup.Name.ToLower().Contains(keyword))
                        || ((q.CreatedDate.Day < 10
                                ? ("0" + q.CreatedDate.Day.ToString())
                                : q.CreatedDate.Day.ToString()) + "/" +
                            (q.CreatedDate.Month < 10
                                ? ("0" + q.CreatedDate.Month.ToString())
                                : q.CreatedDate.Month.ToString()) + "/" + q.CreatedDate.Year).Contains(keyword)
                        || q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword)
                        || q.Sale.Reference != null && q.Sale.Reference.FullRef.ToLower().Contains(keyword)
                    );

                return sales;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, keyword, workGroupId, isFilteredByApproved);
                return null;
            }

        }


        public ReturnJsonModel SaveTraderSaleReturn(TraderReturn saleReturn, string userId, int domainId, string originatingConnectionId = "")
        {

            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, saleReturn);
                    var domain = new DomainRules(dbContext).GetDomainById(domainId);
                    var user = dbContext.QbicleUser.Find(userId);

                    if (saleReturn.Id == 0)
                    {
                        saleReturn.CreatedBy = user;
                        saleReturn.CreatedDate = DateTime.UtcNow;
                    }

                    saleReturn.LastUpdatedBy = user;
                    saleReturn.LastUpdatedDate = DateTime.UtcNow;


                    if (saleReturn.Reference != null)
                        saleReturn.Reference = new TraderReferenceRules(dbContext).GetById(saleReturn.Reference.Id);

                    saleReturn.Location = domain.TraderLocations.FirstOrDefault(q => q.Id == saleReturn.Location.Id);

                    saleReturn.Workgroup = dbContext.WorkGroups.Find(saleReturn.Workgroup.Id);
                    saleReturn.Sale = saleReturn.Location.Sales.FirstOrDefault(s => s.Id == saleReturn.Sale.Id);
                    //saleReturn.Sale = new TraderSaleRules(dbContext).GetById(saleReturn.Sale.Id);

                    if (saleReturn.ReturnItems.Count > 0)
                    {
                        foreach (var item in saleReturn.ReturnItems)
                        {
                            if (item.Id == 0)
                            {
                                item.CreatedDate = DateTime.UtcNow;
                                item.CreatedBy = user;
                                item.LastUpdatedBy = user;
                                item.LastUpdatedDate = DateTime.UtcNow;
                            }
                            else
                            {
                                item.LastUpdatedBy = user;
                                item.LastUpdatedDate = DateTime.UtcNow;
                            }

                            item.SaleItem = dbContext.TraderSaleItems.Find(item.SaleItem.Id);
                        }
                    }


                    if (saleReturn.Id == 0)
                    {
                        dbContext.Entry(saleReturn).State = EntityState.Added;
                        dbContext.TraderReturns.Add(saleReturn);
                        dbContext.SaveChanges();

                        //Create return items log
                        foreach (var item in saleReturn.ReturnItems)
                        {
                            var returnItemLog = new ReturnItemLog
                            {
                                Id = 0,
                                SaleItem = item.SaleItem,
                                Credit = item.Credit,
                                CreatedDate = item.CreatedDate,
                                LastUpdatedBy = item.LastUpdatedBy,
                                IsReturnedToInventory = item.IsReturnedToInventory,
                                LastUpdatedDate = DateTime.Now,
                                CreatedBy = user,
                                ReturnQuantity = item.ReturnQuantity,
                                ParentReturnItem = item,
                                Return = saleReturn
                            };
                            dbContext.Entry(returnItemLog).State = EntityState.Added;
                            dbContext.ReturnItemLogs.Add(returnItemLog);
                        }

                        dbContext.SaveChanges();
                    }
                    else
                    {
                        var saleReturnDb = dbContext.TraderReturns.Find(saleReturn.Id);
                        if (saleReturn.Reference != null)
                            saleReturnDb.Reference = saleReturn.Reference;
                        saleReturnDb.Status = saleReturn.Status;
                        saleReturnDb.LastUpdatedBy = user;
                        saleReturnDb.LastUpdatedDate = DateTime.UtcNow;
                        saleReturnDb.Workgroup = saleReturn.Workgroup;
                        dbContext.SaveChanges();

                        // validation check
                        // if update as new sale selected then remove all return items and re add new return items
                        if (saleReturnDb.Sale.Id != saleReturn.Sale.Id)
                        {
                            var returnItemLogs = saleReturn.ReturnItems.SelectMany(e => e.Logs);
                            dbContext.ReturnItemLogs.RemoveRange(returnItemLogs);
                            saleReturnDb.ReturnItems.Clear();
                            dbContext.SaveChanges();
                            //Create return item to update

                            //Create return items log
                            foreach (var item in saleReturn.ReturnItems)
                            {
                                var returnItem = new ReturnItem
                                {
                                    Id = 0,
                                    SaleItem = item.SaleItem,
                                    Credit = item.Credit,
                                    CreatedDate = item.CreatedDate,
                                    Logs = new List<ReturnItemLog>(),
                                    IsReturnedToInventory = item.IsReturnedToInventory,
                                    ReturnQuantity = item.ReturnQuantity,
                                    LastUpdatedBy = item.LastUpdatedBy,
                                    CreatedBy = item.CreatedBy,
                                    LastUpdatedDate = item.LastUpdatedDate,
                                    Return = saleReturn
                                };
                                var returnItemLog = new ReturnItemLog
                                {
                                    Id = 0,
                                    SaleItem = returnItem.SaleItem,
                                    Credit = returnItem.Credit,
                                    CreatedDate = DateTime.UtcNow,
                                    LastUpdatedBy = user,
                                    IsReturnedToInventory = returnItem.IsReturnedToInventory,
                                    LastUpdatedDate = DateTime.UtcNow,
                                    CreatedBy = user,
                                    ReturnQuantity = returnItem.ReturnQuantity,
                                    ParentReturnItem = returnItem,
                                    Return = saleReturn
                                };

                                dbContext.Entry(returnItemLog).State = EntityState.Added;
                                dbContext.ReturnItemLogs.Add(returnItemLog);

                                saleReturnDb.ReturnItems.Add(returnItem);
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            //Update return items
                            var itemsUi = saleReturn.ReturnItems;
                            var itemsDb = saleReturnDb.ReturnItems;

                            var itemsNew = itemsUi.Where(c => c.Id == 0).ToList();

                            var itemsDelete = itemsDb.Where(c => itemsUi.All(d => c.Id != d.Id));

                            var itemsUpdate = itemsUi.Where(c => itemsDb.Any(d => c.Id == d.Id));
                            //remove item deleted
                            foreach (var itemDel in itemsDelete)
                            {
                                saleReturnDb.ReturnItems.Remove(itemDel);
                                dbContext.Entry(saleReturnDb).State = EntityState.Modified;
                                dbContext.SaveChanges();
                            }

                            //update item
                            foreach (var iDb in saleReturnDb.ReturnItems)
                            {
                                var iUpdate = itemsUpdate.FirstOrDefault(e => e.Id == iDb.Id);
                                if (iUpdate == null) continue;
                                iDb.SaleItem = iUpdate.SaleItem;
                                iDb.Credit = iUpdate.Credit;
                                //iDb.CreatedDate = iUpdate.CreatedDate;
                                iDb.Logs = new List<ReturnItemLog>();
                                iDb.IsReturnedToInventory = iUpdate.IsReturnedToInventory;
                                iDb.ReturnQuantity = iUpdate.ReturnQuantity;
                                iDb.LastUpdatedBy = iUpdate.LastUpdatedBy;
                                //iDb.CreatedBy = iUpdate.CreatedBy;
                                iDb.LastUpdatedDate = iUpdate.LastUpdatedDate;
                                iDb.Return = saleReturnDb;

                                dbContext.Entry(iDb).State = EntityState.Modified;
                                dbContext.SaveChanges();

                                var returnItemLog = new ReturnItemLog
                                {
                                    Id = 0,
                                    SaleItem = iDb.SaleItem,
                                    Credit = iDb.Credit,
                                    CreatedDate = DateTime.UtcNow,
                                    LastUpdatedBy = user,
                                    IsReturnedToInventory = iDb.IsReturnedToInventory,
                                    LastUpdatedDate = DateTime.UtcNow,
                                    CreatedBy = user,
                                    ReturnQuantity = iDb.ReturnQuantity,
                                    ParentReturnItem = iDb,
                                    Return = saleReturnDb
                                };
                                dbContext.Entry(returnItemLog).State = EntityState.Added;
                                dbContext.ReturnItemLogs.Add(returnItemLog);
                                dbContext.SaveChanges();
                            }

                            //Add item
                            if (itemsNew.Count > 0)
                            {
                                saleReturnDb.ReturnItems.AddRange(itemsNew);
                                dbContext.Entry(saleReturnDb).State = EntityState.Modified;
                                dbContext.SaveChanges();
                                foreach (var item in itemsNew)
                                {
                                    var returnItemLog = new ReturnItemLog
                                    {
                                        Id = 0,
                                        SaleItem = item.SaleItem,
                                        Credit = item.Credit,
                                        CreatedDate = DateTime.UtcNow,
                                        LastUpdatedBy = user,
                                        IsReturnedToInventory = item.IsReturnedToInventory,
                                        LastUpdatedDate = DateTime.UtcNow,
                                        CreatedBy = user,
                                        ReturnQuantity = item.ReturnQuantity,
                                        ParentReturnItem = item,
                                        Return = saleReturnDb
                                    };
                                    dbContext.Entry(returnItemLog).State = EntityState.Added;
                                    dbContext.ReturnItemLogs.Add(returnItemLog);
                                }

                                dbContext.SaveChanges();


                            }
                        }

                    }


                    var tradSaleReturnDb = dbContext.TraderReturns.Find(saleReturn.Id);
                    //Sale log created
                    var saleLogCreated = new ReturnLog
                    {
                        ParentReturn = tradSaleReturnDb,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Location = tradSaleReturnDb.Location,
                        Workgroup = tradSaleReturnDb.Workgroup,
                        Status = tradSaleReturnDb.Status,
                        LastUpdatedBy = user,
                        LastUpdatedDate = DateTime.UtcNow,
                        ReturnItems = tradSaleReturnDb.ReturnItems,
                        Sale = tradSaleReturnDb.Sale
                    };
                    dbContext.Entry(saleLogCreated).State = EntityState.Added;
                    dbContext.TraderReturnLogs.Add(saleLogCreated);
                    dbContext.SaveChanges();


                    if (tradSaleReturnDb?.ReturnApprovalProcess != null)
                    {
                        transaction.Commit();
                        return result;
                    }

                    if (tradSaleReturnDb == null || tradSaleReturnDb.Status != TraderReturnStatusEnum.PendingReview)
                    {
                        transaction.Commit();
                        return result;
                    }

                    //return approval
                    tradSaleReturnDb.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                    var appDef = dbContext.SalesReturnApprovalDefinitions.FirstOrDefault(w =>
                        w.SaleReturnWorkGroup.Id == tradSaleReturnDb.Workgroup.Id);
                    var refFull = tradSaleReturnDb.Reference == null ? "" : tradSaleReturnDb.Reference.FullRef;

                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = appDef,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                        TraderReturns = new List<TraderReturn> { tradSaleReturnDb },
                        Name = $"Trader Approval for Sale Return #{refFull}",
                        Qbicle = tradSaleReturnDb.Workgroup.Qbicle,
                        Topic = tradSaleReturnDb.Workgroup.Topic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Notes = "",
                        IsVisibleInQbicleDashboard = true,
                        App = QbicleActivity.ActivityApp.Trader
                    };
                    tradSaleReturnDb.ReturnApprovalProcess = approval;
                    tradSaleReturnDb.ReturnApprovalProcess.ApprovalRequestDefinition = appDef;
                    approval.ActivityMembers.AddRange(tradSaleReturnDb.Workgroup.Members);
                    dbContext.Entry(tradSaleReturnDb).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    //transaction.Commit();
                    //Return log
                    //var returnDb = GetById(tradSaleReturnDb.Id);


                    var saleReturnProcessLog = new ReturnProcessLog()
                    {
                        AssociatedReturn = tradSaleReturnDb,
                        AssociatedReturnLog = saleLogCreated,
                        ReturnStatus = tradSaleReturnDb.Status,
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

                    dbContext.ReturnProcessLogs.Add(saleReturnProcessLog);
                    dbContext.Entry(saleReturnProcessLog).State = EntityState.Added;
                    dbContext.SaveChanges();

                    transaction.Commit();

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
                    return result;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, saleReturn, domainId);
                    result.actionVal = 3;
                    result.msg = ex.Message;
                    return result;
                }
            }
        }


        public List<ApprovalStatusTimeline> SaleReturnApprovalStatusTimeline(int id, string timeZone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var logs = dbContext.ReturnProcessLogs.Where(e => e.AssociatedReturn.Id == id)
                    .OrderByDescending(d => d.CreatedDate).ToList();
                string icon = "";

                foreach (var log in logs)
                {
                    switch (log.ReturnStatus)
                    {
                        case TraderReturnStatusEnum.PendingReview:
                            icon = "fa fa-info bg-aqua";
                            break;
                        case TraderReturnStatusEnum.PendingApproval:
                            icon = "fa fa-truck bg-yellow";
                            break;
                        case TraderReturnStatusEnum.Approved:
                            icon = "fa fa-check bg-green";
                            break;
                        case TraderReturnStatusEnum.Draft:
                            icon = "fa fa-warning bg-yellow";
                            break;
                        case TraderReturnStatusEnum.Denied:
                            icon = "fa fa-warning bg-red";
                            break;
                        case TraderReturnStatusEnum.Discarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }

                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = log.ReturnStatus.GetDescription(),
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


        public void SaleReturnApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var saleReturn = approval.TraderReturns.FirstOrDefault();
                if (saleReturn == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        saleReturn.Status = TraderReturnStatusEnum.PendingReview;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        saleReturn.Status = TraderReturnStatusEnum.PendingApproval;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        saleReturn.Status = TraderReturnStatusEnum.Approved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        saleReturn.Status = TraderReturnStatusEnum.Denied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        saleReturn.Status = TraderReturnStatusEnum.Discarded;
                        break;
                }

                dbContext.Entry(saleReturn).State = EntityState.Modified;
                dbContext.SaveChanges();

                var saleReturnLog = new ReturnLog()
                {
                    ParentReturn = saleReturn,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    Location = saleReturn.Location,
                    Workgroup = saleReturn.Workgroup,
                    Status = saleReturn.Status,
                    LastUpdatedBy = approval.ApprovedOrDeniedAppBy,
                    LastUpdatedDate = DateTime.UtcNow,
                    ReturnItems = saleReturn.ReturnItems,
                    Sale = saleReturn.Sale
                };

                var saleReturnProcessLog = new ReturnProcessLog
                {
                    AssociatedReturn = saleReturn,
                    AssociatedReturnLog = saleReturnLog,
                    ReturnStatus = saleReturn.Status,
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

                dbContext.ReturnProcessLogs.Add(saleReturnProcessLog);
                dbContext.Entry(saleReturnProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();

                if (approval.RequestStatus == ApprovalReq.RequestStatusEnum.Approved)
                {
                    // 1 create new transfer
                    // 2 IncomingInventory if return to inventory = true
                    /*
                1. The Transfer (and TransferItems) are created in a Purchase or Sale.
                2. The Costs of the individual TRansferItems are set at that they are created. 
                As a cost specified by the user in terms of a Purchase or as a price specified by the user in terms of a Sale
                3. When a Transfer OUT occurs (a Sale), after the Batches (out) have been created, that is when we have to calculate the costs for the inverntorydetail OUT of which the batches are leaving
                4. When a Transfer IN occurs (a Purchase), after the Batches (in) have been created, that is when we have to calculate the costs for the inverntorydetail INTO which the batches are coming
                 */
                    //switch (transfer.Status)
                    //{
                    //    case TransferStatus.Delivered:
                    //        IncomingInventory(transfer, approval.ApprovedOrDeniedAppBy);
                    //        break;
                    //    case TransferStatus.PickedUp:
                    //        OutgoingInventory(transfer, approval.ApprovedOrDeniedAppBy);
                    //        break;
                    //}
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }



        public ReturnJsonModel UpdateReturnItemQuantity(ReturnItem returnItem, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, returnItem);
                var item = dbContext.TradeReturnItems.Find(returnItem.Id);
                if (item == null)
                    return refModel;
                item.ReturnQuantity = returnItem.ReturnQuantity;
                dbContext.Entry(item).State = EntityState.Modified;
                dbContext.SaveChanges();
                CreateReturnItemLog(item, userId);

            }
            catch (Exception e)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, returnItem);
                refModel.msg = e.Message;
            }
            return refModel;
        }

        public ReturnJsonModel UpdateReturnItemIsReturnedToInventory(ReturnItem returnItem, string userId)
        {

            var refModel = new ReturnJsonModel
            {
                result = true
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, returnItem);
                var item = dbContext.TradeReturnItems.Find(returnItem.Id);
                if (item == null)
                    return refModel;
                item.IsReturnedToInventory = returnItem.IsReturnedToInventory;
                dbContext.Entry(item).State = EntityState.Modified;

                dbContext.SaveChanges();
                CreateReturnItemLog(item, userId);
            }
            catch (Exception e)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, returnItem);
                refModel.msg = e.Message;
            }


            return refModel;
        }

        public ReturnJsonModel UpdateReturnItemCredit(ReturnItem returnItem, string userId)
        {

            var refModel = new ReturnJsonModel
            {
                result = true
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, returnItem);
                var item = dbContext.TradeReturnItems.Find(returnItem.Id);
                if (item == null)
                    return refModel;
                item.Credit = returnItem.Credit;
                dbContext.Entry(item).State = EntityState.Modified;
                dbContext.SaveChanges();
                CreateReturnItemLog(item, userId);
            }
            catch (Exception e)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, returnItem);
                refModel.msg = e.Message;
            }
            return refModel;
        }

        private void CreateReturnItemLog(ReturnItem returnItem, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, returnItem);
                var user = dbContext.QbicleUser.Find(userId);

                var returnItemLog = new ReturnItemLog
                {
                    Id = 0,
                    SaleItem = returnItem.SaleItem,
                    Credit = returnItem.Credit,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedBy = user,
                    IsReturnedToInventory = returnItem.IsReturnedToInventory,
                    LastUpdatedDate = DateTime.UtcNow,
                    CreatedBy = user,
                    ReturnQuantity = returnItem.ReturnQuantity,
                    ParentReturnItem = returnItem,
                    Return = returnItem.Return
                };
                dbContext.Entry(returnItemLog).State = EntityState.Added;
                dbContext.ReturnItemLogs.Add(returnItemLog);
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, returnItem);
            }
        }

        public ReturnJsonModel DeleteReturnItem(ReturnItem returnItem, string userId)
        {

            var refModel = new ReturnJsonModel
            {
                result = true
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, returnItem);

                var item = dbContext.TradeReturnItems.Find(returnItem.Id);
                if (item == null)
                    return refModel;

                dbContext.ReturnItemLogs.RemoveRange(item.Logs);
                dbContext.TradeReturnItems.Remove(item);

                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, returnItem);
                refModel.msg = e.Message;
            }
            return refModel;
        }
    }
}