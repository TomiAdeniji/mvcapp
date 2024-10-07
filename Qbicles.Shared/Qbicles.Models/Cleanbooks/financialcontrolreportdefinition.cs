namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontrolreportdefinition")]
    public partial class financialcontrolreportdefinition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontrolreportdefinition()
        {
            financialcontrolbalanceaccounts = new HashSet<financialcontrolbalanceaccount>();
            financialcontroltotalprofiles = new HashSet<financialcontroltotalprofile>();
            financialcontrolrecenttransactionaccounts = new HashSet<financialcontrolrecenttransactionaccounts>();            
            financialcontrolaccounttrends = new HashSet<financialcontrolaccounttrend>();
            financialcontrolprofiletrends = new HashSet<financialcontrolprofiletrend>();            
            fcreportexecutioninstances = new HashSet<fcreportexecutioninstance>();
            financialcontrolratios = new HashSet<financialcontrolratio>();
            financialcontrolmanualbalances = new HashSet<financialcontrolmanualbalance>();          
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? TaskId { get; set; }
        public DateTime? CreatedDate { get; set; }       
        public string CreatedBy { get; set; }
        public int IdAutoExecute { get; set; }
        public virtual task tasks { get; set; }        
        public virtual Qbicles.Models.ApplicationUser users { get; set; }
        public virtual fcautoexecute fcautoexecutes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolbalanceaccount> financialcontrolbalanceaccounts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontroltotalprofile> financialcontroltotalprofiles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolrecenttransactionaccounts> financialcontrolrecenttransactionaccounts { get; set; }
      
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolaccounttrend> financialcontrolaccounttrends { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolprofiletrend> financialcontrolprofiletrends { get; set; }
       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcreportexecutioninstance> fcreportexecutioninstances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolratio> financialcontrolratios { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolmanualbalance> financialcontrolmanualbalances { get; set; }
      

    }
}
