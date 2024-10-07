using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{

    /// <summary>
    /// This is the Kind of theme.
    /// It is populated through seeding and can be modified by the system administrator
    /// </summary>
    [Table("sm_IdeaThemeType")]
    public class IdeaThemeType
    {
        /// <summary>
        /// The unique ID to identify the IdeaThemeType in the database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the type
        /// </summary>
        [Required]
        public string Name { get; set; }
    }
}
