using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules
{
    public class DomainRolesRules
    {
        ApplicationDbContext dbContext;
        public DomainRolesRules()
        {

        }
        public DomainRolesRules(ApplicationDbContext context)
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

        public ReturnJsonModel RemoveUserFromRoles(int rolesId, string userId, int domainId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove user from roles", null, null, rolesId, userId, domainId);

                var role = dbContext.DomainRole.FirstOrDefault(x => x.Id == rolesId);
                var user = role.Users.FirstOrDefault(x => x.Id == userId);
                role.Users.Remove(user);

                dbContext.SaveChanges();
                refModel.result = true;
                refModel.Object = new { user.UserName };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, rolesId, userId, domainId);
                refModel.msg = ex.Message;
            }
            return refModel;

        }

        public int CountAllUserByAllRoleOfDomain(string userId, QbicleDomain currentDomain)
        {
            var rs = 0;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count all user by all role of domain",
                        null, null, userId, currentDomain);

                var roles = dbContext.DomainRole.Where(x => x.Domain.Id == currentDomain.Id).ToList();
                foreach (var role in roles)
                {
                    rs = rs + role.Users.Count();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, currentDomain);
            }
            return rs;
        }

        public ReturnJsonModel AddRoleDomainByName(string roleDomainName, string userId, QbicleDomain currentDomain)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add role domain by name", userId, null, roleDomainName, currentDomain);

                if (dbContext.DomainRole.Any(x => x.Domain.Id == currentDomain.Id && x.Name == roleDomainName) == false)
                {
                    var currentUser = dbContext.QbicleUser.Find(userId);
                    var domainRole = new DomainRole()
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Domain = currentDomain,
                        Name = roleDomainName
                    };
                    domainRole.Users.Add(currentUser);
                    dbContext.DomainRole.Add(domainRole);
                    dbContext.Entry(domainRole).State = EntityState.Added;
                    dbContext.SaveChanges();
                    refModel.result = true;
                    refModel.Object = new { domainRole.Id, domainRole.Name };
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_39");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, roleDomainName, currentDomain);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public List<DomainRole> GetDomainRolesDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get domain roles by domain id", null, null, domainId);

                return dbContext.DomainRole.Where(x => x.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<DomainRole>();
            }
        }

        public ReturnJsonModel AddUserToRoles(List<string> listUserId, int roleId, string userId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add user to roles", null, null, listUserId, roleId, userId);

                var role = dbContext.DomainRole.Find(roleId);

                foreach (var uId in listUserId)
                {
                    if (role.Users.Any(x => x.Id == uId) == false)
                    {
                        var user = new UserRules(dbContext).GetUser(uId, 0);
                        role.Users.Add(user);
                    }

                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, listUserId, roleId, userId);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public DataTablesResponse GetDomainUsersByRoleId(IDataTablesRequest requestModel, int domainId, int roleId, string currentUserId)
        {
            try
            {
                int totalcount = 0;
                #region Filters
                var role = dbContext.DomainRole.Find(roleId);
                var domain = dbContext.Domains.Find(domainId);
                var query = from rUser in role.Users
                            join dUser in domain.Users on rUser.Id equals dUser.Id
                            select dUser;
                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    query = query.Where(s => (s.Forename + " " + s.Surname).Contains(requestModel.Search.Value));
                }
                totalcount = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "FullName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    FullName = HelperClass.GetFullNameOfUser(q),
                    AvatarUri =  q.ProfilePic.ToUriString(),
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsCurrentUser = q.Id == currentUserId
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public List<ApplicationUser> GetUsersNotExistInRole(int domainId, int roleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "get users by role id", null, null, domainId, roleId);

                var role = dbContext.DomainRole.Find(roleId);
                var domain = dbContext.Domains.Find(domainId);
                var users = domain.Users.Where(s => !role.Users.Any(u => u.Id == s.Id)).ToList();
                return users;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, roleId);
                return new List<ApplicationUser>();
            }
        }
        public ReturnJsonModel UpdateSystemUserRoles(string userId, string[] roles)
        {

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add/ Remove user from roles", null, null, roles, userId);

                var userManager = new UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.CoreCompat.UserStore<ApplicationUser>(dbContext));

                var userRoles = userManager.GetRoles(userId).ToArray();

                userManager.RemoveFromRoles(userId, userRoles);
                if (roles != null && roles.Length > 0)
                    userManager.AddToRoles(userId, roles);

                return new ReturnJsonModel() { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, roles, userId);
                return new ReturnJsonModel() { result = false, msg = ex.Message };
            }
        }
        public List<BaseModel> GetDomainRolesBase(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get domain roles by domain id", null, null, domainId);

                return dbContext.DomainRole.Where(x => x.Domain.Id == domainId).OrderBy(n => n.Name).Select(l => new BaseModel { Id = l.Id, Name = l.Name }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<BaseModel>();
            }
        }
    }

}
