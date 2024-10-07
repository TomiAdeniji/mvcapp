using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_NetworkType")]
    public class NetworkType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This indicates which type of campaign this Network can be used withis 
        /// Automated, Manual or Both
        /// </summary>
        [Required]
        public CampaignType AllowedCampaignType { get; set; }

        public virtual List<SocialCampaign> SocialCampaigns { get; set; } = new List<SocialCampaign>();

        /// <summary>
        /// This is a list of the Posts that the Network is associated with
        /// </summary>
        public virtual List<SocialCampaignPost> Posts { get; set; }
    }

    /// <summary>
    /// This enum is used in two places
    /// 1. To indicate if the NetworkType can be selected for Manual, Automated or Both types of campaigns
    /// 2. It is also used to indciate Which type of Social Campaign is being created, Manual or Automated NOT!!!! Both
    /// </summary>
    public enum CampaignType
    {
        Automated = 1,
        Manual = 2,
        Both = 3
    }
}
