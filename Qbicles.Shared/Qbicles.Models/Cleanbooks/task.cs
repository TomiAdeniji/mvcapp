namespace CleanBooksData
{
    using Qbicles.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_tasks")]
    public partial class task
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public task()
        {
            taskaccounts = new HashSet<taskaccount>();
            taskinstances = new HashSet<taskinstance>();
            financialcontrolreportdefinitions = new HashSet<financialcontrolreportdefinition>();
            transactionmatchingtaskrulesaccess = new HashSet<transactionmatchingtaskrulesacces>();
            transactionmatchingrules = new HashSet<transactionmatchingrule>();
            fcratioprofiletaskxrefs = new HashSet<fcratioprofiletaskxref>();
            financialcontroltrendprofiletaskxrefs = new HashSet<financialcontroltrendprofiletaskxref>();
            transactionmatchingtaskrules = new HashSet<transactionmatchingtaskrule>();
            singleaccountalerttransanalysisxrefs = new HashSet<singleaccountalerttransanalysisxref>();
            alertmultipleprofiletaskxrefs = new HashSet<alertmultipleprofiletaskxref>();
            balanceanalysismappingrules = new HashSet<balanceanalysismappingrule>();
            balanceanalysisactions = new HashSet<balanceanalysisaction>();
            transactionmatchingunmatcheds = new HashSet<transactionmatchingunmatched>();
            QbicleTask = null;
            DomainRoles = new List<DomainRole>();
        }
        [Required]
        public virtual QbicleTask QbicleTask { get; set; }
        [Key, ForeignKey("QbicleTask")]
        public int Id { get; set; }

        [StringLength(45)]
        public string Name { get; set; }

        [StringLength(3000)]
        public string Description { get; set; }

        public int ReconciliationTaskGroupId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public virtual CBWorkGroup WorkGroup { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public int TaskExecutionIntervalId { get; set; }

        public int TaskTypeId { get; set; }

        [StringLength(128)]
        public string AssignedUserId { get; set; }

        public int TransactionMatchingTypeId { get; set; }

        public sbyte? IsActionNotify { get; set; }

        public sbyte? IsActionReport { get; set; }

        public sbyte? IsActionNewLedger { get; set; }
        public DateTime? InitialTransactionDate { get; set; }

        public virtual taskgroup taskgroup { get; set; }

        public virtual taskexecutioninterval taskexecutioninterval { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        public virtual tasktype tasktype { get; set; }

        public virtual transactionmatchingtype transactionmatchingtype { get; set; }

        public virtual Qbicles.Models.ApplicationUser user1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<taskaccount> taskaccounts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<taskinstance> taskinstances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolreportdefinition> financialcontrolreportdefinitions { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingtaskrulesacces> transactionmatchingtaskrulesaccess { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingrule> transactionmatchingrules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioprofiletaskxref> fcratioprofiletaskxrefs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fctotalprofiletaskxref> fctotalprofiletaskxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontroltrendprofiletaskxref> financialcontroltrendprofiletaskxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingtaskrule> transactionmatchingtaskrules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<singleaccountalerttransanalysisxref> singleaccountalerttransanalysisxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<alertmultipleprofiletaskxref> alertmultipleprofiletaskxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysismappingrule> balanceanalysismappingrules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysisaction> balanceanalysisactions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingunmatched> transactionmatchingunmatcheds { get; set; }
        public virtual List<DomainRole> DomainRoles { get; set; } = new List<DomainRole>();

    }
}
