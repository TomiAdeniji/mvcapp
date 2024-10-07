using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class EventsController : BaseController
    {
        private ReturnJsonModel _refModel;


        public ActionResult DuplicateEventNameCheck(int cubeId, string eventKey, string eventName)
        {
            var eventId = string.IsNullOrEmpty(eventKey) ? 0 : int.Parse(eventKey.Decrypt());
            _refModel = new ReturnJsonModel();
            try
            {
                _refModel.result = new EventsRules(dbContext).DuplicateEventNameCheck(cubeId, eventId, eventName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                _refModel.result = false;
                _refModel.Object = ex;
            }

            return Json(_refModel, JsonRequestBehavior.AllowGet);
        }



        public ActionResult SaveEvent(QbicleEvent qEvent, string[] sendInvitesTo, int[] ActivitiesRelate,
            string eventStart,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize, int TopicId,
            //Recurrence
            int? Type, string LastOccurrence, string DayOrMonth,
            int? pattern, List<string> listDate, short? monthdates)
        {
            _refModel = new ReturnJsonModel();
            try
            {
                var currentDatetimeFormat = CurrentUser().DateTimeFormat;
                var currentDateFormat = CurrentUser().DateFormat;
                DateTime dtLastOccurrence = DateTime.UtcNow;
                var lstDate = new List<CustomDateModel>();
                if (string.IsNullOrEmpty(qEvent.Name))
                {
                    _refModel.result = false;
                    _refModel.msg = ResourcesManager._L("ERROR_MSG_238");
                    return Json(_refModel, JsonRequestBehavior.AllowGet);
                }
                if (ActivitiesRelate != null && ActivitiesRelate.Count() > 31)
                {
                    _refModel.result = false;
                    _refModel.msg = ResourcesManager._L("ERROR_MSG_95");
                    return Json(_refModel, JsonRequestBehavior.AllowGet);
                }
                //valid event dates
                var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                try
                {
                    qEvent.Start = TimeZoneInfo.ConvertTimeToUtc(eventStart.ConvertDateFormat(currentDatetimeFormat), tz);
                }
                catch
                {
                    qEvent.Start = DateTime.UtcNow;
                }

                QbicleRecurrance recurrance = null;
                if (qEvent.isRecurs)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(LastOccurrence))
                            dtLastOccurrence = TimeZoneInfo.ConvertTimeToUtc(LastOccurrence.ConvertDateFormat(currentDateFormat), tz);
                        if (listDate != null && listDate.Any())
                        {
                            var arrDate = listDate[0].Split(',');
                            if (arrDate != null)
                            {
                                CustomDateModel cDate;
                                foreach (var item in arrDate)
                                {
                                    cDate = new CustomDateModel
                                    {
                                        StartDate = TimeZoneInfo.ConvertTimeToUtc(item.ConvertDateFormat(currentDateFormat), tz)
                                    };
                                    lstDate.Add(cDate);
                                }
                            }
                        }
                    }
                    catch
                    {
                        dtLastOccurrence = DateTime.UtcNow;
                    }
                    recurrance = new QbicleRecurrance
                    {
                        Days = Type == 0 || Type == 1 ? DayOrMonth : "",
                        Months = Type == 2 ? DayOrMonth : "",
                        FirstOccurrence = qEvent.Start,
                        LastOccurrence = dtLastOccurrence,
                        MonthDate = monthdates.HasValue ? monthdates.Value : (short)0
                    };
                    if (Type != null)
                        recurrance.Type = (QbicleRecurrance.RecurranceTypeEnum)Type;
                    if (Type == 2)
                        recurrance.Pattern = (short)pattern;
                }


                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(System.IO.Path.GetExtension(mediaObjectName))
                };

                _refModel = new EventsRules(dbContext).SaveEvent(qEvent,
                    eventStart, CurrentQbicleId(), sendInvitesTo, ActivitiesRelate, media, CurrentUser().Id, TopicId, recurrance, lstDate, GetOriginatingConnectionIdFromCookies(), AppType.Web);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                _refModel.result = false;
                _refModel.msg = ex.Message;
                _refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(_refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CountCanEventsDelete(int ceventId)
        {
            try
            {
                return Json(new EventsRules(dbContext).CountCanEventsDelete(ceventId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RecurringEventsDelete(int ceventId)
        {
            try
            {
                var result = new EventsRules(dbContext).RecurringEventsDelete(ceventId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SetEventSelected(string key, string goBack)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                //Check for activity accessibility
                var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(id, CurrentUser().Id);
                if (checkResult.result && (bool)checkResult.Object == true)
                {
                    _refModel = new ReturnJsonModel();

                    var ev = new EventsRules(dbContext).GetEventById(id);
                    _refModel.msgId = ev.Qbicle?.Domain?.Id.ToString();
                    _refModel.result = true;
                    SetCurrentQbicleIdCookies(ev.Qbicle?.Id ?? 0);
                    SetCurrentEventIdCookies(id);
                    SetCookieGoBackPage(goBack);

                    return Json(_refModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(checkResult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }


        public bool CannotAttend(int eventId)
        {
            try
            {
                if (CurrentUser().Id != null && eventId > 0)
                {
                    var rs = new EventsRules(dbContext).CannotAttend(eventId, CurrentUser().Id);
                    return rs;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }


        public ActionResult UpdateEvent(QbicleEvent qEvent)
        {
            var result = new ReturnJsonModel();
            try
            {
                result.actionVal = 1;
                if (qEvent.Id <= 0) return Json(result, JsonRequestBehavior.AllowGet);
                var eventAdapter = new EventsRules(dbContext);
                eventAdapter.UpdateEvent(qEvent);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result.actionVal = 2;
                result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateAttend(int peopleId, bool isPresent)
        {
            var result = new ReturnJsonModel();
            try
            {

                var eventAdapter = new EventsRules(dbContext);
                result.result = eventAdapter.UpdateAttend(peopleId, isPresent);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result.result = false;
                result.actionVal = 2;
                result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}