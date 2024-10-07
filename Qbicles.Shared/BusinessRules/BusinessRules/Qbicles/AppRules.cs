using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class AppRules
    {
        ApplicationDbContext dbContext;
        public AppRules()
        {

        }
        public AppRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ReturnJsonModel ChangePermissions(int appId, int rightId, bool isCheck, int roleId, int domainId)
        {
            var refModel = new ReturnJsonModel { result = false, msgId = "" };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Change permission", null, null, appId, rightId, isCheck, roleId, domainId);

                var domain = dbContext.Domains.FirstOrDefault(x => x.Id == domainId);
                var right = dbContext.AppRight.FirstOrDefault(x => x.Id == rightId);
                var role = dbContext.DomainRole.FirstOrDefault(x => x.Id == roleId);

                var instance = domain.AssociatedApps.FirstOrDefault(x => x.QbicleApplication.Id == appId);

                if (isCheck)
                {

                    var rraXref = new RoleRightAppXref()
                    {
                        Right = right,
                        Role = role,
                        AppInstance = instance
                    };
                    dbContext.RoleRightAppXref.Add(rraXref);
                    dbContext.SaveChanges();
                }
                else
                {
                    var rra = dbContext.RoleRightAppXref.FirstOrDefault(x => x.AppInstance.Id == instance.Id && x.Right.Id == right.Id && x.Role.Id == role.Id);
                    if (rra != null)
                    {
                        dbContext.RoleRightAppXref.Remove(rra);
                        dbContext.SaveChanges();

                        //if not exists right then remove app in AppInstance
                        if (dbContext.RoleRightAppXref.Any(x => x.Role.Id == roleId && x.AppInstance.QbicleApplication.Id == appId) == false)
                            refModel.msgId = appId.ToString();
                    }
                }

                refModel.result = true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, appId, rightId, isCheck, roleId, domainId);
            }

            return refModel;
        }

        public ReturnJsonModel UpdateAppsForDomain(List<int> appsIdAdded, List<int> appsIdRemoved, string userId, int roleId, int domainId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update apps for domain", userId,
                        null, appsIdAdded, appsIdRemoved, roleId, domainId);

                var currentDomain = dbContext.Domains.Find(domainId);
                if (currentDomain == null)
                    return refModel;

                if (appsIdRemoved != null)
                    foreach (var appId in appsIdRemoved)
                    {
                        var instanceAppDomain = currentDomain.AssociatedApps.FirstOrDefault(x => x.QbicleApplication.Id == appId);
                        if (instanceAppDomain == null)
                            continue;

                        var roleRightAppXrefs = dbContext.RoleRightAppXref.
                            Where(x => x.AppInstance.Id == instanceAppDomain.Id && x.Role.Id == roleId);

                        foreach (var rra in roleRightAppXrefs)
                        {
                            dbContext.RoleRightAppXref.Remove(rra);
                        }
                        if (roleRightAppXrefs.Any())
                            dbContext.SaveChanges();
                    }

                var listAdded = new List<object>();
                var role = dbContext.DomainRole.Find(roleId);
                //var apps = currentDomain.SubscribedApps;
                if (appsIdAdded != null)
                    foreach (var appId in appsIdAdded)
                    {
                        var app = dbContext.Applications.Find(appId);

                        var instanceAppDomain = currentDomain.AssociatedApps.FirstOrDefault(x => x.QbicleApplication.Id == appId);
                        if (instanceAppDomain == null)
                        {
                            instanceAppDomain = new AppInstance
                            {
                                CreatedBy = dbContext.QbicleUser.Find(userId),
                                CreatedDate = System.DateTime.UtcNow,
                                Domain = currentDomain,
                                QbicleApplication = app
                            };
                            dbContext.AppInstances.Add(instanceAppDomain);
                            dbContext.SaveChanges();
                        }
                        var listRight = new List<object>();

                        if (dbContext.RoleRightAppXref.Any(x => x.AppInstance.Id == instanceAppDomain.Id && x.Role.Id == roleId) == false)
                        {
                            foreach (var r in app.Rights)
                            {
                                var rra = new RoleRightAppXref()
                                {
                                    AppInstance = instanceAppDomain,
                                    Right = r,
                                    Role = role
                                };

                                dbContext.RoleRightAppXref.Add(rra);

                                listRight.Add(new { r.Id, r.Name, Ischeck = "checked" });
                            }
                            dbContext.SaveChanges();

                            listAdded.Add(new { app.Id, app.Name, listRight });
                        }
                    }

                refModel.result = true;
                refModel.Object = listAdded;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, appsIdAdded, appsIdRemoved, roleId, domainId);
            }

            return refModel;
        }

    }
}
