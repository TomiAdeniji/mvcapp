using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_ExclusionCategoryName")]
    public class ExclusionCategoryName : DataModelBase
    {
        /// <summary>
        /// This is the name of the Category to be excluded
        /// </summary>
        [Required]
        public string CategoryName { get; set; }

    }
}
