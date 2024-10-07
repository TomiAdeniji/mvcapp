namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_uploadformat")]
    public partial class UploadFormat
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UploadFormat()
        {
            uploads = new HashSet<upload>();
        }

        public int Id { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Name { get; set; }

        public int DateIndex { get; set; }

        public int ReferenceIndex { get; set; }

        public int Reference1Index { get; set; }

        public int DescriptionIndex { get; set; }

        public int CreditIndex { get; set; }

        public int DebitIndex { get; set; }

        public int BalanceIndex { get; set; }

        public int AmountIndex { get; set; }

        public int PriceIndex { get; set; }

        public int BankAccountIndex { get; set; }

        public int DescCol1Index { get; set; }

        public int DescCol2Index { get; set; }

        public int DescCol3Index { get; set; }

        public long FileTypeId { get; set; }

        public long DateFormatId { get; set; }

        public long CSVDelimiter { get; set; }

        [StringLength(1073741823)]
        public string CSVDelimiterValue { get; set; }



        [Column(TypeName = "bit")]
        public bool IsDateAscending { get; set; }



        [Column(TypeName = "bit")]
        public bool IsReferenceCusExp { get; set; }



        [Column(TypeName = "bit")]
        public bool IsReference1CusExp { get; set; }



        [Column(TypeName = "bit")]
        public bool IsFirstRowHeaders { get; set; }



        [Column(TypeName = "bit")]
        public bool IsDecimalRoundingEnabled { get; set; }

        public int? NumberOfDecimalPlaces { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public virtual dateformat dateformat { get; set; }

        public virtual filetype filetype { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<upload> uploads { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }
    }
}
