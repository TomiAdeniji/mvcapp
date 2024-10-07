using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Data;
using GemBox.Spreadsheet;
using System.IO.Packaging;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Reflection;

namespace Qbicles.BusinessRules.FilesUploadUtility
{ 
    public class ExcelReader
    {
        /// <summary>
        /// read all sheets in excel file
        /// </summary>
        /// <param name="file">HttpPostedFileBase file</param>
        /// <returns>
        /// data:
        /// data.Status.Message
        /// data.Success
        /// data.Sheets type of (SheetUtility)
        /// </returns>
        public ExcelSheets ReadSheets(HttpPostedFileBase file, string new_file_name)
        {
            var data = new ExcelSheets();

            if (file != null)
            {
                if (file.ContentLength <= 0)
                {
                    data.Status.Message = $"Unable to work with file {file.FileName}. Please check that the file can be opened in Microsoft Excel.";
                    return data;
                }
                string GemboxLicense = System.Configuration.ConfigurationManager.AppSettings["GemboxLicense"];
                SpreadsheetInfo.SetLicense(GemboxLicense);
                if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    //Save file xls and Conver XLS to XLSX
                    string new_file_name_xls = new_file_name.Replace(".xlsx", ".xls");
                    file.SaveAs(new_file_name_xls);
                    ExcelFile ef = ExcelFile.Load(new_file_name_xls);
                    ef.Save(new_file_name);
                    System.IO.File.Delete(new_file_name_xls);
                }
                else
                {
                    file.SaveAs(new_file_name); //Save file xlsx
                }
            }         

            try
            {
                //Read file and return LIST<Worksheets>
                Package spreadsheetPackage = Package.Open(new_file_name, FileMode.Open, FileAccess.ReadWrite);
                // Open a SpreadsheetDocument based on a package. Save status Date1904 to prive static
                SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(spreadsheetPackage); 
                //Find row, column hiden
                WorkbookPart wbPart = spreadsheetDocument.WorkbookPart;
                List<Sheet> theSheet = wbPart.Workbook.Descendants<Sheet>().ToList();
                foreach(var _sheet in theSheet)
                {
                    var s = new SheetUtility()
                    {
                        sheetId = _sheet.Id.ToString(),
                        sheetName = _sheet.Name
                    };
                    data.Sheets.Add(s);
                }
                spreadsheetDocument.Close();
                spreadsheetPackage.Close();
            } 
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                data.Status.Message = "Unable to open the file";
                return data;
            }
            return data;
        }

        private string GetColumnName(string cellReference)
        {
            var regex = new Regex("[A-Za-z]+");
            var match = regex.Match(cellReference);

            return match.Value;
        }

        private int ConvertColumnNameToNumber(string columnName)
        {
            var alpha = new Regex("^[A-Z]+$");
            if (!alpha.IsMatch(columnName)) throw new ArgumentException();

            char[] colLetters = columnName.ToCharArray();
            Array.Reverse(colLetters);

            var convertedValue = 0;
            for (int i = 0; i < colLetters.Length; i++)
            {
                char letter = colLetters[i];
                int current = i == 0 ? letter - 65 : letter - 64; // ASCII 'A' = 65
                convertedValue += current * (int)Math.Pow(26, i);
            }

            return convertedValue;
        }

        private IEnumerator<Cell> GetExcelCellEnumerator(Row row)
        {
            int currentCount = 0;
            foreach (Cell cell in row.Descendants<Cell>())
            {
                string columnName = GetColumnName(cell.CellReference);

                int currentColumnIndex = ConvertColumnNameToNumber(columnName);

                for (; currentCount < currentColumnIndex; currentCount++)
                {
                    var emptycell = new Cell() { DataType = null, CellValue = new CellValue(string.Empty) };
                    yield return emptycell;
                }

                yield return cell;
                currentCount++;
            }
        }

        private string ReadExcelCell(Cell cell, WorkbookPart workbookPart)
        {
            var cellValue = cell.CellValue;
            var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                text = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(
                        Convert.ToInt32(cell.CellValue.Text)).InnerText;
            }

            return (text ?? string.Empty).Trim();
        }
        /// <summary>
        /// read excel file with first sheet default
        /// </summary>
        /// <param name="file">HttpPostedFileBase file</param>
        /// <returns></returns>
        public ExcelData ReadExcel(HttpPostedFileBase file)
        {
            var data = new ExcelData();

            // Check if the file is excel
            if (file.ContentLength <= 0)
            {
                data.Status.Message = $"Unable to work with file {file.FileName}. Please check that the file can be opened in Microsoft Excel.";
                return data;
            }

            if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                data.Status.Message = "Please upload a valid excel file of version 2007 and above";
                return data;
            }

            // Open the excel document
            WorkbookPart workbookPart; List<Row> rows;
            try
            {
                var document = SpreadsheetDocument.Open(file.InputStream, false);
                workbookPart = document.WorkbookPart;

                var sheets = workbookPart.Workbook.Descendants<Sheet>();
                var sheet = sheets.First();
                data.SheetName = sheet.Name;

                var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;
                var columns = workSheet.Descendants<Columns>().FirstOrDefault();
                data.ColumnConfigurations = columns;

                var sheetData = workSheet.Elements<SheetData>().First();
                rows = sheetData.Elements<Row>().ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                data.Status.Message = "Unable to open the file";
                return data;
            }

            // Read the header
            if (rows.Count > 0)
            {
                var row = rows[0];
                var cellEnumerator = GetExcelCellEnumerator(row);
                while (cellEnumerator.MoveNext())
                {
                    var cell = cellEnumerator.Current;
                    var text = ReadExcelCell(cell, workbookPart).Trim();
                    data.Headers.Add(text);
                }
            }

            // Read the sheet data
            if (rows.Count > 1)
            {
                for (var i = 1; i < rows.Count; i++)
                {
                    var dataRow = new List<string>();
                    data.DataRows.Add(dataRow);
                    var row = rows[i];
                    var cellEnumerator = GetExcelCellEnumerator(row);
                    while (cellEnumerator.MoveNext())
                    {
                        var cell = cellEnumerator.Current;
                        var text = ReadExcelCell(cell, workbookPart).Trim();
                        dataRow.Add(text);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Read excel file with sheetId
        /// parameter: headerRow - get list header in row = headerrow
        /// parameter: accountRow - get Account number in row = accountRow
        /// parameter: dataRow - get data transaction from row = accountRow to end file
        /// </summary>
        /// <param name="file">HttpPostedFileBase file</param>
        /// <param name="sheetId">string sheetId</param>
        /// <param name="headersRowInt">int - header row</param>
        /// <param name="accountRowInt">int - Account row</param>
        /// <param name="dataRowInt">int - data row start</param>
        /// <returns></returns>
        public ExcelData ReadExcel(HttpPostedFileBase file, string sheetId, int headersRowInt, int accountRowInt, int dataRowInt)
        {
            // begin use parameter, have - 1 (e.g:headersRowInt -1)  because row start =0, but user input row number of file start = 1;
            var data = new ExcelData();

            // Check if the file is excel
            if (file.ContentLength <= 0)
            {
                data.Status.Message = $"Unable to work with file {file.FileName}. Please check that the file can be opened in Microsoft Excel.";
                return data;
            }

            if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                data.Status.Message = "Please upload a valid excel file of version 2007 and above";
                return data;
            }

            // Open the excel document
            WorkbookPart workbookPart; List<Row> rows;
            try
            {
                var document = SpreadsheetDocument.Open(file.InputStream, false);
                workbookPart = document.WorkbookPart;

                var sheet = workbookPart.Workbook.Descendants<Sheet>().Where(e => e.Id == sheetId).FirstOrDefault();
                //var sheet = sheets.Where(e => e.Id == sheetId).FirstOrDefault();
                data.SheetName = sheet.Name;

                var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;
                var columns = workSheet.Descendants<Columns>().FirstOrDefault();
                data.ColumnConfigurations = columns;

                var sheetData = workSheet.Elements<SheetData>().First();
                rows = sheetData.Elements<Row>().ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                data.Status.Message = "Unable to open the file";
                return data;
            }
            try
            {
                // Read the header
                if (rows.Count > 0)
                {
                    var row = rows[headersRowInt - 1];
                    var cellEnumerator = GetExcelCellEnumerator(row);
                    while (cellEnumerator.MoveNext())
                    {
                        var cell = cellEnumerator.Current;
                        var text = ReadExcelCell(cell, workbookPart).Trim();
                        data.Headers.Add(text);
                    }
                }

                // Read the sheet data
                if (rows.Count > 1)
                {
                    // get account number
                    string accounttmp = "";
                    var rowAcc = rows[accountRowInt - 1];
                    var cellEnumeratorAcc = GetExcelCellEnumerator(rowAcc);
                    while (cellEnumeratorAcc.MoveNext())
                    {
                        var cell = cellEnumeratorAcc.Current;
                        var text = ReadExcelCell(cell, workbookPart).Trim();
                        accounttmp = text;
                        break;// deault fix account in row accountRowIn and column 0
                    }
                    string[] acc = accounttmp.Trim().Replace(" ", "").Split('(');
                    data.AccountNumber = acc[0].ToString();
                    // get data transactions
                    for (var i = dataRowInt - 1; i < rows.Count; i++)
                    {
                        var dataRow = new List<string>();

                        data.DataRows.Add(dataRow);

                        var row = rows[i];
                        var cellEnumerator = GetExcelCellEnumerator(row);
                        while (cellEnumerator.MoveNext())
                        {

                            var cell = cellEnumerator.Current;
                            var text = ReadExcelCell(cell, workbookPart).Trim();
                            dataRow.Add(text);

                        }
                    }
                }
                DataTable dt = new DataTable();
                int index = 1;
                foreach (var item in data.Headers)
                {

                    if (string.IsNullOrWhiteSpace(item))
                    {
                        dt.Columns.Add("Ignore Column " + index.ToString(), typeof(string));
                        index++;
                    }
                    else
                        dt.Columns.Add(item);

                }
                foreach (var datarows in data.DataRows)
                {
                    dt.Rows.Add();
                    for (int i = 0; i < datarows.Count; i++)
                    {
                        dt.Rows[dt.Rows.Count - 1][i] = datarows[i];
                    }
                }
                data.datatransactions = HelperClass.ConvertDataTable2List<transactionStructure>(dt);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                data.Status.Message = "Unable to open the file";
                return data;
            }
            
            return data;
        }

    }
}