using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/b2c/comms")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroB2CCommsController : BaseApiController
    {
        [Route("menu")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetB2CMenu(string domainKey)
        {
            HeaderInformation(Request);

            var showB2C = new MicroB2CCommsRules(_microContext).GetB2CMenu(domainKey);
            return Request.CreateResponse(HttpStatusCode.OK, new { ShowMenuB2C = showB2C }, Configuration.Formatters.JsonFormatter);

        }

        [Route("subnavigation")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetB2CSubnavigation(string domainKey)
        {
            HeaderInformation(Request);

            var subnavigation = new MicroB2CCommsRules(_microContext).GetB2CSubnavigation(domainKey);
            return Request.CreateResponse(HttpStatusCode.OK, subnavigation, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// list b2c community
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        [Route("communicate/list")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2CCommunicate(B2CSearchParameter search)
        {
            HeaderInformation(Request);

            var b2cComms = new MicroB2CCommsRules(_microContext).B2CCommunicate(search);
            return Request.CreateResponse(HttpStatusCode.OK, b2cComms, Configuration.Formatters.JsonFormatter);


        }

        [Route("communicate/profile")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CCommViewProfile(string profileKey)
        {

            HeaderInformation(Request);

            var refModel = new MicroUserProfilesRules(_microContext).GetUserProfile(profileKey.Decrypt());

            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);

        }

        [Route("communicate/unblock")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CCommUnblock(string key, string domainKey)
        {

            HeaderInformation(Request);

            var result = new MicroB2CCommsRules(_microContext).B2CCommUnblock(key, domainKey, Models.B2C_C2C.CommsStatus.Approved);
            if (result.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = result.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("communicate/block")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CCommBlock(string key, string domainKey)
        {
            HeaderInformation(Request);

            var result = new MicroB2CCommsRules(_microContext).B2CCommUnblock(key, domainKey, Models.B2C_C2C.CommsStatus.Blocked);
            if (result.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = result.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("communicate/remove")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CCommRemove(string key)
        {

            HeaderInformation(Request);

            var result = new MicroB2CCommsRules(_microContext).B2CCommRemove(key);
            if (result.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = result.msg }, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// Get info to init modal fro create new Order - business
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="key">Qbicle key</param>
        /// <returns></returns>
        [Route("communicate/order/init")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetInitB2CCommAddOrder(string domainKey, string key)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).GetInitB2CCommAddOrder(domainKey, key);

            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// Create new Order from B2C Manager
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [Route("communicate/order")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateB2CCommAddOrder(B2OrderParameter parameter)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).CreateB2CCommAddOrder(parameter, _microContext.UserId, false);

            return Request.CreateResponse(HttpStatusCode.OK, new { DiscussionId = refModel.Object2, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// GET init workgroup before submit
        /// if all setting then submit order
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [Route("communicate/order/complete")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetInitCompleteOrder(int tradeOrderId)
        {
            HeaderInformation(Request);
            var refModel = new MicroB2CCommsRules(_microContext).InitCompleteB2COrder(tradeOrderId);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// Business confirm B2C order
        /// </summary>
        /// <param name="b2cOrder"></param>
        /// <returns></returns>
        [Route("communicate/order/confirm")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2CBusinessOrderConfirm(B2CCustomerAcceptedInfo b2cOrder)
        {
            HeaderInformation(Request);
            try
            {
                new MicroB2CRules(_microContext).B2CBusinessOrderConfirm(b2cOrder);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        /// <summary>
        /// Confirm B2C order by management
        /// if all setting then submit order
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [Route("communicate/order/complete")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CompleteB2COrder(B2BSubmitProposal proposal)
        {
            HeaderInformation(Request);
            if (proposal.saleWGId > 0 && proposal.invoiceWGId > 0
                && proposal.paymentWGId > 0 && proposal.transferWGId > 0
                && proposal.paymentAccId > 0)
            {
                var complete = new MicroB2CCommsRules(_microContext).CompleteB2COrder(proposal);
                if (complete.result)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { complete.msg }, Configuration.Formatters.JsonFormatter);

            }

            return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = "Workgroup required" }, Configuration.Formatters.JsonFormatter);
        }
        //Promote a Catalogue modal

        /// <summary>
        /// promotecatalogue - creating: get location by domain for display on UI
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        [Route("communicate/promotecatalogue/location")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CPromoteCatalogueGetLocation(string domainKey)
        {

            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).B2CPromoteCatalogueGetLocation(domainKey);

            return Request.CreateResponse(HttpStatusCode.OK, new { Locations = refModel }, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// promotecatalogue - creating: get catalogues by Location
        /// </summary>
        /// <param name="locationKey"></param>
        /// <returns></returns>
        [Route("communicate/promotecatalogue/location/catalogues")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CPromoteGetCataloguesByLocation(string locationKey)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).B2CPromoteGetLocationCatalogues(locationKey);

            return Request.CreateResponse(HttpStatusCode.OK, new { Catalogues = refModel }, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// promotecatalogue - creating: get catalogues by domain
        /// </summary>
        /// <param name="locationKey"></param>
        /// <returns></returns>
        [Route("communicate/promotecatalogue/catalogues")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CPromoteGetCataloguesByDomain(string domainKey)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).B2CPromoteGetCatalogues(domainKey);

            return Request.CreateResponse(HttpStatusCode.OK, new { Catalogues = refModel }, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// Create new promotecatalogue
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>SignalR render UI</returns>
        [Route("communicate/promotecatalogue")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2CPromoteCatalogue(B2OrderParameter parameter)
        {
            HeaderInformation(Request);
            var refModel = new MicroCommunityRules(_microContext).B2CCreatePromoteCatalogue(parameter, _microContext.UserId);

            return Request.CreateResponse(HttpStatusCode.OK, new { DiscussionKey = refModel.Object2, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// Browse promote catalog by business
        /// </summary>
        /// <param name="key">discussion key get from Key in streamActivity rendered on UI</param>
        /// <returns></returns>
        [Route("communicate/promotecatalogue/brochure")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage B2CPromoteCatalogueBrowse(string key)
        {

            HeaderInformation(Request);
            var refModel = new MicroCommunityRules(_microContext).B2CPromoteCatalogueBrowse(key);

            return Request.CreateResponse(HttpStatusCode.OK, new { promote = refModel }, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// Browse promote catalog - get items from catalogues
        /// </summary>
        /// <param name="key">discussion key get from Key in streamActivity rendered on UI</param>
        /// <returns></returns>
        [Route("communicate/promotecatalogue/brochure/items")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2CPromoteCatalogueItems(B2CMenuItemsRequestModel request)
        {
            if (string.IsNullOrEmpty(request.DomainKey))
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Domain key is required!" }, Configuration.Formatters.JsonFormatter);
            HeaderInformation(Request);
            var refModel = new MicroCommunityRules(_microContext).B2CPromoteCatalogueItems(request);

            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// Create new B2C order from catalog brochure
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="catalogKey">menu key</param>
        /// <returns></returns>
        [Route("communicate/promotecatalogue/brochure/placeorder")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2CPromotePlaceOrder(B2OrderParameter parameter)
        {

            HeaderInformation(Request);
            var catalogId = int.Parse(parameter.MenuKey.Decrypt());
            var refModel = new MicroB2CRules(_microContext).CreateB2COrder(parameter.DomainKey, catalogId, false);
            if (refModel != null)
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }


        [Route("discussion")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicleDiscussion(DiscussionQbicleModel discussion)
        {
            HeaderInformation(Request);

            var refModel = new MicroDiscussionsRules(_microContext).CreateQbicleDiscussion(discussion, false);

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }


        [Route("discussion/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicleDiscussionCustomer(DiscussionQbicleModel discussion)
        {
            HeaderInformation(Request);

            var refModel = new MicroDiscussionsRules(_microContext).CreateQbicleDiscussion(discussion, true);

            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }


        [Route("media")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UploadMediaToB2C(MicroMediaUpload media)
        {
            HeaderInformation(Request);

            var generalFolder = new MediaFolderRules(_microContext.Context).GetMediaFolderByName(HelperClass.GeneralName, media.QbicleId, _microContext.UserId);
            media.FolderId = generalFolder.Id;
            media.TopicName = HelperClass.GeneralName;

            var refModel = new MicroMediasRules(_microContext).AddActivityMedia(media, false);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
        }


        [Route("media/customer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UploadMediaToB2CCustomer(MicroMediaUpload media)
        {
            HeaderInformation(Request);

            var generalFolder = new MediaFolderRules(_microContext.Context).GetMediaFolderByName(HelperClass.GeneralName, media.QbicleId, _microContext.UserId);
            media.FolderId = generalFolder.Id;
            media.TopicName = HelperClass.GeneralName;

            var refModel = new MicroMediasRules(_microContext).AddActivityMedia(media, true);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.actionVal, ResponseObject = refModel.Object }, Configuration.Formatters.JsonFormatter);
            return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
        }
    }
}