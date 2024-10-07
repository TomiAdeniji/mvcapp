using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Budgets
{
    /// <summary>
    /// This indicates days tha tare required for payment
    /// It is set against BudgetGroups because, 
    /// at a later stage of the process when working on CashFlow,
    /// the payment terms will be ised on a BudgetGroup basis to work out cash flow
    /// taking the payment terms into account
    /// </summary>
    [Table("trad_PaymentTerms")]
    public class PaymentTerms
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the name of the Payment Terms
        /// Typically it shoud leb something like 30 days, 60 days etc
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This is the number of days that are in the payment terms
        /// </summary>
        [Required]
        public int NumberOfDays { get; set; }
    }
}
