using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_ordertype")]
    public class PosOrderType
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// This is the name of the PosOrderType
        /// It must be unique within a Location
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Order Type should be minimum 4 characters and a maximum of 40 characters")]
        public string Name { get; set; }

        /// <summary>
        /// This is the underlying type/classification of the PosOrderType
        /// </summary>
        [Required]
        public OrderTypeClassification Classification { get; set; }

        /// <summary>
        /// This is the Location with which the type is associated
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is other end of a many to many relationship with PosDeviceTypes
        /// </summary>
        public virtual ICollection<PosDeviceType> PosDeviceTypes { get; set; }

        /// <summary>
        /// This is other end of a many to many relationship with OdsDeviceType
        /// </summary>
        public virtual ICollection<OdsDeviceType> OdsDeviceTypes { get; set; }

        /// <summary>
        /// This is a description of the PosOrderType.
        /// </summary>
        public string Summary { get; set; }

        public PosOrderType()
        {
            PosDeviceTypes = new HashSet<PosDeviceType>();
            OdsDeviceTypes = new HashSet<OdsDeviceType>();
        }
    }
}