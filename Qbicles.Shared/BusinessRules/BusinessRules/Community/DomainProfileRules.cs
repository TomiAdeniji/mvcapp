using Newtonsoft.Json;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.Community;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules.Community
{
    public class DomainProfileRules
    {
        private ApplicationDbContext _db;

        public DomainProfileRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }

        public DomainProfile GetDomainProfile(int domainId)
        {
            return DbContext.DomainProfiles.FirstOrDefault(d =>
                       d.Domain.Id == domainId && d.PageType == CommunityPageTypeEnum.DomainProfile) ??
                   new DomainProfile();
        }

        public List<int> Search(List<int> lstId)
        {
            var modelSearch = DbContext.DomainProfiles.Where(d => d.IsSuspended == false)
                .Select(q => new { q.Id, tag = q.Tags.Select(x => x.Id).ToList() });
            if (!modelSearch.Any()) return new List<int>();
            {
                var domainProfiles = modelSearch.ToList()
                    .Where(q => q.tag.Any() && q.tag.Where(x => lstId.Contains(x)).Any());
                return domainProfiles.Any() ? domainProfiles.ToList().Select(q => q.Id).ToList() : new List<int>();
            }

        }

        public List<DomainProfile> GetDomainProfileByListId(List<int> lstId)
        {
            return DbContext.DomainProfiles.Where(d =>
                    d.PageType == CommunityPageTypeEnum.DomainProfile && lstId.Contains(d.Id) && d.IsSuspended == false)
                .ToList();
        }

        public List<DomainProfile> GetByUser(string userId)
        {
            return DbContext.DomainProfiles.Where(q => q.Followers.Any(x => x.Id == userId)).ToList();
        }

        public bool CommunityChangeFeatureDomainImage(MediaModel media, int domainId)
        {
            var domainProfile = GetDomainProfile(domainId);
            domainProfile.StoredFeaturedImageName = media.UrlGuid;
            if (DbContext.Entry(domainProfile).State == EntityState.Detached)
                DbContext.DomainProfiles.Attach(domainProfile);
            DbContext.Entry(domainProfile).State = EntityState.Modified;
            DbContext.SaveChanges();
            return true;
        }

        public bool CommunityUpdateLogo(MediaModel media, int domainId)
        {
            var domainProfile = GetDomainProfile(domainId);
            domainProfile.StoredLogoName = media.UrlGuid;
            if (DbContext.Entry(domainProfile).State == EntityState.Detached)
                DbContext.DomainProfiles.Attach(domainProfile);
            DbContext.Entry(domainProfile).State = EntityState.Modified;
            DbContext.SaveChanges();
            return true;
        }

        public DomainProfile SaveAndFinish(DomainProfile domainProfile, List<Location> locations
            , List<CommunityPage> pages
            , int domainId, string userId)
        {
            if (domainProfile == null) return domainProfile;
            var user = DbContext.QbicleUser.Find(userId);
            var dProfile = GetDomainProfile(domainId);
            dProfile.StrapLine = domainProfile.StrapLine;
            dProfile.ProfileText = domainProfile.ProfileText;
            //safe validation entity
            if (dProfile.Domain == null)
                dProfile.Domain = new DomainRules(DbContext).GetDomainById(domainId);
            if (dProfile.CreatedBy == null)
                dProfile.CreatedBy = user;
            if (string.IsNullOrEmpty(dProfile.StoredLogoName))
                dProfile.StoredLogoName = HelperClass.ImageNotFoundUrl;
            if (string.IsNullOrEmpty(dProfile.StoredFeaturedImageName))
                dProfile.StoredFeaturedImageName = HelperClass.ImageNotFoundUrl;
            if (dProfile.Id == 0)
            {
                dProfile.CreatedDate = DateTime.UtcNow;
                DbContext.DomainProfiles.Add(dProfile);
            }
            else
            {
                dProfile.LastUpdated = DateTime.UtcNow;
                if (DbContext.Entry(dProfile).State == EntityState.Detached)
                    DbContext.DomainProfiles.Attach(dProfile);
                DbContext.Entry(dProfile).State = EntityState.Modified;
            }

            // profile tags
            if (domainProfile.Tags != null)
            {
                if (dProfile.Tags == null)
                    dProfile.Tags = new List<Tag>();
                var tagsAdd = domainProfile.Tags.Select(q => q.Id).ToList();
                var tagsRemove = dProfile.Tags.Where(q => !tagsAdd.Contains(q.Id)).ToList();
                foreach (var tag in tagsRemove)
                {
                    var domainsProfileAll = DbContext.DomainProfiles.ToList();
                    domainsProfileAll.ForEach(e => e.Tags = e.Tags.Where(t => t.Id == tag.Id).ToList());
                    if (domainsProfileAll.Count == 0)
                        tag.IsDomainProfileTag = false;

                    dProfile.Tags.Remove(tag);
                }

                // add new tags
                var lstIdPageTags = dProfile.Tags.Select(q => q.Id).ToList();
                var itemAdd = domainProfile.Tags.Where(q => !lstIdPageTags.Contains(q.Id)).Select(q => q.Id)
                    .ToList();
                if (itemAdd.Count > 0)
                {
                    var tags = DbContext.Tags.Where(q => itemAdd.Contains(q.Id)).ToList();
                    foreach (var tag in tags)
                    {
                        tag.IsDomainProfileTag = true;
                        dProfile.Tags.Add(tag);
                    }
                }
            }

            //location
            if (locations != null && locations.Count > 0)
            {
                var oldLocations = new LocationRules(DbContext).GetLocations(dProfile.Id);
                DbContext.Locations.RemoveRange(oldLocations);
                var index = 1;
                foreach (var location in locations)
                {
                    location.Address = JsonConvert.DeserializeObject<string>(location.Address);
                    location.CreatedBy = user;
                    location.DomainProfile = dProfile;
                    location.EditedDate = DateTime.UtcNow;
                    location.DisplayOrder = index;
                    index++;
                    DbContext.Locations.Add(location);
                }
            }

            //page
            if (pages != null && pages.Count > 0)
                foreach (var page in pages)
                {
                    var commPage = new CommunityPageRules(DbContext).GetCommunityPage(page.Id);
                    commPage.IsDisplayedOnDomainProfile = page.IsDisplayedOnDomainProfile;
                    commPage.DisplayOrderOnDomainProfile = page.DisplayOrderOnDomainProfile;
                    if (DbContext.Entry(commPage).State == EntityState.Detached)
                        DbContext.CommunityPages.Attach(commPage);
                    DbContext.Entry(commPage).State = EntityState.Modified;
                }

            DbContext.SaveChanges();

            return domainProfile;
        }

        public bool FollowDomain(int id, string userId)
        {
            var applicationUser = DbContext.QbicleUser.Find(userId);
            var domainProfile = DbContext.DomainProfiles.Find(id);
            domainProfile?.Followers.Add(applicationUser);
            DbContext.SaveChanges();
            return true;
        }

        public bool UnFollowDomain(int id, string userId)
        {
            var applicationUser = DbContext.QbicleUser.Find(userId);
            var domainProfile = GetDomainProfileById(id);
            domainProfile.Followers.Remove(applicationUser);
            DbContext.SaveChanges();
            return true;
        }

        private DomainProfile GetDomainProfileById(int id)
        {
            return DbContext.DomainProfiles.Find(id);
        }

        public IQueryable<DomainProfile> GetAllDomainProfiles()
        {
            var domainProfiles = DbContext.DomainProfiles.Where(d => d.PageType == CommunityPageTypeEnum.DomainProfile);
            return !domainProfiles.Any() ? null : domainProfiles.OrderByDescending(q => q.CreatedDate);
        }

        public ReturnJsonModel BusinessProfileWizardGeneral(B2BProfileWizardModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, model);

                var domain = DbContext.Domains.FirstOrDefault(e => e.Id == model.DomainId);

                var isDomainAdmin = domain.Administrators.Any(p => p.Id == userId);
                if (!isDomainAdmin /*&& !new B2BWorkgroupRules(dbContext).GetCheckPermission(model.Domain.Id, model.CurrentUser.Id, B2bProcessesConst.ProfileEditing)*/)
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                    return returnJson;
                }
                // The email address for a business must be one of the email addresses associated with a member of the Domain
                var domainMemEmails = domain.Users == null ? new List<string>() : domain.Users.Select(t => t.Email).Distinct().ToList();
                if (!domainMemEmails.Contains(model.BusinessEmail))
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_BUSINESSEMAIL_NOT_IN_MEM_EMAIL");
                    return returnJson;
                }

                if (!string.IsNullOrEmpty(model.LogoUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);
                    s3Rules.ProcessingMediaS3(model.LogoUri);
                }

                var currentUser = DbContext.QbicleUser.Find(userId);

                var profile = DbContext.B2BProfiles.FirstOrDefault(s => s.Id == model.Id || s.Domain.Id == model.DomainId);

                domain.WizardStep = DomainWizardStep.General;
                if (profile != null)
                {
                    profile.BusinessName = model.BusinessName;
                    profile.BusinessSummary = model.BusinessSummary ?? model.BusinessName;
                    profile.BusinessEmail = model.BusinessEmail;
                    profile.IsDisplayedInB2BListings = model.IsDisplayedInB2BListings;
                    profile.IsDisplayedInB2CListings = model.IsDisplayedInB2CListings;
                    profile.LastUpdatedBy = currentUser;
                    profile.LastUpdatedDate = DateTime.UtcNow;

                    profile.DefaultB2BRelationshipManagers.Clear();
                    if (model.B2BManagers != null)
                    {
                        foreach (var item in model.B2BManagers)
                        {
                            var user = DbContext.QbicleUser.Find(item);
                            if (user != null)
                                profile.DefaultB2BRelationshipManagers.Add(user);
                        }
                    }

                    profile.DefaultB2CRelationshipManagers.Clear();
                    if (model.B2CManagers != null)
                    {
                        foreach (var item in model.B2CManagers)
                        {
                            var user = DbContext.QbicleUser.Find(item);
                            if (user != null)
                                profile.DefaultB2CRelationshipManagers.Add(user);
                        }
                    }

                    if (!string.IsNullOrEmpty(model.LogoUri))
                        profile.LogoUri = model.LogoUri;


                    var areasOperations = DbContext.B2BAreasOfOperation.Where(e => e.Profile.Id == profile.Id).ToList();
                    DbContext.B2BAreasOfOperation.RemoveRange(areasOperations);
                    DbContext.SaveChanges();

                    if (model.AreasOperation != null)
                        foreach (var item in model.AreasOperation)
                        {
                            if (!string.IsNullOrEmpty(item))
                                profile.AreasOperation.Add(new AreaOfOperation
                                {
                                    Profile = profile,
                                    Name = item,
                                    CreatedBy = currentUser,
                                    CreatedDate = DateTime.UtcNow
                                });
                        }
                    //tags
                    DbContext.B2BTags.RemoveRange(profile.Tags);
                    if (model.Tags != null)
                        foreach (var tagName in model.Tags.Take(8))
                        {
                            if (string.IsNullOrEmpty(tagName)) continue;
                            var tag = new B2BTag
                            {
                                B2BProfile = profile,
                                TagName = tagName
                            };
                            profile.Tags.Add(tag);
                        }
                    //Business Categories
                    profile.BusinessCategories.Clear();
                    if (model.Categories != null)
                        foreach (var catid in model.Categories)
                        {
                            var category = DbContext.BusinessCategories.FirstOrDefault(e => e.Id == catid);
                            if (category != null)
                                profile.BusinessCategories.Add(category);
                        }
                    DbContext.Entry(profile).State = EntityState.Modified;
                }
                else
                {
                    profile = new B2BProfile
                    {
                        BusinessName = model.BusinessName,
                        BusinessSummary = model.BusinessSummary ?? model.BusinessName,
                        BusinessEmail = model.BusinessEmail,
                        IsDisplayedInB2BListings = model.IsDisplayedInB2BListings,
                        IsDisplayedInB2CListings = model.IsDisplayedInB2CListings,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedBy = currentUser,
                        LastUpdatedDate = DateTime.UtcNow,
                        Domain = domain
                    };

                    if (!string.IsNullOrEmpty(model.LogoUri))
                        profile.LogoUri = model.LogoUri;

                    if (model.IsDisplayedInB2BListings)
                    {
                        profile.DefaultB2BRelationshipManagers.Add(currentUser);
                    }

                    if (model.IsDisplayedInB2CListings)
                    {
                        profile.DefaultB2CRelationshipManagers.Add(currentUser);
                    }
                    if (model.AreasOperation != null)
                        foreach (var item in model.AreasOperation)
                        {
                            if (!string.IsNullOrEmpty(item))
                                profile.AreasOperation.Add(new AreaOfOperation
                                {
                                    Profile = profile,
                                    Name = item,
                                    CreatedBy = currentUser,
                                    CreatedDate = DateTime.UtcNow
                                });
                        }

                    // profile tags

                    DbContext.B2BTags.RemoveRange(profile.Tags);
                    if (model.Tags != null)
                        foreach (var tagName in model.Tags.Take(8))
                        {
                            if (string.IsNullOrEmpty(tagName)) continue;
                            var tag = new B2BTag
                            {
                                B2BProfile = profile,
                                TagName = tagName
                            };
                            profile.Tags.Add(tag);
                        }
                    //Business Categories
                    profile.BusinessCategories.Clear();
                    if (model.Categories != null)
                        foreach (var catid in model.Categories)
                        {
                            var category = DbContext.BusinessCategories.FirstOrDefault(e => e.Id == catid);
                            if (category != null)
                                profile.BusinessCategories.Add(category);
                        }
                    //When a Business Profile is created I think you should Add all TraderLocations for a Domain to your property BusinessLocations
                    #region Add default TraderLocations
                    var locations = new TraderLocationRules(DbContext).GetTraderLocation(model.DomainId);
                    if (locations != null && locations.Any())
                        profile.BusinessLocations = locations;
                    #endregion
                    #region Add default Catalogues
                    var catalogues = new B2CRules(DbContext).GetCatalogsByDomainId(model.DomainId);
                    if (catalogues != null && catalogues.Any())
                    {
                        profile.BusinessCatalogues = catalogues;
                    }
                    #endregion
                    DbContext.B2BProfiles.Add(profile);
                    DbContext.Entry(profile).State = EntityState.Added;
                }
                domain.Name = model.BusinessName;
                DbContext.SaveChanges();
                returnJson.result =  true;
                returnJson.msgId = profile.Id.ToString();
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
            }
            return returnJson;
        }

        public ReturnJsonModel FinishBusinessProfileWizard(string domainKey, bool fromWeb)
        {
            var domainId = domainKey.Decrypt2Int();
            var domain = DbContext.Domains.FirstOrDefault(e => e.Id == domainId);

            if (fromWeb)
            {
                domain.IsBusinessProfileWizard = true;
                domain.WizardStep = DomainWizardStep.Done;
            }
            else
            {
                domain.IsBusinessProfileWizardMicro = true;
                domain.WizardStepMicro = DomainWizardStepMicro.Done;
            }

            DbContext.SaveChanges();
            return new ReturnJsonModel() { result = true };
        }


        public object GetDomainUsersInvitation(IDataTablesRequest requestModel, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, domainId);


                var query = DbContext.Invitations.Where(p => p.Domain.Id == domainId);

                var totals = query.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Email":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Email" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion

                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();


                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Email,
                    Status = q.Status.GetDescription(),
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totals, totals);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId);
                return null;
            }
        }

        public bool ValidationBusinessName(string name, string id)
        {
            var pId = int.Parse(id);
            if (pId == 0)
                return !DbContext.B2BProfiles.Any(e => e.BusinessName.Equals(name, StringComparison.OrdinalIgnoreCase) && e.Domain.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return !DbContext.B2BProfiles.Any(e => e.Id != pId && e.BusinessName.Equals(name, StringComparison.OrdinalIgnoreCase) && e.Domain.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

    }
}