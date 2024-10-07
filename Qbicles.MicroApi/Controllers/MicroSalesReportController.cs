using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/sales/report")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroSalesReportController : BaseApiController
    {
        [Route("option/filters")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetOptionFilter(int domainId)
        {
            HeaderInformation(Request);

            var refModel = new MicroSalesReportRules(_microContext).GetOptionFilter(domainId);

            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }

        [Route("summaries")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetSummaries(SalesReportFilterParameter filter)
        {
            HeaderInformation(Request);           

            var refModel = new MicroSalesReportRules(_microContext).GetSummaries(filter);
            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }


        [Route("productgroupdetail")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ProductGroupDetail(SalesReportFilterParameter filter)
        {
            HeaderInformation(Request);

            var refModel = new MicroSalesReportRules(_microContext).ProductGroupDetail(filter);
            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }
    }
}