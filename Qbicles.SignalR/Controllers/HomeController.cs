using Qbicles.BusinessRules.Helper;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Qbicles.SignalR.Controllers
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
    [System.Web.Http.Authorize]
    public class PingController : ApiController
    {
        [System.Web.Http.Route("api/pos/ping")]
        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage Ping()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.ReasonPhrase = "OK";
            return response;
        }
    }
}