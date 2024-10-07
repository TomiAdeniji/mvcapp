using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Qbicles.BusinessRules.FilesUploadUtility
{
    public class ExcelStatus
    {
        public string Message { get; set; }
        public bool Success
        {
            get { return string.IsNullOrWhiteSpace(Message); }
        }
    }
    public class ExcelData
    {
        public ExcelStatus Status { get; set; }
        public Columns ColumnConfigurations { get; set; }
        public List<string> Headers { get; set; }
        public List<List<string>> DataRows { get; set; }
        public List<transactionStructure> datatransactions { get; set; }
        public string SheetName { get; set; }
        public string AccountNumber { get; set; }
        public ExcelData()
        {
            Status = new ExcelStatus();
            Headers = new List<string>();
            DataRows = new List<List<string>>();
            datatransactions = new List<transactionStructure>();
        }
    }

    public class ExcelSheets
    {
        public ExcelStatus Status { get; set; }
        public List<SheetUtility> Sheets { get; set; }
        public ExcelSheets()
        {
            Status = new ExcelStatus();
            Sheets = new List<SheetUtility>();
        }
    }

}