using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Bookkeeping;
using System;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class BKTransactionRules
    {
        ApplicationDbContext dbContext;
       
        public BKTransactionRules(ApplicationDbContext context)
        {
            dbContext = context;
        }       

        public BKTransaction GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BKTransaction GetById", null, null, id);


                var bkTransaction = dbContext.BKTransactions.Find(id);
                return bkTransaction;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

    }
}
