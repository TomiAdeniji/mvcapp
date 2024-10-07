using Qbicles.BusinessRules.Model;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader;
using System;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.BusinessRules.Loyalty
{
    public class StoreDebitRules
    {
        private readonly ApplicationDbContext _dbContext;

        public StoreDebitRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }
        /// <summary>
        /// Add Points to the supplied TraderContact based on the Cash and Bank Transaction (Payment) supplied
        /// </summary>
        /// <param name="id">Payment Id</param>
        public void DecreaseStoreCreditFromPaymentApproved(int id)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var payment = _dbContext.CashAccountTransactions.FirstOrDefault(e => e.Id == id && e.Status == TraderPaymentStatusEnum.PaymentApproved);

                    if (payment == null)
                    {
                        LogManager.Warn(MethodBase.GetCurrentMethod(), "The payment is not Approved!", null, id);
                        throw new Exception();
                    }

                    var contact = payment.Contact;

                    var storeCreditAccount = new StoreCreditRules(_dbContext).GetOrCreateStoreCreditAccount(contact);
                    storeCreditAccount.CurrentBalance -= payment.Amount;

                    var storeDebit = new StoreDebit
                    {
                        Payment = payment,
                        Account = _dbContext.StoreCreditAccounts.FirstOrDefault(p => p.Contact.Id == contact.Id),
                        CreatedBy = payment.CreatedBy,
                        Amount = payment.Amount,
                        CreatedDate = DateTime.UtcNow,
                        Reason = StoreCreditTransactionReason.UsedInPayment,
                        Type = StoreCreditTransactionType.Debit
                    };

                    _dbContext.StoreDebits.Add(storeDebit);
                    _dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                    throw ex;
                }
            }
        }
    }
}
