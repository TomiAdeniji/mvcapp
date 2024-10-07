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
    public class MicroEventsController : BaseApiController
    {
        [Route("event")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicleEventActivity(MicroEventQbicleModel eventQbicle)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroEventsRules(_microContext).CreateQbicleEventActivity(eventQbicle);
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


        [Route("event")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbicleQbicleEvent(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroEventsRules(_microContext).GetQbicleEvent(id);

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

        [Route("event/attending")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AttendingQbicleEventActivity(int attendingId, bool isAttending)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroEventsRules(_microContext).AttendingQbicleEventActivity(attendingId, isAttending);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

    }
}
