using Qbicles.BusinessRules;
using Qbicles.BusinessRules.MicroApi;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/trader/contact")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroContactController : BaseApiController
    {
        [Route("getbyid")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetContactById(string contactId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroContactRules(_microContext).GetContactById(contactId);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        
        }
    }
}