namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingrules")]
    public partial class transactionmatchingrule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingrule()
        {            
           
        }

        public int Id { get; set; } 
        public int TaskId { get; set; }
        public int? IsManyToMany_Set { get; set; }
        public int? IsOneToOne_Set { get; set; }
        public int? IsReferenceAndDate_Set { get; set; }
        public int? IsRefAndDate_RefToRef1_Set { get; set; }
        public int? IsRefAndDate_RefToDesc_Set { get; set; }
        public int? IsReference_Set { get; set; }
        public int? IsRef_RefToRef1_Set { get; set; }
        public int? IsRef_RefToDesc_Set { get; set; }
        public int? IsDescriptionAndDate_Set { get; set; }
        public int? IsDescription_Set { get; set; }
        public int? IsAmountAndDate_Set { get; set; }
        public int? IsReversals_Set { get; set; }
        public int? DateVarianceValue { get; set; }
        public decimal? Amount1VarianceValue { get; set; }
        public decimal? Amount2VarianceValue { get; set; }
        public string VarianceName { get; set; }

        public virtual task tasks { get; set; }
       
    }
}
