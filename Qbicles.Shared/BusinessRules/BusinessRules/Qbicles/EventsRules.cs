using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class EventsRules
    {
        private ApplicationDbContext dbContext;

        public EventsRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public QbicleEvent UpdateEvent(QbicleEvent qEvent)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update event", null, null, qEvent);

                var _event = dbContext.Events.Find(qEvent.Id);
                if (_event == null) return null;
                _event.IsVisibleInQbicleDashboard = true;
                _event.TimeLineDate = DateTime.UtcNow;
                _event.Description = qEvent.Description;
                _event.EventType = qEvent.EventType;
                _event.Name = qEvent.Name;
                _event.Location = qEvent.Location;
                _event.StartedDate = qEvent.StartedDate;
                _event.End = qEvent.End;
                dbContext.Entry(_event).State = EntityState.Modified;
                dbContext.SaveChanges();
                return _event;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qEvent);
                return null;
            }

        }

        /// <summary>
        ///     Get list events by qbicle id
        /// </summary>
        /// <param name="cubeId">int Qbicle Id</param>
        /// <param name="orderBy"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public List<QbicleEvent> GetEventsOrderByQbicleId(int cubeId, Enums.OrderByDate orderBy, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get events order by qbicle id", null, null, cubeId, orderBy, topicId);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:
                        if (topicId == 0)
                            return dbContext.Events.Where(c => c.Qbicle.Id == cubeId && c.Topic != null)
                                .OrderByDescending(d => d.TimeLineDate).ToList();
                        return dbContext.Events.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId)
                            .OrderByDescending(d => d.TimeLineDate).ToList();
                    case Enums.OrderByDate.OldestFirst:
                        if (topicId == 0)
                            return dbContext.Events.Where(c => c.Qbicle.Id == cubeId && c.Topic != null)
                                .OrderBy(d => d.TimeLineDate).ToList();
                        return dbContext.Events.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId)
                            .OrderBy(d => d.TimeLineDate).ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, topicId);
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

            return new List<QbicleEvent>();
        }

        public bool DuplicateEventNameCheck(int cubeId, int eventId, string eventName)
        {
            bool exists;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate event name", null, null, cubeId, eventId, eventName);

                if (eventId > 0)
                    exists = dbContext.Activities.Any(x =>
                        x.Id != eventId && x.Qbicle.Id == cubeId && x.Name == eventName);
                else
                    exists = dbContext.Activities.Any(x => x.Qbicle.Id == cubeId && x.Name == eventName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, eventId, eventName);
                exists = false;
            }

            return exists;
        }

        public ReturnJsonModel SaveEvent(QbicleEvent qEvent, string eventStart, int qbicleId,
            string[] sendInvitesTo, int[] activitiesRelate, MediaModel mediaModel, string userId, int topicId, QbicleRecurrance qbicleRecurance, List<CustomDateModel> lstDate,
            string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            if (eventStart == null) throw new ArgumentNullException(nameof(eventStart));

            var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);

            var result = new ReturnJsonModel();
            var currentInvite = new QbiclePeople();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save event", userId, null, qEvent, eventStart, qbicleId, sendInvitesTo, activitiesRelate, mediaModel, topicId, qbicleRecurance, lstDate);
                var currentUser = dbContext.QbicleUser.Find(userId);
                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                qEvent.Topic = topic;
                qEvent.StartedBy = currentUser;
                qEvent.StartedDate = DateTime.UtcNow;
                qEvent.State = QbicleActivity.ActivityStateEnum.Open;
                if (qEvent.Id == 0)
                {
                    qbicle.LastUpdated = DateTime.UtcNow;
                    qEvent.Qbicle = qbicle;
                }
                qEvent.ActivityType = QbicleActivity.ActivityTypeEnum.EventActivity;
                var dateStart = qEvent.Start;
                switch (qEvent.DurationUnit)
                {
                    case QbicleEvent.EventDurationUnitEnum.Days:
                        qEvent.End = dateStart.AddDays(qEvent.Duration);
                        break;
                    case QbicleEvent.EventDurationUnitEnum.Hours:
                        qEvent.End = dateStart.AddHours(qEvent.Duration);
                        break;
                    default:
                        qEvent.End = dateStart.AddDays(qEvent.Duration * 7);
                        break;
                }

                qEvent.TimeLineDate = DateTime.UtcNow;
                qEvent.ProgrammedStart = qEvent.Start;
                qEvent.ProgrammedEnd = qEvent.End;
                QbicleMedia eventMedia = null;
                if (!string.IsNullOrEmpty(mediaModel.Name))
                {
                    if (!string.IsNullOrEmpty(mediaModel.UrlGuid))
                        new Azure.AzureStorageRules(dbContext).ProcessingMediaS3(mediaModel.UrlGuid);

                    //Media attach
                    eventMedia = new QbicleMedia
                    {
                        StartedBy = currentUser,
                        StartedDate = DateTime.UtcNow,
                        Name = mediaModel.Name,
                        FileType = mediaModel.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, qbicleId),
                        Topic = topic,
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
                    eventMedia.VersionedFiles.Add(versionFile);
                    eventMedia.ActivityMembers.Add(currentUser);
                }

                QbicleSet qbicleSet = null;
                QbicleEvent dbEvent = null;
                var eventNotify = NotificationEventEnum.EventCreation;
                if (qEvent.Id == 0)
                {
                    qbicleSet = new QbicleSet();
                    qEvent.AssociatedSet = qbicleSet;
                    qEvent.TimeLineDate = DateTime.UtcNow;
                    qEvent.IsVisibleInQbicleDashboard = true;
                    if (eventMedia != null)
                        qEvent.SubActivities.Add(eventMedia);

                    dbContext.Events.Add(qEvent);
                    dbContext.Entry(qEvent).State = EntityState.Added;
                    dbContext.SaveChanges();
                    dbEvent = qEvent;
                    result.actionVal = qEvent.Id;
                }
                else
                {
                    dbEvent = dbContext.Events.Find(qEvent.Id);
                    var job = new QbicleJobParameter
                    {
                        EndPointName = "deletehangfirejobstate",
                        JobId = dbEvent.JobId,
                    };
                    Task tskHangfire = new Task(async () =>
                    {
                        await new QbiclesJob().HangFireExcecuteAsync(job);
                    });
                    tskHangfire.Start();

                    dbEvent.Name = qEvent.Name;
                    dbEvent.Description = qEvent.Description;
                    dbEvent.Topic = qEvent.Topic;
                    dbEvent.EventType = qEvent.EventType;
                    dbEvent.Location = qEvent.Location;
                    dbEvent.Start = qEvent.Start;
                    dbEvent.End = qEvent.End;
                    dbEvent.Duration = qEvent.Duration;
                    dbEvent.DurationUnit = qEvent.DurationUnit;
                    dbEvent.ProgrammedStart = qEvent.Start;
                    dbEvent.ProgrammedEnd = qEvent.End;
                    dbEvent.isRecurs = qEvent.isRecurs;
                    if (eventMedia != null)
                        dbEvent.SubActivities.Add(eventMedia);

                    if (dbEvent.AssociatedSet != null)
                        qbicleSet = dbEvent.AssociatedSet;
                    else
                    {
                        qbicleSet = new QbicleSet();
                        dbContext.Sets.Add(qbicleSet);
                        dbContext.Entry(qbicleSet).State = EntityState.Added;
                        dbContext.SaveChanges();
                        dbEvent.AssociatedSet = qbicleSet;
                    }

                    if (dbContext.Entry(dbEvent).State == EntityState.Detached)
                        dbContext.Events.Attach(dbEvent);
                    dbContext.Entry(dbEvent).State = EntityState.Modified;

                    //Update LastUpdated currentDomain
                    var currentDomain = dbContext.Qbicles.Find(qbicleId);
                    if (currentDomain != null)
                    {
                        currentDomain.LastUpdated = DateTime.UtcNow;
                        if (dbContext.Entry(currentDomain).State == EntityState.Detached)
                            dbContext.Qbicles.Attach(currentDomain);
                        dbContext.Entry(currentDomain).State = EntityState.Modified;
                    }
                    //end
                    eventNotify = NotificationEventEnum.EventUpdate;
                }
                #region Peobles
                var _activityMembers = new List<ApplicationUser>();
                _activityMembers.Add(currentUser);
                dbEvent.ActivityMembers.Clear();
                //Remove Watchers Old
                var peoplesInvite = dbContext.People.Where(s => s.AssociatedSet.Id == qbicleSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Invitee).ToList();
                if (peoplesInvite.Count > 0)
                {
                    dbContext.People.RemoveRange(peoplesInvite);
                }
                //Add current User
                currentInvite.isPresent = true;
                currentInvite.Type = QbiclePeople.PeopleTypeEnum.Invitee;
                currentInvite.User = currentUser;
                currentInvite.AssociatedSet = qbicleSet;
                dbContext.People.Add(currentInvite);
                dbContext.Entry(currentInvite).State = EntityState.Added;
                //end
                if (sendInvitesTo != null && sendInvitesTo.Any())
                {
                    foreach (var item in sendInvitesTo.Where(s => s != currentUser.Id))
                    {
                        var peopleInvite = new QbiclePeople
                        {
                            isPresent = true,
                            Type = QbiclePeople.PeopleTypeEnum.Invitee
                        };
                        var user = dbContext.QbicleUser.Find(item);
                        if (user == null) continue;
                        peopleInvite.User = user;
                        peopleInvite.AssociatedSet = qbicleSet;
                        dbContext.People.Add(peopleInvite);
                        dbContext.Entry(peopleInvite).State = EntityState.Added;
                        _activityMembers.Add(user);
                    }
                    dbEvent.ActivityMembers.AddRange(_activityMembers);
                }
                #endregion
                #region Related
                var relates = dbContext.Relateds.Where(s => s.AssociatedSet.Id == qbicleSet.Id).ToList();
                if (relates.Count > 0)
                {
                    dbContext.Relateds.RemoveRange(relates);
                }
                if (activitiesRelate != null && activitiesRelate.Length > 0)
                {
                    foreach (var item in activitiesRelate)
                    {
                        var activity = dbContext.Activities.Find(item);
                        if (activity == null) continue;
                        var rl = new QbicleRelated { AssociatedSet = qbicleSet, Activity = activity };
                        dbContext.Relateds.Add(rl);
                        dbContext.Entry(rl).State = EntityState.Added;
                    }
                }
                #endregion
                #region recurrance
                if (qEvent.isRecurs)
                {
                    if (qbicleRecurance != null)
                    {
                        qbicleRecurance.Id = qbicleSet.Id;
                        var recurrance = dbContext.Recurrances.Where(s => s.AssociatedSet.Id == qbicleSet.Id).ToList();
                        if (recurrance.Count > 0)
                        {
                            dbContext.Recurrances.RemoveRange(recurrance);
                        }

                        dbContext.Recurrances.Add(qbicleRecurance);
                        dbContext.Entry(qbicleRecurance).State = EntityState.Added;
                        foreach (var item in lstDate)
                        {
                            if (qEvent.ProgrammedStart == item.StartDate) continue;
                            var qEvent2 = new QbicleEvent
                            {
                                End = qEvent.End,
                                ActualEnd = qEvent.ActualEnd,
                                ActualStart = qEvent.ActualStart,
                                Description = qEvent.Description,
                                Duration = qEvent.Duration,
                                DurationUnit = qEvent.DurationUnit,
                                EventType = qEvent.EventType,
                                isRecurs = qEvent.isRecurs,
                                Location = qEvent.Location,
                                Name = qEvent.Name,
                                Start = item.StartDate,
                                StartedBy = qEvent.StartedBy,
                                StartedDate = item.StartDate,
                                State = qEvent.State,
                                Topic = topic,
                                Qbicle = qEvent.Qbicle,
                                TimeLineDate = qEvent.TimeLineDate,
                                ActivityType = QbicleActivity.ActivityTypeEnum.EventActivity,
                                IsVisibleInQbicleDashboard = false
                            };

                            dateStart = item.StartDate;
                            switch (qEvent2.DurationUnit)
                            {
                                case QbicleEvent.EventDurationUnitEnum.Days:
                                    qEvent2.End = dateStart.AddDays(qEvent.Duration);
                                    break;
                                case QbicleEvent.EventDurationUnitEnum.Hours:
                                    qEvent2.End = dateStart.AddHours(qEvent.Duration);
                                    break;
                                default:
                                    qEvent2.End = dateStart.AddDays(qEvent.Duration * 7);
                                    break;
                            }
                            qEvent2.ProgrammedStart = qEvent2.Start;
                            qEvent2.ProgrammedEnd = qEvent2.End;
                            if (!string.IsNullOrEmpty(mediaModel.Name))
                            {
                                //Media attach
                                eventMedia = new QbicleMedia
                                {
                                    StartedBy = currentUser,
                                    StartedDate = DateTime.UtcNow,
                                    Name = qEvent.Name,
                                    Description = qEvent.Description,
                                    FileType = mediaModel.Type,
                                    Qbicle = qbicle,
                                    TimeLineDate = DateTime.UtcNow,
                                    MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, qbicleId),
                                    Topic = qEvent.Topic,
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
                                eventMedia.VersionedFiles.Add(versionFile);
                                eventMedia.ActivityMembers.Add(currentUser);
                                qEvent2.SubActivities.Add(eventMedia);
                            }
                            if (_activityMembers.Any())
                                qEvent2.ActivityMembers.AddRange(_activityMembers);
                            qEvent2.AssociatedSet = qbicleSet;
                            dbContext.Events.Add(qEvent2);
                            dbContext.Entry(qEvent2).State = EntityState.Added;
                        }
                    }
                }

                #endregion
                dbContext.SaveChanges();

                result.actionVal = dbEvent.Id;
                result.result = true;
                result.Object = new { topic = new { topic.Id, topic.Name } };
                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = qEvent.Id,
                    EventNotify = eventNotify,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);

                var reminderMinutes = (dateStart.AddHours(-24) - DateTime.UtcNow).TotalMinutes;
                activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = qEvent.Id,
                    EventNotify = NotificationEventEnum.EventNotificationPoints,
                    AppendToPageName = ApplicationPageName.All,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = reminderMinutes <= 0 ? 0 : reminderMinutes,

                };
                nRule.Notification2EventTaskPoints(activityNotification, dbEvent);

                if (!string.IsNullOrEmpty(originatingConnectionId))
                {
                    var notification = new Notification
                    {
                        AssociatedQbicle = qEvent.Qbicle,
                        CreatedBy = qEvent.StartedBy,
                        IsCreatorTheCustomer = qEvent.IsCreatorTheCustomer,
                    };
                    
                    if (appType == AppType.Web)
                        result.msg = new ISignalRNotification().HtmlRender(qEvent, currentUser, ApplicationPageName.Activities, NotificationEventEnum.EventCreation, notification);
                    else
                        result.Object = MicroStreamRules.GenerateActivity(qEvent, qEvent.StartedDate, null, currentUser.Id, currentUser.DateFormat, currentUser.Timezone, false, NotificationEventEnum.EventCreation, notification);
                }
                dbContext.Entry(qEvent).State = EntityState.Unchanged;


                return result;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, qEvent, eventStart, qbicleId, sendInvitesTo, activitiesRelate, mediaModel, topicId, qbicleRecurance, lstDate);
                return result;
            }
        }
        public int CountCanEventsDelete(int eventId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count all event can be deleted", null, null, eventId);

                var dbEvent = dbContext.Events.Find(eventId);
                if (dbEvent != null)
                {
                    return dbEvent.AssociatedSet.Activities.Count(s => s.Id != eventId && s.ProgrammedStart >= DateTime.UtcNow);
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, eventId);
                return 0;
            }
        }
        public ReturnJsonModel RecurringEventsDelete(int eventId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Recurring events delete", null, null, eventId);

                var countEventDelete = 0;
                var dbEvent = dbContext.Events.Find(eventId);
                if (dbEvent == null) return refModel;
                var activities = dbEvent.AssociatedSet.Activities.Where(s => s.Id != eventId && s.ProgrammedStart >= DateTime.UtcNow).ToList();
                foreach (var item in activities)
                {
                    var tsk = (QbicleEvent)item;
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

                        //Remove Comments
                        if (tsk.Posts != null && tsk.Posts.Count > 0)
                            dbContext.Posts.RemoveRange(tsk.Posts);
                        //Remove Notify
                        var notifys = dbContext.Notifications.Where(s => s.AssociatedAcitvity.Id == tsk.Id).ToList();
                        if (notifys != null && notifys.Count > 0)
                        {
                            dbContext.Notifications.RemoveRange(notifys);
                        }
                        //Remove Activity Members
                        tsk.ActivityMembers.Clear();
                        //Remove Mypins
                        var pins = dbContext.MyPins.Where(s => s.PinnedActivity.Id == tsk.Id);
                        if (pins != null && pins.Any())
                            dbContext.MyPins.RemoveRange(pins);
                        //Remove Event
                        dbContext.Events.Remove(tsk);
                        if (dbContext.SaveChanges() > 0)
                        {
                            countEventDelete += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                        continue;
                    }
                }
                var recurrence = dbEvent.AssociatedSet.Recurrance;
                if (recurrence != null)
                    dbContext.Recurrances.Remove(recurrence);
                dbEvent.isRecurs = false;
                if (dbContext.Entry(dbEvent).State == EntityState.Detached)
                    dbContext.Events.Attach(dbEvent);
                dbContext.Entry(dbEvent).State = EntityState.Modified;
                refModel.result = dbContext.SaveChanges() > 0;
                refModel.Object = new { countEventDelete };
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, eventId);
                refModel.msg = ex.Message;
                return refModel;
            }
        }
        public IEnumerable<DateTime> GetEventsDate(List<QbicleEvent> qbicleEvent)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get events date", null, null, qbicleEvent);

                var eventDates = from t in qbicleEvent select t.TimeLineDate.Date;
                return eventDates.Distinct();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleEvent);
                return new List<DateTime>();
            }

        }

        public IEnumerable<DateTime> LoadMoreEvents(int cubeId, int size,
            ref List<QbicleEvent> events, ref int acivitiesDateCount, string currentTimeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more events", null, null, cubeId, size, events, acivitiesDateCount, currentTimeZone);

                IEnumerable<DateTime> activitiesDate = null;

                var qbicleEvents = new List<QbicleEvent>();

                qbicleEvents = GetEventsByQbicleId(cubeId).BusinessMapping(currentTimeZone);

                events = qbicleEvents;
                var taskDates = from t in qbicleEvents select t.TimeLineDate.Date;

                var disDates = taskDates;
                acivitiesDateCount = disDates.Count();

                disDates = disDates.Distinct().OrderByDescending(d => d.Date.Date);
                activitiesDate = disDates.OrderByDescending(d => d.Date).Skip(size).Take(HelperClass.qbiclePageSize);

                events = qbicleEvents.Where(o => activitiesDate.Contains(o.TimeLineDate.Date)).ToList();
                return activitiesDate;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, size, events, acivitiesDateCount, currentTimeZone);
                return new List<DateTime>();
            }

        }

        public List<QbicleEvent> GetEventsByQbicleId(int cubeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get events by qbicle id", null, null, cubeId);

                var qbicleEvent = dbContext.Events.Where(c => c.Qbicle.Id == cubeId).ToList();
                return qbicleEvent;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId);
                return new List<QbicleEvent>();
            }
        }
        public QbicleEvent GetEventById(int eventId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get event by id", null, null, eventId);

                return dbContext.Events.Find(eventId) ?? new QbicleEvent();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, eventId);
                return new QbicleEvent();
            }
        }

        public bool CannotAttend(int eventId, string userId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Cannot attend", userId, null, eventId);
                var currentUser = dbContext.QbicleUser.Find(userId);
                if (currentUser != null && eventId > 0)
                {
                    var qEvent = GetEventById(eventId);
                    qEvent.ActivityMembers.Remove(currentUser);
                    qEvent.TimeLineDate = DateTime.UtcNow;
                    qEvent.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.EventWithdrawl;
                    qEvent.Qbicle.LastUpdated = DateTime.UtcNow;
                    dbContext.SaveChanges();

                    var nRule = new NotificationRules(dbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = qEvent.Id,
                        EventNotify = NotificationEventEnum.EventWithdrawl,
                        AppendToPageName = ApplicationPageName.Activities,
                        CreatedByName = HelperClass.GetFullNameOfUser(currentUser),
                        CreatedById = userId,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2UpdateStatusApprovalCannotAttendEventCloseTask(activityNotification);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, eventId);
            }
            return false;
        }
        public ReturnJsonModel AddPostToEvent(int eventId, QbiclePost post, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add post to event", post.CreatedBy.Id, null, eventId, post);

                var qEvent = GetEventById(eventId);
                qEvent.TimeLineDate = DateTime.UtcNow;
                qEvent.IsVisibleInQbicleDashboard = true;
                qEvent.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                if (qEvent.AssociatedSet != null)
                    post.Set = qEvent.AssociatedSet;
                qEvent.Posts.Add(post);
                //var startedBy = qEvent.StartedBy;
                qEvent.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.Events.Attach(qEvent);
                dbContext.Entry(qEvent).State = EntityState.Modified;
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = eventId,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.EventUpdate,
                    AppendToPageName = ApplicationPageName.Event,
                    AppendToPageId = eventId,
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
                        result.msg = new ISignalRNotification().HtmlRender(post, post.CreatedBy, ApplicationPageName.Event, NotificationEventEnum.EventUpdate, notification);
                    else
                        result.Object = MicroStreamRules.GenerateActivity(post, post.StartedDate, null, post.CreatedBy.Id, post.CreatedBy.DateFormat, post.CreatedBy.Timezone, false, NotificationEventEnum.EventUpdate, notification);
                }

                dbContext.Entry(post).State = EntityState.Unchanged;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, post.CreatedBy.Id, eventId, post);
                return new ReturnJsonModel { result = false };
            }
        }
        public bool UpdateAttend(int peopleId, bool isPresent)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update attend", null, null, peopleId, isPresent);

                var people = dbContext.People.Find(peopleId);
                if (people != null)
                {
                    people.isPresent = isPresent;
                    if (dbContext.Entry(people).State == EntityState.Detached)
                        dbContext.People.Attach(people);
                    dbContext.Entry(people).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, peopleId, isPresent);
                return false;
            }
        }
    }
}