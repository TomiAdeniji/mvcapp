using CleanBooksData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Form;
using Qbicles.Models.Highlight;
using Qbicles.Models.Invitation;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.MyBankMate;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Spannered;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.WaitList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Enums;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleDomain;
using static Qbicles.Models.QbicleTask;

namespace Qbicles.BusinessRules
{
    /// <summary>
    ///     Return json model result from Controller to View
    /// </summary>
    public class ReturnJsonModel
    {
        public string msg { get; set; }
        public string msgId { get; set; }
        public string msgName { get; set; }

        /// <summary>
        ///     1- Add new, 2- Edit, 3- Delete, 4- error
        /// </summary>
        public int actionVal { get; set; }

        /// <summary>
        ///     object value
        /// </summary>
        public object Object { get; set; }

        /// <summary>
        ///     result True/False
        /// </summary>
        public bool result { get; set; } = true;

        public object Object2 { get; set; }
    }

    public class QbicleDateGroup
    {
        public DateTime QbicleDate { get; set; }
    }

    public class PosPaymentCustom
    {
        public string CreatedDate { set; get; }
        public string LocationName { set; get; }
        public string RefFull { set; get; }
        public string Method { set; get; }
        public string AccountName { set; get; }
        public string Cashier { set; get; }
        public string PosDevice { set; get; }
        public string Amount { set; get; }
    }

    public class PosPaymentReport
    {
        public string DateTime { set; get; }
        public string Location { set; get; }
        public string Reference { set; get; }
        public string PaymentMethod { set; get; }
        public string Account { set; get; }
        public string Cashier { set; get; }
        public string PosDevice { set; get; }
        public decimal Amount { set; get; }
    }

    public class PosOrderTypeCustom
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string Summary { set; get; }
        public string Classification { set; get; }
        public bool IsUse { set; get; }
    }

    public class PosDeviceTypeCustom
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string OrderTypes { set; get; }
        public bool IsUse { set; get; }
    }

    public class PosPaymentMenuSources
    {
        public List<TraderLocation> TraderLocations { set; get; } = new List<TraderLocation>();
        public List<TraderCashAccount> TraderCashAccounts { set; get; } = new List<TraderCashAccount>();
        public List<PaymentMethod> Methods { set; get; } = new List<PaymentMethod>();
        public List<ApplicationUser> Cashiers { set; get; } = new List<ApplicationUser>();
        public List<PosDevice> PosDevices { set; get; } = new List<PosDevice>();
    }

    public class CubeModel
    {
        public string QbicleKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DomainId { get; set; }
        public string LogoUri { get; set; }
        public string CubeUser { get; set; }
        public string CubeGuest { get; set; }
        public DateTime? Closed { get; set; }
        public string CubeApprovers { get; set; }
        public bool IsUsingApprovals { get; set; }
        public string OwnedBy { get; set; }
        public List<UserListModel> QbicManager { get; set; }

        /// <summary>
        /// The current User is member of qbicle or not
        /// </summary>
        public bool IsMemberQbicle { get; set; }

        public string Manager { get; set; }
    }

    public class NotificationbarMenu
    {
        public int OrderNo { get; set; }
        public QbicleActivity.ActivityTypeEnum ActivityType { get; set; }
        public int NotificationCount { get; set; }
    }

    public class CreateAccountMain
    {
        public string Id { get; set; }
        public string AccountName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string Password_repeat { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Company { get; set; }
        public int AccountPakgeId { get; set; }
        public RegistrationType RegistrationType { get; set; }

        /// <summary>
        /// Is user id encrypted will be connect to new cuser
        /// </summary>
        public string ConnectCode { get; set; }
    }

    public class FormDefinitionCustom
    {
        [Required] public int Id { get; set; }

        [Required] public string Name { get; set; }

        [Required] public string Description { get; set; }

        [AllowHtml][Required] public string Definition { get; set; }
    }

    public class OurPeopleModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string TypeUseFill { get; set; }
        public string TypeUser { get; set; } //- Domain user.guest
        public int TypeUserId { get; set; } //- Domain user.guest
        public string Image { get; set; }
        public Uri ImageUri { get; set; }
        public int QbiclesCount { get; set; }
        public int TasksCount { get; set; }
        public int TasksCompleteCount { get; set; }
        public string Forename { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> lstRole { get; set; }
    }

    public class ExecuteQueryModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ExecuteQueryResultModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormBuilder { get; set; }
        public string StartedBy_Id { get; set; }
    }

    public class TaskFormDefinitionRefCustom
    {
        [Required] public int Id { get; set; }

        [Required] public string FormData { get; set; }

        [AllowHtml][Required] public string FormBuilder { get; set; }
    }

    public class NotificationBroadcast
    {
        public int Id { get; set; }

        //The date on which the notification was created
        [Required] public DateTime CreatedDate { get; set; }

        //The user who was logged in when the notification was created
        [Required] public virtual ApplicationUser CreatedBy { get; set; }

        //The date on which the notification was sent
        public DateTime SentDate { get; set; }

        //The method by which the notification was sent
        public NotificationSendMethodEnum SentMethod { get; set; }

        //The Activity that caused the Notification (if it was caused by an Activity)
        public virtual QbicleActivity AssociatedAcitvity { get; set; }

        //The Qbicle that cause the notification (it it was caused by a Qbicle)
        public virtual Qbicle AssociatedQbicle { get; set; }

        public virtual QbicleDomain AssociatedDomain { get; set; }

        //The event that caused the Notification
        [Required] public NotificationEventEnum Event { get; set; }

        //The user who is to be notified
        [Required] public virtual ApplicationUser NotifiedUser { get; set; }

        //If an email is used for notification, this is the link to the email that has been sent
        public EmailLog EmailSent { get; set; }

        // This property is to be used to indicate whether a user has 'read' the notification.
        public bool IsRead { get; set; } = false;
    }

    public class AdministratorViewModal
    {
        public static string AccountOwner = "Account Owner";
        public static string AccountAdministrators = "Account Administrators";
        public static string DomainAdministrator = "Domain Administrator";
        public string Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public string Domain { get; set; }
        public string Avatar { get; set; }

        public List<KeyValuePair<string, int>> Levels { get; set; } = new List<KeyValuePair<string, int>>();
        public List<string> Domains { get; set; } = new List<string>();
    }

    public class ProcessStepModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string StepImage { get; set; }

        public int StepOrder { get; set; }

        public string Suffix { get; set; }

        public Guid StepGuidId { get; set; } = Guid.NewGuid();
    }

    public class ProcessDocumentModel
    {
        public string Document { get; set; }

        public string DocumentImage { get; set; }

        public string FileTypeId { get; set; }

        public string FileTypeImage { get; set; }

        public Guid DocumentGuidId { get; set; } = Guid.NewGuid();
    }

    public class ApprovalAppModel
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string ProcessImage { get; set; }
        public ApprovalRequestDefinition.RequestTypeEnum Type { get; set; }
        public string TypeName { get; set; }
        public string Initiate { get; set; }
        public string InitiateName { get; set; }
        public string Reviewer { get; set; }
        public string ReviewerName { get; set; }
        public string Approval { get; set; }
        public string ApprovalName { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public string DocumentHtml { get; set; }

        //public List<KeyValuePair<string, int>> Forms { get; set; }
    }

    public class ApprovalGroupAppsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ApprovalRequestDefinition> Approvals { get; set; }
    }

    public class AttachmentModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int ActivityId { get; set; } = 0;
        public string Name { get; set; }
        public string AttachmentUrl { get; set; }
        public string IconFile { get; set; }
        public bool CanDelete { get; set; } = false;
        public int FileSize { get; set; }
    }

    /// <summary>
    ///     List of reviewer and approver of the Approval application
    /// </summary>
    public class ApprovalUsersAssociatedModel
    {
        public IEnumerable<ApplicationUser> Reviewers { get; set; } = new List<ApplicationUser>();
        public IEnumerable<ApplicationUser> Approvers { get; set; } = new List<ApplicationUser>();
        public IEnumerable<ApplicationUser> Initiators { get; set; } = new List<ApplicationUser>();
    }

    /// <summary>
    ///     get permission review or approval of the user
    /// </summary>
    public class IsReviewerAndApproverModel
    {
        public bool IsInitiators { get; set; }
        public bool IsReviewer { get; set; }
        public bool IsApprover { get; set; }
    }

    public class TaskFormParameter
    {
        public int formDefinitionId { get; set; }
        public int domainId { get; set; }
        public string formQuery { get; set; }
        public string arrDateFields { get; set; }
    }

    public class TaskParameter
    {
        public int qbicleId { get; set; }
        public string taskName { get; set; }
        public TaskPriorityEnum priority { get; set; }
        public TaskRepeatEnum recurring { get; set; }
        public string createdBy { get; set; }
        public string asssignTo { get; set; }
        public string createdDateStart { get; set; }
        public string createdDateEnd { get; set; }
        public string deadlineDateStart { get; set; }
        public string deadlineDateEnd { get; set; }
        public string description { get; set; }
    }

    public class TaskFormDefinitionRefAndFormDefinition
    {
        public List<FormDefinition> FormDefinitions { get; set; }
        public List<TaskFormDefinitionRef> TaskFormDefinitions { get; set; }
    }

    public class VersionedFileDisplay
    {
        public int Id { get; set; }
        public string Uri { get; set; }
        public string FileSize { get; set; }
        public string UploadedDate { get; set; }
    }

    public class CBAccountResult
    {
        public bool existsUpload { get; set; }
        public string result { get; set; }
        public decimal lastbalance { get; set; }
    }

    public class uploadfieldsData
    {
        public string Value { get; set; }
    }

    public class AnalysisModel
    {
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Overdue { get; set; }
        public string Interval { get; set; }
        public string dateexcuted { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public long AccountId2 { get; set; }
        public string AccountName2 { get; set; }
        public int InstanceActive { get; set; }
        public int TransactionMatchingTypeId { get; set; }
        public int IsComplete { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateByUser { get; set; }
        public string AssignedUserId { get; set; }
        public DateTime? DateExcuted { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public int TaskInstanceId { get; set; }
    }

    public class transactionsMatchingModel
    {
        public long transactionId { get; set; }
        public int transactionMatchingRecordId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Balance { get; set; }
        public string Reference1 { get; set; }
        public string DescCol1 { get; set; }
        public string DescCol2 { get; set; }
        public string DescCol3 { get; set; }
        public int transactionGroupId { get; set; }
        public int transactionMethodId { get; set; }
        public string transactionMethodName { get; set; }
        public bool IsAccountA { get; set; }
        public decimal DayNumber { get; set; }
    }

    public class MsgModel
    {
        public string msg { get; set; }
        public string msgId { get; set; }
        public object Object { get; set; }
        public bool result { get; set; }
    }

    public class FinishRemainingAlerts
    {
        public string AccountName { get; set; }
        public DateTime Date { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public DateTime DateExcute { get; set; }
    }

    public class TasksModel
    {
        public task task { get; set; }
        public taskinstance taskInstance { get; set; }
        public ApplicationUser userCreate { get; set; }
        public tasktype tasktype { get; set; }
    }

    public class ModelTask
    {
        public task task { get; set; }
        public bool taskinstance { get; set; }
        public List<taskaccount> taskaccount { get; set; }
        public bool isInPresson { get; set; }
        public int WorkGroupId { get; set; }
        public string Deadline { get; set; }
        public TaskPriorityEnum Priority { get; set; }
    }

    public class QbicleStepInstanceModel
    {
        public int Id { get; set; }

        public int ActivityId { get; set; }

        public int StepId { get; set; }

        public bool Complete { get; set; }
        public short Percent { get; set; }
    }

    public static class RolePermissions
    {
        public const string Administrator = "Administrator";
        public const string DataManager = "Data Manager";
        public const string AccountManager = "Account Manager";
        public const string TaskManager = "Task Manager";
        public const string TaskRunnerManage = "Task Runner";
        public const string FinancialControl = "Financial Control";
        public const string ReconciliationAlerts = "Reconciliation Alerts";
        public const string TransactionMatchingReport = "Transaction Matching Report";
        public const string SystemAdmin = "System Admin";
    }

    public class BalanceanalysisModel
    {
        public string Description1 { get; set; }
        public string Reference1 { get; set; }
        public string Description2 { get; set; }
        public string Reference2 { get; set; }
        public decimal Amount1 { get; set; }
        public decimal Amount2 { get; set; }
        public decimal Difference { get; set; }
        public int WarningLevel { get; set; }
        public string WarningLevelName { get; set; }
        public int Id { get; set; }
        public int BalanceAnalysisActionId { get; set; }
        public int balanceanalysismappingruleId { get; set; }
    }

    public class UserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    public class UserListModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string ProfilePic { get; set; }
    }

    public class MatchingUnmatch
    {
        public TableUnmatch table { get; set; } = new TableUnmatch();
        public SumUnmatch sum { get; set; } = new SumUnmatch();
    }

    public class TableUnmatch
    {
        public string table_creditA { get; set; }
        public string table_debitB { get; set; }
        public string table_debitA { get; set; }
        public string table_creditB { get; set; }
    }

    public class SumUnmatch
    {
        public string Sum_CreditsA { get; set; }
        public string Sum_DebitA { get; set; }
        public string Sum_CreditsB { get; set; }
        public string Sum_DebitB { get; set; }
    }

    public class DateRange
    {
        public int id { get; set; }
        public string start { get; set; }
        public string end { get; set; }
    }

    public class transactionmatchingcopiedrecordsModel
    {
        public int TransactionMatchingRecordId { get; set; }
        public int CopiedFromId { get; set; }
        public long TransactionId { get; set; }
    }

    public class transactionsReportModel
    {
        public string ProfileName { get; set; }
        public string ProfileTag { get; set; }
        public string ProfileValue { get; set; }
        public long TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Balance { get; set; }
        public string Reference1 { get; set; }
        public string DescCol1 { get; set; }
        public string DescCol2 { get; set; }
        public string DescCol3 { get; set; }
    }

    public static class transactionParameter
    {
        public static int AccountId { get; set; }
        public static int AccountId2 { get; set; }
        public static DateTime StartDate { get; set; }
        public static DateTime EndDate { get; set; }
        public static List<transaction> transactionsProceed { get; set; }
        public static List<transactionsReportModel> transactionsApplyProfile { get; set; }
    }

    public class TransactionMatchingReportParameter
    {
        public string accountName { get; set; }
        public long accountId { get; set; }
        public string accountName2 { get; set; }
        public long accountId2 { get; set; }
        public int taskid { get; set; }

        public string taskKey
        { get { return taskid.Encrypt(); } }

        public string taskname { get; set; }
        public int transactionMatchingTypeId { get; set; }
        public int transactionmatchingTaskId { get; set; }
        public int taskInstanceId { get; set; }
        public string userName { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public int countTransactionsMatched { get; set; }
        public int countTransactionsManual { get; set; }
        public int countTransactionsUnMatched { get; set; }
    }

    public class AccountTransactionsReport
    {
        public string Date { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string Balance { get; set; }
    }

    public class AccountAmount
    {
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Balance { get; set; }
    }

    public class MediaModel
    {
        public string Id { get; set; } = new Guid().ToString();
        public string Name { get; set; }
        public QbicleFileType Type { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
        public string UrlGuid { get; set; }
        public string Extension { get; set; }
        public bool IsPublic { get; set; }
    }

    public class BKAccountGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<BKAccountModel> Accounts { get; set; } = new List<BKAccountModel>();
    }

    public class BKAccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class JournalEntryTemplateItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class RepositoryItem
    {
        public string RootDir { get; set; }
        public RepositoryItemType Type { get; set; }
        public int ContentLength { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
        public string Mime { get; set; }
        public string ImageBase64 { get; set; }
        public bool IsPublic { get; set; }
    }

    public class FileModel
    {
        //[AllowHtml]
        public string Uri { get; set; }

        //[AllowHtml]
        public string Name { get; set; }
    }

    public class TagModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EditedDate { get; set; }
        public string Keywords { get; set; }
    }

    public class QbicleSearchParameter
    {
        /// <summary>
        /// required from Micro
        /// </summary>
        public int DomainId { get; set; }

        /// <summary>
        /// Get by token from Micro
        /// </summary>
        public string UserId { get; set; }

        public QbicleOrder Order { get; set; } = 0;
        public bool Open { get; set; } = false;
        public bool Closed { get; set; } = false;
        public string Name { get; set; } = "";
        public string[] Peoples { get; set; }
        public string[] Topics { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsShowHidden { get; set; }
    }

    public class BulkDealSearchParameter
    {
        public string Name { get; set; } = "";
        public int Type { get; set; }
        public string DateRange { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string Timezone { get; set; }
    }

    public enum QbicleOrder
    {
        NameAsc = 1,
        NameDesc = 2,
        DateAsc = 3,
        DateDesc = 4
    }

    public class ItemTopic
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ModelPage
    {
        public int Id { set; get; }
        public Qbicles.Models.Community.CommunityPageTypeEnum PageType { set; get; }
        public DateTime CreatedDate { set; get; }
        public int Followers { set; get; }
        public string BodyText { set; get; }
        public string Title { get; set; }
        public string StrapLine { get; set; }
        public string DomainName { get; set; }
        public virtual QbicleDomain Domain { get; set; }
        public string FeaturedImage { get; set; }
        public string FeaturedImageCaption { get; set; }
        public string StoredLogoName { get; set; }
        public string StoredFeaturedImageName { get; set; }
        public virtual ApplicationUser AssociatedUser { get; set; }
        public virtual List<Qbicles.Models.Community.Tag> Tags { get; set; } = new List<Qbicles.Models.Community.Tag>();
    }

    public class BaseItemModel
    {
        public int Id { set; get; }
        public string Key { set; get; }
        public string Name { set; get; }
        public string CreatedBy { set; get; }
        public string CreatedDate { set; get; }
    }

    public class UnitOfMeasureModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayAbbreviation { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string MeasurementType { get; set; }
        public string CanDelete { get; set; }
    }

    public class TaxRateModel
    {
        public int Id { get; set; }
        public decimal Rate { get; set; }
        public string Name { get; set; }
        public bool IsAccounted { get; set; }
        public bool IsPurchaseTax { get; set; }
        public bool IsCreditToTaxAccount { get; set; }
        public string Description { get; set; }
        public string CanDelete { get; set; }
    }

    public class EmploymentModel
    {
        public int Id { get; set; }

        public string Employer { get; set; }

        public string Role { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }
        public string Summary { get; set; }
        public ApplicationUser User { get; set; }
        public string CurrentTimeZone { get; set; }
    }

    public class EmploymentHistoryModel
    {
        public int Id { get; set; }
        public string Employer { get; set; }
        public string Role { get; set; }
        public string Dates { get; set; }
        public string Summary { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MyfileModal
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Added { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    public class MyfileUploadModal
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ApplicationUser User { get; set; }
        public MediaModel media { get; set; }
    }

    public class UnitModel
    {
        public int Id { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public decimal QuantityOfBaseunit { get; set; }
        public decimal Quantity { get; set; }
        public string Selected { get; set; } = "";
        public bool IsBase { set; get; } = false;

        public decimal BaseUnitCost { set; get; } = 0M;
    }

    public class WorkgroupUser
    {
        public List<WorkgroupMember> Members { get; set; }
        public List<WorkgroupMemberId> Approvers { get; set; }
        public List<WorkgroupMemberId> Reviewers { get; set; }
    }

    public class WorkgroupMember
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
    }

    public class WorkgroupMemberId
    {
        public string Id { get; set; }
    }

    public class SalePurchasePaymentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SalePurchasePaymentEnum Type { get; set; }
    }

    public enum SalePurchasePaymentEnum
    {
        Sale = 1,
        Purchase = 2,
        Payment = 3
    }

    public class TraderSetupInit
    {
        public string LocationClass { get; set; }
        public bool LocationCompleted { get; set; }
        public string ProductGroupClass { get; set; }
        public bool ProductGroupCompleted { get; set; }
        public string ContactGroupClass { get; set; }
        public bool ContactGroupCompleted { get; set; }
        public string WorkgroupClass { get; set; }
        public bool WorkgroupCompleted { get; set; }
        public string AccountingClass { get; set; }
        public bool AccountingCompleted { get; set; }
        public string SetupCompleteClass { get; set; }
        public bool SetupCompleted { get; set; }
    }

    public class SMSetupInit
    {
        public string ContactsClass { get; set; }
        public bool ContactsCompleted { get; set; }
        public string TraderContactsClass { get; set; }
        public bool TraderContactsCompleted { get; set; }
        public string QbileClass { get; set; }
        public bool QbileCompleted { get; set; }
        public string WorkgroupClass { get; set; }
        public bool WorkgroupCompleted { get; set; }
        public string SetupCompleteClass { get; set; }
        public bool SetupCompleted { get; set; }
    }

    public enum IssueType
    {
        Invoice = 1,
        SaleOrder = 2,
        PurchaseOrder = 3
    }

    public class ApprovalStatusTimeline
    {
        public DateTime LogDate { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string Icon { get; set; }
        public string UserAvatar { get; set; }
    }

    public class MarkupDiscountModel
    {
        public int Id { get; set; }
        public string MarkupPercentage { get; set; }
        public decimal MarkUp { get; set; }
        public string DiscountPercentage { get; set; }
        public decimal Discount { get; set; }
        public MarkupDiscountApply ApplyType { get; set; }
    }

    public enum MarkupDiscountApply
    {
        Apply = 1,
        ApplyOverwrite = 2
    }

    public enum RecalculatePricesType
    {
        AverageCost = 1,
        LatestCost = 2
    }

    public class InvoiceContact
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Date_Sort { get; set; }
        public string Type { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public string StatusCss { get; set; }
        public string Ref { set; get; }
        public string AmountPaid { set; get; }
        public string BalanceInvoice { set; get; }
    }

    public class InvoiceCustome
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Ref { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Amount { get; set; }
        public TraderInvoiceStatusEnum Status { get; set; }
        public string Type { get; set; }
    }

    public class CostsFromInventory
    {
        public decimal AverageCost { get; set; }
        public decimal LastCost { get; set; }
    }

    public class CustomDateModel
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public bool selected { get; set; }
        public DateTime StartDate { get; set; }
        public string Date { get; set; }
    }

    public class MyTagCustom
    {
        public int Id { get; set; }
        public int Desk_Id { get; set; }
        public string Name { get; set; }
        public int Activity_Id { get; set; }
        public bool IsPost { get; set; }
    }

    public class UserCustom
    {
        public string Id { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string DisplayUserName { get; set; }
        public string UserName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string ProfilePic { get; set; }
        public string Expression { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Domains { get; set; }
        public List<DomainRoleModel> SystemRoles { get; set; }
        public string Domain { get; set; }
        public DateTime DateBecomesMember { get; set; }
        public string Profile { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class SearchActivityCustom
    {
        public string SearchName { get; set; }
        public int order { get; set; }
        public string dateRange { get; set; }
        public int domainId { get; set; }
        public int qbcileId { get; set; }
        public List<int> tags { get; set; }
        public int status { get; set; }
        public List<string> UserId { get; set; }
        public int actType { get; set; }
        public int isHide { get; set; }
    }

    public class DotDateCustom
    {
        public DateTime? ProgrammedEnd { get; set; }
        public bool IsRedDot { get; set; }
    }

    public class TopicCustom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public DateTime? CreatedDate { get; set; }
        public ApplicationUser Creator { get; set; }
        public int Instances { get; set; }
        public bool isTrader { get; set; }
        public IEnumerable<QbicleActivity> Activities { get; set; }
    }

    public class RelatedActivity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class TopValueSale
    {
        public int GroupId { get; set; }
        public string TraderItemIds { set; get; }
        public string GroupName { get; set; }

        [JsonIgnore]
        public decimal PercentInt { set; get; } = 0;

        public string Percent { get; set; }

        [JsonIgnore]
        public decimal ValueInt { set; get; } = 0;

        public string Value { get; set; }
    }

    public class TraerItemByGroup
    {
        public TraderItem TraderItem { get; set; } = new TraderItem();
        public string Quantity { get; set; }
        public string Price { get; set; }
        public string Cost { get; set; }
        public string Margin { get; set; }
    }

    public class TraderSaleDashBoard
    {
        public string TotalSaleValue { get; set; }
        public string TotalApproved { get; set; }
        public List<TopValueSale> TopSells { get; set; } = new List<TopValueSale>();
        public List<TopValueSale> TopMargin { get; set; } = new List<TopValueSale>();
        public List<TopValueSale> TopGrossMargion { get; set; } = new List<TopValueSale>();
    }

    public class TraderSaleCustom
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string FullRef { get; set; }
        public string WorkgroupName { get; set; }
        public string CreatedDate { get; set; }
        public string SalesChannel { get; set; }
        public string Contact { get; set; }
        public string Dimensions { get; set; }
        public string SaleTotal { get; set; }
        public string Status { get; set; }
        public string LabelStatus { get; set; }
        public bool AllowEdit { get; set; }
        public int TransferCount { get; set; }
        public string SaleOrderId { get; set; }
        public string SaleOderRef { get; set; }
        public string ApprovedOn { get; set; }
    }

    public class TraderSaleReport
    {
        public string Id { get; set; }
        public string Workgroup { get; set; }
        public string Created { get; set; }
        public string Channel { get; set; }
        public string Contact { get; set; }
        public string ReportingFilters { get; set; }
        public decimal Cost { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }

    public class InventoryBatchCustom
    {
        public int Id { get; set; }
        public string ImageUri { get; set; }
        public string ItemName { get; set; }
        public string In { get; set; }
        public string Out { get; set; }
        public string Difference { get; set; }
        public int BaseUnitId { set; get; }

        //Added
        public string ItemType { get; set; }

        public string ProductGroup { get; set; }
        public string SKU { get; set; }
        public string Unit { get; set; }
        public string Cost { get; set; }
        public string PoolPrice { set; get; }
        public string QuantitySold { set; get; }
        public string PurchaseQuantity { set; get; }
        public string TransferInQuantity { set; get; }
        public string TransferOutQuantity { set; get; }
        public string ManufacturedQuantity { set; get; }
        public string GeneratedInventory { set; get; }
        public string SpotCountQuantity { set; get; }
        public string WasteQuantity { set; get; }
        public string OnHandQuantity { set; get; }
    }

    public class InventoryBatchReport
    {
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public string ProductGroup { get; set; }
        public string SKU { get; set; }
        public string Unit { get; set; }
        public decimal Cost { get; set; }
        public decimal PoolPrice { set; get; }
        public decimal QuantitySold { set; get; }
        public string PurchaseQuantity { set; get; }
        public string TransferInQuantity { set; get; }
        public string TransferOutQuantity { set; get; }
        public string ManufacturedQuantity { set; get; }
        public string GeneratedInventory { set; get; }
        public string SpotCountQuantity { set; get; }
        public string WasteQuantity { set; get; }
        public string OnHandQuantity { set; get; }
    }

    public class OrderQueueCustom
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Customer { get; set; }
        public decimal CustomerLongitude { get; set; }
        public decimal CustomerLatitude { get; set; }
        public string OrderRef { get; set; }
        public int OrderItems { get; set; } = 0;
        public string ItemsInfo { get; set; }
        public string OrderTotal { get; set; }
        public string Queued { get; set; }
        public string Pending { get; set; }
        public string Preparing { get; set; }
        public string Completion { get; set; }
        public string DeliveryStatus { get; set; }
        public string Payment { get; set; }
        public bool Discussion { set; get; } = false;
        public string Status { get; set; }
        public string StatusLabel { get; set; }
    }

    public class VirtualSafeTransactionReport
    {
        public string DateTime { get; set; }
        public string TillAccount { get; set; }
        public string SafeInOut { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal Difference { get; set; }
        public string Status { get; set; }
    }

    public class VirtualTillTransactionReport
    {
        [Display(Name = "Date & Time", Description = "Date & Time")]
        public string DateTime { get; set; }

        public string Device { get; set; }
        public string Till { get; set; }
        public string Safe { get; set; }

        [Display(Name = "Till in/out", Description = "Till in/out")]
        public string TillInOut { get; set; }

        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal Difference { get; set; }
        public string Status { get; set; }
    }

    public class ItemOverview
    {
        public int Id { get; set; }
        public string ImageUri { get; set; }
        public string ItemName { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public bool IsBought { get; set; }
        public bool IsSold { get; set; }
        public bool IsCompoundProduct { get; set; }
        public string GroupName { get; set; }
        public string Unit { get; set; }
        public decimal Cost { get; set; }
        public decimal SellingPrice { get; set; }
        public int Inventory { get; set; }
        public string Description { get; set; }
        public string Vendor { get; set; }
        public bool IsActive { get; set; }
    }

    public class ItemOverviewReport
    {
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string ProductGroup { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string UnitofMeasurement { get; set; }
        public decimal Cost { get; set; }
        public decimal SellingPrice { get; set; }
        public int Inventory { get; set; }
        public string Active { get; set; }
    }

    public class TransferCustom
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string FullRef { get; set; }
        public string WorkGroupName { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Route { get; set; }
        public string Date { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public bool AllowEdit { get; set; }
        public object Sale { get; set; }
        public object Purchase { get; set; }
    }

    public class CashBankCustom
    {
        public int Id { set; get; }
        public string Name { get; set; }
        public string BookkeepingAccount { get; set; }
        public string Image { get; set; }
        public bool IsAssociatedBKAcc { get; set; }

        /// <summary>
        /// AssociatedBKAccount.Number - AssociatedBKAccount.Code - AssociatedBKAccount.Name
        /// </summary>
        public string AssociatedBKAccount { get; set; }

        /// <summary>
        /// destinationsIn+ originatingsIn
        /// </summary>
        public string FundsIn { get; set; }

        /// <summary>
        /// destinationsOut+ originatingsOut
        /// </summary>
        public string FundsOut { get; set; }

        public string Charges { get; set; }
        public string Transactions { get; set; }
        public bool AllowEdit { get; set; }
        public BankmateAccountType BankmateType { get; set; }
    }

    public class PurchaseCustom
    {
        public int Id { get; set; }
        public string FullRef { get; set; }
        public string WorkGroupName { get; set; }
        public string CreatedDate { get; set; }
        public string Contact { get; set; }
        public string ReportingFilter { get; set; }
        public string Total { set; get; }
        public string Status { get; set; }
        public bool AllowEdit { get; set; }
    }

    public class PurchaseOrderReport
    {
        public string Id { get; set; }
        public string WorkGroup { get; set; }
        public string CreatedDate { get; set; }
        public string Contact { get; set; }
        public string ReportingFilter { get; set; }
        public decimal Total { set; get; }
        public string Status { get; set; }
    }

    public class InvitationCustom
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string ProfilePic { get; set; }
        public string Note { get; set; }
        public string UserId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public InvitationStatusEnum Status { get; set; }

        public int DomainId { get; set; }
        public string DomainName { get; set; }
        public string DomainPic { get; set; }
        public string InviteBy { get; set; }
    }

    public class MyTagsCustom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedDate { get; set; }
        public string Creator { get; set; }
        public int Instances { get; set; }
    }

    public class Select2Model
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool Selected { get; set; }
    }

    public class WorkgroupProposal
    {
        public List<Select2Model> Sales { get; set; }
        public List<Select2Model> Invoices { get; set; }
        public List<Select2Model> Payments { get; set; }
        public List<Select2Model> Transfers { get; set; }
        public List<Select2Model> CashBankAccounts { get; set; }
    }

    public class Select2CustomModel
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class SelectOption
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class Select2GroupedCustomModel
    {
        public string text { get; set; }
        public List<Select2CustomModel> children { get; set; }
    }

    public class PosUserModel
    {
        public int Id { get; set; }
        public int PosUserId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string ForenameGroup { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int? Pin { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string JobTitle { get; set; }
        public string ProfileSummary { get; set; }
        public List<PosUserType> Types { get; set; }
    }

    public class PosUserDeviceModel
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public int PosUserId { get; set; }
        public string UserId { get; set; }
        public string CurrentRole { get; set; }
        public string PreviousRole { get; set; }
        public bool IsDelete { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int? Pin { get; set; }
    }

    public class PooledUserModel
    {
        public string UserId { get; set; }
        public List<PosUserType> Pools { get; set; }
    }

    public class PosRoleUsersViewModel
    {
        public int Id { get; set; }

        [Required]
        public ApplicationUser User { get; set; }

        [Range(1000, 9999)]
        public int? Pin { get; set; }

        public PosUserType PosUserType { get; set; } = PosUserType.None;

        [Required]
        public ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public QbicleDomain Domain { get; set; }

        public List<int> DeviceIds { get; set; } = new List<int>();
        public List<int> DdsDeviceIds { get; set; } = new List<int>();
        public List<int> PrepDisplayDeviceIds { get; set; } = new List<int>();
    }

    public class CreatePosUserViewModel
    {
        public int Id { get; set; }
        public ApplicationUser User { get; set; }
        public QbicleDomain Domain { get; set; }
        public string UserType { get; set; }
    }

    public class TraderItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public List<int> WgIds { set; get; }
        public string TaxRateName { get; set; }
        public decimal TaxRateValue { get; set; }
        public decimal CostUnit { get; set; }
    }

    public class DataTableModel
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }

        //public object customResponse { get;set; }
        public object data { get; set; }
    }

    public class NotificationModel
    {
        public int Id { get; set; }
        public bool IsRead { get; set; }
        public NotificationEventEnum Event { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public QbicleDomain AssociatedDomain { get; set; }
        public Qbicle AssociatedQbicle { get; set; }
        public QbicleActivity Activity { get; set; }
        public ApplicationUser AssociatedUser { get; set; }
        public QbiclePost AssociatedPost { get; set; }
        public Invitation AssociateInvitation { get; set; }
        public HighlightPost AssociatedHighlight { get; set; }
        public string DomainRequested { get; set; }
        public DomainExtensionRequest ExtensionRequest { get; set; }
        public TradeOrder AssociatedTradeOrder { get; set; }
        public WaitListRequest AssociatedWaitList { get; set; }
        public bool IsCreatorTheCustomer { get; set; }
    }

    public class SignalRParameter
    {
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// connection id, using for case a user log in to multi app
        /// for check on signalR if current connection != originaling creation id then reload
        /// </summary>
        public string OriginatingCreationId { get; set; } = string.Empty;

        public string ObjectById { get; set; }
        public Qbicle Qbicle { get; set; }
        public NotificationEventEnum EventNotify { get; set; }
        public string CreatedById { get; set; }
        public ApplicationPageName AppendToPageName { get; set; }
        public int CurrentQbicleId { get; set; } = 0;
        public QbiclePost Post { get; set; }
        public QbicleActivity Activity { get; set; }
        public string CreatedByName { get; set; }
        public int AppendToPageId { get; set; } = 0;
        public object ActivityBroadcast { get; set; }
        public Notification QbicleNotification { get; set; }
        public QbicleDomain Domain { get; set; }
        public bool HasActionToHandle { get; set; }
        public HighlightPost HLPost { get; set; }
        public DomainRequest DomainRequested { get; set; }
    }

    public class ActivityNotification
    {
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// connection id,(user created notification) using for case a user log in to multi app
        /// for check on signalR if current connection != originaling creation id then reload
        /// </summary>
        public string OriginatingCreationId { get; set; } = string.Empty;

        public int DomainId { get; set; }
        public int QbicleId { get; set; }
        public int DiscussionId { get; set; }

        /// <summary>
        /// Activity Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Post Id
        /// </summary>
        public int PostId { get; set; }

        public object ActivityBroadcast { get; set; }
        public int AppendToPageId { get; set; } = 0;
        public ApplicationPageName AppendToPageName { get; set; }
        public NotificationEventEnum EventNotify { get; set; }
        public string CreatedById { get; set; }
        public string ObjectById { get; set; }
        public string CreatedByName { get; set; }
        public double ReminderMinutes { get; set; } = 0;
        public AwsS3ObjectItem S3ObjectUploadedItem { get; set; }
        public bool HasActionToHandle { get; set; } = false;
        public DomainRequest DomainRequest { get; set; }
        public bool Notify2SysAdmin { get; set; } = false;
    }

    public class AuthorizeModel
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }

    public class SegmentContactModel
    {
        public string lstSegments { get; set; }
        public int totalContacts { get; set; }
    }

    public class EmailPreviewModel
    {
        public string Headline { get; set; }
        public string ButtonLink { get; set; }
        public string ButtonText { get; set; }
        public string BodyContent { get; set; }
        public int PromotionalImg { get; set; }
        public int AdImg { get; set; }
        public HttpPostedFileBase PromotionalImgFile { get; set; }
        public HttpPostedFileBase AdImgFile { get; set; }
        public string PromotionalImgPath { get; set; }
        public string AdImgPath { get; set; }
        public int TemplateId { get; set; }
    }

    public class MyProfileModel
    {
        public string Id { get; set; }
        public string profilePic { get; set; }
        public string Email { get; set; }
        public string DisplayUserName { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string Profile { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public NotificationSendMethodEnum ChosenNotificationMethod { get; set; }
        public NotificationSound NotificationSound { get; set; }
        public string Timezone { get; set; }
        public string Tell { get; set; }
        public int? PreferredDomain_Id { get; set; }
        public string PreferredDomain_Key { get; set; }
        public int? PreferredQbicle_Id { get; set; }
        public string Tagline { get; set; }
        public string Description { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string LinkedlnLink { get; set; }
        public string TwitterLink { get; set; }
        public string WhatsApp { get; set; }
    }

    public class MyProfilePrivacyOptionsModel
    {
        public bool isShareEmail { get; set; }
        public bool isShareCompany { get; set; }
        public bool isShareJobTitle { get; set; }
        public bool isAlwaysLimitMyContact { get; set; }
    }

    public class BudgetItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public string Unit { get; set; }
    }

    public class DiscussionQbicleModel
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ExpiryDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsExpiry { get; set; }
        public int Topic { get; set; }
        public string[] Assignee { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public string CurrentTimezone { get; set; }
        public MediaModel Media { get; set; }
        public int QbicleId { get; set; }
        public int FeaturedOption { get; set; }
        public string MediaDiscussionUse { get; set; }
        public string UploadKey { get; set; }

        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; }
    }

    public class AccountAssociated
    {
        public List<Qbicles.Models.Trader.TaxRate> TaxAccounts { get; set; } = new List<Qbicles.Models.Trader.TaxRate>();
        public List<TraderItem> SaleAccounts { get; set; } = new List<TraderItem>();
        public List<TraderItem> PurchaseAccounts { get; set; } = new List<TraderItem>();
        public List<TraderItem> InventoryAccount { get; set; } = new List<TraderItem>();
        public List<TraderCashAccount> CashAccount { get; set; } = new List<TraderCashAccount>();
        public List<TraderContact> ContactAccount { get; set; } = new List<TraderContact>();
    }

    public class ReportProductItem
    {
        public string Item { get; set; }
        public string Unit { get; set; }
        public string Quantity { get; set; }
        public string UnitPrice { get; set; }
        public string Discount { get; set; }
        public string Tax { get; set; }
        public string Total { get; set; }
    }

    public class ReportOrderInfo
    {
        public string FullRef { get; set; }
        public string OrderDate { get; set; }
        public string AddressLine { get; set; }
        public string AdditionalInformation { get; set; }
        public string BillingAddressLine { get; set; }
        public string ImageTop { get; set; }
        public string ImageBottom { get; set; }
        public string Total { get; set; }
        public string SalesTax { get; set; }
        public string Subtotal { get; set; }
        public string InvoiceAddress { get; set; }
        public string PurchaserName { get; set; }
        public string CurrencySymbol { get; set; }
    }

    public class InventoryModel
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string Icon { get; set; }
        public string Item { get; set; }
        public string Barcode { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string AverageCost { get; set; }
        public string LatestCost { set; get; }
        public string CurrentInventory { get; set; }
        public string DaysToLast { get; set; }
        public bool DaysToLastHighlighted { get; set; }
        public string MinInventory { get; set; }
        public string MaxInventory { set; get; }
        public string InventoryTotal { get; set; }
        public string Associated { get; set; }
        public int EditType { get; set; }
        public bool IsCompoundProduct { get; set; }
        public bool isBought { get; set; }
        public string ListUnitHtmlString { get; set; }
        public List<Select2Model> Units { get; set; }
    }

    public class InventoryReportDetailModel
    {
        public string ItemName { get; set; }
        public string ImageUrl { get; set; }
        public string Barcode { get; set; }
        public string SKU { get; set; }
        public string AverageCost { get; set; }
        public string LatestCost { get; set; }
        public string CurrentInventory { get; set; }
        public string MinInventory { get; set; }
        public string MaxInventory { get; set; }
        public string InventoryTotal { get; set; }
        public string DayToLast { get; set; }
        public string InventoryBasis { get; set; }
        public string DayToLastOperator { get; set; }
    }

    public class SpotCountModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string WorkgroupName { get; set; }
        public int ItemsCount { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }

        /// <summary>
        /// Initiated= 0,
        /// Draft = 1,
        /// CountStarted = 2,    //=> RequestStatusEnum.Pending
        /// CountCompleted = 3,  //=> RequestStatusEnum.Reviewed
        /// StockAdjusted = 4,   //=> RequestStatusEnum.Approved
        /// Discarded = 5,        //=> RequestStatusEnum.Discarded
        /// Denied = 6       //=> RequestStatusEnum.Denied
        /// </summary>
        public SpotCountStatus Status { get; set; }
    }

    public class WasteReportModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string CreatedBy { get; set; }
        public string WorkgroupName { get; set; }
        public int ItemsCount { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Draft = 1,
        /// Started = 2,    //=> RequestStatusEnum.Pending
        /// Completed = 3,  //=> RequestStatusEnum.Reviewed
        /// StockAdjusted = 4,   //=> RequestStatusEnum.Approved
        /// Discarded = 5        //=> RequestStatusEnum.Discarded
        /// </summary>
        public WasteReportStatus Status { get; set; }
    }

    public class ItemUnitChangeModel
    {
        public int ItemId { get; set; }
        public int UnitBaseId { get; set; }
        public string UnitName { get; set; }
    }

    public class ItemManagerTemplate
    {
        public int revId { get; set; }
        public int expId { get; set; }
        public string title { get; set; }
        public string action { get; set; }
    }

    public class ReportIncomeConfig
    {
        public string period { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string timezone { get; set; }
        public string date_format { get; set; }
        public string view { get; set; } = "monthly";
        public List<int> dimensions { get; set; }
        public List<TreeReportEntry> incomeReportEntry { get; set; }
    }

    public class TreeReportEntry
    {
        public int id { get; set; }
        public string text { get; set; }
        public bool isExpanded { get; set; }
        public List<int> children { get; set; }
    }

    public class ReportBalanceConfig
    {
        public string start_date { get; set; }
        public string timezone { get; set; }
        public string date_format { get; set; }
        public List<int> dimensions { get; set; }
        public List<int> allNodeIds { get; set; }
        public List<TreeBalanceReportEntry> incomeReportEntry { get; set; }
    }

    public class TreeBalanceReportEntry
    {
        public int id { get; set; }
        public string text { get; set; }
        public bool isExpanded { get; set; }
        public decimal amount { get; set; }
        public List<TreeBalanceReportEntry> children { get; set; }
        public List<int> allChildIds { get; set; }
    }

    public class BKTransacsionModel
    {
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal InitBabanceAccount { get; set; }
        public int NodeId { get; set; }
        public DateTime date { get; set; }
    }

    public class BKTransacsionsCustome
    {
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal InitBabanceAccount { get; set; }
        public int NodeId { get; set; }
        public string ColumnName { get; set; }
    }

    public class FilterIncomeReport
    {
        public string date { get; set; }
        public string view { get; set; }
        public List<int> dimensions { get; set; }
        public string company_name { get; set; }
        public string report_title { get; set; }
        public bool showlogo { get; set; }
        public string treeConfig { get; set; }
    }

    public class BudgetPanelReport
    {
        public int BudgetScenarioId { get; set; }
        public int BudgetGroupId { get; set; }
        public string Name { get; set; }
        public string ReportingPeriod { get; set; }
        public int TotalItems { get; set; }
        public decimal Amount { get; set; }
        public decimal SinceFiscalStartDate { get; set; }
        public decimal PeriodValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class SaleFilterParameter
    {
        public string Key { get; set; }
        public string Workgroup { get; set; }
        public string SaleChanel { get; set; }
        public string DateRange { get; set; }
    }

    public class TillDetailFilterParameter
    {
        public string Key { get; set; }
        public string DateRange { get; set; }
    }

    public class SafeDetailFilterParameter
    {
        public string Key { get; set; }
        public string DateRange { get; set; }
    }

    public class TraderSaleReturnCustom
    {
        public int Id { get; set; }
        public string FullRef { get; set; }
        public string WorkgroupName { get; set; }
        public string CreatedDate { get; set; }
        public string SalesRef { get; set; }
        public int SaleRefId { get; set; }
        public string SaleRefKey { get; set; }
        public string Dimensions { get; set; }
        public string SaleTotal { get; set; }
        public string Status { get; set; }
        public bool AllowEdit { get; set; }
        public int TransferCount { get; set; }
    }

    public class TraderBudgetGroupReportCustom
    {
        public int Id { get; set; }
        public string FullRef { get; set; }
        public string Item { get; set; }
        public string Dimension { get; set; }
        public string Unit { get; set; }
        public string Quantity { get; set; }
        public string Date { get; set; }
        public string Total { get; set; }
        public decimal TotalValue { get; set; }
        public string TotalAmount { get; set; }
    }

    public class UserSetting
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string ProfilePic { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string Timezone { get; set; }

        public string DateTimeFormat
        { get { return DateFormat + ' ' + TimeFormat; } }

        public bool IsSysAdmin { get; set; }
    }

    public class SpanneredWorkgroupCustom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LocationId { get; set; }
        public QbicleDomain Domain { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public int QbicleId { get; set; }
        public int TopicId { get; set; }
        public List<string> Members { get; set; }
        public List<string> Approvers { get; set; }
        public List<int> Processes { get; set; }
        public List<int> Groups { get; set; }
    }

    public class SpanneredWorkgroupsInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public string Process { get; set; }
        public string JsonTraderGroups { get; set; }
        public string Qbicle { get; set; }
        public int QbicleId { get; set; }
        public int TopicId { get; set; }
    }

    public class SpanneredAssetCustom
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Identification { get; set; }
        public string Description { get; set; }
        public int WorkgroupId { get; set; }
        public int LocationId { get; set; }
        public string FeaturedImageUri { get; set; }
        public QbicleDomain Domain { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public List<int> Tags { get; set; }
        public List<int> OtherAssets { get; set; }
        public List<Meter> Meters { get; set; }

        //public string MetersString { get; set; }
        public MediaModel MediaResponse { get; set; }

        public int LinkTraderItemId { get; set; }

        //public string InventoryCPSString { get; set; }
        public string MediaObjectKey { get; set; }

        public string MediaObjectName { get; set; }
        public string MediaObjectSize { get; set; }
        public List<SpanneredInventoryCPSCustom> AssetInventories { get; set; }
    }

    public class SpanneredInventoryCPSCustom
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public AssetInventory.PurposeEnum Purpose { get; set; }
    }

    public class CreateInventoryCustom
    {
        public int LocationId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Unitcost { get; set; }
    }

    public class ItemsAssociated
    {
        public bool IsCompoundProduct { get; set; } = false;
        public string ItemName { get; set; } = "";
        public List<Ingredient> Items { get; set; } = new List<Ingredient>();
    }

    public class Select2GroupItems
    {
        public string GroupName { get; set; }
        public bool Disabled { get; set; }
        public List<Select2Model> Items { get; set; }
    }

    public class BKTransactionCustom
    {
        public string Id { get; set; }

        public virtual JournalEntry JournalEntry { get; set; }

        public virtual BKAccount Account { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Credit { get; set; }

        public decimal? Balance { get; set; }
        public string Memo { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual List<TransactionDimension> Dimensions { get; set; } = new List<TransactionDimension>();

        public DateTime PostedDate { get; set; }

        //public virtual List<QbiclePost> Posts { get; set; } = new List<QbiclePost>();

        public virtual List<MediaModel> AssociatedFiles { get; set; } = new List<MediaModel>();

        public virtual BKTransaction Parent { get; set; }

        public string Reference { get; set; }

        ///// <summary>
        ///// A Bookkeeping Account can be associated with one or more CleanBooks accounts.
        ///// This property will list those CleanBoosk accounts this BKTransaction has been associated with.
        ///// </summary>
        //public virtual List<transaction> CBTransactions { get; set; } = new List<transaction>();
    }

    public class AlertSettingCustom
    {
        public int alertGroupId { get; set; }
        public string Reference { get; set; }
        public string Date { get; set; }
        public bool NoMvnAlert { get; set; }
        public bool MinMaxAlert { get; set; }
        public bool AccumulationAlert { get; set; }
    }

    public class TraderCashAccountCustom
    {
        public int TraderCashAccountId { get; set; }

        public string Name { get; set; }

        public virtual QbicleDomain Domain { get; set; }

        public virtual BKAccount AssociatedBKAccount { get; set; }

        public string ImageUri { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public BankmateAccountType BankmateType { get; set; }
        public decimal BalanceAmount { get; set; }

        // For Driver Bankmate Account type
        public virtual Driver AssociatedDriver { get; set; } = null;

        //For External Bankmate Account type
        public string NUBAN { get; set; } = "";

        public string IBAN { get; set; } = "";

        public virtual TraderAddress Address { get; set; } = null;

        public string PhoneNumber { get; set; } = "";

        public virtual Bank Bank { get; set; } = null;
    }

    public class Setting
    {
        //public string SignalRApi { get; set; }
        //public string HubUrl { get; set; }
        //public string DocumentsApi { get; set; }
        //public string QbiclesJobApi { get; set; }
        //public string TraderApi { get; set; }
        //public string MicroApi { get; set; }
        public string AWSS3SecretKey { get; set; }

        public string AWSS3AccessKey { get; set; }
        public string AWSS3BucketRegion { get; set; }
        public string AWSS3BucketName { get; set; }
        public string AWSS3IdentityPoolId { get; set; }
        public string QbiclesUrl { get; set; }
    }

    public class QbicleSetting
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string Setting { get; set; }
    }

    public class ListingPropertyCustom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatorName { get; set; }
    }

    public class ListingLocationCustom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupListRenderString { get; set; } = string.Empty;
    }

    public class B2CListingCustomModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string ImgUri { get; set; }
        public string Summary { get; set; }
        public string CreatedDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public bool isInActive { get; set; }
        public bool isLikedByCurrentUser { get; set; }
        public bool isCreatedByCurrentUser { get; set; }
        public string LocationName { get; set; }

        //Extra properties
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public PropertyType PropertyType { get; set; } = new PropertyType();
        public List<PropertyExtras> PropertyList { get; set; } = new List<PropertyExtras>();
        public int BedRoomNum { get; set; } = 0;
        public int BathRoomNum { get; set; } = 0;
        public string FileName { get; set; }
        public string FileUri { get; set; }
        public string SkillRequired { get; set; }
    }

    public class CalendarColor
    {
        public string date { get; set; }
        public string color { get; set; }
    }

    public class DomainAppAccess
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string Checked { set; get; }
    }

    public class CategoryCustomItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal Level { get; set; }
        public string InStockLabel { get; set; }
        public string ImageUri { get; set; }
    }

    public class DomainRoleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class B2COrderSettingDefault
    {
        public int LocationId { get; set; }
        public PrepQueueStatus B2cOrder { get; set; }
        public int DefaultSaleWorkGroupId { get; set; }
        public int DefaultInvoiceWorkGroupId { get; set; }
        public int DefaultPaymentWorkGroupId { get; set; }
        public int DefaultTransferWorkGroupId { get; set; }
        public int DefaultPaymentAccountId { get; set; }

        /// <summary>
        /// Use these settings(default workgroup) for all future orders
        /// </summary>
        public bool SaveSettings { get; set; } = false;
    }

    public class B2BOrderSettingDefault
    {
        public int LocationId { get; set; }
        public PrepQueueStatus B2bOrder { get; set; }
        public int DefaultSaleWorkGroupId { get; set; }
        public int DefaultInvoiceWorkGroupId { get; set; }
        public int DefaultPaymentWorkGroupId { get; set; }
        public int DefaultTransferWorkGroupId { get; set; }
        public int DefaultPaymentAccountId { get; set; }

        public int DefaultPurchaseWorkGroupId { get; set; }
        public int DefaultBillWorkGroupId { get; set; }
        public int DefaultPurchasePaymentWorkGroupId { get; set; }
        public int DefaultPurchaseTransferWorkGroupId { get; set; }
        public int DefaultPurchasePaymentAccountId { get; set; }
    }

    public class UserExpCustomModel
    {
        public int Id { get; set; }
        public string Place { get; set; }
        public string Role { get; set; }
        public string WorkingTime { get; set; }
        public string Summary { get; set; }
    }

    public class ShortlistGroupCandidateCustomModel
    {
        public string userId { get; set; }
        public string userFullName { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public string Job { get; set; }
        public bool isConnectedC2C { get; set; }
        public string LogoUri { get; set; }
        public string isConnectLabel { get; set; }
    }

    public class BusinessProfileAndInterest
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessSummary { get; set; }
        public string LogoUri { get; set; }
        public string ListInterests { get; set; }
        public bool IsConnected { get; set; }
    }

    public class QbicleDomainRequestCustomModel
    {
        public int requestId { get; set; }
        public string RequestedDate { get; set; }
        public string RequestById { get; set; }
        public string RequestedByName { get; set; }
        public string RequestedByLogoUri { get; set; }
        public string DomainName { get; set; }
        public string DomainLogoUri { get; set; }
        public string RequestStatusLabel { get; set; }
        public DomainTypeEnum DomainType { get; set; }
        public DomainRequestStatus Status { get; set; }
        public string RequestTypeStr { get; set; }
    }

    public class DomainExtensionRequestCustomModel
    {
        public int RequestId { get; set; }
        public int DomainId { get; set; }
        public string DomainKey { get; set; }
        public string RequestedDate { get; set; }
        public string ReqeustedById { get; set; }
        public string RequestedByName { get; set; }
        public string RequestedByLogoUri { get; set; }
        public string DomainName { get; set; }
        public string DomainLogoUri { get; set; }
        public string StatusLabel { get; set; }
        public string TypeName { get; set; }
        public ExtensionRequestStatus Status { get; set; }
        public ExtensionRequestType Type { get; set; }
        public string Note { get; set; }
    }

    public class UserVerification
    {
        /// <summary>
        /// token code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// user id
        /// </summary>
        public string Id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Is user id encrypted will be connect to new cuser
        /// </summary>
        public string ConnectCode { get; set; }

        public RegistrationType RegistrationType { get; set; } = RegistrationType.Web;
    }

    public enum RegistrationType
    {
        Web = 1,
        Micro = 2,
        Pos = 3
    }

    public class B2CConnectionTypeNumber
    {
        /// <summary>
        /// All (Exclude blocked) connections number
        /// </summary>
        public int NonBlockedConnectionNumber { get; set; }

        /// <summary>
        /// New Connections number
        /// </summary>
        public int NewConnectionNumber { get; set; }

        /// <summary>
        /// Blocked connections number
        /// </summary>
        public int BlockedConnectionNumber { get; set; }
    }

    public class TraderItemImportModel
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Spreadsheet { get; set; }
        public string SpreadsheetKey { get; set; }
        public string SpreadsheetErrorsKey { get; set; }
        public string CreatedDate { get; set; }
        public string Uploader { get; set; }
        public ImportStatus Status { get; set; }
        public string StatusName { get; set; }
        public string Location { get; set; }
        public int ItemsImported { get; set; }
        public int ItemsUpdated { get; set; }
        public int ItemsError { get; set; }
    }

    public class ItemImportData
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public string Sku { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public string Cost { get; set; }
        public string SellingPrice { get; set; }
        public string Inventory { get; set; }
        public string Active { get; set; }
    }

    public class OrderManagementCustomModel
    {
        public int Id { get; set; }
        public string OrderRef { get; set; }
        public string LocationName { get; set; }
        public string SaleChannel { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public string TotalStr { get; set; }
        public string Status { get; set; }
        public string QueuedInfo { get; set; }
        public string QueuedDate { get; set; }
        public string CompletedDate { get; set; }
        public string DeliveriedDate { get; set; }
        public string PreparedDate { get; set; }
        public string PrepStartedDate { get; set; }
        public string PaidStatus { get; set; }
        public string PaidStatusHtml { get; set; }
        public string Pending { get; set; }
        public string Preparing { get; set; }
        public string Completion { get; set; }
        public string DeliveryStatus { get; set; }
    }

    public class OrderManagementReport
    {
        public string Order { get; set; }
        public string Location { get; set; }
        public string SaleChannel { get; set; }
        public int Items { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public string Queued { get; set; }
        public string Pending { get; set; }
        public string Preparing { get; set; }
        public string Completion { get; set; }
        public string DeliveryStatus { get; set; }
        public string Payment { get; set; }
    }

    public class TraderConfigurationModel
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public ApplicationUser CreatedBy { set; get; }
        public DateTime CreatedDate { set; get; }
        public bool CanDelete { get; set; }
    }

    public class ContactGroupModel : TraderConfigurationModel
    {
        public int Members { get; set; }
    }

    public class WorkroupModel : TraderConfigurationModel
    {
        public string Qbicle { get; set; }
        public string Location { get; set; }
        public int Members { get; set; }
        public int ItemCategories { get; set; }
        public IEnumerable<string> Processes { get; set; }
    }

    public class JournalEntryModel
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public string PostedDate { get; set; }
        public string Status { get; set; }
        public string StatusCss { get; set; }
    }

    public class ChattingModel
    {
        public NotificationEventEnum Event { get; set; }
        public string ChatFromId { get; set; }
        public string ChatFromImg { get; set; }
        public string ChatFromEmail { get; set; }
        public string ChatFromName { get; set; }
        public List<string> ChatToEmails { get; set; } = new List<string>();
        public int DiscussionId { get; set; } = 0;
    }

    public class VoucherModel
    {
        public int Id { set; get; }
        public string Key { set; get; }
        public string Name { set; get; }
        public string Code { set; get; }
    }

    public class TraderItemEditCustomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InventoryDetail Inventory { get; set; }
        public virtual List<Recipe> AssociatedRecipes { get; set; }
    }

    public class StockAuditModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<StockAuditItemModel> ProductList { get; set; }
        public string StartedDate { get; set; }
        public bool IsFinished { get; set; }
        public string FinishedDate { get; set; }
        public BaseModel WorkGroup { get; set; }
        public ShiftAuditStatus Status { get; set; }
    }

    public class StockAuditItemModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string PeriodIn { get; set; }
        public string PeriodOut { get; set; }
        public BaseModel Unit { get; set; }
        public string OpeningCount { get; set; }
        public string ClosingCount { get; set; }
        public string Variance { get; set; }
        public string ExpectedClosing { get; set; }
    }

    public class SpotCountItemReport
    {
        public string ItemName { get; set; }
        public string SKU { get; set; }
        public string Unit { get; set; }
        public decimal SystemInventory { get; set; }
        public decimal ObservedInventory { get; set; }
        public decimal Adjustment { get; set; }
        public string Notes { get; set; }
    }

    public class WasteItemReport
    {
        public string ItemName { get; set; }
        public string SKU { get; set; }
        public string Unit { get; set; }
        public decimal ObservedInventory { get; set; }
        public decimal Wasted { get; set; }
        public string Notes { get; set; }
    }

    public class StockAuditItemReport
    {
        public string ItemName { get; set; }
        public int SKU { get; set; }
        public string Unit { get; set; }
        public int ObservedInventory { get; set; }
        public int In { get; set; }
        public int Out { get; set; }
        public int ExpectedClosing { get; set; }
        public int ObservedClosing { get; set; }
        public int Variance { get; set; }
    }

    public class SESIdentityCustomModel
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Added { get; set; }
        public SESIdentityStatus Status { get; set; }
        public string StatusLabel { get; set; }
    }

    public class PosOrderCancellationPrintCheckModel
    {
        public string Key { get; set; }
        public string Ref { get; set; }
        public string Date { get; set; }
        public string SalesChannel { get; set; }

        /// <summary>
        /// as pos device if using in Print check
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// as till manage if using in Print check
        /// </summary>
        public string CancelledBy { get; set; }

        public string Cashier { get; set; }
        public string Customer { get; set; }
        public string TotalItems { get; set; }
        public string ItemDetail { get; set; }
        public string DiscussionKey { get; set; }
        public List<IdNameModel> PDSOrders { get; set; }
        public string CashierId { get; set; }
        public string ManagerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Device { get; set; }
    }

    public class PosPrintCheckReport
    {
        public string OrderRef { get; set; }
        public string Date { get; set; }
        public string SalesChannel { get; set; }
        public string POSDevice { get; set; }
        public string TillManager { get; set; }
        public string Cashier { get; set; }
        public string Customer { get; set; }
        public string Items { get; set; }
        public string PDSOrders { get; set; }
    }

    public class PosCancellationReport
    {
        public string OrderRef { get; set; }
        public string Date { get; set; }
        public string SalesChannel { get; set; }
        public string Reason { get; set; }
        public string CancelledBy { get; set; }
        public string Cashier { get; set; }
        public string Customer { get; set; }
        public long Items { get; set; }
        public string PDSOrders { get; set; }
    }

    public class CatalogListCustomModel
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string ImageUri { get; set; }
        public int ItemNum { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TraderLocation Location { get; set; }
        public bool IsPublished { get; set; }
    }

    public class ProductItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public string SKU { get; set; }
        public string Group { get; set; }
    }

    public class DeliveryParameter
    {
        public string Keyword { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public List<int> Locations { get; set; }
        public List<int> Drivers { get; set; }

        /// <summary>
        /// All DeliveryStatus
        /// </summary>
        public List<DeliveryStatus> Status { get; set; }

        public bool ShowCompleted { get; set; }

        //second
        public int RefreshEvery { get; set; }

        /// <summary>
        /// 2-Date, 3- location, 4 - driver, 5 - status
        /// </summary>
        public List<string> Columns { get; set; }
    }

    public class CatalogVariantExtraPrice
    {
        public int Id { get; set; }
        public bool IsVariant { get; set; }
        public string Name { get; set; }
        public string ItemSKU { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryItemName { get; set; }
        public int CategoryItemId { get; set; }
        public string AverageAndLastestCost { get; set; }
        public int PriceId { get; set; }
        public decimal NetPrice { get; set; }
        public decimal GrossPrice { get; set; }
        public int TraderItemId { get; set; }
        public List<CustomizedPricingTax> ListTaxes { get; set; }

        /// <summary>
        /// This is a bool to indicate that there are prices in the catalog that have been updated
        /// because of a tax change to the underlying item's taxes
        /// </summary>
        public bool FlaggedForTaxUpdate { get; set; }

        /// <summary>
        /// This is a bool to indicate that there are prices in the catalog that have been updated
        /// because of a change to the underlying item's latest cost at the location at which the catalog is based
        /// </summary>
        public bool FlaggedForLatestCostUpdate { get; set; }

        public string MarginLatestCost { get; set; }
        public string MarginAverageCost { get; set; }

        /// <summary>
        /// Menu id
        /// </summary>
        public int CatalogId { get; set; }
    }

    public class CustomizedPricingTax
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string TaxName { get; set; }
    }

    public class PosUserModelEqualityComparer : IEqualityComparer<PosUserModel>
    {
        public bool Equals(PosUserModel x, PosUserModel y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.UserId == y.UserId; // Define your equality criteria here
        }

        public int GetHashCode(PosUserModel obj)
        {
            return obj.UserId.GetHashCode(); // Ensure this matches your equality criteria
        }
    }

    public class VerificationPinModel
    {
        public HttpStatusCode Status { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string message { get; set; }
    }

    public class SalesReportFilterParameter
    {
        public int DomainId { get; set; }
        public string DateRange { get; set; }
        public int LocationId { get; set; }
        public int ContactId { get; set; }
        public string SaleChanel { get; set; }
        public string TraderItemIds { get; set; } = "";
    }

    public class DomainUsersAllowed
    {
        /// <summary>
        /// the number of actual Domain members.
        /// </summary>
        public int ActualMembers { get; set; }

        /// <summary>
        /// the number of allowed Domain users
        /// </summary>
        public int UsersAllowed { get; set; }
    }

    public class NotificationAlertModel
    {
        public int NotificationId { get; set; }
        public bool IsCreatorTheCustomer { get; set; }
        public string Avatar { get; set; }
        public string Message { get; set; }
        public string Created { get; set; }
        public string ActivityKey { get; set; }
        public string ActivityType { get; set; }
        public object NavigateId { get; set; }
        public NavigateEnum NavigateTo { get; set; }
    }

    public enum NavigateEnum
    {
        /// <summary>
        /// GEt api/micro/qbicle/task?id=xxx
        /// </summary>
        [Description("Task")]
        Task = 1,

        /// <summary>
        /// GET api/micro/activity/mediaview?id=xxx
        /// </summary>
        [Description("Media")]
        Media = 2,

        /// <summary>
        /// GET api/micro/qbicle/discussion?id=xxx
        /// </summary>
        [Description("Discussion")]
        Discussion = 3,

        /// <summary>
        /// GET api/micro/b2c/order/detail?tradeId=xxx
        /// </summary>
        [Description("B2C Order")]
        B2COrder = 4,

        Post = 5,
        Invoice = 6,
        Payment = 7,
        Sale = 8,
        Purchase = 9,
        Transfer = 10,
        IdeaDiscussion = 12,
        PlaceDiscussion = 13,
        PosOrderDiscussion = 14,
        PerformanceDiscussion = 15,
        OrderCancellation = 16,
        GoalDiscussion = 17,
        B2BOrder = 18,
        B2BPartnershipDiscussion = 19,
        Approval = 20,
        Event = 21,
    }

    public class InventoryReportFilterModel : PaginationRequest
    {
        /// <summary>
        /// Use this for Update change Unit
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Use this for Update change Unit
        /// </summary>
        public int UnitId { get; set; }

        public int LocationId { get; set; }
        public string InventoryBasis { get; set; }
        public int MaxDayToLast { get; set; }
        public string Days2Last { get; set; }
        public int DayToLastOperator { get; set; }

        /// <summary>
        /// List of old changes- store in local
        /// </summary>
        public List<ItemUnitChangeModel> UnitsChanged { get; set; }
    }

    public class ProductImageGaleryModel
    {
        public int Order { get; set; }
        public string FileUri { get; set; }
    }

    // Community features
    public class FeaturedStoreDTItem
    {
        public int StoreId { get; set; }
        public int DisplayOrder { get; set; }
        public string DomainName { get; set; }
        public string DomainPlanLevelName { get; set; }
        public BusinessDomainLevelEnum DomainPlanLevel { get; set; }
        public string DomainImageUri { get; set; }
    }

    public class FeaturedProductDTItem
    {
        public int ProductId { get; set; }
        public int DisplayOrder { get; set; }
        public string ProductTypeLabelName { get; set; }
        public string ItemImageUri { get; set; }
        public string BusinessLogUri { get; set; }
        public string BusinessName { get; set; }
        public string CatalogItemName { get; set; }
        public string Link { get; set; }
        public string DomainName { get; set; }
    }

    public class WaitlistRequestRights
    {
        public string waitRequest { get; set; } = "none";
        public string waitPending { get; set; } = "none";
        public string allDomainCustom { get; set; } = "none";
        public string domainWithoutCustom { get; set; } = "none";
        public string waitJoinCustom { get; set; } = "none";
    }

    public class IdNameModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SearchShopProductModel : PaginationRequest
    {
        public string Keyword { get; set; }
    }

    public class ListProductGroupsWithCategory
    {
        public int CategoryId { get; set; }
        public int[] ProductGroupsId { get; set; }
    }

    public class TraderManufacturingView
    {
        public string Item { get; set; }
        public string UnitOfMeasurement { get; set; }
        public string QuantityConsumed { get; set; }
        public string UnitCost { get; set; }
        public string TotalCost { get; set; }
    }

    public class WasteReportItemModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Unit { get; set; }
        public string CurrentStock { get; set; }
        public string Notes { get; set; }
        public string Button { get; set; }
    }

    public class listB2BPartnershipInCatalog
    {
        public int id { get; set; }
        public List<simpleDomainInfo> consumerDomain { get; set; }
    }

    public class simpleDomainInfo
    {
        public int ConsumerDomainId { get; set; }
        public string Name { get; set; }
        public string LogoUriDomain { get; set; }
        public DateTime DateCoOp { get; set; }
        public string Phone { get; set; }
        public string BusinessSummary { get; set; }
        public string BusinessMail { get; set; }
    }

    public class SimpleCategoryExclution
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsSelected { get; set; } = false;
    }

    public class QueueOrderCustom
    {
        public int Id { get; set; }
        public string OrderRef { get; set; }
        public string Table { get; set; }
        public int PrepQueueId { get; set; }
    }
}