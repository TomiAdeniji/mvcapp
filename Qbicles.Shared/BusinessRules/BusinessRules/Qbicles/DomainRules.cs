using PayStackDotNetSDK.Models.Transactions;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.BusinessRules.PayStack;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleDomain;

namespace Qbicles.BusinessRules
{
    public class DomainRules
    {
        private ApplicationDbContext dbContext;
        private ReturnJsonModel refModel;

        public DomainRules()
        {
            dbContext = new ApplicationDbContext();
        }

        public DomainRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ApplicationUser GetUser(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user", null, null, userId);

                var ur = new UserRules(dbContext);
                return ur.GetUser(userId, 0);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;
            }
        }

        public bool IsMemberOrOwnerDomain(string userId, QbicleDomain currentDomain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check is member or owner of domain", null, null, userId, currentDomain);

                if (currentDomain == null)
                    return new AccountRules(dbContext).GetAccountByOwner(userId) != null;
                if (currentDomain.OwnedBy.Id == userId)
                    return true;
                return currentDomain.Users.Any(x => x.Id == userId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, currentDomain);
                return false;
            }
        }

        public bool IsOwnerOrDomainAdmin(string userId, QbicleDomain currentDomain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check is owner or domain admin", null, null, userId, currentDomain);

                if (currentDomain == null)
                    return new AccountRules(dbContext).GetAccountByOwner(userId) != null;
                if (currentDomain.OwnedBy.Id == userId)
                    return true;
                if (currentDomain.Administrators.Any(x => x.Id == userId))
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, currentDomain);
                return false;
            }
        }

        public bool IsDuplicateDomainNameCheck(int domainId, string domainName, int requestId = 0)
        {
            var isDuplicate = false;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate domain name", null, null, domainId, domainName, requestId);

                var domainRequestNameReserveTime = int.Parse(ConfigurationManager.AppSettings["DomainRequestNameReserveTime"]);
                var startTime = DateTime.UtcNow.AddMinutes(-domainRequestNameReserveTime);
                // Domain name must not be the same as DomainRequestName in Reserve time
                var isDuplicatedDomainRequestName = dbContext.QbicleDomainRequests.Any(x =>
                                                        (x.DomainPlan == null || x.DomainPlan.Domain == null || x.DomainPlan.Domain.Id != domainId)
                                                        && x.Id != requestId
                                                        && x.RequestedName == domainName
                                                        && (x.CreatedDate >= startTime));
                var isDuplicatedDomainName = dbContext.Domains.Any(x => x.Id != domainId && x.Name == domainName);
                isDuplicate = isDuplicatedDomainName || isDuplicatedDomainRequestName;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, domainName, requestId);
            }

            return isDuplicate;
        }

        /// <summary>
        ///     Get the Domain by Domain Id
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public QbicleDomain GetDomainById(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get domain by id", null, null, domainId);

                return dbContext.Domains.Find(domainId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }

        /// <summary>
        ///     Get list users associated with the Domain by domainId
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public List<ApplicationUser> GetUsersByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get users by domain id", null, null, domainId);

                var domain = GetDomainById(domainId);
                return domain.Users;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<ApplicationUser>();
            }
        }

        public ReturnJsonModel OpenOrCloseDomainById(int id, string userId)
        {
            refModel = new ReturnJsonModel
            {
                result = false,
                msg = "An error"
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get users by domain id", null, null, id, userId);

                var domain = GetDomainById(id);
                var currentUser = dbContext.QbicleUser.Find(userId);
                if (currentUser.IsSystemAdmin)
                {
                    if (domain.Status == QbicleDomain.DomainStatusEnum.Open)
                        domain.Status = QbicleDomain.DomainStatusEnum.Closed;
                    else
                        domain.Status = QbicleDomain.DomainStatusEnum.Open;
                    if (dbContext.Entry(domain).State == EntityState.Detached)
                        dbContext.Domains.Attach(domain);
                    dbContext.Entry(domain).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.result = true;
                    refModel.msg = domain.Status == QbicleDomain.DomainStatusEnum.Open ? "Active" : "Closed";
                    refModel.Object = new
                    { Label = domain.Status == QbicleDomain.DomainStatusEnum.Open ? "Close Domain" : "Open Domain" };
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_1");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, userId);
            }

            return refModel;
        }

        public DataTableModel GetAllDomain(int column, string orderBy,
           string name, string dateRange, int status, int start, int length, string type,
           string currentDateFormat, string currentTimezone, string currentUserId, int draw, string dateTimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all domain", null, null, column, orderBy, name, dateRange, status, start, length, type, currentDateFormat, currentUserId, draw, dateTimeFormat);

                var query = new List<QbicleDomain>().AsQueryable();

                query = dbContext.Domains.AsQueryable();

                //if (type.Equals("Administrator"))
                //{
                //    query = query.Where(d => d.Administrators.Any(a => a.Id == currentUserId));
                //}

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
                if (status != 0)
                {
                    query = query.Where(p => p.Status == (QbicleDomain.DomainStatusEnum)status);
                }

                if (!String.IsNullOrEmpty(dateRange))
                {
                    string[] splitDate = dateRange.Split('-');
                    var startDate = DateTime.ParseExact(splitDate[0].Trim(), currentDateFormat, new CultureInfo("en-US"));
                    var endDate = DateTime.ParseExact(splitDate[1].Trim(), currentDateFormat, new CultureInfo("en-US")).AddDays(1);
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);
                }
                var totalRecord = query.Count();
                if (column == 1)
                {
                    if (orderBy.Equals("asc"))
                    {
                        query = query.OrderBy(p => p.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.Name);
                    }
                }
                if (column == 2)
                {
                    if (orderBy.Equals("asc"))
                    {
                        query = query.OrderBy(p => p.CreatedDate);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.CreatedDate);
                    }
                }
                if (column == 3)
                {
                    if (orderBy.Equals("asc"))
                    {
                        query = query.OrderBy(p => p.Status);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.Status);
                    }
                }
                query = query.Skip(start).Take(length);

                var domains = query.ToList().Select(s =>
                new
                {
                    s.Id,
                    s.Key,
                    s.LogoUri,
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(currentTimezone).ToString(dateTimeFormat),
                    s.Status,
                    s.Name
                }).ToList();

                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = domains,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };

                return dataTableData;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, column, orderBy, name, dateRange, status, start, length, type, currentDateFormat, currentUserId, draw, dateTimeFormat);
                return new DataTableModel
                {
                    draw = draw,
                    data = new List<QbicleDomain>(),
                    recordsFiltered = 0,
                    recordsTotal = 0
                };
            }
        }

        public List<QbicleDomain> GetAllDomain()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all Domain", null, null);

                return dbContext.Domains.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }

        /// <summary>
        ///     Check user if not exist domain.guests then add the guest to Domain
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool AddGuestToDomain(QbicleDomain domain, ApplicationUser user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add guest to domain", null, null, domain, user);

                if (!domain.Users.Any(u => u.Email == user.Email))
                {
                    domain.Users.Add(user);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domain, user);
            }

            return true;
        }

        public List<QbicleDomain> GetDomainsByUserId(string userId, string filter = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get domains by user id", null, null, userId);
                if (filter == "")
                    return dbContext.Domains.Where(x => x.OwnedBy.Id == userId || x.Users.Any(u => u.Id == userId)).OrderBy(x => x.Name).ToList();
                else
                    return dbContext.Domains.Where(x => x.Name.ToLower().Contains(filter.ToLower()) && (x.OwnedBy.Id == userId || x.Users.Any(u => u.Id == userId))).OrderBy(x => x.Name)
                        .ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new List<QbicleDomain>();
            }
        }

        public async Task<ReturnJsonModel> UpdateOrInsertDomain(QbicleDomain domain, string userId, string serverPath, Qbicle initialQbicle = null, int requestId = 0, bool isCutomDomain = false)
        {
            refModel = new ReturnJsonModel { result = false, msg = "An error" };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Insert or update domain", userId, null, domain, serverPath, initialQbicle, requestId);

                    var user = dbContext.QbicleUser.Find(userId);

                    if (IsDuplicateDomainNameCheck(domain.Id, domain.Name, requestId))
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_9");
                        return refModel;
                    }

                    var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.Id == requestId);

                    /// Get the Subscription account of which the user is the owner
                    var domainAccount = new AccountRules(dbContext).GetAccountByOwner(user.Id);
                    if (domainAccount == null)
                    {
                        //Create new Account
                        domainAccount = new SubscriptionAccount()
                        {
                            AccountName = user.UserName + "_Account",
                            AccountCreator = user,
                            AccountOwner = user,
                            CompanyOrganisationName = user.Company ?? user.UserName + "_Company",
                            AccountPackage = new AccountPackageRules(dbContext).GetAccountPackage(1),
                        };
                        domainAccount.Administrators.Add(user);
                        dbContext.SubscriptionAccounts.Add(domainAccount);
                        dbContext.Entry(domainAccount).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }
                    var domainEx = GetDomainById(domain.Id);
                    if (string.IsNullOrEmpty(domain.LogoUri) && domainEx == null)
                    {
                        var filePath = serverPath + HelperClass.DomainLogoDefault.TrimStart('/').Replace('/', '\\');
                        domain.LogoUri = await AzureStorageHelper.UploadMediaFromPath("icon_domain_default.png", filePath);
                    }
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    if (!string.IsNullOrEmpty(domain.LogoUri) && (domainEx == null || (domainEx != null && domainEx.LogoUri != domain.LogoUri)))
                    {
                        s3Rules.ProcessingMediaS3(domain.LogoUri);
                    }
                    if (domainEx == null)
                    {
                        if (new AccountRules(dbContext).IsValidNumberOfDomain(domainAccount) == false)
                        {
                            refModel.msg = ResourcesManager._L("ERROR_MSG_17");
                            return refModel;
                        }

                        #region Create a Domain

                        domain.OwnedBy = user;
                        domain.Status = QbicleDomain.DomainStatusEnum.Open;
                        domain.CreatedBy = user;
                        domain.CreatedDate = DateTime.UtcNow;
                        domain.Administrators.Add(user);
                        domain.Account = domainAccount;
                        domain.WizardStep = DomainWizardStep.None;
                        domain.WizardStepMicro = DomainWizardStepMicro.None;
                        domain.Users.Add(user);
                        // get App IsCore
                        var applications = dbContext.Applications.Where(e => e.IsCore).ToList();
                        domain.SubscribedApps.AddRange(applications);

                        dbContext.Domains.Add(domain);
                        dbContext.Entry(domain).State = EntityState.Added;
                        dbContext.SaveChanges();

                        var domainPlan = domainRequest?.DomainPlan;
                        if (domainPlan != null)
                        {
                            domainPlan.Domain = domain;
                            dbContext.Entry(domainPlan).State = EntityState.Modified;
                            dbContext.SaveChanges();
                        }

                        applications.ForEach(app =>
                        {
                            dbContext.SubscribedAppsLogs.Add(new SubscribedAppsLog
                            {
                                Status = SubscriptionStatus.Subscribed,
                                Application = app,
                                CreatedBy = user,
                                CreatedDate = DateTime.UtcNow,
                                Domain = domain
                            });

                            //init app instance is core when create new Domain
                            var appInstance = new AppInstance
                            {
                                CreatedBy = user,
                                CreatedDate = DateTime.UtcNow,
                                Domain = domain,
                                QbicleApplication = app
                            };

                            dbContext.AppInstances.Add(appInstance);

                            //Setup the Qbicles Business app correctly
                            if (app.Name == QbiclesBoltOns.QbiclesBusiness)
                            {
                                if (dbContext.DomainRole.Any(r => r.Domain.Id == domain.Id && r.Name == FixedRoles.QbiclesBusinessRole))
                                    return;
                                var domainRole = new DomainRole()
                                {
                                    CreatedBy = user,
                                    CreatedDate = DateTime.UtcNow,
                                    Domain = domain,
                                    Name = FixedRoles.QbiclesBusinessRole
                                };

                                dbContext.DomainRole.Add(domainRole);
                                dbContext.Entry(domainRole).State = EntityState.Added;

                                var roleRightXref = new RoleRightAppXref
                                {
                                    AppInstance = appInstance,
                                    Right = dbContext.AppRight.FirstOrDefault(e => e.Name == RightPermissions.QbiclesBusinessAccess),
                                    Role = domainRole
                                };

                                dbContext.RoleRightAppXref.Add(roleRightXref);
                                dbContext.Entry(roleRightXref).State = EntityState.Added;
                            }
                        });
                        dbContext.SaveChanges();
                        refModel.result = true;

                        #endregion Create a Domain

                        #region Initial Qbicle

                        if (!string.IsNullOrEmpty(initialQbicle?.Name) && refModel.result)
                        {
                            if (new QbicleRules(dbContext).DuplicateQbicleNameCheck(0, initialQbicle.Name, domain.Id))
                            {
                                refModel.result = false;
                                dbTransaction.Rollback();
                                refModel.msg = ResourcesManager._L("ERROR_MSG_41");
                                return refModel;
                            }
                            if (string.IsNullOrEmpty(initialQbicle.LogoUri))
                                initialQbicle.LogoUri = HelperClass.QbicleLogoDefault;
                            else
                            {
                                s3Rules.ProcessingMediaS3(initialQbicle.LogoUri);
                            }
                            initialQbicle.StartedBy = user;
                            initialQbicle.StartedDate = DateTime.UtcNow;
                            initialQbicle.LastUpdated = initialQbicle.StartedDate;
                            initialQbicle.OwnedBy = user;
                            initialQbicle.Manager = user;
                            initialQbicle.Domain = domain;
                            initialQbicle.Members.Add(user);
                            initialQbicle.Domain.Account.QbiclesCount++;
                            dbContext.Qbicles.Add(initialQbicle);
                            dbContext.Entry(initialQbicle).State = EntityState.Added;
                            //Create a Topic and a Media Folder for the Qbicle called General
                            var topic = new TopicRules(dbContext).SaveTopic(initialQbicle.Id, HelperClass.GeneralName);
                            new MediaFolderRules(dbContext).InsertMediaFolder(HelperClass.GeneralName, user.Id, initialQbicle.Id);

                            #region If the Domain.Type = Business or Domain.Type == Premium will automatically (Create a TraderLocation,Create a Trader WorkGroup)

                            if (domain.DomainType == QbicleDomain.DomainTypeEnum.Business || domain.DomainType == DomainTypeEnum.Premium)
                            {
                                var dimension = new TransactionDimension();
                                dimension.Name = TraderConst.DefaultTransactionDimensionName;
                                dimension.CreatedBy = user;
                                dimension.CreatedDate = DateTime.UtcNow;
                                dimension.Domain = domain;
                                dbContext.TransactionDimensions.Add(dimension);
                                dbContext.Entry(dimension).State = EntityState.Added;
                                var location = new TraderLocation();
                                location.Name = TraderConst.DefaultTraderLocationName;
                                location.Domain = domain;
                                location.CreatedDate = DateTime.UtcNow;
                                location.CreatedBy = user;
                                location.IsDefaultAddress = true;
                                location.Address = null;
                                dbContext.TraderLocations.Add(location);
                                dbContext.Entry(location).State = EntityState.Added;
                                var tradergroup = new TraderGroup();
                                tradergroup.Domain = domain;
                                tradergroup.Name = TraderConst.DefaultTraderGroupName;
                                tradergroup.CreatedBy = user;
                                tradergroup.CreatedDate = DateTime.UtcNow;
                                dbContext.TraderGroups.Add(tradergroup);
                                dbContext.Entry(tradergroup).State = EntityState.Added;
                                var workgroup = new WorkGroup();
                                workgroup.Name = TraderConst.DefaultTraderWorkGroupName;
                                workgroup.Domain = domain;
                                workgroup.Qbicle = initialQbicle;
                                workgroup.Topic = topic;
                                workgroup.Location = location;
                                workgroup.CreatedDate = DateTime.UtcNow;
                                workgroup.CreatedBy = user;
                                workgroup.Processes = dbContext.TraderProcesses.ToList();
                                workgroup.Members.Add(user);
                                workgroup.Approvers.Add(user);
                                workgroup.Reviewers.Add(user);
                                workgroup.ItemCategories.Add(tradergroup);
                                //create ApprovalRequestDefinition for process
                                var filePath = serverPath + @"Content//DesignStyle//img//icon_bookkeeping.png";
                                string iconApprUri = await AzureStorageHelper.UploadMediaFromPath($"icon_bookkeeping-{workgroup.Name}.png", filePath);
                                if (!string.IsNullOrEmpty(iconApprUri))
                                    s3Rules.ProcessingMediaS3(initialQbicle.LogoUri);
                                new TraderWorkGroupsRules(dbContext).ApprovalDefsForWorgroup(workgroup, user, iconApprUri);
                                dbContext.WorkGroups.Add(workgroup);
                                dbContext.Entry(workgroup).State = EntityState.Added;
                                var cashaccount = new TraderCashAccount();
                                cashaccount.Name = TraderConst.DefaultTraderCashAccountName;
                                cashaccount.ImageUri = domain.LogoUri;
                                cashaccount.AssociatedBKAccount = null;
                                cashaccount.CreatedDate = DateTime.UtcNow;
                                cashaccount.CreatedBy = domain.CreatedBy;
                                cashaccount.Domain = domain;
                                dbContext.TraderCashAccounts.Add(cashaccount);
                                dbContext.Entry(cashaccount).State = EntityState.Added;
                            }

                            #endregion If the Domain.Type = Business or Domain.Type == Premium will automatically (Create a TraderLocation,Create a Trader WorkGroup)

                            dbContext.SaveChanges();
                            refModel.result = true;
                        }

                        #endregion Initial Qbicle

                        if (isCutomDomain)
                            CreateCustomDomainPlan(domain);

                        //Payment for Domain Creation
                        var job = new QbicleJobParameter
                        {
                            Id = domain.Id,
                            EndPointName = "processnewdomaincreated",
                        };
                        var resultHangFire = await new QbiclesJob().HangFireExcecuteAsync(job);
                        refModel.Object = domain;
                        refModel.Object2 = domain.Key;
                    }
                    else// update domain
                    {
                        domainEx.LogoUri = string.IsNullOrEmpty(domain.LogoUri) ? domainEx.LogoUri : domain.LogoUri;
                        domainEx.Name = domain.Name;

                        // Update the Name of B2BProfile
                        var b2bProfile = dbContext.B2BProfiles.FirstOrDefault(p => p.Domain.Id == domainEx.Id);
                        if (b2bProfile != null)
                        {
                            b2bProfile.BusinessName = domain.Name;
                            dbContext.Entry(b2bProfile).State = EntityState.Modified;
                        }

                        if (dbContext.Entry(domainEx).State == EntityState.Detached)
                            dbContext.Domains.Attach(domainEx);
                        dbContext.Entry(domainEx).State = EntityState.Modified;
                        refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                    }
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, domain, serverPath, initialQbicle, requestId);
                    refModel.result = false;
                    dbTransaction.Rollback();
                }
            }

            return refModel;
        }

        private void CreateCustomDomainPlan(QbicleDomain domain)
        {
            var customDomainPlan = new DomainPlan()
            {
                ActualCost = 0,
                CalculatedCost = 0,
                NumberOfExtraUsers = 0,
                PayStackPlanCreationResponse = "",
                PayStackPlanCode = "",
                PayStackPlanName = "",
                InitTransactionResponseJSON = "",
                Level = dbContext.BusinessDomainLevels.FirstOrDefault(p => p.Level == BusinessDomainLevelEnum.Existing),
                IsArchived = false,
                Domain = domain,
            };
            dbContext.DomainPlans.Add(customDomainPlan);
            dbContext.Entry(customDomainPlan).State = EntityState.Added;

            dbContext.SaveChanges();
        }

        public DataTableModel GetListDomainApplicationAccess(int column, string orderBy,
           string name, int status, int start, int length, int draw)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all domain", null, null, column, orderBy, name, status, start, length, draw);

                var query = new List<QbicleDomain>().AsQueryable();

                query = dbContext.Domains.AsQueryable();

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
                if (status != 0)
                    query = query.Where(p => p.Status == (QbicleDomain.DomainStatusEnum)status);

                var totalRecord = query.Count();
                if (column == 2)
                {
                    if (orderBy.Equals("asc"))
                        query = query.OrderBy(p => p.Name);
                    else
                        query = query.OrderByDescending(p => p.Name);
                }
                if (column == 3)
                {
                    if (orderBy.Equals("asc"))
                        query = query.OrderBy(p => p.Status);
                    else
                        query = query.OrderByDescending(p => p.Status);
                }
                query = query.Skip(start).Take(length);

                var domains = query.ToList().Select(s =>
                new
                {
                    s.Id,
                    s.Key,
                    s.LogoUri,
                    Status = s.Status.GetDescription(),
                    s.Name,
                    //SubscribedApps = string.Join(",", s.SubscribedApps.OrderBy(a=>a.Name).Select(n => n.Name)),
                    AvailableApps = string.Join(",", s.AvailableApps.OrderBy(a => a.Name).Select(n => n.Name)),
                }
                ).ToList();

                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = domains,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };

                return dataTableData;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, column, orderBy, name, status, start, length, draw);
                return new DataTableModel
                {
                    draw = draw,
                    data = new List<QbicleDomain>(),
                    recordsFiltered = 0,
                    recordsTotal = 0
                };
            }
        }

        public List<QbicleApplication> GetDomainAvailableApps(int id)
        {
            try
            {
                return dbContext.Domains.Find(id).AvailableApps.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                throw;
            }
        }

        public ReturnJsonModel UpdateDateDomainSetting(int id, string name, string image)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod());

                var domain = dbContext.Domains.Find(id);
                domain.Name = name;

                // Update B2B Business profile name
                var b2bProfile = dbContext.B2BProfiles.FirstOrDefault(p => p.Domain.Id == domain.Id);
                if (b2bProfile != null)
                {
                    b2bProfile.BusinessName = name;
                    dbContext.Entry(b2bProfile).State = EntityState.Modified;
                }

                if (!string.IsNullOrEmpty(image) && image != domain.LogoUri)
                {
                    new AzureStorageRules(dbContext).ProcessingMediaS3(image);
                    domain.LogoUri = image;
                }
                dbContext.SaveChanges();
                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public void DeleteDomain(int id)
        {
            var domain = dbContext.Domains.Find(id);
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    domain.Administrators.Clear();
                    domain.accountgroups.Clear();
                    domain.AssociatedApps.Clear();
                    domain.AvailableApps.Clear();
                    domain.B2BQbicles.Clear();
                    domain.BKAppSettings.Clear();
                    domain.BusinessPages.Clear();
                    domain.CBWorkgroups.Clear();
                    domain.CoANode.Clear();
                    domain.CommunityPages.Clear();
                    domain.ContactGroups.Clear();
                    domain.Dimensions.Clear();
                    domain.HighlightPosterHiddenUser.Clear();
                    domain.JournalGroups.Clear();
                    domain.QbicleManagers.Clear();
                    domain.Qbicles.Clear();
                    domain.SubscribedApps.Clear();
                    domain.TraderGroups.Clear();
                    domain.TraderItems.Clear();
                    domain.TraderLocations.Clear();
                    domain.Workgroups.Clear();

                    domain.Users.Clear();

                    dbContext.Domains.Remove(domain);

                    dbContext.SaveChanges();
                    dbTransaction.Rollback();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                    dbTransaction.Rollback();
                }
            }
        }

        public ReturnJsonModel CreateDomainRequest(DomainRequest domain, InitialQbicleRequest initialQbicle, DomainTypeEnum domainType, string userId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domain, initialQbicle, userId);

                var domainJson = JsonHelper.ToJson(domain);
                var qbicleJson = JsonHelper.ToJson(initialQbicle);

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                if (!string.IsNullOrEmpty(domain.LogoUri))
                {
                    s3Rules.ProcessingMediaS3(domain.LogoUri);
                }
                if (!string.IsNullOrEmpty(initialQbicle.LogoUri))
                {
                    s3Rules.ProcessingMediaS3(initialQbicle.LogoUri);
                }

                var currentUser = dbContext.QbicleUser.Find(userId);
                var domainRequest = new QbicleDomainRequest()
                {
                    DomainRequestJSON = domainJson,
                    QbicleRequestJSON = qbicleJson,
                    Status = DomainRequestStatus.Pending,
                    DomainType = domainType,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.QbicleDomainRequests.Add(domainRequest);
                dbContext.Entry(domainRequest).State = EntityState.Added;
                dbContext.SaveChanges();

                var job = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = domainRequest.Id
                };
                new NotificationRules(dbContext).RaiseNotificationOnDomainRequestCreated(job);

                return new ReturnJsonModel() { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domain, initialQbicle, userId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel CreateUpgradedDomainRequest(int domainId, string userId, DomainTypeEnum newDomainType, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel() { actionVal = 1, result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, userId, newDomainType);

                var domain = dbContext.Domains.Find(domainId);
                var user = dbContext.QbicleUser.Find(userId);
                var pendingRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.ExistingDomain != null
                                        && p.ExistingDomain.Id == domainId && p.DomainType == DomainTypeEnum.Premium
                                        && p.Status == DomainRequestStatus.Pending);

                if (pendingRequest != null)
                {
                    result.msg = ResourcesManager._L("ERROR_DATA_EXISTED", "Upgrade request");
                    return result;
                }

                if (domain == null)
                {
                    result.msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "domain");
                    return result;
                }

                if (domain != null && domain.DomainType != DomainTypeEnum.Business && newDomainType == DomainTypeEnum.Premium)
                {
                    result.msg = "Update failed. Only Business Domain can be upgraded to Premium Domain.";
                    return result;
                }

                // Create new Domain Request with existing Domain and type is Premium, status is Approved
                var dmInfor = new DomainRequest()
                {
                    Name = domain.Name,
                    LogoUri = domain.LogoUri
                };

                var dmRequest = new QbicleDomainRequest()
                {
                    DomainRequestJSON = dmInfor.ToJson(),
                    ExistingDomain = domain,
                    QbicleRequestJSON = "",
                    DomainType = newDomainType,
                    Status = DomainRequestStatus.Pending,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow
                };
                dbContext.QbicleDomainRequests.Add(dmRequest);
                dbContext.Entry(dmRequest).State = EntityState.Added;

                dbContext.SaveChanges();

                var job = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = dmRequest.Id
                };
                new NotificationRules(dbContext).RaiseNotificationOnDomainRequestCreated(job);

                result.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, userId, newDomainType);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
            }
            return result;
        }

        public List<QbicleDomainRequestCustomModel> GetListDomainRequestPagination(IDataTablesRequest requestModel, string keySearch, string dateRange, string createdUserIdSearch, DomainTypeEnum domainTypeSearch,
            List<DomainRequestStatus> lstRequestStatusSearch, string dateTimeFormat, string dateFormat, string timeZone, ref int total, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, keySearch, dateRange, createdUserIdSearch, domainTypeSearch, lstRequestStatusSearch);

                var query = from request in dbContext.QbicleDomainRequests select request;

                #region Filter

                if (!string.IsNullOrEmpty(dateRange))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    dateRange.ConvertDaterangeFormat(dateFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(p => p.CreatedDate >= startDate && p.CreatedDate < endDate);
                }

                if (!string.IsNullOrEmpty(createdUserIdSearch))
                {
                    query = query.Where(p => p.CreatedBy.Id == createdUserIdSearch);
                }

                if (lstRequestStatusSearch != null && lstRequestStatusSearch.Count > 0)
                {
                    query = query.Where(p => lstRequestStatusSearch.Contains(p.Status));
                }

                if (domainTypeSearch > 0)
                {
                    query = query.Where(p => p.DomainType == domainTypeSearch);
                }

                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.ToList().Where(p => !string.IsNullOrEmpty(p.DomainRequestJSON) && p.DomainRequestJSON.ParseAs<DomainRequest>().Name.ToLower().Contains(keySearch.ToLower())).AsQueryable();
                }

                #endregion Filter

                #region Ordering

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var sortedString = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "RequestedDate":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "CreatedDate asc" : "CreatedDate desc";
                            break;

                        case "RequestTypeStr":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "DomainType asc" : "DomainType desc";
                            break;

                        case "Status":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Status asc" : "Status desc";
                            break;

                        case "RequestedByName":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "CreatedBy.SurName asc, CreatedBy.ForeName asc" : "CreatedBy.SurName desc, CreatedBy.ForeName desc";
                            break;
                    }
                }

                #endregion Ordering

                query = query.OrderBy(string.IsNullOrEmpty(sortedString) ? "CreatedDate desc" : sortedString);
                total = query.Count();

                #region Paging

                query = query.Skip(start).Take(length);

                #endregion Paging

                var lstDomainRequests = query.ToList();
                var resultList = new List<QbicleDomainRequestCustomModel>();

                lstDomainRequests.ForEach(p =>
                {
                    var domainRequested = p.DomainRequestJSON.ParseAs<DomainRequest>();
                    var customModel = new QbicleDomainRequestCustomModel()
                    {
                        DomainName = domainRequested.Name,
                        DomainLogoUri = domainRequested.LogoUri.ToDocumentUri().ToString() + "&size=T",
                        DomainType = p.DomainType,
                        RequestedByName = p.CreatedBy?.GetFullName() ?? "",
                        RequestedByLogoUri = p.CreatedBy.ProfilePic.ToDocumentUri().ToString() + "&size=T",
                        RequestById = p.CreatedBy.Id,
                        RequestedDate = p.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                        requestId = p.Id,
                        Status = p.Status,
                        RequestStatusLabel = getDomainRequestStatusLabel(p.Status),
                        RequestTypeStr = (p.DomainType == DomainTypeEnum.Premium && p.ExistingDomain != null) ? "Upgrade to Premium" : p.DomainType.ToString()
                    };
                    resultList.Add(customModel);
                });
                return resultList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keySearch, dateRange, createdUserIdSearch, domainTypeSearch, lstRequestStatusSearch);
                return new List<QbicleDomainRequestCustomModel>();
            }
        }

        public List<QbicleDomainRequest> GetListDomainRequestByStatus(DomainRequestStatus status)
        {
            var query = from request in dbContext.QbicleDomainRequests where request.Status == status select request;
            return query.ToList();
        }

        public string getDomainRequestStatusLabel(DomainRequestStatus status)
        {
            var statusLbl = "";
            switch (status)
            {
                case DomainRequestStatus.Pending:
                    statusLbl = "<span class='label label-lg label-warning'>Pending</span>";
                    break;

                case DomainRequestStatus.Approved:
                    statusLbl = "<span class='label label-lg label-success'>Approved</span>";
                    break;

                case DomainRequestStatus.Rejected:
                    statusLbl = "<span class='label label-lg label-danger'>Rejected</span>";
                    break;
            }
            return statusLbl;
        }

        public List<ApplicationUser> getDomainRequestCreator()
        {
            var query = (from request in dbContext.QbicleDomainRequests select request.CreatedBy).Distinct();
            return query.ToList();
        }

        public async Task<ReturnJsonModel> ProcessDomainRequest(int requestId, DomainRequestStatus status, string userId, string serverPath, string originatingConnectionId = "")
        {
            var rs = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestId, status, userId, serverPath);

                var request = dbContext.QbicleDomainRequests.Find(requestId);
                var currentUser = dbContext.QbicleUser.Find(userId);
                if (request == null)
                {
                    rs.result = false;
                    rs.msg = "Can not find the Domain Request";
                    return rs;
                }
                else
                {
                    var domainRequest = request.DomainRequestJSON.ParseAs<DomainRequest>();
                    var initialQbicleRequest = request.QbicleRequestJSON.ParseAs<InitialQbicleRequest>();

                    //QBIC-5049: Can't save domain cause of none Description
                    if (domainRequest.Description.IsNullOrEmpty()) domainRequest.Description = domainRequest.Name;
                    //

                    if (status == DomainRequestStatus.Rejected)
                    {
                        request.Status = DomainRequestStatus.Rejected;
                        request.AcceptOrRejectBy = currentUser;
                        request.AcceptOrRejectDate = DateTime.UtcNow;
                        dbContext.Entry(request).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        rs.result = true;
                    }
                    else if (status == DomainRequestStatus.Approved)
                    {
                        if (request.ExistingDomain != null)
                        {
                            request.ExistingDomain.DomainType = request.DomainType;
                            request.Status = DomainRequestStatus.Approved;
                            dbContext.Entry(request.ExistingDomain).State = EntityState.Modified;
                            dbContext.Entry(request).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            rs.result = true;
                        }
                        else
                        {
                            var domain = new QbicleDomain
                            {
                                Name = domainRequest.Name,
                                LogoUri = domainRequest.LogoUri,
                                DomainType = request.DomainType
                            };
                            var initialQbicle = new Qbicle
                            {
                                Name = initialQbicleRequest.Name,
                                LogoUri = initialQbicleRequest.LogoUri,
                                Description = initialQbicleRequest.Description
                            };

                            // Update trial time
                            var domainPlan = request.DomainPlan;
                            if (domainPlan != null && domainPlan.Level.Level != BusinessDomainLevelEnum.Free)
                            {
                                domain.IsTrialTimeStarted = true;
                            }

                            rs = await UpdateOrInsertDomain(domain, request.CreatedBy.Id, serverPath, initialQbicle, requestId);
                            if (rs.result)
                            {
                                request.Status = DomainRequestStatus.Approved;
                                request.AcceptOrRejectBy = currentUser;
                                request.AcceptOrRejectDate = DateTime.UtcNow;

                                dbContext.Entry(request).State = EntityState.Modified;

                                var profile = new B2BProfile
                                {
                                    BusinessName = domainRequest.Name,
                                    BusinessSummary = domainRequest?.Description ?? domainRequest.Name,
                                    BusinessEmail = request.CreatedBy.Email,
                                    CreatedBy = currentUser,
                                    CreatedDate = DateTime.UtcNow,
                                    LastUpdatedBy = currentUser,
                                    LastUpdatedDate = DateTime.UtcNow,
                                    Domain = domain,
                                    //QBIC-5049: Can't save domain cause of none these infomations
                                    IsB2BServicesProvided = false,
                                    IsDisplayedInB2BListings = false,
                                    IsDisplayedInB2CListings = false
                                };

                                dbContext.Entry(profile).State = EntityState.Added;

                                dbContext.SaveChanges();

                                // Create SubAccount for Created Domain
                                if (!string.IsNullOrEmpty(request.SubAccountInformationJSON))
                                {
                                    if (domainPlan.Level.Level > BusinessDomainLevelEnum.Free)
                                    {
                                        var B2COrderChargeSettings = dbContext.B2COrderPaymentCharges.FirstOrDefault();

                                        var SubAccountInfo = request.SubAccountInformationJSON.ParseAs<SubAccountInformationModel>();
                                        if (SubAccountInfo != null && !string.IsNullOrEmpty(SubAccountInfo.BusinessName) && !string.IsNullOrEmpty(SubAccountInfo.BankCode) && !string.IsNullOrEmpty(SubAccountInfo.AccountNumber))
                                        {
                                            var subAccountCreationResult = await new PayStackRules(dbContext)
                                            .CreateSubAccount(SubAccountInfo.BusinessName, SubAccountInfo.BankCode,
                                                                SubAccountInfo.AccountNumber, B2COrderChargeSettings.QbiclesPercentageCharge);
                                            if (subAccountCreationResult.result == false)
                                            {
                                                rs.result = false;
                                                rs.msg = "Domain created, but the SubAccount creation fails";
                                                return rs;
                                            }
                                            var idDomain = string.IsNullOrEmpty((string)rs.Object2) ? 0 : int.Parse(((string)rs.Object2).Decrypt());
                                            var domainCreated = dbContext.Domains.FirstOrDefault(p => p.Id == idDomain);
                                            var subAccCreationResponse = subAccountCreationResult.Object as CreateSubAccountResponseModel;
                                            domain.SubAccountCode = subAccCreationResponse?.data?.subaccount_code ?? "";
                                            dbContext.Entry(domain).State = EntityState.Modified;
                                            dbContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //If an error is encountered (i.e. the Domain name is now taken) the request should be forcefully Rejected.
                                request.Status = DomainRequestStatus.Rejected;
                                request.AcceptOrRejectBy = currentUser;
                                request.AcceptOrRejectDate = DateTime.UtcNow;
                                dbContext.Entry(request).State = EntityState.Modified;
                                dbContext.SaveChanges();
                                rs.result = false;
                            }
                        }
                    }
                    else
                    {
                        return new ReturnJsonModel() { result = false, msg = "The status used for Domain Request must be Approve or Reject." };
                    }
                }
                if (rs.result)
                {
                    var notificationObj = new ActivityNotification()
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        CreatedById = userId,
                        DomainRequest = request.DomainRequestJSON.ParseAs<DomainRequest>(),
                        HasActionToHandle = true,
                        ReminderMinutes = 0,
                        EventNotify = Notification.NotificationEventEnum.ProcessDomainRequest,
                        AppendToPageName = Notification.ApplicationPageName.Domain,
                        CreatedByName = currentUser.GetFullName(),
                        Id = requestId
                    };
                    new NotificationRules(dbContext).RaiseDomainRequestProcessNotification(notificationObj);
                }
                return rs;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestId, status, userId, serverPath);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        /// <summary>
        /// Check and get a domain does not finish setup business profile wizard
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isMicro"></param>
        /// <returns></returns>
        public string GetBusinessProfileUnwizard(string domainKey, bool isMicro)
        {
            try
            {
                var domainId = domainKey.Decrypt2Int();
                var domains = dbContext.Domains.AsNoTracking().Where(e => e.Id == domainId);
                if (isMicro)
                    domainKey = domains.FirstOrDefault(e => e.IsBusinessProfileWizardMicro == null || e.IsBusinessProfileWizardMicro == false)?.Key ?? "";
                else
                    domainKey = domains.FirstOrDefault(e => e.IsBusinessProfileWizard == null || e.IsBusinessProfileWizard == false)?.Key ?? "";

                return domainKey;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainKey);
                return null;
            }
        }

        public ReturnJsonModel ReserveDomainRequestName(string requestedName, int requestId, string currentUserId)
        {
            var resultJson = new ReturnJsonModel()
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestedName, requestId, currentUserId);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var domainRequest = new DomainRequest()
                {
                    Name = requestedName,
                    LogoUri = ""
                };

                // Recheck for the availability of the Domain name
                var isDomainNameUsed = IsDuplicateDomainNameCheck(0, requestedName, requestId);
                if (isDomainNameUsed)
                {
                    resultJson.result = false;
                    resultJson.msg = "The domain name has been taken. Please try another one";
                    return resultJson;
                }

                var domainRequestJson = domainRequest.ToJson();
                if (requestId <= 0)
                {
                    var dmRequest = new QbicleDomainRequest()
                    {
                        DomainRequestJSON = domainRequestJson,
                        QbicleRequestJSON = "",
                        Status = DomainRequestStatus.Pending,
                        RequestedName = requestedName,
                        DomainType = DomainTypeEnum.Premium,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow
                    };
                    dbContext.QbicleDomainRequests.Add(dmRequest);
                    dbContext.Entry(dmRequest).State = EntityState.Added;
                    dbContext.SaveChanges();
                    resultJson.result = true;
                    resultJson.Object = dmRequest.Id;
                }
                else
                {
                    var dmRequest = dbContext.QbicleDomainRequests.FirstOrDefault(x => x.Id == requestId);
                    if (dmRequest != null)
                    {
                        dmRequest.RequestedName = requestedName;
                        dmRequest.DomainRequestJSON = domainRequestJson;
                        dmRequest.CreatedDate = DateTime.UtcNow; // Reset the time that the requestedName start to be reserved

                        dbContext.Entry(dmRequest).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        resultJson.result = true;
                        resultJson.Object = requestId;
                    }
                    else
                    {
                        resultJson.result = false;
                        resultJson.msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Domain Request");
                    }
                }

                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestedName, requestId, currentUserId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }

        #region Paystack business rules

        public async Task<ReturnJsonModel> CreatePaystackPlanForDomainRequest(int domainRequestId, int userNumber,
            BusinessDomainLevelEnum level, string currentUserId, string baseUrl)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainRequestId);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                decimal amount = 0;
                var requestedLevel = dbContext.BusinessDomainLevels.FirstOrDefault(p => p.Level == level);
                var dmRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.Id == domainRequestId);

                if (level != BusinessDomainLevelEnum.Free)
                {
                    // Recalculate the plan money amount
                    var extraUserNumber = userNumber - requestedLevel.NumberOfUsers;
                    if (extraUserNumber < 0)
                    {
                        extraUserNumber = 0;
                    }

                    amount = (requestedLevel.Cost ?? 0) + (extraUserNumber * (requestedLevel.CostPerAdditionalUser ?? 0));

                    var requestedPlan = new DomainPlan()
                    {
                        ActualCost = amount,
                        CalculatedCost = amount,
                        NumberOfExtraUsers = extraUserNumber,
                        PayStackPlanCode = "",
                        PayStackPlanCreationResponse = "",
                        PayStackPlanName = dmRequest?.RequestedName ?? "",
                        Level = requestedLevel,
                        IsArchived = false
                    };
                    dbContext.DomainPlans.Add(requestedPlan);
                    dbContext.Entry(requestedPlan).State = EntityState.Added;
                    dbContext.SaveChanges();
                    dmRequest.DomainPlan = requestedPlan;
                    dbContext.Entry(dmRequest).State = EntityState.Modified;

                    var paystackPlanName = dmRequest.RequestedName + "_" + requestedPlan.Id;
                    var planCreationResponse = await new PayStackRules(dbContext).CreatePaystackPlan(paystackPlanName, (int)amount, PaystackConst.PLANINTERVAL);

                    if (planCreationResponse != null && planCreationResponse.status == true)
                    {
                        requestedPlan.PayStackPlanCode = planCreationResponse.data?.plan_code ?? "";
                        requestedPlan.PayStackPlanName = paystackPlanName;
                        requestedPlan.PayStackPlanCreationResponse = JsonHelper.ToJson(planCreationResponse);

                        var transactionInitResponse = await new PayStackRules(dbContext)
                            .InitAuthVerificationTransaction(planCreationResponse.data.plan_code, (requestedLevel.VerifyAmount ?? 0),
                            currentUser.Email, baseUrl, currentUserId, requestedLevel.Currency);

                        if (transactionInitResponse != null && transactionInitResponse.status == true)
                        {
                            requestedPlan.InitTransactionResponseJSON = transactionInitResponse.ToJson();
                            returnJson.result = true;
                            returnJson.Object = transactionInitResponse.data.authorization_url;
                        }
                        else
                        {
                            returnJson.result = false;
                            returnJson.msg = "Transaction initialization failed.";
                            return returnJson;
                        }
                    }
                    else
                    {
                        returnJson.result = false;
                        returnJson.msg = "Plan creation failed.";
                        return returnJson;
                    }
                }
                else
                {
                    var freeDomainPlan = new DomainPlan()
                    {
                        ActualCost = 0,
                        CalculatedCost = 0,
                        NumberOfExtraUsers = 0,
                        PayStackPlanCreationResponse = "",
                        PayStackPlanCode = "",
                        PayStackPlanName = "",
                        InitTransactionResponseJSON = "",
                        Level = requestedLevel,
                        IsArchived = false,
                    };
                    dbContext.DomainPlans.Add(freeDomainPlan);
                    dbContext.Entry(freeDomainPlan).State = EntityState.Added;

                    dmRequest.DomainPlan = freeDomainPlan;
                    dbContext.Entry(dmRequest).State = EntityState.Modified;

                    var freeSubscription = new Subscription()
                    {
                        PayStackAuthorization = "",
                        PayStackEmailCode = "",
                        PayStackSubscriptionCode = "",
                        Plan = freeDomainPlan,
                        Status = DomainSubscriptionStatus.Valid,
                        StartDate = DateTime.UtcNow,
                        IsActive = true,
                        NexPaymentDate = DateTime.UtcNow
                    };
                    dbContext.DomainSubscriptions.Add(freeSubscription);
                    dbContext.Entry(freeSubscription).State = EntityState.Added;

                    returnJson.result = true;
                }
                dbContext.SaveChanges();

                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainRequestId, userNumber);
                returnJson.result = false;
                returnJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return returnJson;
            }
        }

        public string GetTransactionReferenceByPlanCode(string planCode, string userEmail)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, planCode, userEmail);

                var domainCreationPlan = dbContext.DomainPlans.FirstOrDefault(p => p.PayStackPlanCode == planCode);
                var initTransactionResponse = domainCreationPlan.InitTransactionResponseJSON.ParseAs<TransactionResponseModel>();
                return initTransactionResponse?.data?.reference ?? "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, planCode, userEmail);
                return "";
            }
        }

        public string GetTransactionRefByDomainPlan(int planId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, planId);

                var domainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Id == planId);

                if (domainPlan == null || string.IsNullOrEmpty(domainPlan.InitTransactionResponseJSON))
                    return "";

                var initTransactionResponse = domainPlan.InitTransactionResponseJSON.ParseAs<TransactionResponseModel>();
                return initTransactionResponse?.data?.reference ?? "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, planId);
                return "";
            }
        }

        public async Task<PayStackVerifyTransactionResponseModel> VerifyDomainCreationTransaction(string transactionReference)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, transactionReference);

                var response = await new PayStackRules(dbContext).VerifyPaystackTransaction(transactionReference);

                return response;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, transactionReference);
                return null;
            }
        }

        public async Task<ReturnJsonModel> CreatePayStackRefund(string transactionReference)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, transactionReference);

                var paystackRefundCreationResponse = await new PayStackRules(dbContext).InitRefund(transactionReference);

                return new ReturnJsonModel()
                {
                    result = true,
                    Object = paystackRefundCreationResponse
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, transactionReference);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public async Task<ReturnJsonModel> CreatePayStackSubscription(PayStackAuthorizationModel authorizationModel,
            PayStackCustomerModel customer, string planCode)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, authorizationModel, customer, planCode);

                var planObj = dbContext.DomainPlans.FirstOrDefault(p => p.PayStackPlanCode == planCode);
                if (planObj == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find any domain plan with the plan code"
                    };
                }
                var trialDays = planObj.Level.NumberOfFreeTrialDays;

                var subscriptionStartDate = DateTime.UtcNow.AddDays(trialDays ?? 0);
                var subscriptionCreationResponse = await new PayStackRules(dbContext)
                    .CreateSubscription(customer.customer_code, planCode, authorizationModel.authorization_code, subscriptionStartDate);

                var subscriptionObj = new Subscription();

                if (subscriptionCreationResponse.status == true && subscriptionCreationResponse.data != null)
                {
                    subscriptionObj = new Subscription()
                    {
                        PayStackAuthorization = JsonHelper.ToJson(authorizationModel),
                        PayStackEmailCode = subscriptionCreationResponse.data.email_token,
                        PayStackSubscriptionCode = subscriptionCreationResponse.data.subscription_code,
                        Plan = planObj,
                        StartDate = subscriptionStartDate,
                        Status = DomainSubscriptionStatus.Valid,
                        IsActive = true
                    };

                    dbContext.DomainSubscriptions.Add(subscriptionObj);
                    dbContext.Entry(subscriptionObj).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                else
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Paystack Subscription Creation failed."
                    };
                }

                return new ReturnJsonModel()
                {
                    result = true,
                    Object = subscriptionCreationResponse,
                    Object2 = subscriptionObj
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, authorizationModel, customer, planCode);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            };
        }

        public async Task<ReturnJsonModel> UpdateSlotNumberOfPayStackPlan(int currentDomainId, int newTotalSlotNumber, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, currentDomainId, newTotalSlotNumber);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomainId && p.IsArchived == false);
                if (currentDomainPlan == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find current Domain Plan"
                    };
                }

                var currentDomainRequet = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.DomainPlan.Id == currentDomainPlan.Id);
                var currentDomain = currentDomainPlan.Domain;
                var currentDomainMemberNumber = currentDomain.Users.Count();
                var planCode = currentDomainPlan.PayStackPlanCode;
                var planName = currentDomainPlan.PayStackPlanName;
                var planInterval = PaystackConst.PLANINTERVAL;
                var subscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == currentDomainPlan.Id);
                var subscriptionCode = subscription.PayStackSubscriptionCode;
                var emailToken = subscription.PayStackEmailCode;
                var paystackRules = new PayStackRules(dbContext);

                // Validate the constrains
                // The new total slot MUST >= current number of the Domain member
                if (newTotalSlotNumber < currentDomainMemberNumber)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Total number number of users must not be less than the current number of domain member."
                    };
                }

                // Recalculate the amount needed to be paid monthly for new slot number
                var numberOfExtraUser = newTotalSlotNumber < currentDomainPlan.Level.NumberOfUsers ? 0 : newTotalSlotNumber - currentDomainPlan.Level.NumberOfUsers;
                var amount = (currentDomainPlan.Level.Cost ?? 0) + numberOfExtraUser * (currentDomainPlan.Level.CostPerAdditionalUser ?? 0);

                // Create a new PayStack Plan
                var newDomainPlan = new DomainPlan()
                {
                    Domain = currentDomainPlan.Domain,
                    ActualCost = amount,
                    CalculatedCost = amount,
                    InitTransactionResponseJSON = "",
                    Level = currentDomainPlan.Level,
                    NumberOfExtraUsers = numberOfExtraUser,
                    PayStackPlanName = currentDomainPlan.PayStackPlanName,
                    PayStackPlanCode = "",
                    PayStackPlanCreationResponse = "",
                    IsArchived = false
                };
                dbContext.DomainPlans.Add(newDomainPlan);
                dbContext.Entry(newDomainPlan).State = EntityState.Added;
                dbContext.SaveChanges();

                var newPaystackPlanName = currentDomain.Name + "_" + newDomainPlan.Id;
                var planCreationResponse = await paystackRules.CreatePaystackPlan(newPaystackPlanName, (int)amount, planInterval);
                var currentSubscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == currentDomainPlan.Id);
                var currentPaystackCustomer = dbContext.PaystackCustomers.FirstOrDefault(p => p.Subscriptions.Any(x => x.Id == currentSubscription.Id));

                if (planCreationResponse != null && planCreationResponse.status == true)
                {
                    // Update domain plan
                    newDomainPlan.PayStackPlanCode = planCreationResponse.data.plan_code;
                    newDomainPlan.PayStackPlanCreationResponse = planCreationResponse.ToJson();
                    dbContext.Entry(newDomainPlan).State = EntityState.Modified;

                    // Data will be reused from subscription
                    var currentSubOnPaystack = await paystackRules.GetPaystackSubscription(currentSubscription.PayStackSubscriptionCode);
                    var nextPaidDate = currentSubOnPaystack.next_payment_date;

                    // Create Subscription to the new domain plan
                    // The first payment date for the new subscription will be the next payment date of the current subscription
                    var authorizationObj = currentSubscription.PayStackAuthorization.ParseAs<PayStackAuthorizationModel>();
                    var subscriptionCreationResponse = await paystackRules
                        .CreateSubscription(currentPaystackCustomer.CustomerCode, newDomainPlan.PayStackPlanCode,
                                            authorizationObj.authorization_code, nextPaidDate);
                    if (subscriptionCreationResponse == null || subscriptionCreationResponse.status == false)
                    {
                        return new ReturnJsonModel()
                        {
                            result = false,
                            msg = "Create Subscription to new domain plan fails."
                        };
                    }

                    // Archieve the plan
                    currentDomainPlan.IsArchived = true;
                    currentDomainPlan.ArchivedDate = DateTime.UtcNow;
                    currentDomainPlan.ArchivedBy = currentUser;
                    dbContext.Entry(currentDomainPlan).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    // Disable the current subscription
                    var disableSubscriptionResponse = await paystackRules.DisableSubscription(subscriptionCode, emailToken);
                    if (disableSubscriptionResponse == null || disableSubscriptionResponse.status == false)
                    {
                        return new ReturnJsonModel()
                        {
                            result = false,
                            msg = "Disable subscription fails"
                        };
                    }
                    currentSubscription.IsActive = false;
                    dbContext.Entry(currentSubscription).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    // Create new Subscription
                    var newSub = new Subscription()
                    {
                        PayStackAuthorization = currentSubscription.PayStackAuthorization,
                        PayStackEmailCode = subscriptionCreationResponse.data.email_token,
                        PayStackSubscriptionCode = subscriptionCreationResponse.data.subscription_code,
                        Plan = newDomainPlan,
                        StartDate = currentSubscription.StartDate.AddMonths(1),
                        Status = DomainSubscriptionStatus.Valid,
                        IsActive = true
                    };

                    dbContext.DomainSubscriptions.Add(newSub);
                    dbContext.Entry(newSub).State = EntityState.Added;
                    dbContext.SaveChanges();

                    // Add new subscription to the paystack Customer
                    if (currentPaystackCustomer != null)
                    {
                        currentPaystackCustomer.Subscriptions.Add(newSub);
                        dbContext.Entry(currentPaystackCustomer).State = EntityState.Modified;
                    }

                    dbContext.SaveChanges();
                }
                else
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Create new Domain plan fails. Please contact to the administrator!"
                    };
                }

                return new ReturnJsonModel()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId, newTotalSlotNumber);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            }
        }

        #endregion Paystack business rules

        public ReturnJsonModel SaveDomainSpecifics(string description, string uploadKey, int requestId, string currentUserId, string serverMapPath)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, description, uploadKey, requestId);
                var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.Id == requestId);
                if (domainRequest != null)
                {
                    var domainRequestObj = domainRequest.DomainRequestJSON.ParseAs<DomainRequest>();
                    domainRequestObj.Description = description;
                    if (!string.IsNullOrEmpty(uploadKey))
                    {
                        domainRequestObj.LogoUri = uploadKey;
                    }
                    domainRequest.DomainRequestJSON = domainRequestObj.ToJson();
                    dbContext.Entry(domainRequest).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    // Place for updating the description of the Domain - Not applied yet

                    // Add info for the first Qbicle of the Domain
                    var qbicleRequest = new InitialQbicleRequest()
                    {
                        Description = "This is the default Qbicle for the Domain",
                        Name = "Main Qbicle",
                        LogoUri = ""
                    };
                    domainRequest.QbicleRequestJSON = qbicleRequest.ToJson();
                    dbContext.SaveChanges();
                    returnJson.result = true;
                    return returnJson;
                }
                else
                {
                    returnJson.result = false;
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Domain Request");
                    return returnJson;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, description, uploadKey, requestId);
                returnJson.result = false;
                returnJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return returnJson;
            }
        }

        public ReturnJsonModel ValidateChangingPlanLevel(int domainId, int newDomainLevelId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, newDomainLevelId);

                var domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
                if (domain == null)
                {
                    return new ReturnJsonModel
                    {
                        result = false,
                        msg = "Cannot find the domain"
                    };
                }
                var newDomainLevel = dbContext.BusinessDomainLevels.FirstOrDefault(p => p.Id == newDomainLevelId);
                if (newDomainLevel == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find the new business domain level to update"
                    };
                }

                // Validation

                //Check type changing - Downgrade or Upgrade
                var isDowngrading = true;
                var domainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
                if (domainPlan == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find the associated domain plan"
                    };
                }
                var currentDomainLevel = domainPlan.Level.Level;
                if (newDomainLevel.Level > currentDomainLevel)
                    isDowngrading = false;

                // No need to validate the Upgrade case
                if (!isDowngrading)
                {
                    return new ReturnJsonModel()
                    {
                        result = true,
                        msg = "",
                        Object = true
                    };
                }

                // Downgrading
                // The number of members of the Domain - DomainMemberCount
                var domainMemberCount = domain.Users.Count();

                //Number of extra slots in current plan (Domain.DomainPlan.NumberOfExtraUsers) - NumExtraSlots
                var numExtraSlots = domainPlan.NumberOfExtraUsers;

                // Number of slots available in the plan that the user has selected to downgrade to (BusinessDomainLevel.NumberOfUsers) - DownPlanUserSlots
                var downplanUserSlots = newDomainLevel.NumberOfUsers;

                // The current domain member number must NOT be greater than the user number of the new domain level
                if (domainMemberCount > numExtraSlots + downplanUserSlots)
                {
                    return new ReturnJsonModel()
                    {
                        result = true,
                        msg = "",
                        Object = false //is validation passed
                    };
                }
                else
                {
                    return new ReturnJsonModel()
                    {
                        result = true,
                        msg = "",
                        Object = true // is validation passed
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, newDomainLevelId);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="newDomainLevelId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="baseUrl"></param>
        /// <param name="overwritedCustomerId">On transaction callback, a new customer that holds paystack authorization data is created, this will overwrite the current one</param>
        /// <returns></returns>
        public async Task<ReturnJsonModel> ChangeDomainPlanLevel(int domainId, int newDomainLevelId, string currentUserId,
            string baseUrl, int overwritedCustomerId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, newDomainLevelId, currentUserId, baseUrl);

                var paystackRules = new PayStackRules(dbContext);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);

                var domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
                if (domain == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find the domain"
                    };
                }
                var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
                if (currentDomainPlan == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find the current domain plan"
                    };
                }
                var newDomainLevel = dbContext.BusinessDomainLevels.FirstOrDefault(p => p.Id == newDomainLevelId);
                if (newDomainLevel == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find the new business domain level to update"
                    };
                }
                var currentSubscription = dbContext.DomainSubscriptions
                    .FirstOrDefault(p => p.Plan.Id == currentDomainPlan.Id && p.IsActive == true);
                if (currentSubscription != null && !string.IsNullOrEmpty(currentSubscription.PayStackSubscriptionCode))
                {
                    var currentSubOnPaystack = await paystackRules.GetPaystackSubscription(currentSubscription.PayStackSubscriptionCode);
                }

                var currentAssocitedPaystackCustomer = dbContext.PaystackCustomers
                    .FirstOrDefault(p => p.Subscriptions.Any(x => x.Id == currentSubscription.Id));
                if (overwritedCustomerId > 0)
                {
                    currentAssocitedPaystackCustomer = dbContext.PaystackCustomers.FirstOrDefault(p => p.Id == overwritedCustomerId);
                }
                if (currentAssocitedPaystackCustomer == null && overwritedCustomerId > 0)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find the associated Paystack Authorization information"
                    };
                }

                // Free plan cannot be created on Paystack

                if (newDomainLevel.Level != BusinessDomainLevelEnum.Free)
                {
                    // OverwriteCustomerId > 0 means a new customer that holds new Paystack Authorization data is created
                    if (currentDomainPlan.Level.Level == BusinessDomainLevelEnum.Free && overwritedCustomerId <= 0)
                    {
                        var transactionAmount = newDomainLevel.VerifyAmount ?? 0;
                        var callbackUrl = baseUrl + $"/Paystack/UpdateDomainPlanLevelTransactionCallback?domainId={domainId}&newDomainLevelId={newDomainLevelId}";
                        var cancelUrl = baseUrl + "/Administration/UpdateSubscriptionDetail";
                        string[] transactionChannels = { "card" };

                        var transactionResponse = await paystackRules
                            .InitGettingAuthorizationTransaction(transactionAmount, callbackUrl, cancelUrl, currentUser.Email, transactionChannels, newDomainLevel.Currency);
                        if (transactionResponse == null && transactionResponse.status != true)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = "Transaction initalization to get authorization data fails"
                            };
                        }

                        currentDomainPlan.InitTransactionResponseJSON = transactionResponse.ToJson();
                        dbContext.Entry(currentDomainPlan).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        // Return result
                        return new ReturnJsonModel()
                        {
                            result = true,
                            msg = "",
                            Object = new
                            {
                                RedirectionNeeded = true,
                                RedirectionUrl = transactionResponse.data.authorization_url
                            }
                        };
                    }
                    else
                    {
                        // 1. Validation
                        // Have been done in another separate method

                        // 2. Create new plan with new BusinessDomainLevel
                        // Recaculate the amount needed to be paid for the plan
                        var currentExtraUserNumber = currentDomainPlan?.NumberOfExtraUsers ?? 0;
                        var newDomainPlanAmountMoney = (newDomainLevel.Cost ?? 0) + currentExtraUserNumber * (newDomainLevel.CostPerAdditionalUser ?? 0);
                        var newDomainPlan = new DomainPlan()
                        {
                            ActualCost = newDomainPlanAmountMoney,
                            CalculatedCost = newDomainLevel.Cost ?? 0,
                            NumberOfExtraUsers = currentExtraUserNumber,
                            PayStackPlanCode = "",
                            PayStackPlanCreationResponse = "",
                            PayStackPlanName = "",
                            Level = newDomainLevel,
                            IsArchived = false,
                            Domain = domain
                        };
                        dbContext.DomainPlans.Add(newDomainPlan);
                        dbContext.Entry(newDomainPlan).State = EntityState.Added;
                        dbContext.SaveChanges();

                        var paystackPlanName = currentDomainPlan.Domain.Name + "_" + newDomainPlan.Id;
                        var planCreationResponse = await new PayStackRules(dbContext).CreatePaystackPlan(paystackPlanName, (int)newDomainLevel.Cost, PaystackConst.PLANINTERVAL);

                        if (planCreationResponse != null && planCreationResponse.status == true)
                        {
                            // 3. If the customer DOES have authorization information, create Subscription to the new domain plan

                            // Update domain plan
                            newDomainPlan.PayStackPlanCode = planCreationResponse.data.plan_code;
                            newDomainPlan.PayStackPlanCreationResponse = planCreationResponse.ToJson();
                            dbContext.Entry(newDomainPlan).State = EntityState.Modified;

                            // If the domain does not have trial time started yet, need to involve trial time in the subscription
                            // Else, next payment date will be the same as the next payment date of current subscription
                            var nextPaidDate = currentSubscription?.NexPaymentDate ?? DateTime.UtcNow;
                            var isTrialTimeApplied = false;
                            if (!domain.IsTrialTimeStarted)
                            {
                                nextPaidDate = nextPaidDate.AddDays(newDomainLevel.NumberOfFreeTrialDays ?? 0);

                                domain.IsTrialTimeStarted = true;
                                dbContext.Entry(domain).State = EntityState.Modified;

                                isTrialTimeApplied = true;
                            }

                            // Create Subscription to the new domain plan
                            var subscriptionCreationResponse = await paystackRules
                                .CreateSubscription(currentAssocitedPaystackCustomer.CustomerCode, newDomainPlan.PayStackPlanCode,
                                                    "", nextPaidDate);
                            if (subscriptionCreationResponse == null || subscriptionCreationResponse.status == false)
                            {
                                return new ReturnJsonModel()
                                {
                                    result = false,
                                    msg = "Create Subscription to new domain plan fails."
                                };
                            }

                            // 4. Archieve the plan
                            currentDomainPlan.IsArchived = true;
                            currentDomainPlan.ArchivedDate = DateTime.UtcNow;
                            currentDomainPlan.ArchivedBy = currentUser;
                            dbContext.Entry(currentDomainPlan).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            // 5. De-active the subscription on Paystack
                            if (!string.IsNullOrEmpty(currentSubscription.PayStackSubscriptionCode) && !string.IsNullOrEmpty(currentSubscription.PayStackEmailCode))
                            {
                                var disableSubscriptionResponse = await paystackRules
                                .DisableSubscription(currentSubscription.PayStackSubscriptionCode, currentSubscription.PayStackEmailCode);
                                if (disableSubscriptionResponse == null || disableSubscriptionResponse.status == false)
                                {
                                    return new ReturnJsonModel()
                                    {
                                        result = false,
                                        msg = "Disable subscription fails"
                                    };
                                }
                            }
                            currentSubscription.IsActive = false;
                            dbContext.Entry(currentSubscription).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            // 6. Create new Subscription on the System
                            var newSub = new Subscription()
                            {
                                PayStackAuthorization = currentSubscription.PayStackAuthorization,
                                PayStackEmailCode = subscriptionCreationResponse.data.email_token,
                                PayStackSubscriptionCode = subscriptionCreationResponse.data.subscription_code,
                                Plan = newDomainPlan,
                                StartDate = currentSubscription.StartDate.AddMonths(1),
                                Status = DomainSubscriptionStatus.Valid,
                                IsActive = true
                            };

                            dbContext.DomainSubscriptions.Add(newSub);
                            dbContext.Entry(newSub).State = EntityState.Added;
                            dbContext.SaveChanges();

                            currentAssocitedPaystackCustomer.Subscriptions.Add(newSub);
                            dbContext.Entry(currentAssocitedPaystackCustomer).State = EntityState.Modified;

                            // Add new subscription to the paystack Customer
                            currentAssocitedPaystackCustomer.Subscriptions.Add(newSub);
                            dbContext.Entry(currentAssocitedPaystackCustomer).State = EntityState.Modified;

                            dbContext.SaveChanges();

                            if (newDomainPlan.Level.Level > BusinessDomainLevelEnum.Free)
                            {
                                // Remove notification Hangfire job for trial end
                                new NotificationRules(dbContext).CancelHangfireJob(currentDomainPlan.TrialEndNotiHangfireJobId);

                                // Remove notify the next payment day
                                new NotificationRules(dbContext).CancelHangfireRecurringJob(currentDomainPlan.SubPaymnetDateNotiHangfireJobId);

                                // Notification for trial expiring date
                                if (isTrialTimeApplied)
                                {
                                    // Scheduling the trial expiring notification
                                    // Create Notification
                                    var trialTimeEndingNotification = new ActivityNotification
                                    {
                                        OriginatingConnectionId = "",
                                        DomainId = domain.Id,
                                        QbicleId = 0,
                                        AppendToPageName = ApplicationPageName.Domain,
                                        EventNotify = NotificationEventEnum.DomainSubTrialEnd,
                                        CreatedByName = HelperClass.GetFullName(currentUser),
                                        ObjectById = newDomainPlan.Id.ToString(),
                                        ReminderMinutes = 0,
                                        CreatedById = currentUser.Id,
                                        Id = newSub.Id
                                    };
                                    new NotificationRules(dbContext).NotifyDomainAdminOnTrialTimeEnd(trialTimeEndingNotification);
                                }

                                // Notify the next payment day
                                // Scheduling the payment date notification
                                // Create Notification
                                var paymentDateReminderNotification = new ActivityNotification
                                {
                                    OriginatingConnectionId = "",
                                    DomainId = domain.Id,
                                    QbicleId = 0,
                                    AppendToPageName = ApplicationPageName.Domain,
                                    EventNotify = NotificationEventEnum.DomainSubNextPaymentDate,
                                    CreatedByName = HelperClass.GetFullName(currentUser),
                                    ObjectById = newDomainPlan.Id.ToString(),
                                    ReminderMinutes = 0,
                                    CreatedById = currentUser.Id,
                                    Id = newSub.Id
                                };
                                new NotificationRules(dbContext).NotifyDomainAdminOnPaymentDate(paymentDateReminderNotification);
                            }
                        }
                        else
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = "Create new Domain plan fails. Please contact to the administrator!"
                            };
                        }
                    }
                }
                else // For Free Plan Level
                {
                    var freeDomainPlan = new DomainPlan()
                    {
                        ActualCost = 0,
                        CalculatedCost = 0,
                        NumberOfExtraUsers = 0,
                        PayStackPlanCreationResponse = "",
                        PayStackPlanCode = "",
                        PayStackPlanName = "",
                        InitTransactionResponseJSON = "",
                        Level = newDomainLevel,
                        IsArchived = false,
                        Domain = domain,
                    };
                    dbContext.DomainPlans.Add(freeDomainPlan);
                    dbContext.Entry(freeDomainPlan).State = EntityState.Added;

                    var freeSubscription = new Subscription()
                    {
                        PayStackAuthorization = currentSubscription.PayStackAuthorization,
                        PayStackEmailCode = currentSubscription.PayStackEmailCode,
                        PayStackSubscriptionCode = "",
                        Plan = freeDomainPlan,
                        Status = DomainSubscriptionStatus.Valid,
                        StartDate = DateTime.UtcNow,
                        IsActive = true,
                        NexPaymentDate = currentSubscription?.NexPaymentDate ?? DateTime.UtcNow,
                    };
                    dbContext.DomainSubscriptions.Add(freeSubscription);
                    dbContext.Entry(freeSubscription).State = EntityState.Added;
                    dbContext.SaveChanges();

                    currentAssocitedPaystackCustomer.Subscriptions.Add(freeSubscription);
                    dbContext.Entry(currentAssocitedPaystackCustomer).State = EntityState.Modified;

                    // 4. Archieve the plan
                    currentDomainPlan.IsArchived = true;
                    currentDomainPlan.ArchivedDate = DateTime.UtcNow;
                    currentDomainPlan.ArchivedBy = currentUser;
                    dbContext.Entry(currentDomainPlan).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    // 5. De-active the subscription on Paystack
                    if (!string.IsNullOrEmpty(currentSubscription.PayStackSubscriptionCode) && !string.IsNullOrEmpty(currentSubscription.PayStackEmailCode))
                    {
                        var disableSubscriptionResponse = await paystackRules
                        .DisableSubscription(currentSubscription.PayStackSubscriptionCode, currentSubscription.PayStackEmailCode);
                        if (disableSubscriptionResponse == null || disableSubscriptionResponse.status == false)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = "Disable subscription fails"
                            };
                        }
                    }

                    currentSubscription.IsActive = false;
                    dbContext.Entry(currentSubscription).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    // Remove notification Hangfire job for trial end
                    new NotificationRules(dbContext).CancelHangfireJob(currentDomainPlan.TrialEndNotiHangfireJobId);

                    // Remove notify the next payment day
                    new NotificationRules(dbContext).CancelHangfireRecurringJob(currentDomainPlan.SubPaymnetDateNotiHangfireJobId);
                }

                // Return result
                return new ReturnJsonModel()
                {
                    result = true,
                    msg = "",
                    Object = new
                    {
                        RedirectionNeeded = false,
                        RedirectionUrl = ""
                    }
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, newDomainLevelId, currentUserId, baseUrl);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            }
        }

        public ReturnJsonModel SaveSubAccountInfoToDomainRequest(string businessName, string bankCode, string accountNumber, int domainRequestId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, businessName, bankCode, accountNumber, domainRequestId);

                var domainRequest = dbContext.QbicleDomainRequests.FirstOrDefault(p => p.Id == domainRequestId);
                if (domainRequest == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find the associated domain request"
                    };
                }

                var subAccInforModel = new SubAccountInformationModel()
                {
                    AccountNumber = accountNumber,
                    BankCode = bankCode,
                    BusinessName = businessName
                };

                domainRequest.SubAccountInformationJSON = subAccInforModel.ToJson();
                dbContext.Entry(domainRequest).State = EntityState.Modified;
                dbContext.SaveChanges();

                return new ReturnJsonModel()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, businessName, bankCode, accountNumber, domainRequestId);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            }
        }

        public async Task<ReturnJsonModel> UpdatePaystackSubAccount(int currentDomainId, string businessName, string bankCode, string accountNumber, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, currentDomainId, businessName, bankCode, accountNumber, currentUserId);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var currentDomain = dbContext.Domains.FirstOrDefault(p => p.Id == currentDomainId);
                var accountCode = currentDomain?.SubAccountCode ?? "";
                var b2cOrderChargeSettings = dbContext.B2COrderPaymentCharges.FirstOrDefault();

                var returnResult = new ReturnJsonModel() { result = false };
                if (!string.IsNullOrEmpty(accountCode))
                {
                    var updateSubAccResult = await new PayStackRules(dbContext).UpdateSubAccountInformation(accountCode,
                    businessName, bankCode, accountNumber, b2cOrderChargeSettings.QbiclesPercentageCharge);
                    returnResult = updateSubAccResult;
                }
                else
                {
                    var newSubAccResult = await new PayStackRules(dbContext)
                        .CreateSubAccount(businessName, bankCode, accountNumber, b2cOrderChargeSettings.QbiclesPercentageCharge);

                    if (newSubAccResult != null && newSubAccResult.result == true)
                    {
                        var subAccCreationData = (CreateSubAccountResponseModel)newSubAccResult.Object;
                        currentDomain.SubAccountCode = subAccCreationData?.data?.subaccount_code ?? "";
                        dbContext.Entry(currentDomain).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                    returnResult = newSubAccResult;
                }
                // If no associated Paystack account is available, create a new one
                // 1. Check associated Paystack account availability
                var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomainId && p.IsArchived == false);
                var currentDomainPlanId = currentDomainPlan?.Id ?? 0;
                var currentDomainSubscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == currentDomainPlanId);
                var currentDomainSubscriptionId = currentDomainSubscription?.Id ?? 0;
                var paystackCustomer = dbContext.PaystackCustomers.FirstOrDefault(p => p.Subscriptions.Any(x => x.Id == currentDomainSubscriptionId));

                // 2. Create a new one if no account exists
                if (paystackCustomer == null)
                {
                    var paystackCustomerCreationResult = await new PayStackRules(dbContext).CreatePaystackCustomerAccount(currentUser.Email, currentUser.Forename, currentUser.Surname);
                    var customerCreationResponse = (CustomerAccountCreationResponseModel)paystackCustomerCreationResult.Object;
                    if (paystackCustomerCreationResult == null || !paystackCustomerCreationResult.result || paystackCustomerCreationResult.Object == null || !customerCreationResponse.status)
                    {
                        return new ReturnJsonModel()
                        {
                            result = false,
                            msg = "Create new paystack customer fails. This will be needed for B2C Payment"
                        };
                    }
                    var creationData = customerCreationResponse.data;

                    // If no current subscription - create a new one
                    if (currentDomainSubscription == null)
                    {
                        var blankSubscription = new Subscription()
                        {
                            IsActive = true,
                            Plan = currentDomainPlan,
                            Status = DomainSubscriptionStatus.Valid
                        };
                        dbContext.DomainSubscriptions.Add(blankSubscription);
                        dbContext.Entry(blankSubscription).State = EntityState.Added;
                        dbContext.SaveChanges();

                        var paystackCustomerAccount = new Models.Customer()
                        {
                            CustomerCode = creationData.customer_code,
                            PaystackCustomerId = creationData.id,
                            PaystackEmail = creationData.email,
                            PaystackFirstName = currentUser.Forename,
                            PaystackLastName = currentUser.Surname,
                            User = currentUser,
                            Subscriptions = new List<Subscription>() { blankSubscription }
                        };
                        dbContext.PaystackCustomers.Add(paystackCustomerAccount);
                        dbContext.Entry(paystackCustomerAccount).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }
                }

                return returnResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId, businessName, bankCode, accountNumber, currentUserId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public async Task<ReturnJsonModel> ProcessCustomDomainCreate(string userId, string serverPath, DomainRequest domainRequest, string originatingConnectionId = "")
        {
            var rs = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, serverPath);

                var currentUser = dbContext.QbicleUser.Find(userId);

                var domain = new QbicleDomain
                {
                    Name = domainRequest.Name,
                    LogoUri = domainRequest.LogoUri,
                    DomainType = DomainTypeEnum.Business
                };

                rs = await UpdateOrInsertDomain(domain, currentUser.Id, serverPath, null, 0, true);

                var profile = new B2BProfile
                {
                    BusinessName = domainRequest.Name,
                    BusinessSummary = domainRequest?.Description ?? domainRequest.Name,
                    BusinessEmail = currentUser.Email,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedBy = currentUser,
                    LastUpdatedDate = DateTime.UtcNow,
                    Domain = domain
                };

                dbContext.Entry(profile).State = EntityState.Added;

                dbContext.SaveChanges();

                var domainCustom = (QbicleDomain)rs.Object;
                rs.Object = null;
                var domainHtml = "<li>";
                domainHtml += $"<a href='#' onclick = \"DomainSelected('{domain.Key}')\" >";
                domainHtml += $"<div class='mdv2-activity dash' style='padding: 20px;'>";
                if (domain.Administrators?.Any(x => x.Id == userId) ?? false)
                    domainHtml += $"<span class='label label-lg label-info'>Admin</span>";
                if (domain.QbicleManagers?.Any(x => x.Id == userId) ?? false)
                    domainHtml += $"<br /><span class='label label-lg label-info'>Manager</span>";
                domainHtml += $"<div class='flex-avatar'>";
                domainHtml += $"<div class='col-circleimg'>";
                var domainImg = domain.LogoUri == null ? "/Content/DesignStyle/img/icon_domain_default.png" : domain.LogoUri.ToUriString(Enums.FileTypeEnum.Image, "T");
                domainHtml += $"<div class=\"image\" style=\"background-image: url('{domainImg}');\">&nbsp;</div>";
                domainHtml += $"</div>";
                domainHtml += $"<div class='col'>";
                domainHtml += $"<h2 style='margin: 5px 0 0 0;'>{domain.Name}</h2>";
                domainHtml += $"<ul class='breadcrumb' style='margin: 2px 0 0 0;'>";
                var createdDate = domain.CreatedDate.ConvertTimeFromUtc(currentUser.Timezone).ToOrdinalString(currentUser.DateFormat);
                domainHtml += $"<li style='color: #acabbd;'>Created on {createdDate} </li>";
                domainHtml += $"</ul></div></div></div></a></li>";
                rs.msg = domainHtml;

                rs.msgId = domain.Key;

                return rs;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, serverPath);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }
    }
}