using System;
using System.Web.Mvc;
using Qbicles.BusinessRules;

namespace Qbicles.Web.Controllers
{

    public class ApplicationController : BaseController
    {
        [Authorize]
        public ActionResult GetAppByRoleId(int? roleId = null)
        {
            return Json(new AppInstancesRules(dbContext).GetAppByRoleId(roleId ?? 0, CurrentDomain()));
        }
    }
}