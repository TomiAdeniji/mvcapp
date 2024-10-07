using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Spannered;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules.BusinessRules.Spannered
{
    public class SpanneredConsumeReportRules
    {
        ApplicationDbContext dbContext;
        public SpanneredConsumeReportRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ConsumptionReport GetConsumptionById(int id)
        {
            try
            {
                return dbContext.SpanneredConsumptionReports.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel SaveConsumeReport(ConsumeReportCustome consumeReport, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, consumeReport);
                    var user = dbContext.QbicleUser.Find(userId);
                    var cr = new ConsumptionReport
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        Name = consumeReport.Name,
                        Description = consumeReport.Description,
                        Domain = consumeReport.Domain,
                        Location = dbContext.TraderLocations.Find(consumeReport.LocationId),
                        Workgroup = dbContext.SpanneredWorkgroups.Find(consumeReport.WorkgroupId),
                        Status = consumeReport.Status
                    };
                    if (consumeReport.TaskId > 0)
                        cr.AssociatedTask = dbContext.QbicleTasks.Find(consumeReport.TaskId);
                    //Add Items
                    foreach (var item in consumeReport.Items)
                    {
                        var traderitem = dbContext.TraderItems.Find(item.ItemId);
                        ConsumptionItem ci = new ConsumptionItem
                        {
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow
                        };
                        ci.LastUpdatedBy = user;
                        ci.LastUpdatedDate = DateTime.UtcNow;
                        ci.Note = item.Note;
                        ci.ConsumptionReport = cr;
                        ci.Used = item.Used;
                        ci.Item = traderitem;
                        ci.Unit = item.UnitId > 0 ? dbContext.ProductUnits.Find(item.UnitId) : traderitem.Units.FirstOrDefault(s => s.IsBase);
                        ci.Allocated = item.Allocated;
                        cr.ConsumptionItems.Add(ci);
                    }
                    if (cr == null || cr.Status == ConsumptionReport.ConsumptionReportStatusEnum.Draft)
                    {
                        dbContext.SaveChanges();
                        dbTransaction.Commit();
                        return refModel;
                    }
                    //Add Approval
                    cr.Workgroup.SourceQbicle.LastUpdated = DateTime.UtcNow;
                    var appDef = dbContext.ConsumeReportApprovalDefinitions.FirstOrDefault(w => w.SpanneredWorkgroup.Id == cr.Workgroup.Id);
                    if (appDef == null)
                    {
                        var processConsumeReport = cr.Workgroup.Processes.FirstOrDefault(s => s.Name == ProcessesConst.ConsumptionReports);
                        var rule = new ApprovalAppsRules(dbContext);
                        ApprovalGroup appGroup = rule.GetApprovalAppsGroupByName("Consume Report Approvals", cr.Workgroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, "Consume Report Approvals", user, cr.Workgroup.Domain).Object;
                        var consumeAppDef = new ConsumeReportApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{cr.Workgroup.Name}/ {processConsumeReport.Name}",
                            ApprovalImage = "",
                            Description = $"Spannered WorkGroup: {cr.Workgroup.Name} {processConsumeReport.Name} process",
                            Initiators = cr.Workgroup.Members,
                            Approvers = cr.Workgroup.ReviewersApprovers,
                            Reviewers = cr.Workgroup.ReviewersApprovers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            SpanneredWorkgroup = cr.Workgroup,
                            SpanneredProcessType = processConsumeReport
                        };
                        cr.Workgroup.ApprovalDefs.Add(consumeAppDef);
                    }
                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = appDef,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                        ConsumptionReports = new List<ConsumptionReport> { cr },
                        Name = $"Consume Report: {cr.Name}",
                        Qbicle = cr.Workgroup.SourceQbicle,
                        Topic = cr.Workgroup.DefaultTopic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        StartedBy = cr.CreatedBy,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Notes = "",
                        IsVisibleInQbicleDashboard = true,
                        App = QbicleActivity.ActivityApp.Spannered
                    };
                    cr.ConsumptionApprovalProcess = approval;
                    approval.ActivityMembers.AddRange(cr.Workgroup.Members);
                    //Add Logs
                    var consumeLog = new ConsumptionReportLog
                    {
                        Name = cr.Name,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = cr.CreatedBy,
                        Description = cr.Description,
                        Domain = cr.Domain,
                        AssociatedTask = cr.AssociatedTask,
                        LastUpdatedBy = cr.CreatedBy,
                        LastUpdatedDate = DateTime.UtcNow,
                        Location = cr.Location,
                        Status = cr.Status,
                        ConsumptionApprovalProcess = approval,
                        ConsumptionReport = cr,
                        Workgroup = cr.Workgroup
                    };
                    foreach (var item in cr.ConsumptionItems)
                    {
                        ConsumptionItemLog itemLog = new ConsumptionItemLog();
                        itemLog.CreatedBy = item.CreatedBy;
                        itemLog.CreatedDate = item.CreatedDate;
                        itemLog.LastUpdatedBy = item.LastUpdatedBy;
                        itemLog.LastUpdatedDate = item.LastUpdatedDate;
                        itemLog.Note = item.Note;
                        itemLog.Item = item.Item;
                        itemLog.Unit = item.Unit;
                        itemLog.Used = item.Used;
                        itemLog.Allocated = item.Allocated;
                        itemLog.ConsumptionReport = cr;
                        consumeLog.ConsumptionItemLogs.Add(itemLog);
                    }
                    //end logs
                    var consumeProcessLog = new ConsumptionReportProcessLog
                    {
                        ConsumptionReport = cr,
                        ConsumptionReportLog = consumeLog,
                        ConsumptionReportStatus = cr.Status,
                        CreatedBy = cr.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        ApprovalReqHistory = new ApprovalReqHistory
                        {
                            ApprovalReq = approval,
                            UpdatedBy = cr.CreatedBy,
                            CreatedDate = DateTime.UtcNow,
                            RequestStatus = approval.RequestStatus
                        }
                    };

                    dbContext.SpanneredConsumptionReportProcessLogs.Add(consumeProcessLog);
                    dbContext.Entry(consumeProcessLog).State = EntityState.Added;
                    dbContext.SpanneredConsumptionReports.Add(cr);
                    dbContext.Entry(cr).State = EntityState.Added;
                    refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                    refModel.msg = "ERROR_MSG_EXCEPTION_SYSTEM";
                    dbTransaction.Rollback();
                }
            }

            return refModel;
        }
        public List<QbicleTask> GetAssetTasksCompleted(int domainId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, locationId);
                var query = from asset in dbContext.SpanneredAssets
                            join task in dbContext.QbicleTasks on asset.Id equals task.Asset.Id
                            where asset.Domain.Id == domainId
                            && asset.Location.Id == locationId
                            && task.isComplete
                            select task;

                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<QbicleTask>();
            }
        }
        public List<ConsumablesPartServiceItem> GetConsumedStockByTaskId(int taskId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, taskId);
                var dbtask = dbContext.QbicleTasks.Find(taskId);
                return dbtask?.ConsumableItems.ToList() ?? new List<ConsumablesPartServiceItem>();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<ConsumablesPartServiceItem>();
            }
        }
        public List<ApprovalStatusTimeline> ConsumeApprovalStatusTimeline(int id, string timeZone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var logs = dbContext.SpanneredConsumptionReportProcessLogs.Where(e => e.ConsumptionReport.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string icon = "";

                foreach (var log in logs)
                {
                    switch (log.ConsumptionReportStatus)
                    {
                        case ConsumptionReport.ConsumptionReportStatusEnum.Pending:
                            icon = "fa fa-info bg-aqua";
                            break;
                        case ConsumptionReport.ConsumptionReportStatusEnum.Reviewed:
                            icon = "fa fa-truck bg-yellow";
                            break;
                        case ConsumptionReport.ConsumptionReportStatusEnum.Approved:
                            icon = "fa fa-check bg-green";
                            break;
                        case ConsumptionReport.ConsumptionReportStatusEnum.Draft:
                            icon = "fa fa-warning bg-yellow";
                            break;
                        case ConsumptionReport.ConsumptionReportStatusEnum.Denied:
                            icon = "fa fa-warning bg-red";
                            break;
                        case ConsumptionReport.ConsumptionReportStatusEnum.Discarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = log.ConsumptionReportStatus.GetDescription(),
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
        public void ConsumeReportApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var consumeReportDb = approval.ConsumptionReports.FirstOrDefault();
                if (consumeReportDb == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        consumeReportDb.Status = ConsumptionReport.ConsumptionReportStatusEnum.Pending;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        consumeReportDb.Status = ConsumptionReport.ConsumptionReportStatusEnum.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        consumeReportDb.Status = ConsumptionReport.ConsumptionReportStatusEnum.Approved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        consumeReportDb.Status = ConsumptionReport.ConsumptionReportStatusEnum.Denied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        consumeReportDb.Status = ConsumptionReport.ConsumptionReportStatusEnum.Denied;
                        break;
                }
                consumeReportDb.LastUpdatedDate = DateTime.UtcNow;
                consumeReportDb.LastUpdatedBy = approval.ApprovedOrDeniedAppBy;
                dbContext.Entry(consumeReportDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                //logging

                var consumeLog = new ConsumptionReportLog
                {
                    Name = consumeReportDb.Name,
                    CreatedDate = DateTime.UtcNow,
                    Description = consumeReportDb.Description,
                    Domain = consumeReportDb.Domain,
                    AssociatedTask = consumeReportDb.AssociatedTask,
                    LastUpdatedBy = approval.ApprovedOrDeniedAppBy,
                    LastUpdatedDate = DateTime.UtcNow,
                    Location = consumeReportDb.Location,
                    Status = consumeReportDb.Status,
                    ConsumptionApprovalProcess = approval,
                    ConsumptionReport = consumeReportDb,
                    Workgroup = consumeReportDb.Workgroup,
                    CreatedBy = approval.ApprovedOrDeniedAppBy
                };
                foreach (var item in consumeReportDb.ConsumptionItems)
                {
                    ConsumptionItemLog itemLog = new ConsumptionItemLog();
                    itemLog.CreatedBy = item.CreatedBy;
                    itemLog.CreatedDate = item.CreatedDate;
                    itemLog.LastUpdatedBy = item.LastUpdatedBy;
                    itemLog.LastUpdatedDate = item.LastUpdatedDate;
                    itemLog.Note = item.Note;
                    itemLog.Item = item.Item;
                    itemLog.Unit = item.Unit;
                    itemLog.Used = item.Used;
                    itemLog.Allocated = item.Allocated;
                    itemLog.ConsumptionReport = consumeReportDb;
                    consumeLog.ConsumptionItemLogs.Add(itemLog);
                    //Update Trader Item Inventory
                    //when the Consumption Report is approved, a stock adjustment needs to occur on the inventory affected by the report
                    if (consumeReportDb.Status == ConsumptionReport.ConsumptionReportStatusEnum.Approved)
                    {
                        var ivde = item.Item.InventoryDetails.FirstOrDefault(s => s.Location.Id == consumeReportDb.Location.Id);
                        if (ivde != null)
                        {
                            ivde.CurrentInventoryLevel = ivde.CurrentInventoryLevel - (item.Used * item.Unit.QuantityOfBaseunit);
                            dbContext.Entry(ivde).State = EntityState.Modified;
                        }
                    }
                }
                var consumeProcessLog = new ConsumptionReportProcessLog
                {
                    ConsumptionReport = consumeReportDb,
                    ConsumptionReportLog = consumeLog,
                    ConsumptionReportStatus = consumeReportDb.Status,
                    CreatedBy = consumeReportDb.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = consumeReportDb.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.SpanneredConsumptionReportProcessLogs.Add(consumeProcessLog);
                dbContext.Entry(consumeProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }


        }
        public ReturnJsonModel UpdateUsedOfConsumeItems(int ciId, decimal value, string currentUserId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var consumeItem = dbContext.SpanneredConsumptionItems.Find(ciId);
                if (consumeItem != null)
                {
                    var consumeReport = consumeItem.ConsumptionReport;
                    var isAllowUpdate = dbContext.SpanneredWorkgroups
                     .Where(s => s.Id == consumeReport.Workgroup.Id && s.Domain.Id == consumeReport.Domain.Id && (s.CreatedBy.Id.Equals(currentUserId) || s.ReviewersApprovers.Any(a => a.Id == currentUserId)) && s.Processes.Any(p => p.Name == ProcessesConst.ConsumptionReports)).Any();
                    if (!isAllowUpdate || consumeReport.Status == ConsumptionReport.ConsumptionReportStatusEnum.Approved || consumeReport.Status == ConsumptionReport.ConsumptionReportStatusEnum.Denied || consumeReport.Status == ConsumptionReport.ConsumptionReportStatusEnum.Discarded)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_28");
                        return refModel;
                    }
                    consumeItem.Used = value;
                    refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, ciId);
            }
            return refModel;
        }
    }
}
