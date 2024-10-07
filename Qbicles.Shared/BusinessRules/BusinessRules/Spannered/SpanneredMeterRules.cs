using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Spannered;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.BusinessRules.Spannered
{
    public class SpanneredMeterRules
    {
        ApplicationDbContext dbContext;
        public SpanneredMeterRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public List<Meter> GetMetersByAssetId(int assetId, string name = "")
        {
            try
            {
                return dbContext.SpanneredMeters.Where(s => s.Asset.Id == assetId && (name == "" || s.Name.Trim().ToLower().Contains(name.Trim().ToLower()))).OrderByDescending(s => s.Id).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<Meter>();
            }
        }

        public Meter GetMeterById(int id)
        {
            try
            {
                return dbContext.SpanneredMeters.FirstOrDefault(m => m.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new Meter();
            }
        }

        public ReturnJsonModel SaveMeter(Meter meter, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbMeter = dbContext.SpanneredMeters.Find(meter.Id);
                if (dbMeter != null)
                {
                    dbMeter.Name = meter.Name;
                    dbMeter.Unit = meter.Unit;
                    dbMeter.ValueOfUnit = meter.ValueOfUnit;
                    dbMeter.Description = meter.Description;
                    dbMeter.LastUpdatedBy = meter.CreatedBy;
                    dbMeter.LastUpdateDate = DateTime.UtcNow;
                    if (dbContext.Entry(dbMeter).State == EntityState.Detached)
                        dbContext.SpanneredMeters.Attach(dbMeter);
                    dbContext.Entry(dbMeter).State = EntityState.Modified;
                }
                else
                {
                    var user = dbContext.QbicleUser.Find(userId);
                    dbMeter = new Meter
                    {
                        Name = meter.Name,
                        Unit = meter.Unit,
                        ValueOfUnit = 0,
                        Description = meter.Description,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = user,
                        LastUpdatedBy = user,
                        LastUpdateDate = DateTime.UtcNow,
                        Asset = meter.Asset
                    };
                    dbContext.SpanneredMeters.Add(dbMeter);
                    dbContext.Entry(dbMeter).State = EntityState.Added;
                }
                
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }

        public ReturnJsonModel UpdateValueOfUnit(int meterId, decimal valueOfUnit, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var user = dbContext.QbicleUser.Find(userId);
                var dbMeter = dbContext.SpanneredMeters.Find(meterId);
                dbMeter.ValueOfUnit = valueOfUnit;
                dbMeter.LastUpdatedBy = user;
                dbMeter.LastUpdateDate = DateTime.UtcNow;
                if (dbContext.Entry(dbMeter).State == EntityState.Detached)
                    dbContext.SpanneredMeters.Attach(dbMeter);
                dbContext.Entry(dbMeter).State = EntityState.Modified;

                MeterLog log = new MeterLog();
                log.Meter = dbMeter;
                log.CreatedBy = user;
                log.CreatedDate = DateTime.UtcNow;
                log.ValueOfUnit = dbMeter.ValueOfUnit;
                dbContext.SpanneredMeterLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;

                refModel.result = dbContext.SaveChanges() > 0 ? true : false;


            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
    }
}
