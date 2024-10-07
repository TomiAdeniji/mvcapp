using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Data;
using Microsoft.Reporting.WebForms;
using Qbicles.BusinessRules.ReportTemplates;

namespace Qbicles.Doc.Controllers
{
    [Authorize]
    [RoutePrefix("api/reports")]
    public class ReportsController : ApiController
    {
        private string GemboxLicense = System.Configuration.ConfigurationManager.AppSettings["GemboxLicense"];



        #region Private Methods

        /// <summary>
        /// Generic function to convert Linq query to DataTable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        private DataTable LinqToDataTable<T>(IEnumerable<T> items)
        {
            //Create a DataTable with the Name of the Class i.e. Customer class.
            DataTable dt = new DataTable(typeof(T).Name);

            //Read all the properties of the Class i.e. Customer class.
            PropertyInfo[] propInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            //Loop through each property of the Class i.e. Customer class.
            foreach (PropertyInfo propInfo in propInfos)
            {
                //Add Columns in DataTable based on Property Name and Type.
                dt.Columns.Add(new DataColumn(propInfo.Name, propInfo.PropertyType));
            }

            //Loop through the items if the Collection.
            foreach (T item in items)
            {
                //Add a new Row to DataTable.
                DataRow dr = dt.Rows.Add();

                //Loop through each property of the Class i.e. Customer class.
                foreach (PropertyInfo propInfo in propInfos)
                {
                    //Add value Column to the DataRow.
                    dr[propInfo.Name] = propInfo.GetValue(item, null);
                }
            }

            return dt;
        }

        private byte[] GenerateDataReport(IEnumerable<VirtualTillTransactionReport> data, QbicleDomain domain, string imageTop, string imageBottom, string timezone, CurrencySetting setting, string reportFileName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, data, imageTop, imageBottom, timezone, setting);

                #region Bind data Report

                //var sale = iv.Sale;

                //Data items
                decimal totalValue = 0;
                decimal totalTax = 0;

                //Order info

                var location = domain.TraderLocations.FirstOrDefault(x => x.IsDefaultAddress);
                var address = location.Address;
                var addressBilling = location.Address;
                var lsOrderInfo = new List<ReportOrderInfo>();
                var orderInfo = new ReportOrderInfo
                {
                    FullRef = new RandomGenerator().RandomString(10),
                    AdditionalInformation = "",
                    OrderDate = DateTime.Now.ConvertTimeFromUtc(timezone).ToString("dd MMM, yyyy")
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

                //var dataSource = new List<ReportDataSource>
                //{
                //    new ReportDataSource {Name = "VirtualTill", Value = data},
                //    new ReportDataSource {Name = "OrderInfo", Value = lsOrderInfo}
                //};

                //return ReportRules.RenderReport(dataSource, reportFileName);

                return new byte[] { };

                #endregion Bind data Report
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, data, imageTop, imageBottom, timezone, setting);
                return null;
            }
        }

        /// <summary>
        /// Helper method to safely extract the value from the string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ExtractHtmlValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var splitStart = value.IndexOf('>') + 1;
            var splitEnd = value.IndexOf('<');

            // Ensure that the value contains both delimiters '>' and '<'
            if (splitStart > 0 && splitEnd > splitStart)
            {
                return value.Substring(splitStart, splitEnd - splitStart);
            }

            return value;
        }

        private enum ExportType
        {
            Excel = 1,
            Csv,
            Pdf
        }

        #endregion Private Methods
    }
}