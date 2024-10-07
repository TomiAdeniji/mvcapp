using Hangfire;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class JobApiInventoryController : ApiController
    {
        private string JobId { get; set; }


        [Route("api/job/scheduleonmanufactureproduct")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleOnManufactureProduct(ManufactureProductJobParamenter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueManufactureProduct(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/scheduleonincominginventory")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleOnIncomingInventory(IncomingInventoryJobParamenter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueIncomingInventory(job));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/scheduleonoutgoinginventory")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleOutgoingInventory(OutgoingInventoryJobParamenter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueOutgoingInventory(job));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }
    }

}
