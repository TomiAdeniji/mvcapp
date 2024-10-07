using CleanBooksData;
using DocumentFormat.OpenXml.EMMA;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Form;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Management.Instrumentation;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Services.Description;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class TasksRules
    {
        private readonly ApplicationDbContext dbContext;

        public TasksRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ApplicationUser GetUser(string userId)
        {
            var ur = new UserRules(dbContext);
            return ur.GetUser(userId, 0);
        }

        /// <summary>
        ///     get list tasks by qbicle id
        /// </summary>
        /// <param name="cubeId">int: Qbicle Id</param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public List<QbicleTask> GetTasksByQbicleId(int cubeId, int topicId = 0)
        {
            return topicId == 0
                ? dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId && c.Topic != null).ToList()
                : dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId).ToList();
        }

        public QbicleTask UpdateTask(QbicleTask task)
        {
            var taskDb = dbContext.QbicleTasks.Find(task.Id);
            if (taskDb == null) return null;
            var formDef = new List<FormDefinition>();
            if (task.FormDefinitions.Count > 0)
            {
                var lstId = task.FormDefinitions.Select(q => q.Id).ToList();
                formDef = dbContext.FormDefinition.Where(q => lstId.Contains(q.Id)).ToList();
            }

            taskDb.Name = task.Name;
            taskDb.Priority = task.Priority;
            taskDb.Repeat = task.Repeat;
            taskDb.TimeLineDate = task.TimeLineDate;
            taskDb.Description = task.Description;
            taskDb.FormDefinitions.Clear();
            dbContext.Entry(taskDb).State = EntityState.Modified;
            dbContext.SaveChanges();
            taskDb.FormDefinitions = formDef;
            dbContext.Entry(taskDb).State = EntityState.Modified;
            dbContext.SaveChanges();
            return taskDb;
        }

        /// <summary>
        ///     Get list Tasks order by and filter by Qbicle Id
        /// </summary>
        /// <param name="cubeId">int: Qbicle Id</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <returns></returns>
        public List<QbicleTask> GetTasksOrderByQbicleId(int cubeId, Enums.OrderByDate orderBy)
        {
            var tasks = new List<QbicleTask>();
            try
            {
                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:
                        tasks = dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId)
                            .OrderByDescending(d => d.TimeLineDate).ToList();
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        tasks = dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId).OrderBy(d => d.TimeLineDate)
                            .ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

            return tasks;
        }

        /// <summary>
        ///     Get the Tasks count by Qbicle Id
        /// </summary>
        /// <param name="cubeId">int: Qbicle Id</param>
        /// <returns>Tasks count</returns>
        public int GetTasksCountByQbicleId(int cubeId)
        {
            try
            {
                return dbContext.QbicleTasks.Count(c => c.Qbicle.Id == cubeId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return 0;
            }
        }

        /// <summary>
        ///     Get the Tasks by Qbicle Id and order by and take records
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="takeSize">int: Take size</param>
        /// <returns>IQueryable<QbicleTask />
        /// </returns>
        public IQueryable<QbicleTask> GetTasksOrderByTakeQbicleId(int cubeId, Enums.OrderByDate orderBy,
            int takeSize)
        {
            IQueryable<QbicleTask> tasks;
            try
            {
                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        tasks = dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId)
                            .OrderByDescending(d => d.TimeLineDate)
                            .Take(takeSize);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        tasks = dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId)
                            .OrderBy(d => d.TimeLineDate)
                            .Take(takeSize);
                        break;
                    default:
                        tasks = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                tasks = null;
            }

            return tasks;
        }

        /// <summary>
        ///     Get the Tasks by Qbicle Id and order by and skip and take records
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="skipSize"></param>
        /// <param name="takeSize">int: Take size</param>
        /// <returns>IQueryable<QbicleTask />
        /// </returns>
        public IQueryable<QbicleTask> GetTasksOrderBySkipTakeQbicleId(int cubeId, Enums.OrderByDate orderBy,
            int skipSize, int takeSize)
        {
            IQueryable<QbicleTask> tasks;
            try
            {
                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        tasks = dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId)
                            .OrderByDescending(d => d.TimeLineDate)
                            .Skip(skipSize)
                            .Take(takeSize);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        tasks = dbContext.QbicleTasks.Where(c => c.Qbicle.Id == cubeId)
                            .OrderBy(d => d.TimeLineDate)
                            .Skip(skipSize)
                            .Take(takeSize);
                        break;
                    default:
                        tasks = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                tasks = null;
            }

            return tasks;
        }

        /// <summary>
        ///     Get the Tasks by Qbicle Id and order by and take records has been assigned to the current user
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="takeSize">int: Take size</param>
        /// <param name="curentUserId"></param>
        /// <returns>IQueryable<QbicleTask />
        /// </returns>
        public IQueryable<QbicleTask> GetTasksOrderByTakeQbicleUserId(int cubeId, Enums.OrderByDate orderBy,
            int takeSize, string curentUserId)
        {
            IQueryable<QbicleTask> taks;
            try
            {
                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        taks = dbContext.QbicleTasks.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                            .OrderByDescending(d => d.TimeLineDate)
                            .Take(takeSize)
                            .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        taks = dbContext.QbicleTasks.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                            .OrderBy(d => d.TimeLineDate)
                            .Take(takeSize)
                            .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    default:
                        taks = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId);
                taks = null;
            }

            return taks;
        }

        /// <summary>
        ///     Get the Tasks by Qbicle Id and order by and skip and take records has been assigned to the current user
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="skipSize"></param>
        /// <param name="takeSize">int: Take size</param>
        /// <param name="curentUserId"></param>
        /// <returns>IQueryable<QbicleTask />
        /// </returns>
        public IQueryable<QbicleTask> GetTasksOrderBySkipTakeQbicleUserId(int cubeId, Enums.OrderByDate orderBy,
            int skipSize, int takeSize, string curentUserId)
        {
            IQueryable<QbicleTask> tasks;
            try
            {
                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        tasks = dbContext.QbicleTasks.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                            .OrderByDescending(d => d.TimeLineDate)
                            .Skip(skipSize)
                            .Take(takeSize)
                            .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        tasks = dbContext.QbicleTasks.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                            .OrderBy(d => d.TimeLineDate)
                            .Skip(skipSize)
                            .Take(takeSize)
                            .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    default:
                        tasks = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId);
                tasks = null;
            }

            return tasks;
        }

        /// <summary>
        ///     Get Tasks rules
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="filterType"></param>
        /// <param name="orderDate"></param>
        /// <param name="size"></param>
        /// <param name="typeGet">1: filter,2: Load next</param>
        /// <param name="curentUserId"></param>
        /// <returns></returns>
        public IQueryable<QbicleTask> GetTasks(int cubeId, Enums.TaskStatus filterType, Enums.OrderByDate orderDate,
            int size, Enums.TypeGetData typeGet, string curentUserId)
        {
            IQueryable<QbicleTask> tasks = null;
            switch (filterType)
            {
                case Enums.TaskStatus.AllTasks:
                    switch (typeGet)
                    {
                        case Enums.TypeGetData.LoadNext:
                            tasks = GetTasksOrderBySkipTakeQbicleId(cubeId, orderDate, size,
                                HelperClass.qbiclePageSize);
                            break;
                        case Enums.TypeGetData.Filter:
                            tasks = GetTasksOrderByTakeQbicleId(cubeId, orderDate, size);
                            break;
                    }

                    break;
                case Enums.TaskStatus.MyTasks:
                    switch (typeGet)
                    {
                        case Enums.TypeGetData.LoadNext:
                            tasks = GetTasksOrderBySkipTakeQbicleUserId(cubeId, orderDate, size,
                                HelperClass.qbiclePageSize, curentUserId);
                            break;
                        case Enums.TypeGetData.Filter:
                            tasks = GetTasksOrderByTakeQbicleUserId(cubeId, orderDate, size, curentUserId);
                            break;
                    }

                    break;
                case Enums.TaskStatus.OnlyRegularTasks:
                    //code temp
                    switch (typeGet)
                    {
                        case Enums.TypeGetData.LoadNext:
                            tasks = GetTasksOrderBySkipTakeQbicleId(cubeId, orderDate, size,
                                HelperClass.qbiclePageSize);
                            break;
                        case Enums.TypeGetData.Filter:
                            tasks = GetTasksOrderByTakeQbicleId(cubeId, orderDate, size);
                            break;
                    }

                    break;
                case Enums.TaskStatus.OnlyFormTasks:
                    //code temp
                    switch (typeGet)
                    {
                        case Enums.TypeGetData.LoadNext:
                            tasks = GetTasksOrderBySkipTakeQbicleId(cubeId, orderDate, size,
                                HelperClass.qbiclePageSize);
                            break;
                        case Enums.TypeGetData.Filter:
                            tasks = GetTasksOrderByTakeQbicleId(cubeId, orderDate, size);
                            break;
                    }

                    break;
                case Enums.TaskStatus.CompletedTasks:
                    //code temp
                    switch (typeGet)
                    {
                        case Enums.TypeGetData.LoadNext:
                            tasks = GetTasksOrderBySkipTakeQbicleId(cubeId, orderDate, size,
                                HelperClass.qbiclePageSize);
                            break;
                        case Enums.TypeGetData.Filter:
                            tasks = GetTasksOrderByTakeQbicleId(cubeId, orderDate, size);
                            break;
                    }

                    break;
            }

            return tasks;
        }

        public ReturnJsonModel AddPostToTask(int taskId, QbiclePost post, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            try
            {
                var task = GetTaskById(taskId);
                task.TimeLineDate = DateTime.UtcNow;
                task.IsVisibleInQbicleDashboard = true;
                task.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                if (task.AssociatedSet != null)
                    post.Set = task.AssociatedSet;
                task.Posts.Add(post);
                task.IsCreatorTheCustomer = post.IsCreatorTheCustomer;
                //var startedBy = task.StartedBy;
                task.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.QbicleTasks.Attach(task);
                dbContext.Entry(task).State = EntityState.Modified;
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    DomainId = task.Qbicle.Domain.Id,
                    Id = taskId,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.TaskUpdate,
                    AppendToPageName = ApplicationPageName.Task,
                    AppendToPageId = taskId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);

                var result = new ReturnJsonModel { result = true };


                if (!string.IsNullOrEmpty(originatingConnectionId))
                {
                    var notification = new Notification
                    {
                        AssociatedQbicle = post.Topic.Qbicle,
                        CreatedBy = post.CreatedBy,
                        IsCreatorTheCustomer = post.IsCreatorTheCustomer,
                    };
                    if (appType == AppType.Web)
                        result.msg = new ISignalRNotification().HtmlRender(post, post.CreatedBy, ApplicationPageName.Task, NotificationEventEnum.TaskUpdate, notification);
                    else
                        result.Object = MicroStreamRules.GenerateActivity(post, post.StartedDate, null, post.CreatedBy.Id, post.CreatedBy.DateFormat, post.CreatedBy.Timezone, false, NotificationEventEnum.TaskUpdate, notification);
                }
                dbContext.Entry(post).State = EntityState.Unchanged;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new ReturnJsonModel { result = false };
            }
        }

        /// <summary>
        ///     validate duplicate task name
        ///     But Qbicle another have same task name
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="taskId"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public bool DuplicateTaskNameCheck(int cubeId, int taskId, string taskName)
        {
            bool exist;
            try
            {
                //var saa = dbContext.QbicleTasks
                //.Where(x => x.Id != taskId && x.Name == taskName && x.Qbicle.Id == cubeId).FirstOrDefault();
                if (taskId > 0)
                {
                    var task = dbContext.QbicleTasks.Find(taskId);
                    if (taskName == task?.Name)
                        return false;
                    exist = dbContext.QbicleTasks.Any(
                        x => x.Id != taskId && x.Name == taskName && x.Qbicle.Id == cubeId);
                }
                else
                {
                    exist = dbContext.QbicleTasks.Any(x => x.Name == taskName && x.Qbicle.Id == cubeId);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                exist = false;
            }

            return exist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="task"></param>
        /// <param name="assignee"></param>
        /// <param name="mediaModel"></param>
        /// <param name="watchers"></param>
        /// <param name="cubeId"></param>
        /// <param name="userId"></param>
        /// <param name="topicId"></param>
        /// <param name="activitiesRelate"></param>
        /// <param name="stepLst"></param>
        /// <param name="qbicleRecurrance"></param>
        /// <param name="lstDate"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveTask(QbicleTask task, bool isCreatorTheCustomer, string assignee,
            MediaModel mediaModel, string[] watchers, int cubeId,
            string userId, int topicId, int[] activitiesRelate, List<QbicleStep> stepLst,
            QbicleRecurrance qbicleRecurrance, List<CustomDateModel> lstDate, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            try
            {
                var result = new ReturnJsonModel { result = true };

                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                task.Topic = topic;

                var currentUser = dbContext.QbicleUser.Find(userId);

                task.StartedBy = currentUser;
                task.StartedDate = DateTime.UtcNow;
                task.State = QbicleActivity.ActivityStateEnum.Open;
                task.IsCreatorTheCustomer = isCreatorTheCustomer;
                var currentQbicle = new QbicleRules(dbContext).GetQbicleById(cubeId);


                if (task.Id == 0)
                {
                    currentQbicle.LastUpdated = DateTime.UtcNow;
                    task.Qbicle = currentQbicle;
                }

                task.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;

                if (!task.ProgrammedStart.HasValue)
                    task.ProgrammedStart = DateTime.UtcNow;
                task.TimeLineDate = DateTime.UtcNow;

                switch (task.DurationUnit)
                {
                    case QbicleTask.TaskDurationUnitEnum.Hours:
                        task.ProgrammedEnd = task.ProgrammedStart.Value.AddHours(task.Duration);
                        break;
                    case QbicleTask.TaskDurationUnitEnum.Days:
                        task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration);
                        break;
                    case QbicleTask.TaskDurationUnitEnum.Weeks:
                        task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration * 7);
                        break;
                    default:
                        break;
                }
                QbicleMedia taskMedia = null;
                if (mediaModel != null && !string.IsNullOrEmpty(mediaModel.Name) && !dbContext.VersionedFiles.Any(v => v.Uri == mediaModel.UrlGuid))
                {
                    new Azure.AzureStorageRules(dbContext).ProcessingMediaS3(mediaModel.UrlGuid);
                    //Media attach
                    taskMedia = new QbicleMedia
                    {
                        StartedBy = currentUser,
                        StartedDate = DateTime.UtcNow,
                        Name = mediaModel.Name,
                        FileType = mediaModel.Type,
                        Qbicle = currentQbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic,
                        MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
                        IsVisibleInQbicleDashboard = false,
                        IsCreatorTheCustomer = isCreatorTheCustomer
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = mediaModel.Size,
                        UploadedBy = currentUser,
                        UploadedDate = DateTime.UtcNow,
                        Uri = mediaModel.UrlGuid,
                        FileType = mediaModel.Type
                    };
                    taskMedia.VersionedFiles.Add(versionFile);
                    taskMedia.ActivityMembers.Add(currentUser);

                    dbContext.Medias.Add(taskMedia);
                    dbContext.Entry(taskMedia).State = EntityState.Added;

                }

                #region Steps

                var steps = dbContext.Steps.Where(s => s.ActivityId == task.Id).ToList();
                if (steps.Count > 0)
                    foreach (var item in steps)
                    {
                        var sti = item.StepInstance.FirstOrDefault(s => s.Step.Id == item.Id && s.Task.Id == task.Id);
                        if (sti != null)
                            dbContext.Stepinstances.Remove(sti);
                        dbContext.Steps.Remove(item);
                    }

                if (task.isSteps)
                    foreach (var item in stepLst)
                    {
                        if (task.Id > 0)
                            item.ActivityId = task.Id;
                        dbContext.Steps.Add(item);
                        dbContext.Entry(item).State = EntityState.Added;
                        task.QSteps.Add(item);
                    }

                #endregion

                QbicleSet qbicleSet;
                var isTaskNewCreated = false;
                var taskEvent = NotificationEventEnum.TaskCreation;
                if (task.Id == 0)
                {
                    qbicleSet = new QbicleSet();
                    task.AssociatedSet = qbicleSet;

                    if (taskMedia != null)
                        task.SubActivities.Add(taskMedia);

                    dbContext.QbicleTasks.Add(task);
                    dbContext.Entry(task).State = EntityState.Added;
                    dbContext.SaveChanges();
                    isTaskNewCreated = true;
                    result.actionVal = task.Id;
                }
                else
                {
                    taskEvent = NotificationEventEnum.TaskUpdate;

                    var dbtask = dbContext.QbicleTasks.Find(task.Id);
                    var job = new QbicleJobParameter
                    {
                        EndPointName = "deletehangfirejobstate",
                        JobId = dbtask.JobId,
                    };
                    Task tskHangfire = new Task(async () =>
                    {
                        await new QbiclesJob().HangFireExcecuteAsync(job);
                    });
                    tskHangfire.Start();

                    dbtask.Topic = task.Topic;
                    dbtask.Name = task.Name;
                    dbtask.Description = task.Description;
                    dbtask.Duration = task.Duration;
                    dbtask.DurationUnit = task.DurationUnit;
                    dbtask.Priority = task.Priority;
                    dbtask.TimeLineDate = DateTime.UtcNow;
                    dbtask.ProgrammedStart = task.ProgrammedStart;
                    dbtask.ProgrammedEnd = task.ProgrammedEnd;
                    dbtask.isSteps = task.isSteps;
                    dbtask.isRecurs = task.isRecurs;
                    dbtask.isComplete = task.isComplete;
                    dbtask.IsVisibleInQbicleDashboard = true;
                    dbtask.QSteps = task.QSteps;
                    dbtask.IsCreatorTheCustomer = isCreatorTheCustomer;
                    if (taskMedia != null)
                        dbtask.SubActivities.Add(taskMedia);
                    if (dbtask != null && dbtask.AssociatedSet != null)
                    {
                        qbicleSet = dbtask.AssociatedSet;
                    }
                    else
                    {
                        qbicleSet = new QbicleSet();
                        dbContext.Sets.Add(qbicleSet);
                        dbContext.Entry(qbicleSet).State = EntityState.Added;
                        dbtask.AssociatedSet = qbicleSet;
                    }

                    if (dbContext.Entry(dbtask).State == EntityState.Detached)
                        dbContext.QbicleTasks.Attach(dbtask);
                    dbContext.Entry(dbtask).State = EntityState.Modified;
                    //Update LastUpdated currentDomain
                    var currentDomain = dbContext.Qbicles.Find(cubeId);
                    if (currentDomain != null)
                    {
                        currentDomain.LastUpdated = DateTime.UtcNow;
                        if (dbContext.Entry(currentDomain).State == EntityState.Detached)
                            dbContext.Qbicles.Attach(currentDomain);
                        dbContext.Entry(currentDomain).State = EntityState.Modified;
                    }

                    result.actionVal = task.Id;
                }

                //link
                if (qbicleSet.Id == 0)
                {
                    dbContext.Sets.Add(qbicleSet);
                    dbContext.Entry(qbicleSet).State = EntityState.Added;
                }

                #region Peobles
                var _activityMembers = new List<ApplicationUser>();
                var isNotificationSent2TaskAssignee = false;
                _activityMembers.Add(currentUser);
                //Task People (People->Type 0 = Assignee);(People->Type 1 = Watcher)
                if (!string.IsNullOrEmpty(assignee))
                {
                    var peopleAssignee = dbContext.People.Where(s =>
                            s.AssociatedSet.Id == qbicleSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee)
                        .FirstOrDefault();
                    if (peopleAssignee == null)
                    {
                        peopleAssignee = new QbiclePeople();
                        peopleAssignee.isPresent = true;
                        peopleAssignee.Type = QbiclePeople.PeopleTypeEnum.Assignee;
                        var user = dbContext.QbicleUser.Find(assignee);
                        if (user != null)
                        {
                            peopleAssignee.User = user;
                            peopleAssignee.AssociatedSet = qbicleSet;
                            dbContext.People.Add(peopleAssignee);
                            dbContext.Entry(peopleAssignee).State = EntityState.Added;
                            _activityMembers.Add(user);
                            isNotificationSent2TaskAssignee = true;
                        }
                    }
                    else if (peopleAssignee.User.Id != assignee)
                    {
                        var user = dbContext.QbicleUser.Find(assignee);
                        if (user != null)
                        {
                            peopleAssignee.User = user;
                            //dbContext.People.Add(peopleAssignee);
                            if (dbContext.Entry(peopleAssignee).State == EntityState.Detached)
                                dbContext.People.Attach(peopleAssignee);
                            dbContext.Entry(peopleAssignee).State = EntityState.Modified;
                            _activityMembers.Add(user);
                            isNotificationSent2TaskAssignee = true;
                        }
                    }
                }

                //Remove Watchers Old
                var peoplesWatch = dbContext.People.Where(s =>
                    s.AssociatedSet.Id == qbicleSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Watcher).ToList();
                if (peoplesWatch.Count > 0) dbContext.People.RemoveRange(peoplesWatch);
                if (watchers != null && watchers.Any())
                    foreach (var item in watchers)
                        if (item != assignee)
                        {
                            var peopleWatcher = new QbiclePeople
                            {
                                isPresent = true,
                                Type = QbiclePeople.PeopleTypeEnum.Watcher
                            };
                            var user = dbContext.QbicleUser.Find(item);
                            if (user != null)
                            {
                                peopleWatcher.User = user;
                                peopleWatcher.AssociatedSet = qbicleSet;
                                dbContext.People.Add(peopleWatcher);
                                dbContext.Entry(peopleWatcher).State = EntityState.Added;
                                _activityMembers.Add(user);
                            }
                        }

                if (_activityMembers.Any())
                {
                    var dbtask = dbContext.QbicleTasks.Find(task.Id);
                    dbtask.ActivityMembers.Clear();
                    dbtask.ActivityMembers.AddRange(_activityMembers);
                }
                #endregion

                #region Related

                var relates = dbContext.Relateds.Where(s => s.AssociatedSet.Id == qbicleSet.Id).ToList();
                if (relates.Count > 0) dbContext.Relateds.RemoveRange(relates);
                if (activitiesRelate != null && activitiesRelate.Length > 0)
                    foreach (var item in activitiesRelate)
                    {
                        var activity = dbContext.Activities.Find(item);
                        if (activity != null)
                        {
                            var rl = new QbicleRelated { AssociatedSet = qbicleSet, Activity = activity };
                            dbContext.Relateds.Add(rl);
                            dbContext.Entry(rl).State = EntityState.Added;
                        }
                    }

                #endregion

                #region recurrance

                //dbContext.SaveChanges();
                var recurrance = dbContext.Recurrances.Where(s => s.AssociatedSet.Id == qbicleSet.Id).ToList();
                if (task.isRecurs)
                {
                    qbicleRecurrance.Id = qbicleSet.Id;
                    if (recurrance.Count > 0) dbContext.Recurrances.RemoveRange(recurrance);
                    dbContext.Recurrances.Add(qbicleRecurrance);
                    dbContext.Entry(qbicleRecurrance).State = EntityState.Added;
                    foreach (var item in lstDate)
                        if (task.ProgrammedStart != item.StartDate)
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
                                StartedDate = item.StartDate,
                                Topic = topic,
                                isSteps = task.isSteps,
                                Priority = task.Priority,
                                TimeLineDate = task.TimeLineDate,
                                IsVisibleInQbicleDashboard = false
                            };

                            currentQbicle.LastUpdated = DateTime.UtcNow;
                            task2.Qbicle = currentQbicle;
                            task2.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                            task2.ProgrammedStart = item.StartDate;
                            if (task2.isSteps)
                                foreach (var it in stepLst)
                                {
                                    var step = new QbicleStep
                                    {
                                        Name = it.Name,
                                        Order = it.Order,
                                        Description = it.Description,
                                        Weight = it.Weight
                                    };
                                    dbContext.Steps.Add(step);
                                    dbContext.Entry(step).State = EntityState.Added;
                                    task2.QSteps.Add(step);
                                }

                            if (!task2.ProgrammedStart.HasValue) task2.ProgrammedStart = DateTime.UtcNow;
                            if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Days)
                                task2.ProgrammedEnd = task2.ProgrammedStart.HasValue
                                    ? task2.ProgrammedStart.Value.AddDays(task.Duration)
                                    : DateTime.UtcNow;
                            else if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Hours)
                                task2.ProgrammedEnd = task2.ProgrammedStart.HasValue
                                    ? task2.ProgrammedStart.Value.AddHours(task.Duration)
                                    : DateTime.UtcNow;
                            else
                                task2.ProgrammedEnd = task2.ProgrammedStart.HasValue
                                    ? task2.ProgrammedStart.Value.AddDays(task.Duration * 7)
                                    : DateTime.UtcNow;

                            if (!string.IsNullOrEmpty(mediaModel.Name))
                            {
                                //Media attach
                                taskMedia = new QbicleMedia
                                {
                                    StartedBy = currentUser,
                                    StartedDate = DateTime.UtcNow,
                                    Name = task.Name,
                                    FileType = mediaModel.Type,
                                    Qbicle = currentQbicle,
                                    TimeLineDate = DateTime.UtcNow,
                                    Topic = task.Topic,

                                    MediaFolder =
                                        new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
                                    Description = task.Description,
                                    IsVisibleInQbicleDashboard = false
                                };
                                var versionFile = new VersionedFile
                                {
                                    IsDeleted = false,
                                    FileSize = mediaModel.Size,
                                    UploadedBy = currentUser,
                                    UploadedDate = DateTime.UtcNow,
                                    Uri = mediaModel.UrlGuid,
                                    FileType = mediaModel.Type
                                };
                                taskMedia.VersionedFiles.Add(versionFile);
                                taskMedia.ActivityMembers.Add(currentUser);

                                dbContext.Medias.Add(taskMedia);
                                dbContext.Entry(taskMedia).State = EntityState.Added;

                                task2.SubActivities.Add(taskMedia);
                            }

                            task2.AssociatedSet = qbicleSet;
                            if (_activityMembers.Any())
                                task2.ActivityMembers.AddRange(_activityMembers);
                            dbContext.QbicleTasks.Add(task2);
                            dbContext.Entry(task2).State = EntityState.Added;
                        }
                }

                #endregion

                if (currentQbicle is C2CQbicle)
                {
                    var unseenUser = (currentQbicle as C2CQbicle).Customers.FirstOrDefault(p => p.Id != currentUser.Id);
                    if ((currentQbicle as C2CQbicle).NotViewedBy == null)
                        (currentQbicle as C2CQbicle).NotViewedBy = new List<ApplicationUser>();
                    (currentQbicle as C2CQbicle).NotViewedBy.Add(unseenUser);
                    if (unseenUser.NotViewedQbicle == null)
                        unseenUser.NotViewedQbicle = new List<C2CQbicle>();
                    unseenUser.NotViewedQbicle.Add(currentQbicle as C2CQbicle);

                    currentQbicle.RemovedForUsers.Remove(unseenUser);
                    unseenUser.RemovedQbicle.Remove(currentQbicle);
                }

                dbContext.SaveChanges();


                var nRule = new NotificationRules(dbContext);
                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    DomainId = currentQbicle.Domain.Id,
                    Id = task.Id,
                    EventNotify = taskEvent,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = currentUser.Id,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);

                //send email and alert if the task not started after 30 minutes
                var reminderMinutes = (task.ProgrammedStart?.AddHours(-24) - DateTime.UtcNow)?.TotalMinutes ?? 0;
                activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = task.Id,
                    EventNotify = NotificationEventEnum.TaskNotificationPoints,
                    AppendToPageName = ApplicationPageName.All,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = reminderMinutes <= 0 ? 0 : reminderMinutes,
                };
                nRule.Notification2EventTaskPoints(activityNotification, dbContext.QbicleTasks.Find(task.Id));


                if (isNotificationSent2TaskAssignee && isTaskNewCreated)
                {
                    var userAssigned = dbContext.QbicleUser.Find(assignee);

                    //Send notification to Assignee of Task
                    if (userAssigned != null)
                    {
                        var notification2Assignee = new ActivityNotification
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            DomainId = currentQbicle.Domain.Id,
                            QbicleId = currentQbicle.Id,
                            AppendToPageName = ApplicationPageName.Domain,//TODO: Unknow required???
                            EventNotify = NotificationEventEnum.AssignTask,
                            ObjectById = userAssigned.Id,
                            ReminderMinutes = 0,
                            CreatedById = currentUser.Id,
                            CreatedByName = currentUser.GetFullName(),
                            Id = task.Id
                        };
                        nRule.Notification2TaskAssignee(notification2Assignee);
                    }

                }


                if (!string.IsNullOrEmpty(originatingConnectionId))
                {
                    var notification = new Notification
                    {
                        AssociatedQbicle = task.Qbicle,
                        CreatedBy = task.StartedBy,
                        IsCreatorTheCustomer = task.IsCreatorTheCustomer,
                    };
                    if (appType == AppType.Web)
                        result.msg = new ISignalRNotification().HtmlRender(task, currentUser, ApplicationPageName.Activities, NotificationEventEnum.TaskCreation, notification);
                    else
                        result.Object = MicroStreamRules.GenerateActivity(task, task.StartedDate, null, currentUser.Id, currentUser.DateFormat, currentUser.Timezone, false, NotificationEventEnum.TaskCreation, notification);
                }
                if (dbContext.Entry(task).State == EntityState.Unchanged)
                    dbContext.Entry(task).State = EntityState.Added;
                dbContext.Entry(task).State = EntityState.Unchanged;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId);

                return new ReturnJsonModel { result = true, msg = ex.Message };
            }
        }

        public int CountCanTasksDelete(int ctaskId)
        {
            try
            {
                var dbTask = dbContext.QbicleTasks.Find(ctaskId);
                if (dbTask != null)
                    return dbTask.AssociatedSet.Activities.Where(s =>
                        s.Id != ctaskId && s.ProgrammedStart >= DateTime.UtcNow && s.ActualStart == null).Count();
                return 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return 0;
            }
        }

        public ReturnJsonModel RecurringTasksDelete(int ctaskId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var countTaskStart = 0;
                var countTaskDelete = 0;
                var dbTask = dbContext.QbicleTasks.Find(ctaskId);
                if (dbTask != null)
                {
                    var atvs = dbTask.AssociatedSet.Activities.Where(s =>
                        s.Id != ctaskId && s.ProgrammedStart >= DateTime.UtcNow && s.ActualStart == null).ToList();
                    foreach (var item in atvs)
                    {
                        var tsk = (QbicleTask)item;
                        if (tsk.ActualStart == null)
                            try
                            {
                                //Remove Media 
                                var medias = tsk.Media;
                                if (medias != null && medias.Count > 0)
                                {
                                    foreach (var it in medias)
                                    {
                                        var vs = it.VersionedFiles;
                                        if (vs != null && vs.Count > 0)
                                            dbContext.VersionedFiles.RemoveRange(vs);
                                    }

                                    dbContext.Medias.RemoveRange(medias);
                                }

                                //Remove SubActivities
                                var subActivities = tsk.SubActivities;
                                if (subActivities != null && subActivities.Count > 0)
                                {
                                    foreach (var it in subActivities)
                                    {
                                        var md = (QbicleMedia)it;
                                        var vs = md.VersionedFiles;
                                        if (vs != null && vs.Count > 0)
                                            dbContext.VersionedFiles.RemoveRange(vs);
                                    }

                                    dbContext.Activities.RemoveRange(subActivities);
                                }
                                //Remove Activity Members
                                tsk.ActivityMembers.Clear();
                                //Remove Comments
                                if (tsk.Posts != null && tsk.Posts.Count > 0)
                                    dbContext.Posts.RemoveRange(tsk.Posts);
                                //Remove Steps
                                if (tsk.QStepinstances != null && tsk.QStepinstances.Count > 0)
                                    dbContext.Stepinstances.RemoveRange(tsk.QStepinstances);
                                //Remove Steps
                                if (tsk.QSteps != null && tsk.QSteps.Count > 0)
                                    dbContext.Steps.RemoveRange(tsk.QSteps);
                                //Remove Notify
                                var notifys = dbContext.Notifications.Where(s => s.AssociatedAcitvity.Id == tsk.Id)
                                    .ToList();
                                if (notifys != null && notifys.Count > 0) dbContext.Notifications.RemoveRange(notifys);
                                //Remove Mypins
                                var pins = dbContext.MyPins.Where(s => s.PinnedActivity.Id == tsk.Id);
                                if (pins != null && pins.Any())
                                    dbContext.MyPins.RemoveRange(pins);
                                //Remove Task
                                dbContext.QbicleTasks.Remove(tsk);
                                if (dbContext.SaveChanges() > 0) countTaskDelete += 1;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                            }
                        else
                            countTaskStart += 1;
                    }

                    var recurrence = dbTask.AssociatedSet.Recurrance;
                    if (recurrence != null)
                        dbContext.Recurrances.Remove(recurrence);
                    dbTask.isRecurs = false;
                    if (dbContext.Entry(dbTask).State == EntityState.Detached)
                        dbContext.QbicleTasks.Attach(dbTask);
                    dbContext.Entry(dbTask).State = EntityState.Modified;
                    refModel.result = dbContext.SaveChanges() > 0;
                    refModel.Object = new { countTaskStart, countTaskDelete };
                }

                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        /// <summary>
        ///     Get count of tasks
        /// </summary>
        /// <param name="tasks">List<QbicleTask />
        /// </param>
        /// <returns></returns>
        public int GetTaskCountInTheTasks(List<QbicleTask> tasks)
        {
            return tasks.Count();
        }

        /// <summary>
        ///     Get task whit take size from the tasks list
        /// </summary>
        /// <param name="tasks">List<QbicleTask />
        /// </param>
        /// <returns></returns>
        public List<QbicleTask> GetTasksTakeSizeInTheTasks(List<QbicleTask> tasks)
        {
            return tasks.Take(HelperClass.qbiclePageSize).ToList();
        }

        /// <summary>
        ///     Get list dates Tasks for Qbicle view on the tab Tasks panel
        /// </summary>
        /// <param name="qbicleTaks"></param>
        /// <returns></returns>
        public IEnumerable<DateTime> GetTasksDate(List<QbicleTask> qbicleTaks)
        {
            var taskDates = from t in qbicleTaks select t.TimeLineDate.Date;
            return taskDates.Distinct();
        }

        public IEnumerable<DateTime> LoadMoreTasks(int cubeId, int size,
            ref List<QbicleTask> tasks, ref int acivitiesDateCount, string currentTimeZone)
        {
            IEnumerable<DateTime> activitiesDate = null;

            var qbicleTasks = GetTasksByQbicleId(cubeId).BusinessMapping(currentTimeZone);

            tasks = qbicleTasks;
            var taskDates = from t in qbicleTasks select t.TimeLineDate.Date;

            var disDates = taskDates;
            acivitiesDateCount = disDates.Count();

            disDates = disDates.Distinct().OrderByDescending(d => d.Date.Date);
            activitiesDate = disDates.OrderByDescending(d => d.Date).Skip(size).Take(HelperClass.qbiclePageSize);

            tasks = qbicleTasks.Where(o => activitiesDate.Contains(o.TimeLineDate.Date)).ToList();
            return activitiesDate;
        }

        /// <summary>
        ///     Get a task by task id
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public QbicleTask GetTaskById(int taskId)
        {
            try
            {
                return dbContext.QbicleTasks.Find(taskId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new QbicleTask();
            }
        }


        public object GenerateTaskReport(int taskId, string currentTimeZone)
        {
            try
            {
                var task = dbContext.QbicleTasks.Find(taskId);
                if (task == null) return "";
                var assigns = string.Join(" ,", task.ActivityMembers.Select(n => n.Forename + " " + n.Surname));

                return new
                {
                    task.Name,
                    task.Priority,
                    Recurring = task.Repeat,
                    Deadline = task.DueDate == null
                        ? ""
                        : ((DateTime)task.DueDate).ConvertTimeFromUtc(currentTimeZone).ToString("dd/MM/yyyy"),
                    Assign = assigns,
                    task.Description
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return "";
            }
        }



        public bool PerformanceReview(QbiclePerformance performance)
        {
            try
            {
                if (performance.Id == 0)
                {
                    dbContext.Performances.Add(performance);
                    dbContext.Entry(performance).State = EntityState.Added;
                }

                #region Update task if all step complete

                var task = dbContext.QbicleTasks.Find(performance.Task.Id);
                if (task != null)
                {
                    var isComplete = true;
                    foreach (var item in task.QSteps)
                    {
                        var stI = dbContext.Stepinstances.FirstOrDefault(s => s.Task.Id == performance.Id && s.Step.Id == item.Id);
                        if (stI == null || !stI.isComplete)
                        {
                            isComplete = false;
                            break;
                        }
                    }

                    if (!isComplete)
                    {
                        task.IsVisibleInQbicleDashboard = true;
                        task.TimeLineDate = DateTime.UtcNow;
                        task.ActualEnd = DateTime.UtcNow;
                        task.isComplete = true;
                        if (dbContext.Entry(task).State == EntityState.Detached)
                            dbContext.QbicleTasks.Attach(task);
                        dbContext.Entry(task).State = EntityState.Modified;
                    }
                }

                #endregion

                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }

        public ReturnJsonModel AddWorkLog(QbicleTimeSpent timeSpent, bool isCreatorTheCustomer, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            try
            {
                if (timeSpent.Task.QTimeSpents.Any() == false)
                    timeSpent.Task.ActualStart = DateTime.UtcNow;

                timeSpent.Task.IsCreatorTheCustomer = isCreatorTheCustomer;

                if (timeSpent.Id == 0)
                {
                    dbContext.Timespents.Add(timeSpent);
                    dbContext.Entry(timeSpent).State = EntityState.Added;
                }
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);
                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = timeSpent.Task.Id,
                    EventNotify = NotificationEventEnum.TaskCompletion,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = timeSpent.Task.StartedBy.Id,
                    CreatedByName = timeSpent.Task.StartedBy.DisplayUserName,
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);



                return new ReturnJsonModel { result = true, actionVal = timeSpent.Id };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);

                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }
        public ReturnJsonModel RemoveWorkLog(int id)
        {
            try
            {
                var timeSpent = dbContext.Timespents.Find(id);
                var log = dbContext.Timespents.Remove(timeSpent);

                dbContext.SaveChanges();
                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);

                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public List<QbiclePeople> GetPeoples(int setId)
        {
            try
            {
                return dbContext.People.Where(s => s.AssociatedSet.Id == setId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public bool CheckAddWorkLog(QbicleTask task, string userId)
        {
            try
            {
                if (task != null)
                    if (task.StartedBy.Id == userId || task.AssociatedSet != null &&
                        task.AssociatedSet.Peoples.Any(s =>
                            s.Type == QbiclePeople.PeopleTypeEnum.Assignee && s.User.Id == userId))
                        return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<QbicleStep> GetSteps(int taskId)
        {
            try
            {
                return dbContext.Steps.Where(s => s.ActivityId == taskId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public List<QbicleStepInstanceModel> GetStepInstances(int activityId)
        {
            try
            {
                return dbContext.Stepinstances.Where(s => s.Task.Id == activityId).Select(s =>
                    new QbicleStepInstanceModel
                    {
                        Id = s.Id,
                        ActivityId = s.Task.Id,
                        Complete = s.isComplete,
                        StepId = s.Step.Id,
                        Percent = s.Step.Weight
                    }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public DataTablesResponse GetActivesRelated(IDataTablesRequest requestModel, int currentQbicleId,
            int currentActivityId)
        {
            try
            {
                IQueryable<QbicleActivity> query = dbContext.Activities;

                #region Filtering 

                // Apply filters for searching 

                var value = requestModel.Search.Value != string.Empty ? requestModel.Search.Value.Trim().ToLower() : "";
                if (currentActivityId == 0)
                {
                    if (value != "")
                        query = query.Where(p =>
                            p.Name.Contains(value) && p.Qbicle.Id == currentQbicleId &&
                            (p.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity ||
                             p.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity));
                    else
                        query = query.Where(p =>
                            p.Qbicle.Id == currentQbicleId &&
                            (p.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity ||
                             p.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity));
                }
                else
                {
                    if (value != "")
                        query = query.Where(p =>
                            p.Name.Contains(value) && p.Qbicle.Id == currentQbicleId && p.Id != currentActivityId &&
                            (p.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity ||
                             p.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity));
                    else
                        query = query.Where(p =>
                            p.Qbicle.Id == currentQbicleId && p.Id != currentActivityId &&
                            (p.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity ||
                             p.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity));
                }


                var filteredCount = query.Count();

                #endregion Filtering 

                #region Sorting 

                // Sorting 
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;

                foreach (var column in sortedColumns)
                {
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += (column.Data != "Type" ? column.Data : "ActivityType") +
                                     (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);

                #endregion Sorting 

                // Paging 
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var data = list.Select(av => new RelatedActivity
                {
                    Id = av.Id,
                    Name = av.Name,
                    Type = av.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity ? "Task" : "Event"
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, data, filteredCount, filteredCount);
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// start from web
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="complete"></param>
        /// <param name="actualEnd"></param>
        /// <param name="countdown"></param>
        /// <returns></returns>
        public bool StartProgress(int taskId, UserSetting user, string originatingConnectionId, ref bool complete, ref DateTime? actualEnd,
            ref string countdown)
        {
            try
            {
                var task = dbContext.QbicleTasks.Find(taskId);
                if (task != null)
                {
                    task.IsVisibleInQbicleDashboard = true;
                    task.TimeLineDate = DateTime.UtcNow;
                    task.ActualStart = DateTime.UtcNow;
                    if (task.ProgrammedEnd.HasValue &&
                        task.ActualStart.Value < task.ProgrammedEnd.Value)
                    {
                        var time = task.ProgrammedEnd.Value - task.ActualStart.Value;
                        countdown = $"{time.Days}d {time.Hours}h {time.Minutes}m";
                    }

                    if (!task.isSteps)
                    {
                        task.ActualEnd = task.ActualStart;
                        task.isComplete = true;
                        complete = true;
                        actualEnd = task.ActualStart;
                    }

                    if (dbContext.Entry(task).State == EntityState.Detached)
                        dbContext.Activities.Attach(task);
                    dbContext.Entry(task).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    NotificationStartOrCompleteTask(taskId, user.Id, user.DisplayName, originatingConnectionId, NotificationEventEnum.TaskStart);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }

        public ReturnJsonModel MicroStartProgress(int taskId, string userId, string userDisplayName, string originatingConnectionId)
        {
            try
            {
                var task = dbContext.QbicleTasks.FirstOrDefault(e => e.Id == taskId);
                if (task != null)
                {
                    if (task.ActualStart != null)
                        return new ReturnJsonModel { result = false, msg = "Task has been started" };
                    task.IsVisibleInQbicleDashboard = true;
                    task.TimeLineDate = DateTime.UtcNow;
                    task.ActualStart = DateTime.UtcNow;
                    if (dbContext.Entry(task).State == EntityState.Detached)
                        dbContext.Activities.Attach(task);
                    dbContext.Entry(task).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    NotificationStartOrCompleteTask(taskId, userId, userDisplayName, originatingConnectionId, NotificationEventEnum.TaskStart);
                    return new ReturnJsonModel { result = true };
                }

                return new ReturnJsonModel { result = false, msg = $"Task not found id {taskId}" };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        /// <summary>
        ///     Task Completion
        /// </summary>
        /// <param name="token"></param>
        /// <param name="taskId"></param>
        /// <param name="curentUser"></param>
        /// <returns></returns>
        public bool CloseTask(int taskId, UserSetting user, string originatingConnectionId = "")
        {
            try
            {
                var task = dbContext.QbicleTasks.Find(taskId);
                if (task == null) return false;
                task.TimeLineDate = DateTime.UtcNow;
                task.State = QbicleActivity.ActivityStateEnum.Closed;
                task.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ClosedTask;
                task.ClosedDate = DateTime.UtcNow;
                //var startedBy = task.StartedBy;
                //var createdDate = task.StartedDate;
                task.Qbicle.LastUpdated = DateTime.UtcNow;
                //var startedByQ = task.Qbicle.StartedBy;
                //var createdDateQ = task.Qbicle.StartedDate;

                if (dbContext.Entry(task).State == EntityState.Detached)
                    dbContext.QbicleTasks.Attach(task);
                dbContext.Entry(task).State = EntityState.Modified;

                dbContext.SaveChanges();

                NotificationStartOrCompleteTask(taskId, user.Id, user.DisplayName, originatingConnectionId, NotificationEventEnum.TaskComplete);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }


        public ReturnJsonModel MicroMarkAsComplete(int taskId, string userId, string userDisplayName, string originatingConnectionId)
        {
            try
            {
                var activity = dbContext.QbicleTasks.FirstOrDefault(e => e.Id == taskId);
                if (activity != null)
                {
                    if (activity.ActualStart == null)
                        return new ReturnJsonModel { result = false, msg = "Task has not yet been started" };

                    if (activity.ActualStart == null)
                        activity.ActualStart = DateTime.UtcNow;
                    activity.IsVisibleInQbicleDashboard = true;
                    activity.TimeLineDate = DateTime.UtcNow;
                    activity.ActualEnd = DateTime.UtcNow;
                    activity.isComplete = true;
                    if (dbContext.Entry(activity).State == EntityState.Detached)
                        dbContext.Activities.Attach(activity);
                    dbContext.Entry(activity).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    NotificationStartOrCompleteTask(taskId, userId, userDisplayName, originatingConnectionId, NotificationEventEnum.TaskComplete);
                    return new ReturnJsonModel { result = true };
                }

                return new ReturnJsonModel { result = false, msg = $"Task not found id {taskId}" };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }



        public bool ConfirmSteps(int taskId, List<int> stepInstance, UserSetting user, string originatingConnectionId, ref bool complete, ref DateTime? actualEnd)
        {
            try
            {
                foreach (var item in stepInstance)
                    if (!dbContext.Stepinstances.Any(s => s.Task.Id == taskId && s.Step.Id == item))
                    {
                        var stIns = new QbicleStepInstance();
                        var qbicleTask = dbContext.QbicleTasks.Find(taskId);
                        if (qbicleTask != null)
                        {
                            qbicleTask.IsVisibleInQbicleDashboard = true;
                            qbicleTask.TimeLineDate = DateTime.UtcNow;
                            stIns.Task = qbicleTask;
                            var qbicleStep = dbContext.Steps.Find(item);
                            if (qbicleStep != null)
                            {
                                stIns.Step = qbicleStep;
                                stIns.isComplete = true;
                                dbContext.Stepinstances.Add(stIns);
                                dbContext.Entry(stIns).State = EntityState.Added;
                            }
                        }
                    }

                //Save Steps
                dbContext.SaveChanges();

                #region Update task if all step complete

                var task = dbContext.QbicleTasks.Find(taskId);
                if (task != null)
                {
                    complete = true;
                    foreach (var item in task.QSteps)
                    {
                        var stI = dbContext.Stepinstances
                            .FirstOrDefault(s => s.Task.Id == taskId && s.Step.Id == item.Id);
                        if (stI == null || !stI.isComplete)
                        {
                            complete = false;
                            break;
                        }
                    }

                    if (complete)
                    {
                        task.IsVisibleInQbicleDashboard = true;
                        task.TimeLineDate = DateTime.UtcNow;
                        task.ActualEnd = DateTime.UtcNow;
                        task.isComplete = true;
                        actualEnd = task.ActualEnd;
                        if (dbContext.Entry(task).State == EntityState.Detached)
                            dbContext.QbicleTasks.Attach(task);
                        dbContext.Entry(task).State = EntityState.Modified;
                    }
                }

                #endregion

                //Save Task update if is complete
                dbContext.SaveChanges();

                if (complete)
                    NotificationStartOrCompleteTask(taskId, user.Id, user.DisplayName, originatingConnectionId, NotificationEventEnum.TaskComplete);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }

        private void NotificationStartOrCompleteTask(int taskId, string createdById, string createdByName, string originatingConnectionId, NotificationEventEnum eventTask)
        {

            var activityNotification = new ActivityNotification
            {
                OriginatingConnectionId = originatingConnectionId,
                Id = taskId,
                EventNotify = eventTask,
                AppendToPageName = ApplicationPageName.All,
                AppendToPageId = 0,
                CreatedById = createdById,
                CreatedByName = createdByName,
                ReminderMinutes = 0,
            };
            new NotificationRules(dbContext).Notification2EventTaskPoints(activityNotification, dbContext.QbicleTasks.Find(taskId));
        }


    }
}