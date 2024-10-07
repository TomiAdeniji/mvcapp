using System.Globalization;
using CleanBooksData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Model;
using static Qbicles.BusinessRules.Enums;
using System.Text;
using Qbicles.Models;
using static Qbicles.Models.QbicleTask;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;
using System.Reflection;

namespace Qbicles.BusinessRules
{ 

    public class CBTasksRules
    {
        ApplicationDbContext dbContext;
        

        public CBTasksRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<TasksModel> GetTasks()
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.AutoDetectChangesEnabled = false;
                dbContext.Configuration.ValidateOnSaveEnabled = false;
                dbContext.Configuration.ProxyCreationEnabled = false;

                List<TasksModel> tk = new List<TasksModel>();
                var groups = dbContext.tasks.Include(u => u.user).Include(t => t.tasktype).Select(tks => new
                {
                    task = tks,
                    taskinstance = tks.taskinstances.OrderByDescending(m => m.DateExecuted).FirstOrDefault(),
                    userCreate = tks.user,
                    tasktype = tks.tasktype
                })
                    .ToArrayAsync();

                foreach (var item in groups.Result)
                {
                    tk.Add(new TasksModel
                    {
                        task = item.task,
                        taskInstance = item.taskinstance,
                        userCreate = item.userCreate,
                        tasktype = item.tasktype
                    });
                }
                return tk;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
            finally
            {
                dbContext.Configuration.AutoDetectChangesEnabled = true;
                dbContext.Configuration.ValidateOnSaveEnabled = true;
                dbContext.Configuration.ProxyCreationEnabled = true;
            }
        }

        public List<task> GetTasks(int qbicleId)
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                return dbContext.tasks.Include(t => t.tasktype).Where(qb => qb.QbicleTask.Qbicle.Id == qbicleId).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
            finally
            { }
        }
        public int[] GetDomainRoleAccounts(int accountId)
        {
            return dbContext.tasks.Find(accountId)?.DomainRoles.Select(e => e.Id).ToList().ToArray();
        }
        public task GetTask(int id)
        {
            return dbContext.tasks.Find(id);
        }
        public IEnumerable<taskgroup> GetTaskgroup(int domainId)
        {
            return dbContext.taskgroups.Include(u => u.user).Where(d => d.qbicledomain.Id == domainId);
        }
        public IEnumerable<taskexecutioninterval> GetTaskexecutioninterval()
        {
            return dbContext.taskexecutionintervals; ;
        }
        public IEnumerable<accountgroup> GetAccountgroup()
        {
            return dbContext.accountgroups; ;
        }
        public List<Account> GetAccounts(int domainId)
        {
            var group = new CBAccountRules(dbContext).GetAccountGroup(domainId);
            var acc = new List<Account>();
            foreach (var item in group)
            {
                acc.AddRange(item.Accounts);
            }
            return acc;
        }
        public IEnumerable<transactionmatchingtype> GetTransactionmatchingtype()
        {
            return dbContext.transactionmatchingtypes.Where(c => c.Id != (int)Enums.tranMatchingType.NotApplicable).ToList(); ;
        }
        public IEnumerable<balanceanalysispredefaction> GetBalanceanalysispredefaction()
        {
            return dbContext.balanceanalysispredefactions.ToList(); ;
        }
        public IEnumerable<tasktype> GetTasktype()
        {
            return dbContext.tasktypes.ToList(); ;
        }
        public IEnumerable<task> GetTaskToTaskType(int taskTypeId, int groupId)
        {
            return dbContext.tasks.Include(u => u.user).Include(t => t.tasktype).Where(w => w.ReconciliationTaskGroupId == groupId);
        }
        public bool DupplicateRecTaskGroupCheck(taskgroup recGroup)
        {
            if (recGroup.Id > 0)
            {
                return dbContext.taskgroups.Any(x => x.Id != recGroup.Id && x.Name.ToLower() == recGroup.Name.ToLower());
            }
            else
            {
                return dbContext.taskgroups.Any(x => x.Name.ToLower() == recGroup.Name.ToLower());
            }
        }

        public bool CheckDeleteTaskGroup(int taskGroupId)
        {
            return dbContext.tasks.Any(s => s.taskgroup.Id == taskGroupId);
        }
        public bool DeleteTaskGroup(int taskGroupId)
        {
            var tskGroup = new taskgroup { Id = taskGroupId };
            dbContext.taskgroups.Attach(tskGroup);
            dbContext.taskgroups.Remove(tskGroup);
            dbContext.SaveChanges();
            return true;
        }

        public taskgroup Get_reconciliationtaskgroups(int id)
        {
            var reconciliationtaskgroups = dbContext.taskgroups.FirstOrDefault(p => p.Id == id);
            var obj = new taskgroup();
            if (reconciliationtaskgroups != null)
            {
                obj.Id = reconciliationtaskgroups.Id;
                obj.CreatedById = reconciliationtaskgroups.CreatedById;
                obj.CreatedDate = reconciliationtaskgroups.CreatedDate;
                obj.Name = reconciliationtaskgroups.Name;

            }
            return obj;
        }
        public task Get_reconciliationtasks(int id)
        {
            var reconciliationtasks = dbContext.tasks.Find(id);

            return reconciliationtasks;
        }
        public ModelTask GetTaskEdit(int id)
        {
            var task = dbContext.tasks.Find(id);
            var taskintansce_bool = task.taskinstances.Any();
            var is_InPresson = task.taskinstances.Any(m => m.IsComplete == 0);


            var taskAccount = new List<taskaccount>();
            foreach (var tAccount in task.taskaccounts)
            {
                taskAccount.Add(new taskaccount
                {
                    AccountId = tAccount.AccountId,
                    Order = tAccount.Order
                });
            }

            var model = new ModelTask
            {
                task = new task
                {
                    Id = task.Id,
                    Name = task.Name,
                    TaskTypeId = task.TaskTypeId,
                    TaskExecutionIntervalId = task.TaskExecutionIntervalId,
                    AssignedUserId = task.AssignedUserId,
                    CreatedById = task.CreatedById,
                    CreatedDate = task.CreatedDate,
                    InitialTransactionDate = task.InitialTransactionDate,
                    TransactionMatchingTypeId = task.TransactionMatchingTypeId,
                    Description = task.Description,
                    ReconciliationTaskGroupId = task.ReconciliationTaskGroupId,
                    WorkGroup = new CBWorkGroup()
                    {
                        Id = task.WorkGroup != null ? task.WorkGroup.Id : 0,
                        Name = task.WorkGroup != null ? task.WorkGroup.Name : ""
                    }
                },
                taskinstance = taskintansce_bool,
                taskaccount = taskAccount,
                isInPresson = is_InPresson,
                WorkGroupId = task.WorkGroup != null ? task.WorkGroup.Id : 0,
                Priority = task.QbicleTask.Priority,
                Deadline = HelperClass.Converter.Obj2DateTime(task.QbicleTask.DueDate).ToString("dd/MM/yyyy HH:mm")
            };
            return model;
        }


        public bool CheckEdit(int id)
        {
            return dbContext.taskinstances.Any(m => m.TaskId == id && m.IsComplete == 0);
        }
        public IEnumerable<UserModel> GetUserAsign(int domainId)
        {
            var wgs = dbContext.CBWorkGroups.Where(q =>
                    q.Domain.Id == domainId && q.Processes.Any(p => p.Name == CBProcessName.TaskExecutionProcessName))
                .ToList();
            var userAccount = wgs.SelectMany(q=>q.Members).Distinct().Select(x =>
               new UserModel
               {
                   Id = x.Id,
                   UserName = x.Forename + " " + x.Surname
               }).ToList();
            return userAccount;

        }
        public List<balanceanalysismappingrule> Get_editBalanceMapingRuler(int id, ref List<BalanceanalysisModel> lstBalanceModel1)
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.ProxyCreationEnabled = false;

                var balance = dbContext.balanceanalysismappingrules.Where(p => p.TaskId == id).ToList();
                var lstBalanceModel = new List<BalanceanalysisModel>();
                BalanceanalysisModel balancemodel;
                foreach (var item in balance)
                {
                    balancemodel = new BalanceanalysisModel
                    {
                        Amount1 = item.MinDifference,
                        Amount2 = item.MaxDifference,
                        Description1 = item.Description1,
                        Description2 = item.Description2,
                        Reference1 = item.Reference1,
                        Reference2 = item.Reference2
                    };
                    lstBalanceModel.Add(balancemodel);
                }
                lstBalanceModel1 = lstBalanceModel;

                return balance;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
            finally
            {
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }


        }
        public IEnumerable<balanceanalysisaction> Get_editBalanceAction(int id)
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.ProxyCreationEnabled = false;

                return dbContext.balanceanalysisactions.Where(p => p.TaskId == id).ToList();
            }
            catch
            {
                return null;
            }
            finally
            {
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }

        }
        public transactionmatchingtaskrulesacces Get_rulesaccess(int id)
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.ProxyCreationEnabled = false;

                return dbContext.transactionmatchingtaskrulesaccess.FirstOrDefault(p => p.TaskId == id);
            }
            catch
            {
                return null;
            }
            finally
            {
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }

        }
        public bool TaskNameCheck(string name, int id)
        {
            var dupplicate = false;
            if (id <= 0)
                dupplicate = dbContext.tasks.Any(m => m.Name.ToLower() == name.ToLower());
            else
                dupplicate = dbContext.tasks.Any(m => m.Name.ToLower() == name.ToLower() && m.Id != id);
            return dupplicate;
        }
        public ReturnJsonModel SaveTaskGroups(taskgroup group)
        {
            var refModel = new ReturnJsonModel();
            if (group != null)
            {
                if (group.Id > 0)
                {
                    if (dbContext.Entry(group).State == EntityState.Detached)
                        dbContext.taskgroups.Attach(group);
                    dbContext.Entry(group).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.actionVal = 2;
                    refModel.msg = group.Name;
                    refModel.msgId = group.Id.ToString();
                    refModel.msgName = group.Name;
                }
                else
                {
                    group.CreatedDate = DateTime.UtcNow;
                    dbContext.taskgroups.Add(group);
                    dbContext.Entry(group).State = EntityState.Added;
                    dbContext.SaveChanges();
                    refModel.actionVal = 1;
                    //append to select group
                    refModel.msgId = group.Id.ToString();
                    refModel.msgName = group.Name;


                    var groupViewer = new StringBuilder();
                    groupViewer.Append($"<button onclick = \"EditGroup('{group.Id}')\" class='btn btn-warning'><i class='fa fa-pencil'></i> &nbsp; Edit group</button>&nbsp;");
                    groupViewer.Append($"<button class='btn btn-success' onclick = \"AddTask('{group.Id}')\"><i class='fa fa-plus'></i> &nbsp; Add a Task</button>");
                    groupViewer.Append($"<br /><br />");

                    var groupGrid = new StringBuilder();
                    groupGrid.Append($"<h5><span id='task-group-name-grid-{group.Id}'>{group.Name}</span></h5><hr />");
                    groupGrid.Append(groupViewer);
                    groupGrid.Append($"<ul id='ul-account-{group.Id}' class='grid-list'></ul>");
                    refModel.msg = groupGrid.ToString();

                    var groupList = new StringBuilder();
                    groupList.Append($"<h5><span id='task-group-name-list-{group.Id}'>{group.Name}</span></h5><hr />");
                    groupList.Append(groupViewer);
                    groupList.Append("<br />");

                    groupList.Append($"<table id='tableList-{group.Id}' class='accgroup-table table table-hover t1style valignm custome-table' style='width:100%' cellspacing='0'>");
                    groupList.Append($"<thead>");
                    groupList.Append($"<tr>");
                    groupList.Append($"<th data-priority='1'>Name</th>");
                    groupList.Append($"<th data-priority='1'>Last Updated</th>");
                    groupList.Append($"<th data-priority='1'>Type</th>");
                    groupList.Append($"<th data-priority='1'>Account 1 Balance</th>");
                    groupList.Append($"<th data-priority='1'>Account 2 Balance</th>");
                    groupList.Append($"<th data-priority='1'>Unmatch</th>");
                    groupList.Append($"<th data-priority='1'>Instance</th>");
                    groupList.Append($"<th data-priority='2' data-orderable='false'>Options</th>");
                    groupList.Append($"</tr></thead><tbody></tbody></table>");
                    refModel.Object = groupList.ToString();
                }
                refModel.result = true;

            }
            return refModel;
        }
        public ReturnJsonModel SaveCBTask(List<BalanceanalysisModel> lstBalanceModel,
            task rectask, string InitialTransactionDate, string userId, int AccountId1 = 0, int AccountId2 = 0,
            int TaskTypeIdOld = 0, int DateVariance = 0, int amounVariance = 0, int transactionmatchingtaskrulesaccessId = 0,
            List<balanceanalysismappingrule> balancemapingruler = null, List<balanceanalysisaction> balanceAction = null,
            TaskPriorityEnum priority = TaskPriorityEnum.Low, string deadline = "", string[] role = null, int workgroupId = 0, string originatingConnectionId = "")
        {
            var refModel = new ReturnJsonModel();
            var autoDetect = dbContext.Configuration.AutoDetectChangesEnabled;
            var validateOnSave = dbContext.Configuration.ValidateOnSaveEnabled;
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            var notifiRules = new NotificationRules(dbContext);

            try
            {
                var user = dbContext.QbicleUser.Find(userId);

                var taskUpdate = dbContext.tasks.Find(rectask.Id);
                taskUpdate.WorkGroup = rectask.WorkGroup;
                dbContext.SaveChanges();
                dbContext.Configuration.AutoDetectChangesEnabled = false;
                dbContext.Configuration.ValidateOnSaveEnabled = false;
                dbContext.Configuration.ProxyCreationEnabled = false;

                DateTime? duedate = null;
                var initialTransactionDate = DateTime.ParseExact(InitialTransactionDate, "dd/MM/yyyy", new CultureInfo("en-US"), DateTimeStyles.None);
                if (deadline != "")
                    duedate = DateTime.ParseExact(deadline, "dd/MM/yyyy HH:mm", new CultureInfo("en-US"), DateTimeStyles.None);
                var urRule = new UserRules(dbContext);
                if (rectask.Id > 0)
                {
                    if (rectask.WorkGroup != null)
                    {
                        rectask.QbicleTask.Qbicle = rectask.WorkGroup.Qbicle;
                        rectask.QbicleTask.Topic = rectask.WorkGroup.Topic;
                    }
                    if (rectask.QbicleTask.Priority != priority)
                        rectask.QbicleTask.Priority = priority;
                   
                    rectask.QbicleTask.DueDate = duedate;

                    var first = dbContext.tasks.Find(rectask.Id);
                    first.WorkGroup = rectask.WorkGroup;
                    first.QbicleTask = rectask.QbicleTask;
                    first.CreatedById = rectask.QbicleTask.StartedBy.Id;
                    first.CreatedDate = rectask.QbicleTask.StartedDate;
                    if (rectask.Name != null)
                    {
                        first.Name = rectask.Name;
                        first.QbicleTask.Name = rectask.Name;
                    }
                    if (initialTransactionDate != null)
                        first.InitialTransactionDate =
                            rectask.InitialTransactionDate = initialTransactionDate;
                    if (rectask.TaskTypeId != 0)
                        first.TaskTypeId = rectask.TaskTypeId;
                    if (rectask.TransactionMatchingTypeId != 0)
                        first.TransactionMatchingTypeId = rectask.TransactionMatchingTypeId;
                    if (rectask.TaskExecutionIntervalId != 0)
                        first.TaskExecutionIntervalId = rectask.TaskExecutionIntervalId;
                    if (rectask.AssignedUserId != null)
                        first.AssignedUserId = rectask.AssignedUserId;
                    if (!string.IsNullOrEmpty(rectask.Description))
                    {
                        first.Description = rectask.Description;
                        first.QbicleTask.Description = rectask.Description;
                    }

                    first.QbicleTask.ActivityMembers.Clear();

                    var assignTo = urRule.GetUser(rectask.AssignedUserId, 0);
                    first.QbicleTask.ActivityMembers.Add(assignTo);
                    assignTo = urRule.GetUser(rectask.CreatedById, 0);
                    first.QbicleTask.ActivityMembers.Add(assignTo);
                    first.QbicleTask.Qbicle.LastUpdated = DateTime.UtcNow;
                    dbContext.Entry(first).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    if (TaskTypeIdOld == 1)
                    {
                        if (rectask.TaskTypeId > 0 && rectask.TaskTypeId != 1)
                        {
                            if (transactionmatchingtaskrulesaccessId > 0)
                            {
                                var transaction = new transactionmatchingtaskrulesacces
                                {
                                    IsAmountVarianceVisible = amounVariance,
                                    IsDateVarianceVisible = DateVariance,
                                    TaskId = rectask.Id,
                                    Id = transactionmatchingtaskrulesaccessId
                                };
                                var entry1 = dbContext.Entry(transaction);
                                if (dbContext.Entry(transaction).State == EntityState.Detached)
                                {
                                    dbContext.transactionmatchingtaskrulesaccess.Attach(transaction);
                                }
                                dbContext.Entry(transaction).State = EntityState.Deleted;
                            }
                        }
                        else
                        {
                            if (transactionmatchingtaskrulesaccessId > 0)
                            {
                                var transaction = new transactionmatchingtaskrulesacces
                                {
                                    IsAmountVarianceVisible = amounVariance,
                                    IsDateVarianceVisible = DateVariance,
                                    TaskId = rectask.Id,
                                    Id = transactionmatchingtaskrulesaccessId
                                };
                                var entry1 = dbContext.Entry(transaction);
                                if (dbContext.Entry(transaction).State == EntityState.Detached)
                                {
                                    dbContext.transactionmatchingtaskrulesaccess.Attach(transaction);
                                }
                                dbContext.Entry(transaction).State = EntityState.Modified;
                            }
                            else
                            {
                                var transaction = new transactionmatchingtaskrulesacces
                                {
                                    IsAmountVarianceVisible = amounVariance,
                                    IsDateVarianceVisible = DateVariance,
                                    TaskId = rectask.Id
                                };
                                dbContext.transactionmatchingtaskrulesaccess.Add(transaction);
                            }
                        }
                        dbContext.SaveChanges();
                    }

                    // save account
                    if (rectask.TaskTypeId != TaskTypeIdOld) // check Task of type is change from update
                    {
                        if (AccountId1 != 0 && AccountId2 != 0)
                        {
                            var account = dbContext.taskaccounts.Where(m => m.TaskId == rectask.Id);
                            if (account.Count() > 0)
                            {
                                dbContext.taskaccounts.RemoveRange(account);
                                dbContext.SaveChanges();
                            }
                        }
                        SaveAddAccount(rectask.Id, AccountId1, AccountId2);
                    }
                    else
                    {
                        SaveEditAccount(rectask.Id, AccountId1, AccountId2);
                    }
                    if (rectask.TaskTypeId == Enums.TypeOfTask.BalanceAnalysis)
                    {
                        var balance = dbContext.balanceanalysismappingrules.Where(p => p.TaskId == rectask.Id).ToList();
                        if (balance != null && balance.Any())
                        {
                            dbContext.balanceanalysismappingrules.RemoveRange(balance);
                            dbContext.SaveChanges();
                        }
                        var lstBalancemapingruler = new List<balanceanalysismappingrule>();
                        balanceanalysismappingrule balancemaping;

                        if (lstBalanceModel != null)
                        {
                            foreach (var item in lstBalanceModel)
                            {
                                balancemaping = new balanceanalysismappingrule
                                {
                                    Description1 = item.Description1,
                                    Reference1 = item.Reference1,
                                    Description2 = item.Description2,
                                    Reference2 = item.Reference2,
                                    MaxDifference = item.Amount2,
                                    MinDifference = item.Amount1,
                                    TaskId = rectask.Id
                                };
                                lstBalancemapingruler.Add(balancemaping);
                            }
                        }
                        if (balancemapingruler != null)
                        {
                            foreach (var item in balancemapingruler)
                            {
                                item.TaskId = rectask.Id;
                                lstBalancemapingruler.Add(item);
                            }
                        }
                        if (lstBalancemapingruler.Any())
                        {
                            dbContext.balanceanalysismappingrules.AddRange(lstBalancemapingruler);
                            dbContext.SaveChanges();
                        }
                    }
                    refModel.msg = "Successfully Edited";
                    var balanceaction = dbContext.balanceanalysisactions.Where(p => p.TaskId == rectask.Id);
                    dbContext.balanceanalysisactions.RemoveRange(balanceaction);
                    dbContext.SaveChanges();
                    if (balanceAction != null)
                    {
                        var lstBalanceAction = new List<balanceanalysisaction>();
                        foreach (var b in balanceAction)
                        {
                            b.TaskId = rectask.Id;
                            lstBalanceAction.Add(b);
                        }
                        if (lstBalanceAction.Any())
                        {
                            if (!lstBalanceAction.Any(p => p.Name.ToLower().Contains("No Action Required".ToLower())))
                                lstBalanceAction.Add(new balanceanalysisaction { Name = "No Action Required", TaskId = rectask.Id });
                            dbContext.balanceanalysisactions.AddRange(lstBalanceAction);
                            dbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        dbContext.balanceanalysisactions.Add(new balanceanalysisaction { Name = "No Action Required", TaskId = rectask.Id });
                        dbContext.SaveChanges();
                    }



                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        DomainId = rectask.QbicleTask.Qbicle.Domain.Id,
                        Id = first.QbicleTask.Id,
                        EventNotify = NotificationEventEnum.TaskUpdate,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userId,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0
                    };
                    notifiRules.Notification2Activity(activityNotification);

                }
                else
                {
                    rectask.CreatedDate = DateTime.UtcNow;
                    rectask.CreatedById = user.Id;
                    rectask.WorkGroup = dbContext.CBWorkGroups.Find(workgroupId);
                    if (rectask.TransactionMatchingTypeId == 0)
                        rectask.TransactionMatchingTypeId = 1;
                    if (!string.IsNullOrEmpty(InitialTransactionDate))
                        rectask.InitialTransactionDate = initialTransactionDate;

                    var assignTo = urRule.GetUser(rectask.AssignedUserId, 0);
                    var qb_task = new QbicleTask
                    {
                        Name = rectask.Name,
                        Description = rectask.Description,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Qbicle = rectask.WorkGroup.Qbicle,
                        task = rectask,
                        Topic = rectask.WorkGroup.Topic,
                        Priority = priority,
                        DueDate = duedate,
                        App=QbicleActivity.ActivityApp.CleanBooks
                    };
                    qb_task.ActivityMembers.Add(assignTo);

                    assignTo = urRule.GetUser(rectask.CreatedById, 0);
                    qb_task.ActivityMembers.Add(assignTo);

                    rectask.QbicleTask = qb_task;

                    dbContext.tasks.Add(rectask);
                    dbContext.Entry(rectask).State = EntityState.Added;


                    dbContext.SaveChanges();
                    var ruler = new transactionmatchingrule
                    {
                        Amount1VarianceValue = 0,
                        Amount2VarianceValue = 0,
                        DateVarianceValue = 0,
                        IsAmountAndDate_Set = 1,
                        IsDescription_Set = 1,
                        IsDescriptionAndDate_Set = 1,
                        IsManyToMany_Set = 1,
                        IsOneToOne_Set = null,
                        IsRef_RefToDesc_Set = 1,
                        IsRef_RefToRef1_Set = 1,
                        IsRefAndDate_RefToDesc_Set = 1,
                        IsRefAndDate_RefToRef1_Set = 1,
                        IsReference_Set = 1,
                        IsReferenceAndDate_Set = 1,
                        IsReversals_Set = 1,
                        TaskId = rectask.Id,
                        VarianceName = rectask.Name
                    };
                    dbContext.transactionmatchingrules.Add(ruler);

                    dbContext.SaveChanges();
                    if (rectask.TaskTypeId == 1)
                    {
                        var transaction = new transactionmatchingtaskrulesacces
                        {
                            IsAmountVarianceVisible = amounVariance,
                            IsDateVarianceVisible = DateVariance,
                            TaskId = rectask.Id
                        };
                        dbContext.transactionmatchingtaskrulesaccess.Add(transaction);
                        dbContext.SaveChanges();
                    }


                    int id = rectask.Id;
                    if (id > 0)
                    {
                        if (rectask.TaskTypeId == Enums.TypeOfTask.BalanceAnalysis)
                        {
                            var lstBalancemapingruler = new List<balanceanalysismappingrule>();
                            balanceanalysismappingrule balancemaping;
                            if (lstBalanceModel != null)
                            {
                                foreach (var item in lstBalanceModel)
                                {
                                    balancemaping = new balanceanalysismappingrule
                                    {
                                        Description1 = item.Description1,
                                        Reference1 = item.Reference1,
                                        Description2 = item.Description2,
                                        Reference2 = item.Reference2,
                                        MaxDifference = item.Amount2,
                                        MinDifference = item.Amount1,
                                        TaskId = rectask.Id
                                    };
                                    lstBalancemapingruler.Add(balancemaping);
                                }
                            }
                            if (balancemapingruler != null)
                            {
                                foreach (var item in balancemapingruler)
                                {
                                    item.TaskId = rectask.Id;
                                    lstBalancemapingruler.Add(item);
                                }
                            }
                            if (lstBalancemapingruler.Any())
                            {
                                dbContext.balanceanalysismappingrules.AddRange(lstBalancemapingruler);
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            SaveAddAccount(id, AccountId1, AccountId2);
                        }

                        refModel.msg = "Successfully Added";
                    }
                    if (balanceAction != null)
                    {
                        var lstBalanceAction = new List<balanceanalysisaction>();
                        foreach (var it in balanceAction)
                        {
                            it.TaskId = rectask.Id;
                            lstBalanceAction.Add(it);
                        }
                        if (lstBalanceAction.Any())
                        {
                            if (!lstBalanceAction.Any(p => p.Name.ToLower().Contains("No Action Required".ToLower())))
                                lstBalanceAction.Add(new balanceanalysisaction { Name = "No Action Required", TaskId = rectask.Id });
                            dbContext.balanceanalysisactions.AddRange(lstBalanceAction);
                            dbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        dbContext.balanceanalysisactions.Add(new balanceanalysisaction { Name = "No Action Required", TaskId = rectask.Id });
                        dbContext.SaveChanges();
                    }

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = qb_task.Id,
                        EventNotify = Notification.NotificationEventEnum.TaskCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userId,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0
                    };
                    notifiRules.Notification2Activity(activityNotification);

                }
                if (role != null)
                {
                    string sqlQuery = $"DELETE FROM domainroletasks WHERE task_Id = {rectask.Id};";
                    foreach (var item in role)
                    {
                        var domainRole = dbContext.DomainRole.Find(int.Parse(item));
                        sqlQuery += $"insert INTO domainroletasks value({domainRole.Id},'{rectask.Id}');";
                        //rectask.DomainRoles.Add(domainRole);
                    }
                    dbContext.Database.ExecuteSqlCommand(sqlQuery);
                }
                
                dbContext.SaveChanges();
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return refModel;
            }
            finally
            {
                dbContext.Configuration.AutoDetectChangesEnabled = autoDetect;
                dbContext.Configuration.ValidateOnSaveEnabled = validateOnSave;
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }

        }
        private void SaveAddAccount(int id, int accountId1, int accountId2)
        {
            if (accountId1 != 0 && accountId2 == 0)
            {
                var account = new taskaccount()
                {
                    TaskId = id,
                    Order = 1,
                    AccountId = accountId1
                };
                dbContext.taskaccounts.Add(account);
                dbContext.SaveChanges();

            }
            else if (accountId1 != 0 && accountId2 != 0)
            {

                for (var i = 1; i <= 2; i++)
                {
                    var account = new taskaccount()
                    {
                        TaskId = id
                    };
                    if (i == 1)
                    {
                        account.Order = i;
                        account.AccountId = accountId1;
                    }
                    else
                    {
                        account.Order = i;
                        account.AccountId = accountId2;
                    }
                    dbContext.taskaccounts.Add(account);
                    dbContext.SaveChanges();
                }
            }
        }
        private void SaveEditAccount(int id, int accountId1, int accountId2)
        {
            if (accountId1 != 0 || accountId2 != 0) // case taskid not in table taskinstance
            {
                var account = dbContext.taskaccounts.Where(m => m.TaskId == id);
                if (account.Any())
                {

                    foreach (var b in account)
                    {
                        var entry1 = dbContext.Entry(b);
                        if (dbContext.Entry(b).State == EntityState.Detached)
                        {
                            dbContext.taskaccounts.Attach(b);
                        }
                        dbContext.Entry(b).State = EntityState.Modified;
                        if (b.Order == 1)
                        {
                            b.AccountId = accountId1;
                        }
                        else if (b.Order == 2)
                        {
                            b.AccountId = accountId2;
                        }
                        entry1.Property(m => m.AccountId).IsModified = true;
                        entry1.Property(m => m.Order).IsModified = false;
                        entry1.Property(m => m.TaskId).IsModified = false;
                    }
                    dbContext.SaveChanges();
                }
                else
                {
                    SaveAddAccount(id, accountId1, accountId2);
                }
            }
        }

        public bool DeleteTask(task task, string userId)
        {

            //create deletetasks
            deletedtask delTask = new deletedtask()
            {
                TaskName = task.Name,
                TaskDescription = task.Description,
                DeletedById = userId,
                DeletedDate = DateTime.UtcNow
            };
            dbContext.deletedtasks.Add(delTask);
            dbContext.Entry(delTask).State = EntityState.Added;
            //delete ruler
            var ruler = dbContext.transactionmatchingtaskrulesaccess.Where(p => p.TaskId == task.Id);
            dbContext.transactionmatchingtaskrulesaccess.RemoveRange(ruler);
            var matchingruler = dbContext.transactionmatchingrules.Where(p => p.TaskId == task.Id);
            dbContext.transactionmatchingrules.RemoveRange(matchingruler);
            //delete tasckaccount
            var taskacc = dbContext.taskaccounts.Where(ta => ta.TaskId == task.Id);
            dbContext.taskaccounts.RemoveRange(taskacc);

            var balancemapingRuler = dbContext.balanceanalysismappingrules.Where(p => p.TaskId == task.Id);
            dbContext.balanceanalysismappingrules.RemoveRange(balancemapingRuler);

            var balanceAction = dbContext.balanceanalysisactions.Where(p => p.TaskId == task.Id).ToList();
            dbContext.balanceanalysisactions.RemoveRange(balanceAction);
            //remove task
            var taskRemove = new task { Id = task.Id };
            var taskDb = dbContext.QbicleTasks.Find(task.Id);
            taskRemove.QbicleTask = taskDb;
            dbContext.tasks.Attach(taskRemove);
            dbContext.tasks.Remove(taskRemove);

            var notificationDel = dbContext.Notifications.Where(a => a.AssociatedAcitvity.Id == taskDb.Id).ToList();
            foreach (var item in notificationDel)
            {
                dbContext.Notifications.Attach(item);
                dbContext.Notifications.Remove(item);
            }


            taskDb.ActivityMembers.Clear();
            dbContext.QbicleTasks.Attach(taskDb);
            dbContext.QbicleTasks.Remove(taskDb);

            dbContext.SaveChanges();
            return true;
        }


        public List<taskgroup> LoadTaskGroups(int groupId, SortOrderBy orderBy, int domainId)
        {
            List<taskgroup> taskGroup;
            if (groupId == 0)
                taskGroup = dbContext.taskgroups.Where(d => d.qbicledomain.Id == domainId).ToList();
            else
                taskGroup = dbContext.taskgroups.Where(e => e.Id == groupId).ToList();
            var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId)
                .Where(q=>q.Processes.Any(p=>p.Name == CBProcessName.TaskProcessName || p.Name == CBProcessName.TaskExecutionProcessName)).ToList();
            taskGroup.ForEach(e => e.tasks = e.tasks.Where(q=> (q.WorkGroup != null && wgs.Any(w=>w.Id == q.WorkGroup.Id)) || q.WorkGroup == null).ToList());
            switch (orderBy)
            {
                case SortOrderBy.NameAZ:
                    taskGroup.ForEach(e => e.tasks = e.tasks.OrderBy(r => r.Name).ToList());
                    break;
                case SortOrderBy.NameZA:
                    taskGroup.ForEach(e => e.tasks = e.tasks.OrderByDescending(r => r.Name).ToList());
                    break;
                case SortOrderBy.UnmatchedMost:
                    taskGroup.ForEach(e => e.tasks = e.tasks.OrderByDescending(r => r.transactionmatchingunmatcheds.Count).ToList());
                    break;
                case SortOrderBy.UnmatchedLeast:
                    taskGroup.ForEach(e => e.tasks = e.tasks.OrderBy(r => r.transactionmatchingunmatcheds.Count).ToList());
                    break;
                case SortOrderBy.InstancesMost:
                    taskGroup.ForEach(e => e.tasks = e.tasks.OrderByDescending(r => r.taskinstances.Count).ToList());
                    break;
                case SortOrderBy.InstancesLeast:
                    taskGroup.ForEach(e => e.tasks = e.tasks.OrderBy(r => r.taskinstances.Count).ToList());
                    break;
            }
            return taskGroup;
        }
    }
}
