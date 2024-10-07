using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Form;
using Qbicles.Models.Operator.Compliance;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorTaskRules
    {
        ApplicationDbContext dbContext;
        public OperatorTaskRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ComplianceTask GetComplianceTaskById(int id)
        {
            try
            {
                return dbContext.OperatorComplianceTasks.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new ComplianceTask();
            }
        }
        public TaskInstance GetTaskInstance(ComplianceTask complianceTask, int? taskid)
        {
            try
            {
                var taskinstance = taskid.HasValue ? complianceTask.TaskInstances.FirstOrDefault(s => s.AssociatedQbicleTask.Id == taskid.Value) : complianceTask.TaskInstances.OrderByDescending(s => s.Id).FirstOrDefault();
                if (taskinstance == null)
                {
                    TaskInstance instance = new TaskInstance();
                    instance.AssociatedQbicleTask = dbContext.QbicleTasks.Find(taskid.Value);
                    instance.CreatedBy = complianceTask.CreatedBy;
                    instance.CreatedDate = DateTime.UtcNow;
                    instance.ParentComplianceTask = complianceTask;
                    dbContext.OperatorTaskInstances.Add(instance);
                    dbContext.SaveChanges();
                }
                return taskinstance;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taskid);
                return null;
            }
        }
        public ReturnJsonModel ResetTask(int complianceTaskId,int taskId,string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var dbComplianceTask = dbContext.OperatorComplianceTasks.Find(complianceTaskId);
                if (dbComplianceTask!=null&&dbComplianceTask.Type==TaskType.Repeatable)
                {
                    var task = dbContext.QbicleTasks.Find(taskId);
                    task.isComplete = false;
                    task.ActualEnd = null;
                    TaskInstance instance = new TaskInstance
                    {
                        AssociatedQbicleTask = task,
                        ParentComplianceTask = dbComplianceTask,
                        CreatedBy = dbContext.QbicleUser.Find(userId),
                        CreatedDate = DateTime.UtcNow
                    };
                    dbContext.OperatorTaskInstances.Add(instance);
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }else
                {
                    returnJson.msg = "ERROR_MSG_712";
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, complianceTaskId);
            }
            return returnJson;
        }
        public ReturnJsonModel DeleteByTaskId(int id)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var task= dbContext.OperatorComplianceTasks.Find(id);
                if(task!=null)
                {
                    if(task.TaskInstances.Any(s=>s.FormInstances.Any()))
                    {
                        returnJson.msg = "ERROR_MSG_711";
                        return returnJson;
                    }
                    foreach (var item in task.Tasks.ToList())
                    {
                        item.ComplianceTask = null;
                        if (dbContext.Entry(item).State == EntityState.Detached)
                            dbContext.QbicleTasks.Attach(item);
                        dbContext.Entry(item).State = EntityState.Modified;
                    }
                    dbContext.OperatorComplianceTasks.Remove(task);
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnJson;
        }
        public ReturnJsonModel SaveTaskOperator(OperatorTaskModel formModel, UserSetting userSetting, string originatingConnectionId = "")
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!string.IsNullOrEmpty(formModel.MediaResponse.UrlGuid))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);
                        s3Rules.ProcessingMediaS3(formModel.MediaResponse.UrlGuid);

                    }


                    var currentUser = dbContext.QbicleUser.Find(userSetting.Id);

                    var wg = dbContext.OperatorWorkGroups.Find(formModel.WorkgroupId);
                    if (wg != null)
                    {
                        var recurStart = formModel.RecurStart.ConvertDateFormat(userSetting.DateTimeFormat).ConvertTimeToUtc(userSetting.Timezone);
                        var recurEnd = formModel.RecurEnd.ConvertDateFormat(userSetting.DateTimeFormat).ConvertTimeToUtc(userSetting.Timezone);
                        var complianceTask = dbContext.OperatorComplianceTasks.Find(formModel.Id);
                        if(complianceTask!=null)
                        {
                            complianceTask.Name = formModel.TaskName;
                            complianceTask.Description = formModel.TaskDescription;
                            complianceTask.Type = formModel.TaskType;
                            if (formModel.TaskType == TaskType.Fixed)
                                complianceTask.ExpectedEnd = recurEnd;
                            else
                                complianceTask.ExpectedEnd = null;
                            //complianceTask.la
                            var currentTaskInstance = complianceTask.TaskInstances.OrderByDescending(s => s.Id).FirstOrDefault();
                            var task= currentTaskInstance.AssociatedQbicleTask;
                            task.Name = formModel.TaskName;
                            task.Description= formModel.TaskDescription;
                            var dbAssigee= task.AssociatedSet.Peoples.FirstOrDefault(s=>s.Type==QbiclePeople.PeopleTypeEnum.Assignee);
                            if(dbAssigee!=null)
                            {
                                var _user = dbContext.QbicleUser.Find(formModel.Assignee);
                                var _currentUser = dbAssigee.User;
                                if (_user != null)
                                {
                                    dbAssigee.User = _user;
                                    complianceTask.Assignee = _user;
                                    task.ActivityMembers.Remove(_currentUser);
                                    if (!task.ActivityMembers.Any(s => s.Id == _user.Id))
                                        task.ActivityMembers.Add(_user);
                                }
                            }
                            if (formModel.isRecurs)
                            {
                                QbicleRecurrance qbicleRecur = new QbicleRecurrance();
                                qbicleRecur.AssociatedSet = task.AssociatedSet;
                                qbicleRecur.Days = (formModel.RecurrenceType == 0 || formModel.RecurrenceType == 1 ? formModel.DayOrMonth : "");
                                qbicleRecur.Months = (formModel.RecurrenceType == 2 ? formModel.DayOrMonth : "");
                                qbicleRecur.FirstOccurrence = recurStart;
                                qbicleRecur.LastOccurrence = recurEnd;
                                qbicleRecur.MonthDate = formModel.Monthdates.HasValue ? formModel.Monthdates.Value : (short)0;
                                dbContext.Recurrances.Add(qbicleRecur);
                                dbContext.Entry(qbicleRecur).State = EntityState.Added;
                                foreach (var item in formModel.ListDates)
                                {
                                    var _date = item.ConvertDateFormat(userSetting.DateTimeFormat).ConvertTimeToUtc(userSetting.Timezone);
                                    if (!complianceTask.Tasks.Any(s=>s.ProgrammedStart == _date))
                                    {
                                        var task2 = new QbicleTask
                                        {
                                            ActualEnd = task.ActualEnd,
                                            ActualStart = task.ActualStart,
                                            ClosedBy = task.ClosedBy,
                                            ClosedDate = task.ClosedDate,
                                            Description = task.Description,
                                            Duration = task.Duration,
                                            DurationUnit = task.DurationUnit,
                                            isComplete = task.isComplete,
                                            isRecurs = task.isRecurs,
                                            Name = task.Name,
                                            StartedBy = task.StartedBy,
                                            StartedDate = _date,
                                            Topic = wg.DefaultTopic,
                                            isSteps = task.isSteps,
                                            Priority = task.Priority,
                                            TimeLineDate = task.TimeLineDate,
                                            IsVisibleInQbicleDashboard = false
                                        };

                                        task2.Qbicle = wg.SourceQbicle;
                                        task2.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                                        task2.ProgrammedStart = _date;
                                        if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Days)
                                            task2.ProgrammedEnd = task2.ProgrammedStart.Value.AddDays(task.Duration);
                                        else if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Hours)
                                            task2.ProgrammedEnd = task2.ProgrammedStart.Value.AddHours(task.Duration);
                                        else
                                            task2.ProgrammedEnd = task2.ProgrammedStart.Value.AddDays(task.Duration * 7);

                                        if (!string.IsNullOrEmpty(formModel.MediaResponse.Name))
                                        {
                                            //Media attach
                                           var m = new QbicleMedia
                                            {
                                                StartedBy = currentUser,
                                                StartedDate = DateTime.UtcNow,
                                                Name = task.Name,
                                                FileType = formModel.MediaResponse.Type,
                                                Qbicle = wg.SourceQbicle,
                                                TimeLineDate = DateTime.UtcNow,
                                                Topic = task.Topic,

                                                MediaFolder =
                                                    new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, wg.SourceQbicle.Id),
                                                Description = task.Description,
                                                IsVisibleInQbicleDashboard = false
                                            };
                                            var versionFile = new VersionedFile
                                            {
                                                IsDeleted = false,
                                                FileSize = formModel.MediaResponse.Size,
                                                UploadedBy = currentUser,
                                                UploadedDate = DateTime.UtcNow,
                                                Uri = formModel.MediaResponse.UrlGuid,
                                                FileType = formModel.MediaResponse.Type
                                            };
                                            m.VersionedFiles.Add(versionFile);
                                            m.ActivityMembers.Add(currentUser);

                                            dbContext.Medias.Add(m);
                                            dbContext.Entry(m).State = EntityState.Added;

                                            task2.SubActivities.Add(m);
                                        }

                                        task2.AssociatedSet = task.AssociatedSet;
                                        task2.ActivityMembers.AddRange(task.ActivityMembers);

                                        dbContext.QbicleTasks.Add(task2);
                                        dbContext.Entry(task2).State = EntityState.Added;
                                        complianceTask.Tasks.Add(task2);
                                    }
                                }
                            }

                            if (dbContext.Entry(complianceTask).State == EntityState.Detached)
                                dbContext.OperatorComplianceTasks.Attach(complianceTask);
                            dbContext.Entry(complianceTask).State = EntityState.Modified;
                            returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                        }
                        else
                        {
                            complianceTask = new ComplianceTask();
                            complianceTask.WorkGroup = wg;
                            complianceTask.Domain = formModel.CurrentDomain;
                            complianceTask.Name = formModel.TaskName;
                            complianceTask.Description = formModel.TaskDescription;
                            complianceTask.Type = formModel.TaskType;
                            complianceTask.CreatedDate = DateTime.UtcNow;
                            complianceTask.CreatedBy = currentUser;
                            if(formModel.Forms!=null)
                            {
                                for (int i = 0; i < formModel.Forms.Count(); i++)
                                {
                                    var fd = dbContext.FormDefinition.Find(formModel.Forms[i]);
                                    if (fd != null)
                                    {
                                        OrderedForm ordered = new OrderedForm();
                                        ordered.FormDefinition = fd;
                                        ordered.DisplayOrder = i;
                                        ordered.ParentComplianceTask = complianceTask;
                                        dbContext.OperatorOrderedForms.Add(ordered);
                                        dbContext.Entry(ordered).State = EntityState.Added;
                                    }
                                }
                            }
                            #region Create Task
                            if (formModel.TaskType == TaskType.Fixed)
                                complianceTask.ExpectedEnd = recurEnd;
                            else
                                complianceTask.ExpectedEnd = null;
                            QbicleTask task = new QbicleTask();
                            task.Name = formModel.TaskName;
                            task.Description = formModel.TaskDescription;
                            task.Topic = wg.DefaultTopic;
                            task.StartedBy = currentUser;
                            task.StartedDate = DateTime.UtcNow;
                            task.State = QbicleActivity.ActivityStateEnum.Open;
                            task.App = QbicleActivity.ActivityApp.Operator;
                            task.Qbicle = wg.SourceQbicle;
                            task.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                            task.ProgrammedStart = recurStart;
                            task.TimeLineDate = task.StartedDate;
                            if (task.DurationUnit == QbicleTask.TaskDurationUnitEnum.Days)
                                task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration);
                            else if (task.DurationUnit == QbicleTask.TaskDurationUnitEnum.Hours)
                                task.ProgrammedEnd = task.ProgrammedStart.Value.AddHours(task.Duration);
                            else
                                task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration * 7);
                            QbicleMedia m = null;
                            if (formModel.MediaResponse !=null&&!string.IsNullOrEmpty(formModel.MediaResponse.Name))
                            {
                                //Media attach
                                m = new QbicleMedia
                                {
                                    StartedBy = currentUser,
                                    StartedDate = DateTime.UtcNow,
                                    Name = task.Name,
                                    FileType = formModel.MediaResponse.Type,
                                    Qbicle = wg.SourceQbicle,
                                    TimeLineDate = DateTime.UtcNow,
                                    Topic = task.Topic,

                                    MediaFolder =
                                        new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, wg.SourceQbicle.Id),
                                    Description = task.Description,
                                    IsVisibleInQbicleDashboard = false
                                };
                                var versionFile = new VersionedFile
                                {
                                    IsDeleted = false,
                                    FileSize = formModel.MediaResponse.Size,
                                    UploadedBy = currentUser,
                                    UploadedDate = DateTime.UtcNow,
                                    Uri = formModel.MediaResponse.UrlGuid,
                                    FileType = formModel.MediaResponse.Type
                                };
                                m.VersionedFiles.Add(versionFile);
                                m.ActivityMembers.Add(currentUser);

                                dbContext.Medias.Add(m);
                                dbContext.Entry(m).State = EntityState.Added;

                                task.SubActivities.Add(m);
                            }
                            QbicleSet qbicleSet = new QbicleSet();
                            dbContext.Entry(qbicleSet).State = EntityState.Added;
                            dbContext.Sets.Add(qbicleSet);
                            task.AssociatedSet = qbicleSet;
                            //Update LastUpdated currentDomain
                            wg.SourceQbicle.LastUpdated = DateTime.UtcNow;
                            if (dbContext.Entry(wg.SourceQbicle).State == EntityState.Detached)
                                dbContext.Qbicles.Attach(wg.SourceQbicle);
                            dbContext.Entry(wg.SourceQbicle).State = EntityState.Modified;
                            //end
                            var _activityMembers = new List<ApplicationUser>();
                            _activityMembers.Add(currentUser);
                            if (!string.IsNullOrEmpty(formModel.Assignee))
                            {
                                QbiclePeople peopleAssignee = new QbiclePeople();
                                peopleAssignee.isPresent = true;
                                peopleAssignee.Type = QbiclePeople.PeopleTypeEnum.Assignee;
                                peopleAssignee.User = dbContext.QbicleUser.Find(formModel.Assignee);
                                peopleAssignee.AssociatedSet = qbicleSet;
                                dbContext.People.Add(peopleAssignee);
                                dbContext.Entry(peopleAssignee).State = EntityState.Added;
                                if(!_activityMembers.Any(s=>s.Id== peopleAssignee.User.Id))
                                    _activityMembers.Add(peopleAssignee.User);
                                complianceTask.Assignee = peopleAssignee.User;
                            }
                            #endregion
                            if (formModel.isRecurs)
                            {
                                QbicleRecurrance qbicleRecur = new QbicleRecurrance();
                                qbicleRecur.AssociatedSet = qbicleSet;
                                qbicleRecur.Days = (formModel.RecurrenceType == 0 || formModel.RecurrenceType == 1 ? formModel.DayOrMonth : "");
                                qbicleRecur.Months = (formModel.RecurrenceType == 2 ? formModel.DayOrMonth : "");
                                qbicleRecur.FirstOccurrence = recurStart;
                                qbicleRecur.LastOccurrence= recurEnd;
                                qbicleRecur.MonthDate = formModel.Monthdates.HasValue ? formModel.Monthdates.Value : (short)0;
                                dbContext.Recurrances.Add(qbicleRecur);
                                dbContext.Entry(qbicleRecur).State = EntityState.Added;
                                foreach (var item in formModel.ListDates)
                                {
                                    var _date= item.ConvertDateFormat(userSetting.DateTimeFormat).ConvertTimeToUtc(userSetting.Timezone);
                                    if (task.ProgrammedStart != _date)
                                    {
                                        var task2 = new QbicleTask
                                        {
                                            ActualEnd = task.ActualEnd,
                                            ActualStart = task.ActualStart,
                                            ClosedBy = task.ClosedBy,
                                            ClosedDate = task.ClosedDate,
                                            Description = task.Description,
                                            Duration = task.Duration,
                                            DurationUnit = task.DurationUnit,
                                            isComplete = task.isComplete,
                                            isRecurs = task.isRecurs,
                                            Name = task.Name,
                                            StartedBy = task.StartedBy,
                                            StartedDate = _date,
                                            Topic = wg.DefaultTopic,
                                            isSteps = task.isSteps,
                                            Priority = task.Priority,
                                            TimeLineDate = task.TimeLineDate,
                                            IsVisibleInQbicleDashboard = false
                                        };

                                        task2.Qbicle = wg.SourceQbicle;
                                        task2.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                                        task2.ProgrammedStart = _date;
                                        if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Days)
                                            task2.ProgrammedEnd = task2.ProgrammedStart.Value.AddDays(task.Duration);
                                        else if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Hours)
                                            task2.ProgrammedEnd = task2.ProgrammedStart.Value.AddHours(task.Duration);
                                        else
                                            task2.ProgrammedEnd = task2.ProgrammedStart.Value.AddDays(task.Duration * 7);

                                        if (!string.IsNullOrEmpty(formModel.MediaResponse.Name))
                                        {
                                            //Media attach
                                            m = new QbicleMedia
                                            {
                                                StartedBy = currentUser,
                                                StartedDate = DateTime.UtcNow,
                                                Name = task.Name,
                                                FileType = formModel.MediaResponse.Type,
                                                Qbicle = wg.SourceQbicle,
                                                TimeLineDate = DateTime.UtcNow,
                                                Topic = task.Topic,

                                                MediaFolder =
                                                    new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, wg.SourceQbicle.Id),
                                                Description = task.Description,
                                                IsVisibleInQbicleDashboard = false
                                            };
                                            var versionFile = new VersionedFile
                                            {
                                                IsDeleted = false,
                                                FileSize = formModel.MediaResponse.Size,
                                                UploadedBy = currentUser,
                                                UploadedDate = DateTime.UtcNow,
                                                Uri = formModel.MediaResponse.UrlGuid,
                                                FileType = formModel.MediaResponse.Type
                                            };
                                            m.VersionedFiles.Add(versionFile);
                                            m.ActivityMembers.Add(currentUser);

                                            dbContext.Medias.Add(m);
                                            dbContext.Entry(m).State = EntityState.Added;

                                            task2.SubActivities.Add(m);
                                        }

                                        task2.AssociatedSet = qbicleSet;
                                        if (_activityMembers.Any())
                                            task2.ActivityMembers.AddRange(_activityMembers);
                                        dbContext.QbicleTasks.Add(task2);
                                        dbContext.Entry(task2).State = EntityState.Added;
                                        #region Task Instance
                                        TaskInstance taskInstanceRecurr = new TaskInstance();
                                        taskInstanceRecurr.AssociatedQbicleTask = task2;
                                        taskInstanceRecurr.ParentComplianceTask = complianceTask;
                                        taskInstanceRecurr.CreatedBy = currentUser;
                                        taskInstanceRecurr.CreatedDate = DateTime.UtcNow;
                                        dbContext.OperatorTaskInstances.Add(taskInstanceRecurr);
                                        dbContext.Entry(taskInstanceRecurr).State = EntityState.Added;
                                        #endregion
                                        complianceTask.Tasks.Add(task2);
                                    }
                                }
                            }
                            dbContext.QbicleTasks.Add(task);
                            dbContext.Entry(task).State = EntityState.Added;
                            complianceTask.Tasks.Add(task);
                            #region Task Instance
                            TaskInstance taskInstance = new TaskInstance();
                            taskInstance.AssociatedQbicleTask = task;
                            taskInstance.ParentComplianceTask = complianceTask;
                            taskInstance.CreatedBy = currentUser;
                            taskInstance.CreatedDate = DateTime.UtcNow;
                            dbContext.OperatorTaskInstances.Add(taskInstance);
                            dbContext.Entry(taskInstance).State = EntityState.Added;
                            #endregion
                            dbContext.OperatorComplianceTasks.Add(complianceTask);
                            dbContext.Entry(complianceTask).State = EntityState.Added;

                            returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                            //Notify

                            var activityNotification = new ActivityNotification
                            {
                                OriginatingConnectionId = originatingConnectionId,
                                DomainId = task.Qbicle.Domain.Id,
                                Id = task.Id,
                                EventNotify = NotificationEventEnum.TaskCreation,
                                AppendToPageName = ApplicationPageName.Link,
                                AppendToPageId = 0,
                                CreatedById = userSetting.Id,
                                CreatedByName = userSetting.DisplayName,
                                ReminderMinutes = 0
                            };
                            new NotificationRules(dbContext).Notification2Activity(activityNotification);
                        }
                        
                        transaction.Commit();

                        
                    }
                    
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, formModel);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }
        public DataTablesResponse SearchTasks([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, OperatorSearchTaskModel filter)
        {
            try
            {
                var query = dbContext.OperatorComplianceTasks.Where(t => t.Domain.Id == filter.domainId).AsQueryable();
                int totalcount = 0;
                #region Filter
                if (!string.IsNullOrEmpty(filter.keyword))
                    query = query.Where(q => q.Name.Contains(filter.keyword) || q.Description.Contains(filter.keyword));
                if (!string.IsNullOrEmpty(filter.assignee)&& filter.assignee!="0")
                {
                    query = query.Where(q => q.Assignee.Id==filter.assignee);
                }
                if(filter.form>0)
                {
                    query = query.Where(q => q.OrderedForms.Any(s=>s.FormDefinition.Id==filter.form));
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
                        case "Description":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Description" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Assignee":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Assignee.Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",Assignee.Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Type":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Type" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Due":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "ExpectedEnd" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Name,
                    Assignee = HelperClass.GetFullNameOfUser(q.Assignee),
                    AssigneeId= q.Assignee.Id,
                    Forms = getHtmlForms(q.OrderedForms.Select(s=>s.FormDefinition).ToList()),
                    Type = q.Type.ToString(),
                    Due=q.ExpectedEnd.HasValue? q.ExpectedEnd.Value.ToString(filter.dateformat):""
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        private string getHtmlForms(List<FormDefinition> forms)
        {
            if (forms == null)
                return "";
            string html = "<ul>";
            foreach (var item in forms)
            {
                html += $"<li>{item.Title}</li>";
            }
            html += "</ul>";
            return html;
        }
        public DataTablesResponse SearchTaskInstances([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, OperatorSearchTaskModel filter)
        {
            try
            {
                var query = from f in dbContext.FormInstances
                            join ti in dbContext.OperatorTaskInstances on f.ComplianceTaskInstance.Id equals ti.Id
                            where ti.ParentComplianceTask.Id == filter.complianceTaskId && ti.AssociatedQbicleTask.Id == filter.taskId
                            select f;
                    dbContext.OperatorTaskInstances.Where(t => t.ParentComplianceTask.Id == filter.complianceTaskId).AsQueryable();
                int totalcount = 0;
                #region Filter
                if (!string.IsNullOrEmpty(filter.keyword))
                    query = query.Where(q => q.ParentDefinition.Title.Contains(filter.keyword) || q.ParentDefinition.Title.Contains(filter.keyword));
                if (filter.form > 0)
                {
                    query = query.Where(q => q.ParentDefinition.Id==filter.form);
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
                        case "Form":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "ParentDefinition.Title" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Submitted":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Form=q.ParentDefinition.Title,
                    Submitted = q.CreatedDate.ConvertTimeFromUtc(filter.timezone).ToString(filter.dateformat+" HH:mm"),
                    Score = (q.ElementData.Sum(s=>s.Score)/ (q.ParentDefinition.Elements.Where(s=>s.AllowScore).Count()*5))*100,
                    TaskId= q.ComplianceTaskInstance.AssociatedQbicleTask.Id,
                    ComplianceTaskId = q.ComplianceTaskInstance.ParentComplianceTask.Id,
                    FormInsId=q.Id
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public FormInstance GetFormInstanceById(int fId)
        {
            try
            {
                return dbContext.FormInstances.Find(fId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fId);
                return new FormInstance();
            }
        }
        public ComplianceTask GetComplianceTaskByActivityId(int activityId)
        {
            try
            {
                return dbContext.OperatorComplianceTasks.FirstOrDefault(s => s.Discussion.Id == activityId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
    }
}
