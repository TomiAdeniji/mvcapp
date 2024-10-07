using Qbicles.BusinessRules;
using System;
using System.IO;
using System.Web.Mvc;
using System.Linq;
using System.Web.Script.Serialization;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.BusinessRules.Helper;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class MyDesksController : BaseController
    {
        ReturnJsonModel refModel;


        public ActionResult PinnedActivity(int ActivityId, bool IsPost, int? mydeskId)
        {
            try
            {
                refModel = new ReturnJsonModel();
                refModel.result = new MyDesksRules(dbContext).PinnedActivity(ActivityId, CurrentUser().Id, IsPost, mydeskId);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult DeleteFolder(int folderId)
        {
            refModel = new ReturnJsonModel();
            var removed = new MyDesksRules(dbContext).DeleteFolder(folderId);
            refModel.result = removed;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SaveFolder(int folderId, string folderName)
        {
            try
            {
                refModel = new ReturnJsonModel();
                var activitiesCount = 0;
                var savedId = new MyDesksRules(dbContext).SaveFolder(folderId, folderName, ref activitiesCount, CurrentUser().Id);
                refModel.actionVal = savedId;


                System.Text.StringBuilder newLi = new System.Text.StringBuilder();
                if (folderId == 0)
                    newLi.AppendFormat("<li id='folder-{0}' folderName='{1}' onclick='showMyDeskByFolderId({2})'>", savedId, folderName, savedId);

                newLi.Append("<a href='javascript:void(0);'>");
                newLi.AppendFormat("<img src='/Content/DesignStyle/img/icon_folder.png'><span>{0}</span>", activitiesCount);
                if (folderId == 0)
                {
                    newLi.AppendFormat("<p>{0}</p></a></li>", folderName);
                    string optionMoveFolder = "<option id='ddlMove-" + savedId + "' value='" + savedId + "'>" + folderName + "</option>";
                    refModel.Object = optionMoveFolder;
                }
                else
                {
                    newLi.AppendFormat("<p>{0}</p></a>", folderName);
                }
                refModel.msg = newLi.ToString();
                refModel.result = true;
                if (refModel.actionVal == -1)
                    refModel.result = false;

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult CheckDuplicateFolder(int folderId, string folderName)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
            refModel = new ReturnJsonModel();
            var exist = new MyDesksRules(dbContext).CheckDuplicateFolder(folderId, folderName, CurrentUser().Id);

            refModel.result = exist;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult MyDeskLoadMoreAME(int skip, int folderId)
        {
            try
            {
                var listAc = new MyDesksRules(dbContext).MyDeskLoadMoreAME(skip, folderId, CurrentUser().Id);
                return Json(listAc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }


        [HttpGet]
        public ActionResult MyDeskLoadMorePins(string model, int skip, int myDeskId, int type)
        {

            ViewBag.TabType = type;
            var arrOrder = new int[] { 0, 1, 2, 3 };
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                SearchActivityCustom searchModel = new SearchActivityCustom();
                var jvSerializer = new JavaScriptSerializer();
                if (!string.IsNullOrEmpty(model))
                    searchModel = jvSerializer.Deserialize<SearchActivityCustom>(model);
                var user = CurrentUser();
                var currentTimeZone = user.Timezone;
                var querryActivity = from pin in dbContext.MyPins
                                     join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                                     where desk.Owner.Id == user.Id && pin.PinnedActivity != null
                                     select pin.PinnedActivity;
                var myPinnedActivities = querryActivity.ToList().BusinessMapping(currentTimeZone);
                ViewBag.myPinned = myPinnedActivities;
                int totalRecord = 0;
                var partialView = "";
                var mydeskrule = new MyDesksRules(dbContext);
                if (type == 1)
                {
                    var myPins = mydeskrule.GetPinsByUserId(user.Id, skip, myDeskId, ref totalRecord, currentTimeZone, user.DateFormat, searchModel);
                        partialView = RenderViewToString("~/Views/Qbicles/_MyDeskActivityTab.cshtml", myPins);
                }
                else
                {
                    var activi = mydeskrule.GetActivityByType(user.Id, skip, type, ref totalRecord, currentTimeZone, user.DateFormat, searchModel);
                        partialView = RenderViewToString("~/Views/Qbicles/_MyDeskActivityTab.cshtml", activi);

                }
                refModel = new ReturnJsonModel() { result = false, msg = "An error" };
                refModel.Object = new
                {
                    strResult = partialView,
                    totalRecord = totalRecord

                };
                refModel.result = true;
                #region Stored UiSettings
                var cPage = "MyDesk";
                
                mydeskrule.MyDeskUiSetting(cPage, user.Id, type, searchModel);
                #endregion
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateTagsByActivity(int myDeskId, int activityId, string TagsId, bool IsPost = false)
        {
            try
            {
                var _refModel = new ReturnJsonModel();
                var arrTags = !string.IsNullOrEmpty(TagsId) ? TagsId.Split(',') : new string[] { };
                var result = new MyDesksRules(dbContext).UpdateTagsByActivity(activityId, arrTags, myDeskId, IsPost);
                if (result > 0)
                    _refModel.result = true;

                return Json(_refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult GetAllTagByMyDeskId(int myDeskId)
        {
            try
            {
                var _refModel = new ReturnJsonModel();

                var result = new MyDesksRules(dbContext).GetAllTags(myDeskId);
                _refModel.Object = result;
                _refModel.result = true;

                return Json(_refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult MyDeskLoadByDateSelected(string selectedDate, int myDeskId)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                var ProgrammedStart = DateTime.ParseExact(selectedDate, "dd/MM/yyyy", null);
                var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(ProgrammedStart, tz);
                var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(ProgrammedStart.AddDays(1), tz);
                var userId = CurrentUser().Id;

                var partialView = "";
                var activi = new MyDesksRules(dbContext).GetActivityByDueDate(startDateTimeUTC, endDateTimeUTC, userId, 0, 1);

                var currentTimeZone = CurrentUser().Timezone;
                var querryActivity = from pin in dbContext.MyPins
                                     join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                                     where desk.Owner.Id == userId && pin.PinnedActivity != null
                                     select pin.PinnedActivity;
                var myPinnedActivities = querryActivity.ToList().BusinessMapping(currentTimeZone);
                ViewBag.myPinned = myPinnedActivities;

                partialView = RenderViewToString("~/Views/Qbicles/_MyDeskActivitySideBar.cshtml", activi);

                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult MyDeskLoadUserContact(string searchName)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                var partialView = "";
                var lUser = new UserRules(dbContext).getMyContacts(CurrentUser().Id);
 
                if (!string.IsNullOrEmpty(searchName))
                {
                    searchName = searchName.ToLower();
                    lUser = lUser.Where(p => p.Forename.ToLower().Contains(searchName) || p.Surname.ToLower().Contains(searchName) || p.DisplayUserName.ToLower().Contains(searchName)).ToList();
                }
                    
                partialView = RenderViewToString("~/Views/Qbicles/_ModaContact.cshtml", lUser);

                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetListQbicleByDomainId(int domainId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var parameter = new QbicleSearchParameter
                {
                    UserId = CurrentUser().Id,
                    DomainId = domainId
                };
                refModel.Object = new QbicleRules(dbContext).FilterQbicle(parameter).Select(s => new
                {
                    s.Name,
                    s.Id
                }).ToList();

                refModel.result = true;

            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MyDeskSearchActivity(string model, int skip, int myDeskId, int type)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                //SearchActivityCustom
                SearchActivityCustom searchModel = new SearchActivityCustom();
                var jvSerializer = new JavaScriptSerializer();
                if (!string.IsNullOrEmpty(model))
                    searchModel = jvSerializer.Deserialize<SearchActivityCustom>(model);
                var userId = CurrentUser().Id;
                var currentTimeZone = CurrentUser().Timezone;
                var querryActivity = from pin in dbContext.MyPins
                                     join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                                     where desk.Owner.Id == userId && pin.PinnedActivity != null
                                     select pin.PinnedActivity;
                var myPinnedActivities = querryActivity.ToList().BusinessMapping(currentTimeZone);
                ViewBag.myPinned = myPinnedActivities;
                int totalRecord = 0;
                var partialView = "";
                var mydeskrule = new MyDesksRules(dbContext);
                if (type == 1)
                {
                    var myPins = mydeskrule.GetPinsByUserId(userId, skip, myDeskId, ref totalRecord, currentTimeZone, CurrentUser().DateFormat, searchModel);
                    partialView = RenderViewToString("~/Views/Qbicles/_MyDeskActivityTab.cshtml", myPins);
                }
                else
                {
                    var activi = mydeskrule.GetActivityByType(userId, skip, type, ref totalRecord, currentTimeZone, CurrentUser().DateFormat, searchModel);
                    partialView = RenderViewToString("~/Views/Qbicles/_MyDeskActivityTab.cshtml", activi);
                }

                refModel.Object = new
                {
                    strResult = partialView,
                    totalRecord = totalRecord

                };
                refModel.result = true;
                #region Stored UiSettings
                var cPage = "MyDesk";
                var cUser = CurrentUser().Id;
                mydeskrule.MyDeskUiSetting(cPage, cUser, type, searchModel);
                #endregion
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MoveToFolder(int activityId, int toFolderId, int fromFolderId, string currentUserId, bool IsPost)
        {
            try
            {
                var result = new MyDesksRules(dbContext).MoveToFolder(activityId, toFolderId, IsPost, fromFolderId, currentUserId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        public string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult GetDataTags([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int myDeskId)
        {
            try
            {
                var result = new MyDesksRules(dbContext).GetDataTags(requestModel, myDeskId);
                if (result != null)
                    return Json(result, JsonRequestBehavior.AllowGet);
                return Json(new DataTablesResponse(requestModel.Draw, null, 0, 0), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new DataTablesResponse(requestModel.Draw, null, 0, 0), JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult SaveTag(MyTag tag, int deskId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (deskId > 0)
                {
                    var tagRule = new MyTagsRules(dbContext);
                    tag.Name = tag.Name.Trim('\'');
                    if (tagRule.DuplicateTagNameCheck(deskId, tag.Id, tag.Name))
                    {
                        refModel.result = false;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_390");
                        return Json(refModel, JsonRequestBehavior.AllowGet);
                    }
                    refModel.Object = tagRule.SaveTag(deskId, tag, CurrentUser().Id);
                    refModel.result = refModel.Object == null ? false : true;
                }
                else
                {
                    refModel.result = false;
                }

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTag(int tagId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new MyTagsRules(dbContext).DeleteTag(tagId);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTagNameForTypeahead(int deskId)
        {
            try
            {
                return Json(new MyTagsRules(dbContext).GetTagNameForTypeahead(deskId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult LoadTaskMoreModal(string taskKey)
        {
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                refModel = new ReturnJsonModel();
                var task = new TasksRules(dbContext).GetTaskById(taskId);
                //ViewBag.Recurrance = new RecurranceRules(dbContext).GetRecurranceById(task.AssociatedSet.Id);
                var partialView = RenderViewToString("~/Views/Qbicles/_TaskMore.cshtml", task);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult LoadEventMoreModal(string eventKey)
        {
            try
            {
                var eventId = string.IsNullOrEmpty(eventKey) ? 0 : int.Parse(eventKey.Decrypt());
                refModel = new ReturnJsonModel();
                var ev = new EventsRules(dbContext).GetEventById(eventId);
                var qbicleSet = ev.AssociatedSet;
                ViewBag.IsAttend = qbicleSet != null ? new TasksRules(dbContext).GetPeoples(qbicleSet.Id).Where(s => s.User.Id == CurrentUser().Id).FirstOrDefault()?.isPresent : false;
                var partialView = RenderViewToString("~/Views/Qbicles/_EventMore.cshtml", ev);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult LoadMediaMoreModal(string mediaKey)
        {
            try
            {
                var mediaId = string.IsNullOrEmpty(mediaKey) ? 0 : int.Parse(mediaKey.Decrypt());
                refModel = new ReturnJsonModel();
                ViewBag.VideoRetrievalUrl = ConfigManager.ApiGetVideoUri;
                var media = new MediasRules(dbContext).GetMediaById(mediaId);
                var partialView = RenderViewToString("~/Views/Qbicles/_MediaMore.cshtml", media);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult LoadLinkMoreModal(int linkId)
        {
            try
            {
                refModel = new ReturnJsonModel();
                var link = new LinksRules(dbContext).GetLinkById(linkId);
                var partialView = RenderViewToString("~/Views/Qbicles/_LinkMore.cshtml", link);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult LoadDiscussionMoreModal(int discussionId)
        {
            try
            {
                refModel = new ReturnJsonModel();
                var discussion = new DiscussionsRules(dbContext).GetDiscussionById(discussionId);
                ViewBag.CurrentTimeZone = CurrentUser().Timezone;
                var partialView = RenderViewToString("~/Views/Qbicles/_DiscussionMore.cshtml", discussion);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult LoadProcessMoreModal(string processKey)
        {
            try
            {
                var processId = string.IsNullOrEmpty(processKey) ? 0 : int.Parse(processKey.Decrypt());
                refModel = new ReturnJsonModel();
                var process = new MyDesksRules(dbContext).GetActivityById(processId);
                var partialView = RenderViewToString("~/Views/Qbicles/_ProcessMore.cshtml", process);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }
    }
}