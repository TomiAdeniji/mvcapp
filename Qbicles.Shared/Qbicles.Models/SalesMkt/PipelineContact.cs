using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_PipelineContact")]
    public class PipelineContact
    {
        public int Id { get; set; }

        [Required]
        public virtual SMContact Contact { get; set; }

        public virtual Step Step { get; set; }


        public decimal PotentialValue { get; set; }


        public ProspectRating Rating { get; set; }

        [Required]
        public virtual Pipeline Pipeline { get; set; }


        public virtual List<QbicleTask> Tasks { get; set; } = new List<QbicleTask>();

        public virtual List<QbicleEvent> Events { get; set; } = new List<QbicleEvent>();


        /// <summary>
        /// The user from Qbicles who created the PipelineContact
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this PipelineContact was created i.e. the SMCOntact was added to the Pipeline
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the PipelineContact, this is to be set each time the PipelineContact is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this PipelineContact was last edited.
        /// This is to be set each time the PipelPipelineContactine is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }



        public virtual List<EmailCampaign> EmailCampaigns { get; set; } = new List<EmailCampaign>();



    }


    public enum ProspectRating
    {
        Star_1 = 1,
        Star_2 = 2,
        Star_3 = 3,
        Star_4 = 4,
        Star_5 = 5
    }
}
