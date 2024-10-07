using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    /// <summary>
    /// This class allows the user to define custom critera which are associated with a Contact.
    /// These custom criteria can then be used for filtering the contacts when creating segments
    /// </summary>
    [Table("sm_CustomCriteriaDef")]
    public class CustomCriteriaDefinition
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing Custom Criteria in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who created the CustomCriteriaDefinition
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this CustomCriteriaDefinition was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the CustomCriteriaDefinition, this is to be set each time the CustomCriteriaDefinition is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this CustomCriteriaDefinition was last edited.
        /// This is to be set each time the CustomCriteriaDefinition is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

    

        /// <summary>
        /// This field is used to indicate if the field MUST be set by the user
        /// </summary>
        [Required]
        [Column(TypeName = "bit")]
        public bool IsMandatory { get; set; }


        public virtual List<CustomOption> CustomOptions { get; set; } = new List<CustomOption>();


        /// <summary>
        /// This is used to indicate if a defined criterial should be displaxed to the user or not
        /// </summary>
        [Required]
        public CustomCriteriaStatus Status { get; set; }


        /// <summary>
        /// This indicates the order in which the custom critera is displayed
        /// </summary>
        [Required]
        public int DisplayOrder { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsAgeRange { get; set; }

    }


    public enum CustomCriteriaStatus
    {
        Active = 1,
        InActive = 2
    }




   

}
