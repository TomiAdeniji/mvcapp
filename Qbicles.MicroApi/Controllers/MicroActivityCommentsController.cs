using Qbicles.BusinessRules.Micro;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;


namespace Qbicles.MicroApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/micro/activity")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroActivityCommentsController : BaseApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Route("comment")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetActivityComments(int activityId, int pageSize = 0)
        {
            HeaderInformation(Request);

            if (_authorizationInformation.Status != HttpStatusCode.OK)
                return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

            var refModel = new MicroActivityCommentsRules(_microContext).GetActivityComments(activityId, pageSize);

            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }       
    }
}
