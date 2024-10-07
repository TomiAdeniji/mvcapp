using Qbicles.Models.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.DDS
{
    [Table("dds_DeliveryDriverLog")]
    public class DriverLog
    {
        public int Id { get; set; }

        public virtual Driver Driver { get; set; }

        public virtual Delivery Delivery { get; set; }

        public string DeviceSerial { get; set; }

        public string DeviceName { get; set; }

        [DecimalPrecision(10, 7)]
        public decimal Longitude { get; set; }

        [DecimalPrecision(10, 7)]
        public decimal Latitude { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

