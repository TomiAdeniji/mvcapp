using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Data.Entity;
using System.Globalization;
using System.IO;
//using System.IO.Packaging;
using System.Text;
using CleanBooksData;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using GemBox.Spreadsheet;
using Newtonsoft.Json;
using Qbicles.BusinessRules.FilesUploadUtility;
using Qbicles.Models;
using Qbicles.BusinessRules.Helper;
using System.IO.Packaging;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class TransactionsRules
    {
        ApplicationDbContext _db;
        public TransactionsRules()
        {
        }

        public TransactionsRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }


        public IEnumerable<filetype> GetFileType()
        {
            return DbContext.filetypes.ToList();
        }
        public IEnumerable<accountgroup> GetAccountGroup()
        {
            return DbContext.accountgroups.Include(m => m.Accounts).ToList();
        }
        public IEnumerable<ApplicationUser> GetUserRole()
        {
            return DbContext.QbicleUser.ToList(); ;
        }

        public IEnumerable<upload> GetUploadObject()
        {
            var uploads = DbContext.uploads
                .Include(u => u.user)
                .Include(m => m.account).ToList();
            return uploads;
        }

        public string GetTransactions(int UploadId)
        {
            var uploads = DbContext.transactions
                .Where(m => m.UploadId == UploadId).ToList();

            StringBuilder retTable = new StringBuilder();
            foreach (var transaction in uploads)
            {
                retTable.Append("<tr>");
                retTable.AppendFormat("<td class='Date' Date-value='{1}'>{0}</td>", Convert.ToDateTime(transaction.Date),
                    Convert.ToDateTime(transaction.Date).ToString("MM/dd/yyyy hh:mm"));
                retTable.AppendFormat("<td class='Reference' Reference-value='{0}'>{0}</td>", transaction.Reference);
                retTable.AppendFormat("<td class='Description' Description-value='{0}'>{0}</td>", transaction.Description);
                var debitVal = HelperClass.Converter.Obj2Decimal(transaction.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                retTable.AppendFormat("<td class='Debit' Debit-value='{1}'>{0}</td>", debitVal, debitVal);
                var creditVal = HelperClass.Converter.Obj2Decimal(transaction.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                retTable.AppendFormat("<td class='Credit' Credit-value='{1}'>{0}</td>", creditVal, creditVal);
                var balanceVal = HelperClass.Converter.Obj2Decimal(transaction.Balance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                retTable.AppendFormat("<td class='Balance' Balance-value='{1}'>{0}</td>", balanceVal, balanceVal);
                retTable.Append("</tr>");
            }

            return retTable.ToString();
        }
        /// <summary>
        /// get upload by uploadid
        /// </summary>
        /// <param name="id">uploadid</param>
        /// <returns></returns>
        public object GetUploads(int id)
        {
            bool proxyCreation = DbContext.Configuration.ProxyCreationEnabled;
            try
            {

                DbContext.Configuration.ProxyCreationEnabled = false;
                var transaction = DbContext.uploads
                    .Where(m => m.Id == id)
                    .Select(m => new
                    {
                        transaction = m,
                        account = m.account,
                        lineitems = m.transactions.ToList(),
                        uploadformat = m.UploadFormat
                    })
                    .FirstOrDefault();
                return transaction;
            }
            catch
            {
                return null;
            }
            finally
            {
                DbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }

        /// <summary>
        /// cheking permission upload delete
        /// </summary>
        /// <param name="upload"></param>
        /// <returns></returns>
        public object ValidDelete(upload upload, ref bool resTransaction)
        {
            bool resLastUpload = false;// false - not last upload, true - last upload
                                       // bool resTransaction = true;//true - not associated with taskinstance,false - associated with taskinstance
                                       //get last upload of account
            var dateLastUpload = DbContext.uploads.Where(m => m.AccountId == upload.AccountId).OrderByDescending(m => m.CreatedDate).FirstOrDefault().CreatedDate;
            //check last upload
            if (dateLastUpload != upload.CreatedDate)
                resLastUpload = false;
            else
                resLastUpload = true;
            if (resLastUpload)
            {
                var transactions = DbContext.transactions.Where(tr => tr.UploadId == upload.Id);

                resTransaction = transactions.Any(r => r.transactionmatchingrecords.Count > 0);

            }
            return resLastUpload;
        }

        public bool DeleteUpload(upload upload, string userId, ref decimal lastBalance, ref long accountId, ref string lastUpload, ref int accountGroupId)
        {
            var uploadRemove = DbContext.uploads.Find(upload.Id);
            if (uploadRemove == null) return false;

            if (uploadRemove.account.BookkeepingAccount != null)
                return DeleteImport(uploadRemove, userId, ref lastBalance, ref accountId, ref lastUpload, ref accountGroupId);

            //remove uploadFormat
            var uploadFormat = uploadRemove.UploadFormat;//new UploadFormat { Id = upload.UploadFormatId };
            DbContext.uploadformats.Attach(uploadFormat);
            DbContext.uploadformats.Remove(uploadFormat);
            //remove upload
            DbContext.uploads.Attach(uploadRemove);
            DbContext.uploads.Remove(uploadRemove);

            //delete transaction
            var transaction = uploadRemove.transactions;//DbContext.transactions.Where(ta => ta.UploadId == upload.Id);
            //Check exits Balance in ACC 
            int exitsColumnBalance = DbContext.uploadfields.Count(a => a.AccountId == upload.AccountId && a.Name == "Balance");
            //decimal credit = 0, debits = 0;
            var cbAccount = uploadRemove.account;
            decimal openingBalance = cbAccount?.LastBalance ?? 0;
            if (exitsColumnBalance > 0) //Exit balance --> Update balance when delete Upload
            {
                foreach (var tran in transaction)
                {
                    if (cbAccount.BalanceRecipe == 1)
                    {
                        openingBalance = openingBalance - (tran.Credit ?? 0) + (tran.Debit ?? 0);
                    }
                    else
                    {
                        openingBalance = openingBalance + (tran.Credit ?? 0) - (tran.Debit ?? 0);
                    }
                }
            }
            DbContext.transactions.RemoveRange(transaction);
            //create deletedupload
            var delUpload = new deletedupload()
            {
                AccountId = upload.AccountId,
                UploadName = upload.Name,
                DeletedById = userId,
                NumberOfTransactions = transaction.Count().ToString(),
                DeletedDate = DateTime.UtcNow
            };
            DbContext.deleteduploads.Add(delUpload);
            DbContext.Entry(delUpload).State = EntityState.Added;

            DbContext.SaveChanges();
            // re-update lastbalance of account 

            cbAccount.LastBalance = openingBalance;
            var ac = DbContext.Entry(cbAccount);
            if (DbContext.Entry(cbAccount).State == EntityState.Detached)
                DbContext.Accounts.Attach(cbAccount);
            ac.Property(m => m.LastBalance).IsModified = true;
            DbContext.SaveChanges();

            var uploadByAccount = DbContext.uploads.Where(x => x.AccountId == upload.AccountId);
            if (!uploadByAccount.Any())
            {
                cbAccount.BalanceRecipe = null;
                DbContext.SaveChanges();
            }
            accountGroupId = cbAccount.GroupId;
            lastBalance = openingBalance;
            var lastUploadDb = uploadByAccount.OrderByDescending(d => d.CreatedDate).FirstOrDefault()?.CreatedDate.Value;
            if (lastUploadDb != null)
                lastUpload = lastUploadDb.Value.ToString("dd/MM/yyyy hh:mm tt");
            else
                lastUpload = "";
            accountId = upload.AccountId;
            return true;
        }


        public bool DeleteImport(upload uploadRemove, string userId, ref decimal lastBalance, ref long accountId, ref string lastUpload, ref int accountGroupId)
        {
            //remove uploadFormat
            var uploadFormat = uploadRemove.UploadFormat;// new UploadFormat { Id = uploadRemove.UploadFormatId };
            //DbContext.uploadformats.Attach(uploadFormat);
            DbContext.uploadformats.Remove(uploadFormat);

            //delete transaction

            var transaction = uploadRemove.transactions.Where(ta => ta.UploadId == uploadRemove.Id).ToList();
            DbContext.transactions.RemoveRange(transaction);
            //remove upload
            //DbContext.uploads.Attach(uploadRemove);
            DbContext.uploads.Remove(uploadRemove);

            //create deletedupload
            var delUpload = new deletedupload()
            {
                AccountId = uploadRemove.AccountId,
                UploadName = uploadRemove.Name,
                DeletedById = userId,
                NumberOfTransactions = transaction.Count().ToString(),
                DeletedDate = DateTime.UtcNow
            };
            DbContext.deleteduploads.Add(delUpload);
            DbContext.Entry(delUpload).State = EntityState.Added;

            DbContext.SaveChanges();
            // re-update lastbalance of account 


            lastUpload = "";
            var createdDate = DbContext.uploads.OrderByDescending(d => d.CreatedDate).FirstOrDefault(x => x.AccountId == uploadRemove.AccountId)?.CreatedDate;
            if (createdDate != null)
            {
                lastUpload = createdDate.Value.ToString("dd/MM/yyyy hh:mm tt");
            }

            lastBalance = 0;
            accountGroupId = uploadRemove.account?.GroupId ?? 0;
            accountId = uploadRemove.AccountId;
            return true;
        }




        public List<transaction> GetTransactionObject(long accountId)
        {
            return DbContext.transactions.Where(e => e.upload.AccountId == accountId).OrderBy(d => d.CreatedDate).AsQueryable().ToList();
        }
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
        public string ExcelFileUpload(ref DataTable dtTransactionFinal, ref DataTable dtExcelUpload, ref string headColRequire, ref string tableNew, ref string transaction, ref string bindFormat, ref int colCount, string FileName, string sheetId, int accountId, string _SheetName, decimal lastBalance)
        {

            var upFields = DbContext.uploadfields.Where(a => a.AccountId == accountId).ToList();
            hardColumn.headColRequire = new HashSet<string>();
            foreach (var item in upFields)
            {
                hardColumn.headColRequire.Add(item.Name);
            }
            headColRequire = string.Join(", ", hardColumn.headColRequire);

            int rowNotDataIndex = 0;
            // ***read date 1904 status
            Package spreadsheetPackage = Package.Open(FileName, FileMode.Open, FileAccess.ReadWrite);
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
            ExcelFile excelFile = ExcelFile.Load(FileName);
            //excelFile.LoadXlsx(FileName, XlsxOptions.None);
            string[] columns;
            int curRow = 0;
            int curCol = 0;
            DataTable dataTable = new DataTable();
            ExcelWorksheet worksheet = excelFile.Worksheets[_SheetName];
            for (; curRow < worksheet.Rows.Count; curRow++)
            {
                int total_column = worksheet.Rows[curRow].AllocatedCells.Count;
                if (total_column > dataTable.Columns.Count)
                {
                    for (int x = dataTable.Columns.Count; x < total_column; x++)
                    {
                        dataTable.Columns.Add("Column " + x);
                    }
                }
                columns = new string[dataTable.Columns.Count];
                for (curCol = 0; curCol < total_column; curCol++)
                {
                    //Check Merge and UnMerge
                    ExcelCell cell = worksheet.Rows[curRow].Cells[curCol];
                    // Get merged range in which that cell is.
                    CellRange mergedRange = cell.MergedRange;
                    if (mergedRange != null)
                    {
                        CellRange wsMergedRange = worksheet.Cells.GetSubrangeAbsolute(mergedRange.FirstRowIndex, mergedRange.FirstColumnIndex, mergedRange.LastRowIndex, mergedRange.LastColumnIndex);

                        // Unmerge cells (note that all cells would now have the value that was set to merged range).
                        wsMergedRange.Merged = false;

                        // We want that only cell in the upper-left corner has merged value -> so delete values from other cells.
                        for (int i = mergedRange.FirstRowIndex; i <= mergedRange.LastRowIndex; i++)
                            for (int j = mergedRange.FirstColumnIndex; j <= mergedRange.LastColumnIndex; j++)
                                if (i != mergedRange.FirstRowIndex || j != mergedRange.FirstColumnIndex)
                                    worksheet.Cells[i, j].Value = null;
                    }

                    if (worksheet.Rows[curRow].Cells[curCol].Value == null)
                        columns[curCol] = "";
                    else
                        columns[curCol] = worksheet.Rows[curRow].Cells[curCol].Value.ToString();
                }
                dataTable.Rows.Add(columns);
            }
            //END Read the file and return the table

            int _delete = 1;
            foreach (Column _item in _ListHidenCol)
            {
                dataTable.Columns.RemoveAt((int)_item.Min.Value - _delete);
                _delete++;
            }
            _delete = 1;
            foreach (int x in _ListHidenRow)
            {
                dataTable.Rows.RemoveAt(x - _delete);
                _delete++;
            }
            dataTable.Rows.Cast<DataRow>().ToList().FindAll(Row =>
            { return String.IsNullOrEmpty(String.Join("", Row.ItemArray)); }).ForEach(Row =>
            {
                dataTable.Rows.Remove(Row);
            });
            dataTable.AcceptChanges();
            //find row first data
            int rowFirstData = 0; bool firstCol = false;
            string dateString = "";
            DateTime retVal = DateTime.Today;
            Decimal _decimal = 0;
            string[] DateFormat = _db.dateformats.Select(x => x.Format).ToArray();
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn cl in dataTable.Columns)
                {
                    firstCol = true;
                    try
                    {
                        dateString = row[cl.ColumnName].ToString().TrimStart().TrimEnd();
                        if (!(DateTime.TryParse(dateString, new CultureInfo("en-US"), DateTimeStyles.None, out retVal) || DateTime.TryParse(dateString, new CultureInfo("vi-VN"), DateTimeStyles.None, out retVal)) || Decimal.TryParse(dateString, out _decimal))
                        {
                            firstCol = false;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch
                    {
                        firstCol = false;
                    }
                }
                if (firstCol)
                    break;
                rowFirstData++;
            }
            rowNotDataIndex = rowFirstData;
            //Skip hearder - retaining only one headline
            DataTable _Dataforfinal = new DataTable();
            if (rowFirstData > 3) //Row header >3 --> keep max:3 row header
            {
                dataTable = dataTable.Rows.Cast<System.Data.DataRow>().Skip(rowFirstData - 3).CopyToDataTable();
                //Datatable finish -- remove all hearder, remove all footer
                _Dataforfinal = dataTable.Rows.Cast<System.Data.DataRow>().Skip(3).CopyToDataTable(); // Remove all header
            }
            else
                _Dataforfinal = dataTable.Rows.Cast<System.Data.DataRow>().Skip(rowFirstData).CopyToDataTable(); // Remove all header 
            rowFirstData = _Dataforfinal.Rows.Count;
            dateString = "";
            for (int x = _Dataforfinal.Rows.Count - 1; x >= 0; x--)
            {
                foreach (DataColumn cl in _Dataforfinal.Columns)
                {
                    firstCol = true;
                    try
                    {
                        dateString = _Dataforfinal.Rows[x][cl.ColumnName].ToString().TrimStart().TrimEnd();

                        if (!(DateTime.TryParse(dateString, new CultureInfo("en-US"), DateTimeStyles.None, out retVal) || DateTime.TryParse(dateString, new CultureInfo("vi-VN"), DateTimeStyles.None, out retVal)) || Decimal.TryParse(dateString, out _decimal))
                        {
                            firstCol = false;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch
                    {
                        firstCol = false;
                    }
                }
                if (firstCol)
                    break;
                rowFirstData--;
            }
            dtTransactionFinal = _Dataforfinal.Rows.Cast<System.Data.DataRow>().Take(rowFirstData).CopyToDataTable(); // Remove footer

            dtExcelUpload = dataTable;

            // convert datatabse to string type of child tbody with 10 records begin affter row head
            if (dataTable.Rows.Count == rowNotDataIndex || dataTable.Columns.Count < 3)
            {
                transaction = "No File selected";
                return "NoData";
            }

            var account = DbContext.Accounts.FirstOrDefault(p => p.Id == accountId);
            if (account != null)
            {
                account.LastBalance = lastBalance;
                DbContext.SaveChanges();
            }
            // get last uloadformat by account
            upload up = DbContext.uploads.Where(u => u.AccountId == accountId).OrderByDescending(e => e.CreatedDate).Include(ufo => ufo.UploadFormat).FirstOrDefault();

            // table html to view with 50 records begin affter row head
            tableNew = HelperClass.ConvertDataTableToHTMLTableUpload(dataTable.Rows.Cast<DataRow>().
               Take(50).CopyToDataTable(), (up?.UploadFormat));
            bindFormat = "None";
            if (up != null && up.UploadFormat != null)
                bindFormat = "Done";
            // return view
            colCount = dataTable.Columns.Count;
            return "Done";

        }

        public List<string> GetDateFormat(string headerColumns, DataTable dtTransaction)
        {
            /*  get list format date */
            List<string> listDtFormat = new List<string>();
            string[] dtFormats = _db.dateformats.Select(d => d.Format).ToArray();
            string dateString = "";
            int dateIndex = 0;
            List<string> lstColumns = JsonConvert.DeserializeObject<List<string>>(headerColumns);
            foreach (string item in lstColumns)
            {
                int i = 0;
                if (item == "Date")
                    dateIndex = i;
                i++;
            }

            foreach (var dateFormat in dtFormats)
            {
                bool valid = true;
                foreach (DataRow row in dtTransaction.Rows)
                {
                    try
                    {
                        dateString = row[dateIndex].ToString().TrimStart().TrimEnd();
                        if (HelperClass.Converter.Obj2Date(dateString, dateFormat) == null)
                        {
                            valid = false;
                        }
                        else
                            valid = true;
                    }
                    catch
                    {
                        valid = false;
                    }
                }
                if (valid)
                    listDtFormat.Add(dateFormat);
            }
            //list date format to return view
            var lstDateFormat = listDtFormat.Distinct().ToList();
            return lstDateFormat;

        }
        public ReturnJsonModel Confirm2Save(ref string lastBalance, ref string lastUpload,
            DataTable dtTransactionFinal, upload uploadModal, UploadFormat uploadFormatModal,
            ref string uploadAccountId, ref string uploadGroupAccountId, ref string result_save_upload,
            string dateFormat, string headerColumns, int selectedGroupAccountId = 0, string userId = "",
            string filePathLocation = "", string AccountBalanceRecipe = null)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (DbContext.uploads.Any(m => m.Id != uploadModal.Id && (m.Name.Trim() == uploadModal.Name.Trim() && uploadModal.AccountId == m.AccountId)))
                {
                    refModel.actionVal = 3;
                    return refModel;
                }

                /* 1. [Bind transaction]*/

                DbContext.Configuration.AutoDetectChangesEnabled = false;
                DbContext.Configuration.ValidateOnSaveEnabled = false;

                foreach (DataRow row in dtTransactionFinal.Rows)
                {
                    var tranEntity = new transaction()
                    {
                        CreatedDate = DateTime.UtcNow,
                        IsActive = 1,
                        Date = HelperClass.Converter.Obj2Date(row["Date"], dateFormat)
                    };
                    if (headerColumns.Contains("Debit"))
                        tranEntity.Debit = HelperClass.Converter.Obj2DecimalNull(row["Debit"]) == 0 ? null : HelperClass.Converter.Obj2DecimalNull(row["Debit"]);
                    if (headerColumns.Contains("Credit"))
                        tranEntity.Credit = HelperClass.Converter.Obj2DecimalNull(row["Credit"]) == 0 ? null : HelperClass.Converter.Obj2DecimalNull(row["Credit"]);
                    if (headerColumns.Contains("Balance"))
                        tranEntity.Balance = HelperClass.Converter.Obj2DecimalNull(row["Balance"]);
                    if (headerColumns.Contains("Description"))
                        tranEntity.Description = row["Description"].ToString().TrimStart().TrimEnd();

                    if (headerColumns.Contains("DescCol1"))
                        tranEntity.DescCol1 = row["DescCol1"].ToString().TrimStart().TrimEnd();

                    if (headerColumns.Contains("DescCol2"))
                        tranEntity.DescCol2 = row["DescCol2"].ToString().TrimStart().TrimEnd();

                    if (headerColumns.Contains("DescCol3"))
                        tranEntity.DescCol3 = row["DescCol3"].ToString().TrimStart().TrimEnd();

                    if (headerColumns.Contains("Reference"))
                        tranEntity.Reference = row["Reference"].ToString().TrimStart().TrimEnd();

                    if (headerColumns.Contains("Reference1"))
                        tranEntity.Reference1 = row["Reference1"].ToString().TrimStart().TrimEnd();

                    uploadModal.transactions.Add(tranEntity);
                }
                //last uploadId
                int lastUploadId = 0;
                var first = DbContext.uploads.Where(p => p.AccountId == uploadModal.AccountId).OrderByDescending(o => o.Id).FirstOrDefault();
                if (first != null)
                    lastUploadId = first.Id;



                /* 2. [Bind data upload] */
                uploadModal.CreatedById = userId;
                uploadModal.CreatedDate = DateTime.UtcNow;
                uploadModal.FilePath = string.IsNullOrEmpty(filePathLocation) ? filePathLocation : string.Empty;
                uploadModal.StartDate = HelperClass.Converter.Obj2Date(dtTransactionFinal.Rows[0]["Date"], dateFormat);
                uploadModal.EndDate = HelperClass.Converter.Obj2Date(dtTransactionFinal.Rows[dtTransactionFinal.Rows.Count - 1]["Date"], dateFormat);

                var lstDateformat = DbContext.dateformats.ToList();
                /* 3. [Bind data UploadFormat] */
                var firstDefault = lstDateformat.Where(e => e.Format == dateFormat).FirstOrDefault();
                var dateformatid = firstDefault == null ? lstDateformat.FirstOrDefault().Id : firstDefault.Id;
                uploadFormatModal.CreatedById = userId;
                uploadFormatModal.IsDateAscending = false;
                uploadFormatModal.DateFormatId = dateformatid;
                uploadFormatModal.FileTypeId = DbContext.filetypes.FirstOrDefault(t => t.Type == "Excel").Id;
                List<string> lstColumns = JsonConvert.DeserializeObject<List<string>>(headerColumns);
                int indexCol = 1;
                foreach (string item in lstColumns)
                {
                    if (item.Contains("Date"))
                        uploadFormatModal.DateIndex = indexCol;
                    if (item.Contains("Reference"))
                        uploadFormatModal.ReferenceIndex = indexCol;
                    if (item.Contains("Description"))
                        uploadFormatModal.DescriptionIndex = indexCol;
                    if (item.Contains("Debit"))
                        uploadFormatModal.DebitIndex = indexCol;
                    if (item.Contains("Credit"))
                        uploadFormatModal.CreditIndex = indexCol;
                    if (item.Contains("Balance"))
                        uploadFormatModal.BalanceIndex = indexCol;
                    if (item.Contains("Reference1"))
                        uploadFormatModal.Reference1Index = indexCol;
                    if (item.Contains("DescCol1"))
                        uploadFormatModal.DescCol1Index = indexCol;
                    if (item.Contains("DescCol2"))
                        uploadFormatModal.DescCol2Index = indexCol;
                    if (item.Contains("DescCol3"))
                        uploadFormatModal.DescCol3Index = indexCol;
                    indexCol++;
                }
                uploadFormatModal.uploads.Add(uploadModal);
                decimal accountBefore = 0, accountAfter = 0;

                /* 4. [Upload last balance in account table] */
                if (headerColumns.Contains("Balance"))
                {
                    Account acc = DbContext.Accounts.Find(uploadModal.AccountId);
                    accountBefore = acc.LastBalance ?? 0;
                    acc.LastBalance = HelperClass.Converter.Obj2DecimalNull(dtTransactionFinal.Rows[dtTransactionFinal.Rows.Count - 1]["Balance"]);
                    accountAfter = HelperClass.Converter.Obj2DecimalNull(dtTransactionFinal.Rows[dtTransactionFinal.Rows.Count - 1]["Balance"]) ?? 0;
                    if (!string.IsNullOrEmpty(AccountBalanceRecipe))
                    {
                        switch (AccountBalanceRecipe)
                        {
                            case "1":
                                acc.BalanceRecipe = 1;
                                break;
                            case "0":
                                acc.BalanceRecipe = 0;
                                break;
                            default:
                                acc.BalanceRecipe = null;
                                break;
                        }
                    }
                    var ac = DbContext.Entry(acc);
                    if (DbContext.Entry(acc).State == EntityState.Detached)
                        DbContext.Accounts.Attach(acc);
                    ac.Property(m => m.LastBalance).IsModified = true;
                    ac.Property(m => m.BalanceRecipe).IsModified = true;
                }

                /*5. [Save to db] */
                uploadAccountId = uploadModal.AccountId.ToString();
                uploadGroupAccountId = selectedGroupAccountId.ToString();
                var uformat = DbContext.Entry(uploadFormatModal);
                uformat.State = EntityState.Added;
                if (DbContext.SaveChanges() > 0)
                {
                    var accountRules = new CBAccountRules(DbContext);
                    var account = accountRules.GetAccount((int)uploadModal.AccountId, true);
                    lastBalance = HelperClass.Converter.Obj2Decimal(account.LastBalance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    var lastUploadDate = account.uploads.OrderByDescending(d => d.CreatedDate).FirstOrDefault()?.CreatedDate.Value;
                    if (lastUploadDate != null)
                        lastUpload = lastUploadDate.Value.ToString("dd/MM/yyyy hh:mm tt");
                    else
                        lastUpload = "";
                    result_save_upload = "1";
                    SingleAlertCheck(lastUploadId, uploadModal.AccountId, accountBefore, accountAfter);
                    MultipleAlertCheck(uploadModal.AccountId, uploadModal.Name);
                    refModel.actionVal = 1;
                    return refModel;
                }
                refModel.actionVal = 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.actionVal = 0;
            }
            finally
            {
                DbContext.Configuration.AutoDetectChangesEnabled = true;
                DbContext.Configuration.ValidateOnSaveEnabled = true;
            }
            return refModel;
        }
        public int ColumnsSelectedAnalyse(ref List<string> obj, ref string message1, ref string warningBalance1, DataTable dtDisplay, DataTable dtTransactionFinal, string headerColumns, string accountLastBalance, int accountId, string accountName, string dateFormatSelected)
        {
            string accName = accountName.Split('-')[0];

            List<string> lstColumns = JsonConvert.DeserializeObject<List<string>>(headerColumns);
            /*  get list format date */
            List<string> listDateFormat = new List<string>();
            string[] dtFormats = _db.dateformats.Select(d => d.Format).ToArray();
            string dateString = "";
            int dateIndex = 0, balanceIndex = -1, debitIndex = -1, creditIndex = -1;
            foreach (string item in lstColumns)
            {
                string[] cols = item.Split('_');
                if (cols[0].ToString().TrimStart().TrimEnd() == "Date")
                {
                    dateIndex = HelperClass.Converter.Obj2Int(cols[1]) - 1;// -1 because column starting 0
                }
                if (cols[0].ToString().TrimStart().TrimEnd() == "Balance")
                {
                    balanceIndex = HelperClass.Converter.Obj2Int(cols[1]) - 1;// -1 because column starting 0
                }
                if (cols[0].ToString().TrimStart().TrimEnd() == "Debit")
                {
                    debitIndex = HelperClass.Converter.Obj2Int(cols[1]) - 1;// -1 because column starting 0 
                }
                if (cols[0].ToString().TrimStart().TrimEnd() == "Credit")
                {
                    creditIndex = HelperClass.Converter.Obj2Int(cols[1]) - 1;// -1 because column starting 0 
                }
            }

            //find row first data
            int rowFirstData = 0; bool findFirstrow = false;
            string dateresult = "";
            foreach (var _dateFormat in dtFormats)
            {
                rowFirstData = 0;

                foreach (DataRow row in dtDisplay.Rows)
                {
                    try
                    {
                        dateString = row[dateIndex].ToString().TrimStart().TrimEnd();
                        var tmps = dateString.Split(' ');
                        if (tmps.Length > 2)
                        {
                            var tmp1 = tmps[tmps.Length == 4 ? 2 : 1].Split(':');
                            if (Convert.ToInt32(tmp1[0]) < 10)
                                dateresult = tmps[0] + " 0" + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];
                            else
                                dateresult = tmps[0] + " " + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];

                            if (HelperClass.Converter.Obj2DateNull(dateresult, _dateFormat) == null)
                            {
                                rowFirstData++;
                            }
                            else
                            {
                                findFirstrow = true;
                                break;
                            }

                        }
                        else if (tmps.Length == 2)
                        {
                            var tmp1 = tmps[1].Split(':');
                            if (tmp1.Length >= 3)
                            {
                                if (Convert.ToInt32(tmp1[0]) < 10)
                                    dateresult = tmps[0] + " 0" + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];
                                else
                                    dateresult = tmps[0] + " " + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];

                            }
                            else if (tmp1.Length == 2)
                            {
                                if (Convert.ToInt32(tmp1[0]) < 10)
                                    dateresult = tmps[0] + " 0" + tmp1[0] + (string.IsNullOrEmpty(tmp1[1]) ? "" : ":" + tmp1[1]);
                                else
                                    dateresult = tmps[0] + " " + tmp1[0] + (string.IsNullOrEmpty(tmp1[1]) ? "" : ":" + tmp1[1]);
                            }
                            else
                                dateresult = tmps[0];

                            if (HelperClass.Converter.Obj2DateNull(dateresult, _dateFormat) == null)
                            {
                                rowFirstData++;
                            }
                            else
                            {
                                findFirstrow = true;
                                break;
                            }
                        }
                        else
                        {
                            dateresult = tmps[0];
                            if (HelperClass.Converter.Obj2DateNull(dateresult, _dateFormat) == null)
                            {
                                rowFirstData++;
                            }
                            else
                            {
                                findFirstrow = true;
                                break;
                            }
                        }
                    }
                    catch
                    {
                        rowFirstData++;
                    }

                }
                if (findFirstrow) break;
            }
            if (!findFirstrow)
            {
                return 0;
            }
            dtDisplay = dtDisplay.Rows.Cast<System.Data.DataRow>().Skip(rowFirstData).CopyToDataTable();

            foreach (var _dateFormat in dtFormats)
            {
                bool valid = true;
                foreach (DataRow row in dtDisplay.Rows)
                {
                    try
                    {
                        dateString = row[dateIndex].ToString().TrimStart().TrimEnd();
                        var tmps = dateString.Split(' ');
                        if (tmps.Length > 2)
                        {
                            var tmp1 = tmps[tmps.Length == 4 ? 2 : 1].Split(':');
                            if (Convert.ToInt32(tmp1[0]) < 10)
                                dateresult = tmps[0] + " 0" + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];
                            else
                                dateresult = tmps[0] + " " + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];

                            if (HelperClass.Converter.Obj2Date(dateresult, _dateFormat) == null)
                            {
                                valid = false;
                                break;
                            }
                            valid = true;
                        }
                    }
                    catch
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                    listDateFormat.Add(_dateFormat);
            }
            if (listDateFormat.Count == 0)
            {
                return 2;
            }

            //list date format to return view
            decimal lastBalance = HelperClass.Converter.Obj2Decimal(JsonConvert.DeserializeObject<string>(accountLastBalance ?? "0"));
            decimal openningBalance = 0;
            if (balanceIndex >= 0)
                openningBalance = HelperClass.Converter.Obj2Decimal(dtDisplay.Rows[0][balanceIndex]);
            if (debitIndex >= 0)
                openningBalance -= HelperClass.Converter.Obj2Decimal(dtDisplay.Rows[0][debitIndex]);
            if (creditIndex >= 0)
                openningBalance += HelperClass.Converter.Obj2Decimal(dtDisplay.Rows[0][creditIndex]);

            bool reCalBalance = true; string message = ""; string warningBalance = "";

            if (lastBalance == 0 && balanceIndex < 0)
            {
                reCalBalance = false;
                message = "No Balance is supplied and there is no Last Balance in the Account '" + accName
                + "'</br>No balance values will be calculated for the upload.";
            };
            var analyse = HelperClass.ConvertDataTableToHTMLTableAnalyse(
                dtDisplay, lstColumns, listDateFormat, lastBalance, accountId, accName, dateFormatSelected, reCalBalance);

            if (dtTransactionFinal.Rows.Count > 0)
            {
                openningBalance = 0;
                if (balanceIndex >= 0)
                    openningBalance = HelperClass.Converter.Obj2Decimal(dtTransactionFinal.Rows[0][balanceIndex]);
                if (debitIndex >= 0)
                    openningBalance -= HelperClass.Converter.Obj2Decimal(dtTransactionFinal.Rows[0][debitIndex]);
                if (creditIndex >= 0)
                    openningBalance += HelperClass.Converter.Obj2Decimal(dtTransactionFinal.Rows[0][creditIndex]);

                if (analyse[6] == "") // if not exist duplicate, then check balance again
                {
                    if (lastBalance > 0 && lastBalance != openningBalance && balanceIndex >= 0)
                    {
                        warningBalance = "The Closing Balance of Account: '" + accName +
                            "' (" + lastBalance.ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + " )"
                         + " does not match the opening balance of the transactions being uploaded ("
                         + openningBalance.ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + " )";
                    }
                }
            }
            obj = analyse;
            message1 = message;
            warningBalance1 = warningBalance;
            return 1;
        }
        /// <summary>
        /// analyse data,checking for duplicate records, compare balance,sum credit, sum debit,count data,start date, endate
        /// </summary>
        /// <param name="headerColumns"></param>
        /// <returns></returns>
        public int DataAnalyse(DataTable datatabe_final, ref DataTable dtTransactionFinal, ref List<string> analyse, ref string GenListdateFormat1, ref int? AccountBalanceRecipe, string headColRequire, string path, string userId, string userName, ref string FileErrorNameReport, string AccountId, string headerColumns, string accountName, string accountLastBalance)
        {

            //Get data default

            int _AccountId = HelperClass.Converter.Obj2Int(AccountId);
            Account account = DbContext.Accounts.Where(x => x.Id == _AccountId).FirstOrDefault();
            upload previousUpload = DbContext.uploads.Where(x => x.AccountId == _AccountId).OrderByDescending(y => y.Id).FirstOrDefault();
            UploadFormat _uploadformatnew = null;
            if (previousUpload != null)
            {
                int uploadId = previousUpload.UploadFormatId;
                _uploadformatnew = DbContext.uploadformats.Where(x => x.Id == uploadId).OrderByDescending(y => y.Id).Include(d => d.dateformat).FirstOrDefault();
            }
            string Old_DateFormat = (_uploadformatnew != null) ? _uploadformatnew.dateformat.Format : string.Empty;
            List<string> List_date_format = DbContext.dateformats.Select(x => x.Format).ToList<string>();
            bool Status_CheckDateformat = true, Status_CheckBalance = true;
            decimal Old_lastBalance = HelperClass.Converter.Obj2Decimal(JsonConvert.DeserializeObject<string>(accountLastBalance ?? "0"));
            var OriginalOpeningBalance = Old_lastBalance;
            List<string> lstColumns_header = JsonConvert.DeserializeObject<List<string>>(headerColumns);

            //Start Check data ( Date, debit, credits, balance, if error save to New file excel )
            int dateIndex = 0, balanceIndex = -1, debitIndex = -1, creditIndex = -1;
            //Set column name 
            for (int x = 0; x < datatabe_final.Columns.Count; x++)
            {
                datatabe_final.Columns[x].ColumnName = x.ToString(); //Set for not Duplicate ColumnName
            }
            datatabe_final.AcceptChanges();
            int _col_index = 0;
            foreach (string item in lstColumns_header)
            {
                string[] cols = item.Split('_');
                string _col_name = cols[0].ToString().TrimStart().TrimEnd();
                int _col_number = HelperClass.Converter.Obj2Int(cols[1]) - 1;
                if (_col_name == "Ignorecolumn")
                    _col_name = _col_name + _col_index.ToString();
                datatabe_final.Columns[_col_index].ColumnName = _col_name;
                _col_index++;
                switch (_col_name)
                {
                    case "Date":
                        dateIndex = _col_number;
                        break;
                    case "Balance":
                        balanceIndex = _col_number;
                        break;
                    case "Debit":
                        debitIndex = _col_number;
                        break;
                    case "Credit":
                        creditIndex = _col_number;
                        break;
                }
            }
            datatabe_final.AcceptChanges();
            //Check Duplicate data
            int row_Duplicate = 0;
            DateTime? _row_date;
            decimal? _row_credits, _row_debits, _row_balance;
            foreach (DataRow row in datatabe_final.Rows)
            {
                _row_date = HelperClass.Converter.Obj2Date(row[dateIndex], Old_DateFormat);
                _row_credits = (decimal)((creditIndex != -1) ? Math.Round(HelperClass.Converter.Obj2DecimalNull(row[creditIndex].ToString() != "" ? row[creditIndex] : 0) ?? 0, 2) : 0);
                _row_debits = (decimal)((debitIndex != -1) ? Math.Round(HelperClass.Converter.Obj2DecimalNull(row[debitIndex].ToString() != "" ? row[debitIndex] : 0) ?? 0, 2) : 0);
                _row_balance = (decimal)((balanceIndex != -1) ? Math.Round(HelperClass.Converter.Obj2DecimalNull(row[balanceIndex].ToString() != "" ? row[balanceIndex] : 0) ?? 0, 2) : 0);

                if (Status_CheckDateformat)
                {
                    if (_row_date == null || _row_balance == null
                        || _row_debits == null || _row_credits == null)
                    {
                        break; // can not convert date || debit || credis || balance --> break
                    }
                    // Date = date, debit=debit,credit = credit, balance = balance , accountid = accountid --> Is Duplicate
                    DateTime _returndate = _row_date.Value.Date;
                    int _accountId = HelperClass.Converter.Obj2IntNull(AccountId) == null ? 0 : (int)HelperClass.Converter.Obj2IntNull(AccountId);
                    int _Duplicate = DbContext.transactions.Where(x => x.Date == _returndate &&
                                                                       x.Credit == _row_credits && x.Debit == _row_debits &&
                                                                       x.Balance == _row_balance && x.upload.AccountId == _accountId).AsQueryable().Count();
                    if (_Duplicate > 0) //Is Duplicate
                    {
                        row_Duplicate++;
                    }
                    else break;
                }
            }
            if (row_Duplicate > 0)// Remove Duplicate Data
            {
                DataTable _newdata = datatabe_final.Rows.Cast<DataRow>().Skip(row_Duplicate).CopyToDataTable();
                datatabe_final = _newdata.Copy();
            }
            bool Status_Error = false;
            int Row_index = 2;
            //Create new file error: Col1-date, Col2-Credit, Col3-Debit, Col4-Balance, Col5-Description Error
            string FileErrorName = string.Empty;
            string FileErrorName2 = "";
            FileErrorName = CreateNewExcelFile_ForError(FileErrorName, path, userId, userName, ref FileErrorName2);
            FileErrorNameReport = FileErrorName2;
            string GemboxLicense = ConfigurationManager.AppSettings["GemboxLicense"];
            SpreadsheetInfo.SetLicense(GemboxLicense);
            ExcelFile ef = ExcelFile.Load(FileErrorName);
            ExcelWorksheet ws = ef.Worksheets.FirstOrDefault();
            int? status_checkBalacneReceip = account.BalanceRecipe; //formulaic status assigned to the balance
            foreach (DataRow row in datatabe_final.Rows) //Check error data
            {
                _row_credits = (decimal)((creditIndex != -1) ? Math.Round(HelperClass.Converter.Obj2DecimalNull(row[creditIndex].ToString() != "" ? row[creditIndex] : 0) ?? 0, 2) : 0);
                _row_debits = (decimal)((debitIndex != -1) ? Math.Round(HelperClass.Converter.Obj2DecimalNull(row[debitIndex].ToString() != "" ? row[debitIndex] : 0) ?? 0, 2) : 0);
                _row_balance = (decimal)((balanceIndex != -1) ? Math.Round(HelperClass.Converter.Obj2DecimalNull(row[balanceIndex].ToString() != "" ? row[balanceIndex] : 0) ?? 0, 2) : 0);

                string error = string.Empty;
                //Check Date
                if (List_date_format.Count > 0)
                {
                    List_date_format = CheckFormatDate(row[dateIndex].ToString().TrimStart(' ').TrimEnd(' '), List_date_format);
                    if (List_date_format.Count == 0) //Error no exit date Format
                    {
                        Status_Error = true;
                        error += "No suitable date format from the first line to the this line. ";
                    }
                }
                //Check Credits
                if (creditIndex != -1 && HelperClass.Converter.Obj2DecimalNull(row[creditIndex].ToString()) == null && row[creditIndex].ToString().Replace(" ", "") != string.Empty)
                {
                    Status_Error = true;
                    error += "The Credit on this line cannot be converted into a number. ";
                    Status_CheckBalance = false; //Not check balance
                }
                //Check Debits
                if (debitIndex != -1 && HelperClass.Converter.Obj2DecimalNull(row[debitIndex].ToString()) == null && row[debitIndex].ToString().Replace(" ", "") != string.Empty)
                {
                    Status_Error = true;
                    error += "The Debit on this line cannot be converted into a number. ";
                    Status_CheckBalance = false; //Not check balance
                }
                //Check Balance

                if (headColRequire.IndexOf("Balance") != -1) //headColRequire Exist Balance 
                {
                    if (HelperClass.Converter.Obj2DecimalNull(row[balanceIndex].ToString()) == null)
                    {
                        Status_Error = true;
                        error += "The Balance on this line cannot be converted into a number ";
                        Status_CheckBalance = false; //Not check balance
                    }
                    if (Status_CheckBalance)
                    {
                        if (status_checkBalacneReceip == null)
                        {
                            if (Old_lastBalance == _row_balance + _row_debits - _row_credits)
                            {
                                AccountBalanceRecipe = status_checkBalacneReceip = 1;
                                Old_lastBalance = (decimal)(_row_balance);
                            }
                            else if ((Old_lastBalance == (_row_balance - _row_debits + _row_credits)))
                            {
                                AccountBalanceRecipe = status_checkBalacneReceip = 0;
                                Old_lastBalance = (decimal)(_row_balance);
                            }
                            else
                            {
                                Status_Error = true;
                                Status_CheckBalance = false;
                                error += "The Balance on this line cannot be calculated from the previous entries";
                            }
                        }
                        else if (((status_checkBalacneReceip == 1) && (Old_lastBalance == _row_balance + _row_debits - _row_credits)) ||
                            ((status_checkBalacneReceip == 0) && ((Old_lastBalance == (_row_balance - _row_debits + _row_credits)))))
                        {
                            Old_lastBalance = (decimal)(_row_balance);
                        }
                        else
                        {
                            Status_Error = true;
                            Status_CheckBalance = false;
                            error += "The Balance on this line cannot be calculated from the previous entries";
                        }
                    }
                }
                //Save error in row to new file
                if (error != string.Empty)
                {
                    ws.Cells[Row_index - 1, 0].Value = row[dateIndex];

                    ws.Cells[Row_index - 1, 1].Value = creditIndex != -1 ? row[creditIndex] : "";
                    ws.Cells[Row_index - 1, 1].Style.HorizontalAlignment = HorizontalAlignmentStyle.Right;

                    ws.Cells[Row_index - 1, 2].Value = debitIndex != -1 ? row[debitIndex] : "";
                    ws.Cells[Row_index - 1, 2].Style.HorizontalAlignment = HorizontalAlignmentStyle.Left;

                    ws.Cells[Row_index - 1, 3].Value = balanceIndex != -1 ? row[balanceIndex] : "";

                    ws.Cells[Row_index - 1, 4].Value = error;
                    ws.Cells[Row_index - 1, 4].Style.Font.Color = SpreadsheetColor.FromName(ColorName.Red);
                    Row_index++;
                }
            }
            ef.Save(FileErrorName);
            if (Status_Error)
            {
                //error exists
                return 0;
            }
            //not error exists
            List<string> _header = headerColumns.Split(',').ToList();
            string _newheader = string.Empty; ;
            // remove header Ignorecolumn
            int indext_column = 1;
            foreach (var _item in _header)
            {
                if (_item.IndexOf("Ignorecolumn") == -1)
                {
                    List<string> _name = _item.Replace(" ", "").Split('_').ToList();
                    _newheader += _name[0].Replace(" ", "") + "_" + indext_column.ToString() + "\",";
                    indext_column += 1;
                }
            }
            if (_newheader.Length > 0)
                _newheader = _newheader.Substring(0, _newheader.Length - 1); // remove "," in last string.
            //remove colum Ignorecolumn
            DataTable _Dafordisplay = datatabe_final.Copy();
            indext_column = 0;
            foreach (DataColumn _column in datatabe_final.Columns)
            {
                if (_column.ColumnName.IndexOf("Ignorecolumn") != -1)
                    _Dafordisplay.Columns.RemoveAt(indext_column);
                else
                    indext_column += 1;
            }
            _Dafordisplay.AcceptChanges();
            var analyse1 = HelperClass.ConvertTransactionToHTMLTable(_Dafordisplay, _newheader, List_date_format.FirstOrDefault().Replace("\n", ""), OriginalOpeningBalance, 50);
            analyse = analyse1;
            GenListdateFormat1 = GenListdateFormat(List_date_format);
            dtTransactionFinal = datatabe_final.Copy();
            return 1;
        }
        private string GenListdateFormat(List<string> Listdateformat)
        {
            string _listdateformat = ""; bool _firt = true;
            foreach (var item in Listdateformat)
            {
                if (_firt)
                {
                    _listdateformat += "<option data-mytxt='' selected value ='" + item + "'>" + item + "</option >";
                    _firt = false;
                }
                else
                {
                    _listdateformat += "<option data-mytxt='' value ='" + item + "'>" + item + "</option >";
                }
            }
            return _listdateformat;

        }
        //Funtion check format date
        private List<string> CheckFormatDate(string datetime, List<string> Dateformat)
        {
            List<string> return_list = new List<string>();
            foreach (string _format in Dateformat)
            {
                if (HelperClass.Converter.Obj2DateNull(datetime, _format) != null)
                    return_list.Add(_format);
            }
            return return_list;
        }
        //Funtion Creat new file excel save error
        private string CreateNewExcelFile_ForError(string FileErrorName, string path, string userId, string userName, ref string FileErrorNameReport)
        {
            // Create Path

            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            //Create file error
            //Create path file by user and date.
            FileErrorName = path + DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__") + userId + ".xlsx";
            FileErrorNameReport = "../Upload/Transaction/" + userName.TrimStart().TrimEnd() + "/FileError/" + DateTime.UtcNow.ToString("dd_mm_yyyy_hh_MM_ss__") + userId + ".xlsx";
            //Create new file error
            string GemboxLicense = ConfigurationManager.AppSettings["GemboxLicense"];
            SpreadsheetInfo.SetLicense(GemboxLicense);
            ExcelFile ef = new ExcelFile();
            ExcelWorksheet ws = ef.Worksheets.Add("Report an error");
            ws.Cells[0, 0].Value = "Date";
            ws.Cells[0, 0].Style.Font.Weight = ExcelFont.BoldWeight;
            ws.Cells[0, 1].Value = "Credits";
            ws.Cells[0, 1].Style.Font.Weight = ExcelFont.BoldWeight;
            ws.Cells[0, 2].Value = "Debits";
            ws.Cells[0, 2].Style.Font.Weight = ExcelFont.BoldWeight;
            ws.Cells[0, 3].Value = "Balance";
            ws.Cells[0, 3].Style.Font.Weight = ExcelFont.BoldWeight;
            ws.Cells[0, 4].Value = "Error Description";
            ws.Cells[0, 4].Style.Font.Weight = ExcelFont.BoldWeight;
            ef.Save(FileErrorName);
            return FileErrorName;
        }
        private void SingleAlertCheck(int lastUploadId, long accountId, decimal accountBefore, decimal accountAfter)
        {
            var singleAlert = DbContext.singleaccountalerts.Where(p => p.AccountId == accountId).ToList();

            if (singleAlert != null && singleAlert.Any())
            {
                foreach (var it in singleAlert)
                {
                    if (accountBefore <= 0)
                    {
                        var uploads = (from c in DbContext.transactions.Where(m => m.UploadId == lastUploadId)
                                       join t in DbContext.transactionanalysisrecords.Where(p => p.ProfileValue == it.Profile) on c.Id equals t.TransactionId
                                       select new { c.Credit, c.Debit }).ToList();
                        accountBefore = Math.Abs(uploads.Sum(s => s.Debit ?? 0) - uploads.Sum(s => s.Credit ?? 0));
                    }
                    if (accountAfter <= 0)
                    {
                        var UploadId = DbContext.uploads.Where(p => p.AccountId == accountId).OrderByDescending(o => o.Id).FirstOrDefault().Id;

                        var uploads = (from c in DbContext.transactions.Where(m => m.UploadId == UploadId)
                                       join t in DbContext.transactionanalysisrecords.Where(p => p.ProfileValue == it.Profile) on c.Id equals t.TransactionId
                                       select new { c.Credit, c.Debit }).ToList();
                        accountAfter = Math.Abs(uploads.Sum(s => s.Debit ?? 0) - uploads.Sum(s => s.Credit ?? 0));
                    }
                    var user = (from c in DbContext.singleaccountalertusersxrefs.Where(p => p.SingleAccountAlertId == it.Id)
                                join u in DbContext.QbicleUser on c.UsersId equals u.Id
                                select new { u.Email, u.UserName }).ToList();
                    switch (it.AlertConditionId)
                    {
                        case 1:
                            if (it.Amount != null && it.Amount > 0 && Math.Abs(accountBefore - accountAfter) > it.Amount)
                            {
                                if (user != null && user.Any())
                                {
                                    foreach (var u in user)
                                    {
                                        SendMail(1, u.Email, u.UserName, "Single Account Alert", accountId, HelperClass.Converter.Obj2Decimal(Math.Abs(accountBefore - accountAfter)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), HelperClass.Converter.Obj2Decimal(it.Amount ?? 0).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                }
                            }
                            break;
                        case 2:
                            if (it.Amount != null && it.Amount > 0 && Math.Abs(accountBefore - accountAfter) < it.Amount)
                            {
                                if (user != null && user.Any())
                                {
                                    foreach (var u in user)
                                    {
                                        SendMail(1, u.Email, u.UserName, "Single Account Alert", accountId, HelperClass.Converter.Obj2Decimal(Math.Abs(accountBefore - accountAfter)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), HelperClass.Converter.Obj2Decimal(it.Amount ?? 0).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                }
                            }
                            break;
                        case 3:
                            var pd = (Math.Abs(accountBefore - accountAfter) / ((accountBefore + accountAfter) == 0 ? 1 : (Math.Abs(accountBefore + accountAfter) / 2))) * 100;
                            if (pd > it.Percentage)
                            {
                                if (user != null && user.Any())
                                {
                                    foreach (var u in user)
                                    {
                                        SendMail(2, u.Email, u.UserName, "Single Account Alert", accountId, HelperClass.Converter.Obj2Decimal(pd).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), HelperClass.Converter.Obj2Decimal(it.Percentage ?? 0).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }

            }
        }

        private void MultipleAlertCheck(long AccountId, string Name)
        {
            var multiAlert = DbContext.multipleaccountalerts.Where(p => p.alertmultipleaccounts.Any(a => a.AccountId == AccountId)).ToList();
            if (multiAlert != null && multiAlert.Any())
            {
                foreach (var item in multiAlert)
                {
                    var lstUser = (from c in DbContext.multipleaccountalertuserxrefs.Where(p => p.MultipleAccountAlertId == item.Id)
                                   join u in DbContext.QbicleUser on c.UsersId equals u.Id
                                   select new { u.Email, u.UserName }).ToList();
                    var lstAccount = (from alert in DbContext.alertmultipleaccounts.Where(p => p.MultipleAccountAlertId == item.Id)
                                      join a in DbContext.Accounts on alert.AccountId equals a.Id
                                      select new { alert.AccountId, a.Name, a.LastBalance }).ToList();
                    decimal totalCredit = 0, totalDebit = 0;
                    if (lstAccount != null && lstAccount.Any())
                    {
                        if (lstAccount.Count > 1)
                        {
                            decimal balance1 = 0, balance2 = 0;
                            balance1 = lstAccount[0].LastBalance ?? 0;
                            balance2 = lstAccount[1].LastBalance ?? 0;
                            if (lstAccount[0].LastBalance == null || lstAccount[0].LastBalance == 0)
                            {
                                var acc = lstAccount[0].AccountId;
                                var acc1 = DbContext.transactions.Where(t => t.upload.AccountId == acc).Select(s => new { s.Credit, s.Debit }).ToList();
                                totalCredit = acc1.Sum(s => s.Credit ?? 0);
                                totalDebit = acc1.Sum(s => s.Debit ?? 0);
                                balance1 = totalDebit - totalCredit;
                            }
                            if (lstAccount[1].LastBalance == null || lstAccount[1].LastBalance == 0)
                            {
                                var acc = lstAccount[1].AccountId;
                                var acc2 = DbContext.transactions.Where(t => t.upload.AccountId == acc).Select(s => new { s.Credit, s.Debit }).ToList();
                                totalCredit = acc2.Sum(s => s.Credit ?? 0);
                                totalDebit = acc2.Sum(s => s.Debit ?? 0);
                                balance2 = totalDebit - totalCredit;
                            }
                            var deference = (Math.Abs((balance1 - balance2)) / ((balance1 + balance2) / 2)) * 100;
                            if (deference > item.Percentage)
                            {
                                if (lstUser != null && lstUser.Any())
                                {
                                    foreach (var u in lstUser)
                                    {
                                        SendMailMultipleAlert(1, u.Email, u.UserName, "Multiple Account/Profile Alert", AccountId, Name, lstAccount[0].Name, lstAccount[1].Name, HelperClass.Converter.Obj2Decimal(deference).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), HelperClass.Converter.Obj2Decimal(item.Percentage).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                }
                            }
                        }
                        else
                        {
                            decimal balance1 = lstAccount[0].LastBalance ?? 0;
                            string profileValue;
                            if (lstAccount[0].LastBalance == null || lstAccount[0].LastBalance == 0)
                            {
                                var acc = lstAccount[0].AccountId;
                                var acc1 = DbContext.transactions.Where(t => t.upload.AccountId == acc).Select(s => new { s.Credit, s.Debit }).ToList();
                                totalCredit = acc1.Sum(s => s.Credit ?? 0);
                                totalDebit = acc1.Sum(s => s.Debit ?? 0);
                                balance1 = totalDebit - totalCredit;
                            }
                            decimal profile1 = 0;
                            var alertProfile = DbContext.alertmultipleprofiles.Where(p => p.MultipleAccountAlertId == item.Id).ToList();
                            var tmp = alertProfile.Any() ? alertProfile[0].Id : 0;
                            profileValue = alertProfile.Any() ? alertProfile[0].Profile : "";
                            var profile = (from alert in DbContext.alertmultipleprofiles.Where(p => p.Id == tmp)
                                           join ta in DbContext.alertmultipleprofiletaskxrefs on alert.Id equals ta.AlertMultipleProfileId
                                           join t in DbContext.tasks on ta.TaskId equals t.Id
                                           join ti in DbContext.taskinstances on t.Id equals ti.TaskId
                                           join tt in DbContext.transactionanalysistasks on ti.id equals tt.TaskInstanceId
                                           join tr in DbContext.transactionanalysisrecords.Where(p => p.ProfileValue == profileValue) on tt.Id equals tr.TransactionAnalysisTaskId
                                           join a in DbContext.transactions on tr.TransactionId equals a.Id
                                           select new { a.Credit, a.Debit }).ToList();
                            var listtask = (from t in DbContext.alertmultipleprofiletaskxrefs.Where(p => p.AlertMultipleProfileId == tmp)
                                            join ta in DbContext.tasks on t.TaskId equals ta.Id
                                            select new { ta.Name }).ToList();
                            var taskname = "";
                            if (listtask != null && listtask.Any())
                            {
                                foreach (var ta in listtask)
                                {
                                    taskname += ta.Name + ",";
                                }
                            }
                            if (!string.IsNullOrEmpty(taskname))
                                taskname = taskname.Substring(0, taskname.Length - 1);
                            if (profile.Any())
                            {
                                totalCredit = profile.Sum(s => s.Credit ?? 0);
                                totalDebit = profile.Sum(s => s.Debit ?? 0);
                                profile1 = totalDebit - totalCredit;
                            }
                            var deference = (Math.Abs((balance1 - profile1)) / ((balance1 + profile1) / 2)) * 100;
                            if (deference > item.Percentage)
                            {
                                if (lstUser.Any())
                                {
                                    foreach (var u in lstUser)
                                    {
                                        SendMailMultipleAlert(2, u.Email, u.UserName, "Multiple Account/Profile Alert", AccountId, taskname, lstAccount[0].Name, profileValue ?? "", HelperClass.Converter.Obj2Decimal(deference).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), HelperClass.Converter.Obj2Decimal(item.Percentage).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        decimal profile1 = 0, profile2 = 0;
                        string profileValue;
                        var profile = DbContext.alertmultipleprofiles.Where(p => p.MultipleAccountAlertId == item.Id).ToList();
                        if (profile.Any())
                        {
                            if (profile.Count > 1)
                            {
                                var tmp = profile[0].Id;
                                profileValue = profile[0].Profile;
                                var lstprofile1 = (from alert in DbContext.alertmultipleprofiles.Where(p => p.Id == tmp)
                                                   join ta in DbContext.alertmultipleprofiletaskxrefs on alert.Id equals ta.AlertMultipleProfileId
                                                   join t in DbContext.tasks on ta.TaskId equals t.Id
                                                   join ti in DbContext.taskinstances on t.Id equals ti.TaskId
                                                   join tt in DbContext.transactionanalysistasks on ti.id equals tt.TaskInstanceId
                                                   join tr in DbContext.transactionanalysisrecords.Where(p => p.ProfileValue == profileValue) on tt.Id equals tr.TransactionAnalysisTaskId
                                                   join a in DbContext.transactions on tr.TransactionId equals a.Id
                                                   select new { a.Credit, a.Debit }).ToList(); ;
                                if (lstprofile1.Any())
                                {
                                    totalCredit = lstprofile1.Sum(s => s.Credit ?? 0);
                                    totalDebit = lstprofile1.Sum(s => s.Debit ?? 0);
                                    profile1 = totalDebit - totalCredit;
                                }
                                tmp = profile[1].Id;
                                profileValue = profile[1].Profile;
                                var lstprofile2 = (from alert in DbContext.alertmultipleprofiles.Where(p => p.Id == tmp)
                                                   join ta in DbContext.alertmultipleprofiletaskxrefs on alert.Id equals ta.AlertMultipleProfileId
                                                   join t in DbContext.tasks on ta.TaskId equals t.Id
                                                   join ti in DbContext.taskinstances on t.Id equals ti.TaskId
                                                   join tt in DbContext.transactionanalysistasks on ti.id equals tt.TaskInstanceId
                                                   join tr in DbContext.transactionanalysisrecords.Where(p => p.ProfileValue == profileValue) on tt.Id equals tr.TransactionAnalysisTaskId
                                                   join a in DbContext.transactions on tr.TransactionId equals a.Id
                                                   select new { a.Credit, a.Debit }).ToList();
                                if (lstprofile2.Any())
                                {
                                    totalCredit = lstprofile2.Sum(s => s.Credit ?? 0);
                                    totalDebit = lstprofile2.Sum(s => s.Debit ?? 0);
                                    profile2 = totalDebit - totalCredit;
                                }
                                var deference = (Math.Abs((profile1 - profile2)) / ((profile1 + profile2) / 2)) * 100;
                                if (deference > item.Percentage)
                                {
                                    if (lstUser.Any())
                                    {
                                        foreach (var u in lstUser)
                                        {
                                            SendMailMultipleAlert(3, u.Email, u.UserName, "Multiple Account/Profile Alert", AccountId, Name, profile[0].Profile, profile[1].Profile, HelperClass.Converter.Obj2Decimal(deference).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), HelperClass.Converter.Obj2Decimal(item.Percentage).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Send Mail
        private void SendMail(int type, string userEmail, string userName, string subject, long accountId, string amountdefferences, string amountAlert)
        {
            try
            {
                string emailBody = "";
                switch (type)
                {
                    case 1:
                        var acc = DbContext.Accounts.FirstOrDefault(p => p.Id == accountId);
                        if (acc != null)
                        {
                            emailBody = GenEmailBodySinggleAccountAlertAmount(1, userName, acc.Name, amountdefferences, amountAlert);
                        }
                        break;
                    case 2:
                        var acc1 = DbContext.Accounts.FirstOrDefault(p => p.Id == accountId);
                        if (acc1 != null)
                        {
                            emailBody = GenEmailBodySinggleAccountAlertAmount(2, userName, acc1.Name, amountdefferences, amountAlert);
                        }
                        break;
                }

                new EmailHelperRules(DbContext).SendEmail(emailBody, subject, userEmail);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
        }
        private void SendMailMultipleAlert(int type, string userEmail, string userName, string subject, long accountId, string taskName, string account1Name, string account2Name, string amountdefferences, string amountAlert)
        {
            try
            {
                string emailBody = "";
                var acc2 = DbContext.Accounts.FirstOrDefault(p => p.Id == accountId);
                if (acc2 != null)
                {
                    emailBody = GenEmailBodyMultipleAccountAlertAmount(type, userName, acc2.Name, taskName, account1Name, account2Name, amountdefferences, amountAlert);
                }

                new EmailHelperRules(DbContext).SendEmail(emailBody, subject, userEmail);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);                
            }
        }

        private string GenEmailBodySinggleAccountAlertAmount(int type, string username, string accountname, string amountdifferences, string amountAlert)
        {
            try
            {
                string body = string.Empty;
                switch (type)
                {
                    case 1:
                        using (StreamReader reader =
                        new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        @"EmailTemplates\_EmailTemplateSinggleAccountAlert.cshtml").Replace(@"\bin\Debug", "").Replace(@"\bin\Release", "")))
                        {
                            body = reader.ReadToEnd();
                        }
                        break;
                    case 2:
                        using (StreamReader reader =
                       new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                       @"EmailTemplates\_EmailTemplateSinggleAccountAlertProfile.cshtml").Replace(@"\bin\Debug", "").Replace(@"\bin\Release", "")))
                        {
                            body = reader.ReadToEnd();
                        }
                        break;
                }
                body = body.Replace("{Name}", username);
                body = body.Replace("{ACCOUNT_NAME}", accountname);
                body = body.Replace("{AMOUNT_DIFFERENCE}", amountdifferences);
                body = body.Replace("{ALERT_AMOUNT_VALUE}", amountAlert);
                body = body.Replace("{CompanyName}", "Cleanbooks Cloud");

                return body;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        private string GenEmailBodyMultipleAccountAlertAmount(int type, string username, string accountname, string taskname, string account1Name, string account2Name, string amountdifferences, string amountAlert)
        {
            try
            {
                string body = string.Empty;
                switch (type)
                {
                    case 1:
                        using (StreamReader reader =
                        new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        @"EmailTemplates\_EmailTemplateMultipleAccount2AccountAlert.cshtml").Replace(@"\bin\Debug", "").Replace(@"\bin\Release", "")))
                        {
                            body = reader.ReadToEnd();
                        }
                        break;
                    case 2:
                        using (StreamReader reader =
                       new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                       @"EmailTemplates\_EmailTemplateMultipleAccount2ProfileAlert.cshtml").Replace(@"\bin\Debug", "").Replace(@"\bin\Release", "")))
                        {
                            body = reader.ReadToEnd();
                        }
                        break;
                    case 3:
                        using (StreamReader reader =
                       new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                       @"EmailTemplates\_EmailTemplateMultipleProfile2ProfileAlert.cshtml").Replace(@"\bin\Debug", "").Replace(@"\bin\Release", "")))
                        {
                            body = reader.ReadToEnd();
                        }
                        break;
                }
                body = body.Replace("{Name}", username);
                body = body.Replace("{ACCOUNT_NAME}", accountname);
                body = body.Replace("{TASK_NAME}", taskname);
                switch (type)
                {
                    case 1:
                        body = body.Replace("{ACCOUNT1_NAME}", account1Name);
                        body = body.Replace("{ACCOUNT2_NAME}", account2Name);
                        break;
                    case 2:
                        body = body.Replace("{ACCOUNT1_NAME}", account1Name);
                        body = body.Replace("{ACCOUNT2_NAME}", account2Name + " in " + taskname);
                        break;
                    case 3:
                        body = body.Replace("{ACCOUNT1_NAME}", account1Name + " in " + taskname);
                        body = body.Replace("{ACCOUNT2_NAME}", account2Name + " in " + taskname);
                        break;
                }

                body = body.Replace("{AMOUNT_DIFFERENCE}", amountdifferences);
                body = body.Replace("{ALERT_AMOUNT_VALUE}", amountAlert);
                body = body.Replace("{CompanyName}", "Cleanbooks Cloud");

                return body;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public List<upload> GetUploadsByAccountId(int accountId)
        {
            bool proxyCreation = DbContext.Configuration.ProxyCreationEnabled;
            try
            {
                DbContext.Configuration.ProxyCreationEnabled = false;
                var uploads = DbContext.uploads.Where(a => a.AccountId == accountId).ToList();

                return uploads;
            }
            catch
            {
                return new List<upload>();
            }
            finally
            {
                DbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }
    }
}
