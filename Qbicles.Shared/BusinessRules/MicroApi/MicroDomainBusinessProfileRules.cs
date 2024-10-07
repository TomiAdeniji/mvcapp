using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Qbicles.BusinessRules.MicroApi
{
    public class MicroDomainBusinessProfileRules : MicroRulesBase
    {
        public MicroDomainBusinessProfileRules(MicroContext microContext) : base(microContext)
        {
        }
        public object GetMicroFirstLaunchedBusiness(string key)
        {
            var domainId = int.Parse(key.Decrypt());
            var profile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId) ?? new Models.B2B.B2BProfile();
            var domain = profile.Domain ?? dbContext.Domains.FirstOrDefault(e => e.Id == domainId);

            var categories = (new AdminListingRules(dbContext).GetAllBusinessCategories() ?? new List<BusinessCategory>()).Select(e => new Select2CustomModel { id = e.Id, text = e.Name });
            var operations = new CountriesRules().GetAllCountries().Select(e => new SelectOption { id = e.CommonName, text = e.CommonName });
            var managers = domain.Users.Select(e => new SelectOption { id = e.Id, text = e.GetFullName() });
            var invitations = dbContext.Invitations.Where(p => p.Domain.Id == domainId).ToList().Select(e => new InvitationModal { Email = e.Email, Status = e.Status.GetDescription(), StatusId = e.Status.GetId() });


            var wizard = new B2BProfileWizardModel
            {
                Id = profile.Id,
                DomainId = domain.Id,
                BusinessName = profile.BusinessName,
                BusinessEmail = profile.BusinessEmail,
                BusinessSummary = profile.BusinessSummary,
                LogoUri = profile.LogoUri.ToUriString(),
                IsB2BServicesProvided = profile.IsB2BServicesProvided,
                IsDisplayedInB2CListings = profile.IsDisplayedInB2CListings,
                IsDisplayedInB2BListings = profile.IsDisplayedInB2BListings,
                Tags = profile.Tags.Select(e => e.TagName).ToList(),
                AreasOperation = profile.AreasOperation.Select(e => e.Name).ToList(),
                B2BManagers = profile.DefaultB2BRelationshipManagers.Select(e => e.Id).ToList(),
                B2CManagers = profile.DefaultB2CRelationshipManagers.Select(e => e.Id).ToList(),
                Categories = profile.BusinessCategories.Select(e => e.Id).ToList(),
                WizardStep = domain.WizardStepMicro,
                Invitations = invitations.ToList()
            };


            return new
            {
                wizard,
                categories,
                operations,
                managers,
                invitations
            };
        }

        public ReturnJsonModel BusinessWizardAbout(B2BProfileWizardModel model)
        {
            var domain = dbContext.Domains.FirstOrDefault(e => e.Id == model.DomainId);

            var userId = CurrentUser.Id;

            var isDomainAdmin = domain.Administrators.Any(p => p.Id == userId);
            if (!isDomainAdmin /*&& !new B2BWorkgroupRules(dbContext).GetCheckPermission(model.Domain.Id, model.CurrentUser.Id, B2bProcessesConst.ProfileEditing)*/)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_28") };

            // The email address for a business must be one of the email addresses associated with a member of the Domain
            var domainMemEmails = domain.Users == null ? new List<string>() : domain.Users.Select(t => t.Email).Distinct().ToList();
            if (!domainMemEmails.Contains(model.BusinessEmail))
            {
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_BUSINESSEMAIL_NOT_IN_MEM_EMAIL") };
            }


            if (!string.IsNullOrEmpty(model.LogoUri))
            {
                var s3Rules = new Azure.AzureStorageRules(dbContext);
                s3Rules.ProcessingMediaS3(model.LogoUri);
            }

            var currentUser = CurrentUser;

            var profile = dbContext.B2BProfiles.FirstOrDefault(s => s.Id == model.Id || s.Domain.Id == model.DomainId);

            domain.WizardStepMicro = DomainWizardStepMicro.About;
            if (profile != null)
            {
                profile.BusinessName = model.BusinessName;
                profile.BusinessSummary = model.BusinessSummary;
                profile.BusinessEmail = model.BusinessEmail;
                profile.LastUpdatedBy = currentUser;
                profile.LastUpdatedDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(model.LogoUri))
                    profile.LogoUri = model.LogoUri;

            }
            else
            {
                profile = new B2BProfile
                {
                    BusinessName = model.BusinessName,
                    BusinessSummary = model.BusinessSummary,
                    BusinessEmail = model.BusinessEmail,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedBy = currentUser,
                    LastUpdatedDate = DateTime.UtcNow,
                    Domain = domain
                };
                if (!string.IsNullOrEmpty(model.LogoUri))
                    profile.LogoUri = model.LogoUri;
                //When a Business Profile is created should Add all TraderLocations for a Domain to your property BusinessLocations
                //Add default TraderLocations
                var locations = dbContext.TraderLocations.Where(d => d.Domain.Id == model.DomainId).ToList();
                if (locations != null && locations.Any())
                    profile.BusinessLocations = locations;

                //Add default Catalogues
                var catalogues = new B2CRules(dbContext).GetCatalogsByDomainId(model.DomainId);
                if (catalogues != null && catalogues.Any())
                    profile.BusinessCatalogues = catalogues;

                dbContext.B2BProfiles.Add(profile);

            }
            dbContext.SaveChanges();
            return new ReturnJsonModel { result = true, Object = profile.Id };
        }


        public ReturnJsonModel BusinessWizardAreas(B2BProfileWizardModel model)
        {
            if (model.Id == 0)
                return new ReturnJsonModel { result = false, msg = "Cannot find the associated profile of the business" };

            var userId = CurrentUser.Id;
            var domain = dbContext.Domains.FirstOrDefault(e => e.Id == model.DomainId);
            var isDomainAdmin = domain.Administrators.Any(p => p.Id == userId);
            if (!isDomainAdmin)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_28") };

            var currentUser = dbContext.QbicleUser.Find(userId);
            var profile = dbContext.B2BProfiles.FirstOrDefault(s => s.Id == model.Id || s.Domain.Id == model.DomainId);

            domain.WizardStepMicro = DomainWizardStepMicro.Areas;

            profile.IsDisplayedInB2BListings = model.IsDisplayedInB2BListings;
            profile.IsDisplayedInB2CListings = model.IsDisplayedInB2CListings;
            profile.LastUpdatedBy = currentUser;
            profile.LastUpdatedDate = DateTime.UtcNow;

            profile.DefaultB2BRelationshipManagers.Clear();
            if (model.B2BManagers != null)
            {
                foreach (var item in model.B2BManagers)
                {
                    var user = dbContext.QbicleUser.Find(item);
                    if (user != null)
                        profile.DefaultB2BRelationshipManagers.Add(user);
                }
            }
            profile.DefaultB2CRelationshipManagers.Clear();
            if (model.B2CManagers != null)
            {
                foreach (var item in model.B2CManagers)
                {
                    var user = dbContext.QbicleUser.Find(item);
                    if (user != null)
                        profile.DefaultB2CRelationshipManagers.Add(user);
                }
            }


            var areasOperations = dbContext.B2BAreasOfOperation.Where(e => e.Profile.Id == profile.Id).ToList();
            dbContext.B2BAreasOfOperation.RemoveRange(areasOperations);

            dbContext.SaveChanges();
            if (model.AreasOperation != null)
                foreach (var item in model.AreasOperation)
                {
                    profile.AreasOperation.Add(new AreaOfOperation
                    {
                        Profile = profile,
                        Name = item,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow
                    });
                }

            dbContext.SaveChanges();
            return new ReturnJsonModel { result = true };
        }

        public void BusinessWizardDone(int id, bool isExit)
        {
            var profile = dbContext.B2BProfiles.FirstOrDefault(s => s.Id == id);
            if (!isExit)
                profile.Domain.WizardStepMicro = DomainWizardStepMicro.Done;
            profile.Domain.IsBusinessProfileWizardMicro = true;
            dbContext.SaveChanges();
        }

        public MicroBusinessProfile MicroBusinessProfile(string domainKey, string userId)
        {
            var domainId = domainKey.Decrypt2Int();
            var profile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId);

            //var hasB2CConnect = Utility.CheckHasAccessB2C(profile.Domain.Id, userId);


            var domainBusiness = new List<int>();
            var qbicles = dbContext.B2CQbicles.Where(s => !s.IsHidden && s.Customer.Id == userId).ToList();
            qbicles.ForEach(q =>
            {
                if (q.RemovedForUsers.Count == 0)
                    domainBusiness.Add(q.Business.Id);
                else if (q.RemovedForUsers.Any(r => r.Id != userId))
                    domainBusiness.Add(q.Business.Id);
            });

            var business = new MicroBusinessProfile
            {
                DomainKey = profile.Domain.Key,
                Id = profile.Id,
                Name = profile.BusinessName,
                Summary = profile.BusinessSummary,
                Email = profile.BusinessEmail,
                Phone = "",
                ImageUri = profile.LogoUri.ToUri(),
                Facebook = profile.SocialLinks.FirstOrDefault(s => s.Type == SocialTypeEnum.Facebook)?.Url ?? "",
                Instagram = profile.SocialLinks.FirstOrDefault(s => s.Type == SocialTypeEnum.Instagram)?.Url ?? "",
                Linkedln = profile.SocialLinks.FirstOrDefault(s => s.Type == SocialTypeEnum.LinkedIn)?.Url ?? "",
                Twitter = profile.SocialLinks.FirstOrDefault(s => s.Type == SocialTypeEnum.Twitter)?.Url ?? "",
                Youtube = profile.SocialLinks.FirstOrDefault(s => s.Type == SocialTypeEnum.Youtube)?.Url ?? "",
                Posts = profile.Posts.Where(s => s.IsFeatured).OrderByDescending(s => s.CreatedDate).Select(e =>
                    new MicroBusinessPost { Key = e.Key, Title = e.Title, Content = e.Content, ImageUri = e.FeaturedImageUri.ToUri() }),
                Tags = profile.Tags.OrderBy(p => p.TagName).Select(t => t.TagName),
                Categories = profile.BusinessCategories.OrderBy(p => p.Name).Select(c => c.Name),
                Locations = profile.BusinessLocations.OrderBy(p => p.Name).Select(l =>
                    new MicroBusinessAddress { Key = l.Key, Name = l.Name, Address = l.TraderLocationToAddress(), Latitude = l.Address?.Latitude ?? 0, Longitude = l.Address?.Longitude ?? 0 }),
                //HasConnected = b2cQbicles.Contains(d.Domain.Id),
                ConnectId = "0"
            };

            if (!domainBusiness.Contains(profile.Domain.Id))
                business.ConnectId = profile.Id.ToString();

            if (profile.BusinessCatalogues.Any() && profile.IsDisplayedInB2CListings)
            {
                var b2CQbicle = dbContext.B2CQbicles.FirstOrDefault(s => !s.IsHidden && s.Business.Id == profile.Domain.Id && s.Customer.Id == userId);

                business.QbicleId = b2CQbicle.Id;
                business.Shops = profile.BusinessCatalogues.Select(e =>
                new MicroBusinessShop
                {
                    CatalogId = e.Id,
                    Key = e.Key,
                    Title = e.Name,
                    Summary = e.Description,
                    QbicleKey = b2CQbicle.Key,
                    ImageUri = e.Image.ToUri()
                });
            }

            return business;

        }

        /// <summary>
        /// GetBusinessDomainLevel DomainPlans
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public object GetBusinessDomainLevel(string domainKey)
        {
            var domainId = domainKey.Decrypt2Int();
            var domainPlan = dbContext.DomainPlans.AsNoTracking().FirstOrDefault(e => e.Domain.Id == domainId && e.IsArchived == false);
            return
                new
                {
                    Level = domainPlan?.Level?.Id,
                    Name = domainPlan?.Level == null ? "Existing package" : domainPlan?.Level?.Name
                };
        }
    }
}
