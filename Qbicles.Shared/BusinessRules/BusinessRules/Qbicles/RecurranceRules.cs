using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Data.Entity;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class RecurranceRules
    {
        private ApplicationDbContext _db;
        private readonly string _currentTimeZone = "";

        public RecurranceRules()
        {
        }


        public RecurranceRules(ApplicationDbContext context, string currentTimeZone = "")
        {
            _currentTimeZone = currentTimeZone;
            _db = context;
        }

        private ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            set => _db = value;
        }

        public QbicleRecurrance GetRecurranceById(int Id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get recurrance by id", null, null, Id);

                return DbContext.Recurrances.Find(Id) ?? new QbicleRecurrance();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, Id);
                return new QbicleRecurrance();
            }
        }
        public QbicleRecurrance CreateOrUpdateRecurrance(QbicleRecurrance recurrance)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create or update recurrance", null, null, recurrance);

                if (recurrance.Id>0)
                {                  
                    if (DbContext.Entry(recurrance).State == EntityState.Detached)
                        DbContext.Recurrances.Attach(recurrance);
                    DbContext.Entry(recurrance).State = EntityState.Modified;
                }
                else
                {
                    DbContext.Recurrances.Add(recurrance);
                    DbContext.Entry(recurrance).State = EntityState.Added;
                }
                
                DbContext.SaveChanges();
                return recurrance;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, recurrance);
                return null;
            }
        }
    }
}