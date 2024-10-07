using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader;
using System;
using System.Data.Entity;
using Qbicles.Models;
using System.Collections.Generic;
using Microsoft.Reporting.WebForms;
using System.Linq;
using System.Reflection;
using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderPurchaseOrderRules
    {
        ApplicationDbContext _db;

        public TraderPurchaseOrderRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();

        public TraderPurchaseOrder GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderPurchaseOrders.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderPurchaseOrder();
            }

        }



        public ReturnJsonModel SaveTraderPurchaseOrder(TraderPurchaseOrder purchaseOrder, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0", result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, purchaseOrder);


                if (purchaseOrder.Reference != null)
                    purchaseOrder.Reference = new TraderReferenceRules(DbContext).GetById(purchaseOrder.Reference.Id);

                var tradPurchase = DbContext.TraderPurchases.Find(purchaseOrder.Purchase.Id);
                if (tradPurchase == null) return result;
                tradPurchase.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var sOrder = new TraderPurchaseOrder
                {
                    AdditionalInformation = purchaseOrder.AdditionalInformation,
                    CreatedBy = DbContext.QbicleUser.Find(userId),
                    Reference = purchaseOrder.Reference,
                    CreatedDate = DateTime.UtcNow,
                    Purchase = tradPurchase
                };


                DbContext.TraderPurchaseOrders.Add(sOrder);
                DbContext.Entry(sOrder).State = EntityState.Added;

                DbContext.SaveChanges();
                result.result = true;
                result.msgId = sOrder.Id.ToString();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, purchaseOrder);
                result.result = false;
                result.actionVal = 3;
                result.msg = e.Message;
            }
            return result;
        }

        public ReturnJsonModel IssuepurchaseOrder(int id, string invoiceGuid)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, invoiceGuid);
                var purchaseOrder = GetById(id);
                purchaseOrder.PurchaseOrderPDF = invoiceGuid;
                DbContext.Entry(purchaseOrder).State = EntityState.Modified;


                var purchseRule = new TraderPurchaseRules(DbContext);
                var purchase = purchseRule.GetById(purchaseOrder.Purchase.Id);
                purchase.Status = TraderPurchaseStatusEnum.PurchaseOrderIssued;
                DbContext.Entry(purchase).State = EntityState.Modified;


                DbContext.SaveChanges();
                refModel.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, invoiceGuid);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }
            return refModel;
        }
        public byte[] ReportSalePurchase(TraderPurchaseOrder iv, string imageTop, string imageBottom, string timezone, CurrencySetting setting)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, iv, imageTop, imageBottom, timezone, setting);
                #region Bind data Report
                var purchase = iv.Purchase;
                //product items
                List<ReportProductItem> lst = new List<ReportProductItem>();
                decimal totalValue = 0;
                decimal totalTax = 0;
                if (purchase != null && purchase.PurchaseItems.Any())
                    foreach (var item in purchase.PurchaseItems)
                    {
                        ReportProductItem productItem = new ReportProductItem();
                        productItem.Item = item.TraderItem?.Name;
                        productItem.Unit = item.Unit?.Name;
                        productItem.Quantity = item.Quantity.ToDecimalPlace(setting);
                        productItem.UnitPrice = item.CostPerUnit.ToDecimalPlace(setting);
                        productItem.Discount = item.Discount.ToDecimalPlace(setting) + "%";
                        productItem.Tax = item.StringTaxRates(setting);
                        productItem.Total = item.Cost.ToDecimalPlace(setting);
                        lst.Add(productItem);
                        var taxRate = item.TraderItem.SumTaxRates(false);
                        var discount = item.Quantity * item.CostPerUnit * (item.Discount / 100);

                        var taxValue = (item.Quantity * item.CostPerUnit - discount) * taxRate;
                        totalTax += taxValue;
                        totalValue += taxValue - discount + item.Quantity * item.CostPerUnit;
                    }
                //Order info

                var address = purchase.Workgroup?.Location?.Address;
                var addressBilling = purchase?.Vendor?.Address;
                var lsOrderInfo = new List<ReportOrderInfo>();
                var orderInfo = new ReportOrderInfo
                {
                    FullRef = iv.Reference != null ? iv.Reference.FullRef : purchase.Id.ToString(),
                    AdditionalInformation = purchase.PurchaseOrder.FirstOrDefault()?.AdditionalInformation,
                    OrderDate = purchase.CreatedDate.ConvertTimeFromUtc(timezone).ToString("dd MMM, yyyy")
                };
                if (!string.IsNullOrEmpty(address?.AddressLine1))
                    orderInfo.AddressLine = address?.AddressLine1 + Environment.NewLine;
                if (!string.IsNullOrEmpty(address?.AddressLine2))
                    orderInfo.AddressLine += (address?.AddressLine2 + Environment.NewLine);
                if (!string.IsNullOrEmpty(address?.City))
                    orderInfo.AddressLine += (address?.City + Environment.NewLine);
                if (!string.IsNullOrEmpty(address?.State))
                    orderInfo.AddressLine += (address?.State + Environment.NewLine);
                if (!string.IsNullOrEmpty(address?.Country.ToString()))
                    orderInfo.AddressLine += (address?.Country.ToString() + Environment.NewLine);
                if (!string.IsNullOrEmpty(addressBilling?.AddressLine1))
                    orderInfo.BillingAddressLine = addressBilling?.AddressLine1 + Environment.NewLine;
                if (!string.IsNullOrEmpty(addressBilling?.AddressLine2))
                    orderInfo.BillingAddressLine += (addressBilling?.AddressLine2 + Environment.NewLine);
                if (!string.IsNullOrEmpty(addressBilling?.City))
                    orderInfo.BillingAddressLine += (addressBilling?.City + Environment.NewLine);
                if (!string.IsNullOrEmpty(addressBilling?.State))
                    orderInfo.BillingAddressLine += (addressBilling?.State + Environment.NewLine);
                if (!string.IsNullOrEmpty(addressBilling?.Country.ToString()))
                    orderInfo.BillingAddressLine += (addressBilling?.Country.ToString() + Environment.NewLine);
                orderInfo.SalesTax = totalTax.ToCurrencySymbol(setting);
                orderInfo.Total = totalValue.ToDecimalPlace(setting);
                orderInfo.Subtotal = (totalValue - totalTax).ToCurrencySymbol(setting);
                orderInfo.ImageTop = imageTop;
                orderInfo.ImageBottom = imageBottom;
                orderInfo.CurrencySymbol = setting.CurrencySymbol;
                lsOrderInfo.Add(orderInfo);

                var dataSource = new List<ReportDataSource>
                {
                    new ReportDataSource {Name = "ProductItems", Value = lst},
                    new ReportDataSource {Name = "OrderInfo", Value = lsOrderInfo}
                };

                return ReportRules.RenderReport(dataSource, ReportFileName.SaleOrder);


                #endregion
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
    }
}
