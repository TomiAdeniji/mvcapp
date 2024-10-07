using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using Qbicles.BusinessRules.Helper;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class BookkeepingIntegrationRules
    {
        ApplicationDbContext dbContext;

        public BookkeepingIntegrationRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        private JournalGroup GetJournalGroup(int domainId)
        {
            return new TraderSettingRules(dbContext).GetTraderSettingByDomain(domainId)?.JournalGroupDefault;
        }




        /// <summary>
        /// Bookkeeping Integration for Manufacturing - QBIC-1862
        /// A journal entry, with the associated transactions, must be created each time a quantity of CompoundItems is created from its constituent Ingredients.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="domain"></param>
        /// <param name="transferIn"></param>
        /// <param name="transferOut"></param>
        /// <param name="saleRef"></param>
        /// <param name="manuJob"></param>
        public void AddManufacturingJournalEntry(ApplicationUser currentUser, QbicleDomain domain, TraderTransfer transferIn, TraderTransfer transferOut, TraderSale saleRef = null, ManuJob manuJob = null)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "AddManufacturingJournalEntry", currentUser.Id, null, transferIn, transferOut, saleRef, manuJob);

                    var associatedAccounts = new List<BKAccount>();
                    BKAccount inventoryAccount;
                    decimal quantity;
                    decimal cost;
                    var memo = "Manufacturing"; var reference = "";

                    var journalGroup = GetJournalGroup(domain.Id);
                    if (ConfigManager.LoggingDebugSet && journalGroup == null)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create New Manufactoring Journal Entry WITHOUT(MISSING) A GROUP", null, null, currentUser.Id, domain.Id);

                    var journalEntry = new JournalEntry
                    {
                        WorkGroup = null,
                        Group = journalGroup,
                        BKTransactions = new List<BKTransaction>(),
                        IsApproved = true, //No ApprovalReq is required - set IsApproved to True
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Description = "Manufacturing",
                        Number = 0,//Auto generate Number from the trigger_setNumberForJournalEntry trigger
                        Domain = domain,
                        PostedDate = DateTime.UtcNow,
                        AssociatedAccounts = new List<BKAccount>(),
                    };
                    BKAccount purchaseAccount;

                    transferOut.TransferItems.ForEach(transfer =>
                    {
                        inventoryAccount = transfer.TraderItem.InventoryAccount;
                        purchaseAccount = transfer.TraderItem.PurchaseAccount;
                        quantity = transfer.QuantityAtPickup == 0 ? 1 : transfer.QuantityAtPickup;
                        if (transfer.InventoryBatches != null)
                            cost = transfer.InventoryBatches.Sum(e => e.CostPerUnit) * transfer.Unit.QuantityOfBaseunit *
                                   quantity;
                        else
                            cost = 0;
                        if (saleRef != null)
                        {
                            memo = $"Manufacturing for Sale: {saleRef.Reference?.FullRef} / Transfer: {transferOut.Id}";
                            reference = saleRef.Reference?.FullRef;
                        }
                        else if (manuJob != null)
                        {
                            memo = $"Manufacturing Job: {manuJob.Reference?.FullRef}";
                            reference = manuJob.Reference?.FullRef;
                        }

                        // Inventory transaction
                        NewBkTransactions(journalEntry, inventoryAccount, reference, memo, credit: cost, debit: null);


                        // Expense transaction
                        NewBkTransactions(journalEntry, purchaseAccount, reference, memo, credit: null, debit: cost);
                    });

                    transferIn.TransferItems.ForEach(transfer =>
                    {
                        inventoryAccount = transfer.TraderItem.InventoryAccount;
                        purchaseAccount = transfer.TraderItem.PurchaseAccount;
                        quantity = transfer.QuantityAtDelivery == 0 ? 1 : transfer.QuantityAtDelivery;
                        if (transfer.InventoryBatches != null)
                            cost = transfer.InventoryBatches.Sum(e => e.CostPerUnit) * transfer.Unit.QuantityOfBaseunit * quantity;
                        else
                            cost = 0;
                        if (saleRef != null)
                        {
                            memo = $"Manufacturing for Sale: {saleRef.Reference?.FullRef} / Transfer: {transferIn.Id}";
                            reference = saleRef.Reference?.FullRef;
                        }
                        else if (manuJob != null)
                        {
                            memo = $"Manufacturing Job: {manuJob.Id}";
                            reference = $"{manuJob.Id}";
                        }


                        // inventoryAccount
                        NewBkTransactions(journalEntry, inventoryAccount, reference, memo, credit: null, debit: cost);

                        // Cost of Production transaction
                        NewBkTransactions(journalEntry, purchaseAccount, reference, memo, credit: cost, debit: null);
                    });


                    //Set the journal entry description
                    journalEntry.Description = memo;

                    dbContext.JournalEntrys.Add(journalEntry);
                    dbContext.Entry(journalEntry).State = EntityState.Added;

                    //need to update the CoANodes
                    new BKCoANodesRule(dbContext).UpdateNodeBalanceFromJournal(journalEntry);

                    //Add Manufactoring Bookeeping log
                    var manuBookeepingJob = new ManufacturingBookkeepingLog()
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Domain = domain,
                        JournalEntry = journalEntry,
                        ManufacturingJob = manuJob,
                        TransferOut = transferOut,
                        TransferIn = transferIn,
                        TraderSale = saleRef
                    };
                    dbContext.ManufacturingBookkeepingLogs.Add(manuBookeepingJob);
                    dbContext.Entry(manuBookeepingJob).State = EntityState.Added;

                    dbContext.SaveChanges();
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUser.Id, transferIn, transferOut, saleRef, manuJob);
                }
            }
        }

        private void NewBkTransactions(JournalEntry journalEntry, BKAccount bkAccount, string reference, string memo, decimal? credit, decimal? debit)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "NewBkTransactions", null, null, journalEntry, bkAccount, reference, memo, credit, debit);

                if (ConfigManager.LoggingDebugSet && bkAccount == null)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, bkAccount, reference, memo, credit, debit);

                journalEntry.BKTransactions.Add(new BKTransaction
                {
                    Id = 0,
                    Reference = reference,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = journalEntry.CreatedBy,
                    PostedDate = journalEntry.PostedDate,
                    Account = bkAccount,
                    Credit = credit,
                    Debit = debit,
                    JournalEntry = journalEntry,
                    Memo = memo
                });

                if (journalEntry.AssociatedAccounts == null)
                    journalEntry.AssociatedAccounts = new List<BKAccount>();

                if (bkAccount != null && journalEntry.AssociatedAccounts.All(e => e.Id != bkAccount?.Id))
                    journalEntry.AssociatedAccounts.Add(bkAccount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalEntry, bkAccount, reference, memo, credit, debit);
                throw ex;
            }
        }


        /// <summary>
        /// Bookkeeping for Sales based on Invoice - QBIC-1882
        /// When the Invoice for a Sale is ‘Approved’ this event leads to a requirement to create a Journal Entry in Bookkeeping.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="invoice"></param>
        public void AddSaleInvoiceJournalEntry(ApplicationUser currentUser, Invoice invoice)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateBkTransactionOrderTaxFromSaleInvoice", currentUser.Id, null, invoice);

                    var domain = invoice.Workgroup.Domain;
                    var tradSale = invoice.Sale;
                    var saleFullRef = tradSale.Reference?.FullRef;
                    var invoiceFullRef = invoice.Reference?.FullRef;
                    var memoTax = $"Tax for Sale: {saleFullRef}  / Invoice:  {invoiceFullRef}";
                    var memo = $"Sale: {saleFullRef}  / Invoice:  {invoiceFullRef}";

                    var associatedAccounts = new List<BKAccount>();
                    var customerAccount = invoice.Sale.Purchaser.CustomerAccount;

                    var journalGroup = GetJournalGroup(domain.Id);
                    if (ConfigManager.LoggingDebugSet && journalGroup == null)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create New Sale Invoice Journal Entry WITHOUT(MISSING) A GROUP", null, null, currentUser.Id, domain.Id, saleFullRef, invoiceFullRef, memoTax, memo);

                    var journalEntry = new JournalEntry
                    {
                        WorkGroup = null,
                        Group = journalGroup,
                        BKTransactions = new List<BKTransaction>(),
                        IsApproved = true,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Description = memo, //"Journal Entry from Sale Invoice: " + invoiceFullRef,
                        Number = 0,//Auto generate Number from the trigger_setNumberForJournalEntry trigger
                        Domain = domain,
                        PostedDate = DateTime.UtcNow,
                        AssociatedAccounts = new List<BKAccount>()
                    };

                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), $"journalEntry creating ", currentUser.Id, journalEntry, invoice);


                    invoice.InvoiceItems.ForEach(invoiceTransactionItem =>
                    {
                        var dimension = invoiceTransactionItem.TransactionItem.Dimensions;

                        var salesAccount = invoiceTransactionItem.TransactionItem.TraderItem.SalesAccount;

                        var monetaryValue = invoiceTransactionItem.InvoiceValue;

                        // var quantity = invoiceTransactionItem.InvoiceItemQuantity;

                        //IF QUANTITY IS 0, SKIP THIS invoiceTransactionItem
                        if (invoiceTransactionItem.InvoiceItemQuantity == 0 || invoiceTransactionItem.TransactionItem.Quantity == 0)
                            return;


                        //From the Taxes (InvoiceTransactionItem.TraderTransactionItem.Taxes)

                        decimal creditTaxAccountedFor = 0;
                        decimal debitTaxAccountedFor = 0;

                        invoiceTransactionItem.TransactionItem.Taxes.ForEach(orderTax =>
                        {
                            //Calculate the TaxAmount based on the quantitiy o fthe item in the invoice transation
                            var taxAmount = orderTax.Value * invoiceTransactionItem.InvoiceItemQuantity;
                            //Check if the Tax is to be accounted for separately
                            if (!orderTax.StaticTaxRate.IsAccounted) return;

                            CreateBkTransactionOrderTaxFromSaleInvoice(taxAmount, orderTax, journalEntry, saleFullRef, memoTax, dimension);

                            if (orderTax.StaticTaxRate.IsCreditToTaxAccount)
                                creditTaxAccountedFor += taxAmount;
                            else
                                debitTaxAccountedFor += taxAmount;

                        });

                        //If the tax is accounted for then we must remove that tax from the value of the Transaction
                        // Cost for a Purchase is a Gross Price including tax
                        var transactionValue = monetaryValue - (creditTaxAccountedFor + debitTaxAccountedFor);

                        //Create Customer Transaction (Debit)
                        if (ConfigManager.LoggingDebugSet && customerAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, null, saleFullRef, memo, 0, transactionValue);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Id = 0,
                            Reference = saleFullRef,
                            Debit = transactionValue,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = journalEntry.CreatedBy,
                            PostedDate = journalEntry.PostedDate,
                            Account = customerAccount,
                            JournalEntry = journalEntry,
                            Memo = memo,
                            Dimensions = dimension
                        });

                        if (customerAccount != null && associatedAccounts.All(e => e.Id != customerAccount.Id))
                            associatedAccounts.Add(customerAccount);

                        //If the Tax is credited to the tax account debit it from Customer
                        if (creditTaxAccountedFor > 0)
                            journalEntry.BKTransactions.Add(new BKTransaction
                            {
                                Id = 0,
                                Reference = saleFullRef,
                                Debit = creditTaxAccountedFor,   // The tax credited must be debited from the Customer
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = journalEntry.CreatedBy,
                                PostedDate = journalEntry.PostedDate,
                                Account = customerAccount,
                                JournalEntry = journalEntry,
                                Memo = memoTax,
                                Dimensions = dimension
                            });
                        if (customerAccount != null && associatedAccounts.All(e => e.Id != customerAccount.Id))
                            associatedAccounts.Add(customerAccount);

                        // If the Tax is debited from the tax account Credit it To Customer
                        if (debitTaxAccountedFor > 0)
                            journalEntry.BKTransactions.Add(new BKTransaction
                            {
                                Id = 0,
                                Reference = saleFullRef,
                                Credit = debitTaxAccountedFor,  // The tax debited must be credited to the customer
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = journalEntry.CreatedBy,
                                PostedDate = journalEntry.PostedDate,
                                Account = customerAccount,
                                JournalEntry = journalEntry,
                                Memo = memoTax,
                                Dimensions = dimension
                            });
                        if (customerAccount != null && associatedAccounts.All(e => e.Id != customerAccount.Id))
                            associatedAccounts.Add(customerAccount);

                        //Create Sales Account Transaction (Credit)
                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Id = 0,
                            Reference = saleFullRef,
                            Credit = transactionValue,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = journalEntry.CreatedBy,
                            PostedDate = journalEntry.PostedDate,
                            Account = salesAccount,
                            JournalEntry = journalEntry,
                            Memo = memo,
                            Dimensions = dimension
                        });

                        if (salesAccount != null && associatedAccounts.All(e => e.Id != salesAccount.Id))
                            associatedAccounts.Add(salesAccount);

                    });



                    journalEntry.AssociatedAccounts.AddRange(associatedAccounts);

                    dbContext.JournalEntrys.Add(journalEntry);
                    dbContext.Entry(journalEntry).State = EntityState.Added;

                    //Add new Sale Invoice Bookeeping Log
                    var saleInvoiceLog = new SaleInvoiceBookkeepingLog()
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Domain = domain,
                        Invoice = invoice,
                        JournalEntry = journalEntry
                    };
                    dbContext.SaleInvoiceBookkeepingLogs.Add(saleInvoiceLog);
                    dbContext.Entry(saleInvoiceLog).State = EntityState.Added;

                    dbContext.SaveChanges();
                    dbTransaction.Commit();

                    //need to update the CoANodes
                    new BKCoANodesRule(dbContext).UpdateNodeBalanceFromJournal(journalEntry);
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUser.Id, invoice);
                }
            }
        }

        private void CreateBkTransactionOrderTaxFromSaleInvoice(decimal taxAmount, OrderTax orderTax, JournalEntry journalEntry, string reference, string memo, List<TransactionDimension> dimensions)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateBkTransactionOrderTaxFromSaleInvoice", null, null, taxAmount, orderTax, journalEntry, reference, memo, dimensions);


                if (ConfigManager.LoggingDebugSet && orderTax.StaticTaxRate?.AssociatedAccount == null)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, reference);

                var bkTransaction = new BKTransaction
                {
                    Id = 0,
                    Reference = reference,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = journalEntry.CreatedBy,
                    PostedDate = journalEntry.PostedDate,
                    Account = orderTax.StaticTaxRate.AssociatedAccount,
                    JournalEntry = journalEntry,
                    Memo = memo,
                    Dimensions = dimensions
                };
                if (orderTax.StaticTaxRate.IsCreditToTaxAccount)
                    bkTransaction.Credit = taxAmount;
                else
                    bkTransaction.Debit = taxAmount;

                journalEntry.BKTransactions.Add(bkTransaction);

                if (bkTransaction?.Account != null && journalEntry.AssociatedAccounts.All(e => e.Id != bkTransaction.Account.Id))
                    journalEntry.AssociatedAccounts.Add(bkTransaction.Account);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxAmount, orderTax, journalEntry, reference, memo, dimensions);
                throw ex;
            }
        }



        /// <summary>
        /// Bookkeeping Integration for Sales Transfer - QBIC-1872
        /// The Journal Entries for Sales Transfers are to be created after the Transfers for Sales have been saved to the database when the status of the Transfer is 'Picked Up'
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="transfer"></param>
        public void AddSaleTransferJournalEntry(ApplicationUser currentUser, TraderTransfer transfer, string type)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateBkTransactionFromSaleTransfer", currentUser.Id, null, transfer);


                    var domain = transfer.Workgroup.Domain;
                    var tradSale = transfer.Sale;
                    var locationId = tradSale.Location.Id;

                    var journalGroup = GetJournalGroup(domain.Id);
                    if (ConfigManager.LoggingDebugSet && journalGroup == null)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create New Sale Transfer Journal Entry WITHOUT(MISSING) A GROUP", null, null, currentUser.Id, domain.Id, locationId, tradSale.Id);

                    var journalEntry = new JournalEntry
                    {
                        WorkGroup = null,
                        Group = journalGroup,
                        BKTransactions = new List<BKTransaction>(),
                        IsApproved = true,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Description = $"Transfer for {type} Sale: " + transfer.Sale.Reference.FullRef,
                        Number = 0,//Auto generate Number from the trigger_setNumberForJournalEntry trigger
                        Domain = domain,
                        PostedDate = DateTime.UtcNow,
                        AssociatedAccounts = new List<BKAccount>(),
                    };

                    decimal cost = 0;


                    var salesRef = tradSale.Reference?.FullRef;
                    var memo = $"Sale: {salesRef}  / transfer:  {transfer.Id}";

                    transfer.TransferItems.ForEach(traderTransferItem =>
                    {
                        //If the TraderItem in the TraderTransferItem is an Inventory item i.e. if the TraderTransferItem.TraderItem has an InventoryDetail at the Location
                        //then we treat the item as an inventory item
                        if (traderTransferItem.TraderItem.InventoryDetails.All(e => e.Location.Id != locationId)) return;

                        var inventoryAccount = traderTransferItem.TraderItem.InventoryAccount;
                        var cosAccount = traderTransferItem.TraderItem.PurchaseAccount;
                        var dimensions = traderTransferItem.TransactionItem.Dimensions;
                        var quantity = traderTransferItem.QuantityAtPickup == 0 ? 1 : traderTransferItem.QuantityAtPickup;

                        if (traderTransferItem.InventoryBatches != null)
                            cost = traderTransferItem.InventoryBatches.Sum(e => e.CostPerUnit) * traderTransferItem.Unit.QuantityOfBaseunit * quantity;

                        //Create a new BKTransaction For Inventory Account
                        CreateBkTransactionFromSaleTransfer(journalEntry, inventoryAccount, salesRef, memo, dimensions, cost, null);

                        //Create a new BKTransaction For Cost of Goods Account
                        CreateBkTransactionFromSaleTransfer(journalEntry, cosAccount, salesRef, memo, dimensions, null, cost);

                    });


                    // Check that there is at least ONE Transaction
                    // If there are no transactions do NOT save the journal Entry
                    if (journalEntry.BKTransactions.Count > 0)
                    {
                        dbContext.JournalEntrys.Add(journalEntry);
                        dbContext.Entry(journalEntry).State = EntityState.Added;

                        //Add Sale Transfer Bookeeping Log
                        var saleTransferBkLog = new SaleTransferBookkeepingLog()
                        {
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            Domain = domain,
                            JournalEntry = journalEntry,
                            SaleTransfer = transfer,
                            SaleTransferType = type
                        };
                        dbContext.SaleTransferBookkeepingLogs.Add(saleTransferBkLog);
                        dbContext.Entry(saleTransferBkLog).State = EntityState.Added;

                        dbContext.SaveChanges();
                        dbTransaction.Commit();

                        //need to update the CoANodes
                        new BKCoANodesRule(dbContext).UpdateNodeBalanceFromJournal(journalEntry);
                    }
                    else
                    {
                        dbTransaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUser.Id, transfer);
                }
            }
        }

        private void CreateBkTransactionFromSaleTransfer(JournalEntry journalEntry, BKAccount bkAccount, string reference, string memo, List<TransactionDimension> dimension, decimal? credit, decimal? debit)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateBkTransactionFromSaleTransfer", null, null, journalEntry, bkAccount, reference, memo, dimension, credit, debit);

                if (ConfigManager.LoggingDebugSet && bkAccount == null)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions From Sale Transfer WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, bkAccount, reference, memo, credit, debit);


                journalEntry.BKTransactions.Add(new BKTransaction
                {
                    Id = 0,
                    Reference = reference,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = journalEntry.CreatedBy,
                    PostedDate = journalEntry.PostedDate,
                    Account = bkAccount,
                    Credit = credit,
                    Debit = debit,
                    JournalEntry = journalEntry,
                    Memo = memo,
                    Dimensions = dimension
                });

                if (bkAccount != null && journalEntry.AssociatedAccounts.All(e => e.Id != bkAccount.Id))
                    journalEntry.AssociatedAccounts.Add(bkAccount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalEntry, bkAccount, reference, memo, dimension, credit, debit);
                throw ex;
            }
        }


        /// <summary>
        /// Bookkeeping Integration for Sales Transfer - QBIC-1874
        /// The Journal Entries for Sales Transfers are to be created after the Transfers for Sales have been saved to the database when the status of the Transfer is 'Picked Up'
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="transfer"></param>
        public void AddPurchaseInventoryJournalEntry(ApplicationUser currentUser, TraderTransfer transfer)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "AddPurchaseInventoryJournalEntry", currentUser.Id, null, transfer);

                    var domain = transfer.Workgroup.Domain;
                    var tradPurchase = transfer.Purchase;
                    var locationId = tradPurchase.Location.Id;
                    decimal cost = 0;
                    var associatedAccounts = new List<BKAccount>();
                    var vendorAccount = transfer.Purchase.Vendor.CustomerAccount;
                    var purchasesRef = tradPurchase.Reference?.FullRef;
                    var transferRef = transfer.Reference?.FullRef;

                    var memo = $"Purchase: {purchasesRef}  / Transfer:  {transferRef}";
                    var memoTax = $"Tax for Purchase: {purchasesRef}  / Transfer:  {transferRef}";

                    var journalGroup = GetJournalGroup(domain.Id);
                    if (ConfigManager.LoggingDebugSet && journalGroup == null)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create New Purchase Inventory Journal Entry WITHOUT(MISSING) A GROUP", null, null, currentUser.Id, domain.Id, locationId, purchasesRef, memoTax, memo);

                    var journalEntry = new JournalEntry
                    {
                        WorkGroup = null,
                        Group = journalGroup,
                        BKTransactions = new List<BKTransaction>(),
                        IsApproved = true,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Description = memo, // "Transfer for Purchase: " + purchasesRef,
                        Number = 0,//Auto generate Number from the trigger_setNumberForJournalEntry trigger
                        Domain = domain,
                        PostedDate = DateTime.UtcNow,
                        AssociatedAccounts = new List<BKAccount>(),
                    };



                    transfer.TransferItems.ForEach(traderTransferItem =>
                    {
                        //If the TraderItem in the TraderTransferItem is an Inventory item i.e. if the TraderTransferItem.TraderItem has an InventoryDetail at the Location
                        //then we treat the item as an inventory item
                        if (traderTransferItem.TraderItem.InventoryDetails.All(e => e.Location.Id != locationId)) return;

                       
                        var quantity = traderTransferItem.QuantityAtDelivery;
                        if (quantity == 0) return;

                        var inventoryAccount = traderTransferItem.TraderItem.InventoryAccount;

                        var dimensions = traderTransferItem.TransactionItem.Dimensions;

                        //Work out the cost per base unit of the purchase
                        var costPerBaseunit =
                            (
                                (traderTransferItem.TransactionItem.Cost / traderTransferItem.TransactionItem.Quantity)   // Cost per purchase unit including taxes
                            /
                                traderTransferItem.TransactionItem.Unit.QuantityOfBaseunit
                            );

                        //Work out the cost for the Transfer Item
                        var costOfTransferItem =
                            (
                                quantity
                                *
                                traderTransferItem.Unit.QuantityOfBaseunit
                                *
                                costPerBaseunit
                             );

                        cost = costOfTransferItem;



                        decimal creditTaxAccountedFor = 0;
                        decimal debitTaxAccountedFor = 0;


                        //For Each OrderTax in TraderTransactionItem.Taxes
                        traderTransferItem.TransactionItem.Taxes.ForEach(orderTax =>
                        {

                            //Calculate the TaxAmount
                            var taxAmount =
                                //The tax amount per base unit of the purchase transaction item
                                (
                                    orderTax.Value 
                                    / 
                                    traderTransferItem.TransactionItem.Unit.QuantityOfBaseunit
                                ) 
                                *
                                //quantity of base units in the transfer
                                (
                                    quantity
                                    *
                                    traderTransferItem.Unit.QuantityOfBaseunit
                                );

                            //Check if the Tax is to be accounted for separately
                            if (!orderTax.StaticTaxRate.IsAccounted) return;

                            CreateBkTransactionOrderTaxFromPurchaseInventory(taxAmount, orderTax, journalEntry, purchasesRef, memoTax, dimensions);

                            if (orderTax.StaticTaxRate.IsCreditToTaxAccount)
                                creditTaxAccountedFor += taxAmount;
                            else
                                debitTaxAccountedFor += taxAmount;

                        });

                        // Cost for a Purchase is a Gross Price including tax
                        var transactionValue = cost - (creditTaxAccountedFor + debitTaxAccountedFor);

                        //Create Customer Transaction
                        if (ConfigManager.LoggingDebugSet && vendorAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, vendorAccount, purchasesRef, memo, transactionValue, 0);
                        if (ConfigManager.LoggingDebugSet && inventoryAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, vendorAccount, purchasesRef, memo, transactionValue, 0);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Id = 0,
                            Reference = purchasesRef,
                            Credit = transactionValue,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = journalEntry.CreatedBy,
                            PostedDate = journalEntry.PostedDate,
                            Account = vendorAccount,
                            JournalEntry = journalEntry,
                            Memo = memo,
                            Dimensions = dimensions
                        });

                        if (vendorAccount != null && associatedAccounts.All(e => e.Id != vendorAccount.Id))
                            associatedAccounts.Add(vendorAccount);

                        //If the Tax is credited to the tax account debit it from Customer
                        if (creditTaxAccountedFor > 0)
                            journalEntry.BKTransactions.Add(new BKTransaction
                            {
                                Id = 0,
                                Reference = purchasesRef,
                                Debit = creditTaxAccountedFor,    // The tax credited must be debited from the Vendor
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = journalEntry.CreatedBy,
                                PostedDate = journalEntry.PostedDate,
                                Account = vendorAccount,
                                JournalEntry = journalEntry,
                                Memo = memoTax,
                                Dimensions = dimensions
                            });

                        if (vendorAccount != null && associatedAccounts.All(e => e.Id != vendorAccount.Id))
                            associatedAccounts.Add(vendorAccount);

                        // If the Tax is debited from the tax account Credit it To Customer
                        if (debitTaxAccountedFor > 0)
                            journalEntry.BKTransactions.Add(new BKTransaction
                            {
                                Id = 0,
                                Reference = purchasesRef,
                                Credit = debitTaxAccountedFor,  // The tax debited must be credited to the Vendor
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = journalEntry.CreatedBy,
                                PostedDate = journalEntry.PostedDate,
                                Account = vendorAccount,
                                JournalEntry = journalEntry,
                                Memo = memoTax,
                                Dimensions = dimensions
                            });

                        if (vendorAccount != null && associatedAccounts.All(e => e.Id != vendorAccount.Id))
                            associatedAccounts.Add(vendorAccount);

                        //Create Inventory Account Transaction
                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Id = 0,
                            Reference = purchasesRef,
                            Debit = transactionValue,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = journalEntry.CreatedBy,
                            PostedDate = journalEntry.PostedDate,
                            Account = inventoryAccount,
                            JournalEntry = journalEntry,
                            Memo = memo,
                            Dimensions = dimensions
                        });

                        if (inventoryAccount != null && associatedAccounts.All(e => e.Id != inventoryAccount.Id))
                            associatedAccounts.Add(inventoryAccount);
                    });



                    journalEntry.AssociatedAccounts.AddRange(associatedAccounts);


                    // Check that there is at least ONE Transaction
                    // If there are no transactions do NOT save the journal Entry
                    if (journalEntry.BKTransactions.Count > 0)
                    {

                        dbContext.JournalEntrys.Add(journalEntry);
                        dbContext.Entry(journalEntry).State = EntityState.Added;

                        //Add Purchase Inventory Bookeeping Log
                        var purchaseInventoryBkLog = new PurchaseInventoryBookkeepingLog()
                        {
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            Domain = domain,
                            JournalEntry = journalEntry,
                            PurchaseTransfer = transfer
                        };
                        dbContext.PurchaseInventoryBookkeepingLogs.Add(purchaseInventoryBkLog);
                        dbContext.Entry(purchaseInventoryBkLog).State = EntityState.Added;

                        dbContext.SaveChanges();
                        dbTransaction.Commit();

                        //need to update the CoANodes
                        new BKCoANodesRule(dbContext).UpdateNodeBalanceFromJournal(journalEntry);
                    }
                    else
                    {
                        dbTransaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUser.Id, transfer);
                }
            }
        }

        private void CreateBkTransactionOrderTaxFromPurchaseInventory(decimal taxAmount, OrderTax orderTax, JournalEntry journalEntry, string reference, string memo, List<TransactionDimension> dimensions)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create BkTransaction Order Tax From Purchase Inventory", null, null, taxAmount, orderTax, journalEntry, reference, memo, dimensions);

                if (ConfigManager.LoggingDebugSet && orderTax.StaticTaxRate?.AssociatedAccount == null)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, orderTax, reference, memo);

                var bkTransaction = new BKTransaction
                {
                    Id = 0,
                    Reference = reference,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = journalEntry.CreatedBy,
                    PostedDate = journalEntry.PostedDate,
                    Account = orderTax.StaticTaxRate.AssociatedAccount,
                    JournalEntry = journalEntry,
                    Memo = memo,
                    Dimensions = dimensions
                };

                if (orderTax.StaticTaxRate.IsCreditToTaxAccount)
                    bkTransaction.Credit = taxAmount;
                else
                    bkTransaction.Debit = taxAmount;

                journalEntry.BKTransactions.Add(bkTransaction);

                if (bkTransaction?.Account != null && journalEntry.AssociatedAccounts.All(e => e.Id != bkTransaction.Account.Id))
                    journalEntry.AssociatedAccounts.Add(bkTransaction.Account);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxAmount, orderTax, journalEntry, reference, memo, dimensions);
                throw ex;
            }
        }


        /// <summary>
        /// Bookkeeping for Sales based on Invoice - QBIC-1881
        /// When the Bill for a Purchase is ‘Approved’ this event leads to a requirement to create a Journal Entry in Bookkeeping.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="bill"></param>
        public void AddPurchaseNonInventoryJournalEntry(ApplicationUser currentUser, Invoice bill)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Add Purchase Non Inventory Journal Entry", currentUser.Id, null, bill);


                    var domain = bill.Workgroup.Domain;
                    var tradPurchase = bill.Purchase;
                    var locationId = tradPurchase.Location.Id;
                    var invoiceFullRef = bill.Reference?.FullRef;

                    var purchasesRef = tradPurchase.Reference?.FullRef;
                    var memo = $"Purchase: {purchasesRef}  / Bill:  {invoiceFullRef}";
                    var memoTax = $"Tax for Purchase: {purchasesRef}  / Bill:  {invoiceFullRef}";


                    var vendorAccount = bill.Purchase.Vendor.CustomerAccount;

                    var associatedAccounts = new List<BKAccount>();

                    var journalGroup = GetJournalGroup(domain.Id);
                    if (ConfigManager.LoggingDebugSet && journalGroup == null)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create New Purchase Non Inventory Journal Entry WITHOUT(MISSING) A GROUP", null, null, currentUser.Id, domain.Id, locationId, purchasesRef, invoiceFullRef, memoTax, memo);

                    var journalEntry = new JournalEntry
                    {
                        WorkGroup = null,
                        Group = journalGroup,
                        BKTransactions = new List<BKTransaction>(),
                        IsApproved = true,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Description = memo, // "Journal Entry from Purchase Bill: " + invoiceFullRef,
                        Number = 0,//Auto generate Number from the trigger_setNumberForJournalEntry trigger
                        Domain = domain,
                        PostedDate = DateTime.UtcNow,
                        AssociatedAccounts = new List<BKAccount>()
                    };


                    bill.InvoiceItems.ForEach(billTransactionItem =>
                    {
                        //If the TraderItem in the InvoiceTransactionItem.TransactionItem is NOT an Inventory item
                        //i.e. if the InvoiceTransaction.ItemTransactionItem.TraderItem DOES NOT HAVE an InventoryDetail at the Location then we treat the item as a non-inventory item

                        if (billTransactionItem.TransactionItem.TraderItem.InventoryDetails.Any(e => e.Location.Id == locationId)) return;


                        //IF QUANTITY IS 0, SKIP THIS invoiceTransactionItem
                        if (billTransactionItem.TransactionItem.Quantity == 0) return;

                        var dimension = billTransactionItem.TransactionItem.Dimensions;

                        var expenseAccount = billTransactionItem.TransactionItem.TraderItem.PurchaseAccount;
                        //var quantity = invoiceTransactionItem.InvoiceItemQuantity;

                        var monetaryValue = billTransactionItem.InvoiceValue;

                        





                        //From the Taxes (InvoiceTransactionItem.TraderTransactionItem.Taxes)

                        decimal creditTaxAccountedFor = 0;
                        decimal debitTaxAccountedFor = 0;

                        billTransactionItem.TransactionItem.Taxes.ForEach(orderTax =>
                        {
                            //Calculate the TaxAmount based on the ratio of quantities between 
                            //     Invoice and Transaction

                            var taxAmount = orderTax.Value * billTransactionItem.InvoiceItemQuantity;
                            //Check if the Tax is to be accounted for separately
                            if (!orderTax.StaticTaxRate.IsAccounted) return;

                            CreateBkTransactionOrderTaxFromPurchaseNonInventory(taxAmount, orderTax, journalEntry, purchasesRef, memoTax, dimension);

                            if (orderTax.StaticTaxRate.IsCreditToTaxAccount)
                                creditTaxAccountedFor += taxAmount;
                            else
                                debitTaxAccountedFor += taxAmount;

                        });

                        //If the tax is accounted for then we must remove that tax from the value of the Transaction
                        // Cost for a Purchase is a Gross Price including tax
                        var transactionValue = monetaryValue - (creditTaxAccountedFor + debitTaxAccountedFor);

                        //Create Vendor Transaction (Credit)
                        if (ConfigManager.LoggingDebugSet && vendorAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, purchasesRef);
                        if (ConfigManager.LoggingDebugSet && expenseAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, purchasesRef);



                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Id = 0,
                            Reference = purchasesRef,
                            Debit = transactionValue,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = journalEntry.CreatedBy,
                            PostedDate = journalEntry.PostedDate,
                            Account = vendorAccount,
                            JournalEntry = journalEntry,
                            Memo = memo,
                            Dimensions = dimension
                        });

                        if (vendorAccount != null && associatedAccounts.All(e => e.Id != vendorAccount.Id))
                            associatedAccounts.Add(vendorAccount);

                        //If the Tax is credited to the tax account debit it from Vendor
                        if (creditTaxAccountedFor > 0)
                            journalEntry.BKTransactions.Add(new BKTransaction
                            {
                                Id = 0,
                                Reference = purchasesRef,
                                Debit = debitTaxAccountedFor,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = journalEntry.CreatedBy,
                                PostedDate = journalEntry.PostedDate,
                                Account = vendorAccount,
                                JournalEntry = journalEntry,
                                Memo = memoTax,
                                Dimensions = dimension
                            });
                        if (vendorAccount != null && associatedAccounts.All(e => e.Id != vendorAccount.Id))
                            associatedAccounts.Add(vendorAccount);

                        // If the Tax is debited from the tax account Credit it To Vendor
                        if (debitTaxAccountedFor > 0)
                            journalEntry.BKTransactions.Add(new BKTransaction
                            {
                                Id = 0,
                                Reference = purchasesRef,
                                Credit = creditTaxAccountedFor,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = journalEntry.CreatedBy,
                                PostedDate = journalEntry.PostedDate,
                                Account = vendorAccount,
                                JournalEntry = journalEntry,
                                Memo = memoTax,
                                Dimensions = dimension
                            });
                        if (vendorAccount != null && associatedAccounts.All(e => e.Id != vendorAccount.Id))
                            associatedAccounts.Add(vendorAccount);

                        //Debit Expense (Purchase) Account Transaction
                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Id = 0,
                            Reference = purchasesRef,
                            Credit = transactionValue,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = journalEntry.CreatedBy,
                            PostedDate = journalEntry.PostedDate,
                            Account = expenseAccount,
                            JournalEntry = journalEntry,
                            Memo = memo,
                            Dimensions = dimension
                        });

                        if (expenseAccount != null && associatedAccounts.All(e => e.Id != expenseAccount.Id))
                            associatedAccounts.Add(expenseAccount);

                    });


                    journalEntry.AssociatedAccounts.AddRange(associatedAccounts);


                    // Check that there is at least ONE Transaction
                    // If there are no transactions do NOT save the journal Entry
                    if (journalEntry.BKTransactions.Count > 0)
                    {
                        dbContext.JournalEntrys.Add(journalEntry);
                        dbContext.Entry(journalEntry).State = EntityState.Added;

                        //Add Purchase Non-Inventory Bookeeping Log
                        var purchaseNonInvBkLog = new PurchaseNonInvBookkeepingLog()
                        {
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            Domain = domain,
                            JournalEntry = journalEntry,
                            Bill = bill
                        };
                        dbContext.PurchaseNoninvBookkeepingLogs.Add(purchaseNonInvBkLog);
                        dbContext.Entry(purchaseNonInvBkLog).State = EntityState.Added;

                        dbContext.SaveChanges();
                        dbTransaction.Commit();

                        //need to update the CoANodes
                        new BKCoANodesRule(dbContext).UpdateNodeBalanceFromJournal(journalEntry);
                    }
                    else
                    {
                        dbTransaction.Rollback();
                    }

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUser.Id, bill);
                }
            }
        }

        private void CreateBkTransactionOrderTaxFromPurchaseNonInventory(decimal taxAmount, OrderTax orderTax, JournalEntry journalEntry, string reference, string memo, List<TransactionDimension> dimensions)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create BkTransaction Order Tax From Purchase Non Inventory", null, null, taxAmount, orderTax, journalEntry, reference, memo, dimensions);

                if (ConfigManager.LoggingDebugSet && orderTax.StaticTaxRate?.AssociatedAccount == null)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, orderTax, reference, memo);



                if (ConfigManager.LoggingDebugSet && orderTax.StaticTaxRate?.AssociatedAccount == null)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, reference);

                var bkTransaction = new BKTransaction
                {
                    Id = 0,
                    Reference = reference,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = journalEntry.CreatedBy,
                    PostedDate = journalEntry.PostedDate,
                    Account = orderTax.StaticTaxRate.AssociatedAccount,
                    JournalEntry = journalEntry,
                    Memo = memo,
                    Dimensions = dimensions
                };
                if (orderTax.StaticTaxRate.IsCreditToTaxAccount)
                    bkTransaction.Credit = taxAmount;
                else
                    bkTransaction.Debit = taxAmount;

                journalEntry.BKTransactions.Add(bkTransaction);
                if (bkTransaction != null && journalEntry.AssociatedAccounts.All(e => e.Id != bkTransaction.Account.Id))
                    journalEntry.AssociatedAccounts.Add(bkTransaction.Account);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taxAmount, orderTax, journalEntry, reference, memo, dimensions);
                throw ex;
            }
        }

        /// <summary>
        /// Add in Bookkeeping Integration for Cash Account Payments - QBIC-1902
        /// Integration Bookkeeping with Trader will require that Journal Entries for payments (CashAccountTransactions) are created.
        /// The Journal Entries for Payments are to be created after the Payment has been approved.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="payment"></param>
        public void AddPaymentJournalEntry(ApplicationUser currentUser, CashAccountTransaction payment)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "AddPaymentJournalEntry", currentUser.Id, null, payment);

                    var domain = payment.Workgroup.Domain;

                    var associatedAccounts = new List<BKAccount>();

                    var journalGroup = GetJournalGroup(domain.Id);
                    if (ConfigManager.LoggingDebugSet && journalGroup == null)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create New Payment Journal Entry WITHOUT(MISSING) A GROUP", null, null, currentUser.Id, domain.Id, payment);

                    var journalEntry = new JournalEntry
                    {
                        WorkGroup = null,
                        Group = journalGroup,
                        BKTransactions = new List<BKTransaction>(),
                        IsApproved = true,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Description = "Journal Entry from Payment : " + payment.Reference,
                        Number = 0,//Auto generate Number from the trigger_setNumberForJournalEntry trigger
                        Domain = domain,
                        PostedDate = DateTime.UtcNow,
                        AssociatedAccounts = new List<BKAccount>()
                    };
                    //Case 1: If we have CashAccountTransaction.OriginatingAccount and CashAccountTransaction.DestinationAccount
                    if (payment.OriginatingAccount != null && payment.DestinationAccount != null)
                    {
                        //Debit account

                        if (ConfigManager.LoggingDebugSet && payment.OriginatingAccount?.AssociatedBKAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, payment.Reference);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Account = payment.OriginatingAccount.AssociatedBKAccount,
                            Credit = payment.Amount,
                            Reference = payment.Reference,
                            Dimensions = null,
                            PostedDate = journalEntry.PostedDate,
                            Memo = "Payment: " + payment.Reference + " / " + payment.Description
                        });

                        if (associatedAccounts.All(e => e.Id != payment.OriginatingAccount.AssociatedBKAccount.Id))
                            associatedAccounts.Add(payment.OriginatingAccount.AssociatedBKAccount);

                        //Credit account

                        if (ConfigManager.LoggingDebugSet && payment.DestinationAccount?.AssociatedBKAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, payment.Reference);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Account = payment.DestinationAccount.AssociatedBKAccount,
                            Debit = payment.Amount,
                            Reference = payment.Reference,
                            Dimensions = null,
                            PostedDate = journalEntry.PostedDate,
                            Memo = "Payment: " + payment.Reference + " / " + payment.Description
                        });

                        if (associatedAccounts.All(e => e.Id != payment.DestinationAccount.AssociatedBKAccount.Id))
                            associatedAccounts.Add(payment.DestinationAccount.AssociatedBKAccount);
                    }
                    //Case 2: If we have CashAccountTransaction.OriginatingAccount and CashAccountTransaction.Contact
                    if (payment.OriginatingAccount != null && payment.Contact != null)
                    {
                        //Debit account

                        if (ConfigManager.LoggingDebugSet && payment.OriginatingAccount?.AssociatedBKAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, payment.Reference);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Account = payment.OriginatingAccount.AssociatedBKAccount,
                            Credit = payment.Amount,
                            Reference = payment.Reference,
                            Dimensions = null,
                            PostedDate = journalEntry.PostedDate,
                            Memo = "Payment: " + payment.Reference + " / " + payment.Description
                        });

                        if (associatedAccounts.All(e => e.Id != payment.OriginatingAccount.AssociatedBKAccount.Id))
                            associatedAccounts.Add(payment.OriginatingAccount.AssociatedBKAccount);
                        //Credit account

                        if (ConfigManager.LoggingDebugSet && payment.Contact?.CustomerAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, payment.Reference);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Account = payment.Contact.CustomerAccount,
                            Debit = payment.Amount,
                            Reference = payment.Reference,
                            Dimensions = null,
                            PostedDate = journalEntry.PostedDate,
                            Memo = "Payment: " + payment.Reference + " / " + payment.Description
                        });
                        if (payment.Contact.CustomerAccount != null && associatedAccounts.All(e => e.Id != payment.Contact.CustomerAccount.Id))
                            associatedAccounts.Add(payment.Contact.CustomerAccount);
                    }
                    //Case 3: If we have CashAccountTransaction.DestinationAccount and CashAccountTransaction.Contact
                    if (payment.DestinationAccount != null && payment.Contact != null)
                    {
                        //Debit account

                        if (ConfigManager.LoggingDebugSet && payment.Contact?.CustomerAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, payment.Reference);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Account = payment.Contact.CustomerAccount,
                            Credit = payment.Amount,
                            Reference = payment.Reference,
                            Dimensions = null,
                            PostedDate = journalEntry.PostedDate,
                            Memo = "Payment: " + payment.Reference + " / " + payment.Description
                        });

                        if (payment.Contact.CustomerAccount != null && associatedAccounts.All(e => e.Id != payment.Contact.CustomerAccount.Id))
                            associatedAccounts.Add(payment.Contact.CustomerAccount);
                        //Credit account

                        if (ConfigManager.LoggingDebugSet && payment.DestinationAccount?.AssociatedBKAccount == null)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Add New Bk Transactions WITHOUT(MISSING) AN ACCOUNT", null, null, journalEntry, payment.Reference);

                        journalEntry.BKTransactions.Add(new BKTransaction
                        {
                            Account = payment.DestinationAccount.AssociatedBKAccount,
                            Debit = payment.Amount,
                            Reference = payment.Reference,
                            Dimensions = null,
                            PostedDate = journalEntry.PostedDate,
                            Memo = "Payment: " + payment.Reference + " / " + payment.Description
                        });
                        if (payment.DestinationAccount.AssociatedBKAccount != null && associatedAccounts.All(e => e.Id != payment.DestinationAccount.AssociatedBKAccount.Id))
                            associatedAccounts.Add(payment.DestinationAccount.AssociatedBKAccount);
                    }

                    journalEntry.AssociatedAccounts.AddRange(associatedAccounts);


                    // Check that there is at least ONE Transaction
                    // If there are no transactions do NOT save the journal Entry
                    if (journalEntry.BKTransactions.Count > 0)
                    {
                        dbContext.JournalEntrys.Add(journalEntry);
                        dbContext.Entry(journalEntry).State = EntityState.Added;

                        //Add Payment Bookeeping Log
                        var paymentBkLog = new PaymentBookkeepingLog()
                        {
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            Domain = domain,
                            JournalEntry = journalEntry,
                            Payment = payment
                        };
                        dbContext.PaymentBookkeepingLogs.Add(paymentBkLog);
                        dbContext.Entry(paymentBkLog).State = EntityState.Added;

                        dbContext.SaveChanges();
                        dbTransaction.Commit();

                        //need to update the CoANodes
                        new BKCoANodesRule(dbContext).UpdateNodeBalanceFromJournal(journalEntry);
                    }
                    else
                    {
                        dbTransaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUser.Id, payment);
                }
            }
        }
    }
}
