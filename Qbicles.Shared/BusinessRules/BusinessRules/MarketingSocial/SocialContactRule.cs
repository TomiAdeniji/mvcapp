using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class SocialContactRule
    {
        #region init class
        private ApplicationDbContext _db;
        private readonly string _currentTimeZone = "";
        string SESEmailTemplateNameConst = ConfigManager.SESIdentityVerificationTemplateNamePrefix + "_SES_email_verification";

        public SocialContactRule()
        {
        }
        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }
        public SocialContactRule(ApplicationDbContext context, string currentTimeZone = "")
        {
            _db = context;
            _currentTimeZone = currentTimeZone;
        }
        #endregion
        public ReturnJsonModel SaveContactCriteria(CriteriaCustomeModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save contact criteria", userId, null, model);

                var dbCriteria = DbContext.SMCustomCriteriaDefinitions.Find(model.Id);
                if (dbCriteria != null)
                {
                    dbCriteria.Label = model.Label;
                    dbCriteria.IsMandatory = model.IsMandatory;
                    dbCriteria.LastUpdateDate = DateTime.Now;
                    dbCriteria.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                    //check Option remove
                    var _listCustomValues = new List<CustomOption>();
                    foreach (var item in dbCriteria.CustomOptions)
                    {
                        if (!model.Options.Any(s => s.Id == item.Id))
                        {
                            if (DbContext.SMCriteriaValues.Any(s => s.Option.Id == item.Id) || DbContext.SMSegmentQueryClauses.Any(s => s.Options.Any(o => o.Id == item.Id)))
                            {
                                returnJson.msg = ResourcesManager._L("ERROR_MSG_241");
                                return returnJson;
                            }
                            _listCustomValues.Add(item);
                        }
                    }
                    DbContext.SMCustomOptions.RemoveRange(_listCustomValues);
                    //Check Option add and edit
                    foreach (var item in model.Options)
                    {
                        var ov = DbContext.SMCustomOptions.Find(item.Id);
                        if (ov != null)
                        {
                            ov.Label = item.Label;
                            ov.DisplayOrder = item.DisplayOrder;
                            if (DbContext.Entry(ov).State == EntityState.Detached)
                                DbContext.SMCustomOptions.Attach(ov);
                            DbContext.Entry(ov).State = EntityState.Modified;
                        }
                        else
                        {
                            CustomOption option = new CustomOption();
                            option.Label = item.Label;
                            option.DisplayOrder = item.DisplayOrder;
                            dbCriteria.CustomOptions.Add(option);
                        }
                    }
                    if (DbContext.Entry(dbCriteria).State == EntityState.Detached)
                        DbContext.SMCustomCriteriaDefinitions.Attach(dbCriteria);
                    DbContext.Entry(dbCriteria).State = EntityState.Modified;
                }
                else
                {
                    CustomCriteriaDefinition customCriteria = new CustomCriteriaDefinition();
                    customCriteria.Label = model.Label;
                    customCriteria.IsAgeRange = false;
                    customCriteria.IsMandatory = model.IsMandatory;
                    customCriteria.CreatedDate = DateTime.UtcNow;
                    customCriteria.CreatedBy = DbContext.QbicleUser.Find(userId);
                    customCriteria.Status = CustomCriteriaStatus.Active;
                    customCriteria.DisplayOrder = DbContext.SMCustomCriteriaDefinitions.Where(s => s.Domain.Id == model.Domain.Id).Select(s => s.DisplayOrder).DefaultIfEmpty(0).Max() + 1;
                    customCriteria.Domain = model.Domain;
                    //Option add 
                    foreach (var item in model.Options)
                    {
                        CustomOption option = new CustomOption();
                        option.Label = item.Label;
                        option.DisplayOrder = item.DisplayOrder;
                        DbContext.SMCustomOptions.Add(option);
                        DbContext.Entry(option).State = EntityState.Added;
                        customCriteria.CustomOptions.Add(option);
                    }
                    DbContext.SMCustomCriteriaDefinitions.Add(customCriteria);
                    DbContext.Entry(customCriteria).State = EntityState.Added;
                }
                returnJson.result = DbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
                returnJson.msg = ex.Message;
            }
            return returnJson;
        }
        public List<CustomCriteriaDefinition> GetCriteriaDefinitions(int domainId, bool isLoadAgeRange = false, int[] notCriterias = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get criteria definitions", null, null, domainId, isLoadAgeRange, notCriterias);

                if (notCriterias != null)
                {
                    if (isLoadAgeRange)
                        return DbContext.SMCustomCriteriaDefinitions.Where(s => s.Domain.Id == domainId && !notCriterias.Any(c => c == s.Id)).OrderBy(s => s.DisplayOrder).ToList();
                    return DbContext.SMCustomCriteriaDefinitions.Where(s => s.Domain.Id == domainId && !s.IsAgeRange && !notCriterias.Any(c => c == s.Id)).OrderBy(s => s.DisplayOrder).ToList();
                }
                else
                {
                    if (isLoadAgeRange)
                        return DbContext.SMCustomCriteriaDefinitions.Where(s => s.Domain.Id == domainId).OrderBy(s => s.DisplayOrder).ToList();
                    return DbContext.SMCustomCriteriaDefinitions.Where(s => s.Domain.Id == domainId && !s.IsAgeRange).OrderBy(s => s.DisplayOrder).ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, isLoadAgeRange, notCriterias);
                return new List<CustomCriteriaDefinition>();
            }
        }
        public ReturnJsonModel DeleteContactCriteria(int criteriaId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete contact criteria", null, null, criteriaId);

                var dbCriteria = DbContext.SMCustomCriteriaDefinitions.Find(criteriaId);
                if (dbCriteria != null)
                {
                    if (DbContext.SMCriteriaValues.Any(s => s.Criteria.Id == criteriaId) || DbContext.SMSegmentQueryClauses.Any(s => s.CriteriaDefinition.Id == criteriaId))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_241");
                        return returnJson;
                    }
                    DbContext.SMCustomCriteriaDefinitions.Remove(dbCriteria);
                }
                returnJson.result = DbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, criteriaId);
                returnJson.msg = ex.Message;
            }
            return returnJson;
        }
        public CustomCriteriaDefinition GetCriteriaDefinitionById(int criteriaId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get criteria definition by id", null, null, criteriaId);

                return DbContext.SMCustomCriteriaDefinitions.Find(criteriaId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, criteriaId);
                return new CustomCriteriaDefinition();
            }
        }
        public ReturnJsonModel SetStatusContactCriteria(int criteriaId, bool status)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set status contact criteria", null, null, criteriaId, status);

                var _criteria = DbContext.SMCustomCriteriaDefinitions.Find(criteriaId);
                if (_criteria != null)
                {
                    _criteria.Status = status ? CustomCriteriaStatus.Active : CustomCriteriaStatus.InActive;
                    if (DbContext.Entry(_criteria).State == EntityState.Detached)
                        DbContext.SMCustomCriteriaDefinitions.Attach(_criteria);
                    DbContext.Entry(_criteria).State = EntityState.Modified;
                }
                returnJson.result = DbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, criteriaId, status);
            }
            return returnJson;
        }
        public ReturnJsonModel MoveUpDownOrderCriteria(CriteriaReOrderModel model)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Move up or down criteria", null, null, model);

                var dbMoveUp = DbContext.SMCustomCriteriaDefinitions.Find(model.MoveUpId);
                if (dbMoveUp != null)
                {
                    dbMoveUp.DisplayOrder = model.MoveUpOrder;
                    if (DbContext.Entry(dbMoveUp).State == EntityState.Detached)
                        DbContext.SMCustomCriteriaDefinitions.Attach(dbMoveUp);
                    DbContext.Entry(dbMoveUp).State = EntityState.Modified;
                }
                var dbMoveDown = DbContext.SMCustomCriteriaDefinitions.Find(model.MoveDownId);
                if (dbMoveDown != null)
                {
                    dbMoveDown.DisplayOrder = model.MoveDownOrder;
                    if (DbContext.Entry(dbMoveDown).State == EntityState.Detached)
                        DbContext.SMCustomCriteriaDefinitions.Attach(dbMoveDown);
                    DbContext.Entry(dbMoveDown).State = EntityState.Modified;
                }
                returnJson.result = DbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                returnJson.msg = ex.Message;
            }
            return returnJson;
        }
        public List<SMContact> GetContactsBySegment(int domainId, List<ClauseCriteriaModel> clauses, List<int> areaIds)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get contacts by segment", null, null, domainId, clauses, areaIds);

                var query = DbContext.SMContacts.Where(s => s.Domain.Id == domainId);
                if (areaIds != null && areaIds.Any())
                {
                    query = query.Where(s => s.Places.Any(p => p.Areas.Any(a => areaIds.Any(o => o == a.Id))));
                }
                if (clauses != null && clauses.Any())
                {
                    foreach (var item in clauses)
                    {
                        query = query.Where(s => s.CriteriaValues.Any(c => c.Criteria.Id == item.CriteriaId && item.CriteriaValues.Any(op => op == c.Option.Id)));
                    }
                }

                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, clauses, areaIds);
                return new List<SMContact>();
            }
        }

        public List<SMContactModel> GetListSMContact(int column, string orderby, string search, int domainId, int start, int length, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list SMContact", null, null, column, orderby, search, domainId, start, length, totalRecord);

                var query = DbContext.SMContacts.Where(c => c.Domain.Id == domainId && (search.Equals("") || c.Name.ToLower().Contains(search.Trim().ToLower()) || c.Email.ToLower().Contains(search.Trim().ToLower()) || c.PhoneNumber.ToLower().Contains(search.Trim().ToLower())));
                totalRecord = query.Count();
                if (column == 1)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(p => p.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.Name);
                    }

                }
                else if (column == 2)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(p => p.Email);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.Email);
                    }
                }
                else if (column == 3)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(p => p.PhoneNumber);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.PhoneNumber);
                    }
                }


                List<SMContact> lstContact = query.ToList();

                List<SMContactModel> lstModel = new List<SMContactModel>();

                foreach (SMContact contact in lstContact)
                {
                    SMContactModel model = new SMContactModel();
                    model.Id = contact.Id;
                    model.Name = contact.Name;
                    model.Phone = contact.PhoneNumber;
                    model.Email = contact.Email;
                    model.AvatarUri = contact.AvatarUri;
                    switch (contact.Source)
                    {
                        case ContactSourceEnum.Customer: model.SourceName = "Customer"; break;
                        case ContactSourceEnum.Trader: model.SourceName = "Trader"; break;
                        case ContactSourceEnum.EnquiryForm: model.SourceName = "Enquiry Form"; break;
                        case ContactSourceEnum.SalesCall: model.SourceName = "Sales Call"; break;
                        case ContactSourceEnum.Other: model.SourceName = "Other"; break;
                    }
                    if (contact.IsSubscribed)
                    {
                        model.ReceiveEmail = "Yes";
                    }
                    else
                    {
                        model.ReceiveEmail = "No";
                    }
                    lstModel.Add(model);
                }

                if (column == 4)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstModel = lstModel.OrderBy(p => p.SourceName).ToList();
                    }
                    else
                    {
                        lstModel = lstModel.OrderByDescending(p => p.SourceName).ToList();
                    }
                }

                if (column == 5)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstModel = lstModel.OrderBy(p => p.ReceiveEmail).ToList();
                    }
                    else
                    {
                        lstModel = lstModel.OrderByDescending(p => p.ReceiveEmail).ToList();
                    }
                }


                if (length != 0)
                {
                    lstModel = lstModel.Skip(start).Take(length).ToList();
                }
                else
                {
                    lstModel = lstModel.Skip(start).ToList();
                }

                return lstModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, column, orderby, search, domainId, start, length, totalRecord);
                totalRecord = 0;
                return new List<SMContactModel>();
            }
        }

        public List<SMContact> GetListContact(string search, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list contact", null, null, search, domainId);

                var query = DbContext.SMContacts.Where(c => c.Domain.Id == domainId && (search.Equals("") || c.Name.ToLower().Contains(search.Trim().ToLower()) || c.Email.ToLower().Contains(search.Trim().ToLower()) || c.PhoneNumber.ToLower().Contains(search.Trim().ToLower()))).OrderBy(c => c.Name);
                List<SMContact> lstContact = query.ToList();
                return lstContact;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, search, domainId);
                return new List<SMContact>();
            }
        }

        public SMContact GetSMContractById(long smContractId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get SMContact by id", null, null, smContractId);


                return DbContext.SMContacts.Find(smContractId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, smContractId);
                return null;
            }
        }

        public SMContact GetSMContractByEmail(string email)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get SMContact by email", null, null, email);

                return DbContext.SMContacts.Where(c => c.Email.Equals(email)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email);
                return null;
            }
        }

        public int SaveSMContact(SMContact contact)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save SMContact", null, null, contact);

                if (DbContext.Entry(contact).State == EntityState.Detached)
                    DbContext.SMContacts.Attach(contact);
                DbContext.Entry(contact).State = EntityState.Modified;
                return DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contact);
                return 0;
            }
        }

        public ReturnJsonModel SaveSMContact(SMContactModel model, string userId, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save SMContact", userId, null, model, domain);

                if (!string.IsNullOrEmpty(model.AvatarUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(model.AvatarUri);
                }
                SMContact smContract = DbContext.SMContacts.Find(model.Id);
                var user = DbContext.QbicleUser.Find(userId);
                if (smContract != null)
                {
                    smContract.AvatarUri = smContract.AvatarUri = String.IsNullOrEmpty(model.AvatarUri) ? smContract.AvatarUri : model.AvatarUri;
                    smContract.BirthDay = model.BirthDay;
                    smContract.Domain = domain;
                    smContract.Email = model.Email;
                    smContract.Name = model.Name;
                    smContract.PhoneNumber = model.Phone;
                    switch (model.Source)
                    {
                        case 0: smContract.Source = ContactSourceEnum.Customer; smContract.SourceDescription = "Customer"; break;
                        case 1: smContract.Source = ContactSourceEnum.Trader; smContract.SourceDescription = "Trader"; break;
                        case 2: smContract.Source = ContactSourceEnum.EnquiryForm; smContract.SourceDescription = "Enquiry Form"; break;
                        case 3: smContract.Source = ContactSourceEnum.SalesCall; smContract.SourceDescription = "Sales Call"; break;
                        case 4: smContract.Source = ContactSourceEnum.Other; smContract.SourceDescription = model.SourceDescription; break;
                    }

                    smContract.LastUpdatedBy = user;
                    smContract.LastUpdateDate = smContract.CreatedDate;
                    smContract.Places.Clear();
                    if (model.Places != null)
                    {
                        smContract.Places.AddRange(DbContext.SMPlaces.Where(p => model.Places.Contains(p.Id)));
                    }

                    if (DbContext.Entry(smContract).State == EntityState.Detached)
                        DbContext.SMContacts.Attach(smContract);
                    DbContext.Entry(smContract).State = EntityState.Modified;
                }
                else
                {
                    smContract = new SMContact
                    {
                        AvatarUri = model.AvatarUri,
                        BirthDay = model.BirthDay,
                        Domain = domain,
                        Email = model.Email,
                        Name = model.Name,
                        PhoneNumber = model.Phone
                    };
                    switch (model.Source)
                    {
                        case 0: smContract.Source = ContactSourceEnum.Customer; smContract.SourceDescription = "Customer"; break;
                        case 1: smContract.Source = ContactSourceEnum.Trader; smContract.SourceDescription = "Trader"; break;
                        case 2: smContract.Source = ContactSourceEnum.EnquiryForm; smContract.SourceDescription = "Enquiry Form"; break;
                        case 3: smContract.Source = ContactSourceEnum.SalesCall; smContract.SourceDescription = "Sales Call"; break;
                        case 4: smContract.Source = ContactSourceEnum.Other; smContract.SourceDescription = model.SourceDescription; break;
                    }

                    smContract.CreatedDate = DateTime.UtcNow;
                    smContract.LastUpdatedBy = user;
                    smContract.LastUpdateDate = smContract.CreatedDate;
                    smContract.CreatedBy = user;
                    if (model.Places != null)
                    {
                        smContract.Places.AddRange(DbContext.SMPlaces.Where(p => model.Places.Contains(p.Id)));
                    }
                    DbContext.SMContacts.Add(smContract);
                    DbContext.Entry(smContract).State = EntityState.Added;
                }


                DbContext.SaveChanges();
                DbContext.SMCriteriaValues.RemoveRange(smContract.CriteriaValues);
                DbContext.SaveChanges();
                if (model.AgeRanges != null && model.AgeRanges.Any())
                {
                    //Id CustomCriteriaDefinition "eg:CustomCriteriaDefinitionID_CustomOptionID"
                    var _criteriaIds = model.AgeRanges[0].Split('_');
                    if (!string.IsNullOrEmpty(_criteriaIds[0]))
                    {
                        CustomCriteriaDefinition customCriteria = DbContext.SMCustomCriteriaDefinitions.Find(Convert.ToInt32(_criteriaIds[0]));
                        foreach (var optId in model.AgeRanges)
                        {
                            var _values = optId.Split('_');
                            CustomOption option = DbContext.SMCustomOptions.Find(Convert.ToInt32(_values[1]));//Id custom option "eg:CustomCriteriaDefinitionID_CustomOptionID"
                            if (customCriteria != null && option != null)
                            {
                                var _dbcriteriavalue = smContract.CriteriaValues.FirstOrDefault(s => s.Option.Id == option.Id && s.Criteria.Id == customCriteria.Id);
                                if (_dbcriteriavalue == null)
                                {
                                    CriteriaValue criteriavalue = new CriteriaValue();
                                    criteriavalue.Criteria = customCriteria;
                                    criteriavalue.Option = option;
                                    criteriavalue.Contact = smContract;
                                    DbContext.SMCriteriaValues.Add(criteriavalue);
                                    DbContext.Entry(criteriavalue).State = EntityState.Added;
                                    DbContext.SaveChanges();
                                }
                            }
                        }
                    }

                }

                if (model.Options != null)
                {

                    foreach (var optId in model.Options)
                    {
                        if (!string.IsNullOrEmpty(optId))
                        {
                            var _values = optId.Split('_');
                            CustomOption option = DbContext.SMCustomOptions.Find(Convert.ToInt32(_values[1]));//Id custom option "eg:CustomCriteriaDefinitionID_CustomOptionID"
                            CustomCriteriaDefinition customCriteria = DbContext.SMCustomCriteriaDefinitions.Find(Convert.ToInt32(_values[0]));
                            var _dbcriteriavalue = smContract.CriteriaValues.FirstOrDefault(s => s.Option.Id == option?.Id);
                            if (_dbcriteriavalue == null && option != null && customCriteria != null)
                            {
                                CriteriaValue criteriavalue = new CriteriaValue();
                                criteriavalue.Criteria = customCriteria;
                                criteriavalue.Option = option;
                                criteriavalue.Contact = smContract;
                                DbContext.SMCriteriaValues.Add(criteriavalue);
                                DbContext.Entry(criteriavalue).State = EntityState.Added;
                                DbContext.SaveChanges();
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }

                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model, domain);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel DeleteSMContact(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete SMContact", null, null, id);

                var smContact = DbContext.SMContacts.Find(id);
                if (smContact != null)
                {
                    DbContext.SMContacts.Remove(smContact);
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

        public ReturnJsonModel VerifyIdentity(string email, int domainId, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, email);

                var setting = DbContext.EmailConfigurations.FirstOrDefault();
                var region = RegionEndpoint.GetBySystemName(ConfigManager.BucketRegion);
                var currentIdentity = DbContext.SESIdentities.FirstOrDefault(p => p.Domain.Id == domainId && p.Email == email);
                if (currentIdentity != null)
                {
                    refModel.result = false;
                    refModel.msg = "An identity with the email existed in the current Domain";
                    return refModel;
                }
                var user = DbContext.QbicleUser.FirstOrDefault(p => p.Id == userId);
                var domain = DbContext.Domains.FirstOrDefault(p => p.Id == domainId);
                var client = new AmazonSimpleEmailServiceClient(ConfigManager.SESAccessKey, ConfigManager.SESSecretKey, region);

                // Create template
                var successUrl = ConfigManager.QbiclesUrl + "/SalesMarketing/SESVerifySuccessPage";
                var failUrl = ConfigManager.QbiclesUrl + "/SalesMarketing/SESVerifyFailPage";
                try
                {
                    var template = client.GetCustomVerificationEmailTemplate(new GetCustomVerificationEmailTemplateRequest()
                    {
                        TemplateName = SESEmailTemplateNameConst
                    });
                } catch
                {
                    var create_template_response = client.CreateCustomVerificationEmailTemplate(new CreateCustomVerificationEmailTemplateRequest
                    {
                        TemplateName = SESEmailTemplateNameConst,
                        FromEmailAddress = "noreply@qbicles.com",
                        TemplateSubject = "Please confirm your email address",
                        TemplateContent = "<html><head></head><body style='font-family:sans-serif;'><h1 style='text-align:center'>Verify your email address</h1><p>Before you can start bulk sending emails using Qbicles, you need to verify your address for security purposes. Click or tap the link to confirm your address, and we'll get you started.</p></body></html>",
                        SuccessRedirectionURL = successUrl,
                        FailureRedirectionURL = failUrl
                    });
                }

                var response = client.SendCustomVerificationEmail(new SendCustomVerificationEmailRequest
                {
                    EmailAddress = email,
                    TemplateName = SESEmailTemplateNameConst
                });
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                    return refModel;
                }
                var identity = new SESIdentity()
                {
                    Email = email,
                    Domain = domain,
                    Status = SESIdentityStatus.Pending,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    FailReason = ""
                };
                DbContext.SESIdentities.Add(identity);
                DbContext.Entry(identity).State = EntityState.Added;
                DbContext.SaveChanges();
                refModel.Object = response;
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email);
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return refModel;
            }
        }

        public ReturnJsonModel CreateSESEmailTemplate()
        {
            var result = new ReturnJsonModel()
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, null);
                var region = RegionEndpoint.GetBySystemName(ConfigManager.BucketRegion);
                var client = new AmazonSimpleEmailServiceClient(ConfigManager.SESAccessKey, ConfigManager.SESSecretKey, region);

                var successUrl = ConfigManager.QbiclesUrl + "/SalesMarketing/SESVerifySuccessPage";
                var failUrl = ConfigManager.QbiclesUrl + "/SalesMarketing/SESVerifyFailPage";

                var template = client.GetCustomVerificationEmailTemplate(new GetCustomVerificationEmailTemplateRequest()
                {
                    TemplateName = SESEmailTemplateNameConst
                });
                if(template == null)
                {
                    var response = client.CreateCustomVerificationEmailTemplate(new CreateCustomVerificationEmailTemplateRequest
                    {
                        TemplateName = SESEmailTemplateNameConst,
                        FromEmailAddress = "noreply@qbicles.com",
                        TemplateSubject = "Please confirm your email address",
                        TemplateContent = "<html><head></head><body style='font-family:sans-serif;'><h1 style='text-align:center'>Verify your email address</h1><p>Before you can start bulk sending emails using Qbicles, you need to verify your address for security purposes. Click or tap the link to confirm your address, and we'll get you started.</p></body></html>",
                        SuccessRedirectionURL = successUrl,
                        FailureRedirectionURL = failUrl
                    });
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        result.result = true;
                }
                else
                {
                    var response = client.UpdateCustomVerificationEmailTemplate(new UpdateCustomVerificationEmailTemplateRequest()
                    {
                        TemplateName = SESEmailTemplateNameConst,
                        FromEmailAddress = "noreply@qbicles.com",
                        TemplateSubject = "Please confirm your email address",
                        TemplateContent = "<html><head></head><body style='font-family:sans-serif;'><h1 style='text-align:center'>Verify your email address</h1><p>Before you can start bulk sending emails using Qbicles, you need to verify your address for security purposes. Click or tap the link to confirm your address, and we'll get you started.</p></body></html>",
                        SuccessRedirectionURL = successUrl,
                        FailureRedirectionURL = failUrl
                    });
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        result.result = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
            }

            return result;
        }

        public ReturnJsonModel DeleteSESVerificationEmailTemplate(string templateName)
        {
            var rs = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, templateName);

                var region = RegionEndpoint.GetBySystemName(ConfigManager.BucketRegion);
                var client = new AmazonSimpleEmailServiceClient(ConfigManager.SESAccessKey, ConfigManager.SESSecretKey, region);

                var response = client.DeleteCustomVerificationEmailTemplate(new DeleteCustomVerificationEmailTemplateRequest
                {
                    TemplateName = templateName
                });
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    rs.result = true;
                }
                return rs;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, templateName);
                rs.result = false;
                rs.msg = ex.Message;
                return rs;
            }
        }

        public List<SESIdentityCustomModel> GetListSESIdentities(int domainId, string key, SESIdentityStatus status, string timeZone,
            string timeFormat, ref int totalRecord, IDataTablesRequest requestModel, int start = 0, int length = 10)
        {
            var lst_identities = new List<SESIdentityCustomModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, key, status, timeZone, timeFormat, requestModel);
                }
                var query = DbContext.SESIdentities.Where(p => p.Domain.Id == domainId);
                #region Filtering
                if (!string.IsNullOrEmpty(key))
                {
                    query = query.Where(p => p.Email.ToLower().Contains(key.ToLower()));
                }
                if (status > 0)
                {
                    query = query.Where(p => p.Status == status);
                }
                #endregion
                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var sortedString = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Address":
                            sortedString += String.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Email asc" : "Email desc";
                            break;
                        case "Status":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Status asc" : "Status desc";
                            break;
                        case "AddedDate":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "CreatedDate asc" : "CreatedDate desc";
                            break;
                    }
                }
                query = query.OrderBy(string.IsNullOrEmpty(sortedString) ? "CreatedDate desc" : sortedString);
                #endregion
                #region Paging
                totalRecord = query.Count();
                var sesIdentityList = query.Skip(start).Take(length);
                #endregion
                sesIdentityList.ForEach(p =>
                {
                    var identityItem = new SESIdentityCustomModel()
                    {
                        Id = p.Id,
                        Address = p.Email,
                        Added = p.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(timeFormat),
                        Status = p.Status,
                        StatusLabel = GenerateSESIdentityLabelHtml(p.Status)
                    };
                    lst_identities.Add(identityItem);
                });
                return lst_identities;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, key, status, timeZone, timeFormat, requestModel);
                return new List<SESIdentityCustomModel>();
            }
        }

        public string GenerateSESIdentityLabelHtml(SESIdentityStatus status)
        {
            try
            {
                switch (status)
                {
                    case SESIdentityStatus.Pending:
                        return "<label class=\"label label-lg label-warning\">Pending</label>";
                    case SESIdentityStatus.Verified:
                        return "<label class=\"label label-lg label-success\">Verified</label>";
                    case SESIdentityStatus.Failed:
                        return "<label class=\"label label-lg label-danger\">Failed</label>";
                    default:
                        return "";
                }
            }
            catch
            {
                return "";
            }
        }

        public ReturnJsonModel DeleteSESIdentity(int identityId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, identityId);

                var region = RegionEndpoint.GetBySystemName(ConfigManager.BucketRegion);

                var identity = DbContext.SESIdentities.FirstOrDefault(p => p.Id == identityId);
                if (identity == null)
                {
                    refModel.result = false;
                    refModel.msg = "The email identity does not exist.";
                    return refModel;
                }
                var client = new AmazonSimpleEmailServiceClient(ConfigManager.SESAccessKey, ConfigManager.SESSecretKey, region);
                var response = client.DeleteIdentity(new DeleteIdentityRequest
                {
                    Identity = identity.Email
                });
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                    return refModel;
                }
                DbContext.SESIdentities.Remove(identity);
                DbContext.Entry(identity).State = EntityState.Deleted;
                DbContext.SaveChanges();
                refModel.Object = response;
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, identityId);
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return refModel;
            }
        }

        public ReturnJsonModel UpdateIdentitiesStatus(QbicleDomain domain, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domain, userId);

                var region = RegionEndpoint.GetBySystemName(ConfigManager.BucketRegion);
                var user = DbContext.QbicleUser.Find(userId);
                var domainId = domain.Id;
                var lstEmail = DbContext.SESIdentities.Where(p => p.Domain.Id == domainId).Select(p => p.Email).ToList();
                if (lstEmail == null || lstEmail.Count <= 0)
                {
                    refModel.result = true;
                    return refModel;
                }
                var client = new AmazonSimpleEmailServiceClient(ConfigManager.SESAccessKey, ConfigManager.SESSecretKey, region);
                var response = client.GetIdentityVerificationAttributes(new GetIdentityVerificationAttributesRequest()
                {
                    Identities = lstEmail
                });
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                    return refModel;
                }
                foreach (var emailItem in lstEmail)
                {
                    var identity = DbContext.SESIdentities.FirstOrDefault(p => p.Email == emailItem && p.Domain.Id == domainId);
                    var lstResponseItem = response.VerificationAttributes.Where(x => x.Key == emailItem).ToList();
                    if ((lstResponseItem == null || lstResponseItem.Count <= 0) && identity != null)
                    {
                        DbContext.SESIdentities.Remove(identity);
                        DbContext.Entry(identity).State = EntityState.Deleted;
                        continue;
                    }
                    var responseItem = lstResponseItem.FirstOrDefault();
                    var identityUpdatedStatus = MapIdentityStatus(responseItem.Value.VerificationStatus);
                    if (identity == null)
                    {
                        identity = new SESIdentity()
                        {
                            Email = responseItem.Key,
                            Domain = domain,
                            Status = identityUpdatedStatus,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            FailReason = ""
                        };
                        DbContext.SESIdentities.Add(identity);
                        DbContext.Entry(identity).State = EntityState.Added;
                    }
                    else
                    {
                        identity.Status = identityUpdatedStatus;
                        DbContext.Entry(identity).State = EntityState.Modified;
                    }
                }

                DbContext.SaveChanges();
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domain);
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return refModel;
            }
        }

        public SESIdentityStatus MapIdentityStatus(VerificationStatus sesIdentityAttribute)
        {
            if (sesIdentityAttribute == VerificationStatus.Failed || sesIdentityAttribute == VerificationStatus.TemporaryFailure)
            {
                return SESIdentityStatus.Failed;
            }
            else if (sesIdentityAttribute == VerificationStatus.Pending)
            {
                return SESIdentityStatus.Pending;
            }
            else if (sesIdentityAttribute == VerificationStatus.Success)
            {
                return SESIdentityStatus.Verified;
            }
            else
            {
                return SESIdentityStatus.Pending;
            }
        }

        public List<string> GetListVerifiedSESEmailByDomain(int domainId)
        {
            return DbContext.SESIdentities.Where(p => p.Domain.Id == domainId && p.Status == SESIdentityStatus.Verified).Select(p => p.Email).ToList();
        }
    }
}
