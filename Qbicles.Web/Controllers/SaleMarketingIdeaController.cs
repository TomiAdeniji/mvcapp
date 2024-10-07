using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Enums;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SalesMarketingIdeaController : BaseController
    {
        public ActionResult Detail(int id)
        {
            var idea = new SocialIdeasThemeRule(dbContext).GetIdeaThemeById(id);
            var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
            ViewBag.Setting = setting;
            ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(setting?.SourceQbicle.Id ?? 0);
            if (idea != null)
                return View(idea);
            else
                return View("Error");
        }
        public ActionResult DiscussionIdea(int disId)
        {
            var idea = new SocialIdeasThemeRule(dbContext).GetIdeaThemeByActivityId(disId);
            if (idea != null)
            {
                var currentDomainId = idea?.Domain.Id ?? 0;
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(currentDomainId);
                ValidateCurrentDomain(idea?.Domain, setting?.SourceQbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.SalesAndMarketingAccess))
                    return View("ErrorAccessPage");

                ViewBag.Setting = setting;
                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(setting?.SourceQbicle.Id ?? 0);
                
                ViewBag.CurrentPage = "SocialPostDiscussion"; SetCurrentPage("SocialPostDiscussion");
                
                SetCurrentDiscussionIdCookies(idea.Discussion?.Id ?? 0);
                return View(idea);
            }
            else
                return View("Error");
        }
        public ActionResult GenerateModalIdeaAddEdit(int ideaId)
        {
            var ideaRule = new SocialIdeasThemeRule(dbContext);
            ViewBag.MediaFolders = ideaRule.GetMediaFoldersByQbicleId(CurrentDomainId());
            ViewBag.IdeaTypes = ideaRule.GetIdeaThemeTypes();
            return PartialView("~/Views/SalesMarketingIdea/_IdeaAdd.cshtml", ideaRule.GetIdeaThemeById(ideaId));
        }
        public ActionResult LoadIdeas(string keyword, int isActive, int skip, int take, bool isLoadingHide)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = "An error" };

            int totalRecord = 0;
            var ideas = new SocialIdeasThemeRule(dbContext).GetIdeasByDomainId(CurrentDomainId(), keyword, isActive, skip, take, isLoadingHide, ref totalRecord);
            ContentMoreModel response = new ContentMoreModel();
            var partialView = "";
            if (take != 0)
            {
                partialView = RenderViewToString("_IdeaContent", ideas);
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
        [HttpPost]
        public ActionResult ShowOrHideIdea(int id)
        {
            var rule = new SocialIdeasThemeRule(dbContext);
            var refModel = rule.ShowOrHideIdea(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveIdea(IdeaThemeCustomeModel model, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            var domain = CurrentDomain();
            var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domain.Id);
            if (setting == null)
            {
                refModel.msg = ResourcesManager._L("ERROR_MSG_153");
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            var media = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };
            model.CurrentDomain = domain;
            
            refModel = new SocialIdeasThemeRule(dbContext).SaveIdea(model, media, setting, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult AutoGenerateFolderName()
        {
            try
            {
                return Json(new SocialIdeasThemeRule(dbContext).AutoGenerateFolderName(CurrentDomainId()), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LoadMediasByIdea(int fid, int qid)
        {
            try
            {
                var listMedia = new MediaFolderRules(dbContext).GetMediaItemByFolderId(fid, qid, "", CurrentUser().Timezone);
                return PartialView("_IdeaResources", listMedia);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult LoadCampaignsInTheme(int themeId, int[] types, string search, int draw, int start, int length)
        {
            var totalRecord = 0;
            var lstResult = new SocialIdeasThemeRule(dbContext).GetListCampaignsInTheme(themeId, types, search, start, length, ref totalRecord, CurrentUser().DateFormat);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
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

    }
}