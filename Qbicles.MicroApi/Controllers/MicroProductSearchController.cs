using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/b2c/search")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroProductSearchController : BaseApiController
    {
        [Route("store/option/categories")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetCountriesBusinessCategories()
        {
            HeaderInformation(Request);
            var refModel = new MicroCataloguesRules(_microContext).GetBusinessCategoriesProfile();
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// All store
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("store")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SearchStore(FindBusinessStoresRequest request)
        {
            HeaderInformation(Request);
            var stores = new MicroProductSearchRules(_microContext).SearchStore(request);            
            return Request.CreateResponse(HttpStatusCode.OK, stores, Configuration.Formatters.JsonFormatter);

        }

        [Route("store/featured")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SearchStoreFeatured(FindBusinessStoresRequest request)
        {
            HeaderInformation(Request);
            var stores = new MicroProductSearchRules(_microContext).GetFeaturedBusinessStores(request);
            return Request.CreateResponse(HttpStatusCode.OK, stores, Configuration.Formatters.JsonFormatter);

        }

        [Route("product/option/brands")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetBrandsMaster()
        {
            HeaderInformation(Request);
            var refModel = new MicroCataloguesRules(_microContext).GetBrandsMaster();
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
        }

        [Route("product/option/tags")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetProductTagTagify()
        {
            HeaderInformation(Request);
            //Filter by keyword(s)
            var refModel = new MicroCataloguesRules(_microContext).GetProductTagTagify(); ;
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
        }


        [Route("product")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SearchProduct(FindProductRequest request)
        {
            HeaderInformation(Request);
            var stores = new MicroProductSearchRules(_microContext).SearchProduct(request);
            return Request.CreateResponse(HttpStatusCode.OK, stores, Configuration.Formatters.JsonFormatter);

        }

        [Route("product/featured")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage GetCommunityFeaturedProduct()
        {
            HeaderInformation(Request);
            var featuredProduct = new MicroProductSearchRules(_microContext).GetCommunityFeaturedProduct();
            return Request.CreateResponse(HttpStatusCode.OK, featuredProduct, Configuration.Formatters.JsonFormatter);

        }
    }
}