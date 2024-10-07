using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    /// <summary>
    /// This class contains any settings required by the Sales and Marketing app for a particular Domain
    /// </summary>
    [Table("sm_settings")]
    public class Settings
    {
        /// <summary>
        /// The unique ID to identify the campaign in the database
        /// </summary>
        public int Id { get; set; }
        public SMSetupCurrent IsSetupCompleted { get; set; } = SMSetupCurrent.Contacts;


        /// <summary>
        /// This is the Domain with which this campaign is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// This is the QBicle in the Domain that is used for all Sales and Marketing interaction
        /// </summary>
        public virtual Qbicle SourceQbicle { get; set; }

        /// <summary>
        /// THis is the Topic, from the SourceQBicle, under which all Approvals and Media Items will be assigned to the Qbicle
        /// </summary>
        public virtual Topic DefaultTopic { get; set; }


        /// <summary>
        /// This is a collection of the Social Networks in the the domain
        /// </summary>
        public virtual List<SocialNetworkAccount> SocialNetworkAccounts { get; set; } = new List<SocialNetworkAccount>();


        /// <summary>
        /// This is the collection of WorkGroups associated with Sales & Marketing
        /// </summary>
        public virtual List<SalesMarketingWorkGroup> WorkGroups { get; set; } = new List<SalesMarketingWorkGroup>();

    }

    public enum SMSetupCurrent
    {
        Contacts = 1,
        TraderContacts = 2,
        Qbicle = 3,
        Workgroup = 4,
        Complete = 5,
        SMApp = 6
    }
}
