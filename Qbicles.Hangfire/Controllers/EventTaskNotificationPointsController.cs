using System;
using System.Net;
using System.Web.Http;
using Hangfire;
using Qbicles.BusinessRules.Model;

namespace Qbicles.Hangfire.Controllers
{
    public class EventTaskNotificationPointsController : ApiController
    {
        private string JobId { get; set; }

        [Route("api/job/scheduleeventnotificationpoints")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleEventNotificationPoints(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueEventNotificationPoints(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueEventNotificationPoints(job), TimeSpan.FromMinutes(job.ReminderMinutes));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Event Notification Points Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/scheduletasknotificationpoints")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleTaskNotificationPoints(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueTaskNotificationPoints(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueTaskNotificationPoints(job), TimeSpan.FromMinutes(job.ReminderMinutes));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Task Notification Points Completed!",
                Status = HttpStatusCode.OK
            };
        }
    }
}