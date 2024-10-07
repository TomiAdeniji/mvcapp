using Qbicles.BusinessRules;
using System;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TaskFormDefinitionRefController : BaseController
    {
        ReturnJsonModel refModel;

        
        public ActionResult UpdateFormBuilder(TaskFormDefinitionRefCustom form)
        {
            try
            {
                return Json(new TaskFormDefinitionRefRules(dbContext).UpdateFormBuilder(form, CurrentUser().Id, CurrentTaskId()), JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        
        public ActionResult ExecuteQuery(TaskFormParameter taskFormParameter, TaskParameter taskParameter, bool filterTask, bool filterTaskForm)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var result = new TaskFormDefinitionRefRules(dbContext).ExecuteQueryToTbodyTable(taskFormParameter, taskParameter, filterTask, filterTaskForm,CurrentUser().Timezone);
                refModel = result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
    }
}