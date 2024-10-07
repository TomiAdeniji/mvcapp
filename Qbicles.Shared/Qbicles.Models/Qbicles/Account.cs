using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_subscriptionaccount")]
    public class SubscriptionAccount
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string AccountName { get; set; }
        //for the name of the company or organisation with which the account is to be associated
        public string CompanyOrganisationName { get; set; }
        //Link to the ApplicationUser
        [Required]
        public virtual ApplicationUser AccountCreator { get; set; }
        //ink to the ApplicationUser class as the AccountOwner (this is required separately to the creator because an owner may (possibly in the future) transfer ownership to another user)
        [Required]
        public virtual ApplicationUser AccountOwner { get; set; }
        //link to the QbicleDomain class, for all Domains that are created as part of this account
        public virtual List<QbicleDomain> Domains { get; set; } = new List<QbicleDomain>();
        //link to the AccountPackage
        public virtual AccountPackage AccountPackage { get; set; }
        /// <summary>
        /// Store when the Qbicle, Activity, Sub-Activity invite the guests
        /// </summary>
      
        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual List<ApplicationUser> Administrators { get; set; } = new List<ApplicationUser>();
        // Count of limit the Qbicle/ update ++ when invited guest from the Qbicle or Activity
        public int QbiclesCount { get; set; } = 0;
        
    }
}
