using Qbicles.BusinessRules;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class FileTypesController : BaseController
    {
        public ActionResult GetFileTypes()
        {
            var fileTypes = new FileTypeRules(dbContext).GetFileTypes();

            return Json(fileTypes, JsonRequestBehavior.AllowGet);
        }
    }
}