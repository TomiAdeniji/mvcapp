using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Loyalty;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Loyalty
{
    public class StorePointConversionRules
    {
        private readonly ApplicationDbContext _dbContext;

        public StorePointConversionRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public List<OrderToPointsConversion> GetConversions(QbicleDomain paymentDomain, OrderToPointsConversionType conversionType, bool isArchived = false)
        {
            
            try
            {
                var lstConversions = new List<OrderToPointsConversion>();
                lstConversions = _dbContext.OrderToPointsConversions.Where(p => p.Domain.Id == paymentDomain.Id && p.ConversionType == conversionType && p.IsArchived == isArchived).ToList();
                return lstConversions;
            } catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, paymentDomain, conversionType, isArchived);
                return null;
            }
        }

        /// <summary>
        /// This method retrns a list of the conversions that are based on which type i.e. OrderToPointsConversionType.Payment = 1
        /// In addition the method allows the user to specify whether archived or non-archived conversions should be returned
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="conversionType"></param>
        /// <param name="isArchived"></param>
        /// <returns></returns>
        public List<LoyaltyConversionCustomModel> GetPaymentConversionsPagination(string dateTimeFormat, string timeZone, CurrencySetting currencySettings, OrderToPointsConversionType conversionType, ref int totalRecord,
                                                    IDataTablesRequest requestModel, int domainId, bool isArchived = false, int start = 0, int length = 10)
        {
            //Get the OrderToPointsConversions of Type conversionType
            // taking into consideration if isArchived is true or false 
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, conversionType, domainId, isArchived, dateTimeFormat, timeZone, currencySettings);

                var query = _dbContext.PaymentConversions.Where(p => p.Domain.Id == domainId && p.ConversionType == conversionType && p.IsArchived == isArchived);
                //The conversions must be returned in order: Id DESC i.e. the newest at the top
                query = query.OrderByDescending(p => p.Id);
                totalRecord = query.Count();

                //Paging
                query = query.Skip(start).Take(length);

                //Get data
                var lstConversionRules = query.ToList();
                var lstCustomModels = new List<LoyaltyConversionCustomModel>();
                lstConversionRules.ForEach(p =>
                {
                    var conversionItem = new LoyaltyConversionCustomModel()
                    {
                        RuleId = p.Id,
                        Amount = p.Amount.ToDecimalPlace(currencySettings),
                        Points = p.Points,
                        ArchievedDate = p.ArchivedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                        ArchievedBy = $"<a href=\"/Community/UserProfilePage?uId={p.ArchivedBy.Id}\" target=\"_blank\">{p.ArchivedBy.GetFullName()}</a>"
                    };
                    lstCustomModels.Add(conversionItem);
                });
                return lstCustomModels;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, conversionType, isArchived, domainId, dateTimeFormat, timeZone, currencySettings);
                return new List<LoyaltyConversionCustomModel>();
            }

        }

        public OrderToPointsConversion GetActiveConversion(int domainId, OrderToPointsConversionType conversionType)
        {
            return _dbContext.OrderToPointsConversions.FirstOrDefault(p => p.Domain.Id == domainId && p.ConversionType == conversionType && p.IsArchived == false);
        }

        public ReturnJsonModel AddConversion(int domainId, string currentUserId, PaymentConversion conversion)
        {
            try
            {
                // Find the existing NON-ARCHIVED (i.e. current) OrderToPointsConversion 
                //      Where Type = converstion.Type
                // If there is an existing conversion
                //      For that currentConversion
                //          Archive it i.e. currentConversion.isArchived = true
                //          Set ArchivedDate = UTCNOW
                //          Set ArchivedBy = currentUser 
                var currentConversion = _dbContext.OrderToPointsConversions.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
                var currentUser = _dbContext.QbicleUser.Find(currentUserId);
                var currentDomain = _dbContext.Domains.Find(domainId);
                if (currentConversion != null)
                {
                    currentConversion.IsArchived = true;
                    currentConversion.ArchivedDate = DateTime.UtcNow;
                    currentConversion.ArchivedBy = currentUser;
                    _dbContext.Entry(currentConversion).State = EntityState.Modified;
                }

                // Add the new conversion to the Domain
                // For the new conversion
                //     Set the CreatedDate = UTCNOW
                //     Set the CreatedBy = currentUser
                conversion.Domain = currentDomain;
                conversion.CreatedDate = DateTime.UtcNow;
                conversion.CreatedBy = currentUser;
                conversion.IsArchived = false;
                _dbContext.PaymentConversions.Add(conversion);
                _dbContext.Entry(conversion).State = EntityState.Added;
                _dbContext.SaveChanges();

                return new ReturnJsonModel() { actionVal = 1, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, currentUserId, conversion);
                return new ReturnJsonModel() { actionVal = 1, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel AddLoyaltySystemSettings(string currentUserId, SystemSettings sysSettings)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, currentUserId, sysSettings);

                var currentUser = _dbContext.QbicleUser.Find(currentUserId);

                // Update current Active System Settings
                // Set SystemSettings.IsArchieved = false
                var currentSystemSetting = _dbContext.LoyaltySystemSettings.FirstOrDefault(p => p.IsArchived == false);
                if(currentSystemSetting != null)
                {
                    currentSystemSetting.IsArchived = true;
                    currentSystemSetting.ArchivedBy = currentUser;
                    currentSystemSetting.ArchivedDate = DateTime.UtcNow;
                    _dbContext.Entry(currentSystemSetting).State = EntityState.Modified;
                }

                // Create new SystemSettings
                sysSettings.CreatedBy = currentUser;
                sysSettings.CreatedDate = DateTime.UtcNow;
                sysSettings.IsArchived = false;
                _dbContext.LoyaltySystemSettings.Add(sysSettings);
                _dbContext.Entry(sysSettings).State = EntityState.Added;

                _dbContext.SaveChanges();
                return new ReturnJsonModel() { result = true };
            } catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, sysSettings);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5")};
            }
        }

        public SystemSettings GetActiveSystemSettings()
        {
            var activeSystemSettings = _dbContext.LoyaltySystemSettings.FirstOrDefault(p => !p.IsArchived);
            return activeSystemSettings;
        }

        public DomainLoyaltySettings GetOrCreateDomainLoyaltySetting(int domainId, string userId)
        {
            var currentDomainLoyaltySetting = _dbContext.DomainLoyaltySettings.FirstOrDefault(p => p.Domain.Id == domainId);
            if (currentDomainLoyaltySetting != null)
                return currentDomainLoyaltySetting;

            var currentUser = _dbContext.QbicleUser.Find(userId);
            var domain = _dbContext.Domains.Find(domainId);
            var workGroupWithDebitProcess = _dbContext.WorkGroups.FirstOrDefault(p => p.Domain.Id == domainId && p.Processes.Any(e => e.Name == TraderProcessName.CreditNotes));
            var newSettingObj = new DomainLoyaltySettings()
            {
                Domain = domain,
                DebitProcessWorkGroup = workGroupWithDebitProcess,
                IsPaymentWithStoreCreditActive = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUser
            };

            _dbContext.DomainLoyaltySettings.Add(newSettingObj);
            _dbContext.Entry(newSettingObj).State = EntityState.Added;
            _dbContext.SaveChanges();

            return newSettingObj;
        }

        public ReturnJsonModel UpdateDomainLoyaltySettings(int workgroupId, bool isPaymentActive, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, workgroupId, domainId);

                var currentSetting = _dbContext.DomainLoyaltySettings.FirstOrDefault(p => p.Domain.Id == domainId);
                var debitProcessWorkGroup = _dbContext.WorkGroups.Find(workgroupId);
                currentSetting.DebitProcessWorkGroup = debitProcessWorkGroup;
                currentSetting.IsPaymentWithStoreCreditActive = isPaymentActive;
                _dbContext.Entry(currentSetting).State = EntityState.Modified;
                _dbContext.SaveChanges();
                return new ReturnJsonModel() { result = true, actionVal = 2 };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, workgroupId, domainId);
                return new ReturnJsonModel() { actionVal = 2, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }
    }
}
