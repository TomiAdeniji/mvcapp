using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/qbicle")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroLinksController : BaseApiController
    {
        [Route("link")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicleLinkActivity(MicroLinkQbicleModel linkQbicle)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroLinksRules(_microContext).CreateQbicleLinkActivity(linkQbicle);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }



        [Route("link")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbicleLink(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroLinksRules(_microContext).GetQbicleLink(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}
