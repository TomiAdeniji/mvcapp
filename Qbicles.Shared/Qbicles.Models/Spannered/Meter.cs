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
    /// The Meters class is One or more Meters can be created with the Meters (and/or added later, once the Meters created). 
    /// Their use will be expanded on in later developments. 
    /// </summary>
    [Table("sp_meters")]
    public class Meter
    {
        /// <summary>
        /// The unique ID to identify the spannered Meters in the database
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The Name of the Meters
        /// NOTE: The Name of the Meters MUST be unique within the Domain
        /// </summary>
        [StringLength(250)]
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// The Unit of the Meters
        /// (e.g. “Celsius”)
        /// </summary>
        [StringLength(50)]
        [Required]
        public string Unit { get; set; }
        /// <summary>
        /// The Unit Value of the Meters
        /// </summary>
        [Required]
        public decimal ValueOfUnit { get; set; }
        /// <summary>
        /// This is the Description from the Asset ui
        /// </summary>
        /// </summary>
        [StringLength(500)]
        [Required]
        public string Description { get; set; }
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

        /// <summary>
        /// The user from Qbicles who last updated the spannered Meters, 
        /// This is to be set each time the spannered Meters is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this spannered Meters was last edited.
        /// This is to be set each time the spannered Meters is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        [Required]
        public virtual Asset Asset { get; set; }
        /// <summary>
        /// This is a collection of the MeterLogs associated with a Meter
        /// </summary>
        public virtual List<MeterLog> MeterLogs { get; set; } = new List<MeterLog>();
    }
}
