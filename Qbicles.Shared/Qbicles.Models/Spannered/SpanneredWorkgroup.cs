using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Spannered
{
    [Table("sp_Workgroups")]
    public class SpanneredWorkgroup
    {
        public int Id { get; set; }
        /// <summary>
        /// The Name of the SpanneredWorkgroup
        /// NOTE: The Name of the SpanneredWorkgroup MUST be unique within the Domain
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// This is the Trader Location with this SpanneredWorkgroup is associated.
        /// </summary>
        public virtual TraderLocation Location { get; set; }
        /// <summary>
        /// This is the Domain with which this asset is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        /// <summary>
        /// The user from Qbicles who created the spannered AssetTags
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this SpanneredWorkgroup was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the SpanneredWorkgroup, 
        /// This is to be set each time the spannered SpanneredWorkgroup is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        /// <summary>
        /// This is the QBicle in the Domain that is used for all Spannered interaction
        /// </summary>
        public virtual Qbicle SourceQbicle { get; set; }

        /// <summary>
        /// THis is the Topic, from the SourceQBicle, under which all Approvals and Media Items will be assigned to the Qbicle
        /// </summary>
        public virtual Topic DefaultTopic { get; set; }

        /// <summary>
        /// This is the date and time on which this SpanneredWorkgroup was last edited.
        /// This is to be set each time the SpanneredWorkgroup is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }
        [Required]
        public virtual List<SpanneredProcess> Processes { get; set; } = new List<SpanneredProcess>();

        public virtual List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> ReviewersApprovers { get; set; } = new List<ApplicationUser>();
        public virtual List<Asset> Assets { get; set; } = new List<Asset>();
        public virtual List<TraderGroup> ProductGroups { get; set; } = new List<TraderGroup>();
        public virtual List<ApprovalRequestDefinition> ApprovalDefs { get; set; } = new List<ApprovalRequestDefinition>();
    }
}
