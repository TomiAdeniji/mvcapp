using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using static Qbicles.Models.Notification;
using CleanBooksData;
using Qbicles.Models.Community;
using Qbicles.Models.Trader;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Spannered;
using Qbicles.Models.Operator;
using Qbicles.Models.Operator.Team;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Highlight;
using Microsoft.AspNet.Identity.CoreCompat;
using Qbicles.Models.Network;
using System.ComponentModel;
using Qbicles.Models.Loyalty;

namespace Qbicles.Models
{
    public enum CommunitySubscriptionLevelEnum
    {
        None = 0,
        Premium = 1,
        SuperPremium = 2

    }

    /// <summary>
    /// Model for an user
    /// </summary>
    /// 
    [Table("users")]
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Accounts = new HashSet<Account>();
            accounts1 = new HashSet<Account>();
            accountgroups = new HashSet<accountgroup>();
            dateformats = new HashSet<dateformat>();
            deletedaccounts = new HashSet<deletedaccount>();
            deletedaccounts1 = new HashSet<deletedaccount>();
            deletedtasks = new HashSet<deletedtask>();
            deleteduploads = new HashSet<deletedupload>();
            profiles = new HashSet<profile>();
            profiles1 = new HashSet<profile>();
            projects = new HashSet<project>();
            projectgroups = new HashSet<projectgroup>();
            tasks = new HashSet<task>();
            tasks1 = new HashSet<task>();
            taskgroups = new HashSet<taskgroup>();
            taskinstances = new HashSet<taskinstance>();
            transactionanalysiscomments = new HashSet<transactionanalysiscomment>();
            uploads = new HashSet<upload>();
            uploadformats = new HashSet<UploadFormat>();
            financialcontrolreportdefinitions = new HashSet<financialcontrolreportdefinition>();
            transactionmatchingtaskrules = new HashSet<transactionmatchingtaskrule>();
            tmtaskalerteexrefs = new HashSet<tmtaskalerteexref>();

            manualbalances = new HashSet<manualbalance>();
            singleaccountalerts = new HashSet<singleaccountalert>();
            singleaccountalertusersxrefs = new HashSet<singleaccountalertusersxref>();
            multipleaccountalertuserxrefs = new HashSet<multipleaccountalertuserxref>();
            multipleaccountalerts = new HashSet<multipleaccountalert>();
            balanceanalysiscomments = new HashSet<balanceanalysiscomment>();
            balanceanalysisdocuments = new HashSet<balanceanalysisdocument>();
            Performances = new HashSet<QbiclePerformance>();
            Peoples = new HashSet<QbiclePeople>();
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }


        [StringLength(1500)]
        public string Profile { get; set; }

        [StringLength(256)]
        public string ProfilePic { get; set; }

        [StringLength(35)]
        public string Forename { get; set; }

        [StringLength(35)]
        public string Surname { get; set; }
        public string Timezone { get; set; }

        public DateTime DateBecomesMember { get; set; }
        public string DateFormat { set; get; }
        public string TimeFormat { set; get; }
       

        //public virtual Qbicle CurrentQbicle { get; set; }

        public virtual QbicleDomain PreferredDomain { get; set; }

        public virtual Qbicle PreferredQbicle { get; set; }
        [MaxLength(50)]
        public string TagLine { get; set; }
        [StringLength(500)]
        public string JobTitle { get; set; }
        [StringLength(500)]
        public string Company { get; set; }
        public string Tell { get; set; }
        [Column(TypeName = "bit")]
        public bool? isShareEmail { get; set; }
        [Column(TypeName = "bit")]
        public bool? isShareCompany { get; set; }
        [Column(TypeName = "bit")]
        public bool? isShareJobTitle { get; set; }
        [Column(TypeName = "bit")]
        public bool? isAlwaysLimitMyContact { get; set; }
        [Column(TypeName = "bit")]
        public bool? IsQbiclesBankManager { get; set; }

        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string LinkedlnLink { get; set; }
        public string TwitterLink { get; set; }
        public string WhatsAppLink { get; set; }

        public bool IsUserProfileWizardRun { get; set; }
        public virtual List<QbiclePost> Posts { get; set; } = new List<QbiclePost>();
        public virtual List<UserAvatars> Avatars { get; set; } = new List<UserAvatars>();
        public virtual List<QbicleDomain> Domains { get; set; } = new List<QbicleDomain>();
        public virtual List<QbicleDomain> QbicleCashiersByDomain { get; set; } = new List<QbicleDomain>();
        public virtual List<QbicleDomain> QbicleSupervisorsByDomain { get; set; } = new List<QbicleDomain>();
        public virtual List<QbicleDomain> QbicleManagersByDomain { get; set; } = new List<QbicleDomain>();


        public virtual List<SubscriptionAccount> AccountUsers { get; set; } = new List<SubscriptionAccount>();
        public virtual List<SubscriptionAccount> AccountAdministrators { get; set; } = new List<SubscriptionAccount>();
        public virtual List<QbicleDomain> DomainAdministrators { get; set; } = new List<QbicleDomain>();
        public virtual List<Qbicle> Qbicles { get; set; } = new List<Qbicle>();

        public virtual List<QbicleActivity> Activities { get; set; } = new List<QbicleActivity>();

        public virtual List<TalentPool> TalentPools { get; set; } = new List<TalentPool>();

        public virtual List<DomainProfile> FollowedDomainProfiles { get; set; } = new List<DomainProfile>();

        public virtual List<CommunityPage> FollowedCommunityPages { get; set; } = new List<CommunityPage>();

        public virtual List<UserProfilePage> FollowedUserProfilePages { get; set; } = new List<UserProfilePage>();

        public CommunitySubscriptionLevelEnum CommunitySubscription { get; set; }
        [Column(TypeName = "bit")]
        public bool IsSystemAdmin { get; set; }

        [Column(TypeName = "bit")]
        public bool IsSocialHighlightsBlogger { get; set; }

        public string DisplayUserName { get; set; }

        public bool? IsEnabled { get; set; }

        public bool IsNavigationBarClosed { get; set; } = true;
        //Notification Settings
        public NotificationSendMethodEnum ChosenNotificationMethod { get; set; } = NotificationSendMethodEnum.Email;
        /// <summary>
        /// Play sound notification
        /// True: Play
        /// False: No
        /// </summary>
        public NotificationSound NotificationSound { get; set; } = NotificationSound.No;

        public virtual List<Qbicle> QbiclesManaged { get; set; } = new List<Qbicle>();
        public virtual List<DomainRole> DomainRoles { get; set; } = new List<DomainRole>();
        public virtual List<ApprovalReq> Reviewers { get; set; } = new List<ApprovalReq>();

        public virtual List<ApprovalRequestDefinition> ApprovalInitiators { get; set; } = new List<ApprovalRequestDefinition>();

        public virtual List<ApprovalRequestDefinition> ApprovalReviewers { get; set; } = new List<ApprovalRequestDefinition>();

        public virtual List<ApprovalRequestDefinition> ApprovalApprovers { get; set; } = new List<ApprovalRequestDefinition>();

        public virtual List<WorkGroup> WorkGroupMembers { get; set; } = new List<WorkGroup>();

        public virtual List<WorkGroup> WorkGroupReviewers { get; set; } = new List<WorkGroup>();

        public virtual List<WorkGroup> WorkGroupApprovers { get; set; } = new List<WorkGroup>();
        public virtual List<SalesMarketingWorkGroup> SM_WorkGroupMembers { get; set; } = new List<SalesMarketingWorkGroup>();
        public virtual List<SalesMarketingWorkGroup> SM_WorkGroupApprovers { get; set; } = new List<SalesMarketingWorkGroup>();


        public virtual List<BKWorkGroup> BKWorkGroupMembers { get; set; } = new List<BKWorkGroup>();

        public virtual List<BKWorkGroup> BKWorkGroupReviewers { get; set; } = new List<BKWorkGroup>();

        public virtual List<BKWorkGroup> BKWorkGroupApprovers { get; set; } = new List<BKWorkGroup>();

        public virtual List<SpanneredWorkgroup> SPWorkGroupMembers { get; set; } = new List<SpanneredWorkgroup>();
        public virtual List<SpanneredWorkgroup> SPWorkGroupApprovers { get; set; } = new List<SpanneredWorkgroup>();

        //public virtual List<B2BWorkgroup> B2BWorkGroupInvites { get; set; } = new List<B2BWorkgroup>();
        //public virtual List<B2BWorkgroup> B2BWorkGroupApprovers { get; set; } = new List<B2BWorkgroup>();
        //public virtual List<B2BWorkgroup> B2BWorkGroupReviewers { get; set; } = new List<B2BWorkgroup>();
        public virtual List<B2BRelationship> B2BRelationships { get; set; } = new List<B2BRelationship>();
        public virtual List<C2CQbicle> C2CQbicles { get; set; } = new List<C2CQbicle>();
        public virtual List<CQbicle> CQbicles { get; set; } = new List<CQbicle>();

        public virtual List<CBWorkGroup> CBWorkGroupMembers { get; set; } = new List<CBWorkGroup>();

        public virtual List<CBWorkGroup> CBWorkGroupReviewers { get; set; } = new List<CBWorkGroup>();

        public virtual List<CBWorkGroup> CBWorkGroupApprovers { get; set; } = new List<CBWorkGroup>();

        public virtual List<TraderAddress> TraderAddresses { get; set; } = new List<TraderAddress>();

        /// <summary>
        /// This is part of a Many to Many relationship to indicate the administrators of the PrepDisplayDevices
        /// </summary>
        public virtual List<PrepDisplayDevice> AdminForPrepDisplayDevice { get; set; } = new List<PrepDisplayDevice>();
        /// <summary>
        /// This is a part of the Many to Many relationship to indicate the administrators of the DdsDevice
        /// </summary>
        public virtual List<DdsDevice> AdminForDdsDevice { get; set; } = new List<DdsDevice>();

        public virtual List<WorkGroupTeamMember> TeamMembers { get; set; } = new List<WorkGroupTeamMember>();
        public virtual List<WorkGroupTaskMember> TaskMembers { get; set; } = new List<WorkGroupTaskMember>();
        public virtual List<TeamPerson> TeamPersons { get; set; } = new List<TeamPerson>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Account> Accounts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Account> accounts1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<accountgroup> accountgroups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dateformat> dateformats { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<deletedaccount> deletedaccounts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<deletedaccount> deletedaccounts1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<deletedtask> deletedtasks { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<deletedupload> deleteduploads { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<profile> profiles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<profile> profiles1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<project> projects { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<projectgroup> projectgroups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task> tasks { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task> tasks1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<taskgroup> taskgroups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<taskinstance> taskinstances { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysiscomment> transactionanalysiscomments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<upload> uploads { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UploadFormat> uploadformats { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<role> roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolreportdefinition> financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingtaskrule> transactionmatchingtaskrules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tmtaskalerteexref> tmtaskalerteexrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<manualbalance> manualbalances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<singleaccountalert> singleaccountalerts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<singleaccountalertusersxref> singleaccountalertusersxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<multipleaccountalertuserxref> multipleaccountalertuserxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<multipleaccountalert> multipleaccountalerts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysiscomment> balanceanalysiscomments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysisdocument> balanceanalysisdocuments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QbiclePerformance> Performances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QbiclePeople> Peoples { get; set; }
        public virtual List<B2BProfile> B2bProfileManagers { get; set; }

        public virtual List<B2BProfile> B2CProfileManagers { get; set; }

        public virtual List<QbicleDomain> HighlightDomainHidden { get; set; }
        public virtual List<HighlightPost> LikedHighlightPosts { get; set; }
        public virtual List<HighlightPost> BookmarkedHighlightPosts { get; set; }
        public virtual List<ListingHighlight> FlaggedListings { get; set; }
        public virtual List<Partnership> B2BProviderPartnerships { get; set; }
        public virtual List<Partnership> B2BConsumerPartnerships { get; set; }
        public virtual List<BusinessCategory> Interests { get; set; } = new List<BusinessCategory>();
        public virtual List<ShortListGroup> AssociatedShortListGroups { get; set; }
        public UserWizardStep WizardStep { get; set; }
        public virtual List<StoreCreditPIN> StoreCreditPINs{ get; set; }
        public virtual List<Qbicle> RemovedQbicle { get; set; } = new List<Qbicle>();
        public virtual List<C2CQbicle> NotViewedQbicle { get; set; } = new List<C2CQbicle>();

        public bool? IsUserProfileWizardRunMicro { get; set; }

        /// <summary>
        /// 1 - skip
        /// 2 - complete
        /// </summary>
        public int? IsImportWizardMicro { get; set; } = 0;
        // Loyalty


        /// <summary>
        /// This is a list of the promotions liked by this user
        /// </summary>
        public virtual List<LoyaltyPromotion> LikedPromotions { get; set; } = new List<LoyaltyPromotion>();

        public virtual List<LoyaltyPromotion> MarkedLikedPromotions { get; set; } = new List<LoyaltyPromotion>();
    }

    public enum UserWizardStep
    {
        [Description("Step 1")]
        GeneralSettingsStep = 1,
        [Description("Step 2")]
        AddressAndPhoneSettingStep = 2,
        [Description("Step 3")]
        ShowcaseSettingStep = 3,
        [Description("Step 4")]
        Settings = 4,
        [Description("Step 5")]
        InterestSettingsStep = 5,
        [Description("Step 6")]
        BusinessesConnectStep = 6
    }

    public enum UserWizardPlatformType
    {
        Website = 1,
        MicroApp = 2
    }
}