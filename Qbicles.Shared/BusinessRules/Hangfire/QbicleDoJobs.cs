using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.BusinessRules.Loyalty;
using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Ods;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.Movement;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Hangfire
{
    //Campaign
    internal class ScheduleCampaignPost
    {
        public async Task DoScheduleCampaignPost(int id)
        {
            var dbContext = new ApplicationDbContext();
            await new CampaignRules(dbContext).PostSocialNetworkAccounts(id);
        }
    }

    internal class ExecuteCampaignPost
    {
        public async Task DoExecuteCampaignPost(int id)
        {
            var dbContext = new ApplicationDbContext();
            await new CampaignRules(dbContext).PostSocialNetworkAccounts(id);
        }
    }

    internal class ScheduleEmailCampaignPost
    {
        public async Task DoScheduleEmailCampaignPostAsync(int id)
        {
            var dbContext = new ApplicationDbContext();
            await new CampaignRules(dbContext).PostEmailCampaign(id);
        }
    }

    internal class ExecuteEmailCampaignPost
    {
        public async Task DoExecuteEmailCampaignPostAsync(int id)
        {
            var dbContext = new ApplicationDbContext();
            await new CampaignRules(dbContext).PostEmailCampaign(id);
        }
    }

    // Reminder
    internal class ScheduleCampaignPostReminder
    {
        public async void DoScheduleCampaignPostReminder(int id)
        {
            var dbContext = new ApplicationDbContext();
            await new CampaignRules(dbContext).PostSocialCampaignNotification(id);
        }
    }

    /// <summary>
    /// SignalR on create Activity stream
    /// </summary>
    internal class ScheduleQbicleActivity
    {
        public void DoScheduleQbicleActivity(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalR2Activity(job);
        }
    }

    /// <summary>
    /// SignalR comment on Activity
    /// </summary>
    internal class ScheduleCommentOnActivity
    {
        public void DoScheduleCommentOnActivity(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRComment2Activity(job.ActivityNotification);
        }
    }

    /// <summary>
    /// SignalR Chatting
    /// </summary>
    internal class ScheduleChatting
    {
        public void DoScheduleChatting(ChattingJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalR2UserChatting(job);
        }
    }

    /// <summary>
    /// SignalR on action on Qbicle
    /// </summary>
    internal class ScheduleOnQbicle
    {
        public void DoScheduleOnQbicle(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalR2Qbicle(job);
        }
    }

    internal class ScheduleOnInvitedQbicle
    {
        public void DoScheduleOnInvitedQbicle(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalR2InvitedQbicle(job);
        }
    }

    /// <summary>
    /// SignalR on action on Qbicle
    /// </summary>
    internal class ScheduleMediaUpload
    {
        public void DoScheduleMediaUpload(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRMediaUpload(job);
        }
    }

    /// <summary>
    /// process file upload
    /// </summary>
    internal class ScheduleAwsS3FileUploadProcess
    {
        public async Task DoAwsS3FileUploadProcess(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            await new AzureStorageRules(dbContext).AwsS3FileUploadProcessAsync(job);
        }
    }

    /// <summary>
    /// SignalR on action on Update Status Approval Cannot Attend Event Close Task
    /// </summary>
    internal class ScheduleUpdateStatusApprovalCannotAttendEventCloseTask
    {
        public void DoScheduleUpdateStatusApprovalCannotAttendEventCloseTask(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRUpdateStatusApprovalCannotAttendEventCloseTask(job);
        }
    }

    /// <summary>
    /// SignalR on action on User Create Remove From Domain
    /// </summary>
    internal class ScheduleNotification2UserCreateRemoveFromDomain
    {
        public void DoScheduleNotification2UserCreateRemoveFromDomain(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRUserCreateRemoveFromDomain(job);
        }
    }

    internal class ScheduleProcessOrder
    {
        public async Task DoScheduleProcessOrder(OrderJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            await new ProcessOrderRules(dbContext).ProcessOrderAsync(job);
        }

        public async Task DoScheduleProcessOrderForB2B(OrderJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            await new ProcessOrderRules(dbContext).ProcessOrderForB2BAsync(job);
        }
    }

    internal class ScheduleManufactureProduct
    {
        public async Task DoScheduleManufactureProduct(ManufactureProductJobParamenter manufactureProduct)
        {
            var dbContext = new ApplicationDbContext();
            await new TraderManufacturingRules(dbContext).ScheduleManufactureProduct(manufactureProduct);
        }
    }

    internal class ScheduleOutgoingInventory
    {
        public async Task DoScheduleOutgoingInventory(OutgoingInventoryJobParamenter outgoing)
        {
            var dbContext = new ApplicationDbContext();
            await new TraderTransfersRules(dbContext).ScheduleOutgoingInventory(outgoing);
        }
    }

    internal class ScheduleIncomingInventory
    {
        public async Task DoScheduleIncomingInventory(IncomingInventoryJobParamenter incoming)
        {
            var dbContext = new ApplicationDbContext();
            await new TraderTransfersRules(dbContext).ScheduleIncomingInventory(incoming);
        }
    }

    /// <summary>
    /// Schedule to create No Movement Report Entries checking
    /// </summary>
    internal class ScheduleNoMovementCheck
    {
        public void DoScheduleNoMovementCheck(TraderMovementJobParamenter job)
        {
            var dbContext = new ApplicationDbContext();
            new TraderMovementRules(dbContext).ScheduleNoMovementCheck(job.AlertConstraintId, job.UserId, CheckEvent.Daily, false);
        }
    }

    /// <summary>
    /// Schedule to create Min Max Report Entries checking
    /// </summary>
    internal class ScheduleMinMaxCheck
    {
        public void DoScheduleMinMaxCheck(TraderMovementJobParamenter job)
        {
            var dbContext = new ApplicationDbContext();
            new TraderMovementRules(dbContext).ScheduleMinMaxCheck(job.AlertConstraintId, job.UserId, CheckEvent.Daily, false);
        }
    }

    /// <summary>
    /// Schedule to create Accumulation Report Entries checking
    /// </summary>
    internal class ScheduleAccumulationCheck
    {
        public void DoScheduleAccumulationCheck(TraderMovementJobParamenter job)
        {
            var dbContext = new ApplicationDbContext();
            new TraderMovementRules(dbContext).ScheduleAccumulationCheck(job.AlertConstraintId, job.UserId, CheckEvent.Daily, false);
        }
    }

    internal class ExecuteProcessNewDomainCreated
    {
        public async Task DoExecuteProcessNewDomainCreated(int domainId)
        {
            var dbContext = new ApplicationDbContext();
            await new SubscriptionPayments(dbContext).NewDomainCreated(domainId);
        }
    }

    /// <summary>
    /// SignalR on action on Assign Task to User
    /// </summary>
    internal class ScheduleNotification2TaskAssignee
    {
        public void DoScheduleNotification2TaskAssignee(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRAssignTask(job);
        }
    }

    /// <summary>
    /// SignalR on action on Issue C2C Connection
    /// </summary>
    internal class ScheduleNotificationOnC2CProcesses
    {
        public void DoScheduleNotificationOnC2CConnectionProcess(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRC2CConnection(job);
        }
    }

    internal class ScheduleNotifyOnProcessDomainRequest
    {
        public void DoScheduleNotifyOnProcessDomainRequest(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRProcessDomainRequest(job);
        }
    }

    internal class ScheduleNotifyOnDomainRequestCreated
    {
        public void DoScheduleNotifyOnDomainRequestCreated(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalROnDomainRequestCreated(job);
        }
    }

    internal class ScheduleNotifyOnJoinToWaitlist
    {
        public void DoScheduleNotifyOnJoinToWaitlist(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new WaitListRules(dbContext).SignalRNotificationToAdminUserJoinToWaitlist(job);
        }
    }

    internal class ScheduleNotifyOnProcessExtensionRequest
    {
        public void DoScheduleNotifyOnProcessExtensionRequest(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRProcessExtensionRequest(job);
        }
    }

    internal class ScheduleNotifyOnExtensionRequestCreated
    {
        public void DoScheduleNotifyOnExtensionRequestCreated(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalROnExtensionRequestCreated(job);
        }
    }

    internal class ScheduleNotificationOnB2CProcesses
    {
        public void DoSchedultNotificationOnB2CConnection(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRB2CConnection(job);
        }
    }

    internal class ScheduleCategoryOnCatalogQuickMode
    {
        public void DoScheduleCategoryOnCatalogQuickMode(CatalogJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new PosMenuRules(dbContext).ProcessCategoryWithProductGroupIds(job.ProductGroupIds, job.CatalogId, job.LocationId, job.UserId);
        }
    }

    internal class SchedulePosRequestLog
    {
        public void DoSchedulePosRequestLog(PosProcessLogParameter request)
        {
            var dbContext = new ApplicationDbContext();
            new PosRequestRules(dbContext).PosApiRequestLog(request);
        }
    }

    internal class SchedulePosProcessActiveOrder
    {
        public void DoSchedulePosProcessActiveOrder(PosProcessActiveOrderParameter request)
        {
            var dbContext = new ApplicationDbContext();
            new PosRequestRules(dbContext).PosProcessActiveOrder(request);
        }
    }

    internal class SchedulePosSendToPrep
    {
        public void DoSchedulePosSendToPrep(PosSendToPrepParameter request)
        {
            var dbContext = new ApplicationDbContext();
            new OdsApiRules(dbContext).SenToPrepHangfieExecute(request);
        }
    }

    internal class ScheduleTradeOrderLogging
    {
        public void DoScheduleTradeOrderLogging(TradeOrderLoggingParameter request)
        {
            var dbContext = new ApplicationDbContext();
            new TradeOrderLoggingRules(dbContext).TradeOrderLoggingExecute(request);
        }
    }

    internal class ScheduleRefreshPrices
    {
        public void DoScheduleRefreshPrices(CatalogJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new PosMenuRules(dbContext).ProcessRefreshPrices(job.CategoryIds);
        }
    }

    internal class SchedulePushPricesToPricingPool
    {
        public void DoSchedulePushPricesToPricingPool(CatalogJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new PosMenuRules(dbContext).ProcessPushPricesToPricingPool(job.CategoryIds);
        }
    }

    internal class ScheduleCloneCatalog
    {
        public void DoScheduleCloneCatalog(CatalogJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new PosMenuRules(dbContext).ProcessCloneCatalog(job.CatalogId, job.NewMenuId);
        }
    }

    internal class ScheduleDeleteCatalog
    {
        public void DoScheduleDeleteCatalog(CatalogJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new PosMenuRules(dbContext).ProcessDeleteCatalog(job.CatalogId);
        }
    }

    internal class ScheduleItemsImportProcess
    {
        public async Task DoScheduleItemsImportProcess(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            await new TraderItemImportRules(dbContext).ProcessItemsImportAsync(job.JobId);
        }
    }

    internal class ScheduleUpdateCatalogProductSqlite
    {
        public async Task DoScheduleUpdateCatalogProductSqlite(CatalogJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            await new PosProductRules(dbContext).ProcessUpdateCatalogProductSqliteAsync(job.CatalogId);
        }
    }

    internal class SchedultNotificationListingInterested
    {
        public void DoScheduleNotifyHighlightInterested(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).FlagListing(job);
        }
    }

    internal class SchedultNotificationDiscussionParticipants
    {
        public void DoScheduleNotifyDiscussionParticipants(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalRDiscussionParticipants(job);
        }
    }

    internal class ScheduleGenerateStorePoinFromPaymentApproved
    {
        public void DoScheduleGenerateStorePoinFromPaymentApproved(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new StorePointRules(dbContext).GenerateStorePoinFromPaymentApproved(job.Id);
        }
    }

    internal class ScheduleDecreaseStoreCreditFromPaymentApproved
    {
        public void DoScheduleDecreaseStoreCreditFromPaymentApproved(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new StoreDebitRules(dbContext).DecreaseStoreCreditFromPaymentApproved(job.Id);
        }
    }

    //Event & Task notification points
    internal class ScheduleEventNotificationPoints
    {
        public void DoScheduleEventNotificationPoints(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalREmailEventTaskNotificationPoints(job);
        }
    }

    internal class ScheduleTaskNotificationPoints
    {
        public void DoScheduleTaskNotificationPoints(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).SignalREmailEventTaskNotificationPoints(job);
        }
    }

    internal class ScheduleNotifyOnSubscriptionTrialEnd
    {
        public void DoScheduleNotifyOnSubscriptionTrialEnd(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).CreateNotificationOnSubscriptionTrialEnd(job);
        }
    }

    internal class ScheduleNofifyOnSubscriptionNextPaymentDate
    {
        public void DoScheduleNofifyOnSubscriptionNextPaymentDate(QbicleJobParameter job)
        {
            var dbContext = new ApplicationDbContext();
            new NotificationRules(dbContext).CreateNotificationOnSubscriptionNextPaymentDate(job);
        }
    }

    public class QbiclesJob : ServiceToken
    {
        /// <summary>
        /// Hang Fire Excecute Async
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangFireExcecuteAsync(QbicleJobParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        /// <summary>
        /// Order Hangfire Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangFireExcecuteAsync(OrderJobParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        /// <summary>
        /// Order Chatting Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangFireExcecuteAsync(ChattingJobParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        /// <summary>
        /// Manufacture Product Hangfire Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangFireExcecuteAsync(ManufactureProductJobParamenter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        /// <summary>
        /// Incoming Inventory Hangfire Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangFireExcecuteAsync(IncomingInventoryJobParamenter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        /// <summary>
        /// Outgoing Inventory Hangfire Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangFireExcecuteAsync(OutgoingInventoryJobParamenter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        /// <summary>
        /// Trader Movement Hangfire Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<QbicleJobResult> HangFireExcecuteAsync(TraderMovementJobParamenter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        public async Task<QbicleJobResult> HangFireExcecuteAsync(CatalogJobParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        public async Task<QbicleJobResult> HangFireExcecuteAsync(PosProcessLogParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        public async Task<QbicleJobResult> HangFireExcecuteAsync(PosProcessActiveOrderParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        public async Task<QbicleJobResult> HangFireExcecuteAsync(TradeOrderLoggingParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        public async Task<QbicleJobResult> HangFireExcecuteAsync(PosSendToPrepParameter job)
        {
            return await ExcecuteJobAsync(job, job.EndPointName);
        }

        private readonly ScheduleCampaignPost doScheduleCampaignPost;
        private readonly ScheduleEmailCampaignPost doScheduleEmailCampaignPost;
        private readonly ExecuteCampaignPost doExecuteCampaignPost;
        private readonly ExecuteEmailCampaignPost doExecuteEmailCampaignPost;
        private readonly ScheduleCampaignPostReminder doScheduleCampaignPostReminder;

        private readonly ScheduleQbicleActivity doScheduleQbicleActivity;
        private readonly ScheduleCommentOnActivity doScheduleCommentOnActivity;
        private readonly ScheduleChatting doScheduleChatting;
        private readonly ScheduleOnQbicle doScheduleOnQbicle;
        private readonly ScheduleOnInvitedQbicle doScheduleOnInvitedQbicle;
        private readonly ScheduleMediaUpload doScheduleMediaUpload;

        private readonly ScheduleAwsS3FileUploadProcess doScheduleAwsS3FileUploadProcess;

        private readonly ScheduleUpdateStatusApprovalCannotAttendEventCloseTask
            doScheduleUpdateStatusApprovalCannotAttendEventCloseTask;

        private readonly ScheduleNotification2UserCreateRemoveFromDomain
            doScheduleNotification2UserCreateRemoveFromDomain;

        //Notification to user whom assigned a task to
        private readonly ScheduleNotification2TaskAssignee
            doScheduleNotification2TaskAssignee;

        //Inventory handle
        private readonly ScheduleManufactureProduct doScheduleManufactureProduct;

        private readonly ScheduleOutgoingInventory doScheduleOutgoingInventory;
        private readonly ScheduleIncomingInventory doScheduleIncomingInventory;

        //TraderMovement handle
        private readonly ScheduleNoMovementCheck doScheduleNoMovementCheck;

        private readonly ScheduleMinMaxCheck doScheduleMinMaxCheck;
        private readonly ScheduleAccumulationCheck doScheduleAccumulationCheck;
        private readonly ExecuteProcessNewDomainCreated doExecuteProcessNewDomainCreated;

        //Process order from POS, MyDesl, B2C
        private readonly ScheduleProcessOrder doDoScheduleProcessOrder;

        //Process connection from B2C, C2C
        private readonly ScheduleNotificationOnC2CProcesses doScheduleC2CConnection;

        private readonly ScheduleNotificationOnB2CProcesses doScheduleB2CConnection;

        //process Catalog
        private readonly ScheduleCategoryOnCatalogQuickMode doScheduleCatalogQuickMode;

        private readonly SchedulePosRequestLog doSchedulePosRequestLog;
        private readonly SchedulePosProcessActiveOrder doSchedulePosProcessActiveOrder;
        private readonly SchedulePosSendToPrep doSchedulePosSendToPrep;
        private readonly ScheduleTradeOrderLogging doScheduleTradeOrderLogging;
        private readonly ScheduleRefreshPrices doScheduleRefreshPrices;
        private readonly SchedulePushPricesToPricingPool doSchedulePushPricesToPricingPool;
        private readonly ScheduleCloneCatalog doScheduleCloneCatalog;
        private readonly ScheduleDeleteCatalog doScheduleDeleteCatalog;
        private readonly ScheduleUpdateCatalogProductSqlite doScheduleUpdateCatalogProductSqlite;
        private readonly ScheduleItemsImportProcess doScheduleItemsImportProcess;

        //Process set flag / user express interest in Listing highlight
        private readonly SchedultNotificationListingInterested doScheduleListingInterest;

        private readonly SchedultNotificationDiscussionParticipants doScheduleDiscussionParticipants;

        //Process Monibac > Loyalty > CalculatePoint when a payment is approved
        private readonly ScheduleGenerateStorePoinFromPaymentApproved doScheduleGenerateStorePoinFromPaymentApproved;

        private readonly ScheduleDecreaseStoreCreditFromPaymentApproved doScheduleDecreaseStoreCreditFromPaymentApproved;

        //Process DomainRequest
        private readonly ScheduleNotifyOnProcessDomainRequest doScheduleDomainRequest;

        private readonly ScheduleNotifyOnDomainRequestCreated doScheduleDomainRequestCreated;
        private readonly ScheduleNotifyOnJoinToWaitlist doScheduleJoinToWaitlist;

        //Extension Request
        private readonly ScheduleNotifyOnProcessExtensionRequest doScheduleProcessExtensionRequest;

        private readonly ScheduleNotifyOnExtensionRequestCreated doScheduleProcessExtensionRequestCreated;

        // Domain Subscription
        private readonly ScheduleNotifyOnSubscriptionTrialEnd doScheduleNotifyOnSubscriptionTrialEnd;

        private readonly ScheduleNofifyOnSubscriptionNextPaymentDate doScheduleNofifyOnSubscriptionNextPaymentDate;

        //Event & Task notification points
        private readonly ScheduleEventNotificationPoints doScheduleEventNotificationPoints;

        private readonly ScheduleTaskNotificationPoints doScheduleTaskNotificationPoints;

        public QbiclesJob()
        {
            doScheduleCampaignPost = new ScheduleCampaignPost();
            doExecuteCampaignPost = new ExecuteCampaignPost();
            doScheduleEmailCampaignPost = new ScheduleEmailCampaignPost();
            doExecuteEmailCampaignPost = new ExecuteEmailCampaignPost();
            doScheduleCampaignPostReminder = new ScheduleCampaignPostReminder();

            doScheduleQbicleActivity = new ScheduleQbicleActivity();
            doScheduleCommentOnActivity = new ScheduleCommentOnActivity();
            doScheduleChatting = new ScheduleChatting();
            doScheduleOnQbicle = new ScheduleOnQbicle();
            doScheduleOnInvitedQbicle = new ScheduleOnInvitedQbicle();
            doScheduleMediaUpload = new ScheduleMediaUpload();

            doScheduleAwsS3FileUploadProcess = new ScheduleAwsS3FileUploadProcess();
            doScheduleUpdateStatusApprovalCannotAttendEventCloseTask = new ScheduleUpdateStatusApprovalCannotAttendEventCloseTask();
            doScheduleNotification2UserCreateRemoveFromDomain = new ScheduleNotification2UserCreateRemoveFromDomain();

            doScheduleManufactureProduct = new ScheduleManufactureProduct();
            doScheduleOutgoingInventory = new ScheduleOutgoingInventory();
            doScheduleIncomingInventory = new ScheduleIncomingInventory();

            //TraderMovement job
            doScheduleNoMovementCheck = new ScheduleNoMovementCheck();
            doScheduleMinMaxCheck = new ScheduleMinMaxCheck();
            doScheduleAccumulationCheck = new ScheduleAccumulationCheck();
            doExecuteProcessNewDomainCreated = new ExecuteProcessNewDomainCreated();

            //Process order from POS, MyDesl, B2C
            doDoScheduleProcessOrder = new ScheduleProcessOrder();

            //Notification to user whom assigned a task to
            doScheduleNotification2TaskAssignee = new ScheduleNotification2TaskAssignee();

            //Notification for B2C and C2C
            doScheduleC2CConnection = new ScheduleNotificationOnC2CProcesses();
            doScheduleB2CConnection = new ScheduleNotificationOnB2CProcesses();

            //Catalog
            doScheduleCatalogQuickMode = new ScheduleCategoryOnCatalogQuickMode();
            doSchedulePosRequestLog = new SchedulePosRequestLog();
            doSchedulePosProcessActiveOrder = new SchedulePosProcessActiveOrder();
            doSchedulePosSendToPrep = new SchedulePosSendToPrep();
            doScheduleTradeOrderLogging = new ScheduleTradeOrderLogging();
            doScheduleUpdateCatalogProductSqlite = new ScheduleUpdateCatalogProductSqlite();
            doScheduleRefreshPrices = new ScheduleRefreshPrices();
            doSchedulePushPricesToPricingPool = new SchedulePushPricesToPricingPool();
            doScheduleCloneCatalog = new ScheduleCloneCatalog();
            doScheduleDeleteCatalog = new ScheduleDeleteCatalog();
            doScheduleItemsImportProcess = new ScheduleItemsImportProcess();

            //Notification for Flagging Listing / Expressing interest on Listing
            doScheduleListingInterest = new SchedultNotificationListingInterested();
            doScheduleDiscussionParticipants = new SchedultNotificationDiscussionParticipants();

            //Process Monibac > Loyalty > CalculatePoint when a payment is approved
            doScheduleGenerateStorePoinFromPaymentApproved = new ScheduleGenerateStorePoinFromPaymentApproved();

            doScheduleDecreaseStoreCreditFromPaymentApproved = new ScheduleDecreaseStoreCreditFromPaymentApproved();
            //Process Domain Request
            doScheduleDomainRequest = new ScheduleNotifyOnProcessDomainRequest();
            doScheduleDomainRequestCreated = new ScheduleNotifyOnDomainRequestCreated();
            doScheduleJoinToWaitlist = new ScheduleNotifyOnJoinToWaitlist();
            //Extension Request
            doScheduleProcessExtensionRequest = new ScheduleNotifyOnProcessExtensionRequest();
            doScheduleProcessExtensionRequestCreated = new ScheduleNotifyOnExtensionRequestCreated();

            // Domain Subscription
            doScheduleNotifyOnSubscriptionTrialEnd = new ScheduleNotifyOnSubscriptionTrialEnd();
            doScheduleNofifyOnSubscriptionNextPaymentDate = new ScheduleNofifyOnSubscriptionNextPaymentDate();

            //Event & Task notification points
            doScheduleEventNotificationPoints = new ScheduleEventNotificationPoints();
            doScheduleTaskNotificationPoints = new ScheduleTaskNotificationPoints();
        }

        public async Task ScheduleManufactureProduct(ManufactureProductJobParamenter id)
        {
            await doScheduleManufactureProduct.DoScheduleManufactureProduct(id);
        }

        public async Task ScheduleOutgoingInventory(OutgoingInventoryJobParamenter id)
        {
            await doScheduleOutgoingInventory.DoScheduleOutgoingInventory(id);
        }

        public async Task ScheduleIncomingInventory(IncomingInventoryJobParamenter id)
        {
            await doScheduleIncomingInventory.DoScheduleIncomingInventory(id);
        }

        public async Task ScheduleCampaignPost(int id)
        {
            await doScheduleCampaignPost.DoScheduleCampaignPost(id);
        }

        public async Task ExecuteCampaignPost(int id)
        {
            await doExecuteCampaignPost.DoExecuteCampaignPost(id);
        }

        public async Task ScheduleEmailCampaignPostAsync(int id)
        {
            await doScheduleEmailCampaignPost.DoScheduleEmailCampaignPostAsync(id);
        }

        public async Task ExecuteEmailCampaignPostAsync(int id)
        {
            await doExecuteEmailCampaignPost.DoExecuteEmailCampaignPostAsync(id);
        }

        public void ScheduleCampaignPostReminder(int id)
        {
            doScheduleCampaignPostReminder.DoScheduleCampaignPostReminder(id);
        }

        public void ScheduleQbicleActivity(QbicleJobParameter job)
        {
            doScheduleQbicleActivity.DoScheduleQbicleActivity(job);
        }

        public void ScheduleCommentOnActivity(QbicleJobParameter job)
        {
            doScheduleCommentOnActivity.DoScheduleCommentOnActivity(job);
        }

        public void ScheduleChatting(ChattingJobParameter job)
        {
            doScheduleChatting.DoScheduleChatting(job);
        }

        public void ScheduleOnQbicle(QbicleJobParameter job)
        {
            doScheduleOnQbicle.DoScheduleOnQbicle(job);
        }

        public void ScheduleOnInviteQbicle(QbicleJobParameter job)
        {
            doScheduleOnInvitedQbicle.DoScheduleOnInvitedQbicle(job);
        }

        public void ScheduleMediaUpload(QbicleJobParameter job)
        {
            doScheduleMediaUpload.DoScheduleMediaUpload(job);
        }

        public async Task ScheduleAwsS3FileUploadProcess(QbicleJobParameter job)
        {
            await doScheduleAwsS3FileUploadProcess.DoAwsS3FileUploadProcess(job);
        }

        public void ScheduleUpdateStatusApprovalCannotAttendEventCloseTask(QbicleJobParameter job)
        {
            doScheduleUpdateStatusApprovalCannotAttendEventCloseTask.DoScheduleUpdateStatusApprovalCannotAttendEventCloseTask(job);
        }

        public void ScheduleNotification2UserCreateRemoveFromDomain(QbicleJobParameter job)
        {
            doScheduleNotification2UserCreateRemoveFromDomain.DoScheduleNotification2UserCreateRemoveFromDomain(job);
        }

        public void ScheduleNotification2TaskAssignee(QbicleJobParameter job)
        {
            doScheduleNotification2TaskAssignee.DoScheduleNotification2TaskAssignee(job);
        }

        public void ScheduleGenerateStorePoinFromPaymentApproved(QbicleJobParameter job)
        {
            doScheduleGenerateStorePoinFromPaymentApproved.DoScheduleGenerateStorePoinFromPaymentApproved(job);
        }

        public void ScheduleDecreaseStoreCreditFromPaymentApproved(QbicleJobParameter job)
        {
            doScheduleDecreaseStoreCreditFromPaymentApproved.DoScheduleDecreaseStoreCreditFromPaymentApproved(job);
        }

        #region Do Schedule C2C Jobs

        public void ScheduleNotificationOnC2CConnectionIssued(QbicleJobParameter job)
        {
            doScheduleC2CConnection.DoScheduleNotificationOnC2CConnectionProcess(job);
        }

        public void ScheduleNotificationOnC2CConnectionAccepted(QbicleJobParameter job)
        {
            doScheduleC2CConnection.DoScheduleNotificationOnC2CConnectionProcess(job);
        }

        #endregion Do Schedule C2C Jobs

        #region Do Schedule B2C Jobs

        public void ScheduleNotificationOnB2CConnectionCreated(QbicleJobParameter job)
        {
            doScheduleB2CConnection.DoSchedultNotificationOnB2CConnection(job);
        }

        #endregion Do Schedule B2C Jobs

        #region Do Schedule for Highlight Jobs

        public void ScheduleNotificationListingInterested(QbicleJobParameter job)
        {
            doScheduleListingInterest.DoScheduleNotifyHighlightInterested(job);
        }

        #endregion Do Schedule for Highlight Jobs

        #region Do Schedule for DiscussionParticipants

        public void ScheduleNotificationDiscussionParticipants(QbicleJobParameter job)
        {
            doScheduleDiscussionParticipants.DoScheduleNotifyDiscussionParticipants(job);
        }

        #endregion Do Schedule for DiscussionParticipants

        #region Do Schedule CatalogJobs

        public void ScheduleCategoryOnCatalogQuickMode(CatalogJobParameter job)
        {
            doScheduleCatalogQuickMode.DoScheduleCategoryOnCatalogQuickMode(job);
        }

        public void ScheduleRefreshPrices(CatalogJobParameter job)
        {
            doScheduleRefreshPrices.DoScheduleRefreshPrices(job);
        }

        public void SchedulePushPricesToPricingPool(CatalogJobParameter job)
        {
            doSchedulePushPricesToPricingPool.DoSchedulePushPricesToPricingPool(job);
        }

        public async Task ScheduleUpdateCatalogProductSqlite(CatalogJobParameter job)
        {
            await doScheduleUpdateCatalogProductSqlite.DoScheduleUpdateCatalogProductSqlite(job);
        }

        public void ScheduleCloneCatalog(CatalogJobParameter job)
        {
            doScheduleCloneCatalog.DoScheduleCloneCatalog(job);
        }

        public void ScheduleDeleteCatalog(CatalogJobParameter job)
        {
            doScheduleDeleteCatalog.DoScheduleDeleteCatalog(job);
        }

        public void ScheduleItemsImportProcess(QbicleJobParameter job)
        {
            doScheduleItemsImportProcess.DoScheduleItemsImportProcess(job);
        }

        #endregion Do Schedule CatalogJobs

        public void SchedulePosRequestLog(PosProcessLogParameter request)
        {
            doSchedulePosRequestLog.DoSchedulePosRequestLog(request);
        }

        public void SchedulePosProcessActiveOrder(PosProcessActiveOrderParameter request)
        {
            doSchedulePosProcessActiveOrder.DoSchedulePosProcessActiveOrder(request);
        }

        public void SchedulePosSendToPrep(PosSendToPrepParameter request)
        {
            doSchedulePosSendToPrep.DoSchedulePosSendToPrep(request);
        }

        public void ScheduleTradeOrderLogging(TradeOrderLoggingParameter request)
        {
            doScheduleTradeOrderLogging.DoScheduleTradeOrderLogging(request);
        }

        //TraderMovement handle
        public void ScheduleNoMovementCheck(TraderMovementJobParamenter job)
        {
            doScheduleNoMovementCheck.DoScheduleNoMovementCheck(job);
        }

        public void ScheduleMinMaxCheck(TraderMovementJobParamenter job)
        {
            doScheduleMinMaxCheck.DoScheduleMinMaxCheck(job);
        }

        public void ScheduleAccumulationCheck(TraderMovementJobParamenter job)
        {
            doScheduleAccumulationCheck.DoScheduleAccumulationCheck(job);
        }

        public async Task DoExecuteProcessNewDomainCreated(int id)
        {
            await doExecuteProcessNewDomainCreated.DoExecuteProcessNewDomainCreated(id);
        }

        public async Task ScheduleProcessOrder(OrderJobParameter job)
        {
            await doDoScheduleProcessOrder.DoScheduleProcessOrder(job);
        }

        public async Task ScheduleProcessOrderForB2B(OrderJobParameter job)
        {
            await doDoScheduleProcessOrder.DoScheduleProcessOrderForB2B(job);
        }

        //Domain Request
        public void ScheduleProcessDomainRequest(QbicleJobParameter job)
        {
            doScheduleDomainRequest.DoScheduleNotifyOnProcessDomainRequest(job);
        }

        public void ScheduleOnDomainRequestCreated(QbicleJobParameter job)
        {
            doScheduleDomainRequestCreated.DoScheduleNotifyOnDomainRequestCreated(job);
        }

        public void ScheduleOnJoinToWaitlist(QbicleJobParameter job)
        {
            doScheduleJoinToWaitlist.DoScheduleNotifyOnJoinToWaitlist(job);
        }

        //Extension Request
        public void ScheduleProcessExtensionRequest(QbicleJobParameter job)
        {
            doScheduleProcessExtensionRequest.DoScheduleNotifyOnProcessExtensionRequest(job);
        }

        public void ScheduleOnExtensionRequestCreated(QbicleJobParameter job)
        {
            doScheduleProcessExtensionRequestCreated.DoScheduleNotifyOnExtensionRequestCreated(job);
        }

        // Domain subscription Scheduling Job
        public void ScheduleOnSubscriptionTrialEnd(QbicleJobParameter job)
        {
            doScheduleNotifyOnSubscriptionTrialEnd.DoScheduleNotifyOnSubscriptionTrialEnd(job);
        }

        public void ScheduleOnSubscriptionNextPaymentDate(QbicleJobParameter job)
        {
            doScheduleNofifyOnSubscriptionNextPaymentDate.DoScheduleNofifyOnSubscriptionNextPaymentDate(job);
        }

        //Event & Task notification points
        public void ScheduleEventNotificationPoints(QbicleJobParameter job)
        {
            doScheduleEventNotificationPoints.DoScheduleEventNotificationPoints(job);
        }

        public void ScheduleTaskNotificationPoints(QbicleJobParameter job)
        {
            doScheduleTaskNotificationPoints.DoScheduleTaskNotificationPoints(job);
        }
    }

    public class HangfireState
    {
        public QbicleJobResult VerifyState(string jobId)
        {
            try
            {
                var job = new QbicleJobParameter
                {
                    EndPointName = "verifyhangfirejobstate",
                    JobId = jobId
                };
                //execute SignalR2Activity
                return new QbiclesJob().HangFireExcecuteAsync(job).GetAwaiter().GetResult();
            }
            catch (System.Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new QbicleJobResult { Status = System.Net.HttpStatusCode.InternalServerError, Message = ex.Message, JobId = jobId };
            }
        }
    }
}