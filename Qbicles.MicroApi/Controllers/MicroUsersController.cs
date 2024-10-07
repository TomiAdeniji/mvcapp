using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/user")]
    public class MicroUsersController : BaseApiController
    {

        [Route("nav")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserNavigation()
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                var refModel = new MicroUsersRules(_microContext).GetUserNavigation();
                return Request.CreateResponse(HttpStatusCode.OK, refModel.Object, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}
