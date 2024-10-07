using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroPostController : BaseApiController
    {
        /// <summary>
        /// chat from qbicles
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [Route("stream/post")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreatePostOnStream(MicroPostParameter post)
        {
            HeaderInformation(Request);

            post.IsCreatorTheCustomer = false;
            var refModel = new MicroPostRules(_microContext).CreatePostOnStream(post);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// chat from Community customer
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [Route("stream/post/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreatePostOnStreamCustomer(MicroPostParameter post)
        {
            HeaderInformation(Request);

            post.IsCreatorTheCustomer = true;
            var refModel = new MicroPostRules(_microContext).CreatePostOnStream(post);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// chat from B2C/Detail
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [Route("b2c/comms/post")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreatePost(MicroPostParameter post)
        {
            HeaderInformation(Request);

            post.IsCreatorTheCustomer = false;
            var refModel = new MicroPostRules(_microContext).CreatePostOnStream(post);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// chat from b2c order
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Route("b2c/order/chat")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2COrderChatAdd(MicroPostParameter comment)
        {
            HeaderInformation(Request);

            var qbicleId = _microContext.Context.B2COrderCreations.FirstOrDefault(e => e.Id == comment.ActivityId).Qbicle.Id;

            comment.QbicleId = qbicleId;
            comment.ActivityType = StreamType.DiscussionOrder;
            comment.IsCreatorTheCustomer = false;
            var refModel = new MicroActivityCommentsRules(_microContext).AddActivityComment(comment);

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, refModel.msg, Configuration.Formatters.JsonFormatter);

        }


        /// <summary>
        /// chat from b2c order
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Route("b2c/order/chat/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2COrderChatAddCustomer(MicroPostParameter comment)
        {
            HeaderInformation(Request);

            var qbicleId = _microContext.Context.B2COrderCreations.FirstOrDefault(e => e.Id == comment.ActivityId).Qbicle.Id;

            comment.QbicleId = qbicleId;
            comment.ActivityType = StreamType.DiscussionOrder;
            comment.IsCreatorTheCustomer = true;
            var refModel = new MicroActivityCommentsRules(_microContext).AddActivityComment(comment);

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, refModel.msg, Configuration.Formatters.JsonFormatter);

        }
        /// <summary>
        /// comment/chat in activity detail
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Route("activity/commentadd")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddActivityComment(MicroPostParameter comment)
        {
            HeaderInformation(Request);
            comment.IsCreatorTheCustomer = false;
            var refModel = new MicroActivityCommentsRules(_microContext).AddActivityComment(comment);

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, refModel.msg, Configuration.Formatters.JsonFormatter);

        }
        /// <summary>
        /// comment/chat in activity detail
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Route("activity/commentadd/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddActivityCommentCustomer(MicroPostParameter comment)
        {
            HeaderInformation(Request);
            comment.IsCreatorTheCustomer = true;
            var refModel = new MicroActivityCommentsRules(_microContext).AddActivityComment(comment);

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, refModel.msg, Configuration.Formatters.JsonFormatter);

        }

    }
}
