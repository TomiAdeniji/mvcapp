using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using Newtonsoft.Json;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class ApprovalAppsController : BaseController
    {

        public ActionResult DuplicateApprovalGroupNameCheck(int groupId, string groupName)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result =
                    new ApprovalAppsRules(dbContext).DuplicateApprovalAppsGroupNameCheck(groupId, groupName, CurrentDomainId());
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult SaveApprovalAppGroup(int groupId, string groupName)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel = new ApprovalAppsRules(dbContext).SaveApprovalAppGroup(groupId, groupName, CurrentUser().Id, CurrentDomainId());
                var obj = JsonConvert.DeserializeObject<BusinessRules.Model.ErrorMessageModel>(refModel.msg);
                if (obj.Params == null)
                {
                    refModel.msg = ResourcesManager._L(obj.ErrorCode);
                }
                else
                {
                    refModel.msg = ResourcesManager._L(obj.ErrorCode, obj.Params);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.Object = ex;
            }

            if (refModel.result)
                return Json(refModel, JsonRequestBehavior.AllowGet);
            return null;
        }


        [Authorize]
        public ActionResult FinishAndSaveApproval(ApprovalAppModel approvalApp, string documents, List<int> formRelatedIds)
        {

            var refModel = new ReturnJsonModel();
            if (string.IsNullOrEmpty(approvalApp.Name) || string.IsNullOrEmpty(approvalApp.Description))
            {
                refModel.msg = "This field is required.";
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var proRules = new ApprovalAppsRules(dbContext);
                refModel = proRules.DuplicateApprovalAppNameCheck(approvalApp, CurrentDomainId());
                if (!refModel.result)
                {                   
                    refModel = proRules.SaveApprovalApp(approvalApp, documents, CurrentUser().Id,
                        formRelatedIds);
                    refModel.result = true;
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_299");
                    refModel.result = false;
                }

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult SearchProcess(string textSearch)
        {
            try
            {
                var refModel = new ReturnJsonModel();
                var theGroups = new List<ApprovalGroup>();
                foreach (var ai in CurrentDomain().AssociatedApps)
                {
                    var approvalApp = ai as Approval;
                    if (approvalApp != null && approvalApp.QbicleApplication.Name == HelperClass.appTypeApprovals)
                        theGroups.AddRange(approvalApp.Groups);
                }

                var approvalGroupApp =
                    new ApprovalAppsRules(dbContext).SearchApprovalAppsByText(textSearch.ToUpper(), theGroups);
                refModel.msg = RenderViewToString(approvalGroupApp, "_ApprovalsAppGroup", HelperClass.appTypeApprovals);
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return null;
        }

        public string RenderViewToString(object model, string viewName, string appName)
        {
            ViewData.Model = model;
            ViewBag.UserRoleRights = ApplicationUserRoleRights(appName);
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult GetApprovalApp(int approvalAppId)
        {
            try
            {
                var appRoles = new ApprovalAppsRules(dbContext);
                var appm = appRoles.GetApprovalAppById(approvalAppId);
                var approles = ApplicationUserRoleRights(HelperClass.appTypeApprovals);
                var approvalApp = appRoles.MapApprovalAppToModal(appm, approles);
                return Json(approvalApp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new ApprovalRequestDefinition(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetApprovalGroup(int groupId)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                var appRules = new ApprovalAppsRules(dbContext);
                var approvalApp = appRules.GetApprovalAppsGroupById(groupId);
                refModel.result = true;
                refModel.msgId = approvalApp.Id.ToString();
                refModel.msg = approvalApp.Name;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult DeleteApprovalRequest(int id)
        {
            try
            {
                var accountRules = new ApprovalAppsRules(dbContext);
                accountRules.DeleteApprovalRequest(id);

                return Json(new
                {
                    status = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new
                {
                    status = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}