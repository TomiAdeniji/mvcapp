using Qbicles.BusinessRules;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroUploadController : BaseApiController
    {
        [Route("s3upload")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MicroUpload(string objKey)
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                var s3Rules = new BusinessRules.Azure.AzureStorageRules(_microContext.Context);
                s3Rules.ProcessingMediaS3(objKey);

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
