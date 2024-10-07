using CleanBooksData;
using Qbicles.Models.Base;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Community;
using Qbicles.Models.Loyalty;
using Qbicles.Models.ProfilePage;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_qbicledomain")]
    public class QbicleDomain : DataModelBase
    { 
        [StringLength(45)]
        [Required]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ApplicationUser OwnedBy { get; set; }
        public string LogoUri { get; set; }
        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> QbicleCashiers { get; set; } = new List<ApplicationUser>();
        public virtual List<ApplicationUser> QbicleSupervisors { get; set; } = new List<ApplicationUser>();
        public virtual List<ApplicationUser> QbicleManagers { get; set; } = new List<ApplicationUser>();
        public virtual List<ApplicationUser> Administrators { get; set; } = new List<ApplicationUser>();
        public virtual SubscriptionAccount Account { get; set; }

        public virtual List<Qbicle> Qbicles { get; set; } = new List<Qbicle>();

        public virtual List<AppInstance> AssociatedApps { get; set; } = new List<AppInstance>();

        public virtual List<accountgroup> accountgroups { get; set; }


        public virtual List<Page> CommunityPages { get; set; } = new List<Page>();

        public virtual TalentPool AssociatedTalentPool { get; set; }
        
        public DomainStatusEnum Status { get; set; } = DomainStatusEnum.Open;

        public DomainTypeEnum DomainType { get; set; } = DomainTypeEnum.Premium;
        public enum DomainStatusEnum
        {
            [Description("Active")]
            Open = 1,
            [Description("Closed")]
            Closed = 2
        }
        public enum DomainTypeEnum
        {
            Business = 0,
            Community = 1,
            Premium = 2
        }
        public List<TransactionDimension> Dimensions { get; set; } = new List<TransactionDimension>();
        public virtual List<CoANode> CoANode { get; set; } = new List<CoANode>();
        public virtual List<BKAppSettings> BKAppSettings { get; set; } = new List<BKAppSettings>();
        public virtual List<JournalGroup> JournalGroups { get; set; } = new List<JournalGroup>();


        public virtual List<TraderLocation> TraderLocations { get; set; } = new List<TraderLocation>();
        public virtual List<TraderItem> TraderItems { get; set; } = new List<TraderItem>();

        public virtual List<TraderGroup> TraderGroups { get; set; } = new List<TraderGroup>();


        public virtual List<TraderContactGroup> ContactGroups { get; set; } = new List<TraderContactGroup>();

        public virtual List<WorkGroup> Workgroups { get; set; } = new List<WorkGroup>();
        public virtual List<CBWorkGroup> CBWorkgroups { get; set; } = new List<CBWorkGroup>();

        public virtual List<B2BQbicle> B2BQbicles { get; set; } = new List<B2BQbicle>();

        public virtual List<BulkDealVoucherInfo> BulkDealVoucherInfos { get; set; } = new List<BulkDealVoucherInfo>();


        public virtual List<QbicleApplication> SubscribedApps { get; set; } = new List<QbicleApplication>();
        public virtual List<QbicleApplication> AvailableApps { get; set; } = new List<QbicleApplication>();
        public virtual List<ApplicationUser> HighlightPosterHiddenUser { get; set; } = new List<ApplicationUser>();


        public virtual List<BusinessPage> BusinessPages { get; set; } = new List<BusinessPage>();

        public virtual List<DomainExtensionRequest> ExtensionRequests { get; set; } = new List<DomainExtensionRequest>();

        public bool? IsBusinessProfileWizard { get; set; }
        public bool? IsBusinessProfileWizardMicro { get; set; }
        public DomainWizardStep WizardStep { get; set; }
        public DomainWizardStepMicro WizardStepMicro { get; set; }

        public bool IsTrialTimeStarted { get; set; } = false;

        public string SubAccountCode { get; set; }
    }

    public enum DomainWizardStep
    {
        [Description("Non")]
        None = 0,
        [Description("About your business")]
        General = 1,
        [Description("Invite users to your Domain")]
        InviteUsers = 2,
        [Description("Done")]
        Done = 3
    }
    public enum DomainWizardStepMicro
    {
        [Description("Non")]
        None = 0,
        [Description("About")]
        About = 1,
        [Description("Areas")]
        Areas = 2,
        [Description("Users")]
        Users = 3,
        [Description("Done")]
        Done = 4
    }
}