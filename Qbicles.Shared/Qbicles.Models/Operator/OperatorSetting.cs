using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Operator
{
    [Table("op_settings")]
    public class OperatorSetting
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// This is the QBicle in the Domain that is used for all Operator interaction
        /// </summary>
        public virtual Qbicle SourceQbicle { get; set; }

        /// <summary>
        /// THis is the Topic, from the SourceQBicle, under which all Approvals and Media Items will be assigned to the Qbicle
        /// </summary>
        public virtual Topic DefaultTopic { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
