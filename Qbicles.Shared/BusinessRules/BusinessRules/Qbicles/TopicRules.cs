using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class TopicRules
    {
        ApplicationDbContext _db;

        public TopicRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }

        public List<Topic> GetTopicByQbicle(int qbicleId, string userId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by qbicle", null, null, qbicleId);
                if (qbicleId <= 0)
                    return new List<Topic>();

                var topics = DbContext.Topics.Where(q => q.Qbicle.Id == qbicleId).ToList();
                if (topics == null || !topics.Any())
                {
                    var topic = new Topic
                    {
                        Qbicle = DbContext.Qbicles.Find(qbicleId),
                        CreatedDate = DateTime.UtcNow,
                        Creator = DbContext.QbicleUser.Find(userId),
                        Name = "General",
                        Summary = "Default topic"
                    };
                    DbContext.Topics.Add(topic);
                    DbContext.SaveChanges();
                    topics.Add(topic);
                }
                return topics;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return new List<Topic>();
            }


        }
        public List<Topic> GetTopicByQbicle(int qbicleId, int topicId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by qbicle", null, null, qbicleId, topicId);

                return DbContext.Topics.Where(q => q.Qbicle.Id == qbicleId && q.Id != topicId).ToList().BusinessMapping("");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, topicId);
                return new List<Topic>();
            }
        }
        public List<TopicCustom> GetTopicByQbicle(int qbicleId, int[] topics)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by qbicle", null, null, qbicleId, topics);

                if (topics.Count() > 0 && topics[0] > 0)
                    return DbContext.Topics.Where(q => q.Qbicle.Id == qbicleId && topics.Any(s => s == q.Id)).Select(s => new TopicCustom()
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Summary = s.Summary,
                        CreatedDate = s.CreatedDate,
                        Creator = s.Creator,
                        Activities = DbContext.Activities.Where(a => a.Qbicle.Id == qbicleId && a.Topic.Id == s.Id && a.IsVisibleInQbicleDashboard).ToList()
                    }).ToList();
                else
                    return DbContext.Topics.Where(q => q.Qbicle.Id == qbicleId).Select(s => new TopicCustom()
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Summary = s.Summary,
                        CreatedDate = s.CreatedDate,
                        Creator = s.Creator,
                        Activities = DbContext.Activities.Where(a => a.Qbicle.Id == qbicleId && a.Topic.Id == s.Id && a.IsVisibleInQbicleDashboard).ToList()
                    }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, topics);
                return new List<TopicCustom>();
            }

        }
        public List<TopicCustom> ListTopic(int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "List topic", null, null, qbicleId);

                return DbContext.Topics.Where(q => q.Qbicle.Id == qbicleId).Select(s => new TopicCustom()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Summary = s.Summary,
                    CreatedDate = s.CreatedDate,
                    Creator = s.Creator,
                    isTrader = DbContext.WorkGroups.Any(w => w.Topic.Id == s.Id && w.Qbicle.Id == s.Qbicle.Id),
                    Instances = DbContext.Activities.Where(a => a.Topic.Id == s.Id).Count() + DbContext.Posts.Where(p => p.Topic.Id == s.Id).Count()
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return new List<TopicCustom>();
            }
        }

        public List<TopicCustom> GetTopicByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by domain", null, null, domainId);

                return DbContext.Topics.Where(q => q.Qbicle.Domain.Id == domainId).Select(s => new TopicCustom()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Summary = s.Summary,
                    CreatedDate = s.CreatedDate,
                    Creator = s.Creator,
                    isTrader = DbContext.WorkGroups.Any(w => w.Topic.Id == s.Id && w.Qbicle.Id == s.Qbicle.Id),
                    Instances = DbContext.Activities.Where(a => a.Topic.Id == s.Id).Count() + DbContext.Posts.Where(p => p.Topic.Id == s.Id).Count()
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<TopicCustom>();
            }

        }

        public List<ItemTopic> GetTopicOnlyByQbicle(int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTopicOnlyByQbicle", null, null, qbicleId);

                var topics = DbContext.Topics.Where(q => q.Qbicle.Id == qbicleId).Select(q => new ItemTopic() { Id = q.Id, Name = q.Name }).ToList();
                return topics;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return new List<ItemTopic>();
            }
        }
        /// <summary>
        /// get list Distinct name of topic by Domain
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public List<string> GetTopicByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by domain", null, null, domainId);

                var topics = DbContext.Topics.Where(q => q.Qbicle.Domain.Id == domainId).Select(q => q.Name).Distinct().ToList();
                return topics;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<string>();
            }

        }
        public Topic GetTopicById(int topicId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by id", null, null, topicId);

                return DbContext.Topics.FirstOrDefault(t => t.Id == topicId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, topicId);
                return null;
            }
        }
        public int CountActivitiesByTopic(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count activities by id", null, null, id);

                return DbContext.Activities.Where(a => a.Topic.Id == id).Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return 0;
            }

        }

        public Topic GetTopicByName(string topicName, int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by name", null, null, topicName, qbicleId);

                return DbContext.Topics.FirstOrDefault(t => t.Name == topicName && t.Qbicle.Id == qbicleId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, topicName, qbicleId);
                return null;
            }
        }

        public Topic GetCreateTopicByName(string topicName, int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by name", null, null, topicName, qbicleId);

                var topic = DbContext.Topics.FirstOrDefault(t => t.Name == topicName && t.Qbicle.Id == qbicleId);
                if(topic == null)
                {
                    var cube = new QbicleRules(DbContext).GetQbicleById(qbicleId);
                    topic = new Topic
                    {
                        Name = topicName,
                        Qbicle = cube
                    };
                    DbContext.Topics.Add(topic);
                    DbContext.Entry(topic).State = EntityState.Added;

                    DbContext.SaveChanges();
                }
                return topic;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, topicName, qbicleId);
                return null;
            }
        }

        public List<string> GetTopicNameToAssing(int qbicleId, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get topic by name", null, null, qbicleId, currentUserId);

                return DbContext.Topics.Where(q => q.Qbicle.Id == qbicleId).Select(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, currentUserId);
                return new List<string>();
            }
        }

        public bool DuplicateTopicNameCheck(int qbicleId, string topicName)
        {
            bool exists;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate topic name", null, null, qbicleId, topicName);

                exists = DbContext.Topics.Any(x => x.Qbicle.Id == qbicleId && x.Name == topicName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, topicName);
                exists = false;
            }

            return exists;
        }
        public bool DuplicateTopicNameCheck(int qbicleId, int topicId, string topicName)
        {
            bool exists;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate topic name", null, null, qbicleId, topicId, topicName);

                if (topicId == 0)
                    exists = DbContext.Topics.Any(x => x.Qbicle.Id == qbicleId && x.Name == topicName);
                else
                    exists = DbContext.Topics.Any(x => x.Qbicle.Id == qbicleId && x.Name == topicName && x.Id != topicId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, topicId, topicName);
                exists = false;
            }

            return exists;
        }
        public Topic SaveTopic(int qbicleId, string topicName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save topic", null, null, qbicleId, topicName);

                DbContext.Configuration.AutoDetectChangesEnabled = false;
                DbContext.Configuration.ValidateOnSaveEnabled = false;
                DbContext.Configuration.ProxyCreationEnabled = false;
                var cube = new QbicleRules(DbContext).GetQbicleById(qbicleId);
                var topic = new Topic
                {
                    Name = topicName,
                    Qbicle = cube
                };
                DbContext.Topics.Add(topic);
                DbContext.Entry(topic).State = EntityState.Added;

                DbContext.SaveChanges();
                return topic;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, topicName);
                return null;
            }
            finally
            {
                DbContext.Configuration.AutoDetectChangesEnabled = true;
                DbContext.Configuration.ValidateOnSaveEnabled = true;
                DbContext.Configuration.ProxyCreationEnabled = true;
            }
        }
        public dynamic SaveTopic(int qbicleId, Topic topic, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save topic", null, null, qbicleId, topic);

                DbContext.Configuration.AutoDetectChangesEnabled = false;
                DbContext.Configuration.ValidateOnSaveEnabled = false;
                DbContext.Configuration.ProxyCreationEnabled = false;

                var currentUser = DbContext.QbicleUser.Find(currentUserId);

                var tp = DbContext.Topics.Find(topic.Id);
                Topic t = null;
                if (tp != null)
                {
                    tp.Name = topic.Name;
                    tp.Summary = topic.Summary;
                    tp.CreatedDate = tp.CreatedDate == null ? topic.CreatedDate : tp.CreatedDate;
                    tp.Creator = tp.Creator == null ? currentUser : tp.Creator;
                    if (DbContext.Entry(tp).State == EntityState.Detached)
                        DbContext.Topics.Attach(tp);
                    DbContext.Entry(tp).State = EntityState.Modified;
                    t = tp;
                }
                else
                {
                    var cube = new QbicleRules(DbContext).GetQbicleById(qbicleId);
                    t = new Topic
                    {
                        Name = topic.Name,
                        Summary = topic.Summary,
                        CreatedDate = DateTime.UtcNow,
                        Creator = currentUser,
                        Qbicle = cube
                    };
                    DbContext.Topics.Add(t);
                    DbContext.Entry(t).State = EntityState.Added;
                }
                ;
                if (DbContext.SaveChanges() > 0)
                {
                    dynamic tpic = new
                    {
                        t.Id,
                        t.Name,
                        t.Summary,
                        CreatedDate = t.CreatedDate.HasValue ? t.CreatedDate.Value.ToString("dd-MM-yyyy") : "",
                        Creator = t.Creator != null ? HelperClass.GetFullNameOfUser(t.Creator) : "",
                        isTrader = DbContext.WorkGroups.Any(w => w.Topic.Id == t.Id && w.Qbicle.Id == t.Qbicle.Id),
                        Instances = DbContext.Activities.Where(a => a.Topic.Id == t.Id).Count()
                    };
                    return tpic;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, topic);
                return null;
            }
            finally
            {
                DbContext.Configuration.AutoDetectChangesEnabled = true;
                DbContext.Configuration.ValidateOnSaveEnabled = true;
                DbContext.Configuration.ProxyCreationEnabled = true;
            }
        }
        public bool DeleteTopic(int topicMoveId, int topicDeleteId, int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete topic", null, null, topicMoveId, topicDeleteId, qbicleId);

                var topicMove = DbContext.Topics.Find(topicMoveId);
                if (topicMove != null)
                {
                    //Move Workgroups into topic new
                    var wks_group_topic = DbContext.WorkGroups.Where(w => w.Topic.Id == topicDeleteId && w.Qbicle.Id == qbicleId).ToList();
                    foreach (var item in wks_group_topic)
                    {
                        item.Topic = topicMove;
                        if (DbContext.Entry(item).State == EntityState.Detached)
                            DbContext.WorkGroups.Attach(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                    }
                    //Move Activities into topic new
                    var activities_topic = DbContext.Activities.Where(w => w.Topic.Id == topicDeleteId && w.Qbicle.Id == qbicleId).ToList();
                    foreach (var item in activities_topic)
                    {
                        item.Topic = topicMove;
                        if (DbContext.Entry(item).State == EntityState.Detached)
                            DbContext.Activities.Attach(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                    }
                    var posts = DbContext.Posts.Where(w => w.Topic.Id == topicDeleteId).ToList();
                    foreach (var item in posts)
                    {
                        item.Topic = topicMove;
                        if (DbContext.Entry(item).State == EntityState.Detached)
                            DbContext.Posts.Attach(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                    }
                    var topicDelete = DbContext.Topics.Find(topicDeleteId);
                    if (topicDelete != null)
                    {
                        DbContext.Topics.Remove(topicDelete);
                    }
                    DbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, topicMoveId, topicDeleteId, qbicleId);
                return false;
            }
        }
        public bool MoveAtivityTopic(int topicMoveId, int topicCurrentId, int activityId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MoveAtivityTopic", null, null, topicMoveId, topicCurrentId, activityId);

                var topic_move = DbContext.Topics.Find(topicMoveId);
                if (topic_move != null && topic_move.Id != topicCurrentId)
                {
                    var activity = DbContext.Activities.Find(activityId);
                    if (activity != null)
                    {
                        activity.Topic = topic_move;
                        DbContext.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, topicMoveId, topicCurrentId, activityId);
                return false;
            }
        }
    }
}
