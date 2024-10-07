using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    /// <summary>
    ///  Model for a qbicle
    /// </summary>
    [Table("qb_qbicles")]
    public class Qbicle : DataModelBase
    {
        public string Description { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual ApplicationUser StartedBy { get; set; }

        [Required]
        public DateTime StartedDate { get; set; }

        [Required]
        public virtual ApplicationUser OwnedBy { get; set; }

        public string LogoUri { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        public virtual ApplicationUser ClosedBy { get; set; }

        public DateTime? ClosedDate { get; set; }


        [Column(TypeName = "bit")]
        public bool IsUsingApprovals { get; set; } = false;

        //public virtual ApplicationUser Cashier { get; set; }
        //public virtual ApplicationUser Supervisor { get; set; }
        public virtual ApplicationUser Manager { get; set; }


        //List of all activities associated with this qbicle
        public virtual List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        //List of all activities associated with this qbicle
        public virtual List<QbicleActivity> Activities { get; set; } = new List<QbicleActivity>();

        //List of all media associated with this qbicle
        public virtual List<QbicleMedia> Media { get; set; } = new List<QbicleMedia>();

       
        //public virtual List<Topic> Topics { get; set; } = new List<Topic>();

        //List of all media associated with this qbicle
        public virtual List<MediaFolder> MediaFolders { get; set; } = new List<MediaFolder>();

        //public virtual List<BKAppSettings> BKAppSettings { get; set; } = new List<BKAppSettings>();

        public DateTime LastUpdated { get; set; }

        [Required]
        public bool IsHidden { get; set; }
        public virtual List<ApplicationUser> RemovedForUsers { get; set; } = new List<ApplicationUser>();

        //[Column(TypeName = "VARCHAR")]
        //[StringLength(128)]
        //public string Discriminator { get; set; }
        //[NotMapped]
        //public string Discriminator { get; }
    }
}