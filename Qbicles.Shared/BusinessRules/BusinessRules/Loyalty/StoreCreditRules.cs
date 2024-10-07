using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Payments;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.BusinessRules.Loyalty
{
    public class StoreCreditRules
    {

        private readonly ApplicationDbContext _dbContext;

        public StoreCreditRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// Get Or Create Store Credit Account
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="userId">If need create when doesn't existed then need send the userId</param>
        /// <returns></returns>
        public StoreCreditAccount GetOrCreateStoreCreditAccount(TraderContact contact, string userId = "")
        {
            var account = _dbContext.StoreCreditAccounts.FirstOrDefault(p => p.Contact.Id == contact.Id);
            if (account != null)
                return account;

            var user = _dbContext.QbicleUser.Find(userId);
            account = new StoreCreditAccount()
            {
                Contact = contact,
                CurrentBalance = 0,
                Transactions = new List<StoreCreditTransaction>(),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = user
            };

            _dbContext.StoreCreditAccounts.Add(account);
            _dbContext.Entry(account).State = EntityState.Added;
            _dbContext.SaveChanges();

            return account;
        }

        public StoreCreditExchangeModel GetStoreCreditPointExchangeInfo(string contactKey, string userId, CurrencySetting currencySettings)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactKey);

                var contact = new TraderContactRules(_dbContext).GetTraderContactByKey(contactKey);
                var creditAccount = GetOrCreateStoreCreditAccount(contact, userId);
                var conversion = new StorePointConversionRules(_dbContext).GetActiveSystemSettings();
                var pointAccount = new StorePointRules(_dbContext).GetOrCreateStorePointAccount(contact, userId);
                var domain = contact.ContactGroup.Domain;
                var businessProfile = domain.Id.BusinesProfile();

                var exchangeModel = new StoreCreditExchangeModel()
                {
                    AccountBalance = 0,
                    AccountBalanceString = ((decimal)0).ToCurrencySymbol(currencySettings),
                    ContactKey = contactKey,
                    StoreCreditAccountId = creditAccount.Id,
                    ExchangeRate = conversion?.PointConversionFactor ?? 0,
                    Point = Decimal.Round(pointAccount.CurrentBalance, 0),
                    StoreCredit = creditAccount.CurrentBalance,
                    StoreCreditString = creditAccount.CurrentBalance.ToCurrencySymbol(currencySettings),
                    LogoUri = businessProfile.LogoUri.ToDocumentUri(),
                    Name = businessProfile.BusinessName,
                    CurrencySymbol = currencySettings.CurrencySymbol,
                    DecimalPlace = currencySettings.DecimalPlace,
                    SymbolDisplay = currencySettings.SymbolDisplay
                };
                return exchangeModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactKey);
                return new StoreCreditExchangeModel();
            }
        }

        public StoreCreditExchangeModel GetAccountBalanceExchangeInfo(string contactKey, string userId, CurrencySetting currencySettings)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactKey);

                var contact = new TraderContactRules(_dbContext).GetTraderContactByKey(contactKey);
                var creditAccount = GetOrCreateStoreCreditAccount(contact, userId);
                //var conversion = new StorePointConversionRules(_dbContext).GetActiveSystemSettings();
                var domain = contact.ContactGroup.Domain;
                var domainId = domain.Id;
                var businessProfile = domain.Id.BusinesProfile();

                var accountBalance = new TraderContactRules(_dbContext).GetBalanceContact(contact.Id);

                var exchangeModel = new StoreCreditExchangeModel()
                {
                    AccountBalance = Decimal.Round(accountBalance, (int)currencySettings.DecimalPlace),
                    ContactKey = contactKey,
                    StoreCreditAccountId = creditAccount.Id,
                    ExchangeRate = 1,
                    Point = 0,
                    StoreCredit = creditAccount.CurrentBalance,
                    StoreCreditString = creditAccount.CurrentBalance.ToCurrencySymbol(currencySettings),
                    LogoUri = businessProfile.LogoUri.ToDocumentUri(),
                    Name = businessProfile.BusinessName,
                    AccountBalanceString = accountBalance.ToCurrencySymbol(currencySettings),
                    CurrencySymbol = currencySettings.CurrencySymbol,
                    DecimalPlace = currencySettings.DecimalPlace,
                    SymbolDisplay = currencySettings.SymbolDisplay
                };
                return exchangeModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactKey);
                return new StoreCreditExchangeModel();
            }
        }

        /// <summary>
        /// Exchange Contact Account Balance to Store Credit Balance 
        /// With Exchange rate 1:1
        /// </summary>
        /// <param name="contactKey"></param>
        /// <param name="domainId"></param>
        /// <param name="exchangeBalance"></param>
        /// <returns></returns>
        public ReturnJsonModel GenerateCreditFromContactBalance(string contactKey, decimal exchangeBalance, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactKey);

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var currentUser = _dbContext.QbicleUser.Find(userId);
                        var conversionRules = new StorePointConversionRules(_dbContext);
                        var contact = new TraderContactRules(_dbContext).GetTraderContactByKey(contactKey);
                        var domainId = contact.ContactGroup.Domain.Id;
                        var creditAccount = GetOrCreateStoreCreditAccount(contact, userId);
                        var sysSettings = conversionRules.GetActiveSystemSettings();
                        var dmLoyaltySetting = conversionRules.GetOrCreateDomainLoyaltySetting(domainId, userId);
                        var workgroup = dmLoyaltySetting.DebitProcessWorkGroup;
                        var accountBalance = new TraderContactRules(_dbContext).GetBalanceContact(contact.Id);

                        if (!dmLoyaltySetting.IsPaymentWithStoreCreditActive)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = "Store Credit Conversions are not allowed in the Domain."
                            };
                        }

                        //A validation check is made to ensure that the CreditNote Value is not greater than the customer's Account Balance
                        if (exchangeBalance > accountBalance)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = "The Contact\'s Account Balance is not enough to convert to Store credit"
                            };
                        }

                        if (dmLoyaltySetting.DebitProcessWorkGroup == null)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Work Group in Domain Loyalty Settings")
                            };
                        }


                        //creates a StoreCredit (derived class of StoreCreditTransaction)
                        var creditTransaction = new StoreCredit()
                        {
                            Type = StoreCreditTransactionType.Credit,
                            Reason = StoreCreditTransactionReason.GeneratedFromAccountBalance,
                            Amount = exchangeBalance,
                            Account = creditAccount,
                            SystemSettings = sysSettings,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = currentUser
                        };
                        _dbContext.StoreCredits.Add(creditTransaction);
                        _dbContext.Entry(creditTransaction).State = EntityState.Added;

                        //Create CreditNote
                        var creditNote = new CreditNote()
                        {
                            Contact = contact,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            FinalisedDate = DateTime.UtcNow,
                            LastUpdateDate = DateTime.UtcNow,
                            LastUpdatedBy = currentUser,
                            Reason = CreditNoteReason.DebitNote,
                            WorkGroup = dmLoyaltySetting.DebitProcessWorkGroup,
                            Value = exchangeBalance,
                            Status = CreditNoteStatus.Approved,
                            Reference = new TraderReferenceRules(_dbContext).GetNewReference(domainId, TraderReferenceType.DebitNote)
                        };

                        _dbContext.CreditNotes.Add(creditNote);
                        _dbContext.Entry(creditNote).State = EntityState.Added;

                        //Create Approval Request with Approved Status
                        var appDef = _dbContext.CreditNoteApprovalDefinitions.FirstOrDefault(p => p.CreditNoteWorkGroup.Id == creditNote.WorkGroup.Id);
                        var approvalProcess = new ApprovalReq()
                        {
                            ApprovalRequestDefinition = appDef,
                            Priority = ApprovalReq.ApprovalPriorityEnum.High,
                            RequestStatus = ApprovalReq.RequestStatusEnum.Approved,
                            Qbicle = workgroup.Qbicle,
                            Topic = workgroup.Topic,
                            State = QbicleActivity.ActivityStateEnum.Open,
                            StartedBy = currentUser,
                            ApprovalReqHistories = new List<ApprovalReqHistory>(),
                            StartedDate = DateTime.UtcNow,
                            TimeLineDate = DateTime.UtcNow,
                            Notes = "",
                            IsVisibleInQbicleDashboard = true,
                            Name = $"Debit Note Request for Conversion to Store Credit",
                            ActivityMembers = new List<ApplicationUser>(),
                            CreditNotes = new List<CreditNote>(),
                            ApprovedOrDeniedAppBy = currentUser
                        };
                        approvalProcess.ApprovalReqHistories.Add(new ApprovalReqHistory()
                        {
                            ApprovalReq = approvalProcess,
                            CreatedDate = DateTime.UtcNow,
                            RequestStatus = ApprovalReq.RequestStatusEnum.Approved,
                            UpdatedBy = currentUser
                        });

                        creditTransaction.DebitNote = creditNote;

                        creditNote.ApprovalProcess = approvalProcess;

                        approvalProcess.ActivityMembers.AddRange(workgroup.Members);
                        approvalProcess.CreditNotes.Add(creditNote);
                        _dbContext.ApprovalReqs.Add(approvalProcess);
                        _dbContext.Entry(approvalProcess).State = EntityState.Added;
                        _dbContext.SaveChanges();

                        // Update StoreCredit Account Balance
                        creditAccount.CurrentBalance += exchangeBalance;
                        _dbContext.Entry(creditAccount).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        GenerateStoreCreditPIN(userId, StoreCreditTransactionReason.GeneratedFromAccountBalance, contactKey);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactKey, exchangeBalance, userId);
                        return new ReturnJsonModel() { result = false, msg = ex.Message };
                    }
                }

                var activePin = GetActivePIN(userId);
                return new ReturnJsonModel() { result = true, Object = activePin.PIN };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactKey);
                return new ReturnJsonModel() { result = false, msg = ex.Message };
            }
        }

        /// <summary>
        /// Exchange Store Point to Store Credit Balance 
        /// </summary>
        /// <param name="contactKey"></param>
        /// <param name="domainId"></param>
        /// <param name="exchangeBalance"></param>
        /// <returns></returns>
        public ReturnJsonModel GenerateCreditFromStorePoint(string contactKey, decimal exchangePoint, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactKey);

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var currentUser = _dbContext.QbicleUser.Find(userId);
                        var conversionRules = new StorePointConversionRules(_dbContext);
                        var contact = new TraderContactRules(_dbContext).GetTraderContactByKey(contactKey);
                        var domainId = contact.ContactGroup.Domain.Id;
                        var creditAccount = GetOrCreateStoreCreditAccount(contact, userId);
                        var sysSettings = conversionRules.GetActiveSystemSettings();
                        var dmLoyaltySetting = conversionRules.GetOrCreateDomainLoyaltySetting(domainId, userId);
                        var workgroup = dmLoyaltySetting.DebitProcessWorkGroup;
                        var storepointAccount = new StorePointRules(_dbContext).GetOrCreateStorePointAccount(contact, userId);
                        var pointBalance = storepointAccount.CurrentBalance;

                        var amountReceived = exchangePoint * sysSettings.PointConversionFactor;

                        if (!dmLoyaltySetting.IsPaymentWithStoreCreditActive)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = "Store Credit Conversions are not allowed in the Domain."
                            };
                        }
                        //A validation check is made to ensure that the CreditNote Value is not greater than the customer's Account Balance
                        if (exchangePoint > pointBalance)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = "The Point Balance of StorePoint Account is not enough to convert to Store credit."
                            };
                        }

                        if (dmLoyaltySetting.DebitProcessWorkGroup == null)
                        {
                            return new ReturnJsonModel()
                            {
                                result = false,
                                msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Work Group in Domain Loyalty Settings")
                            };
                        }

                        //creates a StoreCredit (derived class of StoreCreditTransaction)
                        var creditTransaction = new StoreCredit()
                        {
                            Type = StoreCreditTransactionType.Credit,
                            Reason = StoreCreditTransactionReason.GeneratedFromStorePoints,
                            Amount = amountReceived,
                            Account = creditAccount,
                            SystemSettings = sysSettings,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = currentUser
                        };
                        _dbContext.StoreCredits.Add(creditTransaction);
                        _dbContext.Entry(creditTransaction).State = EntityState.Added;

                        //Create Point Transaction
                        var pointTransaction = new StorePointTransaction()
                        {
                            Account = storepointAccount,
                            ConversionUsed = null,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            Points = exchangePoint,
                            Reason = StorePointTransactionReason.ConvertedToStoreCredit,
                            Type = StorePointTransactionType.Debit
                        };
                        var pointTransactionLog = new StorePointTransactionLog()
                        {
                            Account = storepointAccount,
                            AssociatedStorePointTransaction = pointTransaction,
                            ConversionUsed = null,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            Points = exchangePoint,
                            Reason = StorePointTransactionReason.ConvertedToStoreCredit,
                            Type = StorePointTransactionType.Debit
                        };

                        _dbContext.StorePointTransactions.Add(pointTransaction);
                        _dbContext.Entry(pointTransaction).State = EntityState.Added;
                        _dbContext.StorePointTransactionLogs.Add(pointTransactionLog);
                        _dbContext.Entry(pointTransactionLog).State = EntityState.Added;

                        //Merge Point Transaction to StorePoint Account
                        var transactionToAccountReusult = new StorePointRules(_dbContext).MergePointTransactionToPointAccount(storepointAccount, pointTransaction);
                        if (transactionToAccountReusult.result == false)
                        {
                            transaction.Rollback();
                            return new ReturnJsonModel() { result = false, msg = transactionToAccountReusult.msg };
                        }
                        //Create Approval Request with Approved Status

                        creditTransaction.PointTransaction = pointTransaction;
                        _dbContext.SaveChanges();

                        // Update StoreCredit Account Balance
                        creditAccount.CurrentBalance += amountReceived;
                        _dbContext.Entry(creditAccount).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        GenerateStoreCreditPIN(userId, StoreCreditTransactionReason.GeneratedFromStorePoints, contactKey);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactKey, exchangePoint, userId);
                        return new ReturnJsonModel() { result = false, msg = ex.Message };
                    }
                }

                var activePin = GetActivePIN(userId);
                return new ReturnJsonModel() { result = true, Object = activePin.PIN };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactKey);
                return new ReturnJsonModel() { result = false, msg = ex.Message };
            }
        }

        public StoreCreditPIN GetActivePIN(string userId)
        {
            var currentPIN = _dbContext.StoreCreditPINs.FirstOrDefault(p => p.IsArchieved == false && p.AssociatedUser.Id == userId);
            return currentPIN;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="associatedUserId">
        /// Convert from Account Balance or StorePoint to StoreCredit use CurrentUser().Id
        /// User request use userId is currentUser().Id, and contactKey is null
        ///  Payment send contactKey = Payment.Contact.Key, userId = payment.Contact.QbicleUser.Id
        /// </param>
        /// <param name="domainId"> 
        /// Convert from Account Balance or StorePoint to StoreCredit use contact.ContactGroup.Domain.Id
        /// User request CurrentDomainId()
        /// Payment payment.Contact.ContactGroup.Domain.Id use Payment
        /// </param>
        /// <param name="createdReason"></param>
        /// <param name="contactKey"></param>
        /// <returns></returns>
        public ReturnJsonModel GenerateStoreCreditPIN(string associatedUserId, StoreCreditTransactionReason createdReason, string contactKey = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, createdReason, associatedUserId);

                var currentPIN = _dbContext.StoreCreditPINs.FirstOrDefault(p => p.IsArchieved == false && p.AssociatedUser.Id == associatedUserId);
                if (currentPIN != null)
                {
                    currentPIN.IsArchieved = true;
                    currentPIN.ArchivedDate = DateTime.UtcNow;
                    _dbContext.Entry(currentPIN).State = EntityState.Modified;
                }

                var currentUser = _dbContext.QbicleUser.Find(associatedUserId);
                var contact = string.IsNullOrEmpty(contactKey) ? null : new TraderContactRules(_dbContext).GetTraderContactByKey(contactKey);

                var newPIN = new StoreCreditPIN()
                {
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    AssociatedUser = currentUser,
                    CreatedByContact = contact,
                    IsArchieved = false,
                    PIN = new Random().Next(1, 9999).ToString("D4"),
                    Reason = createdReason
                };
                currentUser.StoreCreditPINs.Add(newPIN);

                _dbContext.Entry(currentUser).State = EntityState.Modified;
                _dbContext.StoreCreditPINs.Add(newPIN);
                _dbContext.Entry(newPIN).State = EntityState.Added;
                _dbContext.SaveChanges();
                new EmailRules(_dbContext).SendEmailStoreCreditPin(newPIN);
                return new ReturnJsonModel() { result = true, Object = newPIN.PIN };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, createdReason, associatedUserId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }
    }
}
