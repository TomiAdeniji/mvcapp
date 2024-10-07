using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/notification")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroNotificationsController : BaseApiController
    {

        [Route("list")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetUserNotifications(PaginationRequest pagination)
        {
            HeaderInformation(Request);
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, new MicroNotificationsRules(_microContext).GetNotifications(pagination), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("detail")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetNotification(int id, bool isRenderHtml = false)
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK, new MicroNotificationsRules(_microContext).GetNotificationDetail(id, isRenderHtml), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("count")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetCountNotifications()
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                var refModel = new MicroNotificationsRules(_microContext).GetCountNotifications();
                return Request.CreateResponse(HttpStatusCode.OK, new { Notifications = refModel }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("read")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage ReadNotification(string notificationId)
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                new MicroNotificationsRules(_microContext).MarkAsReadNotification(notificationId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("activity")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetActivityDetail(MicroNotification notification)
        {
            try
            {
                /*
                 var url = "/Qbicles/Approval";
                            switch (approvalType) {
                                case "approval":
                                    url = "/Qbicles/Approval";
                                    break;
                                case "journal":
                                    url = "/Bookkeeping/ApprovalBookkeeping";
                                    break;
                                case "sale":
                                    url = "/TraderSales/SaleReview?id=" + id;
                                    break;
                                case "purchase":
                                    url = "/TraderPurchases/PurchaseReview?id=" + id;
                                    break; 
                                case "transfer":
                                    url = "/TraderTransfers/TransferReview?id=" + id;
                                    break;
                                case "contact":
                                    url = "/TraderContact/ContactReview?id=" + id;
                                    break;
                                case "invoice":
                                    url = "/TraderInvoices/InvoiceReview?id=" + id;
                                    break;
                                case "payment":
                                    url = "/TraderPayments/PaymentReview?id=" + id;
                                    break;
                                case "spotCount":
                                    url = "/TraderSpotCount/SpotCountReview?id=" + id;
                                    break;
                                case "wasteReport":
                                    url = "/TraderWasteReport/WasteReportReview?id=" + id;
                                    break;
                                case "manufacturingjobs":
                                    url = "/Manufacturing/ManuJobReview?id=" + id;
                                    break;
                                default:
                                    url = approvalType;
                                    break;
                            }
                 */
                //MicroNotification {EventId, ApprovalType, ActivityId -- string of Key, convert to int}
                //eventId -> show
                //HeaderInformation(Request);
                //if (_authorizationInformation.Status != HttpStatusCode.OK)
                //ApprovalType: postSelelected(qbicleKey, postKey, module)

                return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                // return Request.CreateResponse(HttpStatusCode.OK, new MicroNotificationsRules(_microContext).GetActivityDetail(id, isRenderHtml), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}
