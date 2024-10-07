using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class AppInstancesRules
    {
        ApplicationDbContext dbContext;
        public AppInstancesRules()
        {

        }
        public AppInstancesRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public List<object> GetAppByRoleId(int roleId, QbicleDomain currentDomain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get App by role id", null, null, roleId, currentDomain);

                var listApps = currentDomain.SubscribedApps.Where(e => !e.IsCore);

                var listAppsId = listApps.Select(x => x.Id).ToList();

                var listRoleRightAppXref = dbContext.RoleRightAppXref.Where(r => r.Role.Id == roleId && listAppsId.Contains(r.AppInstance.QbicleApplication.Id)).ToList();

                var appsReturn = new List<object>();
                if (listRoleRightAppXref.Any())
                {
                    foreach (var app in listApps)
                    {
                        var listRight = new List<object>();
                        var listRightForApp = listRoleRightAppXref.Where(x => x.AppInstance.QbicleApplication.Id == app.Id).Select(t => t.Right.Id);
                        if (!listRightForApp.Any())
                            continue;
                        foreach (var r in app.Rights)
                        {
                            var isCheck = "";
                            if (listRightForApp.Any(ri => ri == r.Id))
                            {
                                isCheck = "checked";
                            }
                            var right = new { r.Id, r.Name, Ischeck = isCheck };
                            listRight.Add(right);
                        }

                        appsReturn.Add(new { app.Id, app.Name, listRight });
                    }
                }
                return appsReturn;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, roleId, currentDomain);
                return new List<object>();
            }
        }
    }
}
