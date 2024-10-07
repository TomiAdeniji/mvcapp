using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Data.Entity;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class QbicleSetRules
    {
        private ApplicationDbContext _db;
        private readonly string _currentTimeZone = "";

        public QbicleSetRules()
        {
        }


        public QbicleSetRules(ApplicationDbContext context, string currentTimeZone = "")
        {
            _currentTimeZone = currentTimeZone;
            _db = context;
        }

        private ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            set => _db = value;
        }

        public QbicleSet GetSetsById(int setId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetSetsById", null, null, setId);

                return DbContext.Sets.Find(setId) ?? new QbicleSet();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, setId);
                return new QbicleSet();
            }
        }
        public QbicleSet CreateQbicleSet()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create qbicle set", null, null);

                var sets = new QbicleSet();
                DbContext.Sets.Add(sets);
                DbContext.Entry(sets).State = EntityState.Added;
                DbContext.SaveChanges();
                return sets;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }
    }
}