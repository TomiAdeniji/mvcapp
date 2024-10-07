using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader.Resources;
using System.Linq;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroProductSearchRules : MicroRulesBase
    {
        public MicroProductSearchRules(MicroContext microContext) : base(microContext)
        {
        }

        public object SearchStore(FindBusinessStoresRequest request)
        {
            request.currentUserId = CurrentUser.Id;
            return new B2CRules(dbContext).GetBusinessStores(request);
        }
        public object GetFeaturedBusinessStores(FindBusinessStoresRequest request)
        {
            request.currentUserId = CurrentUser.Id;
            request.IsAllPublicStoreShown = true;
            request.limitMyConnections = false;
            return new B2CRules(dbContext).GetFeaturedBusinessStores(request);
        }
        /// <summary>
        /// search all
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object SearchProduct(FindProductRequest request)
        {
            var totalRecords = 0;
            var dtData = new C2CRules(dbContext).FindProductsData(request.keyword, request.countryName, request.BrandIds, request.ProductTags, ref totalRecords, request.pageNumber, request.pageSize);
            var totalPage = totalRecords / (request.pageSize == 0 ? 1 : request.pageSize);
            return new
            {
                TotalPage = totalPage == 0 ? 1 : totalPage,
                TotalProducts = totalRecords,
                Products = dtData
            };
        }


        public object GetCommunityFeaturedProduct()
        {
            var featuedProducts= new C2CRules(dbContext).GetCustomizedCommunityFeaturedProductMicro();
            return featuedProducts;
        }
    }
}
