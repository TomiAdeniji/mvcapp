using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Enums;
using static Qbicles.BusinessRules.HelperClass;


namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SalesMarketingLocationController : BaseController
    {
        private ReturnJsonModel refModel;
        public ActionResult PlaceDetail(int placeId)
        {
            var rule = new SocialLocationRule(dbContext);
            var place = rule.GetPlaceById(placeId);
            if (place != null)
                return View(place);
            else
                return View("Error");
        }

        public ActionResult GenerateModalTask(string taskKey, int placeId)
        {
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                var place = new SocialLocationRule(dbContext).GetPlaceById(placeId);
                var mktSettings = dbContext.SalesMarketingSettings.FirstOrDefault(p => p.Domain.Id == place.Domain.Id);
                ViewBag.MarketingSetting = mktSettings;

                var recurrance = new RecurranceRules(dbContext).GetRecurranceById(0);
                ViewBag.lstMonth = Utility.GetListMonth(DateTime.UtcNow);
                ViewBag.Recurrance = recurrance;
                ViewBag.taskId = taskId;
                ViewBag.taskKey = taskKey;
                ViewBag.placeId = placeId;
                return PartialView("_ModalTask");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, User.Identity.GetUserId());
                return null;
            }
        }

        public ActionResult SaveSMLocationQbicleTask(QbicleTask task, string Assignee, string ProgrammedStart, string[] Watchers,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize,
            int qbicleId,  int TopicId, int[] ActivitiesRelate, List<QbicleStep> Steplst, int? Type, string LastOccurrence, string DayOrMonth, int? pattern, int? customDate, string dayofweek, List<string> listDate, short? monthdates, long placeId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var user = CurrentUser();
                string currentDatetimeFormat = user.DateTimeFormat;
                DateTime dtLastOccurrence = DateTime.UtcNow;
                var lstDate = new List<CustomDateModel>();
                if (string.IsNullOrEmpty(task.Name) || string.IsNullOrEmpty(task.Description))
                {
                    refModel.result = false;
                    refModel.msg = "Request to enter information!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (ActivitiesRelate != null && ActivitiesRelate.Count() > 31)
                {
                    refModel.result = false;
                    refModel.msg = "Data associate activities cannot be greater than 31 records!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }

                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };

                var tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                if (!string.IsNullOrEmpty(ProgrammedStart))
                {
                    try
                    {
                        task.ProgrammedStart = TimeZoneInfo.ConvertTimeToUtc(ProgrammedStart.ConvertDateFormat(currentDatetimeFormat), tz);

                    }
                    catch
                    {
                        task.ProgrammedStart = DateTime.UtcNow;
                    }
                }
                try
                {
                    if (!string.IsNullOrEmpty(LastOccurrence))
                        dtLastOccurrence = TimeZoneInfo.ConvertTimeToUtc(LastOccurrence.ConvertDateFormat(user.DateFormat), tz);
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
                                    StartDate = TimeZoneInfo.ConvertTimeToUtc(item.ConvertDateFormat(currentDatetimeFormat), tz)
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
                var _recurrance = new QbicleRecurrance
                {
                    Days = Type == 0 || Type == 1 ? DayOrMonth : "",
                    Months = Type == 2 ? DayOrMonth : "",
                    FirstOccurrence = task.ProgrammedStart ?? DateTime.UtcNow,
                    LastOccurrence = dtLastOccurrence,
                    MonthDate = monthdates.HasValue ? monthdates.Value : (short)0
                };
                if (Type != null)
                    _recurrance.Type = (QbicleRecurrance.RecurranceTypeEnum)Type;
                if (Type == 2)
                    _recurrance.Pattern = (short)pattern;

                refModel.result = new SocialLocationRule(dbContext).
                    SaveSMLocationQbicleTask(task, Assignee, media, Watchers, CurrentQbicleId(),
                    user.Id, TopicId, ActivitiesRelate, Steplst, _recurrance, lstDate, placeId);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
            if (refModel.result)
                return Json(refModel, JsonRequestBehavior.AllowGet);
            else
                return null;
        }

        public PartialViewResult LoadModalActivity(int placeId)
        {
            var rule = new SocialLocationRule(dbContext);
            ViewBag.placeId = placeId;
            return PartialView("_AddActivity");

        }

        public PartialViewResult LoadModalVisit(int placeId)
        {
            var rule = new SocialLocationRule(dbContext);
            var place = rule.GetPlaceById(placeId);
            return PartialView("_AddVisit", place);
        }

        [HttpPost]
        public ActionResult LoadScheduledVisits([Bind(Prefix = "search[value]")] string search, int draw, int placeId, int start, int length)
        {
            var totalRecord = 0;
            List<ScheduledVisitModel> lstResult = new SocialLocationRule(dbContext).GetAllScheduledVisits(placeId, start, length, ref totalRecord);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadActivityLogs([Bind(Prefix = "search[value]")] string search, int draw, int placeId, int start, int length)
        {
            var totalRecord = 0;
            List<PlaceActivityModel> lstResult = new SocialLocationRule(dbContext).GetListActivityLog(placeId, start, length, ref totalRecord);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadVisitLogs([Bind(Prefix = "search[value]")] string search, int draw, int placeId, int start, int length)
        {
            var totalRecord = 0;
            List<VisitLogModel> lstResult = new SocialLocationRule(dbContext).GetListVisitLogs(placeId, start, length, ref totalRecord);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SaveActivityLogs(PlaceActivityModel activityLogs)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                var user = CurrentUser();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                var startDate = DateTime.UtcNow;
                var endDate = DateTime.UtcNow;
                activityLogs.Date.ConvertDaterangeFormat(user.DateTimeFormat, user.Timezone, out startDate, out endDate);
                activityLogs.StartDate = startDate;
                activityLogs.EndDate = endDate;
                refModel = new SocialLocationRule(dbContext).SaveActivityLogs(activityLogs, user.Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveVisitLogs(VisitLogModel visitLogs)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                var user = CurrentUser();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                if (!string.IsNullOrEmpty(visitLogs.DateTimeOfVisit))
                {
                    visitLogs.VisitDate = TimeZoneInfo.ConvertTimeToUtc(visitLogs.DateTimeOfVisit.ConvertDateFormat(user.DateTimeFormat), tz);
                }
                switch (visitLogs.txtReason)
                {
                    case "1": visitLogs.Reason = VisitReason.AssignedTask; break;
                    case "2": visitLogs.Reason = VisitReason.AdvertisingStall; break;
                    case "3": visitLogs.Reason = VisitReason.ColdCalling; break;
                    case "4": visitLogs.Reason = VisitReason.LeafletDistribution; break;
                    case "5": visitLogs.Reason = VisitReason.Other; break;

                }
                refModel = new SocialLocationRule(dbContext).SaveVisitLogs(visitLogs, user.Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DiscussionPlace(int disId)
        {
            var place = new SocialLocationRule(dbContext).GetPlaceByActivityId(disId);
            if (place != null)
            {
                var currentDomainId = place?.Domain.Id ?? 0;
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(currentDomainId);
                ValidateCurrentDomain(place?.Domain, setting?.SourceQbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.SalesAndMarketingAccess))
                    return View("ErrorAccessPage");
                ViewBag.Setting = setting;
                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(setting?.SourceQbicle.Id ?? 0);
                
                ViewBag.CurrentPage = "SocialPostDiscussion"; SetCurrentPage("SocialPostDiscussion");
                
                SetCurrentDiscussionIdCookies(place.Discussion?.Id ?? 0);
                return View(place);
            }
            else
                return View("Error");
        }



    }
}