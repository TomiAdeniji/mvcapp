namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcreporttransactionxref")]
    public partial class fcreporttransactionxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcreporttransactionxref()
        {
           
        }

        public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long TransactionId { get; set; }
        public int FinancialControlRecentTransactionAccountsId { get; set; }
        public virtual transaction transactions { get; set; }
        public virtual financialcontrolrecenttransactionaccounts financialcontrolrecenttransactionaccount { get; set; }
      
    }
}
