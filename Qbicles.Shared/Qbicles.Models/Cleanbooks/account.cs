namespace CleanBooksData
{
    using Qbicles.Models;
    using Qbicles.Models.Bookkeeping;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_accounts")]
    public partial class Account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Account()
        {
            deleteduploads = new HashSet<deletedupload>();
            taskaccounts = new HashSet<taskaccount>();
            uploads = new HashSet<upload>();
            uploadfields = new HashSet<uploadfield>();
            financialcontrolbalanceaccounts = new HashSet<financialcontrolbalanceaccount>();
            financialcontrolrecenttransactionaccounts = new HashSet<financialcontrolrecenttransactionaccounts>();

            financialcontrolaccounttrends = new HashSet<financialcontrolaccounttrend>();
            fcratioaccounts = new HashSet<fcratioaccount>();
            singleaccountalerts = new HashSet<singleaccountalert>();
            alertmultipleaccounts = new HashSet<alertmultipleaccount>();
            DomainRoles = new List<DomainRole>();
        }

        public long Id { get; set; }
        [Required]
        [StringLength(256)]
        public string Name { get; set; }
        public sbyte? BalanceRecipe { get; set; }
        [StringLength(45)]
        public string Number { get; set; }

        public int IsActive { get; set; }

        public int GroupId { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public decimal? LastBalance { get; set; }

        public int? UpdateFrequencyId { get; set; }

        [StringLength(128)]
        public string DataManagerId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<deletedupload> deleteduploads { get; set; }

        public virtual accountupdatefrequency accountupdatefrequency { get; set; }

        public virtual ApplicationUser user { get; set; }

        public virtual ApplicationUser user1 { get; set; }

        public virtual CBWorkGroup WorkGroup { get; set; }

        public virtual accountgroup accountgroup { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<taskaccount> taskaccounts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<upload> uploads { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<uploadfield> uploadfields { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolbalanceaccount> financialcontrolbalanceaccounts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolrecenttransactionaccounts> financialcontrolrecenttransactionaccounts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolaccounttrend> financialcontrolaccounttrends { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioaccount> fcratioaccounts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<singleaccountalert> singleaccountalerts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<alertmultipleaccount> alertmultipleaccounts { get; set; }
        public virtual List<DomainRole> DomainRoles { get; set; } = new List<DomainRole>();


        /// <summary>
        /// This is the Bookkeeping account with which this CleanBooks account is associated
        /// A CleanBooks account can be associated with ONE Bookkeeping account only
        /// </summary>
        public virtual BKAccount BookkeepingAccount { get; set; }

    }

}

