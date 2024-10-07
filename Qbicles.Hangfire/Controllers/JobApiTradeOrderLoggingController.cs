using Hangfire;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    [RoutePrefix("api/job")]
    public class JobApiTradeOrderLoggingController : ApiController    
    {
        private string JobId { get; set; }

        #region Disabled

        //[Route("scheduletradeorderlogging")]
        //[AcceptVerbs("POST")]
        //public QbicleJobResult ScheduleTradeOrderLogging(TradeOrderLoggingParameter request)
        //{
        //    JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueTradeOrderLogging(request));
        //    return new QbicleJobResult
        //    {
        //        JobId = JobId,
        //        Message = "Hangfire Processing TradeOrderLogging!",
        //        Status = HttpStatusCode.OK
        //    };
        //}

        #endregion

        /// <summary>
        /// This method exist because we had a goal to exclude this jobs from hangfire, and have them hit the DB directly.
        /// Only the method contents was changed
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [Route("scheduletradeorderlogging")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleTradeOrderLogging(TradeOrderLoggingParameter request)
        {
            new QbiclesJob().ScheduleTradeOrderLogging(request);
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing TradeOrderLogging!",
                Status = HttpStatusCode.OK
            };
        }

    }
}