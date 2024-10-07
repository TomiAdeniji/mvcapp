using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model.Firebase;
using Qbicles.BusinessRules.Ods;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models.TraderApi;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Qbicles.TraderAPI.Controllers
{
    [RoutePrefix("api/pos")]
    public class PosSaleOrderController : BaseApiController
    {
        [Route("testfirebase")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage TestFirebase(DevicesToken registrationTokens)
        {
            //List<DeviceToken> registrationTokens = new List<DeviceToken>();
            new OdsApiRules(dbContext).TestFirebase(registrationTokens,RequestValue(Request, ClientIdType.PosSerial));
            return Request.CreateResponse(HttpStatusCode.OK);

        }

        /// <summary>
        /// pos_serial
        /// </summary>
        /// <param name="saleOrder"></param>
        /// <returns></returns>
        [Route("sendtoprep")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SendToPrep(Order saleOrder)
        {
            var result = new OdsApiRules(dbContext).SendToPrep(saleOrder, RequestValue(Request, ClientIdType.PosSerial));
            if (result.Status == HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.OK, new { result.TraderId }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(result.Status, result, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// client_id: pos_serial
        /// </summary>
        /// <param name="saleOrder"></param>
        /// <returns></returns>
        [Route("order")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Order(Order saleOrder)
        {
            if (saleOrder == null)
            {
                var validResult = new IPosResult
                {
                    IsTokenValid = true,
                    Message = ResourcesManager._L("ERROR_MSG_4"),
                    Status = HttpStatusCode.NotAcceptable
                };
                return Request.CreateResponse(validResult.Status, validResult, Configuration.Formatters.JsonFormatter);
            }

            var result = new PosRules(dbContext).PosOrder(saleOrder, RequestValue(Request, ClientIdType.PosSerial));

            if (result.Status == HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.OK, new { result.TraderId }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(result.Status, result, Configuration.Formatters.JsonFormatter);
        }


        [Route("voucher/order")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage OrderVoucherCalculation(Order order)
        {

            if (order == null)
            {
                var validResult = new IPosResult
                {
                    IsTokenValid = true,
                    Message = ResourcesManager._L("ERROR_MSG_4"),
                    Status = HttpStatusCode.NotAcceptable
                };
                return
                    Request.CreateResponse(validResult.Status, validResult, Configuration.Formatters.JsonFormatter);
            }
            if (order.VoucherId <= 0)
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = "Voucher required!" }, Configuration.Formatters.JsonFormatter);
            //Test
            //var resultWeb = new PosRules(dbContext).OrderVoucherCalculation2Web(order);
            RequestValue(Request, ClientIdType.PosSerial);

            var result = new PosRules(dbContext).OrderVoucherCalculation2Pos(order);

            return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }

        [Route("order/cancel")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> OrderCancel(OrderCancelOrPrintCheckModel posOrderCancel)
        {

            var result = await new PosRules(dbContext).PosOrderCancel(posOrderCancel, RequestValue(Request, ClientIdType.PosSerial));

            if (result.Status == HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(result.Status, new { result.Status, result.Message }, Configuration.Formatters.JsonFormatter);
        }

        [Route("order/comment")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage OrderComment(OrderMessageModel orderMessage)
        {
            new PosRules(dbContext).OrderComment(RequestValue(Request, ClientIdType.PosDriver).UserId, orderMessage);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("order/printcheck")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> PosOrderPrintCheck(OrderCancelOrPrintCheckModel oosOrderPrintCheck)
        {
            var result = await new PosRules(dbContext).PosOrderPrintCheck(oosOrderPrintCheck, RequestValue(Request, ClientIdType.PosSerial));

            if (result.Status == HttpStatusCode.OK)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(result.Status, new { result.Status, result.Message }, Configuration.Formatters.JsonFormatter);
        }
    }
}
