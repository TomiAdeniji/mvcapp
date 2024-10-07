using Qbicles.Models.Trader.PoS;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Net;

namespace Qbicles.BusinessRules.Model
{
    public class QbicleJobResult
    {
        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
        public string JobId { get; set; }
    }
    public class QbicleJobParameter
    {
        /// <summary>
        /// Sale and marketing as campaign post Id
        /// </summary>
        public int Id { get; set; }
        public string JobId { get; set; }
        public double ReminderMinutes { get; set; } = 0;
        public string EndPointName { get; set; }
        public ActivityNotification ActivityNotification { get; set; }
        public DateTimeOffset JobExecuteTime { get; set; } = DateTimeOffset.UtcNow;
    }


    public class IncomingInventoryJobParamenter
    {
        public string EndPointName { get; set; }
        public string UserId { get; set; }
        public int TransferId { get; set; }
        public decimal? OverrideUnitCost { get; set; }
        public List<int> TraderTransferItemIds { get; set; }
        public bool SendToQueue { get; set; } = false;
    }
    public class ManufactureProductJobParamenter
    {
        public string EndPointName { get; set; }
        public string UserId { get; set; }

        public int TraderItemId { get; set; }
        public int ProductUnitId { get; set; }
        public int TraderLocationId { get; set; }
        public decimal ManufacturingQuantity { get; set; }
        public int? WorkGroupId { get; set; }
        public int? ManuJobId { get; set; }
        public int? TraderSaleId { get; set; }
        public bool SendToQueue { get; set; } = false;
    }
    public class OutgoingInventoryJobParamenter
    {
        public string EndPointName { get; set; }
        public string UserId { get; set; }
        public int TransferId { get; set; }
        public List<int> OutgoingItems { get; set; }
        public bool SendToQueue { get; set; } = false;
    }

    public class TraderMovementJobParamenter
    {
        public string EndPointName { get; set; }
        public bool SendToQueue { get; set; }
        public int AlertConstraintId { get; set; }
        public string UserId { get; set; }
    }

    public class CatalogJobParameter
    {
        public string EndPointName { get; set; }
        //List<int> productGroupIds, int catalogId, int locationId, string userId
        public List<int> ProductGroupIds { get; set; } = new List<int>();
        /// <summary>
        /// Menu Id
        /// </summary>
        public int CatalogId { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
        public int NewMenuId { get; set; }
        public int LocationId { get; set; }
        public string UserId { get; set; }
    }


    public class OrderJobParameter
    {
        /// <summary>
        /// Sale and marketing as campaign post Id
        /// </summary>
        public int Id { get; set; }
        public string JobId { get; set; }
        public double ReminderMinutes { get; set; } = 0;
        public string EndPointName { get; set; }
        public ActivityNotification ActivityNotification { get; set; }
        public string Address { get; set; }
        public string InvoiceDetail { get; set; }
    }

    public class ChattingJobParameter
    {
        public ChattingModel Chatting { get; set; }
        public string EndPointName { get; set; }
    }

    public class PosProcessLogParameter
    {
        public PosRequest Request { get; set; }
        public PosApiRequestLog LogInput { get; set; }
        public string EndPointName { get; set; }
    }

    public class PosProcessActiveOrderParameter
    {
        /// <summary>
        /// As User Id
        /// </summary>
        public string DriverId { get; set; }
        public string LinkedOrderId { get; set; }
        public DssLocationModel Location { get; set; }
        public string EndPointName { get; set; }
    }

    public class TradeOrderLoggingParameter
    {
        public TradeOrderLoggingType Type { get; set; }
        public int TradeId { get; set; }
        public string CreatedById { get; set; }
        public List<int> QueueOrderIds { get; set; }
        public string EndPointName { get; set; }
        public string Message { get; set; }
        public bool IsCutomer { get; set; }
    }

    public enum TradeOrderLoggingType
    {
        SaleAdd = 1,
        SaleApproval,
        PurchaseAdd,
        PurchaseApproval,
        InvoiceAdd,
        InvoiceApproval,
        PaymentAdd,
        PaymentApproval,
        TransferAdd,
        TransferApproval,
        Preparation,
        DeliveryStatus,
        DriverSendMessage,
    }

    public class PosSendToPrepParameter
    {
        public PosRequest Request { get; set; }
        public Order SaleOrder { get; set; }
        public string LinkedOrderId { get; set; }
        public string EndPointName { get; set; }
    }
}
