using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;

namespace Qbicles.Models.TraderApi
{
    public class DsDeviceForUser
    {
        public List<OdsDeviceResult> Devices { get; set; }
    }
    public class OdsDeviceResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string QueueName { get; set; }
        public string SerialNumber { get; set; }
        public string Location { get; set; }
        public string Domain { get; set; }
    }


    public class DdsDriverParameter
    {
        public bool IsAvailable { get; set; }
        public bool IsBusy { get; set; }
        public MapWindow Window { get; set; }
    }

    public class MapWindow
    {
        public decimal MinLat { get; set; }
        public decimal MinLon { get; set; }
        public decimal MaxLat { get; set; }
        public decimal MaxLon { get; set; }
    }


    public class DsDriver
    {
        public List<DsDriverResult> Drivers { get; set; }
    }
    public class DsDriverResult
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Waiting = 1,
        /// Out = 2,
        /// Returning = 3
        /// </summary>
        public int Status { get; set; }
        public DriverStatus DriverStatus { get; set; }
        public string DriverStatusLable { get; set; }
        public string DriverStatusName { get; set; }
        public int DeliveryStatus { get; set; }
        public DriverDeliveryStatus DriverDeliveryStatus { get; set; }
        public string DriverDeliveryStatusLable { get; set; }
        public string DriverDeliveryStatusName { get; set; }
        public DateTime Time { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Email { get; set; }
        public Uri AvatarUrl { get; set; }
        public string Telephone { get; set; }
        public int? DeliveryId { get; set; }
        public string DeliveryReference { get; set; }
        /// <summary>
        /// Time to call Google Directions API (GDA) ( Minute)
        /// </summary>
        public int APICallThresholdTimeInterval { get; set; }
        /// <summary>
        /// Time to update location ( second) - get from system setting dds_systemsettings
        /// </summary>
        public int DriverUpdateLocationTimeInterval { get; set; } = 5;

        /// <summary>
        /// Can not change if delivery.Status != New
        /// </summary>
        public bool CanChangeDriver { get; set; }
    }

    public class DeliveryDriverParameter
    {
        public bool IsDelete { get; set; }
        public int DeliveryId { get; set; }
        public int DriverId { get; set; }
    }
    public class DeliveryOrderParameter
    {
        public int DeliveryId { get; set; }
        public int OrderId { get; set; }
    }

    public class DsDeliveryRefs
    {
        public List<DsDelivery> Deliveries { get; set; } = new List<DsDelivery>();
    }
    public class DsDelivery
    {
        public int TraderId { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public decimal? DriverLongitude { get; set; }
        public decimal? DriverLatitude { get; set; }
        public decimal Total { get; set; }
        public int Status { get; set; }
        public int? DriverDeliveryStatus { get; set; }
        public DateTime? TimeStarted { get; set; }
        public DateTime? TimeFinished { get; set; }
        public string Token { get; set; }
        public string OrderComplete { get; set; }
        public int? EstimateTime { get; set; }
        public int? EstimateDistance { get; set; }
        public string Routes { get; set; }

        public List<Order> Orders { get; set; }
        public string Reference { get; set; }
    }
    public class RoutesModel
    {
        public string Name { get; set; }
        public string Routes { get; set; }
    }

    public class DsDeliveriesRefs
    {
        public List<DsDeliveries> Deliveries { get; set; } = new List<DsDeliveries>();
    }
    public class DsDeliveries
    {
        public int TraderId { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public int? DriverDeliveryStatus { get; set; }
        public string DriverStatus { get; set; }
        public decimal? DriverLongitude { get; set; }
        public decimal? DriverLatitude { get; set; }
        public decimal Total { get; set; }
        public int Status { get; set; }
        public DateTime? TimeStarted { get; set; }
        public DateTime? TimeFinished { get; set; }
        public string Token { get; set; }
        public string OrderComplete { get; set; }
        public int? EstimateTime { get; set; }
        public int? EstimateDistance { get; set; }
        public string Routes { get; set; }

        public List<OrderDelivery> Orders { get; set; }

        public DssLocationModel Location { get; set; }
        public string Reference { get; set; }
    }


    public class DsDeliveryRoutes
    {
        public int TraderId { get; set; }
        public string Routes { get; set; }
    }

    public class DssLocationModel
    {
        public int? DeliveryId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        /// <summary>
        /// Minute: The Time from the Driver location to the Order Location must be calculated
        /// Update to the TradeOrder 
        /// </summary>        
        public int ETA { get; set; } = -1;
    }

    public class DsDeliveryParameter
    {
        public int Id { get; set; }
        public int CompleteOrderId { get; set; }
        public DriverDeliveryStatus Status { get; set; }
        public string DeliveryProblemNote { get; set; }
        public List<OrderStatus> Orders { get; set; }
    }

    public class OrderStatus
    {
        public int Id { get; set; }
        public int DeliverySequence { get; set; }
        public PrepQueueStatus Status { get; set; }//  Completed = 4, CompletedWithProblems = 5
        public string ProblemNote { get; set; }
    }

    public class DeliveryRoutes
    {
        public string route { get; set; }
        /// <summary>
        /// orders positions
        /// </summary>
        public List<RouteDetail> detailed { get; set; }
        public RouteDetail depot { get; set; }
    }
    public class RouteDetail
    {
        public int from { get; set; }
        public int to { get; set; }
        public int time { get; set; }
        public int distance { get; set; }
        public decimal lat { get; set; }
        public decimal lng { get; set; }
    }
}
