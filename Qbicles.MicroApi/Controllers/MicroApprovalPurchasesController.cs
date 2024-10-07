using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.MicroApi;
using Qbicles.BusinessRules;
using System.Net.Http;
using System.Net;
using System;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    /// <summary>
    ///
    /// </summary>
    [RoutePrefix("api/micro/approval/purchase")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroApprovalPurchasesController : BaseApiController
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("approvalrights")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewApprovalRight(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewApprovalRight(key);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="approval"></param>
        /// <returns></returns>
        [Route("approval")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> PurchaseReviewApproval(MicroApprovalModel approval)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = await new MicroApprovalPurchasesRules(_microContext).MicroPurchaseReviewApproval(approval);
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("overview")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewOverview(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewOverview(key);
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("status")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewStatus(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewStatus(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("items")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewItems(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewItems(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("teams")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewTeams(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewTeams(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("timelines")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewTimelines(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewTimelines(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("comments")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewComments(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewComments(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("files")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPurchaseReviewFiles(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).GetPurchaseReviewFiles(key);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Route("comment")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddCommentToPurchaseApproval(MicroCommentApprovalModel comment)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroApprovalPurchasesRules(_microContext).AddCommentToApproval(comment);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("file/topicfolder")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetTopicFolders(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).GetTopicFolders(key, "purchase");

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="microMedia"></param>
        /// <returns></returns>
        [Route("file")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddMediaPurchaseApprovalReview(MicroMediaUpload microMedia)
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