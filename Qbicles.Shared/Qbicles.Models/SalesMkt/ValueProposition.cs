using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table ("sm_ValueProposition")]
    public class ValueProposition
    {

        /// <summary>
        /// The unique ID to identify the value proposition in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who created the value proposition
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this value proposition was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the value proposition, this is to be set each time the attribute is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this value proposition was last edited.
        /// This is to be set each time the attribute is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the Brand with which this value proposition is associated
        /// </summary>
        [Required]
        public virtual Brand Brand { get; set; }


 


        /// <summary>
        /// The Product part of the value proposition.
        /// </summary>
        /// 
        [Required]
        public virtual BrandProduct BrandProduct{ get; set; }

        /// <summary>
        /// The collection of CustomerSegmenst associated with the value proposition
        /// </summary>
        /// 
        [Required]
        public virtual List<Segment> Segments { get; set; } = new List<Segment>();


        /// <summary>
        /// The 'Who want to' part of the value proposition.
        /// </summary>
        [Required]
        public string WhoWantTo { get; set; }


        /// <summary>
        /// The 'By' part of the value proposition.
        /// </summary>
        [Required]
        public string By { get; set; }

        /// <summary>
        /// Status of Value Proposition
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// This is a collection of the Social Campaigns with which the value proposition is associated
        /// This completes a many to many link between the SocialCampaigns and the value propositions.
        /// </summary>
        public virtual List<SocialCampaign> SocialCampaigns { get; set; } = new List<SocialCampaign>();

    }
}
