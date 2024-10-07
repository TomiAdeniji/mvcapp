using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.B2B;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Commerce
{
    public class B2BChargeFrameworkRules
    {
        ApplicationDbContext dbContext;
        public B2BChargeFrameworkRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ChargeFramework ChargeFrameworkById(int chargeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, chargeId);

                var chargeframework= dbContext.B2BChargeFrameworks.Find(chargeId);

                return chargeframework != null ? chargeframework : new ChargeFramework();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex,null, chargeId);
                return new ChargeFramework();
            }
        }
        public bool CheckPricelistName(int chargeId, string name,int pricelistId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, chargeId, name);

                return dbContext.B2BChargeFrameworks.Any(s=>s.Id!= chargeId && s.Name.ToLower()== name.ToLower()&&s.PriceList.Id== pricelistId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, chargeId, name);
                return false;
            }
        }
        public ReturnJsonModel SaveChargeFramework(ChargeFramework model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, model);
                var chargeFramework = dbContext.B2BChargeFrameworks.Find(model.Id);
                if(chargeFramework != null)
                {
                    chargeFramework.Name = model.Name;
                    chargeFramework.DistanceTravelledFlatFee = model.DistanceTravelledFlatFee;
                    chargeFramework.DistanceTravelPerKm = model.DistanceTravelPerKm;
                    chargeFramework.TimeTakenFlatFee = model.TimeTakenFlatFee;
                    chargeFramework.TimeTakenPerSecond = model.TimeTakenPerSecond;
                    chargeFramework.ValueOfDeliveryFlatFee = model.ValueOfDeliveryFlatFee;
                    chargeFramework.ValueOfDeliveryPercentTotal = model.ValueOfDeliveryPercentTotal;
                    chargeFramework.VehicleType = model.VehicleType;
                    chargeFramework.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    chargeFramework.LastUpdatedDate = DateTime.UtcNow;
                    dbContext.Entry(chargeFramework).State = EntityState.Modified;
                }else
                {
                    var createdBy = dbContext.QbicleUser.Find(userId);
                    chargeFramework = new ChargeFramework
                    {
                        PriceList = dbContext.B2BPriceLists.Find(model.PriceList.Id),
                        Name = model.Name,
                        DistanceTravelledFlatFee = model.DistanceTravelledFlatFee,
                        DistanceTravelPerKm = model.DistanceTravelPerKm,
                        TimeTakenFlatFee = model.TimeTakenFlatFee,
                        TimeTakenPerSecond = model.TimeTakenPerSecond,
                        ValueOfDeliveryFlatFee = model.ValueOfDeliveryFlatFee,
                        ValueOfDeliveryPercentTotal = model.ValueOfDeliveryPercentTotal,
                        VehicleType = model.VehicleType,
                        CreatedBy = createdBy,
                        LastUpdatedBy = createdBy,
                        CreatedDate = DateTime.UtcNow
                    };
                    chargeFramework.LastUpdatedDate = chargeFramework.CreatedDate;
                    dbContext.B2BChargeFrameworks.Add(chargeFramework);
                    dbContext.Entry(chargeFramework).State = EntityState.Added;
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
            }
            return returnJson;
        }
        public List<ChargeFramework> SearchChargeFramework(int pricelistId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pricelistId);
                var queryChargeFrameworks = dbContext.B2BChargeFrameworks.Where(s=>s.PriceList.Id== pricelistId);
                return queryChargeFrameworks.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex,null, pricelistId);
                return new List<ChargeFramework>();
            }
        }
        public ReturnJsonModel DeleteChargeFramework(int id)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                var chargeFramework = dbContext.B2BChargeFrameworks.Find(id);
                if (chargeFramework != null)
                {
                    dbContext.B2BChargeFrameworks.Remove(chargeFramework);
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return returnJson;
        }
    }
}
