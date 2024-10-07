using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System.Linq.Dynamic;

namespace Qbicles.BusinessRules
{
    using Qbicles.BusinessRules.Helper;
    using Qbicles.Models.Trader;
    using System.Reflection;
    using System.Web.Script.Serialization;
    using static QbicleActivity;
    using static Qbicles.Models.ApprovalReq;

    public class MyDesksRules
    {
        private ApplicationDbContext _db;

        public MyDesksRules()
        {
        }

        public MyDesksRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }

        /// <summary>
        ///     Get my desk of current user
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <returns>MyDesk</returns>
        public MyDesk GetMyDesk(string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get my desk", null, null, currentUserId);

                return DbContext.MyDesks.First(u => u.Owner.Id == currentUserId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId);
                return null;
            }
        }

        /// <summary>
        ///     Pin or Un-Pin the Discussion
        /// </summary>
        /// <param name="ActivityId"></param>
        /// <param name="curentUserId"></param>
        /// <param name="isTopic"></param>
        /// <returns>bool</returns>
        public bool PinnedActivity(int ActivityId, string curentUserId, bool isTopic, int? mydeskId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "PinnedActivity", curentUserId, null, ActivityId, isTopic, mydeskId);

                var myDesk = mydeskId.HasValue ? DbContext.MyDesks.Find(mydeskId.Value) : GetMyDesk(curentUserId);
                if (myDesk == null)
                    myDesk = new MyDesk
                    {
                        Owner = new UserRules(DbContext).GetUser(curentUserId, 0)
                    };
                if (!isTopic)
                {
                    //check myPin, If exist then unpin else pin
                    //var myPin = myDesk.Pins.Find(d => d.PinnedActivity?.Id == ActivityId);
                    var myPin = DbContext.MyPins.FirstOrDefault(d => d.PinnedActivity.Id == ActivityId && d.Desk.Id == myDesk.Id);
                    if (myPin == null)
                    {
                        var pin = new MyPin
                        {
                            Desk = myDesk,
                            PinnedActivity = DbContext.Activities.Find(ActivityId)
                        };
                        DbContext.MyPins.Add(pin);
                        DbContext.SaveChanges();
                        return true;
                    }

                    if (myDesk.Id == 0)
                        return false;
                    DbContext.MyPins.Remove(myPin);
                    DbContext.SaveChanges();
                    return true;
                }
                else
                {
                    //check myPin, If exist then unpin else pin
                    //var myPin = myDesk.Pins.Find(d => d.PinnerPost?.Id == ActivityId);
                    var myPin = DbContext.MyPins.FirstOrDefault(d => d.PinnerPost.Id == ActivityId && d.Desk.Id == myDesk.Id);
                    if (myPin == null)
                    {
                        var pin = new MyPin
                        {
                            Desk = myDesk,
                            PinnerPost = DbContext.Posts.Find(ActivityId)
                        };
                        DbContext.MyPins.Add(pin);
                        DbContext.Entry(pin).State = EntityState.Added;
                        DbContext.SaveChanges();
                        return true;
                    }

                    if (myDesk.Id == 0)
                        return false;
                    DbContext.MyPins.Remove(myPin);
                    DbContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, ActivityId, isTopic, mydeskId);
                return false;
            }
        }

        public MyDesk CreateMyDesk(string OwnerId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create my desk", null, null, OwnerId);

                var myDesk = new MyDesk
                {
                    Owner = new UserRules(DbContext).GetUser(OwnerId, 0)
                };
                DbContext.MyDesks.Add(myDesk);
                DbContext.Entry(myDesk).State = EntityState.Added;
                DbContext.SaveChanges();
                return myDesk;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, OwnerId);
                return null;
            }
        }
        public List<MyTagCustom> GetAllTags(int myDeskId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all task", null, null, myDeskId);

                var lstTag = DbContext.MyFolders.Where(p => p.Desk.Id == myDeskId).Select(s => new MyTagCustom { Desk_Id = s.Desk.Id, Id = s.Id, Name = s.Name }).ToList();

                return lstTag;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, myDeskId);
                return null;
            }
        }
        /// <summary>
        ///     Delete a folder by folder id
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public bool DeleteFolder(int folderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete folder", null, null, folderId);

                var myFolder = DbContext.MyFolders.Find(folderId);
                DbContext.MyFolders.Remove(myFolder ?? throw new InvalidOperationException());
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, folderId);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderId">folderId = 0 then Add new, folderId >0 then Update</param>
        /// <param name="folderName"></param>
        /// <param name="activitiesCount"></param>
        /// <param name="curentUserId"></param>
        /// <returns></returns>
        public int SaveFolder(int folderId, string folderName, ref int activitiesCount, string curentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save folder", curentUserId, null, folderId, folderName, activitiesCount, curentUserId);

                var mydesk = new MyDesksRules(DbContext).GetMyDesk(curentUserId);
                if (folderId == 0)
                {
                    var myFolder = new MyTag
                    {
                        Name = folderName,
                        Desk = mydesk
                    };
                    DbContext.MyFolders.Add(myFolder);
                    DbContext.Entry(myFolder).State = EntityState.Added;
                    DbContext.SaveChanges();
                    activitiesCount = 0;
                    return myFolder.Id;
                }
                else
                {
                    var myFolder = DbContext.MyFolders.Find(folderId);
                    if (myFolder == null) return folderId;
                    myFolder.Name = folderName;
                    var myDesk = myFolder.Desk; //lazy loading
                    if (DbContext.Entry(myFolder).State == EntityState.Detached)
                        DbContext.MyFolders.Attach(myFolder);
                    DbContext.Entry(myFolder).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    activitiesCount = myFolder.Activities.Count;

                    return folderId;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, null, folderId, folderName, activitiesCount, curentUserId);
                return -1;
            }
        }

        public bool CheckDuplicateFolder(int folderId, string folderName, string curentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate folder", curentUserId, null, folderId, folderName, curentUserId);

                var myDesk = new MyDesksRules(DbContext).GetMyDesk(curentUserId);

                return folderId > 0 ? myDesk.Folders.Any(x => x.Id != folderId && x.Name == folderName) : myDesk.Folders.Any(x => x.Name == folderName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, folderId, folderName, curentUserId);
                return true;
            }
        }


        public List<QbicleActivity> GetActivityByUserId(string userId, int folderId, int skip)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activity by user id", null, null, userId, folderId, skip);

                var activity = new List<QbicleActivity>();
                activity = DbContext.Activities
                    .Where(x => x.ActivityType == ActivityTypeEnum.AlertActivity ||
                                x.ActivityType == ActivityTypeEnum.MediaActivity ||
                                x.ActivityType == ActivityTypeEnum.EventActivity)
                    .Where(c => c.ActivityMembers.Any(x => x.Id == userId) || c.StartedBy.Id == userId).Distinct().ToList();

                if (folderId != 0)
                    activity = activity.Where(x => x.Folders.Any(f => f.Id == folderId))
                        .OrderByDescending(x => x.TimeLineDate).Skip(skip).Take(HelperClass.myDeskPageSize).ToList();
                else
                    activity = activity.OrderByDescending(x => x.TimeLineDate).Skip(skip)
                        .Take(HelperClass.myDeskPageSize).ToList();
                return activity;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, folderId, skip);
                return new List<QbicleActivity>();
            }
        }
        public List<QbicleActivity> GetActivityByType(string userId, int skip, int type, ref int totalRecord, string currentTimeZone, string dateFormat, SearchActivityCustom searchModel = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activity by type", null, null, userId, skip, type, totalRecord, currentTimeZone, dateFormat, searchModel);

                var arrOrder = new int[] { 0, 1, 2, 3 };
                if (type == 2)//Tasks
                {
                    IQueryable<QbicleTask> query = DbContext.QbicleTasks.Where(x => x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId));
                    if (searchModel != null)
                    {
                        if (!string.IsNullOrEmpty(searchModel.SearchName))
                        {
                            string keyword = searchModel.SearchName.Trim();
                            query = DbContext.QbicleTasks.Where(x => (x.Name.Contains(keyword) || x.Description.Contains(keyword)) && (x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId)));
                        }
                        if (!string.IsNullOrEmpty(searchModel.dateRange))
                        {
                            var arrDate = searchModel.dateRange.Split('-');
                            var startDate = DateTime.ParseExact(arrDate[0].Trim(), dateFormat, null);
                            var endDate = DateTime.ParseExact(arrDate[1].Trim(), dateFormat, null);
                            var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                            var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                            var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                            query = query.Where(p => (p.ProgrammedStart >= startDateTimeUTC && p.ProgrammedEnd <= endDateTimeUTC));
                        }
                        if (searchModel.domainId > 0)
                            query = query.Where(p => p.Qbicle.Domain.Id == searchModel.domainId);
                        if (searchModel.qbcileId > 0)
                            query = query.Where(p => p.Qbicle.Id == searchModel.qbcileId);
                        if (searchModel.tags != null && searchModel.tags.Any())
                            query = query.Where(p => p.Folders.Any(a => searchModel.tags.Any(t => t == a.Id)));
                        var currentDate = DateTime.UtcNow;
                        switch (searchModel.status)
                        {
                            case 1:
                                query = query.Where(x => !x.isComplete && x.ActualStart == null && x.ProgrammedEnd >= currentDate);
                                break;
                            case 2:
                                query = query.Where(x => !x.isComplete && x.ActualStart != null && x.ProgrammedEnd >= currentDate);
                                break;
                            case 3:
                                query = query.Where(x => !x.isComplete && x.ProgrammedEnd < currentDate);
                                break;
                            case 4:
                                query = query.Where(x => x.isComplete);
                                break;
                            default:
                                break;
                        }
                        if (searchModel.UserId != null && searchModel.UserId.Any())
                            query = query.Where(p => p.AssociatedSet.Peoples.Any(a => searchModel.UserId.Any(u => u == a.User.Id))
                            || searchModel.UserId.Any(u => u == p.StartedBy.Id)
                            || searchModel.UserId.Any(u => u == p.ClosedBy.Id));
                        if (searchModel.isHide == 1)
                        {
                            query = query.Where(o => !o.isComplete);
                        }
                        switch (searchModel.order)
                        {
                            case 0: //Due date (newest first
                                query = query.OrderBy(o => o.isComplete).ThenByDescending(o => o.ProgrammedEnd);
                                break;
                            case 1://Due date (oldest first)
                                query = query.OrderBy(o => o.isComplete).ThenBy(o => o.ProgrammedEnd);
                                break;
                            case 2://Name A-Z
                                query = query.OrderBy(o => o.isComplete).ThenBy(o => o.Id);
                                break;
                            case 3://Name Z-A
                                query = query.OrderBy(o => o.isComplete).ThenByDescending(o => o.Id);
                                break;
                        }
                    }

                    totalRecord = query.Count();
                    return query.Skip(skip).Take(HelperClass.myDeskPageSize).ToList<QbicleActivity>();
                }
                else if (type == 3)//Events
                {
                    IQueryable<QbicleEvent> query = DbContext.Events.Where(x => x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId));
                    if (searchModel != null)
                    {
                        if (!string.IsNullOrEmpty(searchModel.SearchName))
                        {
                            string keyword = searchModel.SearchName.Trim();
                            query = DbContext.Events.Where(x => (x.Name.Contains(keyword) || x.Description.Contains(keyword)) && (x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId)));
                        }
                        if (!string.IsNullOrEmpty(searchModel.dateRange))
                        {
                            var arrDate = searchModel.dateRange.Split('-');
                            var startDate = DateTime.ParseExact(arrDate[0].Trim(), dateFormat, null);
                            var endDate = DateTime.ParseExact(arrDate[1].Trim(), dateFormat, null);
                            var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                            var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                            var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                            query = query.Where(p => (p.Start >= startDateTimeUTC && p.End <= endDateTimeUTC));
                        }
                        if (searchModel.domainId > 0)
                            query = query.Where(p => p.Qbicle.Domain.Id == searchModel.domainId);
                        if (searchModel.qbcileId > 0)
                            query = query.Where(p => p.Qbicle.Id == searchModel.qbcileId);
                        if (searchModel.tags != null && searchModel.tags.Any())
                            query = query.Where(p => p.Folders.Any(a => searchModel.tags.Any(t => t == a.Id)));
                        if (searchModel.UserId != null && searchModel.UserId.Any())
                            query = query.Where(p => p.AssociatedSet.Peoples.Any(a => searchModel.UserId.Any(u => u == a.User.Id))
                            || searchModel.UserId.Any(u => u == p.StartedBy.Id)
                            || searchModel.UserId.Any(u => u == p.ClosedBy.Id));

                        if (arrOrder.Contains(searchModel.order))
                        {
                            var utcNow = DateTime.UtcNow;
                            IQueryable<QbicleEventModel> tempQuery = query.Select(o => new QbicleEventModel { Status = o.End < utcNow, Event = o });
                            if (searchModel.isHide == 1)
                            {
                                tempQuery = tempQuery.Where(o => !o.Event.isComplete && !o.Status);
                            }
                            switch (searchModel.order)
                            {
                                case 0://Due date (newest first
                                    query = tempQuery.OrderBy(o => o.Status).ThenByDescending(o => o.Event.Start).Select(o => o.Event);
                                    break;
                                case 1://Due date (oldest first)
                                    query = tempQuery.OrderBy(o => o.Status).ThenBy(o => o.Event.Start).Select(o => o.Event);
                                    break;
                                case 2://Name A-Z
                                    query = tempQuery.OrderBy(o => o.Status).ThenBy(o => o.Event.Name).Select(o => o.Event);
                                    break;
                                case 3://Name Z-A
                                    query = tempQuery.OrderBy(o => o.Status).ThenByDescending(o => o.Event.Name).Select(o => o.Event);
                                    break;
                            }
                        }
                    }

                    totalRecord = query.Count();
                    return query.Skip(skip).Take(HelperClass.myDeskPageSize).ToList<QbicleActivity>();
                }
                else if (type == 4)//Medias
                {
                    IQueryable<QbicleMedia> query = DbContext.Medias.Where(x => x.IsVisibleInQbicleDashboard && (x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId)));
                    if (searchModel != null)
                    {
                        if (!string.IsNullOrEmpty(searchModel.SearchName))
                        {
                            string keyword = searchModel.SearchName.Trim();
                            query = DbContext.Medias.Where(x => (x.Name.Contains(keyword) || x.Description.Contains(keyword)) && (x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId)));
                        }
                        if (!string.IsNullOrEmpty(searchModel.dateRange))
                        {
                            var arrDate = searchModel.dateRange.Split('-');
                            var startDate = DateTime.ParseExact(arrDate[0].Trim(), dateFormat, null);
                            var endDate = DateTime.ParseExact(arrDate[1].Trim(), dateFormat, null);
                            var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                            var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                            var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                            query = query.Where(p => (p.StartedDate >= startDateTimeUTC && p.StartedDate <= endDateTimeUTC));
                        }
                        if (searchModel.domainId > 0)
                            query = query.Where(p => p.Qbicle.Domain.Id == searchModel.domainId);
                        if (searchModel.qbcileId > 0)
                            query = query.Where(p => p.Qbicle.Id == searchModel.qbcileId);
                        if (searchModel.tags != null && searchModel.tags.Any())
                            query = query.Where(p => p.Folders.Any(a => searchModel.tags.Any(t => t == a.Id)));
                        switch (searchModel.status)
                        {
                            case 1://Documents
                                query = query.Where(x => x.FileType.Type == "Word Document"
                                || x.FileType.Type == "Excel File"
                                || x.FileType.Type == "Powerpoint Presentation"
                                || x.FileType.Type == "Portable Document Format");
                                break;
                            case 2://Image File
                                query = query.Where(x => x.FileType.Type == "Image File");
                                break;
                            case 3://Videos
                                query = query.Where(x => x.FileType.Type == "Video File");
                                break;
                            default://Show all
                                break;
                        }
                        switch (searchModel.order)
                        {
                            case 0: //Added (newest first)
                                query = query.OrderByDescending(o => o.StartedDate);
                                break;
                            case 1://Added (oldest first)
                                query = query.OrderBy(o => o.StartedDate);
                                break;
                            case 2://Name A-Z
                                query = query.OrderBy(o => o.Name);
                                break;
                            case 3://Name Z-A
                                query = query.OrderByDescending(o => o.Name);
                                break;
                        }
                    }

                    totalRecord = query.Count();
                    return query.Skip(skip).Take(HelperClass.myDeskPageSize).ToList<QbicleActivity>();
                }
                else if (type == 5)//Links
                {
                    IQueryable<QbicleLink> query = DbContext.QbicleLinks.Where(x => x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId));
                    if (searchModel != null)
                    {
                        if (!string.IsNullOrEmpty(searchModel.SearchName))
                        {
                            string keyword = searchModel.SearchName.Trim();
                            query = DbContext.QbicleLinks.Where(x => (x.Name.Contains(keyword) || x.Description.Contains(keyword)) && (x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId)));
                        }
                        if (!string.IsNullOrEmpty(searchModel.dateRange))
                        {
                            var arrDate = searchModel.dateRange.Split('-');
                            var startDate = DateTime.ParseExact(arrDate[0].Trim(), dateFormat, null);
                            var endDate = DateTime.ParseExact(arrDate[1].Trim(), dateFormat, null);
                            var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                            var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                            var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                            query = query.Where(p => (p.StartedDate >= startDateTimeUTC && p.StartedDate <= endDateTimeUTC));
                        }
                        if (searchModel.domainId > 0)
                            query = query.Where(p => p.Qbicle.Domain.Id == searchModel.domainId);
                        if (searchModel.qbcileId > 0)
                            query = query.Where(p => p.Qbicle.Id == searchModel.qbcileId);
                        if (searchModel.tags != null && searchModel.tags.Any())
                            query = query.Where(p => p.Folders.Any(a => searchModel.tags.Any(t => t == a.Id)));
                        switch (searchModel.order)
                        {
                            case 1://Added (oldest first)
                                query = query.OrderBy(o => o.StartedDate);
                                break;
                            case 2://Name A-Z
                                query = query.OrderBy(o => o.Name);
                                break;
                            case 3://Name Z-A
                                query = query.OrderByDescending(o => o.Name);
                                break;
                            default://Added (newest first)
                                query = query.OrderByDescending(o => o.StartedDate);
                                break;
                        }
                    }

                    totalRecord = query.Count();
                    return query.Skip(skip).Take(HelperClass.myDeskPageSize).ToList<QbicleActivity>();
                }
                else if (type == 7)//Discussions
                {
                    IQueryable<QbicleDiscussion> query = DbContext.Discussions.Where(x => x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId));
                    if (searchModel != null)
                    {
                        if (!string.IsNullOrEmpty(searchModel.SearchName))
                        {
                            string keyword = searchModel.SearchName.Trim();
                            query = DbContext.Discussions.Where(x => (x.Name.Contains(keyword) || x.Summary.Contains(keyword)) && (x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId)));
                        }
                        if (!string.IsNullOrEmpty(searchModel.dateRange))
                        {
                            var arrDate = searchModel.dateRange.Split('-');
                            var startDate = DateTime.ParseExact(arrDate[0].Trim(), dateFormat, null);
                            var endDate = DateTime.ParseExact(arrDate[1].Trim(), dateFormat, null);
                            var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                            var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                            var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                            query = query.Where(p => (p.StartedDate >= startDateTimeUTC && p.StartedDate <= endDateTimeUTC));
                        }
                        if (searchModel.domainId > 0)
                            query = query.Where(p => p.Qbicle.Domain.Id == searchModel.domainId);
                        if (searchModel.qbcileId > 0)
                            query = query.Where(p => p.Qbicle.Id == searchModel.qbcileId);
                        if (searchModel.tags != null && searchModel.tags.Any())
                            query = query.Where(p => p.Folders.Any(a => searchModel.tags.Any(t => t == a.Id)));
                        if (arrOrder.Contains(searchModel.order))
                        {
                            var utcNow = DateTime.UtcNow;
                            IQueryable<DiscussionModel> tempQuery = query.Select(o => new DiscussionModel { Status = o.ExpiryDate != null && o.ExpiryDate <= utcNow, Discussion = o });
                            if (searchModel.isHide == 1)
                            {
                                tempQuery = tempQuery.Where(o => !o.Status);
                            }
                            switch (searchModel.order)
                            {
                                case 0://Added (newest first)
                                    query = tempQuery.OrderBy(o => o.Status).ThenByDescending(o => o.Discussion.StartedDate).Select(o => o.Discussion);
                                    break;
                                case 1://Added (oldest first)
                                    query = tempQuery.OrderBy(o => o.Status).ThenBy(o => o.Discussion.StartedDate).Select(o => o.Discussion);
                                    break;
                                case 2://Name A-Z
                                    query = tempQuery.OrderBy(o => o.Status).ThenBy(o => o.Discussion.Name).Select(o => o.Discussion);
                                    break;
                                case 3://Name Z-A
                                    query = tempQuery.OrderBy(o => o.Status).ThenByDescending(o => o.Discussion.Name).Select(o => o.Discussion);
                                    break;
                            }
                        }

                    }

                    totalRecord = query.Count();
                    return query.Skip(skip).Take(HelperClass.myDeskPageSize).ToList<QbicleActivity>();
                }
                else if (type == 6)//Processes
                {
                    IQueryable<ApprovalReq> query = DbContext.ApprovalReqs.Where(x => x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId));
                    if (searchModel != null)
                    {
                        if (!string.IsNullOrEmpty(searchModel.SearchName))
                        {
                            string keyword = searchModel.SearchName.Trim();
                            query = DbContext.ApprovalReqs.Where(x => (x.Name.Contains(keyword) || x.Notes.Contains(keyword)) && (x.StartedBy.Id == userId || x.ActivityMembers.Any(a => a.Id == userId)));
                        }
                        if (!string.IsNullOrEmpty(searchModel.dateRange))
                        {
                            var arrDate = searchModel.dateRange.Split('-');
                            var startDate = DateTime.ParseExact(arrDate[0].Trim(), dateFormat, null);
                            var endDate = DateTime.ParseExact(arrDate[1].Trim(), dateFormat, null);
                            var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                            var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                            var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                            query = query.Where(p => (p.StartedDate >= startDateTimeUTC && p.StartedDate <= endDateTimeUTC));
                        }
                        if (searchModel.domainId > 0)
                            query = query.Where(p => p.Qbicle.Domain.Id == searchModel.domainId);
                        if (searchModel.qbcileId > 0)
                            query = query.Where(p => p.Qbicle.Id == searchModel.qbcileId);
                        if (searchModel.tags != null && searchModel.tags.Any())
                            query = query.Where(p => p.Folders.Any(a => searchModel.tags.Any(t => t == a.Id)));
                        switch (searchModel.status)
                        {
                            case 1://Awaiting Review
                                query = query.Where(x => x.RequestStatus == ApprovalReq.RequestStatusEnum.Pending);
                                break;
                            case 2://Awaiting Approval
                                query = query.Where(x => x.RequestStatus == ApprovalReq.RequestStatusEnum.Reviewed);
                                break;
                            case 3://Approved
                                query = query.Where(x => x.RequestStatus == ApprovalReq.RequestStatusEnum.Approved);
                                break;
                            case 4://Discarded
                                query = query.Where(x => x.RequestStatus == ApprovalReq.RequestStatusEnum.Discarded
                                || x.RequestStatus == ApprovalReq.RequestStatusEnum.Denied);
                                break;
                            default:
                                break;
                        }
                        if (arrOrder.Contains(searchModel.order))
                        {
                            IQueryable<QbicleProcessModel> tempQuery = query.Select(o => new QbicleProcessModel { Status = o.RequestStatus == RequestStatusEnum.Approved || o.RequestStatus == RequestStatusEnum.Discarded, Approval = o });
                            if (searchModel.isHide == 1)
                            {
                                tempQuery = tempQuery.Where(o => !o.Status);
                            }
                            switch (searchModel.order)
                            {
                                case 0://Added (newest first)
                                    query = tempQuery.OrderBy(o => o.Status).ThenByDescending(o => o.Approval.TimeLineDate).Select(o => o.Approval);
                                    break;
                                case 1://Added (oldest first)
                                    query = tempQuery.OrderBy(o => o.Status).ThenBy(o => o.Approval.TimeLineDate).Select(o => o.Approval);
                                    break;
                                case 2://Name A-Z
                                    query = tempQuery.OrderBy(o => o.Status).ThenBy(o => o.Approval.Name).Select(o => o.Approval);
                                    break;
                                case 3://Name Z-A
                                    query = tempQuery.OrderBy(o => o.Status).ThenByDescending(o => o.Approval.Name).Select(o => o.Approval);
                                    break;
                            }
                        }

                    }

                    totalRecord = query.Count();
                    return query.Skip(skip).Take(HelperClass.myDeskPageSize).ToList<QbicleActivity>();
                }
                return new List<QbicleActivity>();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, skip, type, totalRecord, currentTimeZone, dateFormat, searchModel);
                return new List<QbicleActivity>();
            }
        }
        public List<QbicleActivity> GetActivityByDueDate(DateTime? ProgrammedEnd, DateTime? ProgrammedEnd2, string userId, int folderId, int type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activity by due date", null, null, ProgrammedEnd, ProgrammedEnd2, userId, folderId, type);

                IQueryable<QbicleActivity> activity = null;
                if (type == 1)
                {
                    activity = DbContext.Activities
                    .Where(x => (x.ActivityType == ActivityTypeEnum.TaskActivity
                    || x.ActivityType == ActivityTypeEnum.EventActivity
                    ) && x.ProgrammedEnd >= ProgrammedEnd && x.ProgrammedEnd <= ProgrammedEnd2)
                    .Where(c => c.ActivityMembers.Any(x => x.Id == userId) || c.StartedBy.Id == userId).Distinct();

                    if (folderId != 0)
                        activity = activity.Where(x => x.Folders.Any(f => f.Id == folderId))
                            .OrderBy(x => x.TimeLineDate);
                    else
                        activity = activity.OrderBy(x => x.TimeLineDate);
                }
                else
                {
                    activity = DbContext.Activities
                   .Where(x => (x.ActivityType == ActivityTypeEnum.TaskActivity
                    || x.ActivityType == ActivityTypeEnum.EventActivity
                    ) && x.ProgrammedEnd > ProgrammedEnd && x.ProgrammedEnd <= ProgrammedEnd2)
                   .Where(c => c.ActivityMembers.Any(x => x.Id == userId) || c.StartedBy.Id == userId).Distinct();

                    if (folderId != 0)
                        activity = activity.Where(x => x.Folders.Any(f => f.Id == folderId))
                            .OrderBy(x => x.TimeLineDate);
                    else
                        activity = activity.OrderBy(x => x.TimeLineDate);
                }

                return activity.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, ProgrammedEnd, ProgrammedEnd2, userId, folderId, type);
                return new List<QbicleActivity>();
            }
        }

        public List<QbicleActivity> GetPinsByUserId(string curentUserId, int skip, int myDeskId, ref int totalRecord, string currentTimeZone, string formatDate, SearchActivityCustom searchModel = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activity by due date", curentUserId, null, curentUserId, skip, myDeskId, totalRecord, currentTimeZone, formatDate, searchModel);

                var myDesk = GetMyDesk(curentUserId);
                //
                if (myDesk == null)
                    return new List<QbicleActivity>();
                var _dbpins = DbContext.MyPins.Where(s => s.Desk.Id == myDesk.Id);
                var query = from p in _dbpins
                            join a in DbContext.Activities on p.PinnedActivity.Id equals a.Id
                            select a;
                if (searchModel != null)
                {
                    if (!string.IsNullOrEmpty(searchModel.SearchName))
                    {
                        string keyword = searchModel.SearchName.Trim();
                        query = query.Where(x => x.Name.Contains(keyword));
                    }
                    if (!string.IsNullOrEmpty(searchModel.dateRange))
                    {
                        var arrDate = searchModel.dateRange.Split('-');
                        var startDate = DateTime.ParseExact(arrDate[0].Trim(), formatDate, null);
                        var endDate = DateTime.ParseExact(arrDate[1].Trim(), formatDate, null);
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                        var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                        var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                        query = query.Where(p => (p.StartedDate >= startDateTimeUTC && p.StartedDate <= endDateTimeUTC));
                    }
                    if (searchModel.domainId > 0)
                        query = query.Where(p => p.Qbicle.Domain.Id == searchModel.domainId);
                    if (searchModel.qbcileId > 0)
                        query = query.Where(p => p.Qbicle.Id == searchModel.qbcileId);
                    if (searchModel.tags != null && searchModel.tags.Any())
                        query = query.Where(p => p.Folders.Any(a => searchModel.tags.Any(t => t == a.Id)));
                    switch (searchModel.actType)
                    {
                        case 2:
                            query = query.Where(o => o.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity); break;
                        case 3:
                            query = query.Where(o => o.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity); break;
                        case 4:
                            query = query.Where(o => o.ActivityType == QbicleActivity.ActivityTypeEnum.MediaActivity); break;
                        case 5:
                            query = query.Where(o => o.ActivityType == QbicleActivity.ActivityTypeEnum.Link); break;
                        case 6:
                            query = query.Where(o => o.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequest || o.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp); break;
                        case 7:
                            query = query.Where(o => o.ActivityType == QbicleActivity.ActivityTypeEnum.DiscussionActivity); break;
                    }
                    if (searchModel.isHide == 1)
                    {
                        var utcNow = DateTime.UtcNow;
                        query = query.Where(o => !o.isComplete && !(o.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequest || o.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp)
                        && !(o.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity && o.ProgrammedEnd < utcNow));
                    }
                    switch (searchModel.order)
                    {
                        case 0://Added (newest first)
                            query = query.OrderByDescending(o => o.StartedDate);
                            break;
                        case 1://Added (oldest first)
                            query = query.OrderBy(o => o.StartedDate);
                            break;
                        case 2://Name A-Z
                            query = query.OrderBy(o => o.Name);
                            break;
                        case 3://Name Z-A
                            query = query.OrderByDescending(o => o.Name);
                            break;
                    }
                }
                var arrOrder = new int[] { 0, 1, 2, 3 };

                if (arrOrder.Contains(searchModel.order))
                {
                    totalRecord = query.Count();
                    return query.Skip(skip).Take(HelperClass.myDeskPageSize).ToList();
                }
                else
                {
                    totalRecord = 0;
                    return new List<QbicleActivity>();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, curentUserId, skip, myDeskId, totalRecord, currentTimeZone, formatDate, searchModel);
                return new List<QbicleActivity>();
            }
        }
        public List<object> MyDeskLoadMoreAME(int skip, int folderId, string currUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MyDeskLoadMoreAME", currUserId, null, skip, folderId, currUserId);

                var querry = from pin in DbContext.MyPins
                             join desk in DbContext.MyDesks on pin.Desk.Id equals desk.Id
                             where desk.Owner.Id == currUserId && pin.PinnedActivity != null
                             select pin.PinnedActivity;
                var myPinnedActivities = querry.ToList();


                var myPinnedAlerts = myPinnedActivities.Where(at =>
                    at.ActivityType == ActivityTypeEnum.AlertActivity).ToList();
                var myPinnedEvents = myPinnedActivities.Where(at =>
                    at.ActivityType == ActivityTypeEnum.EventActivity).ToList();
                var myPinnedMedias = myPinnedActivities.Where(at =>
                    at.ActivityType == ActivityTypeEnum.MediaActivity).ToList();

                var activi = new MyDesksRules(DbContext).GetActivityByUserId(currUserId, folderId, skip);
                var listAc = new List<object>();
                foreach (var item in activi)
                {
                    QbicleAlert alert = null;
                    QbicleEvent qEvent = null;
                    QbicleMedia media = null;
                    var isEvent = false;
                    var isMedia = false;
                    var isAlert = false;
                    var IsPinned = false;
                    QbicleAlert.AlertPriorityEnum? Priority = null;
                    QbicleAlert.AlertTypeEnum? AlertType = null;
                    var fileSize = "";
                    var fileType = "";
                    var attendingCount = 0;
                    if (item.ActivityType == ActivityTypeEnum.AlertActivity)
                    {
                        alert = new AlertsRules(DbContext).GetAlertById(item.Id);
                        Priority = alert.Priority;
                        AlertType = alert.Type;
                        isAlert = true;
                        IsPinned = myPinnedAlerts.Any(x => x.Id == item.Id);
                    }

                    if (item.ActivityType == ActivityTypeEnum.MediaActivity)
                    {
                        media = new MediasRules(DbContext).GetMediaById(item.Id);
                        fileType = media.FileType.Type;
                        fileSize = media.VersionedFiles.FirstOrDefault()?.FileSize ?? "";
                        isMedia = true;
                        IsPinned = myPinnedMedias.Any(x => x.Id == item.Id);
                    }

                    if (item.ActivityType == ActivityTypeEnum.EventActivity)
                    {
                        qEvent = new EventsRules(DbContext).GetEventById(item.Id);
                        attendingCount = qEvent.ActivityMembers.Count();
                        isEvent = true;
                        IsPinned = myPinnedActivities.Any(x => x.Id == item.Id);
                    }


                    var ac = new
                    {
                        IsPinned,
                        ActivityId = item.Id,
                        IsMember = item.ActivityMembers.Any(m => m.Id == currUserId) || item.StartedBy.Id == currUserId
                            ? true
                            : false,
                        QbicleName = item.Qbicle != null ? item.Qbicle.Name : "",
                        DomainName = item.Qbicle != null ? item.Qbicle.Domain.Name : "",
                        ActivityName = item.Name,
                        State = item.State == ActivityStateEnum.Open ? "In progress" : "Done",
                        ActivityType = item.ActivityType == ActivityTypeEnum.AlertActivity
                            ? "Alert"
                            : (item.ActivityType == ActivityTypeEnum.EventActivity ? "Event" : "Media"),
                        Priority = Priority.ToString(),
                        FileSize = fileSize,
                        FileType = fileType,
                        AttendingCount = attendingCount,
                        IsAlert = isAlert,
                        IsEvent = isEvent,
                        IsMedia = isMedia,
                        AlertType = AlertType.ToString()
                    };

                    listAc.Add(ac);
                }

                return listAc;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currUserId, skip, folderId, currUserId);
                return new List<object>();
            }

        }

        public ReturnJsonModel MoveToFolder(int ActivityId, int toFolderId, bool IsPost, int fromFoldeId = 0,
            string curentUserId = "")
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Move to folder", curentUserId, null, ActivityId, toFolderId, fromFoldeId);

                if (toFolderId == 0)
                {
                    result.result = false;
                    result.msg = ResourcesManager._L("ERROR_MSG_316");
                }
                else
                {
                    if (toFolderId != fromFoldeId)
                    {
                        var myDesk = GetMyDesk(curentUserId);
                        if (myDesk == null)
                            myDesk = new MyDesk
                            {
                                Owner = new UserRules(DbContext).GetUser(curentUserId, 0)
                            };
                        if (!IsPost)
                        {
                            var activity = DbContext.Activities.Find(ActivityId);
                            if (activity == null)
                            {
                                result.result = false;
                                result.msg = ResourcesManager._L("ERROR_MSG_815","Activity");
                            }

                            var toFolder = myDesk.Folders.Find(d => d.Id == toFolderId);
                            if (toFolder != null)
                            {
                                if (activity.Folders.Any(x => x.Id == toFolderId)) activity.Folders.Remove(toFolder);
                                // re-add to on top in list
                                activity.Folders.Add(toFolder);
                                DbContext.SaveChanges();
                                result.result = true;
                            }
                            else
                            {
                                result.result = false;
                                result.msg = ResourcesManager._L("ERROR_MSG_318");
                            }
                        }
                        else
                        {
                            var activity = DbContext.Posts.Find(ActivityId);
                            if (activity == null)
                            {
                                result.result = false;
                                result.msg = ResourcesManager._L("ERROR_MSG_815", "Activity");
                            }

                            var toFolder = myDesk.Folders.Find(d => d.Id == toFolderId);
                            if (toFolder != null)
                            {
                                if (activity.Folders.Any(x => x.Id == toFolderId)) activity.Folders.Remove(toFolder);
                                // re-add to on top in list
                                activity.Folders.Add(toFolder);
                                DbContext.SaveChanges();
                                result.result = true;
                            }
                            else
                            {
                                result.result = false;
                                result.msg = ResourcesManager._L("ERROR_MSG_318");
                            }
                        }
                    }
                    else
                    {
                        result.result = false;
                        result.msg = ResourcesManager._L("ERROR_MSG_319"); ;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, ActivityId, toFolderId, fromFoldeId);
                result.result = false;
                result.msg = ex.Message;
            }

            return result;
        }

        /// <summary>
        ///  get My Pin where PinnedActivity in Activities
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="currUserId"></param>
        /// <returns></returns>
        public List<QbicleActivity> GetMyPinInActivities(IEnumerable<QbicleActivity> activities, string currUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get my pin in activities", currUserId, null, activities, currUserId);

                var querry = (from pin in DbContext.MyPins
                                  //join acti in activities on pin.PinnedActivity.Id equals acti.Id
                              join desk in DbContext.MyDesks on pin.Desk.Id equals desk.Id
                              where desk.Owner.Id == currUserId && pin.PinnedActivity != null
                              select pin.PinnedActivity).ToList();
                var result = from pin in querry
                             join ac in activities on pin.Id equals ac.Id
                             select pin;
                return result.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currUserId, activities, currUserId);
                return new List<QbicleActivity>();
            }

        }

        public List<QbiclePost> GetMyPinInActivities(IEnumerable<QbiclePost> activities, string currUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get my pin in activities", currUserId, null, activities, currUserId);

                var querry = (from pin in DbContext.MyPins
                              join desk in DbContext.MyDesks on pin.Desk.Id equals desk.Id
                              where pin.PinnerPost != null
                              select pin.PinnerPost).ToList();
                var result = from pin in querry
                             join ac in activities on pin.Id equals ac.Id
                             select pin;
                return result.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currUserId, activities, currUserId);
                return new List<QbiclePost>();
            }

        }

        /// <summary>
        ///     Get my pin by activity type
        /// </summary>
        /// <param name="currUserId">current user id</param>
        /// <param name="activityType">QbicleActivity.ActivityTypeEnum</param>
        /// <returns></returns>
        public List<QbicleActivity> GetMyPinInActivities(string currUserId, ActivityTypeEnum activityType)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get my pin in activities", currUserId, null, currUserId, activityType);

                var querry = from pin in DbContext.MyPins
                             join desk in DbContext.MyDesks on pin.Desk.Id equals desk.Id
                             where desk.Owner.Id == currUserId && pin.PinnedActivity.ActivityType == activityType
                             select pin.PinnedActivity;

                return querry.ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currUserId, currUserId, activityType);
                return new List<QbicleActivity>();
            }

        }

        public List<QbicleActivity> GetTasksAndApprovalsByUserId(string userId, int folderId, int skip)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get tasks and approvals by user id", null, null, userId, folderId, skip);

                var activity = DbContext.Activities
                    .Where(x => x.ActivityType == ActivityTypeEnum.TaskActivity
                                || x.ActivityType == ActivityTypeEnum.ApprovalRequestApp
                                || x.ActivityType == ActivityTypeEnum.ApprovalRequest
                                || x.ActivityType == ActivityTypeEnum.MediaActivity
                                || x.ActivityType == ActivityTypeEnum.PostActivity)
                     .Where(c => c.ActivityMembers.Any(x => x.Id == userId)).ToList();

                activity = folderId != 0
                    ? activity.Where(x => x.Folders.Any(f => f.Id == folderId)).OrderByDescending(x => x.TimeLineDate)
                        .Skip(skip).Take(HelperClass.myDeskPageSize).ToList()
                    : activity.OrderByDescending(x => x.TimeLineDate).Skip(skip).Take(HelperClass.myDeskPageSize)
                        .ToList();
                return activity;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, folderId, skip);
                return new List<QbicleActivity>();
            }
        }

        public List<QbiclePost> GetPosts(int qbicleId, int folderId, int skip)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get posts", null, null, qbicleId, folderId, skip);

                var activity = new List<QbiclePost>();
                activity = DbContext.Posts.Where(c => c.Topic.Qbicle.Id == qbicleId && c.Topic != null).ToList();

                if (folderId != 0)
                    activity = activity.Where(x => x.Folders.Any(f => f.Id == folderId))
                        .OrderByDescending(x => x.StartedDate).Skip(skip).Take(HelperClass.myDeskPageSize).ToList();
                else
                    activity = activity.OrderByDescending(x => x.StartedDate).Skip(skip)
                        .Take(HelperClass.myDeskPageSize).ToList();
                return activity;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, folderId, skip);
                return new List<QbiclePost>();
            }
        }

        // get post for mydesk and dashboard
        public List<QbiclePost> GetPostsByUserId(string userId, int folderId, int skip, int take)
        {
            var listPost = new List<QbiclePost>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get posts by user id", null, null, userId, folderId, skip, take);

                if (folderId > 0)
                    listPost = DbContext.Posts.Where(x => x.Topic != null && x.Folders.Any(p => p.Id == folderId))
                        .OrderByDescending(x => x.StartedDate).ToList();
                else
                    listPost = DbContext.Posts.Where(x => x.Topic != null).OrderByDescending(x => x.StartedDate)
                        .ToList();

                if (skip > 0) listPost = listPost.Skip(skip).ToList();

                if (take > 0) listPost = listPost.Take(take).ToList();
                return listPost;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, folderId, skip, take);
                return listPost;
            }
        }
        public int UpdateTagsByActivity(int activityId, string[] TagsId, int myDeskId, bool IsPost = false)
        {
            var result = 0;

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update tags by activity", null, null, activityId, TagsId, myDeskId, IsPost);
                if (IsPost)
                {
                    QbiclePost post = DbContext.Posts.Find(activityId);
                    post.Folders.Clear();
                    if (TagsId != null && TagsId.Length > 0)
                    {
                        foreach (var tagId in TagsId)
                        {
                            int isNumber;
                            if (int.TryParse(tagId, out isNumber))
                            {
                                var tag = DbContext.MyFolders.FirstOrDefault(t => t.Id == isNumber);
                                if (tag != null)
                                {
                                    post.Folders.Add(tag);
                                }
                                else
                                {
                                    var myDesk = DbContext.MyDesks.FirstOrDefault(p => p.Id == myDeskId);
                                    tag = new MyTag
                                    {
                                        Name = tagId,
                                        Desk = myDesk
                                    };
                                    post.Folders.Add(tag);
                                }
                            }
                            else
                            {
                                var myDesk = DbContext.MyDesks.FirstOrDefault(p => p.Id == myDeskId);
                                var tag = new MyTag
                                {
                                    Name = tagId,
                                    Desk = myDesk
                                };
                                post.Folders.Add(tag);
                            }
                        }
                    }
                }
                else
                {
                    QbicleActivity activity = DbContext.Activities.Find(activityId);
                    activity.Folders.Clear();
                    if (TagsId != null && TagsId.Length > 0)
                    {
                        foreach (var tagId in TagsId)
                        {
                            int isNumber;
                            if (int.TryParse(tagId, out isNumber))
                            {
                                var tag = DbContext.MyFolders.FirstOrDefault(t => t.Id == isNumber);
                                if (tag != null)
                                {
                                    activity.Folders.Add(tag);
                                }
                                else
                                {
                                    var myDesk = DbContext.MyDesks.FirstOrDefault(p => p.Id == myDeskId);
                                    tag = new MyTag
                                    {
                                        Name = tagId,
                                        Desk = myDesk
                                    };
                                    activity.Folders.Add(tag);
                                }
                            }
                            else
                            {
                                var myDesk = DbContext.MyDesks.FirstOrDefault(p => p.Id == myDeskId);
                                var tag = new MyTag
                                {
                                    Name = tagId,
                                    Desk = myDesk
                                };
                                activity.Folders.Add(tag);
                            }
                        }
                    }
                }
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activityId, TagsId, myDeskId, IsPost);
            }
            return result;
        }
        public DataTablesResponse GetDataTags(IDataTablesRequest requestModel, int DeskId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get data tags", null, null, requestModel, DeskId);

                IQueryable<MyTag> query = DbContext.MyFolders;

                #region Filtering
                var value = requestModel.Search.Value != string.Empty ? requestModel.Search.Value.Trim().ToLower() : "";
                if (value != "")
                    query = query.Where(s => s.Desk.Id == DeskId && s.Name.Contains(value));
                else
                    query = query.Where(s => s.Desk.Id == DeskId);
                #endregion
                var filteredCount = query.Count();
                #region Sorting 

                // Sorting 
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = String.Empty;

                foreach (var column in sortedColumns)
                {
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += column.Data + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);

                #endregion Sorting 
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var data = list.Select(s =>
                new MyTagsCustom
                {
                    Id = s.Id,
                    Name = s.Name,
                    Creator = s.Creator != null && (!string.IsNullOrEmpty(s.Creator.Forename) || !string.IsNullOrEmpty(s.Creator.Surname))
                        ? s.Creator.Forename + " " + s.Creator.Surname : "",
                    CreatedDate = s.CreatedDate.HasValue ? s.CreatedDate.Value.ToString("dd-MM-yyyy") : "",
                    Instances = s.Activities.Count() + s.Posts.Count()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, data, filteredCount, filteredCount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, DeskId);
                return null;
            }
        }
        public QbicleActivity GetActivityById(int actId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activity by id", null, null, actId);

                return DbContext.Activities.FirstOrDefault(a => a.Id == actId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, actId);
                return null;
            }
        }
        public void MyDeskUiSetting(string page, string userId, int tab, SearchActivityCustom searchModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MyDeskUiSetting", userId, null, page, tab, searchModel);

                var jvSerializer = new JavaScriptSerializer();
                List<UiSetting> uis = new List<UiSetting>();
                #region Genaral stored
                //Tab Index
                var currentUser = DbContext.QbicleUser.Find(userId);
                uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.tabIndex, Value = tab.ToString() });
                #endregion
                switch (tab)
                {
                    case 1://Pinned
                           //Domains
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.dllDomainIdPined, Value = searchModel.domainId.ToString() });
                        //Qbicle
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlQbicleIdPined, Value = searchModel.qbcileId.ToString() });
                        //Tags
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlTagsPined, Value = jvSerializer.Serialize(searchModel.tags) });
                        //Order
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlOrderbyPinned, Value = searchModel.order.ToString() });
                        //Date range
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangePined, Value = searchModel.dateRange });
                        //Activity type
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlActType, Value = searchModel.actType.ToString() });
                        //Hide complete
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.isHideInPined, Value = searchModel.isHide.ToString() });
                        break;
                    case 2://Task
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.dllDomainIdTask, Value = searchModel.domainId.ToString() });
                        //Qbicle
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlQbicleIdTask, Value = searchModel.qbcileId.ToString() });
                        //Tags
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlTagsTask, Value = jvSerializer.Serialize(searchModel.tags) });
                        //Order
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlOrderbyTask, Value = searchModel.order.ToString() });
                        //Date range
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeTask, Value = searchModel.dateRange });
                        //Hide completed Tasks
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.isHideInTask, Value = searchModel.isHide.ToString() });
                        break;
                    case 3://Event
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.dllDomainIdEvent, Value = searchModel.domainId.ToString() });
                        //Qbicle
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlQbicleIdEvent, Value = searchModel.qbcileId.ToString() });
                        //Tags
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlTagsEvent, Value = jvSerializer.Serialize(searchModel.tags) });
                        //Order
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlOrderbyEvent, Value = searchModel.order.ToString() });
                        //Date range
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeEvent, Value = searchModel.dateRange });
                        //Hide past Events 
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.isHideInEvent, Value = searchModel.isHide.ToString() });
                        break;
                    case 4://Media
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.dllDomainIdMedia, Value = searchModel.domainId.ToString() });
                        //Qbicle
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlQbicleIdMedia, Value = searchModel.qbcileId.ToString() });
                        //Tags
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlTagsMedia, Value = jvSerializer.Serialize(searchModel.tags) });
                        //Order
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlOrderbyMedia, Value = searchModel.order.ToString() });
                        //Date range
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeMedia, Value = searchModel.dateRange });
                        break;
                    case 5://Link
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.dllDomainIdLink, Value = searchModel.domainId.ToString() });
                        //Qbicle
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlQbicleIdLink, Value = searchModel.qbcileId.ToString() });
                        //Tags
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlTagsLink, Value = jvSerializer.Serialize(searchModel.tags) });
                        //Order
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlOrderbyLink, Value = searchModel.order.ToString() });
                        //Date range
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeLink, Value = searchModel.dateRange });
                        break;
                    case 7://Discussion
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.dllDomainIdDiscussion, Value = searchModel.domainId.ToString() });
                        //Qbicle
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlQbicleIdDiscussion, Value = searchModel.qbcileId.ToString() });
                        //Tags
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlTagsDiscussion, Value = jvSerializer.Serialize(searchModel.tags) });
                        //Order
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlOrderbyDiscussion, Value = searchModel.order.ToString() });
                        //Date range
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeDiscussion, Value = searchModel.dateRange });
                        //Hide closed
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.isHideInDiscussion, Value = searchModel.isHide.ToString() });
                        break;
                    default://Process
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.dllDomainIdProcess, Value = searchModel.domainId.ToString() });
                        //Qbicle
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlQbicleIdProcess, Value = searchModel.qbcileId.ToString() });
                        //Tags
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlTagsProcess, Value = jvSerializer.Serialize(searchModel.tags) });
                        //Order
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.ddlOrderbyProcess, Value = searchModel.order.ToString() });
                        //Date range
                        uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeProcess, Value = searchModel.dateRange });
                        break;
                }
                new QbicleRules(DbContext).StoredUiSettings(uis);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, page, tab, searchModel);
            }

        }
        public void MyDeskResetDateStoreUiSetting(string page, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MyDeskResetDateStoreUiSetting", currentUserId, null, page);

                var currentUser = DbContext.QbicleUser.Find(currentUserId);

                var uis = new List<UiSetting>
                {
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangePined, Value = "" },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeTask, Value = "" },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeEvent, Value = "" },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeMedia, Value = "" },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeLink, Value = "" },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeDiscussion, Value = "" },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = MyDeskKeyStoredUiSettings.txtDaterangeProcess, Value = "" }
                };
                new QbicleRules(DbContext).StoredUiSettings(uis);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, page);
            }
        }

        public ReturnJsonModel AddUserAddress(string userId, TraderAddress address)
        {
            var result = new ReturnJsonModel()
            {
                actionVal = 2
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, address.Id);

                var _user = new UserRules(DbContext).GetUser(userId, 0);
                if (_user == null)
                {
                    result.result = false;
                    result.Object = "The application user does not exist.";
                    return result;
                }
                else
                {
                    if (!_user.TraderAddresses.Contains(address))
                    {
                        _user.TraderAddresses.Add(address);
                        DbContext.Entry(_user).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }
                    if (address.IsDefault)
                    {
                        foreach(var addressItem in _user.TraderAddresses)
                        {
                            if (addressItem.Id != address.Id)
                                addressItem.IsDefault = false;
                        }
                    }
                    DbContext.SaveChanges();
                    result.result = true;
                    result.msgId = address.Id.ToString();
                    return result;
                }

            } catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, address.Id);
                result.result = false;
                result.Object = "Something wrong. Please contact the administrator.";
                return result;
            }
        }

    }
}