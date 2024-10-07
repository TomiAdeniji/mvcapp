using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.BusinessRules.Commerce;
using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.CashMgt;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class ProcessOrderRules
    {
        private string _currentTimeZone = "";

        private ApplicationUser _traderCreatedAndApprovalBy;
        private readonly ApplicationDbContext dbContext;

        private System.Dynamic.ExpandoObject _logLabels;

        public ProcessOrderRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        ///     Process Order for POS,B2C..
        /// </summary>
        /// <param name="job"></param>
        public async Task ProcessOrderAsync(OrderJobParameter job)
        {
            try
            {
                if (job.ActivityNotification != null && job.ActivityNotification.DiscussionId != 0 && job.ActivityNotification.QbicleId != 0)
                    CreateB2cDicussionOrderSendMessage(job.ActivityNotification);

                var tradeOrderId = job.Id;
                var createdProcess = new ReturnJsonModel();
                var tradeOrder = dbContext.TradeOrders.Find(tradeOrderId);

                //Get the reference from the TradeOrder and add to the log labels
                dynamic logLabels = new System.Dynamic.ExpandoObject();
                logLabels.OrderProcess = QbicleProcessEnum.OrderProcessing;
                logLabels.OrderRef = tradeOrder.OrderReference.FullRef;
                logLabels.OrderId = tradeOrderId;
                logLabels.SalesChannel = tradeOrder.SalesChannel;
                _logLabels = logLabels;
                LogManager.ApplicationInfo(logLabels, "Starting order processing");

                // Check to see if there is a discussion associated with the trade order
                // If not, create one based on the TradeOrder.Customer
                // If the TradeOrder.Customer is not a user in the system, send them an email invitation
                CreateB2CDiscussionForPos(tradeOrder);

                // Need to ensure that ALL Relationship Managers for the B2C Qbicles  from which the Order is created are set up as Reviewers and Approvers.
                CheckAndSetupRelationshipManagers(tradeOrder);

                // Set the cashier forth eorder as the Creator of and Approver for the approvals
                // Also set the timezone based on the cashier's timezone
                SetupApprovalCreatorApprover(tradeOrder);

                // Check if the TradeOrder already has an associated Sale
                TraderSale traderSaleNew = null;
                if (tradeOrder.OrderProblem != TradeOrderProblemEnum.CreateSale)
                {
                    traderSaleNew = tradeOrder.Sale;

                    if (traderSaleNew != null)
                    {
                        LogManager.ApplicationInfo(_logLabels, "TradeOrder already has a sale", null, null,
                                                                  "SaleRef: " + traderSaleNew.Reference.FullRef);
                    }
                }

                //  Need the check if existed sale,PosQueueOrder,invoice,transfer.... then canot create again
                // Create a Trader Sale
                if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateSale || tradeOrder.Sale == null)
                {
                    if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateSale)
                    {
                        LogManager.ApplicationInfo(_logLabels, "Problem with existing sale", null, null,
                                                             "SaleRef: " + traderSaleNew.Reference.FullRef,
                                                             "Problem: " + tradeOrder.ProcessedProblems);
                    }
                    else
                    {
                        LogManager.ApplicationInfo(_logLabels, "Creating a new sale");
                    }

                    // A TraderSale and its associated Approval are created for the TradeOrder
                    createdProcess = CreateTraderSale(tradeOrder);
                    if (!createdProcess.result)
                    {
                        tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                        tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateSale;
                        tradeOrder.ProcessedProblems =
                            $"Failed when creating the Sale with error message {createdProcess.msg}";
                        dbContext.SaveChanges();

                        LogManager.ApplicationInfo(_logLabels, "Problem with creating sale", null, null,
                                                             "Problem: " + tradeOrder.ProcessedProblems);

                        return;
                    }

                    traderSaleNew = (TraderSale)createdProcess.Object;

                    // Create  Queue Order for all POS and B2C
                    switch (tradeOrder.SalesChannel)
                    {
                        case SalesChannelEnum.POS:
                        case SalesChannelEnum.B2C:
                            var creatingQueueOrder = CreateQueueOrder(tradeOrder, traderSaleNew);
                            if (!creatingQueueOrder.result)
                            {
                                tradeOrder.Sale = null;
                                tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                                tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePosQueueOrder;
                                tradeOrder.ProcessedProblems =
                                    $"Failed when creating Queue Order with error message {createdProcess.msg}";
                                dbContext.SaveChanges();

                                LogManager.ApplicationInfo(_logLabels, "Problem with creating queue order for sale", null, null,
                                                             "Problem: " + tradeOrder.ProcessedProblems);

                                return;
                            }

                            tradeOrder.PrepDeliveryOrder = (QueueOrder)creatingQueueOrder.Object;
                            dbContext.SaveChanges();
                            break;

                        case SalesChannelEnum.Trader:

                        default:
                            break;
                    }
                }
                else if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreatePosQueueOrder && tradeOrder.Sale != null)
                {
                    switch (tradeOrder.SalesChannel)
                    {
                        case SalesChannelEnum.POS:
                        case SalesChannelEnum.B2C:
                            var creatingQueueOrder = CreateQueueOrder(tradeOrder, traderSaleNew);
                            if (!creatingQueueOrder.result)
                            {
                                tradeOrder.Sale = null;
                                tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                                tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePosQueueOrder;
                                tradeOrder.ProcessedProblems =
                                    $"Failed when creating Queue Order with error message {createdProcess.msg}";
                                dbContext.SaveChanges();

                                LogManager.ApplicationInfo(_logLabels, "Problem with creating queue order for sale", null, null,
                                                             "Problem: " + tradeOrder.ProcessedProblems);

                                return;
                            }

                            break;

                        case SalesChannelEnum.Trader:

                        default:
                            break;
                    }
                }

                // Create an Invoice for the Sale and Create a payment for the Invoice
                if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateInvoice || tradeOrder.Invoice == null)
                {
                    createdProcess = await CreateInvoiceAsync(tradeOrder, traderSaleNew, job.Address, job.InvoiceDetail);
                    if (!createdProcess.result)
                    {
                        tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                        if (createdProcess.actionVal == 1)
                            tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateInvoice;
                        else if (createdProcess.actionVal == 2)
                            tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePayment;
                        tradeOrder.ProcessedProblems =
                            $"Failed when creating the Invoice/Payment with error message {createdProcess.msg}";
                        tradeOrder.Invoice = null;
                        dbContext.SaveChanges();

                        LogManager.ApplicationInfo(_logLabels, "Problem with creating invoice/payment", null, null,
                                                               "Problem: " + tradeOrder.ProcessedProblems);

                        return;
                    }
                }
                else if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreatePayment)
                {
                    var order = GetOrderFromTradeOrder(tradeOrder);

                    var createdPayment = CreatePayment(tradeOrder, tradeOrder.Invoice, traderSaleNew, order,
                        tradeOrder.PosDevice?.MethodAccount);
                    if (!createdPayment.result)
                    {
                        tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePayment;
                        tradeOrder.ProcessedProblems =
                            $"Failed when creating the Invoice with error message {createdProcess.msg}";
                        dbContext.SaveChanges();
                        return;
                    }
                }

                //Transfer process
                if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateTransfer || tradeOrder.Transfer == null)
                {
                    createdProcess = await CreateTransfer(tradeOrder, traderSaleNew);
                    if (!createdProcess.result)
                    {
                        tradeOrder.Transfer = null;
                        tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                        tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateTransfer;
                        tradeOrder.ProcessedProblems =
                            $"Failed when creating the Transfer with error message {createdProcess.msg}";
                        dbContext.SaveChanges();
                        return;
                    }
                }

                tradeOrder.OrderStatus = TradeOrderStatusEnum.Processed;
                tradeOrder.OrderProblem = TradeOrderProblemEnum.Non;
                dbContext.SaveChanges();
                //TODO QBIC-4161 - send notification alert order completed
                new NotificationRules(dbContext).SignalRB2COrderProcess(tradeOrder, "", NotificationEventEnum.B2COrderCompleted);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
            }
        }

        /// <summary>
        /// Set the cashier forth eorder as the Creator of and Approver for the approvals
        /// Also set the timezone based on the cashier's timezone
        /// </summary>
        /// <param name="tradeOrder"></param>
        private void SetupApprovalCreatorApprover(TradeOrder tradeOrder)
        {
            try
            {
                var cashierId = GetOrderFromTradeOrder(tradeOrder).Cashier.TraderId;
                _traderCreatedAndApprovalBy = new UserRules(dbContext).GetById(cashierId);
                _currentTimeZone = _traderCreatedAndApprovalBy.Timezone;
                LogManager.ApplicationInfo(_logLabels, "ApprovalCreatorApprover", null, null,
                                                  "User Id: " + _traderCreatedAndApprovalBy.Id,
                                                  "User Timezone" + _currentTimeZone);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
            }
        }

        private void CheckAndSetupRelationshipManagers(TradeOrder tradeOrder)
        {
            try
            {
                //need to ensure that ALL Relationship Managers for the B2C Qbicles  from which the Order is created are set up as Reviewers and Approvers.
                var profile = dbContext.B2BProfiles.FirstOrDefault(p => p.Domain.Id == tradeOrder.SaleWorkGroup.Domain.Id);
                profile.DefaultB2CRelationshipManagers.ForEach(user =>
                {
                    if (tradeOrder.SaleWorkGroup != null)
                    {
                        if (tradeOrder.SaleWorkGroup.Members.All(u => u.Id != user.Id))
                            tradeOrder.SaleWorkGroup.Members.Add(user);
                        if (tradeOrder.SaleWorkGroup.Approvers.All(u => u.Id != user.Id))
                            tradeOrder.SaleWorkGroup.Approvers.Add(user);
                        if (tradeOrder.SaleWorkGroup.Reviewers.All(u => u.Id != user.Id))
                            tradeOrder.SaleWorkGroup.Reviewers.Add(user);
                    }

                    if (tradeOrder.InvoiceWorkGroup != null)
                    {
                        if (tradeOrder.InvoiceWorkGroup.Members.All(u => u.Id != user.Id))
                            tradeOrder.InvoiceWorkGroup.Members.Add(user);
                        if (!tradeOrder.InvoiceWorkGroup.Approvers.Any(u => u.Id == user.Id))
                            tradeOrder.InvoiceWorkGroup.Approvers.Add(user);
                        if (!tradeOrder.InvoiceWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                            tradeOrder.InvoiceWorkGroup.Reviewers.Add(user);
                    }

                    if (tradeOrder.PaymentWorkGroup != null)
                    {
                        if (!tradeOrder.PaymentWorkGroup.Members.Any(u => u.Id == user.Id))
                            tradeOrder.PaymentWorkGroup.Members.Add(user);
                        if (!tradeOrder.PaymentWorkGroup.Approvers.Any(u => u.Id == user.Id))
                            tradeOrder.PaymentWorkGroup.Approvers.Add(user);
                        if (!tradeOrder.PaymentWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                            tradeOrder.PaymentWorkGroup.Reviewers.Add(user);
                    }

                    if (tradeOrder.TransferWorkGroup != null)
                    {
                        if (!tradeOrder.TransferWorkGroup.Members.Any(u => u.Id == user.Id))
                            tradeOrder.TransferWorkGroup.Members.Add(user);
                        if (!tradeOrder.TransferWorkGroup.Approvers.Any(u => u.Id == user.Id))
                            tradeOrder.TransferWorkGroup.Approvers.Add(user);
                        if (!tradeOrder.TransferWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                            tradeOrder.TransferWorkGroup.Reviewers.Add(user);
                    }
                });

                dbContext.SaveChanges();

                LogManager.ApplicationInfo(_logLabels, "Approvers and reviewers setup");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
            }
        }

        /// <summary>
        ///     Process Order from B2B
        /// </summary>
        /// <param name="job"></param>
        public async Task ProcessOrderForB2BAsync(OrderJobParameter job)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, job);

                var tradeId = job.Id;
                var tradeOrder = dbContext.B2BTradeOrders.Find(tradeId);

                await ProcessOrderWithProviderDomainAsync(tradeOrder);
                await ProcessOrderWithConsumerDomain(tradeOrder);
                if (tradeOrder.OrderProblem == TradeOrderProblemEnum.Non)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.Processed;
                }
                dbContext.SaveChanges();
                // need push notification to notice use know the procees has been done.
                new NotificationRules(dbContext).SignalRB2COrderProcess(tradeOrder, "", NotificationEventEnum.B2BOrderCompleted);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }
        }

        private async Task ProcessOrderWithProviderDomainAsync(TradeOrderB2B tradeOrder)
        {
            var createdProcess = new ReturnJsonModel { result = false };
            //need to ensure that ALL Relationship Managers for the B2B Qbicles  from which the Order is created are set up as Reviewers and Approvers.
            var providerProfile = dbContext.B2BProfiles.FirstOrDefault(p => p.Domain.Id == tradeOrder.SellingDomain.Id);
            providerProfile.DefaultB2BRelationshipManagers.ForEach(user =>
            {
                if (tradeOrder.SaleWorkGroup != null)
                {
                    if (!tradeOrder.SaleWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.SaleWorkGroup.Members.Add(user);
                    if (!tradeOrder.SaleWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.SaleWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.SaleWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.SaleWorkGroup.Reviewers.Add(user);
                }

                if (tradeOrder.InvoiceWorkGroup != null)
                {
                    if (!tradeOrder.InvoiceWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.InvoiceWorkGroup.Members.Add(user);
                    if (!tradeOrder.InvoiceWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.InvoiceWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.InvoiceWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.InvoiceWorkGroup.Reviewers.Add(user);
                }

                if (tradeOrder.PaymentWorkGroup != null)
                {
                    if (!tradeOrder.PaymentWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.PaymentWorkGroup.Members.Add(user);
                    if (!tradeOrder.PaymentWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.PaymentWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.PaymentWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.PaymentWorkGroup.Reviewers.Add(user);
                }

                if (tradeOrder.TransferWorkGroup != null)
                {
                    if (!tradeOrder.TransferWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.TransferWorkGroup.Members.Add(user);
                    if (!tradeOrder.TransferWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.TransferWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.TransferWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.TransferWorkGroup.Reviewers.Add(user);
                }
            });

            //Get Cashier to Created and Approval
            var cashierId = GetOrderFromTradeOrder(tradeOrder).Cashier.TraderId;
            _traderCreatedAndApprovalBy = new UserRules(dbContext).GetById(cashierId);
            _currentTimeZone = _traderCreatedAndApprovalBy.Timezone;

            #region Create a Trader Sale

            if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateSale || tradeOrder.Sale == null)
            {
                createdProcess = CreateTraderSale(tradeOrder);
                if (!createdProcess.result)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                    tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateSale;
                    tradeOrder.ProcessedProblems =
                        $"Failed when creating the Sale with error message {createdProcess.msg}";
                    dbContext.SaveChanges();
                    return;
                }

                var traderSaleNew = (TraderSale)createdProcess.Object;
                // Create  Queue Order for B2B
                switch (tradeOrder.SalesChannel)
                {
                    case SalesChannelEnum.B2B:
                        var creatingQueueOrder = CreateQueueOrder(tradeOrder, traderSaleNew);
                        if (!creatingQueueOrder.result)
                        {
                            tradeOrder.Sale = null;
                            tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                            tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePosQueueOrder;
                            tradeOrder.ProcessedProblems =
                                $"Failed when creating Queue Order with error message {createdProcess.msg}";
                            dbContext.SaveChanges();
                            return;
                        }

                        break;
                }
            }
            else if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreatePosQueueOrder && tradeOrder.Sale != null)
            {
                switch (tradeOrder.SalesChannel)
                {
                    case SalesChannelEnum.B2B:
                        var creatingQueueOrder = CreateQueueOrder(tradeOrder, tradeOrder.Sale);
                        if (!creatingQueueOrder.result)
                        {
                            tradeOrder.Sale = null;
                            tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                            tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePosQueueOrder;
                            tradeOrder.ProcessedProblems =
                                $"Failed when creating Queue Order with error message {createdProcess.msg}";
                            dbContext.SaveChanges();
                            return;
                        }

                        tradeOrder.PrepDeliveryOrder = (QueueOrder)creatingQueueOrder.Object;
                        break;
                }
            }

            #endregion Create a Trader Sale

            #region Create an Invoice for the Sale and Create an payment for the Invoice

            // Create an Invoice for the Sale and Create an payment for the Invoice
            if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateInvoice || tradeOrder.Invoice == null)
            {
                createdProcess = await CreateInvoiceAsync(tradeOrder, tradeOrder.Sale);
                if (!createdProcess.result)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                    if (createdProcess.actionVal == 1)
                        tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateInvoice;
                    else if (createdProcess.actionVal == 2)
                        tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePayment;
                    tradeOrder.ProcessedProblems =
                        $"Failed when create the Invoice with error message {createdProcess.msg}";
                    tradeOrder.Invoice = null;
                    dbContext.SaveChanges();
                    return;
                }
            }
            else if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreatePayment)
            {
                var order = GetOrderFromTradeOrder(tradeOrder);

                //var createdPayment = CreatePayment(tradeOrder, tradeOrder.Invoice, tradeOrder.Sale, tradeOrder.OrderJson.ParseAs<Order>(),
                var createdPayment = CreatePayment(tradeOrder, tradeOrder.Invoice, tradeOrder.Sale, order,
                    tradeOrder.PosDevice?.MethodAccount);
                if (!createdPayment.result)
                {
                    tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePayment;
                    tradeOrder.ProcessedProblems =
                        $"Failed when creating the Invoice with error message {createdProcess.msg}";
                    dbContext.SaveChanges();
                    return;
                }
            }

            #endregion Create an Invoice for the Sale and Create an payment for the Invoice

            #region Transfer process

            if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateTransfer || tradeOrder.Transfer == null)
            {
                createdProcess = await CreateTransfer(tradeOrder, tradeOrder.Sale);
                if (!createdProcess.result)
                {
                    tradeOrder.Transfer = null;
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                    tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateTransfer;
                    tradeOrder.ProcessedProblems =
                        $"Failed when creating the Transfer with error message {createdProcess.msg}";
                    dbContext.SaveChanges();
                }
            }

            #endregion Transfer process
        }

        private async Task ProcessOrderWithConsumerDomain(TradeOrderB2B tradeOrder)
        {
            var createdProcess = new ReturnJsonModel { result = false };
            //need to ensure that ALL Relationship Managers for the B2B Qbicles  from which the Order is created are set up as Reviewers and Approvers.
            var consumerProfile = dbContext.B2BProfiles.FirstOrDefault(p => p.Domain.Id == tradeOrder.BuyingDomain.Id);
            consumerProfile.DefaultB2BRelationshipManagers.ForEach(user =>
            {
                if (tradeOrder.PurchaseWorkGroup != null)
                {
                    if (!tradeOrder.PurchaseWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.PurchaseWorkGroup.Members.Add(user);
                    if (!tradeOrder.PurchaseWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.PurchaseWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.PurchaseWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.PurchaseWorkGroup.Reviewers.Add(user);
                }

                if (tradeOrder.BillWorkGroup != null)
                {
                    if (!tradeOrder.BillWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.BillWorkGroup.Members.Add(user);
                    if (!tradeOrder.BillWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.BillWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.BillWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.BillWorkGroup.Reviewers.Add(user);
                }

                if (tradeOrder.PurchasePaymentWorkGroup != null)
                {
                    if (!tradeOrder.PurchasePaymentWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.PurchasePaymentWorkGroup.Members.Add(user);
                    if (!tradeOrder.PurchasePaymentWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.PurchasePaymentWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.PurchasePaymentWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.PurchasePaymentWorkGroup.Reviewers.Add(user);
                }

                if (tradeOrder.PurchaseTransferWorkGroup != null)
                {
                    if (!tradeOrder.PurchaseTransferWorkGroup.Members.Any(u => u.Id == user.Id))
                        tradeOrder.PurchaseTransferWorkGroup.Members.Add(user);
                    if (!tradeOrder.PurchaseTransferWorkGroup.Approvers.Any(u => u.Id == user.Id))
                        tradeOrder.PurchaseTransferWorkGroup.Approvers.Add(user);
                    if (!tradeOrder.PurchaseTransferWorkGroup.Reviewers.Any(u => u.Id == user.Id))
                        tradeOrder.PurchaseTransferWorkGroup.Reviewers.Add(user);
                }
            });
            var convertedCurrency = new ExchangeRateRules(dbContext).GetExchangeRateByOrderId(tradeOrder.Id);

            var order = GetOrderFromTradeOrder(tradeOrder);

            //Get Consumer user to Created and Approval
            _traderCreatedAndApprovalBy = tradeOrder.Customer;
            _currentTimeZone = _traderCreatedAndApprovalBy.Timezone;

            #region Create a Trader Purchase

            if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreatePurchase || tradeOrder.Purchase == null)
            {
                createdProcess = CreateTraderPurchase(tradeOrder, order, convertedCurrency);
                if (!createdProcess.result)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                    tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePurchase;
                    tradeOrder.ProcessedProblems =
                        $"Failed when creating the Purchase with error message {createdProcess.msg}";
                    dbContext.SaveChanges();
                    return;
                }
            }

            #endregion Create a Trader Purchase

            #region Create a Bill

            if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreateBill || tradeOrder.Bill == null)
            {
                createdProcess = CreateBill(tradeOrder, order);
                if (!createdProcess.result)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                    tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateBill;
                    tradeOrder.ProcessedProblems =
                        $"Failed when creating the Bill with error message {createdProcess.msg}";
                    dbContext.SaveChanges();
                    return;
                }
            }

            #endregion Create a Bill

            #region create a Payment

            if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreatePurchasePayment ||
                !tradeOrder.PurchasePayments.Any())
            {
                createdProcess = PurchasePaymentB2B(tradeOrder, order, convertedCurrency);
                if (!createdProcess.result)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                    tradeOrder.OrderProblem = TradeOrderProblemEnum.CreateBill;
                    tradeOrder.ProcessedProblems =
                        $"Failed when creating the Payment with error message {createdProcess.msg}";
                    dbContext.SaveChanges();
                    return;
                }
            }

            #endregion create a Payment

            #region Create a Transfer

            if (tradeOrder.OrderProblem == TradeOrderProblemEnum.CreatePurchaseTransfer ||
                tradeOrder.PurchaseTransfer == null)
            {
                createdProcess = await CreatePurchaseTransfer(tradeOrder);
                if (!createdProcess.result)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.ProcessedWithProblems;
                    tradeOrder.OrderProblem = TradeOrderProblemEnum.CreatePurchaseTransfer;
                    tradeOrder.ProcessedProblems =
                        $"Failed when creating the Transfer with error message {createdProcess.msg}";
                    dbContext.SaveChanges();
                }
            }

            #endregion Create a Transfer
        }

        private ReturnJsonModel CreateQueueOrder(TradeOrder tradeOrder, TraderSale traderSale)
        {
            try
            {
                var refModel = new ReturnJsonModel { result = true };
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrder);

                var saleOrder = GetOrderFromTradeOrder(tradeOrder);

                var orderProcessingHelper = new OrderProcessingHelper(dbContext);

                switch (tradeOrder.SalesChannel)
                {
                    case SalesChannelEnum.POS:
                        // Check that we can get a Till associated with the device
                        var associatedTill = new CMsRules(dbContext).GetTillByPosDevice(tradeOrder.PosDevice.Id);
                        if (associatedTill == null)
                            return new ReturnJsonModel
                            {
                                result = false,
                                msg =
                                    $"Create Pos QueueOrder Failed: POS Device {tradeOrder.PosDevice.Name}-{tradeOrder.PosDevice.SerialNumber} required link to Till default.",
                                actionVal = 1
                            };
                        /*
                        QueueOrder from the POS is for Payment only
                        If the POS is working correctly 'Send to prep' is what puts QueueOrders on the PrepQueue
                        In OrderProcessing the QueueOrder for POS is really only to ensure payment is recorded, nor for prep
                        */
                        var prepQueueStatuPos =
                            dbContext.PosSettings.FirstOrDefault(e => e.Location.Id == tradeOrder.Location.Id)
                                ?.OrderStatusWhenAddedToQueue ?? PrepQueueStatus.NotStarted;

                        var queueOrder = orderProcessingHelper.CreateQueueOrder(saleOrder, tradeOrder.OrderCustomer,
                            _traderCreatedAndApprovalBy, prepQueueStatuPos, true, traderSale, null);

                        //Get AssociatedPrepOrders and set them paid
                        var associatedPrepOrders = dbContext.QueueOrders
                            .Where(q => q.LinkedOrderId == saleOrder.LinkedTraderId).ToList();

                        associatedPrepOrders.ForEach(qOrder => { qOrder.IsPaid = true; });

                        var poRef = new PosDeviceOrderXref
                        {
                            PosDevice = tradeOrder.PosDevice,
                            Order = queueOrder,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = _traderCreatedAndApprovalBy,
                            Till = associatedTill
                        };
                        queueOrder.PosDeviceOrder = poRef;
                        dbContext.Entry(queueOrder).State = EntityState.Modified;
                        refModel.Object = queueOrder;
                        break;

                    case SalesChannelEnum.B2C:
                        //if the PrepQueue isn't available for the Location, it must be created
                        var prepQueue = new PDSRules(dbContext).GetOrCreatePrepQueue(tradeOrder.Location.Id,
                            _traderCreatedAndApprovalBy);

                        var prepQueueStatusB2C =
                            dbContext.B2CSettings.FirstOrDefault(e => e.Location.Id == tradeOrder.Location.Id)
                                ?.OrderStatusWhenAddedToQueue ?? PrepQueueStatus.Completed;

                        refModel.Object = orderProcessingHelper.CreateQueueOrder(saleOrder, tradeOrder.OrderCustomer,
                            _traderCreatedAndApprovalBy, prepQueueStatusB2C, false, traderSale, prepQueue);
                        break;

                    case SalesChannelEnum.B2B:
                        //if the PrepQueue isn't available for the Location, it must be created
                        var prepb2BQueue =
                            new PDSRules(dbContext).GetOrCreatePrepQueue(tradeOrder.Location.Id,
                                _traderCreatedAndApprovalBy);

                        var prepQueueStatusB2B =
                            dbContext.B2BSettings.FirstOrDefault(e => e.Location.Id == tradeOrder.Location.Id)
                                ?.OrderStatusWhenAddedToQueue ?? PrepQueueStatus.Completed;

                        refModel.Object = orderProcessingHelper.CreateQueueOrder(saleOrder, tradeOrder.OrderCustomer,
                            _traderCreatedAndApprovalBy, prepQueueStatusB2B, true, traderSale, prepb2BQueue);
                        break;

                    case SalesChannelEnum.Trader:
                    //case SalesChannelEnum.Community:
                    default:
                        break;
                }

                tradeOrder.PrepDeliveryOrder = (QueueOrder)refModel.Object;
                dbContext.SaveChanges();
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrder);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = $"Create Pos QueueOrder Failed, message detail: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// A TraderSale and its associated Approval are created for the TradeOrder
        /// </summary>
        /// <param name="tradeOrder"></param>
        /// <returns></returns>
        private ReturnJsonModel CreateTraderSale(TradeOrder tradeOrder)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrder);

                    var saleOrder = GetOrderFromTradeOrder(tradeOrder);

                    var voucher = dbContext.Vouchers.FirstOrDefault(e => e.Id == saleOrder.VoucherId);
                    if (voucher != null)
                    {
                        voucher.RedeemedDate = DateTime.UtcNow;
                        voucher.RedeemedLocation = tradeOrder.Location;
                        voucher.IsRedeemed = true;
                    }

                    //timezone format
                    var timeZoneCreatedDateTime = saleOrder.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ??
                                                  DateTime.UtcNow;

                    var traderSaleNew = new TraderSale
                    {
                        Location = tradeOrder.Location,
                        SaleTotal = saleOrder.AmountInclTax,
                        CreatedDate = timeZoneCreatedDateTime,
                        CreatedBy = _traderCreatedAndApprovalBy,
                        DeliveryMethod = tradeOrder.DeliveryMethod,
                        SalesChannel = tradeOrder.SalesChannel,
                        Status = TraderSaleStatusEnum.SaleApproved,
                        SaleItems = new List<TraderTransactionItem>(),
                        Workgroup = tradeOrder.SaleWorkGroup,
                        Purchaser = tradeOrder.TraderContact,
                        DeliveryAddress = tradeOrder.DeliveryMethod != DeliveryMethodEnum.Delivery
                            ? null
                            : tradeOrder.TraderContact.Address,
                        Voucher = voucher
                    };

                    traderSaleNew.Purchaser.InUsed = true;

                    // Get Sales Reference
                    traderSaleNew.Reference =
                        new TraderReferenceRules(dbContext).GetNewReference(tradeOrder.SellingDomain.Id, TraderReferenceType.Sale);

                    LogManager.ApplicationInfo(_logLabels, "Creating Sale", null, null,
                                                            "SaleRef: " + traderSaleNew.Reference.FullRef);

                    // Create the TraderTransactionItems
                    if (saleOrder.Items != null)
                        foreach (var orderItem in saleOrder.Items)
                        {
                            //variant

                            var variant = dbContext.PosVariants.Find(orderItem.Variant.TraderId);
                            if (variant == null) continue;
                            var averageCost = variant.TraderItem.InventoryDetails
                                .FirstOrDefault(e => e.Location.Id == tradeOrder.Location.Id)?.AverageCost ?? 1;
                            var costPerUnit = averageCost * variant.Unit.QuantityOfBaseunit;

                            var itemQuantity = saleOrder.VoucherId > 0
                                ? orderItem.Variant.Quantity
                                : orderItem.Quantity;

                            var variantTransactionItem = new TraderTransactionItem
                            {
                                Unit = variant.Unit,
                                CreatedDate = timeZoneCreatedDateTime,
                                Cost = costPerUnit * itemQuantity,
                                CostPerUnit = costPerUnit,
                                CreatedBy = _traderCreatedAndApprovalBy,
                                Dimensions = tradeOrder.ProductMenu.OrderItemDimensions,
                                Discount = orderItem.Variant.Discount,
                                Price = orderItem.Variant.AmountInclTax * itemQuantity,
                                Quantity = itemQuantity,
                                Logs = new List<TransactionItemLog>(),
                                SalePricePerUnit = variant.Price?.NetPrice ?? 0,
                                TraderItem = variant.TraderItem,
                                LastUpdatedBy = _traderCreatedAndApprovalBy,
                                LastUpdatedDate = timeZoneCreatedDateTime,
                                Taxes = new List<OrderTax>()
                            };

                            if (ConfigManager.LoggingDebugSet)
                                LogManager.Debug(MethodBase.GetCurrentMethod(),
                                    "Process Order -  TraderTransactionItem creating ", _traderCreatedAndApprovalBy.Id,
                                    variantTransactionItem);

                            if (orderItem.Variant.Taxes != null)
                            {
                                var taxesId = orderItem.Variant.Taxes.Select(t => t.TraderId);
                                var itemTaxes = dbContext.TraderPriceTaxes.Where(e => taxesId.Contains(e.Id)).ToList();
                                itemTaxes.ForEach(priceTaxItem =>
                                {
                                    var staticTaxRate = priceTaxItem.TaxRate;
                                    variantTransactionItem.Taxes.Add(new OrderTax
                                    {
                                        Value = orderItem.Variant.Taxes.FirstOrDefault(i => i.TraderId == priceTaxItem.Id)
                                            ?.AmountTax ?? 0,
                                        TaxRate = staticTaxRate,
                                        StaticTaxRate = staticTaxRate
                                    });
                                });
                            }

                            var transItemVariantLog = new TransactionItemLog
                            {
                                Unit = variantTransactionItem.Unit,
                                AssociatedTransactionItem = variantTransactionItem,
                                Cost = variantTransactionItem.Cost,
                                CostPerUnit = variantTransactionItem.CostPerUnit,
                                Dimensions = variantTransactionItem.Dimensions,
                                Discount = variantTransactionItem.Discount,
                                Price = variantTransactionItem.Price,
                                Quantity = variantTransactionItem.Quantity,
                                SalePricePerUnit = variantTransactionItem.SalePricePerUnit,
                                TraderItem = variantTransactionItem.TraderItem,
                                TransferItems = variantTransactionItem.TransferItems
                            };

                            variantTransactionItem.Logs.Add(transItemVariantLog);

                            traderSaleNew.SaleItems.Add(variantTransactionItem);

                            //Extra
                            orderItem.Extras?.ForEach(extra =>
                            {
                                var extraItem = dbContext.PosExtras.Find(extra.TraderId);
                                if (extraItem == null)
                                    return;
                                averageCost = extraItem.TraderItem.InventoryDetails
                                    .FirstOrDefault(q => q.Location.Id == tradeOrder.Location.Id)?.AverageCost ?? 1;
                                costPerUnit = averageCost * extraItem.Unit?.QuantityOfBaseunit ?? 1;

                                itemQuantity = saleOrder.VoucherId > 0 ? extra.Quantity : orderItem.Quantity;

                                var exTransactionItem = new TraderTransactionItem
                                {
                                    Unit = extraItem.Unit,
                                    CreatedDate = timeZoneCreatedDateTime,
                                    Cost = costPerUnit * itemQuantity,
                                    CostPerUnit = costPerUnit,
                                    CreatedBy = _traderCreatedAndApprovalBy,
                                    Dimensions = tradeOrder.ProductMenu.OrderItemDimensions,
                                    Discount = extra.Discount,
                                    Price = extra.AmountInclTax * itemQuantity,
                                    //PriceBookPrice = extraItem.BaseUnitPrice,
                                    //PriceBookPriceValue = extraItem.BaseUnitPrice.NetPrice,
                                    Quantity = itemQuantity,
                                    Logs = new List<TransactionItemLog>(),
                                    SalePricePerUnit = extraItem?.Price?.NetPrice ?? 0,
                                    TraderItem = extraItem.TraderItem,
                                    LastUpdatedBy = _traderCreatedAndApprovalBy,
                                    LastUpdatedDate = timeZoneCreatedDateTime,
                                    Taxes = new List<OrderTax>()
                                };

                                if (extra.Taxes != null)
                                {
                                    var taxesId = extra.Taxes.Select(t => t.TraderId);
                                    var itemTaxes = dbContext.TraderPriceTaxes.Where(e => taxesId.Contains(e.Id)).ToList();
                                    itemTaxes.ForEach(priceTaxItem =>
                                    {
                                        var staticTaxRate = priceTaxItem.TaxRate;
                                        exTransactionItem.Taxes.Add(new OrderTax
                                        {
                                            StaticTaxRate = staticTaxRate,
                                            Value =
                                                extra.Taxes.FirstOrDefault(i => i.TraderId == priceTaxItem.Id)?.AmountTax ?? 0,
                                            TaxRate = staticTaxRate
                                        });
                                    });
                                }

                                var transItemLog = new TransactionItemLog
                                {
                                    Unit = exTransactionItem.Unit,
                                    AssociatedTransactionItem = exTransactionItem,
                                    Cost = exTransactionItem.Cost,
                                    CostPerUnit = exTransactionItem.CostPerUnit,
                                    Dimensions = exTransactionItem.Dimensions,
                                    Discount = exTransactionItem.Discount,
                                    Price = exTransactionItem.Price,
                                    //PriceBookPrice = exTransactionItem.PriceBookPrice,
                                    //PriceBookPriceValue = exTransactionItem.PriceBookPriceValue,
                                    Quantity = exTransactionItem.Quantity,
                                    SalePricePerUnit = exTransactionItem.SalePricePerUnit,
                                    TraderItem = exTransactionItem.TraderItem,
                                    TransferItems = exTransactionItem.TransferItems
                                };
                                exTransactionItem.Logs.Add(transItemLog);
                                traderSaleNew.SaleItems.Add(exTransactionItem);
                            });
                        }

                    dbContext.Entry(traderSaleNew).State = EntityState.Added;
                    dbContext.TraderSales.Add(traderSaleNew);

                    tradeOrder.Sale = traderSaleNew;

                    dbContext.SaveChanges();

                    LogManager.ApplicationInfo(_logLabels, "Sale created", null, null,
                                                          "SaleRef: " + traderSaleNew.Reference.FullRef);

                    //Create TraderSale Approval
                    traderSaleNew.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                    var appDef =
                        dbContext.SalesApprovalDefinitions.FirstOrDefault(w =>
                            w.WorkGroup.Id == traderSaleNew.Workgroup.Id);
                    var refFull = traderSaleNew.Reference == null ? "" : traderSaleNew.Reference.FullRef;
                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = appDef,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Approved,
                        Sale = new List<TraderSale> { traderSaleNew },
                        Name =
                            $"Trader Approval for Trader Sale {traderSaleNew.SalesChannel.GetDescription()} #{refFull}",
                        Qbicle = traderSaleNew.Workgroup.Qbicle,
                        Topic = traderSaleNew.Workgroup.Topic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        ReviewedBy = new List<ApplicationUser> { _traderCreatedAndApprovalBy },
                        ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                        StartedBy = _traderCreatedAndApprovalBy,
                        StartedDate = traderSaleNew.CreatedDate,
                        TimeLineDate = traderSaleNew.CreatedDate,
                        Notes = "",
                        App = QbicleActivity.ActivityApp.Trader,
                        IsVisibleInQbicleDashboard = true
                    };

                    traderSaleNew.SaleApprovalProcess = approval;
                    traderSaleNew.SaleApprovalProcess.ApprovalRequestDefinition = appDef;
                    approval.ActivityMembers.AddRange(traderSaleNew.Workgroup.Members);
                    dbContext.Entry(traderSaleNew).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    LogManager.ApplicationInfo(_logLabels, "Sale approval created", null, null,
                                                          "SaleRef: " + traderSaleNew.Reference.FullRef);

                    dbTransaction.Commit();

                    var traderOrderloggingRules = new TradeOrderLoggingRules(dbContext);
                    traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.SaleAdd, traderSaleNew.Id, "", null, "", false);
                    traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.SaleApproval, traderSaleNew.Id, approval.GetCreatedBy().Id, null, "", false);
                    return new ReturnJsonModel { result = true, Object = traderSaleNew };
                }
                catch (Exception ex)
                {
                    LogManager.ApplicationInfo(_logLabels, "Sale creation failed", null, null,
                                                          ex.Message);

                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrder);
                    return new ReturnJsonModel { result = false, msg = ex.Message };
                }
            }
        }

        private async Task<ReturnJsonModel> CreateInvoiceAsync(TradeOrder tradeOrder, TraderSale traderSaleNew, string address = "",
            string invoiceDetail = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(),
                        $"Process Order -  Create Invoice {tradeOrder.SalesChannel.GetDescription()}",
                        traderSaleNew.CreatedBy.Id, null, tradeOrder, traderSaleNew);

                LogManager.ApplicationInfo(_logLabels, "Creating Invoice", null, null,
                                                       "SaleRef: " + traderSaleNew.Reference.FullRef);

                var order = GetOrderFromTradeOrder(tradeOrder);

                var invoiceNew = new Invoice
                {
                    InvoiceItems = new List<InvoiceTransactionItems>(),
                    CreatedDate = order.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                    AssociatedFiles = new List<QbicleMedia>(),
                    CreatedBy = _traderCreatedAndApprovalBy,
                    DueDate = order.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                    PaymentDetails = $"Invoice for {tradeOrder.SalesChannel.GetDescription()}",
                    Payments = new List<CashAccountTransaction>(),
                    Sale = traderSaleNew,
                    Status = TraderInvoiceStatusEnum.InvoiceApproved,
                    Workgroup = tradeOrder.InvoiceWorkGroup
                };

                if (!string.IsNullOrEmpty(address)) invoiceNew.InvoiceAddress = address;
                if (!string.IsNullOrEmpty(invoiceDetail)) invoiceNew.PaymentDetails = invoiceDetail;

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Process Order -  Invoice creating ", traderSaleNew.CreatedBy.Id, invoiceNew, traderSaleNew, order, tradeOrder.PosDevice, tradeOrder.SellingDomain);

                invoiceNew.Reference =
                    new TraderReferenceRules(dbContext).GetNewReference(tradeOrder.SellingDomain.Id, TraderReferenceType.Invoice);

                traderSaleNew.SaleItems?.ForEach(i =>
                {
                    decimal? taxValue = null;
                    if (i.Taxes != null && i.Taxes.Count() > 0)
                    {
                        taxValue = i.Taxes.Sum(t => t.Value);
                    }

                    var item = new InvoiceTransactionItems
                    {
                        InvoiceDiscountValue = (i.Discount / 100) * i.SalePricePerUnit,
                        InvoiceItemQuantity = i.Quantity,
                        InvoiceTaxValue = taxValue,
                        InvoiceValue = i.Price,
                        TransactionItem = i
                    };

                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(),
                            "Process Order -  InvoiceTransactionItems creating ", traderSaleNew.CreatedBy.Id, item,
                            traderSaleNew.CreatedBy.Id, null, tradeOrder, traderSaleNew);

                    invoiceNew.InvoiceItems.Add(item);
                });

                if (invoiceNew.InvoiceItems != null && invoiceNew.InvoiceItems.Count > 0)
                    invoiceNew.TotalInvoiceAmount = invoiceNew.InvoiceItems.Sum(e => e.InvoiceValue);
                traderSaleNew.Invoices.Add(invoiceNew);

                dbContext.Invoices.Add(invoiceNew);
                dbContext.Entry(invoiceNew).State = EntityState.Added;

                tradeOrder.Invoice = invoiceNew;
                dbContext.SaveChanges();

                LogManager.ApplicationInfo(_logLabels, "Invoice created", null, null,
                                                       "InvoiceRef: " + invoiceNew.Reference.FullRef);

                if (tradeOrder.SalesChannel == SalesChannelEnum.B2C || tradeOrder.SalesChannel == SalesChannelEnum.POS || tradeOrder.SalesChannel == SalesChannelEnum.B2B)
                    await CreateInvoicePdfAsync(tradeOrder, invoiceNew);

                var traderOrderloggingRules = new TradeOrderLoggingRules(dbContext);

                traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.InvoiceAdd, invoiceNew.Id, "", null, "", false);

                LogManager.ApplicationInfo(_logLabels, "Creating Invoice Approval", null, null,
                                                       "InvoiceRef: " + invoiceNew.Reference.FullRef);

                var appDef =
                    dbContext.PurchaseApprovalDefinitions.FirstOrDefault(w =>
                        w.WorkGroup.Id == invoiceNew.Workgroup.Id);
                var refFull = invoiceNew.Reference == null ? "" : invoiceNew.Reference.FullRef;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Approved,
                    Invoice = new List<Invoice> { invoiceNew },
                    Name = $"Trader Approval for Trader Invoice {tradeOrder.SalesChannel.GetDescription()} #{refFull}",
                    Qbicle = invoiceNew.Workgroup.Qbicle,
                    Topic = invoiceNew.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    ReviewedBy = new List<ApplicationUser>(),
                    ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                    StartedBy = _traderCreatedAndApprovalBy,
                    StartedDate = invoiceNew.CreatedDate,
                    TimeLineDate = invoiceNew.CreatedDate,
                    Notes = "",
                    App = QbicleActivity.ActivityApp.Trader,
                    IsVisibleInQbicleDashboard = true
                };

                invoiceNew.InvoiceApprovalProcess = approval;
                invoiceNew.InvoiceApprovalProcess.ApprovalRequestDefinition = appDef;
                approval.ActivityMembers.AddRange(invoiceNew.Workgroup.Members);
                dbContext.Entry(invoiceNew).State = EntityState.Modified;

                dbContext.SaveChanges();

                LogManager.ApplicationInfo(_logLabels, "Invoice Approval created", null, null,
                                                       "InvoiceRef: " + invoiceNew.Reference.FullRef);

                traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.InvoiceApproval, invoiceNew.Id, approval.GetCreatedBy().Id, null, "", false);

                new BookkeepingIntegrationRules(dbContext).AddSaleInvoiceJournalEntry(_traderCreatedAndApprovalBy, invoiceNew);

                new NotificationRules(dbContext).SignalRB2COrderProcess(tradeOrder, "", NotificationEventEnum.B2COrderInvoiceCreationCompleted);

                var createdPayment = CreatePayment(tradeOrder, invoiceNew, traderSaleNew, order,
                    tradeOrder.PosDevice?.MethodAccount);
                if (!createdPayment.result)
                    return createdPayment;

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, traderSaleNew.CreatedBy.Id,
                    traderSaleNew.CreatedBy.Id, null, tradeOrder, traderSaleNew);

                return new ReturnJsonModel { result = false, msg = ex.Message, actionVal = 1 };
            }
        }

        private async Task CreateInvoicePdfAsync(TradeOrder tradeOrder, Invoice iv)
        {
            try
            {
                var rule = new TraderInvoicesRules(dbContext);

                //var awsS3Bucket = AzureStorageHelper.AwsS3Client();

                var domainUri = tradeOrder.SaleWorkGroup.Domain.LogoUri;
                var qbicleUri = tradeOrder.SaleWorkGroup.Qbicle.LogoUri;

                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(tradeOrder.SaleWorkGroup.Domain.LogoUri);
                var imageTop = HelperClass.GetBase64StringFromStream(s3Object.ObjectStream);

                s3Object = await (AzureStorageHelper.ReadObjectDataAsync(tradeOrder.SaleWorkGroup.Qbicle.LogoUri));
                var imageBottom = HelperClass.GetBase64StringFromStream(s3Object.ObjectStream);

                var fileStreams =
                    rule.ReportSaleInvoice(iv, imageTop, imageBottom, _currentTimeZone, iv.Workgroup.Domain.Id);

                var tempPathRepository = ConfigManager.TempPathRepository;
                var databaseMenu = $"invoice-{iv.Id}.pdf";
                var filePath = $"{tempPathRepository}/{databaseMenu}";
                DirectoryHelper.DeleteFile($"{filePath}");

                File.WriteAllBytes(filePath, fileStreams);
                var length = new FileInfo(filePath).Length;

                var mediaProcess = new MediaProcess
                {
                    FileName = $"invoice-{iv.Id}.pdf",
                    ObjectKey = Guid.NewGuid().ToString(),
                    FilePath = filePath,
                    IsPublic = false
                };
                await (new AzureStorageRules(dbContext).UploadMediaFromPathByQbicleAsync(mediaProcess));

                iv.InvoicePDF = mediaProcess.ObjectKey;
                iv.Status = TraderInvoiceStatusEnum.InvoiceIssued;
                dbContext.Entry(iv).State = EntityState.Modified;
                dbContext.SaveChanges();

                var rules = new MediasRules(dbContext);
                var fileType = new FileTypeRules(dbContext).GetFileTypeByExtension("pdf");
                var versionFile = new VersionedFile
                {
                    Uri = mediaProcess.ObjectKey,
                    FileSize = HelperClass.FileSize((int)length),
                    FileType = fileType
                };

                var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(tradeOrder.Id);
                //for B2B order
                var discussionB2B = new DiscussionsRules(dbContext).GetB2BDiscussionOrderByTradeorderId(tradeOrder.Id);
                if (discussion == null && discussionB2B == null)
                {
                    LogManager.ApplicationInfo(_logLabels, "Unable to add Media Item for Invoice PDF - no Discussion/Qbicle associated with TradeOrder", null, null,
                                                           "InvoiceRef: " + iv.Reference.FullRef,
                                                           "URI: " + mediaProcess.ObjectKey);
                }
                else if (discussion != null)
                {
                    var media = new QbicleMedia
                    {
                        Name = mediaProcess.FileName,
                        Description = $"Invoice PDF {iv.Reference.FullRef}",
                        FileType = fileType,
                        Qbicle = discussion.Qbicle
                    };

                    var folder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, media.Qbicle.Id, _traderCreatedAndApprovalBy.Id);

                    rules.SaveMedia(media, false, iv.CreatedBy.Id, false,
                        discussion.Id, 0, 0, 0, 0, 0, 0, HelperClass.GeneralName, versionFile, folder.Id);
                }
                else if (discussionB2B != null)
                {
                    var media = new QbicleMedia
                    {
                        Name = mediaProcess.FileName,
                        Description = $"Invoice PDF {iv.Reference.FullRef}",
                        FileType = fileType,
                        Qbicle = discussionB2B.Qbicle
                    };

                    var folder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, media.Qbicle.Id, _traderCreatedAndApprovalBy.Id);

                    rules.SaveMedia(media, false, iv.CreatedBy.Id, false,
                        discussionB2B.Id, 0, 0, 0, 0, 0, 0, HelperClass.GeneralName, versionFile, folder.Id);
                }

                LogManager.ApplicationInfo(_logLabels, "Invoice PDF created", null, null,
                                                       "InvoiceRef: " + iv.Reference.FullRef);

                new EmailRules(dbContext).SendEmailQbicleIssue(iv, filePath, null,
                    $"{ConfigManager.QbiclesUrl}/Account/CreateAccount", IssueType.Invoice);

                LogManager.ApplicationInfo(_logLabels, "Invoice PDF emailed", null, null,
                                                       "InvoiceRef: " + iv.Reference.FullRef);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, iv.CreatedBy.Id, iv.CreatedBy.Id, null, tradeOrder, iv);
            }
        }

        private ReturnJsonModel CreatePayment(TradeOrder tradeOrder, Invoice invoiceNew, TraderSale traderSaleNew,
            Order order, List<PosPaymentMethodAccountXref> posPaymentMethodAccountXrefs)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Order process - Creatingpayment  ",
                        traderSaleNew.CreatedBy.Id, null, invoiceNew, traderSaleNew, order,
                        posPaymentMethodAccountXrefs);

                LogManager.ApplicationInfo(_logLabels, "Creating Payment", null, null,
                                                       "InvoiceRef: " + invoiceNew.Reference.FullRef,
                                                       posPaymentMethodAccountXrefs);

                switch (tradeOrder.SalesChannel)
                {
                    case SalesChannelEnum.POS:
                        LogManager.ApplicationInfo(_logLabels, "Creating POS Payment", null, null,
                                                       "InvoiceRef: " + invoiceNew.Reference.FullRef);

                        PaymentPos(tradeOrder, invoiceNew, traderSaleNew, order, posPaymentMethodAccountXrefs);
                        break;

                    case SalesChannelEnum.B2C:

                        LogManager.ApplicationInfo(_logLabels, "NO B2C Payment created. This is created only though the UI.", null, null,
                                                       "InvoiceRef: " + invoiceNew.Reference.FullRef);

                        break;

                    case SalesChannelEnum.B2B:

                        LogManager.ApplicationInfo(_logLabels, "Creating B2B Payment", null, null,
                                                      "InvoiceRef: " + invoiceNew.Reference.FullRef);

                        SalePaymentB2B(tradeOrder, invoiceNew, traderSaleNew, order);
                        break;

                    case SalesChannelEnum.Trader:

                    default:
                        break;
                }

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, traderSaleNew.CreatedBy.Id, null, invoiceNew,
                    traderSaleNew, order, posPaymentMethodAccountXrefs);
                return new ReturnJsonModel { result = false, msg = ex.Message, actionVal = 2 };
            }
        }

        private void PaymentPos(TradeOrder tradeOrder, Invoice invoiceNew, TraderSale traderSaleNew, Order order, List<PosPaymentMethodAccountXref> posPaymentMethodAccountXrefs)
        {
            var bkIntegrationRule = new BookkeepingIntegrationRules(dbContext);

            var traderOrderloggingRules = new TradeOrderLoggingRules(dbContext);

            order.Payments?.ForEach(oPayment =>
            {
                var paymentPos = new CashAccountTransaction
                {
                    CreatedDate = order.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                    Amount = oPayment.AmountAccepted,
                    AssociatedInvoice = invoiceNew,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    Status = TraderPaymentStatusEnum.PaymentApproved,
                    Description =
                        $"PoS Payment for Invoice {tradeOrder.SalesChannel.GetDescription()} #{invoiceNew.Reference?.FullRef}",
                    Workgroup = tradeOrder.PaymentWorkGroup,
                    Type = CashAccountTransactionTypeEnum.PaymentIn,
                    Contact = traderSaleNew.Purchaser,
                    Charges = 0,
                    AssociatedSale = traderSaleNew
                };
                var paymentXref = posPaymentMethodAccountXrefs.FirstOrDefault(e => e.Id == oPayment.Method);
                paymentPos.DestinationAccount = paymentXref?.CollectionAccount;
                paymentPos.PaymentMethod = paymentXref?.PaymentMethod;
                var oPaymentRef = string.IsNullOrEmpty(oPayment.Reference) ? "" : $" Ref: {oPayment.Reference}";
                paymentPos.Reference =
                    $"{PaymentReferenceConst.PosCashPaymentReferenceString} {paymentXref?.TabletDisplayName} ({paymentXref?.PaymentMethod.Name}{oPaymentRef}) Trader Order Reference:{order.Reference}";

                tradeOrder.PaymentWorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(),
                        "POS Order CashAccountTransaction Payment creating ", traderSaleNew.CreatedBy.Id, paymentPos);

                dbContext.CashAccountTransactions.Add(paymentPos);
                dbContext.Entry(paymentPos).State = EntityState.Added;

                var paymentPosLog = new CashAccountTransactionLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Id = 0,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    Status = paymentPos.Status,
                    Description = paymentPos.Description,
                    Type = paymentPos.Type,
                    AssociatedInvoice = paymentPos.AssociatedInvoice,
                    Workgroup = tradeOrder.PaymentWorkGroup,
                    DestinationAccount = paymentPos.DestinationAccount,
                    OriginatingAccount = paymentPos.OriginatingAccount,
                    AssociatedFiles = paymentPos.AssociatedFiles,
                    Amount = paymentPos.Amount,
                    AssociatedSale = paymentPos.AssociatedSale,
                    AssociatedPurchase = paymentPos.AssociatedPurchase,
                    PaymentApprovalProcess = paymentPos.PaymentApprovalProcess,
                    Contact = paymentPos.Contact,
                    AssociatedTransaction = paymentPos,
                    AssociatedBKTransaction = paymentPos.AssociatedBKTransaction,
                    Charges = paymentPos.Charges,
                    Reference = paymentPos.Reference,
                    PaymentMethod = paymentPos.PaymentMethod
                };

                var wasteProcessPosLog = new PaymentProcessLog
                {
                    AssociatedTransaction = paymentPos,
                    AssociatedCashAccountTransactionLog = paymentPosLog,
                    PaymentStatus = paymentPos.Status,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.PaymentProcessLogs.Add(wasteProcessPosLog);
                dbContext.Entry(wasteProcessPosLog).State = EntityState.Added;

                if (tradeOrder.Payments == null)
                    tradeOrder.Payments = new List<CashAccountTransaction>();

                tradeOrder.Payments.Add(paymentPos);

                dbContext.SaveChanges();

                traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentAdd, paymentPos.Id, "", null, "", false);
                traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentApproval, paymentPos.Id, paymentPos.CreatedBy.Id, null, "", false);

                bkIntegrationRule.AddPaymentJournalEntry(_traderCreatedAndApprovalBy, paymentPos);

                //If the payment uses the Store Credit payment method then decrease the StoreCredit
                if (paymentPos.PaymentMethod.Name == PaymentMethodNameConst.StoreCredit)
                    new TraderEventRules(dbContext).DecreaseStoreCreditFromPaymentApproved(paymentPos.Id);

                //Hangfire to generate store points from the payment approved
                if (paymentPos.Status == TraderPaymentStatusEnum.PaymentApproved)
                    new TraderEventRules(dbContext).GenerateStorePoinFromPaymentApproved(paymentPos.Id);
            });
        }

        /// <summary>
        /// Check to see if there is a discussion associated with the trade order
        //  If not, create one based on the TradeOrder.Customer
        //  If the TradeOrder.Customer is not a user in the system, send them an email invitation
        /// </summary>
        /// <param name="tradeOrder"></param>
        private void CreateB2CDiscussionForPos(TradeOrder tradeOrder)
        {
            // Be sure there is only one discussion associated with the B2C TradeOrder
            var associatedDiscussion = dbContext.B2COrderCreations.FirstOrDefault(dis => dis.TradeOrder.Id == tradeOrder.Id);
            if (associatedDiscussion == null)
            {
                if (tradeOrder.Customer != null)
                {
                    //Create a discussion and a Qbicle to hold the discussion if needed
                    new DiscussionsRules(dbContext).CreateB2CDiscussionForPos(tradeOrder);
                    LogManager.ApplicationInfo(_logLabels, "B2C Discussion created for POS order", null, null,
                                                           "Customer Id: " + tradeOrder.Customer.Id);
                }
                else if (tradeOrder.OrderCustomer != null && !tradeOrder.OrderCustomer.IsDefaultWalkinCustomer)
                {
                    //Send an email invitaion for an Order customer to join Qbicles
                    new EmailRules(dbContext).SendEmailImportInvite(_traderCreatedAndApprovalBy,
                        new List<Micro.Model.MicroContact> {
                        new Micro.Model.MicroContact {
                            Email = tradeOrder.OrderCustomer.Email,
                            Name = tradeOrder.OrderCustomer.CustomerName,
                            Phone = tradeOrder.OrderCustomer.PhoneNumber
                        }
                        });

                    LogManager.ApplicationInfo(_logLabels, "Email invitation sent", null, null,
                                                           "Email address: " + tradeOrder.OrderCustomer.Email);
                }
            }
        }

        /// <summary>
        ///     Unuse this method to create payment because the order created from B2C need reate and manegement from the Trader
        ///     app
        /// </summary>
        /// <param name="tradeOrder"></param>
        /// <param name="invoiceNew"></param>
        /// <param name="traderSaleNew"></param>
        /// <param name="order"></param>
        /// <param name="user"></param>
        private void PaymentB2C(TradeOrder tradeOrder, Invoice invoiceNew, TraderSale traderSaleNew, Order order)
        {
            var bkIntegrationRule = new BookkeepingIntegrationRules(dbContext);

            var paymentB2C = new CashAccountTransaction
            {
                CreatedDate = order.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                Amount = order.AmountInclTax,
                AssociatedInvoice = invoiceNew,
                CreatedBy = _traderCreatedAndApprovalBy,
                Status = TraderPaymentStatusEnum.PendingReview,
                Description =
                    $"Payment for B2C Order {tradeOrder.SalesChannel.GetDescription()} #{invoiceNew.Reference?.FullRef}",
                Workgroup = tradeOrder.PaymentWorkGroup,
                Type = CashAccountTransactionTypeEnum.PaymentIn,
                DestinationAccount = tradeOrder.PaymentAccount,
                Reference = $"{PaymentReferenceConst.B2CCashPaymentReferenceString} {order.Reference}",
                Contact = traderSaleNew.Purchaser,
                Charges = 0,
                AssociatedSale = traderSaleNew,
                PaymentMethod = null
            };

            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "B2C Order CashAccountTransaction Payment creating ",
                    traderSaleNew.CreatedBy.Id, paymentB2C);

            tradeOrder.PaymentWorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;

            var approval = new ApprovalReq
            {
                ApprovalRequestDefinition =
                    dbContext.PaymentApprovalDefinitions.FirstOrDefault(w =>
                        w.WorkGroup.Id == tradeOrder.PaymentWorkGroup.Id),
                ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                Priority = ApprovalReq.ApprovalPriorityEnum.High,
                RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                Qbicle = tradeOrder.PaymentWorkGroup.Qbicle,
                Topic = tradeOrder.PaymentWorkGroup.Topic,
                State = QbicleActivity.ActivityStateEnum.Open,
                ReviewedBy = new List<ApplicationUser> { _traderCreatedAndApprovalBy },
                ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                StartedBy = _traderCreatedAndApprovalBy,
                StartedDate = paymentB2C.CreatedDate,
                TimeLineDate = paymentB2C.CreatedDate,
                Notes = "",
                IsVisibleInQbicleDashboard = true,
                App = QbicleActivity.ActivityApp.Trader,
                Name = $"Approval Payment for B2C Order #{paymentB2C.Reference}",
                Payments = new List<CashAccountTransaction> { paymentB2C }
            };
            paymentB2C.PaymentApprovalProcess = approval;
            approval.ActivityMembers.AddRange(tradeOrder.PaymentWorkGroup.Members);

            dbContext.CashAccountTransactions.Add(paymentB2C);
            dbContext.Entry(paymentB2C).State = EntityState.Added;

            var paymentB2CLog = new CashAccountTransactionLog
            {
                CreatedDate = DateTime.UtcNow,
                Id = 0,
                CreatedBy = _traderCreatedAndApprovalBy,
                Status = paymentB2C.Status,
                Description = paymentB2C.Description,
                Type = paymentB2C.Type,
                AssociatedInvoice = paymentB2C.AssociatedInvoice,
                Workgroup = tradeOrder.PaymentWorkGroup,
                DestinationAccount = paymentB2C.DestinationAccount,
                OriginatingAccount = paymentB2C.OriginatingAccount,
                AssociatedFiles = paymentB2C.AssociatedFiles,
                Amount = paymentB2C.Amount,
                AssociatedSale = paymentB2C.AssociatedSale,
                AssociatedPurchase = paymentB2C.AssociatedPurchase,
                PaymentApprovalProcess = paymentB2C.PaymentApprovalProcess,
                Contact = paymentB2C.Contact,
                AssociatedTransaction = paymentB2C,
                AssociatedBKTransaction = paymentB2C.AssociatedBKTransaction,
                Charges = paymentB2C.Charges,
                Reference = paymentB2C.Reference,
                PaymentMethod = paymentB2C.PaymentMethod
            };

            var wasteProcessB2CLog = new PaymentProcessLog
            {
                AssociatedTransaction = paymentB2C,
                AssociatedCashAccountTransactionLog = paymentB2CLog,
                PaymentStatus = paymentB2C.Status,
                CreatedBy = _traderCreatedAndApprovalBy,
                CreatedDate = DateTime.UtcNow
            };

            dbContext.PaymentProcessLogs.Add(wasteProcessB2CLog);
            dbContext.Entry(wasteProcessB2CLog).State = EntityState.Added;

            tradeOrder.Payments.Add(paymentB2C);

            dbContext.SaveChanges();

            bkIntegrationRule.AddPaymentJournalEntry(_traderCreatedAndApprovalBy, paymentB2C);
        }

        private void SalePaymentB2B(TradeOrder tradeOrder, Invoice invoiceNew, TraderSale traderSaleNew, Order order)
        {
            var bkIntegrationRule = new BookkeepingIntegrationRules(dbContext);

            var paymentB2B = new CashAccountTransaction
            {
                CreatedDate = order.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                Amount = order.AmountInclTax,
                AssociatedInvoice = invoiceNew,
                CreatedBy = _traderCreatedAndApprovalBy,
                Status = TraderPaymentStatusEnum.PendingReview,
                Description =
                    $"Payment for B2B Order {tradeOrder.SalesChannel.GetDescription()} #{invoiceNew.Reference?.FullRef}",
                Workgroup = tradeOrder.PaymentWorkGroup,
                Type = CashAccountTransactionTypeEnum.PaymentIn,
                DestinationAccount = tradeOrder.PaymentAccount,
                Reference = $"{PaymentReferenceConst.B2BCashPaymentReferenceString} {order.Reference}",
                Contact = traderSaleNew.Purchaser,
                Charges = 0,
                AssociatedSale = traderSaleNew,
                PaymentMethod = null
            };

            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "B2B Order CashAccountTransaction Payment creating ",
                    traderSaleNew.CreatedBy.Id, paymentB2B);

            tradeOrder.PaymentWorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;

            var approval = new ApprovalReq
            {
                ApprovalRequestDefinition =
                    dbContext.PaymentApprovalDefinitions.FirstOrDefault(w =>
                        w.WorkGroup.Id == tradeOrder.PaymentWorkGroup.Id),
                ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                Priority = ApprovalReq.ApprovalPriorityEnum.High,
                RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                Qbicle = tradeOrder.PaymentWorkGroup.Qbicle,
                Topic = tradeOrder.PaymentWorkGroup.Topic,
                State = QbicleActivity.ActivityStateEnum.Open,
                ReviewedBy = new List<ApplicationUser>(),
                ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                StartedBy = _traderCreatedAndApprovalBy,
                StartedDate = paymentB2B.CreatedDate,
                TimeLineDate = paymentB2B.CreatedDate,
                Notes = "",
                IsVisibleInQbicleDashboard = true,
                App = QbicleActivity.ActivityApp.Trader,
                Name = $"Approval Payment for B2B Order #{paymentB2B.Reference}",
                Payments = new List<CashAccountTransaction> { paymentB2B }
            };
            paymentB2B.PaymentApprovalProcess = approval;
            approval.ActivityMembers.AddRange(tradeOrder.PaymentWorkGroup.Members);

            dbContext.CashAccountTransactions.Add(paymentB2B);
            dbContext.Entry(paymentB2B).State = EntityState.Added;

            var paymentB2BLog = new CashAccountTransactionLog
            {
                CreatedDate = DateTime.UtcNow,
                Id = 0,
                CreatedBy = _traderCreatedAndApprovalBy,
                Status = paymentB2B.Status,
                Description = paymentB2B.Description,
                Type = paymentB2B.Type,
                AssociatedInvoice = paymentB2B.AssociatedInvoice,
                Workgroup = tradeOrder.PaymentWorkGroup,
                DestinationAccount = paymentB2B.DestinationAccount,
                OriginatingAccount = paymentB2B.OriginatingAccount,
                AssociatedFiles = paymentB2B.AssociatedFiles,
                Amount = paymentB2B.Amount,
                AssociatedSale = paymentB2B.AssociatedSale,
                AssociatedPurchase = paymentB2B.AssociatedPurchase,
                PaymentApprovalProcess = paymentB2B.PaymentApprovalProcess,
                Contact = paymentB2B.Contact,
                AssociatedTransaction = paymentB2B,
                AssociatedBKTransaction = paymentB2B.AssociatedBKTransaction,
                Charges = paymentB2B.Charges,
                Reference = paymentB2B.Reference,
                PaymentMethod = paymentB2B.PaymentMethod
            };

            var wasteProcessB2BLog = new PaymentProcessLog
            {
                AssociatedTransaction = paymentB2B,
                AssociatedCashAccountTransactionLog = paymentB2BLog,
                PaymentStatus = paymentB2B.Status,
                CreatedBy = _traderCreatedAndApprovalBy,
                CreatedDate = DateTime.UtcNow
            };

            dbContext.PaymentProcessLogs.Add(wasteProcessB2BLog);
            dbContext.Entry(wasteProcessB2BLog).State = EntityState.Added;

            tradeOrder.Payments.Add(paymentB2B);

            dbContext.SaveChanges();

            var loggingRules = new TradeOrderLoggingRules(dbContext);
            loggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentAdd, paymentB2B.Id, "", null, "", false);
            loggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentApproval, paymentB2B.Id, approval.GetCreatedBy().Id, null, "", false);

            bkIntegrationRule.AddPaymentJournalEntry(_traderCreatedAndApprovalBy, paymentB2B);
        }

        private ReturnJsonModel PurchasePaymentB2B(TradeOrderB2B tradeOrder, Order orderJson, ExchangeRate exchangeRate)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Order process CreatePosPayment creating ",
                        _traderCreatedAndApprovalBy.Id, null, tradeOrder, orderJson);
                var bkIntegrationRule = new BookkeepingIntegrationRules(dbContext);

                var paymentB2B = new CashAccountTransaction
                {
                    CreatedDate = orderJson.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                    Amount = orderJson.AmountInclTax * exchangeRate.ExchangeRateValue,
                    AssociatedInvoice = tradeOrder.Bill,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    Status = TraderPaymentStatusEnum.PendingReview,
                    Description =
                        $"Payment for B2B Order {tradeOrder.SalesChannel.GetDescription()} #{tradeOrder.Bill.Reference?.FullRef}",
                    Workgroup = tradeOrder.PurchasePaymentWorkGroup,
                    Type = CashAccountTransactionTypeEnum.PaymentOut,
                    OriginatingAccount = tradeOrder.PurchasePaymentAccount,
                    Reference = $"{PaymentReferenceConst.B2BCashPaymentReferenceString} {orderJson.Reference}",
                    Contact = tradeOrder.VendorTraderContact,
                    Charges = 0,
                    AssociatedPurchase = tradeOrder.Purchase,
                    PaymentMethod = null
                };

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(),
                        "B2B Order CashAccountTransaction Payment creating ", paymentB2B.CreatedBy.Id, paymentB2B);

                tradeOrder.PurchasePaymentWorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition =
                        dbContext.PaymentApprovalDefinitions.FirstOrDefault(w =>
                            w.WorkGroup.Id == tradeOrder.PurchasePaymentWorkGroup.Id),
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = tradeOrder.PurchasePaymentWorkGroup.Qbicle,
                    Topic = tradeOrder.PurchasePaymentWorkGroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    ReviewedBy = new List<ApplicationUser>(),
                    ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                    StartedBy = _traderCreatedAndApprovalBy,
                    StartedDate = paymentB2B.CreatedDate,
                    TimeLineDate = paymentB2B.CreatedDate,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Approval Payment for B2B Order #{paymentB2B.Reference}",
                    Payments = new List<CashAccountTransaction> { paymentB2B }
                };
                paymentB2B.PaymentApprovalProcess = approval;
                approval.ActivityMembers.AddRange(tradeOrder.PurchasePaymentWorkGroup.Members);

                dbContext.CashAccountTransactions.Add(paymentB2B);
                dbContext.Entry(paymentB2B).State = EntityState.Added;

                var paymentB2BLog = new CashAccountTransactionLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Id = 0,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    Status = paymentB2B.Status,
                    Description = paymentB2B.Description,
                    Type = paymentB2B.Type,
                    AssociatedInvoice = paymentB2B.AssociatedInvoice,
                    Workgroup = tradeOrder.PurchasePaymentWorkGroup,
                    DestinationAccount = paymentB2B.DestinationAccount,
                    OriginatingAccount = paymentB2B.OriginatingAccount,
                    AssociatedFiles = paymentB2B.AssociatedFiles,
                    Amount = paymentB2B.Amount,
                    AssociatedSale = paymentB2B.AssociatedSale,
                    AssociatedPurchase = paymentB2B.AssociatedPurchase,
                    PaymentApprovalProcess = paymentB2B.PaymentApprovalProcess,
                    Contact = paymentB2B.Contact,
                    AssociatedTransaction = paymentB2B,
                    AssociatedBKTransaction = paymentB2B.AssociatedBKTransaction,
                    Charges = paymentB2B.Charges,
                    Reference = paymentB2B.Reference,
                    PaymentMethod = paymentB2B.PaymentMethod
                };

                var wasteProcessB2BLog = new PaymentProcessLog
                {
                    AssociatedTransaction = paymentB2B,
                    AssociatedCashAccountTransactionLog = paymentB2BLog,
                    PaymentStatus = paymentB2B.Status,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.PaymentProcessLogs.Add(wasteProcessB2BLog);
                dbContext.Entry(wasteProcessB2BLog).State = EntityState.Added;

                tradeOrder.PurchasePayments.Add(paymentB2B);

                dbContext.SaveChanges();

                var loggingRules = new TradeOrderLoggingRules(dbContext);
                loggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentAdd, paymentB2B.Id, "", null, "", false);
                loggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentApproval, paymentB2B.Id, approval.GetCreatedBy().Id, null, "", false);

                bkIntegrationRule.AddPaymentJournalEntry(_traderCreatedAndApprovalBy, paymentB2B);

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, _traderCreatedAndApprovalBy.Id, null, tradeOrder,
                    orderJson);
                return new ReturnJsonModel { result = false, msg = ex.Message, actionVal = 2 };
            }
        }

        private async Task<ReturnJsonModel> CreateTransfer(TradeOrder tradeOrder, TraderSale traderSaleNew)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create Transfer", traderSaleNew.CreatedBy.Id,
                        null, traderSaleNew);

                var tradTransfer = new TraderTransfer
                {
                    CreatedBy = _traderCreatedAndApprovalBy,
                    CreatedDate = DateTime.UtcNow,
                    DestinationLocation = null,
                    OriginatingLocation = traderSaleNew.Location,
                    Sale = traderSaleNew,
                    Workgroup = tradeOrder.TransferWorkGroup,
                    Address = traderSaleNew.DeliveryAddress,
                    Contact = traderSaleNew.Purchaser,
                    Status = TransferStatus.Delivered,
                    Reason = TransferReasonEnum.Sale
                };

                tradTransfer.Reference =
                    new TraderReferenceRules(dbContext).GetNewReference(tradTransfer.Workgroup.Domain.Id, TraderReferenceType.Transfer);

                if (tradeOrder.SalesChannel == SalesChannelEnum.B2C || tradeOrder.SalesChannel == SalesChannelEnum.B2B)
                    tradTransfer.Status = TransferStatus.PendingPickup;

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Creating transfer", traderSaleNew.CreatedBy.Id,
                        tradTransfer);

                LogManager.ApplicationInfo(_logLabels, "Creating Transfer", null, null,
                                                       "TransferRef: " + tradTransfer.Reference.FullRef);

                var compressTransactionItems = CompressTransactionItems(traderSaleNew.SaleItems);
                foreach (var saleItem in compressTransactionItems)
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create TraderTransferItem",
                            traderSaleNew.CreatedBy.Id, saleItem);
                    var unit = saleItem.TraderItem.Units.FirstOrDefault(s => s.IsBase);
                    tradTransfer.TransferItems.Add(new TraderTransferItem
                    {
                        Unit = unit,
                        QuantityAtPickup = saleItem.TotalQuantity,
                        QuantityAtDelivery = saleItem.TotalQuantity,
                        TransactionItem = saleItem.TransactionItem,
                        TraderItem = saleItem.TraderItem
                    });
                }

                dbContext.TraderTransfers.Add(tradTransfer);
                dbContext.Entry(tradTransfer).State = EntityState.Added;
                LogManager.ApplicationInfo(_logLabels, "Transfer created", null, null,
                                                       "TransferRef: " + tradTransfer.Reference.FullRef);

                tradeOrder.Transfer = tradTransfer;

                tradTransfer.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;

                tradTransfer.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var transferLog = new TransferLog
                {
                    Address = tradTransfer.Address,
                    AssociatedTransfer = tradTransfer,
                    Contact = tradTransfer.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Sale = tradTransfer.Sale,
                    Status = tradTransfer.Status,
                    UpdatedBy = _traderCreatedAndApprovalBy,
                    AssociatedShipment = tradTransfer.AssociatedShipment,
                    DestinationLocation = tradTransfer.DestinationLocation,
                    OriginatingLocation = tradTransfer.OriginatingLocation,
                    TransferItems = tradTransfer.TransferItems,
                    Workgroup = tradeOrder.PaymentWorkGroup
                };

                var transferProcessLog = new TransferProcessLog
                {
                    AssociatedTransfer = tradTransfer,
                    AssociatedTransferLog = transferLog,
                    TransferStatus = tradTransfer.Status,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.TraderTransferProcessLogs.Add(transferProcessLog);
                dbContext.Entry(transferProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();

                var loggingRules = new TradeOrderLoggingRules(dbContext);
                loggingRules.TradeOrderLogging(TradeOrderLoggingType.TransferAdd, tradTransfer.Id, "", null, "", false);

                if (tradeOrder.SalesChannel == SalesChannelEnum.B2C || tradeOrder.SalesChannel == SalesChannelEnum.B2B)
                {
                    LogManager.ApplicationInfo(_logLabels, "Creating Transfer Approval", null, null,
                                                       "TransferRef: " + tradTransfer.Reference.FullRef);

                    var appDef =
                        dbContext.TransferApprovalDefinitions.FirstOrDefault(w =>
                            w.WorkGroup.Id == tradTransfer.Workgroup.Id);
                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = appDef,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                        Qbicle = tradTransfer.Workgroup.Qbicle,
                        Topic = tradTransfer.Workgroup.Topic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        ReviewedBy = new List<ApplicationUser>(),
                        ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                        StartedBy = _traderCreatedAndApprovalBy,
                        StartedDate = tradTransfer.CreatedDate,
                        TimeLineDate = tradTransfer.CreatedDate,
                        Notes = "",
                        IsVisibleInQbicleDashboard = true,
                        App = QbicleActivity.ActivityApp.Trader,
                        Name = $"Transfer for B2C Order {tradeOrder.SalesChannel.GetDescription()} #" +
                            (tradTransfer.Reference == null
                                ? ""
                                : tradTransfer.Reference.FullRef),
                        Transfer = new List<TraderTransfer> { tradTransfer }
                    };

                    tradTransfer.TransferApprovalProcess = approval;
                    tradTransfer.TransferApprovalProcess.ApprovalRequestDefinition = appDef;

                    approval.ActivityMembers.AddRange(tradTransfer.Workgroup.Members);

                    dbContext.Entry(tradTransfer).State = EntityState.Modified;

                    dbContext.SaveChanges();

                    LogManager.ApplicationInfo(_logLabels, "Transfer Approval created", null, null,
                                                       "TransferRef: " + tradTransfer.Reference.FullRef);

                    loggingRules.TradeOrderLogging(TradeOrderLoggingType.TransferApproval, tradTransfer.Id, approval.GetCreatedBy().Id, null, "", false);
                }
                else if (tradeOrder.SalesChannel == SalesChannelEnum.POS)
                {
                    loggingRules.TradeOrderLogging(TradeOrderLoggingType.TransferApproval, tradTransfer.Id, tradTransfer.CreatedBy.Id, null, "", false);
                    //When a Transfer OUT occurs (a Sale), after the Batches (out) have been created, that is when we have to calculate the costs for the inverntorydetail OUT of which the batches are leaving
                    await new TraderTransfersRules(dbContext).OutgoingInventory(tradTransfer, _traderCreatedAndApprovalBy);
                }
                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderSaleNew);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        private ReturnJsonModel CreateTraderPurchase(TradeOrderB2B tradeOrder, Order orderJson,
            ExchangeRate exchangeRate)
        {
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrder);

                    //timezone format
                    var timeZoneCreatedDateTime = orderJson.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ??
                                                  DateTime.UtcNow;
                    var purchase = new TraderPurchase
                    {
                        Location = tradeOrder.DestinationLocation,
                        Workgroup = tradeOrder.PurchaseWorkGroup,
                        Vendor = tradeOrder.VendorTraderContact,
                        DeliveryMethod = tradeOrder.DeliveryMethod,
                        Reference = new TraderReferenceRules(dbContext).GetNewReference(tradeOrder.BuyingDomain.Id,
                            TraderReferenceType.Purchase),
                        CreatedBy = _traderCreatedAndApprovalBy,
                        CreatedDate = DateTime.UtcNow,
                        Status = TraderPurchaseStatusEnum.PurchaseApproved,
                        PurchaseTotal = orderJson.AmountInclTax * exchangeRate.ExchangeRateValue,
                    };
                    purchase.PurchaseChannel = SalesChannelEnum.B2B;
                    if (orderJson.Items != null)
                        foreach (var orderItem in orderJson.Items)
                        {
                            //variant

                            var variant = dbContext.PosVariants.Find(orderItem.Variant.TraderId);
                            if (variant == null) continue;

                            // Find TraderItem in BuyingDomain associated with Variant
                            var tradingItem = tradeOrder.TradingItems.FirstOrDefault(s => s.Variant.Id == variant.Id);
                            if (tradingItem == null) continue;

                            var itemQuantity = orderJson.VoucherId > 0 ? orderItem.Variant.Quantity : orderItem.Quantity;

                            // First find the cost per unit inclusive of tax
                            // Cost including Tax
                            var costPerUnitIncludingTax = orderItem.Variant.AmountInclTax * exchangeRate.ExchangeRateValue;

                            // Now find the TaxRates associated with the traderItem in the BUYING Domain
                            //Tax Rate(s)
                            var taxRates = tradingItem.ConsumerDomainItem.TaxRates.FindAll(t => t.IsPurchaseTax == true);
                            // Total tax rate
                            var totalTaxRates = taxRates?.Sum(s => s.Rate) ?? 0;

                            // With the costPerUnitIncludingTax & taxRates calculate back to thecostPerUnit (i.e. the cost BEFORE tax is added)
                            //Cost Excluding Tax = Cost including Tax / (1+(Total tax rate/100))
                            var costPerUnitExcludeTax = costPerUnitIncludingTax / (1 + (totalTaxRates / 100));

                            //Total Tax Value = Cost including Tax - Cost including Tax / (1 + (Total tax rate/100))
                            var totalTaxValue = costPerUnitIncludingTax - costPerUnitIncludingTax / (1 + (totalTaxRates / 100));

                            var variantTransactionItem = new TraderTransactionItem
                            {
                                Unit = tradingItem.ConsumerUnit,
                                TraderItem = tradingItem.ConsumerDomainItem,
                                CreatedDate = timeZoneCreatedDateTime,
                                Cost = costPerUnitIncludingTax * itemQuantity,
                                CostPerUnit = costPerUnitExcludeTax,
                                CreatedBy = _traderCreatedAndApprovalBy,
                                //Dimensions = tradeOrder.ProductMenu.OrderItemDimensions, QBIC-3799
                                Discount = orderItem.Variant.Discount * exchangeRate.ExchangeRateValue,
                                Quantity = itemQuantity,
                                LastUpdatedBy = _traderCreatedAndApprovalBy,
                                LastUpdatedDate = timeZoneCreatedDateTime
                            };

                            // The Taxes base them off the purchase domain taxes for that Item
                            if (taxRates != null)
                            {
                                taxRates.ForEach(taxRate =>
                                {
                                    var staticTaxRate = new TaxRateRules(dbContext).CloneStaticTaxRateById(taxRate.Id);
                                    // var taxValue = (totalTaxValue / totalTaxRates) * staticTaxRate?.Rate ?? 1;
                                    variantTransactionItem.Taxes.Add(new OrderTax
                                    {
                                        StaticTaxRate = staticTaxRate,
                                        Value = (totalTaxValue / totalTaxRates) * staticTaxRate?.Rate ?? 1,
                                        TaxRate = taxRate
                                    });
                                });
                            }

                            var transItemVariantLog = new TransactionItemLog
                            {
                                Unit = variantTransactionItem.Unit,
                                AssociatedTransactionItem = variantTransactionItem,
                                Cost = variantTransactionItem.Cost,
                                CostPerUnit = variantTransactionItem.CostPerUnit,
                                Dimensions = variantTransactionItem.Dimensions,
                                Discount = variantTransactionItem.Discount,
                                Price = variantTransactionItem.Price,
                                PriceBookPrice = variantTransactionItem.PriceBookPrice,
                                PriceBookPriceValue = variantTransactionItem.PriceBookPriceValue,
                                Quantity = variantTransactionItem.Quantity,
                                SalePricePerUnit = variantTransactionItem.SalePricePerUnit,
                                TraderItem = variantTransactionItem.TraderItem,
                                TransferItems = variantTransactionItem.TransferItems
                            };

                            variantTransactionItem.Logs.Add(transItemVariantLog);

                            purchase.PurchaseItems.Add(variantTransactionItem);
                        }

                    dbContext.Entry(purchase).State = EntityState.Added;
                    dbContext.TraderPurchases.Add(purchase);
                    dbContext.SaveChanges();

                    tradeOrder.Purchase = purchase;

                    #region Create purchase Appoval

                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Creating TradePurchase Approval.",
                            purchase.CreatedBy.Id, null, purchase);

                    purchase.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                    var appDef =
                        dbContext.PurchaseApprovalDefinitions.FirstOrDefault(w =>
                            w.WorkGroup.Id == purchase.Workgroup.Id);
                    var refFull = purchase.Reference == null ? "" : purchase.Reference.FullRef;
                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = appDef,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Approved,
                        Purchase = new List<TraderPurchase> { purchase },
                        Name =
                            $"Trader Approval for Trader Purchase {tradeOrder.SalesChannel.GetDescription()} #{refFull}",
                        Qbicle = purchase.Workgroup.Qbicle,
                        Topic = purchase.Workgroup.Topic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        ReviewedBy = new List<ApplicationUser> { _traderCreatedAndApprovalBy },
                        ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                        StartedBy = _traderCreatedAndApprovalBy,
                        StartedDate = purchase.CreatedDate,
                        TimeLineDate = purchase.CreatedDate,
                        Notes = "",
                        App = QbicleActivity.ActivityApp.Trader,
                        IsVisibleInQbicleDashboard = true
                    };

                    purchase.PurchaseApprovalProcess = approval;
                    purchase.PurchaseApprovalProcess.ApprovalRequestDefinition = appDef;
                    approval.ActivityMembers.AddRange(purchase.Workgroup.Members);
                    dbContext.Entry(purchase).State = EntityState.Modified;

                    #endregion Create purchase Appoval

                    var result = dbContext.SaveChanges() > 0;
                    dbTransaction.Commit();

                    var traderOrderloggingRules = new TradeOrderLoggingRules(dbContext);
                    traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.PurchaseAdd, purchase.Id, "", null, "", false);
                    traderOrderloggingRules.TradeOrderLogging(TradeOrderLoggingType.PurchaseApproval, purchase.Id, approval.GetCreatedBy().Id, null, "", false);
                    return new ReturnJsonModel { result = result };
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrder);
                    return new ReturnJsonModel { result = false, msg = ex.Message };
                }
            }
        }

        private ReturnJsonModel CreateBill(TradeOrderB2B tradeOrder, Order orderJson)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(),
                        $"B2B: Create Bill {tradeOrder.SalesChannel.GetDescription()}", _traderCreatedAndApprovalBy.Id,
                        null, tradeOrder);

                var invoiceNew = new Invoice
                {
                    InvoiceItems = new List<InvoiceTransactionItems>(),
                    CreatedDate = orderJson.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                    AssociatedFiles = new List<QbicleMedia>(),
                    CreatedBy = _traderCreatedAndApprovalBy,
                    DueDate = orderJson.Status?.PaidDateTime?.ConvertTimeToUtc(_currentTimeZone) ?? DateTime.UtcNow,
                    PaymentDetails = $"Bill for {tradeOrder.SalesChannel.GetDescription()}",
                    Payments = new List<CashAccountTransaction>(),
                    Sale = null,
                    Purchase = tradeOrder.Purchase,
                    Status = TraderInvoiceStatusEnum.PendingReview,
                    Workgroup = tradeOrder.BillWorkGroup,
                    Reference = new TraderReferenceRules(dbContext).GetNewReference(tradeOrder.BuyingDomain.Id, TraderReferenceType.Bill)
                };

                tradeOrder.Purchase.PurchaseItems?.ForEach(i =>
                {
                    var item = new InvoiceTransactionItems
                    {
                        InvoiceDiscountValue = i.Discount,
                        InvoiceItemQuantity = i.Quantity,
                        InvoiceTaxValue = i.Taxes?.Sum(s => s.Value) ?? 0,
                        InvoiceValue = i.Cost,
                        TransactionItem = i
                    };
                    invoiceNew.InvoiceItems.Add(item);
                });

                if (invoiceNew.InvoiceItems != null && invoiceNew.InvoiceItems.Count > 0)
                    invoiceNew.TotalInvoiceAmount = invoiceNew.InvoiceItems.Sum(e => e.InvoiceValue);
                tradeOrder.Purchase.Invoices.Add(invoiceNew);

                dbContext.Invoices.Add(invoiceNew);
                dbContext.Entry(invoiceNew).State = EntityState.Added;

                tradeOrder.Bill = invoiceNew;
                dbContext.SaveChanges();

                var appDef = dbContext.PurchaseApprovalDefinitions.FirstOrDefault(p => p.WorkGroup.Id == invoiceNew.Workgroup.Id);
                var refFull = invoiceNew.Reference == null ? "" : invoiceNew.Reference.FullRef;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Invoice = new List<Invoice> { invoiceNew },
                    Name = $"Trader Approval for Trader Bill {tradeOrder.SalesChannel.GetDescription()} #{refFull}",
                    Qbicle = invoiceNew.Workgroup.Qbicle,
                    Topic = invoiceNew.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    ReviewedBy = new List<ApplicationUser>(),
                    ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                    StartedBy = _traderCreatedAndApprovalBy,
                    StartedDate = invoiceNew.CreatedDate,
                    TimeLineDate = invoiceNew.CreatedDate,
                    Notes = "",
                    App = QbicleActivity.ActivityApp.Trader,
                    IsVisibleInQbicleDashboard = true
                };

                invoiceNew.InvoiceApprovalProcess = approval;
                invoiceNew.InvoiceApprovalProcess.ApprovalRequestDefinition = appDef;
                approval.ActivityMembers.AddRange(invoiceNew.Workgroup.Members);
                dbContext.Entry(invoiceNew).State = EntityState.Modified;
                dbContext.SaveChanges();
                LogManager.ApplicationInfo(_logLabels, "Bill Approval created", null, null,
                                                       "BillRef: " + invoiceNew.Reference.FullRef);

                new BookkeepingIntegrationRules(dbContext).AddPurchaseNonInventoryJournalEntry(
                    _traderCreatedAndApprovalBy, invoiceNew);
                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrder);
                return new ReturnJsonModel { result = false, msg = ex.Message, actionVal = 1 };
            }
        }

        private async Task<ReturnJsonModel> CreatePurchaseTransfer(TradeOrderB2B tradeOrder)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create B2B Transfer",
                        _traderCreatedAndApprovalBy.Id);

                var tradTransfer = new TraderTransfer
                {
                    CreatedBy = _traderCreatedAndApprovalBy,
                    CreatedDate = DateTime.UtcNow,
                    DestinationLocation = tradeOrder.DestinationLocation,
                    OriginatingLocation = null,
                    Sale = null,
                    Purchase = tradeOrder.Purchase,
                    Workgroup = tradeOrder.PurchaseTransferWorkGroup,
                    Address = tradeOrder.DestinationLocation.Address,
                    Contact = tradeOrder.VendorTraderContact,
                    Status = TransferStatus.PendingPickup,
                    Reason = TransferReasonEnum.Purchase,
                    Reference = new TraderReferenceRules(dbContext).GetNewReference(tradeOrder.BuyingDomain.Id, TraderReferenceType.Transfer)
                };

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create Purchase Transfer",
                        _traderCreatedAndApprovalBy.Id, tradTransfer);

                var compressTransactionItems = CompressTransactionItems(tradeOrder.Purchase.PurchaseItems);
                foreach (var purchaseItem in compressTransactionItems)
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Create Pos TraderTransferItem",
                            _traderCreatedAndApprovalBy.Id, purchaseItem);
                    var unit = purchaseItem.TraderItem.Units.FirstOrDefault(s => s.IsBase);
                    tradTransfer.TransferItems.Add(new TraderTransferItem
                    {
                        Unit = unit,
                        QuantityAtPickup = purchaseItem.TotalQuantity,
                        QuantityAtDelivery = purchaseItem.TotalQuantity,
                        TransactionItem = purchaseItem.TransactionItem,
                        TraderItem = purchaseItem.TraderItem
                    });
                }

                dbContext.TraderTransfers.Add(tradTransfer);
                dbContext.Entry(tradTransfer).State = EntityState.Added;
                dbContext.SaveChanges();
                tradeOrder.PurchaseTransfer = tradTransfer;

                tradTransfer.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;

                #region Create transfer approval request

                var appDef =
                    dbContext.TransferApprovalDefinitions.FirstOrDefault(w =>
                        w.WorkGroup.Id == tradTransfer.Workgroup.Id);
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = tradTransfer.Workgroup.Qbicle,
                    Topic = tradTransfer.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    ReviewedBy = new List<ApplicationUser>(),
                    ApprovedOrDeniedAppBy = _traderCreatedAndApprovalBy,
                    StartedBy = _traderCreatedAndApprovalBy,
                    StartedDate = tradTransfer.CreatedDate,
                    TimeLineDate = tradTransfer.CreatedDate,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Transfer for B2B Order {tradeOrder.SalesChannel.GetDescription()} #" +
                        tradTransfer.Reference == null
                            ? ""
                            : tradTransfer.Reference.FullRef,
                    Transfer = new List<TraderTransfer> { tradTransfer }
                };

                tradTransfer.TransferApprovalProcess = approval;
                tradTransfer.TransferApprovalProcess.ApprovalRequestDefinition = appDef;

                approval.ActivityMembers.AddRange(tradTransfer.Workgroup.Members);
                dbContext.Entry(tradTransfer).State = EntityState.Modified;

                tradTransfer.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var transferLog = new TransferLog
                {
                    Address = tradTransfer.Address,
                    AssociatedTransfer = tradTransfer,
                    Contact = tradTransfer.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Sale = tradTransfer.Sale,
                    Status = tradTransfer.Status,
                    UpdatedBy = _traderCreatedAndApprovalBy,
                    AssociatedShipment = tradTransfer.AssociatedShipment,
                    DestinationLocation = tradTransfer.DestinationLocation,
                    OriginatingLocation = tradTransfer.OriginatingLocation,
                    TransferItems = tradTransfer.TransferItems,
                    Workgroup = tradeOrder.PaymentWorkGroup
                };

                var transferProcessLog = new TransferProcessLog
                {
                    AssociatedTransfer = tradTransfer,
                    AssociatedTransferLog = transferLog,
                    TransferStatus = tradTransfer.Status,
                    CreatedBy = _traderCreatedAndApprovalBy,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.TraderTransferProcessLogs.Add(transferProcessLog);
                dbContext.Entry(transferProcessLog).State = EntityState.Added;

                #endregion Create transfer approval request

                dbContext.SaveChanges();

                var loggingRules = new TradeOrderLoggingRules(dbContext);
                loggingRules.TradeOrderLogging(TradeOrderLoggingType.TransferAdd, tradTransfer.Id, "", null, "", false);
                loggingRules.TradeOrderLogging(TradeOrderLoggingType.TransferApproval, tradTransfer.Id, approval.GetCreatedBy().Id, null, "", false);

                //When a Transfer In occurs (a Purchase), after the Batches (In) have been created, that is when we have to calculate the costs for the inverntorydetail OUT of which the batches are leaving

                await new TraderTransfersRules(dbContext).IncomingInventory(tradTransfer, _traderCreatedAndApprovalBy);

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrder);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public async Task<ReturnJsonModel> ReProcessOrder(int tradeOderId)
        {
            var returnJson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOderId);
                var tradeOrder = dbContext.TradeOrders.Find(tradeOderId);
                if (tradeOrder.OrderStatus == TradeOrderStatusEnum.Processed)
                    return returnJson;
                //call to hangfire process order
                var job = new OrderJobParameter
                {
                    Id = tradeOrder.Id,
                    EndPointName = "processorder",
                    InvoiceDetail = "",
                    Address = ""
                };
                var resultHangFire = new QbiclesJob();
                await resultHangFire.HangFireExcecuteAsync(job);

                tradeOrder.OrderStatus = TradeOrderStatusEnum.InProcessing;
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOderId);
            }

            return returnJson;
        }

        private List<CompressTraderTransferItem> CompressTransactionItems(List<TraderTransactionItem> transactionItems)
        {
            return transactionItems.GroupBy(s => s.TraderItem,
                (key, g) => new CompressTraderTransferItem
                {
                    TraderItem = key,
                    TotalQuantity = g.Sum(p => p.Quantity * p.Unit.QuantityOfBaseunit),
                    TransactionItem = g.FirstOrDefault()
                }).ToList();
        }

        /// <summary>
        ///     Get the Order From TradeOrder
        /// </summary>
        /// <param name="tradeOrder"></param>
        /// <returns>
        ///     if salesChannel = B2C return tradeOrder.OrderJsonOrig for the case customer use the Vouhcher discount and business
        ///     change discount too
        ///     else return tradeOrder.OrderJson
        /// </returns>
        public Order GetOrderFromTradeOrder(TradeOrder tradeOrder)
        {
            var orderJson = tradeOrder.OrderJson;
            if (tradeOrder.SalesChannel == SalesChannelEnum.B2C && !string.IsNullOrEmpty(tradeOrder.OrderJsonOrig))
                orderJson = tradeOrder.OrderJsonOrig;
            return orderJson.ParseAs<Order>();
        }

        private void CreateB2cDicussionOrderSendMessage(ActivityNotification notification)
        {
            try
            {
                new B2CRules(dbContext).B2CDicussionOrderSendMessage(false, ResourcesManager._L("B2C_PROCESSED_ORDER"), notification.DiscussionId, notification.CreatedById, notification.QbicleId, "", true);
            }
            catch
            {
            }
        }
    }
}