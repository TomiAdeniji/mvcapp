using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_QbicleLink")]
    public class QbicleLink : QbicleActivity
    {

        public QbicleLink()
        {
            this.ActivityType = ActivityTypeEnum.Link;
            this.App = ActivityApp.Qbicles;
        }


        public virtual QbicleMedia FeaturedImage { get; set; }


        [Required]
        public string URL { get; set; }

        public String Description { get; set; }


    }
}
