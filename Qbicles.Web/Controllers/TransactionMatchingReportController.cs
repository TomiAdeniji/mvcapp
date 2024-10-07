using GemBox.Spreadsheet;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.CleanBooks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using static Qbicles.BusinessRules.TransactionMatchingRules;

namespace Qbicles.Web.Controllers
{
    public class TransactionMatchingReportController : BaseController
    {
        // GET: TransactionMatchingReport
        // this ajax function is called by the client for each draw of the information on the page (i.e. when paging, ordering, searching, etc.).
        public ActionResult binTableMatchingReport(int draw, int start, int length, int accountId1, int accountId2, int transactionmatchingtaskId, int type)
        {
            try
            {
                string search = Request.QueryString["search[value]"];
                int sortColumn = -1;
                string sortDirection = "asc";

                // note: we only sort one column at a time
                if (Request.QueryString["order[0][column]"] != null)
                {
                    sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
                }
                if (Request.QueryString["order[0][dir]"] != null)
                {
                    sortDirection = Request.QueryString["order[0][dir]"];
                }

                var dataTableData = new DataTableDataReport();
                dataTableData.draw = draw;

                int recordsFiltered = 0;
                int recordsTotal = 0;
                dataTableData.data = new TransactionMatchingReportRules(dbContext).FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection, false, ref recordsTotal, accountId1, accountId2, transactionmatchingtaskId, type);
                dataTableData.recordsTotal = recordsTotal;
                dataTableData.recordsFiltered = recordsFiltered;

                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult bindingForReconciliationStatement(long accountId, long accountId2, int transactionmatchingtaskId, int taskId)
        {
            try
            {
                var refModel = new TransactionMatchingReportRules(dbContext).BindingForReconciliationStatement(accountId, accountId2, transactionmatchingtaskId, taskId);

                if (!refModel.result)
                {
                    var listErr = (List<string>)refModel.Object;
                    return Json(new { kq = "Err", listErr }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { kq = "Succ", refModel.Object }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var listErr = new List<string>();
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new { kq = "Err", listErr }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetMatchingProgress(int accountId, int accountId2, int taskId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel = new TransactionMatchingReportRules(dbContext).GetMatchingProgress(taskId, accountId, accountId2);

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new { refModel }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Export to excel

        public ActionResult ExportToExcel(int accountId1, string account1Name, int accountId2, string account2Name, int transactionmatchingtaskId, int taskId)
        {
            MsgModel model = new MsgModel();

            try
            {
                var path = "~/Upload/FileUpload";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path));

                var fileName = DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__") + "TransactionMatchingReport.xlsx";
                path += "/" + fileName;
                var refModel = new TransactionMatchingReportRules(dbContext).ExportToExcel(accountId1, account1Name, accountId2, account2Name, transactionmatchingtaskId, taskId);
                CreateFileExcel((DataSet)refModel.Object, path, (List<string>)refModel.Object2, account1Name, account2Name);
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

        public void CreateFileExcel(DataSet dsTable, string fileName, List<string> SheetNames, string account1Name, string account2Name)
        {
            string GemboxLicense = System.Configuration.ConfigurationManager.AppSettings["GemboxLicense"];
            SpreadsheetInfo.SetLicense(GemboxLicense);
            ExcelFile ef = new ExcelFile();
            int sheetno = 1, isfirst = 0;
            foreach (System.Data.DataTable dTable in dsTable.Tables)
            {
                System.Data.DataTable dataTable = dTable;
                int rowNo = dataTable.Rows.Count;
                int columnNo = dataTable.Columns.Count;
                int colIndex = 1;
                ExcelWorksheet ws = ef.Worksheets.Add(Convert.ToString(SheetNames[sheetno - 1]));

                sheetno++;
                //Generate Field Names
                if (isfirst > 0)
                {
                    switch (isfirst)
                    {
                        case 1:
                            ws.Cells[0, 0].Value = account1Name + " Unmatched Transactions";
                            ws.Cells[0, 0].Style.Font.Weight = GemBox.Spreadsheet.ExcelFont.BoldWeight;
                            break;

                        case 2:
                            ws.Cells[0, 0].Value = account2Name + " Unmatched Transactions";
                            ws.Cells[0, 0].Style.Font.Weight = GemBox.Spreadsheet.ExcelFont.BoldWeight;
                            break;

                        case 3:
                            ws.Cells[0, 0].Value = account1Name + " Matched Transactions";
                            ws.Cells[0, 0].Style.Font.Weight = GemBox.Spreadsheet.ExcelFont.BoldWeight;
                            break;

                        default:
                            ws.Cells[0, 0].Value = account2Name + " Matched Transactions";
                            ws.Cells[0, 0].Style.Font.Weight = GemBox.Spreadsheet.ExcelFont.BoldWeight;
                            break;
                    }

                    foreach (DataColumn dataColumn in dataTable.Columns)
                    {
                        ws.Cells[2, colIndex].Value = dataColumn.ColumnName;
                        ws.Cells[2, colIndex].Style.Font.Weight = GemBox.Spreadsheet.ExcelFont.BoldWeight;
                        ws.Columns[colIndex].Width = 20 * 256;
                        colIndex++;
                    }
                    //Convert DataSet to Cell Data
                    for (int row = 0; row < rowNo; row++)
                    {
                        for (int col = 0; col < columnNo; col++)
                        {
                            ws.Cells[row + 3, col + 1].Value = dataTable.Rows[row][col];
                        }
                    }
                }
                else
                {
                    //Convert DataSet to Cell Data
                    for (int row = 0; row < rowNo; row++)
                    {
                        for (int col = 0; col < columnNo; col++)
                        {
                            ws.Cells[row, col].Value = dataTable.Rows[row][col];
                        }
                    }
                }
                isfirst++;
            }
            if (System.IO.File.Exists(Server.MapPath(fileName)))
                System.IO.File.Delete(Server.MapPath(fileName));
            ef.Save(Server.MapPath(fileName));
        }

        #endregion Export to excel
    }
}