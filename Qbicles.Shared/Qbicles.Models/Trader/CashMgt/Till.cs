using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.CashMgt
{
    [Table("trad_till")]
    public class Till
    {
        public int Id {get;set;}

        /// <summary>
        /// This is the name of the till
        /// It MUST be unique at a Location
        /// </summary>
        public string Name { get; set; }


        [Required]
        /// <summary>
        /// This is the Location at which the Till is located
        /// </summary>
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// This is the list of PosDevices that are associated with the Till
        /// </summary>
        public virtual List<PosDevice> PosDevices { get; set; } = new List<PosDevice>();


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }

        public DateTime LastUpdatedDate { get; set; }


        /// <summary>
        /// A collection of the sessions associated with this till
        /// </summary>
        public virtual List<Checkpoint> Checkpoints { get; set; } = new List<Checkpoint>();


        public virtual List<TillPayment> Payments { get; set; } = new List<TillPayment>();

        public virtual Safe Safe { get; set; }

        //public List<PosDeviceOrderXref> ListPosDeviceOrderXrefs { get; set; }
    }
}
