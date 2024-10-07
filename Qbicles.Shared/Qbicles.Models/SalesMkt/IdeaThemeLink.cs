using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_IdeaThemeLink")]
    public class IdeaThemeLink
    {
        /// <summary>
        /// The unique ID to identify the IdeaThemeLink in the database
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// The user from Qbicles who created the IdeaThemeLink
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this IdeaThemeLink was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The URL that is the link
        /// </summary>
        [Required]
        public string URL { get; set; }
    }
}
