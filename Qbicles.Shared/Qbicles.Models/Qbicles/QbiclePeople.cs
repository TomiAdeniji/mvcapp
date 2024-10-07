using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Qbicles.Models
{
    [Table("qb_people")]
    public class QbiclePeople
    {
        public QbiclePeople()
        {
        }

        public int Id { get; set; }

        public PeopleTypeEnum Type { get; set; }
        /// <summary>
        /// false = No, true = Yes
        /// </summary>
        [Column(TypeName = "bit")]
        public bool? isPresent { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual QbicleSet AssociatedSet { get; set; }
        public enum PeopleTypeEnum
        {
            Assignee=0,
            Watcher=1,
            Invitee=2
        }
    }
}