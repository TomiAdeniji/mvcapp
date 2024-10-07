using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Spannered
{
    /// <summary>
    /// The MeterLogs class is noted above, as Meter readings are provided by users they will be added to the Meter Log with the value. 
    /// </summary>
    [Table("sp_meterlogs")]
    public class MeterLog
    {
        /// <summary>
        /// The unique ID to identify the spannered MeterLogs in the database
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The Unit of the Meters
        /// (e.g. “Celsius”)
        /// </summary>
        [Required]
        public decimal ValueOfUnit { get; set; }
        /// <summary>
        /// The user from Qbicles who created the spannered Meters
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this spannered Meters was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual Meter Meter { get; set; }
    }
}
