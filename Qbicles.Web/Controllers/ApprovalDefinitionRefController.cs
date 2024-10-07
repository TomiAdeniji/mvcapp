using Qbicles.BusinessRules;
using System;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class ApprovalDefinitionRefController : BaseController
    {

        [Authorize]
        public ActionResult UpdateFormBuilder(TaskFormDefinitionRefCustom form)
        {
            try
            {
                return Json(new ApprovalReqFormRefRules(dbContext).UpdateFormBuilder(form, CurrentUser().Id, CurrentApprovalId()), JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

    }
}