using Qbicles.BusinessRules;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Operator;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.EnterpriseServices;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Ast;
using static Qbicles.Models.Notification;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class QbicleCommentsController : BaseController
    {
        public ActionResult SignalRTyping(string toUsers, bool typing, int discussionId = 0)
        {
            new NotificationRules(dbContext).TypingChat(CurrentUser(), toUsers, typing ? NotificationEventEnum.TypingChat : NotificationEventEnum.EndTypingChat, discussionId);
            return Json(new { Status = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Qbicles Dashboard/ Community/B2C/B2B Chat for add post with TopicId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public ActionResult AddPostWithTopicId(string message, int topicId)
        {
            var refModel = new PostsRules(dbContext).SavePostTopic(IsCreatorTheCustomer(), message, topicId, CurrentUser().Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies(), AppType.Web);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Qbicles Dashboard/ Community/B2C/B2B Chat for add post with TopicName
        /// </summary>
        /// <param name="message"></param>
        /// <param name="topicName"></param>
        /// <returns></returns>
        public ActionResult AddPostToTopic(string message, string topicName)
        {
            var refModel = new PostsRules(dbContext).SavePostTopic(IsCreatorTheCustomer(), message, topicName, CurrentUser().Id, CurrentQbicleId(), GetOriginatingConnectionIdFromCookies(), AppType.Web);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddCommentB2BDiscussion(string message, string disKey)
        {
            try
            {
                var result = new ReturnJsonModel { result = false };
                var isCreatorTheCustomer = IsCreatorTheCustomer();
                var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new DiscussionsRules(dbContext).AddComment(isCreatorTheCustomer, disKey, post, GetOriginatingConnectionIdFromCookies(), ApplicationPageName.DiscussionOrder, false, true, GetOriginatingConnectionIdFromCookies(), AppType.Web);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult AddComment2Discussion(string message, string disKey)
        {
            try
            {
                var result = new ReturnJsonModel { result = false };
                var isCreatorTheCustomer = IsCreatorTheCustomer();
                var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new DiscussionsRules(dbContext).AddComment(isCreatorTheCustomer, disKey, post, GetOriginatingConnectionIdFromCookies(), ApplicationPageName.Discussion, false, true, GetOriginatingConnectionIdFromCookies(), AppType.Web);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult AddComment2DiscussionOrder(string message, string disKey)
        {
            try
            {
                var result = new ReturnJsonModel { result = false };
                var isCreatorTheCustomer = IsCreatorTheCustomer();
                var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, CurrentUser().Id, CurrentQbicleId());

                //new DiscussionsRules(dbContext).AddCommentAsync(isCreatorTheCustomer, disKey, post, GetHubIdFromCookies(), ApplicationPageName.DiscussionOrder, false, false, GetHubIdFromCookies(), AppType.Web);
                new DiscussionsRules(dbContext).AddComment(isCreatorTheCustomer, disKey, post, GetOriginatingConnectionIdFromCookies(), ApplicationPageName.DiscussionOrder, false, false, GetOriginatingConnectionIdFromCookies(), AppType.Web);


                //var notification = new ISignalRNotification().HtmlRender(post, post.CreatedBy, ApplicationPageName.DiscussionOrder, NotificationEventEnum.DiscussionUpdate);


                var notification = new Notification
                {
                    AssociatedQbicle = post.Topic.Qbicle,
                    CreatedBy = post.CreatedBy,
                    IsCreatorTheCustomer = post.IsCreatorTheCustomer,
                };

                result = new ReturnJsonModel
                {
                    result = true,
                    msg = new ISignalRNotification().HtmlRender(post, post.CreatedBy, ApplicationPageName.DiscussionOrder, NotificationEventEnum.DiscussionUpdate, notification)
                };
                dbContext.Entry(post).State = EntityState.Unchanged;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult AddBulkComment2DiscussionOrder(List<string> messages, string disKey)
        {
            try
            {
                var results = new ReturnJsonModel { result = false };
                List<string> result = new List<string>();
                var isCreatorTheCustomer = IsCreatorTheCustomer();

                foreach (var message in messages)
                {
                    var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, CurrentUser().Id, CurrentQbicleId());
                    new DiscussionsRules(dbContext).AddComment(isCreatorTheCustomer, disKey, post, GetOriginatingConnectionIdFromCookies(), ApplicationPageName.DiscussionOrder, false, false, GetOriginatingConnectionIdFromCookies(), AppType.Web);
                    var notification = new Notification
                    {
                        AssociatedQbicle = post.Topic.Qbicle,
                        CreatedBy = post.CreatedBy,
                        IsCreatorTheCustomer = post.IsCreatorTheCustomer,
                    };

                    result.Add(new ISignalRNotification().HtmlRender(post, post.CreatedBy, ApplicationPageName.DiscussionOrder, NotificationEventEnum.DiscussionUpdate, notification));
                    dbContext.Entry(post).State = EntityState.Unchanged;
                }
                results = new ReturnJsonModel
                {
                    result = true,
                    Object = result
                };
                return Json(results, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        /// <summary>
        /// Add comment to the Activity Approval page
        /// </summary>
        /// <param name="message"></param>
        /// <param name="approvalKey"></param>
        /// <returns></returns>
        public ActionResult AddCommentToApproval(string message, string approvalKey)
        {
            var approvalId = string.IsNullOrEmpty(approvalKey) ? 0 : int.Parse(approvalKey.Decrypt());
            SetCurrentApprovalIdCookies(approvalId);
            var appRules = new ApprovalsRules(dbContext);
            var result = appRules.AddPostToApproval(IsCreatorTheCustomer(), message, approvalId, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddCommentToTask(string message, string taskKey)
        {
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                var result = new ReturnJsonModel { result = true };
                var post = new PostsRules(dbContext).SavePost(IsCreatorTheCustomer(), message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new TasksRules(dbContext).AddPostToTask(taskId, post, GetOriginatingConnectionIdFromCookies(), AppType.Web);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult AddCommentJournalEntry(string message, int id)
        {
            var result = new PostsRules(dbContext).SavePostJournal(message, CurrentUser().Id, id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddCommentToSocialPostDiscussion(string message, string socialPostKey)
        {
            try
            {
                var socialPostId = string.IsNullOrEmpty(socialPostKey) ? 0 : int.Parse(socialPostKey.Decrypt());
                var result = false;
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
                if (setting != null)
                {
                    var user = CurrentUser();
                    var post = new PostsRules(dbContext).SavePost(IsCreatorTheCustomer(), message, CurrentUser().Id, CurrentQbicleId());
                    if (post != null)
                        result = new CMsRules(dbContext).AddPostToSocialPostDiscussion(socialPostId, post,
                            user.Id, setting.SourceQbicle.Id);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult AddCommentToSocialPost(string message, string socialPostKey)
        {
            try
            {
                var socialPostId = string.IsNullOrEmpty(socialPostKey) ? 0 : int.Parse(socialPostKey.Decrypt());
                var result = false;
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
                if (setting != null)
                {
                    var isCreatorTheCustomer = IsCreatorTheCustomer();
                    var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, CurrentUser().Id, CurrentQbicleId());
                    if (post != null)
                        result = new DiscussionsRules(dbContext).AddComment(isCreatorTheCustomer, socialPostId.Encrypt(), post, GetOriginatingConnectionIdFromCookies()).result;
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult AddPostToPerformanceDiscussion(string message, int performanceId)
        {
            try
            {
                var result = false;
                var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(CurrentDomainId());
                if (setting != null)
                {
                    string timezone = CurrentUser().Timezone;
                    var post = new PostsRules(dbContext).SavePost(IsCreatorTheCustomer(), message, CurrentUser().Id, setting.SourceQbicle.Id);
                    if (post != null)
                        result = new OperatorPerformanceTrackingRules(dbContext).AddPostToPerformanceDiscussion(performanceId, post,
                            CurrentUser().Id, setting.SourceQbicle.Id);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AddCommentToLink(string message, string linkKey)
        {
            try
            {
                var linkId = string.IsNullOrEmpty(linkKey) ? 0 : int.Parse(linkKey.Decrypt());
                var result = new ReturnJsonModel { result = true };

                var isCreatorTheCustomer = IsCreatorTheCustomer();
                var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new DiscussionsRules(dbContext).AddComment(isCreatorTheCustomer, linkId.Encrypt(), post, GetOriginatingConnectionIdFromCookies(), ApplicationPageName.Link, false, true, GetOriginatingConnectionIdFromCookies(), AppType.Web);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }


        public ActionResult AddCommentToEvent(string message, string eventKey)
        {
            try
            {
                var eventId = string.IsNullOrEmpty(eventKey) ? 0 : int.Parse(eventKey.Decrypt());
                var result = new ReturnJsonModel { result = true };
                var post = new PostsRules(dbContext).SavePost(IsCreatorTheCustomer(), message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new EventsRules(dbContext).AddPostToEvent(eventId, post, GetOriginatingConnectionIdFromCookies(), AppType.Web);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }
        public ActionResult AddCommentToAlert(string message, int alertId)
        {
            try
            {
                var result = false;
                var post = new PostsRules(dbContext).SavePost(IsCreatorTheCustomer(), message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new AlertsRules(dbContext).AddPostToAlert(alertId, post);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AddCommentToMedia(string message, string mediaKey)
        {
            try
            {
                var mediaId = string.IsNullOrEmpty(mediaKey) ? 0 : int.Parse(mediaKey.Decrypt());
                var result = new ReturnJsonModel { result = true };
                var post = new PostsRules(dbContext).SavePost(IsCreatorTheCustomer(), message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new MediasRules(dbContext).AddPostToMedia(mediaId, post, GetOriginatingConnectionIdFromCookies(), AppType.Web);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult AddCommentTransaction(string message, int id)
        {
            var result = new PostsRules(dbContext).SavePostTransaction(message, CurrentUser().Id, id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePost(string key, string message, int topicId)
        {
            var postId = 0;
            if (!string.IsNullOrEmpty(key?.Trim()))
            {
                postId = Int32.Parse(EncryptionService.Decrypt(key));
            }
            var refModel = new PostsRules(dbContext).UpdatePost(postId, message, topicId, CurrentUser().Id, GetOriginatingConnectionIdFromCookies());
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddComment2DiscussionMenu(string message, string disKey)
        {
            try
            {
                var result = new ReturnJsonModel { result = false };
                var isCreatorTheCustomer = IsCreatorTheCustomer();
                var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, CurrentUser().Id, CurrentQbicleId());
                if (post != null)
                    result = new DiscussionsRules(dbContext).AddComment(isCreatorTheCustomer, disKey, post, GetOriginatingConnectionIdFromCookies(), ApplicationPageName.DiscussionMenu, false, true, GetOriginatingConnectionIdFromCookies(), AppType.Web);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }
    }
}