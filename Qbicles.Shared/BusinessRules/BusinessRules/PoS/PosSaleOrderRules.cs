using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules.PoS
{
    public class PosSaleOrderRules
    {
        private ApplicationDbContext dbContext;

        public PosSaleOrderRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ReturnJsonModel SaveDiscussionFromTraderReport(QueueOrder queueOrder, string userId, int qbicleId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDiscussion", userId, null, queueOrder, qbicleId);

                var queueOrderDb = dbContext.QueueOrders.Find(queueOrder.Id);
                if (queueOrder.Discussion == null || queueOrderDb == null)
                {
                    result.actionVal = 3;
                    result.msg = "Discussion or queueOrder is empty";
                    return result;
                }
                if (queueOrder.Discussion.ActivityMembers.Any())
                {
                    for (int i = 0; i < queueOrder.Discussion.ActivityMembers.Count; i++)
                    {
                        queueOrder.Discussion.ActivityMembers[i] = dbContext.QbicleUser.Find(queueOrder.Discussion.ActivityMembers[i].Id);
                    }
                }

                if (queueOrder.Discussion.Topic != null && queueOrder.Discussion.Topic.Id > 0)
                {
                    queueOrder.Discussion.Topic = dbContext.Topics.Find(queueOrder.Discussion.Topic.Id);
                }
                queueOrder.Discussion.Qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                queueOrder.Discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.QbicleDiscussion;
                queueOrder.Discussion.TimeLineDate = DateTime.UtcNow;
                queueOrder.Discussion.App = QbicleActivity.ActivityApp.Trader;
                queueOrder.Discussion.ActivityType = QbicleActivity.ActivityTypeEnum.DiscussionActivity;
                queueOrder.Discussion.StartedDate = DateTime.UtcNow;
                queueOrder.Discussion.StartedBy = dbContext.QbicleUser.Find(userId);
                if (queueOrder.Discussion.Id == 0)
                {
                    queueOrderDb.Discussion = queueOrder.Discussion;
                    dbContext.SaveChanges();
                    result.msgId = queueOrder.Id.ToString();
                }
                else
                {
                    queueOrderDb.Discussion.Name = queueOrder.Discussion.Name;
                    queueOrderDb.Discussion.ExpiryDate = queueOrder.Discussion.ExpiryDate;
                    queueOrderDb.Discussion.DiscussionType = queueOrder.Discussion.DiscussionType;
                    queueOrderDb.Discussion.FeaturedImageUri = queueOrder.Discussion.FeaturedImageUri;
                    queueOrderDb.Discussion.Summary = queueOrder.Discussion.Summary;
                    queueOrderDb.Discussion.ActivityMembers.Clear();
                    dbContext.SaveChanges();
                    queueOrderDb.Discussion.ActivityMembers = queueOrder.Discussion.ActivityMembers;
                    dbContext.SaveChanges();
                    result.actionVal = 2;
                }
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, queueOrder, qbicleId);
                result.actionVal = 3;
                result.msg = ex.Message;
                return result;
            }
        }

        public QueueOrder GetQueueOrderById(int id)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "GetQueueOrderById", null, null, id);

            var queue = dbContext.QueueOrders.Find(id);
            if (queue != null)
            {
                return queue;
            }
            return new QueueOrder();
        }

        public PosPaymentMenuSources GetSourceItems(int domainId)
        {
            var item = new PosPaymentMenuSources();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Source Items", null, null, domainId);

                var queueOrders = dbContext.QueueOrders.Where(q => q.Sale != null && q.Sale.Location.Domain.Id == domainId);
                queueOrders = queueOrders.Where(q => q.Sale.Invoices.Any(i => i.Payments.Any()));
                var orderOLists = queueOrders.Select(s => new
                {
                    QueueOrder = s,
                    Payments = s.Sale.Invoices.SelectMany(i => i.Payments)
                });
                var queueQs = orderOLists.SelectMany(s => s.Payments.Select(p => new
                {
                    s.QueueOrder,
                    Payment = p
                }));
                var payments = queueQs.Select(p => new
                {
                    Device = p.QueueOrder.PosDeviceOrder != null ? new
                    {
                        p.QueueOrder.PosDeviceOrder.PosDevice.Id,
                        p.QueueOrder.PosDeviceOrder.PosDevice.Name
                    } : new { Id = 0, Name = "" },
                    p.QueueOrder.Cashier,
                    Account = p.Payment.DestinationAccount != null ? p.Payment.DestinationAccount : null,
                    PaymentMethod = p.Payment.PaymentMethod != null ? p.Payment.PaymentMethod : null,
                    p.QueueOrder.Sale.Location
                }).Distinct().ToList();
                if (payments.Any())
                {
                    // cashier
                    if (payments.Where(q => q.Cashier != null).Any())
                    {
                        var itemSelects = payments.Where(q => q.Cashier != null).Select(q => new ApplicationUser()
                        {
                            Id = q.Cashier.Id,
                            DisplayUserName = q.Cashier.DisplayUserName,
                            Forename = q.Cashier.Forename,
                            Surname = q.Cashier.Surname
                        }).ToList();
                        foreach (var itemSelect in itemSelects)
                        {
                            if (!item.Cashiers.Any(q => q.Id == itemSelect.Id))
                            {
                                item.Cashiers.Add(itemSelect);
                            }
                        }
                    }
                    // method
                    if (payments.Where(q => q.PaymentMethod != null).Any())
                    {
                        var itemSelects = payments.Where(q => q.PaymentMethod != null).Select(q => new PaymentMethod()
                        {
                            Id = q.PaymentMethod.Id,
                            Name = q.PaymentMethod.Name
                        }).OrderBy(n => n.Name).ToList();
                        foreach (var itemSelect in itemSelects)
                        {
                            if (!item.Methods.Any(q => q.Id == itemSelect.Id))
                            {
                                item.Methods.Add(itemSelect);
                            }
                        }
                    }
                    // device
                    if (payments.Where(q => q.Device != null).Any())
                    {
                        var itemSelects = payments.Where(q => q.Device != null && q.Device.Id > 0).Select(q => new PosDevice()
                        {
                            Id = q.Device.Id,
                            Name = q.Device.Name
                        }).OrderBy(n => n.Name).ToList();
                        foreach (var itemSelect in itemSelects)
                        {
                            if (!item.PosDevices.Any(q => q.Id == itemSelect.Id))
                            {
                                item.PosDevices.Add(itemSelect);
                            }
                        }
                    }
                    // account
                    if (payments.Where(q => q.Account != null).Any())
                    {
                        var itemSelects = payments.Where(q => q.Account != null).Select(q => new TraderCashAccount()
                        {
                            Id = q.Account.Id,
                            Name = q.Account.Name
                        }).OrderBy(n => n.Name).ToList();
                        foreach (var itemSelect in itemSelects)
                        {
                            if (!item.TraderCashAccounts.Any(q => q.Id == itemSelect.Id))
                            {
                                item.TraderCashAccounts.Add(itemSelect);
                            }
                        }
                    }
                    // location
                    if (payments.Where(q => q.Location != null).Any())
                    {
                        var locations = payments.Where(q => q.Location != null).Select(q => new TraderLocation()
                        {
                            Id = q.Location.Id,
                            Name = q.Location.Name
                        }).OrderBy(n => n.Name).ToList();
                        foreach (var location in locations)
                        {
                            if (!item.TraderLocations.Any(q => q.Id == location.Id))
                            {
                                item.TraderLocations.Add(location);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDiscussion", null, domainId);

                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="domainId"></param>
        /// <param name="currencySettings"></param>
        /// <param name="keyword"></param>
        /// <param name="datelimit"></param>
        /// <param name="locations"></param>
        /// <param name="methods"></param>
        /// <param name="accounts"></param>
        /// <param name="cashiers"></param>
        /// <param name="devices"></param>
        /// <param name="userDateTimeFormat"></param>
        /// <returns></returns>
        public DataTablesResponse GetPosPaymentDataTable(IDataTablesRequest requestModel, int domainId, CurrencySetting currencySettings, string keyword,
            string datelimit, int[] locations, int[] methods, int[] accounts, string[] cashiers, int[] devices, UserSetting userDateTimeFormat)
        {
            var totalRecords = 0;
            var response = new DataTablesResponse(requestModel.Draw, new List<PosPaymentCustom>(), 0, 0);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "domainId", null, null, requestModel, currencySettings, keyword, datelimit, locations, methods, accounts, cashiers, devices, userDateTimeFormat);

                var queueOrders = dbContext.QueueOrders.Where(q => q.Sale != null && q.Sale.Location.Domain.Id == domainId);
                queueOrders = queueOrders.Where(q => q.Sale.Invoices.Any(i => i.Payments.Any()));
                if (!queueOrders.Any())
                    return response;
                var orderOLists = queueOrders.Select(s => new
                {
                    QueueOrder = s,
                    Payments = s.Sale.Invoices.SelectMany(i => i.Payments)
                });
                var queueQs = orderOLists.SelectMany(s => s.Payments.Select(p => new
                {
                    s.QueueOrder,
                    Payment = p
                }));
                var payments = queueQs.Select(s => new
                {
                    s.QueueOrder.Sale.CreatedDate,
                    Location = new { s.QueueOrder.Sale.Location.Id, s.QueueOrder.Sale.Location.Name },
                    RefFull = s.QueueOrder.OrderRef,
                    Method = s.Payment.PaymentMethod != null ? new { s.Payment.PaymentMethod.Id, s.Payment.PaymentMethod.Name } : new { Id = 0, Name = "" },
                    Account = s.Payment.DestinationAccount != null ? new { s.Payment.DestinationAccount.Id, s.Payment.DestinationAccount.Name } : new { Id = 0, Name = "" },
                    Cashier = new { s.QueueOrder.Cashier.Id },
                    CashierName = !string.IsNullOrEmpty(s.QueueOrder.Cashier.Forename) && !string.IsNullOrEmpty(s.QueueOrder.Cashier.Surname)
                             ? s.QueueOrder.Cashier.Surname + " " + s.QueueOrder.Cashier.Forename
                             : s.QueueOrder.Cashier.DisplayUserName,
                    PosDevice = s.QueueOrder.PosDeviceOrder != null
                    ? new { s.QueueOrder.PosDeviceOrder.PosDevice.Id, s.QueueOrder.PosDeviceOrder.PosDevice.Name }
                    : new { Id = 0, Name = "" },
                    s.Payment.Amount,
                });
                // keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    payments = payments.Where(q => q.Location.Name.ToLower().Contains(keyword) || q.RefFull.ToLower().Contains(keyword)
                                || q.Method.Name.ToLower().Contains(keyword) || q.Account.Name.ToLower().Contains(keyword) || q.CashierName.ToLower().Contains(keyword)
                                || q.PosDevice.Name.ToLower().Contains(keyword) || q.Amount.ToString().Contains(keyword));
                }
                //// date - old codebase
                //if (!string.IsNullOrEmpty(datelimit) && payments.Any())
                //{
                //    if (!datelimit.Contains('-'))
                //    {
                //        datelimit += "-";
                //    }
                //    var startDate = DateTime.UtcNow;
                //    var endDate = DateTime.UtcNow;
                //    datelimit.ConvertDaterangeFormat(dateFortmat, "", out startDate, out endDate);
                //    startDate = startDate.AddTicks(1);
                //    endDate = endDate.AddDays(1).AddTicks(-1);
                //    payments = payments.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                //}

                //Filter by dateTime
                if (!string.IsNullOrEmpty(datelimit.Trim()) && payments.Any())
                {
                    //define the variables
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(userDateTimeFormat.Timezone);

                    datelimit.ConvertDaterangeFormat(userDateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    //startDate = startDate.ConvertTimeToUtc(tz);
                    //endDate = endDate.ConvertTimeToUtc(tz);

                    //update filter
                    payments = payments.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                }

                // location
                if (locations != null && payments.Any())
                {
                    payments = payments.Where(q => locations.Contains(q.Location.Id));
                }
                // payment method
                if (methods != null && payments.Any())
                {
                    payments = payments.Where(q => methods.Contains(q.Method.Id));
                }
                // account
                if (accounts != null && payments.Any())
                {
                    payments = payments.Where(q => accounts.Contains(q.Account.Id));
                }
                // cashier
                if (cashiers != null && payments.Any())
                {
                    payments = payments.Where(q => cashiers.Contains(q.Cashier.Id));
                }
                // devices
                if (devices != null && payments.Any())
                {
                    payments = payments.Where(q => devices.Contains(q.PosDevice.Id));
                }

                if (payments.Any())
                {
                    totalRecords = payments.Count();
                    var sortedColumns = requestModel.Columns.GetSortedColumns();
                    var orderByString = string.Empty;
                    foreach (var column in sortedColumns)
                    {
                        switch (column.Name)
                        {
                            case "CreatedDate":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            case "LocationName":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "Location.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            case "RefFull":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "RefFull" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            case "Method":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "Method.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            case "AccountName":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "Account.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            case "Cashier":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "CashierName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            case "PosDevice":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "PosDevice.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            case "Amount":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "Amount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;
                            default:
                                orderByString = "QueueOrder.Sale.CreatedDate asc";
                                break;
                        }
                    }
                    payments = payments.OrderBy(orderByString == string.Empty ? "QueueOrder.Sale.CreatedDate asc" : orderByString);
                    var lstItems = payments.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                    var returnListItem = new List<PosPaymentCustom>();
                    foreach (var item in lstItems)
                    {
                        var paymentCustomer = new PosPaymentCustom()
                        {
                            CreatedDate = item.CreatedDate.FormatDateTimeByUser(userDateTimeFormat.DateFormat, userDateTimeFormat.TimeFormat),
                            LocationName = item.Location.Name,
                            RefFull = item.RefFull,
                            Method = item.Method.Name,
                            AccountName = item.Account.Name,
                            Cashier = item.CashierName,
                            PosDevice = item.PosDevice.Name,
                            Amount = item.Amount.ToString()
                        };
                        returnListItem.Add(paymentCustomer);
                    }
                    return new DataTablesResponse(requestModel.Draw, returnListItem, totalRecords, totalRecords);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, currencySettings, keyword, datelimit, locations, methods, accounts, cashiers, devices, userDateTimeFormat);

            }

            return response;
        }


        public DataTablesResponse GetTraderOrderHistoryDataTable(IDataTablesRequest requestModel, int locationId, string daterange, string dateFortmat, bool isCompletedShown)
        {
            var totalRecords = 0;
            var response = new DataTablesResponse(requestModel.Draw, new List<OrderQueueCustom>(), 0, 0);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTraderOrderHistoryDataTable", null, null, requestModel, locationId, daterange, dateFortmat);

                var queueOrders = dbContext.QueueOrders.Where(q => q.PrepQueue.Location.Id == locationId);
                if (isCompletedShown)
                    queueOrders = dbContext.QueueOrders.Where(q => q.PrepQueue.Location.Id == locationId
                                                                    || q.PrepQueueArchive.Location.Id == locationId);
                if (!string.IsNullOrEmpty(daterange) && queueOrders.Any())
                {
                    if (!daterange.Contains('-'))
                    {
                        daterange += "-";
                    }
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFortmat, "", out startDate, out endDate);
                    startDate = startDate.AddTicks(1);
                    endDate = endDate.AddDays(1).AddTicks(-1);
                    queueOrders = queueOrders.Where(q => q.QueuedDate != null && q.QueuedDate >= startDate && q.QueuedDate <= endDate);
                }

                if (queueOrders.Any())
                {
                    totalRecords = queueOrders.Count();
                    var sortedColumns = requestModel.Columns.GetSortedColumns();
                    var orderByString = string.Empty;
                    foreach (var column in sortedColumns)
                    {
                        switch (column.Name)
                        {
                            case "OrderRef":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "OrderRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;

                            case "OrderTotal":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "OrderTotal" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;

                            case "OrderItems":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "OrderItems.Count" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;

                            default:
                                orderByString = "QueuedDate.Value desc";
                                break;
                        }
                    }
                    queueOrders = queueOrders.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                    var lstItems = queueOrders.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                    var returnListItem = new List<OrderQueueCustom>();
                    foreach (var queueOrder in lstItems)
                    {
                        var orderHistoryCustom = new OrderQueueCustom()
                        {
                            Id = queueOrder.Id,
                            OrderRef = queueOrder.OrderRef,
                            OrderTotal = queueOrder.OrderTotal.ToString("N2"),
                            OrderItems = queueOrder.OrderItems.Count,
                            Discussion = queueOrder.Discussion != null
                        };
                        // Queue
                        if (queueOrder.QueuedDate != null)
                        {
                            orderHistoryCustom.Queued = "<span>" + queueOrder.QueuedDate.Value.ToString(dateFortmat) +
                                                        " by " + HelperClass.GetFullNameOfUser(queueOrder.Cashier) +
                                                        "</span>";
                        }
                        // Pending
                        var timeSpan = new TimeSpan();
                        var time = "";
                        if (queueOrder.PrepStartedDate != null && queueOrder.QueuedDate != null)
                        {
                            time = GetTimeElapsedFormated((DateTime)queueOrder.QueuedDate, (DateTime)queueOrder.PrepStartedDate);
                            orderHistoryCustom.Pending =
                                "<span>" + time + " &nbsp; <i class=\"fa fa-check green\"></i></span>";
                        }
                        else if (queueOrder.PrepStartedDate == null && queueOrder.QueuedDate != null)
                        {
                            time = GetTimeElapsed((DateTime)queueOrder.QueuedDate, DateTime.UtcNow);
                            orderHistoryCustom.Pending = "<span class=\"timer-order\">" + time + "</span>";
                        }
                        // Preparing
                        if (queueOrder.PrepStartedDate != null)
                        {
                            if (queueOrder.PreparedDate != null)
                            {
                                time = GetTimeElapsedFormated((DateTime)queueOrder.PrepStartedDate, (DateTime)queueOrder.PreparedDate);
                                orderHistoryCustom.Preparing = "<span>" + time + " &nbsp;<i class=\"fa fa-check green\"></i></span>";
                            }
                            else
                            {
                                time = GetTimeElapsed((DateTime)queueOrder.PrepStartedDate, DateTime.UtcNow);
                                orderHistoryCustom.Preparing = "<span class=\"timer-order\">" + time + "</span>";
                            }
                        }
                        // Completion
                        if (queueOrder.PreparedDate != null)
                        {
                            if (queueOrder.CompletedDate != null)
                            {
                                //timeSpan = (queueOrder.PreparedDate - queueOrder.CompletedDate).Value;
                                time = GetTimeElapsedFormated((DateTime)queueOrder.PreparedDate, (DateTime)queueOrder.CompletedDate);
                                orderHistoryCustom.Completion = "<span>" + time + " &nbsp; <i class=\"fa fa-check green\"></i></span>";
                            }
                            else
                            {
                                //timeSpan = (DateTime.UtcNow - queueOrder.PreparedDate).Value;
                                time = GetTimeElapsed((DateTime)queueOrder.PreparedDate, DateTime.UtcNow);
                                orderHistoryCustom.Completion = "<span class=\"timer-order\">" + time + "</span>";
                            }
                        }
                        // Delivery status
                        if (queueOrder.Delivery != null)
                        {
                            if (queueOrder.Delivery.TimeFinished != null)
                            {
                                orderHistoryCustom.DeliveryStatus = "<span class=\"label label-lg label-success\">Delivered</span>";
                            }
                            else if (queueOrder.Delivery.TimeStarted != null && queueOrder.Delivery.TimeFinished != null)
                            {
                                timeSpan = (queueOrder.Delivery.TimeStarted - queueOrder.Delivery.TimeFinished).Value;
                                time = timeSpan.ToString(@"mm\:ss");
                                orderHistoryCustom.DeliveryStatus = "<span class=\"label label-lg label-warning\">In transit - <span class=\"timer-order\">" + time + "</span></span>";
                            }
                        }
                        // Payment
                        if (queueOrder.IsPaid)
                        {
                            orderHistoryCustom.Payment = "<span class=\"label label-lg label-success\">Paid</span>";
                        }
                        else
                        {
                            orderHistoryCustom.Payment = "<span class=\"label label-lg label-warning\">Unpaid</span>";
                        }
                        //Status
                        orderHistoryCustom.Status = queueOrder.Status.GetDescription() ?? "";

                        returnListItem.Add(orderHistoryCustom);
                    }
                    return new DataTablesResponse(requestModel.Draw, returnListItem, totalRecords, totalRecords);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, locationId, daterange, dateFortmat);
            }

            return response;
        }

        // pos OrderType
        public PosOrderType getOrderTypeById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "getOrderTypeById", null, null, id);

                return dbContext.PosOrderTypes.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public List<PosOrderType> GetOrderTypeInLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "getOrderTypeInLocation", null, null, locationId);

                return dbContext.PosOrderTypes.Where(q => q.Location.Id == locationId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public ReturnJsonModel SavePosOrderType(PosOrderType orderType, string userId)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            var orderTypeExists = dbContext.PosOrderTypes.FirstOrDefault(q => q.Name == orderType.Name && q.Location.Id == orderType.Location.Id && q.Id != orderType.Id);
            if (orderTypeExists != null)
            {
                result.result = false;
                result.actionVal = 4;
                result.msg = ResourcesManager._L("ERROR_DATA_EXISTED", orderType.Name);
                return result;
            }
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePosOrderType", null, null, orderType);

                orderType.Location = dbContext.TraderLocations.Find(orderType.Location.Id);
                if (orderType.Id > 0)
                {
                    result.actionVal = 2;
                    var orderTypeDb = dbContext.PosOrderTypes.Find(orderType.Id);
                    orderTypeDb.Name = orderType.Name;
                    orderTypeDb.Summary = orderType.Summary;
                    orderTypeDb.Classification = orderType.Classification;
                    dbContext.Entry(orderTypeDb).State = EntityState.Modified;
                }
                else
                {
                    orderType.CreatedDate = DateTime.UtcNow;
                    orderType.CreatedBy = dbContext.QbicleUser.Find(userId);
                    dbContext.PosOrderTypes.Add(orderType);
                    dbContext.Entry(orderType).State = EntityState.Added;
                }
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                result.result = false;
                result.actionVal = 3;
                result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, orderType);
            }
            return result;
        }

        public bool DeletePosOrderType(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosOrderTypeDataTable", null, null, id);
                var posOrderType = getOrderTypeById(id);
                dbContext.PosOrderTypes.Remove(posOrderType);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }

        public DataTablesResponse GetPosOrderTypeDataTable(IDataTablesRequest requestModel, string keyword, int locationId)
        {
            var totalRecords = 0;
            var response = new DataTablesResponse(requestModel.Draw, new List<PosOrderTypeCustom>(), 0, 0);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosOrderTypeDataTable", null, null, requestModel, keyword);
                var orderTypes = dbContext.PosOrderTypes.Where(q => q.Location.Id == locationId);
                // keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    orderTypes = orderTypes.Where(q => q.Name.ToLower().Contains(keyword) || q.Summary.ToLower().Contains(keyword)
                    || q.Classification.GetDescription().ToLower().Contains(keyword));
                }
                // date

                if (orderTypes.Any())
                {
                    totalRecords = orderTypes.Count();
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

                            case "Classification":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "Classification" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;

                            case "Summary":
                                orderByString += orderByString != string.Empty ? "," : "";
                                orderByString += "Summary" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                break;

                            default:
                                orderByString = "Id asc";
                                break;
                        }
                    }
                    orderTypes = orderTypes.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                    var lstItems = orderTypes.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                    var returnListItem = new List<PosOrderTypeCustom>();
                    foreach (var item in lstItems)
                    {
                        var pushItem = new PosOrderTypeCustom()
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classification = item.Classification.GetDescription(),
                            Summary = item.Summary,
                            IsUse = item.PosDeviceTypes.Any()
                        };
                        returnListItem.Add(pushItem);
                    }
                    return new DataTablesResponse(requestModel.Draw, returnListItem, totalRecords, totalRecords);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
            }

            return response;
        }

        // pos DeviceType
        public List<PosOrderType> GetFilterOrderTypeInLocation(int locationId)
        {
            var lstOrderTypes = new List<PosOrderType>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetFilterOrderTypeInLocation", null, null, locationId);

                var devices = dbContext.PosDeviceTypes.Where(q => q.Location.Id == locationId).OrderBy(n => n.Name);
                if (devices.Any())
                {
                    lstOrderTypes = devices.SelectMany(q => q.PosOrderTypes).Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
            }
            return lstOrderTypes;
        }

        public List<PosDeviceType> GetFilterDeviceTypeInLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "getFilterDeviceTypeInLocation", null, null, locationId);

                var devices = dbContext.PosDeviceTypes.Where(q => q.Location.Id == locationId).OrderBy(n => n.Name).ToList();
                return devices;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return null;
            }
        }

        public DataTablesResponse GetPosDeviceTypeDataTable(IDataTablesRequest requestModel, string keyword, int locationId, string orderTypeId = "")
        {
            var totalRecords = 0;
            var response = new DataTablesResponse(requestModel.Draw, new List<PosDeviceTypeCustom>(), 0, 0);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosDeviceTypeDataTable");
                var deviceTypes = dbContext.PosDeviceTypes.Where(q => q.Location.Id == locationId).Select(q => new
                {
                    Id = q.Id,
                    Name = q.Name,
                    TypeIds = q.PosOrderTypes.Select(s => s.Id),
                    TypeNames = q.PosOrderTypes.Select(s => s.Name),
                    Devices = q.PosDevices
                });
                // keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    deviceTypes = deviceTypes.Where(q => q.Name.ToLower().Contains(keyword));
                }
                if (orderTypeId != "" && orderTypeId != "0")
                {
                    var typeId = int.Parse(orderTypeId);
                    deviceTypes = deviceTypes.Where(q => q.TypeIds.Contains(typeId));
                }
                // date

                if (deviceTypes.Any())
                {
                    totalRecords = deviceTypes.Count();
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
                                orderByString = "Id asc";
                                break;
                        }
                    }
                    deviceTypes = deviceTypes.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                    var lstItems = deviceTypes.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                    var returnListItem = new List<PosDeviceTypeCustom>();
                    foreach (var item in lstItems)
                    {
                        var pushItem = new PosDeviceTypeCustom()
                        {
                            Id = item.Id,
                            Name = item.Name,
                            OrderTypes = string.Join(",", item.TypeNames),
                            IsUse = item.Devices.Any()
                        };
                        returnListItem.Add(pushItem);
                    }
                    return new DataTablesResponse(requestModel.Draw, returnListItem, totalRecords, totalRecords);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

            return response;
        }

        public PosDeviceType getDeviceTypeById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "getDeviceTypeById", null, null, id);

                return dbContext.PosDeviceTypes.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public ReturnJsonModel SavePosDeviceType(PosDeviceType deviceType, string userId)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePosDeviceType", null, null, deviceType);

                var location = dbContext.TraderLocations.Find(deviceType.Location.Id);
                deviceType.Location = location;
                var orderTypeExists = dbContext.PosDeviceTypes.FirstOrDefault(q => q.Name == deviceType.Name && q.Location.Id == deviceType.Location.Id && q.Id != deviceType.Id);
                if (orderTypeExists != null)
                {
                    result.result = false;
                    result.actionVal = 4;
                    result.msg = ResourcesManager._L("ERROR_DATA_EXISTED", deviceType.Name);
                    return result;
                }
                if (deviceType.PosOrderTypes.Any())
                {
                    var posOrderTypeList = new List<PosOrderType>();
                    foreach (var posOrderType in deviceType.PosOrderTypes)
                    {
                        posOrderTypeList.Add(getOrderTypeById(posOrderType.Id));
                    }
                    deviceType.PosOrderTypes.Clear();
                    deviceType.PosOrderTypes = posOrderTypeList;
                }
                if (deviceType.Id > 0)
                {
                    result.actionVal = 2;
                    var deviceTypeDb = dbContext.PosDeviceTypes.Find(deviceType.Id);
                    deviceTypeDb.Name = deviceType.Name;
                    deviceTypeDb.PosOrderTypes.Clear();
                    dbContext.SaveChanges();
                    deviceType.Location = location;
                    deviceTypeDb.PosOrderTypes = deviceType.PosOrderTypes;
                    dbContext.Entry(deviceTypeDb).State = EntityState.Modified;
                }
                else
                {
                    deviceType.Location = location;
                    deviceType.CreatedDate = DateTime.UtcNow;
                    deviceType.CreatedBy = dbContext.QbicleUser.Find(userId);
                    dbContext.PosDeviceTypes.Add(deviceType);
                }
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                result.result = false;
                result.actionVal = 3;
                result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, deviceType);
            }
            return result;
        }

        public bool DeletePosDeviceType(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeletePosDeviceType", null, null, id);

                var posDeviceType = getDeviceTypeById(id);
                dbContext.PosDeviceTypes.Remove(posDeviceType);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        /// <summary>
        /// Get Elapsed Time with Format: Month -> Week -> Day -> Hour -> Min -> Second
        /// In average: 1 month have 30.4368 days
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static string GetTimeElapsed(DateTime startDate, DateTime endDate)
        {
            TimeSpan ts = endDate - startDate;
            var miliseconds = ts.TotalMilliseconds;
            //Month
            var month = Math.Truncate((miliseconds / 1000 / 60 / 60 / 24 / 30.4368));
            var week = Math.Truncate(((miliseconds / 1000 / 60 / 60 / 24 - month * 30.4368) / 7));
            var day = Math.Truncate((miliseconds / 1000 / 60 / 60 / 24 - month * 30.4368 - week * 7));
            var dayInvolved = day + week * 7 + month * 30.4368;
            var hour = Math.Truncate((miliseconds / 1000 / 60 / 60 - dayInvolved * 24));
            var minute = Math.Truncate((miliseconds / 1000 / 60 - dayInvolved * 24 * 60 - hour * 60));
            var second = Math.Truncate((miliseconds / 1000 - dayInvolved * 24 * 60 * 60 - hour * 60 * 60 - minute * 60));

            return month + ":" + week + ":" + day + ":" + hour + ":" + minute + ":" + second;
        }

        public static string GetTimeElapsedFormated(DateTime startDate, DateTime endDate)
        {
            TimeSpan ts = endDate - startDate;
            var miliseconds = ts.TotalMilliseconds;
            //Month
            var month = Math.Truncate((miliseconds / 1000 / 60 / 60 / 24 / 30.4368));
            var week = Math.Truncate(((miliseconds / 1000 / 60 / 60 / 24 - month * 30.4368) / 7));
            var day = Math.Truncate((miliseconds / 1000 / 60 / 60 / 24 - month * 30.4368 - week * 7));
            var dayInvolved = day + week * 7 + month * 30.4368;
            var hour = Math.Truncate((miliseconds / 1000 / 60 / 60 - dayInvolved * 24));
            var minute = Math.Truncate((miliseconds / 1000 / 60 - dayInvolved * 24 * 60 - hour * 60));
            var second = Math.Truncate((miliseconds / 1000 - dayInvolved * 24 * 60 * 60 - hour * 60 * 60 - minute * 60));

            //Init Strings
            var monthStr = "";
            var weekStr = "";
            var dayStr = "";
            var hourStr = "";
            var minStr = minute + "m ";
            var secondStr = second + "s";
            if (month > 0)
                monthStr = month + "m ";
            if (week > 0)
                weekStr = week + "w ";
            if (day > 0)
                dayStr = day + "d ";
            if (hour > 0)
                hourStr = hour + "h ";
            if (minute < 10)
                minStr = "0" + minStr;
            if (second < 10)
                secondStr = "0" + secondStr;

            return monthStr + weekStr + dayStr + hourStr + minStr + secondStr;
        }
    }
}