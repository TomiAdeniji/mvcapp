using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;


namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/report/inventory")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroInventoryReportsController : BaseApiController
    {
        [Route("option/filters")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetOptionFilter(int domainId)
        {
            HeaderInformation(Request);

            var refModel = new MicroInventoryReportRules(_microContext).GetOptionFilter(domainId);

            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }

        [Route("lists")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetList(InventoryReportFilterModel parameter)
        {
            HeaderInformation(Request);

            var result = new MicroInventoryReportRules(_microContext).GetInventoryServerSide(parameter);

            return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);

        }

        [Route("unit/update")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateInventoryUnit(InventoryReportFilterModel parameter)
        {
            HeaderInformation(Request);

            var result = new MicroInventoryReportRules(_microContext).UpdateInventoryUnit(parameter);

            return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);

        }
    }
}