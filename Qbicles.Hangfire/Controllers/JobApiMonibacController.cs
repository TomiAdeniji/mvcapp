using Hangfire;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    public class JobApiMonibacController : ApiController
    {
        private string JobId { get; set; }

        [Route("api/job/generatestorepoinfrompaymentapproved")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleGenerateStorePoinFromPaymentApproved(QbicleJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueGenerateStorePoinFromPaymentApproved(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Point On Payment Approved!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/decreasestorecreditfrompaymentapproved")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleDecreaseStoreCreditFromPaymentApproved(QbicleJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueDecreaseStoreCreditFromPaymentApproved(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Point On Payment Approved!",
                Status = HttpStatusCode.OK
            };
        }
    }
}