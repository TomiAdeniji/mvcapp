using CleanBooksData;
using DocumentFormat.OpenXml.Wordprocessing;
using MySqlX.XDevAPI.Common;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Community;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Qbicles.BusinessRules.Commerce
{
    public class CommerceRules
    {
        ApplicationDbContext dbContext;
        public CommerceRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public B2BProfile GetB2bProfileById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                return dbContext.B2BProfiles.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new B2BProfile();
            }
        }

        public B2BProfile GetB2bProfileByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId);
                return dbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == domainId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new B2BProfile();
            }
        }

        public PaginationResponse GetBusinesses(FindBusinesesRequest request)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, request);
                var query = dbContext.B2BProfiles.Where(s => s.IsDisplayedInB2BListings && s.Domain.Id != request.currentDomainId);
                if (!string.IsNullOrEmpty(request.keyword))
                {
                    query = query.Where(s => s.BusinessName.ToLower().Contains(request.keyword) || s.BusinessSummary.ToLower().Contains(request.keyword));
                }
                if (request.services != null && request.services.Any())
                {
                    var isLogistic = false;
                    var isDesign = false;
                    var isMaintenance = false;
                    foreach (var item in request.services)
                    {
                        switch (item)
                        {
                            case B2bServicesConst.Logistics:
                                isLogistic = true;
                                break;
                            case B2bServicesConst.Design:
                                isDesign = true;
                                break;
                            case B2bServicesConst.Maintenance:
                                isMaintenance = true;
                                break;
                            default:
                                break;
                        }
                    }
                    query = from p in query
                            join c in dbContext.B2BSettings on p.Domain.Id equals c.Location.Id
                            where (c.IsLogistics == isLogistic
                            || c.IsDesign == isDesign
                            || c.IsMaintenance == isMaintenance)
                            select p;
                }
                //remove all business is connected
                var relationships = dbContext.B2BRelationships.Where(s => s.Domain1.Id == request.currentDomainId || s.Domain2.Id == request.currentDomainId);
                query = query.Where(s => !relationships.Any(r => r.Domain1.Id == s.Domain.Id || r.Domain2.Id == s.Domain.Id));
                //end
                response.totalNumber = query.Count();
                response.items = query.OrderBy(s => s.BusinessName)
                    .Skip((request.pageNumber - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(s => new { s.Id, DomainId = s.Domain.Id, s.LogoUri, s.BusinessName, s.BusinessSummary, HasB2BDefaultManager = s.DefaultB2BRelationshipManagers.Any() }).ToList();
                return response;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                return response;
            }
        }
        public ReturnJsonModel SaveProfile(B2bProfileModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, model);
                var isDomainAdmin = model.Domain.Administrators.Any(p => p.Id == userId);
                if (!isDomainAdmin /*&& !new B2BWorkgroupRules(dbContext).GetCheckPermission(model.Domain.Id, model.CurrentUser.Id, B2bProcessesConst.ProfileEditing)*/)
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                    return returnJson;
                }
                // The email address for a business must be one of the email addresses associated with a member of the Domain
                var domainMemEmails = model.Domain.Users == null ? new List<string>() : model.Domain.Users.Select(t => t.Email).Distinct().ToList();
                if (!domainMemEmails.Contains(model.BusinessEmail))
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_BUSINESSEMAIL_NOT_IN_MEM_EMAIL");
                    return returnJson;
                }

                if (!string.IsNullOrEmpty(model.LogoUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(model.LogoUri);
                }
                if (!string.IsNullOrEmpty(model.BannerUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(model.BannerUri);
                }

                var currentUser = dbContext.QbicleUser.Find(userId);

                var profile = dbContext.B2BProfiles.FirstOrDefault(s => s.Id == model.Id || s.Domain.Id == model.Domain.Id);
                if (profile != null)
                {
                    profile.BusinessName = model.BusinessName;
                    profile.BusinessSummary = model.BusinessSummary;
                    profile.BusinessEmail = model.BusinessEmail;
                    //it seems these attributes get false in default cause no content for them in this model. These attributes could use updateDefaultManagers() controller to update.
                    //these atributes will use in this controller when profile is null
                    //profile.IsDisplayedInB2BListings = model.IsDisplayedInB2BListings;
                    //profile.IsDisplayedInB2CListings = model.IsDisplayedInB2CListings;
                    profile.DefaultB2BRelationshipManagers.Clear();
                    if (model.UserIdB2BRelationshipManagers != null)
                    {
                        foreach (var item in model.UserIdB2BRelationshipManagers)
                        {
                            var user = dbContext.QbicleUser.Find(item);
                            if (user != null)
                                profile.DefaultB2BRelationshipManagers.Add(user);
                        }
                    }
                    profile.DefaultB2CRelationshipManagers.Clear();
                    if (model.UserIdB2CRelationshipManagers != null)
                    {
                        foreach (var item in model.UserIdB2CRelationshipManagers)
                        {
                            var user = dbContext.QbicleUser.Find(item);
                            if (user != null)
                                profile.DefaultB2CRelationshipManagers.Add(user);
                        }
                    }
                    profile.LastUpdatedBy = currentUser;
                    profile.LastUpdatedDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(model.LogoUri))
                        profile.LogoUri = model.LogoUri;
                    if (!string.IsNullOrEmpty(model.BannerUri))
                        profile.BannerUri = model.BannerUri;
                    profile.AreasOperation.Clear();

                    var areasOperations = dbContext.B2BAreasOfOperation.Where(e => e.Profile.Id == profile.Id).ToList();
                    dbContext.B2BAreasOfOperation.RemoveRange(areasOperations);

                    dbContext.SaveChanges();
                    if (model.AreasOfOperation != null)
                        foreach (var item in model.AreasOfOperation)
                        {
                            profile.AreasOperation.Add(new AreaOfOperation
                            {
                                Profile = profile,
                                Name = item,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    dbContext.Entry(profile).State = EntityState.Modified;

                    // Update Domain name
                    profile.Domain.Name = profile.BusinessName;
                    dbContext.Entry(profile).State = EntityState.Modified;

                    dbContext.SaveChanges();
                }
                else
                {
                    profile = new B2BProfile
                    {
                        BusinessName = model.BusinessName,
                        BusinessSummary = model.BusinessSummary,
                        BusinessEmail = model.BusinessEmail,
                        IsDisplayedInB2BListings = model.IsDisplayedInB2BListings,
                        IsDisplayedInB2CListings = model.IsDisplayedInB2CListings
                    };
                    if (model.UserIdB2BRelationshipManagers != null)
                    {
                        foreach (var item in model.UserIdB2BRelationshipManagers)
                        {
                            var user = dbContext.QbicleUser.Find(item);
                            if (user != null)
                                profile.DefaultB2BRelationshipManagers.Add(user);
                        }
                    }
                    if (model.UserIdB2CRelationshipManagers != null)
                    {
                        foreach (var item in model.UserIdB2CRelationshipManagers)
                        {
                            var user = dbContext.QbicleUser.Find(item);
                            if (user != null)
                                profile.DefaultB2CRelationshipManagers.Add(user);
                        }
                    }
                    profile.CreatedBy = currentUser;
                    profile.CreatedDate = DateTime.UtcNow;
                    profile.LastUpdatedBy = currentUser;
                    profile.LastUpdatedDate = profile.CreatedDate;
                    profile.Domain = model.Domain;
                    if (!string.IsNullOrEmpty(model.LogoUri))
                        profile.LogoUri = model.LogoUri;
                    if (!string.IsNullOrEmpty(model.BannerUri))
                        profile.BannerUri = model.BannerUri;
                    
                    if (model.AreasOfOperation != null)
                        foreach (var item in model.AreasOfOperation)
                        {
                            AreaOfOperation areaOfOperation = new AreaOfOperation
                            {
                                Profile = profile,
                                Name = item,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow
                            };
                            profile.AreasOperation.Add(areaOfOperation);
                        }
                    //When a Business Profile is created I think you should Add all TraderLocations for a Domain to your property BusinessLocations
                    #region Add default TraderLocations
                    var locations = new TraderLocationRules(dbContext).GetTraderLocation(model.Domain.Id);
                    if (locations != null && locations.Any())
                        profile.BusinessLocations = locations;
                    #endregion
                    #region Add default Catalogues
                    var catalogues = new B2CRules(dbContext).GetCatalogsByDomainId(model.Domain.Id);
                    if (catalogues != null && catalogues.Any())
                    {
                        profile.BusinessCatalogues = catalogues;
                    }
                    #endregion
                    dbContext.B2BProfiles.Add(profile);
                    dbContext.Entry(profile).State = EntityState.Added;

                    // Update Domain name
                    profile.Domain.Name = profile.BusinessName;
                    dbContext.Entry(profile).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }


                returnJson.result = true ;
                returnJson.Object = profile.Id;
                returnJson.msgName = model.BusinessName;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
            }
            return returnJson;
        }
        /// <summary>
        /// This is funtion update Default Relationship Manager of Business Profile
        /// </summary>
        /// <param name="profileId">B2bProfileId</param>
        /// <param name="managers">List Id of Users</param>
        /// <returns></returns>
        public ReturnJsonModel UpdateDefaultManagers(B2bProfileModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);
                var profile = dbContext.B2BProfiles.Find(model.Id);
                if (profile == null)
                {
                    returnJson.msg = "B2B_NOT_CREATE";
                    return returnJson;
                }
                profile.IsDisplayedInB2BListings = model.IsDisplayedInB2BListings;
                profile.IsDisplayedInB2CListings = model.IsDisplayedInB2CListings;
                profile.DefaultB2BRelationshipManagers.Clear();
                if (model.UserIdB2BRelationshipManagers != null)
                {
                    foreach (var item in model.UserIdB2BRelationshipManagers)
                    {
                        var user = dbContext.QbicleUser.Find(item);
                        if (user != null)
                            profile.DefaultB2BRelationshipManagers.Add(user);
                    }
                }

                profile.DefaultB2CRelationshipManagers.Clear();
                if (model.UserIdB2CRelationshipManagers != null)
                {
                    foreach (var item in model.UserIdB2CRelationshipManagers)
                    {
                        var user = dbContext.QbicleUser.Find(item);
                        if (user != null)
                            profile.DefaultB2CRelationshipManagers.Add(user);
                    }
                }
                profile.LastUpdatedBy = dbContext.QbicleUser.Find(userId); ;
                profile.LastUpdatedDate = DateTime.UtcNow;
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                returnJson.Object = new
                {
                    defaultb2bmanagers = profile.DefaultB2BRelationshipManagers.Select(s => HelperClass.GetFullNameOfUser(s)).ToList(),
                    defaultb2cmanagers = profile.DefaultB2CRelationshipManagers.Select(s => HelperClass.GetFullNameOfUser(s)).ToList()
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
            }
            return returnJson;
        }
        public ReturnJsonModel SavePost(B2bPostModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, model);
                var profile = dbContext.B2BProfiles.Find(model.ProfileId);
                if (profile == null)
                {
                    returnJson.msg = "B2B_NOT_CREATE";
                    return returnJson;
                }
                var isDomainAdmin = profile.Domain.Administrators.Any(p => p.Id == userId);
                if (!isDomainAdmin /*&& !new B2BWorkgroupRules(dbContext).GetCheckPermission(profile.Domain.Id, model.CurrentUser.Id, B2bProcessesConst.ProfileEditing)*/)
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                    return returnJson;
                }
                if (!string.IsNullOrEmpty(model.FeaturedImageUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(model.FeaturedImageUri);
                }
                var currentUser = dbContext.QbicleUser.Find(userId);
                var post = dbContext.B2BPosts.Find(model.Id);
                if (post != null)
                {
                    post.Title = model.Title;
                    post.Content = model.Content;
                    post.IsFeatured = model.IsFeatured;
                    post.LastUpdatedBy = currentUser;
                    post.LastUpdatedDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(model.FeaturedImageUri))
                        post.FeaturedImageUri = model.FeaturedImageUri;
                    dbContext.Entry(post).State = EntityState.Modified;
                }
                else
                {
                    post = new B2BPost
                    {
                        Profile = profile,
                        Title = model.Title,
                        Content = model.Content,
                        IsFeatured = model.IsFeatured,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedBy = currentUser,
                        LastUpdatedDate = DateTime.UtcNow
                    };
                    if (!string.IsNullOrEmpty(model.FeaturedImageUri))
                        post.FeaturedImageUri = model.FeaturedImageUri;
                    dbContext.B2BPosts.Add(post);
                    dbContext.Entry(post).State = EntityState.Added;
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
            }
            return returnJson;
        }
        public ReturnJsonModel GetB2bPostById(int id)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                var post = dbContext.B2BPosts.Find(id);
                if (post != null)
                {
                    returnJson.result = true;
                    returnJson.Object = new { post.Id, post.Title, post.Content };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnJson;
        }
        public ReturnJsonModel DeleteB2bPostById(int id)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                var post = dbContext.B2BPosts.Find(id);
                if (post != null)
                {
                    dbContext.B2BPosts.Remove(post);
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnJson;
        }
        public List<B2BPost> GetB2BPosts(int profileId, bool isFeatured, string search)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, profileId, isFeatured);
                if (string.IsNullOrEmpty(search))
                    return dbContext.B2BPosts.Where(s => s.Profile.Id == profileId && s.IsFeatured == isFeatured).ToList();
                else
                {
                    search = search.ToLower();
                    return dbContext.B2BPosts.Where(s => s.Profile.Id == profileId && s.IsFeatured == isFeatured && (s.Title.ToLower().Contains(search))).ToList();
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, profileId, isFeatured);
                return new List<B2BPost>();
            }
        }
        public ReturnJsonModel SaveB2BConfig(B2bConfigModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var user = dbContext.QbicleUser.Find(userId);
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);
                var config = dbContext.B2BSettings.FirstOrDefault(s => s.Location.Id == model.LocationId);
                if (config != null)
                {
                    //reset services
                    config.IsDesign = false;
                    config.IsLogistics = false;
                    config.IsMaintenance = false;
                    //end reset
                    if (model.Services != null)
                        foreach (var item in model.Services)
                        {
                            switch (item)
                            {
                                case "Design":
                                    config.IsDesign = true;
                                    break;
                                case "Logistics":
                                    config.IsLogistics = true;
                                    break;
                                case "Maintenance":
                                    config.IsMaintenance = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    config.OrderStatusWhenAddedToQueue = model.SettingOrder;
                    config.SalesChannel = SalesChannelEnum.B2B;
                    //Sale
                    config.DefaultSaleWorkGroup = dbContext.WorkGroups.Find(model.DefaultSaleWorkGroupId);
                    config.DefaultInvoiceWorkGroup = dbContext.WorkGroups.Find(model.DefaultInvoiceWorkGroupId);
                    config.DefaultPaymentWorkGroup = dbContext.WorkGroups.Find(model.DefaultPaymentWorkGroupId);
                    config.DefaultTransferWorkGroup = dbContext.WorkGroups.Find(model.DefaultTransferWorkGroupId);
                    config.DefaultPaymentAccount = dbContext.TraderCashAccounts.Find(model.DefaultPaymentAccountId);
                    //end Sale
                    //Purchase
                    config.DefaultBillWorkGroup = dbContext.WorkGroups.Find(model.DefaultBillWorkGroupId);
                    config.DefaultPurchaseWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchaseWorkGroupId);
                    config.DefaultPurchasePaymentWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchasePaymentWorkGroupId);
                    config.DefaultPurchaseTransferWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchaseTransferWorkGroupId);
                    config.DefaultPurchasePaymentAccount = dbContext.TraderCashAccounts.Find(model.DefaultPurchasePaymentAccountId);
                    //End
                }
                else
                {
                    config = new B2BSettings
                    {
                        Location = dbContext.TraderLocations.Find(model.LocationId),
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        SalesChannel = SalesChannelEnum.B2B,
                        OrderStatusWhenAddedToQueue = model.SettingOrder,
                        //Sale
                        DefaultSaleWorkGroup = dbContext.WorkGroups.Find(model.DefaultSaleWorkGroupId),
                        DefaultInvoiceWorkGroup = dbContext.WorkGroups.Find(model.DefaultInvoiceWorkGroupId),
                        DefaultPaymentWorkGroup = dbContext.WorkGroups.Find(model.DefaultPaymentWorkGroupId),
                        DefaultTransferWorkGroup = dbContext.WorkGroups.Find(model.DefaultTransferWorkGroupId),
                        DefaultPaymentAccount = dbContext.TraderCashAccounts.Find(model.DefaultPaymentAccountId),
                        //end Sale
                        //Purchase
                        DefaultBillWorkGroup = dbContext.WorkGroups.Find(model.DefaultBillWorkGroupId),
                        DefaultPurchaseWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchaseWorkGroupId),
                        DefaultPurchasePaymentWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchasePaymentWorkGroupId),
                        DefaultPurchaseTransferWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchaseTransferWorkGroupId),
                        DefaultPurchasePaymentAccount = dbContext.TraderCashAccounts.Find(model.DefaultPurchasePaymentAccountId),
                        //End
                    };
                    if (model.Services != null)
                        foreach (var item in model.Services)
                        {
                            switch (item)
                            {
                                case "Design":
                                    config.IsDesign = true;
                                    break;
                                case "Logistics":
                                    config.IsLogistics = true;
                                    break;
                                case "Maintenance":
                                    config.IsMaintenance = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    dbContext.B2BSettings.Add(config);
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
            }
            return returnJson;
        }
        public B2BSettings GetB2BConfigByLocationId(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, locationId);
                var config = dbContext.B2BSettings.FirstOrDefault(s => s.Location.Id == locationId);
                return config != null ? config : new B2BSettings();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return new B2BSettings();
            }
        }

        public void RemoveUserFromDefaultRelationshipManagers(ApplicationUser removeUser, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove member from all B2CQbicle in Domain", null, null, removeUser, domainId);

                var b2bProfile = dbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == domainId);
                if (b2bProfile != null)
                {
                    b2bProfile.DefaultB2CRelationshipManagers.Remove(removeUser);
                }
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, removeUser, domainId);
            }

        }

        public B2CSettings GetB2CConfigByLocationId(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, locationId);
                var config = dbContext.B2CSettings.FirstOrDefault(s => s.Location.Id == locationId);
                return config != null ? config : new B2CSettings();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return new B2CSettings();
            }
        }
        public ReturnJsonModel SaveB2CConfig(B2cConfigModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var user = dbContext.QbicleUser.Find(userId);
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);
                var config = dbContext.B2CSettings.FirstOrDefault(s => s.Location.Id == model.LocationId);
                if (config != null)
                {
                    config.OrderStatusWhenAddedToQueue = model.SettingOrder;
                    config.SalesChannel = SalesChannelEnum.B2C;
                    config.DefaultSaleWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultSaleWorkGroupId);
                    config.DefaultInvoiceWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultInvoiceWorkGroupId);
                    config.DefaultPaymentWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultPaymentWorkGroupId);
                    config.DefaultTransferWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultTransferWorkGroupId);
                    config.DefaultPaymentAccount = dbContext.TraderCashAccounts.FirstOrDefault(e => e.Id == model.DefaultPaymentAccountId);
                }
                else
                {
                    config = new B2CSettings
                    {
                        Location = dbContext.TraderLocations.FirstOrDefault(e => e.Id == model.LocationId),
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        SalesChannel = SalesChannelEnum.B2C,
                        OrderStatusWhenAddedToQueue = model.SettingOrder,
                        DefaultSaleWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultSaleWorkGroupId),
                        DefaultInvoiceWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultInvoiceWorkGroupId),
                        DefaultPaymentWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultPaymentWorkGroupId),
                        DefaultTransferWorkGroup = dbContext.WorkGroups.FirstOrDefault(e => e.Id == model.DefaultTransferWorkGroupId),
                        DefaultPaymentAccount = dbContext.TraderCashAccounts.FirstOrDefault(e => e.Id == model.DefaultPaymentAccountId),
                        UseDefaultWorkgroupSettings = false
                    };

                    dbContext.B2CSettings.Add(config);
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
            }
            return returnJson;
        }

        public B2BOrderSettingDefault GetB2BOrderSettingDefault(int domainId, string userId, int locationId = 0)
        {
            try
            {
                if (locationId == 0)
                    locationId = dbContext.TraderLocations.Where(l => l.Domain.Id == domainId).OrderBy(n => n.Name).FirstOrDefault()?.Id ?? 0;
                var b2b = dbContext.B2BSettings.FirstOrDefault(b => b.Location.Id == locationId);
                if (b2b == null)
                {
                    this.SaveB2BConfig(new B2bConfigModel
                    {
                        LocationId = locationId,
                        SourceQbicleId = 0,
                        DefaultTopicId = 0,
                        CurrentUser = dbContext.QbicleUser.Find(userId),
                        SalesChannel = SalesChannelEnum.B2B,
                        SettingOrder = PrepQueueStatus.NotStarted,
                        OrderStatusWhenAddedToQueue = PrepQueueStatus.NotStarted,
                        Services = new List<string> { "Logistics" }
                    }, userId);
                    return new B2BOrderSettingDefault
                    {
                        LocationId = locationId,
                        B2bOrder = PrepQueueStatus.NotStarted,
                    };
                }
                else
                {
                    return new B2BOrderSettingDefault
                    {
                        LocationId = locationId,
                        B2bOrder = b2b.OrderStatusWhenAddedToQueue,
                        DefaultSaleWorkGroupId = b2b.DefaultSaleWorkGroup?.Id ?? 0,
                        DefaultInvoiceWorkGroupId = b2b.DefaultInvoiceWorkGroup?.Id ?? 0,
                        DefaultPaymentWorkGroupId = b2b.DefaultPaymentWorkGroup?.Id ?? 0,
                        DefaultTransferWorkGroupId = b2b.DefaultTransferWorkGroup?.Id ?? 0,
                        DefaultPaymentAccountId = b2b.DefaultPaymentAccount?.Id ?? 0,

                        DefaultPurchaseWorkGroupId = b2b.DefaultPurchaseWorkGroup?.Id ?? 0,
                        DefaultBillWorkGroupId = b2b.DefaultBillWorkGroup?.Id ?? 0,
                        DefaultPurchasePaymentWorkGroupId = b2b.DefaultPurchasePaymentWorkGroup?.Id ?? 0,
                        DefaultPurchaseTransferWorkGroupId = b2b.DefaultPurchaseTransferWorkGroup?.Id ?? 0,
                        DefaultPurchasePaymentAccountId = b2b.DefaultPurchasePaymentAccount?.Id ?? 0
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
            }
            return null;
        }
        public B2COrderSettingDefault GetB2COrderSettingDefault(int domainId, string userId, int locationId = 0)
        {
            try
            {
                if (locationId == 0)
                    locationId = dbContext.TraderLocations.Where(l => l.Domain.Id == domainId).OrderBy(n => n.Name).FirstOrDefault()?.Id ?? 0;
                var b2c = dbContext.B2CSettings.FirstOrDefault(c => c.Location.Id == locationId);
                if (b2c == null)
                {
                    this.SaveB2CConfig(new B2cConfigModel
                    {
                        LocationId = locationId,
                        CurrentUser = dbContext.QbicleUser.Find(userId),
                        SalesChannel = SalesChannelEnum.B2C,
                        SettingOrder = PrepQueueStatus.NotStarted,
                        OrderStatusWhenAddedToQueue = PrepQueueStatus.NotStarted,
                        DefaultSaleWorkGroupId = b2c?.DefaultSaleWorkGroup?.Id ?? 0,
                        DefaultInvoiceWorkGroupId = b2c?.DefaultInvoiceWorkGroup?.Id ?? 0,
                        DefaultPaymentWorkGroupId = b2c?.DefaultPaymentWorkGroup?.Id ?? 0,
                        DefaultTransferWorkGroupId = b2c?.DefaultTransferWorkGroup?.Id ?? 0,
                        DefaultPaymentAccountId = b2c?.DefaultPaymentAccount?.Id ?? 0,
                    }, userId);
                    return new B2COrderSettingDefault
                    {
                        LocationId = locationId,
                        B2cOrder = PrepQueueStatus.NotStarted,
                        SaveSettings = false
                    };
                }
                else
                {
                    return new B2COrderSettingDefault
                    {
                        LocationId = locationId,
                        B2cOrder = b2c.OrderStatusWhenAddedToQueue,
                        DefaultSaleWorkGroupId = b2c.DefaultSaleWorkGroup?.Id ?? 0,
                        DefaultInvoiceWorkGroupId = b2c.DefaultInvoiceWorkGroup?.Id ?? 0,
                        DefaultPaymentWorkGroupId = b2c.DefaultPaymentWorkGroup?.Id ?? 0,
                        DefaultTransferWorkGroupId = b2c.DefaultTransferWorkGroup?.Id ?? 0,
                        DefaultPaymentAccountId = b2c.DefaultPaymentAccount?.Id ?? 0,
                        SaveSettings = b2c.UseDefaultWorkgroupSettings
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
            }
            return null;
        }

        public ReturnJsonModel B2CSaveOrderConfigDefault(B2COrderSettingDefault model)
        {
            try
            {
                var b2c = dbContext.B2CSettings.FirstOrDefault(d => d.Location.Id == model.LocationId);
                b2c.OrderStatusWhenAddedToQueue = model.B2cOrder;
                Thread.Sleep(1000);
                if (model.DefaultSaleWorkGroupId == 0)
                    b2c.DefaultSaleWorkGroup = null;
                else
                    b2c.DefaultSaleWorkGroup = dbContext.WorkGroups.Find(model.DefaultSaleWorkGroupId);

                if (model.DefaultInvoiceWorkGroupId == 0)
                    b2c.DefaultInvoiceWorkGroup = null;
                else
                    b2c.DefaultInvoiceWorkGroup = dbContext.WorkGroups.Find(model.DefaultInvoiceWorkGroupId);

                if (model.DefaultPaymentWorkGroupId == 0)
                    b2c.DefaultPaymentWorkGroup = null;
                else
                    b2c.DefaultPaymentWorkGroup = dbContext.WorkGroups.Find(model.DefaultPaymentWorkGroupId);

                if (model.DefaultTransferWorkGroupId == 0)
                    b2c.DefaultTransferWorkGroup = null;
                else
                    b2c.DefaultTransferWorkGroup = dbContext.WorkGroups.Find(model.DefaultTransferWorkGroupId);

                if (model.DefaultPaymentAccountId == 0)
                    b2c.DefaultPaymentAccount = null;
                else
                    b2c.DefaultPaymentAccount = dbContext.TraderCashAccounts.Find(model.DefaultPaymentAccountId);
                
                b2c.UseDefaultWorkgroupSettings = model.SaveSettings;


                if (dbContext.Entry(b2c).State == EntityState.Detached)
                    dbContext.B2CSettings.Attach(b2c);
                dbContext.Entry(b2c).State = EntityState.Modified;

                dbContext.SaveChanges();
                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }
        public ReturnJsonModel B2BSaveOrderConfigDefault(B2BOrderSettingDefault model)
        {
            try
            {
                var b2b = dbContext.B2BSettings.FirstOrDefault(d => d.Location.Id == model.LocationId);
                b2b.OrderStatusWhenAddedToQueue = model.B2bOrder;
                //Sale
                b2b.DefaultSaleWorkGroup = dbContext.WorkGroups.Find(model.DefaultSaleWorkGroupId);
                b2b.DefaultInvoiceWorkGroup = dbContext.WorkGroups.Find(model.DefaultInvoiceWorkGroupId);
                b2b.DefaultPaymentWorkGroup = dbContext.WorkGroups.Find(model.DefaultPaymentWorkGroupId);
                b2b.DefaultTransferWorkGroup = dbContext.WorkGroups.Find(model.DefaultTransferWorkGroupId);
                b2b.DefaultPaymentAccount = dbContext.TraderCashAccounts.Find(model.DefaultPaymentAccountId);
                //end Sale
                //Purchase
                b2b.DefaultBillWorkGroup = dbContext.WorkGroups.Find(model.DefaultBillWorkGroupId);
                b2b.DefaultPurchaseWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchaseWorkGroupId);
                b2b.DefaultPurchasePaymentWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchasePaymentWorkGroupId);
                b2b.DefaultPurchaseTransferWorkGroup = dbContext.WorkGroups.Find(model.DefaultPurchaseTransferWorkGroupId);
                b2b.DefaultPurchasePaymentAccount = dbContext.TraderCashAccounts.Find(model.DefaultPurchasePaymentAccountId);
                //End
                dbContext.SaveChanges();
                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }
        public ReturnJsonModel UpdateSocialLinks(List<B2BSocialLink> socialLinks)
        {
            var returnjson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, socialLinks);
                B2BProfile b2bProfile = null;
                if (socialLinks != null)
                    foreach (var item in socialLinks)
                    {
                        if (b2bProfile == null)
                            b2bProfile = dbContext.B2BProfiles.Find(item.B2BProfile.Id);
                        var link = dbContext.B2BSocialLinks.FirstOrDefault(s => s.B2BProfile.Id == item.B2BProfile.Id && s.Type == item.Type);
                        if (link == null)
                        {
                            if (!string.IsNullOrEmpty(item.Url))
                            {
                                link = new B2BSocialLink
                                {
                                    Type = item.Type,
                                    Url = item.Url,
                                    B2BProfile = b2bProfile
                                };
                                dbContext.B2BSocialLinks.Add(link);
                                dbContext.Entry(link).State = EntityState.Added;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.Url))
                            {
                                link.Url = item.Url;
                                dbContext.Entry(link).State = EntityState.Modified;
                            }
                            else
                                dbContext.B2BSocialLinks.Remove(link);
                        }
                    }
                dbContext.SaveChanges();
                returnjson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, socialLinks);
            }
            return returnjson;
        }
        public ReturnJsonModel UpdateTags(List<string> tags, int profileId)
        {
            var returnjson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, tags);
                B2BProfile b2bProfile = dbContext.B2BProfiles.Find(profileId);
                dbContext.B2BTags.RemoveRange(b2bProfile.Tags);
                if (tags != null)
                    foreach (var tagName in tags.Take(8))
                    {
                        var tag = new B2BTag
                        {
                            B2BProfile = b2bProfile,
                            TagName = tagName
                        };
                        b2bProfile.Tags.Add(tag);
                    }
                dbContext.SaveChanges();
                returnjson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tags);
            }
            return returnjson;
        }
        public ReturnJsonModel UpdateCategories(List<int> categories, int profileId)
        {
            var returnjson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, categories, profileId);
                B2BProfile b2bProfile = dbContext.B2BProfiles.Find(profileId);
                b2bProfile.BusinessCategories.Clear();
                if (categories != null)
                    foreach (var catid in categories)
                    {
                        var category = dbContext.BusinessCategories.Find(catid);
                        if (category != null)
                            b2bProfile.BusinessCategories.Add(category);
                    }
                dbContext.SaveChanges();
                returnjson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, categories, profileId);
            }
            return returnjson;
        }
        public ReturnJsonModel CloneTradingItemFromCatalogueItemId(CloneDistributorCatalogueItem model)
        {
            var returnjson = new ReturnJsonModel { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);
                    var currentUser = dbContext.QbicleUser.Find(model.CurrentUserId);
                    var desLocations = dbContext.TraderLocations.Where(d => d.Domain.Id == model.destinationDomainId);
                    var catalogueItem = dbContext.PosCategoryItems.Find(model.itemId);
                    if (catalogueItem != null)
                    {
                        TraderItem cloneItem = catalogueItem.PosVariants.Select(s => s.TraderItem).FirstOrDefault();
                        if (cloneItem != null)
                        {
                            if (cloneItem.Domain.Id == model.destinationDomainId)
                            {
                                returnjson.msg = ResourcesManager._L("ERROR_DATA_EXISTED", cloneItem.Name);
                                return returnjson;
                            }
                            if (dbContext.TraderItems.Any(s => s.Barcode == cloneItem.Barcode && s.Domain.Id == model.destinationDomainId))
                            {
                                returnjson.msg = ResourcesManager._L("ERROR_DATA_EXISTED", cloneItem.Name);
                                return returnjson;
                            }
                            TraderItem item = new TraderItem();
                            item.Domain = dbContext.Domains.Find(model.destinationDomainId);
                            item.Name = cloneItem.Name;
                            item.ImageUri = cloneItem.ImageUri;
                            item.Group = dbContext.TraderGroups.Find(model.groupId);
                            item.IsBought = true;
                            item.IsSold = true;
                            item.IsCommunityProduct = false;
                            item.IsActiveInAllLocations = true;
                            item.Barcode = cloneItem.Barcode;
                            item.SKU = model.sku;
                            item.Description = model.description;
                            //item.DescriptionText = 
                            item.CreatedBy = currentUser;
                            item.CreatedDate = DateTime.UtcNow;
                            item.Locations.AddRange(desLocations);
                            #region Clone Product Units
                            List<ProductUnit> productUnits = new List<ProductUnit>();
                            foreach (var unit in cloneItem.Units.ToList())
                            {
                                CloneProductUnits(productUnits, unit, item);
                            }
                            item.Units = productUnits;
                            #endregion
                            #region Create an InventoryDetail in the importing Domain for the TraderItem at each Location in the Importing Domain
                            foreach (var location in desLocations)
                            {
                                InventoryDetail inventory = new InventoryDetail();
                                inventory.Location = location;
                                inventory.MinInventorylLevel = 0;
                                inventory.MaxInventoryLevel = 0;
                                inventory.CurrentInventoryLevel = 0;
                                inventory.AverageCost = 0;
                                inventory.LatestCost = 0;
                                inventory.Item = item;
                                inventory.CreatedBy = item.CreatedBy;
                                inventory.CreatedDate = item.CreatedDate;
                                inventory.LastUpdatedBy = item.CreatedBy;
                                inventory.LastUpdatedDate = item.CreatedDate;
                                dbContext.InventoryDetails.Add(inventory);
                                dbContext.Entry(inventory).State = EntityState.Added;
                                //item.InventoryDetails.Add(inventory);
                            }
                            #endregion
                            if (model.isprimaryVendor)
                            {
                                var b2bRelationship = dbContext.B2BRelationships.Find(model.b2bRelationshipId);
                                var tradercontact = b2bRelationship.Domain1.Id == model.destinationDomainId ? b2bRelationship.Domain1TraderContactForDomain2 : b2bRelationship.Domain2TraderContactForDomain1;
                                if (tradercontact == null)
                                {
                                    returnjson.msg = "ERROR_MSG_B2BCONTACTNOTFOUND";
                                    dbTransaction.Rollback();
                                    return returnjson;
                                }
                                foreach (var location in desLocations)
                                {
                                    var vendor = new TraderItemVendor
                                    {
                                        Id = 0,
                                        IsPrimaryVendor = true,
                                        Item = item,
                                        Vendor = tradercontact,
                                        Location = location
                                    };
                                    dbContext.TraderItemVendors.Add(vendor);
                                }
                            }
                            dbContext.TraderItems.Add(item);
                            returnjson.result = dbContext.SaveChanges() > 0;
                            dbTransaction.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                    dbTransaction.Rollback();
                }
            }
            return returnjson;
        }
        private ProductUnit CloneProductUnits(List<ProductUnit> productUnits, ProductUnit CloneUnit, TraderItem item)
        {
            ProductUnit productUnit = new ProductUnit();
            if (!productUnits.Any(s => s.Name == CloneUnit.Name && s.Quantity == CloneUnit.Quantity && s.QuantityOfBaseunit == CloneUnit.QuantityOfBaseunit))
            {
                productUnit.Name = CloneUnit.Name;
                productUnit.Item = item;
                productUnit.Quantity = CloneUnit.Quantity;
                productUnit.QuantityOfBaseunit = CloneUnit.QuantityOfBaseunit;
                productUnit.CreatedDate = item.CreatedDate;
                productUnit.CreatedBy = item.CreatedBy;
                productUnit.IsActive = CloneUnit.IsActive;
                productUnit.IsBase = CloneUnit.IsBase;
                productUnit.IsPrimary = CloneUnit.IsPrimary;
                productUnit.MeasurementType = CloneUnit.MeasurementType;
                if (CloneUnit.ParentUnit != null)
                    productUnit.ParentUnit = CloneProductUnits(productUnits, CloneUnit.ParentUnit, item);
                else
                    productUnit.ParentUnit = null;
                productUnits.Add(productUnit);
            }
            else
            {
                productUnit = productUnits.FirstOrDefault(s => s.Name == CloneUnit.Name && s.Quantity == CloneUnit.Quantity && s.QuantityOfBaseunit == CloneUnit.QuantityOfBaseunit);
            }
            return productUnit;
        }
        public ReturnJsonModel SetDefaultAddress(int domainId, int setDefaultLocationId)
        {
            var returnjson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId, setDefaultLocationId);
                var locations = dbContext.TraderLocations.Where(s => s.Domain.Id == domainId).ToList();
                if (locations != null)
                    foreach (var item in locations)
                    {
                        item.IsDefaultAddress = false;
                    }
                var defaultLocation = dbContext.TraderLocations.Find(setDefaultLocationId);
                if (defaultLocation != null)
                    defaultLocation.IsDefaultAddress = true;
                dbContext.SaveChanges();
                returnjson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, setDefaultLocationId);
            }
            return returnjson;
        }
        public TraderLocation GetDefaultLocationOfDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId);
                return dbContext.TraderLocations.Where(s => s.Domain.Id == domainId && s.IsDefaultAddress).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
            }
            return null;
        }
        public ReturnJsonModel SetContactForB2BRelationship(int relationshipId, int currentDomainId, int contactId, int groupId, int workgroupId, string currentUserId)
        {
            var returnjson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentDomainId, relationshipId, contactId);
                var b2bRelationship = dbContext.B2BRelationships.Find(relationshipId);
                if (b2bRelationship != null)
                {
                    //Get BusinessProfile of Partnership
                    var partnerDomain = b2bRelationship.Domain1.Id == currentDomainId ? b2bRelationship.Domain2 : b2bRelationship.Domain1;
                    var businessProfile = partnerDomain.Id.BusinesProfile();
                    var contact = dbContext.TraderContacts.Find(contactId);
                    if (contact == null)
                    {
                        var traderLocation = GetDefaultLocationOfDomain(partnerDomain.Id);
                        if (traderLocation == null)
                            traderLocation = dbContext.TraderLocations.FirstOrDefault(s => s.Domain.Id == partnerDomain.Id);
                        contact = new TraderContact();
                        contact.ContactRef = new TraderContactRules(dbContext).CreateNewTraderContactRef(currentDomainId);
                        contact.AvatarUri = businessProfile?.LogoUri ?? partnerDomain.LogoUri;
                        contact.Email = businessProfile?.BusinessEmail ?? (traderLocation?.Address?.Email);
                        contact.Address = traderLocation?.Address;
                        contact.Name = businessProfile?.BusinessName ?? partnerDomain.Name;
                        contact.Workgroup = dbContext.WorkGroups.Find(workgroupId);
                        contact.CompanyName = contact.Name;
                        contact.ContactGroup = dbContext.TraderContactGroups.Find(groupId);
                        contact.CreatedBy = dbContext.QbicleUser.Find(currentUserId);
                        contact.CreatedDate = DateTime.UtcNow;
                        contact.Status = TraderContactStatusEnum.ContactApproved;
                        dbContext.TraderContacts.Add(contact);
                    }
                    if (b2bRelationship.Domain1.Id == partnerDomain.Id)
                    {
                        b2bRelationship.Domain2TraderContactForDomain1 = contact;
                    }
                    else
                    {
                        b2bRelationship.Domain1TraderContactForDomain2 = contact;
                    }
                }
                returnjson.result = dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId, currentDomainId, relationshipId, contactId);
            }
            return returnjson;
        }
        public ReturnJsonModel AddItemToB2BOrder(B2BOrderItemModel model)
        {
            var result = new ReturnJsonModel { actionVal = 2 };

            try
            {
                var tradeOrder = dbContext.B2BTradeOrders.Find(model.OrderId);
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, model.CurrentUserId, null, model);
                }

                var b2bOrder = new Order();
                if (tradeOrder.OrderJson != null)
                {
                    b2bOrder = JsonHelper.ParseAs<Order>(tradeOrder.OrderJson);
                    ValidateB2BOrder(b2bOrder);
                }
                var itemNew = new Item
                {
                    Prepared = false,
                    TraderId = model.ItemId,
                    Quantity = model.Quantity,
                };

                var posItem = dbContext.PosCategoryItems.FirstOrDefault(p => p.Id == model.ItemId);
                if (posItem != null)
                    itemNew.ImageUri = posItem.ImageUri;

                if (b2bOrder.Items == null || b2bOrder.Items.Count() == 0)
                    itemNew.Id = 1;
                else
                {
                    var maxItemId = b2bOrder.Items.Max(p => p.Id);
                    itemNew.Id = maxItemId + 1;
                }
    
                //Calculate variant price
                itemNew.Name = posItem?.Name ?? "";
                itemNew.Variant = new Models.TraderApi.Variant();

                //Get Variant
                if (model.Variant != null && model.Variant.Id > 0)
                {
                    model.Variant = dbContext.PosVariants.FirstOrDefault(p => p.Id == model.Variant.Id);
                    itemNew.Variant.TraderId = model.Variant.Id;
                    itemNew.Variant.Name = model.Variant.Name;

                    itemNew.Variant.Discount = 0;

                    itemNew.Variant.AmountExclTax = model.Variant.Price?.NetPrice ?? 0;

                    itemNew.Variant.AmountInclTax += model.Variant.Price?.GrossPrice ?? 0;

                    // B2B QBIC-5004 Incase discount = 100, all AmountExclTax, AmountInclTax will be 0 and can't be change anymore.
                    // To avoid that, using Net and Gross value to caculator.
                    itemNew.Variant.NetValue = model.Variant.Price?.NetPrice ?? 0;
                    itemNew.Variant.GrossValue = model.Variant.Price?.GrossPrice ?? 0;


                    var taxRateList = model.Variant.Price?.Taxes ?? new List<PriceTax>();

                    if (itemNew.Variant.Taxes == null)
                        itemNew.Variant.Taxes = new List<Tax>();

                    foreach (var taxItem in taxRateList)
                    {
                        Tax taxModel = new Tax()
                        {
                            TraderId = taxItem.Id,
                            AmountTax = taxItem.Amount,
                            TaxName = taxItem.TaxName,
                            TaxRate = taxItem.Rate,
                        };
                        itemNew.Variant.Taxes.Add(taxModel);
                    }
                    //QBIC-5074 Overlay item display the price is incorrectly
                    itemNew.Variant.TotalAmountWithoutDiscount = itemNew.Variant.AmountInclTax * (itemNew?.Quantity ?? 0);
                    //There is no discount(item.Discount = 0) for new item adding to the cart in B2B
                    itemNew.Variant.TotalAmount = itemNew.Variant.TotalAmountWithoutDiscount;
                    
                    if (model.AssociatedItemId > 0)
                    {
                        // B2B QBIC-2723: The model.AssociatedItemId below looks to me to be from the ProviderDomain, it should be from the ConsumingDomain
                        var traderItem = dbContext.TraderItems.Find(model.AssociatedItemId); ;
                        var b2bTradingItem = tradeOrder.TradingItems.FirstOrDefault(s => s.ConsumerDomainItem.Id == model.AssociatedItemId && s.Variant.Id == model.Variant.Id);
                        if (b2bTradingItem == null)
                        {
                            b2bTradingItem = new B2BTradingItem
                            {
                                RelatedOrder = tradeOrder,
                                CreatedBy = dbContext.QbicleUser.Find(model.CurrentUserId),
                                CreatedDate = DateTime.UtcNow,
                                // B2B QBIC-2723: The TraderItem below looks to me to be the TraderItem in the ProviderDomain, it should be the TraderItem in the ConsumingDomain
                                ConsumerDomainItem = traderItem,
                                ConsumerUnit = dbContext.ProductUnits.Find(model.AssociatedUnitId),
                                Variant = model.Variant
                            };
                            dbContext.B2BTradingItems.Add(b2bTradingItem);
                        }
                        else
                        {
                            b2bTradingItem.ConsumerUnit = dbContext.ProductUnits.Find(model.AssociatedUnitId);
                        }

                        #region your primary vendor for this item
                        if (model.PrimaryVendor)
                        {
                            var tradercontact = tradeOrder.VendorTraderContact;
                            if (tradercontact == null)
                            {
                                result.result = false;
                                result.msg = "ERROR_MSG_B2BCONTACTNOTFOUND";
                                return result;
                            }
                            var locations = traderItem.Locations;
                            foreach (var item in locations)
                            {
                                var vendorcontact = traderItem.VendorsPerLocation.FirstOrDefault(s => s.Location.Id == item.Id && s.Vendor.Id == tradercontact.Id);
                                if (vendorcontact == null)
                                {
                                    //set other vendor not IsPrimary
                                    var otherVendors = traderItem.VendorsPerLocation.Where(s => s.Location.Id == item.Id && s.Vendor.Id != tradercontact.Id).ToList();
                                    foreach (var othv in otherVendors)
                                    {
                                        othv.IsPrimaryVendor = false;
                                    }
                                    vendorcontact = new TraderItemVendor
                                    {
                                        Id = 0,
                                        IsPrimaryVendor = true,
                                        Item = traderItem,
                                        Vendor = tradercontact,
                                        Location = item
                                    };
                                    dbContext.TraderItemVendors.Add(vendorcontact);
                                }
                                else
                                {
                                    vendorcontact.IsPrimaryVendor = true;
                                    //set other vendor not IsPrimary
                                    var otherVendors = traderItem.VendorsPerLocation.Where(s => s.Location.Id == item.Id && s.Vendor.Id != tradercontact.Id).ToList();
                                    foreach (var othv in otherVendors)
                                    {
                                        othv.IsPrimaryVendor = false;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }

                b2bOrder.AmountTax += (model.Variant?.Price?.TotalTaxAmount ?? 0) * model.Quantity;

                //Insert extra info
                itemNew.Extras = new List<Variant>();
                if (model.Extras != null)
                {
                    foreach (var extraItem in model.Extras)
                    {
                        var extraNew = new Variant();

                        if (extraItem.Id > 0)
                        {
                            var extraItemInDb = dbContext.PosExtras.FirstOrDefault(p => p.Id == extraItem.Id);
                            extraNew.TraderId = extraItem.Id;
                            extraNew.Name = extraItemInDb.Name;
                            extraNew.Discount = 0;
                            extraNew.AmountExclTax = extraItemInDb.Price?.NetPrice ?? 0;
                            extraNew.AmountInclTax = extraItemInDb.Price?.GrossPrice ?? 0;
                            extraNew.TaxAmount = extraNew.AmountInclTax - extraNew.AmountExclTax;
                            b2bOrder.AmountTax += extraNew.TaxAmount * model.Quantity;
                            
                            // B2B QBIC-5004 Incase discount = 100, all AmountExclTax, AmountInclTax will be 0 and can't be change anymore.
                            // To avoid that, using Net and Gross value to caculator.
                            extraNew.NetValue = extraItemInDb.Price?.NetPrice ?? 0;
                            extraNew.GrossValue = extraItemInDb.Price?.GrossPrice ?? 0;
                            
                            //QBIC-5074 Overlay item display the price is incorrectly
                            extraNew.TotalAmountWithoutDiscount = extraNew.AmountInclTax * (model?.Quantity ?? 0);
                            //There is no discount(extraNew.Discount = 0) for new extra adding to the cart in B2B
                            extraNew.TotalAmount = extraNew.TotalAmountWithoutDiscount;

                            var taxRateExtraList = extraItemInDb.Price?.Taxes ?? new List<PriceTax>();

                            if (extraNew.Taxes == null)
                            {
                                extraNew.Taxes = new List<Tax>();
                            }
                            foreach (var taxItem in taxRateExtraList)
                            {
                                Tax taxModel = new Tax()
                                {
                                    TraderId = taxItem.Id,
                                    AmountTax = taxItem.Amount,
                                    TaxName = taxItem.TaxName,
                                    TaxRate = taxItem.Rate
                                };
                                extraNew.Taxes.Add(taxModel);
                            }
                        }
                        itemNew.Extras.Add(extraNew);
                    }
                }

                if (b2bOrder.Items == null)
                {
                    b2bOrder.Items = new List<Item>();
                }
                b2bOrder.Items.Add(itemNew);
                b2bOrder.AmountInclTax += model.IncludedTaxAmount;
                b2bOrder.AmountExclTax = b2bOrder.AmountInclTax - b2bOrder.AmountTax;

                var _orderJson = JsonHelper.ToJson(b2bOrder);
                tradeOrder.OrderJson = _orderJson;

                //Remove customer and business accepted flag
                tradeOrder.IsAgreedByBusiness = false;
                tradeOrder.IsAgreedByCustomer = false;
                tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;


                dbContext.Entry(tradeOrder).State = EntityState.Modified;
                result.result = dbContext.SaveChanges() > 0;

                result.msgName = tradeOrder.BuyingDomain.Id.BusinesProfile().BusinessName;
                result.Object = tradeOrder.Id;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, model.CurrentUserId, model);
                result.result = false;
                result.msg = ex.Message;
                return result;
            }
        }
        public List<B2BCartItemModel> GetB2BOrderItemsPagination(int orderId, string keySearch, List<int> catIds, IDataTablesRequest requestModel, bool isViewBuy, ref int totalRecord, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, orderId, keySearch, catIds, requestModel, totalRecord, start, length);

                var resultList = new List<B2BCartItemModel>();
                //var b2bOrderDiscussion = dbContext.B2BOrderCreations.Find(discussionId);
                var tradeOrder = dbContext.B2BTradeOrders.Find(orderId);
                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(tradeOrder.SellingDomain.Id);

                var _order = JsonHelper.ParseAs<Order>(tradeOrder?.OrderJson ?? "");
                if (_order == null || _order.Items == null || _order.Items.Count <= 0)
                    return new List<B2BCartItemModel>();
                var query = from item in _order.Items select item;
                decimal totalPrice = 0;
                _order.Items.ForEach(p =>
                {
                    totalPrice += p.Variant.AmountInclTax * p.Quantity;
                    p.Extras.ForEach(e =>
                    {
                        totalPrice += e.AmountInclTax * p.Quantity;
                    });
                });

                #region filter
                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(keySearch.ToLower()));
                }

                if (catIds != null && catIds.Count > 0)
                {
                    query = from orderItem in query
                            join posItem in dbContext.PosCategoryItems on orderItem.TraderId equals posItem.Id
                            where catIds.Contains(posItem.Category.Id)
                            select orderItem;
                }
                #endregion

                totalRecord = query.Count();

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "ItemName":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Quantity":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Quantity" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                    }
                    query = query.OrderBy(string.IsNullOrEmpty(orderString) ? "Name asc" : orderString);
                }
                #endregion

                #region Paging
                var orderItemList = query.Skip(start).Take(length).ToList();
                #endregion
                //var order = b2bOrderDiscussion.TradeOrder;
                var _isAllowEdit = true;
                if (tradeOrder != null && tradeOrder.IsAgreedByBusiness && tradeOrder.IsAgreedByCustomer)
                {
                    _isAllowEdit = false;
                }
                orderItemList.ForEach((Action<Item>)(p =>
                {
                    var orderItemModel = new B2BCartItemModel()
                    {
                        ItemId = p.Id,
                        ItemName = p.Name,
                        Quantity = p.Quantity,
                        TotalPrice = totalPrice,
                        IsAllowEdit = _isAllowEdit,
                        Variant = new Models.TraderApi.Variant(),
                        Extras = new List<Models.TraderApi.Variant>()
                    };

                    //_discount = 1/(1 - discount/100) - 
                    var _discount = p.Variant.Discount == 100 ? 0 : 1 / (1 - p.Variant.Discount / 100);

                    //Re-calculate variant amount
                    orderItemModel.Variant.AmountInclTax = p.Variant.AmountInclTax * _discount;
                    orderItemModel.Variant.Name = p.Variant.Name;
                    orderItemModel.Variant.TraderId = p.Variant.TraderId;
                    p.Extras.ForEach((Action<Models.TraderApi.Variant>)(e =>
                    {
                        var exItem = new Variant()
                        {
                            AmountInclTax = e.AmountInclTax * _discount,
                            Name = e.Name
                        };
                        orderItemModel.Extras.Add((Variant)exItem);
                    }));

                    var posItem = dbContext.PosCategoryItems.Find(p.TraderId);
                    orderItemModel.CategoryName = posItem?.Category?.Name ?? "";

                    var itemVariant = p.Variant;
                    orderItemModel.Discount = itemVariant.Discount;

                    // For setting tooltip text
                    var defaultPosVariant = posItem.PosVariants.FirstOrDefault(x => x.IsDefault);
                    var associatedTraderItem = defaultPosVariant.TraderItem;
                    orderItemModel.SourceName = associatedTraderItem.Name == p.Name ? "" : associatedTraderItem.Name;

                    //OrderItemModel Price = Variant.AmountInclTax + Sum(Extra.AmountInclTax)
                    orderItemModel.Price += p.Variant.AmountInclTax * p.Quantity;
                    orderItemModel.Price += p.Extras.Sum(e => e.AmountInclTax * p.Quantity);

                    //OrderItemModel PriceWithoutDiscount = its Price * _discount
                    orderItemModel.PriceWithoutDiscount = orderItemModel.Price * _discount;

                    //OrderItemModel Initial Price = (Variant.AmountExclTax + Sum(Extra.AmountExclTax)) * _discount 
                    orderItemModel.ItemInitialPrice = 0;
                    orderItemModel.ItemInitialPrice += p.Variant.AmountExclTax * _discount;
                    p.Extras.ForEach(e =>
                    {
                        orderItemModel.ItemInitialPrice += e.AmountExclTax * _discount;
                    });
                    orderItemModel.ItemInitialPrice *= p.Quantity;
                    if (orderItemModel.ItemInitialPrice == 0)
                    {
                    orderItemModel.ItemInitialPrice = (p.Variant.NetValue + p.Extras.Sum(e => e.NetValue)) * p.Quantity;
                    }

                    //Taxes List Info
                    orderItemModel.Taxes = new List<Tax>();
                    
                    p.Variant.Taxes?.ForEach(t =>
                    {
                        t.AmountTax *= p.Quantity;
                        //t.AmountTax = t.AmountTax * (1 - orderItemModel.Discount / 100);
                        if (orderItemModel.Taxes.Any(x => x.TraderId == t.TraderId))
                        {
                            var taxitem = orderItemModel.Taxes.FirstOrDefault(x => x.TraderId == t.TraderId);
                            taxitem.AmountTax += t.AmountTax;
                        }
                        else
                        {
                            orderItemModel.Taxes.Add(t);
                        }
                    });

                    p.Extras.ForEach(extraItem =>
                    {
                        extraItem.Taxes?.ForEach(t =>
                        {
                            t.AmountTax *= p.Quantity;
                            //t.AmountTax = t.AmountTax * (1 - orderItemModel.Discount / 100);
                            if (orderItemModel.Taxes.Any(x => x.TraderId == t.TraderId))
                            {
                                var taxitem = orderItemModel.Taxes.FirstOrDefault(x => x.TraderId == t.TraderId);
                                taxitem.AmountTax += t.AmountTax;
                            }
                            else
                            {
                                orderItemModel.Taxes.Add(t);
                            }
                        });
                    });

                    orderItemModel.TaxInfo = "";
                    if (orderItemModel.Taxes == null || orderItemModel.Taxes.Count <= 0)
                    {
                        orderItemModel.TaxInfo = "--";
                    }
                    else
                    {
                        var htmlString = "";
                        htmlString += "<ul id='taxes" + p.Id + "' class='unstyled'>";
                        foreach (var taxitem in orderItemModel.Taxes)
                        {

                            htmlString += "<li>";
                            htmlString += currencySetting.CurrencySymbol + taxitem.AmountTax.ToDecimalPlace(currencySetting);
                            htmlString += "<small><i>(";
                            htmlString += taxitem.TaxName;
                            htmlString += ")</i></small></li>";

                        }
                        htmlString += "</ul>";
                        //htmlString += '<input type="hidden" value="' + row.TaxInfo + '" id="taxname' + row.ItemId + '" />';
                        orderItemModel.TaxInfo = htmlString;
                    }

                    //Price html String
                    var priceString = "<input itemId='" + p.Id + "' " + (_isAllowEdit ? "" : "disabled") + " type='number' id='itemprice" + p.Id + "' class='form-control itemprice" + p.Id + "' value=\'" + orderItemModel.Price.ToDecimalPlace(currencySetting).Replace(",", "") + "\' min=\"0\" oninput=\"validity.valid||(value='0')\";/>";
                    priceString += "<input type='hidden' value='" + orderItemModel.PriceWithoutDiscount + "' id='pureprice" + p.Id + "'/>";
                    orderItemModel.PriceString = priceString;

                    if (isViewBuy)
                    {
                        var b2bItem = tradeOrder.TradingItems.FirstOrDefault(s => s.Variant.Id == p.Variant.TraderId);
                        if (b2bItem != null)
                        {
                            orderItemModel.AssociatedItem = new Select2Option
                            {
                                id = b2bItem.ConsumerDomainItem?.Id.ToString(),
                                text = b2bItem.ConsumerDomainItem?.Name,
                                selected = true
                            };
                            orderItemModel.AssociatedUnits = b2bItem.ConsumerDomainItem.Units.Select(x => new Select2Option
                            {
                                id = x.Id.ToString(),
                                text = x.Name,
                                selected = (b2bItem.ConsumerUnit != null && x.Id == b2bItem.ConsumerUnit.Id ? true : false)
                            }).ToList();
                        }

                    }

                    resultList.Add(orderItemModel);
                }));
                return resultList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, orderId, keySearch, catIds, requestModel, totalRecord, start, length);
                return new List<B2BCartItemModel>();
            }
        }
        public ReturnJsonModel GetListTraderItem(int domainId, string search, int itemId)
        {
            var returnJson = new ReturnJsonModel() { result = true };
            try
            {
                if (itemId == 0)
                {

                    var query = from it in dbContext.TraderItems
                                where it.Domain.Id == domainId
                                && it.IsBought
                                select it;
                    if (!string.IsNullOrEmpty(search))
                    {
                        search = search.ToLower();
                        query = query.Where(q => q.Name.ToLower().Contains(search) || q.SKU.ToLower().Contains(search));
                    }

                    returnJson.Object = query.ToList().Select(s => new
                    {
                        id = s.Id,
                        text = $"{s.SKU} - {s.Name}",
                        units = s.Units.Select(u => new { id = u.Id, text = u.Name }).ToList(),
                        itemname = s.Name,
                        sku = s.SKU
                    }).ToList();
                }
                else
                {
                    var item = dbContext.TraderItems.Find(itemId);
                    returnJson.Object = new
                    {
                        id = item.Id,
                        text = $"{item.SKU} - {item.Name}",
                        units = item.Units.Select(u => new { id = u.Id, text = u.Name }).ToList(),
                        itemname = item.Name,
                        sku = item.SKU
                    };
                }


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                returnJson.result = false;
            }
            return returnJson;

        }

        /// <summary>
        /// The buyers item should be found by checking if there is a TraderItem in the buyer’s Domain that has the same SKU as the item selected from the seller’s catalog.
        /// If the item is not in the buyer’s Domain so nothing should be shown.
        /// </summary>
        /// <param name="sellingDomainId"></param>
        /// <param name="buyingDomainId"></param>
        /// <param name="sku"></param>
        /// <returns></returns>
        public TraderItem GetDefaultAssociatedTraderItem(int sellingDomainId, int buyingDomainId, string sku)
        {
            var returnJson = new ReturnJsonModel() { result = true };
            try
            {
                var itemsAssociated = dbContext.TraderItems.Where(it => (it.Domain.Id == sellingDomainId || it.Domain.Id == buyingDomainId) && it.SKU == sku).ToList();
                if (itemsAssociated.Count == 2)
                    return itemsAssociated.FirstOrDefault(e => e.Domain.Id == buyingDomainId);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public TraderItem FindAssociatedTraderItem(int sellingDomainId, int buyingDomainId, string itemBarcode, string itemSku)
        {
            var returnJson = new ReturnJsonModel() { result = true };
            try
            {
                if (itemBarcode.IsNullOrEmpty() && itemSku.IsNullOrEmpty())
                {
                    return null;
                }
                var itemsAssociated = dbContext.TraderItems.Where(it => it.Domain.Id == buyingDomainId && it.Barcode == itemBarcode).ToList();
                if (itemsAssociated.IsEmpty())
                {
                    itemsAssociated = dbContext.TraderItems.Where(it => it.Domain.Id == buyingDomainId && it.SKU == itemSku).ToList();
                }
                if (!itemsAssociated.IsEmpty()) { return itemsAssociated.FirstOrDefault(); }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public ReturnJsonModel UpdateQuantityOrderItem(int tradeOrderId, Item updatedItem)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrderId, updatedItem);

                var _tradeOrder = dbContext.TradeOrders.Find(tradeOrderId);
                var _parsedOrder = JsonHelper.ParseAs<Order>(_tradeOrder.OrderJson);
                decimal totalPrice = 0;
                decimal amountInclTax = 0;
                decimal amountExclTax = 0;
                _parsedOrder.Items.ForEach(it =>
                {
                    if (it.Id == updatedItem.Id)
                        it.Quantity = updatedItem.Quantity;
                    amountInclTax += it.Variant.AmountInclTax * it.Quantity;
                    amountExclTax += it.Variant.AmountExclTax * it.Quantity;
                    totalPrice += amountInclTax;
                    it.Extras.ForEach(e =>
                    {
                        amountInclTax += e.AmountInclTax * it.Quantity;
                        amountExclTax += e.AmountExclTax * it.Quantity;
                        totalPrice += amountInclTax;
                    });
                });
                _parsedOrder.AmountInclTax = amountInclTax;
                _parsedOrder.AmountExclTax = amountExclTax;
                _parsedOrder.AmountTax = amountInclTax - amountExclTax;

                if (_tradeOrder.IsAgreedByBusiness && _tradeOrder.IsAgreedByCustomer)
                {
                    resultJson.result = true;
                    resultJson.actionVal = -1;
                    resultJson.Object = totalPrice;
                    return resultJson;
                }
                //B2COrderReCalculateAmounts(ref _parsedOrder);

                var _newOrderJson = JsonHelper.ToJson(_parsedOrder);
                _tradeOrder.OrderJson = _newOrderJson;
                _tradeOrder.IsAgreedByBusiness = false;
                _tradeOrder.IsAgreedByCustomer = false;
                _tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;

                dbContext.Entry(_tradeOrder).State = EntityState.Modified;
                dbContext.SaveChanges();
                resultJson.msgName = _tradeOrder.BuyingDomain.Id.BusinesProfile().BusinessName;
                resultJson.result = true;
                resultJson.Object = totalPrice;

                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId, updatedItem);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
        public ReturnJsonModel UpdateDiscountOrderItem(int tradeOrderId, Item updatedItem)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrderId, updatedItem);
                var _tradeOrder = dbContext.TradeOrders.Find(tradeOrderId);
                var _parsedOrder = JsonHelper.ParseAs<Order>(_tradeOrder.OrderJson);
                decimal totalPrice = 0;
                decimal amountInclTax = 0;
                decimal amountExclTax = 0;
                _parsedOrder.Items.ForEach(it =>
                {
                    decimal oldDiscount = 0;
                    if (it.Id == updatedItem.Id)
                    {
                        oldDiscount = it.Variant.Discount == 100 ? 0 : 1 / (1 - it.Variant.Discount / 100);
                        it.Variant.Discount = updatedItem.Variant.Discount;
                        
                        // B2B QBIC-5004 Incase discount = 100, all AmountExclTax, AmountInclTax will be 0 and can't be change anymore.
                        // To avoid that, using Net and Gross value to caculator.
                        if (oldDiscount == 0)
                        {
                            it.Variant.Taxes.ForEach(t =>
                            {
                                t.AmountTax = it.Variant.NetValue * t.TaxRate / 100 * (1 - it.Variant.Discount / 100);
                            });

                            it.Extras.ForEach(x =>
                            {
                                x.Discount = updatedItem.Variant.Discount;
                                x.AmountExclTax = x.NetValue * (1 - x.Discount / 100);
                                x.Taxes.ForEach(t =>
                                {
                                    t.AmountTax = x.NetValue * t.TaxRate / 100 * (1 - x.Discount / 100);
                                });
                                x.TaxAmount = x.Taxes.Sum(t => t.AmountTax);
                                x.AmountInclTax = x.AmountExclTax + x.TaxAmount;
                                x.TotalAmountWithoutDiscount = x.AmountInclTax * it.Quantity;
                                x.TotalAmount = x.TotalAmountWithoutDiscount * (1 - x.Discount / 100);
                            });

                            //Re-calculate variant amounts
                            it.Variant.AmountExclTax = it.Variant.NetValue * (1 - it.Variant.Discount / 100);
                            it.Variant.AmountInclTax = it.Variant.GrossValue * (1 - it.Variant.Discount / 100);

                            it.Variant.TotalAmountWithoutDiscount = it.Variant.GrossValue * it.Quantity;
                            it.Variant.TotalAmount = it.Variant.TotalAmountWithoutDiscount * (1 - it.Variant.Discount / 100);

                            // delta must less than 0.001 (casuse of round number)
                            var checkAmountInclTax = it.Variant.NetValue * (1 - it.Variant.Discount / 100) + it.Variant.Taxes.Sum(t => t.AmountTax) + it.Extras.Sum(t => t.TaxAmount);
                            var delta = Math.Abs(checkAmountInclTax - it.Variant.AmountInclTax);
                        }
                        else
                        {
                            it.Variant.Taxes.ForEach(t =>
                            {
                                t.AmountTax = t.AmountTax * oldDiscount * (1 - it.Variant.Discount / 100);
                            });

                            it.Extras.ForEach(x =>
                            {
                                x.Discount = updatedItem.Variant.Discount;
                                x.AmountInclTax = x.AmountInclTax * oldDiscount * (1 - it.Variant.Discount / 100);
                                x.AmountExclTax = x.AmountExclTax * oldDiscount * (1 - it.Variant.Discount / 100);
                                x.Taxes.ForEach(t =>
                                {
                                    t.AmountTax = t.AmountTax * oldDiscount * (1 - it.Variant.Discount / 100);
                                });
                                x.TaxAmount = x.Taxes.Sum(e => e.AmountTax);
                                x.TotalAmountWithoutDiscount = x.GrossValue * it.Quantity;
                                x.TotalAmount = x.TotalAmountWithoutDiscount * (1 - x.Discount / 100);
                            });


                            //Re-calculate variant amounts
                            it.Variant.AmountInclTax = it.Variant.AmountInclTax * oldDiscount * (1 - it.Variant.Discount / 100);
                            it.Variant.AmountExclTax = it.Variant.AmountExclTax * oldDiscount * (1 - it.Variant.Discount / 100);

                            it.Variant.TotalAmountWithoutDiscount = it.Variant.GrossValue * it.Quantity;
                            it.Variant.TotalAmount = it.Variant.TotalAmountWithoutDiscount * (1 - it.Variant.Discount / 100);
                        }
                       

                    }
                    amountInclTax += it.Variant.AmountInclTax * it.Quantity;
                    amountExclTax += it.Variant.AmountExclTax * it.Quantity;
                    totalPrice += amountInclTax;
                    it.Extras.ForEach(e =>
                    {
                        amountInclTax += e.AmountInclTax * it.Quantity;
                        amountExclTax += e.AmountExclTax * it.Quantity;
                        totalPrice += amountInclTax;
                    });
                });
                _parsedOrder.AmountInclTax = amountInclTax;
                _parsedOrder.AmountExclTax = amountExclTax;
                _parsedOrder.AmountTax = amountInclTax - amountExclTax;

                if (_tradeOrder.IsAgreedByBusiness && _tradeOrder.IsAgreedByCustomer)
                {
                    resultJson.result = true;
                    resultJson.actionVal = -1;
                    resultJson.Object = totalPrice;
                    return resultJson;
                }

                var _newOrderJson = JsonHelper.ToJson(_parsedOrder);
                _tradeOrder.OrderJson = _newOrderJson;
                _tradeOrder.IsAgreedByBusiness = false;
                _tradeOrder.IsAgreedByCustomer = false;
                _tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;

                dbContext.Entry(_tradeOrder).State = EntityState.Modified;
                dbContext.SaveChanges();
                resultJson.msgName = _tradeOrder.SellingDomain.Id.BusinesProfile().BusinessName;
                resultJson.result = true;
                resultJson.Object = totalPrice;

                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId, updatedItem);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
        public ReturnJsonModel UpdateAssociatedItem(int tradeOrderId, int itemId, int unitId, int variantId)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrderId, itemId, unitId);

                var _tradeOrder = dbContext.B2BTradeOrders.Find(tradeOrderId);
                if (_tradeOrder != null)
                {
                    var item = _tradeOrder.TradingItems.FirstOrDefault(s => s.Variant.Id == variantId);
                    if (item != null)
                    {
                        item.ConsumerDomainItem = dbContext.TraderItems.Find(itemId);
                        item.ConsumerUnit = dbContext.ProductUnits.Find(unitId);
                    }
                    else
                    {
                        var b2bTradingItem = new B2BTradingItem();
                        b2bTradingItem.RelatedOrder = _tradeOrder;
                        b2bTradingItem.CreatedBy = _tradeOrder.CreatedBy;
                        b2bTradingItem.CreatedDate = DateTime.UtcNow;
                        b2bTradingItem.ConsumerDomainItem = dbContext.TraderItems.Find(itemId);
                        b2bTradingItem.ConsumerUnit = dbContext.ProductUnits.Find(unitId);
                        b2bTradingItem.Variant = dbContext.PosVariants.Find(variantId);
                        dbContext.B2BTradingItems.Add(b2bTradingItem);
                    }
                }
                dbContext.SaveChanges();
                resultJson.result = true;

                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId, itemId, unitId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
        public ReturnJsonModel RemoveItemFromOrder(int disId, int itemId)
        {
            var result = new ReturnJsonModel() { actionVal = 2, Object = 0 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, disId, itemId);

                var discussion = dbContext.B2BOrderCreations.Find(disId);

                var _tradeOrder = discussion.TradeOrder;
                if (_tradeOrder.IsAgreedByBusiness && _tradeOrder.IsAgreedByCustomer)
                {
                    result.result = true;
                    result.actionVal = -1;
                    return result;
                }
                var _order = JsonHelper.ParseAs<Order>(_tradeOrder.OrderJson);
                //Calculate price of item to remove
                var _itemToRemove = _order.Items.FirstOrDefault(p => p.Id == itemId);
                decimal _itemAmountInclTax = 0;
                decimal _itemAmountExclTax = 0;
                _itemAmountInclTax += _itemToRemove.Variant.AmountInclTax;
                _itemAmountExclTax += _itemToRemove.Variant.AmountExclTax;
                _itemToRemove.Extras.ForEach(extraItem =>
                {
                    _itemAmountInclTax += extraItem.AmountInclTax;
                    _itemAmountExclTax += extraItem.AmountExclTax;
                });
                _order.AmountInclTax -= _itemAmountInclTax * _itemToRemove.Quantity;
                _order.AmountExclTax -= _itemAmountExclTax * _itemToRemove.Quantity;
                _order.AmountTax = _order.AmountInclTax - _order.AmountExclTax;

                _order.Items.Remove(_order.Items.FirstOrDefault(p => p.Id == itemId));

                var _orderJson = JsonHelper.ToJson(_order);
                _tradeOrder.OrderJson = _orderJson;
                _tradeOrder.IsAgreedByBusiness = false;
                _tradeOrder.IsAgreedByCustomer = false;
                _tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;
                dbContext.Entry(_tradeOrder).State = EntityState.Modified;
                dbContext.SaveChanges();
                result.result = true;
                result.Object = _order.AmountInclTax;
                result.msgName = _tradeOrder.BuyingDomain.Id.BusinesProfile().BusinessName;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, disId, itemId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }
        public ReturnJsonModel BuyingDomainSubmitProposal(B2BSubmitProposal proposal)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, proposal);

                var discussion = dbContext.B2BOrderCreations.Find(proposal.discussionId);
                var tradeOrder = discussion.TradeOrder;
                if (tradeOrder.IsAgreedByBusiness && tradeOrder.IsAgreedByCustomer)
                {
                    resultJson.result = false;
                    resultJson.msg = "ERROR_MSG_CANNOT_CHANGED";
                    return resultJson;
                }
                tradeOrder.DestinationLocation = dbContext.TraderLocations.Find(proposal.destinationLocationId);
                tradeOrder.IsAgreedByCustomer = true;
                tradeOrder.PurchaseWorkGroup = dbContext.WorkGroups.Find(proposal.purchaseWGId);
                tradeOrder.BillWorkGroup = dbContext.WorkGroups.Find(proposal.invoiceWGId);
                tradeOrder.PurchaseTransferWorkGroup = dbContext.WorkGroups.Find(proposal.transferWGId);
                tradeOrder.PurchasePaymentWorkGroup = dbContext.WorkGroups.Find(proposal.paymentWGId);
                tradeOrder.PurchasePaymentAccount = dbContext.TraderCashAccounts.Find(proposal.paymentAccId);


                var customerUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == proposal.CurrentUserId);
                tradeOrder.Customer = customerUser;

                //All TradeOrder for B2B have a delivery method
                tradeOrder.DeliveryMethod = DeliveryMethodEnum.Delivery;



                dbContext.Entry(tradeOrder).State = EntityState.Modified;
                dbContext.SaveChanges();
                resultJson.msgName = tradeOrder.BuyingDomain.Id.BusinesProfile().BusinessName;
                resultJson.Object = new { sellingName = tradeOrder.SellingDomain.Id.BusinesProfile().BusinessName };
                resultJson.result = true;

                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, proposal);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
        public ReturnJsonModel SellingDomainSubmitProposal(int disId)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, disId);

                var discussion = dbContext.B2BOrderCreations.Find(disId);
                var _tradeOrder = discussion.TradeOrder;
                if (_tradeOrder.IsAgreedByBusiness && _tradeOrder.IsAgreedByCustomer)
                {
                    resultJson.result = false;
                    resultJson.msg = "ERROR_MSG_CANNOT_CHANGED";
                    return resultJson;
                }
                _tradeOrder.IsAgreedByBusiness = true;
                dbContext.Entry(_tradeOrder).State = EntityState.Modified;
                dbContext.SaveChanges();
                resultJson.msgName = _tradeOrder.SellingDomain.Id.BusinesProfile().BusinessName;
                resultJson.Object = new { buyingName = _tradeOrder.BuyingDomain.Id.BusinesProfile().BusinessName };
                resultJson.result = true;

                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, disId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
        public TradeOrderB2B GetTradeOrderById(int orderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, orderId);
                return dbContext.B2BTradeOrders.Find(orderId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, orderId);
                return new TradeOrderB2B();
            }
        }
        public ReturnJsonModel ProcessB2BOrder(B2BSubmitProposal proposal)
        {
            var resultJson = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, proposal);

                var linkedOrderId = Guid.NewGuid().ToString();

                var tradeOrder = dbContext.TradeOrders.Find(proposal.tradeOrderId);

                if (!tradeOrder.IsAgreedByCustomer)
                {
                    resultJson.result = false;
                    resultJson.msg = "Customer does not agreed the order!";
                    return resultJson;
                }

                if (tradeOrder.TraderContact != null)
                {
                    tradeOrder.OrderCustomer = new OrderProcessingHelper(dbContext).MapTraderContact2OrderCustomer(tradeOrder.TraderContact);
                }
                else if (tradeOrder.Customer != null)
                {
                    var cAddress = tradeOrder.Customer.TraderAddresses.FirstOrDefault(e => e.IsDefault);
                    if (cAddress != null)
                    {
                        tradeOrder.OrderCustomer = new OrderCustomer
                        {
                            CustomerId = cAddress.Id,
                            Address = cAddress.ToAddress(),
                            CustomerName = tradeOrder.Customer.GetFullName(),
                            CustomerRef = $"ref #{tradeOrder.Customer.UserName}",
                            Email = tradeOrder.Customer.Email,
                            PhoneNumber = tradeOrder.Customer.PhoneNumber,
                            FullAddress = cAddress
                        };
                    }
                }


                var b2bOrderJson = tradeOrder.OrderJson.ParseAs<Order>();

                var cashier = dbContext.QbicleUser.Find(proposal.CurrentUserId);
                b2bOrderJson.Cashier = new Cashier
                {
                    TraderId = cashier.Id,
                    Avatar = cashier.ProfilePic.ToUri(),
                    Forename = cashier.Forename,
                    Surname = cashier.Surname
                };

                b2bOrderJson.Reference = tradeOrder.OrderReference.FullRef;
                b2bOrderJson.TradeOrderId = proposal.tradeOrderId;
                b2bOrderJson.LinkedTraderId = linkedOrderId;

                tradeOrder.OrderJson = b2bOrderJson.ToJson();
                tradeOrder.ProvisionalOrder = b2bOrderJson;

                tradeOrder.LinkedOrderId = linkedOrderId;
                tradeOrder.IsAgreedByBusiness = true;
                tradeOrder.SaleWorkGroup = dbContext.WorkGroups.Find(proposal.saleWGId);
                tradeOrder.InvoiceWorkGroup = dbContext.WorkGroups.Find(proposal.invoiceWGId);
                tradeOrder.PaymentWorkGroup = dbContext.WorkGroups.Find(proposal.paymentWGId);
                tradeOrder.TransferWorkGroup = dbContext.WorkGroups.Find(proposal.transferWGId);
                tradeOrder.PaymentAccount = dbContext.TraderCashAccounts.Find(proposal.paymentAccId);

                if (tradeOrder.IsAgreedByBusiness && tradeOrder.IsAgreedByCustomer)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.AwaitingProcessing;
                }
                tradeOrder.OrderStatus = TradeOrderStatusEnum.InProcessing;
                dbContext.SaveChanges();

                resultJson.result = true;
                B2BProcessOrderJob(tradeOrder.Id);
                dbContext.Entry(tradeOrder).State = EntityState.Modified;
                dbContext.SaveChanges();
                return resultJson;
            }
            catch (Exception ex)
            {
                // Log the information for the CURRENT USER
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, proposal);
                resultJson.result = false;
                resultJson.msg = ex.Message;
                return resultJson;
            }
        }
        public ReturnJsonModel ChangeB2BProfileUsabilityStatus(int profileId, bool isActive = false)
        {
            var result = new ReturnJsonModel() { actionVal = 2, result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, profileId, isActive);

                var b2bProfile = dbContext.B2BProfiles.Find(profileId);
                if (b2bProfile != null)
                {
                    b2bProfile.IsB2BServicesProvided = isActive;
                    dbContext.Entry(b2bProfile).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                result.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, profileId, isActive);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
            }
            return result;
        }
        private void B2BProcessOrderJob(int tradeOrderId)
        {
            var job = new OrderJobParameter
            {
                Id = tradeOrderId,

                EndPointName = "b2bprocessorder",
                Address = "",
                InvoiceDetail = ""
            };
            Task tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        public DataTablesResponse PromotionVoucches(IDataTablesRequest requestModel, string key, string search, string status, string dates, UserSetting user)
        {
            var lstCashAccountTransactionModel = new List<CashAccountTransactionModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel);

                var promotionId = int.Parse(key.Decrypt());
                var vouchers = dbContext.Vouchers.Where(e => e.Promotion.Id == promotionId);

                var startDate = new DateTime();
                var endDate = new DateTime();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);

                if (!string.IsNullOrEmpty(dates.Trim()))
                {
                    dates.ConvertDaterangeFormat(user.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);

                    vouchers = vouchers.Where(e => e.CreatedDate >= startDate && e.CreatedDate <= endDate);
                }


                if (!string.IsNullOrWhiteSpace(search))
                {
                    vouchers = vouchers.Where(g => g.Code.Contains(search) || g.ClaimedBy.UserName.Contains(search));
                }
                if (!string.IsNullOrWhiteSpace(status) && status != "0")
                {
                    var remd = status == "Yes";

                    vouchers = vouchers.Where(s => s.IsRedeemed == remd);
                }

                var totalVouches = vouchers.Count();
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Code":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Code" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Claimed":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "By":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "ClaimedBy.UserName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Redeemed":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "IsRedeemed" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Redemption":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "RedeemedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                vouchers = vouchers.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion

                #region Paging
                var list = vouchers.Skip(requestModel.Start).Take(requestModel.Length).ToList();


                #endregion

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Code,
                    Claimed = q.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString($"{user.DateFormat} {user.TimeFormat}"),
                    By = q.ClaimedBy.GetFullName(),
                    Redeemed = q.IsRedeemed ? "Yes" : "No",
                    Redemption = q.IsRedeemed ? q.RedeemedDate.ConvertTimeFromUtc(user.Timezone).ToString($"{user.DateFormat} {user.TimeFormat}") : "",
                    StatusCss = q.IsRedeemed ? "success" : "danger"
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalVouches, totalVouches);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel);
            }

            return null;
        }


        public string GetB2bBusinessNameById(int domainId)
        {
            return dbContext.B2BProfiles.Where(s => s.Domain.Id == domainId).AsNoTracking().FirstOrDefault().BusinessName;
        }

        public ReturnJsonModel B2BMatchSellerAndPurchaserTaxes(int traderItemId, int variantId)
        {
            var returnModel = new ReturnJsonModel() { result = false };

            var purchaseTaxRates = dbContext.TraderItems.FirstOrDefault(e => e.Id == traderItemId)?.TaxRates.FindAll(t => t.IsPurchaseTax == true).OrderBy(o => o.Rate).Select(e => e.Rate).ToList();
            var saleTaxRates = dbContext.PosVariants.FirstOrDefault(e => e.Id == variantId)?.Price?.Taxes.OrderBy(o => o.Rate).Select(e => e.Rate).ToList();

            if (purchaseTaxRates == null || saleTaxRates == null)
            {
                returnModel.result = false;
                returnModel.actionVal = 4;
            }
            else
            {
                //returnModel.result = Enumerable.SequenceEqual(purchaseTaxRates, saleTaxRates);
                returnModel.result = true;
                returnModel.actionVal = 1;
            }
            return returnModel;
        }

        public bool IsPartnershipAvailableToDomains(QbicleDomain providerDomain, QbicleDomain consumerDomain)
        {
            var queryDomain = dbContext.DomainPlans.Where(e => e.Domain.Id == providerDomain.Id || e.Domain.Id == consumerDomain.Id).All(t => t.Level.Level == BusinessDomainLevelEnum.Existing);
            return queryDomain;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="qbicleKey"></param>
        /// <param name="type">b2c - c2c</param>
        /// <param name="dateFormat"></param>
        /// <param name="timezone"></param>
        /// <param name="keyword"></param>
        /// <param name="daterange"></param>
        /// <param name="status"></param>
        /// <param name="orderBy">
        /// 0>Latest activity, 1> Date(recent first) 2>Date(oldest first)
        /// </param>
        /// <returns></returns>
        public DataTablesResponse GetOrderContextFlyoutB2B([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string qbicleKey, string type, string dateFormat, string timezone,
            string keyword = "", string daterange = "", List<int> status = null, int orderBy = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, daterange, keyword);

                int totalrecords = 0;
                #region Filters
                var qbicleId = qbicleKey.Decrypt2Int();
                IQueryable<B2BOrderCreation> query = dbContext.B2BOrderCreations.Where(e => e.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2BOrder && e.Qbicle.Id == qbicleId && e.TradeOrder != null);
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.TradeOrder.OrderReference.FullRef.Contains(keyword));
                }

                if (status != null)
                {
                    List<TradeOrderStatusEnum> enumList = status.Select(x => (TradeOrderStatusEnum)Enum.Parse(typeof(TradeOrderStatusEnum), x.ToString())).ToList();
                    query = query.Where(s => enumList.Contains(s.TradeOrder.OrderStatus));
                }
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFormat, timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.TimeLineDate >= startDate && s.TimeLineDate < endDate);
                }
                totalrecords = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();

                switch (orderBy)
                {
                    case 1:
                        query = query.OrderByDescending(e => e.TradeOrder.CreateDate);
                        break;
                    case 2:
                        query = query.OrderBy(e => e.TradeOrder.CreateDate);
                        break;
                    default:
                        query = query.OrderByDescending(e => e.TimeLineDate);
                        break;
                }
                #endregion
                #region Paging
                var orderCreations = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var tableContents = new List<string>();

                orderCreations.ForEach(o =>
                {
                    var order = o.TradeOrder.OrderJson.ParseAs<Order>();

                    var oPost = o.Posts.OrderByDescending(e => e.TimeLineDate).FirstOrDefault();

                    var lastUpdate = oPost?.TimeLineDate;
                    if (lastUpdate == null)
                        lastUpdate = o.TimeLineDate;

                    var tableContent = $"<a href='/Commerce/DiscussionOrder?disKey={o.Key}' target='_blank'>";
                    tableContent += $"<div class='order-summary'>";
                    tableContent += $"<div class='flexit'>";
                    tableContent += $"<div class='order--0'>";
                    tableContent += $"<h1>{o.TradeOrder?.OrderReference?.FullRef}</h1>";
                    tableContent += $"<label id='order-context-flyout-status-{o.Id}' class='label label-lg label-{o.TradeOrder.GetClass()}'>{o.TradeOrder.OrderStatus.GetDescription()}</label>";
                    tableContent += $"<small>{lastUpdate?.ConvertTimeFromUtc(timezone, dateFormat + " hh:mmtt").ToLower()}</small>";
                    tableContent += $"</div>";
                    tableContent += $"<div class='order--1'>";
                    tableContent += $"<div class='flexitems'>";

                    var img = "/Content/DesignStyle/img/item-placeholder.png";
                    if (!order.Items.Any())
                        tableContent += $"<div class='pimg' style=\"background-image: url('{img}');\">&nbsp;</div>";
                    else
                    {
                        var itemIndex = 1;
                        order.Items.ForEach(item =>
                        {
                            if (itemIndex > 2) return;
                            var itemImg = item.ImageUri.ToUriString(Enums.FileTypeEnum.Image);
                            if (item.ImageUri.Contains("retriever/getdocument") && item.ImageUri.Contains("="))
                                itemImg = item.ImageUri.Split('=')[1].ToUriString(Enums.FileTypeEnum.Image);
                            tableContent += $"<div class='pimg' style=\"background-image: url('{itemImg}');\">&nbsp;</div>";
                            itemIndex++;
                        });
                        if (order.Items.Count > 2)
                        {
                            tableContent += $"<div class='pimg andmore'>+{order.Items.Count - 2}</div>";
                        }
                    }
                    var message = oPost?.Message;
                    if (string.IsNullOrEmpty(message))
                        message = o.StartedBy.GetFullName() + " has created order";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"<div class='order--detes'>";
                    tableContent += $"<div class='well custom rounded' style='margin: 0; padding: 18px 20px 12px 20px;'>";
                    tableContent += $"<p style='margin: 0; padding: 0;'><strong><label>Last update:</label> </strong>{message}</p>";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"</a>";

                    tableContents.Add(tableContent);
                });


                #endregion
                var dataJson = tableContents.Select(q => new
                {
                    tableContent = q
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public bool CheckExistB2BOrders(string qbicleKey)
        {
            var qbicleId = qbicleKey.Decrypt2Int();
            IQueryable<B2BOrderCreation> query = dbContext.B2BOrderCreations.Where(e => e.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2BOrder && e.Qbicle.Id == qbicleId && e.TradeOrder != null);
            if (query.Any(e => e.Id > 0)) return true;
            return false;
        }

        public ReturnJsonModel CheckStatusB2BOrders(string DiscussionOrderKey)
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                var discussionOrderId = DiscussionOrderKey.Decrypt2Int();
                var discussion = new DiscussionsRules(dbContext).GetDiscussionByB2BOrderById(discussionOrderId);
                var statusOrder = discussion.TradeOrder.OrderStatus;
                var statusLabel = "";
                switch ((int)(statusOrder))
                {
                    case 0:
                        statusLabel = "label label-lg label-info";
                        break;
                    case 1:
                    case 2:
                        statusLabel = "label label-lg label-primary";
                        break;
                    case 3:
                        statusLabel = "label label-lg label-success";
                        break;
                    case 4:
                        statusLabel = "label label-lg label-danger";
                        break;
                }

                returnModel.Object = new
                {
                    statusOrder = statusOrder.GetDescription(),
                    labelOrder = statusLabel
                };
                returnModel.result = true;
                return returnModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, DiscussionOrderKey);
                returnModel.result = false;
                return returnModel;
            }
        }

        //OrderOrig - recheck AmountInclTax and AmountExclTax
        public void ValidateB2BOrder(Order b2bOrder)
        {
            //fix image of items 
            //string ApiGetDocumentUri = "getdocument?file=";
            //b2bOrder.Items.Where(e => e?.ImageUri?.IndexOf(ApiGetDocumentUri) > 0).ForEach(e =>
            //{
            //    e.ImageUri = e.ImageUri.Remove(0, e.ImageUri.IndexOf(ApiGetDocumentUri) + ApiGetDocumentUri.Length);
            //});

            //Caculator OrderOrig

            {
                var lstItems = b2bOrder.Items;
                b2bOrder.AmountInclTax = 0;
                b2bOrder.AmountExclTax = 0;
                foreach (var item in lstItems)
                {
                    decimal itemInclTax = 0;
                    decimal itemExclTax = 0;

                    itemInclTax += (item?.Variant?.AmountInclTax ?? 0) * (item?.Quantity ?? 0);
                    itemExclTax += (item?.Variant?.AmountExclTax ?? 0) * (item?.Quantity ?? 0);
                    //QBIC-5074 Overlay item display the price is incorrectly
                    item.Variant.TotalAmountWithoutDiscount = item.Variant.AmountInclTax * (item?.Quantity ?? 0);
                    item.Variant.TotalAmount = item.Variant.TotalAmountWithoutDiscount * (1 - item.Variant.Discount / 100);
                    if (item.Extras != null)
                    {
                        foreach (var extraItem in item.Extras)
                        {
                            extraItem.TotalAmountWithoutDiscount = extraItem.AmountInclTax * item.Quantity;
                            extraItem.TotalAmount = extraItem.TotalAmountWithoutDiscount * (1 - item.Variant.Discount / 100); 
                            itemInclTax += extraItem.AmountInclTax * item.Quantity;
                            itemExclTax += extraItem.AmountExclTax * item.Quantity;
                        }
                    }
                    b2bOrder.AmountInclTax += itemInclTax;
                    b2bOrder.AmountExclTax += itemExclTax;
                }
                b2bOrder.AmountTax = b2bOrder.AmountInclTax - b2bOrder.AmountExclTax;
            }
        }
    }
}
