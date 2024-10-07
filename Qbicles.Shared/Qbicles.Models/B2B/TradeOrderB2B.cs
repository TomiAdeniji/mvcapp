using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using System.Collections.Generic;

namespace Qbicles.Models.B2B
{

    /// <summary>
    /// This is the TradeOrder with the addedn infomration to convert
    /// VariantIds from the Selling Domain's Product menu to TraderItems and ProductUnits in the Buying Domain
    /// </summary>
    public class TradeOrderB2B : TradeOrder
    {
        /// <summary>
        /// This collection provides the connection between the Trader Item from the Selling menu and the Trader Item in the Buying Domain
        /// </summary>
        public virtual List<B2BTradingItem> TradingItems { get; set; } = new List<B2BTradingItem>();


        /// <summary>
        /// This is the location to which the order is going in the Buying Domain
        /// </summary>
        public virtual TraderLocation DestinationLocation { get; set; }


        // This TraderContact is the TraderContact for the Purchase AS SEEN BY the purchaser
        // i.e. if RoadChef buys from DHL
        // RoadChef will SEE DHL's TraderContcat as the Vendor Contact
        public virtual TraderContact VendorTraderContact { get; set; }

        /// <summary>
        /// The following properties are used to identify the information needed to process the order in the Buying Domain
        /// A Ppurchase is created and all necessary information related to the purchase (Bill, Payments, Transfers) 
        /// muts also be identified.
        /// </summary>
        public virtual TraderPurchase Purchase { get; set; }
        public virtual WorkGroup PurchaseWorkGroup { get; set; }
        public virtual Invoice Bill { get; set; }
        public virtual WorkGroup BillWorkGroup { get; set; }
        public virtual List<CashAccountTransaction> PurchasePayments { get; set; }
        public virtual TraderCashAccount PurchasePaymentAccount { get; set; }
        public virtual WorkGroup PurchasePaymentWorkGroup { get; set; }
        public virtual TraderTransfer PurchaseTransfer { get; set; }
        public virtual WorkGroup PurchaseTransferWorkGroup { get; set; }
    }
}
