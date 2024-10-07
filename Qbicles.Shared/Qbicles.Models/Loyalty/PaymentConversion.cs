using System;

namespace Qbicles.Models.Loyalty
{
    public partial class PaymentConversion: OrderToPointsConversion
    {

        private int _points = 0;
        private decimal _amount = Decimal.Zero;

        public PaymentConversion()
        {
            this.ConversionType = OrderToPointsConversionType.Payment;
        }

        /// <summary>
        /// This is the factor that is multiplied against the payment amount to generate the points value
        /// This is the Quotient in the formula
        /// Quotient = Dividend / Divisor i.e.
        /// AmountConversionFactor = Points / Amount
        /// </summary>
        public decimal AmountConversionFactor { get; set; }

        /// <summary>
        /// This is the Dividend to calculate the AmountConversionFactor
        /// </summary>
        public int Points 
        {
            get { return _points; }
            set 
            {
                _points = value;
                CalculateAmountConversionFactor();
            }
        }


        /// <summary>
        /// This is the Divisor to calculate the AmountConversionFactor
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                CalculateAmountConversionFactor();
            }
        }

        /// <summary>
        /// Caluclate the AmountConversionFactor
        /// </summary>
        private void CalculateAmountConversionFactor()
        {
            if (!_amount.Equals(Decimal.Zero))
            {
                this.AmountConversionFactor = Decimal.Round((_points / _amount), 2);
            }
            else
                this.AmountConversionFactor = 0;
        }

    }


    
}
