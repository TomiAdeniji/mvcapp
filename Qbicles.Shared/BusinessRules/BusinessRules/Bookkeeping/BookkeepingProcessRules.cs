using System;
using System.Reflection;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Bookkeeping;

namespace Qbicles.BusinessRules
{
    public class BookkeepingProcessRules
    {
        ApplicationDbContext dbContext;

        public BookkeepingProcessRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public BookkeepingProcess GetById(int id)
        {
            try
            {
                if (Helper.ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BookkeepingProcess GetById", null, null, id);

                return dbContext.BookkeepingProcesses.Find(id);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
    }
}
