using Qbicles.Models.Trader;

namespace Qbicles.Models.Loyalty
{
    public class StoreDebit: StoreCreditTransaction
    {
        public StoreDebit()
        {
            this.Type = StoreCreditTransactionType.Debit;
        }


        /// <summary>
        /// Store credit for a TraderContact can be reduced by a user making a payment
        /// </summary>
        public virtual CashAccountTransaction Payment { get; set; }


        /// <summary>
        /// WE WILL LEAVE THIS OUT FOR NOW
        /// Store credit can be reduced by converting store credit back to a balance using a Credit note
        /// </summary>
        //public virtual CreditNote CreditNote { get; set; }
    }
}
