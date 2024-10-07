using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroChattingController : BaseApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toUsers"></param>
        /// <param name="typing">true: chatting - false: end chat</param>
        /// <returns></returns>
        [Route("community/chatting")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MicroChatting(string toUsers, bool typing, int dicussionId = 0)
        {
                HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                new MicroChattingRules(_microContext).MicroChatting(toUsers, typing, dicussionId);
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