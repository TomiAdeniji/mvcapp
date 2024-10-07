using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.TraderApi;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.PoS
{
    public class PosRequestRules
    {
        private ApplicationDbContext dbContext;

        public PosRequestRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        #region Request Logging

        public void HangfirePosRequestLog(PosRequest request, string retrieved, string methodName)
        {
            var log = new PosApiRequestLog
            {
                QueryString = request.ToJson(),
                Retrieved = retrieved,
                ApiControllerName = methodName,
                RequestHeader = request.Header
            };

            //var job = new PosProcessLogParameter
            //{
            //    Request = request,
            //    LogInput = log,
            //    EndPointName = "posrequestlog"
            //};
            //Task tskHangfire = new Task(async () =>
            //{
            //    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            //});
            //tskHangfire.Start();
        }

        /// <summary>
        /// Call from hangfire
        /// </summary>
        /// <param name="requestLog"></param>
        public void PosApiRequestLog(PosProcessLogParameter requestLog)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Pos Api Request logger", null, null, requestLog);

                var log = new PosApiRequestLog
                {
                    ApiControllerName = requestLog.LogInput?.ApiControllerName ?? "",
                    CreatedDate = DateTime.UtcNow,
                    IsTokenValid = requestLog.Request.IsTokenValid,
                    QueryString = requestLog.LogInput?.QueryString ?? requestLog.Request.ToJson(),
                    RequestHeader = requestLog.Request.Header ?? "",
                    Token = requestLog.Request.AccessToken,
                    Retrieved = requestLog.LogInput?.Retrieved ?? requestLog.Request.ToJson(),
                };

                dbContext.PosApiRequestLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
            }
        }

        #endregion Request Logging

        #region Driver update location and process actived order

        /// <summary>
        /// process the ActiveOrder and DriverLog record to provide a progress update to the customer on delivery of their order.
        /// </summary>
        /// <param name="linkedOrderId"></param>
        /// <param name="driverName"></param>
        /// <param name="location"></param>
        public void HangfirePosProcessActiveOrder(string linkedOrderId, string driverId, DssLocationModel location)
        {
            var job = new PosProcessActiveOrderParameter
            {
                LinkedOrderId = linkedOrderId,
                DriverId = driverId,
                Location = location,
                EndPointName = "posprocessactiveorder"
            };
            Task tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        #endregion Driver update location and process actived order

        public void PosProcessActiveOrder(PosProcessActiveOrderParameter request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "PosProcessActiveOrder", null, request);

                var tradeOrder = dbContext.TradeOrders.FirstOrDefault(o => o.LinkedOrderId == request.LinkedOrderId);
                var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(tradeOrder.Id);
                var b2cqbicle = discussion.Qbicle as B2CQbicle;

                tradeOrder.ETA = DateTime.UtcNow.AddMinutes(request.Location.ETA);

                var message = "The order is at the delivery location.";
                if (request.Location.ETA > 0)
                {
                    tradeOrder.IsInTransit = true;

                    var ts = TimeSpan.FromMinutes(request.Location.ETA);
                    message = $"The order is in transit and is expected to be delivered in ";
                    if (ts.Hours > 0)
                        message += $"{ts.Hours} hours and {ts.Minutes} minutes.";
                    else
                        message += $"{ts.Minutes} minutes.";
                }
                else
                {
                    tradeOrder.IsInTransit = false;
                }

                dbContext.SaveChanges();

                new B2CRules(dbContext).B2CDicussionOrderSendMessage(false, message, discussion.Id, request.DriverId, b2cqbicle.Id, "");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
            }
        }
    }
}