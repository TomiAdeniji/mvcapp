using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/community")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroCommunityController : BaseApiController
    {


        /// <summary>
        /// get Categories Items from catalog (pos_menu)
        /// </summary>
        /// <param name="catalogKey">key of catalog</param>
        /// <returns></returns>
        [Route("shop/catalog/items")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetCatalogItems(B2CMenuItemsRequestModel request)
        {
            HeaderInformation(Request);

            var refModel = new MicroB2CRules(_microContext).GetCatalogCategoriesItem(request);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }


        [Route("option")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetCommunityOption()
        {
            HeaderInformation(Request);
            var refModel = new MicroCommunityRules(_microContext).GetCommunityOption();
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
        }

        [Route("favourite")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage LikeCommunity(MicroCommunity like)
        {
            HeaderInformation(Request);


            var refModel = new MicroCommunityRules(_microContext).LikeUnLikeCommunity(like, true);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("unfavourite")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UnLikeCommunity(MicroCommunity like)
        {
            HeaderInformation(Request);


            var refModel = new MicroCommunityRules(_microContext).LikeUnLikeCommunity(like, false);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);


        }

        [Route("c2c/accept")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AcceptC2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);


            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineC2CCommunity(block, Models.B2C_C2C.CommsStatus.Approved);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("c2c/decline")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DeclineC2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);


            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineC2CCommunity(block, Models.B2C_C2C.CommsStatus.Blocked);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("c2c/cancel")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CancelC2CRequest(MicroCommunity block)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineC2CCommunity(block, Models.B2C_C2C.CommsStatus.Cancel);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }


        [Route("c2c/unblock")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UnbockC2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineC2CCommunity(block, Models.B2C_C2C.CommsStatus.Approved);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("c2c/block")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage BlockC2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineC2CCommunity(block, Models.B2C_C2C.CommsStatus.Blocked);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("c2c/remove/contact")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage RemoveC2CQbicleById(int qbicleId)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).RemoveC2CQbicleById(qbicleId);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }


        [Route("b2c/block")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage BlockB2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineB2CCommunity(block, Models.B2C_C2C.CommsStatus.Blocked);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("b2c/unblock")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UnblockB2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineB2CCommunity(block, Models.B2C_C2C.CommsStatus.Approved);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("b2c/decline")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DeclineB2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineB2CCommunity(block, Models.B2C_C2C.CommsStatus.Blocked);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("b2c/accept")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AcceptB2CCommunity(MicroCommunity block)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).AcceptDeclineB2CCommunity(block, Models.B2C_C2C.CommsStatus.Approved);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }
        [Route("b2c/remove/contact")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage RemoveB2CQbicleById(int qbicleId)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).RemoveB2CQbicleById(qbicleId);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("connect/option")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetCommunityConnectOption()
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).GetCommunityConnectOption();
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("connect/list")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MicroGetConnectes(FindPeopleRequest filter)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).MicroGetConnectes(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("connect/connect")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Connect2Community(MicroCommunityConnect connect)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).Connect2Community(connect);
            if (refModel.result)
                return Request.CreateResponse(HttpStatusCode.OK);
            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

        }

        [Route("shopping/categories")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage CommunityShoppingCategories()
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).CommunityShoppingCategories();
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("shopping")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CommunityShopping(ShoppingFilter filter)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).CommunityShopping(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("myorder/option")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMyOrderOption()
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).GetMyOrderOption();
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }


        [Route("myorder")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MyOrder(MyOrderFilter filter)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).GetMyOrders(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel.Object, Configuration.Formatters.JsonFormatter);

        }

        [Route("myorder/detail")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage MyOrderDetail(int id)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).GetMyOrderDetail(id);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("subnavigation")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetCommunitySubNavigation(int contactType = 0, string textFilter = "")
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).GetCommunitySubNavigation(contactType, textFilter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
        }

        [Route("list")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetCommunities(CommunityParameter filter)
        {
            HeaderInformation(Request);
            filter.ShownAll = true;
            var refModel = new MicroCommunityRules(_microContext).GetCommunities(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("list/favourite")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetCommunitiesFavourite(CommunityParameter filter)
        {
            HeaderInformation(Request);
            filter.ShownFavourite = true;
            var refModel = new MicroCommunityRules(_microContext).GetCommunities(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("list/request")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetCommunitiesRequest(CommunityParameter filter)
        {
            HeaderInformation(Request);
            filter.ShownRequest = true;
            var refModel = new MicroCommunityRules(_microContext).GetCommunities(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("list/sent")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetCommunitiesSent(CommunityParameter filter)
        {
            HeaderInformation(Request);
            filter.ShownSent = true;
            var refModel = new MicroCommunityRules(_microContext).GetCommunities(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("list/blocked")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetCommunitiesBlocked(CommunityParameter filter)
        {
            HeaderInformation(Request);
            filter.ShownBlocked = true;
            var refModel = new MicroCommunityRules(_microContext).GetCommunities(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

    }
}
