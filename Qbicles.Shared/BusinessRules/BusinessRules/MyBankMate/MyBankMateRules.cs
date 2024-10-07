using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.MyBankMate;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.MyBankMate
{
    public class MyBankMateRules
    {
        ApplicationDbContext dbContext;
        public MyBankMateRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public bool CheckBankmateAccountSetup(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId);

                return dbContext.TraderCashAccounts.Any(s => s.Domain.Id == domainId && s.BankmateType == Models.Trader.BankmateAccountType.Domain);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return false;
            }
        }
        public List<Bank> GetBanks()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null);
                return dbContext.Banks.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<Bank>();
            }
        }
        public ReturnJsonModel UpdateCashBankAccountTransactions(int[] ids, TraderPaymentStatusEnum status, string userId, int externalBankAccountId = 0, CashAccountTransactionTypeEnum type = 0)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, ids, status);
                foreach (var id in ids)
                {
                    var cashaccountrans = dbContext.CashAccountTransactions.Find(id);
                    if (cashaccountrans != null && cashaccountrans.Status == TraderPaymentStatusEnum.PendingApproval)
                    {
                        if (status == TraderPaymentStatusEnum.PaymentApproved && externalBankAccountId > 0)
                        {
                            if (type == CashAccountTransactionTypeEnum.PaymentIn)//Approve Funds In
                                cashaccountrans.OriginatingAccount = dbContext.ExternalBankAccounts.Find(externalBankAccountId);
                            else//Approve Funds out
                                cashaccountrans.DestinationAccount = dbContext.ExternalBankAccounts.Find(externalBankAccountId);
                        }
                        cashaccountrans.Status = status;
                        var paymentLog = new CashAccountTransactionLog
                        {
                            Amount = cashaccountrans.Amount,
                            DestinationAccount = cashaccountrans.DestinationAccount,
                            OriginatingAccount = cashaccountrans.OriginatingAccount,
                            Status = cashaccountrans.Status,
                            PaymentMethod = cashaccountrans.PaymentMethod,
                            Reference = cashaccountrans.Reference,
                            CreatedBy = cashaccountrans.CreatedBy,
                            CreatedDate = cashaccountrans.CreatedDate,
                            Description = cashaccountrans.Description,
                            AssociatedBKTransaction = cashaccountrans.AssociatedBKTransaction,
                            AssociatedFiles = cashaccountrans.AssociatedFiles,
                            AssociatedInvoice = cashaccountrans.AssociatedInvoice,
                            AssociatedPurchase = cashaccountrans.AssociatedPurchase,
                            AssociatedSale = cashaccountrans.AssociatedSale,
                            AssociatedTransaction = cashaccountrans,
                            Charges = cashaccountrans.Charges,
                            Contact = cashaccountrans.Contact,
                            Id = 0,
                            PaymentApprovalProcess = cashaccountrans.PaymentApprovalProcess,
                            Type = cashaccountrans.Type,
                            Workgroup = cashaccountrans.Workgroup
                        };

                        var paymentProcessLog = new PaymentProcessLog
                        {
                            AssociatedTransaction = cashaccountrans,
                            AssociatedCashAccountTransactionLog = paymentLog,
                            PaymentStatus = cashaccountrans.Status,
                            CreatedBy = dbContext.QbicleUser.Find(userId),
                            CreatedDate = DateTime.UtcNow
                        };

                        dbContext.PaymentProcessLogs.Add(paymentProcessLog);
                        dbContext.Entry(paymentProcessLog).State = System.Data.Entity.EntityState.Added;
                    }
                }

                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, ids, status);
            }
            return returnJson;
        }
        public BankmateTransactionsFilterModel GetParmatersForFilter(bool isPendingStatus = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, isPendingStatus);
                IQueryable<CashAccountTransaction> query = null;
                if (isPendingStatus)
                    query = dbContext.CashAccountTransactions.Where(
                    s => s.Status == TraderPaymentStatusEnum.PendingApproval
                    && ((s.OriginatingAccount.BankmateType == BankmateAccountType.Domain && s.DestinationAccount.BankmateType == BankmateAccountType.ExternalBank)
                    || (s.OriginatingAccount.BankmateType == BankmateAccountType.ExternalBank && s.DestinationAccount.BankmateType == BankmateAccountType.Domain))
                    );
                else
                    query = dbContext.CashAccountTransactions.Where(
                    s => s.Status != TraderPaymentStatusEnum.PendingApproval
                    && ((s.OriginatingAccount.BankmateType == BankmateAccountType.Domain && s.DestinationAccount.BankmateType == BankmateAccountType.ExternalBank)
                    || (s.OriginatingAccount.BankmateType == BankmateAccountType.ExternalBank && s.DestinationAccount.BankmateType == BankmateAccountType.Domain))
                    );
                BankmateTransactionsFilterModel filterModel = new BankmateTransactionsFilterModel();
                filterModel.Creators = query.Select(s => s.CreatedBy).Distinct().ToList();
                filterModel.ExternalBanks.AddRange(query.Select(s => s.OriginatingAccount).Distinct().ToList());
                filterModel.ExternalBanks.AddRange(query.Select(s => s.DestinationAccount).Distinct().ToList());
                filterModel.ExternalBanks = filterModel.ExternalBanks.Where(s => s.BankmateType == BankmateAccountType.ExternalBank).Distinct().ToList();
                return filterModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, isPendingStatus);
                return new BankmateTransactionsFilterModel();
            }
        }
        public ReturnJsonModel SaveBankmate(BankMateModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);
                if (dbContext.ExternalBankAccounts.Any(s => s.Domain.Id == model.Domain.Id && s.Name.ToLower() == model.accountName.ToLower()))
                {
                    returnJson.msg = ResourcesManager._L("ERROR_DATA_EXISTED", model.accountName);
                    return returnJson;
                }

                ExternalBankAccount externalBank = new ExternalBankAccount
                {
                    Domain = model.Domain,
                    CreatedBy = dbContext.QbicleUser.Find(userId),
                    CreatedDate = DateTime.UtcNow,
                    Name = model.accountName,
                    Bank = dbContext.Banks.Find(model.bankId)
                };
                externalBank.IBAN = model.IBAN;
                externalBank.NUBAN = model.NUBAN;
                externalBank.ImageUri = externalBank.Bank.LogoUri;
                externalBank.Address = new Models.Trader.TraderAddress
                {
                    AddressLine1 = model.address,
                    Phone = model.phone,
                    Country = new CountriesRules().GetCountryByName(model.countryCode),
                };
                dbContext.ExternalBankAccounts.Add(externalBank);
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
            }
            return returnJson;
        }
        public List<TraderCashAccountCustom> GetBankMateAccountCustomByDomain(int domainId, BankmateAccountType accountType, bool isBalanceCalculated)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var lstBankMateAccountCustoms = new List<TraderCashAccountCustom>();

                var lstBankMateAccounts = dbContext.TraderCashAccounts.Where(p => p.BankmateType == accountType
                                            && p.Domain.Id == domainId).ToList();

                foreach (TraderCashAccount accountItem in lstBankMateAccounts)
                {
                    var accountCustomItem = new TraderCashAccountCustom()
                    {
                        TraderCashAccountId = accountItem.Id,
                        AssociatedBKAccount = accountItem.AssociatedBKAccount,
                        BankmateType = accountItem.BankmateType,
                        Domain = accountItem.Domain,
                        Workgroup = accountItem.Workgroup,
                        Name = accountItem.Name
                    };

                    if (accountType == BankmateAccountType.Domain)
                    {
                        accountCustomItem.ImageUri = accountItem.Domain.LogoUri;
                    }
                    else if (accountType == BankmateAccountType.ExternalBank)
                    {
                        accountCustomItem.ImageUri = accountItem.ImageUri;
                        ExternalBankAccount _externalAccount = dbContext.ExternalBankAccounts.FirstOrDefault(p => p.Id == accountItem.Id);
                        if (_externalAccount != null)
                        {
                            accountCustomItem.NUBAN = _externalAccount.NUBAN;
                            accountCustomItem.IBAN = _externalAccount.IBAN;
                            accountCustomItem.Address = _externalAccount.Address;
                            accountCustomItem.PhoneNumber = _externalAccount.PhoneNumber;
                            accountCustomItem.Bank = _externalAccount.Bank;
                        }
                    }
                    else if (accountType == BankmateAccountType.Driver)
                    {
                        DriverBankmateAccount _driverAccount = dbContext.DriverBankmateAccounts.FirstOrDefault(p => p.Id == accountItem.Id);
                        accountCustomItem.ImageUri = _driverAccount.Driver?.User?.User?.ProfilePic ?? "";
                        accountCustomItem.AssociatedDriver = _driverAccount.Driver;
                    }

                    var lstInComeTransactions = dbContext.CashAccountTransactions.Where(p => p.DestinationAccount.Id == accountItem.Id
                                                    && p.Status == TraderPaymentStatusEnum.PaymentApproved);
                    var lstOutComeTransactions = dbContext.CashAccountTransactions.Where(p => p.OriginatingAccount.Id == accountItem.Id
                                                    && p.Status == TraderPaymentStatusEnum.PaymentApproved);

                    if (isBalanceCalculated)
                    {
                        decimal _inBalance = 0;
                        decimal _outBalance = 0;
                        foreach (var inComeTrans in lstInComeTransactions)
                        {
                            _inBalance += inComeTrans?.Amount ?? 0;
                        }
                        foreach (var outComeTrans in lstOutComeTransactions)
                        {
                            _outBalance += outComeTrans?.Amount ?? 0;
                        }

                        accountCustomItem.BalanceAmount = _inBalance - _outBalance;
                    }

                    lstBankMateAccountCustoms.Add(accountCustomItem);
                }

                return lstBankMateAccountCustoms;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<TraderCashAccountCustom>();
            }
        }
        public TraderCashAccountCustom GetBankMateAccountById(int accountId, bool isBalanceCalculated = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, accountId);

                var bankMateAccount = dbContext.TraderCashAccounts.FirstOrDefault(p => p.Id == accountId);
                if (bankMateAccount == null)
                    return null;

                var bankMateAccountCustom = new TraderCashAccountCustom()
                {
                    AssociatedBKAccount = bankMateAccount.AssociatedBKAccount,
                    BankmateType = bankMateAccount.BankmateType,
                    Domain = bankMateAccount.Domain,
                    ImageUri = bankMateAccount.ImageUri,
                    Name = bankMateAccount.Name,
                    TraderCashAccountId = bankMateAccount.Id,
                    Workgroup = bankMateAccount.Workgroup,
                    BalanceAmount = 0
                };

                var lstInComeTransactions = dbContext.CashAccountTransactions.Where(p => p.DestinationAccount.Id == bankMateAccount.Id
                                                    && p.Status == TraderPaymentStatusEnum.PaymentApproved);
                var lstOutComeTransactions = dbContext.CashAccountTransactions.Where(p => p.OriginatingAccount.Id == bankMateAccount.Id
                                                && p.Status == TraderPaymentStatusEnum.PaymentApproved);

                if (isBalanceCalculated)
                {
                    decimal _inBalance = 0;
                    decimal _outBalance = 0;
                    foreach (var inComeTrans in lstInComeTransactions)
                    {
                        _inBalance += inComeTrans?.Amount ?? 0;
                    }
                    foreach (var outComeTrans in lstOutComeTransactions)
                    {
                        _outBalance += outComeTrans?.Amount ?? 0;
                    }
                    bankMateAccountCustom.BalanceAmount = _inBalance - _outBalance;
                }

                return bankMateAccountCustom;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountId);
                return null;
            }
        }

        public List<CashAccountTransaction> GetListAccountTransactionByStatus(int accountId, List<TraderPaymentStatusEnum> lstTransactionStatuses)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, accountId, lstTransactionStatuses);

                var _lstTransactions = dbContext.CashAccountTransactions.Where(p => (p.DestinationAccount.Id == accountId || p.OriginatingAccount.Id == accountId)
                                            && lstTransactionStatuses.Contains(p.Status)).ToList();

                return _lstTransactions;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountId, lstTransactionStatuses);
                return new List<CashAccountTransaction>();
            }
        }

        public List<CashAccountTransaction> GetListTransactionForPagination(int accountId, List<TraderPaymentStatusEnum> lstTransactionStatuses,
            string daterangeString, string keysearch, string searchBankIdList, int showTypeId, string dateFormat, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, accountId, lstTransactionStatuses, daterangeString, keysearch,
                        searchBankIdList, showTypeId, dateFormat, timeZone);

                var lstTransactions = GetListAccountTransactionByStatus(accountId, lstTransactionStatuses);

                if (!String.IsNullOrEmpty(daterangeString))
                {
                    var startTime = DateTime.UtcNow;
                    var endTime = DateTime.UtcNow;

                    HelperClass.ConvertDaterangeFormat(daterangeString, dateFormat, timeZone, out startTime, out endTime, HelperClass.endDateAddedType.minute);
                    lstTransactions = lstTransactions.Where(p => p.CreatedDate >= startTime && p.CreatedDate < endTime).ToList();
                }

                if (!String.IsNullOrEmpty(keysearch))
                {
                    keysearch = keysearch.ToLower();
                    lstTransactions = lstTransactions.Where(c => c.Id.ToString().Contains(keysearch) || (c.DestinationAccount != null && !String.IsNullOrEmpty(c.DestinationAccount.Name)
                                        && c.DestinationAccount.Name.ToLower().Contains(keysearch)) || (c.Reference != null && c.Reference.ToLower().Contains(keysearch))
                                        || (c.OriginatingAccount != null && !String.IsNullOrEmpty(c.OriginatingAccount.Name)
                                        && c.OriginatingAccount.Name.ToLower().Contains(keysearch)) || c.PaymentMethod.Name.ToLower().Contains(keysearch)
                                        || c.PaymentMethod.Name.ToLower().Contains(keysearch) || c.Status.GetDescription().ToLower().Contains(keysearch)).ToList();
                }

                var searchBankIds = searchBankIdList.Split(',').ToList();
                if (searchBankIds.Count() > 0)
                {
                    var externalBanks = dbContext.TraderCashAccounts.Where(p => searchBankIds.Contains(p.Id.ToString()) && p.BankmateType == BankmateAccountType.ExternalBank).ToList();
                    if (externalBanks != null && externalBanks.Count() > 0)
                        lstTransactions = lstTransactions.Where(p => (p.DestinationAccount != null && searchBankIds.Contains(p.DestinationAccount.Id.ToString()))
                                                || (p.OriginatingAccount != null && searchBankIds.Contains(p.OriginatingAccount.Id.ToString()))).ToList();
                }

                //showType == 2 - Debit:  OriginatingAccount = to the currently selected account
                //showType == 1 - Crebit: DestinationAccount = to the currently selected account
                if (showTypeId == 1)
                {
                    lstTransactions = lstTransactions.Where(p => p.DestinationAccount != null && p.DestinationAccount.Id == accountId).ToList();
                }
                else if (showTypeId == 2)
                {
                    lstTransactions = lstTransactions.Where(p => p.OriginatingAccount != null && p.OriginatingAccount.Id == accountId).ToList();
                }

                //lstTransactions = lstTranskactions.Skip(skip).Take(takeNumber).ToList();
                return lstTransactions;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountId, lstTransactionStatuses, daterangeString, keysearch, searchBankIdList,
                    showTypeId, dateFormat, timeZone);
                return new List<CashAccountTransaction>();
            }
        }

        public ReturnJsonModel CreateDomainBankMateTransaction(decimal transactionAmount, TraderCashAccount originalAccount, TraderCashAccount destinationAccount,
            string reference, string paymentDescription, string userId, CashAccountTransactionTypeEnum transactionType, S3ObjectUploadModel s3ObjectUpload,
            int qbicleId)
        {
            var result = new ReturnJsonModel() { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, transactionAmount, originalAccount, destinationAccount, reference,
                        paymentDescription, userId, s3ObjectUpload, qbicleId);

                var paymentMethod = dbContext.PaymentMethods.FirstOrDefault(p => p.Name == "Electronic Transfer");
                var user = dbContext.QbicleUser.Find(userId);

                var newPayment = new CashAccountTransaction
                {
                    Amount = transactionAmount,
                    DestinationAccount = destinationAccount,
                    OriginatingAccount = originalAccount,
                    Status = TraderPaymentStatusEnum.PendingApproval,
                    PaymentMethod = paymentMethod,
                    Reference = reference,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user,
                    Description = paymentDescription,
                    Type = transactionType
                };

                //Start: Processing with file uploaded
                var uploadedFile = new QbicleMedia();
                if (s3ObjectUpload != null)
                {
                    var extension = HelperClass.GetFileExtension(s3ObjectUpload.FileName);

                    var fileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extension);

                    uploadedFile = new QbicleMedia()
                    {
                        FileType = fileType,
                        Name = s3ObjectUpload.FileName,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        App = QbicleActivity.ActivityApp.Bankmate
                        //TimeLineDate = DateTime.UtcNow,
                        //ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalActivity
                    };

                    var versionFile = new VersionedFile
                    {
                        Uri = s3ObjectUpload.FileKey,
                        FileSize = HelperClass.FileSize(int.Parse(s3ObjectUpload.FileSize)),
                        FileType = fileType
                    };

                    uploadedFile.VersionedFiles = new List<VersionedFile>() { versionFile };
                    var generalFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, qbicleId);
                    uploadedFile.MediaFolder = generalFolder;

                    newPayment.AssociatedFiles = new List<QbicleMedia>() { uploadedFile };
                }
                //End: processing with file uploaded

                dbContext.CashAccountTransactions.Add(newPayment);
                dbContext.Entry(newPayment).State = System.Data.Entity.EntityState.Added;
                dbContext.SaveChanges();

                var paymentLog = new CashAccountTransactionLog
                {
                    Amount = newPayment.Amount,
                    DestinationAccount = newPayment.DestinationAccount,
                    OriginatingAccount = newPayment.OriginatingAccount,
                    Status = newPayment.Status,
                    PaymentMethod = newPayment.PaymentMethod,
                    Reference = newPayment.Reference,
                    CreatedBy = newPayment.CreatedBy,
                    CreatedDate = newPayment.CreatedDate,
                    Description = newPayment.Description,
                    AssociatedBKTransaction = newPayment.AssociatedBKTransaction,
                    AssociatedFiles = newPayment.AssociatedFiles,
                    AssociatedInvoice = newPayment.AssociatedInvoice,
                    AssociatedPurchase = newPayment.AssociatedPurchase,
                    AssociatedSale = newPayment.AssociatedSale,
                    AssociatedTransaction = newPayment,
                    Charges = newPayment.Charges,
                    Contact = newPayment.Contact,
                    Id = 0,
                    PaymentApprovalProcess = newPayment.PaymentApprovalProcess,
                    Type = newPayment.Type,
                    Workgroup = newPayment.Workgroup
                };

                var paymentProcessLog = new PaymentProcessLog
                {
                    AssociatedTransaction = newPayment,
                    AssociatedCashAccountTransactionLog = paymentLog,
                    PaymentStatus = newPayment.Status,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.PaymentProcessLogs.Add(paymentProcessLog);
                dbContext.Entry(paymentProcessLog).State = System.Data.Entity.EntityState.Added;
                dbContext.SaveChanges();

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, transactionAmount, originalAccount, destinationAccount, reference, paymentDescription, userId,
                    s3ObjectUpload, qbicleId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                result.Object = ex.ToString();
                return result;
            }
        }
        public DataTablesResponse GetBankmateTransactions(IDataTablesRequest requestModel, int cashAccountId, string userId, string keyword, string daterange, string dateFormat, string timezone, bool isPendingStatus = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, cashAccountId, userId, keyword);
                int totalrecords = 0;
                #region Filters
                IQueryable<CashAccountTransaction> query = null;
                if (isPendingStatus)
                {
                    query = dbContext.CashAccountTransactions.Where(
                    s => s.Status == TraderPaymentStatusEnum.PendingApproval
                    && ((s.OriginatingAccount.BankmateType == BankmateAccountType.Domain && s.DestinationAccount.BankmateType == BankmateAccountType.ExternalBank)
                    || (s.OriginatingAccount.BankmateType == BankmateAccountType.ExternalBank && s.DestinationAccount.BankmateType == BankmateAccountType.Domain))
                    );
                }
                else
                {
                    query = dbContext.CashAccountTransactions.Where(
                    s => s.Status != TraderPaymentStatusEnum.PendingApproval
                    && ((s.OriginatingAccount.BankmateType == BankmateAccountType.Domain && s.DestinationAccount.BankmateType == BankmateAccountType.ExternalBank)
                    || (s.OriginatingAccount.BankmateType == BankmateAccountType.ExternalBank && s.DestinationAccount.BankmateType == BankmateAccountType.Domain))
                    );
                }


                if (!string.IsNullOrEmpty(userId))
                    query = query.Where(s => s.CreatedBy.Id == userId);
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(
                        s => s.OriginatingAccount.Domain.Name.Contains(keyword)
                    || (s.CreatedBy.Forename + " " + s.CreatedBy.Surname).Contains(keyword)
                    || (s.Type == CashAccountTransactionTypeEnum.PaymentIn && s.OriginatingAccount.Name.Contains(keyword))
                    || (s.Type == CashAccountTransactionTypeEnum.PaymentOut && s.DestinationAccount.Name.Contains(keyword))
                    );
                }
                if (cashAccountId > 0)
                {
                    query = query.Where(s => s.OriginatingAccount.Id == cashAccountId || s.DestinationAccount.Id == cashAccountId);
                }
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFormat, timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }
                totalrecords = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                var isOrderDomain = false;
                var sortDirection = OrderDirection.Ascendant;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Domain":
                            isOrderDomain = true;
                            sortDirection = column.SortDirection;
                            break;
                        case "Creator":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",CreatedBy.Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreateDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Type":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Type" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Amount" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }
                if (isOrderDomain)
                {
                    if (sortDirection == OrderDirection.Ascendant)
                        query = query.OrderBy(ExpressionOrderbyDomain());
                    else
                        query = query.OrderByDescending(ExpressionOrderbyDomain());
                }
                else
                    query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);

                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Domain = q.OriginatingAccount.Domain.Name,
                    CreatorName = HelperClass.GetFullNameOfUser(q.CreatedBy),
                    CreatorUri = q.CreatedBy.ProfilePic.ToUriString(Enums.FileTypeEnum.Image,"T"),
                    CreateDate = q.CreatedDate.ConvertTimeFromUtc(timezone).ToString(dateFormat + " hh:mmtt"),
                    q.Type,
                    Amount = showAmountByCurrency(q.Amount, q.OriginatingAccount.Domain.Id),
                    ExternalAccountUri = (q.Type == CashAccountTransactionTypeEnum.PaymentIn ? q.OriginatingAccount.ImageUri : q.DestinationAccount.ImageUri).ToUriString(),
                    ExternalAccountName = (q.Type == CashAccountTransactionTypeEnum.PaymentIn ? q.OriginatingAccount.Name : q.DestinationAccount.Name),
                    ExternalNUBAN = (q.Type == CashAccountTransactionTypeEnum.PaymentIn ? ((ExternalBankAccount)q.OriginatingAccount)?.NUBAN : ((ExternalBankAccount)q.DestinationAccount).NUBAN),
                    ExternalIBAN = (q.Type == CashAccountTransactionTypeEnum.PaymentIn ? ((ExternalBankAccount)q.OriginatingAccount)?.IBAN : ((ExternalBankAccount)q.DestinationAccount).IBAN),
                    Attachments = GetAttachments(q.AssociatedFiles), //q.AssociatedFiles
                    q.Status
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, cashAccountId, userId, keyword);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        private string showAmountByCurrency(decimal value, int domainId)
        {
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            return value.ToCurrencySymbol(currencySetting);
        }
        private List<KeyValuePair<string, string>> GetAttachments(List<QbicleMedia> medias)
        {
            //var s3Client = AzureStorageHelper.AwsS3Client();
            List<KeyValuePair<string, string>> attachments = new List<KeyValuePair<string, string>>();
            if (medias != null)
                foreach (var item in medias)
                {
                    var vs = item.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                    if (vs != null)
                    {
                        var storefile = dbContext.StorageFiles.Find(vs.Uri);
                        attachments.Add(new KeyValuePair<string, string>(storefile != null ? storefile.Name : item.Name, AzureStorageHelper.SignedUrl(vs.Uri, storefile != null ? storefile.Name : item.Name)));
                    }

                }
            else
                return new List<KeyValuePair<string, string>>();
            return attachments;
        }
        private static System.Linq.Expressions.Expression<Func<CashAccountTransaction, object>> ExpressionOrderbyDomain()
        {
            return x => (x.Type == CashAccountTransactionTypeEnum.PaymentIn ? x.DestinationAccount.Domain.Name : x.OriginatingAccount.Domain.Name);
        }
    }
}
