using Qbicles.BusinessRules.Helper;
using System.Web.Mvc;

namespace Qbicles.Doc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var redirectUrl = ConfigManager.QbiclesUrl;
            if (string.IsNullOrEmpty(redirectUrl))
                return View();
            return Redirect(redirectUrl);
        }
    }
}