using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement
{
    [Table("trad_shipment")]
    public class Shipment
    {
        /// <summary>
        /// Unique identifier for the shipment
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The Domain with which the shipment is associated
        /// </summary>
        public QbicleDomain Domain { get; set; }

        /// <summary>
        /// A list of the transfers associated with this shipment
        /// </summary>
        public List<TraderTransfer> IncludedTransfers { get; set; }

        /// <summary>
        /// The driver doing the shipment
        /// </summary>
        public TraderContact Driver { get; set; }

        /// <summary>
        /// The reference for the Shipment
        /// </summary>
        public string Reference { get; set; }

    }
}
