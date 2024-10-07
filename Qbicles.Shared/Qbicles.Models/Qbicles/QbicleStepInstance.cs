using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Qbicles.Models
{
    [Table("qb_stepinstance")]
    public class QbicleStepInstance
    {
        public int Id { get; set; }
        /// <summary>
        /// false = not selected, true = selected
        /// </summary>
        [Column(TypeName = "bit")]
        public bool isComplete { get; set; }

        public virtual QbicleTask Task { get; set; }

        public virtual QbicleStep Step { get; set; }
    }
}