using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/catalogues")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroCataloguesController : BaseApiController
    {

        [Route("countries")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetCountries()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroCataloguesRules(_microContext).GetCountries();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("qbicle/topics")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTopics(int qbicleId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroCataloguesRules(_microContext).GetTopic(qbicleId);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("interests")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetInterests()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroCataloguesRules(_microContext).GetInterests();
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