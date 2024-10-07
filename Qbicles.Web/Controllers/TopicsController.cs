using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TopicsController : BaseController
    {
        private ReturnJsonModel refModel;

        public ActionResult GetTopicByQbicle(int qbicleId)
        {
            refModel = new ReturnJsonModel();

            try
            {
                var topics = new TopicRules(dbContext).GetTopicByQbicle(qbicleId).Select(n => n.Name).ToList();

                refModel.Object = topics;
                refModel.result = true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTopicByQbicleId(int qbicleId, int currentTopicId)
        {
            if (qbicleId == 0)
                return Json(null, JsonRequestBehavior.AllowGet);
            var topics = new TopicRules(dbContext).GetTopicOnlyByQbicle(qbicleId);
            if (currentTopicId == 0)
                return Json(topics.Select(s => new { id = s.Id, text = s.Name, selected = s.Name == HelperClass.GeneralName ? true : false }).ToList(), JsonRequestBehavior.AllowGet);
            return Json(topics.Select(s => new { id = s.Id, text = s.Name, selected = s.Id == currentTopicId ? true : false }).ToList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetTopicNameToAssing()
        {
            refModel = new ReturnJsonModel();

            try
            {
                var topicName = new TopicRules(dbContext).GetTopicNameToAssing(CurrentQbicleId(), CurrentUser().Id);
                if (topicName != null)
                {
                    refModel.Object = topicName;
                    refModel.result = true;
                }
                else
                {
                    refModel.msg = "";
                    refModel.result = false;
                }
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult CheckDuplicateTopPicName(string topicName)
        {
            refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new TopicRules(dbContext).DuplicateTopicNameCheck(CurrentQbicleId(), topicName);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult SaveTopic(Topic topic)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var topicRule = new TopicRules(dbContext);
                var currentQbicleId = CurrentQbicleId();
                topic.Name = topic.Name.Trim('\'');
                if (topicRule.DuplicateTopicNameCheck(currentQbicleId, topic.Id, topic.Name))
                {
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_74");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (topic.Name.Length > 50)
                {
                    topic.Name = topic.Name.Substring(0, 50);
                }
                if (!string.IsNullOrEmpty(topic.Summary) && topic.Summary.Length > 250)
                {
                    topic.Summary = topic.Summary.Substring(0, 250);
                }
                var tp = dbContext.Topics.Find(topic.Id);
                if (tp != null)
                {
                    if (topic.Name != HelperClass.GeneralName && tp.Name == HelperClass.GeneralName)
                    {
                        refModel.result = false;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_102");
                        return Json(refModel, JsonRequestBehavior.AllowGet);
                    }
                }
                
                refModel.Object = new TopicRules(dbContext).SaveTopic(CurrentQbicleId(), topic, CurrentUser().Id);
                refModel.result = refModel.Object == null ? false : true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        public ActionResult SetTopicSelected(int id, string goBack)
        {
            try
            {
                refModel = new ReturnJsonModel();

                var topic = new TopicRules(dbContext).GetTopicById(id);
                refModel.result = true;
                refModel.msgId = topic.Qbicle.Domain.Id.ToString();
                SetCurrentQbicleIdCookies(topic.Qbicle?.Id ?? 0);
                SetCurrentTopicIdCookies(id);
                SetCookieGoBackPage(goBack);

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, id);
                return View("Error");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult MyDeskLoadMorePosts(int skip, int folderId)
        {
            try
            {
                var currentUserId = CurrentUser().Id;
                var postTopic = new MyDesksRules(dbContext).GetPostsByUserId(currentUserId, folderId, skip,
                    HelperClass.myDeskPageSize);

                var querry = from pin in dbContext.MyPins
                             join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                             where desk.Owner.Id == currentUserId && pin.PinnerPost != null
                             select pin.PinnerPost;

                var pinnedTopicPosts = querry.ToList();

                var result = postTopic.Select(x => new
                {
                    IsPinned = pinnedTopicPosts.Any(y => y.Id == x.Id),
                    x.Id,
                    QbicleName = x.Topic.Qbicle?.Name,
                    DomainName = x.Topic.Qbicle?.Domain.Name,
                    ToppicName = x.Topic.Name,
                    TopicId = x.Topic.Id,
                    x.Message
                }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult GetTopic2SelectByQbicle(int qbicleId)
        {
            refModel = new ReturnJsonModel();

            try
            {
                var topics = new TopicRules(dbContext).GetTopicByQbicle(qbicleId).ToList();
                var str = new StringBuilder();
                foreach (var tp in topics)
                {
                    str.AppendFormat($"<option value='{tp.Id}'>{tp.Name}</option>");
                }
                refModel.Object = str.ToString();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTopicDelMoveByQbicle(string key, int topicId)
        {
            refModel = new ReturnJsonModel();

            try
            {
                int qbicleId = int.Parse(key.Decrypt());
                var topics = new TopicRules(dbContext).GetTopicByQbicle(qbicleId, topicId).ToList();
                var str = new StringBuilder();
                foreach (var tp in topics)
                {
                    str.AppendFormat($"<option value='{tp.Id}'>{tp.Name}</option>");
                }
                refModel.Object = str.ToString();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadTopicById(int id)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var topic = new TopicRules(dbContext).GetTopicById(id);
                if (topic != null)
                {
                    refModel.result = true;
                    refModel.Object = new { topic.Id, topic.Name, topic.Summary };
                }
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CountAssociatedActivities(int id)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var countActivities = new TopicRules(dbContext).CountActivitiesByTopic(id);
                refModel.result = true;
                refModel.Object = new { countActivities };
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteTopic(int topicMoveId, int topicCurrentId, string key)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (topicMoveId == topicCurrentId)
                {
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_103");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                var qbicleId = int.Parse(key.Decrypt());
                refModel.result = new TopicRules(dbContext).DeleteTopic(topicMoveId, topicCurrentId, qbicleId);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MoveAtivityTopic(int topicMoveId, int topicCurrentId, int activityId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (topicMoveId == topicCurrentId)
                {
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_103");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                refModel.result = new TopicRules(dbContext).MoveAtivityTopic(topicMoveId, topicCurrentId, activityId);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
    }
}