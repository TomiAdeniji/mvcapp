using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Commerce
{
    public class B2BRelationshipRules
    {
        ApplicationDbContext dbContext;
        public B2BRelationshipRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ReturnJsonModel CreateRelationship(int fromDomainId, int toDomainId, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, fromDomainId, toDomainId);
                    var relationship = dbContext.B2BRelationships.FirstOrDefault(s => s.Domain1.Id == fromDomainId && s.Domain2.Id == toDomainId);
                    if (relationship != null)
                    {
                        returnJson.msg = ResourcesManager._L("WARNING_MSG_DOMAIN_EXISTED");
                    }
                    else
                    {
                        var user = dbContext.QbicleUser.Find(userId);
                        #region CREATE a B2BRelationship and B2BQbicle
                        relationship = new B2BRelationship
                        {
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedBy = user
                        };
                        relationship.LastUpdatedDate = relationship.CreatedDate;
                        relationship.Domain1 = dbContext.Domains.Find(fromDomainId);
                        relationship.Domain2 = dbContext.Domains.Find(toDomainId);

                        var profile1 = dbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == relationship.Domain1.Id);
                        var profile2 = dbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == relationship.Domain2.Id);
                        if (profile1 == null)
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_NOTPROFILEDOMAIN");
                            dbTransaction.Rollback();
                            return returnJson;
                        }
                        var qbicle = new B2BQbicle
                        {
                            Name = $"{relationship.Domain1.Name} & {relationship.Domain2.Name} hub",
                            Description = $"{profile1?.BusinessName} - {relationship.Domain1.Name} & {relationship.Domain2.Name}",
                            LogoUri = HelperClass.QbicleLogoDefault,
                            IsHidden = false,
                            OwnedBy = user,
                            StartedBy = user,
                            Manager = user,
                            StartedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow
                        };
                        qbicle.Domains.Add(relationship.Domain1);
                        qbicle.Domains.Add(relationship.Domain2);
                        qbicle.Domain = dbContext.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.BUSINESS2BUSINESS && s.Type == SystemDomainType.B2B);


                        var members = new List<ApplicationUser>
                        {
                            user
                        };
                        members.AddRange(profile1.DefaultB2BRelationshipManagers);
                        members.AddRange(profile2.DefaultB2BRelationshipManagers);
                        members.AddRange(relationship.Domain1.Administrators);
                        members.AddRange(relationship.Domain2.Administrators);
                        foreach (var item in members.Distinct())
                        {
                            qbicle.Members.Add(item);
                        }
                        dbContext.B2BQbicles.Add(qbicle);
                        dbContext.Entry(qbicle).State = EntityState.Added;
                        relationship.CommunicationQbicle = qbicle;
                        dbContext.Entry(relationship).State = EntityState.Added;
                        dbContext.B2BRelationships.Add(relationship);
                        #endregion
                        #region CREATE a B2BPartnershipDiscussion
                        var discussion = new B2BPartnershipDiscussion
                        {
                            IsVisibleInQbicleDashboard = false,
                            Relationship = relationship,
                            StartedBy = user,
                            StartedDate = DateTime.UtcNow,
                            State = QbicleActivity.ActivityStateEnum.Open,
                            Qbicle = qbicle,
                            TimeLineDate = DateTime.UtcNow,
                            Name = "Discussion for Relationship hub",
                            Summary = "Discussion for Relationship hub",
                            FeaturedImageUri = null
                        };
                        discussion.ActivityMembers.AddRange(qbicle.Members);
                        discussion.Topic = new TopicRules(dbContext).GetTopicByName(HelperClass.GeneralName, qbicle.Id);
                        dbContext.Discussions.Add(discussion);
                        dbContext.Entry(discussion).State = EntityState.Added;
                        #endregion
                        #region CREATE Domain1 could provide Products to Domain2
                        var partDomain12 = new PurchaseSalesPartnership
                        {
                            IsProviderConfirmed = false,
                            IsConsumerConfirmed = false,
                            ProviderDomain = relationship.Domain1,
                            ConsumerDomain = relationship.Domain2,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = user,
                            LastUpdatedDate = DateTime.UtcNow,
                            LastUpdatedBy = user,
                            ParentRelationship = relationship,
                            Type = B2BService.Products
                        };
                        dbContext.Entry(partDomain12).State = EntityState.Added;
                        dbContext.B2BPartnerships.Add(partDomain12);
                        #endregion
                        #region CREATE Domain2 could provide Products to Domain1
                        var partDomain21 = new PurchaseSalesPartnership
                        {
                            IsProviderConfirmed = false,
                            IsConsumerConfirmed = false,
                            ProviderDomain = relationship.Domain2,
                            ConsumerDomain = relationship.Domain1,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = user,
                            LastUpdatedDate = DateTime.UtcNow,
                            LastUpdatedBy = user,
                            ParentRelationship = relationship,
                            Type = B2BService.Products
                        };
                        dbContext.Entry(partDomain21).State = EntityState.Added;
                        dbContext.B2BPartnerships.Add(partDomain21);
                        #endregion
                        #region CREATE Domain1 could provide Logistics to Domain2
                        var partLogisticsDomain12 = new LogisticsPartnership
                        {
                            IsProviderConfirmed = false,
                            IsConsumerConfirmed = false,
                            ProviderDomain = relationship.Domain1,
                            ConsumerDomain = relationship.Domain2,
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow,
                            CreatedBy = user,
                            LastUpdatedBy = user,
                            ParentRelationship = relationship,
                            Type = B2BService.Logistics
                        };
                        dbContext.Entry(partLogisticsDomain12).State = EntityState.Added;
                        dbContext.B2BPartnerships.Add(partLogisticsDomain12);
                        #endregion
                        #region CREATE Domain2 could provide Logistics to Domain1
                        var partLogisticsDomain21 = new LogisticsPartnership
                        {
                            IsProviderConfirmed = false,
                            IsConsumerConfirmed = false,
                            ProviderDomain = relationship.Domain2,
                            ConsumerDomain = relationship.Domain1,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = user,
                            LastUpdatedDate = DateTime.UtcNow,
                            LastUpdatedBy = user,
                            ParentRelationship = relationship,
                            Type = B2BService.Logistics
                        };
                        dbContext.Entry(partLogisticsDomain21).State = EntityState.Added;
                        dbContext.B2BPartnerships.Add(partLogisticsDomain21);
                        #endregion
                        returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                        returnJson.Object = qbicle.Id;
                    }

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fromDomainId, toDomainId);
                    returnJson.msg = ex.Message;
                    dbTransaction.Rollback();
                }
            }
            return returnJson;
        }

        public List<B2bRelationshipsModel> GetRelationships(int domainId, string keyword, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId);
                var query = from rl in dbContext.B2BRelationships.Where(s => (s.Domain1.Id == domainId || s.Domain2.Id == domainId) && s.CommunicationQbicle.Members.Any(m => m.Id == currentUserId))
                            join p in dbContext.B2BProfiles on rl.Domain1.Id equals p.Domain.Id into p1
                            from profile1 in p1.DefaultIfEmpty()
                            join p in dbContext.B2BProfiles on rl.Domain2.Id equals p.Domain.Id into p2
                            from profile2 in p2.DefaultIfEmpty()
                            select new B2bRelationshipsModel
                            {
                                PartnerDomainLogoUri = (domainId == rl.Domain1.Id ? profile2.LogoUri : profile1.LogoUri),
                                PartnerDomainName = (domainId == rl.Domain1.Id ? profile2.BusinessName : profile1.BusinessName),
                                RelationshipId = rl.Id,
                                PartnerDomainId = (domainId == rl.Domain1.Id ? profile2.Id : profile1.Id),
                                RelationshipHub = rl.CommunicationQbicle,
                                Partnerships = rl.Partnerships,
                            };
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(s => s.PartnerDomainName.Contains(keyword));
                var result = query.OrderByDescending(s => s.RelationshipHub.LastUpdated).ToList();
                result.ForEach(p =>
                {
                    p.PartnerDomainKey = p.PartnerDomainId.Encrypt();
                });
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<B2bRelationshipsModel>();
            }
        }
        public B2BRelationship GetRelationship(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);

                return dbContext.B2BRelationships.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new B2BRelationship();
            }
        }
        public B2BRelationship GetRelationshipByDomainId(int domainid1, int domainid2)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainid1, domainid2);

                return dbContext.B2BRelationships.FirstOrDefault(s =>/*(s.Status==RelationshipStatus.Accepted||s.Status==RelationshipStatus.Pending)&&*/((s.Domain1.Id == domainid1 && s.Domain2.Id == domainid2) || (s.Domain1.Id == domainid2 && s.Domain2.Id == domainid1)));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainid1, domainid2);
                return new B2BRelationship();
            }
        }
        public int CountConnectionByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId);

                return dbContext.B2BRelationships.Count(s =>/*s.Status==RelationshipStatus.Accepted&&*/(s.Domain1.Id == domainId || s.Domain2.Id == domainId));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return 0;
            }
        }
        public ReturnJsonModel UpdateMembersRelationship(int qbicleId, int currentDomainId, List<string> members, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qbicleId, currentDomainId, members, userId);

                    var b2bQbicle = dbContext.B2BQbicles.Find(qbicleId);
                    if (b2bQbicle != null)
                    {
                        b2bQbicle.LastUpdated = DateTime.UtcNow;
                        var currentDomain = dbContext.Domains.Find(currentDomainId);
                        var isDomainAdmin = currentDomain != null && currentDomain.Administrators.Any(s => s.Id == userId);
                        if (!isDomainAdmin && !b2bQbicle.Members.Any(s => s.Id == userId))
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                            return returnJson;
                        }
                        //Remove users of current domain
                        foreach (var u in currentDomain.Users)
                        {
                            if (u.Id != userId)
                                b2bQbicle.Members.Remove(u);
                        }
                        //Add users of current domain
                        if (members != null)
                        {
                            foreach (var item in members)
                            {
                                var member = dbContext.QbicleUser.Find(item);
                                if (member != null && !b2bQbicle.Members.Any(s => s.Id == member.Id))
                                    b2bQbicle.Members.Add(member);
                            }
                        }
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, currentDomainId, members, userId);
                    dbTransaction.Rollback();

                }
            }
            return returnJson;
        }
        public ReturnJsonModel PublishCatalogue(int relationshipId, /*bool isPublish,*/string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, relationshipId/*, isPublish*/);
                var relationship = dbContext.B2BRelationships.Find(relationshipId);
                if (relationship != null)
                {
                    //relationship.IsProductCatalogDisplayed = isPublish;
                    relationship.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    relationship.LastUpdatedDate = DateTime.UtcNow;
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, relationshipId/*, isPublish*/);
            }
            return returnJson;
        }
        public ReturnJsonModel HaltAllPartnerships(int relationshipId, int currentDomainId, bool status, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, relationshipId, currentDomainId, status, userId);
                var currentUser = dbContext.QbicleUser.Find(userId);
                var relationship = dbContext.B2BRelationships.Find(relationshipId);
                if (relationship != null)
                {
                    foreach (var partnership in relationship.Partnerships)
                    {
                        partnership.LastUpdatedBy = currentUser;
                        partnership.LastUpdatedDate = DateTime.UtcNow;
                        partnership.IsProviderConfirmed = status;
                        partnership.IsConsumerConfirmed = status;
                        if (partnership is LogisticsPartnership)
                        {
                            var logisticsPartnership = (LogisticsPartnership)partnership;
                            var currentLogisticsAgreement = logisticsPartnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsDraft);
                            if (currentLogisticsAgreement != null)
                            {
                                currentLogisticsAgreement.ConsumerLocations.Clear();
                                if (currentLogisticsAgreement.PriceList != null)
                                    dbContext.B2BProviderPriceLists.Remove(currentLogisticsAgreement.PriceList);
                                dbContext.B2BLogisticsAgreements.Remove(currentLogisticsAgreement);
                            }
                            //The LogisticsAgreement have Status=Active move to the Archive 
                            var activeLogisticsAgreement = logisticsPartnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsActive);
                            if (activeLogisticsAgreement != null)
                            {
                                activeLogisticsAgreement.Status = AgreementStatus.IsArchived;
                                activeLogisticsAgreement.ArchivedDate = DateTime.UtcNow;
                            }
                        }
                    }
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, relationshipId, currentDomainId, status, userId);
            }
            return returnJson;
        }
        public ReturnJsonModel HaltPartnership(int partnershipId, int currentDomainId, bool status, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId, currentDomainId, status, userId);
                var currentUser = dbContext.QbicleUser.Find(userId);
                var partnership = dbContext.B2BPartnerships.Find(partnershipId);
                if (partnership != null)
                {
                    partnership.LastUpdatedBy = currentUser;
                    partnership.LastUpdatedDate = DateTime.UtcNow;
                    partnership.IsProviderConfirmed = status;
                    partnership.IsConsumerConfirmed = status;
                    if (partnership is LogisticsPartnership)
                    {
                        var logisticsPartnership = (LogisticsPartnership)partnership;
                        var currentLogisticsAgreement = logisticsPartnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsDraft);
                        if (currentLogisticsAgreement != null)
                        {
                            currentLogisticsAgreement.ConsumerLocations.Clear();
                            if (currentLogisticsAgreement.PriceList != null)
                                dbContext.B2BProviderPriceLists.Remove(currentLogisticsAgreement.PriceList);
                            dbContext.B2BLogisticsAgreements.Remove(currentLogisticsAgreement);
                        }
                        //The LogisticsAgreement have Status=Active move to the Archive 
                        var activeLogisticsAgreement = logisticsPartnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsActive);
                        if (activeLogisticsAgreement != null)
                        {
                            activeLogisticsAgreement.Status = AgreementStatus.IsArchived;
                            activeLogisticsAgreement.ArchivedDate = DateTime.UtcNow;
                        }
                        returnJson.Object = new { isLogistics = true };
                    }
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId, currentDomainId, status, userId);
            }
            return returnJson;
        }
        public ReturnJsonModel SettingsPartnership(int partnershipId, int currentDomainId, List<string> members, List<int> menus, int accountId, string type, bool status, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId/*, isPublish*/);
                var partnership = dbContext.B2BPurchaseSalesPartnerships.Find(partnershipId);
                if (partnership != null)
                {
                    partnership.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    partnership.LastUpdatedDate = DateTime.UtcNow;
                    if (partnership.ProviderDomain.Id == currentDomainId)
                    {
                        partnership.IsProviderConfirmed = status;
                        partnership.ProviderPartnershipManagers.Clear();
                        if (members != null)
                            foreach (var item in members)
                            {
                                var u = dbContext.QbicleUser.Find(item);
                                if (u != null)
                                {
                                    partnership.ProviderPartnershipManagers.Add(u);
                                }
                            }
                        partnership.ProviderPaymentAccount = dbContext.TraderCashAccounts.Find(accountId);
                    }
                    else if (partnership.ConsumerDomain.Id == currentDomainId)
                    {
                        partnership.IsConsumerConfirmed = status;
                        partnership.ConsumerPartnershipManagers.Clear();
                        if (members != null)
                            foreach (var item in members)
                            {
                                var u = dbContext.QbicleUser.Find(item);
                                if (u != null)
                                {
                                    partnership.ConsumerPartnershipManagers.Add(u);
                                }
                            }
                        partnership.ConsumerPaymentAccount = dbContext.TraderCashAccounts.Find(accountId);
                    }
                    partnership.Catalogs.Clear();
                    if (menus != null)
                        foreach (var item in menus)
                        {
                            var posmenu = dbContext.PosMenus.Find(item);
                            if (posmenu != null)
                                partnership.Catalogs.Add(posmenu);
                        }
                    if (status && partnership.CommunicationQbicle == null)
                    {
                        var qbicle = partnership.ParentRelationship.CommunicationQbicle;
                        partnership.CommunicationQbicle = new B2BQbicle
                        {
                            Domains = qbicle.Domains,
                            Domain = qbicle.Domain,
                            Name = $"{partnership.ProviderDomain.Name} & {partnership.ConsumerDomain.Name} hub",
                            Description = $"{partnership.ProviderDomain.Name} & {partnership.ConsumerDomain.Name}",
                            OwnedBy = partnership.LastUpdatedBy,
                            StartedBy = partnership.LastUpdatedBy,
                            Manager = partnership.LastUpdatedBy,
                            StartedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                            IsHidden = false,
                            Members = qbicle.Members
                        };
                    }
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId/*, isPublish*/);
            }
            return returnJson;
        }
        public LogisticsPartnership GetLogisticsPartnership(int partnershipId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId);
                return dbContext.B2BLogisticsPartnerships.Find(partnershipId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId);
                return null;
            }
        }
        public bool CheckLogisticsAgreement(int relationshipId, int partnershipId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, relationshipId, partnershipId);
                return dbContext.B2BLogisticsPartnerships.Any(
                    s => s.B2BRelationship_Id == relationshipId
                    && s.Id != partnershipId
                    && ((s.IsConsumerConfirmed && !s.IsProviderConfirmed)
                    || (!s.IsConsumerConfirmed && s.IsProviderConfirmed)
                    || (s.IsConsumerConfirmed && s.IsProviderConfirmed))
                );
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, relationshipId, partnershipId);
                return false;
            }
        }
        public LogisticsPartnership CurrentLogisticsAgreement(int relationshipId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, relationshipId);
                var currentLogisticsPartnership = dbContext.B2BLogisticsPartnerships.FirstOrDefault(
                    s => s.B2BRelationship_Id == relationshipId
                    && ((s.IsConsumerConfirmed && !s.IsProviderConfirmed)
                    || (!s.IsConsumerConfirmed && s.IsProviderConfirmed)
                    || (s.IsConsumerConfirmed && s.IsProviderConfirmed))
                );
                return currentLogisticsPartnership;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, relationshipId);
                return null;
            }
        }
        public B2BLogisticsAgreement GetLogisticsAgreement(int logisticsAgreementId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, logisticsAgreementId);
                return dbContext.B2BLogisticsAgreements.Find(logisticsAgreementId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, logisticsAgreementId);
                return null;
            }
        }
        public ReturnJsonModel UpdateLocationsPartnership(int partnershipId, int currentDomainId, List<int> locids, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId, currentDomainId, locids, userId);
                var partnership = dbContext.B2BLogisticsPartnerships.Find(partnershipId);
                var locations = new List<TraderLocation>();
                if (partnership != null)
                {
                    if (CheckLogisticsAgreement(partnership.B2BRelationship_Id, partnership.Id))
                    {
                        returnJson.msg = "WARNING_MSG_EXISTAGREEMENT";
                        return returnJson;
                    }
                    partnership.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    partnership.LastUpdatedDate = DateTime.UtcNow;
                    //create a B2BLogisticsAgreement
                    var currenLogisticstAgreement = partnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsDraft);
                    if (currenLogisticstAgreement == null)
                    {
                        currenLogisticstAgreement = new B2BLogisticsAgreement();
                        currenLogisticstAgreement.Status = AgreementStatus.IsDraft;
                        currenLogisticstAgreement.IsConsumerAgreed = false;
                        currenLogisticstAgreement.IsProviderAgreed = false;
                        currenLogisticstAgreement.LogisticsPartnership = partnership;
                        currenLogisticstAgreement.CreatedDate = DateTime.UtcNow;
                        if (locids != null && locids.Any())
                        {
                            currenLogisticstAgreement.ConsumerLocations.Clear();
                            foreach (var lid in locids)
                            {
                                var location = dbContext.TraderLocations.Find(lid);
                                if (location != null)
                                {
                                    currenLogisticstAgreement.ConsumerLocations.Add(location);
                                    locations.Add(location);
                                }

                            }
                        }
                        partnership.LogisticsAgreements.Add(currenLogisticstAgreement);
                    }
                    else
                    {
                        if (locids != null && locids.Any())
                        {
                            currenLogisticstAgreement.ConsumerLocations.Clear();
                            foreach (var lid in locids)
                            {
                                var location = dbContext.TraderLocations.Find(lid);
                                if (location != null)
                                {
                                    currenLogisticstAgreement.ConsumerLocations.Add(location);
                                    locations.Add(location);
                                }

                            }
                        }
                    }
                    if (partnership.ConsumerDomain.Id == currentDomainId)
                        partnership.IsConsumerConfirmed = true;
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    if (returnJson.result)
                    {
                        var consumerBusinessProfile = partnership.ConsumerDomain.Id.BusinesProfile();
                        var consumerBusinessName = consumerBusinessProfile?.BusinessName ?? partnership.ConsumerDomain.Name;
                        var sLocations = string.Join(" ", locations.Select(s => $"<strong>{s.Name}</strong>"));
                        string[] arrObjects = { consumerBusinessName, sLocations };
                        var discussion = dbContext.B2BPartnershipDiscussions.FirstOrDefault(s => s.Relationship.Id == partnership.ParentRelationship.Id);
                        SendMessage(ResourcesManager._L("CONSUMER_MSG_UPDATELOCATIONS", arrObjects), discussion.Id, partnership.LastUpdatedBy.Id, partnership.ParentRelationship.CommunicationQbicle.Id, "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId, currentDomainId, locids, userId);
            }
            return returnJson;
        }
        public ReturnJsonModel AddPriceListLogisticsAgreement(int partnershipId, int priceListId, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbtransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId, priceListId, userId);
                    var partnership = dbContext.B2BLogisticsPartnerships.Find(partnershipId);
                    if (partnership != null)
                    {
                        partnership.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                        partnership.LastUpdatedDate = DateTime.UtcNow;

                        var currenLogisticstAgreement = partnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsDraft);
                        if (currenLogisticstAgreement != null)
                        {
                            var priceList = dbContext.B2BPriceLists.Find(priceListId);
                            B2BProviderPriceList providerPriceList = new B2BProviderPriceList();
                            providerPriceList.PriceList = priceList;
                            foreach (var cfk in priceList.ChargeFrameworks)
                            {
                                #region Clone charge framework from PriceList
                                B2BProviderChargeFramework chargeFramework = new B2BProviderChargeFramework();
                                chargeFramework.Name = cfk.Name;
                                chargeFramework.IsActive = true;
                                chargeFramework.CreatedBy = partnership.LastUpdatedBy;
                                chargeFramework.CreatedDate = partnership.LastUpdatedDate;
                                chargeFramework.DistanceTravelPerKm = cfk.DistanceTravelPerKm;
                                chargeFramework.DistanceTravelledFlatFee = cfk.DistanceTravelledFlatFee;
                                chargeFramework.TimeTakenFlatFee = cfk.TimeTakenFlatFee;
                                chargeFramework.TimeTakenPerSecond = cfk.TimeTakenPerSecond;
                                chargeFramework.ValueOfDeliveryFlatFee = cfk.ValueOfDeliveryFlatFee;
                                chargeFramework.ValueOfDeliveryPercentTotal = cfk.ValueOfDeliveryPercentTotal;
                                chargeFramework.VehicleType = cfk.VehicleType;
                                chargeFramework.LastUpdatedBy = partnership.LastUpdatedBy;
                                chargeFramework.LastUpdatedDate = partnership.LastUpdatedDate;
                                providerPriceList.ChargeFrameworks.Add(chargeFramework);
                                #endregion
                            }
                            currenLogisticstAgreement.PriceList = providerPriceList;
                        }
                        //if (partnership.ConsumerDomain.Id == currentDomainId)
                        //    partnership.IsConsumerConfirmed = true;
                        //else if (partnership.ProviderDomain.Id == currentDomainId)
                        //    partnership.IsProviderConfirmed = true;
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbtransaction.Commit();
                    if (returnJson.result)
                    {
                        var currentBusinesProfile = partnership.ConsumerDomain.Id.BusinesProfile();
                        var partnerBusinessProfile = partnership.ProviderDomain.Id.BusinesProfile();
                        var currentBusinessName = currentBusinesProfile?.BusinessName ?? partnership.ConsumerDomain.Name;
                        var partnerBusinessName = partnerBusinessProfile?.BusinessName ?? partnership.ProviderDomain.Name;
                        string[] arrObjects = { partnerBusinessName, currentBusinessName };
                        var discussion = dbContext.B2BPartnershipDiscussions.FirstOrDefault(s => s.Relationship.Id == partnership.ParentRelationship.Id);
                        SendMessage(ResourcesManager._L("PROVIDER_MSG_ADDPRICE", arrObjects), discussion.Id, partnership.LastUpdatedBy.Id, partnership.ParentRelationship.CommunicationQbicle.Id, "");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId, priceListId, userId);
                    dbtransaction.Rollback();
                }
            }
            return returnJson;
        }
        public ReturnJsonModel DeletePriceListLogisticsAgreement(int partnershipId, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbtransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId, userId);
                    var partnership = dbContext.B2BLogisticsPartnerships.Find(partnershipId);
                    if (partnership != null)
                    {
                        partnership.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                        partnership.LastUpdatedDate = DateTime.UtcNow;

                        var currenLogisticstAgreement = partnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsDraft);
                        if (currenLogisticstAgreement != null)
                        {
                            if (!currenLogisticstAgreement.IsProviderAgreed)
                            {
                                dbContext.B2BProviderPriceLists.Remove(currenLogisticstAgreement.PriceList);
                                currenLogisticstAgreement.PriceList = null;
                            }
                        }
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbtransaction.Commit();
                    if (returnJson.result)
                    {
                        var currentBusinesProfile = partnership.ConsumerDomain.Id.BusinesProfile();
                        var partnerBusinessProfile = partnership.ProviderDomain.Id.BusinesProfile();
                        var currentBusinessName = currentBusinesProfile?.BusinessName ?? partnership.ConsumerDomain.Name;
                        var partnerBusinessName = partnerBusinessProfile?.BusinessName ?? partnership.ProviderDomain.Name;
                        string[] arrObjects = { partnerBusinessName, currentBusinessName };
                        var discussion = dbContext.B2BPartnershipDiscussions.FirstOrDefault(s => s.Relationship.Id == partnership.ParentRelationship.Id);
                        SendMessage(ResourcesManager._L("PROVIDER_MSG_UPDATEPRICE", arrObjects), discussion.Id, partnership.LastUpdatedBy.Id, partnership.ParentRelationship.CommunicationQbicle.Id, "");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId, userId);
                    dbtransaction.Rollback();
                }
            }
            return returnJson;
        }
        public ReturnJsonModel UpdateProviderChargeFrameworks(int partnershipId, List<B2BProviderChargeFramework> chargeFrameworks, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbtransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId, chargeFrameworks, userId);
                    if (!chargeFrameworks.Any(s => s.IsActive))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_MINPRICE");
                        return returnJson;
                    }
                    var partnership = dbContext.B2BLogisticsPartnerships.Find(partnershipId);
                    if (partnership != null)
                    {
                        partnership.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                        partnership.LastUpdatedDate = DateTime.UtcNow;
                        foreach (var item in chargeFrameworks)
                        {
                            var chargeFramework = dbContext.B2BProviderChargeFrameworks.Find(item.Id);
                            if (chargeFramework != null)
                            {
                                chargeFramework.IsActive = item.IsActive;
                                chargeFramework.DistanceTravelPerKm = item.DistanceTravelPerKm;
                                chargeFramework.DistanceTravelledFlatFee = item.DistanceTravelledFlatFee;
                                chargeFramework.TimeTakenFlatFee = item.TimeTakenFlatFee;
                                chargeFramework.TimeTakenPerSecond = item.TimeTakenPerSecond;
                                chargeFramework.ValueOfDeliveryFlatFee = item.ValueOfDeliveryFlatFee;
                                chargeFramework.ValueOfDeliveryPercentTotal = item.ValueOfDeliveryPercentTotal;
                                chargeFramework.LastUpdatedBy = partnership.LastUpdatedBy;
                                chargeFramework.LastUpdatedDate = partnership.LastUpdatedDate;
                            }
                        }
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbtransaction.Commit();
                    if (returnJson.result)
                    {
                        var currentBusinesProfile = partnership.ConsumerDomain.Id.BusinesProfile();
                        var partnerBusinessProfile = partnership.ProviderDomain.Id.BusinesProfile();
                        var currentBusinessName = currentBusinesProfile?.BusinessName ?? partnership.ConsumerDomain.Name;
                        var partnerBusinessName = partnerBusinessProfile?.BusinessName ?? partnership.ProviderDomain.Name;
                        string[] arrObjects = { partnerBusinessName, currentBusinessName };
                        var discussion = dbContext.B2BPartnershipDiscussions.FirstOrDefault(s => s.Relationship.Id == partnership.ParentRelationship.Id);
                        SendMessage(ResourcesManager._L("PROVIDER_MSG_UPDATEPRICE", arrObjects), discussion.Id, partnership.LastUpdatedBy.Id, partnership.ParentRelationship.CommunicationQbicle.Id, "");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId, chargeFrameworks, userId);
                    dbtransaction.Rollback();
                }
            }
            return returnJson;
        }
        public ReturnJsonModel AgreeTerms(int partnershipId, int currentDomainId, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbtransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId, userId);
                    var partnership = dbContext.B2BLogisticsPartnerships.Find(partnershipId);
                    if (partnership != null)
                    {
                        if (currentDomainId != partnership.ConsumerDomain.Id)
                        {
                            returnJson.msg = "ERROR_MSG_28";
                            return returnJson;
                        }
                        partnership.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                        partnership.LastUpdatedDate = DateTime.UtcNow;

                        var currenLogisticstAgreement = partnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsDraft);
                        if (currenLogisticstAgreement != null && currenLogisticstAgreement.PriceList != null)
                        {
                            currenLogisticstAgreement.IsConsumerAgreed = true;
                        }
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbtransaction.Commit();
                    if (returnJson.result)
                    {
                        var consumerBusinessProfile = partnership.ConsumerDomain.Id.BusinesProfile();
                        var consumerBusinessName = consumerBusinessProfile?.BusinessName ?? partnership.ConsumerDomain.Name;
                        var discussion = dbContext.B2BPartnershipDiscussions.FirstOrDefault(s => s.Relationship.Id == partnership.ParentRelationship.Id);
                        SendMessage(ResourcesManager._L("CONSUMER_MSG_AGREETERMS", consumerBusinessName), discussion.Id, partnership.LastUpdatedBy.Id, partnership.ParentRelationship.CommunicationQbicle.Id, "");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId, userId);
                    dbtransaction.Rollback();
                }
            }
            return returnJson;
        }
        public ReturnJsonModel FinaliseAgreement(int partnershipId, int currentDomainId, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbtransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId, userId);
                    var partnership = dbContext.B2BLogisticsPartnerships.Find(partnershipId);
                    if (partnership != null)
                    {
                        if (currentDomainId != partnership.ProviderDomain.Id)
                        {
                            returnJson.msg = "ERROR_MSG_28";
                            return returnJson;
                        }
                        partnership.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                        partnership.LastUpdatedDate = DateTime.UtcNow;
                        partnership.IsProviderConfirmed = true;
                        if (partnership.CommunicationQbicle == null)
                        {
                            var qbicleRelationship = partnership.ParentRelationship.CommunicationQbicle;
                            partnership.CommunicationQbicle = new B2BQbicle
                            {
                                Domains = qbicleRelationship.Domains,
                                Domain = qbicleRelationship.Domain,
                                Name = $"{partnership.ProviderDomain.Name} & {partnership.ConsumerDomain.Name} hub",
                                Description = $"{partnership.ConsumerDomain.Name} & {partnership.ProviderDomain.Name}",
                                OwnedBy = partnership.LastUpdatedBy,
                                StartedBy = partnership.LastUpdatedBy,
                                Manager = partnership.LastUpdatedBy,
                                StartedDate = partnership.LastUpdatedDate,
                                LastUpdated = partnership.LastUpdatedDate,
                                IsHidden = false,
                                Members = qbicleRelationship.Members
                            };
                        }
                        //Update a LogisticsAgreement have Status=IsActive to IsArchived
                        var currentActiveAgreement = partnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsActive);
                        if (currentActiveAgreement != null)
                        {
                            currentActiveAgreement.Status = AgreementStatus.IsArchived;
                            currentActiveAgreement.ArchivedDate = partnership.LastUpdatedDate;
                        }
                        //end
                        var currenLogisticstAgreement = partnership.LogisticsAgreements.FirstOrDefault(s => s.Status == AgreementStatus.IsDraft);
                        if (currenLogisticstAgreement != null && currenLogisticstAgreement.IsConsumerAgreed)
                        {
                            currenLogisticstAgreement.IsProviderAgreed = true;
                            currenLogisticstAgreement.Status = AgreementStatus.IsActive;
                            currenLogisticstAgreement.ActivatedDate = partnership.LastUpdatedDate;
                            #region Clone a LogisticstAgreement
                            var cloneLogisticstAgreement = new B2BLogisticsAgreement();
                            cloneLogisticstAgreement.IsConsumerAgreed = false;
                            cloneLogisticstAgreement.IsProviderAgreed = false;
                            cloneLogisticstAgreement.LogisticsPartnership = partnership;
                            B2BProviderPriceList providerPriceList = new B2BProviderPriceList();
                            var providerpriceList = currenLogisticstAgreement.PriceList;
                            providerPriceList.PriceList = providerpriceList.PriceList;
                            foreach (var cfk in providerpriceList.ChargeFrameworks)
                            {
                                #region Clone charge framework from PriceList
                                B2BProviderChargeFramework chargeFramework = new B2BProviderChargeFramework();
                                chargeFramework.Name = cfk.Name;
                                chargeFramework.IsActive = cfk.IsActive;
                                chargeFramework.CreatedBy = partnership.LastUpdatedBy;
                                chargeFramework.CreatedDate = DateTime.UtcNow;
                                chargeFramework.DistanceTravelPerKm = cfk.DistanceTravelPerKm;
                                chargeFramework.DistanceTravelledFlatFee = cfk.DistanceTravelledFlatFee;
                                chargeFramework.TimeTakenFlatFee = cfk.TimeTakenFlatFee;
                                chargeFramework.TimeTakenPerSecond = cfk.TimeTakenPerSecond;
                                chargeFramework.ValueOfDeliveryFlatFee = cfk.ValueOfDeliveryFlatFee;
                                chargeFramework.ValueOfDeliveryPercentTotal = cfk.ValueOfDeliveryPercentTotal;
                                chargeFramework.VehicleType = cfk.VehicleType;
                                chargeFramework.LastUpdatedBy = partnership.LastUpdatedBy;
                                chargeFramework.LastUpdatedDate = partnership.LastUpdatedDate;
                                chargeFramework.PriceList = providerPriceList;
                                dbContext.B2BProviderChargeFrameworks.Add(chargeFramework);
                                dbContext.Entry(chargeFramework).State = EntityState.Added;
                                providerPriceList.ChargeFrameworks.Add(chargeFramework);
                                #endregion
                            }
                            cloneLogisticstAgreement.PriceList = providerPriceList;
                            cloneLogisticstAgreement.ConsumerLocations = currenLogisticstAgreement.ConsumerLocations;
                            cloneLogisticstAgreement.CreatedDate = DateTime.UtcNow;
                            cloneLogisticstAgreement.Status = AgreementStatus.IsDraft;
                            dbContext.B2BLogisticsAgreements.Add(cloneLogisticstAgreement);
                            #endregion
                        }
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbtransaction.Commit();
                    if (returnJson.result)
                    {
                        var partnerBusinessProfile = partnership.ProviderDomain.Id.BusinesProfile();
                        var partnerBusinessName = partnerBusinessProfile?.BusinessName ?? partnership.ProviderDomain.Name;
                        var discussion = dbContext.B2BPartnershipDiscussions.FirstOrDefault(s => s.Relationship.Id == partnership.ParentRelationship.Id);
                        SendMessage(ResourcesManager._L("PROVIDER_MSG_FINALISEDAGREEMENT", partnerBusinessName), discussion.Id, partnership.LastUpdatedBy.Id, partnership.ParentRelationship.CommunicationQbicle.Id, "");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId, userId);
                    dbtransaction.Rollback();
                }
            }
            return returnJson;
        }
        /// <summary>
        /// relationship message, always business create
        /// </summary>
        /// <param name="message"></param>
        /// <param name="disId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="currentQbicleId"></param>
        /// <param name="originatingCreationId"></param>
        public void SendMessage(string message, int disId, string currentUserId, int currentQbicleId, string originatingCreationId)
        {
            var post = new PostsRules(dbContext).SavePost(false, message, currentUserId, currentQbicleId);
            if (post != null)
                new DiscussionsRules(dbContext).AddComment(false, disId.Encrypt(), post, originatingCreationId, ApplicationPageName.Discussion, true);
        }

        public List<Select2Option> GetCatalogsFromRIdAndPDId(int partnershipId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, partnershipId);
                var partnership = dbContext.B2BPurchaseSalesPartnerships.Find(partnershipId);
                if (partnership != null)
                {
                    return partnership.Catalogs.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }).ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, partnershipId);
            }
            return new List<Select2Option>();
        }
    }
}
