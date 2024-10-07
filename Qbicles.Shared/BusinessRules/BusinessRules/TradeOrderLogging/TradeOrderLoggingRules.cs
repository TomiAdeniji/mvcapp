using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.OrderLogging;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.BusinessRules.TradeOrderLogging
{
	public class TradeOrderLoggingRules
	{
		private ApplicationDbContext _dbContext;

		public TradeOrderLoggingRules(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="loggingType">TradeOrderLoggingType
		/// Type add need tradeId only
		/// Type approval need tradeId and created id
		/// </param>
		/// <param name="tradeId">Id of trader (sale,purchase,transfer,invoice,payment,...) associated to tradeOrder</param>
		/// <param name="createdById"></param>
		/// <param name="queueOrderIds"></param>
		public void TradeOrderLogging(TradeOrderLoggingType loggingType, int tradeId, string createdById = "", List<int> queueOrderIds = null, string message = "", bool sendToQueue = true)
		{
			var job = new TradeOrderLoggingParameter
			{
				Type = loggingType,
				EndPointName = "scheduletradeorderlogging",
				TradeId = tradeId,
				CreatedById = createdById,
				QueueOrderIds = queueOrderIds,
				Message = message
			};

			if (!sendToQueue)
			{
				TradeOrderLoggingExecute(job);
				return;
			}

			//Send to hit database directly
			Task.Run(() => new QbiclesJob().ScheduleTradeOrderLogging(job));

			//Task tskHangfire = new Task(async () =>
			//{
			//    await new QbiclesJob().HangFireExcecuteAsync(job);
			//});
			//tskHangfire.Start();
		}

		public void TradeOrderLogging2PaymentAdd(TradeOrderLoggingType loggingType, int tradeId, bool isCutomer)
		{
			var job = new TradeOrderLoggingParameter
			{
				Type = loggingType,
				EndPointName = "scheduletradeorderlogging",
				TradeId = tradeId,
				IsCutomer = isCutomer
			};

			Task tskHangfire = new Task(async () =>
			{
				await new QbiclesJob().HangFireExcecuteAsync(job);
			});
			tskHangfire.Start();
		}

		public void TradeOrderLoggingExecute(TradeOrderLoggingParameter request)
		{
			switch (request.Type)
			{
				case TradeOrderLoggingType.SaleAdd:
					LogSaleAdded(request.TradeId);
					break;

				case TradeOrderLoggingType.SaleApproval:
					LogSaleApprovalStatusUpdate(request.TradeId, request.CreatedById);
					break;

				case TradeOrderLoggingType.PurchaseAdd:
					LogPurchaseAdded(request.TradeId);
					break;

				case TradeOrderLoggingType.PurchaseApproval:
					LogPurchaseApprovalStatusUpdate(request.TradeId, request.CreatedById);
					break;

				case TradeOrderLoggingType.InvoiceAdd:
					LogInvoiceAdded(request.TradeId);
					break;

				case TradeOrderLoggingType.InvoiceApproval:
					LogInvoiceApprovalStatusUpdate(request.TradeId, request.CreatedById);
					break;

				case TradeOrderLoggingType.PaymentAdd:
					LogPaymentAdded(request.TradeId, request.IsCutomer);
					break;

				case TradeOrderLoggingType.PaymentApproval:
					LogPaymentApprovalStatusUpdate(request.TradeId, request.CreatedById);
					break;

				case TradeOrderLoggingType.TransferAdd:
					LogTransferAdded(request.TradeId);
					break;

				case TradeOrderLoggingType.TransferApproval:
					LogTransferApprovalStatusUpdate(request.TradeId, request.CreatedById);
					break;

				case TradeOrderLoggingType.Preparation:
					LogPreparationStatusUpdate(request.QueueOrderIds, request.CreatedById);
					break;

				case TradeOrderLoggingType.DeliveryStatus:
					LogDeliveryStatusUpdate(request.TradeId, request.CreatedById);
					break;

				case TradeOrderLoggingType.DriverSendMessage:
					DriverSendMessage(request.TradeId, request.Message, request.CreatedById);
					break;

				default:
					break;
			}
		}

		private void AddOrderDiscussionPost(TradeOrderLogEvent loggingEvent, bool isCutomer = false)
		{
			AddOrderPost(loggingEvent.TradeOrder.Id, loggingEvent.Description, loggingEvent.CreatedBy, isCutomer);
		}

		private void AddOrderDiscussionPost(int tradeOrderId, string message, ApplicationUser createdBy, bool isCutomer = false)
		{
			AddOrderPost(tradeOrderId, message, createdBy, isCutomer);
		}

		private void AddOrderPost(int tradeOrderId, string message, ApplicationUser createdBy, bool isCutomer)
		{
			var b2cOrder = _dbContext.B2COrderCreations.FirstOrDefault(d => d.TradeOrder.Id == tradeOrderId);
			//B2B and B2C use same this controller. 1 tradeOrderId only find either b2corder or  b2border
			var b2bOrder = _dbContext.B2BOrderCreations.FirstOrDefault(d => d.TradeOrder.Id == tradeOrderId);

			if (b2cOrder == null && b2bOrder == null)
				return;

			var discussion = new QbicleDiscussion();
			if (b2bOrder == null)
			{
				discussion = (QbicleDiscussion)b2cOrder;
			}
			else
			{
				discussion = (QbicleDiscussion)b2bOrder;
			}

			if (discussion.Qbicle == null)
				return;

			var post = new PostsRules(_dbContext).SavePost(isCutomer, message, createdBy.Id, discussion.Qbicle.Id);

			// Call the AddComment for the discusison that does NOT add the comment creation to a Queue
			// The method that is making the call to AddOrderPost is already being called from a Queue, we do not want multiple HangFire queues involved
			new DiscussionsRules(_dbContext).AddCommentActivity((QbicleActivity)discussion, post, "", ApplicationPageName.DiscussionOrder, false, false);
		}

		#region Sale

		private void LogSaleApprovalStatusUpdate(int tradeId, string createdById)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(t => t.Sale.Id == tradeId);
			var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);
			if (tradeOrder != null)
			{
				var tradeOrderEvent = new SaleApprovalStatusUpdate(tradeOrder, tradeOrder.Sale)
				{
					CreatedBy = createdBy
				};
				tradeOrderEvent.SetDescription(tradeOrder.Sale.Status.GetDescription());

				_dbContext.SaleApprovalStatusUpdates.Add(tradeOrderEvent);
				_dbContext.SaveChanges();

				AddOrderDiscussionPost(tradeOrderEvent);
			}
		}

		private void LogSaleAdded(int tradeId)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(e => e.Sale.Id == tradeId);
			var tradeOrderEvent = new SaleAdded(tradeOrder, tradeOrder.Sale);
			tradeOrderEvent.SetDescription();

			_dbContext.SaleAddeds.Add(tradeOrderEvent);
			_dbContext.SaveChanges();

			AddOrderDiscussionPost(tradeOrderEvent);
		}

		#endregion Sale

		#region Purchase

		private void LogPurchaseApprovalStatusUpdate(int tradeId, string createdById)
		{
			var tradeOrder = _dbContext.B2BTradeOrders.FirstOrDefault(t => t.Purchase.Id == tradeId);
			var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);
			if (tradeOrder != null)
			{
				var tradeOrderEvent = new PurchaseApprovalStatusUpdate(tradeOrder, tradeOrder.Purchase)
				{
					CreatedBy = createdBy
				};

				tradeOrderEvent.SetDescription(tradeOrder.Purchase.Status.GetDescription());

				_dbContext.PurchaseApprovalStatusUpdates.Add(tradeOrderEvent);
				_dbContext.SaveChanges();

				AddOrderDiscussionPost(tradeOrderEvent);
			}
		}

		private void LogPurchaseAdded(int tradeId)
		{
			var tradeOrder = _dbContext.B2BTradeOrders.FirstOrDefault(t => t.Purchase.Id == tradeId);

			var tradeOrderEvent = new PurchaseAdded(tradeOrder, tradeOrder.Purchase);
			tradeOrderEvent.SetDescription();

			_dbContext.PurchaseAddeds.Add(tradeOrderEvent);
			_dbContext.SaveChanges();

			AddOrderDiscussionPost(tradeOrderEvent);
		}

		#endregion Purchase

		#region Invoice

		private void LogInvoiceApprovalStatusUpdate(int tradeId, string createdById)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(t => t.Invoice.Id == tradeId);
			var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);

			if (tradeOrder != null)
			{
				var tradeOrderEvent = new InvoiceApprovalStatusUpdate(tradeOrder, tradeOrder.Invoice)
				{
					CreatedBy = createdBy
				};

				tradeOrderEvent.SetDescription(tradeOrder.Invoice.Status.GetDescription());

				_dbContext.InvoiceApprovalStatusUpdates.Add(tradeOrderEvent);
				_dbContext.SaveChanges();

				AddOrderDiscussionPost(tradeOrderEvent);
			}
		}

		private void LogInvoiceAdded(int tradeId)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(t => t.Invoice.Id == tradeId);
			var tradeOrderEvent = new InvoiceAdded(tradeOrder, tradeOrder.Invoice);
			tradeOrderEvent.SetDescription();

			_dbContext.InvoiceAddeds.Add(tradeOrderEvent);
			_dbContext.SaveChanges();

			AddOrderDiscussionPost(tradeOrderEvent);
		}

		#endregion Invoice

		#region Payment

		private void LogPaymentApprovalStatusUpdate(int tradeId, string createdById)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(p => p.Payments.Select(i => i.Id).Contains(tradeId));
			//B2B trader - PaymentID from BuyerDomain is different PaymentID from SellerDomain.
			if (tradeOrder == null)
			{
				tradeOrder = _dbContext.B2BTradeOrders.FirstOrDefault(p => p.PurchasePayments.Select(i => i.Id).Contains(tradeId));
			}
			var payment = _dbContext.CashAccountTransactions.FirstOrDefault(p => p.Id == tradeId);
			if (tradeOrder != null)
			{
				var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);
				var tradeOrderEvent = new PaymentApprovalStatusUpdate(tradeOrder, payment)
				{
					CreatedBy = createdBy
				};

				tradeOrderEvent.SetDescription(payment.Status.GetDescription());

				_dbContext.PaymentApprovalStatusUpdates.Add(tradeOrderEvent);
				_dbContext.SaveChanges();

				new NotificationRules(_dbContext).SignalRB2COrderProcess(tradeOrder, "", NotificationEventEnum.B2COrderPaymentApproved);

				AddOrderDiscussionPost(tradeOrderEvent);
			}
		}

		private void LogPaymentAdded(int tradeId, bool isCutomer)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(p => p.Payments.Select(i => i.Id).Contains(tradeId));
			//B2B trader - PaymentID from BuyerDomain is different PaymentID from SellerDomain.
			if (tradeOrder == null)
			{
				tradeOrder = _dbContext.B2BTradeOrders.FirstOrDefault(p => p.PurchasePayments.Select(i => i.Id).Contains(tradeId));
			}
			var payment = _dbContext.CashAccountTransactions.FirstOrDefault(p => p.Id == tradeId);
			var tradeOrderEvent = new PaymentAdded(tradeOrder, payment);
			tradeOrderEvent.SetDescription();

			_dbContext.PaymentAddeds.Add(tradeOrderEvent);
			_dbContext.SaveChanges();

			AddOrderDiscussionPost(tradeOrderEvent, isCutomer);
		}

		#endregion Payment

		#region Transfer

		private void LogTransferApprovalStatusUpdate(int tradeId, string createdById)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(p => p.Transfer.Id == tradeId);
			if (tradeOrder != null)
			{
				var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);
				var tradeOrderEvent = new TransferApprovalStatusUpdate(tradeOrder, tradeOrder.Transfer)
				{
					CreatedBy = createdBy
				};

				tradeOrderEvent.SetDescription(tradeOrder.Transfer.Status.GetDescription());

				_dbContext.TransferApprovalStatusUpdates.Add(tradeOrderEvent);
				_dbContext.SaveChanges();

				AddOrderDiscussionPost(tradeOrderEvent);
			}
		}

		private void LogTransferAdded(int tradeId)
		{
			var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(p => p.Transfer.Id == tradeId);
			//B2B and B2C use same this controller
			//B2B Consumer uses attribute PurchseTransfer, Providers(Include B2B, B2C) use attribute PruchaseTransfer
			if (tradeOrder == null)
			{
				var tradeOrderB2B = _dbContext.B2BTradeOrders.FirstOrDefault(p => p.PurchaseTransfer.Id == tradeId);
				var tradeOrderEvent = new TransferAdded(tradeOrderB2B, tradeOrderB2B.PurchaseTransfer);
				tradeOrderEvent.SetDescription();
				_dbContext.TransferAddeds.Add(tradeOrderEvent);
				_dbContext.SaveChanges();
				AddOrderDiscussionPost(tradeOrderEvent);
			}
			else
			{
				var tradeOrderEvent = new TransferAdded(tradeOrder, tradeOrder.Transfer);
				tradeOrderEvent.SetDescription();
				_dbContext.TransferAddeds.Add(tradeOrderEvent);
				_dbContext.SaveChanges();
				AddOrderDiscussionPost(tradeOrderEvent);
			}
		}

		#endregion Transfer

		#region Preparation

		private void LogPreparationStatusUpdate(List<int> queueOrderIds, string createdById)
		{
			var queueOrders = _dbContext.QueueOrders.Where(e => queueOrderIds.Contains(e.Id)).ToList();

			var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);
			queueOrders.ForEach(queueOrder =>
			{
				var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(p => p.LinkedOrderId == queueOrder.LinkedOrderId);
				if (tradeOrder != null)
				{
					var tradeOrderEvent = new PreparationStatusUpdate(tradeOrder, queueOrder)
					{
						CreatedBy = createdBy
					};

					tradeOrderEvent.SetDescription(queueOrder.Status.GetDescription());

					_dbContext.PreparationStatusUpdates.Add(tradeOrderEvent);
					_dbContext.SaveChanges();

					AddOrderDiscussionPost(tradeOrderEvent);
				}
			});
		}

		#endregion Preparation

		#region Delivery

		private void LogDeliveryStatusUpdate(int tradeId, string createdById)
		{
			//var tradeOrdersX = _dbContext.TradeOrders.Where(p => p.LinkedOrderId == p.PrepDeliveryOrder.LinkedOrderId).ToList();

			var delivery = _dbContext.Deliveries.FirstOrDefault(e => e.Id == tradeId);
			var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);

			var linkedOrderIds = delivery.Orders.Select(o => o.LinkedOrderId).ToList();

			var tradeOrders = _dbContext.TradeOrders.Where(p => linkedOrderIds.Contains(p.LinkedOrderId)).ToList();

			tradeOrders.ForEach(tradeOrder =>
			{
				var tradeOrderEvent = new DeliveryStatusUpdate(tradeOrder, delivery.Status)
				{
					CreatedBy = createdBy
				};

				tradeOrderEvent.SetDescription(delivery.Status.GetDescription());

				_dbContext.DeliveryStatusUpdates.Add(tradeOrderEvent);
				_dbContext.SaveChanges();

				// Only add the information to the Order discussion if the delivery has one of teh following statuses
				if ((delivery.Status == DeliveryStatus.Started)
					||
					(delivery.Status == DeliveryStatus.Completed)
					||
					(delivery.Status == DeliveryStatus.CompletedWithProblems)
					)
					AddOrderDiscussionPost(tradeOrderEvent);

				//if the delivery status is started
				if (delivery.Status == DeliveryStatus.Started)
				{
					//for any order in the delivery that has a completed or completed with problems status
					// set TradeOrder.IsInTransit to false
					if (delivery.Orders.Any(o => o.Status == PrepQueueStatus.Completed || o.Status == PrepQueueStatus.CompletedWithProblems))
						tradeOrder.IsInTransit = false;
					//for an order in the delivery does not have a completed or completed with problems status
					//set TradeOrder.IsInTransit to true
					else if (delivery.Orders.Any(o => o.Status != PrepQueueStatus.Completed && o.Status != PrepQueueStatus.CompletedWithProblems))
						tradeOrder.IsInTransit = true;
					_dbContext.SaveChanges();
				}
				else //if the delivery status is anything else
				{
					//for any order in the delivery set TradeOrder.IsInTransit to false
					tradeOrder.IsInTransit = false;
					_dbContext.SaveChanges();
				}
			});
		}

		private void DriverSendMessage(int queueOrderId, string message, string createdById)
		{
			var createdBy = _dbContext.QbicleUser.FirstOrDefault(t => t.Id == createdById);
			var linkedOrderId = _dbContext.QueueOrders.AsNoTracking().FirstOrDefault(o => o.Id == queueOrderId)?.LinkedOrderId ?? "";
			var orderId = _dbContext.TradeOrders.AsNoTracking().FirstOrDefault(o => o.LinkedOrderId == linkedOrderId)?.Id ?? 0;

			AddOrderDiscussionPost(orderId, message, createdBy);
		}

		#endregion Delivery
	}
}