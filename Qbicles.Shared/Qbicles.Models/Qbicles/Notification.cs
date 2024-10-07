using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Base;
using Qbicles.Models.Highlight;
using Qbicles.Models.WaitList;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_Notifications")]
    public class Notification : DataModelBase
    {
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        [NotMapped]
        public string OriginatingConnectionId { get; set; } = string.Empty;
        /// <summary>
        /// connection id,(user created notification) using for case a user log in to multi app
        /// for check on signalR if current connection != originaling creation id then reload
        /// </summary>
        [NotMapped]
        public string OriginatingCreationId { get; set; } = string.Empty;

        public enum ActivityType
        {
            Link = 1,
            Task = 2,
            Discussion = 3,
            Alert = 4,
            Media = 5,
            Event = 6,
            Approval = 7,
            Process = 8,
            UploadTransaction = 9,
            UserAvatar = 10,
            BookKepping = 11,
            Community = 12,
            Trader = 13
        }

        public enum NotificationSendMethodEnum
        {
            [Description("Notify By Email")]
            Email = 1,
            [Description("Notify By Broadcast")] // Through SignalR
            Broadcast = 2,
            [Description("Notify By Broadcast and Email")] // Through SignalR and Email
            Both = 3,
            [Description("No notification")]
            None = 4
        }

        public enum NotificationSound
        {
            No = 1,
            Yes = 2
        }

        public enum NotificationEventEnum
        {
            [Description("Qbicle Creation")]
            QbicleCreation = 1,
            [Description("Qbicle Update")]
            QbicleUpdate = 2,
            [Description("Discussion Creation")]
            DiscussionCreation = 3,
            [Description("Discussion Update")]
            DiscussionUpdate = 4, //This includes when any Post, Task, Event, Alert or File is added to a discussion
            [Description("Task Creation")]
            TaskCreation = 5,
            [Description("Task Completion")]
            TaskCompletion = 6,
            [Description("Task WorkLog")]
            TaskWorkLog = 46,
            [Description("Alert Creation")]
            AlertCreation = 7,
            [Description("Event Creation")]
            EventCreation = 8,
            [Description("Event Withdrawal")]
            EventWithdrawl = 9, // When a user indicates that they are not going to attend an event
            [Description("Media Creation")]
            MediaCreation = 10,
            [Description("Post Creation")]
            PostCreation = 11,
            [Description("Approval Creation")]
            ApprovalCreation = 12,
            [Description("Create Member")]
            CreateMember = 13,
            [Description("Invited Member")]
            InvitedMember = 14,
            [Description("Alert Update")]
            AlertUpdate = 15,
            [Description("Approval Update")]
            ApprovalUpdate = 16,
            [Description("Task Update")]
            TaskUpdate = 17,
            [Description("Event update")]
            EventUpdate = 18,
            [Description("Topic post")]
            TopicPost = 19,
            [Description("Media Update")]
            MediaUpdate = 20,
            [Description("Approval Reviewed")]
            ApprovalReviewed = 21,
            [Description("Approval Approved")]
            ApprovalApproved = 22,
            [Description("Approval Denied")]
            ApprovalDenied = 23,
            [Description("Journal Entry Post")]
            JournalPost = 24,
            [Description("Transaction Post")]
            TransactionPost = 25,
            [Description("Link Post")]
            LinkCreation = 26,
            [Description("Remove user out of Domain")]
            RemoveUserOutOfDomain = 27,
            [Description("Reminder campaign post")]
            ReminderCampaignPost = 28,
            [Description("Remove Queue")]
            RemoveQueue = 29,
            [Description("Qbicles invited")]
            QbicleInvited = 30,
            [Description("Assign task")]
            AssignTask = 31,
            [Description("C2C Connection Issued")]
            C2CConnectionIssued = 32,
            [Description("C2C Connection Accepted")]
            C2CConnectionAccepted = 33,
            [Description("B2C Connection Created")]
            B2CConnectionCreated = 34,
            [Description("Link Update")]
            LinkUpdate = 35,
            [Description("Activity Comment")]
            ActivityComment = 36,
            [Description("Listing interested")]
            ListingInterested = 37,
            [Description("Remove user out of Qbicle")]
            RemoveUserOutOfQbicle = 38,
            [Description("Add User to Discussion Participants")]
            AddUserParticipants = 39,
            [Description("Remove User to Discussion Participants")]
            RemoveUserParticipants = 40,
            [Description("Add user to Qbicle")]
            AddUserToQbicle = 41,
            [Description("User is typing")]
            TypingChat = 66,
            [Description("User end typing")]
            EndTypingChat = 88,
            [Description("Approve or Reject Domain Request")]
            ProcessDomainRequest = 89,
            [Description("Create Request")]
            CreateRequest = 90,
            [Description("Approve or Reject Extension Request")]
            ProcessExtensionRequest = 91,
            [Description("Updated B2C Order")]
            B2COrderUpdated = 92,
            [Description("Edit post")]
            PostEdit = 93,
            [Description("Invoice creation is completed")]
            B2COrderInvoiceCreationCompleted = 94,
            [Description("Order being processed")]
            B2COrderBeginProcess = 95,
            [Description("Order completed")]
            B2COrderCompleted = 96,
            [Description("B2C order payment approved")]
            B2COrderPaymentApproved = 97,
            [Description("Media remove version")]
            MediaRemoveVersion = 98,
            [Description("Media add version")]
            MediaAddVersion = 99,
            [Description("Event Notification Points")]
            EventNotificationPoints = 42,
            [Description("Task Notification Points")]
            TaskNotificationPoints = 43,
            [Description("Begun work on Task")]
            TaskStart = 44,
            [Description("Completed task")]
            TaskComplete = 45,
            [Description("Domain Subscription trial time end")]
            DomainSubTrialEnd = 100,
            [Description("Domain Subscription payment date")]
            DomainSubNextPaymentDate = 101,
            [Description("Media Tab Creation")]
            MediaTabCreation = 107,
            [Description("Join to waitlist")]
            JoinToWaitlist = 102,
            [Description("Approval subscription and custom waitlist")]
            ApprovalSubscriptionAndCustomWaitlist = 103,
            [Description("Approval subscription waitlist")]
            ApprovalSubscriptionWaitlist = 104,
            [Description("Approval custom waitlist")]
            ApprovalCustomWaitlist = 105,
            [Description("Reject waitlist")]
            RejectWaitlist = 106,
            [Description("Update MembersList")]
            UpdateMembersList = 108,
            [Description("Order Completed")]
            B2BOrderCompleted = 110,
            [Description("Reload C2C SubNavigation")]
            ReloadC2CSubNavigation = 47,
        }

        public enum ApplicationPageName
        {
            All = 0,
            [Description("Domain")]
            Domain = 1,
            [Description("Qbicle")]
            Qbicle = 2,
            [Description("Activities")]
            Activities = 3,
            [Description("Discussion")]
            Discussion = 4,
            [Description("Alert")]
            Alert = 5,
            [Description("Event")]
            Event = 6,
            [Description("Task")]
            Task = 7,
            [Description("Approval")]
            Approval = 8,
            [Description("Media")]
            Media = 9,
            //[Description("Discussions")]
            //Discussions = 10,
            //[Description("Alerts")]
            //Alerts = 11,
            //[Description("Events")]
            //Events = 12,
            //[Description("Tasks")]
            //Tasks = 13,
            //[Description("Approvals")]
            //Approvals = 14,
            //[Description("Medias")]
            //Medias = 15,
            [Description("bookkeeping")]
            bookkeeping = 16,
            [Description("Link")]
            Link = 17,
            //[Description("Links")]
            //Links = 18,
            [Description("Discussion Order")]
            DiscussionOrder = 10,
            [Description("Activity")]
            Activity = 86,
            [Description("B2C Order")]
            B2COrder = 88,
            [Description("Discussion Menu")]
            DiscussionMenu = 89,
            [Description("B2B Order")]
            B2BOrder = 90,
        }


        //The date on which the notification was created
        [Required]
        public DateTime CreatedDate { get; set; }

        //The user who was logged in when the notification was created
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        //The date on which the notification was sent
        public DateTime SentDate { get; set; }

        //The method by which the notification was sent
        public NotificationSendMethodEnum SentMethod { get; set; }

        //The AppendToPageName on which the Page can Append
        public ApplicationPageName AppendToPageName { get; set; }
        public bool? HasActionToHandle { get; set; }

        //The Activity that caused the Notification (if it was caused by an Activity)
        public virtual QbicleActivity AssociatedAcitvity { get; set; }

        //The Qbicle that cause the notification (it it was caused by a Qbicle)
        public virtual ApplicationUser AssociatedUser { get; set; }

        //The Qbicle that cause the notification (it it was caused by a Qbicle)
        public virtual Qbicle AssociatedQbicle { get; set; }

        //The Qbicle that cause the notification (it it was caused by a Qbicle)
        public virtual QbiclePost AssociatedPost { get; set; }

        //When an user express interest in a Listing Highlight post
        public virtual HighlightPost AssociatedHighlight { get; set; }

        public virtual QbicleDomain AssociatedDomain { get; set; }
        public virtual Invitation.Invitation AssociateInvitation { get; set; }

        //The event that caused the Notification
        [Required]
        public NotificationEventEnum Event { get; set; }

        //The user who is to be notified
        [Required]
        public virtual ApplicationUser NotifiedUser { get; set; }

        //If an email is used for notification, this is the link to the email that has been sent
        public EmailLog EmailSent { get; set; }
        // This property is to be used to indicate whether a user has 'read' the notification.

        [Column(TypeName = "bit")]
        public bool IsRead { get; set; } = false;

        public virtual QbicleDomainRequest AssociatedDomainRequest { get; set; }

        public virtual DomainExtensionRequest AssociatedExtensionRequest { get; set; }

        //The TradeOrder that cause the notification (it it was caused by a TradeOrder)
        public virtual TradeOrder AssociatedTradeOrder { get; set; }

        public bool IsCreatorTheCustomer { get; set; } = false;
        /// <summary>
        /// Show alert
        /// </summary>
        public bool IsAlertDisplay { get; set; }

        public virtual WaitListRequest AssociatedWaitList { get; set; }
    }
}