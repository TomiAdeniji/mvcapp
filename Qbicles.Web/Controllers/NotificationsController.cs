using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        ReturnJsonModel refModel;

        /// <summary>
        /// Get notification by id to render UI
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetNotificationById(int id)
        {
            var clientInfo = new ClientInfoModel
            {
                CurrentTaskId = CurrentTaskId(),
                CurrentMediaId = CurrentMediaId(),
                CurrentEventId = CurrentEventId(),
                CurrentLinkId = CurrentLinkId(),
                CurrentApprovalId = CurrentApprovalId(),
                CurrentJournalEntryId = CurrentJournalEntryId(),
                CurrentAlertId = CurrentAlertId(),
                CurrentDiscussionId = CurrentDiscussionId(),
                CurrentUserId = CurrentUser().Id
            };

            var currentQbicleId = CurrentQbicleId();
            var currentDomainId = CurrentDomainId();
            var notifi = new NotificationRules(dbContext).GetNotificationById(id, true);
            var jsonObject = new
            {
                notifi.HtmlNotification,
                notifi.Event,
                notifi.AssociatedById,
                notifi.AppendToPageName,
                notifi.AppendToPageId,
                notifi.ElementId,
                notifi.HasActionToHandle,
                notifi.IsCreatorTheCustomer,
                notifi.CreatorTheQbcile,
                notifi.IsAlertDisplay,
                IsCurrentQbicle = currentQbicleId != 0 && currentQbicleId == notifi.CurrentQbicleId,
                IsCurrentDomain = currentDomainId != 0 && currentDomainId == notifi.CurrentDomainId,
                IsCurrentTask = clientInfo.CurrentTaskId != 0 && (clientInfo.CurrentTaskId == notifi.AppendToPageId || clientInfo.CurrentTaskId == notifi.ElementId),
                IsCurrentAlert = clientInfo.CurrentAlertId != 0 && (clientInfo.CurrentAlertId == notifi.AppendToPageId || clientInfo.CurrentAlertId == notifi.ElementId),
                IsCurrentEvent = clientInfo.CurrentEventId != 0 && (clientInfo.CurrentEventId == notifi.AppendToPageId || clientInfo.CurrentEventId == notifi.ElementId),
                IsCurrentMedia = clientInfo.CurrentMediaId != 0 && (clientInfo.CurrentMediaId == notifi.AppendToPageId || clientInfo.CurrentMediaId == notifi.ElementId),
                IsCurrentLink = clientInfo.CurrentLinkId != 0 && (clientInfo.CurrentLinkId == notifi.AppendToPageId || clientInfo.CurrentLinkId == notifi.ElementId),
                IsCurrentApproval = clientInfo.CurrentApprovalId != 0 && (clientInfo.CurrentApprovalId == notifi.AppendToPageId || clientInfo.CurrentApprovalId == notifi.ElementId),
                IsCurrentDiscussion = clientInfo.CurrentDiscussionId != 0 && (clientInfo.CurrentDiscussionId == notifi.AppendToPageId || clientInfo.CurrentDiscussionId == notifi.ElementId),
                IsCurrentJournalEntry = clientInfo.CurrentJournalEntryId != 0 && (clientInfo.CurrentJournalEntryId == notifi.AppendToPageId || clientInfo.CurrentJournalEntryId == notifi.ElementId),
                IsCurrentCreator = clientInfo.CurrentUserId != "" && clientInfo.CurrentUserId == notifi.CreatedById,
                CurrentConnectionId = GetOriginatingConnectionIdFromCookies(),
                IsCurrentAssociator = clientInfo.CurrentUserId != "" && clientInfo.CurrentUserId == notifi.AssociatedById,
                CurrentQbicleKey = notifi.CurrentQbicleId.Encrypt()//Use for clients Order by latest activity of the Qbicle
            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check if there are unread notifications
        /// </summary>
        /// <returns></returns>
        public bool CheckUnReadNotification()
        {
            var notifiRule = new NotificationRules(dbContext);
            return notifiRule.GetNotificationByUser(CurrentUser().Id, false) > 0;
        }


        public ActionResult ShowNotificationsModal(PaginationRequest pagination)
        {
            return Json(new NotificationRules(dbContext).ShowNotificationsModal(pagination, CurrentUser().Id, false, CurrentUser().Timezone), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteAllNotifications()
        {
            var result = new NotificationRules(dbContext).DeleteAllNotificationByUser(CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// mark a activity notification
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>

        public ActionResult MarkAsReadNotification(int notificationId, string activityKey)
        {
            try
            {
                var activityId = string.IsNullOrEmpty(activityKey) ? 0 : int.Parse(EncryptionService.Decrypt(activityKey));
                refModel = new ReturnJsonModel();
                new NotificationRules(dbContext).MarkAsReadNotification(notificationId, activityId);
                var notifiRule = new NotificationRules(dbContext);
                refModel.Object = notifiRule.GetNotificationByUser(CurrentUser().Id, false) > 0;
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// mark multiple activity notification
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>

        public ActionResult MarkAsMultipleReadNotification(string strType)
        {
            try
            {
                refModel = new ReturnJsonModel();
                var totalCount = 0;
                if (!string.IsNullOrEmpty(strType))
                {
                    var arr = strType.Split(',');
                    for (var i = 0; i < arr.Length; ++i)
                    {
                        var arrType = arr[i].Split('_');
                        if (arrType.Length > 1)
                        {
                            totalCount += new NotificationRules(dbContext).MarkAsReadNotification(Convert.ToInt32(arrType[0]));
                        }
                    }
                }
                var notifiRule = new NotificationRules(dbContext);
                refModel.Object = notifiRule.GetNotificationByUser(CurrentUser().Id, false) > 0;
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// add id to cookie and get data for display alert
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult NewAlertNotification(int id)
        {
            try
            {
                refModel = new ReturnJsonModel();

                var alertNotification = AlertNotificationCookieGet();
                alertNotification.Ids.Add(id);
                AlertNotificationCookieSet(alertNotification);
                var notifyCircleStyle = "";
                var notification = new NotificationRules(dbContext).GetAlertNotification(CurrentUser().Timezone, id, CurrentUser().Id, ref notifyCircleStyle);

                alertNotification.NotifyCircleClass = notifyCircleStyle;
                AlertNotificationCookieSet(alertNotification);

                return Json(notification, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get all alert into modal dialog
        /// </summary>
        /// <returns></returns>
        public ActionResult GetListAlertNotification(PaginationRequest pagination)
        {
            try
            {
                var alertNotification = AlertNotificationCookieGet();
                var notification = new NotificationRules(dbContext).GetListAlertNotification(pagination, CurrentUser().Timezone, alertNotification);
                notification.IsShowAlertBusiness = alertNotification.IsShowAlertBusiness;
                notification.IsShowAlertCustomer = alertNotification.IsShowAlertCustomer;
                return Json(notification, JsonRequestBehavior.AllowGet);
                //return Json(new { data = notification, IsShowAlertBusiness = alertNotification.IsShowAlertBusiness, IsShowAlertCustomer = alertNotification.IsShowAlertCustomer }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
            }
            return View("Error");
        }

        public ActionResult UpdateAlertFilter(bool isShowAlertBusiness, bool isShowAlertCustomer, bool isClear)
        {
            var alertNotification = AlertNotificationCookieGet();
            if (isClear)
                alertNotification.Ids.Clear();
            alertNotification.IsShowAlertBusiness = isShowAlertBusiness;
            alertNotification.IsShowAlertCustomer = isShowAlertCustomer;
            AlertNotificationCookieSet(alertNotification);
            return Json("Done", JsonRequestBehavior.AllowGet);
        }
    }
}