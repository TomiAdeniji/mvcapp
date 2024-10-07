using System.Configuration;
using System.Web.Mvc;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;

namespace Qbicles.TraderAPI.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
       // public ApplicationDbContext dbContext = new ApplicationDbContext();

    }

    //public class HomeController : Controller
    //{
    //    public ActionResult Index()
    //    {
    //        return Redirect(ConfigManager.QbiclesUrl);
    //    }
    //}
}
