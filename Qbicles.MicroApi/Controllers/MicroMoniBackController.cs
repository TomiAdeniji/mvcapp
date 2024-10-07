using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/moniback")]
    public class MicroMoniBackController : BaseApiController
    {
        [Route("mystores")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMyStores(int page, string filter)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).GetMyStores(page, filter);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("mystoreinfo")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMyStoreInfo(string key)
        {
            HeaderInformation(Request);

            try
            {
                var refModel = new MicroMoniBackRules(_microContext).GetMyStoreInfo(key);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("storecreditbalanceinfo")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetStoreCreditBalanceInfo(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).GetStoreCreditBalanceInfo(key);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("storecreditpointinfo")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetStoreCreditPointInfo(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).GetStoreCreditPointInfo(key);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("balance2credit")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Balance2Credit(StoreCreditExchangeModel balance)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).Balance2Credit(balance);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("point2credit")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Point2Credit(StoreCreditExchangeModel point)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).Point2Credit(point);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("convertbalance2credit")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ConvertBalance2Credit(StoreCreditExchangeModel balance)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).ConvertBalance2Credit(balance);

                if (!refModel.result)
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, refModel.msg, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("convertpoint2credit")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ConvertPoint2Credit(StoreCreditExchangeModel point)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).ConvertPoint2Credit(point);

                if (!refModel.result)
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, refModel.msg, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("securepin")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetStoreCreditSecurePIN()
        {
            HeaderInformation(Request);
            try
            {
                var pin = new MicroMoniBackRules(_microContext).GetStoreCreditSecurePIN();

                return Request.CreateResponse(HttpStatusCode.OK, new { pin }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("securepin")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GenerateStoreCreditSecurePIN()
        {
            HeaderInformation(Request);
            try
            {
                var pin = new MicroMoniBackRules(_microContext).GenerateStoreCreditSecurePIN();

                return Request.CreateResponse(HttpStatusCode.OK, new { PIN = pin.Object }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("myvouchers")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetVouchers(VoucherByUserAndShopModel filterModel)
        {
            HeaderInformation(Request);
            try
            {
                var vouchers = new MicroMoniBackRules(_microContext).GetVouchers(filterModel);

                return Request.CreateResponse(HttpStatusCode.OK, vouchers, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        [Route("deals")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetDeals(PromotionPublishFilterModel filterModel)
        {
            HeaderInformation(Request);

            try
            {
                var vouchers = new MicroMoniBackRules(_microContext).GetDeals(filterModel);
                return Request.CreateResponse(HttpStatusCode.OK, vouchers, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/claim")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage VoucherClaim(VoucherClaimParameter voucher)
        {
            HeaderInformation(Request);
            try
            {
                var claimed = new MicroMoniBackRules(_microContext).VoucherClaim(voucher);

                if (!claimed.result)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = claimed.msg }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK, claimed.Object, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/like")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage VoucherLike(string promotionKey)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).VoucherLike(promotionKey, true);

                if (!refModel.result)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/unlike")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage VoucherUnLike(string promotionKey)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).VoucherLike(promotionKey, false);

                if (!refModel.result)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/bookmark")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage BookmarkVoucher(string promotionKey)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).VoucherBookmark(promotionKey, true);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/unbookmark")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage UnBookmarkVoucher(string promotionKey)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).VoucherBookmark(promotionKey, false);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/share/contacts")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetContactsForHLShare()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroMoniBackRules(_microContext).GetContactsForPromotionShare();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/share/contacts")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> ShareHighlight2Contact(HighlightPromotionShareParameter share)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = await new MicroMoniBackRules(_microContext).SharePromotionContact(share);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher/share/email")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> ShareHighlight2Email(HighlightPromotionShareParameter share)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = await new MicroMoniBackRules(_microContext).SharePromotionEmail(share);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("voucher")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetVoucherInfo(int id)
        {
            HeaderInformation(Request);

            try
            {
                var refModel = new MicroMoniBackRules(_microContext).GetVoucherInfo(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("addtomystores")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage AddToMyStores(string businessKey)
        {
            HeaderInformation(Request);
            try
            {
                var addToMyStore = new MicroMoniBackRules(_microContext).AddToMyStores(businessKey);

                if (!addToMyStore.result)
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = addToMyStore.msg }, Configuration.Formatters.JsonFormatter);

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