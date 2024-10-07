using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models.MicroQbicleStream;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroCataloguesRules : MicroRulesBase
    {
        public MicroCataloguesRules(MicroContext microContext) : base(microContext)
        {
        }
        public object GetCountries()
        {
            var area = new C2CRules(dbContext).GetAreaOfOperations();
            var countries = new List<object>();
            var country = new
            {
                Value = "All",
                Key = "0"
            };
            countries.Add(country);
            countries.AddRange(area.Select(name => new
            {
                Value = name,
                Key = name
            }));
            

            return countries;
        }

        public object GetTopic(int qbicleId)
        {
            return new TopicRules(dbContext).GetTopicByQbicle(qbicleId).Select(d => new BaseModel { Id = d.Id, Name = d.Name });
        }
        public object GetInterests()
        {
            return new AdminListingRules(dbContext).GetAllBusinessCategories().Select(d => new BaseModel { Id = d.Id, Name = d.Name });
        }

        public object GetBrandsMaster()
        {
            return new C2CRules(dbContext).GetBrandsMaster().Select(e=> new {e.Id, e.Name});
        }

        public object GetProductTagTagify()
        {
            return new C2CRules(dbContext).GetProductTagTagify().Select(e => new { e.Id, e.Name });
        }

        public object GetBusinessCategoriesProfile()
        {
            return new C2CRules(dbContext).GetBusinessCategoriesProfile();
        }

    }
}
