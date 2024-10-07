using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.CashMgt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.Models.Notification;
using static Qbicles.Models.Trader.CashMgt.TillPayment;

namespace Qbicles.BusinessRules.CMs
{
    public class CMsRules
    {
        private ApplicationDbContext _dbContext;

        public CMsRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        private ApplicationDbContext dbContext => _dbContext ?? new ApplicationDbContext();

        // Get Tills, Safe data
        public Safe GetSafeByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetSafeByLocation", null, null, locationId);

                return dbContext.Safes.Where(e => e.Location.Id == locationId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new Safe();
            }
        }

        public Safe GetSafeByLocationAsNoTracking(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetSafeByLocation", null, null, locationId);

                return dbContext.Safes.AsNoTracking().FirstOrDefault(e => e.Location.Id == locationId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new Safe();
            }
        }

        public List<Safe> GetSafesByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetSafesByDomain", null, null, domainId);

                return dbContext.Safes.Where(e => e.Location.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<Safe>();
            }
        }

        public Safe GetSafeById(int safeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetSafeById", null, null, safeId);

                return dbContext.Safes.Where(e => e.Id == safeId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, safeId);
                return new Safe();
            }
        }

        public List<Safe> GetSafesByBankAccount(int accountId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetSafeById", null, null, accountId);

                return dbContext.Safes.Where(e => e.CashAndBankAccount.Id == accountId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, accountId);
                return new List<Safe>();
            }
        }

        public List<Till> GetTillsByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTillsByLocation", null, null, locationId);

                return dbContext.Tills.Where(e => e.Location.Id == locationId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new List<Till>();
            }
        }

        public Till GetTillById(int tillId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTillById", null, null, tillId);

                return dbContext.Tills.Where(e => e.Id == tillId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, tillId);
                return new Till();
            }
        }

        public Till GetTillByIdAsNoTracking(int tillId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTillById", null, null, tillId);

                return dbContext.Tills.AsNoTracking().FirstOrDefault(e => e.Id == tillId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, tillId);
                return new Till();
            }
        }

        public Till GetTillByPosDevice(int posDeviceId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTillById", null, null, posDeviceId);

                return dbContext.Tills.Where(e => e.PosDevices.Any(p => p.Id == posDeviceId)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, posDeviceId);
                return new Till();
            }
        }

        // Save Tills, Safe Changes
        public ReturnJsonModel SaveSafe(Safe safe, string userId, int locationId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateSafe", null, null, safe, userId, locationId);
                var user = dbContext.QbicleUser.Find(userId);
                var location = dbContext.TraderLocations.FirstOrDefault(p => p.Id == locationId);
                if (location != null)
                {
                    safe.Location = location;
                    if (safe.Id != 0)
                    {
                        var safeInDatabase = new CMsRules(dbContext).GetSafeById(safe.Id);
                        safeInDatabase.Name = safe.Name;
                        safeInDatabase.CashAndBankAccount = safe.CashAndBankAccount;

                        safeInDatabase.LastUpdatedBy = user;
                        safeInDatabase.LastUpdatedDate = DateTime.UtcNow;
                        dbContext.Entry(safeInDatabase).State = EntityState.Modified;
                    }
                    else
                    {
                        safe.Id = new int();
                        safe.CreatedBy = user;
                        safe.CreatedDate = DateTime.UtcNow;
                        safe.LastUpdatedBy = user;
                        safe.LastUpdatedDate = DateTime.UtcNow;
                        dbContext.Entry(safe).State = EntityState.Added;
                        dbContext.Safes.Add(safe);
                    }

                    dbContext.SaveChanges();
                    refModel.result = true;
                }
                else
                {
                    refModel.msg = "Location does not exsist.";
                    refModel.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, userId, locationId);
                refModel.msg = ex.Message;
                refModel.result = false;
            }

            return refModel;
        }

        public ReturnJsonModel SaveTill(Till till, string userId, int locationId, string listPOSDeviceIdsString)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateTill", null, null, till, userId, locationId, listPOSDeviceIdsString);

                var location = dbContext.TraderLocations.FirstOrDefault(p => p.Id == locationId);
                if (location != null)
                {
                    var listTillsInLocation = GetTillsByLocation(locationId);
                    var isTillNameExist = listTillsInLocation.Any(p => p.Id != till.Id && p.Name.ToUpper() == till.Name.ToUpper());
                    if (isTillNameExist)
                    {
                        refModel.msg = "The name of the till exsisted in the location.";
                        refModel.result = false;
                        return refModel;
                    }

                    var safeInLocation = GetSafeByLocation(locationId);
                    if (safeInLocation == null)
                    {
                        refModel.msg = "The location does not have any safe";
                        refModel.result = false;
                        return refModel;
                    }

                    // Create Virtual Till
                    var listPosDeviceIds = listPOSDeviceIdsString.Split(',').ToList();
                    var listPosDevicesInLocation = new PosDeviceRules(dbContext).GetByLocation(locationId);
                    foreach (var posDeviceIdItem in listPosDeviceIds)
                    {
                        var posDeviceId = Convert.ToInt32(posDeviceIdItem);
                        var isPosDeviceExistedInLocation = listPosDevicesInLocation.Any(p => p.Id == posDeviceId);
                        if (!isPosDeviceExistedInLocation)
                        {
                            refModel.msg = $"PosDevice with ID {posDeviceId} does not exist in the current location.";
                            refModel.result = false;
                            return refModel;
                        }
                        var posDevice = new PosDeviceRules(dbContext).GetById(posDeviceId);
                        if (posDevice == null)
                        {
                            refModel.msg = $"PosDevice with ID {posDeviceId} does not exist.";
                            refModel.result = false;
                            return refModel;
                        }

                        till.PosDevices.Add(posDevice);
                    }

                    // Binding data
                    till.Location = location;
                    till.Safe = safeInLocation;
                    till.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    till.LastUpdatedDate = DateTime.UtcNow;

                    if (till.Id == 0)
                    {
                        till.Id = new int();
                        till.CreatedBy = dbContext.QbicleUser.Find(userId);
                        till.CreatedDate = DateTime.UtcNow;

                        dbContext.Entry(till).State = EntityState.Added;
                        dbContext.Tills.Add(till);
                    }
                    else
                    {
                        var tillInDb = GetTillById(till.Id);
                        tillInDb.Name = till.Name;
                        tillInDb.PosDevices = till.PosDevices;

                        dbContext.Entry(tillInDb).State = EntityState.Modified;
                    }

                    dbContext.SaveChanges();
                    refModel.result = true;
                }
                else
                {
                    refModel.msg = "Location does not exsist.";
                    refModel.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, userId, locationId, listPOSDeviceIdsString);
                refModel.msg = ex.Message;
                refModel.result = false;
            }

            return refModel;
        }

        public ReturnJsonModel DeleteTill(Till till)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                till.PosDevices.Clear();
                till.Payments.Clear();
                till.Checkpoints.Clear();
                dbContext.SaveChanges();

                dbContext.Tills.Remove(till);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, till);
                refModel.msg = ex.Message;
                refModel.result = false;
            }

            return refModel;
        }

        // Managing the cash
        public ReturnJsonModel SaveTillPayment(TillPayment tillPayment, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, tillPayment);

                if (tillPayment.AssociatedTill.Id != 0)
                {
                    tillPayment.AssociatedTill = dbContext.Tills.Find(tillPayment.AssociatedTill.Id);
                }
                else
                {
                    result.msg = "Associated Till of the Till Payment must have Id > 0";
                    result.result = false;
                    return result;
                }

                if (tillPayment.AssociatedSafe.Id != 0)
                {
                    tillPayment.AssociatedSafe = dbContext.Safes.Find(tillPayment.AssociatedSafe.Id);
                }
                else
                {
                    result.msg = "Associated Safe of the Till Payment must have Id > 0";
                    result.result = false;
                    return result;
                }

                if (tillPayment.WorkGroup.Id != 0)
                {
                    tillPayment.WorkGroup = dbContext.WorkGroups.Find(tillPayment.WorkGroup.Id);
                }
                else
                {
                    result.result = false;
                    return result;
                }

                var user = dbContext.QbicleUser.Find(userId);

                if (tillPayment.Id == 0)
                {
                    tillPayment.CreatedBy = user;
                    tillPayment.CreatedDate = DateTime.UtcNow;
                    tillPayment.LastUpdatedBy = user;
                    tillPayment.LastUpdatedDate = DateTime.UtcNow;

                    dbContext.Entry(tillPayment).State = EntityState.Added;
                    dbContext.TillPayments.Add(tillPayment);
                    dbContext.SaveChanges();
                }
                else
                {
                    tillPayment.LastUpdatedBy = user;
                    tillPayment.LastUpdatedDate = DateTime.UtcNow;
                }

                var tradTillPaymentDb = dbContext.TillPayments.Find(tillPayment.Id);

                if (tradTillPaymentDb?.TillPaymentApprovalProcess != null)
                    return result;

                if (tradTillPaymentDb == null || tradTillPaymentDb.Status != TraderTillPaymentStatusEnum.PendingReview)
                    return result;

                tradTillPaymentDb.WorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var appDef = dbContext.TillPaymentApprovalDefinitions.FirstOrDefault(p => p.TillPaymentWorkGroup.Id == tradTillPaymentDb.WorkGroup.Id);

                //var refFull
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    TillPayment = new List<TillPayment> { tradTillPaymentDb },
                    Name = $"Trader Approval for Till payment #{tradTillPaymentDb.Id}",
                    Qbicle = tradTillPaymentDb.WorkGroup.Qbicle,
                    Topic = tradTillPaymentDb.WorkGroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader
                };

                tradTillPaymentDb.TillPaymentApprovalProcess = approval;
                tradTillPaymentDb.TillPaymentApprovalProcess.ApprovalRequestDefinition = appDef;
                tradTillPaymentDb.LastUpdatedDate = DateTime.UtcNow;
                tradTillPaymentDb.LastUpdatedBy = user;
                approval.ActivityMembers.AddRange(tradTillPaymentDb.WorkGroup.Members);
                dbContext.Entry(tradTillPaymentDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                var tillPaymentLog = new TillPaymentLog
                {
                    AssociatedTillPayment = tradTillPaymentDb,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    AssociatedSafe = tradTillPaymentDb.AssociatedSafe,
                    AssociatedTill = tradTillPaymentDb.AssociatedTill,
                    Amount = tradTillPaymentDb.Amount,
                    Direction = tradTillPaymentDb.Direction,
                    Approval = approval
                };

                var tillProcessLog = new TillPaymentProcessLog
                {
                    AssociatedTillPayment = tradTillPaymentDb,
                    AssociatedTillPaymentLog = tillPaymentLog,
                    TillPaymentStatus = tradTillPaymentDb.Status,
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

                dbContext.TillPaymentProcessLogs.Add(tillProcessLog);
                dbContext.Entry(tillProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();

                var notificationRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = user.GetFullName(),
                    ReminderMinutes = 0
                };
                notificationRule.Notification2Activity(activityNotification);

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, tillPayment);
                result.actionVal = 3;
                result.msg = ex.Message;
                result.result = false;
                return result;
            }
        }

        public ReturnJsonModel SaveCheckpoint(Checkpoint checkpoint, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, checkpoint);

                checkpoint.CreatedBy = dbContext.QbicleUser.Find(userId);
                checkpoint.CreatedDate = DateTime.UtcNow;

                if (checkpoint.VirtualTill != null && checkpoint.VirtualTill.Id > 0)
                {
                    checkpoint.VirtualTill = dbContext.Tills.Find(checkpoint.VirtualTill.Id);
                }
                else if (checkpoint.VirtualSafe != null && checkpoint.VirtualSafe.Id > 0)
                {
                    checkpoint.VirtualSafe = GetSafeById(checkpoint.VirtualSafe.Id);
                }

                if (checkpoint.WorkGroup != null && checkpoint.WorkGroup.Id > 0)
                {
                    checkpoint.WorkGroup = dbContext.WorkGroups.Find(checkpoint.WorkGroup.Id);
                }

                dbContext.Checkpoints.Add(checkpoint);
                dbContext.Entry(checkpoint).State = EntityState.Added;
                dbContext.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, checkpoint);
                result.actionVal = 3;
                result.msg = ex.Message;
                result.result = false;
                return result;
            }
        }

        public ReturnJsonModel UpdateTillPayment(int tillPaymentId, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            var tillPayment = dbContext.TillPayments.Find(tillPaymentId);

            try
            {
                tillPayment.LastUpdatedDate = DateTime.UtcNow;
                tillPayment.LastUpdatedBy = dbContext.QbicleUser.Find(userId);

                dbContext.Entry(tillPayment).State = EntityState.Modified;
                dbContext.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, tillPayment);
                result.result = false;
                result.msg = ex.Message;
                return result;
            }
        }

        public TillPayment GetTillPaymentByActivityId(int activityId)
        {
            try
            {
                return dbContext.TillPayments.FirstOrDefault(p => p.Discussion.Id == activityId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public Checkpoint GetCheckpointByActitvity(int activityId)
        {
            try
            {
                return dbContext.Checkpoints.FirstOrDefault(p => p.Discussion.Id == activityId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        // Manage Till Detail
        private List<TillTransaction> FilteredTillTransactions(List<TillTransaction> listTillTransactions, int locationId, string timeZone, UserSetting userDateTimeFormat,
            string keyword = "", string datetime = "", bool isApproved = false, string[] status = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, timeZone, userDateTimeFormat, keyword, datetime, isApproved);

                #region Filter

                var tillTransactions = listTillTransactions;

                //Filter by dates
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

                    if (!string.IsNullOrEmpty(datetime.Trim()))
                    {
                        datetime.ConvertDaterangeFormat(userDateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    }
                    tillTransactions = listTillTransactions.Where(q => q.TransactionDate >= startDate && q.TransactionDate < endDate).ToList();
                }

                if (status.Count() > 0 && status[0] != "")
                {
                    tillTransactions = tillTransactions.Where(q => status.Contains(q.Status, StringComparer.OrdinalIgnoreCase)).ToList();
                }
                //If the sales must be filered by Approved
                //Note: Approves => Sale Approved AND Sales Order Issued (If a Sales order is issued it can only be after it is approved.
                if (isApproved)
                {
                    tillTransactions = tillTransactions.Where(q => q.Status == TraderTillPaymentStatusEnum.Approved.GetDescription()).ToList();
                }

                //Filter by keyword

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower().Trim();
                    tillTransactions = tillTransactions.Where(q =>
                          q.DeviceName.ToLower().Contains(keyword) || q.TillName.ToLower().Contains(keyword) || q.SafeName.ToLower().Contains(keyword)).ToList();
                }

                #endregion Filter

                return tillTransactions;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, timeZone, userDateTimeFormat, keyword, datetime, isApproved);
                return null;
            }
        }

        public DataTablesResponse TillTransactionsSearch(IDataTablesRequest requestModel, string userId, int locationId, int domainId,
            List<TillTransaction> listTillTransaction, string keyword, string datetime, string timeZone, bool isApproved, UserSetting userDateTimeFormat
            , string[] status, CurrencySetting currencySetting)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, requestModel, userId, locationId, domainId,
                        listTillTransaction, keyword, datetime, timeZone, isApproved, userDateTimeFormat);

                //Get the filtered till payments, get all till payments not just the approved sales
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var tillTransactions = FilteredTillTransactions(listTillTransaction, locationId, timeZone, userDateTimeFormat, keyword, datetime, isApproved, status);

                if (tillTransactions == null)
                {
                    return null;
                }

                var totalTillTransactions = tillTransactions.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "DeviceName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DeviceName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "TillName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TillName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "TransactionDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TransactionDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "SafeName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SafeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "DirectionName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DirectionName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Amount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Balance":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Balance" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Difference":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Difference" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status.GetDescription":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status.GetDescription" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "TransactionDate desc";
                            break;
                    }
                }

                var sortedTillTransaction = tillTransactions.OrderBy(orderByString == string.Empty ? "TransactionDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var pagedTillTransaction = sortedTillTransaction.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var tillBalance = (pagedTillTransaction.FirstOrDefault(e => e.StatusObj == ApprovalReq.RequestStatusEnum.Approved)?.BalanceNumber ?? 0).ToCurrencySymbol(currencySetting) ?? "";
                return new DataTablesResponse(requestModel.Draw, pagedTillTransaction, totalTillTransactions, totalTillTransactions, tillBalance);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, requestModel, userId, locationId, domainId, keyword,
                    timeZone, isApproved);
                return null;
            }
        }

        public List<TillTransaction> GetListTillTransaction(int tillId, UserSetting userDateTimeFormat, TimeZoneInfo timezone, int domainId, CurrencySetting currencySettings)
        {
            var queryTillPayment = (from tillPayment in dbContext.TillPayments
                                    where tillPayment.AssociatedTill.Id == tillId
                                    select new TillTransaction()
                                    {
                                        Id = tillPayment.Id,
                                        DeviceName = "",
                                        SafeName = tillPayment.AssociatedSafe.Name,
                                        TillName = tillPayment.AssociatedTill.Name,
                                        AssociatedTillId = tillPayment.AssociatedTill.Id,
                                        AmountNumber = tillPayment.Amount,
                                        Difference = "",
                                        DirectionName = tillPayment.Direction == TillPaymentDirection.InToTill ? "In" : "Out",
                                        StatusObj = tillPayment.TillPaymentApprovalProcess.RequestStatus,
                                        TransactionDate = tillPayment.CreatedDate,
                                        isCheckpoint = false,
                                        isPosPayment = false,
                                        LabelStatusObj = tillPayment.Status,
                                    });

            var queryTillCheckpoint = (from tillCheckpoint in dbContext.Checkpoints
                                       where tillCheckpoint.VirtualTill.Id == tillId
                                       select new TillTransaction()
                                       {
                                           Id = tillCheckpoint.Id,
                                           DeviceName = "",
                                           SafeName = tillCheckpoint.VirtualSafe.Name,
                                           TillName = tillCheckpoint.VirtualTill.Name,
                                           AssociatedTillId = tillCheckpoint.VirtualTill.Id,
                                           AmountNumber = tillCheckpoint.Amount,
                                           Difference = "",
                                           DirectionName = "",
                                           StatusObj = ApprovalReq.RequestStatusEnum.Approved,
                                           TransactionDate = tillCheckpoint.CheckpointDate,
                                           isCheckpoint = true,
                                           isPosPayment = false,
                                           LabelStatusObj = TraderTillPaymentStatusEnum.Approved
                                       });

            var queryPosPayment = (from xref in dbContext.PosDeviceOrderXrefs
                                   from invoice in xref.Order.Sale.Invoices
                                   from payment in invoice.Payments
                                   where xref.Till.Id == tillId
                                   && payment.PaymentMethod.Name == PaymentMethodNameConst.Cash
                                   select new TillTransaction()
                                   {
                                       Id = payment.Id,
                                       DeviceName = xref.PosDevice.Name,
                                       SafeName = xref.Till.Safe.Name,
                                       TillName = xref.Till.Name,
                                       AssociatedTillId = xref.Till.Id,
                                       AmountNumber = payment.Amount,
                                       Difference = "",
                                       DirectionName = "In",
                                       StatusObj = ApprovalReq.RequestStatusEnum.Approved,
                                       TransactionDate = payment.CreatedDate,
                                       isCheckpoint = false,
                                       isPosPayment = true,
                                       LabelStatusObj = TraderTillPaymentStatusEnum.Approved
                                   });

            var tillTransactionList = queryTillPayment.Union(queryTillCheckpoint).Union(queryPosPayment).OrderBy(p => p.TransactionDate).ToList();
            decimal _balance = 0;
            foreach (var tillTransactionItem in tillTransactionList)
            {
                tillTransactionItem.Amount = tillTransactionItem.AmountNumber.ToDecimalPlace(currencySettings);
                tillTransactionItem.TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(tillTransactionItem.TransactionDate, timezone);
                tillTransactionItem.TransactionDateString = tillTransactionItem.TransactionDate.ToString(userDateTimeFormat.DateTimeFormat);

                //if (!tillTransactionItem.isCheckpoint && !tillTransactionItem.isPosPayment)
                //{
                //    tillTransactionItem.LabelStatus = GetLabelStatusFromEnum(tillTransactionItem.LabelStatusObj);
                //    tillTransactionItem.Status = tillTransactionItem.StatusObj.GetDescription();
                //}
                tillTransactionItem.LabelStatus = GetLabelStatusFromEnum(tillTransactionItem.LabelStatusObj);
                tillTransactionItem.Status = tillTransactionItem.StatusObj.GetDescription();

                if (tillTransactionItem.isCheckpoint)
                {
                    tillTransactionItem.Balance = _balance.ToDecimalPlace(currencySettings);
                    tillTransactionItem.BalanceNumber = _balance;
                    var _difference = tillTransactionItem.AmountNumber - tillTransactionItem.BalanceNumber;
                    tillTransactionItem.Difference = _difference.ToDecimalPlace(currencySettings);
                }
                else
                {
                    if (tillTransactionItem.DirectionName.ToLower() == "in")
                    {
                        if (tillTransactionItem.isPosPayment || (!tillTransactionItem.isPosPayment
                            && tillTransactionItem.Status.ToLower().Equals("approved")))
                        {
                            _balance += tillTransactionItem.AmountNumber;
                        }
                    }
                    else
                    {
                        if (tillTransactionItem.Status.ToLower().Equals("approved"))
                        {
                            _balance -= tillTransactionItem.AmountNumber;
                        }
                    }
                    tillTransactionItem.Balance = _balance.ToDecimalPlace(currencySettings);
                    tillTransactionItem.BalanceNumber = _balance;
                }
            }

            return tillTransactionList;
        }

        private List<SafeTransaction> FilteredSafeTransactions(List<SafeTransaction> listSafeTransactions, int locationId, string timeZone, UserSetting userDateTimeFormat,
            string keyword = "", string datetime = "", bool isApproved = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, timeZone, userDateTimeFormat, keyword,
                        datetime, isApproved);
                var startDate = new DateTime();
                var endDate = new DateTime();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    datetime.ConvertDaterangeFormat(userDateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                }

                #region Filter

                var safeTransactions = listSafeTransactions;

                //Filter by dates
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    safeTransactions = listSafeTransactions.Where(q => q.TransactionDate >= startDate && q.TransactionDate < endDate).ToList();
                }

                //If the sales must be filered by Approved
                //Note: Approves => Sale Approved AND Sales Order Issued (If a Sales order is issued it can only be after it is approved.
                if (isApproved)
                {
                    safeTransactions = safeTransactions.Where(q => q.Status == TraderTillPaymentStatusEnum.Approved.GetDescription()).ToList();
                }

                //Filter by keyword
                keyword = keyword.ToLower().Trim();

                if (!string.IsNullOrEmpty(keyword))
                    safeTransactions = safeTransactions.Where(q =>
                        q.DeviceName.ToLower().Contains(keyword)
                        || q.TillName.ToLower().Contains(keyword)
                        || q.SafeName.ToLower().Contains(keyword)).ToList();

                #endregion Filter

                return safeTransactions;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, timeZone, userDateTimeFormat, keyword, datetime, isApproved);
                return null;
            }
        }

        public DataTablesResponse SafeTransactionsSearch(IDataTablesRequest requestModel, string userId, int locationId, int domainId,
            List<SafeTransaction> listSafeTransaction, string keyword, string datetime, string timeZone, bool isApproved, UserSetting userDateTimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, requestModel, userId, locationId, domainId,
                        listSafeTransaction, keyword, datetime, timeZone, isApproved, userDateTimeFormat);
                //Get the filtered till payments, get all till payments not just the approved sales
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var safeTransactions = FilteredSafeTransactions(listSafeTransaction, locationId, timeZone, userDateTimeFormat, keyword, datetime, isApproved);

                if (safeTransactions == null)
                {
                    return null;
                }

                var totalSafeTransactions = safeTransactions.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "TillName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TillName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "TransactionDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TransactionDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "DirectionName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DirectionName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Amount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Balance":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Balance" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Difference":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Difference" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status.GetDescription":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status.GetDescription" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "TransactionDate desc";
                            break;
                    }
                }

                var sortedTillTransaction = safeTransactions.OrderBy(orderByString == string.Empty ? "TransactionDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var pagedTillTransaction = sortedTillTransaction.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                return new DataTablesResponse(requestModel.Draw, pagedTillTransaction, totalSafeTransactions, totalSafeTransactions);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, requestModel, userId, locationId, domainId, keyword,
                    timeZone, isApproved);
                return null;
            }
        }

        public List<SafeTransaction> GetListSafeTransaction(int safeId, UserSetting userDateTimeFormat, TimeZoneInfo timezone, int domainId)
        {
            var resultList = new List<SafeTransaction>();
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

            var safe = GetSafeById(safeId);

            var listTillPayment = dbContext.TillPayments.Where(p => p.AssociatedSafe.Id == safeId).ToList();

            var listSafeCheckpoint = dbContext.Checkpoints.Where(p => p.VirtualSafe.Id == safeId).ToList();

            foreach (var tillPaymentItem in listTillPayment)
            {
                resultList.Add(new SafeTransaction
                {
                    Id = tillPaymentItem.Id,
                    DeviceName = "",
                    SafeName = tillPaymentItem.AssociatedSafe.Name,
                    TillName = tillPaymentItem.AssociatedTill.Name,
                    AssociatedSafeId = tillPaymentItem.AssociatedSafe.Id,
                    Amount = tillPaymentItem.Amount.ToDecimalPlace(currencySettings),
                    AmountNumber = tillPaymentItem.Amount,
                    Difference = "",
                    DirectionName = tillPaymentItem.Direction == TillPaymentDirection.InToTill ? "Out" : "In",
                    Status = tillPaymentItem.TillPaymentApprovalProcess.RequestStatus.GetDescription(),
                    LabelStatus = GetLabelStatusFromEnum(tillPaymentItem.Status),
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(tillPaymentItem.CreatedDate, timezone),
                    TransactionDateString = TimeZoneInfo.ConvertTimeFromUtc(tillPaymentItem.CreatedDate, timezone).ToString(userDateTimeFormat.DateTimeFormat),
                    isTillPayment = true
                });
            }

            foreach (var safeCheckpointItem in listSafeCheckpoint)
            {
                resultList.Add(new SafeTransaction
                {
                    Id = safeCheckpointItem.Id,
                    isCheckpoint = true,
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(safeCheckpointItem.CheckpointDate, timezone),
                    TransactionDateString = TimeZoneInfo.ConvertTimeFromUtc(safeCheckpointItem.CheckpointDate, timezone).ToString(userDateTimeFormat.DateTimeFormat),
                    Amount = safeCheckpointItem.Amount.ToDecimalPlace(currencySettings),
                    AmountNumber = safeCheckpointItem.Amount,
                    AssociatedSafeId = safeCheckpointItem.VirtualSafe.Id,
                });
            }

            //All Transfers (CashAccountTransactions where Type = Transfer) associated with the Account associated with the Safe.
            var listInTransfers = dbContext.CashAccountTransactions.Where(c => c.DestinationAccount.Id == safe.CashAndBankAccount.Id && c.Type == CashAccountTransactionTypeEnum.Transfer).ToList();
            var listOutTransfers = dbContext.CashAccountTransactions.Where(c => c.OriginatingAccount.Id == safe.CashAndBankAccount.Id && c.Type == CashAccountTransactionTypeEnum.Transfer).ToList();

            foreach (var InTransferItem in listInTransfers)
            {
                resultList.Add(new SafeTransaction
                {
                    Id = InTransferItem.Id,
                    isTransfer = true,
                    TillName = InTransferItem.DestinationAccount.Name,
                    SafeName = safe.Name,
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(InTransferItem.CreatedDate, timezone),
                    TransactionDateString = TimeZoneInfo.ConvertTimeFromUtc(InTransferItem.CreatedDate, timezone).ToString(userDateTimeFormat.DateTimeFormat),
                    Amount = InTransferItem.Amount.ToDecimalPlace(currencySettings),
                    AmountNumber = InTransferItem.Amount,
                    AssociatedSafeId = safeId,
                    DirectionName = "In",
                    Status = InTransferItem.Status.GetDescription(),
                    LabelStatus = getLabelStatusTransfer(InTransferItem.Status)
                });
            }

            foreach (var OutTransferItem in listOutTransfers)
            {
                resultList.Add(new SafeTransaction
                {
                    Id = OutTransferItem.Id,
                    isTransfer = true,
                    TillName = OutTransferItem.DestinationAccount.Name,
                    SafeName = safe.Name,
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(OutTransferItem.CreatedDate, timezone),
                    TransactionDateString = TimeZoneInfo.ConvertTimeFromUtc(OutTransferItem.CreatedDate, timezone).ToString(userDateTimeFormat.DateTimeFormat),
                    Amount = OutTransferItem.Amount.ToDecimalPlace(currencySettings),
                    AmountNumber = OutTransferItem.Amount,
                    AssociatedSafeId = safeId,
                    DirectionName = "Out",
                    Status = OutTransferItem.Status.GetDescription(),
                    LabelStatus = getLabelStatusTransfer(OutTransferItem.Status)
                });
            }

            var safeTransactions = resultList.OrderBy(p => p.TransactionDate);
            decimal _balance = 0;
            foreach (var safeTransactionItem in safeTransactions)
            {
                if (safeTransactionItem.isCheckpoint)
                {
                    safeTransactionItem.Balance = _balance.ToDecimalPlace(currencySettings);
                    safeTransactionItem.BalanceNumber = _balance;
                    var difference = safeTransactionItem.AmountNumber - safeTransactionItem.BalanceNumber;
                    safeTransactionItem.Difference = difference.ToDecimalPlace(currencySettings);
                    safeTransactionItem.DifferenceNumber = difference;
                }
                else
                {
                    if (safeTransactionItem.DirectionName.ToLower() == "in")
                    {
                        if (safeTransactionItem.Status.ToLower().Equals("approved"))
                        {
                            _balance += safeTransactionItem.AmountNumber;
                        }
                    }
                    else
                    {
                        if (safeTransactionItem.Status.ToLower().Equals("approved"))
                        {
                            _balance -= safeTransactionItem.AmountNumber;
                        }
                    }
                    safeTransactionItem.BalanceNumber = _balance;
                    safeTransactionItem.Balance = _balance.ToDecimalPlace(currencySettings);
                }
            }

            var result = safeTransactions.Reverse().ToList();

            return result;
        }

        // Add post discussion
        public bool AddPostToSocialPostDiscussion(int socialPostId, QbiclePost post,
            string currentUserId, int currentQbicleId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add posts to social post discussion", currentUserId, null, socialPostId, post, currentUserId, currentQbicleId);

                var socialPostReq = dbContext.Activities.Find(socialPostId);
                socialPostReq.TimeLineDate = DateTime.UtcNow;
                socialPostReq.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                socialPostReq.Posts.Add(post);
                socialPostReq.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.Activities.Attach(socialPostReq);
                dbContext.Entry(socialPostReq).State = EntityState.Modified;
                dbContext.SaveChanges();
                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = socialPostReq.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.DiscussionCreation,
                    AppendToPageName = ApplicationPageName.Discussion,
                    AppendToPageId = socialPostId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, socialPostId, post, currentUserId, currentQbicleId);
                return false;
            }
        }

        // Discussion
        public bool CreateTillPaymentDiscussion(int paymentId, string userId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, paymentId, userId);

            var tillPayment = dbContext.TillPayments.Find(paymentId);

            if (tillPayment.Discussion == null)
            {
                try
                {
                    var workgroup = tillPayment.WorkGroup;
                    var currentQbicleId = workgroup.Qbicle.Id;
                    var currentQbicle = dbContext.Qbicles.Find(currentQbicleId);
                    if (currentQbicle == null)
                        return false;
                    var user = dbContext.QbicleUser.Find(userId);
                    var newDiscussion = new QbicleDiscussion();
                    newDiscussion.Id = new int();
                    newDiscussion.StartedBy = user;
                    newDiscussion.StartedDate = DateTime.UtcNow;
                    newDiscussion.State = QbicleActivity.ActivityStateEnum.Open;
                    newDiscussion.Qbicle = currentQbicle;
                    newDiscussion.TimeLineDate = DateTime.UtcNow;
                    newDiscussion.Name = "Till Payment " + tillPayment.Id;
                    newDiscussion.Summary = "";
                    newDiscussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.CashManagement;
                    newDiscussion.App = QbicleActivity.ActivityApp.Trader;
                    newDiscussion.ActivityMembers.AddRange(currentQbicle.Members);

                    dbContext.Discussions.Add(newDiscussion);
                    dbContext.Entry(newDiscussion).State = System.Data.Entity.EntityState.Added;

                    tillPayment.Discussion = newDiscussion;
                    tillPayment.LastUpdatedBy = user;
                    tillPayment.LastUpdatedDate = DateTime.UtcNow;
                    dbContext.Entry(tillPayment).State = System.Data.Entity.EntityState.Modified;
                    var saveRs = dbContext.SaveChanges();

                    if (saveRs > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, paymentId);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CreateCheckpointDiscussion(int checkpointId, string userId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, checkpointId, userId);

            var checkpoint = dbContext.Checkpoints.Find(checkpointId);

            if (checkpoint.Discussion == null)
            {
                try
                {
                    var workgroup = checkpoint.WorkGroup;

                    if (workgroup == null || workgroup.Id <= 0)
                    {
                        return false;
                    }

                    var currentQbicleId = workgroup.Qbicle.Id;
                    var currentQbicle = dbContext.Qbicles.Find(currentQbicleId);
                    if (currentQbicle == null)
                        return false;
                    var user = dbContext.QbicleUser.Find(userId);
                    var newDiscussion = new QbicleDiscussion();
                    newDiscussion.Id = new int();
                    newDiscussion.StartedBy = user;
                    newDiscussion.StartedDate = DateTime.UtcNow;
                    newDiscussion.State = QbicleActivity.ActivityStateEnum.Open;
                    newDiscussion.Qbicle = currentQbicle;
                    newDiscussion.TimeLineDate = DateTime.UtcNow;
                    newDiscussion.Name = (checkpoint.VirtualTill != null ? "Till Checkpoint " : "Safe Checkpoint ") + checkpoint.Id;
                    newDiscussion.Summary = "";
                    newDiscussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.CashManagement;
                    newDiscussion.App = QbicleActivity.ActivityApp.Trader;
                    newDiscussion.ActivityMembers.AddRange(currentQbicle.Members);

                    dbContext.Discussions.Add(newDiscussion);
                    dbContext.Entry(newDiscussion).State = System.Data.Entity.EntityState.Added;

                    checkpoint.Discussion = newDiscussion;
                    dbContext.Entry(checkpoint).State = System.Data.Entity.EntityState.Modified;
                    var saveRs = dbContext.SaveChanges();

                    if (saveRs > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, checkpointId);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Common
        public void TillPaymentApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var tillPaymentDb = approval.TillPayment.FirstOrDefault();
                if (tillPaymentDb == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        tillPaymentDb.Status = TraderTillPaymentStatusEnum.PendingReview;
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        tillPaymentDb.Status = TraderTillPaymentStatusEnum.PendingApproval;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        tillPaymentDb.Status = TraderTillPaymentStatusEnum.Approved;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                        tillPaymentDb.Status = TraderTillPaymentStatusEnum.Denied;
                        break;

                    case ApprovalReq.RequestStatusEnum.Discarded:
                        tillPaymentDb.Status = TraderTillPaymentStatusEnum.Discarded;
                        break;
                }
                tillPaymentDb.LastUpdatedDate = DateTime.UtcNow;
                tillPaymentDb.LastUpdatedBy = approval.ApprovedOrDeniedAppBy;
                dbContext.Entry(tillPaymentDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                //logging

                var tillPaymentLog = new TillPaymentLog
                {
                    Amount = tillPaymentDb.Amount,
                    Approval = tillPaymentDb.TillPaymentApprovalProcess,
                    AssociatedSafe = tillPaymentDb.AssociatedSafe,
                    AssociatedTill = tillPaymentDb.AssociatedTill,
                    AssociatedTillPayment = tillPaymentDb,
                    Direction = tillPaymentDb.Direction,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = approval.ApprovedOrDeniedAppBy
                };

                var tillPaymentProcessLog = new TillPaymentProcessLog
                {
                    AssociatedTillPayment = tillPaymentDb,
                    AssociatedTillPaymentLog = tillPaymentLog,
                    TillPaymentStatus = tillPaymentDb.Status,
                    CreatedBy = tillPaymentDb.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = tillPaymentDb.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.TillPaymentProcessLogs.Add(tillPaymentProcessLog);
                dbContext.Entry(tillPaymentProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public string GetLabelStatusFromEnum(TraderTillPaymentStatusEnum status)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, status);
            var label = "";
            switch (status)
            {
                case TraderTillPaymentStatusEnum.Approved:
                    label = StatusLabelStyle.Approved;
                    break;

                case TraderTillPaymentStatusEnum.Denied:
                    label = StatusLabelStyle.Denied;
                    break;

                case TraderTillPaymentStatusEnum.Discarded:
                    label = StatusLabelStyle.Discarded;
                    break;

                case TraderTillPaymentStatusEnum.PendingReview:
                    label = StatusLabelStyle.Pending;
                    break;

                case TraderTillPaymentStatusEnum.PendingApproval:
                    label = StatusLabelStyle.Reviewed;
                    break;
            }
            return label;
        }

        public string getLabelStatusTransfer(TraderPaymentStatusEnum status)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, status);
            var label = "";
            switch (status)
            {
                case TraderPaymentStatusEnum.Draft:
                    label = StatusLabelStyle.Draft;
                    break;

                case TraderPaymentStatusEnum.PaymentApproved:
                    label = StatusLabelStyle.Approved;
                    break;

                case TraderPaymentStatusEnum.PaymentDenied:
                    label = StatusLabelStyle.Denied;
                    break;

                case TraderPaymentStatusEnum.PaymentDiscarded:
                    label = StatusLabelStyle.Discarded;
                    break;

                case TraderPaymentStatusEnum.PendingReview:
                    label = StatusLabelStyle.Pending;
                    break;

                case TraderPaymentStatusEnum.PendingApproval:
                    label = StatusLabelStyle.Reviewed;
                    break;
            }
            return label;
        }

        public List<ApprovalStatusTimeline> TillPaymentApprovalStatusTimeline(int id, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var timeline = new List<ApprovalStatusTimeline>();
                var logs = dbContext.TillPaymentProcessLogs.Where(e => e.AssociatedTillPayment.Id == id).OrderByDescending(d => d.CreatedDate);
                string icon = StatusLabelStyle.Reviewed;

                foreach (var log in logs)
                {
                    switch (log.TillPaymentStatus)
                    {
                        case TraderTillPaymentStatusEnum.PendingReview:
                            icon = "fa fa-info bg-aqua";
                            break;

                        case TraderTillPaymentStatusEnum.PendingApproval:
                            icon = "fa fa-truck bg-yellow";
                            break;

                        case TraderTillPaymentStatusEnum.Approved:
                            icon = "fa fa-check bg-green";
                            break;

                        case TraderTillPaymentStatusEnum.Denied:
                            icon = "fa fa-warning bg-red";
                            break;

                        case TraderTillPaymentStatusEnum.Discarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = log.TillPaymentStatus.GetDescription(),
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

        public string getSafeBalance(int safeId, UserSetting userDateTimeFormat, TimeZoneInfo timezone, int domainId)
        {
            var resultList = new List<SafeTransaction>();
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

            var safe = GetSafeById(safeId);

            var listTillPayment = dbContext.TillPayments.Where(p => p.AssociatedSafe.Id == safeId).ToList();

            foreach (var tillPaymentItem in listTillPayment)
            {
                resultList.Add(new SafeTransaction
                {
                    Id = tillPaymentItem.Id,
                    DeviceName = "",
                    SafeName = tillPaymentItem.AssociatedSafe.Name,
                    TillName = tillPaymentItem.AssociatedTill.Name,
                    AssociatedSafeId = tillPaymentItem.AssociatedSafe.Id,
                    Amount = tillPaymentItem.Amount.ToDecimalPlace(currencySettings),
                    AmountNumber = tillPaymentItem.Amount,
                    Difference = "",
                    DirectionName = tillPaymentItem.Direction == TillPaymentDirection.InToTill ? "Out" : "In",
                    Status = tillPaymentItem.TillPaymentApprovalProcess.RequestStatus.GetDescription(),
                    LabelStatus = GetLabelStatusFromEnum(tillPaymentItem.Status),
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(tillPaymentItem.CreatedDate, timezone),
                    TransactionDateString = TimeZoneInfo.ConvertTimeFromUtc(tillPaymentItem.CreatedDate, timezone).ToString(userDateTimeFormat.DateTimeFormat),
                    isTillPayment = true
                });
            }

            //All Transfers (CashAccountTransactions where Type = Transfer) associated with the Account associated with the Safe.
            var listInTransfers = dbContext.CashAccountTransactions.Where(c => c.DestinationAccount.Id == safe.CashAndBankAccount.Id && c.Type == CashAccountTransactionTypeEnum.Transfer).ToList();
            var listOutTransfers = dbContext.CashAccountTransactions.Where(c => c.OriginatingAccount.Id == safe.CashAndBankAccount.Id && c.Type == CashAccountTransactionTypeEnum.Transfer).ToList();

            foreach (var InTransferItem in listInTransfers)
            {
                resultList.Add(new SafeTransaction
                {
                    Id = InTransferItem.Id,
                    isTransfer = true,
                    TillName = InTransferItem.DestinationAccount.Name,
                    SafeName = safe.Name,
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(InTransferItem.CreatedDate, timezone),
                    TransactionDateString = TimeZoneInfo.ConvertTimeFromUtc(InTransferItem.CreatedDate, timezone).ToString(userDateTimeFormat.DateTimeFormat),
                    Amount = InTransferItem.Amount.ToDecimalPlace(currencySettings),
                    AmountNumber = InTransferItem.Amount,
                    AssociatedSafeId = safeId,
                    DirectionName = "In",
                    Status = InTransferItem.Status.GetDescription(),
                    LabelStatus = getLabelStatusTransfer(InTransferItem.Status)
                });
            }

            foreach (var OutTransferItem in listOutTransfers)
            {
                resultList.Add(new SafeTransaction
                {
                    Id = OutTransferItem.Id,
                    isTransfer = true,
                    TillName = OutTransferItem.DestinationAccount.Name,
                    SafeName = safe.Name,
                    TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(OutTransferItem.CreatedDate, timezone),
                    TransactionDateString = TimeZoneInfo.ConvertTimeFromUtc(OutTransferItem.CreatedDate, timezone).ToString(userDateTimeFormat.DateTimeFormat),
                    Amount = OutTransferItem.Amount.ToDecimalPlace(currencySettings),
                    AmountNumber = OutTransferItem.Amount,
                    AssociatedSafeId = safeId,
                    DirectionName = "Out",
                    Status = OutTransferItem.Status.GetDescription(),
                    LabelStatus = getLabelStatusTransfer(OutTransferItem.Status)
                });
            }

            var safeTransactions = resultList.OrderBy(p => p.TransactionDate);
            decimal _balance = 0;
            foreach (var safeTransactionItem in safeTransactions)
            {
                if (safeTransactionItem.isCheckpoint)
                {
                }
                else
                {
                    if (safeTransactionItem.DirectionName.ToLower() == "in")
                    {
                        if (safeTransactionItem.Status.ToLower().Equals("approved"))
                        {
                            _balance += safeTransactionItem.AmountNumber;
                        }
                    }
                    else
                    {
                        if (safeTransactionItem.Status.ToLower().Equals("approved"))
                        {
                            _balance -= safeTransactionItem.AmountNumber;
                        }
                    }
                    safeTransactionItem.BalanceNumber = _balance;
                    safeTransactionItem.Balance = _balance.ToDecimalPlace(currencySettings);
                }
            }

            return _balance.ToDecimalPlace(currencySettings);
        }

        public decimal GetTillBalance(int tillId)
        {
            var queryTillPayment = (from tillPayment in dbContext.TillPayments
                                    where tillPayment.AssociatedTill.Id == tillId
                                    select new TillTransaction()
                                    {
                                        Id = tillPayment.Id,
                                        DeviceName = "",
                                        SafeName = tillPayment.AssociatedSafe.Name,
                                        TillName = tillPayment.AssociatedTill.Name,
                                        AssociatedTillId = tillPayment.AssociatedTill.Id,
                                        AmountNumber = tillPayment.Amount,
                                        Difference = "",
                                        DirectionName = tillPayment.Direction == TillPaymentDirection.InToTill ? "In" : "Out",
                                        StatusObj = tillPayment.TillPaymentApprovalProcess.RequestStatus,
                                        TransactionDate = tillPayment.CreatedDate,
                                        isCheckpoint = false,
                                        isPosPayment = false,
                                        LabelStatusObj = tillPayment.Status,
                                    });

            var queryTillCheckpoint = (from tillCheckpoint in dbContext.Checkpoints
                                       where tillCheckpoint.VirtualTill.Id == tillId
                                       select new TillTransaction()
                                       {
                                           Id = tillCheckpoint.Id,
                                           DeviceName = "",
                                           SafeName = tillCheckpoint.VirtualSafe.Name,
                                           TillName = tillCheckpoint.VirtualTill.Name,
                                           AssociatedTillId = tillCheckpoint.VirtualTill.Id,
                                           AmountNumber = tillCheckpoint.Amount,
                                           Difference = "",
                                           DirectionName = "",
                                           StatusObj = ApprovalReq.RequestStatusEnum.Approved,
                                           TransactionDate = tillCheckpoint.CheckpointDate,
                                           isCheckpoint = true,
                                           isPosPayment = false,
                                           LabelStatusObj = TraderTillPaymentStatusEnum.Approved
                                       });

            var queryPosPayment = (from xref in dbContext.PosDeviceOrderXrefs
                                   from invoice in xref.Order.Sale.Invoices
                                   from payment in invoice.Payments
                                   where xref.Till.Id == tillId
                                   && payment.PaymentMethod.Name == PaymentMethodNameConst.Cash
                                   select new TillTransaction()
                                   {
                                       Id = payment.Id,
                                       DeviceName = xref.PosDevice.Name,
                                       SafeName = xref.Till.Safe.Name,
                                       TillName = xref.Till.Name,
                                       AssociatedTillId = xref.Till.Id,
                                       AmountNumber = payment.Amount,
                                       Difference = "",
                                       DirectionName = "In",
                                       StatusObj = ApprovalReq.RequestStatusEnum.Approved,
                                       TransactionDate = payment.CreatedDate,
                                       isCheckpoint = false,
                                       isPosPayment = true,
                                       LabelStatusObj = TraderTillPaymentStatusEnum.Approved
                                   });

            var tillTransactionList = queryTillPayment.Union(queryTillCheckpoint).Union(queryPosPayment).OrderBy(p => p.TransactionDate).ToList();
            decimal _balance = 0;
            foreach (var tillTransactionItem in tillTransactionList)
            {
                if (!tillTransactionItem.isCheckpoint && !tillTransactionItem.isPosPayment)
                {
                    tillTransactionItem.Status = tillTransactionItem.StatusObj.GetDescription();
                }

                if (tillTransactionItem.isCheckpoint)
                {
                    tillTransactionItem.BalanceNumber = _balance;
                }
                else
                {
                    if (tillTransactionItem.DirectionName.ToLower() == "in")
                    {
                        if (tillTransactionItem.isPosPayment || (!tillTransactionItem.isPosPayment
                            && tillTransactionItem.Status.ToLower().Equals("approved")))
                        {
                            _balance += tillTransactionItem.AmountNumber;
                        }
                    }
                    else
                    {
                        if (tillTransactionItem.Status.ToLower().Equals("approved"))
                        {
                            _balance -= tillTransactionItem.AmountNumber;
                        }
                    }
                    tillTransactionItem.BalanceNumber = _balance;
                }
            }

            return tillTransactionList.LastOrDefault()?.BalanceNumber ?? 0;
        }
    }
}