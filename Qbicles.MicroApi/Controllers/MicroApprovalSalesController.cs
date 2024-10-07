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
    [RoutePrefix("api/micro/approval/sale")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroApprovalSalesController : BaseApiController
    {
        [Route("approvalrights")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetSaleReviewApprovalRight(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewApprovalRight(key);
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
        public async Task<HttpResponseMessage> SaleReviewApproval(MicroApprovalModel approval)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = await new MicroApprovalSalesRules(_microContext).MicroSaleReviewApproval(approval);
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
        public HttpResponseMessage GetSaleReviewOverview(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewOverview(key);
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
        public HttpResponseMessage GetSaleReviewStatus(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewStatus(key);

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
        public HttpResponseMessage GetSaleReviewItems(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewItems(key);

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
        public HttpResponseMessage GetSaleReviewTeams(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewTeams(key);

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
        public HttpResponseMessage GetSaleReviewTimelines(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewTimelines(key);

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
        public HttpResponseMessage GetSaleReviewComments(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewComments(key);

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
        public HttpResponseMessage GetSaleReviewFiles(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).GetSaleReviewFiles(key);

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
        public HttpResponseMessage AddCommentToSaleApproval(MicroCommentApprovalModel comment)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalSalesRules(_microContext).AddCommentToApproval(comment);

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
                var refModel = new MicroMediasRules(_microContext).GetTopicFolders(key, "sale");

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
        public HttpResponseMessage AddMediaSaleApprovalReview(MicroMediaUpload microMedia)
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