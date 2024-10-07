using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;

namespace Qbicles.Models.TraderApi
{
    public class PosSettingsModel
    {
        public string TabletPrefix { get; set; }
        public TimeSpan RolloverTime { get; set; }
        public string MoneyCurrency { get; set; }
        public int MoneyDecimalPlaces { get; set; }
        public string ReceiptHeader { get; set; }
        public string ReceiptFooter { get; set; }
        public List<PosOrderTypeModel> OrderTypes { get; set; } = new List<PosOrderTypeModel>();
        public Table Table { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

    }
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }
    public class Table
    {
        public Uri LayoutImage { get; set; }
        public List<PosTableModel> Tables { get; set; } = new List<PosTableModel>();
    }

    public class PosTableModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
    }


    public class PosOrderTypeModel
    {
        public string Type { get; set; }
        public int Classification { get; set; }
    }


    public class PdsSettingsModel
    {
        public string MoneyCurrency { get; set; }
        public int MoneyDecimalPlaces { get; set; }
        /// <summary>
        /// The refresh time in minutes for the delivery display device
        ///  is the number of seconds between automatic refreshes in the PDS/DDS display. Cannot be null
        /// </summary>
        public int RefreshInterval { get; set; }
    }

    public class DdsSettingsModel
    {
        public string MoneyCurrency { get; set; }
        public int MoneyDecimalPlaces { get; set; }
        /// <summary>
        /// The refresh time in minutes for the delivery display device
        ///  is the number of seconds between automatic refreshes in the PDS/DDS display. Cannot be null
        /// </summary>
        public int RefreshInterval { get; set; }
        public string ReceiptHeader { get; set; }
        public string ReceiptFooter { get; set; }
        public int LingerTime { get; set; }
        public LocationModal Location { get; set; }
        public SpeedDistance SpeedDistance { get; set; }
    }

    public class LocationModal
    {
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }
}
