using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class FormManagerRules
    {
        ApplicationDbContext _db;

        public FormManagerRules()
        {

        }
        public FormManagerRules(ApplicationDbContext context)
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

        public bool InsertManageTaskFormsPermissionByUserIdAndDomainId(string userId, int domainId) {

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "InsertManageTaskFormsPermissionByUserIdAndDomainId", null, null, userId, domainId);

                var isExistsFormManager = DbContext.FormManager.Any(x => x.User.Id == userId && x.Domain.Id == domainId);
                if (isExistsFormManager == true)
                {
                    var fManager = DbContext.FormManager.FirstOrDefault(x => x.User.Id == userId && x.Domain.Id == domainId);
                    fManager.Manage = true;

                    // fix for lazy loading
                    var domain =fManager.Domain;
                    var user = fManager.User;
                }
                else { 

                    var domain = new DomainRules(DbContext).GetDomainById(domainId);
                var user = new UserRules(DbContext).GetUser(userId, 0);
                    var formManager = new FormManager
                    {
                        Domain = domain,
                        User = user,
                        Manage = true
                };
                DbContext.FormManager.Add(formManager);
                DbContext.Entry(formManager).State = EntityState.Added;
                }
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return false;
            }
        }

        public bool InsertQueryOrReportPermissionByUserIdAndDomainId(string userId, int domainId)
        {

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "InsertQueryOrReportPermissionByUserIdAndDomainId", null, null, userId, domainId);

                var isExistsFormManager = DbContext.FormManager.Any(x => x.User.Id == userId && x.Domain.Id == domainId);
                if (isExistsFormManager == true)
                {
                    var fManager = DbContext.FormManager.FirstOrDefault(x => x.User.Id == userId && x.Domain.Id == domainId);
                    fManager.QueryOrReport = true;

                    // fix for lazy loading
                    var domain = fManager.Domain;
                    var user = fManager.User;

                }
                else
                {

                    var domain = new DomainRules(DbContext).GetDomainById(domainId);
                    var user = new UserRules(DbContext).GetUser(userId, 0);
                    var formManager = new FormManager
                    {
                        Domain = domain,
                        User = user,
                        QueryOrReport = true
                    };
                    DbContext.FormManager.Add(formManager);
                    DbContext.Entry(formManager).State = EntityState.Added;
                }
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return false;
            }

        }

 

        public bool RemoveManageTaskFormsPermissionByUserIdAndDomainId(string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "RemoveManageTaskFormsPermissionByUserIdAndDomainId", null, null, userId, domainId);

                var formManager = DbContext.FormManager.FirstOrDefault(x => x.User.Id == userId && x.Domain.Id == domainId);
                formManager.Manage = false;

                // fix for lazy loading
                var domain = formManager.Domain;
                var user = formManager.User;

                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return false;
            }
        }

        public bool RemoveQueryOrReportPermissionByUserIdAndDomainId(string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "RemoveManageTaskFormsPermissionByUserIdAndDomainId", null, null, userId, domainId);

                var formManager = DbContext.FormManager.FirstOrDefault(x => x.User.Id == userId && x.Domain.Id == domainId);
                formManager.QueryOrReport = false;
                // fix for lazy loading
                var domain = formManager.Domain;
                var user = formManager.User;

                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return false;
            }
        }
        
        public bool InsertOrDeleteManageTaskPermissionFormsByChecked(bool isChecked, string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "InsertOrDeleteManageTaskPermissionFormsByChecked", null, null, isChecked, userId, domainId);

                if (isChecked)
                {
                    return InsertManageTaskFormsPermissionByUserIdAndDomainId(userId, domainId);
                }
                else {
                    return RemoveManageTaskFormsPermissionByUserIdAndDomainId(userId, domainId);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, isChecked, userId, domainId);
                return false;
            }
        }

        public bool InsertOrDeleteQueryOrReportPermissionByChecked(bool isChecked, string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "InsertOrDeleteQueryOrReportPermissionByChecked", null, null, isChecked, userId, domainId);

                if (isChecked)
                {
                    return InsertQueryOrReportPermissionByUserIdAndDomainId(userId, domainId);
                }
                else
                {
                    return RemoveQueryOrReportPermissionByUserIdAndDomainId(userId, domainId);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, isChecked, userId, domainId);
                return false;
            }
        }


        
        public List<FormManager> GetFormManagerHasPermissionManageTaskFormsByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "InsertOrDeleteQueryOrReportPermissionByChecked", null, null, domainId);

                return DbContext.FormManager.Where(x => x.Domain.Id == domainId && x.Manage == true).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<FormManager>();
            }   
        }

        public List<FormManager> GetFormManagerHasPermissionQueryOrReportByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetFormManagerHasPermissionQueryOrReportByDomainId", null, null, domainId);

                return DbContext.FormManager.Where(x => x.Domain.Id == domainId && x.QueryOrReport == true).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<FormManager>();
            }

        }

        public bool IsManageTaskForm(string userId, QbicleDomain CurrentDomain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "IsManageTaskForm", null, null, CurrentDomain);

                var user = new UserRules(DbContext).GetUser(userId, 0);
                return DbContext.FormManager.Any(x => x.User.Id == userId && x.Domain.Id == CurrentDomain.Id && x.Manage == true) ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, CurrentDomain);
                return false;
            }
        }

        public bool isQueryOrReport(string userId, QbicleDomain CurrentDomain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check query or report", null, null, CurrentDomain);

                var user = new UserRules(DbContext).GetUser(userId, 0);
                return DbContext.FormManager.Any(x => x.User.Id == userId && x.Domain.Id == CurrentDomain.Id && x.QueryOrReport == true) ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, CurrentDomain);
                return false;
            }
        }
        
    }
}
