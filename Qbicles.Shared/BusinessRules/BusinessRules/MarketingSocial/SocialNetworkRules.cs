using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class SocialNetworkRules
    {
        private ApplicationDbContext _db;

        public SocialNetworkRules()
        {
        }

        public SocialNetworkRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }
        public List<SocialNetworkAccount> GetSocialNetworkAccountsByNWType(int[] networkTypes, int currentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get social network accounts by NWType", null, null, networkTypes, currentDomainId);

                return DbContext.SocialNetworkAccounts.Where(s => s.Settings.Domain.Id==currentDomainId && networkTypes.Contains(s.Type.Id) && !s.IsDisabled).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, networkTypes, currentDomainId);
                return null;
            }
        }
        public List<NetworkType> GetNetworkTypes()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get network types", null, null);

                return DbContext.NetworkTypes.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<NetworkType>();
            }
        }
    }
}
