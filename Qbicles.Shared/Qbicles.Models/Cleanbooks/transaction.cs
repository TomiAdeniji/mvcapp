namespace CleanBooksData
{
    using Qbicles.Models.Bookkeeping;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transaction")]
    public partial class transaction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transaction()
        {
            transactionanalysisrecords = new HashSet<transactionanalysisrecord>();
            transactionmatchingrecords = new HashSet<transactionmatchingrecord>();
            fcreporttransactionxrefs = new HashSet<fcreporttransactionxref>();
            transactionmatchingunmatcheds= new HashSet<transactionmatchingunmatched>();
        }

        public long Id { get; set; }

        public DateTime Date { get; set; }

        [StringLength(250)]
        public string Reference { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Credit { get; set; }

        public decimal? Balance { get; set; }

        [StringLength(250)]
        public string Reference1 { get; set; }

        [StringLength(250)]
        public string DescCol1 { get; set; }

        [StringLength(250)]
        public string DescCol2 { get; set; }

        [StringLength(250)]
        public string DescCol3 { get; set; }

        public sbyte IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int UploadId { get; set; }

        public virtual upload upload { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysisrecord> transactionanalysisrecords { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingrecord> transactionmatchingrecords { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcreporttransactionxref> fcreporttransactionxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingunmatched> transactionmatchingunmatcheds { get; set; }


        /// <summary>
        /// When a BKTransaction is added as part of an upload from Bookkeeping to Cleanbooks,
        /// this property will record which BKTransaction this CleanBooks transaction came from
        /// </summary>
        public virtual BKTransaction BKTransaction { get; set; }
    }
}
