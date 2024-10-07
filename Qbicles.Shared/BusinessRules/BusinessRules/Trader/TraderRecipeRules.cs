using Qbicles.BusinessRules.Model;
using System;
using System.Data.Entity;
using System.Reflection;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models.Trader;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderRecipeRules
    {
        ApplicationDbContext _db;

        public TraderRecipeRules(ApplicationDbContext context)
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

        public Recipe GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.Recipes.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new Recipe();
            }
        }

        public ReturnJsonModel ChangeActive(int recipeId, bool value)
        {
            var result = new ReturnJsonModel();
            result.actionVal = 1;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, recipeId, value);
                var rec = DbContext.Recipes.Find(recipeId);
                rec.IsActive = value;
                DbContext.Entry(rec).State = EntityState.Modified;
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, recipeId, value);
                result.msg = ex.Message;
                result.actionVal = 3;
            }
            return result;
        }
    }
}
