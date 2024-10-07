using Hangfire;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Model;
using System.Threading.Tasks;

namespace Qbicles.Hangfire.Controllers
{
    public static class BackgroundJobEnqueue
    {
        //Inventory job
        [Queue("inventoryjob")]
        public static async Task QueueManufactureProduct(ManufactureProductJobParamenter job)
        {
            await new QbiclesJob().ScheduleManufactureProduct(job);
        }

        [Queue("inventoryjob")]
        public static async Task QueueIncomingInventory(IncomingInventoryJobParamenter job)
        {
            await new QbiclesJob().ScheduleIncomingInventory(job);
        }

        [Queue("inventoryjob")]
        public static async Task QueueOutgoingInventory(OutgoingInventoryJobParamenter job)
        {
            await new QbiclesJob().ScheduleOutgoingInventory(job);
        }

        // Campaign job
        [Queue("campaignjob")]
        public static async Task QueueCampaignPost(QbicleJobParameter job)
        {
            await new QbiclesJob().ScheduleCampaignPost(job.Id);
        }

        [Queue("campaignjob")]
        public static void QueueRemoveCampaignPost(QbicleJobParameter job)
        {
            BackgroundJob.Delete(job.JobId, "campaignjob");
        }

        [Queue("campaignjob")]
        public static async Task QueueExecuteCampaignPost(QbicleJobParameter job)
        {
            if (!string.IsNullOrEmpty(job.JobId))//execute from queue
            {
                BackgroundJob.Requeue(job.JobId);
            }
            else //execute from approved
            {
                await new QbiclesJob().ExecuteCampaignPost(job.Id);
            }
        }

        [Queue("campaignjob")]
        public static async Task QueueEmailCampaignPost(QbicleJobParameter job)
        {
            await new QbiclesJob().ScheduleEmailCampaignPostAsync(job.Id);
        }

        [Queue("campaignjob")]
        public static void QueueRemoveEmailCampaignPost(QbicleJobParameter job)
        {
            BackgroundJob.Delete(job.JobId, "campaignjob");
        }

        [Queue("campaignjob")]
        public static async Task QueueExecuteEmailCampaignPost(QbicleJobParameter job)
        {
            if (!string.IsNullOrEmpty(job.JobId))//execute from queue
            {
                BackgroundJob.Requeue(job.JobId, "campaignjob");
            }
            else //execute from approved
            {
                await new QbiclesJob().ExecuteEmailCampaignPostAsync(job.Id);
            }
        }

        [Queue("campaignjob")]
        public static void QueueScheduleCampaignPostReminder(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleCampaignPostReminder(job.Id);
        }

        [Queue("campaignjob")]
        public static void QueueRemoveCampaignPostReminder(QbicleJobParameter job)
        {
            BackgroundJob.Delete(job.JobId, "campaignjob");
        }

        //Qbicles Job
        [Queue("qbiclesjob")]
        public static void QueueQbicleActivity(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleQbicleActivity(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueCommentOnActivity(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleCommentOnActivity(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueOnQbicle(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleOnQbicle(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueUpdateStatusApprovalCannotAttendEventCloseTask(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleUpdateStatusApprovalCannotAttendEventCloseTask(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotification2UserCreateRemoveFromDomain(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleNotification2UserCreateRemoveFromDomain(job);
        }

        [Queue("qbiclesjob")]
        public static async Task QueueProcessNewDomainCreated(QbicleJobParameter job)
        {
            await new QbiclesJob().DoExecuteProcessNewDomainCreated(job.Id);
        }

        [Queue("qbiclesjob")]
        public static void QueueOnInvitedQbicle(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleOnInviteQbicle(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotification2TaskAssignee(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleNotification2TaskAssignee(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotifyOnProcessDomainRequest(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleProcessDomainRequest(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotifyOnDomainRequestCreated(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleOnDomainRequestCreated(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotifyOnJoinToWaitlist(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleOnJoinToWaitlist(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotifyOnProcessExtensionRequest(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleProcessExtensionRequest(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotifyOnExtensionRequestCreated(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleOnExtensionRequestCreated(job);
        }

        // TraderMovement
        [Queue("tradermovementjob")]
        public static void QueueNoMovementCheck(TraderMovementJobParamenter job)
        {
            new QbiclesJob().ScheduleNoMovementCheck(job);
        }

        [Queue("tradermovementjob")]
        public static void QueueMinMaxCheck(TraderMovementJobParamenter job)
        {
            new QbiclesJob().ScheduleMinMaxCheck(job);
        }

        [Queue("tradermovementjob")]
        public static void QueueAccumulationCheck(TraderMovementJobParamenter job)
        {
            new QbiclesJob().ScheduleAccumulationCheck(job);
        }

        //Inventory job
        [Queue("processorder")]
        public static async Task QueueProcessOrder(OrderJobParameter job)
        {
            await new QbiclesJob().ScheduleProcessOrder(job);
        }

        [Queue("processorder")]
        public static async Task QueueProcessOrderForB2B(OrderJobParameter job)
        {
            await new QbiclesJob().ScheduleProcessOrderForB2B(job);
        }

        #region Jobs for C2C Processes

        [Queue("qbiclesjob")]
        public static void QueueNotificationOnC2CConnectionIssued(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleNotificationOnC2CConnectionIssued(job);
        }

        [Queue("qbiclesjob")]
        public static void QueueNotificationOnC2CConnectionAccepted(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleNotificationOnC2CConnectionAccepted(job);
        }

        #endregion Jobs for C2C Processes

        #region Jobs for B2C

        [Queue("qbiclesjob")]
        public static void QueueNotificationOnB2CConnectionCreated(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleNotificationOnB2CConnectionCreated(job);
        }

        #endregion Jobs for B2C

        #region Jobs for Catalog

        [Queue("catalogjob")]
        public static void QueueCatalogQuickMode(CatalogJobParameter job)
        {
            new QbiclesJob().ScheduleCategoryOnCatalogQuickMode(job);
        }

        [Queue("catalogjob")]
        public static void QueueRefreshPrices(CatalogJobParameter job)
        {
            new QbiclesJob().ScheduleRefreshPrices(job);
        }

        [Queue("catalogjob")]
        public static void QueuePushPricesToPricingPool(CatalogJobParameter job)
        {
            new QbiclesJob().SchedulePushPricesToPricingPool(job);
        }

        [Queue("catalogjob")]
        public static async Task QueueUpdateCatalogProductSqlite(CatalogJobParameter job)
        {
            await new QbiclesJob().ScheduleUpdateCatalogProductSqlite(job);
        }

        [Queue("catalogjob")]
        public static void QueueCloneCatalog(CatalogJobParameter job)
        {
            new QbiclesJob().ScheduleCloneCatalog(job);
        }

        [Queue("catalogjob")]
        public static void QueueDeleteCatalog(CatalogJobParameter job)
        {
            new QbiclesJob().ScheduleDeleteCatalog(job);
        }

        [Queue("itemsimportjob")]
        public static void QueueItemsImportProcess(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleItemsImportProcess(job);
        }

        #endregion Jobs for Catalog

        #region Jobs for Highlight

        [Queue("qbiclesjob")]
        public static void QueueNotificationListingInterested(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleNotificationListingInterested(job);
        }

        #endregion Jobs for Highlight

        #region Jobs for DiscussionParticipants

        [Queue("qbiclesjob")]
        public static void QueueNotificationDiscussionParticipants(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleNotificationDiscussionParticipants(job);
        }

        #endregion Jobs for DiscussionParticipants

        #region Jobs for Monibac, Loyalty, StoreCredit

        [Queue("loyaltyjob")]
        public static void QueueGenerateStorePoinFromPaymentApproved(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleGenerateStorePoinFromPaymentApproved(job);
        }

        [Queue("loyaltyjob")]
        public static void QueueDecreaseStoreCreditFromPaymentApproved(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleDecreaseStoreCreditFromPaymentApproved(job);
        }

        #endregion Jobs for Monibac, Loyalty, StoreCredit

        #region Media upload

        [Queue("processmedia")]
        public static void QueueMediaUpload(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleMediaUpload(job);
        }

        [Queue("processmedia")]
        public static async Task QueueAwsS3FileUploadProcess(QbicleJobParameter job)
        {
            await new QbiclesJob().ScheduleAwsS3FileUploadProcess(job);
        }

        #endregion Media upload

        #region Chatting

        [Queue("chatting")]
        public static void QueueChatting(ChattingJobParameter job)
        {
            new QbiclesJob().ScheduleChatting(job);
        }

        #endregion Chatting

        #region Trade order logging

        [Queue("orderstatus")]
        public static void QueueTradeOrderLogging(TradeOrderLoggingParameter request)
        {
            new QbiclesJob().ScheduleTradeOrderLogging(request);
        }

        #endregion Trade order logging

        #region POS Request log

        [Queue("pos")]
        public static void QueuePosRequestLog(PosProcessLogParameter request)
        {
            new QbiclesJob().SchedulePosRequestLog(request);
        }

        #endregion POS Request log

        #region Driver update location/ process active order

        [Queue("orderstatus")]
        public static void QueuePosProcessActiveOrder(PosProcessActiveOrderParameter request)
        {
            new QbiclesJob().SchedulePosProcessActiveOrder(request);
        }

        [Queue("pos")]
        public static void QueuePosSendToPrep(PosSendToPrepParameter request)
        {
            new QbiclesJob().SchedulePosSendToPrep(request);
        }

        #endregion Driver update location/ process active order

        #region Domain Subscription Job

        [Queue("domainsubscriptionjob")]
        public static void QueueNotifyOnSubscriptionTrialEnd(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleOnSubscriptionTrialEnd(job);
        }

        [Queue("domainsubscriptionjob")]
        public static void QueueNotifyOnNextSubscriptionPaymentDate(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleOnSubscriptionNextPaymentDate(job);
        }

        #endregion Domain Subscription Job

        #region Event & Task notification points

        // Campaign job
        [Queue("eventtasknotificationpoints")]
        public static void QueueEventNotificationPoints(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleEventNotificationPoints(job);
        }

        [Queue("eventtasknotificationpoints")]
        public static void QueueTaskNotificationPoints(QbicleJobParameter job)
        {
            new QbiclesJob().ScheduleTaskNotificationPoints(job);
        }

        #endregion Event & Task notification points
    }
}