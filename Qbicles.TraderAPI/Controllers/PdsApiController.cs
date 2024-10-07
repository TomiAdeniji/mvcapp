using Qbicles.BusinessRules.Firebase;
using Qbicles.BusinessRules.Ods;
using Qbicles.Models.TraderApi;
using System.Net;
using System.Net.Http;
using System.Web.Http;
namespace Qbicles.TraderAPI.Controllers
{
    [RoutePrefix("api/pds")]
    public class PdsApiController : BaseApiController
    {
        /// <summary>
        /// pos_user
        /// </summary>
        /// <returns></returns>
        [Route("device")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DeviceForUser()
        {
            var devices = new OdsApiRules(dbContext).GetDeviceForUser(RequestValue(Request, ClientIdType.PosUser));
            return devices != null ?
                Request.CreateResponse(HttpStatusCode.OK, devices, Configuration.Formatters.JsonFormatter) :
                Request.CreateResponse(HttpStatusCode.NotFound);

        }

        /// <summary>
        /// pos_user
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        [Route("device")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage TabletForDevice(PosDeviceResult device)
        {
            var valid = RequestValue(Request, ClientIdType.PosUser);

            if (string.IsNullOrEmpty($"{device.Id}") || string.IsNullOrEmpty(device.SerialNumber))
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            valid.DeviceId = device.Id;
            valid.SerialNumber = device.SerialNumber;

            var result = new OdsApiRules(dbContext).TabletForDevice(valid);

            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK) :
                Request.CreateResponse(result.Status);
        }


        /// <summary>
        /// pos_serial
        /// API to provide a list of the Orders in the current queue.
        /// This list of orders will be displayed in the Preparation Display System(PDS)
        /// </summary>
        /// <param name="numberOfHours">GetOrderTimeSpan enum
        /// Hours_24 = 1,
        /// Hours_48 = 2,
        /// Hours_72 = 3
        /// </param>
        /// <returns></returns>
        [Route("queue")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage PrepQueue()
        {
            var prepQueue = new OdsApiRules(dbContext).PrepQueue(RequestValue(Request, ClientIdType.PosSerial));
            if (prepQueue == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, prepQueue, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// pos_serial
        /// API to provide a list of the Orders in the current queue.
        /// This list of orders will be displayed in the Preparation Display System(PDS)
        /// </summary>
        /// <param name="numberOfHours">GetOrderTimeSpan enum
        /// Hours_24 = 1,
        /// Hours_48 = 2,
        /// Hours_72 = 3
        /// </param>
        /// <returns></returns>
        [Route("queue/v2")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage PrepQueueV2()
        {
            var prepQueue = new OdsApiRules(dbContext).PrepQueue(RequestValue(Request, ClientIdType.PosSerial), showIsPrepared : true);
            if (prepQueue == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, prepQueue, Configuration.Formatters.JsonFormatter);

        }
        /// <summary>
        /// pos_serial
        /// API to update the status of an order in a Queue
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [Route("status")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage PrepQueueStatusUpdate(PdsOrderUpdate order)
        {
            var qStatus = new Status();
            var prepQueue = new OdsApiRules(dbContext).PrepQueueStatusUpdate(order, RequestValue(Request, ClientIdType.PosSerial), ref qStatus);
            if (prepQueue.Status == HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(prepQueue.Status, prepQueue, Configuration.Formatters.JsonFormatter);
        }

        [Route("status/v2")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage PrepQueueStatusUpdateV2(PdsOrderUpdate order)
        {
            var qStatus = new Status();
            var prepQueue = new OdsApiRules(dbContext).PrepQueueStatusUpdate(order, RequestValue(Request, ClientIdType.PosSerial), ref qStatus);
            if (prepQueue.Status == HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.OK, new { status = qStatus }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(prepQueue.Status, prepQueue, Configuration.Formatters.JsonFormatter);
        }

        [Route("updatedevicetoken")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage PrepUpdateFirebaseToken(PdsFirebaseTokenUpdate pdsFirebase)
        {
            var response = new FirebaseNotificationRules(dbContext).AddUpdateDeviceToken(pdsFirebase, RequestValue(Request, ClientIdType.PosSerial));
            if (response)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [Route("queue/order")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPrepQueue(int id)
        {
            var prepQueue = new OdsApiRules(dbContext).GetQueueOrderById(id,RequestValue(Request, ClientIdType.PosSerial));
            if (prepQueue == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, prepQueue, Configuration.Formatters.JsonFormatter);

        }
    }
}
