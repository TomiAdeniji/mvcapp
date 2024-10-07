using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderSettingRules
    {
        private ApplicationDbContext dbContext;

        public TraderSettingRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ReturnJsonModel ChangeStatus(int idSetting, bool value, QbicleDomain domain)
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, idSetting, value, domain);
                if (idSetting == 0)
                {
                    var setting = new TraderSettings { Domain = domain, IsQbiclesBookkeepingEnabled = value };
                    dbContext.Entry(setting).State = EntityState.Added;

                    dbContext.TraderSettings.Add(setting);
                    dbContext.SaveChanges();
                    result.result = setting.IsQbiclesBookkeepingEnabled;
                    result.msgId = setting.Id.ToString();
                    result.Object = setting;
                }
                else
                {
                    var setting = dbContext.TraderSettings.Find(idSetting) ?? (new TraderSettings() { Domain = domain });
                    setting.IsQbiclesBookkeepingEnabled = value;
                    dbContext.SaveChanges();

                    result.result = setting.IsQbiclesBookkeepingEnabled;
                    result.msgId = setting.Id.ToString();
                    result.Object = setting;
                }
                result.actionVal = 2;
                if (value)
                    return result;
                var taxRate = new TaxRateRules(dbContext).GetByDomainId(domain.Id).Any();
                if (taxRate)
                    return result;

                var traderSeting = dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domain.Id);
                if (traderSeting == null) return result;
                traderSeting.IsSetupCompleted = TraderSetupCurrent.Accounting;
                dbContext.Entry(traderSeting).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, idSetting, value, domain);
                result.actionVal = 3;
                result.msg = ex.Message;
            }
            return result;
        }

        public List<ResourceCategory> GetResouceCategoriesByType(int domainId, ResourceCategoryType type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, type);
                return dbContext.ResourceCategorys.Where(q => q.Domain.Id == domainId && q.Type == type).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, type);
                return new List<ResourceCategory>();
            }
        }
        // Category
        public ReturnJsonModel SaveCategory(ResourceCategory category, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, result = true };
            try
            {
                var checkCategory = dbContext.ResourceCategorys.Where(q =>
                q.Domain.Id == category.Domain.Id && q.Id != category.Id && q.Name.ToLower() == category.Name.ToLower() && q.Type == category.Type);
                if (checkCategory.Any())
                {
                    return new ReturnJsonModel { actionVal = 3, result = false, msg = category.Name + ": Already exist!" };
                }

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, category);
                // add new
                if (category.Id == 0)
                {
                    category.CreatedDate = DateTime.UtcNow;
                    category.CreatedBy = dbContext.QbicleUser.Find(userId);
                    dbContext.Entry(category).State = EntityState.Added;
                    dbContext.ResourceCategorys.Add(category);
                    dbContext.SaveChanges();
                }
                else // edit
                {
                    var updateCategory = dbContext.ResourceCategorys.Find(category.Id);
                    updateCategory.Name = category.Name;
                    dbContext.Entry(updateCategory).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    result.actionVal = 2;
                }

                result.msgId = category.Id.ToString();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, category);
                result.actionVal = 3;
                result.result = false;
                result.msg = ex.Message;
            }

            return result;
        }

        public void DeleteReCategory(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var category = dbContext.ResourceCategorys.Find(id);
                dbContext.Entry(category).State = EntityState.Deleted;
                dbContext.ResourceCategorys.Remove(category);
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
            }
        }
        public ReturnJsonModel UpdateSetting(TraderSettings traderSetting)
        {
            var result = new ReturnJsonModel()
            {
                result = false,
                actionVal = 3,
                msg = ""

            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderSetting);
                if (traderSetting.Id == 0)
                {
                    dbContext.Entry(traderSetting).State = EntityState.Added;

                    dbContext.TraderSettings.Add(traderSetting);
                    dbContext.SaveChanges();
                    result.result = true;
                    result.msg = "";
                    result.actionVal = 1;
                }
                else
                {
                    var setting = dbContext.TraderSettings.Find(traderSetting.Id);
                    setting.Delimeter = traderSetting.Delimeter;

                    setting.SalePrefix = traderSetting.SalePrefix;
                    setting.SaleSuffix = traderSetting.SaleSuffix;

                    setting.SalesOrderPrefix = traderSetting.SalesOrderPrefix;
                    setting.SalesOrderSuffix = traderSetting.SalesOrderSuffix;


                    setting.SaleReturnPrefix = traderSetting.SaleReturnPrefix;
                    setting.SaleReturnSuffix = traderSetting.SaleReturnSuffix;

                    setting.SalesReturnOrderPrefix = traderSetting.SalesReturnOrderPrefix;
                    setting.SalesReturnOrderSuffix = traderSetting.SalesReturnOrderSuffix;

                    setting.PurchaseOrderPrefix = traderSetting.PurchaseOrderPrefix;
                    setting.PurchaseOrderSuffix = traderSetting.PurchaseOrderSuffix;

                    setting.PurchasePrefix = traderSetting.PurchasePrefix;
                    setting.PurchaseSuffix = traderSetting.PurchaseSuffix;

                    setting.TransferPrefix = traderSetting.TransferPrefix;
                    setting.TransferSuffix = traderSetting.TransferSuffix;

                    setting.ManuJobPrefix = traderSetting.ManuJobPrefix;
                    setting.ManuJobSuffix = traderSetting.ManuJobSuffix;

                    setting.InvoicePrefix = traderSetting.InvoicePrefix;
                    setting.InvoiceSuffix = traderSetting.InvoiceSuffix;

                    setting.BillPrefix = traderSetting.BillPrefix;
                    setting.BillSuffix = traderSetting.BillSuffix;

                    setting.AllocationPrefix = traderSetting.AllocationPrefix;
                    setting.AllocationSuffix = traderSetting.AllocationSuffix;

                    setting.CreditNotePrefix = traderSetting.CreditNotePrefix;
                    setting.CreditNoteSuffix = traderSetting.CreditNoteSuffix;

                    setting.DebitNotePrefix = traderSetting.DebitNotePrefix;
                    setting.DebitNoteSuffix = traderSetting.DebitNoteSuffix;

                    setting.ReorderPrefix = traderSetting.ReorderPrefix;
                    setting.ReorderSuffix = traderSetting.ReorderSuffix;

                    setting.OrderPrefix = traderSetting.OrderPrefix;
                    setting.OrderSuffix = traderSetting.OrderSuffix;

                    setting.PaymentPrefix = traderSetting.PaymentPrefix;
                    setting.PaymentSuffix = traderSetting.PaymentSuffix;
                    

                    setting.DeliveryPrefix = traderSetting.DeliveryPrefix;
                    setting.DeliverySuffix = traderSetting.DeliverySuffix;

                    setting.AlertGroupPrefix = traderSetting.AlertGroupPrefix;
                    setting.AlertGroupSuffix = traderSetting.AlertGroupSuffix;
                    setting.AlertReportPrefix = traderSetting.AlertReportPrefix;
                    setting.AlertReportSuffix = traderSetting.AlertReportSuffix;

                    dbContext.SaveChanges();

                    result.result = true;
                    result.msg = "";
                    result.msgId = setting.Delimeter;
                    result.actionVal = 2;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderSetting);
                result.actionVal = 3;
                result.msg = ex.Message;
            }
            return result;
        }
        public bool ChangeIsQbiclesBookkeepingEnabled(bool isCheck, int traderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, isCheck, traderId);
                var trader = dbContext.TraderSettings.Find(traderId);
                if (trader == null) return false;
                trader.IsQbiclesBookkeepingEnabled = isCheck;

                if (dbContext.Entry(trader).State == EntityState.Detached)
                    dbContext.TraderSettings.Attach(trader);
                dbContext.Entry(trader).State = EntityState.Modified;
                dbContext.SaveChanges();

                if (isCheck)
                    return true;
                var taxRate = new TaxRateRules(dbContext).GetByDomainId(trader.Domain.Id).Any();
                if (taxRate)
                    return true;

                var traderSeting = dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == trader.Domain.Id);
                if (traderSeting == null) return true;
                traderSeting.IsSetupCompleted = TraderSetupCurrent.Accounting;
                dbContext.Entry(traderSeting).State = EntityState.Modified;
                dbContext.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, isCheck, traderId);
                return false;
            }
        }

        public bool CheckTraderIsSetupComplete(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var traderSetting = dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (traderSetting != null) return traderSetting.IsSetupCompleted == TraderSetupCurrent.TraderApp;
                var setting = new TraderSettings
                {
                    Domain = dbContext.Domains.Find(domainId),
                    IsQbiclesBookkeepingEnabled = false,
                    IsSetupCompleted = TraderSetupCurrent.Location
                };
                dbContext.TraderSettings.Add(setting);
                dbContext.SaveChanges();
                return false;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return false;
            }

        }

        public TraderSettings GetTraderSettingByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new TraderSettings();
            }
        }

        public bool UpdateTraderIsSettingComplete(int domainId, TraderSetupCurrent currentStep)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, currentStep);
                var trader = dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (trader == null)
                    return false;
                trader.IsSetupCompleted = currentStep;

                if (dbContext.Entry(trader).State == EntityState.Detached)
                    dbContext.TraderSettings.Attach(trader);
                dbContext.Entry(trader).State = EntityState.Modified;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, currentStep);
                return false;
            }
        }


        public bool UpdateJournalGroupDefault(int journalGroupId, int traderSettingId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, journalGroupId, traderSettingId);
                var trader = dbContext.TraderSettings.Find(traderSettingId);
                if (trader == null) return false;
                if (journalGroupId == 0)
                {
                    trader.JournalGroupDefault = null;
                }
                else
                {
                    trader.JournalGroupDefault = dbContext.JournalGroups.Find(journalGroupId);
                }


                if (dbContext.Entry(trader).State == EntityState.Detached)
                    dbContext.TraderSettings.Attach(trader);
                dbContext.Entry(trader).State = EntityState.Modified;
                dbContext.SaveChanges();

                if (journalGroupId > 0)
                    return true;
                var taxRate = new TaxRateRules(dbContext).GetByDomainId(trader.Domain.Id).Any();
                if (taxRate)
                    return true;

                var traderSeting = dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == trader.Domain.Id);
                if (traderSeting == null) return true;
                traderSeting.IsSetupCompleted = TraderSetupCurrent.Accounting;
                dbContext.Entry(traderSeting).State = EntityState.Modified;
                dbContext.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, journalGroupId, traderSettingId);
                return false;
            }

        }
    }
}