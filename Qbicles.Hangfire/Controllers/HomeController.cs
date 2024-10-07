using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using System.Web.Mvc;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public ApplicationDbContext dbContext = new ApplicationDbContext();
        public bool SystemRoleValidation(string userId, string roleName)
        {
            return new QbicleRules(dbContext).SystemRoleValidation(userId, roleName);
        }

    }


    public class HomeController : BaseController
    {
        private string baseUrl = ConfigManager.QbiclesUrl;
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdAuth = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userIdAuth))
                    return RedirectToAction("Login", "Account");
                if(!SystemRoleValidation(userIdAuth, SystemRoles.SystemAdministrator))
                    return Redirect(baseUrl);


                var user = dbContext.QbicleUser.Find(userIdAuth);

                if (user == null)
                    return RedirectToAction("Login", "Account");


                return View();
            }
            return RedirectToAction("Login", "Account");

        }
        public ActionResult Hangfire()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdAuth = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userIdAuth))
                    return RedirectToAction("Login", "Account");

                if (!SystemRoleValidation(userIdAuth, SystemRoles.SystemAdministrator))
                    return Redirect(baseUrl);

                var user = dbContext.QbicleUser.Find(userIdAuth);

                if (user == null)
                    return RedirectToAction("Login", "Account");


                return Redirect("Dashboard");
            }
            return RedirectToAction("Login", "Account");
        }
    }
}