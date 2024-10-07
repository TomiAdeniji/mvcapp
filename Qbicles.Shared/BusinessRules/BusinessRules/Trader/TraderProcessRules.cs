using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderProcessRules
    {
        ApplicationDbContext _db;

        public TraderProcessRules(ApplicationDbContext context)
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

        public List<TraderProcess> GetAll()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                return DbContext.TraderProcesses.ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return  new List<TraderProcess>();
            }
            
        }
        public TraderProcess GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderProcesses.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return  new TraderProcess();
            }

        }
        public List<BaseModel> GetAllBase()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                return DbContext.TraderProcesses.OrderBy(n => n.Name).Select(l => new BaseModel { Id = l.Id, Name = l.Name }).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<BaseModel>();
            }

        }
    }
}
