using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_step")]
    public class Step
    {
        public int Id { get; set; }

        [Required]
        public virtual Pipeline Pipeline { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Order { get; set; }

        public virtual List<PipelineContact> Contacts { get; set; }

        /// <summary>
        /// The user from Qbicles who created the Step
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Szep was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

    }
}
