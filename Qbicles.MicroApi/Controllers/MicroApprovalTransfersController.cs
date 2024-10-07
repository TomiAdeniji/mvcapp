using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.MicroApi;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroApprovalTransfersController : BaseApiController
    {
        [Route("approvalrights")]
        [AcceptVerbs("GET")]

        public HttpResponseMessage GetTransferReviewApprovalRight(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GettransferReviewApprovalRight(key);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("approval")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> TransferReviewApproval(MicroApprovalModel approval)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = await new MicroApprovalTransfersRules(_microContext).MicroTransferReviewApproval(approval);
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

        [Route("overview")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTransferReviewOverview(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GetTransferReviewOverview(key);
                if (refModel.ActivityId == -1)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "You do not have access" }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("status")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTransferReviewStatus(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GetTransferReviewStatus(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("items")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTransferReviewItems(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GetTransferReviewItems(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("teams")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTransferReviewTeams(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GetTransferReviewTeams(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("timelines")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTransferReviewTimelines(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GettransferReviewTimelines(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("comments")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTransferReviewComments(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GettransferReviewComments(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("files")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTransferReviewFiles(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).GettransferReviewFiles(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("comment")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddCommentToTransferApproval(MicroCommentApprovalModel comment)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalTransfersRules(_microContext).AddCommentToApproval(comment);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("file/topicfolder")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTopicFolders(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).GetTopicFolders(key, "transfer");

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("file")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddMediaTransferApprovalReview(MicroMediaUpload microMedia)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).AddActivityMedia(microMedia, false);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, refModel.msg, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}