using System;
using Qbicles.BusinessRules.Model;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models.Trader;
using Qbicles.Models;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderReferenceRules
    {
        private ApplicationDbContext dbContext;

        public TraderReferenceRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public TraderReference GetNewReference(int domainId, TraderReferenceType type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, type);
                var traderSetting = dbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                string prefix = "";
                string suffix = "";
                if (traderSetting != null)
                {
                    switch (type)
                    {
                        case TraderReferenceType.Sale:
                            prefix = traderSetting.SalePrefix;
                            suffix = traderSetting.SaleSuffix;
                            break;
                        case TraderReferenceType.SalesOrder:
                            prefix = traderSetting.SalesOrderPrefix;
                            suffix = traderSetting.SalesOrderSuffix;
                            break;
                        case TraderReferenceType.Purchase:
                            prefix = traderSetting.PurchasePrefix;
                            suffix = traderSetting.PurchaseSuffix;
                            break;
                        case TraderReferenceType.PurchaseOrder:
                            prefix = traderSetting.PurchaseOrderPrefix;
                            suffix = traderSetting.PurchaseOrderSuffix;
                            break;
                        case TraderReferenceType.Transfer:
                            prefix = traderSetting.TransferPrefix;
                            suffix = traderSetting.TransferSuffix;
                            break;
                        case TraderReferenceType.ManuJob:
                            prefix = traderSetting.ManuJobPrefix;
                            suffix = traderSetting.ManuJobSuffix;
                            break;
                        case TraderReferenceType.Invoice:
                            prefix = traderSetting.InvoicePrefix;
                            suffix = traderSetting.InvoiceSuffix;
                            break;
                        case TraderReferenceType.Bill:
                            prefix = traderSetting.BillPrefix;
                            suffix = traderSetting.BillSuffix;
                            break;
                        case TraderReferenceType.Allocation:
                            prefix = traderSetting.AllocationPrefix;
                            suffix = traderSetting.AllocationSuffix;
                            break;
                        case TraderReferenceType.CreditNote:
                            prefix = traderSetting.CreditNotePrefix;
                            suffix = traderSetting.CreditNoteSuffix;
                            break;
                        case TraderReferenceType.DebitNote:
                            prefix = traderSetting.DebitNotePrefix;
                            suffix = traderSetting.DebitNoteSuffix;
                            break;
                        case TraderReferenceType.SaleReturn:
                            prefix = traderSetting.SaleReturnPrefix;
                            suffix = traderSetting.SaleReturnSuffix;
                            break;
                        case TraderReferenceType.Reorder:
                            prefix = traderSetting.ReorderPrefix;
                            suffix = traderSetting.ReorderSuffix;
                            break;
                        case TraderReferenceType.Order:
                            prefix = traderSetting.OrderPrefix;
                            suffix = traderSetting.OrderSuffix;
                            break;
                        case TraderReferenceType.Payment:
                            prefix = traderSetting.PaymentPrefix;
                            suffix = traderSetting.PaymentSuffix;
                            break;
                        case TraderReferenceType.Delivery:
                            prefix = traderSetting.DeliveryPrefix;
                            suffix = traderSetting.DeliverySuffix;
                            break;
                        case TraderReferenceType.AlertGroup:
                            prefix = traderSetting.AlertGroupPrefix;
                            suffix = traderSetting.AlertGroupSuffix;
                            break;
                        case TraderReferenceType.AlertReport:
                            prefix = traderSetting.AlertReportPrefix;
                            suffix = traderSetting.AlertReportSuffix;
                            break;
                    }
                }

                var traderReference = new TraderReference
                {
                    Domain = traderSetting?.Domain??dbContext.Domains.Find(domainId),
                    Type = type,
                    Prefix = prefix,
                    Suffix = suffix,
                    Delimeter = traderSetting?.Delimeter??"",
                    FullRef = "",
                    NumericPart = 0
                };

                dbContext.TraderReferences.Add(traderReference);
                dbContext.Entry(traderReference).State = EntityState.Added;
                dbContext.SaveChanges();

                dbContext.Entry(traderReference).Reload();

                //var reference = dbContext.TraderReferences.FirstOrDefault(e => e.Id == traderReference.Id);
                return traderReference;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, type);
                return null;
            }
        }

        public TraderReference GetById(int id)
        {
            return dbContext.TraderReferences.Find(id);
        }
    }
}