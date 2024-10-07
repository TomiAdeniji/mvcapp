using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Loyalty
{
    public class StorePointRules
    {
        private readonly ApplicationDbContext _dbContext;

        public StorePointRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// Caculate Points from the Payment
        /// </summary>
        /// <param name="conversion">PaymentConversion</param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private int CalculateFromPayment(PaymentConversion conversion, decimal amount)
        {
            return (int)Math.Round((conversion.AmountConversionFactor * amount), MidpointRounding.AwayFromZero);
        }

        private TradeOrder GetTradeOrder(CashAccountTransaction payment)
        {
            return _dbContext.TradeOrders.FirstOrDefault(p => p.Payments.Select(i => i.Id).Contains(payment.Id));            
        }

        /// <summary>
        /// Get StorePointAccount by contact
        /// If null, create a new Account
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public StorePointAccount GetOrCreateStorePointAccount(TraderContact contact, string userId)
        {

            // Search first
            var storePointAccount = _dbContext.StorePointAccounts.FirstOrDefault(p => p.Contact.Id == contact.Id);
            if (storePointAccount != null)
                return storePointAccount;

            // If no account found, create a new one
            var user = _dbContext.QbicleUser.Find(userId);
            storePointAccount = new StorePointAccount()
            {
                CurrentBalance = 0,
                Contact = contact,
                Transactions = new List<StorePointTransaction>(),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = user
            };
            _dbContext.StorePointAccounts.Add(storePointAccount);
            _dbContext.Entry(storePointAccount).State = EntityState.Added;
            _dbContext.SaveChanges();

            return storePointAccount;
        }


        public ReturnJsonModel MergePointTransactionToPointAccount(StorePointAccount account, StorePointTransaction transaction)
        {
            var rs = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, account, transaction);

                if (transaction.Type == StorePointTransactionType.Debit)
                {
                    if (account.CurrentBalance < transaction.Points)
                    {
                        rs.result = false;
                        rs.msg = ResourcesManager._L("ERROR_MSG_NOTENOUGHSTOREPOINT");
                        return rs;
                    }
                    account.CurrentBalance -= transaction.Points;
                }
                else //(transaction.Type == StorePointTransactionType.Credit)
                    account.CurrentBalance += transaction.Points;
               
                rs.result = true;
                return rs;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, account, transaction);
                rs.result = false;
                rs.msg = ResourcesManager._L("ERROR_MSG_5");
                return rs;
            }
        }



        /// <summary>
        /// Add Points to the supplied TraderContact based on the Cash and Bank Transaction (Payment) supplied
        /// </summary>
        /// <param name="id">Payment Id</param>
        public void GenerateStorePoinFromPaymentApproved(int id)
        {
            try
            {
                var payment = _dbContext.CashAccountTransactions.FirstOrDefault(e => e.Id == id && e.Status == TraderPaymentStatusEnum.PaymentApproved);

                if (payment == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), "The payment is not Approved!",null, id);
                    throw new Exception();
                }

                var contact = payment.Contact;

                // Get the Domain associated with the payment
                var paymentDomain = new QbicleDomain();
                if (payment.DestinationAccount?.Domain != null)
                    paymentDomain = payment.DestinationAccount.Domain;
                else if (payment.OriginatingAccount?.Domain != null)
                    paymentDomain = payment.OriginatingAccount.Domain;

                // Get the Domain associated with the contact
                var contactDomain = contact.ContactGroup.Domain;
                // Verify that they are the same Domain or throw an error
                if (paymentDomain != contactDomain)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), "The Domain of the contact is different from the domain of the payment", null, payment, contact);
                    throw new Exception();
                }

                // Check if the Payment Method for the Payment is StoreCredit
                // If it is then NO POINTS ARE GENERATED
                if (payment.PaymentMethod != null && payment.PaymentMethod.Name == PaymentMethodNameConst.StoreCredit)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), "The Payment Method for the Payment is StoreCredit then NO POINTS ARE GENERATED", null, payment, contact);
                    return;
                }

                // Get the TradeOrder associated with the payment if one exists
                var tradeOrder = GetTradeOrder(payment);

                // Get the StorePointAccount for the Contact
                // if one does not exist, create it with Current balance = 0
                var user = payment.PaymentApprovalProcess?.ApprovedOrDeniedAppBy ?? payment.CreatedBy;
                var account = GetOrCreateStorePointAccount(contact, user.Id);

                // Get the PaymentConversions associated with the Domain
                // where the PaymentConversioins are NOT archived
                // Apply each PaymentConversion to the Payment
                var conversion = new StorePointConversionRules(_dbContext).GetActiveConversion(paymentDomain.Id, OrderToPointsConversionType.Payment);
                if (conversion == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), "No active Payment Conversion found", null, payment, contact);
                    return;
                }


               var points = CalculateFromPayment((PaymentConversion)conversion, payment.Amount);

                // Create a Store Point Transaction
                var transaction = new StorePointTransaction
                {
                    CreatedDate = DateTime.UtcNow,
                    Order = tradeOrder,
                    Payment = payment,
                    Type = StorePointTransactionType.Credit,
                    Reason = StorePointTransactionReason.GeneratedFromPayment,
                    Points = points,
                    Account = account,
                    ConversionUsed = conversion,
                    CreatedBy = user
                };
                _dbContext.StorePointTransactions.Add(transaction);
                _dbContext.Entry(transaction).State = EntityState.Added;

                MergePointTransactionToPointAccount(account, transaction);

                //Create a Store Point Transaction Log
                var transactionLog = new StorePointTransactionLog
                {
                    Account = account,
                    AssociatedStorePointTransaction = transaction,
                    ConversionUsed = conversion,
                    CreatedDate = DateTime.UtcNow,
                    Order = tradeOrder,
                    Payment = payment,
                    Points = points,
                    Reason = StorePointTransactionReason.GeneratedFromPayment,
                    Type = StorePointTransactionType.Credit,
                    CreatedBy = user
                };
                _dbContext.StorePointTransactionLogs.Add(transactionLog);
                _dbContext.Entry(transactionLog).State = EntityState.Added;
                _dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }

        }


        /// <summary>
        /// This call will return a list of the StorePointTransactions associated with the account
        /// The initial order must be by Id descending i.e. newest first
        /// This method will be used to implement the UseCase: Get StorePoints History
        /// It will have to be adapted to handle server side pagination and filtering
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public List<StorePointTransaction> GetTransactions(StorePointAccount account)
        {
            return null;
        }


    }
}
