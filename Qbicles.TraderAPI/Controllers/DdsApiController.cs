using System.Net;
using System.Net.Http;
using Qbicles.Models.TraderApi;
using Qbicles.BusinessRules.Dds;
using Qbicles.BusinessRules;
using Qbicles.Models.Trader.DDS;
using System.Web.Http;

namespace Qbicles.TraderAPI.Controllers
{
    [RoutePrefix("api/dds")]
    public class DdsApiController : BaseApiController
    {

        [Route("device")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DeviceForUser()
        {
            var devices = new DdsApiRules(dbContext).GetDeviceForUser(RequestValue(Request, ClientIdType.PosUser));
            return devices != null ?
                Request.CreateResponse(HttpStatusCode.OK, devices, Configuration.Formatters.JsonFormatter) :
                Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [Route("device")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage TabletForDevice(PosDeviceResult device)
        {
            var valid = RequestValue(Request, ClientIdType.PosUser);
            valid.DeviceId = device.Id;
            valid.SerialNumber = device.SerialNumber;

            var result = new DdsApiRules(dbContext).TabletForDevice(valid);

            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK) :
                Request.CreateResponse(result.Status, result, Configuration.Formatters.JsonFormatter);
        }


        //============= API DDS Driver =======================
        /// <summary>
        /// API is required to get Driver information to display in the Delivery Display System.
        /// </summary>
        /// <returns></returns>
        [Route("driverget")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetDriver(DdsDriverParameter parameter)
        {
            var drivers = new DdsApiRules(dbContext).DriverGet(RequestValue(Request, ClientIdType.PosSerial), parameter);
            if (!drivers.result)
                return Request.CreateResponse((HttpStatusCode)drivers.actionVal, drivers.msg, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.OK, drivers.Object, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// API is required to be able to add a driver to a delivery.
        /// The delivery and driver are identified by the supplied parameters.
        /// The Driver property of the identified Delivery is to be set to the Driver identified by the Driver Id.
        /// Delivery Id (Must be supplied in response to call to 'delivery')
        /// Id(Driver Id is supplied in the response to 'driver' GetDriver) 
        /// </summary>
        /// <returns>
        /// If Driver cannot be found then return an error to say that is not possible.
        /// If Delivery cannot be found then return an error to say that is not possible.
        /// </returns>
        [Route("driver")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddDriver(DeliveryDriverParameter deliveryDriver)
        {
            var result = new DdsApiRules(dbContext).DriverAdd(RequestValue(Request, ClientIdType.PosSerial), deliveryDriver);
            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK) :
                Request.CreateResponse(result.Status, new { result.Message }, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// API is required to be able to delete the Driver from a Delivery.
        /// The Driver identified by the Driver ID is to be removed from the Driver property of the Delivery identified by the Delivery Id.
        /// Delivery Id (Must be supplied in response to call to 'delivery')
        /// Driver Id(Driver Id is supplied in the response to 'driver' GetDriver)
        /// </summary>
        /// <returns>
        /// If Driver cannot be found then return an error to say that is not possible.
        /// If Delivery cannot be found then return an error to say that is not possible.
        /// </returns>
        [Route("driver")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage RemoveDriver(DeliveryDriverParameter deliveryDriver)
        {
            var result = new DdsApiRules(dbContext).DriverRemove(RequestValue(Request, ClientIdType.PosSerial), deliveryDriver);
            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK) :
                Request.CreateResponse(result.Status, result, Configuration.Formatters.JsonFormatter);
        }

        //============= API DDS Delivery =======================

        /// <summary>
        /// API call is required to be able to add a delivery to the system.
        /// Device serial number (from token) => Device => DeliveryQueue
        /// The UserId can be extracted from the token for the CreatedBy field
        /// </summary>
        /// <returns></returns>
        [Route("delivery")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddDelivery()
        {
            var result = new DdsApiRules(dbContext).DeliveryAdd(RequestValue(Request, ClientIdType.PosSerial));
            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK, new { Token = result.Message }, Configuration.Formatters.JsonFormatter) :
                Request.CreateResponse(result.Status, result, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// An API is required to be able to delete a Delivery.
        /// The Delivery, identified by the Delivery Id, is added to the DeliveryQueueArchive that is associated with the current DeliveryQueue.
        /// The Delivery is then removed from the DeliveryQueue(Delivery.DeliveryQueue = NULL).
        /// The status of the Delivers is set to Deleted.
        /// Delivery Id (Must be supplied in response to call to 'delivery')
        /// </summary>
        /// <returns>
        ///    If Delivery cannot be found then return an error to say that is not possible.
        /// Set Status = Deleted
        /// Move it to the DeliveryQueueArchive
        /// break the connection with the DeliveryQueue
        /// </returns>
        [Route("delivery")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteDelivery(DsDeliveryParameter deliveryParameter)
        {
            var result = new DdsApiRules(dbContext).DeliveryRemove(RequestValue(Request, ClientIdType.PosSerial), deliveryParameter);
            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK) :
                Request.CreateResponse(result.Status, result, Configuration.Formatters.JsonFormatter);
        }


        /// <summary>
        /// API that is used to set just the Route proterty of a Delivery.
        /// </summary>
        /// <returns></returns>
        [Route("delivery/route")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DeliveryRoute(DsDeliveryRoutes deliveryRoutes)
        {
            var result = new DdsApiRules(dbContext).DeliveryRoute(RequestValue(Request, ClientIdType.PosSerial), deliveryRoutes);
            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK) :
                Request.CreateResponse(result.Status, result, Configuration.Formatters.JsonFormatter);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="deliveryOrder"></param>
        /// <returns></returns>
        [Route("updatedelivery")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DeliveryUpdate(DsDelivery delivery)
        {
            lock (MultipleThreads.DeliveryUpdateInstance)
            {
                var result = new DdsApiRules(dbContext).DeliveryUpdate(RequestValue(Request, ClientIdType.PosSerial), delivery);
                if ((HttpStatusCode)result.Object != HttpStatusCode.NotAcceptable && !result.result)
                    return Request.CreateResponse((HttpStatusCode)result.Object, new { Message = result.msg }, Configuration.Formatters.JsonFormatter);


                if ((HttpStatusCode)result.Object != HttpStatusCode.OK)
                    if (result.actionVal == 3)
                        return Request.CreateResponse((HttpStatusCode)result.Object, new { Message = result.msg }, Configuration.Formatters.JsonFormatter);
                    else
                        return Request.CreateResponse((HttpStatusCode)result.Object, result.msg, Configuration.Formatters.JsonFormatter);

                if (string.IsNullOrEmpty(result.msgName))
                    return Request.CreateResponse(HttpStatusCode.OK, new { Token = result.msg }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse((HttpStatusCode)result.Object,
                    new
                    {
                        Token = result.msg,
                        Warning = result.msgName
                    }, Configuration.Formatters.JsonFormatter);
            }
        }



        /// <summary>
        /// The API to get information from delivery information from the Trader system
        /// Parameters: Device (from token) => DeliveryQueue => Collection of Deliveries
        /// </summary>
        /// <returns></returns>
        [Route("delivery")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDelivery(int traderId)
        {
            var delivery = new DdsApiRules(dbContext).DeliveryGet(RequestValue(Request, ClientIdType.PosSerial), traderId);
            if (delivery == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, delivery, Configuration.Formatters.JsonFormatter);
        }


        [Route("deliveries")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDeliveries(DeliveryStatus? status)
        {
            var deliveries = new DdsApiRules(dbContext).DeliveriesGet(RequestValue(Request, ClientIdType.PosSerial), status);
            if (deliveries == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, deliveries, Configuration.Formatters.JsonFormatter);
        }

        //============= API DDS Order =======================
        /// <summary>
        /// API to provide a list of the Orders in the current queue.
        /// This list of orders will be displayed in the Preparation Display System(PDS)
        /// </summary>
        /// <param name="includeInDelivery">Default is false I.e.IncludeInDelivery = false returns those QueueOrders that are NOT in any Delivery</param>
        /// <returns></returns>
        [Route("order")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DdsOrderGet(bool includeInDelivery = false)
        {

            var prepQueue = new DdsApiRules(dbContext).OrderGet(RequestValue(Request, ClientIdType.PosSerial), includeInDelivery);
            if (prepQueue == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            return Request.CreateResponse(HttpStatusCode.OK, prepQueue, Configuration.Formatters.JsonFormatter);

        }

    }
}
