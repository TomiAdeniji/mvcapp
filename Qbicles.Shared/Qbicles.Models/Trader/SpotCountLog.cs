using Qbicles.Models.Base;
using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_spotcountlog")]
    public class SpotCountLog : DataModelBase
    {

        public virtual SpotCount AssociatedSpotCount { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        public virtual List<SpotCountItem> ProductList { get; set; } = new List<SpotCountItem>();

        public virtual ApprovalReq SpotCountApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual SpotCountStatus Status { get; set; } = SpotCountStatus.Draft;

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the spot count is first created the LastUpdateDate = CreatedDate
        /// </summary>
        [Required]
        public DateTime LastUpdatedDate { get; set; }


        /// <summary>
        /// When the spot count is first created the LastUpdatedBy = CreatedBy
        /// </summary>
        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

    }


}
