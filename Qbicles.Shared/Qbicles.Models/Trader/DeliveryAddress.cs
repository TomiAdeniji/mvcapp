using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Attributes;
using Qbicles.Models.Base;
using Qbicles.Models.Qbicles;

namespace Qbicles.Models.Trader
{
    [Table("trad_address")]
    public class TraderAddress : DataModelBase
    {
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public Country Country { get; set; }

        public string PostCode { get; set; }
        [DecimalPrecision(10, 7)]
        public decimal Longitude { get; set; }
        [DecimalPrecision(10, 7)]
        public decimal Latitude { get; set; }

        public bool IsDefault { get; set; } = false;
    }

    public class TraderAddressCustomModel
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string AddressFull { get; set; }

        [DecimalPrecision(10, 7)]
        public decimal Longitude { get; set; }
        [DecimalPrecision(10, 7)]
        public decimal Latitude { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}
