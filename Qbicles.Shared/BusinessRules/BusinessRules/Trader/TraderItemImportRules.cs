using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using GemBox.Spreadsheet;
using Google.Protobuf.WellKnownTypes;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.FilesUploadUtility;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderItemImportRules
    {
        private ApplicationDbContext dbContext;

        public TraderItemImportRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<TraderItemImport> GetItemImports(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return dbContext.TraderItemImports.Where(e => e.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<TraderItemImport>();
            }
        }

        public ReturnJsonModel FileNameAndExtensionVerify(string fileName, int domainId, int locationId)
        {
            var excelExtensions = new FileTypeRules(dbContext).GetExtension("Excel File");
            var extensionValid = excelExtensions.Contains(Path.GetExtension(fileName).Replace(".", ""));
            if (!extensionValid)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_SPREADSHEET_EXTENSION"), actionVal = 1 };

            var nameValid = dbContext.TraderItemImports.Any(e => e.Spreadsheet.ToLower() == fileName.ToLower() && e.Domain.Id == domainId && e.Location.Id == locationId);
            if (nameValid)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_SPREADSHEET_NAMEEXISTED"), actionVal = 2 };
            return new ReturnJsonModel { result = true };
        }

        public object GetItemImports(IDataTablesRequest requestModel, string keyword, string datetime, int domainId, int locationId, UserSetting dateTimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, keyword, datetime, domainId);
                //Get the filtered sales, get all sales not just the approved sales
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var itemImports = dbContext.TraderItemImports.Where(e => e.Domain.Id == domainId);

                if (!string.IsNullOrEmpty(keyword))
                {
                    itemImports = itemImports.Where(e => e.Spreadsheet.Contains(keyword));
                }

                if (!string.IsNullOrEmpty(datetime))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(dateTimeFormat.Timezone);

                    if (!string.IsNullOrEmpty(datetime.Trim()))
                    {
                        datetime.ConvertDaterangeFormat(dateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                        startDate = startDate.ConvertTimeToUtc(tz);
                        endDate = endDate.ConvertTimeToUtc(tz);

                        itemImports = itemImports.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);
                    }
                }

                if (locationId > 0)
                    itemImports = itemImports.Where(e => e.Location.Id == locationId);

                var totalItemImports = itemImports.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Spreadsheet":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Spreadsheet" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Uploader":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.Forename" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Location":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Location.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                itemImports = itemImports.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = itemImports.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new TraderItemImportModel
                {
                    Id = q.Id,
                    Key = q.Key,
                    Spreadsheet = q.Spreadsheet,
                    SpreadsheetKey = q.SpreadsheetKey,
                    SpreadsheetErrorsKey = q.SpreadsheetErrorsKey,
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat + " " + dateTimeFormat.TimeFormat),
                    Uploader = q.CreatedBy.GetFullName(),
                    Status = q.Status,
                    StatusName = q.Status.GetDescription(),
                    Location = q.Location.Name,
                    ItemsError = q.ItemsError,
                    ItemsImported = q.ItemsImported,
                    ItemsUpdated = q.ItemsUpdated
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalItemImports, totalItemImports);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword, datetime, domainId);
                return null;
            }
        }

        public TraderItemImport GetItemImport(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderItemImports.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new TraderItemImport();
            }
        }

        public ReturnJsonModel SaveItemImport(TraderItemImport itemImport)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemImport);

                itemImport.Domain = dbContext.Domains.FirstOrDefault(e => e.Id == itemImport.Domain.Id);
                itemImport.Location = dbContext.TraderLocations.FirstOrDefault(e => e.Id == itemImport.Location.Id);
                itemImport.CreatedBy = dbContext.QbicleUser.Find(itemImport.CreatedBy.Id);
                itemImport.CreatedDate = DateTime.UtcNow;
                itemImport.Status = ImportStatus.Verifying;

                dbContext.TraderItemImports.Add(itemImport);
                dbContext.Entry(itemImport).State = EntityState.Added;
                dbContext.SaveChanges();
                refModel.actionVal = 1;
                refModel.result = true;

                var job = new QbicleJobParameter
                {
                    JobId = itemImport.Key,
                    EndPointName = "itemsimportprocess",
                };

                var tskHangfire = new Task(async () =>
                {
                    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, itemImport);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public ReturnJsonModel DeleteItemImport(string key)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, key);

                var id = HelperClass.Converter.Obj2Int(key.Decrypt());
                var iImport = dbContext.TraderItemImports.FirstOrDefault(e => e.Id == id);

                dbContext.Entry(iImport).State = EntityState.Deleted;
                dbContext.TraderItemImports.Remove(iImport);
                dbContext.SaveChanges();

                //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
                AzureStorageHelper.DeleteObject(iImport.SpreadsheetKey);

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, key);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        //Process Spreadsheet
        private readonly object lockProcessItemsImport = new object();

        private string _pathRepository = ConfigManager.TempPathRepository;
        private string _fileName = "";
        private string _extension = "";
        private int _colNumber = 11;
        private string[] headerColumns;
        private int _imtemsImported = 0, _itemsUpdated = 0, _itemsError = 0;

        internal async Task ProcessItemsImportAsync(string key)
        {
            try
            {
                //lock (lockProcessItemsImport)
                //{
                var id = int.Parse(key.Decrypt());
                var itemImport = dbContext.TraderItemImports.FirstOrDefault(e => e.Id == id);
                itemImport.Status = ImportStatus.Uploading;
                dbContext.SaveChanges();
                //download file from S3

                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(itemImport.SpreadsheetKey);

                _fileName = Path.GetFileNameWithoutExtension(s3Object.ObjectName);
                _extension = s3Object.Extension;

                var fileGuid = $"{Guid.NewGuid()}";
                var filePath = $"{_pathRepository}/{fileGuid}.{_extension}";

                using (FileStream outputFileStream = new FileStream($"{filePath}", FileMode.Create))
                {
                    s3Object.ObjectStream.CopyTo(outputFileStream);
                }

                var sheets = (new ExcelReader()).ReadSheets(null, $"{filePath}");

                var sheet = sheets.Sheets[0];
                var errorObjectKey = "";

                var processed = await ProcessSpreadsheetAsync(itemImport, filePath, sheet.sheetId, sheet.sheetName, (e) =>
                {
                    errorObjectKey = e;
                });

                DirectoryHelper.DeleteFile(filePath);

                itemImport.ItemsImported = _imtemsImported;
                itemImport.ItemsUpdated = _itemsUpdated;
                itemImport.ItemsError = _itemsError;
                switch (processed)
                {
                    case ImportStatus.Uploaded:
                        itemImport.Status = ImportStatus.Uploaded;
                        dbContext.SaveChanges();
                        break;

                    case ImportStatus.UploadedWithErrors:
                        itemImport.SpreadsheetErrorsKey = errorObjectKey;
                        itemImport.Status = ImportStatus.UploadedWithErrors;
                        dbContext.SaveChanges();
                        break;

                    case ImportStatus.FileError:
                        itemImport.SpreadsheetErrorsKey = itemImport.SpreadsheetKey;
                        itemImport.Status = ImportStatus.FileError;
                        dbContext.SaveChanges();
                        break;
                }
                //}
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, key);
            }
        }

        private string[] columnsRequired = new string[] {
                "Item Type",
                "Item Name",
                "Item Descrption",
                "Product Group",
                "SKU",
                "Barcode",
                "Unit Of Measure",
                "Cost",
                "Selling Price",
                "Inventory",
                "Active",
        };

        private async Task<ImportStatus> ProcessSpreadsheetAsync(TraderItemImport itemImport, string filePath, string sheetId, string sheetName, Action<string> saveErrorObject)
        {
            var itemsCorrect = new List<ItemImportData>();
            var itemsWrong = new List<ItemImportData>();
            // ***read date 1904 status
            Package spreadsheetPackage = Package.Open(filePath, FileMode.Open, FileAccess.ReadWrite);
            // Open a SpreadsheetDocument based on a package. Save status Date1904 to prive static
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(spreadsheetPackage);
            bool dtDate1904 = false; // Status excel using Date1904
            if (spreadsheetDocument.WorkbookPart.Workbook.WorkbookProperties != null && spreadsheetDocument.WorkbookPart.Workbook.WorkbookProperties.Date1904 != null)
                dtDate1904 = spreadsheetDocument.WorkbookPart.Workbook.WorkbookProperties.Date1904;
            //Find row, column hiden
            WorkbookPart wbPart = spreadsheetDocument.WorkbookPart;
            Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where((s) => s.Id == sheetId).FirstOrDefault();
            WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
            Worksheet ws = wsPart.Worksheet;
            List<uint> _ListHidenRow = ws.Descendants<Row>().Where((r) => r.Hidden != null && r.Hidden.Value).Select(r => r.RowIndex.Value).ToList<uint>();
            var _ListHidenCol = ws.Descendants<Column>().Where((c) => c.Hidden != null && c.Hidden.Value);
            spreadsheetDocument.Close();
            // ***end read date 1904
            //Read the file and return the table
            string GemboxLicense = ConfigurationManager.AppSettings["GemboxLicense"];
            SpreadsheetInfo.SetLicense(GemboxLicense);
            ExcelFile excelFile = ExcelFile.Load(filePath);
            //excelFile.LoadXlsx(FileName, XlsxOptions.None);

            DataTable dataTable = new DataTable();

            int curRow = 0;
            int curCol = 0;
            ExcelWorksheet worksheet = excelFile.Worksheets[sheetName];

            int total_column = worksheet.Rows[curRow].AllocatedCells.Count;
            headerColumns = new string[total_column];

            for (; curRow < worksheet.Rows.Count; curRow++)
            {
                if (total_column > dataTable.Columns.Count)
                {
                    for (int x = dataTable.Columns.Count; x < total_column; x++)
                    {
                        dataTable.Columns.Add("Column " + x);
                    }
                }

                var tableColumns = new string[dataTable.Columns.Count];

                var iImport = new ItemImportData();

                for (curCol = 0; curCol < total_column; curCol++)
                {
                    var cellValue = worksheet.Rows[curRow].Cells[curCol].Value?.ToString() ?? "";

                    tableColumns[curCol] = cellValue;

                    if (curRow == 0)//header columns
                        headerColumns[curCol] = cellValue;
                    else
                        CollectData(curCol, cellValue, iImport);
                }

                dataTable.Rows.Add(tableColumns);

                if (total_column == 11 && curRow > 0)
                {
                    //Ignore row empty
                    //if (iImport.StringPropertiesEmpty())
                    //     continue;

                    if (Verifydata(iImport))
                        itemsCorrect.Add(iImport);
                    else
                        itemsWrong.Add(iImport);
                }
            }
            //if total_colum< 11 get row 0 column header and compare missing columns
            if (total_column < 11 || curRow == 0)
                return ImportStatus.FileError;

            await ImportTraderItems(itemsCorrect, itemImport, itemsWrong);
            if (itemsWrong.Count > 0)
            {
                var errorFileName = $"{_fileName}-error-items-{itemImport.CreatedDate.ToString("dd-MM-yyyy-hh-MM-ss")}.{_extension}";
                saveErrorObject(await WriteWrongItemsAsync(itemsWrong, errorFileName));
            }
            _itemsError = itemsWrong.Count;
            if (itemsWrong.Count > 0)
                return ImportStatus.UploadedWithErrors;
            return ImportStatus.Uploaded;
        }

        /// <summary>
        /// write items error to a file and upload to S3
        /// </summary>
        /// <param name="itemsWrong"></param>
        /// <returns>s3 object key uploaded</returns>
        private async Task<string> WriteWrongItemsAsync(List<ItemImportData> itemsWrong, string errorFileName)
        {
            var pathFileError = _pathRepository + errorFileName;
            //Create new file error
            string gemboxLicense = ConfigurationManager.AppSettings["GemboxLicense"];
            SpreadsheetInfo.SetLicense(gemboxLicense);
            ExcelFile ef = new ExcelFile();
            ExcelWorksheet ws = ef.Worksheets.Add("Items error");

            for (int i = 0; i < columnsRequired.Length; i++)
            {
                ws.Cells[0, i].Value = columnsRequired[i];
                ws.Cells[0, i].Style.Font.Weight = ExcelFont.BoldWeight;
            }

            ef.Save(pathFileError);

            ExcelFile excelFile = ExcelFile.Load(pathFileError);
            ExcelWorksheet workSheet = excelFile.Worksheets.FirstOrDefault();

            int rowIndex = 2;
            foreach (var item in itemsWrong)
            {
                workSheet.Cells[rowIndex - 1, 0].Value = item.Type;
                ValidStyle(workSheet, rowIndex, 0, item.Type);

                workSheet.Cells[rowIndex - 1, 1].Value = item.Name;
                ValidStyle(workSheet, rowIndex, 1, item.Name);

                workSheet.Cells[rowIndex - 1, 2].Value = item.Description;
                ValidStyle(workSheet, rowIndex, 2, item.Description);

                workSheet.Cells[rowIndex - 1, 3].Value = item.Group;
                ValidStyle(workSheet, rowIndex, 3, item.Group);

                workSheet.Cells[rowIndex - 1, 4].Value = item.Sku;
                ValidStyle(workSheet, rowIndex, 4, item.Sku);

                workSheet.Cells[rowIndex - 1, 5].Value = item.Barcode;
                ValidStyle(workSheet, rowIndex, 5, item.Barcode);

                workSheet.Cells[rowIndex - 1, 6].Value = item.Unit;
                ValidStyle(workSheet, rowIndex, 6, item.Unit);

                workSheet.Cells[rowIndex - 1, 7].Value = item.Cost;
                ValidStyle(workSheet, rowIndex, 7, item.Cost);

                workSheet.Cells[rowIndex - 1, 8].Value = item.SellingPrice;
                ValidStyle(workSheet, rowIndex, 8, item.SellingPrice);

                workSheet.Cells[rowIndex - 1, 9].Value = item.Inventory;
                ValidStyle(workSheet, rowIndex, 9, item.Inventory);

                workSheet.Cells[rowIndex - 1, 10].Value = item.Active;
                ValidStyle(workSheet, rowIndex, 10, item.Active);

                rowIndex++;
            }
            excelFile.Save(pathFileError);

            //Upload to S3, update table ItemImport
            var objectKey = await AzureStorageHelper.UploadMediaFromPath(errorFileName, pathFileError, true);

            DirectoryHelper.DeleteFile($"{pathFileError}");
            return objectKey;
        }

        private void ValidStyle(ExcelWorksheet worksheet, int rowIndex, int colIndex, string value)
        {
            if (!VerifyData(colIndex, value))
            {
                if (string.IsNullOrEmpty(value))
                    worksheet.Cells[rowIndex - 1, colIndex].Style.FillPattern.SetSolid(SpreadsheetColor.FromName(ColorName.Red));
                else
                    worksheet.Cells[rowIndex - 1, colIndex].Style.Font.Color = SpreadsheetColor.FromName(ColorName.Red);
            }
        }

        private async Task ImportTraderItems(List<ItemImportData> items, TraderItemImport import, List<ItemImportData> itemsWrong)
        {
            LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, $"total item: {items.Count}");

            var skus = items.Select(e => e.Sku.ToLower()).ToList();

            var itemsUpdate = dbContext.TraderItems.Where(e => e.Domain.Id == import.Domain.Id && skus.Contains(e.SKU.ToLower())).ToList();
            var itemsSkuUpdate = itemsUpdate.Select(e => e.SKU.ToLower());
            var itemsNew = items.Where(e => !itemsSkuUpdate.Contains(e.Sku.ToLower())).ToList();
            //var tradItems = new List<TraderItem>();

            itemsNew.ForEach(item =>
            {
                var iNew = CreateItem(item, import, itemsWrong);
                if (iNew != null)
                {
                    _imtemsImported++;
                }
            });

            foreach (var itemUp in itemsUpdate)
            {
                var itemImportData = items.FirstOrDefault(e => e.Sku.ToLower() == itemUp.SKU.ToLower());
                await UpdateItem(itemUp, itemImportData, import, itemsWrong);
            }
        }

        private TraderItem CreateItem(ItemImportData item, TraderItemImport import, List<ItemImportData> itemsWrong)
        {
            try
            {
                /// quantity
                var iInventoryLevel = HelperClass.Converter.Obj2Decimal(item.Inventory);
                var iCost = HelperClass.Converter.Obj2Decimal(item.Cost);
                var iSellingPrice = HelperClass.Converter.Obj2Decimal(item.SellingPrice);
                var tradItem = new TraderItem
                {
                    Domain = import.Domain,
                    ImageUri = ConfigManager.DefaultProductPlaceholderImageUrl,
                    Name = item.Name,
                    Description = item.Description,
                    DescriptionText = item.Description,
                    Barcode = item.Barcode,
                    SKU = item.Sku,
                    Group = GetOrCreateGroupByName(import.Domain, import.CreatedBy, item.Group),
                    CreatedBy = import.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    Locations = new List<TraderLocation>(),
                    Units = new List<ProductUnit>(),
                    InventoryDetails = new List<InventoryDetail>(),
                    IsBought = item.Type.ToLower() == "buy",
                    IsSold = item.Type.ToLower() == "sell",
                    IsCommunityProduct = false,
                    IsCompoundProduct = false,
                    IsActiveInAllLocations = true
                };
                if (item.Type.ToLower() == "buy & sell")
                {
                    tradItem.IsBought = true;
                    tradItem.IsSold = true;
                }

                /*
                 Get or create the ProductUnit based on the Unit of Measure column
                It will only be possible to get a ProductUnit if this is a TraderItem update.
                For a TraderItem that is being created the ProductUnit will have to be created.
                 */
                tradItem.Units.Add(new ProductUnit
                {
                    CreatedBy = import.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = item.Active == "Yes",
                    IsPrimary = true,
                    IsBase = true,
                    MeasurementType = MeasurementTypeEnum.Each,
                    Name = item.Unit,
                    Quantity = 1,
                    QuantityOfBaseunit = 1
                });

                //If TraderItem.IsBought = False AND TraderItem.IsSold = true then NO InventoryDetail or InventoryBatch is created
                if (!tradItem.IsBought && tradItem.IsSold)
                { }
                else
                {
                    //Create the InventoryDetails
                    foreach (var dLocation in tradItem.Domain.TraderLocations)
                    {
                        var inventoryDetail = new InventoryDetail
                        {
                            MinInventorylLevel = 0,
                            MaxInventoryLevel = 0,
                            CurrentInventoryLevel = dLocation.Id == import.Location.Id ? iInventoryLevel : 0,
                            Location = dLocation,
                            CreatedBy = import.CreatedBy,
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedBy = import.CreatedBy,
                            LastUpdatedDate = DateTime.UtcNow,
                            AverageCost = iCost,
                            LatestCost = iCost,
                        };

                        if (dLocation.Id == import.Location.Id)
                        {
                            //Create the InventoryBatch
                            var inventoryBatch = new Models.Trader.Inventory.Batch
                            {
                                CreatedDate = DateTime.UtcNow,
                                CostPerUnit = iCost,
                                OriginalQuantity = iInventoryLevel,
                                UnusedQuantity = iInventoryLevel,
                                CurrentBatchValue = iInventoryLevel * iCost,
                                Direction = Models.Trader.Inventory.BatchDirection.In,
                                LastUpdatedDate = DateTime.UtcNow,
                                CreatedBy = import.CreatedBy,
                                InventoryDetail = inventoryDetail,
                                LastUpdatedBy = import.CreatedBy,
                                IsInvented = false,
                            };

                            dbContext.InventoryBatches.Add(inventoryBatch);
                        }

                        tradItem.InventoryDetails.Add(inventoryDetail);
                    }
                }

                //Create the Location Xrefs
                //Add each TraderLocation to the TraderItem.Locations collection
                tradItem.Locations.AddRange(tradItem.Domain.TraderLocations);
                //Create the Price, For Each Sales Channel (B2B, B2C, POS)
                tradItem.Locations.ForEach(l =>
                {
                    dbContext.TraderPrices.AddRange(CreatePrice(tradItem, l, iSellingPrice));
                });

                //Add each TraderLocation to the TraderItem.Locations collection
                tradItem.Locations.AddRange(tradItem.Domain.TraderLocations);

                dbContext.TraderItems.Add(tradItem);
                dbContext.Entry(tradItem).State = EntityState.Added;
                dbContext.SaveChanges();
                return tradItem;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                itemsWrong.Add(item);
                return null;
            }
        }

        private async Task UpdateItem(TraderItem traderItem, ItemImportData item, TraderItemImport import, List<ItemImportData> itemsWrong)
        {
            try
            {
                var domain = import.Domain;
                var location = import.Location;
                var createdBy = import.CreatedBy;

                var unit = new TraderConversionUnitRules(dbContext).GetOrCreateByName(createdBy, item.Unit, traderItem);
                // quantity
                var iInventoryLevel = HelperClass.Converter.Obj2Decimal(item.Inventory);

                var iCost = HelperClass.Converter.Obj2Decimal(item.Cost);
                var iSellingPrice = HelperClass.Converter.Obj2Decimal(item.SellingPrice);

                //https://atomsinteractive.atlassian.net/browse/QBIC-4636
                //If the item, for which the price is being updated, has no associated tax then
                //the Net Price and the Gross Price are the same = Import->Selling Price
                // the Price.TotalTaxAmount must also be set (to zero)
                //the Price.Taxes collection must be set to empty
                if (!traderItem.TaxRates.Any())
                {
                    var prices = new TraderPriceRules(dbContext).GetPriceByLocationIdItemId(location.Id, traderItem.Id);
                    prices.ForEach(price =>
                    {
                        price.NetPrice = iSellingPrice;
                        price.GrossPrice = iSellingPrice;
                        price.TotalTaxAmount = 0;
                        price.Taxes.Clear();

                        dbContext.Entry(price).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    });

                    UpdateCatalogPrices(traderItem.Id, location.Id, iSellingPrice);
                }
                else //traderItem.TaxRates > 0
                {
                    /*
                     If the item does have an associated tax then
                        for both Pricing Pool price and all associated Catalog Prices
                        update the net price to be the Import -> Selling Price
                        calculate the tax amount based on the taxes associated with the item
                        update the gross price to be the net price + tax amount
                        variant.BaseUnitPrice and Extra.BaseUnitPrice = PricingPoolPrice(Qbicles.Models.Trader.Pricing.Price)
                     */

                    //Pricing Pool price
                    var prices = new TraderPriceRules(dbContext).GetPriceByLocationIdItemId(location.Id, traderItem.Id);

                    prices.ForEach(price =>
                    {
                        // The selling price is the GrossPrice
                        price.GrossPrice = iSellingPrice;
                        // Calculate the NetPrice based on the Gross Prices and the associated taxes
                        price.NetPrice = price.GrossPrice / (1 + (price.Taxes.Sum(tx => tx.Rate) / 100));
                        // The Total Tax Amount is the GrossPrice minus the Net Price
                        price.TotalTaxAmount = price.GrossPrice - price.NetPrice;

                        dbContext.SaveChanges();
                    });

                    UpdateCatalogPrices(traderItem.Id, location.Id, iSellingPrice);
                }

                traderItem.Barcode = item.Barcode;
                traderItem.SKU = item.Sku;
                traderItem.Name = item.Name;
                traderItem.Description = item.Description;
                traderItem.DescriptionText = item.Description;
                traderItem.Units.ForEach(un =>
                {
                    if (un.Id == unit.Id)
                        return;
                    un.IsBase = false;
                    un.IsPrimary = false;
                });

                traderItem.IsBought = item.Type.ToLower() == "buy";
                traderItem.IsSold = item.Type.ToLower() == "sell";
                if (item.Type.ToLower() == "buy & sell")
                {
                    traderItem.IsBought = true;
                    traderItem.IsSold = true;
                }
                //Verify create new group
                traderItem.Group = GetOrCreateGroupByName(domain, createdBy, item.Group);
                dbContext.Entry(traderItem).State = EntityState.Modified;
                dbContext.SaveChanges();

                var inventoryDetail = dbContext.InventoryDetails.FirstOrDefault(i => i.Item.Id == traderItem.Id && i.Location.Id == location.Id);

                //Safe check create new inventory detail for old data wrong
                if (inventoryDetail == null)
                {
                    inventoryDetail = new InventoryDetail
                    {
                        MinInventorylLevel = 0,
                        MaxInventoryLevel = 0,
                        CurrentInventoryLevel = iInventoryLevel,
                        Location = location,
                        CreatedBy = import.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedBy = import.CreatedBy,
                        LastUpdatedDate = DateTime.UtcNow,
                        AverageCost = iCost,
                        LatestCost = iCost,
                    };

                    // No Inventory Batches are created here on an update
                    // Create only through InitialInventory

                    traderItem.InventoryDetails.Add(inventoryDetail);
                    dbContext.SaveChanges();
                }

                //If inventory in Spreadsheet is > 0
                // add in the inventory ALWAYS
                if (iInventoryLevel > 0)
                {
                    //Determine the cost for the incoming inventory
                    var invCost = iCost;
                    if (iCost == 0M)
                    {
                        invCost = inventoryDetail.AverageCost;
                    }

                    //Create the inventory
                    await new TraderInventoryRules(dbContext).InitialInventory(createdBy, traderItem, location, iInventoryLevel, invCost);

                    //No need to recalculate costs for the inventory, they are already calculated in InitialInventory
                }

                _itemsUpdated++;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                itemsWrong.Add(item);
            }
        }

        private void UpdateCatalogPrices(int traderItemId, int locationId, decimal iSellingPrice)
        {
            //associated Catalog Prices
            var posVariants = dbContext.PosVariants.Where(e => e.TraderItem.Id == traderItemId && e.CategoryItem.Category.Menu.Location.Id == locationId).ToList();
            var posExtras = dbContext.PosExtras.Where(e => e.TraderItem.Id == traderItemId && e.CategoryItem.Category.Menu.Location.Id == locationId).ToList();

            posVariants.ForEach(variant =>
            {
                if (variant.Price == null) return;
                //Update Price for variant

                // The selling price is the GrossPrice
                variant.Price.GrossPrice = iSellingPrice;
                // Calculate the NetPrice based on the Gross Prices and the associated taxes
                variant.Price.NetPrice = variant.Price.GrossPrice / (1 + (variant.Price.Taxes.Sum(tx => tx.Rate) / 100));
                // The TotalTaxAmount is the GrossPrice minus the NetPrice
                variant.Price.TotalTaxAmount = variant.Price.GrossPrice - variant.Price.NetPrice;

                variant.Price.Taxes.ForEach(tx =>
                {
                    tx.Amount = variant.Price.NetPrice * (tx.Rate / 100);
                    dbContext.Entry(tx).State = EntityState.Modified;
                });
                dbContext.Entry(variant).State = EntityState.Modified;
                dbContext.SaveChanges();
            });
            posExtras.ForEach(extra =>
            {
                if (extra.Price == null) return;

                //Update Price for extra

                // The selling price is the GrossPrice
                extra.Price.GrossPrice = iSellingPrice;
                // Calculate the NetPrice based on the Gross Prices and the associated taxes
                extra.Price.NetPrice = extra.Price.GrossPrice / (1 + (extra.Price.Taxes.Sum(tx => tx.Rate) / 100));
                // The TotalTaxAmount is the GrossPrice minus the NetPrice
                extra.Price.TotalTaxAmount = extra.Price.GrossPrice - extra.Price.NetPrice;

                extra.Price.Taxes.ForEach(tx =>
                {
                    tx.Amount = extra.Price.NetPrice * (tx.Rate / 100);
                    dbContext.Entry(tx).State = EntityState.Modified;
                });
                dbContext.Entry(extra).State = EntityState.Modified;
                dbContext.SaveChanges();
            });
        }

        /// <summary>
        /// Create the Price
        /// For Each Sales Channel (B2B, B2C, POS)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="location"></param>
        /// <param name="sellingPrice"></param>
        /// <returns></returns>
        private List<Price> CreatePrice(TraderItem item, TraderLocation location, decimal sellingPrice)
        {
            return new List<Price>
            {
                    new Price
                    {
                        Item = item,
                        SalesChannel = SalesChannelEnum.B2B,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = item.CreatedBy,
                        Location = location,
                        NetPrice = sellingPrice,
                        GrossPrice = sellingPrice,
                        TotalTaxAmount = 0
                    },
                    new Price
                    {
                        Item = item,
                        SalesChannel = SalesChannelEnum.B2C,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = item.CreatedBy,
                        Location = location,
                        NetPrice = sellingPrice,
                        GrossPrice = sellingPrice,
                        TotalTaxAmount = 0
                    },
                    new Price
                    {
                        Item = item,
                        SalesChannel = SalesChannelEnum.POS,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = item.CreatedBy,
                        Location = location,
                        NetPrice = sellingPrice,
                        GrossPrice = sellingPrice,
                        TotalTaxAmount = 0
                    },
            };
        }

        private void CollectData(int colIndex, string value, ItemImportData importData)
        {
            switch (colIndex)
            {
                case 0:
                    importData.Type = value;
                    break;

                case 1:
                    importData.Name = value;
                    break;

                case 2:
                    importData.Description = value;
                    break;

                case 3:
                    importData.Group = value;
                    break;

                case 4:
                    importData.Sku = value;
                    break;

                case 5:
                    importData.Barcode = value;
                    break;

                case 6:
                    importData.Unit = value;
                    break;

                case 7:
                    importData.Cost = value;
                    break;

                case 8:
                    importData.SellingPrice = value;
                    break;

                case 9:
                    importData.Inventory = value;
                    break;

                case 10:
                    importData.Active = value;
                    break;
            }
        }

        private bool Verifydata(ItemImportData import)
        {
            var correctData = true;
            for (int index = 0; index < _colNumber; index++)
            {
                switch (index)
                {
                    case 0://type - sring
                        correctData = !string.IsNullOrEmpty(import.Type);
                        break;

                    case 1://name - string
                        correctData = !string.IsNullOrEmpty(import.Name) && import.Name.Length < 50;
                        break;

                    case 2://description - string
                        correctData = !string.IsNullOrEmpty(import.Description) && import.Description.Length < 200;
                        break;

                    case 3://group - string
                        correctData = !string.IsNullOrEmpty(import.Group);
                        break;

                    case 4://sku - string
                        correctData = !string.IsNullOrEmpty(import.Sku);
                        break;

                    case 5://barcode - string
                        correctData = !string.IsNullOrEmpty(import.Barcode);
                        break;

                    case 6://unit - string
                        correctData = !string.IsNullOrEmpty(import.Unit);
                        break;

                    case 7://cost - decimal
                        correctData = HelperClass.Converter.Obj2DecimalNull(import.Cost) != null;
                        break;

                    case 8://selling price - int
                        correctData = HelperClass.Converter.Obj2DecimalNull(import.SellingPrice) != null;
                        break;

                    case 9://inventory - in
                        correctData = HelperClass.Converter.Obj2DecimalNull(import.Inventory) != null;
                        break;

                    case 10: //active - string ( bool Yes/No)
                        if (import.Active == "Yes" || import.Active == "No")
                            correctData = true;
                        else
                            correctData = false;
                        break;
                }
                if (!correctData)
                    break;
            }
            return correctData;
        }

        private bool VerifyData(int colIndex, string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            switch (colIndex)
            {
                case 1://name - string
                    return value.Length > 4 && value.Length < 50;

                case 0://type - sring
                case 2://description - string
                case 3://group - string
                case 4://sku - string
                case 5://barcode - string
                case 6://unit - string
                    return true;

                case 7://cost - decimal
                    return HelperClass.Converter.Obj2DecimalNull(value) != null;

                case 8://selling price - int
                    return HelperClass.Converter.Obj2IntNull(value) != null;

                case 9://inventory - in
                    return HelperClass.Converter.Obj2IntNull(value) != null;

                case 10: //active - string ( bool Yes/No)
                    if (value != "Yes" && value != "No")
                        return false;
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get or create a TraderGroup based on the Product Group column
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public TraderGroup GetOrCreateGroupByName(QbicleDomain domain, ApplicationUser user, string name)
        {
            try
            {
                var group = dbContext.TraderGroups.FirstOrDefault(e => e.Domain.Id == domain.Id && e.Name.ToLower() == name.ToLower());
                if (group != null)
                    return group;
                group = new TraderGroup
                {
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Domain = domain,
                    Name = name
                };
                return group;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
                return null;
            }
        }
    }
}