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
    public class MicroMediaFolderController : BaseApiController
    {
        [Route("filesaccept")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetFileAccept()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediaFolderRules(_microContext).FilesAccept();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("mediafolders")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMediaFolders(int qbicleId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediaFolderRules(_microContext).GetMediaFolders(qbicleId);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("mediafolder/medias")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMediasInFolder(int qbicleId, int folderId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediaFolderRules(_microContext).GetMediasInFolder(qbicleId, folderId);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("mediafolder/movetofolder")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MediaMoveToFolder(int mediaId, int folderId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediaFolderRules(_microContext).MediaMoveFolderById(folderId, mediaId, false);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("mediafolder/movetofolder/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CustomerMediaMoveToFolder(int mediaId, int folderId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediaFolderRules(_microContext).MediaMoveFolderById(folderId, mediaId, true);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("mediafolder")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateUpdateMediaFolder(int qbicleId, int folderId, string name)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediaFolderRules(_microContext).CreateUpdateMediaFolder(folderId, name, qbicleId, _microContext.UserId);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("mediafolder/uploadmedia")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UploadMediaToFolder(MicroMediaUpload media)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).AddActivityMedia(media, false);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("mediafolder/uploadmedia/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UploadMediaToFolderCustomer(MicroMediaUpload media)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).AddActivityMedia(media, true);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}
