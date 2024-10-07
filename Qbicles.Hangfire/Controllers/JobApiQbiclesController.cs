using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Hangfire;
using Hangfire.Common;
using Qbicles.BusinessRules.Model;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class JobApiQbiclesController : ApiController
    {
        private string JobId { get; set; }

        [Route("api/pos/ping")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Ping()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.ReasonPhrase = "OK";
            return response;
        }

        [Route("api/job/verifyhangfirejobstate")]
        [AcceptVerbs("POST")]
        public QbicleJobResult VerifyHangfireJobState(QbicleJobParameter job)
        {
            var connection = JobStorage.Current.GetConnection();
            var jobData = connection.GetJobData(job.JobId);
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = jobData.State,
                Status = HttpStatusCode.OK
            };

        }


        [Route("api/job/deletehangfirejobstate")]
        [AcceptVerbs("POST")]
        public bool DeleteHangfireJobState(QbicleJobParameter job)
        {
            var connection = JobStorage.Current.GetConnection();
            //var jobData = connection.GetJobData(job.JobId);
            return BackgroundJob.Delete(job.JobId);
            //RecurringJob.RemoveIfExists(job.JobId);          
        }

        [Route("api/job/scheduleqbicleactivity")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleQbicleActivity(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueQbicleActivity(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueQbicleActivity(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/schedulecommentonactivity")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleCommentOnActivity(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueCommentOnActivity(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueCommentOnActivity(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/scheduleonqbicle")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleOnQbicle(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueOnQbicle(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueOnQbicle(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/schedulemediaupload")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleMediaUpload(QbicleJobParameter job)
        {

            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueMediaUpload(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueMediaUpload(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/updatestatusapprovalcannotattendeventclosetask")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleUpdateStatusApprovalCannotAttendEventCloseTask(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueUpdateStatusApprovalCannotAttendEventCloseTask(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueUpdateStatusApprovalCannotAttendEventCloseTask(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/notification2usercreateremovefromdomain")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleNotification2UserCreateRemoveFromDomain(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotification2UserCreateRemoveFromDomain(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotification2UserCreateRemoveFromDomain(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/processnewdomaincreated")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ProcessNewDomainCreated(QbicleJobParameter job)
        {
            var JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueProcessNewDomainCreated(job));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Process New Domain Created Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/scheduleoninvitedqbicle")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleOnInvitedQbicle(QbicleJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueOnInvitedQbicle(job));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Invited Qbicles!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/notification2taskassignee")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleNotification2TaskAssignee(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotification2TaskAssignee(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotification2TaskAssignee(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/notificationonc2cconnectionissued")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleNotificationOnC2CConnectionIssued(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotificationOnC2CConnectionIssued(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotificationOnC2CConnectionIssued(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/notificationonc2cconnectionaccepted")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleNotificationOnC2CConnectionAccepted(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotificationOnC2CConnectionAccepted(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotificationOnC2CConnectionAccepted(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/notificationb2cconnectioncreated")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleNotificationOnB2CConnectionCreated(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotificationOnB2CConnectionCreated(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotificationOnB2CConnectionCreated(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/notifyonflaglisting")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleSetFlagListingPost(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotificationListingInterested(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotificationListingInterested(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Setting Flag For Listing Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/discussionparticipants")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleDiscussionParticipants(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotificationDiscussionParticipants(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotificationDiscussionParticipants(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Discussion Participant Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/processdomainrequest")]
        [AcceptVerbs("POST")]
        public QbicleJobResult NotifyOnProcessDomainRequest(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotifyOnProcessDomainRequest(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotifyOnProcessDomainRequest(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/domainrequestcreated")]
        [AcceptVerbs("POST")]
        public QbicleJobResult NotifyOnDomainRequestCreated(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotifyOnDomainRequestCreated(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotifyOnDomainRequestCreated(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/processextensionrequest")]
        [AcceptVerbs("POST")]
        public QbicleJobResult NotifyOnProcessExtensionRequest(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotifyOnProcessExtensionRequest(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotifyOnProcessExtensionRequest(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/extensionrequestcreated")]
        [AcceptVerbs("POST")]
        public QbicleJobResult NotifyOnExtensionRequestCreated(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotifyOnExtensionRequestCreated(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotifyOnExtensionRequestCreated(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/notifysubscriptiontrialend")]
        [AcceptVerbs("POST")]
        public QbicleJobResult NofifyOnSubscriptionTrialEnd(QbicleJobParameter job)
        {
            var trialEndJobId = BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotifyOnSubscriptionTrialEnd(job), job.JobExecuteTime);

            var dbContext = new ApplicationDbContext();
            var currentDomainPlanId = job.ActivityNotification.ObjectById;
            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Id.ToString() == currentDomainPlanId);
            currentDomainPlan.TrialEndNotiHangfireJobId = trialEndJobId;
            dbContext.Entry(currentDomainPlan).State = System.Data.Entity.EntityState.Modified;
            dbContext.SaveChanges();

            return new QbicleJobResult
            {
                JobId = trialEndJobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }


        [Route("api/job/notifynextsubscriptionpaymentdate")]
        [AcceptVerbs("POST")]
        public QbicleJobResult NofifyOnNextSubscriptionPaymentDate(QbicleJobParameter job)
        {
            var jobStartTime = job.JobExecuteTime;
            var day = jobStartTime.Day;
            var hour = jobStartTime.Hour;

            var recurringString = $"0 {hour} {day} * *";
            var paymentJobId = "domain-id-" + job.ActivityNotification.DomainId + "-next-payment-reminder";

            new RecurringJobManager().AddOrUpdate(paymentJobId, () => BackgroundJobEnqueue.QueueNotifyOnNextSubscriptionPaymentDate(job), recurringString);

            var dbContext = new ApplicationDbContext();
            var currentDomainPlanId = job.ActivityNotification.ObjectById;
            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Id.ToString() == currentDomainPlanId);
            currentDomainPlan.SubPaymnetDateNotiHangfireJobId = paymentJobId;
            dbContext.Entry(currentDomainPlan).State = System.Data.Entity.EntityState.Modified;
            dbContext.SaveChanges();

            return new QbicleJobResult
            {
                JobId = paymentJobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/deletehangfirerecurringjobstate")]
        [AcceptVerbs("POST")]
        public QbicleJobResult DeleteHangfireRecurringJobState(QbicleJobParameter job)
        {
            var connection = JobStorage.Current.GetConnection();
            //var jobData = connection.GetJobData(job.JobId);
            RecurringJob.RemoveIfExists(job.JobId);
            return new QbicleJobResult
            {
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/jountowaitlist")]
        [AcceptVerbs("POST")]
        public QbicleJobResult NotifyOnJoinToWaitlist(QbicleJobParameter job)
        {
            JobId = Math.Abs(job.ReminderMinutes) < 1 ?
                BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueNotifyOnJoinToWaitlist(job)) :
                BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueNotifyOnJoinToWaitlist(job), TimeSpan.FromMinutes(job.ReminderMinutes));

            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Schedule Activity Completed!",
                Status = HttpStatusCode.OK
            };
        }
    }
}