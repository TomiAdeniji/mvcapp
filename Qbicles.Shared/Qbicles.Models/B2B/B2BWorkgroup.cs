//using Qbicles.Models.Trader;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Qbicles.Models.B2B
//{
//    [Table("b2b_workgroups")]
//    public class B2BWorkgroup
//    {
//        public int Id { get; set; }
//        /// <summary>
//        /// The Name of the B2BWorkgroup
//        /// NOTE: The Name of the B2BWorkgroup MUST be unique within the Domain
//        /// </summary>
//        [Required]
//        [StringLength(150)]
//        public string Name { get; set; }
//        /// <summary>
//        /// This is the Trader Location with this B2BWorkgroup is associated.
//        /// </summary>
//        [Required]
//        public virtual TraderLocation Location { get; set; }
//        /// <summary>
//        /// This is the Domain with which this asset is associated
//        /// </summary>
//        [Required]
//        public virtual QbicleDomain Domain { get; set; }
//        /// <summary>
//        /// The user from Qbicles who created the B2BWorkgroup
//        /// </summary>
//        [Required]
//        public virtual ApplicationUser CreatedBy { get; set; }


//        /// <summary>
//        /// This is the date and time on which this B2BWorkgroup was created
//        /// </summary>
//        [Required]
//        public DateTime CreatedDate { get; set; }

//        /// <summary>
//        /// This is the QBicle in the Domain that is used for all B2B interaction
//        /// </summary>
//        public virtual Qbicle SourceQbicle { get; set; }

//        /// <summary>
//        /// THis is the Topic, from the SourceQBicle, under which all Approvals and Media Items will be assigned to the Qbicle
//        /// </summary>
//        public virtual Topic DefaultTopic { get; set; }

//        /// <summary>
//        /// The user from Qbicles who last updated the B2BWorkgroup, 
//        /// This is to be set each time the spannered B2BWorkgroup is saved.
//        /// So, the first setting will equal the CreatedBy
//        /// </summary>
//        public virtual ApplicationUser LastUpdatedBy { get; set; }

//        /// <summary>
//        /// This is the date and time on which this B2BWorkgroup was last edited.
//        /// This is to be set each time the B2BWorkgroup is saved.
//        /// So, the first setting will equal the CreatedDate
//        /// </summary>
//        public DateTime LastUpdateDate { get; set; }
//        [Required]
//        public virtual List<B2BProcess> Processes { get; set; } = new List<B2BProcess>();

//        public virtual List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

//        public virtual List<ApplicationUser> Reviewers { get; set; } = new List<ApplicationUser>();
//        public virtual List<ApplicationUser> Approvers { get; set; } = new List<ApplicationUser>(); 
//    }
//}
