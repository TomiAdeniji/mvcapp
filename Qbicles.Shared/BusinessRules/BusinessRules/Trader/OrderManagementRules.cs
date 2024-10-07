using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.Firebase;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Model.Firebase;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.SalesChannel;

namespace Qbicles.BusinessRules.BusinessRules.Trader
{
    public class OrderManagementRules
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderManagementRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public DataTableModel GetListOrderPagination(IDataTablesRequest request, int locationId,
            List<SalesChannelEnum> saleChannels,
            string daterange, string keyword, bool isCompletedShownOnly, string dateFormat, string dateTimeFormat,
            string timeZone, CurrencySetting currencySetting)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request, locationId, saleChannels,
                        daterange,
                        keyword, isCompletedShownOnly, dateFormat, dateTimeFormat, timeZone);

                var orderQuery = _dbContext.QueueOrders.Where(p => p.PrepQueue.Location.Id == locationId
                                                                  || p.PrepQueueArchive.Location.Id == locationId);
                if (!isCompletedShownOnly)
                    //QBIC-3237: Thomas fix bugs "If this is off then QueueOrders with status ‘Completed with problems’ should not be shown. "
                    orderQuery = orderQuery.Where(s =>
                        s.Status != PrepQueueStatus.Completed && s.Status != PrepQueueStatus.CompletedWithProblems);

                #region Filter

                if (saleChannels == null) saleChannels = new List<SalesChannelEnum>();
                orderQuery = orderQuery.Where(p => saleChannels.Contains(p.SalesChannel));
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow.Date;
                    var endDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1);
                    daterange.ConvertDaterangeFormat(dateTimeFormat, timeZone, out startDate, out endDate);

                    orderQuery = orderQuery.Where(p =>
                        p.QueuedDate != null && p.QueuedDate >= startDate && p.QueuedDate <= endDate);
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    orderQuery = orderQuery.Where(p => p.OrderRef.ToLower().Contains(keyword));
                }

                #endregion

                #region Ordering

                var columns = request.Columns;
                var orderByString = "";
                foreach (var columnItem in columns.GetSortedColumns())
                    switch (columnItem.Name)
                    {
                        case "OrderRef":
                            orderByString += string.IsNullOrEmpty(orderByString) ? "" : ",";
                            orderByString += "OrderRef " +
                                             (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? "asc"
                                                 : "desc");
                            break;
                        case "Location":
                            orderByString += string.IsNullOrEmpty(orderByString) ? "" : ",";
                            orderByString += "PrepQueue.Location.Name " +
                                             (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? "asc"
                                                 : "desc");
                            break;
                        case "SaleChannel":
                            orderByString += string.IsNullOrEmpty(orderByString) ? "" : ",";
                            orderByString += "Sale.SalesChannel " +
                                             (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant
                                                 ? "asc"
                                                 : "desc");
                            break;
                        case "Status":
                            orderByString += string.IsNullOrEmpty(orderByString) ? "" : ",";
                            orderByString += "Status " + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant
                                ? "asc"
                                : "desc");
                            break;
                        default:
                            orderByString += string.IsNullOrEmpty(orderByString) ? "" : ",";
                            orderByString += "Id desc";
                            break;
                    }

                orderQuery = orderQuery.OrderBy(string.IsNullOrEmpty(orderByString) ? "Id desc" : orderByString);

                #endregion

                #region Pagination

                var totalRecords = orderQuery.Count();
                orderQuery = orderQuery.Skip(request.Start).Take(request.Length);
                var queryResult = orderQuery.ToList();

                #endregion

                var lstResults = new List<OrderManagementCustomModel>();
                queryResult.ForEach(p =>
                {
                    var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(e => e.LinkedOrderId == p.LinkedOrderId);
                    var paidStatus = "";
                    if (tradeOrder == null)
                    {
                        paidStatus = p.IsPaid ? "Paid" : "Unpaid";
                    }
                    else
                    {
                        if (tradeOrder.Payments.Count <= 0)
                            paidStatus = "Unpaid";
                        else
                        {
                            if (tradeOrder.Payments.Any(pm => pm.Status == TraderPaymentStatusEnum.PaymentApproved)
                                && tradeOrder.Sale.SaleTotal > tradeOrder.Payments.Where(pm2 =>
                                    pm2.Type == CashAccountTransactionTypeEnum.PaymentIn &&
                                    pm2.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(a => a.Amount))
                                paidStatus = B2CFilterPaymentType.PaidInPart.GetDescription();
                            if (tradeOrder.Sale.SaleTotal <= tradeOrder.Payments.Where(pm =>
                                    pm.Type == CashAccountTransactionTypeEnum.PaymentIn &&
                                    pm.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(a => a.Amount))
                                paidStatus = B2CFilterPaymentType.PaidInFull.GetDescription();
                        }
                    }

                    var orderItem = new OrderManagementCustomModel
                    {
                        Id = p.Id,
                        OrderRef = p.OrderRef,
                        LocationName = p.PrepQueue?.Location?.Name ?? (p.PrepQueueArchive?.Location?.Name ?? ""),
                        SaleChannel = p.SalesChannel.GetDescription() ?? "",
                        ItemCount = p.OrderItems?.Count(ot => ot.IsInPrep == false) ?? 0,
                        Total = p.OrderTotal,
                        Status = p.Status.GetDescription() ?? "",
                        PaidStatus = paidStatus,//p.IsPaid ? "Paid" : "Unpaid",
                        PaidStatusHtml = paidStatus == "Unpaid"//p.IsPaid
                            ? $"<span class=\'label label-lg label-warning\'>{paidStatus}</span>"
                            : $"<span class=\'label label-lg label-success\'/>{paidStatus}</span>",
                        TotalStr = p.OrderTotal.ToCurrencySymbol(currencySetting)
                    };

                    //Queue Infor
                    if (p.QueuedDate != null)
                        orderItem.QueuedInfo = p.QueuedDate.Value.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat)
                                               + " by "
                                               + HelperClass.GetFullNameOfUser(p.Cashier);

                    //Pending
                    // var timeSpan = new TimeSpan();
                    string time;
                    if (p.PrepStartedDate != null && p.QueuedDate != null)
                    {
                        time = PosSaleOrderRules.GetTimeElapsedFormated((DateTime)p.QueuedDate,
                            (DateTime)p.PrepStartedDate);
                        orderItem.Pending =
                            "<span>" + time + " &nbsp; <i class=\"fa fa-check green\"></i></span>";
                    }
                    else if (p.PrepStartedDate == null && p.QueuedDate != null)
                    {
                        time = PosSaleOrderRules.GetTimeElapsed((DateTime)p.QueuedDate, DateTime.UtcNow);
                        orderItem.Pending = "<span class=\"timer-order\">" + time + "</span>";
                    }

                    // Preparing
                    if (p.PrepStartedDate != null)
                    {
                        if (p.PreparedDate != null)
                        {
                            time = PosSaleOrderRules.GetTimeElapsedFormated((DateTime)p.PrepStartedDate,
                                (DateTime)p.PreparedDate);
                            orderItem.Preparing = "<span>" + time + " &nbsp;<i class=\"fa fa-check green\"></i></span>";
                        }
                        else
                        {
                            time = PosSaleOrderRules.GetTimeElapsed((DateTime)p.PrepStartedDate, DateTime.UtcNow);
                            orderItem.Preparing = "<span class=\"timer-order\">" + time + "</span>";
                        }
                    }

                    // Completion
                    if (p.PreparedDate != null)
                    {
                        if (p.CompletedDate != null)
                        {
                            //timeSpan = (queueOrder.PreparedDate - queueOrder.CompletedDate).Value;
                            time = PosSaleOrderRules.GetTimeElapsedFormated((DateTime)p.PreparedDate,
                                (DateTime)p.CompletedDate);
                            orderItem.Completion =
                                "<span>" + time + " &nbsp; <i class=\"fa fa-check green\"></i></span>";
                        }
                        else
                        {
                            //timeSpan = (DateTime.UtcNow - queueOrder.PreparedDate).Value;
                            time = PosSaleOrderRules.GetTimeElapsed((DateTime)p.PreparedDate, DateTime.UtcNow);
                            orderItem.Completion = "<span class=\"timer-order\">" + time + "</span>";
                        }
                    }

                    // Delivery status
                    if (p.Delivery != null)
                    {
                        if (p.Delivery.TimeFinished != null)
                        {
                            orderItem.DeliveryStatus = "<span class=\"label label-lg label-success\">Delivered</span>";
                        }
                        else if (p.Delivery.TimeStarted != null && p.Delivery.TimeFinished != null)
                        {
                            var timeSpan = (p.Delivery.TimeStarted - p.Delivery.TimeFinished).Value;
                            time = timeSpan.ToString(@"mm\:ss");
                            orderItem.DeliveryStatus =
                                "<span class=\"label label-lg label-warning\">In transit - <span class=\"timer-order\">" +
                                time + "</span></span>";
                        }
                    }


                    lstResults.Add(orderItem);
                });

                return new DataTableModel
                {
                    data = lstResults,
                    draw = request.Draw,
                    recordsFiltered = totalRecords,
                    recordsTotal = totalRecords
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request, locationId, saleChannels, daterange,
                    keyword, isCompletedShownOnly, dateFormat, dateTimeFormat, timeZone);
                return new DataTableModel
                {
                    data = new OrderManagementCustomModel(),
                    draw = request.Draw,
                    recordsFiltered = 0,
                    recordsTotal = 0
                };
            }
        }

        /// <summary>
        ///     This allow to update the status for an order: Jumping forward/Jumping back bypass the intermediate steps
        ///     When an order status is Jumping forward that skips intermediate steps, the datetimes for those steps are to be set
        ///     to the date-times for the new status.
        ///     When an order status is Jumping backward that skips intermediate steps, the datetimes for those skipped steps are
        ///     to be set to null.
        /// </summary>
        /// <param name="lstOrderIds">List PdsOrderUpdates Id</param>
        /// <param name="currentUserId"></param>
        /// <param name="upcommingStatus"></param>
        /// <param name="problemDescription"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdatePrepQueueStatus(List<int> lstOrderIds, string currentUserId, PrepQueueStatus upcommingStatus, string problemDescription)
        {
            var resultJsonModel = new ReturnJsonModel { actionVal = 2, result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, lstOrderIds, currentUserId, upcommingStatus, problemDescription);

                var queueOrders = new List<QueueOrder>();

                var loggingRules = new TradeOrderLoggingRules(_dbContext);

                foreach (var orderId in lstOrderIds)
                {
                    var queueOrder = _dbContext.QueueOrders.FirstOrDefault(e => e.Id == orderId);
                    if (queueOrder == null)
                        continue;

                    if (queueOrder.Status == upcommingStatus)
                        continue;

                    new OrderProcessingHelper(_dbContext).QueueOrderApplyDate(queueOrder, upcommingStatus,
                        queueOrder.QueuedDate, queueOrder.PrepStartedDate, queueOrder.PreparedDate, queueOrder.CompletedDate);

                    queueOrders.Add(new QueueOrder
                    {
                        Id = queueOrder.Id,
                        OrderRef = queueOrder.OrderRef,
                        Table = queueOrder.Table,
                        PrepQueue = new PrepQueue { Id = queueOrder.PrepQueue?.Id ?? 0 }
                    });

                    queueOrder.Status = upcommingStatus;
                    //When the Order Status is set to Completing
                    if (upcommingStatus == PrepQueueStatus.Completing)
                    {     //If the Order is associated with a Delivery
                        if (queueOrder.Delivery != null) //orders on Delivery are Status = Completing and there is a Driver associated with Delivery
                            if (queueOrder.Delivery.Orders.Count > 0 &&
                                queueOrder.Delivery.Orders.TrueForAll(s => s.Status == PrepQueueStatus.Completing) && queueOrder.Delivery.Driver != null)
                            {
                                queueOrder.Delivery.Status = DeliveryStatus.Started;
                                queueOrder.Delivery.TimeStarted = null;
                                loggingRules.TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, queueOrder.Delivery.Id, currentUserId);
                            }
                    }


                    

                    if (upcommingStatus == PrepQueueStatus.Completed || upcommingStatus == PrepQueueStatus.CompletedWithProblems)
                    {
                        //(1) get the PrepQueue from the QueueOrder)
                        var prepQueue = queueOrder.PrepQueue;
                        var prepQueueId = prepQueue?.Id ?? 0;

                        //(2) Get the PrepQueueArchive with the PrepQueue as ParentPrepQueue (var archivePrepQueue)
                        var archivePrepQueue = _dbContext.PrepQueueArchives.FirstOrDefault(a => a.ParentPrepQueue.Id == prepQueueId);

                        //(3) Then, to move the QueueOrder to archive
                        queueOrder.PrepQueue = null;
                        queueOrder.PrepQueueArchive = archivePrepQueue;
                        queueOrder.ArchivedDate = DateTime.UtcNow;
                        queueOrder.OrderProblemNote = problemDescription;
                    }


                    if (_dbContext.Entry(queueOrder).State == EntityState.Detached)
                        _dbContext.QueueOrders.Attach(queueOrder);
                    _dbContext.Entry(queueOrder).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                }

                if (queueOrders.Count > 0)
                    loggingRules.TradeOrderLogging(TradeOrderLoggingType.Preparation, 0, currentUserId, queueOrders.Select(e => e.Id).ToList());

                resultJsonModel.result = true;

                //push notification to PDS(s)
                var fireBaseMessages = new List<FireBaseMessage>();

                queueOrders.ForEach(queueOrder =>
                {
                    queueOrder.PushPdsStatusUpdate();
                });

                return resultJsonModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstOrderIds, currentUserId, upcommingStatus,
                    problemDescription);
                resultJsonModel.result = false;
                resultJsonModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJsonModel;
            }
        }
    }
}