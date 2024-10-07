using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_SystemSettings")]
    public class SystemSettings
    {

      
        public int Id { get; set; }


        [Required]
        [Column(TypeName = "bit")]
        public bool IsArchived { get; set; } = false;

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual ApplicationUser ArchivedBy { get; set; }

        public DateTime ArchivedDate { get; set; }

        private int _points = 0;
        private decimal _amount = Decimal.Zero;


        /// <summary>
        /// This is the factor that is multiplied against the points  to generate the points value
        /// This is the Quotient in the formula
        /// Quotient = Dividend / Divisor i.e.
        /// PointConversionFactor = Amount / Points
        /// </summary>
        public decimal PointConversionFactor { get; set; }

        /// <summary>
        /// This is the Divisor to calculate the PointConversionFactor
        /// </summary>
        public int Points
        {
            get { return _points; }
            set
            {
                _points = value;
                CalculatePointConversionFactor();
            }
        }


        /// <summary>
        /// This is the Dividend to calculate the PointConversionFactor
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                CalculatePointConversionFactor();
            }
        }

        /// <summary>
        /// Calculate the PointConversionFactor
        /// </summary>
        private void CalculatePointConversionFactor()
        {
            if (_points != 0)
            {
                this.PointConversionFactor = _amount / _points;
            }
            else
                this.PointConversionFactor = 0;
        }
    }

}

