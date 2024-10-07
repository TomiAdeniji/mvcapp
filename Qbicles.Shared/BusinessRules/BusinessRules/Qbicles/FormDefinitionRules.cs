using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Form;
using System;
using System.Data.Entity;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class FormDefinitionRules
    {
        ApplicationDbContext _db;
        public FormDefinitionRules()
        {
        }
        public FormDefinitionRules(ApplicationDbContext context)
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

        public int SaveFormDefinition(FormDefinition form, string currentId, QbicleDomain CurrentDomain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save form definition", currentId, null, form, currentId, CurrentDomain);

                var currentUser = new UserRules(DbContext).GetUser(currentId, 0);
                //form.CreatedBy = currentUser;
                //form.CreatedDate = DateTime.UtcNow;
                //form.Domain = CurrentDomain;
                if (form.Id == 0)
                {
                    DbContext.FormDefinition.Add(form);
                    DbContext.Entry(form).State = EntityState.Added;
                }
                else
                {
                    if (DbContext.Entry(form).State == EntityState.Detached)
                        DbContext.FormDefinition.Attach(form);
                    DbContext.Entry(form).State = EntityState.Modified;
                }
                DbContext.SaveChanges();
                return form.Id;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentId, form, currentId, CurrentDomain);
                return 0;
            }
        }

        //public List<FormDefinition> GetFormDefinitionsByDomainId(int domainId)
        //{
        //    try
        //    {
        //        return DbContext.FormDefinition.Where(x => x.Domain.Id == domainId).OrderByDescending(x => x.CreatedDate).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(),ex);
        //        return new List<FormDefinition>();
        //    }
        //}

        public bool DeleteFormDefinitionById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete form definition by id", null, null);

                var formdefinition = DbContext.FormDefinition.Find(id);
                DbContext.FormDefinition.Remove(formdefinition);
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public FormDefinition GetFormDefinitionById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get form definition by id", null, null, id);

                return DbContext.FormDefinition.Find(id) ?? new FormDefinition();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new FormDefinition();
            }
        }

        //public IQueryable<FormDefinition> GetFormDefinitionsByDomain(int domainId)
        //{
        //    return DbContext.FormDefinition.Where(d => d.Domain.Id == domainId);
        //}
    }
}
