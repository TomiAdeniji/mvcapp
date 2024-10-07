using Hangfire;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    [RoutePrefix("api/job")]
    public class JobApiPosController : ApiController
    {
        private string JobId { get; set; }


        [Route("posrequestlog")]
        [AcceptVerbs("POST")]
        public QbicleJobResult SchedulePosRequestLog(PosProcessLogParameter request)
        {
            //JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueuePosRequestLog(request));

            /// This exist because we had a goal to exclude this jobs from hangfire, and have them hit the DB directly.
            new QbiclesJob().SchedulePosRequestLog(request);

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing PosRequestLog was not Queued!",
                Status = HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Process ActiveOrder while driver update location
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("posprocessactiveorder")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleProcessActiveOrder(PosProcessActiveOrderParameter request)
        {
			//JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueuePosProcessActiveOrder(request));

			/// This exist because we had a goal to exclude this jobs from hangfire, and have them hit the DB directly.
			new QbiclesJob().SchedulePosProcessActiveOrder(request);

			return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Active order was not queued!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("sendtoprep")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleSendToPrep(PosSendToPrepParameter request)
        {
            //JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueuePosSendToPrep(request));

            /// This exist because we had a goal to exclude this jobs from hangfire, and have them hit the DB directly.
            new QbiclesJob().SchedulePosSendToPrep(request);

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Active order was not queued!",
                Status = HttpStatusCode.OK
            };
        }
    }
}