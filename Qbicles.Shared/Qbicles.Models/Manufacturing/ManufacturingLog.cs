using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Manufacturing
{
    [Table("trad_ManufacturingLog")]
    public class ManufacturingLog
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the user who updated the manufacturing job when the log was created
        /// </summary>
        [Required]
        public virtual ApplicationUser UpdatedBy { get; set; }

        /// <summary>
        /// This is the link to the ManuJob being logged
        /// </summary>
        [Required]
        public virtual ManuJob AssociatedManuJob { get; set; }

        /// <summary>
        /// The Compound item that is to be manufactured
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
        public virtual ManuProcessLog AssociatedProcessLog { get; set; }
    }

}
