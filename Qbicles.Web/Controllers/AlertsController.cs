using Qbicles.BusinessRules;
using Qbicles.Models;
using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{

    [Authorize]
    public class AlertsController : BaseController
    {

        ReturnJsonModel refModel;

        public ActionResult DuplicateAlertNameCheck(int cubeId, int alertId, string alertName)
        {
            refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new AlertsRules().DuplicateAlertNameCheck(cubeId, alertId, alertName);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SaveAlert(QbicleAlert alert, string[] linkAlertTo,
            int qbicleId, HttpPostedFileBase alertAttachments, string topic_name)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var media = new MediaModel();
                
                var qb = new AlertsRules(dbContext);
                refModel = qb.SaveAlert(alert, linkAlertTo, CurrentQbicleId(), media,
                    CurrentUser().Id, topic_name);

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        public ActionResult SetAlertSelected(string key, string goBack)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());

                //Check for activity accessibility
                var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(id, CurrentUser().Id);
                if (checkResult.result && (bool)checkResult.Object == true)
                {
                    refModel = new ReturnJsonModel();

                    var alert = new AlertsRules(dbContext).GetAlertById(id);
                    this.SetCurrentQbicleIdCookies(alert.Qbicle?.Id ?? 0);
                    this.SetCurrentAlertIdCookies(id);
                    this.SetCookieGoBackPage(goBack);
                    if (alert.Qbicle != null) refModel.msgId = alert.Qbicle.Domain.Id.ToString();
                    refModel.result = true;

                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(checkResult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }




        [HttpPost]
        public ActionResult UpdateAlert(QbicleAlert alert)
        {
            var result = new ReturnJsonModel();
            try
            {
                result.actionVal = 1;
                if (alert.Id > 0)
                {
                    var _alertAdapter = new AlertsRules(dbContext);
                    alert = _alertAdapter.UpdateAlert(alert);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result.actionVal = 2;
                result.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}