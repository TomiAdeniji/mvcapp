using System.Collections.Generic;
using System.Linq;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Community;

namespace Qbicles.BusinessRules.Community
{
    public class LocationRules
    {
        private ApplicationDbContext _db;

        public LocationRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }

        public List<Location> GetLocations(int domainProfileId)
        {
            return DbContext.Locations.Where(d => d.DomainProfile.Id == domainProfileId).ToList();
        }
    }
}