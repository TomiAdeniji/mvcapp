using Qbicles.Models.Base;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2B
{
    /// <summary>
    /// The B2BProfile is a profile definition within per Domain
    /// </summary>
    [Table("b2b_domainprofiles")]
    public class B2BProfile: DataModelBase
    {
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        [StringLength(150)]
        public string BusinessName { get; set; }

        [Required]
        [StringLength(500)]
        public string BusinessSummary { get; set; }

        public string BusinessEmail { get; set; }

        public string LogoUri { get; set; }

        public string BannerUri { get; set; }

        [Required]
        public bool IsB2BServicesProvided { get; set; } = false;

        [Required]
        public bool IsDisplayedInB2BListings { get; set; } = false;

        [Required]
        public bool IsDisplayedInB2CListings { get; set; } = false;

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        public virtual List<AreaOfOperation> AreasOperation { get; set; } = new List<AreaOfOperation>();
        public virtual List<B2BSocialLink> SocialLinks { get; set; } = new List<B2BSocialLink>();
        public virtual List<B2BTag> Tags { get; set; } = new List<B2BTag>();
        public virtual List<B2BPost> Posts { get; set; } = new List<B2BPost>();
        public virtual List<ApplicationUser> DefaultB2BRelationshipManagers { get; set; } = new List<ApplicationUser>();
        public virtual List<ApplicationUser> DefaultB2CRelationshipManagers { get; set; } = new List<ApplicationUser>();
        public virtual List<TraderLocation> BusinessLocations { get; set; } = new List<TraderLocation>();
        /// <summary>
        /// pos_menu
        /// </summary>
        public virtual List<Catalog> BusinessCatalogues { get; set; } = new List<Catalog>();
        public virtual List<BusinessCategory> BusinessCategories { get; set; } = new List<BusinessCategory>();
    }
}
