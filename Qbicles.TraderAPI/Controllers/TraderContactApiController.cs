using System.Net;
using System.Net.Http;
using Qbicles.Models.TraderApi;
using System.Web.Http;
using Qbicles.BusinessRules.TraderApi;
using Qbicles.Models.Qbicles;
using Qbicles.Models.Trader;

namespace Qbicles.TraderAPI.Controllers
{
    [RoutePrefix("api/trader")]
    public class TraderContactApiController : BaseApiController
    {
        [Route("contact/filter")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ContactFilter(Customer filter, int page, int pageSize = 10)
        {
            var totalRecord = 0;
            var totalCustomer = 0;

            var contacts = new TraderContactApiRules(dbContext).ContactFilter(filter, RequestValue(Request, ClientIdType.PosSerial), page, pageSize, ref totalRecord, ref totalCustomer);
            return Request.CreateResponse(HttpStatusCode.OK, new { contacts, TotalPage = totalRecord, TotalContact = totalCustomer }, Configuration.Formatters.JsonFormatter);

        }

        [Route("contact")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ContactCreate(Customer customer)
        {
            var result = new TraderContactApiRules(dbContext).ContactCreate(customer, RequestValue(Request, ClientIdType.PosSerial));

            if (!result.result)
                return Request.CreateResponse((HttpStatusCode)result.Object, new { Message = result.msg }, Configuration.Formatters.JsonFormatter);

            return Request.CreateResponse(HttpStatusCode.OK, new { Id = result.msgId }, Configuration.Formatters.JsonFormatter);
        }


        [Route("contact/address")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ContactAddressCreate(TraderAddress address, CountryCode countryCode, int contactId)
        {
            RequestValue(Request, ClientIdType.PosSerial);
            var result = new TraderContactApiRules(dbContext).ContactAddressCreate(address, countryCode, contactId);

            if (!result.result)
                return Request.CreateResponse((HttpStatusCode)result.Object, result.msg, Configuration.Formatters.JsonFormatter);

            return Request.CreateResponse(HttpStatusCode.OK, new { Id = result.msgId }, Configuration.Formatters.JsonFormatter);
        }

        [Route("contact/pin/verify")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ContactPINVerify(PinVerify verify)
        {
            var verified = new TraderContactApiRules(dbContext).ContactPINVerify(verify, RequestValue(Request, ClientIdType.PosSerial));

            if (!verified.result)
                return Request.CreateResponse((HttpStatusCode)verified.Object, new { verified = false, message = verified.msg }, Configuration.Formatters.JsonFormatter);

            return Request.CreateResponse(HttpStatusCode.OK, new { verified = true }, Configuration.Formatters.JsonFormatter);
        }




        [Route("voucher/contact")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage ContactGetVoucher(int id)
        {
            try
            {
                var voucheres = new TraderContactApiRules(dbContext).ContactGetVoucher(id, RequestValue(Request, ClientIdType.PosSerial));

                if (!voucheres.result)
                    return Request.CreateResponse((HttpStatusCode)voucheres.Object, new { verified = false, message = voucheres.msg }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK, voucheres.Object, Configuration.Formatters.JsonFormatter);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message }, Configuration.Formatters.JsonFormatter);

            }

        }
    }
}