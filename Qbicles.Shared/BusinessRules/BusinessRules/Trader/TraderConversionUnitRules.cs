using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Qbicles.BusinessRules.Helper;
using MySql.Data.MySqlClient;
using Qbicles.Models;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderConversionUnitRules
    {
        ApplicationDbContext _db;

        public TraderConversionUnitRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }
        public ProductUnit GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.ProductUnits.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new ProductUnit();
            }
        }
        public ReturnJsonModel DeleteByUnit(int unitId)
        {
            var result = new ReturnJsonModel();
            result.actionVal = 1;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, unitId);
                var unit = GetById(unitId);
                if (unit != null)
                {
                    DbContext.ProductUnits.Remove(unit);
                    DbContext.Entry(unit).State = EntityState.Deleted;
                    DbContext.SaveChanges();

                }
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, unitId);
                result.actionVal = 3;
                result.msg = "Delete failed. The unit is in use elsewhere.";
                return result;
            }
        }

        /// <summary>
        /// Get or Create product unit ( item.Units)
        /// </summary>
        /// <param name="createdBy"></param>
        /// <param name="unitName"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal ProductUnit GetOrCreateByName(ApplicationUser createdBy, string unitName, TraderItem item)
        {
            try
            {
                var unit = DbContext.ProductUnits.FirstOrDefault(e => e.Item.Id == item.Id && e.Name.ToLower() == unitName.ToLower());
                if (unit != null)
                    return unit;
                unit = new ProductUnit
                {
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsPrimary = true,
                    IsBase = true,
                    MeasurementType = MeasurementTypeEnum.Each,
                    Name = unitName,
                    Quantity = 1,
                    QuantityOfBaseunit = 1,
                    Item = item
                };
                DbContext.ProductUnits.Add(unit);
                DbContext.Entry(unit).State = EntityState.Added;
                DbContext.SaveChanges();
                return unit;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
                return null;
            }
        }

        public string  GetUnitsByItemId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                var units= DbContext.TraderItems.FirstOrDefault(e => e.Id == id)?.Units.ToList();

                var html = "";
                units.ForEach(u =>
                {
                    html += $"<option value=\"" + u.Id + "\">" + u.Name + "</option>";
                });
                return html;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return "";
            }
        }
    }
}
