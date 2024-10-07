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
using System.Web.UI.WebControls;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderWasteReportRules
    {
        ApplicationDbContext dbContext;

        public TraderWasteReportRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public WasteReport GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.WasteReports.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new WasteReport();
            }
        }
        public List<WorkGroup> GetTraderWasteReportGroupFilter(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                return dbContext.WasteReports.Where(d => d.Domain.Id == domainId && d.Location.Id == locationId).Select(q => q.Workgroup).Distinct().OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, domainId);
                return new List<WorkGroup>();
            }
        }
        public DataTablesResponse GetByLocationPagination(int locationId, int domainId, IDataTablesRequest requestModel, UserSetting user, string keyword, string datetime, int[] workgroups, WasteReportStatus[] status)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId, workgroups, requestModel);
                var query = dbContext.WasteReports.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId);
                int totalSpot;
                #region Filter

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => q.Name.Contains(keyword) || q.Description.Contains(keyword));

                if (workgroups.Count() > 0 && workgroups.Any(w => w != -1))
                    query = query.Where(q => workgroups.Contains(q.Workgroup.Id));

                if (status.Count() > 0 && Enum.IsDefined(typeof(WasteReportStatus), status[0]))
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
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
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
                            orderByString = "Name asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new WasteReportModel
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
        public void WasteReportApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var wasteReportDb = approval.WasteReports.FirstOrDefault();
                if (wasteReportDb == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        wasteReportDb.Status = WasteReportStatus.Started;
                        foreach (var item in wasteReportDb.ProductList)
                        {
                            //item.Adjustment = 0;
                            item.SavedInventoryCount = 0;
                        }
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        wasteReportDb.Status = WasteReportStatus.Completed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        wasteReportDb.Status = WasteReportStatus.StockAdjusted;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        wasteReportDb.Status = WasteReportStatus.Discarded;
                        foreach (var item in wasteReportDb.ProductList)
                        {
                            //item.Adjustment = 0;
                            item.SavedInventoryCount = 0;
                        }
                        break;
                }
                wasteReportDb.LastUpdatedDate = DateTime.UtcNow;
                wasteReportDb.LastUpdatedBy = approval.ApprovedOrDeniedAppBy;
                dbContext.Entry(wasteReportDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                //logging

                var wasteLog = new WasteReportLog
                {
                    Name = wasteReportDb.Name,
                    CreatedDate = DateTime.UtcNow,
                    Description = wasteReportDb.Description,
                    Domain = wasteReportDb.Domain,
                    LastUpdatedBy = approval.ApprovedOrDeniedAppBy,
                    LastUpdatedDate = DateTime.UtcNow,
                    Location = wasteReportDb.Location,
                    Status = wasteReportDb.Status,
                    UpdatedBy = approval.ApprovedOrDeniedAppBy,
                    WasteApprovalProcess = approval,
                    WasteItemLogs = dbContext.WasteItemLogs.Where(e => e.WasteReport.Id == wasteReportDb.Id).ToList(),
                    WasteReport = wasteReportDb,
                    Workgroup = wasteReportDb.Workgroup
                };

                var wasteProcessLog = new WasteReportProcessLog
                {
                    AssociatedWasteReport = wasteReportDb,
                    AssociatedWasteReportLog = wasteLog,
                    WasteReportStatus = wasteReportDb.Status,
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

                dbContext.WasteReportProcessLogs.Add(wasteProcessLog);
                dbContext.Entry(wasteProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();

                //Create new Transfer and Outgoing when StockAdjusted -- Waste Report
                if (wasteReportDb.Status != WasteReportStatus.StockAdjusted)
                    return;
                //Transfer process

                var transferItems = new List<TraderTransferItem>();
                foreach (var item in wasteReportDb.ProductList)
                {
                    transferItems.Add(new TraderTransferItem
                    {
                        Unit = item.CountUnit,
                        QuantityAtPickup = item.WasteCountValue,
                        QuantityAtDelivery = item.WasteCountValue,
                        TransactionItem = null,// For a Point-to-Point transfer there is no Tansaction Item
                        TraderItem = item.Product
                    });
                }
                var tradTransfer = new TraderTransfer
                {
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    DestinationLocation = null,
                    OriginatingLocation = wasteReportDb.Location,
                    Workgroup = wasteReportDb.Workgroup,
                    Address = null,
                    Contact = null,
                    Status = TransferStatus.Delivered,
                    Reason = TransferReasonEnum.WasteReportAdjustment,
                    WasteReport = wasteReportDb,
                    TransferItems = transferItems
                };
                tradTransfer.Reference = new TraderReferenceRules(dbContext).GetNewReference(tradTransfer.Workgroup.Domain.Id, TraderReferenceType.Transfer);

                dbContext.TraderTransfers.Add(tradTransfer);
                dbContext.Entry(tradTransfer).State = EntityState.Added;

                dbContext.SaveChanges();

                // When a Transfer OUT occurs (a Sale), after the Batches (out) have been created, that is when we have to calculate the costs for the inverntorydetail OUT of which the batches are leaving
                new TraderTransfersRules(dbContext).OutgoingInventory(tradTransfer, approval.ApprovedOrDeniedAppBy);

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

        public ReturnJsonModel SaveWasteReport(WasteReport wasteReport, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, wasteReport, userId);

                var user = new UserRules(dbContext).GetById(userId);

                if (wasteReport.Workgroup != null && wasteReport.Workgroup.Id > 0)
                {
                    wasteReport.Workgroup = dbContext.WorkGroups.Find(wasteReport.Workgroup.Id);
                }


                if (wasteReport.Id == 0)
                {
                    wasteReport.CreatedBy = user;
                    wasteReport.LastUpdatedBy = user;
                    wasteReport.LastUpdatedDate = wasteReport.CreatedDate;
                    wasteReport.Description = wasteReport.Description.Trim();
                    if (wasteReport.ProductList != null && wasteReport.ProductList.Count > 0)
                    {
                        foreach (var item in wasteReport.ProductList)
                        {
                            item.CreatedBy = user;
                            item.CreatedDate = wasteReport.CreatedDate;
                            item.Product = new TraderItemRules(dbContext).GetById(item.Product.Id);
                            item.CountUnit = dbContext.ProductUnits.Find(item.CountUnit.Id);
                            item.WasteReport = wasteReport;
                            item.LastUpdatedDate = wasteReport.LastUpdatedDate;
                            item.LastUpdatedBy = wasteReport.LastUpdatedBy;

                        }
                    }
                    dbContext.Entry(wasteReport).State = EntityState.Added;
                    dbContext.WasteReports.Add(wasteReport);
                    dbContext.SaveChanges();
                    if (wasteReport.ProductList != null)
                        foreach (var item in wasteReport.ProductList)
                        {
                            //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                            var wasteItemLog = new WasteItemLog
                            {
                                CreatedDate = DateTime.UtcNow,
                                WasteReport = wasteReport,
                                CountUnit = item.CountUnit,
                                CreatedBy = user,
                                LastUpdatedBy = user,
                                LastUpdatedDate = DateTime.MaxValue,
                                Notes = item.Notes,
                                Product = item.Product,
                                SavedInventoryCount = item.SavedInventoryCount,
                                WasteCountValue = item.WasteCountValue
                            };
                            dbContext.Entry(wasteItemLog).State = EntityState.Added;
                            dbContext.WasteItemLogs.Add(wasteItemLog);
                        }

                    dbContext.SaveChanges();
                    result.msgId = wasteReport.Id.ToString();
                    result.actionVal = 1;
                }
                else
                {

                    var wasteDb = dbContext.WasteReports.Find(wasteReport.Id);
                    if (wasteDb != null)
                    {
                        wasteDb.LastUpdatedBy = user;
                        wasteDb.LastUpdatedDate = DateTime.UtcNow;
                        wasteDb.Name = wasteReport.Name;
                        wasteDb.Description = wasteReport.Description.Trim();
                        wasteDb.Status = wasteReport.Status;
                        wasteDb.Workgroup = wasteReport.Workgroup;
                        dbContext.Entry(wasteDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        ////item                        

                        //foreach (var item in wasteReport.ProductList)
                        //{
                        //    item.CreatedBy = user;
                        //    item.CreatedDate = wasteReport.CreatedDate;
                        //    item.Product = new TraderItemRules(dbContext).GetById(item.Product.Id);
                        //    item.CountUnit = dbContext.ProductUnits.Find(item.CountUnit.Id);
                        //    item.WasteReport = wasteDb;
                        //    item.LastUpdatedDate = wasteDb.LastUpdatedDate;
                        //    item.LastUpdatedBy = wasteDb.LastUpdatedBy;
                        //}
                        ////Update Transaction Items

                        //var itemsUi = wasteReport.ProductList;
                        //var itemsDb = wasteDb.ProductList;

                        //var itemsNew = itemsUi.Where(c => c.Id == 0).ToList();

                        //var itemsDelete = itemsDb.Where(c => itemsUi.All(d => c.Id != d.Id)).ToList();

                        //var itemsUpdate = itemsUi.Where(c => itemsDb.Any(d => c.Id == d.Id)).ToList();


                        //foreach (var itemDel in itemsDelete)
                        //{
                        //    wasteDb.ProductList.Remove(itemDel);
                        //    dbContext.Entry(wasteDb).State = EntityState.Modified;
                        //    dbContext.SaveChanges();
                        //}

                        //foreach (var iDb in wasteDb.ProductList)
                        //{
                        //    var iUpdate = itemsUpdate.FirstOrDefault(e => e.Id == iDb.Id);
                        //    if (iUpdate == null) continue;

                        //    iDb.Product = iUpdate.Product;
                        //    iDb.CountUnit = iUpdate.CountUnit;
                        //    iDb.WasteReport = wasteDb;
                        //    iDb.LastUpdatedDate = wasteDb.LastUpdatedDate;
                        //    iDb.LastUpdatedBy = wasteDb.LastUpdatedBy;
                        //    iDb.Notes = iUpdate.Notes;

                        //    dbContext.Entry(iDb).State = EntityState.Modified;
                        //    dbContext.SaveChanges();

                        //    var transItemLog = new WasteItemLog
                        //    {
                        //        CreatedDate = DateTime.UtcNow,
                        //        CreatedBy = user,
                        //        LastUpdatedBy = user,
                        //        Product = iDb.Product,
                        //        CountUnit = iDb.CountUnit,
                        //        Notes = iDb.Notes,
                        //        LastUpdatedDate = DateTime.UtcNow,
                        //        SavedInventoryCount = iDb.SavedInventoryCount,
                        //        WasteReport = wasteDb,
                        //        WasteCountValue = iDb.WasteCountValue
                        //    };
                        //    dbContext.Entry(transItemLog).State = EntityState.Added;
                        //    dbContext.WasteItemLogs.Add(transItemLog);
                        //    dbContext.SaveChanges();
                        //}
                        //if (itemsNew.Count > 0)
                        //{
                        //    wasteDb.ProductList.AddRange(itemsNew);
                        //    dbContext.Entry(wasteDb).State = EntityState.Modified;
                        //    dbContext.SaveChanges();
                        //    foreach (var item in itemsNew)
                        //    {
                        //        //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                        //        var transItemLog = new WasteItemLog
                        //        {
                        //            CreatedDate = DateTime.UtcNow,
                        //            CreatedBy = user,
                        //            LastUpdatedBy = user,
                        //            Product = item.Product,
                        //            CountUnit = item.CountUnit,
                        //            Notes = item.Notes,
                        //            LastUpdatedDate = DateTime.UtcNow,
                        //            SavedInventoryCount = item.SavedInventoryCount,
                        //            WasteReport = wasteDb,
                        //            WasteCountValue = item.WasteCountValue
                        //        };
                        //        dbContext.Entry(transItemLog).State = EntityState.Added;
                        //        dbContext.WasteItemLogs.Add(transItemLog);
                        //    }

                        dbContext.SaveChanges();


                        //}

                    }

                    //DbContext.SaveChanges();
                    result.msgId = wasteReport.Id.ToString();
                    result.actionVal = 2;

                }

                var wasteReportDb = dbContext.WasteReports.Find(wasteReport.Id);

                if (wasteReportDb?.WasteApprovalProcess != null)
                    return result;

                if (wasteReportDb == null || wasteReportDb.Status == WasteReportStatus.Draft) return result;
                wasteReportDb.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var appDef =
                    dbContext.WasteReportApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == wasteReportDb.Workgroup.Id);
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    WasteReports = new List<WasteReport> { wasteReportDb },
                    Name = $"Waste Report: {wasteReportDb.Name}",
                    Qbicle = wasteReportDb.Workgroup.Qbicle,
                    Topic = wasteReportDb.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = wasteReportDb.CreatedBy,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader
                };
                wasteReportDb.WasteApprovalProcess = approval;
                wasteReportDb.WasteApprovalProcess.ApprovalRequestDefinition = appDef;
                approval.ActivityMembers.AddRange(wasteReportDb.Workgroup.Members);
                dbContext.Entry(wasteReportDb).State = EntityState.Modified;
                dbContext.SaveChanges();
                //logging

                var wasteLog = new WasteReportLog
                {
                    Name = wasteReportDb.Name,
                    CreatedDate = DateTime.UtcNow,
                    Description = wasteReportDb.Description,
                    Domain = wasteReportDb.Domain,
                    LastUpdatedBy = user,
                    LastUpdatedDate = DateTime.UtcNow,
                    Location = wasteReportDb.Location,
                    Status = wasteReportDb.Status,
                    UpdatedBy = user,
                    WasteApprovalProcess = approval,
                    WasteItemLogs = dbContext.WasteItemLogs.Where(e => e.WasteReport.Id == wasteReportDb.Id).ToList(),
                    WasteReport = wasteReportDb,
                    Workgroup = wasteReportDb.Workgroup
                };

                var wasteProcessLog = new WasteReportProcessLog
                {
                    AssociatedWasteReport = wasteReportDb,
                    AssociatedWasteReportLog = wasteLog,
                    WasteReportStatus = wasteReportDb.Status,
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

                dbContext.WasteReportProcessLogs.Add(wasteProcessLog);
                dbContext.Entry(wasteProcessLog).State = EntityState.Added;
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
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, wasteReport, userId);
                result.msg = e.Message;
            }


            return result;

        }

        public ReturnJsonModel UpdateWasteReportItems(WasteReport wasteReport, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, wasteReport, userId);

                var user = new UserRules(dbContext).GetById(userId);
                wasteReport.LastUpdatedBy = user;

                foreach (var product in wasteReport.ProductList)
                {
                    var wasteItem = dbContext.WasteItems.Find(product.Id);
                    if (wasteItem == null) continue;
                    wasteItem.WasteReport.LastUpdatedDate = DateTime.UtcNow;
                    wasteItem.WasteReport.LastUpdatedBy = user;

                    wasteItem.CountUnit = new TraderConversionUnitRules(dbContext).GetById(product.CountUnit.Id);
                    wasteItem.WasteCountValue = product.WasteCountValue;
                    wasteItem.Notes = product.Notes;

                    dbContext.Entry(wasteItem).State = EntityState.Modified;
                    dbContext.SaveChanges();


                    //itemlog
                    var transItemLog = new WasteItemLog
                    {
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = user,
                        LastUpdatedBy = user,
                        Product = wasteItem.Product,
                        CountUnit = wasteItem.CountUnit,
                        Notes = wasteItem.Notes,
                        LastUpdatedDate = DateTime.UtcNow,
                        SavedInventoryCount = wasteItem.SavedInventoryCount,
                        WasteReport = wasteItem.WasteReport,
                        WasteCountValue = wasteItem.WasteCountValue
                    };
                    dbContext.Entry(transItemLog).State = EntityState.Added;
                    dbContext.WasteItemLogs.Add(transItemLog);
                    dbContext.SaveChanges();
                }

                result.actionVal = 2;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, wasteReport, userId);
                result.msg = e.Message;
            }


            return result;
        }

        public ReturnJsonModel UpdateDescription(WasteReport wasteReport)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, wasteReport);
                var wasteDb = dbContext.WasteReports.Find(wasteReport.Id);
                if (wasteDb == null) return result;
                wasteDb.Description = wasteReport.Description.Trim();
                dbContext.Entry(wasteDb).State = EntityState.Modified;
                dbContext.SaveChanges();
                result.actionVal = 2;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, wasteReport);
                result.msg = e.Message;
            }

            return result;
        }

        public List<ApprovalStatusTimeline> WasteApprovalStatusTimeline(int id, string timeZone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var logs = dbContext.WasteReportProcessLogs.Where(e => e.AssociatedWasteReport.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string status = "", icon = "";

                foreach (var log in logs)
                {
                    switch (log.WasteReportStatus)
                    {
                        case WasteReportStatus.Started:
                            status = "Started";
                            icon = "fa fa-info bg-aqua";
                            break;
                        case WasteReportStatus.Completed:
                            status = "Completed";
                            icon = "fa fa-truck bg-yellow";
                            break;
                        case WasteReportStatus.StockAdjusted:
                            status = "Stock Adjusted";
                            icon = "fa fa-check bg-green";
                            break;
                        case WasteReportStatus.Draft:
                            status = "Draft";
                            icon = "fa fa-warning bg-yellow";
                            break;
                        case WasteReportStatus.Discarded:
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

        /// <summary>
        /// Using for case add/edit/remove item in a wasteReport existed
        /// </summary>
        /// <param name="wasteItem"></param>
        /// <param name="userId"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateWasteReportProduct(WasteItem wasteItem, string userId, bool isDelete)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, "", null, wasteItem, isDelete);

                var user = new UserRules(dbContext).GetById(userId);

                var wasteDb = dbContext.WasteReports.Find(wasteItem.WasteReport.Id);
                var wasteItemDb = wasteDb.ProductList.FirstOrDefault(e => e.Id == wasteItem.Id);
                if (wasteItemDb != null)
                {
                    if (isDelete)
                    {
                        wasteDb.ProductList.Remove(wasteItemDb);
                    }
                    else
                    {
                        wasteItemDb.Notes = wasteItem.Notes;
                        wasteItemDb.CountUnit = dbContext.ProductUnits.Find(wasteItem.CountUnit.Id);
                    }
                }
                else
                {
                    var wItem = new WasteItem
                    {
                        Id = wasteItem.Id,
                        CountUnit = dbContext.ProductUnits.Find(wasteItem.CountUnit.Id),
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Product = new TraderItemRules(dbContext).GetById(wasteItem.Product.Id),
                        WasteReport = wasteDb,
                        LastUpdatedDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        Notes = wasteItem.Notes,
                        SavedInventoryCount = wasteItem.SavedInventoryCount,
                        WasteCountValue = wasteItem.WasteCountValue
                    };
                    wasteDb.ProductList.Add(wItem);
                    //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                    var transItemLog = new WasteItemLog
                    {
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = user,
                        LastUpdatedBy = user,
                        Product = wItem.Product,
                        CountUnit = wItem.CountUnit,
                        Notes = wItem.Notes,
                        LastUpdatedDate = DateTime.UtcNow,
                        SavedInventoryCount = wItem.SavedInventoryCount,
                        WasteReport = wasteDb,
                        WasteCountValue = wItem.WasteCountValue
                    };
                    dbContext.Entry(transItemLog).State = EntityState.Added;
                    dbContext.WasteItemLogs.Add(transItemLog);
                }

                dbContext.Entry(wasteDb).State = EntityState.Modified;
                dbContext.SaveChanges();



            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, wasteItem, userId);
                result.msg = e.Message;
                result.result = false;
            }
            result.result = true;

            return result;

        }

        public DataTablesResponse GetWasteReportItem(IDataTablesRequest requestModel,
            int wasteReportId, int locationId, int domainId, UserSetting user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, wasteReportId, domainId);

                var wasteReport = new TraderWasteReportRules(dbContext).GetById(wasteReportId);

                var totalItems = wasteReport.ProductList.Count();
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

                            //orderByString += orderByString != string.Empty ? "," : "";
                            //if (wasteReport.Status == WasteReportStatus.Completed || wasteReport.Status == WasteReportStatus.StockAdjusted)
                            //{
                            //    orderByString += "SavedInventoryCount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");                                
                            //}
                            //else
                            //{
                            //    orderByString += "Product.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId).CurrentInventoryLevel" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                
                            //}

                            break;
                        default:
                            orderByString = "Product.Name asc";
                            break;
                    }
                }

                var items = wasteReport.ProductList.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);


                //Paging
                var list = items.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                var dataJson = new List<WasteReportItemModel>();

                list.ForEach(item =>
                {
                    var waster = new WasteReportItemModel
                    {
                        Id = item.Id,
                        ProductId = item.Product?.Id ?? 0,
                        Name = $"<span>{item.Product?.Name}</span><input type='hidden' value='{item.Id}' class='waste_id' />",
                        Unit = InitWasterItemUnit(item),
                        SKU = $"<span>{item.Product.SKU}</span><input type='hidden' value='{item.Product.Id}' class='waste_item_id' />",
                        Notes = $"<input type='text' name='item-1-notes' onchange='UpdateWasteReportProduct({item.Product.Id},false)' value='{item.Notes}' class='form-control' style='width: 100%;'>",
                        Button=$"<button class='btn btn-danger' onclick='removeWasteItem({item.Product.Id})'><i class='fa fa-trash'></i></button>"
                    };
                    if (wasteReport.Status == WasteReportStatus.Completed || wasteReport.Status == WasteReportStatus.StockAdjusted)
                    {
                        waster.CurrentStock = $"<span class='demo'>{item.SavedInventoryCount}</span>";
                    }
                    else
                    {
                        var inventory = item.Product.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId);
                        if (inventory == null)
                        {
                            inventory = new InventoryDetail();
                        }
                        waster.CurrentStock = $"<span class='demo'>{inventory.CurrentInventoryLevel}</span>";
                    }
                    dataJson.Add(waster);

                });

                return new DataTablesResponse(requestModel.Draw, dataJson, totalItems, totalItems);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel, wasteReportId, domainId);
            }

            return null;
        }

        private string InitWasterItemUnit(WasteItem item)
        {
            string selectUnit = "";
            selectUnit += $"<select id='{Guid.NewGuid().ToString().Replace("-", "")}'";
            selectUnit += $"onchange='UpdateWasteReportProduct({item.Product.Id},false)' data-placeholder='Please select' class='form-control unit-select2' style='width: 100%;'>";
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
