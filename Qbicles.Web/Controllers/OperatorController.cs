using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Operator;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Operator;
using Qbicles.Models.Operator.Goals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Qbicles.Models.Form;
using Qbicles.Models.Operator.Compliance;
using System.Globalization;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class OperatorController : BaseController
    {
        //check permissions and navigate to the required tab
        public ActionResult Index()
        {
            var domain = CurrentDomain();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domain.Id);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");
            ViewBag.AllCountries = new CountriesRules().GetAllCountries();
            ViewBag.Locations = new OperatorLocationRules(dbContext).getAlLocationsByDomainId(domain.Id);
            ViewBag.Roles = new OperatorRoleRules(dbContext).getAlRolesByDomainId(domain.Id);
            var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(domain.Id);
            ViewBag.Setting = setting;
            var qbicleId = setting?.SourceQbicle?.Id ?? 0;
            ViewBag.Folder = dbContext.MediaFolders.FirstOrDefault(s => s.Name == HelperClass.GeneralName && s.Qbicle.Id == qbicleId);
            ViewBag.CurrentPage = "Operator"; SetCurrentPage("Operator");
            ViewBag.Workgroups = new OperatorWorkgroupRules(dbContext).GetAllWorkgroupsHasTeamPerson(domain.Id);
            return View();
        }
        public ActionResult Goal(int id)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");
            var goal = new OperatorGoalRules(dbContext).GetGoalById(id);
            var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.topics = new TopicRules(dbContext).GetTopicByQbicle(setting.SourceQbicle.Id);
            ViewBag.setting = setting;
            ViewBag.folder = new OperatorGoalRules(dbContext).getDefaultFolderByQbicleId(setting.SourceQbicle.Id); ;
            return View(goal);
        }
        public ActionResult Clocked(int id, string type)
        {
            var clock = new OperatorAttendanceRules(dbContext).GetAttendanceById(id);
            if (clock == null)
                return View("Error");
            ViewBag.CurrentPage = "Approval"; SetCurrentPage("Approval");
            var domainId = clock.WorkGroup.Domain.Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");
            ViewBag.type = type;
            return View(clock);
        }
        public ActionResult ComplianceTask(int id, int? tid)
        {
            var taskrule = new OperatorTaskRules(dbContext);
            var complianceTask = taskrule.GetComplianceTaskById(id);
            if (complianceTask == null)
                return View("Error");
            var taskInstance = taskrule.GetTaskInstance(complianceTask, tid);
            ViewBag.TaskInstance = taskInstance;
            SetCurrentTaskIdCookies(taskInstance?.AssociatedQbicleTask.Id ?? 0);
            ViewBag.CurrentPage = "Task"; SetCurrentPage("Task");
            var domainId = complianceTask.WorkGroup.Domain.Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");

            return View("ComplianceTask", complianceTask);
        }
        public ActionResult ComplianceTaskInstances(int id, int? tid)
        {
            var taskrule = new OperatorTaskRules(dbContext);
            var complianceTask = taskrule.GetComplianceTaskById(id);
            if (complianceTask == null)
                return View("Error");
            ViewBag.TaskInstance = taskrule.GetTaskInstance(complianceTask, tid);
            ViewBag.Forms = new OperatorFormRules(dbContext).GetFormDefinitionsAll(CurrentDomainId());
            var domainId = complianceTask.WorkGroup.Domain.Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");
            return View("ComplianceTaskInstances", complianceTask);
        }
        public ActionResult ComplianceTaskSubmission(int id, int? tid, int fid)
        {
            var taskrule = new OperatorTaskRules(dbContext);
            var complianceTask = taskrule.GetComplianceTaskById(id);
            if (complianceTask == null)
                return View("Error");
            ViewBag.TaskInstance = taskrule.GetTaskInstance(complianceTask, tid);
            var domainId = complianceTask.WorkGroup.Domain.Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");
            ViewBag.Form = taskrule.GetFormInstanceById(fid);
            return View("ComplianceTaskSubmission", complianceTask);
        }
        public ActionResult LoadMedias(int qid, string fileType)
        {
            try
            {
                var folder = new OperatorGoalRules(dbContext).getDefaultFolderByQbicleId(qid);
                var listMedia = new MediaFolderRules(dbContext).GetMediaItemByFolderId(folder?.Id ?? 0, qid, fileType, CurrentUser().Timezone);
                return PartialView("_GoalResources", listMedia);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }
        public ActionResult LoadOperatorConfigs()
        {
            var domain = CurrentDomain();
            ViewBag.qbicles = domain.Qbicles;
            var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(CurrentDomainId());
            if (setting != null)
            {
                ViewBag.Setting = setting;
                ViewBag.topics = new TopicRules(dbContext).GetTopicByQbicle(setting.SourceQbicle != null ? setting.SourceQbicle.Id : 0);
            }
            ViewBag.Setting = setting;

            return PartialView("_ConfigsContent");
        }
        public ActionResult LoadOperatorGoals()
        {
            return PartialView("_GoalsContent");
        }
        public ActionResult LoadOperatorMeasures()
        {
            return PartialView("_MeasuresContent");
        }

        public ActionResult LoadTopicsByQbicleId(int qid)
        {
            try
            {
                var topics = new TopicRules(dbContext).GetTopicByQbicle(qid);
                if (topics != null && topics.Count > 0)
                    return Json(topics.Select(s => new { id = s.Id, text = s.Name }).ToList(), JsonRequestBehavior.AllowGet);
                else
                    return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFolderInfor()
        {
            try
            {
                var domain = CurrentDomain();
                var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(domain.Id);
                var qbicleId = setting?.SourceQbicle?.Id ?? 0;
                var folder = dbContext.MediaFolders.FirstOrDefault(s => s.Name == HelperClass.GeneralName && s.Qbicle.Id == qbicleId);
                return Json(new { folderId = folder?.Id, qbicleId = folder?.Qbicle?.Id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateSetting(int id, int qId, int tId)
        {
            return Json(new OperatorConfigRules(dbContext).UpdateSetting(id, CurrentDomain(), tId, qId, CurrentUser().Id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult getTagById(int id)
        {
            var tag = new OperatorTagRules(dbContext).getTagById(id);
            if (tag != null)
                return Json(new { tag.Id, tag.Name, tag.Summary }, JsonRequestBehavior.AllowGet);
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveTag(OperatorTag tag)
        {
            tag.Domain = CurrentDomain();
            return Json(new OperatorTagRules(dbContext).SaveTag(tag, CurrentUser().Id));
        }
        public ActionResult getTagsAll()
        {
            var tags = new OperatorTagRules(dbContext).getTagsAll(CurrentDomainId());
            return Json(tags.Select(s => new { s.Id, s.Name }).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchTags([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string tagName)
        {
            var user = CurrentUser();
            return Json(new OperatorTagRules(dbContext).SearchTags(requestModel, tagName, CurrentDomainId(), user.Id, user.DateFormat), JsonRequestBehavior.AllowGet);
        }

        public ActionResult getRoleById(int id)
        {
            var role = new OperatorRoleRules(dbContext).getRoleById(id);
            if (role != null)
                return Json(new { role.Id, role.Name, role.Summary }, JsonRequestBehavior.AllowGet);
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveRole(OperatorRole role)
        {
            role.Domain = CurrentDomain();

            return Json(new OperatorRoleRules(dbContext).SaveRole(role, CurrentUser().Id));
        }

        public ActionResult SearchRoles([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string roleName)
        {
            return Json(new OperatorRoleRules(dbContext).SearchRoles(requestModel, roleName, CurrentDomainId(), CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetOperatorStatus(int id, bool status)
        {
            return Json(new OperatorRoleRules(dbContext).SetOperatorStatus(id, status), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveOperatorRole(int id)
        {
            return Json(new OperatorRoleRules(dbContext).RemoveOperatorRole(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult getLocationById(int id)
        {
            var location = new OperatorLocationRules(dbContext).getLocationById(id);
            if (location != null)
                return Json(new { location.Id, location.Name, location.AddressLine1, location.AddressLine2, location.City, location.State, location.Postcode, Country = location.Country.CommonName, location.Telephone, location.Email }, JsonRequestBehavior.AllowGet);
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveLocation(OperatorLocation location, string CountryName)
        {
            location.Domain = CurrentDomain();
            return Json(new OperatorLocationRules(dbContext).SaveLocation(location, CountryName, CurrentUser().Id));
        }

        public ActionResult SearchLocations([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string locationSearch)
        {
            return Json(new OperatorLocationRules(dbContext).SearchLocations(requestModel, locationSearch, CurrentDomainId(), CurrentUser().Id, CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveOperatorLocation(int id)
        {
            return Json(new OperatorLocationRules(dbContext).RemoveOperatorLocation(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadModalPeople(int workgroupId)
        {
            var wgRule = new OperatorWorkgroupRules(dbContext);
            var domain = CurrentDomain();
            ViewBag.members = domain.Users;
            return PartialView("_PeopleModal", wgRule.getWorkgroupById(workgroupId));
        }

        public ActionResult SearchWorkgroups([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int type, string name)
        {
            return Json(new OperatorWorkgroupRules(dbContext).SearchWorkgroups(requestModel, CurrentDomainId(), type, name, CurrentUser().Id, CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadModalWorkgroup(int id)
        {
            var wgRule = new OperatorWorkgroupRules(dbContext);
            var domain = CurrentDomain();
            var workgroup = wgRule.getWorkgroupById(id);
            var qbicles = domain.Qbicles != null ? domain.Qbicles : new List<Qbicle>();
            var defaultQbicle = qbicles.Any() ? qbicles.FirstOrDefault() : new Qbicle();
            var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(domain.Id);
            var locations = new OperatorLocationRules(dbContext).getAlLocationsByDomainId(domain.Id);
            ViewBag.topics = new TopicRules(dbContext).GetTopicOnlyByQbicle(defaultQbicle.Id);
            ViewBag.members = domain.Users;
            ViewBag.qbicles = qbicles;
            ViewBag.setting = setting;
            ViewBag.locations = locations;
            return PartialView("_WorkgroupModal", workgroup);
        }

        public ActionResult RemoveOperatorWorkgroup(int id)
        {
            return Json(new OperatorWorkgroupRules(dbContext).RemoveWorkgroup(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult getMeasureById(int id)
        {
            var measure = new OperatorMeasureRules(dbContext).getMeasureById(id);
            if (measure != null)
                return Json(new { measure.Id, measure.Name, measure.Summary }, JsonRequestBehavior.AllowGet);
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveMeasure(Measure measure)
        {
            measure.Domain = CurrentDomain();
            return Json(new OperatorMeasureRules(dbContext).SaveMeasure(measure, CurrentUser().Id));
        }
        public ActionResult SearchMeasures([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string measureName)
        {
            return Json(new OperatorMeasureRules(dbContext).SearchMeasures(requestModel, measureName, CurrentDomainId(), CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveGoal(OperatorGoalModel operatorGoal,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            operatorGoal.Domain = CurrentDomain();

            var jvSerializer = new JavaScriptSerializer();
            if (!string.IsNullOrEmpty(operatorGoal.sLeadingIndicators))
                operatorGoal.LeadingIndicators = jvSerializer.Deserialize<List<OperatorMeasureModel>>(operatorGoal.sLeadingIndicators);
            if (!string.IsNullOrEmpty(operatorGoal.sGoalMeasures))
                operatorGoal.GoalMeasures = jvSerializer.Deserialize<List<OperatorMeasureModel>>(operatorGoal.sGoalMeasures);

            if (!string.IsNullOrEmpty(mediaObjectKey))
            {
                operatorGoal.FeaturedImageURI = mediaObjectKey;
                operatorGoal.MediaResponse = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(Converter.Obj2Int(mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };
            }

            return Json(new OperatorGoalRules(dbContext).SaveGoal(operatorGoal, CurrentUser().Id));
        }
        public ActionResult UpdateHiddenGoal(int id, bool isHidden)
        {
            return Json(new OperatorGoalRules(dbContext).UpdateHiddenGoal(id, isHidden, CurrentUser().Id));
        }
        public ActionResult LoadModalGoal(int id)
        {
            var domainId = CurrentDomainId();
            ViewBag.measures = new OperatorMeasureRules(dbContext).GetMeasuresAll(domainId);
            ViewBag.tags = new OperatorTagRules(dbContext).getTagsAll(domainId);
            var goal = new OperatorGoalRules(dbContext).GetGoalById(id);
            return PartialView("_GoalModal", goal != null ? goal : new Goal());
        }
        public ActionResult LoadGoals(int skip, int take, string keyword, bool isHide, List<int> tags)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                int totalRecord = 0;
                var goals = new OperatorGoalRules(dbContext).LoadGoalsByDomainId(CurrentDomainId(), isHide, skip, take, keyword.Trim(), tags, ref totalRecord);
                var partialView = RenderViewToString("~/Views/Operator/_GoalsContent.cshtml", goals);
                refModel.Object = new
                {
                    strResult = partialView,
                    totalRecord = totalRecord
                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult UpdateOptionGoal(int id, bool isHide)
        {
            return Json(new OperatorGoalRules(dbContext).UpdateHiddenGoal(id, isHide, CurrentUser().Id));
        }
        public ActionResult SaveResourceGoal(string name, string description, int qbicleId, int topicId, int mediaFolderId,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {

            var mediaModel = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(Converter.Obj2Int(mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };
            var refModel = new OperatorGoalRules(dbContext).SaveResource(mediaModel, CurrentUser().Id, qbicleId, mediaFolderId, name, description, topicId);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
       
        public ActionResult DiscussionGoal(int disId)
        {
            var goal = new OperatorGoalRules(dbContext).GetGoalByActivityId(disId);
            if (goal != null)
            {
                var currentDomainId = goal?.Domain.Id ?? 0;
                var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(currentDomainId);
                ValidateCurrentDomain(goal?.Domain, setting?.SourceQbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                    return View("ErrorAccessPage");
                ViewBag.Setting = setting;
                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(setting?.SourceQbicle.Id ?? 0);

                ViewBag.CurrentPage = "SocialPostDiscussion"; SetCurrentPage("SocialPostDiscussion");
                
                SetCurrentDiscussionIdCookies(goal.Discussion?.Id ?? 0);
                return View(goal);
            }
            else
                return View("Error");
        }
        public ActionResult LoadGoalMeasuresForChart(OperatorGoalChart goalChart)
        {
            goalChart.currentTimeZone = CurrentUser().Timezone;
            goalChart.dateFormat = CurrentUser().DateFormat;
            return Json(new OperatorGoalRules(dbContext).GetGoalChart(goalChart), JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckMeasuresExist()
        {
            return Json(new { isExist = new OperatorMeasureRules(dbContext).CheckMeasuresExist(CurrentDomainId()) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteMeasure(int id)
        {
            return Json(new OperatorMeasureRules(dbContext).DeleteMeasure(id));
        }
        public ActionResult DeleteGoalMeasure(int id)
        {
            return Json(new OperatorMeasureRules(dbContext).DeleteGoalMeasure(id));
        }
        public ActionResult LoadTabGoalMeasures(OperatorGoalChart goalChart, string search)
        {
            switch (goalChart.timeframe)
            {
                case 0:
                    ViewBag.TitleTimeframe = "Last 7 days";
                    break;
                case 1:
                    ViewBag.TitleTimeframe = "Last 30 days";
                    break;
                default:
                    ViewBag.TitleTimeframe = goalChart.customDate;
                    break;
            }
            goalChart.currentTimeZone = CurrentUser().Timezone;
            goalChart.dateFormat = CurrentUser().DateFormat;
            return PartialView("_GoalMeasures", new OperatorGoalRules(dbContext).GetGoalMeasures(goalChart, search));
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

        public ActionResult SaveWorkgroup(OperatorWorkgroupCustom workgroup)
        {
            workgroup.Domain = CurrentDomain();
            
            return Json(new OperatorWorkgroupRules(dbContext).SaveWorkgroup(workgroup, CurrentUser().Id));
        }

        public ActionResult GetTopicByQbicle(int qbicleId, int currentTopicId)
        {
            if (qbicleId == 0)
                return Json(null, JsonRequestBehavior.AllowGet);
            var topics = new TopicRules(dbContext).GetTopicOnlyByQbicle(qbicleId);
            if (currentTopicId == 0)
                return Json(topics.Select(s => new { id = s.Id, text = s.Name, selected = s.Name == HelperClass.GeneralName ? true : false }).ToList(), JsonRequestBehavior.AllowGet);
            return Json(topics.Select(s => new { id = s.Id, text = s.Name, selected = s.Id == currentTopicId ? true : false }).ToList(), JsonRequestBehavior.AllowGet);
        }

        #region Team person
        public ActionResult LoadTeamPersonModal(int id)
        {
            var rules = new OperatorPersonRules(dbContext);
            var teamPerson = rules.GetPersonById(id);
            var domain = CurrentDomain();
            ViewBag.Members = new OperatorPersonRules(dbContext).GetAllUsersAvailable(domain, teamPerson);
            ViewBag.Locations = new OperatorLocationRules(dbContext).getAlLocationsByDomainId(domain.Id);
            ViewBag.Roles = new OperatorRoleRules(dbContext).getAlRolesByDomainId(domain.Id);
            return PartialView("_PersonModel", teamPerson);
        }

        public ActionResult SearchTeamPersons([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string teamPersonSearch, int teamPersonLocationSearch, int teamPersonRoleSearch)
        {
            return Json(new OperatorPersonRules(dbContext).SearchTeamPersons(requestModel, teamPersonSearch, teamPersonLocationSearch, teamPersonRoleSearch, CurrentDomainId(), CurrentUser().Id, CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveTeamPerson(int id, string memberId, int[] lstRoleIds, int[] lstLocationIds)
        {
            return Json(new OperatorPersonRules(dbContext).SaveTeamPerson(id, memberId, CurrentUser().Id, lstRoleIds, lstLocationIds, CurrentDomain()));
        }
        public ActionResult RemoveTeamPerson(int id)
        {
            return Json(new OperatorPersonRules(dbContext).RemoveTeamPerson(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DetailTeamPerson(int id)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");
            return View(new OperatorPersonRules(dbContext).GetPersonById(id));
        }
        public ActionResult LoadMedia(int fid, int qid, string fileType, string rs)
        {
            try
            {
                var listMedia = new MediaFolderRules(dbContext).GetMediaItemByFolderIdWithName(fid,
                    qid, fileType, rs, CurrentUser().Timezone);
                return PartialView("_PersonResources", listMedia);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }
        public ActionResult SaveResource(string name, string description, int qbicleId, int topicId, int mediaFolderId,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var mediaModel = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(Converter.Obj2Int(mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };
            var refModel = new OperatorPersonRules(dbContext).SaveResource(mediaModel, CurrentUser().Id, qbicleId, mediaFolderId, name, description, topicId);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadResourceModal(int folderId)
        {
            var folder = dbContext.MediaFolders.FirstOrDefault(m => m.Id == folderId);
            ViewBag.topics = new TopicRules(dbContext).GetTopicOnlyByQbicle(folder?.Qbicle?.Id ?? 0);
            return PartialView("_ResourceModal", folder);
        }
        #endregion

        #region Performance tracking
        public ActionResult LoadPerformanceTrackingModal(int id)
        {
            var domain = CurrentDomain();
            var performanceTracking = new OperatorPerformanceTrackingRules(dbContext).getPerformanceTrackingById(id);
            ViewBag.Workgroups = new OperatorWorkgroupRules(dbContext).GetAllWorkgroupsHasTeamPerson(domain.Id);
            ViewBag.Measures = new OperatorMeasureRules(dbContext).GetMeasuresAll(domain.Id);
            if (performanceTracking != null)
            {
                var teamPersons = new OperatorPerformanceTrackingRules(dbContext).GetWorkgroupInfor(performanceTracking.WorkGroup.Id, CurrentUser().Id).Persons;
                if (!teamPersons.Any(t => t.Id == performanceTracking.Team.Id))
                {
                    teamPersons.Add(new PersonModal()
                    {
                        Id = performanceTracking.Team.Id,
                        Name = HelperClass.GetFullNameOfUser(performanceTracking.Team.User, CurrentUser().Id),
                        ProfilePic = performanceTracking.Team.User.ProfilePic
                    });
                }
                ViewBag.TeamPersons = teamPersons.OrderBy(p => p.Name).ToList();
            }

            return PartialView("_PerformanceTrackingModal", performanceTracking);
        }

        public ActionResult ShowListMemberForWorkGroup(int performanceId, int wgId)
        {
            var rules = new OperatorPerformanceTrackingRules(dbContext);
            var teamPersons = rules.GetAllTeamPerson(performanceId, wgId);
            return PartialView("_WorkgroupMembers", teamPersons);
        }

        public ActionResult LoadPerformanceTrackings(string keyword, int locationId, bool isLoadingHide, int skip, int take)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = "An error" };

            int totalRecord = 0;
            var performanceTrackings = new OperatorPerformanceTrackingRules(dbContext).SearchPerformanceTrackings(keyword, locationId, isLoadingHide, skip, take, ref totalRecord);
            ContentMoreModel response = new ContentMoreModel();
            var partialView = "";
            if (take != 0)
            {
                partialView = RenderViewToString("_PerformanceTrackingContent", performanceTrackings);
            }
            else
            {
                partialView = "";
            }

            refModel.Object = new
            {
                strResult = partialView,
                totalRecord
            };
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SavePerformanceTracking(PerformanceTrackingModel model)
        {
            return Json(new OperatorPerformanceTrackingRules(dbContext).SavePerformanceTracking(model), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowOrHidePerformanceTracking(int id, bool isHide)
        {
            return Json(new OperatorPerformanceTrackingRules(dbContext).ShowOrHidePerformanceTracking(id, isHide), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWorkgroupInfor(int id)
        {
            return Json(new OperatorPerformanceTrackingRules(dbContext).GetWorkgroupInfor(id, CurrentUser().Id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailPerformanceTracking(int id)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                return View("ErrorAccessPage");
            var performanceTracking = new OperatorPerformanceTrackingRules(dbContext).getPerformanceTrackingById(id);
            ViewBag.topics = new TopicRules(dbContext).GetTopicOnlyByQbicle(performanceTracking.Team.ResourceFolder.Qbicle.Id);
            return View(performanceTracking);
        }

        public ActionResult LoadPerfomanceMeasuresForChart(OperatorPerformanceChart performanceChart)
        {
            performanceChart.currentTimeZone = CurrentUser().Timezone;
            performanceChart.dateFormat = CurrentUser().DateFormat;
            return Json(new OperatorPerformanceTrackingRules(dbContext).GetPerformanceChart(performanceChart), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadTabPerformanceMeasures(OperatorPerformanceChart performanceChart, string search)
        {
            switch (performanceChart.timeframe)
            {
                case 0:
                    ViewBag.TitleTimeframe = "Last 7 days";
                    break;
                case 1:
                    ViewBag.TitleTimeframe = "Last 30 days";
                    break;
                default:
                    ViewBag.TitleTimeframe = performanceChart.customDate;
                    break;
            }
            performanceChart.currentTimeZone = CurrentUser().Timezone;
            performanceChart.dateFormat = CurrentUser().DateFormat;
            return PartialView("_PerformanceMeasures", new OperatorPerformanceTrackingRules(dbContext).GetPerformanceMeasures(performanceChart, search));
        }

        public ActionResult DeletePerformanceMeasure(int id)
        {
            return Json(new OperatorPerformanceTrackingRules(dbContext).DeleteTrakingMeasure(id));
        }

        public ActionResult DiscussionPerformance(int disId)
        {
            var performance = new OperatorPerformanceTrackingRules(dbContext).GetPerformanceByActivityId(disId);
            if (performance != null)
            {
                var currentDomainId = CurrentDomainId();
                var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(currentDomainId);
                ValidateCurrentDomain(CurrentDomain(), setting?.SourceQbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                    return View("ErrorAccessPage");
                ViewBag.Setting = setting;
                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(setting?.SourceQbicle.Id ?? 0);

                ViewBag.CurrentPage = "SocialPostDiscussion"; SetCurrentPage("SocialPostDiscussion");
                
                SetCurrentDiscussionIdCookies(performance.Discussion?.Id ?? 0);
                return View(performance);
            }
            else
                return View("Error");
        }
        #endregion

        public ActionResult SaveClock(OperatorClockIn operatorClock)
        {
            return Json(new OperatorAttendanceRules(dbContext).SaveClock(operatorClock, CurrentUser()));
        }
        public ActionResult UpdateClock(int id, string clockIn, string clockOut)
        {
            return Json(new OperatorAttendanceRules(dbContext).UpdateClockOut(id, clockIn, clockOut, CurrentUser().Timezone, CurrentUser().DateFormat, CurrentDomainId(), CurrentUser().Id));
        }
        public ActionResult LoadWorkgroupPreview(int wgid)
        {
            return Json(new OperatorWorkgroupRules(dbContext).getWorkgroupPreviewById(wgid), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchAttendances([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string searchFullname, string filterdate, string peoples)
        {
            var lstPeoples = !string.IsNullOrEmpty(peoples) ? new JavaScriptSerializer().Deserialize<List<string>>(peoples) : null;
            return Json(new OperatorAttendanceRules(dbContext).SearchAttendances(requestModel, searchFullname, filterdate, lstPeoples, CurrentDomainId(), CurrentUser().DateFormat, CurrentUser().Timezone), JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckIsManagerOrSupervisor()
        {
            return Json(new { allow = new OperatorWorkgroupRules(dbContext).checkIsManagerOrSupervisor(CurrentDomainId(), CurrentUser().Id) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTeamMembers()
        {
            return Json(new OperatorWorkgroupRules(dbContext).getTeamMembersByDomain(CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetWorkgroups(WorkGroupTypeEnum type)
        {
            var wgs = new OperatorWorkgroupRules(dbContext).GetOperatorWorksAll(CurrentDomainId(), type);
            return Json(wgs.Select(s => new { s.Id, s.Name }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveSchedule(OperatorScheduleModel scheduleModel)
        {
            scheduleModel.Domain = CurrentDomain();
            return Json(new OperatorScheduleRule(dbContext).SaveSchedule(scheduleModel, CurrentUser()));
        }
        public ActionResult UpdateSchedule(OperatorScheduleUpdateModel scheduleUpdateModel)
        {
            scheduleUpdateModel.Dateformat = CurrentUser().DateFormat;
            scheduleUpdateModel.Timezone = CurrentUser().Timezone;
            return Json(new OperatorScheduleRule(dbContext).UpdateSchedule(scheduleUpdateModel));
        }
        public ActionResult GetScheduleById(int id)
        {
            var timezone = CurrentUser().Timezone;
            var schedule = new OperatorScheduleRule(dbContext).GetScheduleById(id);
            return Json(new { schedule.Id, ShiftStart = schedule.StartDate.ConvertTimeFromUtc(timezone).ToString("HH:mm"), ShiftEnd = schedule.EndDate.ConvertTimeFromUtc(timezone).ToString("HH:mm") }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchSchedulesDaily(OperatorSearchScheduleModel operatorSearch, string sPeoples, string sRoles, string sLocations)
        {
            operatorSearch.Dateformat = CurrentUser().DateFormat;
            operatorSearch.Timezone = CurrentUser().Timezone;
            operatorSearch.DomainId = CurrentDomainId();
            operatorSearch.Peoples = !string.IsNullOrEmpty(sPeoples) ? new JavaScriptSerializer().Deserialize<List<string>>(sPeoples) : null;
            operatorSearch.Roles = !string.IsNullOrEmpty(sRoles) ? new JavaScriptSerializer().Deserialize<List<int>>(sRoles) : null;
            operatorSearch.Locations = !string.IsNullOrEmpty(sLocations) ? new JavaScriptSerializer().Deserialize<List<int>>(sLocations) : null;
            return Json(new OperatorScheduleRule(dbContext).SearchSchedulesDaily(operatorSearch), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchSchedulesWeekly(OperatorSearchScheduleModel operatorSearch)
        {
            operatorSearch.Dateformat = CurrentUser().DateFormat;
            operatorSearch.Timezone = CurrentUser().Timezone;
            operatorSearch.DomainId = CurrentDomainId();
            ViewBag.tableId = "tblsched-view-week";
            var table = new OperatorScheduleRule(dbContext).SearchSchedulesWeekly(operatorSearch);
            return PartialView("_WeeklyMonthlyView", table);
        }
        public ActionResult SearchSchedulesMonthly(OperatorSearchScheduleModel operatorSearch)
        {
            operatorSearch.Dateformat = CurrentUser().DateFormat;
            operatorSearch.Timezone = CurrentUser().Timezone;
            operatorSearch.DomainId = CurrentDomainId();
            ViewBag.tableId = "tblsched-view-month";
            var table = new OperatorScheduleRule(dbContext).SearchSchedulesMonthly(operatorSearch);
            return PartialView("_WeeklyMonthlyView", table);
        }
        public ActionResult SearchTimesheetsDaily(OperatorSearchScheduleModel operatorSearch, string sPeoples, string sRoles, string sLocations)
        {
            operatorSearch.Dateformat = CurrentUser().DateFormat;
            operatorSearch.Timezone = CurrentUser().Timezone;
            operatorSearch.DomainId = CurrentDomainId();
            operatorSearch.Peoples = !string.IsNullOrEmpty(sPeoples) ? new JavaScriptSerializer().Deserialize<List<string>>(sPeoples) : null;
            operatorSearch.Roles = !string.IsNullOrEmpty(sRoles) ? new JavaScriptSerializer().Deserialize<List<int>>(sRoles) : null;
            operatorSearch.Locations = !string.IsNullOrEmpty(sLocations) ? new JavaScriptSerializer().Deserialize<List<int>>(sLocations) : null;
            return Json(new OperatorScheduleRule(dbContext).SearchTimesheetsDaily(operatorSearch), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchTimesheetsWeekly(OperatorSearchScheduleModel operatorSearch)
        {
            operatorSearch.Dateformat = CurrentUser().DateFormat;
            operatorSearch.Timezone = CurrentUser().Timezone;
            operatorSearch.DomainId = CurrentDomainId();
            ViewBag.tableId = "tbltimesh-view-week";
            var table = new OperatorScheduleRule(dbContext).SearchTimesheetsWeekly(operatorSearch);
            return PartialView("_WeeklyMonthlyView", table);
        }
        public ActionResult SearchTimesheetsMonthly(OperatorSearchScheduleModel operatorSearch)
        {
            operatorSearch.Dateformat = CurrentUser().DateFormat;
            operatorSearch.Timezone = CurrentUser().Timezone;
            operatorSearch.DomainId = CurrentDomainId();
            ViewBag.tableId = "tbltimesh-view-month";
            var table = new OperatorScheduleRule(dbContext).SearchTimesheetsMonthly(operatorSearch);
            return PartialView("_WeeklyMonthlyView", table);
        }
        public ActionResult LoadModalForm(int id)
        {
            var domainId = CurrentDomainId();
            ViewBag.tags = new OperatorTagRules(dbContext).getTagsAll(domainId);
            var form = new OperatorFormRules(dbContext).GetFormDefinitionById(id);
            ViewBag.measures = new OperatorMeasureRules(dbContext).GetMeasuresAll(domainId);
            return PartialView("_FormModal", form == null ? new FormDefinition() : form);
        }
        public ActionResult SaveForm(OperatorFormModel formModel)
        {
            formModel.DomainId = CurrentDomainId();
            return Json(new OperatorFormRules(dbContext).SaveForm(formModel, CurrentUser().Id));
        }
        public ActionResult RemoveForm(int id)
        {
            return Json(new OperatorFormRules(dbContext).removeForm(id));
        }
        public ActionResult SearchForms([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int tag)
        {
            OperatorSearchFormModel search = new OperatorSearchFormModel();
            search.keyword = keyword;
            search.tag = tag;
            search.dateformat = CurrentUser().DateFormat;
            search.timezone = CurrentUser().Timezone;
            search.domainId = CurrentDomainId();
            return Json(new OperatorFormRules(dbContext).SearchForms(requestModel, search), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetMeasures()
        {
            var measures = new OperatorMeasureRules(dbContext).GetMeasuresAll(CurrentDomainId());
            return Json(measures.Select(s => new { s.Id, s.Name }).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadModalTask(int id)
        {
            var domainId = CurrentDomainId();
            ViewBag.Months = Utility.GetListMonth(DateTime.UtcNow);
            ViewBag.Forms = new OperatorFormRules(dbContext).GetFormDefinitionsAll(domainId);
            ViewBag.Workgroups = new OperatorWorkgroupRules(dbContext).GetOperatorWorksAll(CurrentDomainId(), WorkGroupTypeEnum.Tasks);
            var complianceTask = new OperatorTaskRules(dbContext).GetComplianceTaskById(id);
            return PartialView("_ComplianceTaskModal", complianceTask == null ? new ComplianceTask() : complianceTask);
        }
        public ActionResult LoadWorkgroupTaskPreview(int wgid)
        {
            return Json(new OperatorWorkgroupRules(dbContext).getWorkgroupPreviewTaskById(wgid), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveTaskOperator(OperatorTaskModel formModel, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                
                formModel.CurrentDomain = CurrentDomain();
                DateTime dtLastOccurrence = DateTime.UtcNow;


                if (!string.IsNullOrEmpty(mediaObjectKey))
                    formModel.MediaResponse = new MediaModel
                    {
                        UrlGuid = mediaObjectKey,
                        Name = mediaObjectName,
                        Size = HelperClass.FileSize(Converter.Obj2Int(mediaObjectSize)),
                        Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                    };

                refModel = new OperatorTaskRules(dbContext).SaveTaskOperator(formModel, CurrentUser());
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetListDate(string dayofweek, int type, int Pattern, int customDate, string LastOccurenceDate,
            string FirstOccurenceDate)
        {
            try
            {
                string formatDatetime = CurrentUser().DateTimeFormat;
                var dtLastOccurenceDate =
                    DateTime.ParseExact(LastOccurenceDate, formatDatetime, CultureInfo.InvariantCulture);
                var dtCalculator =
                    DateTime.ParseExact(FirstOccurenceDate, formatDatetime, CultureInfo.InvariantCulture);
                switch (type)
                {
                    case 0:
                        {
                            var lstDay = Utility.GetListDayToTable(dtCalculator, dayofweek, dtLastOccurenceDate);
                            return Json(lstDay, JsonRequestBehavior.AllowGet);
                        }

                    case 1:
                        {
                            var lstWeek = Utility.GetListWeekToTable(dtCalculator, dayofweek, dtLastOccurenceDate);
                            return Json(lstWeek, JsonRequestBehavior.AllowGet);
                        }
                }

                dtCalculator = new DateTime(dtCalculator.Year, dtCalculator.Month + 1, 1, dtCalculator.Hour,
                    dtCalculator.Minute, dtCalculator.Second);
                var lstMonth =
                    Utility.GetListMonthToTable(dtCalculator, Pattern, customDate, dayofweek, dtLastOccurenceDate, formatDatetime);
                return Json(lstMonth, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }
        public ActionResult SearchTasks([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string assignee, int form)
        {
            OperatorSearchTaskModel search = new OperatorSearchTaskModel();
            search.keyword = keyword;
            search.assignee = assignee;
            search.dateformat = CurrentUser().DateFormat;
            search.timezone = CurrentUser().Timezone;
            search.domainId = CurrentDomainId();
            return Json(new OperatorTaskRules(dbContext).SearchTasks(requestModel, search), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetFormsAll()
        {
            var forms = new OperatorFormRules(dbContext).GetFormDefinitionsAll(CurrentDomainId());
            return Json(forms.Select(s => new { s.Id, s.Title }).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteTask(int id)
        {
            return Json(new OperatorTaskRules(dbContext).DeleteByTaskId(id));
        }
        public ActionResult OperatorFormSubmit(OperatorFormSubmissionsModel model)
        {
            var refModel = new ReturnJsonModel() { result = false };
            if (model.TaskInstanceId == 0)
            {
                refModel.msg = "ERROR_MSG_EXCEPTION_SYSTEM";
                return Json(refModel);
            }
            foreach (var item in model.Elements)
            {
                #region Validate Element value
                if (string.IsNullOrEmpty(item.Value))
                {
                    refModel.msg = "ERROR_MSG_361";
                    return Json(refModel);
                }
                #endregion

                if (!string.IsNullOrEmpty(item.ImageKey))
                    item.ImageFileResponse = new MediaModel
                    {
                        UrlGuid = item.ImageKey,
                        Name = item.ImageName,
                        Size = HelperClass.FileSize(Converter.Obj2Int(item.ImageSize)),
                        Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(item.ImageName))
                    };
                if (!string.IsNullOrEmpty(item.DocKey))
                    item.DocFileResponse = new MediaModel
                    {
                        UrlGuid = item.DocKey,
                        Name = item.DocName,
                        Size = HelperClass.FileSize(Converter.Obj2Int(item.DocSize)),
                        Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(item.DocName))
                    };

            }
        
            return Json(new OperatorFormRules(dbContext).OperatorFormSubmissions(model,CurrentUser().Id));
        }
        public ActionResult RestartTask(int complianceTaskId, int taskId)
        {
            return Json(new OperatorTaskRules(dbContext).ResetTask(complianceTaskId, taskId, CurrentUser().Id));
        }
        public ActionResult SearchTaskIntances([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int form, int complianceTaskId, int taskId)
        {
            OperatorSearchTaskModel search = new OperatorSearchTaskModel();
            search.keyword = keyword;
            search.form = form;
            search.taskId = taskId;
            search.complianceTaskId = complianceTaskId;
            search.dateformat = CurrentUser().DateFormat;
            search.timezone = CurrentUser().Timezone;
            search.domainId = CurrentDomainId();
            return Json(new OperatorTaskRules(dbContext).SearchTaskInstances(requestModel, search), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DiscussionComplianceTask(int disId)
        {
            var complianceTask = new OperatorTaskRules(dbContext).GetComplianceTaskByActivityId(disId);
            if (complianceTask != null)
            {
                var currentDomainId = complianceTask?.Domain.Id ?? 0;
                ValidateCurrentDomain(complianceTask?.Domain, complianceTask?.WorkGroup.SourceQbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
                    return View("ErrorAccessPage");
                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(complianceTask?.WorkGroup.SourceQbicle.Id ?? 0);

                ViewBag.CurrentPage = "SocialPostDiscussion"; SetCurrentPage("SocialPostDiscussion");
                
                SetCurrentDiscussionIdCookies(complianceTask.Discussion?.Id ?? 0);
                return View(complianceTask);
            }
            else
                return View("Error");
        }
        public ActionResult LoadWorkgroupTeamMembers(int wgid)
        {
            return PartialView("_TeamMembersModal", new OperatorWorkgroupRules(dbContext).getWorkgroupById(wgid));
        }
        public ActionResult LoadWorkgroupTaskMembers(int wgid)
        {
            return PartialView("_TaskMembersModal", new OperatorWorkgroupRules(dbContext).getWorkgroupById(wgid));
        }
    }
}