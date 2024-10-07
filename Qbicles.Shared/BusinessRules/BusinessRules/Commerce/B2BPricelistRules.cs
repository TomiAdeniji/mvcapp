using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Commerce
{
    public class B2BPricelistRules
    {
        ApplicationDbContext dbContext;
        public B2BPricelistRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public PriceList PricelistById(int priceId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, priceId);

                return dbContext.B2BPriceLists.Find(priceId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, priceId);
                return new PriceList();
            }
        }
        public bool CheckPricelistName(int priceId, string name, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, priceId, name);

                return dbContext.B2BPriceLists.Any(s => s.Id != priceId && s.Name.ToLower() == name.ToLower() && s.Location.Id == locationId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, priceId, name);
                return false;
            }
        }
        public ReturnJsonModel SavePricelist(B2bPriceListModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, model);


                var user = dbContext.QbicleUser.Find(userId);
                if (!string.IsNullOrEmpty(model.IconUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(model.IconUri);

                }

                var pricelist = dbContext.B2BPriceLists.Find(model.Id);
                if (pricelist != null)
                {
                    pricelist.Name = model.Name;
                    pricelist.Summary = model.Summary;
                    if (model.LocationId > 0)
                        pricelist.Location = dbContext.TraderLocations.Find(model.LocationId);
                    pricelist.LastUpdatedBy = user;
                    if (!string.IsNullOrEmpty(model.IconUri))
                        pricelist.Icon = model.IconUri;
                    pricelist.LastUpdatedDate = DateTime.UtcNow;
                    dbContext.Entry(pricelist).State = EntityState.Modified;
                }
                else
                {
                    pricelist = new PriceList();
                    pricelist.Location = dbContext.TraderLocations.Find(model.LocationId);
                    pricelist.Name = model.Name;
                    pricelist.Summary = model.Summary;
                    pricelist.CreatedBy = user;
                    pricelist.LastUpdatedBy = user;
                    pricelist.CreatedDate = DateTime.UtcNow;
                    pricelist.LastUpdatedDate = pricelist.CreatedDate;
                    if (!string.IsNullOrEmpty(model.IconUri))
                        pricelist.Icon = model.IconUri;
                    dbContext.B2BPriceLists.Add(pricelist);
                    dbContext.Entry(pricelist).State = EntityState.Added;
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
            }
            return returnJson;
        }
        public List<PriceList> SearchPricelist(string keyword, int currentLocationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, keyword, currentLocationId);
                var queryPricelist = dbContext.B2BPriceLists.Where(s => s.Location.Id == currentLocationId);
                if (!string.IsNullOrEmpty(keyword))
                    queryPricelist = queryPricelist.Where(s => s.Name.Contains(keyword) || s.Summary.Contains(keyword));
                return queryPricelist.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, keyword, currentLocationId);
                return new List<PriceList>();
            }
        }
        public List<PriceList> SearchPricelistByDomain(string keyword, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, keyword, domainId);
                var domain = dbContext.Domains.Find(domainId);
                var listLocationIDs = domain.TraderLocations.Select(s => s.Id).ToList();
                var queryPricelist = dbContext.B2BPriceLists.Where(s => listLocationIDs.Contains(s.Location.Id));
                if (!string.IsNullOrEmpty(keyword))
                    queryPricelist = queryPricelist.Where(s => s.Name.Contains(keyword) || s.Summary.Contains(keyword));
                return queryPricelist.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, keyword, domainId);
                return new List<PriceList>();
            }
        }
        public ReturnJsonModel DeletePricelist(int id)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                var priceList = dbContext.B2BPriceLists.Find(id);
                if (priceList != null)
                {
                    if (dbContext.B2BProviderPriceLists.Any(s => s.PriceList.Id == priceList.Id))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_DATA_IN_USED_CAN_NOT_DELETE", priceList.Name);
                        return returnJson;
                    }
                    dbContext.B2BPriceLists.Remove(priceList);
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
            return returnJson;
        }
        public ReturnJsonModel ClonePricelist(int cloneId, string cloneName, TraderLocation location, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, cloneId, cloneName, location);
                var clonepricelist = dbContext.B2BPriceLists.Find(cloneId);
                if (clonepricelist == null)
                    return returnJson;
                var user = dbContext.QbicleUser.Find(userId);
                var pricelist = new PriceList
                {
                    Location = location,
                    Name = cloneName,
                    Summary = clonepricelist.Summary,
                    CreatedBy = user,
                    LastUpdatedBy = user,
                    CreatedDate = DateTime.UtcNow
                };
                pricelist.LastUpdatedDate = pricelist.CreatedDate;
                pricelist.Icon = clonepricelist.Icon;
                #region Clone Charge Frameworks
                foreach (var item in clonepricelist.ChargeFrameworks)
                {
                    ChargeFramework framework = new ChargeFramework();
                    framework.CreatedBy = user;
                    framework.CreatedDate = pricelist.CreatedDate;
                    framework.LastUpdatedBy = user;
                    framework.LastUpdatedDate = pricelist.CreatedDate;
                    framework.Name = item.Name;
                    framework.DistanceTravelledFlatFee = item.DistanceTravelledFlatFee;
                    framework.DistanceTravelPerKm = item.DistanceTravelPerKm;
                    framework.TimeTakenFlatFee = item.TimeTakenFlatFee;
                    framework.TimeTakenPerSecond = item.TimeTakenPerSecond;
                    framework.ValueOfDeliveryFlatFee = item.ValueOfDeliveryFlatFee;
                    framework.ValueOfDeliveryPercentTotal = item.ValueOfDeliveryPercentTotal;
                    framework.VehicleType = item.VehicleType;
                    framework.PriceList = pricelist;
                    pricelist.ChargeFrameworks.Add(framework);
                }
                #endregion
                dbContext.B2BPriceLists.Add(pricelist);
                dbContext.Entry(pricelist).State = EntityState.Added;
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cloneId, cloneName, location);
            }
            return returnJson;
        }
    }
}
