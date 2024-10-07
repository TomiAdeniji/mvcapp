using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Operator.Goals;
using Qbicles.Models.Operator.Team;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorPerformanceTrackingRules
    {
        ApplicationDbContext dbContext;
        public OperatorPerformanceTrackingRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public PerformanceTracking getPerformanceTrackingById(int id)
        {
            try
            {
                return dbContext.OperatorPerformanceTrackings.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public ReturnJsonModel SavePerformanceTracking(PerformanceTrackingModel model)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbPerformanceTracking = dbContext.OperatorPerformanceTrackings.Find(model.Id);
                var workgroup = dbContext.OperatorWorkGroups.Find(model.WorkgroupId);
                var person = dbContext.OperatorTeamPersons.Find(model.TeamPersonId);
                List<TrackingMeasure> lstTrackingMeasure = new List<TrackingMeasure>();

                var trackingMeasures = JsonConvert.DeserializeObject<List<TrackingMesureModel>>(model.TrackingMeasures);
                if (trackingMeasures != null)
                {
                    foreach (var item in trackingMeasures)
                    {
                        TrackingMeasure trackingMeasure = new TrackingMeasure();
                        trackingMeasure.Measure = dbContext.OperatorMeasures.Find(item.MeasureId);
                        trackingMeasure.Weight = item.Weight;
                        lstTrackingMeasure.Add(trackingMeasure);
                    }
                }

                if (dbPerformanceTracking != null)
                {
                    dbContext.OperatorTrackingMeasures.RemoveRange(dbContext.OperatorTrackingMeasures.Where(t => t.Tracking.Id == dbPerformanceTracking.Id));
                    dbPerformanceTracking.Team = person;
                    dbPerformanceTracking.WorkGroup = workgroup;
                    dbPerformanceTracking.Description = model.Description;
                    if (lstTrackingMeasure.Count() > 0) dbPerformanceTracking.TrackingMeasures.AddRange(lstTrackingMeasure);

                    if (dbContext.Entry(dbPerformanceTracking).State == EntityState.Detached)
                        dbContext.OperatorPerformanceTrackings.Attach(dbPerformanceTracking);
                    dbContext.Entry(dbPerformanceTracking).State = EntityState.Modified;
                }
                else
                {
                    dbPerformanceTracking = new PerformanceTracking();
                    dbPerformanceTracking.Team = person;
                    dbPerformanceTracking.WorkGroup = workgroup;
                    dbPerformanceTracking.Description = model.Description;
                    if (lstTrackingMeasure.Count() > 0) dbPerformanceTracking.TrackingMeasures.AddRange(lstTrackingMeasure);

                    dbContext.OperatorPerformanceTrackings.Add(dbPerformanceTracking);
                    dbContext.Entry(dbPerformanceTracking).State = EntityState.Added;
                }

                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                return refModel;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }

        public List<PerformanceTracking> SearchPerformanceTrackings(string search, int locationId, bool isLoadingHide, int skip, int take, ref int totalRecord)
        {
            try
            {
                search = search.Trim().ToLower();
                var query = dbContext.OperatorPerformanceTrackings.AsQueryable();
                if (!isLoadingHide)
                {
                    query = query.Where(t => !t.IsHide);
                }

                if (!String.IsNullOrEmpty(search))
                {
                    query = query.Where(t => t.Description.Trim().ToLower().Contains(search)
                                      || ((t.Team.User.Forename != null || t.Team.User.Surname != null) && (t.Team.User.Forename + " " + t.Team.User.Surname).ToLower().Contains(search)) ||
                                      t.Team.User.DisplayUserName.Trim().ToLower().Contains(search));
                }
                totalRecord = query.Count();

                var list = query.OrderByDescending(t => t.Id).Skip(skip).Take(take).ToList();
                return list;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<PerformanceTracking>();
            }
        }

        public ReturnJsonModel ShowOrHidePerformanceTracking(int id, bool isHide)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbPerformanceTracking = dbContext.OperatorPerformanceTrackings.Find(id);
                dbPerformanceTracking.IsHide = isHide;
                if (dbContext.Entry(dbPerformanceTracking).State == EntityState.Detached)
                    dbContext.OperatorPerformanceTrackings.Attach(dbPerformanceTracking);
                dbContext.Entry(dbPerformanceTracking).State = EntityState.Modified;
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }

        public WorkgroupInforModel GetWorkgroupInfor(int id, string currentUserId)
        {
            try
            {
                WorkgroupInforModel infor = new WorkgroupInforModel();
                var workgroup = dbContext.OperatorWorkGroups.Find(id);
                infor.Location = workgroup.Location.Name;
                infor.Process = "Performance Management";
                infor.Qbicle = workgroup.SourceQbicle.Name;
                var lstIds = workgroup.TeamMembers.Select(m => m.Member.Id).ToList();
                var teamPersons = dbContext.OperatorTeamPersons.Where(o => !o.IsHide && lstIds.Contains(o.User.Id)).ToList();
                infor.Members = teamPersons.Count();
                foreach (var item in teamPersons)
                {
                    PersonModal person = new PersonModal();
                    person.Id = item.Id;
                    person.Name = HelperClass.GetFullNameOfUser(item.User, currentUserId);
                    person.ProfilePic = item.User.ProfilePic;
                    infor.Persons.Add(person);
                }
                return infor;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public List<TeamPerson> GetAllTeamPerson(int performanceId, int wgId)
        {
            try
            {
                var performance = dbContext.OperatorPerformanceTrackings.Find(performanceId);
                var workgroup = dbContext.OperatorWorkGroups.Find(wgId);
                var lstIds = workgroup.TeamMembers.Select(m => m.Member.Id).ToList();
                var teamPersons = dbContext.OperatorTeamPersons.Where(o => !o.IsHide && lstIds.Contains(o.User.Id)).ToList();
                if (performance != null && performance.WorkGroup.Id == wgId && !teamPersons.Any(t => t.Id == performance.Team.Id))
                {
                    teamPersons.Add(performance.Team);
                }
                return teamPersons;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public PerformanceChart GetPerformanceChart(OperatorPerformanceChart performanceChart)
        {
            try
            {
                PerformanceChart chart = new PerformanceChart();
                #region Dates filter
                var tz = TimeZoneInfo.FindSystemTimeZoneById(performanceChart.currentTimeZone);
                var endDateTime = DateTime.UtcNow.Date;
                var startDateTime = endDateTime.AddDays(-7);
                if (performanceChart.timeframe == 1)
                {
                    endDateTime = DateTime.UtcNow.Date;
                    startDateTime = endDateTime.AddDays(-30);
                }
                else if (performanceChart.timeframe == 2)
                {
                    var arrDate = performanceChart.customDate.Split('-');
                    var startDate = DateTime.ParseExact(arrDate[0].Trim(), performanceChart.dateFormat, null);
                    var endDate = DateTime.ParseExact(arrDate[1].Trim(), performanceChart.dateFormat, null);
                    startDateTime = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                    endDateTime = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                }
                endDateTime = endDateTime.AddDays(1);
                #endregion
                var performanceMeasures = dbContext.OperatorTrackingMeasures.Where(s => s.Tracking.Id == performanceChart.performanceId);
                var userId = dbContext.OperatorPerformanceTrackings.Find(performanceChart.performanceId).Team.User.Id;
                var queryProcess = from pm in performanceMeasures
                                   join fe in dbContext.FormElement.Where(s => s.AllowScore) on pm.Measure.Id equals fe.AssociatedMeasure.Id into fet
                                   from fed in fet.DefaultIfEmpty()
                                   select new OperatorMeasureModel
                                   {
                                       Id = pm.Id,
                                       MeasureId = pm.Measure.Id,
                                       MeasureName = pm.Measure.Name,
                                       MeasureDesc = pm.Measure.Summary,
                                       Weight = pm.Weight,
                                       Score = fed != null ? fed.FormElementDatas.Where(s => s.CreatedBy.Id == userId && s.CreatedDate >= startDateTime && s.CreatedDate < endDateTime).Average(s => s.Score) : 0
                                   };
                chart.ChartProcessMeasures = queryProcess.GroupBy(q => new { q.Id, q.MeasureId, q.MeasureName, q.MeasureDesc, q.Weight })
                    .Select(g => new OperatorMeasureModel
                    {
                        Id = g.Key.Id,
                        MeasureId = g.Key.MeasureId,
                        MeasureName = g.Key.MeasureName,
                        MeasureDesc = g.Key.MeasureDesc,
                        Weight = g.Key.Weight,
                        Score = g.Average(s => s.Score)
                    }).ToList();
                var queryMonitor = from pm in performanceMeasures
                                   join fe in dbContext.FormElement.Where(s => s.AllowScore) on pm.Measure.Id equals fe.AssociatedMeasure.Id into fet
                                   from fed in fet.DefaultIfEmpty()
                                   join fd in dbContext.FormElementData.Where(f => f.CreatedBy.Id == userId) on fed.Id equals fd.ParentElement.Id into fdt
                                   from fdd in fdt.DefaultIfEmpty()
                                   select new OperatorMonitorData
                                   {
                                       MeasureId = pm.Measure.Id,
                                       Weight = pm.Weight,
                                       Score = fdd != null ? fdd.Score : 0,
                                       Day = fdd != null ? fdd.CreatedDate : DateTime.MinValue
                                   };

                if (performanceChart.isDay)
                {
                    chart.CharMonitorMeasures = queryMonitor.ToList().Select(s1 => new OperatorMonitorDataChart
                    {
                        MeasureId = s1.MeasureId,
                        Weight = s1.Weight,
                        Score = s1.Score,
                        Day = s1.Day.ConvertTimeFromUtc(performanceChart.currentTimeZone).ToString("dd/MM")
                    }).GroupBy(g1 => new { g1.Day, g1.MeasureId, g1.Weight }).Select(s2 => new OperatorMonitorDataChart
                    {
                        MeasureId = s2.Key.MeasureId,
                        Weight = s2.Key.Weight,
                        Day = s2.Key.Day,
                        Score = s2.Average(a => a.Score)
                    }).GroupBy(g2 => g2.Day).Select(s3 => new { key = s3.Key, processPercent = s3.Sum(t => t.Score * t.Weight) / 5 }).ToList();
                }
                else
                {
                    chart.CharMonitorMeasures = queryMonitor.ToList().Select(s1 => new OperatorMonitorDataChart
                    {
                        MeasureId = s1.MeasureId,
                        Weight = s1.Weight,
                        Score = s1.Score,
                        Day = s1.Day.ConvertTimeFromUtc(performanceChart.currentTimeZone).ToString("MM/yyyy")
                    }).GroupBy(g1 => new { g1.Day, g1.MeasureId, g1.Weight }).Select(s2 => new OperatorMonitorDataChart
                    {
                        MeasureId = s2.Key.MeasureId,
                        Weight = s2.Key.Weight,
                        Day = s2.Key.Day,
                        Score = s2.Average(a => a.Score)
                    }).GroupBy(g2 => g2.Day).Select(s3 => new { key = s3.Key, processPercent = s3.Sum(t => t.Score * t.Weight) / 5 }).ToList();
                }

                #region Total Performance Process
                chart.TotalProgressPerformance = 0;
                decimal topSum = 0;
                decimal buttomSum = 0;
                if (chart.ChartProcessMeasures != null)
                    foreach (var item in chart.ChartProcessMeasures)
                    {
                        topSum += ((item.Score.HasValue ? item.Score.Value : 0) * item.Weight);
                        buttomSum += (5 * item.Weight);
                    }
                chart.TotalProgressPerformance = (topSum / buttomSum) * 100;
                #endregion
                return chart;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public List<OperatorMeasureModel> GetPerformanceMeasures(OperatorPerformanceChart performanceChart, string search)
        {
            try
            {
                #region Dates filter
                var tz = TimeZoneInfo.FindSystemTimeZoneById(performanceChart.currentTimeZone);
                var endDateTime = DateTime.UtcNow.Date;
                var startDateTime = endDateTime.AddDays(-7);
                if (performanceChart.timeframe == 1)
                {
                    endDateTime = DateTime.UtcNow.Date;
                    startDateTime = endDateTime.AddDays(-30);
                }
                else if (performanceChart.timeframe == 2)
                {
                    var arrDate = performanceChart.customDate.Split('-');
                    var startDate = DateTime.ParseExact(arrDate[0].Trim(), performanceChart.dateFormat, null);
                    var endDate = DateTime.ParseExact(arrDate[1].Trim(), performanceChart.dateFormat, null);
                    startDateTime = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                    endDateTime = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                }
                endDateTime = endDateTime.AddDays(1);
                #endregion
                var performanceMeasures = dbContext.OperatorTrackingMeasures.Where(s => s.Tracking.Id == performanceChart.performanceId);
                var userId = dbContext.OperatorPerformanceTrackings.Find(performanceChart.performanceId).Team.User.Id;
                if (!string.IsNullOrEmpty(search))
                {
                    performanceMeasures = performanceMeasures.Where(s => s.Measure.Name.Contains(search) || s.Measure.Summary.Contains(search));
                }
                var queryProcess = from pm in performanceMeasures
                                   join fe in dbContext.FormElement.Where(s => s.AllowScore) on pm.Measure.Id equals fe.AssociatedMeasure.Id into fet
                                   from fed in fet.DefaultIfEmpty()
                                   select new OperatorMeasureModel
                                   {
                                       Id = pm.Id,
                                       MeasureId = pm.Measure.Id,
                                       MeasureName = pm.Measure.Name,
                                       MeasureDesc = pm.Measure.Summary,
                                       Weight = pm.Weight,
                                       SubmitCount = fed != null ? fed.FormElementDatas.Where(s => s.CreatedBy.Id == userId && s.CreatedDate >= startDateTime && s.CreatedDate < endDateTime).Count() : 0,
                                       Score = fed != null ? fed.FormElementDatas.Where(s => s.CreatedBy.Id == userId && s.CreatedDate >= startDateTime && s.CreatedDate < endDateTime).Average(s => s.Score) : 0
                                   };

                return queryProcess.GroupBy(q => new { q.Id, q.MeasureId, q.MeasureName, q.MeasureDesc, q.Weight })
                    .Select(g => new OperatorMeasureModel
                    {
                        Id = g.Key.Id,
                        MeasureId = g.Key.MeasureId,
                        MeasureName = g.Key.MeasureName,
                        MeasureDesc = g.Key.MeasureDesc,
                        Weight = g.Key.Weight,
                        SubmitCount = g.Sum(s => s.SubmitCount),
                        Score = g.Average(s => s.Score)
                    }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<OperatorMeasureModel>();
            }
        }

        public ReturnJsonModel DeleteTrakingMeasure(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbTrakingMeasure = dbContext.OperatorTrackingMeasures.Find(id);
                if (dbTrakingMeasure != null)
                {
                    dbContext.OperatorTrackingMeasures.Remove(dbTrakingMeasure);
                    refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }

        public PerformanceTracking GetPerformanceByActivityId(int activityId)
        {
            try
            {
                return dbContext.OperatorPerformanceTrackings.FirstOrDefault(s => s.Discussion.Id == activityId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public bool AddPostToPerformanceDiscussion(int performanceId, QbiclePost post, string currentUserId, int currentQbicleId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add posts to performance post discussion", currentUserId, null, performanceId, post, currentUserId, currentQbicleId);

                var performancePostReq = dbContext.Activities.Find(performanceId);
                performancePostReq.TimeLineDate = DateTime.UtcNow;
                performancePostReq.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                performancePostReq.Posts.Add(post);
                performancePostReq.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.Activities.Attach(performancePostReq);
                dbContext.Entry(performancePostReq).State = EntityState.Modified;
                dbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = performancePostReq.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.DiscussionCreation,
                    AppendToPageName = ApplicationPageName.Discussion,
                    AppendToPageId = performanceId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).NotificationComment2Activity(activityNotification);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, performanceId, post, currentUserId, currentQbicleId);
                return false;
            }

        }
    }
}
