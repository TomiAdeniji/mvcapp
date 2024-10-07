using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Pricing
{
    /// <summary>
    /// The Pricebook is used to assign prices to a list of items grouped by the Product Group (TraderGroup)
    /// </summary>
    [Table("trad_pricebook")]
    public class PriceBook
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// This is the Sales channel with which the PriceBook is associated.
        /// A procebook will be associated with only one channel.
        /// </summary>
        public virtual SalesChannelEnum SalesChannel { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the collection of the Versions of a PriceBook that a USer creates/sees
        /// The idea is that there could be a PriceBook called Shoes
        /// THere there xould be versions of thet PriceBook Shoes-Summer, Shoes-Winter
        /// </summary>
        public virtual List<PriceBookVersion> Versions { get; set; }

        /// <summary>
        /// This is the Location with which the PriceBook is associated.
        /// A PriceBook can be assocoated with only one Location.
        /// </summary>
        public virtual TraderLocation Location { get; set; }

        

        public virtual List<TraderGroup> AssociatedProductGroups { get; set; } = new List<TraderGroup>();
    }


}
