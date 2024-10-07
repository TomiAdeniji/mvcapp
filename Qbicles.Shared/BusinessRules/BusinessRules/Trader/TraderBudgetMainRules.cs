using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Budgets;
using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderBudgetMainRules
    {
        private ApplicationDbContext dbContext;

        public TraderBudgetMainRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public BudgetScenarioItemGroup GetBudgetScenarioItemGroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.BudgetScenarioItemGroups.FirstOrDefault(e => e.Id == id)?? new BudgetScenarioItemGroup();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null , id);
                return new BudgetScenarioItemGroup();
            }
        }

        public List<BudgetScenarioItem> GetBudgetScenarioItemByBudgetScenarioItemGroupId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.BudgetScenarioItems.Where(e => e.BudgetScenarioItemGroup.Id == id).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new List<BudgetScenarioItem>();
            }
        }


        public List<BudgetItemModel> GetAllByItemType(int budgetScenarioId, ItemGroupType type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, budgetScenarioId, type);
                var budgetScenario = dbContext.BudgetScenarios.Find(budgetScenarioId);
                if (budgetScenario == null) return new List<BudgetItemModel>();
                var quantities = budgetScenario.ScenarioItemStartingQuantities ?? new List<ScenarioItemStartingQuantity>();

                var traderItems = new List<BudgetItemModel>();

                quantities.ForEach(itemQty =>
                {
                    switch (type)
                    {
                        case ItemGroupType.ItemsIBuy:
                            if (itemQty.Item.IsBought && !itemQty.Item.IsSold)
                                AddItem(itemQty.Item, itemQty.Unit?.Name ?? "", ref traderItems);
                            break;
                        case ItemGroupType.ItemsISell:
                            if (!itemQty.Item.IsBought && itemQty.Item.IsSold)
                                AddItem(itemQty.Item, itemQty.Unit?.Name ?? "", ref traderItems);
                            break;
                        case ItemGroupType.ItemsIBuyAndSell:
                            if (itemQty.Item.IsBought && itemQty.Item.IsSold)
                                AddItem(itemQty.Item, itemQty.Unit?.Name ?? "", ref traderItems);
                            break;
                    }
                });

                return traderItems;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, budgetScenarioId, type);
                return new List<BudgetItemModel>();
            }
        }

        private void AddItem(TraderItem item, string unitName, ref List<BudgetItemModel> itemsList)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, item, unitName, itemsList);
            itemsList.Add(new BudgetItemModel
            {
                Id = item.Id,
                Name = $"{item.SKU} - {item.Name}",
                ImageUri = item.ImageUri.ToUriString(),
                Unit = unitName
            });
        }

        public ReturnJsonModel AddBudgetScenarioItem(BudgetScenarioItemGroup budgetScenarioGroup, string userId)
        {
            var result = new ReturnJsonModel {actionVal = 1, msgId = "0", result = true};
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, budgetScenarioGroup);
                var group = dbContext.BudgetScenarioItemGroups.Find(budgetScenarioGroup.Id);
                if (group == null)
                {
                    group = new BudgetScenarioItemGroup
                    {
                        //ApprovalRequest = new ApprovalReq(),
                        BudgetScenario = dbContext.BudgetScenarios.Find(budgetScenarioGroup.BudgetScenario.Id),
                        CreatedBy = dbContext.QbicleUser.Find(userId),
                        CreatedDate = DateTime.UtcNow,
                        Status = BudgetScenarioItemGroupStatus.Draft,
                        Type = budgetScenarioGroup.Type,
                        WorkGroup = dbContext.WorkGroups.Find(budgetScenarioGroup.WorkGroup.Id),
                        BudgetScenarioItems = new List<BudgetScenarioItem>()
                    };
                    dbContext.BudgetScenarioItemGroups.Add(group);
                    dbContext.SaveChanges();
                }

                //new group, item, item projection
                result.msgId = group.Id.ToString();


                var reportingPeriods = dbContext.ReportingPeriods
                    .Where(e => e.BudgetScenario.Id == budgetScenarioGroup.BudgetScenario.Id).OrderBy(d => d.Start)
                    .ToList();

                var startingQuantities =
                    dbContext.ScenarioItemStartingQuantities.Where(e =>
                        e.BudgetScenario.Id == budgetScenarioGroup.BudgetScenario.Id);


                budgetScenarioGroup.BudgetScenarioItems.ForEach(item =>
                {
                    if (group.BudgetScenarioItems.Any(e => e.Item.Id == item.Item.Id) || item.Item.Id == 0)
                        return;
                    var bItem = new BudgetScenarioItem
                    {
                        Item = dbContext.TraderItems.Find(item.Item.Id),
                        AveragePurchaseCost = item.AveragePurchaseCost,
                        PurchaseQuantity = item.PurchaseQuantity,
                        AverageSalePrice = item.AverageSalePrice,
                        SaleQuantity = item.SaleQuantity,
                        StartingQuantity = startingQuantities.FirstOrDefault(e => e.Item.Id == item.Item.Id),
                        ItemProjections = new List<ItemProjection>()
                    };

                    var periodsCount = reportingPeriods.Count();
                    for (var i = 0; i < reportingPeriods.Count(); i++)
                    {
                        decimal expenditureQuantity = 0;
                        decimal revenueQuantity = 0;

                        switch (budgetScenarioGroup.Type)
                        {
                            case ItemGroupType.ItemsIBuy:
                                expenditureQuantity = Calculation(item.PurchaseQuantity, periodsCount, i);
                                break;
                            case ItemGroupType.ItemsISell:
                                revenueQuantity = Calculation(item.SaleQuantity, periodsCount, i);
                                break;
                            case ItemGroupType.ItemsIBuyAndSell:
                                expenditureQuantity = Calculation(item.PurchaseQuantity, periodsCount, i);
                                revenueQuantity = Calculation(item.SaleQuantity, periodsCount, i);
                                break;
                        }

                        var projection = new ItemProjection
                        {
                            //BudgetScenarioItem = new BudgetScenarioItem(),
                            ExpenditureQuantity = expenditureQuantity,
                            ExpenditureValue = expenditureQuantity * item.AveragePurchaseCost,
                            ReportingPeriod = reportingPeriods[i],
                            RevenueQuantity = revenueQuantity,
                            RevenueValue = revenueQuantity * item.AverageSalePrice
                        };

                        bItem.ItemProjections.Add(projection);
                    }


                    group.BudgetScenarioItems.Add(bItem);
                });
                dbContext.Entry(group).State = EntityState.Modified;
                dbContext.SaveChanges();
                var itemId = budgetScenarioGroup.BudgetScenarioItems.FirstOrDefault()?.Item.Id ?? 0;
                result.actionVal = group.BudgetScenarioItems.FirstOrDefault(e => e.Item.Id == itemId)?.Id ?? 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, budgetScenarioGroup, userId);
                result.msg = ex.Message;
                result.msgId = "0";
            }

            return result;
        }

        private decimal Calculation(decimal quantity, int count, int index)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, quantity, count, index);
            var integralPart = (int) quantity / count;
            var remainder = (int) quantity % count;
            if (index == 0)
                return integralPart + remainder;
            return integralPart;
        }

        public ReturnJsonModel RemoveBudgetScenarioItem(int budgetScenarioGroupId, int itemId)
        {
            var result = new ReturnJsonModel {actionVal = 1, msgId = "0", result = true};
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, budgetScenarioGroupId, itemId);
                var item = dbContext.BudgetScenarioItems.FirstOrDefault(e =>
                    e.BudgetScenarioItemGroup.Id == budgetScenarioGroupId && e.Item.Id == itemId);
                if (item != null)
                {
                    //item.ItemProjections.Clear();
                    foreach (var itemItemProjection in item.ItemProjections.ToList())
                        dbContext.ItemProjections.Remove(itemItemProjection);
                    //dbContext.Entry(item).State = EntityState.Deleted;
                    dbContext.BudgetScenarioItems.Remove(item);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, budgetScenarioGroupId, itemId);
                result.result = false;
                result.msg = ex.Message;
            }

            return result;
        }

        public List<ItemProjection> PeriodBreakdownItemManagement(int budgetScenarioItemId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, budgetScenarioItemId);
                return dbContext.ItemProjections.Where(e => e.BudgetScenarioItem.Id == budgetScenarioItemId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, budgetScenarioItemId);
                return new List<ItemProjection>();
            }
        }


        public ReturnJsonModel ValidBudgetGroupItemStatus(int id)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var status = dbContext.BudgetScenarioItemGroups.Find(id)?.Status ?? BudgetScenarioItemGroupStatus.Draft;
                if (status != BudgetScenarioItemGroupStatus.Draft)
                    refModel.result = false;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
            }
            return refModel;
        }

        public ReturnJsonModel SendToReviewBudgetScenarioItemGroup(int budgetScenarioItemGroupId, string userId, string originatingConnectionId = "")
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, budgetScenarioItemGroupId);
            var result = new ReturnJsonModel {actionVal = 1, msgId = "0", result = true};
            try
            {
                var budgetScenarioItemGroup =
                    dbContext.BudgetScenarioItemGroups.FirstOrDefault(e => e.Id == budgetScenarioItemGroupId);
                if (budgetScenarioItemGroup != null)
                    budgetScenarioItemGroup.Status = BudgetScenarioItemGroupStatus.Pending;
                if (budgetScenarioItemGroup == null ||
                    budgetScenarioItemGroup.Status != BudgetScenarioItemGroupStatus.Pending)
                    return result;

                var user = dbContext.QbicleUser.Find(userId);
                budgetScenarioItemGroup.WorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var appDef =
                    dbContext.BudgetScenarioItemsApprovalDefinitions.FirstOrDefault(w =>
                        w.BudgetGroupItemsWorkGroup.Id == budgetScenarioItemGroup.WorkGroup.Id);

                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    BudgetScenarioItemGroups = new List<BudgetScenarioItemGroup> {budgetScenarioItemGroup},
                    Name = $"Budget Scenario Item Approval Request",
                    Qbicle = budgetScenarioItemGroup.WorkGroup.Qbicle,
                    Topic = budgetScenarioItemGroup.WorkGroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    App = QbicleActivity.ActivityApp.Trader,
                    IsVisibleInQbicleDashboard = true,
                };

                budgetScenarioItemGroup.ApprovalRequest = approval;
                budgetScenarioItemGroup.ApprovalRequest.ApprovalRequestDefinition = appDef;

                approval.ActivityMembers.AddRange(budgetScenarioItemGroup.WorkGroup.Members);


                dbContext.Entry(budgetScenarioItemGroup).State = EntityState.Modified;
                dbContext.SaveChanges();

                //loging
                var itemsLog = new List<BudgetScenarioItemLog>();
                foreach (var item in budgetScenarioItemGroup.BudgetScenarioItems)
                    itemsLog.Add(new BudgetScenarioItemLog
                    {
                        AveragePurchaseCost = item.AveragePurchaseCost,
                        Item = item.Item,
                        SaleQuantity = item.SaleQuantity,
                        StartingQuantity = item.StartingQuantity,
                        AverageSalePrice = item.AverageSalePrice,
                        BudgetScenarioItemGroup = item.BudgetScenarioItemGroup,
                        PurchaseQuantity = item.PurchaseQuantity,
                        ItemProjections = item.ItemProjections
                    });
                var bsiLog = new BudgetScenarioItemGroupLog
                {
                    Name = budgetScenarioItemGroup.BudgetScenario.Title,
                    CreatedDate = DateTime.UtcNow,
                    Description = budgetScenarioItemGroup.BudgetScenario.Description,
                    Domain = budgetScenarioItemGroup.WorkGroup.Domain,
                    LastUpdatedBy = user,
                    LastUpdatedDate = DateTime.UtcNow,
                    Location = budgetScenarioItemGroup.WorkGroup.Location,
                    Status = budgetScenarioItemGroup.Status,
                    UpdatedBy = user,
                    BudgetScenarioItemGroupApprovalProcess = approval,
                    BudgetScenarioItemLogs = itemsLog,
                    BudgetScenarioItemGroup = budgetScenarioItemGroup,
                    Workgroup = budgetScenarioItemGroup.WorkGroup
                };

                var bsiProcessLog = new BudgetScenarioItemGroupProcessLog
                {
                    AssociatedBudgetScenarioItemGroup = budgetScenarioItemGroup,
                    AssociatedBudgetScenarioItemGroupLog = bsiLog,
                    BudgetScenarioItemGroupStatus = budgetScenarioItemGroup.Status,
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

                dbContext.BudgetScenarioItemGroupProcessLogs.Add(bsiProcessLog);
                dbContext.Entry(bsiProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();


                var nRule = new NotificationRules(dbContext);

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
                nRule.Notification2Activity(activityNotification);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, budgetScenarioItemGroupId, userId);
                result.result = false;
                result.msg = ex.Message;
            }

            return result;
        }

        public void BudgetScenarioItemGroupApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var budgetScenarioItemGroup = approval.BudgetScenarioItemGroups.FirstOrDefault();
                if (budgetScenarioItemGroup == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        budgetScenarioItemGroup.Status = BudgetScenarioItemGroupStatus.Pending;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        budgetScenarioItemGroup.Status = BudgetScenarioItemGroupStatus.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        budgetScenarioItemGroup.Status = BudgetScenarioItemGroupStatus.Approved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        budgetScenarioItemGroup.Status = BudgetScenarioItemGroupStatus.Discarded;
                        break;
                }

                budgetScenarioItemGroup.LastUpdateDate = DateTime.UtcNow;
                budgetScenarioItemGroup.LastUpdatedBy = approval.ApprovedOrDeniedAppBy;
                dbContext.Entry(budgetScenarioItemGroup).State = EntityState.Modified;
                dbContext.SaveChanges();

                //logging
                var itemsLog = new List<BudgetScenarioItemLog>();
                foreach (var item in budgetScenarioItemGroup.BudgetScenarioItems)
                    itemsLog.Add(new BudgetScenarioItemLog
                    {
                        AveragePurchaseCost = item.AveragePurchaseCost,
                        Item = item.Item,
                        SaleQuantity = item.SaleQuantity,
                        StartingQuantity = item.StartingQuantity,
                        AverageSalePrice = item.AverageSalePrice,
                        BudgetScenarioItemGroup = item.BudgetScenarioItemGroup,
                        PurchaseQuantity = item.PurchaseQuantity,
                        ItemProjections = item.ItemProjections
                    });


                var bsiLog = new BudgetScenarioItemGroupLog
                {
                    Name = budgetScenarioItemGroup.BudgetScenario.Title,
                    CreatedDate = DateTime.UtcNow,
                    Description = budgetScenarioItemGroup.BudgetScenario.Description,
                    Domain = budgetScenarioItemGroup.WorkGroup.Domain,
                    LastUpdatedBy = approval.ApprovedOrDeniedAppBy,
                    LastUpdatedDate = DateTime.UtcNow,
                    Location = budgetScenarioItemGroup.WorkGroup.Location,
                    Status = budgetScenarioItemGroup.Status,
                    UpdatedBy = approval.ApprovedOrDeniedAppBy,
                    BudgetScenarioItemGroupApprovalProcess = approval,
                    BudgetScenarioItemLogs = itemsLog,
                    BudgetScenarioItemGroup = budgetScenarioItemGroup,
                    Workgroup = budgetScenarioItemGroup.WorkGroup
                };

                var bsiProcessLog = new BudgetScenarioItemGroupProcessLog
                {
                    AssociatedBudgetScenarioItemGroup = budgetScenarioItemGroup,
                    AssociatedBudgetScenarioItemGroupLog = bsiLog,
                    BudgetScenarioItemGroupStatus = budgetScenarioItemGroup.Status,
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

                dbContext.BudgetScenarioItemGroupProcessLogs.Add(bsiProcessLog);
                dbContext.Entry(bsiProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public List<ApprovalStatusTimeline> BudgetGroupItemApprovalStatusTimeline(int id, string timeZone)
        {
            try
            {
                //SaleLog
                //    SaleProcessLog
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var timeline = new List<ApprovalStatusTimeline>();
                var logs = dbContext.BudgetScenarioItemGroupProcessLogs
                    .Where(e => e.AssociatedBudgetScenarioItemGroup.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string status = "", icon = "";

                foreach (var log in logs)
                {
                    switch (log.BudgetScenarioItemGroupStatus)
                    {
                        case BudgetScenarioItemGroupStatus.Pending:
                            status = StatusLabelName.Pending;
                            icon = "fa fa-info bg-aqua";
                            break;
                        case BudgetScenarioItemGroupStatus.Reviewed:
                            status = StatusLabelName.Reviewed;
                            icon = "fa fa-truck bg-yellow";
                            break;
                        case BudgetScenarioItemGroupStatus.Approved:
                            status = StatusLabelName.Approved;
                            icon = "fa fa-check bg-green";
                            break;
                        case BudgetScenarioItemGroupStatus.Denied:
                            status = StatusLabelName.Denied;
                            icon = "fa fa-warning bg-red";
                            break;
                        case BudgetScenarioItemGroupStatus.Discarded:
                            status = StatusLabelName.Discarded;
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

                return timeline;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timeZone);
                return new List<ApprovalStatusTimeline>();
            }
        }

        public ReturnJsonModel ConfirmPeriodChange(List<ItemProjection> itemProjections)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemProjections);
                var budgetScenarioItemId = itemProjections.FirstOrDefault()?.BudgetScenarioItem.Id ?? 0;
                var budgetScenarioItem = dbContext.BudgetScenarioItems.Find(budgetScenarioItemId);
                if (budgetScenarioItem == null) return refModel;
                budgetScenarioItem.ItemProjections.ForEach(projection =>
                {
                    var projectionUpdate = itemProjections.FirstOrDefault(e => e.Id == projection.Id);
                    if (projectionUpdate == null) return;
                    projection.ExpenditureQuantity = projectionUpdate.ExpenditureQuantity;
                    projection.ExpenditureValue = projectionUpdate.ExpenditureValue;

                    projection.RevenueQuantity = projectionUpdate.RevenueQuantity;
                    projection.RevenueValue = projectionUpdate.RevenueValue;
                });
                dbContext.Entry(budgetScenarioItem).State = EntityState.Modified;
                dbContext.SaveChanges();


            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, itemProjections);
                refModel.result = false;
            }

            return refModel;
        }
    }
}