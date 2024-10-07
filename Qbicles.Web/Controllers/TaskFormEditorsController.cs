using Qbicles.BusinessRules;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    
    public class TaskFormEditorsController : BaseController
    {
        public ActionResult GetDataForTaskFormEditorsByDomainId(int domainId)
        {
            try
            {

                bool isMemberOrOwnerDomain = new DomainRules(dbContext).IsMemberOrOwnerDomain(CurrentUser().Id, CurrentDomain());
                if (isMemberOrOwnerDomain)
                {

                    var listDomainAdminId = new DomainRules(dbContext).GetDomainById(domainId).Administrators.Select(x => x.Id).ToList();
                    var listCurrentUserByDomain = new UserRules(dbContext).GetListUserByDomainId(domainId).Where(x => !listDomainAdminId.Contains(x.Id));


                    var TaskFormEditors = new FormManagerRules(dbContext).GetFormManagerHasPermissionManageTaskFormsByDomainId(domainId);
                    var queryOrReportTaskForm = new FormManagerRules(dbContext).GetFormManagerHasPermissionQueryOrReportByDomainId(domainId);
                    var DataForTaskFormEditorsByDomainId = listCurrentUserByDomain.Select(x => new
                    {
                        Id = x.Id,
                        LastName = x.Surname,
                        FirstName = x.Forename,
                        LogoUrl = x.ProfilePic,
                        Email = x.Email,
                        IsEditor = TaskFormEditors.Any(t => t.User.Id == x.Id),
                        IsQueryOrReport = queryOrReportTaskForm.Any(t => t.User.Id == x.Id)

                    });
                    return Json(DataForTaskFormEditorsByDomainId, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

    }
}