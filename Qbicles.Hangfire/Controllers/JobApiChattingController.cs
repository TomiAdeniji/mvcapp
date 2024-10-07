using System.Net;
using System.Web.Http;
using Hangfire;
using Qbicles.BusinessRules.Model;

namespace Qbicles.Hangfire.Controllers
{
    public class JobApiChattingController : ApiController
    {
        private string JobId { get; set; }

        [Route("api/job/chatting")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleChatting(ChattingJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueChatting(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Chatting Completed!",
                Status = HttpStatusCode.OK
            };
        }
    }
}