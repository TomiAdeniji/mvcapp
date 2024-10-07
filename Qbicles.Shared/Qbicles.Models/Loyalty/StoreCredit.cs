using Qbicles.Models.Trader.Payments;

namespace Qbicles.Models.Loyalty
{
    public class StoreCredit: StoreCreditTransaction
    {
        public StoreCredit()
        {
            this.Type = StoreCreditTransactionType.Credit;
        }

        /// <summary>
        /// This is the SystemSettings that were current at the time the transaction occurred.
        /// This is needed because the System wide conversion factor could change over time
        /// </summary>
        public virtual SystemSettings SystemSettings { get; set; }

        /// <summary>
        /// Store credit for a TraderContact can be generated from a conversion from points
        /// </summary>
        public virtual StorePointTransaction PointTransaction { get; set; }


        /// <summary>
        /// Store credit for a TraderContact can be generated from a conversion from a user's balance which 
        /// requires that a debit note be created and associated with the Trader Contact
        /// </summary>
        public virtual CreditNote DebitNote { get; set; }
    }
}
