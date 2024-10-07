using Qbicles.Models.Base;
using Qbicles.Models.Catalogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_CategoryExclusion")]
    public class CategoryExclusionSet : DataModelBase
    {
        /// <summary>
        /// This is the name of the Category Exclusion Set
        /// </summary>
        [Required]
        public string Name { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the collection of DISTINCT Category Names that the user has decided to put into this set.
        /// There must not be aby duplicate names
        /// </summary>
        public virtual List<ExclusionCategoryName> CategoryNames { get; set; }

        /// <summary>
        /// This is the collection of PDS Devices that are using this CategoryExclusionSet
        /// </summary>
        public virtual List<PrepDisplayDevice> PrepDisplayDevices { get; set;}


        /// <summary>
        /// The Location in Trader at which this CategoryExclusionSet is 'stored'
        /// </summary>
        public virtual TraderLocation Location { get; set; }

        
    }
}
