using Hangfire;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class JobApiProcessOrderController : ApiController
    {
        private string JobId { get; set; }

        #region Disabled

        //[Route("api/job/processorder")]
        //[AcceptVerbs("POST")]
        //public QbicleJobResult ScheduleOnProcessOrder(OrderJobParameter job)
        //{
        //    JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueProcessOrder(job));
        //    return new QbicleJobResult
        //    {
        //        JobId = JobId,
        //        Message = "Hangfire Processing Order Completed!",
        //        Status = HttpStatusCode.OK
        //    };
        //}

        //[Route("api/job/b2bprocessorder")]
        //[AcceptVerbs("POST")]
        //public QbicleJobResult ScheduleOnProcessOrderForB2B(OrderJobParameter job)
        //{
        //	JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueProcessOrderForB2B(job));
        //	return new QbicleJobResult
        //	{
        //		JobId = JobId,
        //		Message = "Hangfire Processing Order Completed!",
        //		Status = HttpStatusCode.OK
        //	};
        //}

        #endregion Disabled

        /// <summary>
        /// This method exist because we had a goal to exclude this jobs from hangfire, and have them hit the DB directly.
        /// Only the method contents was changed
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [Route("api/job/processorder")]
        [AcceptVerbs("POST")]
        public async Task<QbicleJobResult> ScheduleOnProcessOrder(OrderJobParameter job)
        {
            //Ignore the naming conventions, the properties were reused so this is hitting the DB directly
            await new QbiclesJob().ScheduleProcessOrder(job);

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Order Was Not Queued!",
                Status = HttpStatusCode.OK
            };
        }

        /// <summary>
        /// This method exist because we had a goal to exclude this jobs from hangfire, and have them hit the DB directly.
        /// Only the method contents was changed
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [Route("api/job/b2bprocessorder")]
        [AcceptVerbs("POST")]
        public async Task<QbicleJobResult> ScheduleOnProcessOrderForB2B(OrderJobParameter job)
        {
            //Ignore the naming conventions, the properties were reused so this is hitting the DB directly
            await new QbiclesJob().ScheduleProcessOrderForB2B(job);

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Order Was Not Queued!",
                Status = HttpStatusCode.OK
            };
        }
    }
}