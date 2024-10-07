using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Operator.Goals;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorGoalRules
    {
        ApplicationDbContext dbContext;
        public OperatorGoalRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public Goal GetGoalById(int id)
        {
            try
            {
                return dbContext.OperatorGoals.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new Goal();
            }
        }
        public Goal GetGoalByActivityId(int activityId)
        {
            try
            {
                return dbContext.OperatorGoals.FirstOrDefault(s => s.Discussion.Id == activityId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public List<Goal> LoadGoalsByDomainId(int currentDomainId, bool isHide, int skip, int take, string keyword, List<int> tags, ref int totalRecord)
        {
            try
            {
                var query = dbContext.OperatorGoals.Where(s => s.Domain.Id == currentDomainId && (!s.isHide || s.isHide == isHide));

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Name.Contains(keyword) || s.Summary.Contains(keyword));
                }
                if (tags != null && tags.Any())
                {
                    query = query.Where(s => s.Tags.Any(t => tags.Any(g => g == t.Id)));
                }
                totalRecord = query.Count();
                return query.OrderByDescending(s => s.CreatedDate).Skip(skip).Take(take).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<Goal>();
            }
        }
        public ReturnJsonModel SaveGoal(OperatorGoalModel operatorGoal, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var setting = dbContext.OperatorSettings.FirstOrDefault(s => s.Domain.Id == operatorGoal.Domain.Id);
                if (setting == null)
                {
                    refModel.msg = "ERROR_MSG_153";
                    return refModel;
                }

                if (!string.IsNullOrEmpty(operatorGoal.FeaturedImageURI))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(operatorGoal.FeaturedImageURI);

                }

                var user = dbContext.QbicleUser.Find(userId);
                var dbGoal = dbContext.OperatorGoals.Find(operatorGoal.Id);
                if (dbGoal != null)
                {
                    dbGoal.Name = operatorGoal.Name;
                    dbGoal.Summary = operatorGoal.Summary;
                    dbGoal.LastUpdatedBy = user;
                    dbGoal.LastUpdateDate = DateTime.UtcNow;
                    #region Add media by folder default qbicle
                    if (!string.IsNullOrEmpty(operatorGoal.FeaturedImageURI))
                    {
                        var folder = dbContext.MediaFolders.FirstOrDefault(s => s.Name == HelperClass.GeneralName && s.Qbicle.Id == setting.SourceQbicle.Id);
                        if (folder == null)
                        {
                            folder = new MediaFolder();
                            folder.Name = HelperClass.GeneralName;
                            folder.Qbicle = setting.SourceQbicle;
                            folder.CreatedDate = DateTime.UtcNow;
                            folder.CreatedBy = user;
                            dbContext.MediaFolders.Add(folder);
                            dbContext.Entry(folder).State = EntityState.Added;
                        }
                        AddMediaQbicle(operatorGoal.MediaResponse, user, setting.SourceQbicle, folder, operatorGoal.Name, operatorGoal.Summary, setting.DefaultTopic);
                        dbGoal.FeaturedImageUri = operatorGoal.FeaturedImageURI;
                    }
                    #endregion
                    if (operatorGoal.Tags != null)
                    {
                        dbGoal.Tags.Clear();
                        foreach (var item in operatorGoal.Tags)
                        {
                            var tag = dbContext.OperatorTags.Find(item);
                            if (tag != null)
                                dbGoal.Tags.Add(tag);
                        }
                    }

                    if (operatorGoal.LeadingIndicators != null)
                    {
                        var lstRemove = dbGoal.GoalMeasures.Where(s => s.Type == GoalMeasureTypeEnum.LeadingIndicator).ToList();
                        if (lstRemove.Any())
                            dbContext.OperatorGoalMeasures.RemoveRange(lstRemove);
                        foreach (var item in operatorGoal.LeadingIndicators)
                        {
                            var measure = dbContext.OperatorMeasures.Find(item.MeasureId);
                            if (measure != null)
                            {
                                GoalMeasure goal = new GoalMeasure();
                                goal.Measure = measure;
                                goal.Type = GoalMeasureTypeEnum.LeadingIndicator;
                                goal.Weight = item.Weight;
                                goal.Goal = dbGoal;
                                dbContext.Entry(goal).State = EntityState.Added;
                                dbGoal.GoalMeasures.Add(goal);
                            }

                        }
                    }
                    if (operatorGoal.GoalMeasures != null)
                    {
                        var lstRemove = dbGoal.GoalMeasures.Where(s => s.Type == GoalMeasureTypeEnum.GoalMeasure).ToList();
                        if (lstRemove.Any())
                            dbContext.OperatorGoalMeasures.RemoveRange(lstRemove);
                        foreach (var item in operatorGoal.GoalMeasures)
                        {
                            var measure = dbContext.OperatorMeasures.Find(item.MeasureId);
                            if (measure != null)
                            {
                                GoalMeasure goal = new GoalMeasure();
                                goal.Measure = measure;
                                goal.Type = GoalMeasureTypeEnum.GoalMeasure;
                                goal.Weight = item.Weight;
                                goal.Goal = dbGoal;
                                dbContext.Entry(goal).State = EntityState.Added;
                                dbGoal.GoalMeasures.Add(goal);
                            }

                        }
                    }
                    if (dbContext.Entry(dbGoal).State == EntityState.Detached)
                        dbContext.OperatorGoals.Attach(dbGoal);
                    dbContext.Entry(dbGoal).State = EntityState.Modified;
                }
                else
                {
                    dbGoal = new Goal();
                    dbGoal.Name = operatorGoal.Name;
                    dbGoal.Summary = operatorGoal.Summary;
                    dbGoal.Domain = operatorGoal.Domain;
                    dbGoal.isHide = false;
                    dbGoal.CreatedBy = user;
                    dbGoal.CreatedDate = DateTime.UtcNow;
                    #region Add media by folder default qbicle
                    if (!string.IsNullOrEmpty(operatorGoal.FeaturedImageURI))
                    {
                        var folder = dbContext.MediaFolders.FirstOrDefault(s => s.Name == HelperClass.GeneralName && s.Qbicle.Id == setting.SourceQbicle.Id);
                        if (folder == null)
                        {
                            folder = new MediaFolder();
                            folder.Name = HelperClass.GeneralName;
                            folder.Qbicle = setting.SourceQbicle;
                            folder.CreatedDate = DateTime.Now;
                            folder.CreatedBy = user;
                            dbContext.MediaFolders.Add(folder);
                            dbContext.Entry(folder).State = EntityState.Added;
                        }
                        AddMediaQbicle(operatorGoal.MediaResponse, user, setting.SourceQbicle, folder, operatorGoal.Name, operatorGoal.Summary, setting.DefaultTopic);
                        dbGoal.FeaturedImageUri = operatorGoal.FeaturedImageURI;
                    }
                    #endregion
                    if (operatorGoal.Tags != null)
                    {
                        foreach (var item in operatorGoal.Tags)
                        {
                            var tag = dbContext.OperatorTags.Find(item);
                            if (tag != null)
                                dbGoal.Tags.Add(tag);
                        }
                    }

                    if (operatorGoal.LeadingIndicators != null)
                    {
                        foreach (var item in operatorGoal.LeadingIndicators)
                        {
                            var measure = dbContext.OperatorMeasures.Find(item.MeasureId);
                            if (measure != null)
                            {
                                GoalMeasure goal = new GoalMeasure();
                                goal.Measure = measure;
                                goal.Type = GoalMeasureTypeEnum.LeadingIndicator;
                                goal.Weight = item.Weight;
                                goal.Goal = dbGoal;
                                dbContext.Entry(goal).State = EntityState.Added;
                                dbGoal.GoalMeasures.Add(goal);
                            }

                        }
                    }
                    if (operatorGoal.GoalMeasures != null)
                    {
                        foreach (var item in operatorGoal.GoalMeasures)
                        {
                            var measure = dbContext.OperatorMeasures.Find(item.MeasureId);
                            if (measure != null)
                            {
                                GoalMeasure goal = new GoalMeasure();
                                goal.Measure = measure;
                                goal.Type = GoalMeasureTypeEnum.GoalMeasure;
                                goal.Weight = item.Weight;
                                goal.Goal = dbGoal;
                                dbContext.Entry(goal).State = EntityState.Added;
                                dbGoal.GoalMeasures.Add(goal);
                            }

                        }
                    }
                    dbContext.Entry(dbGoal).State = EntityState.Added;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
            }
            return refModel;
        }
        public bool UpdateHiddenGoal(int id, bool isHidden, string userId)
        {
            try
            {
                var goal = dbContext.OperatorGoals.Find(id);
                if (goal != null)
                {
                    goal.isHide = isHidden;
                    goal.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    goal.LastUpdateDate = DateTime.UtcNow;
                }
                return dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }
        public MediaFolder getDefaultFolderByQbicleId(int qbicleId)
        {
            try
            {
                return dbContext.MediaFolders.FirstOrDefault(s => s.Name == HelperClass.GeneralName && s.Qbicle.Id == qbicleId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        private QbicleMedia AddMediaQbicle(MediaModel media, ApplicationUser user, Qbicle qbicle, MediaFolder folder, string name, string descript, Topic topic)
        {
            try
            {
                if (!string.IsNullOrEmpty(media.Name))
                {
                    //DbContext.Entry(media.Type).State = System.Data.Entity.EntityState.Modified;
                    //Media attach
                    var m = new QbicleMedia
                    {
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        Name = name,
                        FileType = media.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic,
                        MediaFolder = folder,
                        Description = descript,
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = user,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    m.VersionedFiles.Add(versionFile);

                    dbContext.Medias.Add(m);
                    dbContext.Entry(m).State = EntityState.Added;
                    return m;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel SaveResource(MediaModel media, string userId, int qbicleId, int mediaFolderId, string name, string description, int topicId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                s3Rules.ProcessingMediaS3(media.UrlGuid);

                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                if (topic == null)
                {
                    refModel.msg = "Error not finding the current Topic!";
                    return refModel;
                }
                var folder = new MediaFolderRules(dbContext).GetMediaFolderById(mediaFolderId, qbicleId);
                if (folder == null)
                {
                    refModel.msg = "Error not finding the current Folder!";
                    return refModel;
                }
                var qbicle = dbContext.Qbicles.Find(qbicleId);
                if (qbicle == null)
                {
                    refModel.msg = "Error not finding the current Qbicle!";
                    return refModel;
                }
                var dbMedia = AddMediaQbicle(media, dbContext.QbicleUser.Find(userId), qbicle, folder, name, description, topic);
                if (dbMedia == null) return refModel;
                qbicle.Media.Add(dbMedia);
                if (dbContext.SaveChanges() > 0)
                    refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
                return refModel;
            }
        }
        public GoalChart GetGoalChart(OperatorGoalChart goalChart)
        {
            try
            {
                GoalChart chart = new GoalChart();
                #region Dates filter
                var tz = TimeZoneInfo.FindSystemTimeZoneById(goalChart.currentTimeZone);
                var endDateTime = DateTime.UtcNow.Date;
                var startDateTime = endDateTime.AddDays(-7);
                if (goalChart.timeframe == 1)
                {
                    endDateTime = DateTime.UtcNow.Date;
                    startDateTime = endDateTime.AddDays(-30);
                }
                else if (goalChart.timeframe == 2)
                {
                    var arrDate = goalChart.customDate.Split('-');
                    var startDate = DateTime.ParseExact(arrDate[0].Trim(), goalChart.dateFormat, null);
                    var endDate = DateTime.ParseExact(arrDate[1].Trim(), goalChart.dateFormat, null);
                    startDateTime = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                    endDateTime = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                }
                endDateTime = endDateTime.AddDays(1);
                #endregion
                var goalMeasures = dbContext.OperatorGoalMeasures.Where(s => s.Goal.Id == goalChart.goalId);
                var queryProcess = from gm in goalMeasures
                                   join fe in dbContext.FormElement.Where(s => s.AllowScore) on gm.Measure.Id equals fe.AssociatedMeasure.Id into fet
                                   from fed in fet.DefaultIfEmpty()
                                   select new OperatorMeasureModel
                                   {
                                       Id = gm.Id,
                                       MeasureId = gm.Measure.Id,
                                       MeasureName = gm.Measure.Name,
                                       Weight = gm.Weight,
                                       Type = gm.Type,
                                       Score = (fed != null && fed.FormElementDatas.Any() ? (fed.FormElementDatas.Where(s => s.CreatedDate >= startDateTime && s.CreatedDate < endDateTime).Average(s => s.Score)) : 0)
                                   };
                chart.ChartProcessMeasures = queryProcess.ToList();
                var queryMonitor = from gm in goalMeasures
                                   join fe in dbContext.FormElement.Where(s => s.AllowScore) on gm.Measure.Id equals fe.AssociatedMeasure.Id into fet
                                   from fed in fet.DefaultIfEmpty()
                                   join fd in dbContext.FormElementData on fed.Id equals fd.ParentElement.Id into fdt
                                   from fdd in fdt.DefaultIfEmpty()
                                   where fdd.CreatedDate >= startDateTime && fdd.CreatedDate < endDateTime
                                   select new OperatorMonitorData
                                   {
                                       MeasureId = gm.Measure.Id,
                                       Weight = gm.Weight,
                                       Score = fdd != null ? fdd.Score : 0,
                                       Day = fdd != null ? fdd.CreatedDate : DateTime.MinValue
                                   };
                List<OperatorMonitorDataChart> listChartMonitor = null;
                if (goalChart.isDay)
                {
                    listChartMonitor = queryMonitor.ToList().Select(s => new OperatorMonitorDataChart
                    {
                        MeasureId = s.MeasureId,
                        Weight = s.Weight,
                        Score = s.Score,
                        Day = s.Day.ConvertTimeFromUtc(goalChart.currentTimeZone).ToString("dd/MM")
                    }).ToList();
                }
                else
                {
                    listChartMonitor = queryMonitor.ToList().Select(s => new OperatorMonitorDataChart
                    {
                        MeasureId = s.MeasureId,
                        Weight = s.Weight,
                        Score = s.Score,
                        Day = s.Day.ConvertTimeFromUtc(goalChart.currentTimeZone).ToString("MM/yyyy")
                    }).ToList();
                }
                #region Group by day
                var operatorMonitorDatas = listChartMonitor.GroupBy(s => new { s.Day }).Select(group => new OperatorMonitorDataChart
                {
                    Weight = (int)group.Average(s => s.Weight),
                    Day = group.Key.Day,
                    Score = group.Average(s => s.Score)
                });
                chart.CharMonitorMeasures = operatorMonitorDatas.GroupBy(s => new { s.Day, s.MeasureId }).Select(group => new { key = group.Key.Day, totalweight = group.Sum(s => ((s.Score * s.Weight) / (5 * s.Weight)) * 100) });
                #endregion
                #region Total Goal Process
                chart.TotalProgressGoal = 0;
                if (chart.CharMonitorMeasures != null&& chart.CharMonitorMeasures.Any())
                {
                    chart.TotalProgressGoal = (decimal)chart.CharMonitorMeasures.Average(s=>(int)s.totalweight);
                }
                #endregion
                return chart;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public List<OperatorMeasureModel> GetGoalMeasures(OperatorGoalChart goalChart, string search)
        {
            try
            {
                #region Dates filter
                var tz = TimeZoneInfo.FindSystemTimeZoneById(goalChart.currentTimeZone);
                var endDateTime = DateTime.UtcNow.Date;
                var startDateTime = endDateTime.AddDays(-7);
                if (goalChart.timeframe == 1)
                {
                    endDateTime = DateTime.UtcNow.Date;
                    startDateTime = endDateTime.AddDays(-30);
                }
                else if (goalChart.timeframe == 2)
                {
                    var arrDate = goalChart.customDate.Split('-');
                    var startDate = DateTime.ParseExact(arrDate[0].Trim(), goalChart.dateFormat, null);
                    var endDate = DateTime.ParseExact(arrDate[1].Trim(), goalChart.dateFormat, null);
                    startDateTime = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
                    endDateTime = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);
                }
                endDateTime = endDateTime.AddDays(1);
                #endregion
                var goalMeasures = dbContext.OperatorGoalMeasures.Where(s => s.Goal.Id == goalChart.goalId);
                if (!string.IsNullOrEmpty(search))
                {
                    goalMeasures = dbContext.OperatorGoalMeasures.Where(s => s.Goal.Id == goalChart.goalId && (s.Measure.Name.Contains(search) || s.Measure.Summary.Contains(search)));
                }
                var queryProcess = from gm in goalMeasures
                                   join fe in dbContext.FormElement.Where(s => s.AllowScore) on gm.Measure.Id equals fe.AssociatedMeasure.Id into fet
                                   from fed in fet.DefaultIfEmpty()
                                   select new OperatorMeasureModel
                                   {
                                       Id = gm.Id,
                                       MeasureId = gm.Measure.Id,
                                       MeasureName = gm.Measure.Name,
                                       MeasureDesc = gm.Measure.Summary,
                                       Weight = gm.Weight,
                                       Type = gm.Type,
                                       Score = fed != null ? fed.FormElementDatas.Where(s => s.CreatedDate >= startDateTime && s.CreatedDate < endDateTime).Average(s => s.Score) : 0
                                   };
                return queryProcess.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<OperatorMeasureModel>();
            }
        }
    }
}
