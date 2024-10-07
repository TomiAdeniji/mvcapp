using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_pipeline")]
    public class Pipeline
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        public virtual List<PipelineContact> Contacts { get; set; } = new List<PipelineContact>();

        public virtual List<Step> Steps { get; set; } = new List<Step>();

        /// <summary>
        /// This is the name of the Pipeline 
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// This is the image that is associated with the Pipeline and shown in the UI for the Pipeline.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        public string FeaturedImageUri { get; set; }


        /// <summary>
        /// This is the summary of the Pipeline 
        /// </summary>
        public string Summary { get; set; }


        /// <summary>
        /// The user from Qbicles who created the Pipeline
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Pipeline was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the Pipeline, this is to be set each time the Pipeline is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Pipeline was last edited.
        /// This is to be set each time the Pipeline is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        // <summary>
        /// Status of Pipeline
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsHidden { get; set; }


    }
}
