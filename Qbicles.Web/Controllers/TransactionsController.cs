using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using CleanBooksData;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Qbicles.BusinessRules;
using Newtonsoft.Json;
using nClam;
using Qbicles.BusinessRules.FilesUploadUtility;
using System.Web.Configuration;
using System.Globalization;
using Qbicles.BusinessRules.Helper;

namespace Qbicles.Web.Controllers
{

    public class TransactionsController : BaseController
    {

        public ActionResult ManageTransactions()
        {
            try
            {
                var transactionRules = new TransactionsRules(dbContext);
                ViewData["ColCount"] = 0;
                var fileTypes = transactionRules.GetFileType();
                var groups = transactionRules.GetAccountGroup();

                var uploads = transactionRules.GetUploadObject();

                ViewBag.CurrentPage = "managetransactions"; SetCurrentPage("managetransactions");
                
                ViewBag.groups = groups;
                ViewBag.fileTypes = fileTypes;
                return View(uploads);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// get transactions by uploadid
        /// </summary>
        /// <param name="UploadId"></param>
        /// <returns>uploads</returns>
        public ActionResult GetTransactions(int UploadId)
        {
            try
            {
                var transactionRules = new TransactionsRules(dbContext);
                var result = Json(transactionRules.GetTransactions(UploadId), JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = Int32.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }
        /// <summary>
        /// get upload by uploadid
        /// </summary>
        /// <param name="id">uploadid</param>
        /// <returns></returns>
        public ActionResult GetUploads(int id)
        {
            try
            {
                var transactionRules = new TransactionsRules(dbContext);

                var result = JsonConvert.SerializeObject(transactionRules.GetUploads(id), Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        /// <summary>
        /// cheking permission upload delete
        /// </summary>
        /// <param name="upload"></param>
        /// <returns></returns>
        public ActionResult ValidDelete(upload upload)
        {
            try
            {
                bool resTransaction = true;
                var transactionRules = new TransactionsRules(dbContext);
                return Json(new { resLastUpload = transactionRules.ValidDelete(upload, ref resTransaction), resTransaction = resTransaction }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DeleteUpload(upload upload)
        {
            try
            {
                decimal lastBalance = 0;
                int accountGroupId = 0;
                long accountId = 0;
                string lastUpload = "";
                var transactionRules = new TransactionsRules(dbContext);
                var rs = transactionRules.DeleteUpload(upload, User.Identity.GetUserId(), ref lastBalance, ref accountId, ref lastUpload, ref accountGroupId);
                return Json(new
                {
                    status = rs,
                    LastBalance = HelperClass.Converter.Obj2Decimal(lastBalance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat),
                    LastUpload = lastUpload,
                    AccountId = accountId,
                    AccountGroupId = accountGroupId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new
                {
                    status = false,
                    msg = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Read uload file and insert data to database

        /// <summary>
        /// validation file type upload
        /// </summary>
        /// <param name="fileutility"></param>
        /// <returns>
        /// 0- validation false
        /// 1- validation true
        /// 2- Please upload a valid excel file of version 2007 and above
        /// </returns>
        [HttpPost]
        public ActionResult ValidationFileType(FileUtility fileutility)
        {
            try
            {
                var valid = 0;
                if (fileutility.fileName == HelperClass.FileType.excelName)
                {
                    if (fileutility.fileExtension != HelperClass.FileType.excelExtension03 && fileutility.fileExtension != HelperClass.FileType.excelExtension07)
                        valid = 2;
                    else
                        valid = 1;
                }
                else if (fileutility.fileName == HelperClass.FileType.csvlName && fileutility.fileExtension == HelperClass.FileType.csvExtension)
                {
                    valid = 1;
                }
                else
                    valid = 0;

                return Json(new { valid = valid }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        /// <summary>
        /// scan virus file upload
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult VirusSacning(HttpPostedFileBase file)
        {
            try
            {
                //Remove session for active Bin Data
                Session.Remove("StartReadSheetData");
                Session.Remove("sheetId");
                //Scaner virut for file
                var scanner = VirusScannerFactory.GetVirusScanner();
                //var result = scanner.ScanStream(file.InputStream);

                var result = new ScanResult { IsVirusFree = true };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// Read sheet list using DocumentFormat.OpenXml
        /// </summary>
        /// <param name="file">file base</param>
        /// <returns>list sheet id,name</returns>
        public ActionResult ReadSheets(HttpPostedFileBase file)
        {
            try
            {
                var subPath = ConfigManager.UploadTransaction + CurrentUser().UserName + "/";
                if (!Directory.Exists(Server.MapPath(subPath)))
                    Directory.CreateDirectory(Server.MapPath(subPath));

                Session["FileName"] = Server.MapPath(subPath) + DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__") + CurrentUser().Id + ".xlsx";
                var data = (new ExcelReader()).ReadSheets(file, Session["FileName"].ToString());
                return Json(data, "application/json");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        //Funtion remove session -- Call when close popup or finish upload
        public void RemoveSession()
        {
            try
            {

                Session.Remove("dtTransactionFinal");
                Session.Remove("dtExcelUpload");
                Session.Remove("dtTransaction");
                Session.Remove("FileName");
                Session.Remove("FileErrorNameReport");
                Session.Remove("headColRequire");
                Session.Remove("filePathLocation");
                Session.Remove("AccountBalanceRecipe");
                Session.Remove("StartReadSheetData");
                Session.Remove("sheetId");
                Session.Remove("StartDataAnalyse");
                Session.Remove("headerColumns");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return;
            }
        }

        /// <summary>
        /// Compare list columns header have selected with columns required have fixed
        /// </summary>
        /// <param name="heads">list string columns header have selected</param>
        /// <returns>
        /// 1: if list columns required Contains list columns selected, and enable button Analyse
        /// 2: if list columns required not Contains list columns selected, and disnabe button Analyse
        /// </returns>
        public ActionResult ValidationSelectedHeadColumns(string heads, int accountId)
        {
            try
            {
                HashSet<string> transaction = JsonConvert.DeserializeObject<HashSet<string>>(heads);

                var result = HelperClass.ContainsAll(transaction, hardColumn.headColRequire);
                if (result)
                    return Json(1, JsonRequestBehavior.AllowGet);
                return Json(2, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }
        /// <summary>
        /// Covnert object to string function
        /// </summary>
        /// <param name="_string">object</param> 
        /// <returns>_string.ToString()  or string.Emty when convert _string error</returns>
        public string ConvertToString(object _string)
        {
            try
            {
                string output = _string.ToString();
                return output;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// upload and read data from excel file function
        /// check exist folder, if not exist then create folder in server and save file with path ../username/filename_dd_MM_yyyy_hh_mm.file extension
        /// read data upload file
        /// create a datatable after discard all row template, reusing next step
        /// convert data from upload file to display
        /// </summary>
        /// <param name="file">HttpPostedFileBase file</param>
        /// <param name="sheetId">string sheet id</param>
        /// <param name="_SheetName">string sheet name</param>
        /// <returns>status = "Done", dataTransactions = htmltable, colTransactions = colums names, lstdateformat = list date format</returns>
        public ActionResult ExcelFileUpload(string sheetId, int accountId, string _SheetName, decimal lastBalance)
        {
            try
            {
                if (ConvertToString(Session["StartReadSheetData"]) == "1" && ConvertToString(Session["sheetId"]) == sheetId.ToString())
                {
                    // Do not recompile the data 
                    var _return_old = Json(new
                    {
                        bindFormat = "Done",
                        StartReadSheetData = 1,
                        headColRequire = Session["headColRequire"]
                    }, JsonRequestBehavior.AllowGet);
                    _return_old.MaxJsonLength = Int32.MaxValue;
                    return _return_old;
                }
                // Set value for Session["StartReadSheetData"] and Session["sheetId"] 
                Session["StartReadSheetData"] = "1";
                Session["sheetId"] = sheetId;
                Session.Remove("StartDataAnalyse");
                Session.Remove("headerColumns");
                var transactionRules = new TransactionsRules(dbContext);
                DataTable dtTransactionFinal = new DataTable();
                DataTable dtExcelUpload = new DataTable();
                string headColRequire2 = "";
                string tableNew = "";
                string transaction = "";
                string bindFormat = "";
                int colCount = 0;
                var status = transactionRules.ExcelFileUpload(ref dtTransactionFinal, ref dtExcelUpload,
                    ref headColRequire2, ref tableNew, ref transaction, ref bindFormat, ref colCount,
                    Session["FileName"].ToString(), sheetId, accountId, _SheetName, lastBalance);

                Session["headColRequire"] = headColRequire2;

                Session["dtTransactionFinal"] = dtTransactionFinal; // Remove footer

                // DataTable dtPreview = dsFiles.Tables[0];
                Session["dtExcelUpload"] = dtExcelUpload;// dsFiles.Tables[0];

                // convert datatabse to string type of child tbody with 10 records begin affter row head
                if (status == "NoData")
                    return Json(new { status = status, transaction = transaction }, JsonRequestBehavior.AllowGet);
                //uploadfile.dtTransaction = uploadfile.dtExcelUpload.Rows.Cast<System.Data.DataRow>().Skip(rowNotDataIndex).CopyToDataTable();

                Session["dtTransaction"] = Session["dtExcelUpload"];

                var _return = Json(new
                {
                    status = status,
                    dataTransactionsNew = tableNew,
                    colTransactions = HelperClass.TransactionColName().ToList(),
                    colCount = colCount,
                    bindFormat = bindFormat,
                    headColRequire = headColRequire2
                }, JsonRequestBehavior.AllowGet);
                _return.MaxJsonLength = Int32.MaxValue;
                return _return;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new { status = "Error", transaction = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// get date from column date of upload file with list date format
        /// </summary>
        /// <param name="headerColumns"> Parameter check, if columns date then valid</param>
        /// <returns>list string date format</returns>
        public ActionResult GetDateFormat(string headerColumns)
        {
            try
            {
                var transactionRules = new TransactionsRules(dbContext);
                var listDtFormat = transactionRules.GetDateFormat(headerColumns, (DataTable)Session["dtTransaction"]);
                /*  get list format date */

                var _return = Json(new
                {
                    lstdateformat = listDtFormat
                }, JsonRequestBehavior.AllowGet);
                _return.MaxJsonLength = Int32.MaxValue;
                return _return;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        /// <summary>
        /// Confirm data and save to database
        /// </summary>
        /// <param name="uploadModal">upload entity</param>
        /// <param name="uploadFormatModal">UploadFormat entity</param>
        /// <param name="dateFormat">string date format</param>
        /// <returns>TempData["result_save_upload"] = "1" success,2 false,3 duplicate upload name</returns>
        public ActionResult Confirm2Save(upload uploadModal, UploadFormat uploadFormatModal, string dateFormat, string headerColumns, int selectedGroupAccountId = 0)
        {
            try
            {
                var transactionRules = new TransactionsRules(dbContext);
                string uploadAccountId = "", result_save_upload = "", uploadGroupAccountId = "", lastBalance = "", lastUpload = "";

                var result = transactionRules.Confirm2Save(ref lastBalance, ref lastUpload,
                    (DataTable)Session["dtTransactionFinal"], uploadModal, uploadFormatModal,
                    ref uploadAccountId, ref uploadGroupAccountId, ref result_save_upload,
                    dateFormat, headerColumns, selectedGroupAccountId = 0, User.Identity.GetUserId(),
                    Session["filePathLocation"] + "", Session["AccountBalanceRecipe"] + "");
                result.msg = lastBalance;
                result.msgName = lastUpload;
                if (result.actionVal == 1)
                    RemoveSession();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new ReturnJsonModel
                {
                    actionVal = 2
                };
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Validation data on upload file
        /// date, number on debit,credit, balance
        /// </summary>
        /// <param name="dateFormat">date format if exist</param>
        /// <param name="headerColumns"></param>
        /// <returns></returns>
        public ActionResult ColumnsSelectedAnalyse(string headerColumns, string accountLastBalance, int accountId, string accountName, string dateFormatSelected)
        {
            // table validate
            try
            {
                var transactionRules = new TransactionsRules(dbContext);
                List<string> obj = new List<string>();
                string message1 = "";
                string warningBalance = "";
                var rs = transactionRules.ColumnsSelectedAnalyse(ref obj, ref message1, ref warningBalance, (DataTable)Session["dtTransaction"], (DataTable)Session["dtTransactionFinal"], headerColumns, accountLastBalance, accountId, accountName, dateFormatSelected);
                if (rs == 0)
                {
                    return Json(new
                    {
                        analysis = false,
                        message = "A date format cannot be found that matches the data in the column you have selected. Please inform the Administrator."
                    }, JsonRequestBehavior.AllowGet);
                }
                if (rs == 2)
                {
                    return Json(new
                    {
                        analysis = false,
                        message = "A date format cannot be found that matches the data in the column you have selected. Please inform the Administrator."
                    }, JsonRequestBehavior.AllowGet);
                }

                var errorAnalyse = obj[0];
                var table = obj[1];
                warningBalance = obj[2] == "" ? warningBalance : obj[2];
                var addDebitCol = obj[3];
                var addCreditCol = obj[4];
                var addBalanceCol = obj[5];
                var warringDuplicate = obj[6];
                var _return = Json(new
                {
                    dataTransactionsNew = table,
                    dataError = errorAnalyse,
                    warningbalance = warningBalance,
                    addDebitCol = addDebitCol,
                    addCreditCol = addCreditCol,
                    addBalanceCol = addBalanceCol,
                    warringDuplicate = warringDuplicate,
                    message = message1
                }, JsonRequestBehavior.AllowGet);
                _return.MaxJsonLength = Int32.MaxValue;
                return _return;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// analyse data,checking for duplicate records, compare balance,sum credit, sum debit,count data,start date, endate
        /// </summary>
        /// <param name="headerColumns"></param>
        /// <returns></returns>
        public ActionResult DataAnalyse(string AccountId, string headerColumns, string accountName, string accountLastBalance)
        {
            try
            {
                if (ConvertToString(Session["StartDataAnalyse"]) == "1" && ConvertToString(Session["headerColumns"]) == headerColumns)
                {
                    // Do not recompile the data 
                    var _return_old = Json(new
                    {
                        StartDataAnalyse = 1
                    }, JsonRequestBehavior.AllowGet);
                    _return_old.MaxJsonLength = Int32.MaxValue;
                    return _return_old;
                }
                // Set value for Session["StartReadSheetData"] and Session["sheetId"] 
                Session["StartDataAnalyse"] = "1";
                Session["headerColumns"] = headerColumns;
                var transactionRules = new TransactionsRules(dbContext);
                List<string> analyse = new List<string>();
                DataTable datatabe_final = new DataTable();
                string GenListdateFormat1 = "";
                int? AccountBalanceRecipe = null;
                string FileErrorNameReport = "";
                var path = Server.MapPath("~/Upload/Transaction/" + User.Identity.GetUserName() + "/FileError/");
                var userinfo = CurrentUser();
                var rs = transactionRules.DataAnalyse((DataTable)Session["dtTransactionFinal"], ref datatabe_final, ref analyse, ref GenListdateFormat1, ref AccountBalanceRecipe, Session["headColRequire"] + "", path, userinfo.Id, userinfo.UserName, ref FileErrorNameReport, AccountId, headerColumns, accountName, accountLastBalance);
                Session["FileErrorNameReport"] = FileErrorNameReport;
                Session["AccountBalanceRecipe"] = AccountBalanceRecipe;
                if (rs == 0)
                {
                    var _return = Json(new
                    {
                        dataTransactions = "",
                        warningbalance = "error",
                        Errorfile = Session["FileErrorNameReport"],
                        GenListdateFormat = "",
                        dataAnalysedData = ""
                    }, JsonRequestBehavior.AllowGet);
                    _return.MaxJsonLength = Int32.MaxValue;
                    return _return;
                }

                Session["dtTransactionFinal"] = datatabe_final;
                var retAnalysedData = analyse[0];
                var retTransaction = analyse[1];

                var _return2 = Json(new
                {
                    dataTransactions = retTransaction,
                    warningbalance = "",
                    Errorfile = "",
                    GenListdateFormat = GenListdateFormat1,
                    dataAnalysedData = retAnalysedData
                }, JsonRequestBehavior.AllowGet);
                _return2.MaxJsonLength = Int32.MaxValue;
                return _return2;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// generate table again when date column is not selected
        /// </summary>
        /// <param name="headerColumns">list columns selected</param>
        /// <returns></returns>
        public ActionResult ReGenTable(string headerColumns)
        {
            try
            {
                DataTable dtDisplay = (DataTable)Session["dtExcelUpload"];
                List<string> lstColumns = JsonConvert.DeserializeObject<List<string>>(headerColumns);
                string table = HelperClass.ConvertDataTableToHTMLTableFirst(dtDisplay, lstColumns);
                var _return = Json(new
                {
                    dataTransactions = table
                }, JsonRequestBehavior.AllowGet);
                _return.MaxJsonLength = Int32.MaxValue;
                return _return;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        #endregion

    }
    #region NClam Antivirus
    public class VirusScannerFactory
    {
        public static IScanViruses GetVirusScanner()
        {
            return new ClamAvScanner();
        }
    }

    public class ScanResult
    {
        public string Message { get; set; }

        public object IsVirusFree { get; set; }
    }
    public interface IScanViruses
    {
        ScanResult ScanFile(string fullPath);


        ScanResult ScanBytes(byte[] bytes);

        ScanResult ScanStream(Stream stream);
    }

    public class ClamAvScanner : IScanViruses
    {
        public ScanResult ScanFile(string pathToFile)
        {
            var clam = new ClamClient("localhost", 3310);
            return MapScanResult(clam.ScanFileOnServer(pathToFile));
        }

        /// Scans some bytes for virus
        public ScanResult ScanBytes(byte[] data)
        {
            var clam = new ClamClient("localhost", 3310);
            return MapScanResult(clam.SendAndScanFile(data));
        }

        /// Scans your data stream for virus
        public ScanResult ScanStream(Stream stream)
        {
            var clam = new ClamClient("localhost", 3310);
            return MapScanResult(clam.SendAndScanFile(stream));
        }


        private ScanResult MapScanResult(ClamScanResult scanResult)
        {
            var result = new ScanResult();
            switch (scanResult.Result)
            {
                case ClamScanResults.Unknown:
                    result.Message = "Could not scan file";
                    result.IsVirusFree = false;
                    break;
                case ClamScanResults.Clean:
                    result.Message = "No Virus found";
                    result.IsVirusFree = true;
                    break;
                case ClamScanResults.VirusDetected:
                    result.Message = "Virus found: " + scanResult.InfectedFiles.First().VirusName;
                    result.IsVirusFree = false;
                    break;
                case ClamScanResults.Error:
                    result.Message = string.Format("VIRUS SCAN ERROR! {0}", scanResult.RawResult);
                    result.IsVirusFree = false;
                    break;
            }
            return result;
        }


    }
    #endregion
}