using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleTask;

namespace Qbicles.BusinessRules
{
    public class SocialLocationRule
    {
        #region init class
        private ApplicationDbContext _db;
        private readonly string _currentTimeZone = "";

        public SocialLocationRule()
        {
        }
        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }
        public SocialLocationRule(ApplicationDbContext context, string currentTimeZone = "")
        {
            _db = context;
            _currentTimeZone = currentTimeZone;
        }
        #endregion

        public Place GetPlaceByActivityId(int activityId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get place by activity id", null, null, activityId);

                return DbContext.SMPlaces.FirstOrDefault(s => s.Discussion.Id == activityId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activityId);
                return null;
            }
        }
        public Area GetAreaById(int areaId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get area by id", null, null, areaId);

                return DbContext.SMAreas.Find(areaId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, areaId);
                return null;
            }
        }

        public ReturnJsonModel ShowOrHideArea(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide area", null, null, id);

                var area = DbContext.SMAreas.FirstOrDefault(s => s.Id == id);
                area.IsHidden = !area.IsHidden;
                if (DbContext.Entry(area).State == EntityState.Detached)
                    DbContext.SMAreas.Attach(area);
                DbContext.Entry(area).State = EntityState.Modified;
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }

        public Place GetPlaceById(int areaId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get place by id", null, null, areaId);

                return DbContext.SMPlaces.Find(areaId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, areaId);
                return null;
            }

        }

        public ReturnJsonModel ShowOrHidePlace(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide place", null, null, id);

                var place = DbContext.SMPlaces.FirstOrDefault(s => s.Id == id);
                place.IsHidden = !place.IsHidden;
                if (DbContext.Entry(place).State == EntityState.Detached)
                    DbContext.SMPlaces.Attach(place);
                DbContext.Entry(place).State = EntityState.Modified;
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }

        public List<Area> GetListArea(string name, int domainId, int skip, int take, bool isLoadingHide = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list area", null, null, name, domainId, skip, take, isLoadingHide);

                return DbContext.SMAreas.Where(p => p.Domain.Id == domainId && (isLoadingHide || (!isLoadingHide && !p.IsHidden)) && (name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()))).OrderByDescending(p => p.Id).Skip(skip).Take(take).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, domainId, skip, take, isLoadingHide);
                return new List<Area>();
            }
        }

        public int CountListArea(string name, int domainId, bool isLoadingHide)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count list area", null, null, name, domainId, isLoadingHide);

                return DbContext.SMAreas.Where(p => p.Domain.Id == domainId && (isLoadingHide || (!isLoadingHide && !p.IsHidden)) && (name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()))).Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, domainId, isLoadingHide);
                return 0;
            }
        }

        public List<Place> GetListPlace(string name, int domainId, int areaId, int skip, int take, bool isLoadingHide = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list place", null, null, name, domainId, skip, take, isLoadingHide);

                if (areaId == 0)
                {
                    return DbContext.SMPlaces.Where(p => p.Domain.Id == domainId && (isLoadingHide || (!isLoadingHide && !p.IsHidden)) && (name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()))).OrderByDescending(p => p.Id).Skip(skip).Take(take).ToList();
                }
                else
                {
                    var listPlaces = from p in DbContext.SMPlaces
                                     where p.Domain.Id == domainId && (isLoadingHide || (!isLoadingHide && !p.IsHidden)) && (name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()))
                                     && p.Areas.Any(a => a.Id == areaId)
                                     orderby p.Id descending
                                     select p;
                    return listPlaces.ToList();
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, domainId, skip, take, isLoadingHide);
                return new List<Place>();
            }
        }

        public int CountListPlace(string name, int domainId, int areaId, bool isLoadingHide)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count list place", null, null, name, domainId, areaId, isLoadingHide);

                var query = DbContext.SMPlaces.Where(p => p.Domain.Id == domainId);
                if (!isLoadingHide)
                {
                    query = query.Where(s => !s.IsHidden);
                }
                if (areaId == 0)
                {
                    query = query.Where(p => name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()));
                }
                else
                {
                    query = query.Where(p => name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()));
                }
                return query.Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, domainId, areaId, isLoadingHide);
                return 0;
            }
        }

        /// <summary>
        ///     validate duplicate Area name
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="areaName"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public bool DuplicateAreaNameCheck(int areaId, string areaName, int domainId)
        {
            bool exist;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate area name", null, null, areaId, areaName, domainId);

                var lstArea = DbContext.SMAreas.Where(p => p.Domain.Id == domainId).ToList();
                if (lstArea != null && lstArea.Any())
                {
                    exist = areaId > 0 ? lstArea.Any(x => x.Id != areaId && x.Name.Trim() == areaName.Trim()) : lstArea.Any(x => x.Name.Trim() == areaName.Trim());
                }
                else
                    exist = false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, areaId, areaName, domainId);
                exist = false;
            }

            return exist;
        }

        /// <summary>
        ///     validate duplicate Place name
        /// </summary>
        /// <param name="placeId"></param>
        /// <param name="areaName"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public bool DuplicatePlaceNameCheck(int placeId, string placeName, int domainId)
        {
            bool exist;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate place name", null, null, placeId, placeName, domainId);

                var lstPlace = DbContext.SMAreas.Where(p => p.Domain.Id == domainId).ToList();
                if (lstPlace != null && lstPlace.Any())
                {
                    exist = placeId > 0 ? lstPlace.Any(x => x.Id != placeId && x.Name.Trim() == placeName.Trim()) : lstPlace.Any(x => x.Name.Trim() == placeName.Trim());
                }
                else
                    exist = false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, placeId, placeName, domainId);
                exist = false;
            }

            return exist;
        }

        public ReturnJsonModel SaveSMPlaceArea(Area area, string userId, int[] AssociatePlaces)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save area", userId, null, area, AssociatePlaces);

                if (!string.IsNullOrEmpty(area.FeaturedImageUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(area.FeaturedImageUri);

                }

                var socialLocationRule = new SocialLocationRule(DbContext);
                List<Place> listPlaces = null;
                if (AssociatePlaces != null)
                {
                    listPlaces = DbContext.SMPlaces.Where(p => AssociatePlaces.Contains(p.Id)).ToList();
                }
                var user = DbContext.QbicleUser.Find(userId);
                Area mdArea = DbContext.SMAreas.Find(area.Id);
                if (mdArea != null)
                {
                    mdArea.Name = area.Name;
                    mdArea.FeaturedImageUri = string.IsNullOrEmpty(area.FeaturedImageUri) ? mdArea.FeaturedImageUri : area.FeaturedImageUri;
                    mdArea.Places.Clear();
                    if (listPlaces != null) mdArea.Places.AddRange(listPlaces);
                    mdArea.LastUpdatedBy = user;
                    mdArea.LastUpdateDate = DateTime.UtcNow;
                    if (DbContext.Entry(mdArea).State == EntityState.Detached)
                        DbContext.SMAreas.Attach(mdArea);
                    DbContext.Entry(mdArea).State = EntityState.Modified;
                }
                else
                {
                    if (listPlaces != null) area.Places.AddRange(listPlaces);
                    area.CreatedDate = DateTime.UtcNow;
                    area.LastUpdatedBy = user;
                    area.LastUpdateDate = area.CreatedDate;
                    area.CreatedBy = user;
                    DbContext.SMAreas.Add(area);
                    DbContext.Entry(area).State = EntityState.Added;
                }
                refModel.result = DbContext.SaveChanges() > 0 ? true : false;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, area, AssociatePlaces);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel SavePlace(Place place, string userId, int[] AssociateAreas)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save place", userId, null, place, AssociateAreas);

                if (!string.IsNullOrEmpty(place.FeaturedImageUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(place.FeaturedImageUri);

                }

                var socialLocationRule = new SocialLocationRule(DbContext);
                List<Area> listAreas = null;
                if (AssociateAreas != null)
                {
                    listAreas = DbContext.SMAreas.Where(p => AssociateAreas.Contains(p.Id)).ToList();
                }
                Place mdPlace = DbContext.SMPlaces.Find(place.Id);
                var user = DbContext.QbicleUser.Find(userId);
                if (mdPlace != null)
                {
                    mdPlace.Name = place.Name;
                    mdPlace.Prospects = place.Prospects;
                    mdPlace.Summary = place.Summary;
                    mdPlace.FeaturedImageUri = string.IsNullOrEmpty(place.FeaturedImageUri) ? mdPlace.FeaturedImageUri : place.FeaturedImageUri;
                    mdPlace.Areas.Clear();
                    if (listAreas != null) mdPlace.Areas.AddRange(listAreas);
                    mdPlace.LastUpdatedBy = user;
                    mdPlace.LastUpdateDate = DateTime.UtcNow;
                    if (DbContext.Entry(mdPlace).State == EntityState.Detached)
                        DbContext.SMPlaces.Attach(mdPlace);
                    DbContext.Entry(mdPlace).State = EntityState.Modified;
                }
                else
                {
                    if (listAreas != null) place.Areas.AddRange(listAreas);
                    place.CreatedDate = DateTime.UtcNow;
                    place.LastUpdatedBy = user;
                    place.LastUpdateDate = place.CreatedDate;
                    place.CreatedBy = user;
                    DbContext.SMPlaces.Add(place);
                    DbContext.Entry(place).State = EntityState.Added;
                }
                refModel.result = DbContext.SaveChanges() > 0 ? true : false;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, place, AssociateAreas);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel SaveVisitLogs(VisitLogModel visitLogs, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save visit logs", userId, null, visitLogs);

                QbicleTask associatedTask = visitLogs.TaskId != 0 ? DbContext.QbicleTasks.FirstOrDefault(q => q.Id == visitLogs.TaskId) : null;
                Place place = DbContext.SMPlaces.FirstOrDefault(p => p.Id == visitLogs.placeId);
                var user = DbContext.QbicleUser.Find(userId);
                var visit = new Visit
                {
                    Notes = visitLogs.Notes,
                    Reason = visitLogs.Reason,
                    Agent = user,
                    AssociatedTask = associatedTask,
                    NumberOfGeneratedLeads = visitLogs.Leads,
                    VisitDate = visitLogs.VisitDate,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow,
                    CreatedBy = user,
                    LastUpdatedBy = user,
                    Place = place
                };

                DbContext.SMVisits.Add(visit);
                DbContext.Entry(visit).State = EntityState.Added;
                DbContext.SaveChanges();


                if (place != null)
                {
                    var maxId = DbContext.SMVisits.Max(v => v.Id);
                    Visit newVisit = DbContext.SMVisits.FirstOrDefault(v => v.Id == maxId);
                    place.Visits.Add(newVisit);

                    if (DbContext.Entry(place).State == EntityState.Detached)
                        DbContext.SMPlaces.Attach(place);
                    DbContext.Entry(place).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, visitLogs);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel SaveActivityLogs(PlaceActivityModel activityLogs, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save activity logs", userId, null, activityLogs);

                Place place = DbContext.SMPlaces.FirstOrDefault(p => p.Id == activityLogs.placeId);
                PlaceActivity placeActivity = new PlaceActivity
                {
                    RecordedFootfall = activityLogs.Recorded,
                    Notes = activityLogs.Notes,
                    StartDate = activityLogs.StartDate,
                    EndDate = activityLogs.EndDate,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = DbContext.QbicleUser.Find(userId),
                    Place = place
                };


                DbContext.SMPlaceActivitys.Add(placeActivity);
                DbContext.Entry(placeActivity).State = EntityState.Added;
                DbContext.SaveChanges();


                if (place != null)
                {
                    var maxId = DbContext.SMPlaceActivitys.Max(v => v.Id);
                    PlaceActivity newActivity = DbContext.SMPlaceActivitys.FirstOrDefault(v => v.Id == maxId);
                    place.PlaceActivities.Add(newActivity);

                    if (DbContext.Entry(place).State == EntityState.Detached)
                        DbContext.SMPlaces.Attach(place);
                    DbContext.Entry(place).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }

                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, activityLogs);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public bool SaveSMLocationQbicleTask(QbicleTask task, string assignee,
            MediaModel media, string[] watchers, int cubeId,
            string userId, int topicId, int[] activitiesRelate, List<QbicleStep> stepLst,
            QbicleRecurrance qbicleRecurrance, List<CustomDateModel> lstDate, long placeId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save task", userId, null, task, assignee, media, watchers, cubeId, topicId, activitiesRelate, stepLst, qbicleRecurrance, lstDate, placeId);

                var currentUser = DbContext.QbicleUser.Find(userId);

                var qbicle = new QbicleRules(DbContext).GetQbicleById(cubeId);
                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var topic = new TopicRules(DbContext).GetTopicById(topicId);
                task.Topic = topic;
                //var uRules = new UserRules(DbContext);
                task.StartedBy = currentUser;
                task.StartedDate = DateTime.UtcNow;
                task.State = QbicleActivity.ActivityStateEnum.Open;
                if (task.Id == 0)
                {
                    qbicle.LastUpdated = DateTime.UtcNow;
                    task.Qbicle = qbicle;
                }

                task.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                if (!task.ProgrammedStart.HasValue)
                    task.ProgrammedStart = DateTime.UtcNow;
                task.TimeLineDate = DateTime.UtcNow;
                if (task.DurationUnit == QbicleTask.TaskDurationUnitEnum.Days)
                    task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration);
                else if (task.DurationUnit == QbicleTask.TaskDurationUnitEnum.Hours)
                    task.ProgrammedEnd = task.ProgrammedStart.Value.AddHours(task.Duration);
                else
                    task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration * 7);
                QbicleMedia m = null;
                if (!string.IsNullOrEmpty(media.Name))
                {
                    //Media attach
                    m = new QbicleMedia
                    {
                        StartedBy = currentUser,
                        StartedDate = DateTime.UtcNow,
                        Name = task.Name,
                        FileType = media.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = task.Topic,

                        MediaFolder =
                            new MediaFolderRules(DbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
                        Description = task.Description,
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = currentUser,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    m.VersionedFiles.Add(versionFile);
                    m.ActivityMembers.Add(currentUser);

                    DbContext.Medias.Add(m);
                    DbContext.Entry(m).State = EntityState.Added;

                    task.SubActivities.Add(m);
                }

                #region Steps

                var steps = DbContext.Steps.Where(s => s.ActivityId == task.Id).ToList();
                if (steps.Count > 0)
                    foreach (var item in steps)
                    {
                        var sti = item.StepInstance.FirstOrDefault(s => s.Step.Id == item.Id && s.Task.Id == task.Id);
                        if (sti != null)
                            DbContext.Stepinstances.Remove(sti);
                        DbContext.Steps.Remove(item);
                    }

                if (task.isSteps)
                    foreach (var item in stepLst)
                    {
                        if (task.Id > 0)
                            item.ActivityId = task.Id;
                        DbContext.Steps.Add(item);
                        DbContext.Entry(item).State = EntityState.Added;
                        task.QSteps.Add(item);
                    }

                #endregion

                QbicleSet set;

                if (task.Id == 0)
                {
                    set = new QbicleSet();
                    task.AssociatedSet = set;
                    task.App = QbicleActivity.ActivityApp.SalesAndMarketing;
                    DbContext.QbicleTasks.Add(task);
                    DbContext.Entry(task).State = EntityState.Added;
                    DbContext.SaveChanges();
                }
                else
                {
                    var dbtask = DbContext.QbicleTasks.Find(task.Id);
                    dbtask.Topic = task.Topic;
                    dbtask.Name = task.Name;
                    dbtask.Description = task.Description;
                    dbtask.Duration = task.Duration;
                    dbtask.DurationUnit = task.DurationUnit;
                    dbtask.Priority = task.Priority;
                    dbtask.ProgrammedStart = task.ProgrammedStart;
                    dbtask.ProgrammedEnd = task.ProgrammedEnd;
                    dbtask.isSteps = task.isSteps;
                    dbtask.isRecurs = task.isRecurs;
                    dbtask.QSteps = task.QSteps;
                    dbtask.IsVisibleInQbicleDashboard = true;
                    dbtask.TimeLineDate = DateTime.UtcNow;
                    if (m != null)
                        dbtask.SubActivities.Add(m);
                    if (dbtask != null && dbtask.AssociatedSet != null)
                    {
                        set = dbtask.AssociatedSet;
                        //var lstTaskOldRemove = DbContext.QbicleTasks.Where(s => s.AssociatedSet.Id == Set.Id).ToList();
                        //if (lstTaskOldRemove.Count > 0)
                        //{
                        //    DbContext.QbicleTasks.RemoveRange(lstTaskOldRemove.Where(p => p.Id != dbtask.Id));
                        //}
                    }
                    else
                    {
                        set = new QbicleSet();
                        DbContext.Sets.Add(set);
                        DbContext.Entry(set).State = EntityState.Added;
                        dbtask.AssociatedSet = set;
                    }

                    if (DbContext.Entry(dbtask).State == EntityState.Detached)
                        DbContext.QbicleTasks.Attach(dbtask);
                    DbContext.Entry(dbtask).State = EntityState.Modified;
                    //Update LastUpdated currentDomain
                    qbicle.LastUpdated = DateTime.UtcNow;

                    if (DbContext.Entry(qbicle).State == EntityState.Detached)
                        DbContext.Qbicles.Attach(qbicle);
                    DbContext.Entry(qbicle).State = EntityState.Modified;

                    //end
                }

                //link
                if (set.Id == 0)
                {
                    DbContext.Sets.Add(set);
                    DbContext.Entry(set).State = EntityState.Added;
                }

                #region Peoples
                var _activityMembers = new List<ApplicationUser>();
                _activityMembers.Add(currentUser);
                //Task People (People->Type 0 = Assignee);(People->Type 1 = Watcher)
                if (!string.IsNullOrEmpty(assignee))
                {
                    var peopleAssignee = DbContext.People.Where(s =>
                            s.AssociatedSet.Id == set.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee)
                        .FirstOrDefault();
                    if (peopleAssignee == null)
                    {
                        peopleAssignee = new QbiclePeople();
                        peopleAssignee.isPresent = true;
                        peopleAssignee.Type = QbiclePeople.PeopleTypeEnum.Assignee;
                        var user = DbContext.QbicleUser.Find(assignee);
                        if (user != null)
                        {
                            peopleAssignee.User = user;
                            peopleAssignee.AssociatedSet = set;
                            DbContext.People.Add(peopleAssignee);
                            DbContext.Entry(peopleAssignee).State = EntityState.Added;
                            _activityMembers.Add(user);
                        }
                    }
                    else if (peopleAssignee.User.Id != assignee)
                    {
                        var user = DbContext.QbicleUser.Find(assignee);
                        if (user != null)
                        {
                            peopleAssignee.User = user;
                            //DbContext.People.Add(peopleAssignee);
                            if (DbContext.Entry(peopleAssignee).State == EntityState.Detached)
                                DbContext.People.Attach(peopleAssignee);
                            DbContext.Entry(peopleAssignee).State = EntityState.Modified;
                            _activityMembers.Add(user);
                        }
                    }
                }

                //Remove Watchers Old
                var peoplesWatch = DbContext.People.Where(s =>
                    s.AssociatedSet.Id == set.Id && s.Type == QbiclePeople.PeopleTypeEnum.Watcher).ToList();
                if (peoplesWatch.Count > 0) DbContext.People.RemoveRange(peoplesWatch);
                if (watchers != null && watchers.Any())
                    foreach (var item in watchers)
                        if (item != assignee)
                        {
                            var peopleWatcher = new QbiclePeople
                            {
                                isPresent = true,
                                Type = QbiclePeople.PeopleTypeEnum.Watcher
                            };
                            var user = DbContext.QbicleUser.Find(item);
                            if (user != null)
                            {
                                peopleWatcher.User = user;
                                peopleWatcher.AssociatedSet = set;
                                DbContext.People.Add(peopleWatcher);
                                DbContext.Entry(peopleWatcher).State = EntityState.Added;
                                _activityMembers.Add(user);
                            }
                        }
                if (_activityMembers.Any())
                {
                    var dbtask = DbContext.QbicleTasks.Find(task.Id);
                    dbtask.ActivityMembers.Clear();
                    dbtask.ActivityMembers.AddRange(_activityMembers);
                }
                #endregion

                #region Related

                var relates = DbContext.Relateds.Where(s => s.AssociatedSet.Id == set.Id).ToList();
                if (relates.Count > 0) DbContext.Relateds.RemoveRange(relates);
                if (activitiesRelate != null && activitiesRelate.Length > 0)
                    foreach (var item in activitiesRelate)
                    {
                        var activity = DbContext.Activities.Find(item);
                        if (activity != null)
                        {
                            var rl = new QbicleRelated { AssociatedSet = set, Activity = activity };
                            DbContext.Relateds.Add(rl);
                            DbContext.Entry(rl).State = EntityState.Added;
                        }
                    }

                #endregion

                #region recurrance

                //DbContext.SaveChanges();
                var recurrance = DbContext.Recurrances.Where(s => s.AssociatedSet.Id == set.Id).ToList();
                if (task.isRecurs)
                {
                    qbicleRecurrance.Id = set.Id;
                    if (recurrance.Count > 0) DbContext.Recurrances.RemoveRange(recurrance);
                    DbContext.Recurrances.Add(qbicleRecurrance);
                    DbContext.Entry(qbicleRecurrance).State = EntityState.Added;
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
                                App = QbicleActivity.ActivityApp.SalesAndMarketing,
                                IsVisibleInQbicleDashboard = false
                            };

                            qbicle.LastUpdated = DateTime.UtcNow;
                            task2.Qbicle = qbicle;
                            task2.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                            //task2.TimeLineDate = DateTime.UtcNow;
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
                                    DbContext.Steps.Add(step);
                                    DbContext.Entry(step).State = EntityState.Added;
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

                            if (!string.IsNullOrEmpty(media.Name))
                            {
                                //Media attach
                                m = new QbicleMedia
                                {
                                    StartedBy = currentUser,
                                    StartedDate = DateTime.UtcNow,
                                    Name = task.Name,
                                    FileType = media.Type,
                                    Qbicle = qbicle,
                                    TimeLineDate = DateTime.UtcNow,
                                    Topic = task.Topic,

                                    MediaFolder = new MediaFolderRules(DbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
                                    Description = task.Description,
                                    IsVisibleInQbicleDashboard = false
                                };
                                var versionFile = new VersionedFile
                                {
                                    IsDeleted = false,
                                    FileSize = media.Size,
                                    UploadedBy = currentUser,
                                    UploadedDate = DateTime.UtcNow,
                                    Uri = media.UrlGuid,
                                    FileType = media.Type
                                };
                                m.VersionedFiles.Add(versionFile);
                                m.ActivityMembers.Add(currentUser);

                                DbContext.Medias.Add(m);
                                DbContext.Entry(m).State = EntityState.Added;

                                task2.SubActivities.Add(m);
                            }

                            task2.AssociatedSet = set;
                            if (_activityMembers.Any())
                                task2.ActivityMembers.AddRange(_activityMembers);
                            DbContext.QbicleTasks.Add(task2);
                            DbContext.Entry(task2).State = EntityState.Added;
                        }
                }

                #endregion

                DbContext.SaveChanges();
                Place place = DbContext.SMPlaces.Find(placeId);
                if (place != null)
                {
                    place.Tasks.Add(task);
                }
                DbContext.SaveChanges();

                var nRule = new NotificationRules(DbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = task.Id,
                    EventNotify = NotificationEventEnum.TaskCreation,
                    AppendToPageName = ApplicationPageName.Link,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, task, assignee, media, watchers, cubeId, topicId, activitiesRelate, stepLst, qbicleRecurrance, lstDate, placeId);
                return false;
            }
        }

        public List<ScheduledVisitModel> GetAllScheduledVisits(int placeId, int start, int length, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get All scheduled visits", null, null, placeId, start, length, totalRecord);

                Place place = DbContext.SMPlaces.Find(placeId);
                totalRecord = place.Tasks.Count();
                List<ScheduledVisitModel> lstModel = new List<ScheduledVisitModel>();
                List<QbicleTask> lstQbicleTask = place.Tasks.OrderByDescending(t => t.Id).Skip(start).Take(length).ToList();

                foreach (QbicleTask task in lstQbicleTask)
                {
                    ScheduledVisitModel model = new ScheduledVisitModel();
                    model.Agent = HelperClass.GetFullNameOfUser(DbContext.People.Where(s => s.AssociatedSet.Id == task.AssociatedSet.Id).FirstOrDefault().User);

                    switch (task.DurationUnit)
                    {
                        case TaskDurationUnitEnum.Hours: model.Duration = task.Duration + " " + "Hours"; break;
                        case TaskDurationUnitEnum.Days: model.Duration = task.Duration + " " + "Days"; break;
                        case TaskDurationUnitEnum.Weeks: model.Duration = task.Duration + " " + "Weeks"; break;
                    }

                    if (!task.isComplete && task.ActualStart == null && task.ProgrammedEnd >= DateTime.UtcNow)
                    { model.Status = "Pending"; }
                    else if (!task.isComplete && task.ActualStart != null && task.ProgrammedEnd >= DateTime.UtcNow)
                    { model.Status = "In progress"; }
                    else if (!task.isComplete && task.ProgrammedEnd < DateTime.UtcNow)
                    { model.Status = "Overdue"; }
                    else if (task.isComplete)
                    { model.Status = "Complete"; }

                    if (task.ProgrammedStart != null)
                    {
                        model.DateTimeOfVisit = task.ProgrammedStart?.DatetimeToOrdinal().Replace(',', ' ') + ", " + task.ProgrammedStart?.ToString("hh:mmtt").ToLower();
                    }

                    lstModel.Add(model);
                }

                return lstModel.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, placeId, start, length, totalRecord);
                totalRecord = 0;
                return new List<ScheduledVisitModel>();
            }
        }

        public List<PlaceActivityModel> GetListActivityLog(int placeId, int start, int length, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list activity log", null, null, placeId, start, length, totalRecord);

                Place place = DbContext.SMPlaces.Find(placeId);
                totalRecord = place.PlaceActivities.Count();
                List<PlaceActivity> lstActivity = place.PlaceActivities.OrderByDescending(p => p.Id).Skip(start).Take(length).ToList();
                List<PlaceActivityModel> lstModel = new List<PlaceActivityModel>();

                foreach (PlaceActivity activity in lstActivity)
                {
                    PlaceActivityModel model = new PlaceActivityModel();
                    model.Agent = HelperClass.GetFullNameOfUser(activity.CreatedBy);
                    if (activity.StartDate != null)
                    {
                        model.Date = activity.StartDate.DatetimeToOrdinal().Replace(',', ' ');
                    }
                    TimeSpan span = activity.EndDate.Subtract(activity.StartDate);
                    model.Timeframe = activity.StartDate.ToString("hh:mmtt").ToLower() + " - " + activity.EndDate.ToString("hh:mmtt").ToLower() + " (" + span.Hours + " hrs " + span.Minutes + " m)";
                    model.Notes = activity.Notes;
                    model.Recorded = activity.RecordedFootfall;
                    lstModel.Add(model);
                }

                return lstModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, placeId, start, length, totalRecord);
                totalRecord = 0;
                return new List<PlaceActivityModel>();
            }
        }

        public List<VisitLogModel> GetListVisitLogs(int placeId, int start, int length, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list visit logs", null, null, placeId, start, length, totalRecord);

                Place place = DbContext.SMPlaces.Find(placeId);
                totalRecord = place.Visits.Count();
                List<Visit> lstVisit = place.Visits.OrderByDescending(v => v.Id).Skip(start).Take(length).ToList();
                List<VisitLogModel> lstModel = new List<VisitLogModel>();

                foreach (Visit visit in lstVisit)
                {
                    VisitLogModel model = new VisitLogModel();
                    model.TaskId = visit.AssociatedTask == null ? 0 : (long)visit.AssociatedTask.Id;
                    model.Agent = HelperClass.GetFullNameOfUser(visit.CreatedBy);
                    if (visit.VisitDate != null)
                    {
                        model.DateTimeOfVisit = visit.VisitDate.DatetimeToOrdinal().Replace(',', ' ') + ", " + visit.VisitDate.ToString("hh:mmtt").ToLower();
                    }
                    model.Notes = visit.Notes;
                    model.Leads = visit.NumberOfGeneratedLeads;
                    switch (visit.Reason)
                    {
                        case VisitReason.AssignedTask: model.txtReason = visit.AssociatedTask == null ? "Assigned Task" : visit.AssociatedTask.Name; break;
                        case VisitReason.AdvertisingStall: model.txtReason = "Stall"; break;
                        case VisitReason.ColdCalling: model.txtReason = "Cold Calling"; break;
                        case VisitReason.LeafletDistribution: model.txtReason = "Leaflet Distribution"; break;
                        case VisitReason.Other: model.txtReason = "Other"; break;
                    }
                    lstModel.Add(model);
                }

                return lstModel.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, placeId, start, length, totalRecord);
                totalRecord = 0;
                return new List<VisitLogModel>();
            }
        }
    }
}
