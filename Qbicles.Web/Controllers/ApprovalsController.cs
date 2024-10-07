using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class ApprovalsController : BaseController
    {
        private ReturnJsonModel refModel;


        public ActionResult ApprovalRequest()
        {
            try
            {
                ViewBag.CurrentPage = "ApprovalRequest"; SetCurrentPage("ApprovalRequest");
                ViewBag.PageTitle = "Approval request";

                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                ViewBag.ApprovalsApp =
                    new ApprovalAppsRules(dbContext).GetCurrentApprovalApps(CurrentUser().Id, CurrentDomainId());
                ViewBag.ApprovalPrioritys =
                    HelperClass.EnumModel.GetEnumValuesAndDescriptions<ApprovalReq.ApprovalPriorityEnum>();
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult GetDocumentByApprovalApp(int Id)
        {
            try
            {
                refModel = new ReturnJsonModel();

                var attachments = new ApprovalsRules(dbContext).GetAttachmentsTypeByApprovalId(Id);

                refModel.msg = RenderViewToString(attachments, "_Attachments");
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        private string RenderViewToString(object model, string viewName)
        {
            ViewData.Model = model;
            ViewBag.UserRoleRights = ApplicationUserRoleRights(HelperClass.appTypeApprovals);
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult SetApprovalSelected(string key, string goBack)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());

            //Check for activity accessibility
            var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(id, CurrentUser().Id);
            if (checkResult.result && (bool)checkResult.Object == true)
            {
                refModel = new ReturnJsonModel();

                var apr = new ApprovalsRules(dbContext).GetApprovalById(id);
                SetCurrentDomainIdCookies(apr.Qbicle.Domain.Id);
                SetCurrentQbicleIdCookies(apr.Qbicle?.Id ?? 0);
                SetCurrentApprovalIdCookies(id);
                SetCookieGoBackPage(goBack);
                if (apr.Qbicle != null) refModel.msgId = apr.Qbicle.Domain.Id.ToString();
                refModel.result = true;


                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(checkResult, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public JsonResult LoadApproval()
        {
            try
            {
                var model = new ApprovalAppsRules(dbContext)
                    .GetCurrentApprovalAppsGroup(CurrentUser().Id, CurrentDomainId()).ToList();

                var modelString = RenderViewToString(model, "_ApprovalContent");

                return Json(new { ModelString = modelString });
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }


        [HttpPost]
        public ActionResult UploadFile(string strMedia)
        {
            refModel = new ReturnJsonModel();
            var mediaModel = new List<MediaModel>();
            try
            {
                if (Request.Files.Count > 0)
                {
                    var fileRequests = Request.Files;

                    for (var i = 0; i < fileRequests.Count; i++)
                    {
                        var media = new MediaModel
                        {
                            Id = Guid.NewGuid().ToString()
                        };
                        var file = fileRequests[i];

                        if (file != null && HelperClass.CheckFileUpload(file.FileName) == true)
                        {
                            media.Name = fileRequests.AllKeys[i];
                            media.Type =
                                new FileTypeRules(dbContext).GetFileTypeById(Path.GetExtension(file.FileName)
                                    ?.ToLower()
                                    .Replace(".", ""));
                            mediaModel.Add(media);
                        }
                    }

                    refModel.actionVal = 1;
                    var saveMediaModel = new List<MediaModel>();

                    saveMediaModel.AddRange(mediaModel);
                    refModel.Object = saveMediaModel;
                    TempData[CurrentUser().Id + "aprovalAttachments"] = fileRequests;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

    }
}