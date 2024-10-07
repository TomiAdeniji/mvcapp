using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_EmailLog")]
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime SendDate { get; set; }

        [Required]
        [StringLength(256)]
        public string SentTo { get; set; }

        [Required]
        [StringLength(500)]
        public string Subject { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string EmailBody { get; set; }

        public ReasonSent ReasonSentId { get; set; }

        public virtual CleanBooksData.reasonsentemail reasonsentemail { get; set; }
        /// <summary>
        /// Enum to indicate whether in a Reason Sent Email
        /// </summary>
        public enum ReasonSent
        {
            [Description("Confirm your account")]
            UserCreation = 1,
            [Description("Reset your password")]
            ForgotPassword = 2,
            [Description("Qbicle Creation")]
            QbicleCreation = 3,
            [Description("Qbicle Update")]
            QbicleUpdate = 4,
            [Description("Discussion Creation")]
            DiscussionCreation = 5,
            [Description("Discussion Update")]
            DiscussionUpdate = 6, //This includes when any Post, Task, Event, Alert or File is added to a discussion
            [Description("Task Creation")]
            TaskCreation = 7,
            [Description("Task Completion")]
            TaskCompletion = 8,
            [Description("Alert Creation")]
            AlertCreation = 9,
            [Description("Event Creation")]
            EventCreation = 10,
            [Description("Event Withdrawal")]
            EventWithdrawl = 11, // When a user indicates that they are not going to attend an event
            [Description("Invited guest")]
            InvitedGuest = 12,
            [Description("Post Creation")]
            PostCreation = 13,
            [Description("Approval Creation")]
            ApprovalCreation = 14,
            [Description("Create Member")]
            CreateMember = 15,
            [Description("Invited Member")]
            InvitedMember = 16,
            [Description("Alert Update")]
            AlertUpdate = 17,
            [Description("Approval Update")]
            ApprovalUpdate = 18,
            [Description("Task Update")]
            TaskUpdate = 19,
            [Description("Event update")]
            EventUpdate = 20,
            [Description("Topic Post")]
            TopicPost = 21,
            [Description("Approval Reviewed")]
            ApprovalReviewed = 22,
            [Description("Approval Approved")]
            ApprovalApprove = 23,
            [Description("Approval Denied")]
            ApprovalDenied = 24,
            [Description("Media Creation")]
            MediaCreation = 25,
            [Description("Media Update")]
            MediaUpdate = 26,
            [Description("Approval Approved")]
            ApprovalApproved = 27,
            [Description("Journal Post")]
            JournalPost = 28,
            [Description("Transaction Post")]
            TransactionPost = 29,
            [Description("Invoice Issue")]
            InvoiceIssue = 30,
            [Description("Link Creation")]
            LinkCreation = 31,
            [Description("Remove user out of Domain")]
            RemoveUserOutOfDomain = 32,
            [Description("Reminder campaign post")]
            ReminderCampaignPost = 33,
            [Description("Email campaign post")]
            EmailCampaignPost = 34,
            [Description("Assign task to User")]
            AssignTask = 35,
            [Description("C2C Connection Issued")]
            CreateC2CConnectionRequest = 36,
            [Description("C2C Connection Accepted")]
            AcceptC2CConnectionRequest = 37,
            [Description("B2C Connection Created")]
            CreateB2CConnection = 38,
            [Description("Listing Post Flagged")]
            FlagListingPost = 39,
            [Description("Link Update")]
            LinkUpdate = 40,
            [Description("Comment on Activity")]
            ActivityComment = 41,
            [Description("Remove user out of Qbicle")]
            RemoveUserOutOfQbicle = 42,
            [Description("Add User to Discussion Participants")]
            AddUserParticipants = 43,
            [Description("Remove User to Discussion Participants")]
            RemoveUserParticipants = 44,
            [Description("Qbicles invited")]
            QbicleInvited = 45,
            [Description("Remove Queue")]
            RemoveQueue = 46,
            [Description("Add User To Qbicle")]
            AddUserToQbicle = 47,
            [Description("Generate new StoreCredit PIN")]
            GenerateNewStoreCreditPIN = 48,
            [Description("Approve or Reject Domain Request")]
            ProcessDomainRequest = 49,
            [Description("Send email verification")]
            EmailVerification = 50,
            [Description("Send email share highlight post")]
            EmailHLPostSharing = 51,
            [Description("Post edit")]
            PostEdit = 52,
            MediaRemoveVersion=53,
            MediaAddVersion = 54,
            [Description("Event Notification Points")]
            EventNotificationPoints = 55,
            [Description("Task Notification Points")]
            TaskNotificationPoints = 56,
            [Description("Begun work on Task")]
            TaskStart = 57,
            [Description("Completed task")]
            TaskComplete = 58,
            [Description("Join to waitlist request")]
            WaitlistJoin = 59,
            [Description("Waitlist approval")]
            WaitlistApproval = 60,
            [Description("Waitlist reject")]
            WaitlistReject = 61
        };
    }
}