using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class AppRightRules
    {
        ApplicationDbContext dbContext;
        public AppRightRules()
        {

        }
        public AppRightRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return dbContext ?? new ApplicationDbContext();
            }
            private set
            {
                dbContext = value;
            }
        }

        public List<AppRight> GetAllRights()
        {
            try
            {
                return dbContext.AppRight.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<AppRight>();
            }
        }

        public List<string> UserRoleRights(string userId, int currentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user role rights", null, null, userId, currentDomainId);

                if (string.IsNullOrEmpty(userId))
                    return new List<string>();
                var currentUser = new UserRules(DbContext).GetUser(userId, 0);
                var rights = new List<string>();
                var roleRights = new List<RoleRightAppXref>();

                List<DomainRole> domainRoles;
                if (currentDomainId > 0)
                    domainRoles = currentUser.DomainRoles.Where(d => d.Domain.Id == currentDomainId).ToList();
                else
                    domainRoles = currentUser.DomainRoles;

                foreach (var role in domainRoles)
                {
                    roleRights.AddRange(DbContext.RoleRightAppXref.Where(d => d.Role.Id == role.Id).ToList());
                }
                if (roleRights.Count > 0)
                    rights.AddRange(roleRights.Select(r => r.Right).Select(n => n.Name));
                return rights.Distinct().ToList();
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, currentDomainId);
                return new List<string>();
            }
            
        }

        public bool CheckTaskFormPermission(string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check task form permission", null, null, userId, userId, domainId);

                var domain = dbContext.Domains.Find(domainId);
                var isManageTaskForm =
                    new FormManagerRules(dbContext).IsManageTaskForm(userId, domain);
                var aList = new AccountRules(dbContext)
                    .ListAdministrator(domain, userId)
                    .GroupBy(x => new { x.Id, x.Name, x.Avatar }).Select(y => new AdministratorViewModal
                    {
                        Id = y.Key.Id,
                        Name = y.Key.Name,
                        Levels = y.Select(z => new KeyValuePair<string, int>(z.Level,
                                z.Level == AdministratorViewModal.AccountOwner
                                    ? 1
                                    : (z.Level == AdministratorViewModal.AccountAdministrators
                                        ? 2
                                        : (z.Level == AdministratorViewModal.DomainAdministrator ? 3 : 999))))
                            .Distinct()
                            .ToList()
                    }).FirstOrDefault();
                var permissions = aList.Levels.ToDictionary(k => k.Key);

                if (permissions.ContainsKey(AdministratorViewModal.DomainAdministrator) || isManageTaskForm)
                    return true;
                return false;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, userId, domainId);
                return false;
            }
            
        }

        /// <summary>
        /// CleanBooks Security
        /// Access to Accounts and Tasks
        /// When the list of Roles to be displayed at (Grant access to groups) , on the Account/Task Add/Edit dialog, the application will find all Roles in the Domain that have the right 'rightNameFill'.
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="user"></param>
        /// <param name="rightNameFill"></param>
        /// <returns></returns>
        public List<DomainRole> GrantAccessToGroups(int domainId, string rightNameFill)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Grant access to group", null, null, domainId, rightNameFill);

                var roles = new List<DomainRole>();
                var domainRoles = dbContext.DomainRole.Where(d => d.Domain.Id == domainId).ToList();
                foreach (var role in domainRoles)
                {
                    var roleRights = dbContext.RoleRightAppXref.Where(d => d.Role.Id == role.Id).ToList();
                    if (roleRights.Any(r => r.Right.Name == rightNameFill))
                        roles.Add(role);
                }

                return roles;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, rightNameFill);
                return new List<DomainRole>();
            }
            
        }

        public List<string> SystemRolesQBicles(string userId)
        {            
            var userManager = new UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.CoreCompat.UserStore<ApplicationUser>(dbContext));
            var roles = userManager.GetRoles(userId);
            return roles.ToList();
        }

    }
}


