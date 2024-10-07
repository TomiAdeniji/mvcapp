using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/b2c/order")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroB2COrderController : BaseApiController
    {
        /// <summary>
        /// Get catalogues from a business
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        [Route("catalogues")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetB2CCatalogues(string domainKey)
        {
            HeaderInformation(Request);

            var refModel = new MicroB2CRules(_microContext).GetB2CCatalogues(domainKey);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// Create b2c order when click to a catalog by customer
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="catalogKey"></param>
        /// <returns>
        /// order infomation
        /// category's items 
        /// Categories list( for search)
        /// </returns>
        [Route("create")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateB2COrder(string domainKey, int catalogId)
        {
            if (string.IsNullOrEmpty(domainKey))
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Domain key is required!" }, Configuration.Formatters.JsonFormatter);

            HeaderInformation(Request);

            var refModel = new MicroB2CRules(_microContext).CreateB2COrder(domainKey, catalogId, false);
            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }

        [Route("create/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateCustomerB2COrder(string domainKey, int catalogId)
        {
            if (string.IsNullOrEmpty(domainKey))
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Domain key is required!" }, Configuration.Formatters.JsonFormatter);

            HeaderInformation(Request);

            var refModel = new MicroB2CRules(_microContext).CreateB2COrder(domainKey, catalogId, true);
            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }


        /// <summary>
        /// Call this api to get changed when recieved a notifiation update the Order fro Business
        /// Get B2COrderCreation (Discussion) by tradeOrderId
        /// </summary>
        /// <param name="tradeId">B2COrderCreation.TradeOrder.Id(B2COrderCreation as Discussion)</param>
        /// <returns></returns>
        [Route("detail")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetB2COrder(int tradeId)
        {
            HeaderInformation(Request);

            var refModel = new MicroB2CRules(_microContext).GetB2COrder(tradeId);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }


        /// <summary>
        /// get first catalogues and items after create Trade Order
        /// response items in ALL caregoty
        /// </summary>
        /// <param name="traderId"></param>
        /// <returns></returns>
        [Route("items")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetInitCatalogAndItems(int tradeId, string domainKey, int page = 0)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroB2CRules(_microContext).GetInitCatalogAndItems(tradeId, domainKey, page);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// get catalogue and items
        /// response items in a caregoty
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [Route("category/items")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetCategoryItems(int categoryId, string domainKey, int page = 0)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroB2CRules(_microContext).GetCategoryItems(categoryId, domainKey, page);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Filter items
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("items")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SearchB2bOrderItems(B2COrderItemsRequestModel filter)
        {
            HeaderInformation(Request);
            try
            {

                var refModel = new MicroB2CRules(_microContext).SearchB2bOrderItems(filter);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("item/detail")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetItemDetail(int itemId, string domainKey)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroB2CRules(_microContext).GetItemDetail(itemId, domainKey);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("item/variant/price")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetItemPriceBySelectedVariantOption(B2CCategoryItemPriceRequestModel request)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroB2CRules(_microContext).GetVariantBySelectedOptions(request.SelectedOptions, request.CategoryItemId, request.Quantity);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        /// <summary>
        /// Business Update the Order Add/Edit/Remove item
        /// </summary>
        /// <param name="b2COrder"></param>
        /// <returns></returns>
        [Route("update")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2COrderMicroUpdate(B2COrder b2COrder)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroB2CRules(_microContext).B2COrderMicroUpdate(b2COrder, false);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Customer Update the Order Add/Edit/Remove item
        /// </summary>
        /// <param name="b2COrder"></param>
        /// <returns></returns>
        [Route("update/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2COrderMicroUpdateCustomer(B2COrder b2COrder)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroB2CRules(_microContext).B2COrderMicroUpdate(b2COrder, true);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("customer/address")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2COrderGetDeliveryAddresses()
        {
            HeaderInformation(Request);

            try
            {
                var refModel = new MicroB2CRules(_microContext).B2COrderGetDeliveryAddresses();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        /// <summary>
        /// Customer confirm B2C order
        /// </summary>
        /// <param name="b2cOrder"></param>
        /// <returns></returns>
        [Route("confirm")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2COrderConfirm(B2CCustomerAcceptedInfo b2cOrder)
        {
            HeaderInformation(Request);
            try
            {
                var confirm = new MicroB2CRules(_microContext).B2CCustomerOrderConfirm(b2cOrder);
                if (confirm.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = confirm.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("document")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2COrderDocument(int traderId, int pageIndex)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMediasRules(_microContext).GetActivityMedias(traderId, pageIndex);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// business add document in a Order
        /// </summary>
        /// <param name="microMedia"></param>
        /// <returns></returns>
        [Route("document")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2COrderDocumentAdd(MicroMediaUpload microMedia)
        {
            HeaderInformation(Request);
            try
            {
                UpdateMediaData(microMedia);

                var refModel = new MicroMediasRules(_microContext).AddActivityMedia(microMedia, false);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, refModel.msg, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        private void UpdateMediaData(MicroMediaUpload microMedia)
        {
            var discussion = new DiscussionsRules(_microContext.Context).GetB2CDiscussionOrderByDiscussionId(microMedia.ActivityId);

            microMedia.QbicleId = discussion.Qbicle.Id;
            microMedia.ActivityType = StreamType.Discussion;
            microMedia.TopicName = HelperClass.GeneralName;
            microMedia.FolderId = 0;
        }
        /// <summary>
        /// customer add document in a Order
        /// </summary>
        /// <param name="microMedia"></param>
        /// <returns></returns>
        [Route("document/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2COrderDocumentAddCustomer(MicroMediaUpload microMedia)
        {
            HeaderInformation(Request);
            try
            {
                UpdateMediaData(microMedia);

                var refModel = new MicroMediasRules(_microContext).AddActivityMedia(microMedia, true);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal }, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, refModel.msg, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        /// <summary>
        /// Get chat in b2c order
        /// </summary>
        /// <param name="traderId"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [Route("chat/get")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2COrderChat(int traderId, int pageIndex)
        {
            HeaderInformation(Request);
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, new MicroActivityCommentsRules(_microContext).GetActivityComments(traderId, pageIndex), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        #region B2C Order Payment
        /// <summary>
        /// Get all payment of the Order
        /// </summary>
        /// <param name="tradeId"></param>
        /// <returns></returns>
        [Route("payment/getall")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetB2CPayments(int tradeId)
        {
            HeaderInformation(Request);
            try
            {
                var payments = new MicroB2CRules(_microContext).B2COrderPaymentsGet(tradeId);
                return Request.CreateResponse(HttpStatusCode.OK, payments, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Get all payment of the Order
        /// </summary>
        /// <param name="tradeId"></param>
        /// <returns></returns>
        [Route("payment/get")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetB2CPayment(int tradeId, string paymentId)
        {
            HeaderInformation(Request);
            try
            {
                var payments = new MicroB2CRules(_microContext).B2COrderPaymentGet(paymentId, tradeId);
                return Request.CreateResponse(HttpStatusCode.OK, payments, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// add a payment into B2C order
        /// </summary>
        /// <param name="payment">Amount,Reference,PaymentMethod
        /// </param>
        /// <returns></returns>
        [Route("payment/add")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateB2CPayment(B2COderPayment payment)
        {
            HeaderInformation(Request);
            try
            {
                var resultPayment = "";
                var refModel = new MicroB2CRules(_microContext).B2COrderPaymentCreate(payment, ref resultPayment);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion

        #region B2C Order Voucher
        /// <summary>
        /// Get voucher customer
        /// <returns></returns>
        [Route("voucher/eligible")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2COrderGetVoucher(string domainKey)
        {
            HeaderInformation(Request);
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, new MicroActivityCommentsRules(_microContext).B2COrderGetVoucher(domainKey), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion


        [Route("starus")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetB2COrderstatus(int id)
        {
            HeaderInformation(Request);

            var refModel = new MicroB2CRules(_microContext).GetB2COrderstatus(id);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }
    }
}