using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules;
using Qbicles.Models;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderCashBankRules
    {
        private ApplicationDbContext dbContext;

        public TraderCashBankRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public bool TraderCashAccountNameCheck(TraderCashAccount cashAccount)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, cashAccount);
                if (cashAccount.Id > 0)
                    return dbContext.TraderCashAccounts.Any(x =>
                        x.Id != cashAccount.Id && x.Domain.Id == cashAccount.Domain.Id && x.Name == cashAccount.Name);
                return dbContext.TraderCashAccounts.Any(x =>
                    x.Name == cashAccount.Name && x.Domain.Id == cashAccount.Domain.Id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, cashAccount);
                return false;
            }
        }

        public decimal GetSumFunds(List<CashAccountTransaction> destinations, List<CashAccountTransaction> originations, int cashId, FundEnum type)
        {
            try
            {
                var cashAndBankRules = new TraderCashBankRules(dbContext);

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, destinations, originations, cashId, type);
                if (type == FundEnum.FundsIn)
                    return
                        destinations.Where(e => e.DestinationAccount.Id == cashId &&
                            e.Status == TraderPaymentStatusEnum.PaymentApproved &&
                            cashAndBankRules.GetTransactionDirection(e, cashId).ToLower() == "in")
                        .Sum(a => a.Amount)
                        + originations.Where(e => e.OriginatingAccount.Id == cashId
                            && e.Status == TraderPaymentStatusEnum.PaymentApproved
                            && cashAndBankRules.GetTransactionDirection(e, cashId).ToLower() == "in").Sum(a => a.Amount);
                else if (type == FundEnum.FundsOut)
                    return
                        destinations.Where(e => e.DestinationAccount.Id == cashId
                            && e.Status == TraderPaymentStatusEnum.PaymentApproved
                            && cashAndBankRules.GetTransactionDirection(e, cashId).ToLower() == "out")
                        .Sum(a => a.Amount)
                        + originations.Where(e => e.OriginatingAccount.Id == cashId
                            && e.Status == TraderPaymentStatusEnum.PaymentApproved
                            && cashAndBankRules.GetTransactionDirection(e, cashId).ToLower() == "out")
                        .Sum(a => a.Amount);
                else return 0;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, destinations, originations, cashId, type);
                return 0;
            }
        }

        public decimal GetSumCharges(List<CashAccountTransaction> destinations, List<CashAccountTransaction> originations, int cashId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, destinations, originations, cashId);
                return destinations.Where(e => e.DestinationAccount.Id == cashId && e.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(a => a.Charges)
                       + originations.Where(e => e.OriginatingAccount.Id == cashId && e.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(a => a.Charges);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, destinations, originations, cashId);
                return 0;
            }
        }
        public decimal GetTotalTransactions(List<CashAccountTransaction> destinations, List<CashAccountTransaction> originations, int cashId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, destinations, originations, cashId);
                return originations.Count(e => e.OriginatingAccount.Id == cashId) + destinations.Count(e => e.DestinationAccount.Id == cashId);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, destinations, originations, cashId);
                return 0;
            }
        }
        public enum FundEnum
        {
            FundsIn = 1,
            FundsOut = 2
        }
        public DataTablesResponse TraderCashBankSearch(IDataTablesRequest requestModel, string keyword, int locationId, int domainId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, requestModel, keyword, locationId, domainId, userId);

                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var workGroupIds = dbContext.WorkGroups.Where(q =>
                   (locationId == 0 || q.Location.Id == locationId)
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPaymentProcessName))
                    && q.Members.Select(u => u.Id).Contains(userId)).Select(q => q.Id).ToList();

                var cashBanks = dbContext.TraderCashAccounts.Where(q => q.Domain.Id == domainId);
                var cashBankIds = cashBanks.Select(q => q.Id).ToList();
                var destinations = GetCashAccountTransactionByCashAccountId(cashBankIds, "Destination");
                var originations = GetCashAccountTransactionByCashAccountId(cashBankIds, "Originating");
                int total;
                string nameEmpty = "No Account selected";
                #region Filter
                keyword = string.IsNullOrEmpty(keyword) ? requestModel.Search.Value.ToLower().Trim() : keyword;


                if (!string.IsNullOrEmpty(keyword))
                    cashBanks = cashBanks.Where(q =>
                        q.Name.ToLower().Trim().Contains(keyword)
                        || ((q.AssociatedBKAccount == null && nameEmpty.ToLower().Contains(keyword))
                            || (q.AssociatedBKAccount != null && (q.AssociatedBKAccount.Number + " - " + q.AssociatedBKAccount.Code + " - " + q.AssociatedBKAccount.Name).Contains(keyword)))
                    //|| (GetSumFunds(destinations, originations, q.Id, FundEnum.FundsIn).ToString("N2").Contains(keyword))
                    //|| (GetSumFunds(destinations, originations, q.Id, FundEnum.FundsOut).ToString("N2").Contains(keyword))
                    //|| GetSumCharges(destinations, originations, q.Id).ToString().Contains(keyword)
                    //|| GetTotalTransactions(destinations, originations, q.Id).ToString().Contains(keyword)
                    );

                total = cashBanks.Count();
                #endregion

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Id desc";
                            break;
                    }
                }

                cashBanks = cashBanks.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                #endregion

                #region Paging
                var list = cashBanks.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion

                var dataJson = list.Select(q => new CashBankCustom
                {
                    Id = q.Id,
                    Name = q.Name,
                    BookkeepingAccount = (q.AssociatedBKAccount != null) ? q.AssociatedBKAccount.Number + " - " + (q.AssociatedBKAccount.Code.Replace(",", "").Replace(".", "")) + " - " + q.AssociatedBKAccount.Name : "No Account selected",
                    Image = q.ImageUri,
                    AllowEdit = workGroupIds.Count > 0,
                    Charges = GetSumCharges(destinations, originations, q.Id).ToDecimalPlace(currencySettings),
                    FundsIn = GetSumFunds(destinations, originations, q.Id, FundEnum.FundsIn).ToDecimalPlace(currencySettings),
                    FundsOut = GetSumFunds(destinations, originations, q.Id, FundEnum.FundsOut).ToDecimalPlace(currencySettings),
                    Transactions = GetTotalTransactions(destinations, originations, q.Id).ToString(),
                    BankmateType = q.BankmateType
                });
                return new DataTablesResponse(requestModel.Draw, dataJson, total, total);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword, locationId, domainId, userId);
                return null;
            }
        }

        public ReturnJsonModel SaveTraderCashAccount(TraderCashAccount cashAccount, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, cashAccount);

                var refModel = new ReturnJsonModel { result = true, actionVal = 3, msg = "" };
                if (!string.IsNullOrEmpty(cashAccount.ImageUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(cashAccount.ImageUri);
                }

                if (cashAccount.Id > 0)
                {
                    var cash = GeTraderCashAccountById(cashAccount.Id);
                    cash.Name = cashAccount.Name;
                    cash.AssociatedBKAccount = new BKCoANodesRule(dbContext).GetAccountById(cashAccount.AssociatedBKAccount.Id);
                    cash.ImageUri = cashAccount.ImageUri == "" ? cash.ImageUri : cashAccount.ImageUri;
                    if (dbContext.Entry(cash).State == EntityState.Detached)
                        dbContext.TraderCashAccounts.Attach(cash);
                    dbContext.Entry(cash).State = EntityState.Modified;
                    refModel.actionVal = 2;
                }
                else
                {
                    cashAccount.ImageUri = cashAccount.ImageUri == "" ? cashAccount.Domain.LogoUri : cashAccount.ImageUri;
                    cashAccount.CreatedBy = dbContext.QbicleUser.Find(userId);
                    cashAccount.CreatedDate = DateTime.UtcNow;
                    cashAccount.AssociatedBKAccount = new BKCoANodesRule(dbContext).GetAccountById(cashAccount.AssociatedBKAccount.Id);
                    dbContext.TraderCashAccounts.Add(cashAccount);
                    dbContext.Entry(cashAccount).State = EntityState.Added;
                    refModel.actionVal = 1;
                }

                dbContext.SaveChanges();
                return refModel;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, cashAccount);
                return new ReturnJsonModel { result = false, actionVal = 3, msg = e.Message };
            }

        }

        public List<TraderCashAccount> GetTraderCashAccounts(int domainId, bool isLoadMyBankMate = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                if (isLoadMyBankMate)
                    return dbContext.TraderCashAccounts.Where(d => d.Domain.Id == domainId).ToList();
                else
                    return dbContext.TraderCashAccounts.Where(d => d.Domain.Id == domainId && d.BankmateType == BankmateAccountType.NotInBankMate).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderCashAccount>();
            }

        }

        public TraderCashAccount GeTraderCashAccountById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderCashAccounts.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderCashAccount();
            }
        }

        public List<CashAccountTransactionModel> GetListPaymentTransaction(int column, string orderby, int locationId, int id,
            string fromDate, string toDate, string search, QbicleDomain domain, string currentUserId, string currentTimezone,
            int start, int length, ref int totalRecord, string dateFormat)
        {
            var lstCashAccountTransactionModel = new List<CashAccountTransactionModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, column, orderby, locationId, id,
                        fromDate, toDate, search, domain, currentUserId, currentTimezone, start, length, totalRecord, dateFormat);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                search = search != null ? search.Trim().ToLower() : "";
                var isMember = domain.Workgroups.
                   Where(q => q.Location.Id == locationId
                              && q.Processes.Any(p => p.Name == TraderProcessName.TraderPaymentProcessName)
                              && q.Members.Select(u => u.Id).Contains(currentUserId)).Any();

                List<CashAccountTransaction> destinationAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashAccountId(new List<int>() { id }, "Destination");
                List<CashAccountTransaction> originationAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashAccountId(new List<int>() { id }, "Originating");
                destinationAccounts.AddRange(originationAccounts);
                destinationAccounts = destinationAccounts.Distinct().ToList();
                if (!(String.IsNullOrEmpty(fromDate) && String.IsNullOrEmpty(toDate)))
                {
                    DateTime fromDateTime = DateTime.ParseExact(fromDate, dateFormat, System.Globalization.CultureInfo.InvariantCulture);
                    DateTime toDateTime = DateTime.ParseExact(toDate, dateFormat, System.Globalization.CultureInfo.InvariantCulture).AddDays(1);
                    destinationAccounts = destinationAccounts.Where(d => d.CreatedDate >= fromDateTime && d.CreatedDate < toDateTime).ToList();
                }

                if (column == 1)
                {
                    if (orderby.Equals("asc"))
                    {
                        destinationAccounts = destinationAccounts.OrderBy(c => c.CreatedDate).ToList();
                    }
                    else
                    {
                        destinationAccounts = destinationAccounts.OrderByDescending(c => c.CreatedDate).ToList();
                    }
                }

                foreach (var acc in destinationAccounts)
                {
                    var model = new CashAccountTransactionModel
                    {
                        Id = $"{acc.Id}",
                        Date = acc.CreatedDate.ConvertTimeFromUtc(currentTimezone).ToString(dateFormat),
                        Reference = acc.Reference
                    };

                    model.Source = acc.OriginatingAccount?.Name ?? "Not chosen";
                    if (acc.PaymentMethod != null) model.PaymentMethod = acc.PaymentMethod.Name; else model.PaymentMethod = "";
                    model.Destination = acc.DestinationAccount?.Name ?? "Not chosen";
                    if (acc.Type == CashAccountTransactionTypeEnum.Transfer)
                    {
                        model.Type = "Transfer";
                    }
                    else
                    {
                        model.Type = "Payment";
                    }

                    model.InOut = GetTransactionDirection(acc, id);

                    if (acc.Type != CashAccountTransactionTypeEnum.Transfer)
                    {
                        if (acc.AssociatedInvoice == null && acc.Contact != null)
                        {
                            model.For = "Payment on account";
                        }
                        else if (acc.AssociatedInvoice != null)
                        {
                            model.For = "Invoice #" + acc.AssociatedInvoice?.Reference?.FullRef ?? "";
                        }
                    }
                    else
                    {
                        model.For = "Transfer funds";
                    }
                    model.Amount = acc.Amount.ToCurrencySymbol(currencySettings);
                    switch (acc.Status)
                    {
                        case TraderPaymentStatusEnum.PendingReview: model.Status = "Pending Review"; break;
                        case TraderPaymentStatusEnum.PendingApproval: model.Status = "Pending Approval"; break;
                        case TraderPaymentStatusEnum.PaymentApproved: model.Status = "Approved"; break;
                        case TraderPaymentStatusEnum.PaymentDenied: model.Status = "Denied"; break;
                        case TraderPaymentStatusEnum.Draft: model.Status = "Draft"; break;
                        case TraderPaymentStatusEnum.PaymentDiscarded: model.Status = "Discarded"; break;
                    }
                    model.Action = "";
                    if (acc.Type != CashAccountTransactionTypeEnum.Transfer)
                    {
                        if (acc.Status == TraderPaymentStatusEnum.Draft && isMember)
                        {
                            model.Action = "Continue";
                        }
                        else
                        {
                            model.Action = "Manage";
                        }
                    }
                    else if (acc.Type == CashAccountTransactionTypeEnum.Transfer)
                    {
                        if (acc.Status == TraderPaymentStatusEnum.Draft && isMember)
                        {
                            model.Action = "Continue";
                        }
                        else
                        {
                            model.Action = "Manage";
                        }
                    }
                    model.Action.TrimStart(',');
                    lstCashAccountTransactionModel.Add(model);
                }

                lstCashAccountTransactionModel = lstCashAccountTransactionModel.Where(c => search.Equals("") || c.Id.ToString().Contains(search) || (c.Destination != null && c.Destination.ToLower().Contains(search)) || (c.Reference != null && c.Reference.ToLower().Contains(search))
                                                || c.Source.ToLower().Contains(search) || c.PaymentMethod.ToLower().Contains(search) || c.Type.ToLower().Contains(search)
                                                || c.PaymentMethod.ToLower().Contains(search) || c.Type.ToLower().Contains(search) || c.InOut.ToLower().Contains(search)
                                                || c.For.ToLower().Contains(search) || c.Status.ToLower().Contains(search)).ToList();

                if (column == 0)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.Id).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.Id).ToList();
                    }
                }
                else if (column == 2)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.Reference).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.Reference).ToList();
                    }
                }
                else if (column == 3)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.Source).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.Source).ToList();
                    }
                }
                else if (column == 4)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.PaymentMethod).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.PaymentMethod).ToList();
                    }
                }
                else if (column == 5)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.Destination).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.Destination).ToList();
                    }
                }
                else if (column == 6)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.Type).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.Type).ToList();
                    }
                }
                else if (column == 7)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.InOut).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.InOut).ToList();
                    }
                }
                else if (column == 8)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.For).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.For).ToList();
                    }
                }
                else if (column == 9)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.Amount).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.Amount).ToList();
                    }
                }
                else if (column == 10)
                {
                    if (orderby.Equals("asc"))
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderBy(c => c.Status).ToList();
                    }
                    else
                    {
                        lstCashAccountTransactionModel = lstCashAccountTransactionModel.OrderByDescending(c => c.Status).ToList();
                    }
                }
                totalRecord = lstCashAccountTransactionModel.Count();
                lstCashAccountTransactionModel = lstCashAccountTransactionModel.Skip(start).Take(length).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, currentUserId, column, orderby, locationId, id, fromDate, toDate, search, domain, currentUserId, currentTimezone, start, length, totalRecord, dateFormat);
            }

            return lstCashAccountTransactionModel;
        }

        public List<QbicleMedia> ShowCashAccountTransactionAttachments(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.Medias.Where(e => e.CashAccountTransaction.Id == id).OrderByDescending(d => d.TimeLineDate)
                    .ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new List<QbicleMedia>();
            }

        }

        public CashAccountTransaction GeCashAccountTransactionById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.CashAccountTransactions.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new CashAccountTransaction();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="ids">list id of trader cash account</param>
        /// <param name="type">Destination or Originating</param>
        /// <returns></returns>
        public List<CashAccountTransaction> GetCashAccountTransactionByCashAccountId(List<int> ids, string type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, ids, type);
                switch (type)
                {
                    case "Destination":
                        return dbContext.CashAccountTransactions.Where(c => c.DestinationAccount != null && ids.Contains(c.DestinationAccount.Id)).ToList();
                    case "Originating":
                        return dbContext.CashAccountTransactions.Where(c => c.OriginatingAccount != null && ids.Contains(c.OriginatingAccount.Id)).ToList();
                    default:
                        return new List<CashAccountTransaction>();
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, ids, type);
                return new List<CashAccountTransaction>();
            }
        }

        public List<CashAccountTransaction> GetCashAccountTransactionByCashId(int id, string type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, type);
                switch (type)
                {
                    case "Destination":
                        return dbContext.CashAccountTransactions.Where(c => c.DestinationAccount.Id == id).ToList();
                    case "Originating":
                        return dbContext.CashAccountTransactions.Where(c => c.OriginatingAccount.Id == id).ToList();
                    default:
                        return new List<CashAccountTransaction>();
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, type);
                return new List<CashAccountTransaction>();
            }
        }
        public decimal SumCashAccountTransactionByCashId(int id, string type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, type);
                switch (type)
                {
                    case "Destination":
                        return dbContext.CashAccountTransactions.Where(c => c.DestinationAccount.Id == id && c.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(q => q.Amount);
                    case "Originating":
                        return dbContext.CashAccountTransactions.Where(c => c.OriginatingAccount.Id == id && c.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(q => q.Amount);
                    default:
                        return 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, type);
                return 0;
            }
        }

        public CashAccountTransaction GetCashAccountTransactionById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.CashAccountTransactions.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new CashAccountTransaction();
            }

        }

        private Models.Trader.PaymentMethod GetPaymentMethodByName(string name)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, name);
                return dbContext.PaymentMethods.FirstOrDefault(e => e.Name == name);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, name);
                return new Models.Trader.PaymentMethod();
            }

        }
        private Models.Trader.PaymentMethod GetPaymentMethodById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.PaymentMethods.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new Models.Trader.PaymentMethod();
            }
        }

        public CashAccountTransaction SaveCashAccountPayment(CashAccountTransaction payment, string userId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, payment);
                if (payment == null) return null;

                PaymentMethod paymentMethod;
                if (payment.PaymentMethod?.Id == null)
                    paymentMethod = GetPaymentMethodByName("Electronic Transfer");
                else
                    paymentMethod = GetPaymentMethodById(payment.PaymentMethod?.Id ?? 0);

                var user = dbContext.QbicleUser.Find(userId);
                if (payment.Id > 0)
                {
                    var cashPay = GeCashAccountTransactionById(payment.Id);
                    cashPay.Amount = payment.Amount;
                    cashPay.Charges = payment.Charges;
                    cashPay.Type = payment.Type;
                    cashPay.Description = payment.Description;
                    cashPay.Status = payment.Status;
                    cashPay.Reference = payment.Reference;
                    cashPay.PaymentMethod = paymentMethod;
                    cashPay.Workgroup = new TraderWorkGroupsRules(dbContext).GetById(payment.Workgroup.Id);

                    if (payment.AssociatedPurchase?.Id > 0)
                    {
                        cashPay.AssociatedPurchase =
                            new TraderPurchaseRules(dbContext).GetById(payment.AssociatedPurchase.Id);
                        cashPay.Contact = payment.AssociatedPurchase.Vendor;
                    }

                    if (payment.AssociatedSale?.Id > 0)
                    {
                        cashPay.AssociatedSale = new TraderSaleRules(dbContext).GetById(payment.AssociatedSale.Id);
                        cashPay.Contact = payment.AssociatedSale.Purchaser;
                    }

                    if (payment.AssociatedInvoice?.Id > 0)
                    {
                        cashPay.AssociatedInvoice = new TraderInvoicesRules(dbContext).GetById(payment.AssociatedInvoice.Id);
                        if (payment.AssociatedInvoice.Sale != null)
                            cashPay.Contact = payment.AssociatedInvoice.Sale.Purchaser;
                        else if (payment.AssociatedInvoice.Purchase != null)
                            cashPay.Contact = payment.AssociatedInvoice.Purchase.Vendor;
                    }

                    if (payment.Contact?.Id > 0)
                        cashPay.Contact = new TraderContactRules(dbContext).GetById(payment.Contact.Id);
                    else
                        payment.Contact = null;

                    if (payment.DestinationAccount != null && payment.DestinationAccount.Id > 0)
                        cashPay.DestinationAccount = GeTraderCashAccountById(payment.DestinationAccount.Id);
                    else
                        payment.DestinationAccount = null;

                    if (payment.OriginatingAccount != null && payment.OriginatingAccount.Id > 0)
                        cashPay.OriginatingAccount = GeTraderCashAccountById(payment.OriginatingAccount.Id);
                    else
                        payment.OriginatingAccount = null;


                    if (dbContext.Entry(cashPay).State == EntityState.Detached)
                        dbContext.CashAccountTransactions.Attach(cashPay);
                    dbContext.Entry(cashPay).State = EntityState.Modified;
                }
                else
                {
                    payment.CreatedBy = user;
                    payment.CreatedDate = DateTime.UtcNow;
                    payment.PaymentMethod = paymentMethod;
                    if (payment.AssociatedPurchase?.Id > 0)
                    {
                        payment.AssociatedPurchase =
                            new TraderPurchaseRules(dbContext).GetById(payment.AssociatedPurchase.Id);
                        payment.Contact = payment.AssociatedPurchase.Vendor;
                    }

                    if (payment.AssociatedSale?.Id > 0)
                    {
                        payment.AssociatedSale = new TraderSaleRules(dbContext).GetById(payment.AssociatedSale.Id);
                        payment.Contact = payment.AssociatedSale.Purchaser;
                    }

                    if (payment.AssociatedInvoice?.Id > 0)
                    {
                        payment.AssociatedInvoice = new TraderInvoicesRules(dbContext).GetById(payment.AssociatedInvoice.Id);
                        if (payment.AssociatedInvoice.Sale != null)
                            payment.Contact = payment.AssociatedInvoice.Sale.Purchaser;
                        else if (payment.AssociatedInvoice.Purchase != null)
                            payment.Contact = payment.AssociatedInvoice.Purchase.Vendor;
                    }


                    if (payment.Contact?.Id > 0)
                        payment.Contact = new TraderContactRules(dbContext).GetById(payment.Contact.Id);
                    else
                        payment.Contact = null;


                    payment.Workgroup = new TraderWorkGroupsRules(dbContext).GetById(payment.Workgroup.Id);

                    if (payment.DestinationAccount?.Id > 0)
                        payment.DestinationAccount = GeTraderCashAccountById(payment.DestinationAccount.Id);
                    else
                        payment.DestinationAccount = null;

                    if (payment.OriginatingAccount?.Id > 0)
                        payment.OriginatingAccount = GeTraderCashAccountById(payment.OriginatingAccount.Id);
                    else
                        payment.OriginatingAccount = null;


                    dbContext.CashAccountTransactions.Add(payment);
                    dbContext.Entry(payment).State = EntityState.Added;
                }

                dbContext.SaveChanges();

                //approval payment
                var traderPayment = GetCashAccountTransactionById(payment.Id);
                if (traderPayment == null || traderPayment.Status != TraderPaymentStatusEnum.PendingReview)
                    return traderPayment;

                if (traderPayment.PaymentApprovalProcess != null) return payment;

                var workgroup = traderPayment.Workgroup;

                workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var refFull = traderPayment.Reference == null ? "" : traderPayment.Reference;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = dbContext.PaymentApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == workgroup.Id),
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = workgroup.Qbicle,
                    Topic = workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Trader Approval for Payment #{refFull}",
                    Payments = new List<CashAccountTransaction> { traderPayment }
                };
                traderPayment.PaymentApprovalProcess = approval;
                approval.ActivityMembers.AddRange(workgroup.Members);

                dbContext.SaveChanges();


                var paymentDb = traderPayment;
                var paymentLog = new CashAccountTransactionLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Id = 0,
                    CreatedBy = user,
                    Status = paymentDb.Status,
                    Description = paymentDb.Description,
                    Type = paymentDb.Type,
                    AssociatedInvoice = paymentDb.AssociatedInvoice,
                    Workgroup = paymentDb.Workgroup,
                    DestinationAccount = paymentDb.DestinationAccount,
                    OriginatingAccount = paymentDb.OriginatingAccount,
                    AssociatedFiles = paymentDb.AssociatedFiles,
                    Amount = paymentDb.Amount,
                    AssociatedSale = paymentDb.AssociatedSale,
                    AssociatedPurchase = paymentDb.AssociatedPurchase,
                    PaymentApprovalProcess = paymentDb.PaymentApprovalProcess,
                    Contact = paymentDb.Contact,
                    AssociatedTransaction = paymentDb,
                    AssociatedBKTransaction = paymentDb.AssociatedBKTransaction,
                    Charges = paymentDb.Charges,
                    Reference = paymentDb.Reference,
                    PaymentMethod = paymentDb.PaymentMethod
                };

                var wasteProcessLog = new PaymentProcessLog
                {
                    AssociatedTransaction = paymentDb,
                    AssociatedCashAccountTransactionLog = paymentLog,
                    PaymentStatus = paymentDb.Status,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.PaymentProcessLogs.Add(wasteProcessLog);
                dbContext.Entry(wasteProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();


                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);
                return traderPayment;

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, payment);
                return new CashAccountTransaction();
            }
        }
        public bool DeleteCashBank(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var ids = new List<int> { id };
                var originating = GetCashAccountTransactionByCashAccountId(ids, "Originating");
                var destination = GetCashAccountTransactionByCashAccountId(ids, "Destination");
                dbContext.CashAccountTransactions.RemoveRange(originating);
                dbContext.CashAccountTransactions.RemoveRange(destination);
                var cashAccount = GeTraderCashAccountById(id);
                dbContext.TraderCashAccounts.Remove(cashAccount);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return false;
            }
        }
        public bool DeleteCashAccountTransactionFile(int fileId, int accId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, fileId, accId);
                var accTt = dbContext.CashAccountTransactions.Find(accId);
                if (accTt == null) return false;
                var file = accTt.AssociatedFiles.FirstOrDefault(q => q.Id == fileId);
                accTt.AssociatedFiles.Remove(file);
                dbContext.Entry(accTt).State = EntityState.Modified;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, fileId, accId);
                return false;
            }

        }
        public List<SalePurchasePaymentModel> GeSalePurchasePaymentByLocationId(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                var list = new List<SalePurchasePaymentModel>();
                var sales = new TraderSaleRules(dbContext).GetByLocation(locationId, domainId);
                var purchases = new TraderPurchaseRules(dbContext).GetByLocation(locationId, domainId);
                foreach (var s in sales)
                    list.Add(new SalePurchasePaymentModel
                    {
                        Id = s.Id,
                        Name = $"Sale: #{s.Id}",
                        Type = SalePurchasePaymentEnum.Sale
                    });
                foreach (var p in purchases)
                    list.Add(new SalePurchasePaymentModel
                    {
                        Id = p.Id,
                        Name = $"Purchase: #{p.Id}",
                        Type = SalePurchasePaymentEnum.Purchase
                    });
                return list;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, domainId);
                return new List<SalePurchasePaymentModel>();
            }

        }

        public CashAccountTransaction SaveCashAccountTransfer(CashAccountTransaction transfer, ApplicationUser user, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, transfer, user);

                if (transfer == null) return null;
                if (transfer.Id > 0)
                {
                    var cashPay = GeCashAccountTransactionById(transfer.Id);
                    cashPay.Amount = transfer.Amount;
                    cashPay.Charges = transfer.Charges;
                    cashPay.Type = transfer.Type;
                    cashPay.Description = transfer.Description;
                    if (transfer.Contact != null)
                    {
                        cashPay.Contact = new TraderContactRules(dbContext).GetById(transfer.Contact.Id);
                        cashPay.Contact.InUsed = true;
                    }
                    if (transfer.AssociatedPurchase != null)
                        cashPay.AssociatedPurchase =
                            new TraderPurchaseRules(dbContext).GetById((int)transfer.AssociatedPurchase?.Id);
                    if (transfer.AssociatedSale != null)
                        cashPay.AssociatedSale = new TraderSaleRules(dbContext).GetById((int)transfer.AssociatedSale?.Id);
                    cashPay.DestinationAccount = GeTraderCashAccountById(transfer.DestinationAccount?.Id ?? 0);
                    cashPay.OriginatingAccount = GeTraderCashAccountById(transfer.OriginatingAccount?.Id ?? 0);
                    if (dbContext.Entry(cashPay).State == EntityState.Detached)
                        dbContext.CashAccountTransactions.Attach(cashPay);
                    dbContext.Entry(cashPay).State = EntityState.Modified;
                }
                else
                {
                    transfer.CreatedDate = DateTime.UtcNow;
                    if (transfer.Contact != null)
                    {
                        transfer.Contact = new TraderContactRules(dbContext).GetById(transfer.Contact.Id);
                        transfer.Contact.InUsed = true;
                    }
                    if (transfer.AssociatedPurchase != null)
                        transfer.AssociatedPurchase =
                            new TraderPurchaseRules(dbContext).GetById((int)transfer.AssociatedPurchase?.Id);
                    if (transfer.AssociatedSale != null)
                        transfer.AssociatedSale = new TraderSaleRules(dbContext).GetById((int)transfer.AssociatedSale?.Id);
                    transfer.DestinationAccount = GeTraderCashAccountById(transfer.DestinationAccount?.Id ?? 0);
                    transfer.OriginatingAccount = GeTraderCashAccountById(transfer.OriginatingAccount?.Id ?? 0);
                    dbContext.CashAccountTransactions.Add(transfer);
                    dbContext.Entry(transfer).State = EntityState.Added;
                }

                dbContext.SaveChanges();

                //approval payment
                var traderPayment = GetCashAccountTransactionById(transfer.Id);
                if (traderPayment == null || traderPayment.Status != TraderPaymentStatusEnum.PendingReview) return transfer;

                if (traderPayment.PaymentApprovalProcess != null) return transfer;


                traderPayment.DestinationAccount = dbContext.TraderCashAccounts.FirstOrDefault(n => n.Id == 1);

                var workgroup = traderPayment.Workgroup;
                workgroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = dbContext.ContactApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == workgroup.Id),
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = workgroup.Qbicle,
                    Topic = workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Trader Approval for Payment #{traderPayment.Id}",
                    Payments = new List<CashAccountTransaction> { traderPayment }
                };
                traderPayment.PaymentApprovalProcess = approval;
                approval.ActivityMembers.AddRange(workgroup.Members);

                dbContext.SaveChanges();


                var paymentDb = traderPayment;
                var paymentLog = new CashAccountTransactionLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Id = 0,
                    CreatedBy = user,
                    Status = paymentDb.Status,
                    Description = paymentDb.Description,
                    Type = paymentDb.Type,
                    AssociatedInvoice = paymentDb.AssociatedInvoice,
                    Workgroup = paymentDb.Workgroup,
                    DestinationAccount = paymentDb.DestinationAccount,
                    OriginatingAccount = paymentDb.OriginatingAccount,
                    AssociatedFiles = paymentDb.AssociatedFiles,
                    Amount = paymentDb.Amount,
                    AssociatedSale = paymentDb.AssociatedSale,
                    AssociatedPurchase = paymentDb.AssociatedPurchase,
                    PaymentApprovalProcess = paymentDb.PaymentApprovalProcess,
                    Contact = paymentDb.Contact,
                    AssociatedTransaction = paymentDb,
                    AssociatedBKTransaction = paymentDb.AssociatedBKTransaction,
                    Charges = paymentDb.Charges,
                    Reference = paymentDb.Reference,
                    PaymentMethod = paymentDb.PaymentMethod
                };

                var wasteProcessLog = new PaymentProcessLog
                {
                    AssociatedTransaction = paymentDb,
                    AssociatedCashAccountTransactionLog = paymentLog,
                    PaymentStatus = paymentDb.Status,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.PaymentProcessLogs.Add(wasteProcessLog);
                dbContext.Entry(wasteProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = user.Id,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                 new NotificationRules(dbContext).Notification2Activity(activityNotification);
                return transfer;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, user.Id, transfer, user);
                return new CashAccountTransaction();
            }
        }

        public List<CashAccountTransaction> GetByInvoice(int invoiceId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, invoiceId);
                return dbContext.CashAccountTransactions.Where(d => d.AssociatedInvoice.Id == invoiceId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, invoiceId);
                return new List<CashAccountTransaction>();
            }
        }
        public DataTablesResponse GetByContact(int contactId, IDataTablesRequest requestModel, UserSetting user, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId, requestModel, user, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var query = dbContext.CashAccountTransactions.Where(l => l.Contact.Id == contactId);
                int totalSpot = 0;
                #region Filter
                var keyword = requestModel.Search != null ? requestModel.Search.Value : "";
                if (!string.IsNullOrEmpty(keyword))
                {
                    //Check keyword is date and convert keyword date to DateTime.UTC
                    DateTime date = DateTime.UtcNow;
                    bool isDate = false;
                    Regex rx = new Regex("^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[0-9]|[12][0-9]|[3][01])[./-]([0-9]{4}|[0-9]{2})$");
                    var match = rx.Matches(keyword);
                    if (match.Count > 0)
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                        try
                        {
                            isDate = true;
                            date = TimeZoneInfo.ConvertTimeToUtc(keyword.ConvertDateFormat(user.DateFormat), tz);
                        }
                        catch
                        {
                            date = DateTime.UtcNow;
                        }

                    }
                    //End
                    var _id = 0;
                    var endate = date.AddDays(1);
                    int.TryParse(keyword.TrimStart('0'), out _id);
                    query = query.Where(q =>
                        q.Id == _id
                        || (isDate && q.CreatedDate >= date && q.CreatedDate < endate)
                        || q.Description.Contains(keyword)
                    );
                }
                totalSpot = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Id" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Amount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Ref = q.Id.ToString("D6"),
                    Date = q.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateTimeFormat),//"dd/MM/yyyy".replace(/\//g,".") javascript
                    Amount = q.Amount.ToDecimalPlace(currencySettings),
                    Type = q.AssociatedInvoice != null ? q.AssociatedInvoice.Id.ToString("D6") : "",//q.AssociatedInvoice != null ? $"Invoice #{q.AssociatedInvoice.Id:D6}" : "Payment on account",
                    IvId = q.AssociatedInvoice != null ? q.AssociatedInvoice.Id : 0,
                    IvKey = q.AssociatedInvoice != null ? q.AssociatedInvoice.Key : "",
                    q.Status,
                    q.Description
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSpot, totalSpot);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactId, requestModel, user.Timezone, user.DateFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }
        public void PaymentApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var payment = approval.Payments.FirstOrDefault();
                if (payment == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        payment.Status = TraderPaymentStatusEnum.PendingReview;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        payment.Status = TraderPaymentStatusEnum.PendingApproval;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        payment.Status = TraderPaymentStatusEnum.PaymentApproved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        payment.Status = TraderPaymentStatusEnum.PaymentDenied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        payment.Status = TraderPaymentStatusEnum.PaymentDiscarded;
                        break;
                }
                dbContext.Entry(payment).State = EntityState.Modified;
                dbContext.SaveChanges();

                //The Journal Entries for Payments are to be created after the Payment has been approved.
                if (approval.RequestStatus == ApprovalReq.RequestStatusEnum.Approved)
                    new BookkeepingIntegrationRules(dbContext).AddPaymentJournalEntry(approval.ApprovedOrDeniedAppBy, payment);

                //logging

                var paymentLog = new CashAccountTransactionLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Id = 0,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    Status = payment.Status,
                    Description = payment.Description,
                    Type = payment.Type,
                    AssociatedInvoice = payment.AssociatedInvoice,
                    Workgroup = payment.Workgroup,
                    DestinationAccount = payment.DestinationAccount,
                    OriginatingAccount = payment.OriginatingAccount,
                    AssociatedFiles = payment.AssociatedFiles,
                    Amount = payment.Amount,
                    AssociatedSale = payment.AssociatedSale,
                    AssociatedPurchase = payment.AssociatedPurchase,
                    PaymentApprovalProcess = payment.PaymentApprovalProcess,
                    Contact = payment.Contact,
                    AssociatedTransaction = payment,
                    AssociatedBKTransaction = payment.AssociatedBKTransaction,
                    Charges = payment.Charges,
                    Reference = payment.Reference,
                    PaymentMethod = payment.PaymentMethod
                };

                var wasteProcessLog = new PaymentProcessLog
                {
                    AssociatedTransaction = payment,
                    AssociatedCashAccountTransactionLog = paymentLog,
                    PaymentStatus = payment.Status,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = approval.ApprovedOrDeniedAppBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.PaymentProcessLogs.Add(wasteProcessLog);
                dbContext.Entry(wasteProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();


                //Hangfire to generate store points from the payment approved
                if (payment.Status == TraderPaymentStatusEnum.PaymentApproved)
                    new TraderEventRules(dbContext).GenerateStorePoinFromPaymentApproved(payment.Id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public List<ApprovalStatusTimeline> PaymentApprovalStatusTimeline(int id, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var timeline = new List<ApprovalStatusTimeline>();
                var logs = dbContext.PaymentProcessLogs.Where(e => e.AssociatedTransaction.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string icon = StatusLabelStyle.Reviewed;

                foreach (var log in logs)
                {
                    switch (log.PaymentStatus)
                    {
                        case TraderPaymentStatusEnum.PendingReview:
                            icon = "fa fa-info bg-aqua";
                            break;
                        case TraderPaymentStatusEnum.PendingApproval:
                            icon = "fa fa-truck bg-yellow";
                            break;
                        case TraderPaymentStatusEnum.PaymentApproved:
                            icon = "fa fa-check bg-green";
                            break;
                        case TraderPaymentStatusEnum.Draft:
                            icon = "fa fa-warning bg-yellow";
                            break;
                        case TraderPaymentStatusEnum.PaymentDenied:
                            icon = "fa fa-warning bg-red";
                            break;
                        case TraderPaymentStatusEnum.PaymentDiscarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = log.PaymentStatus.GetDescription(),
                            Icon = icon,
                            UserAvatar = log.CreatedBy.ProfilePic
                        }
                    );
                }

                return timeline;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timeZone);
                return new List<ApprovalStatusTimeline>();
            }

        }

        public string GetTransactionDirection(CashAccountTransaction transaction, int referenceAccountId)
        {
            if (transaction.Type == CashAccountTransactionTypeEnum.Transfer)
            {
                if (transaction.OriginatingAccount != null && transaction.DestinationAccount != null)
                {
                    if (referenceAccountId == transaction.OriginatingAccount.Id)
                        return "Out";
                    else if (referenceAccountId == transaction.DestinationAccount.Id)
                        return "In";
                }
                else if (transaction.OriginatingAccount != null)
                {
                    if (referenceAccountId == transaction.OriginatingAccount.Id)
                        return "Out";
                    else
                        return "In";
                }
                else if (transaction.DestinationAccount != null)
                {
                    if (referenceAccountId == transaction.DestinationAccount.Id)
                        return "In";
                    else
                        return "Out";
                }
            }
            else if (transaction.Type == CashAccountTransactionTypeEnum.PaymentIn)
            {
                return "In";
            }
            else if (transaction.Type == CashAccountTransactionTypeEnum.PaymentOut)
            {
                return "Out";
            }
            return "Unknown";
        }

        public DataTablesResponse GetReportPayments(IDataTablesRequest requestModel, string keyword, string datetime, int contactId, UserSetting dateTimeFormat, int domainId, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, contactId, dateTimeFormat, domainId, keyword);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var query = dbContext.CashAccountTransactions.Where(s => s.Workgroup.Domain.Id == domainId);
                int totalRecords = 0;
                #region Filter
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Reference.Contains(keyword) || s.Contact.Name.Contains(keyword));
                }
                if (contactId>0)
                {
                    query = query.Where(s => s.Contact.Id==contactId);
                }
                if (!string.IsNullOrEmpty(datetime))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(dateTimeFormat.Timezone);
                    datetime.ConvertDaterangeFormat(dateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }
                totalRecords = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Contact":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Contact.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Amount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Ref = q.Reference != null ? q.Reference : q.Id.ToString(),
                    Contact = q.Contact?.Name,
                    ContactId = q.Contact?.QbicleUser?.Id,
                    Date = q.CreatedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat),
                    Amount = q.Amount.ToDecimalPlace(currencySettings),
                    q.Status,
                    q.Description,
                    AllowEdit = (q.Workgroup != null
                        && q.Workgroup.Processes.Any(s => s.Name.Equals(TraderProcessName.TraderPaymentProcessName))
                        && q.Workgroup.Members.Any(s => s.Id == currentUserId)
                        )
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword, datetime, contactId, dateTimeFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }
    }
}