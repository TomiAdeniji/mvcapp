using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/qbicle")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroTaksController : BaseApiController
    {
        [Route("task")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicleTaskActivity(MicroTaskQbicleModel taskQbicle)
        {
            HeaderInformation(Request);
            var refModel = new MicroTasksRules(_microContext).CreateQbicleTaskActivity(taskQbicle, false);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new 
                {
                    Id = refModel.actionVal,
                    ResponseObject = refModel.Object,
                    Priority = refModel.msg,
                    TimeBegin = refModel.msgName
                }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("task/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicleTaskActivityCustomer(MicroTaskQbicleModel taskQbicle)
        {
            HeaderInformation(Request);
            var refModel = new MicroTasksRules(_microContext).CreateQbicleTaskActivity(taskQbicle, true);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Id = refModel.actionVal, 
                    ResponseObject = refModel.Object,
                    Priority = refModel.msg,
                    TimeBegin = refModel.msgName
                }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("task/timelogging")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage TimeLogging(TimeLogging timeLogging)
        {
            HeaderInformation(Request);
            var refModel = new MicroTasksRules(_microContext).TaskTimeLogging(timeLogging, false);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, refModel.Object, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
        }

        [Route("task/timelogging/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage TimeLoggingCustomer(TimeLogging timeLogging)
        {
            HeaderInformation(Request);
            var refModel = new MicroTasksRules(_microContext).TaskTimeLogging(timeLogging, true);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, refModel.Object, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);


        }

        [Route("task/timelogging")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage TimeLoggingDelete(int id)
        {
            HeaderInformation(Request);
            var refModel = new MicroTasksRules(_microContext).TimeLoggingDelete(id);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("task")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbicleTask(int id)
        {
            HeaderInformation(Request);
            var refModel = new MicroTasksRules(_microContext).GetQbicleTask(id);

            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }

        [Route("task/startprogress")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage StartProgress(int id)
        {
            HeaderInformation(Request);
            var refModel = new MicroTasksRules(_microContext).StartProgress(id,"");

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
        }


        [Route("task/markcomplete")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MarkAsComplete(int id)
        {
            HeaderInformation(Request);

            var refModel = new MicroTasksRules(_microContext).MarkAsComplete(id,"");

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }


    }
}
