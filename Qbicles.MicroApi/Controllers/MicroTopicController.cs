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
    [RoutePrefix("api/micro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroTopicController : BaseApiController
    {
        [Route("qbicle/topic")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbicleTopics(int qbicleId)
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                var topics = new MicroTopicRules(_microContext).GetTopicByQbicleId(qbicleId);

                return Request.CreateResponse(HttpStatusCode.OK, topics, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("domain/topic")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDomainTopicNames(int domainId)
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                var topics = new MicroTopicRules(_microContext).GetTopicNamesByDomainId(domainId);

                return Request.CreateResponse(HttpStatusCode.OK, topics, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("topic")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SaveTopic(MicroTopic microTopic)
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                var saved = new MicroTopicRules(_microContext).SaveTopic(microTopic, _microContext.UserId);

                if (saved.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = saved.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("topic")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteTopic(int topicDeleteId, int topicMoveId)
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                var topics = new MicroTopicRules(_microContext).DeleteTopic(topicMoveId, topicDeleteId);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}
