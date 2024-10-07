using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;
namespace Qbicles.Models.Manufacturing
{
    [Table("trad_ManufacturingJob")]
    public class ManuJob : DataModelBase
    {
        /// <summary>
        /// This is the unit of the Product that is used to quantify how many items are to be manufactured.
        /// </summary>
        [Required]
        public virtual ProductUnit AssemblyUnit { get; set; }

        /// <summary>
        /// This is the workgroup with which this manufacturing job is associated
        /// </summary>
        [Required]
        public virtual WorkGroup WorkGroup { get; set; }

        /// <summary>
        /// The Domain with which the manufacturing job is associated
        /// </summary>
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// (TraderItem) The Compound item that is to be manufactured
        /// </summary>
        [Required]
        public virtual TraderItem Product { get; set; }

        /// <summary>
        /// The quantity of the item that is to be manufactured
        /// </summary>
        [Required]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The Recipe, associated with the Product, to be used in making the compound item
        /// The Recipe is NOT selected by the user, 
        /// the Recipe is set based on
        ///     the CurrentRecipe of the InventoryDetail
        ///     for the TraderItem
        ///     at the Location the Manufacturing is to take place.
        /// </summary>
        [Required]
        public virtual Recipe SelectedRecipe {get; set;}


        /// <summary>
        /// The location at which the item is to be manufactured
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// The user that created the job 
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// The date on which the job was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the status for the manufacturing job
        /// </summary>
        [Required]
        public ManuJobStatus Status { get; set; }

        /// <summary>
        /// This is a collection of the Log records that are created each time the ManuJob is updated.
        /// </summary>
        public virtual List<ManufacturingLog> Logs { get; set; }


        public virtual ApprovalReq ManuApprovalProcess { get; set; }

        /// <summary>
        /// This is a collection of the process logs created when processing the ManuJob through the process
        /// </summary>
        public virtual List<ManuProcessLog> ProcessLogs { get; set; }

        public virtual TraderReference Reference { get; set; }


    }


    public enum ManuJobStatus
    {
        [Description("Pending")]
        Pending = 1,
        [Description("Reviewed")]
        Reviewed = 2,
        [Description("Approved")]
        Approved = 3,
        [Description("Discarded")]
        Discarded = 4,
        [Description("Denied")]
        Denied = 5
    }
}
