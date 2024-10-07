using CleanBooksData;
using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Loyalty;
using Qbicles.Models.MyBankMate;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Spannered;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Budgets;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.TraderWorkgroup;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;

namespace Qbicles.Web.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("MySql.Data.MySqlClient", new SqlGenerator());
        }

        // data base
        private string[] transactionMatchingTypes =
        {
            TransactionMatchingTypesConst.NotApplicable,
            TransactionMatchingTypesConst.DebitToDebit,
            TransactionMatchingTypesConst.DebitToCredit
        };

        private string[] relationships = { "Not Set",
                                               "One to One",
                                               "One to Many",
                                               "Many to One",
                                               "Many to Many" };

        private string[] transactionMatchingMethods = new string[] {
                "Not Set",
                "Many To Many",
                "Reference And Date",
                "Reference To Reference1 And Date",
                "Reference To Description And Date",
                "Reference",
                "Reference To Reference1",
                "Reference To Description",
                "Description And Date",
                "Description",
                "Amount And Date",
                "Reversals",
                "Manual" };

        private int[] transactionmatchingamountvariancevalues = { 5, 10, 15, 20, 25 };
        private int[] transactionmatchingdatevariancevalues = { 1, 5, 7, 30, 90 };

        // Change to the constants in RightPermissions
        private string[] cbRights = {
            "View CleanBooks", "View CleanBooks Accounts", "Add/Edit Account Group",
            "Add Account", "Edit Account", "Delete Account","Upload Transactions",
            "Delete Uploaded Transactions","View CleanBooks Tasks","Add/Edit Task Group",
            "Add Task", "Edit Task", "Delete Task","Run Task", "App Config" };

        // Change to the constants in RightPermissions
        private string[] bkRights = new[] {
            "View Bookkeeping", "Manage App Settings" };

        // Change to the constants in RightPermissions
        private string[] communityRight = {
            "Add/Edit Domain Profile","Delete Domain Profile",
            "Add/Edit Community Page","Delete Community Page",
            "Add / Edit User Profile page"
        };

        // Change to the constants in RightPermissions
        private string[] traderRight = new[]
        {
            RightPermissions.TraderAccess,//"Access Trader",
            RightPermissions.TraderContactUpdate//"Update Trader Contact"
        };

        private string[] businessRight = new[]
        {
            RightPermissions.QbiclesBusinessAccess
        };

        private string[] traderProcesses = {
            TraderProcessesConst.Purchase,
            TraderProcessesConst.Sale,
            TraderProcessesConst.Transfer,
            TraderProcessesConst.Payment,
            TraderProcessesConst.Contact,
            TraderProcessesConst.Invoice,
            TraderProcessesConst.SpotCount,
            TraderProcessesConst.WasteReport,
            TraderProcessesConst.Manufacturing,
            TraderProcessesConst.StockAudits,
            TraderProcessesConst.PointOfSale,
            TraderProcessesConst.CreditNotes,
            TraderProcessesConst.Budget,
            TraderProcessesConst.ShiftAudit,
            TraderProcessesConst.SaleReturn,
            TraderProcessesConst.CashManagement,
            TraderProcessesConst.Reorder
        };

        private string[] cbProcesses = {
            CBProcessesConst.Account,
            CBProcessesConst.Task,
            CBProcessesConst.AccountData,
            CBProcessesConst.TaskExecution
        };

        private string[] smNetworkTypes = {
            SMNetworkTypesConst.FaceBook,
            SMNetworkTypesConst.Instagram,
            SMNetworkTypesConst.Twitter,
            SMNetworkTypesConst.LinkedIn,
            SMNetworkTypesConst.Pinterest,
            SMNetworkTypesConst.Youtube,
            SMNetworkTypesConst.WhatApps,
            SMNetworkTypesConst.Other
        };

        private string[] bookkeepingProcesses = {
            BookkeepingProcessesConst.ViewCharTofAccounts,
            BookkeepingProcessesConst.Accounts,
            BookkeepingProcessesConst.JournalEntry,
            BookkeepingProcessesConst.ViewJournalEntries,
            BookkeepingProcessesConst.Reports
        };

        private string[] smProcesses =
        {
            SMProcessesConst.SocialCampaigns,
            SMProcessesConst.AdCampaigns,
            SMProcessesConst.IdeasAndThemes,
            SMProcessesConst.EmailCampaigns
        };

        private string[] spanneredProcesses =
        {
            ProcessesConst.Assets,
            ProcessesConst.AssetTasks,
            ProcessesConst.Meters,
            ProcessesConst.ConsumptionReports,
            ProcessesConst.Purchases,
            ProcessesConst.Transfers
        };

        private string[] salesMarketingRight = new[]
        {
            RightPermissions.SalesAndMarketingAccess//"Access Sales and Marketing"
        };

        private string[] PaymentMethod = new[]
        {
            PaymentMethodNameConst.Cash,
            PaymentMethodNameConst.Card,
            PaymentMethodNameConst.ElectronicTransfer,
            PaymentMethodNameConst.CashOnDelivery,
            PaymentMethodNameConst.StoreCredit
        };

        private string[] spanneredRight = new[]
        {
            RightPermissions.SpanneredAccess//"Access Spannered"
        };

        private string[] operatorRight = new[]
        {
            RightPermissions.OperatorAccess//"Access Operator"
        };

        private string[] commerceRight = new[]
        {
            RightPermissions.CommerceAccess//"Access Commerce"
        };

        private string[] b2bProcesses =
        {
            B2bProcessesConst.ProfileEditing,
            B2bProcessesConst.Partnerships,
            B2bProcessesConst.Relationships
        };

        private string[] myBankMateRight = new[]
        {
            RightPermissions.MyBankMateAccess//"Access Bankmate"
        };

        /// <summary>
        /// Only run one time
        /// The SQL must
        //* for each OrderTax
        //* create a new TaxRate
        //* that is a copy of the existing OrderTax.TaxRate
        //* with IsStatic = true
        //and
        //* TraderItems = null
        /// </summary>
        private void FixOldDataOrderTaxOnlyRunOneTime(ApplicationDbContext context)
        {
            context.Configuration.AutoDetectChangesEnabled = false;
            context.Configuration.ValidateOnSaveEnabled = false;
            var orderTaxs = context.OrderTaxs.Where(s => s.TaxRate != null).ToList();
            var taxRule = new TaxRateRules(context);
            foreach (var item in orderTaxs)
            {
                item.StaticTaxRate = taxRule.CloneStaticTaxRateById(item.TaxRate.Id);
            }
            context.SaveChanges();
            context.Configuration.AutoDetectChangesEnabled = true;
            context.Configuration.ValidateOnSaveEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();
            //SeeUpdateVariantOptions(context);
            //SeedUpdateDeliveryDriverArchived(context);
            //SeedMoveDeliveryQueueCompletedToQueueArchive(context);
            //Seed_UpdateCatalogMeu(context);
            //Seed_DeliverySystemSetting(context);
            //Seed_UpdateShiftAuditApprovalDefinition(context);
            //FixOldDataOrderTaxOnlyRunOneTime(context);
            //Seed_SystemRolesQBicles(context);
            ////// Init system data
            //Seed_QbicleApplications(context);
            //Seed_QbicleFileTypes(context);
            ////Seed_AccountUpdateFrequencies(context);
            ////Seed_CleanBooksFileTypes(context);
            ////Seed_DateFormats(context);
            ////Seed_AlertCondition(context);
            ////Seed_BalanceAnalysisWarningLevel(context);
            ////Seed_ReasonSentEmail(context);
            ////Seed_TransactionMatchingMethod(context);
            ////Seed_TransactionMatchingRelationship(context);
            ////Seed_TransactionMatchingType(context);
            ////Seed_SubscriptionPackage(context);
            ////Seed_CBTasks(context);
            ////Seed_transactionmatchingamountvariancevalues(context);
            ////Seed_transactionmatchingdatevariancevalues(context);
            //Seed_TraderProcesses(context);
            ////Seed_SocialMediaNetworks(context);
            //Seed_SalesMktProcesses(context);
            //Seed_SpanneredProcesses(context);
            //Seed_PaymentMethod(context);
            //Seed_CBProcesses(context);
            ////DecryptFields(context);
            //Seed_PaymentTerms(context);
            //Seed_CreateView(context);
            //Seed_BookkeepingProcesses(context);
            //Seed_CreateTriggerJournalEntry(context);
            ////sp_GetCbAccountById(context);
            //Seed_CreateSystemDomain(context);
            //Seed_ListingProperty(context);
            //Seed_ListingPropertyType(context);
            //Seed_BusinesseAccessApplications(context);
            //Seed_SubscriptionApplicationIsCore(context);
            //Seed_QbiclesBusinesseAccessRole(context);

            //Seed_DropTriggerAlertGroup(context);
            //Seed_CreateTriggerSetNumberForReference(context);
            //Seed_CreateTriggerContactRef(context);
            //Seed_UpdateDataVariantOption(context);
            //Seed_Banks(context);
            //Seed_InitialSetSystemSettings(context);
            //Seed_UpdatePrice(context);
            //Seed_CatalogPrice(context);

            //SeedUpdateUserProfileWizardRunMicro(context);
            //SeedUpdateBusinessProfileWizard(context);

            //SeedUpdateTopicInPost(context);

            //UpdateCatalogIsPublished(context);

            //ClearAreasOperationEmpty(context);
            //Seed_CloneStaticTaxRate(context);
            //Seed_CreateDomainLevel(context);
            //Seed_CreateExistingDomainPlan(context);
            //Seed_CreateB2COrderPaymentCharge(context);
            //UpdateTraderItemDescription(context);
            //UpdateWaitlist(context);
        }

        private void Seed_CreateB2COrderPaymentCharge(ApplicationDbContext context)
        {
            try
            {
                var recordNumber = context.B2COrderPaymentCharges.Count();
                if (recordNumber == 0)
                {
                    var b2cOrderPaymentChargeObj = new B2COrderPaymentCharges()
                    {
                        QbiclesFlatFee = 10000,
                        QbiclesPercentageCharge = (decimal)0.2
                    };
                    context.B2COrderPaymentCharges.Add(b2cOrderPaymentChargeObj);
                    context.Entry(b2cOrderPaymentChargeObj).State = EntityState.Added;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Seed_CreateDomainLevel(ApplicationDbContext context)
        {
            try
            {
                // Create and init Transaction - To get authorization
                // Only create transaction with a minium amount of money
                // NGN 50.00,
                // GHS 0.10,
                // ZAR 1.00,
                // USD 0.20

                string[] currencyArray = { "NGN", "GHS", "ZAR", "USD" };
                var lstCurrency = currencyArray.ToList();

                // Free
                var freeDomainRequestLevelInDB = context.BusinessDomainLevels.FirstOrDefault(p => p.Level == BusinessDomainLevelEnum.Free);
                var freeDomainRequestLevel = new BusinessDomainLevel()
                {
                    Id = freeDomainRequestLevelInDB?.Id ?? 0,
                    Cost = 0,
                    CostPerAdditionalUser = null,
                    Level = BusinessDomainLevelEnum.Free,
                    Name = "Free",
                    NumberOfUsers = 1,
                    Currency = null,
                    Description = "Everything you need to establish an online presence for your business. Create a business profile, " +
                        "landing pages, posts, and product catalogs. Start connecting with other businesses and customers.",
                    IsVisible = true,
                    VerifyAmount = 0,
                    NumberOfFreeTrialDays = 0
                };
                if (freeDomainRequestLevelInDB == null)
                {
                    context.BusinessDomainLevels.Add(freeDomainRequestLevel);
                    context.Entry(freeDomainRequestLevel).State = EntityState.Added;
                }
                else
                {
                    context.Entry(freeDomainRequestLevelInDB).CurrentValues.SetValues(freeDomainRequestLevel);
                }

                // Business Starter
                var starterLevelInDB = context.BusinessDomainLevels.FirstOrDefault(p => p.Level == BusinessDomainLevelEnum.Starter);
                var starterLevel = new BusinessDomainLevel()
                {
                    Id = starterLevelInDB?.Id ?? 0,
                    Name = "Business Starter",
                    Description = "Everything you need to organize your business. Start selling, and building powerful relationships with customers. Create B2C catalogs, chat and sell to customers, and manage loyalty and promotions. Powerful sales reports to let you know how your business is doing.",
                    Cost = 10000,
                    CostPerAdditionalUser = 5000,
                    NumberOfUsers = 3,
                    Level = BusinessDomainLevelEnum.Starter,
                    IsVisible = true,
                    Currency = "NGN",
                    VerifyAmount = 50,
                    NumberOfFreeTrialDays = 15
                };
                if (starterLevelInDB == null)
                {
                    context.BusinessDomainLevels.Add(starterLevel);
                    context.Entry(starterLevel).State = EntityState.Added;
                }
                else
                {
                    context.Entry(starterLevelInDB).CurrentValues.SetValues(starterLevelInDB);
                }

                // Business Standard
                var standardLevelInDB = context.BusinessDomainLevels.FirstOrDefault(p => p.Level == BusinessDomainLevelEnum.Standard);
                var standardLevel = new BusinessDomainLevel()
                {
                    Id = standardLevelInDB?.Id ?? 0,
                    Name = "Business Standard",
                    Description = "Level up your business with professional reporting of inventory and more staff users. Start trading both B2B and B2C to keep inventory, orders and finances in-sync. Advanced order and delivery management to keep you and your customers happy. Add our BookKeeping Bolt-on to get extended financial information, such as income statements and balance sheet.",
                    Cost = 20000,
                    CostPerAdditionalUser = 5000,
                    NumberOfUsers = 5,
                    Level = BusinessDomainLevelEnum.Standard,
                    IsVisible = false,
                    Currency = "NGN",
                    VerifyAmount = 50,
                    NumberOfFreeTrialDays = 15
                };
                if (standardLevelInDB == null)
                {
                    context.BusinessDomainLevels.Add(standardLevel);
                    context.Entry(standardLevel).State = EntityState.Added;
                }
                else
                {
                    context.Entry(standardLevelInDB).CurrentValues.SetValues(standardLevel);
                }

                // Business Expert
                var expertLevelInDB = context.BusinessDomainLevels.FirstOrDefault(p => p.Level == BusinessDomainLevelEnum.Expert);
                var expertLevel = new BusinessDomainLevel()
                {
                    Id = expertLevelInDB?.Id ?? 0,
                    Name = "Business Expert",
                    Description = "Take your multi-location business even further with a free point-of-sale. With advanced features for more complex businesses, such as tracking ingredients and recipes (manufacturing). This plan gives you the option for up to 3 inventory locations.",
                    Cost = 40000,
                    CostPerAdditionalUser = 5000,
                    NumberOfUsers = 5,
                    Level = BusinessDomainLevelEnum.Expert,
                    IsVisible = false,
                    Currency = "NGN",
                    VerifyAmount = 50,
                    NumberOfFreeTrialDays = 15
                };
                if (expertLevelInDB == null)
                {
                    context.BusinessDomainLevels.Add(expertLevel);
                    context.Entry(expertLevel).State = EntityState.Added;
                }
                else
                {
                    context.Entry(expertLevelInDB).CurrentValues.SetValues(expertLevel);
                }

                // Existing
                var existingLevelInDB = context.BusinessDomainLevels.FirstOrDefault(p => p.Level == BusinessDomainLevelEnum.Existing);
                var existingLevel = new BusinessDomainLevel()
                {
                    Id = existingLevelInDB?.Id ?? 0,
                    Name = "Existing",
                    Description = "",
                    Cost = null,
                    CostPerAdditionalUser = null,
                    NumberOfUsers = 1000,
                    Level = BusinessDomainLevelEnum.Existing,
                    IsVisible = false,
                    Currency = "NGN",
                    VerifyAmount = null,
                    NumberOfFreeTrialDays = null
                };
                if (existingLevelInDB == null)
                {
                    context.BusinessDomainLevels.Add(existingLevel);
                    context.Entry(existingLevel).State = EntityState.Added;
                }
                else
                {
                    context.Entry(existingLevelInDB).CurrentValues.SetValues(existingLevel);
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Seed_CreateExistingDomainPlan(ApplicationDbContext context)
        {
            try
            {
                var lstPlanAssociatedDomainId = context.DomainPlans.Where(p => p.Domain != null).Select(p => p.Domain.Id).ToList();
                var domainWithoutPlans = context.Domains.Where(p => !lstPlanAssociatedDomainId.Any(x => x == p.Id)).ToList();
                var businessDomainLevelTypeExisting = context.BusinessDomainLevels.FirstOrDefault(p => p.Level == BusinessDomainLevelEnum.Existing);
                foreach (var domainItem in domainWithoutPlans)
                {
                    var domainPlan = new DomainPlan()
                    {
                        Domain = domainItem,
                        CalculatedCost = 0,
                        ActualCost = 0,
                        PayStackPlanName = "",
                        InitTransactionResponseJSON = "",
                        PayStackPlanCode = "",
                        PayStackPlanCreationResponse = "",
                        ArchivedBy = null,
                        ArchivedDate = null,
                        IsArchived = false,
                        NumberOfExtraUsers = 0,
                        Level = businessDomainLevelTypeExisting
                    };
                    context.DomainPlans.Add(domainPlan);
                    context.Entry(domainPlan).State = EntityState.Added;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                throw ex;
            }
        }

        private void Seed_UpdatePrice(ApplicationDbContext context)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var price_query = context.TraderPrices.Where(p => p.Item.TaxRates.Any()).ToList();
                //Calculate taxes
                foreach (var price in price_query)
                {
                    var lstTaxes = price.Item.TaxRates.Where(s => !s.IsPurchaseTax).ToList();

                    foreach (var taxitem in lstTaxes)
                    {
                        var priceTaxItem = price.Taxes.FirstOrDefault(p => p.TaxName == taxitem.Name);
                        if (priceTaxItem == null || priceTaxItem.Id == 0)
                        {
                            var staticTaxItem = new TaxRateRules(context).CloneStaticTaxRateById(taxitem.Id);
                            priceTaxItem = new PriceTax
                            {
                                TaxName = taxitem.Name,
                                Rate = taxitem.Rate,
                                TaxRate = staticTaxItem,
                                Amount = price.NetPrice * (taxitem.Rate / 100)
                            };
                            context.Entry(priceTaxItem).State = EntityState.Added;
                            context.TraderPriceTaxes.Add(priceTaxItem);
                            price.Taxes.Add(priceTaxItem);
                        }
                        else
                        {
                            priceTaxItem.Amount = price.NetPrice * (taxitem.Rate / 100);
                            context.Entry(priceTaxItem).State = EntityState.Modified;
                        }
                        context.SaveChanges();
                    }
                    price.Taxes.RemoveAll(p => !lstTaxes.Any(x => x.Name == p.TaxName));
                    price.TotalTaxAmount = price.Taxes.Sum(p => p.Amount);
                    price.GrossPrice = price.NetPrice + price.TotalTaxAmount;
                }
                context.TraderPrices.Where(p => !p.Taxes.Any()).ForEach(item =>
                {
                    item.GrossPrice = item.NetPrice;
                });
                context.SaveChanges();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("elapsedMs: " + elapsedMs);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void Seed_CloneStaticTaxRate(ApplicationDbContext context)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var price_query = context.TraderPrices.Where(p => p.Item.TaxRates.Any()).ToList();
                // Add Static Taxes for trader prices
                foreach (var price in price_query)
                {
                    var lstTaxes = price.Item.TaxRates.Where(s => !s.IsPurchaseTax);

                    foreach (var taxitem in lstTaxes)
                    {
                        var priceTaxItem = price.Taxes.FirstOrDefault(p => p.TaxName == taxitem.Name);
                        if (priceTaxItem != null && priceTaxItem.Id > 0)
                        {
                            var staticTaxItem = new TaxRateRules(context).CloneStaticTaxRateById(taxitem.Id);
                            priceTaxItem.TaxRate = staticTaxItem;

                            context.Entry(priceTaxItem).State = EntityState.Modified;
                        }
                        context.SaveChanges();
                    }
                }
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Cloning TaxRate for Trader Price: " + elapsedMs);

                // Add Static Taxes for variant
                var variantWatch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var variantItem in context.PosVariants
                    .Where(vItem => vItem.BaseUnitPrice != null && vItem.BaseUnitPrice.Taxes.Any()).ToList())
                {
                    var price = variantItem.BaseUnitPrice;
                    price.Taxes.ForEach(txItem =>
                    {
                        var variantTaxItem = variantItem.Price.Taxes.FirstOrDefault(vtax => vtax.TaxName == txItem.TaxName);
                        if (variantTaxItem != null)
                        {
                            variantTaxItem.TaxRate = txItem.TaxRate;
                            context.Entry(variantTaxItem).State = EntityState.Modified;
                        }
                    });
                }
                context.SaveChanges();
                variantWatch.Stop();
                var variantelapsedMs = variantWatch.ElapsedMilliseconds;
                Console.WriteLine("Cloning TaxRate for Variant Items: " + variantelapsedMs);

                // Add Static taxes for extras
                var extraWatch = System.Diagnostics.Stopwatch.StartNew();

                foreach (var extraItem in context.PosExtras
                    .Where(exItem => exItem.BaseUnitPrice != null && exItem.BaseUnitPrice.Taxes.Any()).ToList())
                {
                    var price = extraItem.BaseUnitPrice;

                    price.Taxes.ForEach(txItem =>
                    {
                        var extraTaxItem = extraItem.Price.Taxes.FirstOrDefault(extra_tax => extra_tax.TaxName == txItem.TaxName);
                        if (extraTaxItem != null)
                        {
                            extraTaxItem.TaxRate = txItem.TaxRate;
                            context.Entry(extraTaxItem).State = EntityState.Modified;
                        }
                    });
                }
                context.SaveChanges();
                extraWatch.Stop();
                var extraElapseMs = extraWatch.ElapsedMilliseconds;
                Console.WriteLine("Cloneing TaxRate for Extra Items: " + extraElapseMs);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void Seed_CreateTriggerContactRef(ApplicationDbContext context)
        {
            string script = @"
            DROP TRIGGER IF EXISTS `trigger_setNumberForContactRef`;
            CREATE TRIGGER trigger_setNumberForContactRef
                BEFORE INSERT
                ON trad_contactref FOR EACH ROW
            BEGIN
	            DECLARE maxNumber INT(11);
                DECLARE prefixdomain nvarchar(50);
		        SELECT SUBSTRING(Name, 1, 3) INTO prefixdomain from qb_qbicledomain where Id=NEW.Domain_Id;
		        SELECT max(ReferenceNumber) INTO maxNumber from trad_contactref where Domain_Id=NEW.Domain_Id;
		        IF maxNumber > 0 THEN
			        SET NEW.ReferenceNumber=maxNumber+1;
                    SET NEW.Reference=CONCAT(UPPER(prefixdomain),'-',LPAD(maxNumber+1, 7, '0'));
		        ELSE
			        SET NEW.ReferenceNumber=1;
                    SET NEW.Reference=CONCAT(UPPER(prefixdomain),'-',LPAD(1, 7, '0'));
		        END IF;
            END";
            context.Database.ExecuteSqlCommand(script);
        }

        private void Seed_CreateTriggerJournalEntry(ApplicationDbContext context)
        {
            string script = @"
            DROP TRIGGER IF EXISTS `trigger_setNumberForJournalEntry`;
            CREATE TRIGGER trigger_setNumberForJournalEntry
                BEFORE INSERT
                ON bk_journalentry FOR EACH ROW
            BEGIN
	            DECLARE maxNumber INT(11);
	            SELECT max(Number) INTO maxNumber from bk_journalentry where Domain_Id=NEW.Domain_Id;
	            IF maxNumber > 0 THEN
                    SET NEW.Number=maxNumber+1;
                ELSE
                    SET NEW.Number=1;
                END IF;
            END";
            context.Database.ExecuteSqlCommand(script);
        }

        private void Seed_DropTriggerAlertGroup(ApplicationDbContext context)
        {
            string script = "DROP TRIGGER IF EXISTS `trigger_setNumberForMovementAlert`;";
            context.Database.ExecuteSqlCommand(script);
        }

        private void Seed_CreateTriggerSetNumberForReference(ApplicationDbContext context)
        {
            string script = @"
            DROP TRIGGER IF EXISTS `trigger_setNumberForReference`;
            CREATE TRIGGER trigger_setNumberForReference
                BEFORE INSERT
                    ON trad_reference FOR EACH ROW
                BEGIN

                  DECLARE maxNumber INT(11);
                  SELECT max(NumericPart) INTO maxNumber from trad_reference where Domain_Id=NEW.Domain_Id and Type = NEW.Type;

                  IF maxNumber > 0 THEN
                    SET NEW.NumericPart=maxNumber+1;
                  ELSE
                    SET NEW.NumericPart=1;
                  END IF;

                  SET @Prefix = TRIM(NEW.Prefix);
                  SET @Delimeter = NEW.Delimeter;
                  SET @Suffix = TRIM(NEW.Suffix);
                  SET @Number = CONVERT(NEW.NumericPart, CHAR);

                  SET @FullRef = '';

                  IF @Prefix is NULL OR @Prefix ='' then
                    SET @FullRef = @Number;
                  ELSE
                    SET @FullRef = Concat(@Prefix, @Delimeter, @Number);
                  END IF;

                  IF @Suffix is NULL OR @Suffix='' then
                    SET NEW.FullRef = @FullRef;
                  ELSE
                    SET NEW.FullRef = Concat(@FullRef, @Delimeter, @Suffix);
                  END IF;

                END";
            context.Database.ExecuteSqlCommand(script);
        }

        private void Seed_CreateView(ApplicationDbContext context)
        {
            string script =
           @"DROP TABLE IF EXISTS `[view_lstdates_qbicle_stream]`;
            DROP VIEW IF EXISTS `dbo.view_lstdates_qbicle_stream`;
            CREATE VIEW `dbo.view_lstdates_qbicle_stream` AS
                (SELECT DISTINCT
		                CAST(`po`.`TimeLineDate` AS DATE) AS `TimeLineDate`,
		                `tp`.`Qbicle_Id` AS `QbicleId`,
		                99 AS `ActivityType`,
		                `po`.`Topic_Id` AS `TopicId`,
		                'Qbicles' AS `APP`
	                FROM
		                (`qb_qbiclepost` `po`
		                JOIN `qb_topic` `tp` ON ((`tp`.`Id` = `po`.`Topic_Id`)))

                        WHERE ISNULL(`po`.`QbicleActivity_Id`)
                )
                UNION ALL
                SELECT DISTINCT
	                CAST(`qb_qbicleactivities`.`TimeLineDate` AS DATE) AS `TimeLineDate`,
	                `qb_qbicleactivities`.`Qbicle_Id` AS `QbicleId`,
	                `qb_qbicleactivities`.`ActivityType` AS `ActivityType`,
	                `qb_qbicleactivities`.`Topic_Id` AS `TopicId`,
	                 (CASE
                            WHEN `qb_qbicleactivities`.`App`=1 THEN 'Trader'
			                WHEN `qb_qbicleactivities`.`App`=2 THEN 'Bookkeeping'
			                WHEN `qb_qbicleactivities`.`App`=3 THEN 'SalesAndMarketing'
			                WHEN `qb_qbicleactivities`.`App`=4 THEN 'CleanBooks'
			                WHEN `qb_qbicleactivities`.`App`=5 THEN 'Spannered'
			                WHEN `qb_qbicleactivities`.`App`=6 THEN 'Operator'
                            WHEN `qb_qbicleactivities`.`App`=7 THEN 'Commerce'
			                ELSE 'Qbicles'
		                END
		                ) AS `APP`
                FROM `qb_qbicleactivities`
                WHERE (`qb_qbicleactivities`.`IsVisibleInQbicleDashboard` = TRUE)";
            context.Database.ExecuteSqlCommand(script);

            //view_unusedinventory
            script =
           @"DROP TABLE IF EXISTS `[view_unusedinventory]`;
            DROP VIEW IF EXISTS `dbo.view_unusedinventory`;
            CREATE VIEW `dbo.view_unusedinventory` AS
                SELECT
                    `trad_inventorydetail`.`Id` AS `Id`,
                    SUM(`trad_inventorybatch`.`UnusedQuantity`) AS `CurrentInventory`
                FROM
                    (`trad_inventorydetail`
                    JOIN `trad_inventorybatch` ON ((`trad_inventorybatch`.`InventoryDetail_Id` = `trad_inventorydetail`.`Id`)))
                WHERE
                    ((`trad_inventorybatch`.`Direction` = 0)
                        AND (`trad_inventorybatch`.`UnusedQuantity` > 0))
                GROUP BY `trad_inventorydetail`.`Id`";
            context.Database.ExecuteSqlCommand(script);

            //view_unusedbatches
            script =
           @"DROP TABLE IF EXISTS `[view_unusedbatches]`;
            DROP VIEW IF EXISTS `dbo.view_unusedbatches`;
            CREATE VIEW `dbo.view_unusedbatches` AS
                SELECT
                    `trad_inventorybatch`.`InventoryDetail_Id` AS `InventoryId`,
                    `trad_inventorybatch`.`Id` AS `BatchId`
                FROM
                    `trad_inventorybatch`
                WHERE
                    ((`trad_inventorybatch`.`Direction` = 0)
                        AND (`trad_inventorybatch`.`UnusedQuantity` > 0))
                ORDER BY `trad_inventorybatch`.`Id`";
            context.Database.ExecuteSqlCommand(script);

            //view for micro stream
            script =
           @"DROP TABLE IF EXISTS `[view_lstactivities_qbicle_stream]`;
            DROP VIEW IF EXISTS `dbo.view_lstactivities_qbicle_stream`;
            CREATE VIEW `dbo.view_lstactivities_qbicle_stream` AS
                SELECT
                    `v`.`Id` AS `Id`,
                    `v`.`TimeLineDate` AS `TimeLineDate`,
                    `v`.`QbicleId` AS `QbicleId`,
                    `v`.`ActivityType` AS `ActivityType`,
                    `v`.`TopicId` AS `TopicId`,
                    `v`.`APP` AS `APP`
                FROM
                    (SELECT
                        `po`.`Id` AS `Id`,
                            CAST(`po`.`TimeLineDate` AS DATE) AS `TimeLineDate`,
                            `tp`.`Qbicle_Id` AS `QbicleId`,
                            99 AS `ActivityType`,
                            `po`.`Topic_Id` AS `TopicId`,
                            'Qbicles' AS `APP`
                    FROM
                        (`qb_qbiclepost` `po`
                    JOIN `qb_topic` `tp` ON ((`tp`.`Id` = `po`.`Topic_Id`)))
                        WHERE ISNULL(`po`.`QbicleActivity_Id`)
                    UNION ALL SELECT
                        `qb`.`Id` AS `Id`,
                            CAST(`qb`.`TimeLineDate` AS DATE) AS `TimeLineDate`,
                            `qb`.`Qbicle_Id` AS `QbicleId`,
                            `qb`.`ActivityType` AS `ActivityType`,
                            `qb`.`Topic_Id` AS `TopicId`,
                            (CASE
                                WHEN (`qb`.`App` = 1) THEN 'Trader'
                                WHEN (`qb`.`App` = 2) THEN 'Bookkeeping'
                                WHEN (`qb`.`App` = 3) THEN 'SalesAndMarketing'
                                WHEN (`qb`.`App` = 4) THEN 'CleanBooks'
                                WHEN (`qb`.`App` = 5) THEN 'Spannered'
                                WHEN (`qb`.`App` = 6) THEN 'Operator'
                                WHEN (`qb`.`App` = 7) THEN 'Commerce'
                                ELSE 'Qbicles'
                            END) AS `APP`
                    FROM
                        `qb_qbicleactivities` `qb`
                    WHERE
                        (`qb`.`IsVisibleInQbicleDashboard` = TRUE)) `v`
                ORDER BY `v`.`TimeLineDate` DESC";
            context.Database.ExecuteSqlCommand(script);
        }

        public void Seed_CatalogPrice(ApplicationDbContext context)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var variantItem in context.PosVariants
                    .Where(vItem => vItem.BaseUnitPrice != null && vItem.BaseUnitPrice.Taxes.Any()).ToList())
                {
                    var price = variantItem.BaseUnitPrice;
                    price.Taxes.ForEach(txItem =>
                    {
                        var newTaxItem = new PriceTax()
                        {
                            Rate = txItem.Rate,
                            Amount = txItem.Amount,
                            TaxName = txItem.TaxName,
                            TaxRate = txItem.TaxRate
                        };
                        context.TraderPriceTaxes.Add(newTaxItem);
                        context.Entry(newTaxItem).State = EntityState.Added;
                        variantItem.Price.Taxes.Add(newTaxItem);
                    });
                    context.Entry(variantItem.Price).State = EntityState.Modified;
                }

                foreach (var extraItem in context.PosExtras
                    .Where(exItem => exItem.BaseUnitPrice != null && exItem.BaseUnitPrice.Taxes.Any()).ToList())
                {
                    var price = extraItem.BaseUnitPrice;

                    price.Taxes.ForEach(txItem =>
                    {
                        var newTaxItem = new PriceTax()
                        {
                            Rate = txItem.Rate,
                            Amount = txItem.Amount,
                            TaxName = txItem.TaxName,
                            TaxRate = txItem.TaxRate
                        };
                        context.TraderPriceTaxes.Add(newTaxItem);
                        context.Entry(newTaxItem).State = EntityState.Added;
                        extraItem.Price.Taxes.Add(newTaxItem);
                    });

                    context.Entry(extraItem.Price).State = EntityState.Modified;
                }
                context.SaveChanges();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("elapsedMs: " + elapsedMs);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Seed_PaymentTerms(ApplicationDbContext context)
        {
            var terms = context.PaymentTerms;

            if (!terms.Any(o => o.Name == "30 Days"))
            {
                context.PaymentTerms.Add(new PaymentTerms()
                {
                    Name = "30 Days",
                    NumberOfDays = 30
                });
            }

            if (!terms.Any(o => o.Name == "60 Days"))
            {
                context.PaymentTerms.Add(new PaymentTerms()
                {
                    Name = "60 Days",
                    NumberOfDays = 60
                });
            }

            if (!terms.Any(o => o.Name == "90 Days"))
            {
                context.PaymentTerms.Add(new PaymentTerms()
                {
                    Name = "90 Days",
                    NumberOfDays = 90
                });
            }

            context.SaveChanges();
        }

        private void Seed_PaymentMethod(ApplicationDbContext context)
        {
            var methods = context.PaymentMethods;
            foreach (var method in PaymentMethod)
            {
                if (methods.Any(o => o.Name == method)) continue;
                var r = new PaymentMethod
                {
                    Name = method
                };
                context.PaymentMethods.Add(r);
            }
            context.SaveChanges();
        }

        private void DecryptFields(ApplicationDbContext context)
        {
            // Decrypt the Qbicle properties
            // This only works if the [Encrypted] attribute is removed from the Name and Description properties
            var theQbicles = (from o in context.Qbicles select o);
            if (theQbicles.Count() > 0)
            {
                foreach (Qbicle cube in theQbicles)
                {
                    cube.Name = cube.Name.Decrypt();
                    cube.Description = cube.Description.Decrypt();
                }
            }
            context.SaveChanges();

            // Decrypt the QbicleActivity property
            // This only works if the [Encrypted] attribute is removed from the Name property
            var theActivities = (from o in context.Activities select o);
            if (theActivities.Count() > 0)
            {
                foreach (QbicleActivity a in theActivities)
                {
                    a.Name = a.Name.Decrypt();
                }
            }
            context.SaveChanges();

            // Decrypt the QbicleAlert property
            // This only works if the [Encrypted] attribute is removed from the Content property
            var theAlerts = (from o in context.Alerts select o);
            if (theAlerts.Count() > 0)
            {
                foreach (QbicleAlert a in theAlerts)
                {
                    a.Content = a.Content.Decrypt();
                }
            }
            context.SaveChanges();

            // Decrypt the QbicleEvent property
            // This only works if the [Encrypted] attribute is removed from the Description property
            var theEvents = (from o in context.Events select o);
            if (theEvents.Count() > 0)
            {
                foreach (QbicleEvent e in theEvents)
                {
                    e.Description = e.Description.Decrypt();
                }
            }
            context.SaveChanges();

            // Decrypt the QbicleMedia property
            // This only works if the [Encrypted] attribute is removed from the Description property
            var theMedia = (from o in context.Medias select o);
            if (theMedia.Count() > 0)
            {
                foreach (QbicleMedia m in theMedia)
                {
                    m.Description = m.Description.Decrypt();
                }
            }
            context.SaveChanges();

            // Decrypt the QbiclePost property
            // This only works if the [Encrypted] attribute is removed from the Message property
            var thePosts = (from o in context.Posts select o);
            if (thePosts.Count() > 0)
            {
                foreach (QbiclePost p in thePosts)
                {
                    p.Message = p.Message.Decrypt();
                }
            }
            context.SaveChanges();

            // Decrypt the QbicleTask property
            // This only works if the [Encrypted] attribute is removed from the Description property
            var theTasks = (from o in context.QbicleTasks select o);
            if (theTasks.Count() > 0)
            {
                foreach (QbicleTask t in theTasks)
                {
                    t.Description = t.Description.Decrypt();
                }
            }

            context.SaveChanges();
        }

        private void Seed_SalesMktProcesses(ApplicationDbContext context)
        {
            var processes = context.SalesMarketingProcesses;
            foreach (var process in smProcesses)
            {
                if (processes.Any(o => o.Name == process)) continue;
                var r = new SalesMarketingProcess()
                {
                    Name = process
                };
                context.SalesMarketingProcesses.Add(r);
            }
            context.SaveChanges();
        }

        private void Seed_CBProcesses(ApplicationDbContext context)
        {
            var processes = context.CBProcesses;
            foreach (var process in cbProcesses)
            {
                if (processes.Any(o => o.Name == process)) continue;
                var r = new CBProcess()
                {
                    Name = process
                };
                context.CBProcesses.Add(r);
            }
            context.SaveChanges();
        }

        private void Seed_SpanneredProcesses(ApplicationDbContext context)
        {
            var processes = context.SpanneredProcesses;
            foreach (var process in spanneredProcesses)
            {
                if (processes.Any(o => o.Name == process)) continue;
                var r = new SpanneredProcess()
                {
                    Name = process
                };
                context.SpanneredProcesses.Add(r);
            }
            context.SaveChanges();
        }

        private void Seed_SocialMediaNetworks(ApplicationDbContext context)
        {
            var networkTypes = context.NetworkTypes;

            foreach (var networkType in smNetworkTypes)
            {
                NetworkType net = networkTypes.FirstOrDefault(o => o.Name.Equals(networkType));
                if (net != null)
                {
                    net.AllowedCampaignType = CampaignType.Both;
                    context.Entry(net).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    var r = new NetworkType()
                    {
                        Name = networkType,
                        AllowedCampaignType = networkType.Equals("FaceBook") || networkType.Equals("Twitter") ? CampaignType.Both : CampaignType.Manual
                    };
                    context.NetworkTypes.Add(r);
                    context.Entry(r).State = EntityState.Added;
                    context.SaveChanges();
                }
            }
        }

        private void Seed_TraderProcesses(ApplicationDbContext context)
        {
            var processes = context.TraderProcesses;
            foreach (var process in traderProcesses)
            {
                if (processes.Any(o => o.Name == process)) continue;
                var r = new TraderProcess()
                {
                    Name = process
                };
                context.TraderProcesses.Add(r);
            }

            context.SaveChanges();
        }

        private void Seed_BookkeepingProcesses(ApplicationDbContext context)
        {
            var processes = context.BookkeepingProcesses;
            foreach (var process in bookkeepingProcesses)
            {
                if (processes.Any(o => o.Name == process)) continue;
                var r = new BookkeepingProcess()
                {
                    Name = process
                };
                context.BookkeepingProcesses.Add(r);
            }

            context.SaveChanges();
        }

        private void Seed_CBTasks(ApplicationDbContext context)
        {
            var data = context.tasktypes;
            if (!data.Any(o => o.Name == "Transaction Matching"))
            {
                context.tasktypes.Add(new tasktype()
                {
                    Name = "Transaction Matching"
                });
            }
            if (!data.Any(o => o.Name == "Transaction Analysis"))
            {
                context.tasktypes.Add(new tasktype()
                {
                    Name = "Transaction Analysis"
                });
            }
            if (!data.Any(o => o.Name == "Balance Analysis"))
            {
                context.tasktypes.Add(new tasktype()
                {
                    Name = "Balance Analysis"
                });
            }

            var data1 = context.taskexecutionintervals;
            if (!data1.Any(o => o.Interval == "Daily"))
            {
                context.taskexecutionintervals.Add(new taskexecutioninterval()
                {
                    Interval = "Daily"
                });
            }
            if (!data1.Any(o => o.Interval == "Weekly"))
            {
                context.taskexecutionintervals.Add(new taskexecutioninterval()
                {
                    Interval = "Weekly"
                });
            }
            if (!data1.Any(o => o.Interval == "Monthly"))
            {
                context.taskexecutionintervals.Add(new taskexecutioninterval()
                {
                    Interval = "Monthly"
                });
            }
            if (!data1.Any(o => o.Interval == "Yearly"))
            {
                context.taskexecutionintervals.Add(new taskexecutioninterval()
                {
                    Interval = "Yearly"
                });
            }

            context.SaveChanges();
        }

        private void Seed_SubscriptionPackage(ApplicationDbContext context)
        {
            if (!context.AccountPackages.Any(o => o.AccessLevel == "Trial"))
            {
                var ap = new AccountPackage
                {
                    AccessLevel = "Trial",
                    Cost = 100,
                    NumberOfDomains = 1000,
                    NumberOfUsers = 1000,
                    NumberOfQbicles = 1000,
                    NumberOfGuests = 1000,
                    PerTimes = AccountPackage.PerTimeEnum.Free
                };
                context.AccountPackages.Add(ap);

                context.SaveChanges();
            }
        }

        private void Seed_TransactionMatchingType(ApplicationDbContext context)
        {
            var datas = context.transactionmatchingtypes;

            foreach (var tmtype in transactionMatchingTypes)
            {
                if (!datas.Any(o => o.Name == tmtype))
                {
                    var tmt = new transactionmatchingtype()
                    {
                        Name = tmtype
                    };
                    context.transactionmatchingtypes.Add(tmt);
                }
            }
            context.SaveChanges();
        }

        private void Seed_TransactionMatchingRelationship(ApplicationDbContext context)
        {
            var datas = context.transactionmatchingrelationships;

            foreach (var relationship in relationships)
            {
                if (!datas.Any(o => o.Name == relationship))
                {
                    var tmr = new transactionmatchingrelationship()
                    {
                        Name = relationship
                    };
                    context.transactionmatchingrelationships.Add(tmr);
                }
            }
            context.SaveChanges();
        }

        private void Seed_TransactionMatchingMethod(ApplicationDbContext context)
        {
            var datas = context.transactionmatchingmethods;

            foreach (var method in transactionMatchingMethods)
            {
                if (!datas.Any(o => o.Name == method))
                {
                    var tmm = new transactionmatchingmethod()
                    {
                        Name = method
                    };
                    context.transactionmatchingmethods.Add(tmm);
                }
            }
            context.SaveChanges();
        }

        private void Seed_transactionmatchingamountvariancevalues(ApplicationDbContext context)
        {
            var datas = context.transactionmatchingamountvariancevalues;

            foreach (var method in transactionmatchingamountvariancevalues)
            {
                if (!datas.Any(o => o.Percentage == method))
                {
                    var tmm = new transactionmatchingamountvariancevalue()
                    {
                        Percentage = method
                    };
                    context.transactionmatchingamountvariancevalues.Add(tmm);
                }
            }
            context.SaveChanges();
        }

        private void Seed_transactionmatchingdatevariancevalues(ApplicationDbContext context)
        {
            var datas = context.transactionmatchingdatevariancevalues;

            foreach (var method in transactionmatchingdatevariancevalues)
            {
                if (!datas.Any(o => o.NumberOfDays == method))
                {
                    var tmm = new transactionmatchingdatevariancevalue()
                    {
                        NumberOfDays = method
                    };
                    context.transactionmatchingdatevariancevalues.Add(tmm);
                }
            }
            context.SaveChanges();
        }

        private void Seed_ReasonSentEmail(ApplicationDbContext context)
        {
            var datas = context.reasonsentemails;
            if (!datas.Any(o => o.Name == "User Creation"))
            {
                var rse1 = new reasonsentemail()
                {
                    Id = 1,
                    Name = "User Creation"
                };
                context.reasonsentemails.Add(rse1);
            }

            if (!datas.Any(o => o.Name == "Forgot Password"))
            {
                var rse2 = new reasonsentemail()
                {
                    Id = 2,
                    Name = "Forgot Password"
                };
                context.reasonsentemails.Add(rse2);
            }
            context.SaveChanges();
        }

        private void Seed_BalanceAnalysisWarningLevel(ApplicationDbContext context)
        {
            var datas = context.balanceanalysiswarninglevels;
            if (!datas.Any(o => o.Name == "Very Low"))
            {
                var baw1 = new balanceanalysiswarninglevel()
                {
                    Id = 1,
                    Name = "Very Low"
                };
                context.balanceanalysiswarninglevels.Add(baw1);
            }

            if (!datas.Any(o => o.Name == "Normal"))
            {
                var baw2 = new balanceanalysiswarninglevel()
                {
                    Id = 2,
                    Name = "Normal"
                };
                context.balanceanalysiswarninglevels.Add(baw2);
            }

            if (!datas.Any(o => o.Name == "Very High"))
            {
                var baw3 = new balanceanalysiswarninglevel()
                {
                    Id = 3,
                    Name = "Very High"
                };
                context.balanceanalysiswarninglevels.Add(baw3);
            }

            context.SaveChanges();
        }

        private void Seed_AlertCondition(ApplicationDbContext context)
        {
            var datas = context.alertconditions;
            if (!datas.Any(o => o.Condition == "is greater than"))
            {
                var ac1 = new alertcondition()
                {
                    Id = 1,
                    Condition = "is greater than"
                };
                context.alertconditions.Add(ac1);
            }

            if (!datas.Any(o => o.Condition == "is less than"))
            {
                var ac2 = new alertcondition()
                {
                    Id = 2,
                    Condition = "is less than"
                };
                context.alertconditions.Add(ac2);
            }

            if (!datas.Any(o => o.Condition == "varies by more than %"))
            {
                var ac3 = new alertcondition()
                {
                    Id = 3,
                    Condition = "varies by more than %"
                };
                context.alertconditions.Add(ac3);
            }

            context.SaveChanges();
        }

        private HashSet<string> AllCulturedDateTimePatterns
        {
            get
            {
                var patterns = new HashSet<string>();
                var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                foreach (var culture in cultures)
                {
                    patterns.UnionWith(culture.DateTimeFormat.GetAllDateTimePatterns());
                }
                return patterns;
            }
        }

        public void Seed_DateFormats(ApplicationDbContext context)
        {
            // Get the system administrator
            var user = context.QbicleUser.First(o => o.UserName == "SysAdmin");

            var dateFormats = AllCulturedDateTimePatterns;
            var datas = context.dateformats;

            foreach (var newFormat in dateFormats)
            {
                var format = new dateformat();

                if (!datas.Any(o => o.Format == newFormat))
                {
                    try
                    {
                        format = new dateformat()
                        {
                            Format = newFormat,
                            CreatedById = user.Id,
                            CreatedDate = DateTime.UtcNow
                        };
                        context.dateformats.Add(format);
                        context.SaveChanges();
                    }
                    catch
                    {
                        context.dateformats.Remove(format);                    // Ignore any formats we can't save to the database because we can't use them anyway
                    }
                }
            }
        }

        private void Seed_QbicleApplications(ApplicationDbContext context)
        {
            var datas = context.AppRight;
            if (!datas.Any(o => o.Name == "View Content"))
            {
                var r1 = new AppRight()
                {
                    Id = 1,
                    Name = "View Content"
                };
                context.AppRight.Add(r1);
            }

            if (!datas.Any(o => o.Name == "Edit Content"))
            {
                var r2 = new AppRight()
                {
                    Id = 2,
                    Name = "Edit Content"
                };
                context.AppRight.Add(r2);
            }

            if (!datas.Any(o => o.Name == "Add Task to Qbicle"))
            {
                var r3 = new AppRight()
                {
                    Id = 3,
                    Name = "Add Task to Qbicle"
                };
                context.AppRight.Add(r3);
            }

            if (!datas.Any(o => o.Name == "Add Document to Qbicle"))
            {
                var r4 = new AppRight()
                {
                    Id = 4,
                    Name = "Add Document to Qbicle"
                };
                context.AppRight.Add(r4);
            }

            context.SaveChanges();

            var viewContentRight = context.AppRight.First(r => r.Name == "View Content");
            var editContentRight = context.AppRight.First(r => r.Name == "Edit Content");
            var data = context.Applications;
            //Group = "Core (integral to Qbicles)"
            if (!data.Any(o => o.Name == "Approvals"))
            {
                var approvalsApp = new QbicleApplication()
                {
                    Name = "Approvals",
                    Description = "Manage approval types for use throughout Qbicles.",
                    AppIcon = "fa fa-thumbs-up",
                    AppImage = "/Content/DesignStyle/img/icon_operator.png",
                    Group = "Core (integral to Qbicles)",
                    IsCore = true
                };
                approvalsApp.Rights.Add(viewContentRight);
                approvalsApp.Rights.Add(editContentRight);
                context.Applications.Add(approvalsApp);
            }
            if (!data.Any(o => o.Name == "Task Forms"))
            {
                var formsApp = new QbicleApplication()
                {
                    Name = "Task Forms",
                    Description = "Create dynamic forms and use them quickly and easily in tasks, events listings and more.",
                    AppIcon = "fa fa-file-code-o",
                    AppImage = "/Content/DesignStyle/img/icon_spannered.png",
                    Group = "Core (integral to Qbicles)",
                    IsCore = true
                };
                formsApp.Rights.Add(viewContentRight);
                formsApp.Rights.Add(editContentRight);
                context.Applications.Add(formsApp);
            }

            // Community
            if (!data.Any(o => o.Name == "Community"))
            {
                var rightId = 31;
                var app = new QbicleApplication()
                {
                    Name = "Community",
                    Description = "Engage in the Qbicles community",
                    AppIcon = "fa fa-users",
                    AppImage = "/Content/DesignStyle/img/icon_socialpost.png",
                    Group = "Core (integral to Qbicles)",
                    IsCore = true
                };
                foreach (var right in communityRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 31;
                var app = data.FirstOrDefault(o => o.Name == "Community");
                foreach (var right in communityRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            // Commented out by DJN to exclude from Release
            //if (!appData.Any(o => o.Name == "Topics / Project Management"))
            //{
            //    var app = new QbicleApplication()
            //    {
            //        Name = "Topics / Project Management",
            //        Description = "Content coming soon - placeholder.",
            //        AppIcon = "fa fa-object-group",
            //        AppImage = "/Content/DesignStyle/img/.png",
            //        Group = "Core (integral to Qbicles)"
            //    };
            //    context.Applications.Add(app);

            //}

            // Group = "Operations & Accounting"
            var cleanBooksApp = data.FirstOrDefault(q => q.Name == "CleanBooks");
            if (cleanBooksApp == null) //Account
            {
                cleanBooksApp = new QbicleApplication()
                {
                    Name = "CleanBooks",
                    Description =
                        "Cleanbooks helps you keep on top of your accounts and balance the books with its suite of accountancy tools.",
                    AppIcon = "fa fa-check-square-o",
                    AppImage = "/Content/DesignStyle/img/icon_cleanbooks.png",
                    Group = "Operations & Accounting"
                };
                context.Applications.Add(cleanBooksApp);
                context.SaveChanges();
            }
            else
            {
                cleanBooksApp.Name = "CleanBooks";
                cleanBooksApp.Description = "Cleanbooks helps you keep on top of your accounts and balance the books with its suite of accountancy tools.";
                cleanBooksApp.AppIcon = "fa fa-check-square-o";
                cleanBooksApp.AppImage = "/Content/DesignStyle/img/icon_cleanbooks.png";
                cleanBooksApp.Group = "Operations & Accounting";
            }

            var cleanBookRight = 5;
            foreach (var right in cbRights)
            {
                if (!datas.Any(o => o.Name == right))
                {
                    var r = new AppRight()
                    {
                        Id = cleanBookRight,
                        Name = right
                    };
                    context.AppRight.Add(r);
                    cleanBooksApp.Rights.Add(r);
                }
                cleanBookRight++;
            }

            // Bookkeeping

            if (!data.Any(o => o.Name == "Bookkeeping"))
            {
                var rightId = 25;
                var app = new QbicleApplication()
                {
                    Name = "Bookkeeping",
                    Description = "Streamline your bookkeeping with our comprehensive suite of tools.",
                    AppIcon = "fa fa-database",
                    AppImage = "/Content/DesignStyle/img/icon_bookkeeping.png",
                    Group = "Operations & Accounting"
                };
                foreach (var right in bkRights)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 25;
                var app = data.FirstOrDefault(o => o.Name == "Bookkeeping");
                app.Name = "Bookkeeping";
                app.Description = "Streamline your bookkeeping with our comprehensive suite of tools.";
                app.AppIcon = "fa fa-database";
                app.AppImage = "/Content/DesignStyle/img/icon_bookkeeping.png";
                app.Group = "Operations & Accounting";
                foreach (var right in bkRights)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            // Commented out by DJN to exclude from Release
            if (!data.Any(o => o.Name == "Trader"))
            {
                var rightId = 36;
                var app = new QbicleApplication()
                {
                    Name = "Trader",
                    Description = "Manage all aspects of your business revenue stream - maintain inventory and issue/manage invoices and sales.",
                    AppIcon = "fa fa-truck",
                    AppImage = "/Content/DesignStyle/img/icon_trader.png",
                    Group = "Operations & Accounting"
                };
                foreach (var right in traderRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 36;
                var app = data.FirstOrDefault(o => o.Name == "Trader");
                app.Name = "Trader";
                app.Description = "Manage all aspects of your business revenue stream - maintain inventory and issue/manage invoices and sales.";
                app.AppIcon = "fa fa-truck";
                app.AppImage = "/Content/DesignStyle/img/icon_trader.png";
                app.Group = "Operations & Accounting";
                foreach (var right in traderRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            // Add basic rights for Sales & Marketing
            if (!data.Any(o => o.Name == "Sales & Marketing"))
            {
                var rightId = 38;
                var app = new QbicleApplication()
                {
                    Name = "Sales & Marketing",
                    Description = "Manage your outreach, loyalty schemes and sales pipelines.",
                    AppIcon = "fa fa-piggy-bank",
                    AppImage = "/Content/DesignStyle/img/icon_sm.png",
                    Group = "Business Services"
                };
                foreach (var right in salesMarketingRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 36;
                var app = data.FirstOrDefault(o => o.Name == "Sales & Marketing");
                app.Name = "Sales & Marketing";
                app.Description = "Manage your outreach, loyalty schemes and sales pipelines.";
                app.AppIcon = "fa fa-piggy-bank";
                app.AppImage = "/Content/DesignStyle/img/icon_sm.png";
                app.Group = "Business Services";
                foreach (var right in salesMarketingRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            // Commented out by DJN to exclude from Release
            //if (!appData.Any(o => o.Name == "People"))
            //{
            //    var app = new QbicleApplication()
            //    {
            //        Name = "People",
            //        Description = "Content coming soon - placeholder",
            //        AppIcon = "fa fa-id-card",
            //        Group = "Operations & Accounting"
            //    };
            //    context.Applications.Add(app);
            //}

            // Commented out by DJN to exclude from Release
            if (!data.Any(o => o.Name == "Spannered"))
            {
                var rightId = 39;
                var app = new QbicleApplication()
                {
                    Name = "Spannered",
                    Description = "Asset and resource management for your business.",
                    AppIcon = "fa fa-wrench",
                    AppImage = "/Content/DesignStyle/img/icon_spannered.png",
                    Group = "Operations & Accounting"
                };
                foreach (var right in spanneredRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 39;
                var app = data.FirstOrDefault(o => o.Name == "Spannered");
                app.Name = "Spannered";
                app.Description = "Asset and resource management for your business.";
                app.AppIcon = "fa fa-wrench";
                app.AppImage = "/Content/DesignStyle/img/icon_spannered.png";
                app.Group = "Operations & Accounting";
                foreach (var right in spanneredRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            // Add basic rights for Operator
            if (!data.Any(o => o.Name == "Operator"))
            {
                var rightId = 40;
                var app = new QbicleApplication()
                {
                    Name = "Operator",
                    Description = "Manage HR, payroll, internal standards and compliance with Qbicles Operator.",
                    AppIcon = "fa fa-coffee",
                    AppImage = "/Content/DesignStyle/img/icon_operator.png",
                    Group = "Operations & Accounting"
                };
                foreach (var right in operatorRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 40;
                var app = data.FirstOrDefault(o => o.Name == "Operator");
                app.Name = "Operator";
                app.Description = "Manage HR, payroll, internal standards and compliance with Qbicles Operator.";
                app.AppIcon = "fa fa-coffee";
                app.AppImage = "/Content/DesignStyle/img/icon_operator.png";
                app.Group = "Operations & Accounting";
                foreach (var right in operatorRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            //Add basic rights for B2B
            if (!data.Any(o => o.Name == "Commerce"))
            {
                var rightId = 41;
                var app = new QbicleApplication()
                {
                    Name = "Commerce",
                    Description = "A marketplace for business to business transactions and listings.",
                    AppIcon = "fa fa-dumster",
                    AppImage = "/Content/DesignStyle/img/icon_commerce.png",
                    Group = "Business Services"
                };
                foreach (var right in commerceRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 41;
                var app = data.FirstOrDefault(o => o.Name == "Commerce");
                app.Name = "Commerce";
                app.Description = "A marketplace for business to business transactions and listings.";
                app.AppIcon = "fa fa-dumster";
                app.AppImage = "/Content/DesignStyle/img/icon_commerce.png";
                app.Group = "Business Services";
                foreach (var right in commerceRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            //Add basic rights for MyBankMate
            if (!data.Any(o => o.Name == "MyBankMate"))
            {
                var rightId = 41;
                var app = new QbicleApplication()
                {
                    Name = "MyBankMate",
                    Description = "Manage and facilitate seamless payment integrations with BankMate.",
                    AppIcon = "fa fa-university",
                    AppImage = "/Content/DesignStyle/img/icon_bankmate.png",
                    Group = "Business Services"
                };
                foreach (var right in myBankMateRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 42;
                var app = data.FirstOrDefault(o => o.Name == "MyBankMate");
                app.Name = "MyBankMate";
                app.Description = "Manage and facilitate seamless payment integrations with BankMate.";
                app.AppIcon = "fa fa-university";
                app.AppImage = "/Content/DesignStyle/img/icon_bankmate.png";
                app.Group = "Business Services";
                foreach (var right in myBankMateRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            //Add basic rights for Business Access
            if (!data.Any(o => o.Name == "Business Access"))
            {
                var rightId = 43;
                var app = new QbicleApplication()
                {
                    Name = "Business Access",
                    Description = "Business Access.",
                    AppIcon = "fa fa-university",
                    AppImage = "/Content/DesignStyle/img/icon_bankmate.png",
                    Group = "Core (integral to Qbicles)",
                    IsCore = true
                };
                foreach (var right in businessRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 43;
                var app = data.FirstOrDefault(o => o.Name == "Business Access");
                app.Name = "Business Access";
                app.Description = "Business Access.";
                app.AppIcon = "fa fa-university";
                app.AppImage = "/Content/DesignStyle/img/icon_bankmate.png";
                app.Group = "Core (integral to Qbicles)";
                app.IsCore = true;
                foreach (var right in businessRight)
                {
                    if (!datas.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }
            //===============Business Services

            // Commented out by DJN to exclude from Release
            //if (!appData.Any(o => o.Name == "Knowledge Base"))
            //{
            //    var app = new QbicleApplication()
            //    {
            //        Name = "Knowledge Base",
            //        Description = "Content coming soon - placeholder",
            //        AppIcon = "fa fa-lightbulb-o",
            //        AppImage = "/Content/DesignStyle/img/.png",
            //        Group = "Business Services"
            //    };
            //    context.Applications.Add(app);

            //}

            // Commented out by DJN to exclude from Release
            //if (!appData.Any(o => o.Name == "Payroll / Taxes"))
            //{
            //    var app = new QbicleApplication()
            //    {
            //        Name = "Payroll / Taxes",
            //        Description = "Content coming soon - placeholder",
            //        AppIcon = "fa fa-money",
            //        AppImage = "/Content/DesignStyle/img/.png",
            //        Group = "Business Services"
            //    };
            //    context.Applications.Add(app);

            //}

            // Commented out by DJN to exclude from Release
            //if (!appData.Any(o => o.Name == "Insurance"))
            //{
            //    var app = new QbicleApplication()
            //    {
            //        Name = "Insurance",
            //        Description = "Content coming soon - placeholder",
            //        AppIcon = "fa fa-wrench",
            //        AppImage = "/Content/DesignStyle/img/.png",
            //        Group = "Business Services"
            //    };
            //    context.Applications.Add(app);
            //}

            context.SaveChanges();
        }

        private void Seed_QbicleFileTypes(ApplicationDbContext context)
        {
            var datas = context.QbicleFileTypes;
            if (!datas.Any(o => o.Extension == "webp"))
            {
                var fileTypeWebp = new QbicleFileType()
                {
                    Extension = "webp",
                    Type = "Image File",
                    ImgPath = "/Content/DesignStyle/img/media-item-webp.jpg",
                    IconPath = "/Content/DesignStyle/img/media-item-webp.jpg",
                    MaxFileSize = 3097152,
                };
                context.QbicleFileTypes.Add(fileTypeWebp);
            }

            if (!datas.Any(o => o.Extension == "ppt"))
            {
                var fileTypePpt = new QbicleFileType()
                {
                    Extension = "ppt",
                    Type = "Powerpoint Presentation",
                    ImgPath = "/Content/DesignStyle/img/media-item-ppt.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_ppt.png",
                    MaxFileSize = 3097152,
                };
                context.QbicleFileTypes.Add(fileTypePpt);
            }

            if (!datas.Any(o => o.Extension == "pptx"))
            {
                var fileTypePptx = new QbicleFileType()
                {
                    Extension = "pptx",
                    Type = "Powerpoint Presentation",
                    ImgPath = "/Content/DesignStyle/img/media-item-pptx.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_pptx.png",
                    MaxFileSize = 3097152,
                };
                context.QbicleFileTypes.Add(fileTypePptx);
            }

            if (!datas.Any(o => o.Extension == "doc"))
            {
                var fileTypeDoc = new QbicleFileType()
                {
                    Extension = "doc",
                    Type = "Word Document",
                    ImgPath = "/Content/DesignStyle/img/media-item-doc.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_doc.png",
                    MaxFileSize = 3097152,
                };
                context.QbicleFileTypes.Add(fileTypeDoc);
            }

            if (!datas.Any(o => o.Extension == "docx"))
            {
                var fileTypeDocx = new QbicleFileType()
                {
                    Extension = "docx",
                    Type = "Word Document",
                    ImgPath = "/Content/DesignStyle/img/media-item-docx.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_docx.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeDocx);
            }

            if (!datas.Any(o => o.Extension == "xls"))
            {
                var fileTypeXls = new QbicleFileType()
                {
                    Extension = "xls",
                    Type = "Excel File",
                    ImgPath = "/Content/DesignStyle/img/media-item-xls.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_xls.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeXls);
            }

            if (!datas.Any(o => o.Extension == "xlsx"))
            {
                var fileTypeXslx = new QbicleFileType()
                {
                    Extension = "xlsx",
                    Type = "Excel File",
                    ImgPath = "/Content/DesignStyle/img/media-item-xlsx.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_xlsx.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeXslx);
            }

            if (!datas.Any(o => o.Extension == "jpg"))
            {
                var fileTypeJpg = new QbicleFileType()
                {
                    Extension = "jpg",
                    Type = "Image File",
                    ImgPath = "/Content/DesignStyle/img/media-item-jpg.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_jpg.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeJpg);
            }

            if (!datas.Any(o => o.Extension == "jpeg"))
            {
                var fileTypeJpeg = new QbicleFileType()
                {
                    Extension = "jpeg",
                    Type = "Image File",
                    ImgPath = "/Content/DesignStyle/img/media-item-jpeg.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_jpeg.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeJpeg);
            }

            if (!datas.Any(o => o.Extension == "png"))
            {
                var fileTypePng = new QbicleFileType()
                {
                    Extension = "png",
                    Type = "Image File",
                    ImgPath = "/Content/DesignStyle/img/media-item-png.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_png.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypePng);
            }

            if (!datas.Any(o => o.Extension == "zip"))
            {
                var fileTypeZip = new QbicleFileType()
                {
                    Extension = "zip",
                    Type = "Compressed File",
                    ImgPath = "/Content/DesignStyle/img/media-item-zip.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_zip.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeZip);
            }

            if (!datas.Any(o => o.Extension == "pdf"))
            {
                var fileTypePdf = new QbicleFileType()
                {
                    Extension = "pdf",
                    Type = "Image File",
                    ImgPath = "/Content/DesignStyle/img/media-item-pdf.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_pdf.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypePdf);
            }

            if (!datas.Any(o => o.Extension == "gif"))
            {
                var fileTypeGif = new QbicleFileType()
                {
                    Extension = "gif",
                    Type = "Portable Document Format",
                    ImgPath = "/Content/DesignStyle/img/media-item-gif.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_gif.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeGif);
            }

            if (!datas.Any(o => o.Extension == "mp4"))
            {
                var fileTypeGif = new QbicleFileType()
                {
                    Extension = "mp4",
                    Type = "Video File",
                    ImgPath = "/Content/DesignStyle/img/media-item-mp4.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_mp4.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeGif);
            }

            if (!datas.Any(o => o.Extension == "webm"))
            {
                var fileTypeGif = new QbicleFileType()
                {
                    Extension = "webm",
                    Type = "Video File",
                    ImgPath = "/Content/DesignStyle/img/media-item-webm.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_webm.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeGif);
            }

            if (!datas.Any(o => o.Extension == "ogv"))
            {
                var fileTypeGif = new QbicleFileType()
                {
                    Extension = "ogv",
                    Type = "Video File",
                    ImgPath = "/Content/DesignStyle/img/media-item-ogv.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_ogv.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeGif);
            }

            if (!datas.Any(o => o.Extension == "txt"))
            {
                var fileTypeGif = new QbicleFileType()
                {
                    Extension = "txt",
                    Type = "Plain Text File",
                    ImgPath = "/Content/DesignStyle/img/media-item-txt.jpg",
                    IconPath = "/Content/DesignStyle/img/icon_file_txt.png",
                    MaxFileSize = 3097152
                };
                context.QbicleFileTypes.Add(fileTypeGif);
            }
            context.SaveChanges();
        }

        private void Seed_AccountUpdateFrequencies(ApplicationDbContext context)
        {
            accountupdatefrequency cbAccountupdatefrequency;
            var datas = context.accountupdatefrequencies;

            if (!datas.Any(o => o.frequency == "Daily"))
            {
                cbAccountupdatefrequency = new accountupdatefrequency { frequency = "Daily" };
                context.accountupdatefrequencies.Add(cbAccountupdatefrequency);
            }

            if (!datas.Any(o => o.frequency == "Weekly"))
            {
                cbAccountupdatefrequency = new accountupdatefrequency { frequency = "Weekly" };
                context.accountupdatefrequencies.Add(cbAccountupdatefrequency);
            }

            if (!datas.Any(o => o.frequency == "Monthly"))
            {
                cbAccountupdatefrequency = new accountupdatefrequency { frequency = "Monthly" };
                context.accountupdatefrequencies.Add(cbAccountupdatefrequency);
            }

            if (!datas.Any(o => o.frequency == "Annually"))
            {
                cbAccountupdatefrequency = new accountupdatefrequency { frequency = "Annually" };
                context.accountupdatefrequencies.Add(cbAccountupdatefrequency);
            }

            context.SaveChanges();
        }

        private void Seed_CleanBooksFileTypes(ApplicationDbContext context)
        {
            var data = context.filetypes;
            if (!data.Any(o => o.Type == "CSV"))
            {
                var fileTypeCsv = new filetype { Type = "CSV" };

                context.filetypes.Add(fileTypeCsv);
            }

            if (!data.Any(o => o.Type == "Excel"))
            {
                var fileTypeExcel = new filetype { Type = "Excel" };

                context.filetypes.Add(fileTypeExcel);
            }

            context.SaveChanges();
        }

        private void Seed_CreateSystemDomain(ApplicationDbContext context)
        {
            var systemdomainB2B = context.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.BUSINESS2BUSINESS && s.Type == SystemDomainType.B2B);
            if (systemdomainB2B == null)
            {
                //Remove systemdomain is wrong
                string query = $"UPDATE `qb_qbicledomain` SET `Discriminator` = 'QbicleDomain' WHERE Id>0 and Discriminator='';";
                query += $"UPDATE `qb_qbicles` SET `Discriminator` = 'Qbicle' WHERE Id>0 and Discriminator='';";
                context.Database.ExecuteSqlCommand(query);
                //Create Systemdomain
                systemdomainB2B = new SystemDomain();
                systemdomainB2B.Type = SystemDomainType.B2B;
                systemdomainB2B.Name = SystemDomainConst.BUSINESS2BUSINESS;
                systemdomainB2B.CreatedDate = DateTime.UtcNow;
                context.SystemDomains.Add(systemdomainB2B);
                context.SaveChanges();
            }
            var systemdomainB2C = context.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.BUSINESS2CUSTOMER && s.Type == SystemDomainType.B2C);
            if (systemdomainB2C == null)
            {
                systemdomainB2C = new SystemDomain();
                systemdomainB2C.Type = SystemDomainType.B2C;
                systemdomainB2C.Name = SystemDomainConst.BUSINESS2CUSTOMER;
                systemdomainB2C.CreatedDate = DateTime.UtcNow;
                context.SystemDomains.Add(systemdomainB2C);
                context.SaveChanges();
            }
            var systemdomainC2C = context.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.CUSTOMER2CUSTOMER && s.Type == SystemDomainType.C2C);
            if (systemdomainC2C == null)
            {
                systemdomainC2C = new SystemDomain();
                systemdomainC2C.Type = SystemDomainType.C2C;
                systemdomainC2C.Name = SystemDomainConst.CUSTOMER2CUSTOMER;
                systemdomainC2C.CreatedDate = DateTime.UtcNow;
                context.SystemDomains.Add(systemdomainC2C);
                context.SaveChanges();
            }
        }

        private void Seed_SubscriptionApplicationIsCore(ApplicationDbContext context)
        {
            var appsCore = context.Applications.Where(e => e.IsCore).ToList();
            var domains = context.Domains.ToList();
            domains.ForEach(d =>
            {
                // No apps added to system domains
                if (d is SystemDomain)
                    return;

                //create Core appinstance for any Domain that has NO associated apps
                if (d.AssociatedApps == null || d.AssociatedApps.Count == 0)
                {
                    appsCore.ForEach(app =>
                    {
                        context.AppInstances.Add(new AppInstance
                        {
                            CreatedBy = d.CreatedBy,
                            CreatedDate = DateTime.UtcNow,
                            Domain = d,
                            QbicleApplication = app
                        });
                    });
                }

                // add an app instance for Qbicles Business app for any Domain that does not have one
                var appInstance = d.AssociatedApps.FirstOrDefault(aa => aa.QbicleApplication.Name == QbiclesBoltOns.QbiclesBusiness);
                if (appInstance == null)
                {
                    var businessApp = context.Applications.FirstOrDefault(qa => qa.Name == QbiclesBoltOns.QbiclesBusiness);
                    context.AppInstances.Add(new AppInstance
                    {
                        CreatedBy = d.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        Domain = d,
                        QbicleApplication = businessApp
                    });

                    d.SubscribedApps.Add(businessApp);
                }

                //subscribe is core app
                if (d.SubscribedApps.Any(a => a.IsCore))
                    return;
                d.SubscribedApps.AddRange(appsCore);
            });
            context.SaveChanges();
        }

        private void sp_GetCbAccountById(ApplicationDbContext context)
        {
            string dropSp =
                @"DROP PROCEDURE IF EXISTS `sp_GetCbAccountById`;
                CREATE PROCEDURE `sp_GetCbAccountById`(
                    IN id int
                )
                BEGIN
                    select name as Name, number as Value  from cb_accounts where id=id;
                END;";
            context.Database.ExecuteSqlCommand(dropSp);

            //var x = new StringBuilder();
            //x.AppendLine(@"create procedure updateusagecosts");
            //x.AppendLine(@"@InvoicePeriod datetime,");
            //x.AppendLine(@"@CustomerContractId bigint,");
            //x.AppendLine(@"@TypeA nvarChar(80),");
            //x.AppendLine(@"@TypeB nvarChar(80),");
            //x.AppendLine(@"@VatValue decimal(18, 2),");
            //x.AppendLine(@"@PurchaseCosts decimal(18, 2),");
            //x.AppendLine(@"@RetailCosts decimal(18, 2)");
            //x.AppendLine(@"as");
            //x.AppendLine(@"begin");
            //x.AppendLine(@"Merge [usagecosts]");
            //x.AppendLine(@"Using (Select @InvoicePeriod as invoicePeriod,");
            //x.AppendLine(@"              @CustomerContractId as customercontractId,");
            //x.AppendLine(@"              @TypeA as typeA,");
            //x.AppendLine(@"              @TypeB as typeB,");
            //x.AppendLine(@"              @VatValue as vatvalue)");
            //x.AppendLine(@"              As tmp ");
            //x.AppendLine(@"On ([usagecosts].[invoiceperiod] = tmp.invoiceperiod");
            //x.AppendLine(@"AND [usagecosts].[customercontractId] = tmp.customercontractid");
            //x.AppendLine(@"AND [usagecosts].[typeA] = tmp.typeA");
            //x.AppendLine(@"AND [usagecosts].[typeB] = tmp.typeB");
            //x.AppendLine(@"AND [usagecosts].[vatvalue] = tmp.Vatvalue)");
            //x.AppendLine(@"When Matched Then ");
            //x.AppendLine(@"    Update Set [usagecosts].[purchasecosts] = [usagecosts].[purchasecosts] + @purchasecosts,");
            //x.AppendLine(@"               [usagecosts].[retailcosts] = [usagecosts].[retailcosts] + @retailcosts");
            //x.AppendLine(@"When Not Matched Then");
            //x.AppendLine(@"    Insert (InvoicePeriod, CustomerContractId, typea, typeb, vatvalue, purchasecosts, retailcosts)");
            //x.AppendLine(@"    Values (@invoiceperiod, @CustomerContractId, @TypeA, @TypeB, @VatValue, @PurchaseCosts, @RetailCosts);");
            //x.AppendLine(@"end");
            //context.Database.ExecuteSqlCommand(x.ToString());
        }

        /// <summary>
        /// Update data relationshipt between pos variant and option - ONLY ONE RUN
        /// </summary>
        /// <param name="context"></param>
        private void Seed_UpdateDataVariantOption(ApplicationDbContext context)
        {
            string insertVariantOption = "";
            var posVariants = context.PosVariants.ToList();

            //safe case validation for the case has inserted data, but not yet for remove this method
            if (posVariants.Any(o => o.VariantOptions.Count > 0))
                return;

            posVariants.ForEach(variant =>
            {
                if (variant.CategoryItem == null) return;
                var properties = context.PosVariantProperties.Where(item => item.CategoryItem.Id == variant.CategoryItem.Id).ToList();
                if (properties.Count == 0) return;

                var variantOptions = variant.Name.Split('/');

                properties.ForEach(propety =>
                {
                    var options = context.PosVariantOptions.Where(option => option.VariantProperty.Id == propety.Id && variantOptions.Contains(option.Name)).ToList();
                    if (options.Count == 0) return;
                    options.ForEach(o =>
                    {
                        insertVariantOption += $"INSERT INTO posvariantoptionposvariants SELECT {o.Id},{variant.Id};";
                    });
                });
            });

            if (!string.IsNullOrEmpty(insertVariantOption))
                context.Database.ExecuteSqlCommand(insertVariantOption);
        }

        private void Seed_Banks(ApplicationDbContext context)
        {
            //banks name
            var bank_gtb_name = "Guaranty Trust Bank (GTB)";
            var bank_zenith_name = "Zenith Bank (Zenith)";
            var bank_united_name = "United Bank for Africa";
            var bank_fidelity_name = "Fidelity Bank (Fidelity)";
            var bank_sterling_name = "Sterling Bank (Sterling)";
            var bank_fcmb_name = "First City Monument Bank (FCMB) ";
            //end
            //banks icon
            var bank_gtb_icon = "694aac6e-c0d7-4902-8bce-080ab7921d4d";
            var bank_zenith_icon = "d87a83fa-b4b8-4024-b6f4-e7d519a0e9bc";
            var bank_united_icon = "f76b6baa-69e5-4f16-a1b2-8311bd12d416";
            var bank_fidelity_icon = "cf239c7b-e643-4931-8fb3-f07cc10f62d2";
            var bank_sterling_icon = "4a676c19-9e2a-461f-9f9d-ab30b024ec31";
            var bank_fcmb_icon = "3d625898-e279-4a88-96dd-98fcf6d90410";
            //end
            var datas = context.Banks;
            if (!datas.Any(o => o.Name == bank_gtb_name))
            {
                var bank = new Bank()
                {
                    Name = bank_gtb_name,
                    LogoUri = bank_gtb_icon
                };
                context.Banks.Add(bank);
            }
            else
            {
                var bank = datas.First(o => o.Name == bank_gtb_name);
                bank.LogoUri = bank_gtb_icon;
            }

            if (!datas.Any(o => o.Name == bank_zenith_name))
            {
                var bank = new Bank()
                {
                    Name = bank_zenith_name,
                    LogoUri = bank_zenith_icon
                };
                context.Banks.Add(bank);
            }
            else
            {
                var bank = datas.First(o => o.Name == bank_zenith_name);
                bank.LogoUri = bank_zenith_icon;
            }

            if (!datas.Any(o => o.Name == bank_united_name))
            {
                var bank = new Bank()
                {
                    Name = bank_united_name,
                    LogoUri = bank_united_icon
                };
                context.Banks.Add(bank);
            }
            else
            {
                var bank = datas.First(o => o.Name == bank_united_name);
                bank.LogoUri = bank_united_icon;
            }

            if (!datas.Any(o => o.Name == bank_fidelity_name))
            {
                var bank = new Bank()
                {
                    Name = bank_fidelity_name,
                    LogoUri = bank_fidelity_icon
                };
                context.Banks.Add(bank);
            }
            else
            {
                var bank = datas.First(o => o.Name == bank_fidelity_name);
                bank.LogoUri = bank_fidelity_icon;
            }

            if (!datas.Any(o => o.Name == bank_sterling_name))
            {
                var bank = new Bank()
                {
                    Name = bank_sterling_name,
                    LogoUri = bank_sterling_icon
                };
                context.Banks.Add(bank);
            }
            else
            {
                var bank = datas.First(o => o.Name == bank_sterling_name);
                bank.LogoUri = bank_sterling_icon;
            }

            if (!datas.Any(o => o.Name == bank_fcmb_name))
            {
                var bank = new Bank()
                {
                    Name = bank_fcmb_name,
                    LogoUri = bank_fcmb_icon
                };
                context.Banks.Add(bank);
            }
            else
            {
                var bank = datas.First(o => o.Name == bank_fcmb_name);
                bank.LogoUri = bank_fcmb_icon;
            }

            context.SaveChanges();
        }

        private void Seed_BusinesseAccessApplications(ApplicationDbContext context)
        {
            var appright = context.AppRight;
            var applications = context.Applications;
            //Add basic rights for Business Access
            if (!applications.Any(o => o.Name == QbiclesBoltOns.QbiclesBusiness))
            {
                var rightId = 43;
                var app = new QbicleApplication()
                {
                    Name = QbiclesBoltOns.QbiclesBusiness,
                    Description = "Qbicles Business.",
                    AppIcon = "fa fa-university",
                    AppImage = "/Content/DesignStyle/img/icon_bankmate.png",
                    Group = "Core (integral to Qbicles)",
                    IsCore = true
                };
                foreach (var right in businessRight)
                {
                    if (!appright.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app.Rights.Add(r);
                    }
                    rightId++;
                }
                context.Applications.Add(app);
            }
            else
            {
                var rightId = 43;
                var app = applications.FirstOrDefault(o => o.Name == QbiclesBoltOns.QbiclesBusiness);
                foreach (var right in businessRight)
                {
                    if (!appright.Any(o => o.Name == right))
                    {
                        var r = new AppRight()
                        {
                            Id = rightId,
                            Name = right
                        };
                        context.AppRight.Add(r);
                        app?.Rights.Add(r);
                    }
                    rightId++;
                }
            }

            context.SaveChanges();
        }

        private void Seed_QbiclesBusinesseAccessRole(ApplicationDbContext context)
        {
            var domains = context.Domains.ToList();
            foreach (var domain in domains)
            {
                if (context.DomainRole.Any(r => r.Domain.Id == domain.Id && r.Name == FixedRoles.QbiclesBusinessRole))
                    continue;

                var user = domain.Users.FirstOrDefault(u => u.IsSystemAdmin);
                var domainRole = new DomainRole()
                {
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Domain = domain,
                    Name = FixedRoles.QbiclesBusinessRole
                };
                domainRole.Users.AddRange(domain.Administrators);
                context.DomainRole.Add(domainRole);
                context.Entry(domainRole).State = EntityState.Added;

                var appInstance = context.AppInstances.FirstOrDefault(a => a.Domain.Id == domain.Id && a.QbicleApplication.Name == QbiclesBoltOns.QbiclesBusiness);
                var roleRightXref = new RoleRightAppXref
                {
                    AppInstance = appInstance,
                    Right = context.AppRight.FirstOrDefault(e => e.Name == RightPermissions.QbiclesBusinessAccess),
                    Role = domainRole
                };

                context.RoleRightAppXref.Add(roleRightXref);
                context.Entry(roleRightXref).State = EntityState.Added;
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Implement system roles to Qbicles
        /// </summary>
        /// <param name="context"></param>
        private void Seed_SystemRolesQBicles(ApplicationDbContext context)
        {
            var roles = context.Roles.ToList();

            if (!roles.Any(r => r.Name == SystemRoles.SystemAdministrator))
                context.Roles.Add(
                    new Microsoft.AspNet.Identity.CoreCompat.IdentityRole
                    {
                        NormalizedName = SystemRoles.SystemAdministrator.ToUpper(),
                        Name = SystemRoles.SystemAdministrator
                    });

            if (!roles.Any(r => r.Name == SystemRoles.QbiclesBankManager))
                context.Roles.Add(
                    new Microsoft.AspNet.Identity.CoreCompat.IdentityRole
                    {
                        NormalizedName = SystemRoles.QbiclesBankManager.ToUpper(),
                        Name = SystemRoles.QbiclesBankManager
                    });

            if (!roles.Any(r => r.Name == SystemRoles.SocialHighlightsBlogger))
                context.Roles.Add(
                    new Microsoft.AspNet.Identity.CoreCompat.IdentityRole
                    {
                        NormalizedName = SystemRoles.SocialHighlightsBlogger.ToUpper(),
                        Name = SystemRoles.SocialHighlightsBlogger
                    });
            context.SaveChanges();

            var userManager = new UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.CoreCompat.UserStore<ApplicationUser>(context));
            var users = context.QbicleUser.Where(u => u.IsSystemAdmin || (u.IsQbiclesBankManager ?? false) || u.IsSocialHighlightsBlogger).ToList();
            users.ForEach(user =>
            {
                var userRoles = userManager.GetRoles(user.Id);
                if (user.IsSystemAdmin && !userRoles.Any(r => r == SystemRoles.SystemAdministrator))
                    userManager.AddToRole(user.Id, SystemRoles.SystemAdministrator);
                if (user.IsQbiclesBankManager ?? false && !userRoles.Any(r => r == SystemRoles.QbiclesBankManager))
                    userManager.AddToRole(user.Id, SystemRoles.QbiclesBankManager);
                if (user.IsSocialHighlightsBlogger && !userRoles.Any(r => r == SystemRoles.SocialHighlightsBlogger))
                    userManager.AddToRole(user.Id, SystemRoles.SocialHighlightsBlogger);
            });
        }

        /// <summary>
        /// Initial set Loyalty System Settings
        /// </summary>
        /// <param name="context"></param>
        private void Seed_InitialSetSystemSettings(ApplicationDbContext context)
        {
            var lstSettings = context.LoyaltySystemSettings;
            if (lstSettings.Count() == 0)
            {
                var userManager = new UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.CoreCompat.UserStore<ApplicationUser>(context));
                var firstSystemAdministration = context.QbicleUser.FirstOrDefault(p => p.IsSystemAdmin);
                var initialSystemSettings = new SystemSettings()
                {
                    Amount = 1,
                    Points = 1,
                    IsArchived = false,
                    CreatedBy = firstSystemAdministration,
                    CreatedDate = DateTime.UtcNow
                };

                context.LoyaltySystemSettings.Add(initialSystemSettings);
                context.Entry(initialSystemSettings).State = EntityState.Added;
                context.SaveChanges();
            }
        }

        private void Seed_UpdateShiftAuditApprovalDefinition(ApplicationDbContext context)
        {
            try
            {
                var user = context.QbicleUser.First(o => o.IsSystemAdmin);

                var workGroups = context.WorkGroups.ToList();
                workGroups.ForEach(wg =>
                {
                    var process = wg.Processes.FirstOrDefault(p => p.Name == TraderProcessName.ShiftAudits);
                    if (process == null)
                        return;

                    var shiftAuditApproval = context.ShiftAuditApprovalDefinitions.FirstOrDefault(s =>
                        s.ShiftAuditTraderProcessType.Name == TraderProcessName.ShiftAudits && s.ShiftAuditWorkGroup.Id == wg.Id);

                    if (shiftAuditApproval != null) return;

                    var groupName = $"{process.Name} Processes";
                    var appGroup = GetApprovalAppGroup(context, groupName, 0, user, wg.Domain);

                    var stockAuditAppDef = new ShiftAuditApprovalDefinition
                    {
                        Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                        Title = $"{wg.Name}/ {process.Name}",
                        ApprovalImage = "icon_bookkeeping.png",
                        Description = $"Trader WorkGroup: {wg.Name} {process.Name} process",
                        Initiators = wg.Members,
                        Approvers = wg.Approvers,
                        Reviewers = wg.Reviewers,
                        IsViewOnly = true,
                        Group = appGroup,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        ShiftAuditWorkGroup = wg,
                        ShiftAuditTraderProcessType = process
                    };
                    wg.ApprovalDefs.Add(stockAuditAppDef);

                    if (context.Entry(wg).State == EntityState.Detached)
                        context.WorkGroups.Attach(wg);
                    context.Entry(wg).State = EntityState.Modified;
                    context.SaveChanges();
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ApprovalGroup GetApprovalAppGroup(ApplicationDbContext context, string groupName, int groupId, ApplicationUser user, QbicleDomain currentDomain)
        {
            try
            {
                ApprovalGroup appGroup;
                var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                if (ai == null) return null;

                appGroup = context.ApprovalAppsGroup.FirstOrDefault(x => x.AppInstance.Id == ai.Id && x.Name == groupName);
                if (appGroup != null) return appGroup;

                var approvalGroup = new ApprovalGroup
                {
                    Name = groupName,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    AppInstance = ai
                };
                context.ApprovalAppsGroup.Add(approvalGroup);
                context.Entry(approvalGroup).State = EntityState.Added;
                context.SaveChanges();

                return approvalGroup;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Seed_DeliverySystemSetting(ApplicationDbContext context)
        {
            try
            {
                var setting = context.DeliverySystemSettings.FirstOrDefault();
                if (setting != null)
                    return;
                setting = new DeliverySystemSetting
                {
                    DriverUpdateLocationTimeInterval = 5
                };
                context.DeliverySystemSettings.Add(setting);
                context.Entry(setting).State = EntityState.Added;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Seed_UpdateCatalogMeu(ApplicationDbContext context)
        {
            var rules = new PosProductRules(context);
            var catalogues = context.PosMenus.Where(e => e.Devices.Count > 0).ToList();
            catalogues.ForEach(async catalog =>
            {
                await rules.ProcessUpdateCatalogProductSqliteAsync(catalog.Id);
            });
        }

        private void SeeUpdateVariantOptions(ApplicationDbContext context)
        {
            //var catalogues = context.PosMenus.Where(e => e.Id == 20).ToList();
            var catalogues = context.PosMenus.Where(m => !m.IsDeleted).ToList();
            var posMenuRules = new PosMenuRules(context);
            catalogues.ForEach(catalog =>
            {
                catalog.Categories.ForEach(category =>
                {
                    category.PosCategoryItems.ForEach(categoryItem =>
                    {
                        //if (categoryItem.Id != 17092) return;

                        var properties = categoryItem.VariantProperties.Where(q => q.VariantOptions != null && q.VariantOptions.Any()).OrderBy(q => q.Id).ToList();
                        var lstNameVariants = posMenuRules.GetPosVariantsByOption(properties);

                        var expectedOptions = categoryItem.VariantProperties.Count;

                        categoryItem.PosVariants.ForEach(variant =>
                        {
                            if (variant.VariantOptions.Count != expectedOptions)
                            {
                                var variantNames = variant.Name.Split('/').OrderBy(e => e).ToList();

                                lstNameVariants.ForEach(name =>
                                {
                                    var variantOptionName = name.Name.Split('/').OrderBy(e => e).ToList();
                                    if (variantNames.SequenceEqual(variantOptionName))
                                    {
                                        variant.VariantOptions.Clear();
                                        name.Options.ForEach(o =>
                                        {
                                            var option = context.PosVariantOptions.FirstOrDefault(e => e.Id == o.Id);
                                            variant.VariantOptions.Add(option);
                                        });
                                    }
                                });
                            }
                        });
                    });
                });
            });
            context.SaveChanges();
        }

        private void SeedMoveDeliveryQueueCompletedToQueueArchive(ApplicationDbContext context)
        {
            var deliveries = context.Deliveries.Where(d => d.Status == DeliveryStatus.Completed || d.Status == DeliveryStatus.CompletedWithProblems).ToList();

            foreach (var delivery in deliveries)
            {
                if (delivery.DeliveryQueueArchive != null)
                    return;

                var deliveryQueueArchive = context.DeliveryQueueArchives.FirstOrDefault(e => e.ParentDeliveryQueue.Id == delivery.DeliveryQueue.Id);

                if (deliveryQueueArchive == null)
                {
                    deliveryQueueArchive = new DeliveryQueueArchive
                    {
                        CreatedBy = delivery.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        Location = delivery.Driver.EmploymentLocation,
                        ParentDeliveryQueue = delivery.DeliveryQueue,
                        PrepQueue = delivery.DeliveryQueue.PrepQueue,
                        Name = $"{delivery.DeliveryQueue.Name} Delivery Queue Archive",
                    };
                    deliveryQueueArchive.Deliveries.Add(delivery);
                }

                delivery.DeliveryQueueArchive = deliveryQueueArchive;
                delivery.ActiveOrder = null;
                delivery.DeliveryQueue = null;

                if (delivery.TimeStarted == null)
                    delivery.TimeStarted = delivery.CreatedDate;
                if (delivery.TimeFinished == null)
                    delivery.TimeFinished = DateTime.UtcNow;

                //10 all Orders in the Delivery have been completed,
                if (delivery.Orders.All(o => o.Status == PrepQueueStatus.Completed))
                {
                    delivery.Status = DeliveryStatus.Completed;
                    delivery.DeliveryProblemNote = "";
                }
                else
                {
                    delivery.Status = DeliveryStatus.CompletedWithProblems;
                }

                //When the Delivery is completed, the Delivery.ActiveOrder will be set to null..

                delivery.Orders.OrderBy(s => s.DeliverySequence).ForEach(queueOrder =>
                {
                    if (queueOrder.PrepQueueArchive != null)
                        return;
                    //set the PrepQueueArchive for the QueueOrder
                    //(1) get the PrepQueue from the QueueOrder)
                    var prepQueue = queueOrder.PrepQueue;
                    //(2) Get the PrepQueueArchive with the PrepQueue as ParentPrepQueue (var archivePrepQueue)
                    var archivePrepQueue = queueOrder.PrepQueueArchive;
                    //(3) Then, to move the QueueOrder to archive
                    queueOrder.PrepQueue = null;
                    queueOrder.PrepQueueArchive = archivePrepQueue;
                    if (queueOrder.ArchivedDate == null)
                        queueOrder.ArchivedDate = DateTime.UtcNow;
                });

                context.SaveChanges();
            }
        }

        private void SeedUpdateDeliveryDriverArchived(ApplicationDbContext context)
        {
            var deliveries = context.Deliveries.Where(d => d.Driver != null).ToList();

            foreach (var d in deliveries)
            {
                d.DriverArchived = d.Driver;
                context.SaveChanges();
            }

            deliveries = context.Deliveries.Where(d => d.Driver == null && d.Status != DeliveryStatus.New).ToList();

            foreach (var d in deliveries)
            {
                var log = context.DriverLogs.FirstOrDefault(e => e.Delivery.Id == d.Id);
                if (log != null)
                {
                    d.DriverArchived = log.Driver;
                    context.SaveChanges();
                }
            }
        }

        private void SeedUpdateUserProfileWizardRunMicro(ApplicationDbContext context)
        {
            var users = context.QbicleUser.Where(e => e.IsUserProfileWizardRun).ToList();
            foreach (var user in users)
            {
                user.IsUserProfileWizardRunMicro = user.IsUserProfileWizardRun;
                context.SaveChanges();
            }
        }

        private void SeedUpdateBusinessProfileWizard(ApplicationDbContext context)
        {
            var domains = context.Domains.ToList();
            foreach (var d in domains)
            {
                d.IsBusinessProfileWizard = true;
                d.IsBusinessProfileWizardMicro = true;
                d.WizardStep = DomainWizardStep.Done;
                d.WizardStepMicro = DomainWizardStepMicro.Done;
                context.SaveChanges();
            }
        }

        private void SeedUpdateTopicInPost(ApplicationDbContext context)
        {
            var activities = context.Activities.Where(e => e.Posts.Any(t => t.Topic == null)).ToList();
            int index = 0;
            activities.ForEach(activity =>
            {
                var posts = activity.Posts.Where(e => e.Topic == null);
                if (!posts.Any()) return;
                var topic = new TopicRules(context).GetCreateTopicByName(HelperClass.GeneralName, activity.Qbicle.Id);

                posts.ForEach(post =>
                {
                    post.Topic = topic;

                    context.SaveChanges();
                    Console.WriteLine($"Update {index++}");
                });
            });
        }

        private void UpdateCatalogIsPublished(ApplicationDbContext context)
        {
            var businessProfiles = context.B2BProfiles.ToList();
            businessProfiles.ForEach(profile =>
            {
                profile.BusinessCatalogues.ForEach(catalog =>
                {
                    catalog.IsPublished = true;
                    context.SaveChanges();
                });
            });
        }

        private void ClearAreasOperationEmpty(ApplicationDbContext context)
        {
            var areasOperations = context.B2BAreasOfOperation.Where(e => e.Profile == null).ToList();
            context.B2BAreasOfOperation.RemoveRange(areasOperations);

            context.SaveChanges();
        }

        private void UpdateTraderItemDescription(ApplicationDbContext context)
        {
            string dropSp =
               @"UPDATE trad_item SET DescriptionText =
                    CASE
                        WHEN CHAR_LENGTH(Description) > 150 THEN CONCAT(SUBSTRING(Description, 1, 150), '...')
                        ELSE Description
                    END
                WHERE `Id` = `Id`;";
            context.Database.ExecuteSqlCommand(dropSp);
        }

        private void UpdateWaitlist(ApplicationDbContext context)
        {
            var sqlQuery =
                $"SET @adminId = (select id from users where IsSystemAdmin=true limit 1);" +
                $"INSERT INTO `system_waitlistrequest` " +
                $"(`CreatedDate`,`CountryName`,`CountryCode`,`NumberOfEmployees`,`DiscoveredVia`, " +
                $"`IsApprovedForSubsDomain`,`IsApprovedForCustomDomain`, " +
                $"`IsRejected`,`ReviewedDate`,`LastRequesstdDate`,`ReviewedBy_Id`,`User_Id`)" +
                $"select " +
                $"UTC_TIMESTAMP(),'Nigeria',164,3,0, " +
                $"true,true,false,UTC_TIMESTAMP(), " +
                $"UTC_TIMESTAMP(), " +
                $"@adminId,id from users;";
            //Console.WriteLine("system_waitlistrequest: " + sqlQuery);
            context.Database.ExecuteSqlCommand(sqlQuery);

            sqlQuery = $"SET @adminId = (select id from users where IsSystemAdmin=true limit 1);" +
                $"INSERT INTO `system_domaincreationrights` " +
                $"(`IsApprovedForSubsDomain`,`IsApprovedForCustomDomain`,`CreatedDate`,`LastModifiedDate`,`LastModifiedBy_Id`,`AssociatedUser_Id`)" +
                $"select true,true,UTC_TIMESTAMP(),UTC_TIMESTAMP(),@adminId,id from users;";
            //Console.WriteLine("system_domaincreationrights: " + sqlQuery);
            context.Database.ExecuteSqlCommand(sqlQuery);
        }
    }
}