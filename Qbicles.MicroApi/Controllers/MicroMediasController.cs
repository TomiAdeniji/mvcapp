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
    [RoutePrefix("api/micro/activity")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroMediasController : BaseApiController
    {

        [Route("media")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetActivityMedias(int activityId, int pageSize)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).GetActivityMedias(activityId, pageSize);

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

        [Route("media")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddActivityMedia(MicroMediaUpload microMedia)
        {
            HeaderInformation(Request);
            try
            {
                ReturnJsonModel refModel;
                if (microMedia.Id == 0)
                    refModel = new MicroMediasRules(_microContext).AddActivityMedia(microMedia, false);
                else
                    refModel = new MicroMediasRules(_microContext).UpdateActivityMedia(microMedia, false);


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

        [Route("media/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddActivityMediaCustomer(MicroMediaUpload microMedia)
        {
            HeaderInformation(Request);
            try
            {
                ReturnJsonModel refModel;
                if (microMedia.Id == 0)
                    refModel = new MicroMediasRules(_microContext).AddActivityMedia(microMedia, true);
                else
                    refModel = new MicroMediasRules(_microContext).UpdateActivityMedia(microMedia, true);


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

        [Route("mediaview")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMedia(int id)
        {
            HeaderInformation(Request);
            try
            {

                var refModel = new MicroMediasRules(_microContext).GetMedia(id);

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

        [Route("media/version")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage MediaDeleteVersion(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).MediaDeleteVersion(id, false);

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

        [Route("media/version/customer")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage CustomerMediaDeleteVersion(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).MediaDeleteVersion(id, true);

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

        [Route("media/version")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MediaAddVersion(MicroMediaUpload microMedia)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).MediaVersionAdd(microMedia, false);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }



        [Route("media/version/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CustomerMediaAddVersion(MicroMediaUpload microMedia)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).MediaVersionAdd(microMedia, true);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("media/version/download")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage MediaDownloadVersion(string fileKey)
        {
            HeaderInformation(Request);
            try
            {

                var refModel = new MicroMediasRules(_microContext).MediaVersionDownload(fileKey);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { uri = refModel.msg }, Configuration.Formatters.JsonFormatter);
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
