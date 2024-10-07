using System;

namespace Qbicles.Models.Trader.CashMgt
{
    public class SafeTransaction
    {
        public int Id { get; set; }

        public int AssociatedTillId { get; set; } = 0;

        public int AssociatedSafeId { get; set; } = 0;

        public DateTime TransactionDate { get; set; }

        public string TransactionDateString { get; set; }

        public string DeviceName { get; set; } = "";

        public string TillName { get; set; } = "";

        public string SafeName { get; set; } = "";

        public string DirectionName { get; set; } = "";

        public string Amount { get; set; } = "";

        public decimal AmountNumber { get; set; } = 0;

        public string Balance { get; set; } = "";

        public decimal BalanceNumber { get; set; } = 0;

        public string Difference { get; set; } = "";

        public decimal DifferenceNumber { get; set; } = 0;

        public string Status { get; set; } = "";

        public string LabelStatus { get; set; } = "";

        public bool isCheckpoint { get; set; } = false;

        public bool isTillPayment { get; set; } = false;

        public bool isTransfer { get; set; } = false;
    }
}
