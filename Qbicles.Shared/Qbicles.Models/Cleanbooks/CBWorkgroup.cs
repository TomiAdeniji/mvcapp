using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanBooksData
{
    [Table("cb_workgroup")]
    public class CBWorkGroup
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual Qbicle Qbicle { get; set; }
        
        [Required]
        public virtual QbicleDomain Domain { set; get; }

        [Required]
        public virtual Topic Topic { get; set; }

        [Required]
        public virtual List<CBProcess> Processes { get; set; } = new List<CBProcess>();
        
        public virtual List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Reviewers { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Approvers { get; set; } = new List<ApplicationUser>();

        public  DateTime CreatedDate { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }


        public virtual List<Account> Accounts { get; set; } = new List<Account>();

        public virtual List<task> Tasks { get; set; } = new List<task>();
    }


}
