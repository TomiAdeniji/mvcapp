using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.Models;
using Qbicles.Models.Highlight;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/highlight")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroHighlightController : BaseApiController
    {
        [Route("all")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetHighlights(HighlightParameter highlight)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).GetHighlight(highlight);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, (HighlightPostStreamModel)refModel.Object, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("domain/followings")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDomainFollowings()
        {
            HeaderInformation(Request);
            try
            {
                var followings = new MicroHighlightRules(_microContext).GetDomainFollowings();
                return Request.CreateResponse(HttpStatusCode.OK, followings, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("domain/recommends")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDomainRecommends()
        {
            HeaderInformation(Request);
            try
            {
                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                var followings = new MicroHighlightRules(_microContext).GetDomainRecommends();
                return Request.CreateResponse(HttpStatusCode.OK, followings, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("filteroptions")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetfFilteroOptions()
        {
            HeaderInformation(Request);
            try
            {
                var followings = new MicroHighlightRules(_microContext).GetFilterOption();
                return Request.CreateResponse(HttpStatusCode.OK, followings, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("interested")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Interested(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroInterestedHighlight(id, 1);
                //EventNotify = Notification.NotificationEventEnum.ListingInterested ??
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

        [Route("uninterested")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage NotInterested(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroInterestedHighlight(id, 2);

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

        [Route("bookmark")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage BookmarkHighlight(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroBookmarkHighlight(id, 1);
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

        [Route("unbookmark")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage UnBookmarkHighlight(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroBookmarkHighlight(id, 2);

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

        [Route("like")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage LikeHighlight(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroLikeHighlight(id, 1);
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

        [Route("unlike")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage UnLikeHighlight(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroLikeHighlight(id, 2);

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

        [Route("follow")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage FollowStatusHighlight(string domainId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroDomainFollowStatusHighlight(domainId.Decrypt2Int(), 1);
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

        [Route("unfollow")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage UnFollowStatus(string domainId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroDomainFollowStatusHighlight(domainId.Decrypt2Int(), 2);

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

        [Route("readarticle")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage ReadArticle(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroHighlightReadArticle(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, (HighlightArticle)refModel.Object, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("openproperty")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage OpenProperty(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).MicroHighlightPropertyInfo(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, (HighlightPropertyInfo)refModel.Object, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("share/contacts")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetContactsForHLShare()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHighlightRules(_microContext).GetContactsForHLShare();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("share/contacts")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> ShareHighlight2Contact(HighlightPromotionShareParameter share)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = await (new MicroHighlightRules(_microContext).ShareHL2Contacts(share));
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

        [Route("share/email")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> ShareHighlight2Email(HighlightPromotionShareParameter share)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = await (new MicroHighlightRules(_microContext).ShareHL2Email(share));
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
    }
}