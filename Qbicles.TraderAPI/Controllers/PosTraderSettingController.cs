
using Qbicles.Models.TraderApi;
using Qbicles.BusinessRules.PoS;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Qbicles.TraderAPI.Controllers
{
    public class PosTraderSettingController : BaseApiController
    {
        /// <summary>
        /// client_id: pos_user
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/pos/settings")]
        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage PosSettings()
        {
            var setting = new PosSettingRules(dbContext).GetPosSettings(RequestValue(Request, ClientIdType.PosSerial));
            if (!setting.result)
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, (IPosResult)setting.Object, Configuration.Formatters.JsonFormatter);
            else
                return Request.CreateResponse(HttpStatusCode.OK, (PosSettingsModel)setting.Object, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// Preparation settings
        /// client_id: pos_user
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/pds/settings")]
        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage PdsSettings()
        {

            var setting = new PosSettingRules(dbContext).GetPreparationSettings(RequestValue(Request, ClientIdType.PosSerial));
            if (!setting.result)
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, (IPosResult)setting.Object, Configuration.Formatters.JsonFormatter);
            else
                return Request.CreateResponse(HttpStatusCode.OK, (PdsSettingsModel)setting.Object, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// Get Delivery setttings
        /// client_id: pos_user
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/dds/settings")]
        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage DdsSettings()
        {
            var setting = new PosSettingRules(dbContext).GetDeliverySettings(RequestValue(Request, ClientIdType.PosSerial));
            if (!setting.result)
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, (IPosResult)setting.Object, Configuration.Formatters.JsonFormatter);
            else
                return Request.CreateResponse(HttpStatusCode.OK, (DdsSettingsModel)setting.Object, Configuration.Formatters.JsonFormatter);

        }
    }
}
