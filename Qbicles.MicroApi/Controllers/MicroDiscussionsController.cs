using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/qbicle")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroDiscussionsController : BaseApiController
    {
        [Route("discussion")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbicleDiscussion(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroDiscussionsRules(_microContext).GetQbicleDiscussion(id);

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

        [Route("discussion")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicleDiscussion(DiscussionQbicleModel discussion)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroDiscussionsRules(_microContext).CreateQbicleDiscussion(discussion, false);

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

        /// <summary>
        /// Add a Participant to the discussion
        /// </summary>
        /// <param name="discussion">{Id=x,Assignee=['userid'],}</param>
        /// <returns></returns>
        [Route("discussion/participant")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage QbicleDiscussionParticipantAdd(DiscussionQbicleModel discussion)
        {
            return DiscussionParticipant(discussion, true, false);
        }

        [Route("discussion/participant")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage QbicleDiscussionParticipantRemove(int id, int qbicleId, string assignee)
        {
            var discussion = new DiscussionQbicleModel
            {
                Id = id,
                QbicleId = qbicleId,
                Assignee = assignee.Split(',')
            };
            return DiscussionParticipant(discussion, false, false);
        }

        /// <summary>
        /// Add a Participant to the discussion
        /// </summary>
        /// <param name="discussion">{Id=x,Assignee=['userid'],}</param>
        /// <returns></returns>
        [Route("discussion/participant/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CustomerQbicleDiscussionParticipantAdd(DiscussionQbicleModel discussion)
        {
            return DiscussionParticipant(discussion, true, true);
        }

        [Route("discussion/participant/customer")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage CustomerQbicleDiscussionParticipantRemove(int id, int qbicleId, string assignee)
        {
            var discussion = new DiscussionQbicleModel
            {
                Id = id,
                QbicleId = qbicleId,
                Assignee = assignee.Split(',')
            };
            return DiscussionParticipant(discussion, false, true);
        }
        
        /// <summary>
        /// Add a Participant to the discussion
        /// </summary>
        /// <param name="discussion">{Id=x,Assignee=['userid'],}</param>
        ///  <param name="isAdd">true-add, false-remove</param>
        /// <returns></returns>
        public HttpResponseMessage DiscussionParticipant(DiscussionQbicleModel discussion, bool isAdd, bool isCreatorTheCustomer)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroDiscussionsRules(_microContext).QbicleDiscussionParticipant(discussion, isAdd, isCreatorTheCustomer);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}
