using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Dds;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.TraderApi;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Qbicles.TraderAPI.Controllers
{
    [RoutePrefix("api/driver")]
    public class DeliveryDriverApiController : BaseApiController
    {
        [Route("logout")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Logout()
        {
            var drivers = new DeliveryDriverApiRules(dbContext).DriverLogout(RequestValue(Request, ClientIdType.PosDriver));
            if (drivers.Status != HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { drivers.Message }, Configuration.Formatters.JsonFormatter);
            return
                Request.CreateResponse(HttpStatusCode.OK);
        }


        [Route("status")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Status(DriverStatus status)
        {
            var drivers = new DeliveryDriverApiRules(dbContext).DriverUpdateStatus(RequestValue(Request, ClientIdType.PosDriver), status);
            if (drivers.Status != HttpStatusCode.OK)
                return Request.CreateResponse(drivers.Status, new { drivers.Message }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [Route("profile")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DriverProfile()
        {
            var driver = new DeliveryDriverApiRules(dbContext).DriverProfile(RequestValue(Request, ClientIdType.PosDriver));
            if (driver != null)
                return Request.CreateResponse(HttpStatusCode.OK, driver, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.NotAcceptable);
        }


        [Route("deliveryinfo")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DeliveryInfo(int deliveryId)
        {
            var delyvery = new DeliveryDriverApiRules(dbContext).DeliveryInfo(RequestValue(Request, ClientIdType.PosDriver), deliveryId);
            if (delyvery != null)
                return Request.CreateResponse(HttpStatusCode.OK, delyvery, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.NotAcceptable);
        }


        [Route("updatelocation")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateLocation(DssLocationModel location)
        {
            var update = new DeliveryDriverApiRules(dbContext).DriverUpdateLocation(RequestValue(Request, ClientIdType.PosDriver), location);
            if (update.Status != HttpStatusCode.OK)
                return Request.CreateResponse(update.Status, new { update.Message }, Configuration.Formatters.JsonFormatter);
            
            return Request.CreateResponse(HttpStatusCode.OK, new { DeliveryId = HelperClass.Converter.Obj2Int(update.TraderId) }, Configuration.Formatters.JsonFormatter);
        }


        [Route("updatestatusdelivery")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateStatusDelivery(DsDeliveryParameter info)
        {
            var updateStatusDelivery = new DeliveryDriverApiRules(dbContext).DriverUpdateStatusDelivery(RequestValue(Request, ClientIdType.PosDriver), info);
            if (updateStatusDelivery.Status == HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.OK, new { Orders = updateStatusDelivery.Message.ParseAs<List<OrderDelivery>>() }, Configuration.Formatters.JsonFormatter);
            return
                Request.CreateResponse(updateStatusDelivery.Status, new { updateStatusDelivery.Message }, Configuration.Formatters.JsonFormatter);
        }
    }
}
