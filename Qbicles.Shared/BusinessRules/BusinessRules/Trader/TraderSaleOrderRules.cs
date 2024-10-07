using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader;
using System;
using System.Data.Entity;
using Qbicles.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Reporting.WebForms;
using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderSaleOrderRules
    {
        ApplicationDbContext _db;

        public TraderSaleOrderRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();

        public TraderSalesOrder GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                return DbContext.TraderSalesOrders.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderSalesOrder();
            }
        }



        public ReturnJsonModel SaveTraderSaleOrder(TraderSalesOrder salesOrder, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0", result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, salesOrder);


                salesOrder.Reference = new TraderReferenceRules(DbContext).GetById(salesOrder.Reference.Id);

                var tradSale = DbContext.TraderSales.Find(salesOrder.Sale.Id);
                if (tradSale == null) return result;
                tradSale.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var sOrder = new TraderSalesOrder
                {
                    AdditionalInformation = salesOrder.AdditionalInformation,
                    CreatedBy = DbContext.QbicleUser.Find(userId),
                    Reference = salesOrder.Reference,
                    CreatedDate = DateTime.UtcNow,
                    Sale = tradSale
                };

                DbContext.TraderSalesOrders.Add(sOrder);
                DbContext.Entry(sOrder).State = EntityState.Added;

                DbContext.SaveChanges();
                result.result = true;
                result.msgId = sOrder.Id.ToString();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, salesOrder);
                result.result = false;
                result.actionVal = 3;
                result.msg = e.Message;
            }
            return result;
        }

        public ReturnJsonModel IssueSalesOrder(int id, string invoiceGuid)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, invoiceGuid);
                var saleOrder = GetById(id);
                saleOrder.SalesOrderPDF = invoiceGuid;
                DbContext.Entry(saleOrder).State = EntityState.Modified;


                var saleRule = new TraderSaleRules(DbContext);
                var sale = saleRule.GetById(saleOrder.Sale.Id);
                sale.Status = TraderSaleStatusEnum.SalesOrderedIssued;
                DbContext.Entry(sale).State = EntityState.Modified;


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
        
        public byte[] ReportSaleOrder(TraderSalesOrder iv, string imageTop, string imageBottom, string timezone, CurrencySetting setting)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, iv, imageTop, imageBottom, timezone, setting);
                #region Bind data Report
                var sale = iv.Sale;
                
                //product items
                var lst = new List<ReportProductItem>();
                decimal totalValue = 0;
                decimal totalTax = 0;
                if (sale != null && sale.SaleItems.Any())
                    foreach (var item in sale.SaleItems)
                    {
                        ReportProductItem productItem = new ReportProductItem();
                        productItem.Item = item.TraderItem?.Name;
                        productItem.Unit = item.Unit?.Name;
                        productItem.Quantity = item.Quantity.ToDecimalPlace(setting);
                        productItem.UnitPrice = item.SalePricePerUnit.ToDecimalPlace(setting);
                        productItem.Discount = item.Discount.ToDecimalPlace(setting) + "%";
                        productItem.Tax = item.StringTaxRates(setting);
                        productItem.Total = item.Price.ToDecimalPlace(setting);
                        lst.Add(productItem);
                        var discount = item.Quantity * item.SalePricePerUnit * (item.Discount / 100);

                        var taxValue = (item.Quantity * item.SalePricePerUnit - discount) * item.TraderItem.SumTaxRates(true);
                        totalTax += taxValue;
                        totalValue += taxValue - discount + item.Quantity * item.SalePricePerUnit;
                    }
                
                //Order info

                var address = sale.Workgroup?.Location?.Address;
                var addressBilling = sale?.Purchaser?.Address;
                var lsOrderInfo = new List<ReportOrderInfo>();
                var orderInfo = new ReportOrderInfo
                {
                    FullRef = iv.Reference != null ? iv.Reference.FullRef : sale.Id.ToString(),
                    AdditionalInformation = sale.SalesOrders.FirstOrDefault()?.AdditionalInformation,
                    OrderDate = sale.CreatedDate.ConvertTimeFromUtc(timezone).ToString("dd MMM, yyyy")
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
                orderInfo.Total = totalValue.ToCurrencySymbol(setting);
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
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, iv, imageTop, imageBottom, timezone, setting);
                return null;
            }
        }
    }
}
