using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.B2B;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules.Commerce
{
    public class B2BPartnershipRules
    {
        ApplicationDbContext dbContext;
        public B2BPartnershipRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public PurchaseSalesPartnership GetPartnershipById(int partnershipId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId);
                return dbContext.B2BPurchaseSalesPartnerships.Find(partnershipId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId);
                return null;
            }
        }
        public List<B2bLocationsModel> ProviderLocations(List<TraderLocation> locations)
        {
            List<B2bLocationsModel> b2BLocations = new List<B2bLocationsModel>();
            foreach (var item in locations)
            {
                var location = new B2bLocationsModel
                {
                    Id = item.Id,
                    Name = item.Name
                };
                var query = from d in dbContext.Drivers
                            join tc in dbContext.DriverBankmateAccounts on d.Id equals tc.Driver.Id
                            where d.EmploymentLocation.Id == item.Id
                            select d;
                var pricelistAny = dbContext.B2BPriceLists.Any(s => s.Location.Id == item.Id && s.ChargeFrameworks.Any());
                location.AllowSelect = pricelistAny && query.Any();
                location.Location = item;
                b2BLocations.Add(location);
            }
            return b2BLocations;
        }
    }
}
