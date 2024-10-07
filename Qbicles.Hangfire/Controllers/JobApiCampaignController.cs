using System;
using System.Net;
using System.Web.Http;
using Hangfire;
using Qbicles.BusinessRules.Model;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class JobApiCampaignController : ApiController
    {
        private string JobId { get; set; }

        [Route("api/job/schedulecampaignpost")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleCampaignPost(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueCampaignPost(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueCampaignPost(job), TimeSpan.FromMinutes(job.ReminderMinutes));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Campaign Post Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/removecampaignpost")]
        [AcceptVerbs("POST")]
        public QbicleJobResult RemoveCampaignPost(QbicleJobParameter job)
        {
            BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueRemoveCampaignPost(job));
            return new QbicleJobResult
            {
                JobId = job.JobId,
                Message = "Hangfire Remove Campaign Post completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/executecampaignpost")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ExecuteCampaignPost(QbicleJobParameter job)
        {
            var jobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueExecuteCampaignPost(job));

            return new QbicleJobResult
            {
                JobId = jobId,
                Message = "Hangfire Execute Campaign Post complete",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/scheduleemailcampaignpost")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleEmailCampaignPost(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueEmailCampaignPost(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueEmailCampaignPost(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Campaign Post Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/removeemailcampaignpost")]
        [AcceptVerbs("POST")]
        public QbicleJobResult RemoveEmailCampaignPost(QbicleJobParameter job)
        {
            BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueRemoveEmailCampaignPost(job));
            return new QbicleJobResult
            {
                JobId = job.JobId,
                Message = "Hangfire Remove Campaign Post completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/executeemailcampaignpost")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ExecuteEmailCampaignPost(QbicleJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueExecuteEmailCampaignPost(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Execute Campaign Post complete",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/schedulecampaignpostreminder")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleCampaignPostReminder(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueScheduleCampaignPostReminder(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueScheduleCampaignPostReminder(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Campaign Reminder Post Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/removecampaignpostreminder")]
        [AcceptVerbs("POST")]
        public QbicleJobResult RemoveCampaignPostReminder(QbicleJobParameter job)
        {
            BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueRemoveCampaignPostReminder(job));

            return new QbicleJobResult
            {
                JobId = job.JobId,
                Message = "Hangfire Remove Campaign Reminder Post completed!",
                Status = HttpStatusCode.OK
            };
        }
    }
}
