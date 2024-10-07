using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Attributes;

namespace Qbicles.Models.Trader
{
    [Table("trad_productunit")]
    public class ProductUnit
    {
        
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        public virtual ProductUnit ParentUnit { get; set; }


        /// <summary>
        /// This is the quantity of the Parent Product Unit (ParentUnit) that goes into making this ProductUnit
        /// If there is no ParentUnit this value shoudl be 0
        /// </summary>
        [DecimalPrecision(10, 3)]
        public decimal Quantity { get; set; }


        /// <summary>
        /// This is the Quantity of the Base unit for the ProductUnit.
        /// There is only one ProductUnit for an Item that is the Baseunit. It is the Product unit with IsBase = TRUE.
        /// The purpose of the Baseunit is to provide a defined ProductUNit for an Item off which all other ProductUnits for the item can be based and
        /// it procides a common frame of reference for quantity beacuse all units know what quantity of base unit they are.
        /// </summary>
        [DecimalPrecision(10, 3)]
        public decimal QuantityOfBaseunit { get; set; }

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; }

        [Column(TypeName = "bit")]
        public bool IsPrimary { get; set; }


        [Column(TypeName = "bit")]
        public bool IsBase { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual TraderItem Item { get; set; }

        [Required]
        public MeasurementTypeEnum MeasurementType { get; set; }



    }
}