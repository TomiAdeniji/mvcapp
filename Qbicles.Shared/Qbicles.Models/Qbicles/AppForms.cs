using System;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Form;

namespace Qbicles.Models
{
    [Table("qb_appform")]
    public abstract class AppForm
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual FormDefinition FormDefinition { get; set; }

        public string FormData { get; set; }

        public string FormBuilder { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}