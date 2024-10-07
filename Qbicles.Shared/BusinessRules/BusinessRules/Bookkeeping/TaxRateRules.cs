using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class TaxRateRules
    {
        ApplicationDbContext dbContext;

        public TaxRateRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        private ReturnJsonModel CheckExistName(TaxRate taxRate)
        {
            var refModel = new ReturnJsonModel();

            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Check Exist Taxrate Name", null, null, taxRate);
            try
            {
                var taxrates = dbContext.TaxRates.Where(q => q.Name == taxRate.Name && q.Domain.Id == taxRate.Domain.Id && q.IsStatic == false);
                if (taxRate.Id > 0 && taxrates.Any())
                    taxrates = taxrates.Where(q => q.Id != taxRate.Id);
                if (taxrates.Any())
                {
                    refModel.actionVal = 3;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_632", taxRate.Name);
                    refModel.msgName = taxRate.Name;
                    refModel.result = true;
                }

            }
            catch (Exception ex)
            {
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxRate);
            }
            return refModel;
        }

        public TaxRate GetById(int taxrateId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "TaxRate GetById", null, null, taxrateId);

                return dbContext.TaxRates.Find(taxrateId);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxrateId);
                return null;
            }
        }

        public IQueryable<TaxRate> GetByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get TaxRate By DomainId", null, null, domainId);

                return dbContext.TaxRates.Where(q => q.Domain.Id == domainId && q.IsStatic == false);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }

        public TaxRate GetItemTaxRateItemById(int taxrateId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "TaxRate GetItemTaxRateItemById", null, null, taxrateId);

                var taxrate = dbContext.TaxRates.Find(taxrateId);
                if (taxrate == null) return null;
                return new TaxRate()
                {
                    Id = taxrate.Id,
                    Name = taxrate.Name,
                    Rate = taxrate.Rate,
                    IsAccounted = taxrate.IsAccounted,
                    IsCreditToTaxAccount = taxrate.IsCreditToTaxAccount,
                    IsPurchaseTax = taxrate.IsPurchaseTax,
                    Description = taxrate.Description,
                    AssociatedAccount = taxrate.AssociatedAccount != null ? new BKAccount() { Id = taxrate.AssociatedAccount.Id, Name = taxrate.AssociatedAccount.Name } : null
                };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxrateId);
                return null;
            }

        }
        public TaxRate CloneStaticTaxRateById(int taxrateId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "TaxRate GetItemTaxRateItemById", null, null, taxrateId);

                var taxrate = dbContext.TaxRates.Find(taxrateId);
                if (taxrate == null) return null;
                var cloneTaxRate = new TaxRate
                {
                    Id = 0,
                    IsStatic = true,
                    Name = taxrate.Name,
                    Rate = taxrate.Rate,
                    Description = taxrate.Description,
                    IsAccounted = taxrate.IsAccounted,
                    Domain = taxrate.Domain,
                    AssociatedAccount = taxrate.AssociatedAccount,
                    IsCreditToTaxAccount = taxrate.IsCreditToTaxAccount,
                    IsPurchaseTax = taxrate.IsPurchaseTax
                };
                dbContext.TaxRates.Add(cloneTaxRate);
                dbContext.Entry(cloneTaxRate).State = EntityState.Added;
                return cloneTaxRate;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxrateId);
                return null;
            }

        }

        public ReturnJsonModel SaveTaxRate(TaxRate taxRate, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save TaxRate Debug", null, null, taxRate);


                taxRate.Domain = domain;

                if (taxRate != null && !string.IsNullOrEmpty(taxRate.Name))
                {
                    refModel = CheckExistName(taxRate);
                    if (refModel.actionVal == 3)
                        return refModel;

                    if (taxRate.AssociatedAccount != null && taxRate.AssociatedAccount.Id == 0)
                        taxRate.AssociatedAccount = null;
                    else if (taxRate.AssociatedAccount != null && taxRate.AssociatedAccount.Id > 0)
                    {
                        taxRate.AssociatedAccount = dbContext.BKAccounts.FirstOrDefault(q => q.Id == taxRate.AssociatedAccount.Id);
                    }
                    if (taxRate.Id > 0)
                    {
                        var taxRateUpdate = dbContext.TaxRates.FirstOrDefault(q => q.Id == taxRate.Id);

                        var isUpdateCatalogPricingBasedOnTaxEvents = taxRateUpdate.Rate != taxRate.Rate && !taxRate.IsPurchaseTax;

                        taxRateUpdate.AssociatedAccount = taxRate.AssociatedAccount;
                        taxRateUpdate.Name = taxRate.Name;
                        taxRateUpdate.Rate = taxRate.Rate;
                        taxRateUpdate.IsAccounted = taxRate.IsAccounted;
                        taxRateUpdate.IsPurchaseTax = taxRate.IsPurchaseTax;
                        taxRateUpdate.IsCreditToTaxAccount = taxRate.IsCreditToTaxAccount;
                        taxRateUpdate.Description = taxRate.Description;
                        taxRateUpdate.IsStatic = false;
                        if (dbContext.Entry(taxRateUpdate).State == EntityState.Detached)
                            dbContext.TaxRates.Attach(taxRateUpdate);
                        dbContext.Entry(taxRateUpdate).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        refModel.actionVal = 2;
                        //refModel.msg = _taxRate.Name;
                        refModel.msgId = taxRateUpdate.Id.ToString();
                        refModel.msgName = taxRateUpdate.Name;
                        if (isUpdateCatalogPricingBasedOnTaxEvents)
                            UpdateCatalogPricingBasedOnTaxEvents(taxRateUpdate);
                    }
                    else
                    {
                        taxRate.IsStatic = false;
                        dbContext.TaxRates.Add(taxRate);
                        dbContext.Entry(taxRate).State = EntityState.Added;
                        dbContext.SaveChanges();
                        refModel.actionVal = 1;
                        //append to select group
                        refModel.msgId = taxRate.Id.ToString();
                        refModel.msgName = taxRate.Name;
                    }



                    refModel.result = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxRate);
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
            }

            return refModel;
        }

        public bool DeleteTaxRate(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete TaxRate", null, null, id);
                var taxRate = dbContext.TaxRates.Find(id);
                var domainId = taxRate.Domain.Id;
                dbContext.TaxRates.Remove(taxRate);
                dbContext.SaveChanges();
                var taxRates = dbContext.TaxRates.Any(d => d.Domain.Id == domainId);
                var traderSeting = dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (traderSeting == null) return true;
                if (traderSeting.IsQbiclesBookkeepingEnabled || taxRates) return true;
                if (!traderSeting.IsQbiclesBookkeepingEnabled && !taxRates)
                {
                    traderSeting.IsSetupCompleted = TraderSetupCurrent.Accounting;
                    dbContext.Entry(traderSeting).State = EntityState.Modified;
                    dbContext.SaveChanges();

                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);

                return false;
            }
        }

        public List<TaxRateModel> GetTaxRateByDomainId(int domainId)
        {
            var taxRateModel = new List<TaxRateModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get TaxRate By DomainId", null, null, domainId);


                var taxRates = dbContext.TaxRates.Where(q => q.Domain.Id == domainId && q.IsStatic == false).OrderBy(n => n.Name).ToList();
                var traderItem = dbContext.TraderItems.Where(d => d.Domain.Id == domainId).OrderBy(n => n.Name);
                foreach (var item in taxRates)
                {
                    var canDel = "";
                    if (traderItem.Any(t => t.TaxRates.Any(s => s.Id == item.Id)))
                        canDel = "disabled";

                    taxRateModel.Add(new TaxRateModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        IsAccounted = item.IsAccounted,
                        IsCreditToTaxAccount = item.IsCreditToTaxAccount,
                        IsPurchaseTax = item.IsPurchaseTax,
                        Description = item.Description,
                        Rate = item.Rate,
                        CanDelete = canDel
                    });
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
            }

            return taxRateModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taxRateUpdate"></param>
        private void UpdateCatalogPricingBasedOnTaxEvents(TaxRate taxRateUpdate)
        {            
            taxRateUpdate.TraderItems.ForEach(traderItem =>
            {
                traderItem.UpdateCatalogPricingBasedOnTaxEvents();
            });
        }
    }
}
