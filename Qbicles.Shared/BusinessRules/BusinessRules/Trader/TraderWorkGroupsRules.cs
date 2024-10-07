using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.TraderWorkgroup;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderWorkGroupsRules
    {
        private readonly ApplicationDbContext _db;

        public TraderWorkGroupsRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();

        public List<WorkGroup> GetWorkGroups(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.WorkGroups.Where(d => d.Domain.Id == domainId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<WorkGroup>();
            }
        }
        public List<WorkGroup> GetWorkGroupsByLocationId(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);
                return DbContext.WorkGroups.Where(p => p.Location.Id == locationId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId);
                return new List<WorkGroup>();
            }
        }

        public WorkGroup GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.WorkGroups.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new WorkGroup();
            }
        }

        public bool WorkGroupNameCheck(WorkGroup wg, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, wg, domainId);
                if (wg.Id > 0)
                    return DbContext.WorkGroups.Any(x =>
                        x.Id != wg.Id && x.Domain.Id == domainId && x.Name == wg.Name);
                return DbContext.WorkGroups.Any(x =>
                    x.Name == wg.Name && x.Domain.Id == domainId);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, wg, domainId);
                return false;
            }
        }

        public bool Create(WorkGroup wg, string userId, string appImage)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, wg, userId, appImage);

                var currentUser = DbContext.QbicleUser.Find(userId);
                var qbicle = new QbicleRules(DbContext).GetQbicleById(wg.Qbicle.Id);
                var workGroup = new WorkGroup
                {
                    Name = wg.Name,
                    Processes = new List<TraderProcess>(),
                    Qbicle = qbicle,
                    Topic = new TopicRules(DbContext).GetTopicById(wg.Topic.Id),
                    ItemCategories =
                        new TraderGroupRules(DbContext).GetByIds(wg.ItemCategories.Select(e => e.Id)),
                    Location = new TraderLocationRules(DbContext).GetById(wg.Location.Id),
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    Domain = wg.Domain,
                    ApprovalDefs = new List<ApprovalRequestDefinition>()
                };
                foreach (var process in wg.Processes)
                {
                    var proc = new TraderProcessRules(DbContext).GetById(process.Id);
                    workGroup.Processes.Add(proc);
                }

                if (wg.Members != null)
                    foreach (var user in wg.Members)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroup.Members.Add(member);
                    }

                if (wg.Reviewers != null)
                    foreach (var user in wg.Reviewers)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroup.Reviewers.Add(member);
                    }

                if (wg.Approvers != null)
                    foreach (var user in wg.Approvers)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroup.Approvers.Add(member);
                    }
                // create ApprovalRequestDefinition
                ApprovalDefsForWorgroup(workGroup, currentUser, appImage);


                DbContext.WorkGroups.Add(workGroup);
                DbContext.Entry(workGroup).State = EntityState.Added;
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, wg, userId, appImage);
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workGroup"></param>
        /// <param name="currentUser">Send to from Rule - can use user object</param>
        /// <param name="imageUri"></param>
        public void ApprovalDefsForWorgroup(WorkGroup workGroup, ApplicationUser currentUser, string imageUri)
        {
            foreach (var process in workGroup.Processes)
            {
                string groupName;
                var rule = new ApprovalAppsRules(DbContext);
                ApprovalGroup appGroup;


                switch (process.Name)
                {
                    case TraderProcessName.TraderPurchaseProcessName:
                        groupName = "Trader Purchase Approvals";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var purchaseAppDef = new PurchaseApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(purchaseAppDef);
                        break;
                    case TraderProcessName.TraderSaleProcessName:
                        groupName = "Trader Sale Approvals";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var saleAppDef = new SalesApprovalDefinition()
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(saleAppDef);
                        break;
                    case TraderProcessName.TraderSaleReturnProcessName:
                        groupName = "Trader Sale Return Approvals";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var saleReturnAppDef = new SalesReturnApprovalDefinition()
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            SaleReturnWorkGroup = workGroup,
                            SaleReturnTraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(saleReturnAppDef);
                        break;
                    case TraderProcessName.TraderTransferProcessName:
                        groupName = "Trader Transfer Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var transferAppDef = new TransferApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(transferAppDef);
                        break;
                    case TraderProcessName.TraderPaymentProcessName:
                        groupName = "Trader Payment Approvals";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var paymentAppDef = new PaymentApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(paymentAppDef);
                        break;
                    case TraderProcessName.TraderContactProcessName:
                        groupName = "Trader Contact Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var contactAppDef = new ContactApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(contactAppDef);
                        break;
                    case TraderProcessName.TraderInvoiceProcessName:
                        groupName = "Trader Invoice Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var invoiceAppDef = new InvoiceApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(invoiceAppDef);
                        break;

                    case TraderProcessName.TraderSpotCountProcessName:
                        groupName = "Trader Spot Count Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var spotCountAppDef = new SpotCountApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(spotCountAppDef);
                        break;

                    case TraderProcessName.TraderWasteReportProcessName:
                        groupName = "Trader Waste Report Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var wasteReportAppDef = new WasteReportApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(wasteReportAppDef);
                        break;

                    case TraderProcessName.Manufacturing:
                        groupName = "Trader Manufacturing Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var manufacturingCountAppDef = new ManuApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            WorkGroup = workGroup,
                            TraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(manufacturingCountAppDef);
                        break;

                    case TraderProcessName.CreditNotes:
                        groupName = "Credit Notes Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var creditNotesAppDef = new CreditNoteApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            CreditNoteWorkGroup = workGroup,
                            CreditNoteProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(creditNotesAppDef);
                        break;

                    case TraderProcessName.Budget:
                        groupName = "Budget";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var budgetScenarioAppDef = new BudgetScenarioItemsApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            BudgetGroupItemsWorkGroup = workGroup,
                            BudgetGroupItemsProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(budgetScenarioAppDef);
                        break;
                    case TraderProcessName.StockAudits:
                    case TraderProcessName.ShiftAudits:
                        groupName = $"{process.Name} Processes";

                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var stockAuditAppDef = new ShiftAuditApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            ShiftAuditWorkGroup = workGroup,
                            ShiftAuditTraderProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(stockAuditAppDef);
                        break;
                    case TraderProcessName.TraderCashManagement:
                        groupName = "Trader Cash Management Processes";
                        appGroup = rule.GetApprovalAppsGroupByName(groupName, workGroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, workGroup.Domain).Object;
                        var cashManagementAppDef = new TillPaymentApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{workGroup.Name}/ {process.Name}",
                            ApprovalImage = imageUri,
                            Description = $"Trader WorkGroup: {workGroup.Name} {process.Name} process",
                            Initiators = workGroup.Members,
                            Approvers = workGroup.Approvers,
                            Reviewers = workGroup.Reviewers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            TillPaymentWorkGroup = workGroup,
                            TillPaymentProcessType = process
                        };
                        workGroup.ApprovalDefs.Add(cashManagementAppDef);
                        break;
                }
            }
        }
        public bool Update(WorkGroup wg, string userId, string appImage)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, wg, userId, appImage);

                var currentUser = DbContext.QbicleUser.Find(userId);

                var qbicle = new QbicleRules(DbContext).GetQbicleById(wg.Qbicle.Id);
                foreach (var user in wg.Members)
                {
                    var member = new UserRules(DbContext).GetUser(user.Id, 0);
                    if (qbicle.Members.All(u => u.Id != user.Id))
                        qbicle.Members.Add(member);
                }

                var workGroupDb = GetById(wg.Id);
                workGroupDb.ItemCategories.Clear();
                workGroupDb.Members.Clear();
                workGroupDb.Approvers.Clear();
                workGroupDb.Reviewers.Clear();
                if (wg.Members != null)
                    foreach (var user in wg.Members)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroupDb.Members.Add(member);
                    }

                if (wg.Reviewers != null)
                    foreach (var user in wg.Reviewers)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroupDb.Reviewers.Add(member);
                    }

                if (wg.Approvers != null)
                    foreach (var user in wg.Approvers)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroupDb.Approvers.Add(member);
                    }
                //var lazyLoad = wkg.ItemCategories;
                workGroupDb.Name = wg.Name;

                foreach (var process in wg.Processes)
                {
                    if (workGroupDb.Processes.Any(p => p.Id == process.Id)) continue;
                    var proc = new TraderProcessRules(DbContext).GetById(process.Id);
                    workGroupDb.Processes.Add(proc);

                    string groupName;
                    var rule = new ApprovalAppsRules(DbContext);
                    ApprovalGroup appGroup;

                    switch (proc.Name)
                    {
                        case TraderProcessName.TraderPurchaseProcessName:
                            groupName = "Trader Purchase Approvals";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var purchaseAppDef = new PurchaseApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(purchaseAppDef);
                            break;
                        case TraderProcessName.TraderSaleProcessName:
                            groupName = "Trader Sale Approvals";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var saleAppDef = new SalesApprovalDefinition()
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(saleAppDef);
                            break;

                        case TraderProcessName.TraderSaleReturnProcessName:
                            groupName = "Trader Sale Return Approvals";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var saleReturnAppDef = new SalesReturnApprovalDefinition()
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                SaleReturnWorkGroup = workGroupDb,
                                SaleReturnTraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(saleReturnAppDef);
                            break;
                        case TraderProcessName.TraderTransferProcessName:
                            groupName = "Trader Transfer Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var transferAppDef = new TransferApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(transferAppDef);
                            break;
                        case TraderProcessName.TraderPaymentProcessName:
                            groupName = "Trader Payment Approvals";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var paymentAppDef = new PaymentApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(paymentAppDef);
                            break;
                        case TraderProcessName.TraderContactProcessName:
                            groupName = "Trader Contact Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var contactAppDef = new ContactApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(contactAppDef);
                            break;
                        case TraderProcessName.TraderInvoiceProcessName:
                            groupName = "Trader Invoice Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var invoiceAppDef = new InvoiceApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(invoiceAppDef);
                            break;

                        case TraderProcessName.TraderSpotCountProcessName:
                            groupName = "Trader Spot Count Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var spotCountAppDef = new SpotCountApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(spotCountAppDef);
                            break;

                        case TraderProcessName.TraderWasteReportProcessName:
                            groupName = "Trader Waste Report Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var wasteReportAppDef = new WasteReportApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(wasteReportAppDef);
                            break;

                        case TraderProcessName.Manufacturing:
                            groupName = "Trader Manufacturing Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var manufacturingCountAppDef = new ManuApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{wg.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {wg.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                WorkGroup = workGroupDb,
                                TraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(manufacturingCountAppDef);
                            break;

                        case TraderProcessName.CreditNotes:
                            groupName = "Credit Notes Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var creditNotesAppDef = new CreditNoteApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{workGroupDb.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {workGroupDb.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                CreditNoteWorkGroup = workGroupDb,
                                CreditNoteProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(creditNotesAppDef);
                            break;

                        case TraderProcessName.Budget:
                            groupName = "Budget Scenario Item Addition";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var budgetScenarioAppDef = new BudgetScenarioItemsApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{workGroupDb.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {workGroupDb.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                BudgetGroupItemsWorkGroup = workGroupDb,
                                BudgetGroupItemsProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(budgetScenarioAppDef);
                            break;
                        case TraderProcessName.StockAudits:
                        case TraderProcessName.ShiftAudits:
                            groupName = $"{proc.Name} Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var stockAuditAppDef = new ShiftAuditApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{workGroupDb.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {workGroupDb.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                ShiftAuditWorkGroup = workGroupDb,
                                ShiftAuditTraderProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(stockAuditAppDef);
                            break;
                        case TraderProcessName.TraderCashManagement:
                            groupName = "Trader Cash Management Processes";
                            appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                       (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                            var cashManagementAppDef = new TillPaymentApprovalDefinition
                            {
                                Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                                Title = $"{workGroupDb.Name}/ {proc.Name}",
                                ApprovalImage = appImage,
                                Description = $"Trader WorkGroup: {workGroupDb.Name} {proc.Name} process",
                                Initiators = workGroupDb.Members,
                                Approvers = workGroupDb.Approvers,
                                Reviewers = workGroupDb.Reviewers,
                                IsViewOnly = true,
                                Group = appGroup,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow,
                                TillPaymentWorkGroup = workGroupDb,
                                TillPaymentProcessType = proc
                            };
                            workGroupDb.ApprovalDefs.Add(cashManagementAppDef);
                            break;
                    }
                }

                workGroupDb.Qbicle = qbicle;
                workGroupDb.Topic = new TopicRules(DbContext).GetTopicById(wg.Topic.Id);
                workGroupDb.ItemCategories =
                    new TraderGroupRules(DbContext).GetByIds(wg.ItemCategories.Select(e => e.Id));
                workGroupDb.Location = new TraderLocationRules(DbContext).GetById(wg.Location.Id);
                //update workgroup processes
                var removeProcesses = new List<TraderProcess>();
                foreach (var p in workGroupDb.Processes)
                {
                    if (wg.Processes.Any(e => e.Id == p.Id)) continue;
                    var pRemove = workGroupDb.Processes.FirstOrDefault(e => e.Id == p.Id);
                    removeProcesses.Add(pRemove);
                }

                removeProcesses.ForEach(process => { workGroupDb.Processes.Remove(process); });

                foreach (var approvalDef in workGroupDb.ApprovalDefs)
                {
                    if (approvalDef.Id == 0) continue;
                    approvalDef.Initiators.Clear();
                    approvalDef.Initiators = workGroupDb.Members;
                    approvalDef.Reviewers.Clear();
                    approvalDef.Reviewers = workGroupDb.Reviewers;
                    approvalDef.Approvers.Clear();
                    approvalDef.Approvers = workGroupDb.Approvers;
                }
                if (DbContext.Entry(workGroupDb).State == EntityState.Detached)
                    DbContext.WorkGroups.Attach(workGroupDb);
                DbContext.Entry(workGroupDb).State = EntityState.Modified;
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, wg, userId, appImage);
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var wg = DbContext.WorkGroups.FirstOrDefault(e => e.Id == id);
                if (wg == null) return false;
                int domainId = wg.Domain.Id;

                if (wg.ApprovalDefs != null && wg.ApprovalDefs.Count > 0)
                {
                    foreach (var approvalDef in wg.ApprovalDefs.ToList())
                    {
                        var approvalReqs =
                            DbContext.ApprovalReqs.Any(e => e.ApprovalRequestDefinition.Id == approvalDef.Id);
                        if (!approvalReqs)
                            DbContext.ApprovalAppsRequestDefinition.Remove(approvalDef);
                    }
                }
                DbContext.WorkGroups.Remove(wg);
                DbContext.SaveChanges();
                if (DbContext.WorkGroups.Any(d => d.Domain.Id == domainId))
                    return true;
                var traderSetting = DbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (traderSetting == null) return true;
                traderSetting.IsSetupCompleted = TraderSetupCurrent.Workgroup;
                DbContext.Entry(traderSetting).State = EntityState.Modified;
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return false;
            }
        }

        public WorkgroupUser GetWorkgroupUser(int id)
        {
            var wgU = new WorkgroupUser();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var wg = GetById(id);
                wgU.Members = wg.Members.Select(u => new WorkgroupMember
                {
                    Id = u.Id,
                    Name = HelperClass.GetFullNameOfUser(u),
                    Pic = u.ProfilePic.ToUriString()
                }).ToList();
                wgU.Approvers = wg.Approvers.Select(u => new WorkgroupMemberId { Id = u.Id }).ToList();
                wgU.Reviewers = wg.Reviewers.Select(u => new WorkgroupMemberId { Id = u.Id }).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
            }

            return wgU;
        }

        public string ReInitUsersEdit(int id, QbicleDomain domain)
        {
            var tr = new StringBuilder();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, domain);
                var wgMembers = GetById(id)?.Members;
                foreach (var u in domain.Users.OrderBy(u => u.UserName))
                {
                    var fullName = HelperClass.GetFullNameOfUser(u);
                    var isMember = "";
                    if (wgMembers != null && wgMembers.Any(user => user.Id == u.Id))
                        isMember = "checked";
                    tr.Append("<tr id=\"tr_edit_user_" + u.Id + "\">");
                    tr.Append("<td>");
                    tr.Append(
                        $"<div class=\"table-avatar mini\" style=\"background-image: url('{u.ProfilePic.ToUri()}');\">&nbsp;</div>");
                    tr.Append("</td>");
                    tr.Append($"<td>{fullName}</td>");
                    tr.Append("<td>");
                    var roleAll = "";
                    foreach (var r in u.DomainRoles.Where(d => d.Domain.Id == domain.Id))
                    {
                        roleAll += r.Name + ",";
                        tr.Append($"<span class=\"label label-lg label-info\">{r.Name}</span> ");
                    }

                    tr.Append($"<span class=\"hidden\">{roleAll}</span>");
                    tr.Append("</td>");
                    tr.Append("<td>");
                    tr.Append("<div class=\"checkbox toggle\">");
                    tr.Append(
                        $"<input {isMember} data-fullname=\"{fullName}\" onchange=\"AddUsersToMembers(this.checked,'{u.Id}', $(this).data('fullname'),'{u.ProfilePic.ToUri()}')\" class=\"check-right\" data-toggle=\"toggle\" data-onstyle=\"success\" data-on=\"<i class='fa fa-check'></i>\" data-off=\" \" type=\"checkbox\">");

                    tr.Append("</div>");
                    tr.Append("</td>");
                    tr.Append("</tr>");
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, domain);
            }


            return tr.ToString();
        }
        public List<WorkGroup> GetWorkGroups(int locationId, QbicleDomain domain, string userId, string processName)
        {
            if (domain != null && domain.Workgroups.Any())
            {
                return domain.Workgroups.Where(q =>
                     q.Location.Id == locationId
                     && q.Processes.Any(p => p.Name.Equals(processName))
                     && q.Members.Select(u => u.Id).Contains(userId)).OrderBy(n => n.Name).ToList();
            }
            return new List<WorkGroup>();
        }

        public List<WorkroupModel> GetWorkGroupsConfig(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return (from d in DbContext.WorkGroups
                        where d.Domain.Id == domainId
                        select new WorkroupModel
                        {
                            Id = d.Id,
                            Name = d.Name,
                            CreatedDate = d.CreatedDate,
                            CreatedBy = d.CreatedBy,
                            CanDelete = d.ApprovalDefs.Count == 0 && d.ApprovalDefs.Any(a => a.ApprovalReqs.Any())
                                        && d.Payments.Count == 0 && d.Purchases.Count == 0 && d.Sales.Count == 0
                                        && d.Contacts.Count == 0 && d.Transfers.Count == 0 && d.Invoices.Count == 0,
                            ItemCategories = d.ItemCategories.Count,
                            Members = d.Members.Count,
                            Qbicle = d.Qbicle.Name,
                            Location = d.Location.Name,
                            Processes = d.Processes.Select(p => p.Name)
                        }).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<WorkroupModel>();
            }
        }

        public DataTablesResponse GetWorkgroupDataTableData(IDataTablesRequest requestModel, string keySearch, List<int> lstProcessIds, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, keySearch, lstProcessIds, domainId);

                int totalcount = 0;
                #region Filters
                var query = DbContext.WorkGroups.Where(d => d.Domain.Id == domainId);
                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(s => s.Name.ToLower().Contains(keySearch.ToLower()));
                }
                if (lstProcessIds != null && lstProcessIds.Count() > 0)
                {
                    if (lstProcessIds.Count() == 1 && lstProcessIds[0] != 0)                    
                        query = query.Where(s => s.Processes.Any(x => lstProcessIds.Contains(x.Id)));
                }

                totalcount = query.Count();
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
                            orderByString += "Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion

                var currentDomainPlan = DbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
                var currentDomainPlanLevel = currentDomainPlan?.Level?.Level ?? BusinessDomainLevelEnum.Free;
                var isOptionShown = currentDomainPlanLevel > BusinessDomainLevelEnum.Starter;

                var dataJson = list.Select(s => new
                {
                    Name = s.Name,
                    Creator = s.CreatedBy.GetFullName(),
                    CreatedDate = s.CreatedDate.ToString(),
                    Location = s.Location?.Name ?? "",
                    Process = String.Join("<br />", s.Processes.Select(x => x.Name).ToList()),
                    QbicleName = s.Qbicle?.Name ?? "",
                    MemberNum = s.Members?.Count() ?? 0,
                    ProductGroupNum = s.ItemCategories.Count(),
                    Id = s.Id,
                    CanBeDeleted =
                    (s.ApprovalDefs == null || !s.ApprovalDefs.Any(a => a.ApprovalReqs.Count > 0))
                    && (s.Invoices == null || !s.Invoices.Any())
                    && (s.Payments == null || !s.Payments.Any())
                    && (s.Purchases == null || !s.Purchases.Any())
                    && (s.Sales == null || !s.Sales.Any())
                    && (s.Contacts == null || !s.Contacts.Any())
                    && (s.Transfers == null || !s.Transfers.Any()),
                    IsOptionBtnShown = isOptionShown,
                    IsDeleteBtnDisabled = totalcount <= 1
                });
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keySearch, lstProcessIds, domainId);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
    }
}