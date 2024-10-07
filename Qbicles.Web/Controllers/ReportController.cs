using GemBox.Spreadsheet;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.CashMgt;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        private string GemboxLicense = System.Configuration.ConfigurationManager.AppSettings["GemboxLicense"];

        #region Table Content Export Functionality

        /// <summary>
        /// Order management export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="locationId"></param>
        /// <param name="saleChannelStr"></param>
        /// <param name="daterange"></param>
        /// <param name="keyword"></param>
        /// <param name="isCompletedShownOnly"></param>
        /// <param name="export"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetOrderTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int locationId,
            string saleChannelStr, string daterange, string keyword, bool isCompletedShownOnly, ExportType export)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Order Management Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var saleChannels = JsonConvert.DeserializeObject<List<SalesChannelEnum>>(saleChannelStr);
                var settings = CurrentUser();
                var dateTimeFormat = settings.DateTimeFormat;
                var dateFormat = settings.DateFormat;
                var timeZone = settings.Timezone;
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);

                //update models
                requestModel.Length = 1000;
                var records = new OrderManagementRules(dbContext).GetListOrderPagination(requestModel, locationId, saleChannels, daterange,
                                                                keyword, isCompletedShownOnly, dateFormat, dateTimeFormat, timeZone, currencySettings);

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    // Select the necessary records
                    var dataRecords = (List<OrderManagementCustomModel>)records.data;
                    var selectedRecords = dataRecords.Select(x => new OrderManagementReport
                    {
                        Order = x.OrderRef,
                        Location = x.LocationName,
                        SaleChannel = x.SaleChannel,
                        Items = x.ItemCount,
                        Total = x.Total,
                        Status = x.Status,
                        Queued = x.QueuedInfo,
                        Pending = ExtractHtmlValue(x.Pending),
                        Preparing = ExtractHtmlValue(x.Preparing),
                        Completion = x.Completion,
                        DeliveryStatus = x.DeliveryStatus,
                        Payment = x.PaidStatus
                    }).ToList();

                    //Init dataTable
                    var dt = LinqToDataTable<OrderManagementReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = selectedRecords.Sum(x => x.Total);
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<OrderManagementReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.OrderManagement, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Purchase order export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="keysearch"></param>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetPurchaseOrderTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, string keysearch = "", string groupid = "0")
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Purchase Order Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var records = new TraderPurchaseRules(dbContext).GetTraderPurchaseDataTable(requestModel, CurrentUser(), CurrentLocationManage(), keysearch, int.Parse(groupid), domain.Id);

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //Select the necessary records
                    var selectedRecords = records.data
                        .Cast<PurchaseCustom>()
                        .Select(x => new PurchaseOrderReport
                        {
                            Id = x.FullRef,
                            WorkGroup = x.WorkGroupName,
                            CreatedDate = x.CreatedDate,
                            Contact = x.Contact,
                            ReportingFilter = x.ReportingFilter,
                            Total = Convert.ToDecimal(x.Total),
                            Status = x.Status
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<PurchaseOrderReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = selectedRecords.Sum(x => x.Total);
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<PurchaseOrderReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.PurchaseOrder, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Trader list item export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetTraderListItemTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Trader Item Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                string keysearch = Request.Params["keysearch"];
                string groupIds = Request.Params["groups[]"];
                string types = Request.Params["types[]"];
                string brands = Request.Params["bands[]"];
                string needs = Request.Params["needs[]"];
                string rating = Request.Params["rating[]"];
                string tags = Request.Params["tags[]"];
                int currentLocation = !string.IsNullOrEmpty(Request.Params["clid"]) ? Convert.ToInt32(Request.Params["clid"]) : CurrentLocationManage();
                int activeInLocation = Convert.ToInt32(Request.Params["activeInLocation"]);
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var records = new TraderItemRules(dbContext).GetItemOverviewItemProduct(requestModel, domain.Id, currentLocation, keysearch, groupIds, types, brands, needs, rating, tags, activeInLocation);

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //Select the necessary records
                    var selectedRecords = records.data
                        .Cast<ItemOverview>()
                        .Select(x => new ItemOverviewReport
                        {
                            ItemType = x.IsBought ? "Buy" : x.IsSold ? "Sell" : (x.IsSold && x.IsBought) ? "Buy & Sell" : "",
                            ItemName = x.ItemName,
                            Description = x.Description,
                            ProductGroup = x.GroupName,
                            SKU = x.SKU,
                            Barcode = x.Barcode,
                            UnitofMeasurement = x.Unit,
                            Cost = x.Cost,
                            SellingPrice = x.SellingPrice,
                            Inventory = x.Id,
                            Active = x.IsActive ? "Yes" : "No"
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<ItemOverviewReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = selectedRecords.Sum(x => x.Cost);
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<ItemOverviewReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.TraderListItem, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Sales order export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="keyword"></param>
        /// <param name="workGroupId"></param>
        /// <param name="channel"></param>
        /// <param name="datetime"></param>
        /// <param name="isApproved"></param>
        /// <param name="export"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetSalesOrderTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int workGroupId, string channel, string datetime, bool isApproved, ExportType export)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Sales Order Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var records = new TraderSaleRules(dbContext).TraderSaleSearch(requestModel, CurrentUser().Id, CurrentLocationManage(), domain.Id, channel, keyword, workGroupId, datetime, CurrentUser().Timezone, isApproved, CurrentUser());

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = records.data
                        .Cast<TraderSaleCustom>()
                        .Select(x => new TraderSaleReport
                        {
                            Id = x.FullRef,
                            Workgroup = x.WorkgroupName,
                            Created = x.CreatedDate,
                            Channel = x.SalesChannel,
                            Contact = x.Contact,
                            ReportingFilters = x.Dimensions,
                            Cost = Convert.ToDecimal(x.SaleTotal),
                            Tax = Convert.ToDecimal(x.SaleTotal),
                            Total = Convert.ToDecimal(x.SaleTotal),
                            Status = x.Status
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<TraderSaleReport>(selectedRecords);

                    //Create variables
                    worksheet.Cells[0, 0].Value = "Domain:";
                    worksheet.Cells[0, 1].Value = CurrentDomain().Name;
                    worksheet.Cells[1, 0].Value = "Location:";
                    worksheet.Cells[1, 1].Value = ViewBag.DefaultLocation.Address?.City;
                    worksheet.Cells[2, 0].Value = "Date Filtered:";
                    worksheet.Cells[2, 1].Value = datetime;

                    //Properties
                    worksheet.Rows[5].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 5 for table
                    worksheet.Cells[0, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[1, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[2, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[2, 0].Style.Font.Weight = ExcelFont.BoldWeight;

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 5,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = selectedRecords.Sum(x => x.Total);
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<TraderSaleReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.SalesOrder, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        public ActionResult GetTraderMovementTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, string keysearch, string stringdate = "")
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Trader Movement Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var domain = CurrentDomain();
                var records = new TraderItemRules(dbContext).GetTraderItemsByDateRange(requestModel, domain.Id, CurrentLocationManage(), CurrentUser().Timezone, keysearch, CurrentUser().DateTimeFormat, stringdate);

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = records.data
                        .Cast<InventoryBatchCustom>()
                        .Select(x => new InventoryBatchReport
                        {
                            ItemType = x.ItemType,
                            ItemName = x.ItemName,
                            ProductGroup = x.ProductGroup,
                            SKU = x.SKU,
                            Unit = x.Unit,
                            Cost = Convert.ToDecimal(x.Cost),
                            PoolPrice = Convert.ToDecimal(x.PoolPrice),
                            QuantitySold = Convert.ToDecimal(x.QuantitySold),
                            PurchaseQuantity = x.PurchaseQuantity,
                            TransferInQuantity = x.TransferInQuantity,
                            TransferOutQuantity = x.TransferOutQuantity,
                            ManufacturedQuantity = x.ManufacturedQuantity,
                            GeneratedInventory = x.GeneratedInventory,
                            SpotCountQuantity = x.SpotCountQuantity,
                            WasteQuantity = x.WasteQuantity,
                            OnHandQuantity = x.OnHandQuantity
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<InventoryBatchReport>(selectedRecords);

                    //Create variables
                    worksheet.Cells[0, 0].Value = "Range Filtered:";
                    worksheet.Cells[0, 1].Value = stringdate;
                    worksheet.Cells[1, 0].Value = "Domain:";
                    worksheet.Cells[1, 1].Value = CurrentDomain().Name;
                    worksheet.Cells[2, 0].Value = "Location:";
                    worksheet.Cells[2, 1].Value = ViewBag.DefaultLocation.Address?.City;

                    //Properties
                    worksheet.Rows[5].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 5 for table
                    worksheet.Cells[0, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[1, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[2, 0].Style.Font.Weight = ExcelFont.BoldWeight;

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 5,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        public ActionResult GetMovementItemTrendTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int workGroupId, string channel, string datetime, bool isApproved, ExportType export)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Sales Order Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var records = new TraderSaleRules(dbContext).TraderSaleSearch(requestModel, CurrentUser().Id, CurrentLocationManage(), CurrentDomainId(), channel, keyword, workGroupId, datetime, CurrentUser().Timezone, isApproved, CurrentUser());

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = records.data
                        .Cast<TraderSaleCustom>()
                        .Select(x => new TraderSaleReport
                        {
                            Id = x.FullRef,
                            Workgroup = x.WorkgroupName,
                            Created = x.CreatedDate,
                            Channel = x.SalesChannel,
                            Contact = x.Contact,
                            ReportingFilters = x.Dimensions,
                            Cost = Convert.ToDecimal(x.SaleTotal),
                            Tax = Convert.ToDecimal(x.SaleTotal),
                            Total = Convert.ToDecimal(x.SaleTotal),
                            Status = x.Status
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<TraderSaleReport>(selectedRecords);

                    //Create variables
                    worksheet.Cells[0, 0].Value = "Domain:";
                    worksheet.Cells[0, 1].Value = CurrentDomain().Name;
                    worksheet.Cells[1, 0].Value = "Location:";
                    worksheet.Cells[1, 1].Value = ViewBag.DefaultLocation.Address?.City;
                    worksheet.Cells[2, 0].Value = "Date Filtered:";
                    worksheet.Cells[2, 1].Value = datetime;

                    //Properties
                    worksheet.Rows[5].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 5 for table
                    worksheet.Cells[0, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[1, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[2, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[2, 0].Style.Font.Weight = ExcelFont.BoldWeight;

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 5,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Virtual safe export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="keyword"></param>
        /// <param name="datetime"></param>
        /// <param name="safeId"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetVirtualSafeTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, string keyword, string datetime, int safeId, bool isApproved)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Virtual Safe Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var user = CurrentUser();
                var domain = CurrentDomain();
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var listSafeTransactions = new CMsRules(dbContext).GetListSafeTransaction(safeId, user, timezone, domain.Id);
                var records = new CMsRules(dbContext).SafeTransactionsSearch(requestModel, user.Id, CurrentLocationManage(), domain.Id, listSafeTransactions, keyword, datetime, CurrentUser().Timezone, isApproved, CurrentUser());

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = records.data
                        .Cast<SafeTransaction>()
                        .Select(x => new VirtualSafeTransactionReport
                        {
                            DateTime = x.TransactionDateString,
                            TillAccount = x.TillName,
                            SafeInOut = x.SafeName,
                            Amount = Convert.ToDecimal(x.AmountNumber),
                            Balance = Convert.ToDecimal(x.BalanceNumber),
                            Difference = Convert.ToDecimal(x.DifferenceNumber),
                            Status = x.Status
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<VirtualSafeTransactionReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = selectedRecords.Sum(x => x.Amount);
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<VirtualSafeTransactionReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.VirtualSafe, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Virtual till table export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="keyword"></param>
        /// <param name="tillId"></param>
        /// <param name="isApproved"></param>
        /// <param name="datetime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetVirtualTillTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, string keyword, int tillId, bool isApproved,
            string datetime, string[] status)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Virtual Till Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var user = CurrentUser();
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                var rules = new CMsRules(dbContext);

                var domain = CurrentDomain();
                //var domainId = CurrentDomainId();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);

                var listTillTransactions = rules.GetListTillTransaction(tillId, user, timezone, domain.Id, currencySettings);

                var records = rules.TillTransactionsSearch(requestModel, user.Id, CurrentLocationManage(), domain.Id, listTillTransactions, keyword,
                    datetime, CurrentUser().Timezone, isApproved, CurrentUser(), status, currencySettings);

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = records.data
                        .Cast<TillTransaction>()
                        .Select(x => new VirtualTillTransactionReport
                        {
                            DateTime = x.TransactionDateString,
                            Device = x.DeviceName,
                            Till = x.TillName,
                            Safe = x.SafeName,
                            TillInOut = x.DirectionName,
                            Amount = Convert.ToDecimal(x.AmountNumber),
                            Balance = Convert.ToDecimal(x.BalanceNumber),
                            Difference = Convert.ToDecimal(x.DifferenceNumber),
                            Status = x.Status
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<VirtualTillTransactionReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = selectedRecords.Sum(x => x.Amount);
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<VirtualTillTransactionReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.VirtualTill, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }

                    //Update response
                    model.result = true;
                    model.Object = path.Replace("~", "..");
                }
                else
                {
                    //Update response
                    model.result = false;
                    //model.Object = path.Replace("~", "..");
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        public ActionResult GetCashBankTableContentExport([Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderby, int id, string fromDate, string toDate, string search, int draw, int start, int length, ExportType export)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Cash Bank Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                length = 1000;
                var totalRecord = 0;
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());

                var traderCashAccount = new TraderCashAccount();
                var destinationAccounts = new List<CashAccountTransaction>();
                var originationAccounts = new List<CashAccountTransaction>();
                if (id > 0)
                {
                    traderCashAccount = new TraderCashBankRules(dbContext).GeTraderCashAccountById(id);
                    destinationAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashAccountId(new List<int>() { id }, "Destination");
                    originationAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashAccountId(new List<int>() { id }, "Originating");
                }

                var cashBankRules = new TraderCashBankRules(dbContext);
                var cashAccountTransactions = new List<CashAccountTransaction>();
                cashAccountTransactions.AddRange(destinationAccounts);
                cashAccountTransactions.AddRange(originationAccounts);

                var inTransactionsSum = cashAccountTransactions
                    .Where(i => i.Status == TraderPaymentStatusEnum.PaymentApproved
                        && cashBankRules.GetTransactionDirection(i, id).ToLower() == "in")
                    .Sum(a => a.Amount);

                var outTransactionsSum = cashAccountTransactions
                    .Where(i => i.Status == TraderPaymentStatusEnum.PaymentApproved
                        && cashBankRules.GetTransactionDirection(i, id).ToLower() == "out")
                    .Sum(a => a.Amount);

                List<CashAccountTransactionModel> records = new TraderCashBankRules(dbContext).GetListPaymentTransaction(column, orderby, CurrentLocationManage(), id, fromDate, toDate, search, CurrentDomain(), CurrentUser().Id, CurrentUser().Timezone, start, length, ref totalRecord, CurrentUser().DateFormat);

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = records
                        .Select(x => new CashAccountTransactionReport()
                        {
                            Id = x.Id,
                            Date = x.Date,
                            Reference = x.Reference,
                            Source = x.Source,
                            PaymentMethod = x.PaymentMethod,
                            Destination = x.Destination,
                            Type = x.Type,
                            InOut = x.InOut,
                            Amount = x.Amount,
                            Status = x.Status
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<CashAccountTransactionReport>(selectedRecords);

                    //Create variables
                    worksheet.Cells[0, 0].Value = "C&B Account Name:";
                    worksheet.Cells[0, 1].Value = traderCashAccount.Name;
                    worksheet.Cells[1, 0].Value = "Bookkeeping Account:";
                    worksheet.Cells[1, 1].Value = "";

                    worksheet.Cells[3, 0].Value = "In";
                    worksheet.Cells[4, 0].Value = inTransactionsSum.ToCurrencySymbol(currencySettings);
                    worksheet.Cells[3, 1].Value = "Out";
                    worksheet.Cells[4, 1].Value = outTransactionsSum.ToCurrencySymbol(currencySettings);
                    worksheet.Cells[3, 2].Value = "Charges";
                    worksheet.Cells[4, 2].Value = ((destinationAccounts.Where(e => e.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(a => a.Charges) + originationAccounts.Where(e => e.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(a => a.Charges)).ToCurrencySymbol(currencySettings));
                    worksheet.Cells[3, 3].Value = "Transactions";
                    worksheet.Cells[4, 3].Value = (originationAccounts.Count() + destinationAccounts.Count());

                    //Properties
                    worksheet.Rows[6].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 6 for table
                    worksheet.Cells[0, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[1, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[3, 0].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[3, 1].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[3, 2].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Cells[3, 3].Style.Font.Weight = ExcelFont.BoldWeight;

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 6,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Shift audit export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetShiftAuditTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, int id)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Shift Audit Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var user = CurrentUser();
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var stockAuditModel = new TraderStockAuditRules(dbContext).GetStockAuditModel(id, CurrentUser(), domain.Id);

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (stockAuditModel != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = stockAuditModel.ProductList
                        .Select(x => new StockAuditItemReport
                        {
                            ItemName = x.Name,
                            SKU = Convert.ToInt32(x.SKU),
                            Unit = x.Unit.Name,
                            ObservedInventory = Convert.ToInt32(x.OpeningCount),
                            In = Convert.ToInt32(x.PeriodIn),
                            Out = Convert.ToInt32(x.PeriodOut),
                            ExpectedClosing = Convert.ToInt32(x.ExpectedClosing),
                            ObservedClosing = Convert.ToInt32(x.ClosingCount),
                            Variance = Convert.ToInt32(x.Variance)
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<StockAuditItemReport>(selectedRecords);

                    worksheet.Cells[0, 0].Value = "Name";
                    worksheet.Cells[1, 0].Value = stockAuditModel.Name;
                    worksheet.Cells[0, 1].Value = "Start";
                    worksheet.Cells[1, 1].Value = stockAuditModel.StartedDate;
                    worksheet.Cells[0, 2].Value = "End";
                    worksheet.Cells[1, 2].Value = stockAuditModel.FinishedDate;
                    worksheet.Cells[0, 3].Value = "Status";
                    worksheet.Cells[1, 3].Value = stockAuditModel.Status.GetDescription();

                    // Get the cell range.
                    var range = worksheet.Cells.GetSubrange("F3:G3");

                    // Merge cells in the current range.
                    range.Merged = true;

                    // Set the value of the merged range.
                    range.Value = "Movement in period";

                    // Set the style of the merged range.
                    range.Style.HorizontalAlignment = HorizontalAlignmentStyle.CenterAcross;
                    range.Style.Font.Weight = ExcelFont.BoldWeight;

                    //Properties
                    worksheet.Rows[4].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 4 for table
                    worksheet.Rows[3].Style.Font.Weight = ExcelFont.BoldWeight;
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight;

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 4,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = 0;
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<StockAuditItemReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.ShiftAudit, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Spot count export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetSpotCountTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, int id)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Spot Count Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var domain = CurrentDomain();
                var currentUser = CurrentUser();
                var rule = new TraderSpotCountRules(dbContext);
                var spotCountModel = rule.GetById(id) ?? new SpotCount();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var timeline = rule.SpotCountApprovalStatusTimeline(spotCountModel.Id, currentUser.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (spotCountModel != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = spotCountModel.ProductList
                        .Select(x => new SpotCountItemReport
                        {
                            ItemName = x.Product.Name,
                            SKU = x.Product.SKU,
                            Unit = x.CountUnit.Name,
                            SystemInventory = x.SavedInventoryCount,
                            ObservedInventory = x.SpotCountValue,
                            Adjustment = x.Adjustment,
                            Notes = x.Notes
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<SpotCountItemReport>(selectedRecords);

                    worksheet.Cells[0, 0].Value = "Name";
                    worksheet.Cells[1, 0].Value = spotCountModel.Name;
                    worksheet.Cells[0, 1].Value = "Date";
                    worksheet.Cells[1, 1].Value = spotCountModel.CreatedDate;
                    worksheet.Cells[0, 2].Value = "Created";
                    worksheet.Cells[1, 2].Value = spotCountModel.CreatedBy.DisplayUserName;
                    worksheet.Cells[0, 3].Value = "Status";
                    worksheet.Cells[1, 3].Value = spotCountModel.Status.GetDescription();

                    //Properties
                    worksheet.Rows[3].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 3 for table
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight;

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 3,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = 0;
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<SpotCountItemReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.SpotCount, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Waste count export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetWasteReportTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, int id)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "Waste Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                var currentUser = CurrentUser();
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var rule = new TraderWasteReportRules(dbContext);
                var wasteReportModel = rule.GetById(id) ?? new WasteReport();

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (wasteReportModel != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //
                    var selectedRecords = wasteReportModel.ProductList
                        .Select(x => new WasteItemReport
                        {
                            ItemName = x.Product.Name,
                            SKU = x.Product.SKU,
                            Unit = x.CountUnit.Name,
                            ObservedInventory = x.Product.InventoryDetails.FirstOrDefault(q => q.Location.Id == wasteReportModel.Location.Id)?.CurrentInventoryLevel ?? 0,
                            Wasted = x.WasteCountValue,
                            Notes = x.Notes
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<WasteItemReport>(selectedRecords);

                    worksheet.Cells[0, 0].Value = "Name";
                    worksheet.Cells[1, 0].Value = wasteReportModel.Name;
                    worksheet.Cells[0, 1].Value = "Date";
                    worksheet.Cells[1, 1].Value = wasteReportModel.CreatedDate;
                    worksheet.Cells[0, 2].Value = "Created";
                    worksheet.Cells[1, 2].Value = wasteReportModel.CreatedBy.DisplayUserName;
                    worksheet.Cells[0, 3].Value = "Status";
                    worksheet.Cells[1, 3].Value = wasteReportModel.Status.GetDescription();

                    //Properties
                    worksheet.Rows[3].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 4 for table
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight;

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 3,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = 0;
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<WasteItemReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.WasteReport, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Pos print check export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="keyword"></param>
        /// <param name="cashiers"></param>
        /// <param name="managers"></param>
        /// <param name="devices"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetPosPrintCheckTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, string keyword, string[] cashiers,
            string[] managers, string[] devices, string datetime)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "POS Print Check Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 10000
                requestModel.Length = 10000;

                keyword = keyword.Trim().ToLower();
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var records = new OrderPrintCheckRules(dbContext).GetTraderPosOrderPrintCheck(requestModel, keyword.ToLower(), cashiers, managers, devices, datetime, CurrentLocationManage(), CurrentUser());

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //Select the necessary records
                    var selectedRecords = records.data
                        .Cast<PosOrderCancellationPrintCheckModel>()
                        .Select(x => new PosPrintCheckReport
                        {
                            OrderRef = x.Ref,
                            Date = x.Date,
                            SalesChannel = x.SalesChannel,
                            POSDevice = x.Reason,
                            TillManager = x.CancelledBy,
                            Cashier = x.Cashier,
                            Customer = x.Customer,
                            Items = x.TotalItems,
                            PDSOrders = string.Join(", ", x.PDSOrders.Select(p => p.Name)) // Converts List<string> to a single string
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<PosPrintCheckReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = 0;
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<PosPrintCheckReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.PosPrintCheck, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Pos cancellation export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="keyword"></param>
        /// <param name="cashiers"></param>
        /// <param name="managers"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetPosCancellationTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, string keyword, string[] cashiers, string[] managers, string datetime)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "POS Cancellation Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                keyword = keyword?.Trim().ToLower();
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var records = new OrderCancellationRules(dbContext).GetTraderPosOrderCancellations(requestModel, keyword.ToLower(), cashiers, managers, datetime, CurrentLocationManage(), CurrentUser());

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //Select the necessary records
                    var selectedRecords = records.data
                        .Cast<PosOrderCancellationPrintCheckModel>()
                        .Select(x => new PosCancellationReport
                        {
                            OrderRef = x.Ref,
                            Date = x.Date,
                            SalesChannel = x.SalesChannel,
                            Reason = x.Reason,
                            CancelledBy = x.CancelledBy,
                            Cashier = x.Cashier,
                            Customer = x.Customer,
                            Items = Convert.ToInt32(x.TotalItems),
                            PDSOrders = string.Join(", ", x.PDSOrders.Select(p => p.Name)) // Converts List<string> to a single string
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<PosCancellationReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = 0;
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<PosCancellationReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.PosCancellation, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        /// Pos payment export functionality i.e. excel, csv, pdf
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="export"></param>
        /// <param name="keyword"></param>
        /// <param name="datelimit"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetPosPaymentTableContentExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, ExportType export, string keyword, string datelimit)
        {
            try
            {
                //Create file path
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                //Generate file name and path
                var name = "POS Payment Report";
                var nameWithOutSpace = name.Replace(" ", "");
                var fileName = $"{DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__")}{nameWithOutSpace}";
                path += "/" + fileName;

                //update limit to 1000
                requestModel.Length = 1000;

                int[] locations = Request.Params["locations[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
                int[] methods = Request.Params["methods[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
                int[] accounts = Request.Params["accounts[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
                string[] cashiers = Request.Params["cashiers[]"]?.Split(',');
                int[] devices = Request.Params["devices[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
                //string dateFormat = CurrentUser().DateFormat;
                //string timeFormat = CurrentUser().TimeFormat;
                keyword = keyword.Trim().ToLower();
                var domain = CurrentDomain();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);
                var records = new PosSaleOrderRules(dbContext).GetPosPaymentDataTable(requestModel, domain.Id, currencySettings, keyword, datelimit, locations, methods, accounts, cashiers, devices, CurrentUser());

                MsgModel model = new MsgModel();
                SpreadsheetInfo.SetLicense(GemboxLicense);

                if (records != null)
                {
                    //create the excel worksheet
                    var workbook = new ExcelFile();
                    var worksheet = workbook.Worksheets.Add(name);

                    //Select the necessary records
                    var selectedRecords = records.data
                        .Cast<PosPaymentCustom>()
                        .Select(x => new PosPaymentReport
                        {
                            DateTime = x.CreatedDate,
                            Location = x.LocationName,
                            Reference = x.RefFull,
                            PaymentMethod = x.Method,
                            Account = x.AccountName,
                            Cashier = x.Cashier,
                            PosDevice = x.PosDevice,
                            Amount = Convert.ToDecimal(x.Amount)
                        });

                    //Init dataTable
                    var dt = LinqToDataTable<PosPaymentReport>(selectedRecords);

                    //Properties
                    worksheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight; //start row is 0 for table

                    //Set dataTable options
                    var dtOptions = new InsertDataTableOptions()
                    {
                        StartRow = 0,
                        StartColumn = 0,
                        ColumnHeaders = true,
                    };

                    // Insert DataTable to an Excel worksheet.
                    worksheet.InsertDataTable(dt, dtOptions);

                    //Process excel
                    if (export == ExportType.Excel)
                    {
                        //update path
                        path = $"{path}.xlsx";

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write Excel file using specified dtOptions.
                        workbook.Save(Server.MapPath(path));
                    }
                    else if (export == ExportType.Csv)
                    {
                        //update path
                        path = $"{path}.csv";

                        //Set csv options
                        var csvOptions = new CsvSaveOptions(CsvType.CommaDelimited);

                        if (System.IO.File.Exists(Server.MapPath(path)))
                            System.IO.File.Delete(Server.MapPath(path));

                        // Write CSV file using specified CsvSaveOptions.
                        workbook.Save(Server.MapPath(path), csvOptions);
                    }
                    else if (export == ExportType.Pdf)
                    {
                        var filePath = Server.MapPath($"{path}.pdf");
                        string imageTop = await GetMediaFileBase64Async(domain.LogoUri);
                        string imageBottom = await GetMediaFileBase64Async(domain.LogoUri);
                        decimal totalValue = selectedRecords.Sum(x => x.Amount);
                        decimal totalTax = 0;
                        var fileStreams = GeneratePDFTableContentExport<PosPaymentReport>(selectedRecords.ToList(),
                            domain, imageTop, imageBottom, CurrentUser().Timezone, currencySettings, ReportFileName.PosPayment, totalValue, totalTax);

                        //Create a filestream for export pdf
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(fileStreams, 0, fileStreams.Length);
                        }

                        //Upload to azure storage and save to database
                        var uri = await UploadMediaFromPath($"{fileName}.pdf", filePath);

                        //Delete file from temp storage
                        System.IO.File.Delete(filePath);

                        //Retrieve file from database
                        path = GetDocumentRetrievalUrl(uri);
                    }
                }

                //Update response
                model.result = true;
                model.Object = path.Replace("~", "..");

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }

        #endregion Table Content Export Functionality

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

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="domain"></param>
        /// <param name="imageTop"></param>
        /// <param name="imageBottom"></param>
        /// <param name="timezone"></param>
        /// <param name="setting"></param>
        /// <param name="reportFileName"></param>
        /// <param name="totalValue"></param>
        /// <param name="totalTax"></param>
        /// <returns></returns>
        private byte[] GeneratePDFTableContentExport<T>(IEnumerable<T> items, QbicleDomain domain, string imageTop, string imageBottom, string timezone, CurrencySetting setting, string reportFileName, decimal totalValue = 0, decimal totalTax = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, items, imageTop, imageBottom, timezone, setting);

                #region Bind data Report

                //Order info
                var lsOrderInfo = new List<ReportOrderInfo>();
                var orderInfo = new ReportOrderInfo
                {
                    FullRef = new RandomGenerator().RandomString(10),
                    AdditionalInformation = "",
                    OrderDate = DateTime.Now.ConvertTimeFromUtc(timezone).ToString("dd MMM, yyyy")
                };

                //Get Location details
                var location = domain.TraderLocations.FirstOrDefault(x => x.IsDefaultAddress);
                var address = location.Address;
                var addressBilling = location.Address;

                //Format location details
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
                    new ReportDataSource {Name = "Items", Value = items},
                    new ReportDataSource {Name = "OrderInfo", Value = lsOrderInfo}
                };

                return ReportRules.RenderReport(dataSource, reportFileName);

                #endregion Bind data Report
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, items, imageTop, imageBottom, timezone, setting);
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

        public enum ExportType
        {
            Excel = 1,
            Csv,
            Pdf
        }

        #endregion Private Methods
    }
}