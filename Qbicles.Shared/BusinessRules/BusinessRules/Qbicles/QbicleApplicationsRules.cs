using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class QbicleApplicationsRules
    {
        ApplicationDbContext dbContext;

        public QbicleApplicationsRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<QbicleApplication> GetAllQbicleApplications()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all qbicle applications", null, null);

                return dbContext.Applications.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<QbicleApplication>();
            }
        }

        public List<QbicleApplication> GetQbicleApplicationsNotCore()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get qbicle applications not core", null, null);

                return dbContext.Applications.Where(e => !e.IsCore).OrderBy(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<QbicleApplication>();
            }
        }

        public ReturnJsonModel SubscribeApp(int applicationId, string currentUserId, int domainId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Subscribe app", null, null, applicationId, currentUserId, domainId);

                var domain = dbContext.Domains.Find(domainId);
                if (domain == null) return refModel;

                if (!domain.AvailableApps.Any(a => a.Id == applicationId))
                {
                    refModel.result = false;
                    refModel.msg = "The app did not available!";
                    return refModel;
                }


                var app = dbContext.Applications.Find(applicationId);
                domain.SubscribedApps.Add(app);
                var user = new UserRules(dbContext).GetUser(currentUserId, 0);
                dbContext.SubscribedAppsLogs.Add(new SubscribedAppsLog
                {
                    Status = SubscriptionStatus.Subscribed,
                    Application = app,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Domain = domain
                });

                //init app instance subscribe an app if not exist
                if (domain.AssociatedApps.All(e => e.QbicleApplication.Id != applicationId))
                {
                    dbContext.AppInstances.Add(new AppInstance
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Domain = domain,
                        QbicleApplication = app
                    });
                }
                dbContext.SaveChanges();

                refModel.result = true;

            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, applicationId, currentUserId, domainId);
            }
            return refModel;
        }


        public ReturnJsonModel UnSubscribeApp(int applicationId, string currentUserId, int domainId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Subscribe app", null, null, applicationId, currentUserId, domainId);



                var domain = dbContext.Domains.Find(domainId);
                if (domain == null) return refModel;
                var user = new UserRules(dbContext).GetUser(currentUserId, 0);
                var app = dbContext.Applications.Find(applicationId);
                domain.SubscribedApps.Remove(app);

                dbContext.SubscribedAppsLogs.Add(new SubscribedAppsLog
                {
                    Status = SubscriptionStatus.Unsubscribed,
                    Application = app,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Domain = domain
                });

                //remove RoleRightAppXref
                var instancesAppDomain = domain.AssociatedApps.Where(x => x.QbicleApplication.Id == applicationId).ToList();

                instancesAppDomain.ForEach(instance =>
                {
                    var roleRightAppXrefs = dbContext.RoleRightAppXref.Where(x => x.AppInstance.Id == instance.Id);

                    foreach (var rra in roleRightAppXrefs)
                    {
                        dbContext.RoleRightAppXref.Remove(rra);
                    }
                });

                dbContext.SaveChanges();
                refModel.result = true;


            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, applicationId, currentUserId, domainId);
            }
            return refModel;
        }



        public List<DomainAppAccess> GetDomainAppAssign(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get qbicle applications not core", null, null);
                var availables = dbContext.Domains.Find(domainId).AvailableApps;
                var apps = dbContext.Applications.Where(e => !e.IsCore).ToList();

                return apps.Select(e => new DomainAppAccess
                {
                    Id = e.Id,
                    Name = e.Name,
                    Checked = availables.Any(a => a.Id == e.Id) ? "checked" : ""
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<DomainAppAccess>();
            }
        }

        public ReturnJsonModel SaveAppAssignModal(string domainKey, string[] addAppIds)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveAddRemoveAppsDomains", null, null, addAppIds);

                var domainId = int.Parse(EncryptionService.Decrypt(domainKey));

                var domain = dbContext.Domains.FirstOrDefault(e => e.Id == domainId);

                domain.AvailableApps.Clear();

                if (addAppIds != null && addAppIds.Any())
                {
                    var appsId = addAppIds.Select(int.Parse).ToArray();
                    var apps = dbContext.Applications.Where(e => appsId.Contains(e.Id)).ToList();
                    domain.AvailableApps.AddRange(apps);
                }

                dbContext.SaveChanges();
                return refModel;

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, addAppIds);
            }
            return refModel;
        }

        public ReturnJsonModel SaveAddRemoveAppsDomains(bool add, bool all, string[] domainKeys, string[] appIds)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveAddRemoveAppsDomains", null, null, domainKeys, appIds);
                //

                var domainIds = domainKeys.Select(p => EncryptionService.Decrypt(p)).ToList();

                if (appIds.Any(a => a == ""))
                {
                    refModel.result = false;
                    refModel.msg = "No app(s) has selected. Please chosen at least one app!";
                    return refModel;
                }

                var appsId = appIds.Select(int.Parse).ToArray();

                List<QbicleDomain> domains;
                if (!all)
                {
                    var domainsId = domainIds.Select(int.Parse).ToArray();
                    domains = dbContext.Domains.Where(d => domainsId.Contains(d.Id)).ToList();
                }
                else
                    domains = dbContext.Domains.ToList();

                var appsSelected = dbContext.Applications.Where(a => appsId.Contains(a.Id)).ToList();

                if (!add)
                {
                    appsSelected.ForEach(a =>
                    {
                        domains.ForEach(d =>
                        {
                            if (d.AvailableApps.Any(ap => ap.Id == a.Id))
                                d.AvailableApps.Remove(a);
                        });
                    });

                    dbContext.SaveChanges();

                    return refModel;
                }

                appsSelected.ForEach(a =>
                {
                    domains.ForEach(d =>
                    {
                        if (!d.AvailableApps.Any(ap => ap.Id == a.Id))
                            d.AvailableApps.Add(a);
                    });
                });

                dbContext.SaveChanges();

                return refModel;


            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainKeys, appIds);
            }
            return refModel;
        }


        public ReturnJsonModel RevokeAllApps(bool all, string[] domainIds, string userId)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "RevokeAllApps", null, null, domainIds);

                List<QbicleDomain> domains;
                if (!all)
                {
                    var domainsId = domainIds.Select(int.Parse).ToArray();
                    domains = dbContext.Domains.Where(d => domainsId.Contains(d.Id)).ToList();
                }
                else
                    domains = dbContext.Domains.ToList();


                domains.ForEach(d =>
                {
                    d.AvailableApps.ForEach(a =>
                    {
                        UnSubscribeApp(a.Id, userId, d.Id);
                    });
                });


                domains.ForEach(d =>
                {
                    d.SubscribedApps.Clear();
                    d.AvailableApps.Clear();
                });
                dbContext.SaveChanges();

                return refModel;

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainIds);
            }
            return refModel;
        }

    }
}
