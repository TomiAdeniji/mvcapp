using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class SocialWorkgroupRules
    {
        private ApplicationDbContext _db;

        public SocialWorkgroupRules()
        {
        }

        public SocialWorkgroupRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }
        public Settings getSettingByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get setting by domain id", null, null, domainId);

                return DbContext.SalesMarketingSettings.FirstOrDefault(s => s.Domain.Id == domainId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }
        public ReturnJsonModel UpdateSetting(int id, QbicleDomain domain, int topicId, int qbicleId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update setting", null, null, id, domain, topicId, qbicleId);

                var setting = DbContext.SalesMarketingSettings.Find(id);
                var sourceQbicle = DbContext.Qbicles.Find(qbicleId);
                if (sourceQbicle == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return refModel;
                }
                var defaultTopic = DbContext.Topics.Find(topicId);
                if (defaultTopic == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_190");
                    return refModel;
                }
                if (setting != null)
                {
                    setting.SourceQbicle = sourceQbicle;
                    setting.DefaultTopic = defaultTopic;
                    if (DbContext.Entry(setting).State == EntityState.Detached)
                        DbContext.SalesMarketingSettings.Attach(setting);
                    DbContext.Entry(setting).State = EntityState.Modified;
                }
                else
                {
                    Settings set = new Settings();
                    set.SourceQbicle = sourceQbicle;
                    set.DefaultTopic = defaultTopic;
                    set.Domain = domain;
                    DbContext.SalesMarketingSettings.Add(set);
                    DbContext.Entry(set).State = EntityState.Added;
                }
                refModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, domain, topicId, qbicleId);
                refModel.msg = ex.Message;
            }
            return refModel;
        }
        public SalesMarketingWorkGroup getWorkgroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get workgroup by id", null, null, id);

                return DbContext.SalesMarketingWorkGroups.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public bool SaveWorkgroup(workgroupCustomeModel model, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save workgroup", userId, null, model);

                var wg = DbContext.SalesMarketingWorkGroups.Find(model.Id);
                if (wg != null)
                {
                    wg.Name = model.Name;
                    //Add Process
                    wg.Processes.Clear();
                    foreach (var item in model.Process)
                    {
                        var process = DbContext.SalesMarketingProcesses.Find(item);
                        if (process != null)
                        {
                            wg.Processes.Add(process);
                        }
                    }
                    //Add Members
                    wg.Members.Clear();
                    if (model.Members != null && model.Members.Count() > 0)
                    {
                        foreach (var item in model.Members)
                        {
                            var user = DbContext.QbicleUser.Find(item);
                            if (user != null)
                            {
                                wg.Members.Add(user);
                            }
                        }
                    }

                    //Add ReviewersApprovers
                    wg.ReviewersApprovers.Clear();
                    if (model.ReviewersApprovers != null && model.ReviewersApprovers.Count() > 0)
                    {
                        foreach (var item in model.ReviewersApprovers)
                        {
                            var user = DbContext.QbicleUser.Find(item);
                            if (user != null)
                            {
                                wg.ReviewersApprovers.Add(user);
                            }
                        }
                    }

                    if (DbContext.Entry(wg).State == EntityState.Detached)
                        DbContext.SalesMarketingWorkGroups.Attach(wg);
                    DbContext.Entry(wg).State = EntityState.Modified;
                }
                else
                {
                    var workgroup = new SalesMarketingWorkGroup();
                    workgroup.Domain = DbContext.Domains.Find(model.DomainId);
                    workgroup.CreatedDate = DateTime.Now;
                    workgroup.CreatedBy = DbContext.QbicleUser.Find(userId);
                    workgroup.Name = model.Name;
                    foreach (var item in model.Process)
                    {
                        var process = DbContext.SalesMarketingProcesses.Find(item);
                        if (process != null)
                        {
                            workgroup.Processes.Add(process);
                        }
                    }
                    if (model.Members != null && model.Members.Count() > 0)
                    {
                        foreach (var item in model.Members)
                        {
                            var user = DbContext.QbicleUser.Find(item);
                            if (user != null)
                            {
                                workgroup.Members.Add(user);
                            }
                        }
                    }
                    if (model.ReviewersApprovers != null && model.ReviewersApprovers.Count() > 0)
                    {
                        foreach (var item in model.ReviewersApprovers)
                        {
                            var user = DbContext.QbicleUser.Find(item);
                            if (user != null)
                            {
                                workgroup.ReviewersApprovers.Add(user);
                            }
                        }
                    }

                    DbContext.SalesMarketingWorkGroups.Add(workgroup);
                    DbContext.Entry(workgroup).State = EntityState.Added;
                    var setting = getSettingByDomainId(model.DomainId);
                    if (setting == null)
                    {
                        setting = new Settings();
                        var qbicle = DbContext.Qbicles.Find(model.QbicleId);
                        var topic = DbContext.Topics.Find(model.TopicId);
                        var domain = DbContext.Domains.Find(model.DomainId);
                        setting.SourceQbicle = qbicle;
                        setting.DefaultTopic = topic;
                        setting.Domain = domain;
                        setting.WorkGroups.Add(workgroup);
                        DbContext.SalesMarketingSettings.Add(setting);
                        DbContext.Entry(setting).State = EntityState.Added;
                    }
                    else
                    {
                        setting.WorkGroups.Add(workgroup);
                        DbContext.Entry(setting).State = EntityState.Modified;
                    }
                }
                return DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
                return false;
            }
        }
        public List<SalesMarketingWorkGroup> GetMarketingWorkGroupsByDomainId(int domainId)
        {
            try
            {
                return DbContext.SalesMarketingWorkGroups.Where(s => s.Domain.Id == domainId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel deleteWorkgroupById(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete workgroup by id", null, null, id);

                if (DbContext.SocialCampaigns.Any(s => s.WorkGroup.Id == id))
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_222");
                    return refModel;
                }
                var wg = DbContext.SalesMarketingWorkGroups.Find(id);
                if (wg != null)
                {
                    DbContext.SalesMarketingWorkGroups.Remove(wg);
                }
                var result = DbContext.SaveChanges();
                if (result > 0)
                {
                    refModel.result = true;
                }
                else
                {
                    refModel.result = false;
                }
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return refModel;
            }
        }
        public List<SalesMarketingProcess> MarketingProcess()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Marketing process", null, null);

                return DbContext.SalesMarketingProcesses.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<SalesMarketingProcess>();
            }
        }

        public List<SMContact> GetSMContacts(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get SM contacts", null, null, domainId);

                var listContact = DbContext.SMContacts.Where(x => x.Domain.Id == domainId).ToList();
                return listContact;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<SMContact>();
            }
        }

        public List<TraderContact> SyncTrader(int domainId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Sync trader", userId, null, domainId);

                var listTrader = DbContext.TraderContacts.Where(x => x.Workgroup.Domain.Id == domainId).ToList();
                var listContact = DbContext.SMContacts.Where(x => x.Domain.Id == domainId).ToList();
                if (listTrader.Count > 0)
                {
                    var birthDay = DateTime.ParseExact("01/01/1900", "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    if (listContact.Count > 0)
                    {
                        List<TraderContact> traderContactsList = new List<TraderContact>();
                        foreach (var item in listTrader)
                        {
                            var checkEmail = listContact.Where(x => x.Email == item.Email).ToList();
                            if (checkEmail.Count == 0)
                            {
                                traderContactsList.Add(item);
                                SMContact smContact = new SMContact
                                {
                                    Domain = item.Workgroup.Domain,
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = DbContext.QbicleUser.Find(userId),
                                    Name = item.Name,
                                    PhoneNumber = item.PhoneNumber,
                                    Email = item.Email,
                                    Source = ContactSourceEnum.Trader,
                                    AvatarUri = item.AvatarUri,
                                    BirthDay = birthDay
                                };
                                DbContext.SMContacts.Add(smContact);
                                DbContext.Entry(smContact).State = EntityState.Added;
                            }
                        }
                        DbContext.SaveChanges();
                        return traderContactsList;
                    }
                    else
                    {
                        foreach (var item in listTrader)
                        {
                            SMContact smContact = new SMContact
                            {
                                Domain = item.Workgroup.Domain,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = DbContext.QbicleUser.Find(userId),
                                Name = item.Name,
                                PhoneNumber = item.PhoneNumber,
                                Email = item.Email,
                                Source = ContactSourceEnum.Trader
                            };
                            DbContext.SMContacts.Add(smContact);
                            smContact.AvatarUri = item.AvatarUri;
                            smContact.BirthDay = birthDay;
                            DbContext.Entry(smContact).State = EntityState.Added;
                        }
                        DbContext.SaveChanges();
                        return listTrader;
                    }
                }
                return new List<TraderContact>();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<TraderContact>();
            }
        }

        public List<CustomOption> GetCustomOptions(int domainId, QbicleDomain domain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get custom options", null, null, domainId, domain);

                var listCustomOption = DbContext.SMCustomCriteriaDefinitions.FirstOrDefault(x => x.Domain.Id == domainId && x.IsAgeRange && x.Status == CustomCriteriaStatus.Active);
                return listCustomOption != null ? listCustomOption.CustomOptions.ToList() : new List<CustomOption>();
            }
            catch (DbEntityValidationException ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, domain);
                return new List<CustomOption>();
            }
        }

        public ReturnJsonModel SaveAgeRanges(int id, int idCustomCriteriaDefinition, int start, int end, string userId, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save age ranges", userId,
                        null, id, idCustomCriteriaDefinition, start, end, domain);

                if (id != -1)
                {
                    var ageRange = DbContext.SMAgeRanges.Find(id);
                    ageRange.Label = start + "-" + end;
                    ageRange.Start = start;
                    ageRange.End = end;
                    DbContext.Entry(ageRange).State = EntityState.Modified;
                }
                else
                {
                    if (idCustomCriteriaDefinition == -1)
                    {
                        var customCriteriaDefinition = DbContext.SMCustomCriteriaDefinitions.FirstOrDefault(x => x.Domain.Id == domain.Id && x.IsAgeRange && x.Status == CustomCriteriaStatus.Active);
                        if (customCriteriaDefinition == null)
                        {
                            customCriteriaDefinition = new CustomCriteriaDefinition
                            {
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = DbContext.QbicleUser.Find(userId),
                                Domain = domain,
                                Label = "Age ranges",
                                IsAgeRange = true,
                                Status = CustomCriteriaStatus.Active
                            };
                        }

                        var ar = new AgeRange();
                        ar.DisplayOrder = 1;
                        ar.Label = "1-10";
                        ar.Start = 1;
                        ar.End = 10;
                        customCriteriaDefinition.CustomOptions.Add(ar);
                        if (customCriteriaDefinition.Id > 0)
                        {
                            if (DbContext.Entry(customCriteriaDefinition).State == EntityState.Detached)
                                DbContext.SMCustomCriteriaDefinitions.Attach(customCriteriaDefinition);
                            DbContext.Entry(customCriteriaDefinition).State = EntityState.Modified;
                        }
                        else
                        {
                            DbContext.SMCustomCriteriaDefinitions.Add(customCriteriaDefinition);
                            DbContext.Entry(customCriteriaDefinition).State = EntityState.Added;
                        }
                    }
                    else
                    {
                        var count = DbContext.SMAgeRanges.OrderByDescending(x => x.DisplayOrder).FirstOrDefault();
                        var customCriteriaDefinition = DbContext.SMCustomCriteriaDefinitions.Find(idCustomCriteriaDefinition);
                        AgeRange ageRange = new AgeRange();
                        ageRange.Label = start + "-" + end;
                        ageRange.DisplayOrder = count == null ? 1 : count.DisplayOrder + 1;
                        ageRange.Start = start;
                        ageRange.End = end;
                        ageRange.CustomCriteriaDefinition = customCriteriaDefinition;
                        DbContext.SMAgeRanges.Add(ageRange);
                        DbContext.Entry(ageRange).State = EntityState.Added;
                    }

                }
                var result = DbContext.SaveChanges();
                if (result > 0)
                {
                    refModel.result = true;
                }
                else
                {
                    refModel.result = false;
                }
                return refModel;
            }
            catch (DbEntityValidationException ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, id,
                    idCustomCriteriaDefinition, start, end, domain);
                return refModel;
            }
        }

        public ReturnJsonModel DeleteAgeRanges(int id, int domainId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete age ranges", null, null, id, domainId);

                var ageRange = DbContext.SMAgeRanges.Find(id);
                if (ageRange != null)
                {
                    DbContext.SMAgeRanges.Remove(ageRange);
                }
                var result = DbContext.SaveChanges();
                if (result > 0)
                {
                    var listCustomOption = DbContext.SMCustomCriteriaDefinitions.FirstOrDefault(x => x.Domain.Id == domainId && x.IsAgeRange);
                    if (listCustomOption != null && listCustomOption.CustomOptions.Count == 0)
                    {
                        DbContext.SMCustomCriteriaDefinitions.Remove(listCustomOption);
                    }
                    DbContext.SaveChanges();
                    refModel.result = true;
                }
                else
                {
                    refModel.result = false;
                }
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, domainId);
                return refModel;
            }
        }
    }
}
