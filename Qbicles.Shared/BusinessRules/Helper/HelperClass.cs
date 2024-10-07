using CleanBooksData;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Qbicles.BusinessRules.FilesUploadUtility;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Loyalty;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using static Qbicles.BusinessRules.Enums;
using static Qbicles.Models.ApprovalReq;
using static Qbicles.Models.Bookkeeping.CoANode;

namespace Qbicles.BusinessRules
{
    public static class ReportFileName
    {
        public const string SaleOrder = "SaleOrder.rdlc";
        public const string Invoice = "Invoice.rdlc";

        //Report
        public const string OrderManagement = "OrderManagement.rdlc";
        public const string PurchaseOrder = "PurchaseOrder.rdlc";
        public const string TraderListItem = "TraderListItem.rdlc";
        public const string SalesOrder = "SalesOrder.rdlc";
        public const string TraderMovement = "TraderMovement.rdlc";
        public const string MovementItemTrend = "MovementItemTrend.rdlc";
        public const string VirtualSafe = "VirtualSafe.rdlc";
        public const string VirtualTill = "VirtualTill.rdlc";
        public const string CashBank = "CashBank.rdlc";
        public const string ShiftAudit = "ShiftAudit.rdlc";
        public const string SpotCount = "SpotCount.rdlc";
        public const string WasteReport = "WasteReport.rdlc";
        public const string PosPrintCheck = "PosPrintCheck.rdlc";
        public const string PosCancellation = "PosCancellation.rdlc";
        public const string PosPayment = "PosPayment.rdlc";
    }

    public static class StatusLabelStyle
    {
        public const string NotApplicable = "";
        public const string Draft = "label-primary";
        public const string Pending = "label-warning";
        public const string Reviewed = "label-warning";
        public const string Approved = "label-success";
        public const string Discarded = "label-danger";
        public const string Denied = "label-danger";

        public const string Secondary = "secondary";
        public const string Info = "info";
        public const string Primary = "primary";
        public const string Warning = "warning";
        public const string Success = "success";
        public const string Danger = "danger";

        public const string SecondaryColor = "#fff";
        public const string InfoColor = "#00c0ef";
        public const string PrimaryColor = "#3c8dbc";
        public const string WarningColor = "#f39c12";
        public const string SuccessColor = "#00a65a";
        public const string DangerColor = "#dd4b39";
    }

    public static class StatusLabelName
    {
        //Spot count
        public const string CountStarted = "Count Started";

        public const string CountCompleted = "Count Completed";
        public const string StockAdjusted = "Stock Adjusted";
        public const string Denied = "Denied";
        public const string Discarded = "Discarded";
        public const string Draft = "Draft";

        public const string Started = "Started";
        public const string Completed = "Completed";

        public const string Pending = "Awaiting review";
        public const string Reviewed = "Awaiting approval";
        public const string Approved = "Approved";
    }

    public static class TraderProcessName
    {
        public const string TraderPurchaseProcessName = "Purchase";

        public const string TraderSaleProcessName = "Sale";

        public const string TraderSaleReturnProcessName = "Sale Return";

        public const string TraderTransferProcessName = "Transfer";

        public const string TraderPaymentProcessName = "Payment";

        public const string TraderContactProcessName = "Contact";

        public const string TraderInvoiceProcessName = "Invoice";

        public const string TraderSpotCountProcessName = "Spot Count";

        public const string TraderWasteReportProcessName = "Waste Report";

        public const string Manufacturing = "Manufacturing";

        public const string StockAudits = "Stock Audits";
        public const string ShiftAudits = "Shift Audit";

        public const string POS = "Point of Sale";

        public const string CreditNotes = "Credit Notes";

        public const string Budget = "Budget";

        public const string TraderCashManagement = "Cash Management";

        public const string Reorder = "Reorder";
    }

    public static class CBProcessName
    {
        public const string AccountProcessName = "Account";
        public const string AccountDataProcessName = "Account Data";

        public const string TaskProcessName = "Task";
        public const string TaskExecutionProcessName = "Task Execution";
    }

    public static class BookkeepingProcessName
    {
        public const string ViewChartOfAccounts = "View Chart of Accounts";

        public const string Accounts = "Accounts";

        public const string JournalEntry = "Journal Entry";

        public const string ViewJournalEntries = "View Journal Entries";

        public const string Reports = "Reports";
    }

    public static class QbiclesBoltOns
    {
        public const string QbiclesBusiness = "Qbicles Business";
    }

    public static class FixedRoles
    {
        public const string QbiclesBusinessRole = "Business User";
    }

    public static class SystemRoles
    {
        public const string SystemAdministrator = "System Administrator";
        public const string QbiclesBankManager = "Qbicles Bank Manager";
        public const string SocialHighlightsBlogger = "Social Highlights Blogger";
        public const string DomainUser = "Domain User";
    }

    public static class RightPermissions
    {
        public const string ViewContent = "View Content";
        public const string EditContent = "Edit Content";
        public const string AddTaskToQbicle = "Add Task to Qbicle";
        public const string AddDocumentToQbicle = "Add Document to Qbicle";

        public const string BKBookkeepingView = "View Bookkeeping";
        public const string BKManageAppSettings = "Manage App Settings";

        public const string CMAddEditDomainProfile = "Add/Edit Domain Profile";
        public const string CMDeleteDomainProfile = "Delete Domain Profile";
        public const string CMAddEditCommunityPage = "Add/Edit Community Page";
        public const string CMDeleteCommunityPage = "Delete Community Page";
        public const string CMAddEditUserProfilePage = "Add / Edit User Profile page";

        public const string CleanBooksAccess = "View CleanBooks";
        public const string CleanBooksConfig = "App Config";
        public const string CleanBooksAccountsAccess = "View CleanBooks Accounts";
        public const string CleanBooksTaskAccess = "View CleanBooks Tasks";

        public const string SalesAndMarketingAccess = "Access Sales and Marketing";

        public const string TraderAccess = "Access Trader";
        public const string TraderContactUpdate = "Update Trader Contact";

        public const string SpanneredAccess = "Access Spannered";
        public const string OperatorAccess = "Access Operator";
        public const string CommerceAccess = "Access Commerce";

        //public const string B2CAccess = "Access B2C";
        public const string MyBankMateAccess = "Access Bankmate";

        public const string QbiclesBusinessAccess = "Business Access";
    }

    public static class HelperClass
    {
        public const string ImageNotFoundUrl = "/Content/DesignStyle/img/icon_image_not_found.png";
        public const string DomainLogoDefault = "/Content/DesignStyle/img/icon_domain_default.png";
        public const string QbicleLogoDefault = "bae23975-5d33-4028-b208-94353fd00fe4";
        public const string UserLogoDefault = "/Content/DesignStyle/img/icon_training.png";
        public const string SubscriptionAdminLabel = "Subscription administrator";
        public const string TypeUserGuest = "Guest";
        public const string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #region Columns field fix default

        private const string _Date = "Date";
        private const string _Reference = "Reference";
        private const string _Description = "Description";
        private const string _Debit = "Debit";
        private const string _Credit = "Credit";
        private const string _Balance = "Balance";
        private const string _Reference1 = "Reference1";
        private const string _DescCol1 = "DescCol1";
        private const string _DescCol2 = "DescCol2";
        private const string _DescCol3 = "DescCol3";
        private const int uploadNameLength = 6;

        /// <summary>
        /// Length of sequence number
        /// </summary>
        public static int UploadCountLength
        { get { return uploadNameLength; } }

        public static string Date
        { get { return _Date; } }
        public static string Reference
        { get { return _Reference; } }
        public static string Description
        { get { return _Description; } }
        public static string Debit
        { get { return _Debit; } }
        public static string Credit
        { get { return _Credit; } }
        public static string Balance
        { get { return _Balance; } }
        public static string Reference1
        { get { return _Reference1; } }
        public static string DescCol1
        { get { return _DescCol1; } }
        public static string DescCol2
        { get { return _DescCol2; } }
        public static string DescCol3
        { get { return _DescCol3; } }

        #endregion Columns field fix default

        public static int CurrentTaskForm { get; set; }

        public static string[] BKGroupsDefault = new string[]
        {
            "Assets","Liabilities","Expenses","Equity","Revenue"
        };

        public static BKAccountTypeEnum Convert2BkAccountTypeEnum(string type)
        {
            switch (type)
            {
                case "Assets":
                    return BKAccountTypeEnum.Assets;

                case "Liabilities":
                    return BKAccountTypeEnum.Liabilities;

                case "Expenses":
                    return BKAccountTypeEnum.Expenses;

                case "Equity":
                    return BKAccountTypeEnum.Equity;

                case "Revenue":
                    return BKAccountTypeEnum.Revenue;

                default:
                    return BKAccountTypeEnum.Assets;
            }
        }

        /// <summary>
        /// page size load in view
        /// </summary>
        public static int qbiclePageSize = Converter.Obj2Int(WebConfigurationManager.AppSettings["qbiclePageSize"]);

        public static int myDeskPageSize = Converter.Obj2Int(WebConfigurationManager.AppSettings["mydeskPageSize"]);
        public static int activitiesPageSize = Converter.Obj2Int(WebConfigurationManager.AppSettings["activitiesPageSize"]);
        public static int brandPageSize = 8;
        public static int segmentPageSize = 8;
        public static int areaPageSize = 8;
        public static int placePageSize = 8;
        public static int pipelinePageSize = 8;
        public static int ideaPageSize = 8;

        public static string appTypeProcessDocumentation = "Process Documentation";
        public static string appTypeApprovals = "Approvals";
        public static string appTypeCleanBooks = "CleanBooks";
        public static string appTypeCBTasks = "Tasks";
        public static string appTypeCBTransactionMatching = "Manage Transaction Matching";

        //
        public static string appTypeTaskForms = "Task Forms";

        public static string appTypeCommunity = "Community";
        public static string appTypeTopicsProjectManagement = "Topics / Project Management";
        public static string appTypeBookkeeping = "Bookkeeping";
        public static string appTypeTrader = "Trader";
        public static string appTypePeople = "People";
        public static string appTypeSpannered = "Spannered";
        public static string appTypeOperator = "Operator";
        public static string appTypeCommerce = "Commerce";
        public static string appMyBankMate = "MyBankMate";
        public static string appTypeKnowledgeBase = "Knowledge Base";
        public static string appTypePayrollTaxes = "Payroll / Taxes";
        public static string appTypeInsurance = "Insurance";
        public static string GeneralName = "General";
        public static string appSalesMarketing = "Sales & Marketing";

        public class EnumModel
        {
            public int Key { get; set; }

            public string Value { get; set; }

            public static ICollection<EnumModel> ConvertEnumToList<T>() where T : struct, IConvertible
            {
                if (!typeof(T).IsEnum)
                {
                    throw new Exception("Type given T must be an Enum");
                }

                var result = Enum.GetValues(typeof(T))
                                 .Cast<T>()
                                 .Select(x => new EnumModel
                                 {
                                     Key = Convert.ToInt32(x),
                                     Value = x.ToString(new CultureInfo("en"))
                                 })
                                 .ToList()
                                 .AsReadOnly();

                return result;
            }

            public static List<string> GetListOfDescription<T>() where T : struct
            {
                Type t = typeof(T);
                return !t.IsEnum ? null : Enum.GetValues(t).Cast<Enum>().Select(x => x.GetDescription()).ToList();
            }

            public static ICollection<EnumModel> GetEnumValuesAndDescriptions<T>()
            {
                Type enumType = typeof(T);

                if (enumType.BaseType != typeof(Enum))
                    throw new ArgumentException("T is not System.Enum");

                ICollection<EnumModel> enumValList = new List<EnumModel>();

                foreach (var e in Enum.GetValues(typeof(T)))
                {
                    var fi = e.GetType().GetField(e.ToString());
                    var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                    enumValList.Add(new EnumModel
                    {
                        Value = (attributes.Length > 0) ? attributes[0].Description : e.ToString(),
                        Key = (int)e
                    });
                }

                return enumValList;
            }

            public static string GetDescriptionFromEnumValue(Enum value)
            {
                DescriptionAttribute attribute = value.GetType()
                    .GetField(value.ToString())
                    ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    ?.SingleOrDefault() as DescriptionAttribute;
                return attribute == null ? value.ToString() : attribute.Description;
            }
        }

        /// <summary>
        /// Try and parse a value into the given enum
        /// </summary>
        /// <typeparam name="T">Type of the desired enum</typeparam>
        /// <param name="value">The value to parse</param>
        /// <param name="enumValue">The resulting enum, if detected</param>
        /// <returns></returns>

        public static bool TryParseEnum<T>(this object value)
        {
            var intValue = Convert.ToInt32(value);

            if (!Enum.IsDefined(typeof(T), intValue))
                return false;

            return true;
        }

        public static List<AttachmentModel> MapAttachments<T>(IEnumerable<T> attactments)
        {
            List<AttachmentModel> valList = new List<AttachmentModel>();
            var attactment = new AttachmentModel();
            foreach (var item in attactments.ToList())
            {
                if (typeof(T) == typeof(ApprovalDocument))
                {
                    attactment = new AttachmentModel
                    {
                        Id = Guid.NewGuid(),
                        ActivityId = (item as ApprovalDocument).Id,
                        Name = (item as ApprovalDocument).Document,
                        AttachmentUrl = (item as ApprovalDocument).AppDocumentImage,
                        IconFile = (item as ApprovalDocument).FileType.IconPath
                    };
                }
                else if (typeof(T) == typeof(QbicleMedia))
                {
                    attactment = new AttachmentModel
                    {
                        Id = Guid.NewGuid(),
                        ActivityId = (item as QbicleMedia).Id,
                        Name = (item as QbicleMedia).Name,
                        AttachmentUrl = (item as QbicleMedia).VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).First().Uri,//(item as QbicleMedia).Uri,
                        IconFile = (item as QbicleMedia).FileType.IconPath,
                        CanDelete = true
                    };
                }
                valList.Add(attactment);
            }
            return valList;
        }

        public static string GetDescription(this Enum value)
        {
            //if (int.Parse(value) == 0) return "";
            try
            {
                DescriptionAttribute attribute = value.GetType()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault() as DescriptionAttribute;
                return attribute == null ? value.ToString() : attribute.Description;
            }
            catch
            {
                return "";
            }
        }

        public static int GetId(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        //
        /// <summary>
        /// Format a date as in "20th August, 2016."
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DatetimeToOrdinal(this DateTime value)
        {
            return value.Day + value.Day.ToOrdinal() + " " +
                value.ToString("MMMM") +
                ", " + value.Year /*+ " " + value.TimeOfDay*/;
        }

        /// <summary>
        /// Format a date as in "20th August, 2016 20:20"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DatetimeToOrdinalAndTime(this DateTime value)
        {
            return value.Day + value.Day.ToOrdinal() + " " +
                   value.ToString("MMMM") +
                   ", " + value.Year + " " + value.ToString(" h:mmtt");
        }

        /// <summary>
        /// one second ago, minute ago, hour ago ...
        /// </summary>
        /// <param name="yourDate"></param>
        /// <returns>one second ago</returns>
        public static string GetTimeRelative(this DateTime yourDate)
        {
            TimeSpan ts = DateTime.UtcNow - yourDate;
            if (ts.TotalMinutes < 1)//seconds ago
                return "just now";
            if (ts.TotalHours < 1)//min ago
                return (int)ts.TotalMinutes == 1 ? "1 minute ago" : (int)ts.TotalMinutes + " minutes ago";
            if (ts.TotalDays < 1)//hours ago
                return (int)ts.TotalHours == 1 ? "1 hour ago" : (int)ts.TotalHours + " hours ago";
            if (ts.TotalDays < 7)//days ago
                return (int)ts.TotalDays == 1 ? "1 day ago" : (int)ts.TotalDays + " days ago";
            if (ts.TotalDays < 30.4368)//weeks ago
                return (int)(ts.TotalDays / 7) == 1 ? "1 week ago" : (int)(ts.TotalDays / 7) + " weeks ago";
            if (ts.TotalDays < 365.242)//months ago
                return (int)(ts.TotalDays / 30.4368) == 1 ? "1 month ago" : (int)(ts.TotalDays / 30.4368) + " months ago";
            //years ago
            return (int)(ts.TotalDays / 365.242) == 1 ? "1 year ago" : (int)(ts.TotalDays / 365.242) + " years ago";
        }

        public static string ConvertDateFormat(this string strDate, string inputFormat, string outputFormat, string timeZone = "")
        {
            var dateTime = DateTime.ParseExact(strDate, inputFormat, CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(timeZone))
                return dateTime.ConvertTimeFromUtc(timeZone).ToString(outputFormat);
            return dateTime.ToString(outputFormat);
        }

        public static DateTime ConvertDateFormat(this string strDate, string inputFormat)
        {
            return DateTime.ParseExact(strDate, inputFormat, CultureInfo.InvariantCulture);
        }

        public static void ConvertDaterangeFormat(this string strDaterange, string inputFormat, string timeZone, out DateTime startDate, out DateTime endDate, endDateAddedType typeToAdd = endDateAddedType.none)
        {
            startDate = DateTime.UtcNow;
            endDate = DateTime.UtcNow;
            var arrDate = strDaterange.Split('-');
            if (!String.IsNullOrEmpty(timeZone))
            {
                startDate = arrDate[0].Trim().ConvertDateFormat(inputFormat).ConvertTimeToUtc(timeZone);
                endDate = arrDate[1].Trim().ConvertDateFormat(inputFormat).ConvertTimeToUtc(timeZone);
            }
            else
            {
                startDate = arrDate[0].Trim().ConvertDateFormat(inputFormat);
                endDate = arrDate[1].Trim().ConvertDateFormat(inputFormat);
            }
            if (typeToAdd == endDateAddedType.day)
                endDate = endDate.AddDays(1);
            else if (typeToAdd == endDateAddedType.minute)
                endDate = endDate.AddMinutes(1);
        }

        public enum endDateAddedType
        {
            none = 1,
            minute = 2,
            day = 3
        }

        public static List<DateTime> GetListDateByThisWeek()
        {
            DateTime today = DateTime.UtcNow.Date;
            int currentDayOfWeek = (int)today.DayOfWeek;
            DateTime sunday = today.AddDays(-currentDayOfWeek);
            DateTime monday = sunday.AddDays(1);
            // If we started on Sunday, we should actually have gone *back*
            // 6 days instead of forward 1...
            if (currentDayOfWeek == 0)
            {
                monday = monday.AddDays(-7);
            }
            return Enumerable.Range(0, 7).Select(days => monday.AddDays(days)).ToList();
        }

        public static List<DateTime> GetListDateByThisMonth()
        {
            DateTime today = DateTime.UtcNow.Date;
            return Enumerable.Range(1, DateTime.DaysInMonth(today.Year, today.Month))  // Days: 1, 2 ... 31 etc.
                    .Select(day => new DateTime(today.Year, today.Month, day)) // Map each day to a date
                    .ToList(); // Load dates into a list
        }

        public static string ToOrdinal(this int value)
        {
            // Start with the most common extension.
            string extension = "th";

            // Examine the last 2 digits.
            int last_digits = value % 100;

            // If the last digits are 11, 12, or 13, use th. Otherwise:
            if (last_digits < 11 || last_digits > 13)
            {
                // Check the last digit.
                switch (last_digits % 10)
                {
                    case 1:
                        extension = "st";
                        break;

                    case 2:
                        extension = "nd";
                        break;

                    case 3:
                        extension = "rd";
                        break;
                }
            }

            return extension;
        }

        public static string ToOrdinalString(this DateTime date, string dateFormat)
        {
            if (dateFormat.Equals("dd/mm/yyyy", StringComparison.OrdinalIgnoreCase))
            {
                //return string.Format("{0}{1} {2:MMM yyyy}", date.Day, date.Day.ToOrdinal(), date);
                return $"{date.Day}{date.Day.ToOrdinal()} {date:MMM} {date:yyyy}";
            }

            return $"{date:MMM} {date.Day}{date.Day.ToOrdinal()} {date:yyyy}";
        }

        public static bool ContainsAll<T>(this HashSet<T> containingList, HashSet<T> lookupList)
        {
            bool contain;
            contain = lookupList.IsProperSubsetOf(containingList);
            if (!contain)
                if (lookupList.Except(containingList).Count() == 0)
                    contain = true;
            return contain;
        }

        public static List<uploadfieldsData> uploadFielsData()
        {
            return new List<uploadfieldsData> {
                new uploadfieldsData(){Value = "Date"},
                new uploadfieldsData(){Value = "Reference"},
                new uploadfieldsData(){Value = "Description"},
                new uploadfieldsData(){Value = "Debit"},
                new uploadfieldsData(){Value = "Credit"},
                new uploadfieldsData(){Value = "Balance"},
                new uploadfieldsData(){Value = "Reference1"},
                new uploadfieldsData(){Value = "DescCol1"},
                new uploadfieldsData(){Value = "DescCol2"},
                new uploadfieldsData(){Value = "DescCol3"}
                };
        }

        public static byte[] Stream2Bytes(Stream stream)
        {
            var length = stream.Length;
            var br = new BinaryReader(stream);
            byte[] bytes = br.ReadBytes((int)length);
            return bytes;
        }

        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        public static Image BytesToImage(byte[] imageBytes)
        {
            // Convert byte[] to Image
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        public static string GetBase64StringFromStream(Stream objectStream)
        {
            try
            {
                var br = new BinaryReader(objectStream);
                Byte[] bytes = br.ReadBytes((Int32)objectStream.Length);
                string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                return base64String;
            }
            catch
            {
                return "";
            }
        }

        public static string GetBase64StringFromLocalImage(string imgPath)
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(imgPath);
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
            catch
            {
                return "";
            }
        }

        public static byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        public static byte[] FileToByteArray(string file)
        {
            return File.ReadAllBytes(file);
        }

        public static string GetFileName(string file)
        {
            return Path.GetFileName(file);
        }

        public static string GetFileExtension(string file)
        {
            return Path.GetExtension(file).Replace(".", "").ToLower();
        }

        /// <summary>
        /// Convert list to datatable
        /// </summary>
        /// <typeparam name="T">list object</typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ConvertList2Datatable<T>(List<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    table.Columns.Add(prop.Name, prop.PropertyType.GetGenericArguments()[0]);
                else
                    table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        /// <summary>
        /// Columns transaction header defaul
        /// </summary>
        /// <returns></returns>
        public static List<transactionStructure> TransactionColName()
        {
            List<FilesUploadUtility.transactionStructure> trans = new List<FilesUploadUtility.transactionStructure>();
            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = "Ignorecolumn",
                colValue = "Ignore column"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = Date,
                colValue = "Date"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = Reference,
                colValue = "Reference"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = Description,
                colValue = "Description"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = Debit,
                colValue = "Debit"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = Credit,
                colValue = "Credit"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = Balance,
                colValue = "Balance"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = Reference1,
                colValue = "Reference 1"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = DescCol1,
                colValue = "Description 1"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = DescCol2,
                colValue = "Description 2"
            });

            trans.Add(new FilesUploadUtility.transactionStructure()
            {
                colKey = DescCol3,
                colValue = "Description 3"
            });

            return trans;
        }

        #region convert datatable to list

        /// <summary>
        /// Converter datatable to list object
        /// </summary>
        /// <typeparam name="T">type of list</typeparam>
        /// <param name="datatable">datatable</param>
        /// <returns>List object</returns>
        public static List<T> ConvertDataTable2List<T>(DataTable dt)
        {
            try
            {
                List<T> data = new List<T>();
                foreach (DataRow row in dt.Rows)
                {
                    T item = GetItem<T>(row);
                    data.Add(item);
                }
                return data;
            }
            catch
            {
                return null;
            }
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        /// <summary>
        /// Convert datatable to list DataRow
        /// </summary>
        /// <param name="dataTable">dataTable</param>
        /// <returns>List DataRow</returns>
        public static List<DataRow> ConvertDataTable2ListDataRow(DataTable dataTable)
        {
            List<DataRow> list = new List<DataRow>();
            foreach (DataRow dr in dataTable.Rows)
            {
                list.Add(dr);
            }
            return list;
        }

        /// <summary>
        /// Convert datatable to list string
        /// </summary>
        /// <param name="dataTable">dataTable</param>
        /// <returns>List string</returns>
        public static List<string> ConvertDataTable2ListString(DataTable dataTable)
        {
            List<string> list = dataTable.Rows.OfType<DataRow>().Select(dr => dr.Field<string>(0)).ToList();
            return list;
        }

        /// <summary>
        /// from the notifi object on the client expect to work out if the notification is
        /// From aC2C Qbicle B2C Qbicle or 'normal' Qbicle
        /// </summary>
        /// <param name="qbicleObj"></param>
        /// <returns></returns>
        public static QbicleType GetCreatorTheQbcile(this object qbicleObj)
        {
            if (qbicleObj is B2CQbicle)
                return QbicleType.B2CQbicle;
            else if (qbicleObj is C2CQbicle)
                return QbicleType.C2CQbicle;
            return QbicleType.Qbicle;
        }

        #endregion convert datatable to list

        #region [File type helper]

        public static class FileType
        {
            private const string _excelName = "Excel";
            private const string _excelExtension03 = ".xls";
            private const string _excelExtension07 = ".xlsx";
            private const string _csvlName = "CSV";
            private const string _csvExtension = ".CSV";

            /// <summary>
            /// file name "Excel"
            /// </summary>
            public static string excelName
            { get { return _excelName; } }

            /// <summary>
            /// excel file of version 2003  ".xls"
            /// </summary>
            public static string excelExtension03
            { get { return _excelExtension03; } }

            /// <summary>
            /// excel file of version 2007 and above ".xlsx"
            /// </summary>
            public static string excelExtension07
            { get { return _excelExtension07; } }

            /// <summary>
            /// file name "CSV"
            /// </summary>
            public static string csvlName
            { get { return _csvlName; } }

            /// <summary>
            /// file extension ".csv"
            /// </summary>
            public static string csvExtension
            { get { return _csvExtension; } }
        }

        #endregion [File type helper]

        public static class Converter
        {
            private static CultureInfo provider = CultureInfo.InvariantCulture;

            public static string OrdinalSuffix(int number)
            {
                var suffix = "";

                if (number / 10 % 10 == 1)
                {
                    suffix = "th";
                }
                else if (number > 0)
                {
                    switch (number % 10)
                    {
                        case 1:
                            suffix = "st";
                            break;

                        case 2:
                            suffix = "nd";
                            break;

                        case 3:
                            suffix = "rd";
                            break;

                        default:
                            suffix = "th";
                            break;
                    }
                }
                return suffix;
            }

            /// <summary>
            /// convert object to Int
            /// </summary>
            /// <param name="objInput">object input</param>
            /// <returns>decimal default 0</returns>
            public static int Obj2Int(object objInput)
            {
                if (objInput == null) return 0;
                int retVal;
                if (int.TryParse(objInput.ToString(), NumberStyles.Integer, provider, out retVal))
                    return retVal;
                else
                    return 0;
            }

            /// <summary>
            /// convert object to Int?
            /// </summary>
            /// <param name="objInput">object input</param>
            /// <returns>decimal or null</returns>
            public static int? Obj2IntNull(object objInput)
            {
                if (objInput == null || objInput.ToString() == string.Empty) return null;
                int retVal;
                if (int.TryParse(objInput.ToString(), NumberStyles.Integer, provider, out retVal))
                    return retVal;
                return null;
            }

            /// <summary>
            /// convert object to decimal
            /// </summary>
            /// <param name="objInput">object input</param>
            /// <returns>decimal default 0</returns>
            public static decimal Obj2Decimal(object objInput)
            {
                if (objInput == null) return 0;
                decimal retVal;
                if (decimal.TryParse(objInput.ToString(), out retVal))
                    return retVal;
                else
                    return 0;
            }

            /// <summary>
            /// convert object to decimal?
            /// </summary>
            /// <param name="objInput">object input</param>
            /// <returns>decimal or null</returns>
            public static decimal? Obj2DecimalNull(object objInput)
            {
                if (objInput == null || objInput.ToString() == string.Empty) return null;
                decimal retVal;
                if (decimal.TryParse(objInput.ToString(), out retVal))
                    return retVal;
                return null;
            }

            public static DateTime Obj2DateTime(object strDate)
            {
                if (strDate == null || strDate.ToString() == string.Empty)
                    return DateTime.MinValue;
                try
                {
                    return (DateTime)strDate;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }

            /// <summary>
            /// convert object to DateTime
            /// </summary>
            /// <param name="strDate">object convert to date</param>
            /// <param name="dateformattype">type of date format</param>
            /// <returns>datetime</returns>
            public static DateTime Obj2Date(object strDate, string dateformattype)
            {
                if (strDate == null || strDate.ToString() == string.Empty)
                    return DateTime.MinValue;
                DateTime retVal = DateTime.Today;
                if (DateTime.TryParseExact(strDate.ToString(), dateformattype, provider, DateTimeStyles.None, out retVal))
                    return retVal;
                return DateTime.MinValue;
            }

            /// <summary>
            /// Converter string to DateTime or null
            /// </summary>
            /// <param name="strDate">object convert to date</param>
            /// <returns>datetime or null</returns>
            public static DateTime? Obj2DateNull(object strDate, string dateformattype)
            {
                if (strDate == null || strDate.ToString() == string.Empty)
                    return null;
                DateTime retVal = DateTime.Today;
                if (DateTime.TryParseExact(strDate.ToString(), dateformattype, provider, DateTimeStyles.None, out retVal))
                    return retVal;
                return null;
            }
        }

        public static bool? CheckFileUpload(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string ext = Path.GetExtension(fileName).Replace(".", "").ToLower();
                var filetype = new FileTypeRules(new ApplicationDbContext()).GetExtension();
                if (filetype.FirstOrDefault(x => x == ext) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        public static string FileSize(int size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static string FileSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static AdministratorViewModal ConvertUserToAdminViewModal(ApplicationUser user, string level)
        {
            return new AdministratorViewModal()
            {
                Id = user.Id,
                Name = user.UserName,
                Avatar = user.ProfilePic,
                Level = level
            };
        }

        public static List<AdministratorViewModal> ConvertListUserToListAdminViewModel(QbicleDomain domain, string level, string currentUserId = "")
        {
            if (currentUserId == "")
                return domain.Administrators.Select(x => new AdministratorViewModal
                {
                    Id = x.Id,
                    Name = x.UserName,
                    Avatar = x.ProfilePic,
                    Domain = domain.Name,
                    Level = level
                }).ToList();
            return domain.Administrators.Where(u => u.Id == currentUserId).Select(x => new AdministratorViewModal
            {
                Id = x.Id,
                Name = x.UserName,
                Avatar = x.ProfilePic,
                Domain = domain.Name,
                Level = level
            }).ToList();
        }

        public static List<AdministratorViewModal> ConvertListUserToListAdminViewModel(SubscriptionAccount account, string level, string currentUserId = "")
        {
            if (currentUserId == "")
                return account.Administrators.Select(x => new AdministratorViewModal
                {
                    Id = x.Id,
                    Name = x.UserName,
                    Avatar = x.ProfilePic,
                    Level = level
                }).ToList();
            return account.Administrators.Where(u => u.Id == currentUserId).Select(x => new AdministratorViewModal
            {
                Id = x.Id,
                Name = x.UserName,
                Avatar = x.ProfilePic,
                Level = level
            }).ToList();
        }

        private static string GetOctagon(string value)
        {
            var regex = new System.Text.RegularExpressions.Regex("(?<=[\\.])[0-9]+");
            return regex.Match(value).Value;
        }

        /// <summary>
        /// generate to html table first when not selected Date column
        /// </summary>
        /// <param name="datatabe"></param>
        /// <param name="colHeader"></param>
        /// <returns></returns>
        public static string ConvertDataTableToHTMLTableFirst(DataTable datatabe, List<string> colHeader)
        {
            List<transactionStructure> lstColInDb = TransactionColName();
            string ret = "";
            // first row select option column Transaction
            ret = "<tr id='columns-row' class='columns-row' style='background-color: #ddd;'>";
            //foreach column, add dropdown value columns of Transaction to columns table html
            // index of columns
            int colIndex = 1;
            int colDatatable = 0;
            foreach (DataColumn col in datatabe.Columns)
            {
                ret += "<td style = 'width: 150px;'>";
                ret += "<select class='columnSelected" + "' name = '" + "Column_" + colIndex.ToString()
                    + "' style = 'width: 100 %;' id='" + "Column_" + colIndex.ToString() + "'"
                    + "onchange=" + "\"configureColumns('" + colIndex + "')\"" + "> ";
                // foreach add columns Transaction table to select option
                foreach (FilesUploadUtility.transactionStructure item in lstColInDb)
                {
                    if (colHeader[colDatatable].ToString() == item.colKey + "_" + colIndex.ToString())
                    {
                        if (hardColumn.headColRequire.Contains(item.colKey))
                            ret += "<option  data-mytxt='*' selected value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                            item.colValue + "</option >";
                        else
                            ret += "<option data-mytxt='' selected value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                            item.colValue + "</option >";
                    }
                    else
                    {
                        if (hardColumn.headColRequire.Contains(item.colKey))
                            ret += "<option  data-mytxt='*' value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                           item.colValue + "</option >";
                        else
                            ret += "<option data-mytxt='' value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                           item.colValue + "</option >";
                    }
                }

                ret += "</select>";
                ret += "</td>";
                colIndex++;
                colDatatable++;
            }
            ret += "</tr>";

            foreach (DataRow row in datatabe.Rows)
            {
                ret += "<tr>";
                for (int i = 0; i < datatabe.Columns.Count; i++)
                {
                    string colName = datatabe.Columns[i].ColumnName;
                    if (colName == "Debit" || colName == "Credit" || colName == "Balance")
                    {
                        if (row[i].ToString() == "")
                            ret += "<td> </td>";
                        else
                        {
                            if (GetOctagon(row[i].ToString()).Length > 2)
                                ret += "<td>" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                            else
                                ret += "<td>" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                        }
                    }
                    else
                        ret += "<td>" + row[i].ToString() + "</td>";
                }
                ret += "</tr>";
            }
            return ret;
        }

        /// <summary>
        /// Convert data transaction from upload file to html table and list columns error validation
        /// 1. add row select option columns
        /// 2. add row date select option
        /// 3. bind data transaction
        /// 3.1 Assign columns name to table upload
        /// 3.2 Check account lastbalance with row first column balance upload
        /// 3.3 Check duplicate rows
        /// 3.4 Bind data to table and vaidation
        /// </summary>
        /// <param name="dataTabeAnalyse">data transaction from upload file</param>
        /// <param name="colHeader">list columns header selected</param>
        /// <param name="listDateFormat">list format date get from upload file</param>
        /// <param name="lastBalance">Last balance of Account selected</param>
        /// <param name="reCalBalance">true- re-calculate balance, false - don not re-calculate balance</param>
        /// <returns>
        /// hrml body table transaction from upload file
        /// list value is error validation
        /// warning balance if balance column selected and not compare with last balance of account
        /// status add new balance column if selected debit,credit column but not selected balance column.
        /// create new Balance columns to recalculate Balance
        /// </returns>
        public static List<string> ConvertDataTableToHTMLTableAnalyse(DataTable dataTabeAnalyse, List<string> colHeader,
            List<string> listDateFormat, decimal lastBalance, int accountId, string accountName, string dateFormatSelected, bool reCalBalance)
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var transactionRules = new TransactionsRules(dbContext);
            List<string> result = new List<string>();
            List<FilesUploadUtility.transactionStructure> lstColInDb = TransactionColName();
            string retTable = "";
            List<string> valiError = new List<string>();
            string valiDupliacte = "";
            // selected columns
            bool isBalanceSelected = false, isDebitSelected = false, isCreditSelected = false, isDescriptionSelected = false;
            if (listDateFormat.Count > 0 && dateFormatSelected == null)
                dateFormatSelected = listDateFormat[listDateFormat.Count - 1];

            //1. first row on table html, get column on Transaction table to set value
            retTable = "<tr id='columns-row' class='columns-row' style='background-color: #ddd;'>";
            //foreach column, add dropdown value columns of Transaction to columns table html
            // index of columns
            int colIndex = 1;
            int colDatatable = 0;
            foreach (DataColumn col in dataTabeAnalyse.Columns)
            {
                retTable += "<td style = 'width: 150px;'>";
                retTable += "<select class='columnSelected" + "' name = '" + "Column_" + colIndex.ToString()
                    + "' style = 'width: 100 %;' id='" + "Column_" + colIndex.ToString() + "'"
                    + "onchange=" + "\"configureColumns('" + colIndex + "')\"" + "> ";
                // foreach add columns Transaction to select option
                foreach (FilesUploadUtility.transactionStructure item in lstColInDb)
                {
                    if (colHeader[colDatatable].ToString() == item.colKey + "_" + colIndex.ToString())
                    {
                        if (hardColumn.headColRequire.Contains(item.colKey))
                            retTable += "<option data-mytxt='*' selected value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                                item.colValue + "</option >";
                        else
                            retTable += "<option data-mytxt='' selected value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                            item.colValue + "</option >";
                    }
                    else
                    {
                        if (hardColumn.headColRequire.Contains(item.colKey))
                            retTable += "<option  data-mytxt='*' value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                                item.colValue + "</option >";
                        else
                            retTable += "<option data-mytxt='' value ='" + item.colKey + "_" + colIndex.ToString() + "'>" +
                                item.colValue + "</option >";
                    }
                }

                retTable += "</select>";
                retTable += "</td>";
                colIndex++;
                colDatatable++;
            }
            retTable += "</tr>";
            //2. 2nd row date of table html

            retTable += "<tr id ='date_options' style = 'background-color: #eee;' " + "onchange=" + "\"dateFormatSlected()\"" + "> ";

            // get column as Date
            int colDateIndex = 0;
            foreach (var item in colHeader)
            {
                string[] cols = item.Split('_');
                if (cols[0].ToString() == "Date")
                {
                    colDateIndex = HelperClass.Converter.Obj2Int(cols[1]) - 1;
                    break;
                }
            }
            int columnIndex = 0;
            foreach (DataColumn colum in dataTabeAnalyse.Columns)
            {
                if (columnIndex == colDateIndex)
                {
                    int dateFormatIndex = 1;
                    retTable += "<td><select class='columnDateSelected' name = 'date_options' id = 'date_optionsId' style = 'width: 100%;'>";
                    foreach (var item in listDateFormat)
                    {
                        if (item == dateFormatSelected)
                            retTable += "<option selected value ='" + item + "'>" + item + "</option >";
                        else
                        {
                            if (dateFormatIndex == listDateFormat.Count && string.IsNullOrEmpty(dateFormatSelected))
                                retTable += "<option selected value ='" + item + "'>" + item + "</option >";
                            else
                                retTable += "<option value ='" + item + "'>" + item + "</option >";
                        }
                        dateFormatIndex++;
                    }
                    retTable += "</select>";
                    retTable += "</td>";
                }
                else
                {
                    retTable += "<td>  </td>";
                }
                columnIndex++;
            }

            retTable += "</tr>";

            // validation if exist a value date is null or empty then breack
            bool dateValueIsNull = false;
            foreach (DataRow row in dataTabeAnalyse.Rows)
            {
                if (row.IsNull(colDateIndex))
                {
                    dateValueIsNull = true;
                    break;
                }
            }
            //if date value have null breck all
            if (dateValueIsNull)
            {
                foreach (DataRow row in dataTabeAnalyse.Rows)
                {
                    string styleClass = "";
                    /* 3.1. [Assign columns name to table upload] */

                    for (int j = 0; j < dataTabeAnalyse.Columns.Count; j++)
                    {
                        if (j < colHeader.Count)
                        {
                            if (!colHeader[j].Split('_')[0].Contains("Ignore"))
                            {
                                dataTabeAnalyse.Columns[j].ColumnName = colHeader[j].Split('_')[0];// columns name ( date,balance,credit...)
                                switch (colHeader[j].Split('_')[0])
                                {
                                    case "Balance":
                                        isBalanceSelected = true;
                                        break;

                                    case "Debit":
                                        isDebitSelected = true;
                                        break;

                                    case "Credit":
                                        isCreditSelected = true;
                                        break;

                                    case "Description":
                                        isDescriptionSelected = true;
                                        break;
                                }
                            }
                            else
                                dataTabeAnalyse.Columns[j].ColumnName = colHeader[j];// column name Ignore
                            dataTabeAnalyse.AcceptChanges();
                        }
                    }
                    // begin generate table
                    retTable += "<tr>";
                    for (int i = 0; i < dataTabeAnalyse.Columns.Count; i++)
                    {
                        string colName = dataTabeAnalyse.Columns[i].ColumnName;
                        switch (colName)
                        {
                            case "Date":
                                if (row[i].ToString() == "")
                                {
                                    retTable += "<td class='error'> </td>";
                                    valiError.Add("Selected <strong><i>Date</i></strong> column does not have a date format! </br>");
                                }
                                else
                                {
                                    bool dateVal = false;
                                    foreach (var dateFormat in listDateFormat)
                                    {
                                        if (Converter.Obj2Date(row[i], dateFormat) != null)
                                            dateVal = true;
                                        else
                                            dateVal = false;
                                    }
                                    if (dateVal)
                                        retTable += "<td" + styleClass + ">" + row[i].ToString() + "</td>";
                                    else
                                    {
                                        retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                        valiError.Add("Selected <strong><i>Date</i></strong> column does not have a date format! </br>");
                                    }
                                }
                                break;

                            case "Debit":
                                if (row[i].ToString() == "")
                                    retTable += "<td" + styleClass + "> </td>";
                                else
                                {
                                    if (Converter.Obj2DecimalNull(row[i]) != null)
                                    {
                                        if (GetOctagon(row[i].ToString()).Length > 2)
                                            retTable += "<td>" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                        else
                                            retTable += "<td" + styleClass + ">" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                    }
                                    else
                                    {
                                        retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                        valiError.Add("Selected <strong><i>Debit</i></strong> column does not have a numeric format! </br>");
                                    }
                                }
                                break;

                            case "Credit":
                                if (row[i].ToString() == "")
                                    retTable += "<td " + styleClass + "> </td>";
                                else
                                {
                                    if (Converter.Obj2DecimalNull(row[i]) != null)
                                    {
                                        if (GetOctagon(row[i].ToString()).Length > 2)
                                            retTable += "<td>" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                        else
                                            retTable += "<td" + styleClass + ">" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                    }
                                    else
                                    {
                                        retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                        valiError.Add("Selected <strong><i>Credit</i></strong> column does not have a numeric format! </br>");
                                    }
                                }
                                break;

                            case "Balance":
                                if (row[i].ToString() == "")
                                    retTable += "<td " + styleClass + "> </td>";
                                else
                                {
                                    if (Converter.Obj2DecimalNull(row[i]) != null)
                                    {
                                        if (GetOctagon(row[i].ToString()).Length > 2)
                                            retTable += "<td>" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                        else
                                            retTable += "<td" + styleClass + ">" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                    }
                                    else
                                    {
                                        retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                        valiError.Add("Selected <strong><i>Balance</i></strong> column does not have a numeric format! </br>");
                                    }
                                }
                                break;

                            default:
                                retTable += "<td" + styleClass + ">" + row[i].ToString() + "</td>";
                                break;
                        }
                    }
                    retTable += "</tr>";
                }
                valiError = valiError.Distinct().ToList();

                result.Add(string.Join("", valiError.ToArray()));
                result.Add(retTable);
                result.Add("");
                result.Add("");
                return result;
            }

            //3. list data transaction
            /* 3.1. [Assign columns name to table upload] */

            for (int j = 0; j < dataTabeAnalyse.Columns.Count; j++)
            {
                if (j < colHeader.Count)
                {
                    if (!colHeader[j].Split('_')[0].Contains("Ignore"))
                    {
                        dataTabeAnalyse.Columns[j].ColumnName = colHeader[j].Split('_')[0];// columns name ( date,balance,credit...)
                        switch (colHeader[j].Split('_')[0])
                        {
                            case "Balance":
                                isBalanceSelected = true;
                                break;

                            case "Debit":
                                isDebitSelected = true;
                                break;

                            case "Credit":
                                isCreditSelected = true;
                                break;

                            case "Description":
                                isDescriptionSelected = true;
                                break;
                        }
                    }
                    else
                        dataTabeAnalyse.Columns[j].ColumnName = colHeader[j];// column name Ignore
                    dataTabeAnalyse.AcceptChanges();
                }
            }
            /* 3.2 [Check account lastbalance with row first column balance upload] */
            //first row in column balance
            //decimal balanceUpload = 0;
            string warningBalance = "";
            string addBalanceCol = "", addDebitCol = "", addCreditCol = "";
            bool reCalculate = false;

            // if selected Balance columns:
            //3.2.1 - check with balance upload
            //3.3.2 - check if selected Debit,Credit column (1):
            //3.3.2.1: if selected true (1) and a value on Balance column is null or empty then AutoCalculateBalance
            //3.3.2.2: if selected true (1) and all value on Balance column is not null or empty then don't AutoCalculateBalance

            //

            if (isBalanceSelected && reCalBalance)// if re-cal balance required and selected balance column, check if exist null of balance column=> re-cal
            {
                foreach (DataRow row in dataTabeAnalyse.Rows)
                {
                    // validation if a value of Balance column upload, then calculate balance again
                    if (string.IsNullOrEmpty(row["Balance"].ToString()))
                    {
                        if (isBalanceSelected && (isDebitSelected || isCreditSelected))
                        {
                            reCalculate = true;
                            break;
                        }
                    }
                }
                if (reCalculate)
                    dataTabeAnalyse = uploadfile.AutoCalculateBalance(
                            lastBalance, dataTabeAnalyse, isBalanceSelected, isDebitSelected, isCreditSelected, true).Copy();
                else
                    dataTabeAnalyse = uploadfile.AutoCalculateBalance(
                            lastBalance, dataTabeAnalyse, isBalanceSelected, isDebitSelected, isCreditSelected, false).Copy();
            }
            else if (!isBalanceSelected && reCalBalance)
                if (isDebitSelected || isCreditSelected)
                    dataTabeAnalyse = uploadfile.AutoCalculateBalance(
                                lastBalance, dataTabeAnalyse, isBalanceSelected, isDebitSelected, isCreditSelected, true).Copy();

            // if not selected Balance column, check selected Debit, Credit - if selected then AutoCalculateBalance
            if (isDebitSelected || isCreditSelected)
            {
                if (!isBalanceSelected)
                    addBalanceCol = "addNewBalance";
                else
                    addBalanceCol = "removeBalance";

                if (!isDebitSelected)
                    addDebitCol = "addNewDebit";
                else
                    addDebitCol = "RemoveDebit";

                if (!isCreditSelected)
                    addCreditCol = "addNewCredit";
                else if (isCreditSelected)
                    addCreditCol = "removeCredit";
            }

            /*3.3 [Check duplicate rows] */
            // Duplicate row of upload file: if true - checking on upload file, else checking on database

            DataTable dt = new DataTable();
            List<transaction> listDuplicates = new List<transaction>();
            bool isDuplicate = false;

            /*3.4. [Bind data to table and vaidation]*/
            List<transactionCompare> transactionsDatabase = new List<transactionCompare>();
            List<transactionCompare> transactionsUpload = new List<transactionCompare>();
            List<transactionCompare> transactionsDuplicate = new List<transactionCompare>();
            if ((isDebitSelected || isCreditSelected) && isDescriptionSelected)
            {
                List<transaction> transDB = transactionRules.GetTransactionObject(accountId);
                if (transDB.Count > 0)
                {
                    // bind data in db
                    if (!isBalanceSelected && (isDebitSelected || isCreditSelected) && isDescriptionSelected)
                    {
                        foreach (transaction item in transDB)
                        {
                            transactionCompare tran = new transactionCompare()
                            {
                                Date = item.Date.ToShortDateString(),
                                Description = item.Description == null ? "" : item.Description.ToString(),
                                Debit = Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat),
                                Credit = Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                            };
                            transactionsDatabase.Add(tran);
                        }
                    }
                    else if (isBalanceSelected && (isDebitSelected || isCreditSelected) && isDescriptionSelected)
                    {
                        foreach (transaction item in transDB)
                        {
                            transactionCompare tran = new transactionCompare()
                            {
                                Date = item.Date.ToShortDateString(),
                                Description = item.Description == null ? "" : item.Description.ToString(),
                                Debit = Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat),
                                Credit = Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat),                               //,
                                Balance = Converter.Obj2Decimal(item.Balance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                            };
                            transactionsDatabase.Add(tran);
                        }
                    }
                    // bind data upload
                    if (!isBalanceSelected && (isDebitSelected || isCreditSelected) && isDescriptionSelected)
                    {
                        foreach (DataRow row in dataTabeAnalyse.Rows)
                        {
                            transactionCompare tran = new transactionCompare();
                            tran.Date = Converter.Obj2Date(row["Date"], dateFormatSelected).ToShortDateString();
                            tran.Description = row["Description"] == null ? "" : row["Description"].ToString();
                            if (colHeader.Contains("Debit"))
                            {
                                if (GetOctagon(row["Debit"].ToString()).Length > 2)
                                    tran.Debit = (Math.Round(Converter.Obj2Decimal(row["Debit"]), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                                else
                                    tran.Debit = Converter.Obj2Decimal(row["Debit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                            }
                            if (colHeader.Contains("Credit"))
                            {
                                if (GetOctagon(row["Credit"].ToString()).Length > 2)
                                    tran.Debit = (Math.Round(Converter.Obj2Decimal(row["Credit"]), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                                else
                                    tran.Credit = Converter.Obj2Decimal(row["Credit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                            }
                            transactionsUpload.Add(tran);
                        }
                    }
                    else if (isBalanceSelected && (isDebitSelected || isCreditSelected) && isDescriptionSelected)
                    {
                        foreach (DataRow row in dataTabeAnalyse.Rows)
                        {
                            transactionCompare tran = new transactionCompare();
                            tran.Date = Converter.Obj2Date(row["Date"], dateFormatSelected).ToShortDateString();
                            tran.Description = row["Description"] == null ? "" : row["Description"].ToString();
                            if (colHeader.Contains("Debit"))
                            {
                                if (GetOctagon(row["Debit"].ToString()).Length > 2)
                                    tran.Debit = (Math.Round(Converter.Obj2Decimal(row["Debit"]), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                                else
                                    tran.Debit = Converter.Obj2Decimal(row["Debit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                            }
                            if (colHeader.Contains("Credit"))
                            {
                                if (GetOctagon(row["Credit"].ToString()).Length > 2)
                                    tran.Debit = (Math.Round(Converter.Obj2Decimal(row["Credit"]), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                                else
                                    tran.Credit = Converter.Obj2Decimal(row["Credit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                            }

                            if (colHeader.Contains("Balance"))
                            {
                                if (GetOctagon(row["Balance"].ToString()).Length > 2)
                                    tran.Debit = (Math.Round(Converter.Obj2Decimal(row["Balance"]), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                                else
                                    tran.Balance = Converter.Obj2Decimal(row["Balance"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                            }

                            transactionsUpload.Add(tran);
                        }
                    }
                    // checking duplicate with db
                    if (transactionsUpload.Count > 0 && transactionsDatabase.Count > 0)
                    {
                        switch (isBalanceSelected)
                        {
                            case true:
                                if (transactionsUpload.Count > transactionsDatabase.Count)
                                {
                                    transactionsDatabase.RemoveAll(x => !transactionsUpload.Exists(y =>
                                        y.Date == x.Date
                                        && y.Description == x.Description
                                        && y.Debit == x.Debit
                                        && y.Credit == x.Credit
                                        && y.Balance == x.Balance
                                        ));
                                    transactionsDuplicate = transactionsDatabase;
                                }
                                else
                                {
                                    transactionsUpload.RemoveAll(x => !transactionsDatabase.Exists(y =>
                                       y.Date == x.Date
                                       && y.Description == x.Description
                                       && y.Debit == x.Debit
                                       && y.Credit == x.Credit
                                       && y.Balance == x.Balance
                                       ));
                                    transactionsDuplicate = transactionsUpload;
                                }
                                break;

                            case false:
                                if (transactionsUpload.Count > transactionsDatabase.Count)
                                {
                                    transactionsDatabase.RemoveAll(x => !transactionsUpload.Exists(y =>
                                        y.Date == x.Date
                                        && y.Description == x.Description
                                        && y.Debit == x.Debit
                                        && y.Credit == x.Credit
                                        ));
                                    transactionsDuplicate = transactionsDatabase;
                                }
                                else
                                {
                                    transactionsUpload.RemoveAll(x => !transactionsDatabase.Exists(y =>
                                       y.Date == x.Date
                                       && y.Description == x.Description
                                       && y.Debit == x.Debit
                                       && y.Credit == x.Credit
                                       ));
                                    transactionsDuplicate = transactionsUpload;
                                }
                                break;
                        }
                        if (transactionsDuplicate.Count > 0)
                        {
                            isDuplicate = true;
                            System.Data.DataColumn newColumn = new System.Data.DataColumn("Duplicate", typeof(System.Boolean));
                            newColumn.DefaultValue = false;
                            dataTabeAnalyse.Columns.Add(newColumn);
                            valiDupliacte = "Duplicate records were found already uploaded to the account.</br>These records have been highlighted in <strong>RED</strong> and will not be uploaded.</br>";
                        }
                    }
                }
            }
            if (!isDuplicate)
            {  //validation openingbalance with lastbalance
                if ((isDebitSelected || isCreditSelected) && isDescriptionSelected)
                {
                    decimal openningBalance = 0;// = first balance - first Debit + first Credit
                    if (isBalanceSelected)
                        openningBalance = Converter.Obj2Decimal(dataTabeAnalyse.Rows[0]["Balance"]);
                    else
                        openningBalance = 0;
                    if (isDebitSelected)
                        openningBalance -= Converter.Obj2Decimal(dataTabeAnalyse.Rows[0]["Debit"]);
                    else
                        openningBalance -= 0;
                    if (isCreditSelected)
                        openningBalance += Converter.Obj2Decimal(dataTabeAnalyse.Rows[0]["Credit"]);
                    else
                        openningBalance += 0;
                    if (openningBalance != lastBalance && lastBalance != 0)
                    {
                        warningBalance = "The Closing Balance of Account: '" + accountName.Split('-')[0].ToString() +
                             "'(" + lastBalance.ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + " )"
                          + " does not match the opening balance of the transactions being uploaded ("
                          + openningBalance.ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + " )";
                    }
                }
            }
            foreach (DataRow row in dataTabeAnalyse.Rows)
            {
                string styleClass = "";
                if (transactionsDuplicate.Count > 0)// checking on database
                {
                    if (!isBalanceSelected && isDebitSelected && isCreditSelected)
                    {
                        foreach (var item in transactionsDuplicate)
                        {
                            if (item.Date == Convert.ToDateTime(row["Date"]).ToShortDateString()
                                && item.Description.ToString() == row["Description"].ToString()
                                  && item.Debit.ToString() == Converter.Obj2Decimal(row["Debit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                  && item.Credit.ToString() == Converter.Obj2Decimal(row["Credit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat))
                            {
                                styleClass = " class='error' ";
                                row["Duplicate"] = true;
                                break;
                            }
                        }
                    }
                    else if (!isBalanceSelected && !isDebitSelected && isCreditSelected)
                    {
                        foreach (var item in transactionsDuplicate)
                        {
                            if (item.Date == Convert.ToDateTime(row["Date"]).ToShortDateString()
                                && item.Description.ToString() == row["Description"].ToString()
                                  //&& item.Debit.ToString() == Converter.Obj2Decimal(row["Debit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                  && item.Credit.ToString() == Converter.Obj2Decimal(row["Credit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat))
                            {
                                styleClass = " class='error' ";
                                row["Duplicate"] = true;
                                break;
                            }
                        }
                    }
                    if (!isBalanceSelected && isDebitSelected && !isCreditSelected)
                    {
                        foreach (var item in transactionsDuplicate)
                        {
                            if (item.Date == Convert.ToDateTime(row["Date"]).ToShortDateString()
                                && item.Description.ToString() == row["Description"].ToString()
                                  && item.Debit.ToString() == Converter.Obj2Decimal(row["Debit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                  //&& item.Credit.ToString() == Converter.Obj2Decimal(row["Credit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                  )
                            {
                                styleClass = " class='error' ";
                                row["Duplicate"] = true;
                                break;
                            }
                        }
                    }
                    else if (isBalanceSelected && isDebitSelected && isCreditSelected)
                    {
                        foreach (var item in transactionsDuplicate)
                        {
                            if (item.Date == Convert.ToDateTime(row["Date"]).ToShortDateString()
                                && item.Description.ToString() == row["Description"].ToString()
                                  && item.Debit.ToString() == Math.Round(Converter.Obj2Decimal(row["Debit"]), 2).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                  && item.Credit.ToString() == Math.Round(Converter.Obj2Decimal(row["Credit"]), 2).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                   && item.Balance.ToString() == Math.Round(Converter.Obj2Decimal(row["Balance"]), 2).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                   )
                            {
                                styleClass = " class='error' ";
                                row["Duplicate"] = true;
                                break;
                            }
                        }
                    }
                    else if (isBalanceSelected && !isDebitSelected && isCreditSelected)
                    {
                        foreach (var item in transactionsDuplicate)
                        {
                            if (item.Date == Convert.ToDateTime(row["Date"]).ToShortDateString()
                                && item.Description.ToString() == row["Description"].ToString()
                                  //&& item.Debit.ToString() == Converter.Obj2Decimal(row["Debit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                  && item.Credit.ToString() == Math.Round(Converter.Obj2Decimal(row["Credit"]), 2).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                   && item.Balance.ToString() == Math.Round(Converter.Obj2Decimal(row["Balance"]), 2).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                   )
                            {
                                styleClass = " class='error' ";
                                row["Duplicate"] = true;
                                break;
                            }
                        }
                    }
                    else if (isBalanceSelected && isDebitSelected && !isCreditSelected)
                    {
                        foreach (var item in transactionsDuplicate)
                        {
                            if (item.Date == Convert.ToDateTime(row["Date"]).ToShortDateString()
                                && item.Description.ToString() == row["Description"].ToString()
                                  && item.Debit.ToString() == Math.Round(Converter.Obj2Decimal(row["Debit"]), 2).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                   //&& item.Credit.ToString() == Converter.Obj2Decimal(row["Credit"]).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                   && item.Balance.ToString() == Math.Round(Converter.Obj2Decimal(row["Balance"]), 2).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat)
                                   )
                            {
                                styleClass = " class='error' ";
                                row["Duplicate"] = true;
                                break;
                            }
                        }
                    }
                }

                // begin generate table
                retTable += "<tr>";
                string dateresult = "";
                for (int i = 0; i < (isDuplicate == true ? dataTabeAnalyse.Columns.Count - 1 : dataTabeAnalyse.Columns.Count); i++) // -1 : not using Duplicate columns
                {
                    string colName = dataTabeAnalyse.Columns[i].ColumnName;
                    switch (colName)
                    {
                        case "Date":
                            if (row[i].ToString() == "")
                            {
                                retTable += "<td class='error'> </td>";
                                valiError.Add("Selected <strong><i>Date</i></strong> column does not have a date format! </br>");
                            }
                            else
                            {
                                bool dateVal = false;
                                if (string.IsNullOrEmpty(dateFormatSelected))
                                {
                                    foreach (var dateFormat in listDateFormat)
                                    {
                                        var tmps = row[i].ToString().Split(' ');
                                        if (tmps.Length > 2)
                                        {
                                            var tmp1 = tmps[tmps.Length == 4 ? 2 : 1].Split(':');
                                            if (Convert.ToInt32(tmp1[0]) < 10)
                                                dateresult = tmps[0] + " 0" + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];
                                            else
                                                dateresult = tmps[0] + " " + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];

                                            if (Converter.Obj2Date(dateresult, dateFormat) != null)
                                                dateVal = true;
                                            else
                                                dateVal = false;
                                        }
                                    }
                                    if (dateVal)
                                        retTable += "<td" + styleClass + ">" + row[i].ToString() + "</td>";
                                    else
                                    {
                                        retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                        valiError.Add("Selected <strong><i>Date</i></strong> column does not have a date format! </br>");
                                    }
                                }
                                else
                                {
                                    var tmps = row[i].ToString().Split(' ');
                                    if (tmps.Length > 2)
                                    {
                                        var tmp1 = tmps[tmps.Length == 4 ? 2 : 1].Split(':');
                                        if (Convert.ToInt32(tmp1[0]) < 10)
                                            dateresult = tmps[0] + " 0" + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];
                                        else
                                            dateresult = tmps[0] + " " + tmp1[0] + ":" + tmp1[1] + ":" + tmp1[2] + " " + tmps[tmps.Length == 4 ? 3 : 2];

                                        if (Converter.Obj2Date(dateresult, dateFormatSelected) != null)
                                            dateVal = true;
                                        else
                                            dateVal = false;
                                    }

                                    if (dateVal)
                                        retTable += "<td" + styleClass + ">" + row[i].ToString() + "</td>";
                                    else
                                    {
                                        retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                        valiError.Add("Selected <strong><i>Date</i></strong> column does not have a date format! </br>");
                                    }
                                }
                            }
                            break;

                        case "Debit":
                            if (row[i].ToString() == "")
                                retTable += "<td" + styleClass + "> </td>";
                            else
                            {
                                if (Converter.Obj2DecimalNull(row[i]) != null)
                                {
                                    if (GetOctagon(row[i].ToString()).Length > 2)
                                        retTable += "<td" + styleClass + ">" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                    else
                                        retTable += "<td" + styleClass + ">" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                }
                                else
                                {
                                    retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                    valiError.Add("Selected <strong><i>Debit</i></strong> column does not have a numeric format! </br>");
                                }
                            }
                            break;

                        case "Credit":
                            if (row[i].ToString() == "")
                                retTable += "<td " + styleClass + "> </td>";
                            else
                            {
                                if (Converter.Obj2DecimalNull(row[i]) != null)
                                {
                                    if (GetOctagon(row[i].ToString()).Length > 2)
                                        retTable += "<td" + styleClass + ">" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                    else
                                        retTable += "<td" + styleClass + ">" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                }
                                else
                                {
                                    retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                    valiError.Add("Selected <strong><i>Credit</i></strong> column does not have a numeric format! </br>");
                                }
                            }
                            break;

                        case "Balance":
                            if (row[i].ToString() == "")
                                retTable += "<td " + styleClass + "> </td>";
                            else
                            {
                                if (Converter.Obj2DecimalNull(row[i]) != null)
                                {
                                    if (GetOctagon(row[i].ToString()).Length > 2)
                                        retTable += "<td" + styleClass + ">" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                    else
                                        retTable += "<td" + styleClass + ">" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                                }
                                else
                                {
                                    retTable += "<td class='error'>" + row[i].ToString() + "</td>";
                                    valiError.Add("Selected <strong><i>Balance</i></strong> column does not have a numeric format! </br>");
                                }
                            }
                            break;

                        default:
                            retTable += "<td" + styleClass + ">" + row[i].ToString() + "</td>";
                            break;
                    }
                }
                retTable += "</tr>";
            }

            valiError = valiError.Distinct().ToList();
            result.Add(string.Join("", valiError.ToArray()));
            result.Add(retTable);
            result.Add(warningBalance);
            result.Add(addDebitCol);
            result.Add(addCreditCol);
            result.Add(addBalanceCol);
            result.Add(valiDupliacte);
            return result;
        }

        /// <summary>
        /// Generate to html table with a column selected by column default in DB
        /// </summary>
        /// <param name="datatabe">data transaction to table</param>
        /// <param name="uploadFormat">last configure upload, using it to set default selected</param>
        /// <returns>table html</returns>
        public static string ConvertDataTableToHTMLTableUpload(DataTable datatabe, UploadFormat uploadFormat)
        {
            List<transactionStructure> lstColInDb = TransactionColName();
            string ret = "";

            // first row select option column Transaction
            ret = "<tr id='columns-row' class='columns-row' style='background-color: #ddd;'>";
            //foreach column, add dropdown value columns of Transaction to columns table html
            // index of columns
            int colIndex = 1;
            int colIndexTable = 1;
            if (uploadFormat == null)
                foreach (DataColumn col in datatabe.Columns)
                {
                    ret += "<td style = 'width: 150px;'>";
                    ret += "<select class='columnSelected" + "' name = '" + "Column_" + colIndex.ToString()
                        + "' style = 'width: 100 %;' id='" + "Column_" + colIndex.ToString() + "'"
                        + "onchange=" + "\"configureColumns('" + colIndex + "')\"" + "> ";

                    foreach (FilesUploadUtility.transactionStructure item in lstColInDb)
                    {
                        if (hardColumn.headColRequire.Contains(item.colKey))
                            ret += "<option  data-mytxt='*' value ='" + item.colKey + "_" + colIndex.ToString() + "'>" + item.colValue + "</option >";
                        else
                            ret += "<option data-mytxt='' value ='" + item.colKey + "_" + colIndex.ToString() + "'>" + item.colValue + "</option >";
                    }

                    ret += "</select>";
                    ret += "</td>";
                    colIndex++;
                }
            else
            {
                List<transactionStructure> tran = new List<transactionStructure>();
                transactionStructure date = new transactionStructure()
                {
                    colKey = "Date",
                    colValue = uploadFormat.DateIndex.ToString()
                };
                tran.Add(date);
                transactionStructure referent = new transactionStructure()
                {
                    colKey = "Reference",
                    colValue = uploadFormat.ReferenceIndex.ToString()
                };
                tran.Add(referent);
                transactionStructure description = new transactionStructure()
                {
                    colKey = "Description",
                    colValue = uploadFormat.DescriptionIndex.ToString()
                };
                tran.Add(description);
                transactionStructure debit = new transactionStructure()
                {
                    colKey = "Debit",
                    colValue = uploadFormat.DebitIndex.ToString()
                };
                tran.Add(debit);
                transactionStructure credit = new transactionStructure()
                {
                    colKey = "Credit",
                    colValue = uploadFormat.CreditIndex.ToString()
                };
                tran.Add(credit);
                transactionStructure balance = new transactionStructure()
                {
                    colKey = "Balance",
                    colValue = uploadFormat.BalanceIndex.ToString()
                };
                tran.Add(balance);
                transactionStructure reference1 = new transactionStructure()
                {
                    colKey = "Reference1",
                    colValue = uploadFormat.Reference1Index.ToString()
                };
                tran.Add(reference1);
                transactionStructure descCol1 = new transactionStructure()
                {
                    colKey = "DescCol1",
                    colValue = uploadFormat.DescCol1Index.ToString()
                };
                tran.Add(descCol1);
                transactionStructure descCol2 = new transactionStructure()
                {
                    colKey = "DescCol2",
                    colValue = uploadFormat.DescCol2Index.ToString()
                };
                tran.Add(descCol2);
                transactionStructure descCol3 = new transactionStructure()
                {
                    colKey = "DescCol2",
                    colValue = uploadFormat.DescCol2Index.ToString()
                };
                tran.Add(descCol2);

                foreach (DataColumn col in datatabe.Columns)
                {
                    ret += "<td style = 'width: 150px;'>";
                    ret += "<select class='columnSelected" + "' name = '" + "Column_" + colIndex.ToString()
                        + "' style = 'width: 100 %;' id='" + "Column_" + colIndex.ToString() + "'"
                        + "onchange=" + "\"configureColumns('" + colIndex + "')\"" + "> ";

                    foreach (FilesUploadUtility.transactionStructure item in lstColInDb)
                    {
                        string selected = "";
                        foreach (var t in tran)
                        {
                            if (t.colKey == item.colKey && t.colValue != "0" && Converter.Obj2Int(t.colValue) == colIndexTable)
                                selected = "selected";
                        }
                        if (hardColumn.headColRequire.Contains(item.colKey))
                            ret += "<option  data-mytxt='*' " + selected + " value ='" + item.colKey + "_" + colIndex.ToString() + "'>" + item.colValue + "</option >";
                        else
                            ret += "<option data-mytxt='' " + selected + " value ='" + item.colKey + "_" + colIndex.ToString() + "'>" + item.colValue + "</option >";
                    }

                    ret += "</select>";
                    ret += "</td>";
                    colIndex++; colIndexTable++;
                }
            }

            ret += "</tr>";

            //

            foreach (DataRow row in datatabe.Rows)
            {
                ret += "<tr>";
                for (int i = 0; i < datatabe.Columns.Count; i++)
                {
                    string colName = datatabe.Columns[i].ColumnName;
                    if (colName == "Debit" || colName == "Credit" || colName == "Balance")
                    {
                        if (row[i].ToString() == "")
                            ret += "<td> </td>";
                        else
                        {
                            if (GetOctagon(row[i].ToString()).Length > 2)
                                ret += "<td>" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                            else
                                ret += "<td>" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                        }
                    }
                    else
                    {
                        decimal isNumber;
                        if (decimal.TryParse(row[i].ToString(), out isNumber))
                        {
                            if (GetOctagon(row[i].ToString()).Length > 2)
                                ret += "<td>" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                            else
                                ret += "<td>" + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                        }
                        else
                            ret += "<td>" + row[i].ToString() + "</td>";
                    }
                }
                ret += "</tr>";
            }
            return ret;
        }

        /// <summary>
        /// convert table to html, check duplicate row, get sum money,count records,start- end date
        /// </summary>
        /// <param name="datatabe">data table transaction</param>
        /// <param name="headerColumns">header columns selected</param>
        /// <param name="dateFormat">format date by selected date format</param>
        /// <param name="accountLastBalance"></param>
        /// <returns>
        /// table transaction Final Version with duplicate if exist
        /// table Analysed Data
        /// </returns>
        public static List<string> ConvertTransactionToHTMLTable(DataTable datatabe, string headerColumns, string dateFormat, decimal openingBalance, int max_row_display)
        {
            List<string> ret = new List<string>();
            //1. Analysed Data
            string retAnalysedData = "";
            retAnalysedData += "<tr>";
            retAnalysedData += "<td>" + Converter.Obj2Date(datatabe.Rows[0]["Date"], dateFormat).ToShortDateString() + "</td>";//strart date
            retAnalysedData += "<td>" + Converter.Obj2Date(datatabe.Rows[datatabe.Rows.Count - 1]["Date"], dateFormat).ToShortDateString() + "</td>";//end date
            retAnalysedData += "<td>" + datatabe.Rows.Count.ToString() + "</td>";// total records
            decimal[] moneysTotal = null;
            if (headerColumns.Contains("Credit"))
            {
                moneysTotal = datatabe.AsEnumerable()
                                           .Select(row => Converter.Obj2Decimal(row["Credit"])).ToArray();
                retAnalysedData += "<td>" + moneysTotal.Sum().ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat); //credit
            }
            else
                retAnalysedData += "<td>";
            if (headerColumns.Contains("Debit"))
            {
                moneysTotal = datatabe.AsEnumerable()
                                     .Select(row => Converter.Obj2Decimal(row["Debit"])).ToArray();
                retAnalysedData += "<td>" + moneysTotal.Sum().ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat); //debit
            }
            else
                retAnalysedData += "<td>";
            retAnalysedData += "<td>" + openingBalance.ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";// Opening Balance
            if (headerColumns.Contains("Balance"))
                retAnalysedData += "<td>" +
                    Converter.Obj2Decimal(datatabe.Rows[datatabe.Rows.Count - 1]["Balance"].ToString()).ToString("#,###.##",
                    CultureInfo.InvariantCulture.NumberFormat) + "</td>";// Closing Balance
            else
                retAnalysedData += "<td></td>";
            retAnalysedData += "</tr>";

            //3. transaction Final Version
            string retTransaction = "";
            retTransaction = "<tr style='background-color: #ddd;'>";
            foreach (DataColumn col in datatabe.Columns)
            {
                retTransaction += "<td>" + col.ColumnName + "</td>";
            }
            retTransaction += "</tr>";

            // body transaction
            int _display_row = 0;
            foreach (DataRow row in datatabe.Rows)
            {
                if (max_row_display > 0)
                {
                    if (_display_row == max_row_display)
                        break;
                    else
                        _display_row++;
                }
                retTransaction += "<tr>";
                for (int i = 0; i < datatabe.Columns.Count; i++)
                {
                    string colName = datatabe.Columns[i].ColumnName;
                    if (colName == "Debit" || colName == "Credit" || colName == "Balance")
                    {
                        if (row[i].ToString() == "")
                            retTransaction += "<td> </td>";
                        else
                        {
                            if (GetOctagon(row[i].ToString()).Length > 2)
                                retTransaction += "<td>" + (Math.Round(decimal.Parse(row[i].ToString()), 2)).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                            else
                                retTransaction += "<td> " + decimal.Parse(row[i].ToString()).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                        }
                    }
                    else
                        retTransaction += "<td>" + row[i].ToString() + "</td>";
                }
                retTransaction += "</tr>";
            }

            ret.Add(retAnalysedData);
            ret.Add(retTransaction);
            return ret;
        }

        /// <summary>
        /// set Traffic Light for Manage Transaction Matching
        /// </summary>
        /// <param name="tNowA">now account 1</param>
        /// <param name="tPreviousA">previous Account 1</param>
        /// <param name="tNowB">now account 2</param>
        /// <param name="tPreviousB">previous Account 2</param>
        /// <returns>string status traffic light</returns>
        public static string setTrafficLightMatching(bool tNowA, bool tPreviousA, bool tNowB, bool tPreviousB)
        {
            string status = "";
            if (tNowA && tPreviousA && tNowB && tPreviousB) // 1 1 1 1
                status = "status_ok";
            else if (!tNowA && tPreviousA && tNowB && tPreviousB) // 0 1 1 1
                status = "status_amber";
            else if (tNowA && !tPreviousA && tNowB && tPreviousB) // 1 0 1 1
                status = "status_amber";
            else if (tNowA && tPreviousA && !tNowB && tPreviousB) // 1 1 0 1
                status = "status_amber";
            else if (tNowA && tPreviousA && tNowB && tPreviousB) // 1 1 1 0
                status = "status_amber";
            //
            else if (tNowA && tPreviousA && tNowB && tPreviousB) // 1 1 0 0
                status = "status_error";
            else if (tNowA && !tPreviousA && tNowB && !tPreviousB) // 1 0 1 0
                status = "status_warrning";
            else if (tNowA && tPreviousA && tNowB && tPreviousB) // 1 0 0 1
                status = "status_amber";
            else if (!tNowA && tPreviousA && !tNowB && tPreviousB) // 0 1 0 1
                status = "status_amber";
            else if (!tNowA && !tPreviousA && tNowB && tPreviousB) // 0 0 1 1
                status = "status_error";
            else if (!tNowA && tPreviousA && tNowB && !tPreviousB) // 0 1 1 0
                status = "status_warrning";
            //
            else if (!tNowA && !tPreviousA && !tNowB && !tPreviousB) //0 0 0 0
                status = "status_error";
            else if (tNowA && !tPreviousA && !tNowB && !tPreviousB) // 1 0 0 0
                status = "status_error";
            else if (!tNowA && tPreviousA && !tNowB && !tPreviousB) // 0 1 0 0
                status = "status_error";
            else if (!tNowA && !tPreviousA && tNowB && !tPreviousB) // 0 0 1 0
                status = "status_error";
            else if (!tNowA && !tPreviousA && !tNowB && tPreviousB) // 0 0 0 1
                status = "status_error";
            else
                status = "status_error";
            return status;
        }

        #region Datetime get

        /// <summary>
        /// Start Business day
        /// </summary>
        public static DayOfWeek StartBusinessDay()
        {
            return DayOfWeek.Monday;
        }

        /// <summary>
        /// set wekkends
        /// </summary>
        /// <returns></returns>
        public static DayOfWeek[] weekends()
        {
            return new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
        }

        /// <summary>
        /// Get First Business Day of week
        /// </summary>
        /// <param name="dt">date time get</param>
        /// <param name="startOfWeek">start Business Of Week</param>
        /// <returns>date</returns>
        public static DateTime GetFirstBusinessWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Get First Business Day of month, exclude day is weekends
        /// </summary>
        /// <param name="dt">date time get</param>
        /// <returns>date</returns>
        public static DateTime GetFirstBusinessMonth(this DateTime dt)
        {
            int daysInMonth = DateTime.DaysInMonth(dt.Year, dt.Month);
            IEnumerable<int> businessDaysInMonth = Enumerable.Range(1, daysInMonth)
                                        .Where(d => !weekends().Contains(new DateTime(dt.Year, dt.Month, d).DayOfWeek));
            return new DateTime(dt.Year, dt.Month, (int)businessDaysInMonth.FirstOrDefault());
        }

        /// <summary>
        ///  Get First Business Day of year, exclude day is weekends
        /// </summary>
        /// <param name="dt">date time get</param>
        /// <returns>date</returns>
        public static DateTime GetFirstBusinessYear(this DateTime dt)
        {
            int daysInMonth = DateTime.DaysInMonth(dt.Year, 1);
            IEnumerable<int> businessDaysInMonth = Enumerable.Range(1, daysInMonth)
                                        .Where(d => !weekends().Contains(new DateTime(dt.Year, 1, d).DayOfWeek));
            return new DateTime(dt.Year, dt.Month, (int)businessDaysInMonth.FirstOrDefault());
        }

        #endregion Datetime get

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="currentUserId">not required, if has value then check and return "Me" if equal createdby</param>
        /// <returns></returns>
        public static string GetFullNameOfUser(ApplicationUser user, string currentUserId = "")
        {
            var result = "";
            if (user != null)
            {
                if (user.Id == currentUserId)
                    result = "Me";
                else
                {
                    if (!string.IsNullOrEmpty(user.DisplayUserName))
                        result = user.DisplayUserName;
                    else if (!string.IsNullOrEmpty(user.Forename) && !string.IsNullOrEmpty(user.Surname))
                        result = user.Forename + " " + user.Surname;
                    else
                        result = user.Email;
                }
            }
            return result;
        }
         public static string GetPosUserType(PosUserType valueToCheck)
        {
            bool isDefined = Enum.IsDefined(typeof(PosUserType), valueToCheck);
            return isDefined ? $"{valueToCheck}" : "";
        }
        public static bool GetPosUserType(string valueToCheck)
        {
            return Enum.IsDefined(typeof(PosUserType), valueToCheck);
            
        }

        public static string GetFullName(this ApplicationUser user, string currentUserId = "")
        {
            var result = "";
            if (user != null)
            {
                if (user.Id == currentUserId)
                    result = "Me";
                else
                {
                    if (!string.IsNullOrEmpty(user.DisplayUserName))
                        result = user.DisplayUserName;
                    else if (!string.IsNullOrEmpty(user.Forename) && !string.IsNullOrEmpty(user.Surname))
                        result = user.Forename + " " + user.Surname;
                    else
                        result = user.Email;
                }
            }
            return result;
        }

        /// <summary>
        ///  Algorithm for checking Domain Access permission
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static bool CheckingDomainAccessPermission(string userId, QbicleDomain domain)
        {
            try
            {
                var dbContext = new ApplicationDbContext();
                var _user = new UserRules(dbContext).GetUser(userId, 0);
                if (_user == null || domain == null)
                {
                    return false;
                }

                if (domain.Users.Any(e => e.Id == userId))
                    return true;
                //Is the CurrentUser a DomainAdministrator
                if (domain.Administrators.Any(e => e.Id == userId))
                    return true;

                LogManager.Warn(MethodBase.GetCurrentMethod(), $"Unauthorised access was attempted by UserId {userId} into DomainId {domain.Id}", userId);
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId);
                LogManager.Warn(MethodBase.GetCurrentMethod(), $"Unauthorised access was attempted by UserId {userId} into DomainId {domain.Id}", userId);
                return false;
            }
        }

        /// <summary>
        /// Algorithm for checking Qbicle Access permission
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="qbicleId"></param>
        /// <returns></returns>
        public static int CheckingQbicleAccessPermission(string userId, int qbicleId)
        {
            try
            {
                var dbContext = new ApplicationDbContext();
                var _user = new UserRules(dbContext).GetUser(userId, 0);

                if (_user == null || qbicleId <= 0)
                {
                    return -1;
                }
                var qbicle = dbContext.Qbicles.Find(qbicleId);
                if (qbicle == null) return -1;
                //Is the CurrentUser a QbicleUser
                if (qbicle.Members.Any(e => e.Id == _user.Id))
                    return qbicleId;

                //Is the CurrentUser a Domain User of the Domain in which the Qbicle is located AND the Qbicle is a Public Qbicle
                if (qbicle.Domain.Users.Any(e => e.Id == _user.Id))
                    return qbicleId;
                //Is the CurrentUser a DomainAdministrator of the Domain in which the Qbicle is located AND the Qbicle is a Public Qbicle
                if (qbicle.Domain.Administrators.Any(e => e.Id == _user.Id))
                    return qbicleId;
                //Allow access if is B2CQbicle
                if (qbicle is Models.B2C_C2C.B2CQbicle)
                    return qbicleId;

                LogManager.Warn(MethodBase.GetCurrentMethod(), $"Unauthorised access was attempted by UserId {userId} into QbicleId {qbicleId}", userId);
                return -1;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId);
                LogManager.Warn(MethodBase.GetCurrentMethod(), $"Unauthorised access was attempted by UserId {userId} into QbicleId {qbicleId}", userId);
                return -1;
            }
        }

        public static bool ContainsKey(List<string> lstKey, string key)
        {
            foreach (var item in lstKey)
            {
                if (key.Contains(item)) return true;
            }
            return false;
        }

        public static string Encrypt(string encryptString)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }

        public static string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static List<DefaultMedia> GetListDefaultMedia(string domainLink)
        {
            string folderPath = WebConfigurationManager.AppSettings["DefaultMedia"];
            DirectoryInfo di = new DirectoryInfo(HttpContext.Current.Server.MapPath(folderPath));
            FileInfo[] lstInfo = di.GetFiles("*.jpg");
            List<DefaultMedia> lstDefaultMedia = new List<DefaultMedia>();
            foreach (FileInfo info in lstInfo)
            {
                lstDefaultMedia.Add(new DefaultMedia
                {
                    Id = Path.GetFileNameWithoutExtension(info.Name),
                    FileUrl = domainLink + folderPath.Replace("\\", "/") + "/" + info.Name,
                    FilePath = info.FullName,
                    FileName = info.Name,
                    Extension = Path.GetExtension(info.FullName)
                });
            }
            return lstDefaultMedia.OrderBy(d => d.Id).ToList();
        }

        public static string GetIPAddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        public static string GetCurrentSessionID()
        {
            return HttpContext.Current.Session.SessionID;
        }

        public static string isInitiatorReviewerApprover(ApplicationUser user, ApplicationUser initiator, List<ApplicationUser> reviewers, List<ApplicationUser> approvers)
        {
            string _info = "";
            if (user == initiator)
                _info += "Initiator, ";
            if (reviewers != null && reviewers.Any(s => s.Id == user.Id))
                _info += "Reviewer, ";
            if (approvers != null && approvers.Any(s => s.Id == user.Id))
                _info += "Approver";
            return _info;
        }

        //public static string TempPathSQLiteRepository()
        //{
        //    var tempPath = ConfigManager.SQLiteRepository;
        //    DirectoryInfo info = new DirectoryInfo(tempPath);
        //    if (!info.Exists)
        //        info.Create();
        //    return tempPath;
        //}

        public static PasswordValidator PasswordValidator()
        {
            return new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
        }

        public static FileTypeEnum GetFileTypeEnum(this string type)
        {
            var fileType = FileTypeEnum.Document;
            if (type.ToLower() == "image file")
                fileType = FileTypeEnum.Image;
            else if (type.ToLower() == "video file")
                fileType = FileTypeEnum.Video;
            return fileType;
        }

        /// <summary>
        /// Convert uri string ito a new instance of the System.Uri class with the specified URI.
        /// </summary>
        /// <param name="mediaKey"></param>
        /// <returns></returns>
        public static Uri ToUri(this string mediaKey, FileTypeEnum fileType = FileTypeEnum.Image, string size = "")
        {
            switch (fileType)
            {
                case FileTypeEnum.Video:
                    return new Uri($"{string.Format(ConfigManager.ApiGetVideoUri, mediaKey, "mp4")}");

                case FileTypeEnum.Image:
                    if (string.IsNullOrEmpty(size))
                        return new Uri($"{ConfigManager.ApiGetDocumentUri}{mediaKey}");
                    return new Uri($"{ConfigManager.ApiGetDocumentUri}{mediaKey}&size={size}");

                default:
                    return new Uri($"{ConfigManager.ApiGetDocumentUri}{mediaKey}");
            }
        }

        public static string ToUriString(this string mediaKey, FileTypeEnum fileType = FileTypeEnum.Image, string size = "")
        {
            if (string.IsNullOrEmpty(mediaKey))
                return "";
            switch (fileType)
            {
                case FileTypeEnum.Video:
                    return $"{string.Format(ConfigManager.ApiGetVideoUri, mediaKey, "mp4")}";

                case FileTypeEnum.Image:
                    if (string.IsNullOrEmpty(size))
                        return $"{ConfigManager.ApiGetDocumentUri}{mediaKey}";
                    return $"{ConfigManager.ApiGetDocumentUri}{mediaKey}&size={size}";

                default:
                    return $"{ConfigManager.ApiGetDocumentUri}{mediaKey}";
            }
        }

        /// <summary>
        /// Convert ToBase64 i.e. azure metadata
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToBase64(this string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Convert ToBase64 i.e. azure metadata
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FromBase64(this string input)
        {
            var bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }

        public static Uri ToDocumentUri(this string mediaKey, FileTypeEnum fileType = FileTypeEnum.Image, string size = "")
        {
            switch (fileType)
            {
                case FileTypeEnum.Video:
                    return new Uri($"{string.Format(ConfigManager.ApiGetVideoUri, mediaKey, "mp4")}");

                case FileTypeEnum.Image:
                    if (string.IsNullOrEmpty(size))
                        return new Uri($"{ConfigManager.ApiGetDocumentUri}{mediaKey}");
                    return new Uri($"{ConfigManager.ApiGetDocumentUri}{mediaKey}&size={size}");

                default:
                    return new Uri($"{ConfigManager.ApiGetDocumentUri}{mediaKey}");
            }
        }

        public static Uri ToVideoUri(this string mediaKey, string type)
        {
            return new Uri($"{string.Format(ConfigManager.ApiGetVideoUri, mediaKey, type)}");
        }

        /// <summary>
        /// Get Created by folow the ApprovalRed
        /// </summary>
        /// <param name="approval">ApprovalReq</param>
        /// <returns></returns>
        public static ApplicationUser GetCreatedBy(this ApprovalReq approval)
        {
            switch (approval.RequestStatus)
            {
                case ApprovalReq.RequestStatusEnum.Pending:
                    return approval.StartedBy;

                case ApprovalReq.RequestStatusEnum.Reviewed:
                    return approval.ReviewedBy.FirstOrDefault();

                case ApprovalReq.RequestStatusEnum.Approved:
                case ApprovalReq.RequestStatusEnum.Denied:
                case ApprovalReq.RequestStatusEnum.Discarded:
                    return approval.ApprovedOrDeniedAppBy;

                default:
                    return null;
            }
        }

        public static string GetClass(this Enum value)
        {
            try
            {
                if (value is PrepQueueStatus)
                {
                    return ((PrepQueueStatus)value).PrepQueueStatusClass();
                }
                if (value is DeliveryStatus)
                {
                    return ((DeliveryStatus)value).DeliveryStatusClass();
                }
                if (value is DriverDeliveryStatus)
                {
                    return ((DriverDeliveryStatus)value).DriverDeliveryStatusClass();
                }
                if (value is DriverStatus)
                {
                    return ((DriverStatus)value).DriverStatusClass();
                }
                if (value is TradeOrderStatusEnum)
                {
                    return ((TradeOrderStatusEnum)value).B2COrderStatusClass();
                }
                if (value is PosDeviceStatus)
                {
                    return ((PosDeviceStatus)value).PosDeviceStatusClass();
                }
            }
            catch
            {
                return StatusLabelStyle.Secondary;
            }
            return StatusLabelStyle.Secondary;
        }

        private static string PosDeviceStatusClass(this PosDeviceStatus status)
        {
            switch (status)
            {
                case PosDeviceStatus.Active:
                    return $"{StatusLabelStyle.Success}";
                case PosDeviceStatus.InActive:
                    return $"{StatusLabelStyle.Warning}";
                default:
                    return $"{StatusLabelStyle.Primary}";
            }
        }

        private static string DeliveryStatusClass(this DeliveryStatus status)
        {
            switch (status)
            {
                case DeliveryStatus.New:
                    return StatusLabelStyle.Info;

                case DeliveryStatus.Accepted:
                    return StatusLabelStyle.Success;

                case DeliveryStatus.Started:
                    return StatusLabelStyle.Primary;

                case DeliveryStatus.Completed:
                    return StatusLabelStyle.Success;

                case DeliveryStatus.CompletedWithProblems:
                    return StatusLabelStyle.Warning;

                case DeliveryStatus.Deleted:
                    return StatusLabelStyle.Danger;

                default:
                    return StatusLabelStyle.Info; ;
            }
        }

        private static string PrepQueueStatusClass(this PrepQueueStatus status)
        {
            switch (status)
            {
                case PrepQueueStatus.NotStarted:
                    return StatusLabelStyle.Info;

                case PrepQueueStatus.Preparing:
                    return StatusLabelStyle.Info;

                case PrepQueueStatus.Completing:
                    return StatusLabelStyle.Primary;

                case PrepQueueStatus.Completed:
                    return StatusLabelStyle.Success;

                case PrepQueueStatus.CompletedWithProblems:
                    return StatusLabelStyle.Warning;

                default:
                    return StatusLabelStyle.Secondary;
            }
        }

        private static string DriverDeliveryStatusClass(this DriverDeliveryStatus status)
        {
            switch (status)
            {
                case DriverDeliveryStatus.NotSet:
                    return StatusLabelStyle.Info;

                case DriverDeliveryStatus.Accepted:
                    return StatusLabelStyle.Primary;

                case DriverDeliveryStatus.Rejected:
                    return StatusLabelStyle.Warning;

                case DriverDeliveryStatus.HeadingToDepot:
                    return StatusLabelStyle.Info;

                case DriverDeliveryStatus.ReadyForPickup:
                    return StatusLabelStyle.Primary;

                case DriverDeliveryStatus.StartedDelivery:
                    return StatusLabelStyle.Info;

                case DriverDeliveryStatus.Completed:
                    return StatusLabelStyle.Success;

                case DriverDeliveryStatus.CompletedWithProblems:
                    return StatusLabelStyle.Warning;

                default:
                    return StatusLabelStyle.Secondary;
            }
        }

        private static string DriverStatusClass(this DriverStatus status)
        {
            switch (status)
            {
                case DriverStatus.IsAvailable:
                    return StatusLabelStyle.Success;

                case DriverStatus.IsBusy:
                    return StatusLabelStyle.Warning;

                default:
                    return StatusLabelStyle.Secondary;
            }
        }

        private static string B2COrderStatusClass(this TradeOrderStatusEnum status)
        {
            switch (status)
            {
                case TradeOrderStatusEnum.Draft:
                    return $"{StatusLabelStyle.Primary}";

                case TradeOrderStatusEnum.AwaitingProcessing:
                    return $"{StatusLabelStyle.Primary}";

                case TradeOrderStatusEnum.InProcessing:
                    return $"{StatusLabelStyle.Warning}";

                case TradeOrderStatusEnum.Processed:
                case TradeOrderStatusEnum.ProcessedWithProblems:
                    return $"{StatusLabelStyle.Success}";

                default:
                    return $"{StatusLabelStyle.Primary}";
            }
        }

        public static string GetClass(this TradeOrder order)
        {
            try
            {
                switch (order.OrderStatus)
                {
                    case TradeOrderStatusEnum.Draft:
                    case TradeOrderStatusEnum.AwaitingProcessing:
                        return StatusLabelStyle.Primary;

                    case TradeOrderStatusEnum.InProcessing:
                        return StatusLabelStyle.Warning;

                    case TradeOrderStatusEnum.Processed:
                    case TradeOrderStatusEnum.ProcessedWithProblems:
                        if ((order.Invoice?.TotalInvoiceAmount ?? 0) > order.Payments.Sum(e => e.Amount))
                            return StatusLabelStyle.Warning;
                        return StatusLabelStyle.Success;

                    default:
                        return StatusLabelStyle.Primary;
                }
            }
            catch
            {
                return StatusLabelStyle.Secondary;
            }
        }

        public static string GetClassColor(this TradeOrder order)
        {
            try
            {
                switch (order.OrderStatus)
                {
                    case TradeOrderStatusEnum.Draft:
                    case TradeOrderStatusEnum.AwaitingProcessing:
                        return StatusLabelStyle.PrimaryColor;

                    case TradeOrderStatusEnum.InProcessing:
                        return StatusLabelStyle.InfoColor;

                    case TradeOrderStatusEnum.Processed:
                    case TradeOrderStatusEnum.ProcessedWithProblems:
                        if ((order.Invoice?.TotalInvoiceAmount ?? 0) > order.Payments.Sum(e => e.Amount))
                            return StatusLabelStyle.WarningColor;
                        return StatusLabelStyle.SuccessColor;

                    default:
                        return StatusLabelStyle.PrimaryColor;
                }
            }
            catch
            {
                return StatusLabelStyle.SecondaryColor;
            }
        }

        public static string GetDescription(this TradeOrder order)
        {
            try
            {
                if ((order.Invoice?.TotalInvoiceAmount ?? 0) > order.Payments.Sum(e => e.Amount))
                    return "Ready for payment";
                if (order.OrderStatus == TradeOrderStatusEnum.Draft && order.IsAgreedByCustomer)
                    return "Awaiting Processing";

                return order.OrderStatus.GetDescription();
            }
            catch
            {
                return "Draft";
            }
        }

        public static string GetDescription(this MyOrder order)
        {
            try
            {
                if ((order.Invoice?.TotalInvoiceAmount ?? 0) > order.Payments.Sum(e => e.Amount))
                    return "Ready for payment";
                return order.Status.GetDescription();
            }
            catch
            {
                return "Draft";
            }
        }

        public static DomainUsersAllowed GetDomainUsersAllowed(int domainId)
        {
            var dbContext = new ApplicationDbContext();
            var plan = dbContext.DomainPlans.AsNoTracking().FirstOrDefault(e => e.Domain.Id == domainId && e.IsArchived == false);

            var allowedUserNumber = (plan?.NumberOfExtraUsers ?? 0) + (plan?.Level?.NumberOfUsers ?? 0);

            return new DomainUsersAllowed
            {
                UsersAllowed = allowedUserNumber,
                ActualMembers = plan?.Domain.Users?.Count ?? 0
            };
        }

        public static string TruncateForDisplay(this string input, int maxLength)
        {
            if (input.Length <= maxLength)
                return input;

            string truncatedString = input.Substring(0, maxLength);

            // If the last character is a space, return the truncated string
            if (truncatedString[truncatedString.Length - 1] == ' ')
                return truncatedString + "..."; ;

            // Otherwise, find the last space before the truncation point and truncate there
            int lastSpace = input.Substring(0, maxLength + 1).LastIndexOf(' ');
            if (lastSpace != -1)
                return truncatedString.Substring(0, lastSpace) + "..."; ;

            // If there are no spaces before the truncation point, return the truncated string
            return truncatedString;
        }

        /// <summary>
        /// Return createdByImg and created name
        /// </summary>
        /// <param name="post"></param>
        /// <param name="createdByImg"></param>
        /// <returns></returns>
        public static string GetCommentAvatar(this QbiclePost post, ref string createdByImg, string currentUserId)
        {
            var creatorTheQbcile = post.Topic.Qbicle.GetCreatorTheQbcile();
            //var createdBy = ""; var createdByImg = "";
            var domainId = 0;
            var createdByUser = post.CreatedBy;

            var createdBy = createdByUser.GetFullName(createdByUser.Id == currentUserId ? currentUserId : "");
            createdByImg = createdByUser.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");
            //if bussiness
            if (post.IsCreatorTheCustomer == false)
            {
                if (creatorTheQbcile == QbicleType.B2CQbicle)
                {
                    domainId = (post.Topic.Qbicle as B2CQbicle).Business.Id;
                    var b2BProfiles = new ApplicationDbContext().B2BProfiles.AsNoTracking().Where(s => s.Domain.Id == domainId).FirstOrDefault() ?? new B2BProfile();
                    createdBy = b2BProfiles.BusinessName;
                    createdByImg = b2BProfiles.LogoUri.ToUriString(Enums.FileTypeEnum.Image, "T");
                }
            }
            return createdBy;
        }

        public static bool StringPropertiesEmpty(this object value)
        {
            foreach (PropertyInfo objProp in value.GetType().GetProperties())
            {
                if (objProp.CanRead)
                {
                    object val = objProp.GetValue(value, null);
                    if (val.GetType() == typeof(string))
                    {
                        if ((string)val == "" || val == null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Update catalog pricing based on tax and latest cost events
        /// QBIC-4474 //PART 1: Updating catalog pricing based on tax changes
        /// </summary>
        public static void UpdateCatalogPricingBasedOnTaxEvents(this TraderItem item)
        {
            var dbContext = new ApplicationDbContext();
            using (DbContextTransaction transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    //QBIC-4951 the sales taxes should be the only taxes used in generating a catalog price
                    var colTaxes = item.TaxRates?.Where(e => !e.IsPurchaseTax) ?? new List<TaxRate>();
                    //var colTaxes = item.TaxRates?? new List<TaxRate>();

                    // get the Variants associated with the TraderItem
                    var colVariants = dbContext.PosVariants.Where(e => e.TraderItem.Id == item.Id).ToList();
                    //get the Extras associated with the TraderItem
                    var colExtras = dbContext.PosExtras.Where(e => e.TraderItem.Id == item.Id).ToList();
                    var colAffectedCatalogs = new List<Catalog>();

                    colVariants.ForEach(variant =>
                    {
                        var catalogPrice = variant.Price;//CatalogPrice;
                        catalogPrice.NetPrice = catalogPrice.NetPrice;// (the NetPrice DOES NOT change);

                        //catalogPrice.Taxes is created from colTaxes
                        catalogPrice.Taxes.Clear();//clear all PriceTax
                        colTaxes.ForEach(taxRate =>
                        {
                            //Copied from Qbicles.Web.Migrations.Configuration.Seed_UpdatePrice
                            var staticTaxItem = new TaxRateRules(dbContext).CloneStaticTaxRateById(taxRate.Id);

                            var priceTax = new PriceTax
                            {
                                TaxRate = staticTaxItem,
                                TaxName = taxRate.Name,
                                Amount = catalogPrice.NetPrice * (taxRate.Rate / 100),
                                Rate = taxRate.Rate,
                            };

                            dbContext.Entry(priceTax).State = EntityState.Added;
                            dbContext.TraderPriceTaxes.Add(priceTax);

                            catalogPrice.Taxes.Add(priceTax);
                        });

                        catalogPrice.TotalTaxAmount = catalogPrice.Taxes.Sum(e => e.Amount);//calculate tax amount based on catalogPrice.Taxes;
                        catalogPrice.GrossPrice = catalogPrice.NetPrice + catalogPrice.TotalTaxAmount;
                        catalogPrice.FlaggedForTaxUpdate = true;
                        catalogPrice.TaxUpdateDate = DateTime.UtcNow;

                        dbContext.Entry(catalogPrice).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        if (variant.CategoryItem?.Category?.Menu != null && colAffectedCatalogs.TrueForAll(c => c.Id != variant.CategoryItem.Category.Menu.Id))
                            colAffectedCatalogs.Add(variant.CategoryItem.Category.Menu);//Add Catalog(variant->categoryItem->category->catalog)
                    });

                    colExtras.ForEach(extra =>
                    {
                        var catalogPrice = extra.Price;//CatalogPrice;
                        catalogPrice.NetPrice = catalogPrice.NetPrice;// (the NetPrice DOES NOT change)
                                                                      //catalogPrice.Taxes is created from colTaxes
                        catalogPrice.Taxes.Clear();//clear all PriceTax
                        colTaxes.ForEach(taxRate =>
                        {
                            //Copied from Qbicles.Web.Migrations.Configuration.Seed_UpdatePrice
                            var staticTaxItem = new TaxRateRules(dbContext).CloneStaticTaxRateById(taxRate.Id);

                            var priceTax = new PriceTax
                            {
                                TaxRate = staticTaxItem,
                                TaxName = taxRate.Name,
                                Amount = catalogPrice.NetPrice * (taxRate.Rate / 100),
                                Rate = taxRate.Rate,
                            };

                            dbContext.Entry(priceTax).State = EntityState.Added;
                            dbContext.TraderPriceTaxes.Add(priceTax);

                            catalogPrice.Taxes.Add(priceTax);
                        });
                        catalogPrice.TotalTaxAmount = catalogPrice.Taxes.Sum(e => e.Amount);//calculate tax amount based on catalogPrice.Taxes
                        catalogPrice.GrossPrice = catalogPrice.NetPrice + catalogPrice.TotalTaxAmount;
                        catalogPrice.FlaggedForTaxUpdate = true;
                        catalogPrice.TaxUpdateDate = DateTime.UtcNow;

                        dbContext.Entry(catalogPrice).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        if (extra.CategoryItem?.Category?.Menu != null && colAffectedCatalogs.TrueForAll(c => c.Id != extra.CategoryItem.Category.Menu.Id))
                            colAffectedCatalogs.Add(extra.CategoryItem.Category.Menu);//Add Catalog(extra->categoryItem->category->catalog)
                    });

                    colAffectedCatalogs.ForEach(catalog =>
                    {
                        catalog.FlaggedForTaxUpdate = true;
                        dbContext.SaveChanges();
                    });

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                }
            }
        }

        /// <summary>
        /// Updating catalog pricing based on latest cost changes
        /// QBIC-4474 PART 2: Updating catalog pricing based on latest cost changes
        /// </summary>
        /// <param name="item"></param>
        public static void UpdatingCatalogPricingBasedOnLatestCostChanges(this InventoryDetail inventoryDetail)
        {
            var dbContext = new ApplicationDbContext();
            using (DbContextTransaction transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    //Location of InventoryDetail where the item's latest cost is being updated
                    var updateLocationId = inventoryDetail.Location.Id;
                    //get the Variants associated with the TraderItem where the Catalog containing the variant = updateLocation
                    var colVariants = dbContext.PosVariants.Where(variant => variant.TraderItem.Id == inventoryDetail.Item.Id && variant.CategoryItem.Category.Menu.Location.Id == updateLocationId).ToList();
                    //get the Extras associated with the TraderItem where the Catalog containing the extra = updateLocation
                    var colExtras = dbContext.PosExtras.Where(extra => extra.TraderItem.Id == inventoryDetail.Item.Id && extra.CategoryItem.Category.Menu.Location.Id == updateLocationId).ToList();
                    var colAffectedCatalogs = new List<Catalog>();

                    colVariants.ForEach(variant =>
                    {
                        var catalogPrice = variant.Price;
                        catalogPrice.FlaggedForLatestCostUpdate = true;
                        catalogPrice.LatestCostUpdateDate = DateTime.UtcNow;

                        dbContext.Entry(catalogPrice).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        if (variant.CategoryItem?.Category?.Menu != null && colAffectedCatalogs.TrueForAll(c => c.Id != variant.CategoryItem.Category.Menu.Id))
                            colAffectedCatalogs.Add(variant.CategoryItem.Category.Menu);//.Add Catalog(variant->categoryItem->category->catalog)
                    });
                    colExtras.ForEach(extra =>
                    {
                        var catalogPrice = extra.Price;
                        catalogPrice.FlaggedForLatestCostUpdate = true;
                        catalogPrice.LatestCostUpdateDate = DateTime.UtcNow;

                        dbContext.Entry(catalogPrice).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        if (extra.CategoryItem?.Category?.Menu != null && colAffectedCatalogs.TrueForAll(c => c.Id != extra.CategoryItem.Category.Menu.Id))
                            colAffectedCatalogs.Add(extra.CategoryItem.Category.Menu);//.Add Catalog(extra->categoryItem->category->catalog)
                    });
                    colAffectedCatalogs.ForEach(catalog =>
                    {
                        catalog.FlaggedForLatestCostUpdate = true;
                        dbContext.SaveChanges();
                    });

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                }
            }

        }

        public static string GetLinkedItemId(this object obj)
        {
            Type type = obj.GetType();
            PropertyInfo prop = type.GetProperty("LinkedItemId");

            if (prop == null)
            {
                return Guid.NewGuid().ToString();
            }

            var linkedItemId= prop.GetValue(obj).ToString();
            return string.IsNullOrEmpty(linkedItemId)?Guid.NewGuid().ToString(): linkedItemId;
        }

        public static bool PropertyExists(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }
    }

    public static class Utility
    {
        #region Bytes to size

        public static string BytesToSize(int bytes)
        {
            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
            if (bytes == 0) return "0 Byte";
            var i = Convert.ToInt32(Math.Floor(Math.Log(bytes) / Math.Log(1024)));
            return Math.Round(bytes / Math.Pow(1024, i), 2).ToString() + " " + sizes[i];
        }

        #endregion Bytes to size

        #region Batching, take a certain number of collection and return it each time through the loop

        public static IEnumerable<IEnumerable<T>> Batch<T>(
            this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    yield return YieldBatchElements(enumerator, batchSize - 1);
        }

        private static IEnumerable<T> YieldBatchElements<T>(
            IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (int i = 0; i < batchSize && source.MoveNext(); i++)
                yield return source.Current;
        }

        #endregion Batching, take a certain number of collection and return it each time through the loop

        #region Caculator date

        public static List<CustomDateModel> GetListMonth(DateTime dtCalculator)
        {
            var lst = new List<CustomDateModel>();
            //get max 6 month from dtCalculator
            DateTime dt;
            CustomDateModel CustomDate;
            for (var i = 1; i <= 6; ++i)
            {
                CustomDate = new CustomDateModel();
                dt = dtCalculator.AddMonths(i);
                CustomDate.StartDate = dt;
                CustomDate.Value = dt.Month;
                CustomDate.Name = dt.ToString("MMMM", CultureInfo.CreateSpecificCulture("en"));
                lst.Add(CustomDate);
            }
            return lst;
        }

        public static List<CustomDateModel> GetListDayToTable(DateTime dtCalculator, string dayofweek, DateTime LastOccurenceDate)
        {
            var lst = new List<CustomDateModel>();
            DateTime dt;
            CustomDateModel CustomDate;
            var countDay = Math.Round(LastOccurenceDate.Subtract(dtCalculator).TotalDays);
            // var dtCalculator = new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month+1,1);
            for (var i = 0; i <= countDay; ++i)
            {
                dt = dtCalculator.AddDays(i);
                if (dt.Subtract(LastOccurenceDate).Days <= 0)
                {
                    CustomDate = new CustomDateModel();
                    CustomDate.StartDate = dt;
                    CustomDate.Value = dt.Month;
                    CustomDate.Name = dt.ToString("dddd") + " " + dt.DatetimeToOrdinal();
                    CustomDate.ShortName = dt.ToString("dddd");
                    CustomDate.Date = dt.ToString("dd/MM/yyyy HH:mm");
                    CustomDate.selected = dayofweek.Contains(dt.ToString("dddd"));
                    lst.Add(CustomDate);
                }
                else
                    break;
            }
            return lst;
        }

        public static List<CustomDateModel> GetListWeekToTable(DateTime dtCalculator, string dayofweek, DateTime LastOccurenceDate)
        {
            var lst = new List<CustomDateModel>();
            DateTime dt;
            CustomDateModel CustomDate;
            // var dtCalculator = new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month+1,1);
            var lastDay = dtCalculator.AddMonths(1).AddDays(-1);
            DateTime dtCal = dtCalculator;
            for (var i = 0; i <= lastDay.Day; ++i)
            {
                if (dtCalculator.AddDays(i).DayOfWeek.ToString() == dayofweek)
                {
                    dtCal = dtCalculator.AddDays(i);
                    break;
                }
            }
            var countDay = Math.Round(LastOccurenceDate.Subtract(dtCalculator).TotalDays);
            for (var i = 0; i <= countDay; i++)
            {
                dt = dtCal.AddDays(i * 7);
                if (dt.Subtract(LastOccurenceDate).Days <= 0)
                {
                    CustomDate = new CustomDateModel();
                    CustomDate.StartDate = dt;
                    CustomDate.Value = dt.Month;
                    CustomDate.Name = dt.ToString("dddd") + " " + dt.DatetimeToOrdinal();
                    CustomDate.ShortName = dt.ToString("dddd");
                    CustomDate.Date = dt.ToString("dd/MM/yyyy HH:mm");
                    CustomDate.selected = dayofweek.Contains(dt.ToString("dddd"));
                    lst.Add(CustomDate);
                }
                else
                    break;
            }
            return lst;
        }

        public static List<CustomDateModel> GetListMonthToTable(DateTime dtCalculator, int Pattern, int customDate, string monthselected, DateTime LastOccurenceDate, string formatDatetime)
        {
            var lst = new List<CustomDateModel>();
            DateTime dt;
            CustomDateModel CustomDate;
            for (var i = 0; i < 24; i++)
            {
                dt = dtCalculator.AddMonths(i);
                if (Pattern == 0)
                    dt = new DateTime(dt.Year, dt.Month, 1, dt.Hour, dt.Minute, dt.Second);
                else if (Pattern == 1)
                {
                    dt = new DateTime(dt.Year, dt.Month, 1, dt.Hour, dt.Minute, dt.Second);
                    dt = dt.AddMonths(1).AddDays(-1);
                }
                else
                {
                    if (customDate <= 0)
                        customDate = 1;
                    if (dt.Month == 2 && (dt.Year % 4 == 0 || dt.Year % 400 == 0))
                    {
                        if (customDate > 29)
                            customDate = 29;
                    }
                    else
                    {
                        if (dt.Month == 2 && customDate > 28)
                            customDate = 28;
                    }
                    dt = new DateTime(dt.Year, dt.Month, customDate, dt.Hour, dt.Minute, dt.Second);
                }

                if (dt.Subtract(LastOccurenceDate).Days <= 0)
                {
                    CustomDate = new CustomDateModel();
                    CustomDate.StartDate = dt;
                    CustomDate.Value = dt.Month;
                    CustomDate.Name = dt.ToString("dddd") + " " + dt.DatetimeToOrdinal();
                    CustomDate.ShortName = dt.ToString("MMMM");
                    CustomDate.Date = dt.ToString(formatDatetime);
                    CustomDate.selected = monthselected.Contains(dt.ToString("MMMM"));
                    lst.Add(CustomDate);
                }
                else
                    break;
            }
            return lst;
        }

        public static string ShowRecurring(QbicleRecurrance recurrance, string time)
        {
            if (recurrance == null)
                return "";
            string[] days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            if (recurrance.Type == QbicleRecurrance.RecurranceTypeEnum.Daily)
            {
                return string.Format("{0} {1}", recurrance.Type, time);
            }
            else if (recurrance.Type == QbicleRecurrance.RecurranceTypeEnum.Weekly)
            {
                string day = recurrance.Days;
                if (!string.IsNullOrEmpty(day))
                {
                    int id = day.IndexOf('1');
                    return string.Format("{0} on {1} {2}", recurrance.Type, days[id], time);
                }
                else
                    return "";
            }
            else
            {
                if (recurrance.Pattern == 0)
                {
                    return string.Format("{0} first day {1}", recurrance.Type, time);
                }
                else if (recurrance.Pattern == 1)
                {
                    return string.Format("{0} last day {1}", recurrance.Type, time);
                }
                else
                    return string.Format("{0} on {1} day {2}", recurrance.Type, AddOrdinal(recurrance.MonthDate), time);
            }
        }

        #endregion Caculator date

        #region AddOrdinal

        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";

                case 2:
                    return num + "nd";

                case 3:
                    return num + "rd";

                default:
                    return num + "th";
            }
        }

        //Friday 30th August 2019 12:00pm
        public static string FormatDatetimeOrdinal(this DateTime dateTime)
        {
            return string.Format("{0} {1} {2}", dateTime.ToString("dddd"), AddOrdinal(dateTime.Day), dateTime.ToString("MMMM yyyy h:mm") + dateTime.ToString("tt").ToLower());
        }

        #endregion AddOrdinal

        #region Validate Url

        public static bool ValidateUrl(string url)
        {
            Uri validatedUri;

            if (Uri.TryCreate(url, UriKind.Absolute, out validatedUri)) //.NET URI validation.
            {
                //If true: validatedUri contains a valid Uri. Check for the scheme in addition.
                return (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
            }
            return false;
        }

        #endregion Validate Url

        #region Checking image Path

        public static string CheckImgPath(QbicleFileType fType, VersionedFile file, String size = "")
        {
            var url = "";
            if (fType != null)
                switch (fType.Extension)
                {
                    case "jpg":
                    case "jpeg":
                    case "gif":
                    case "png":
                        if (file != null)
                            url = ConfigManager.ApiGetDocumentUri + file.Uri + (!String.IsNullOrEmpty(size) ? ("&size=" + size) : "");
                        else
                            url = fType.ImgPath;
                        break;

                    default:
                        url = fType.ImgPath;
                        break;
                }
            return url;
        }

        #endregion Checking image Path

        #region Get File Type

        public static string GetFileTypeDescription(string type)
        {
            var sFileType = "";
            switch (type)
            {
                case "doc":
                    sFileType = "Word Doc";
                    break;

                case "docx":
                    sFileType = "Word Doc";
                    break;

                case "gif":
                    sFileType = "GIF image";
                    break;

                case "jpg":
                    sFileType = "JPG image";
                    break;

                case "jpeg":
                    sFileType = "JPEG image";
                    break;

                case "png":
                    sFileType = "PNG image";
                    break;

                case "pdf":
                    sFileType = "PDF Doc";
                    break;

                case "ppt":
                    sFileType = "PPT Doc";
                    break;

                case "pptx":
                    sFileType = "PPTX Doc";
                    break;

                case "xls":
                    sFileType = "XLS Doc";
                    break;

                case "xlsx":
                    sFileType = "XLSX Doc";
                    break;

                case "txt":
                    sFileType = "TXT Doc";
                    break;

                case "zip":
                    sFileType = "ZIP File";
                    break;

                case "mp4":
                    sFileType = "MP4 Video";
                    break;

                case "ogv":
                    sFileType = "OGV Video";
                    break;

                case "webm":
                    sFileType = "WEBM Video";
                    break;

                default:
                    sFileType = type;
                    break;
            }
            return sFileType;
        }

        #endregion Get File Type

        #region Social Files Accept

        public static List<string> SocialFilesAccept()
        {
            return new List<string>
            {
                "jpg",
                "jpeg",
                "png",
                "gif",
                "mp4",
                "webm",
                "ogv"
            };
        }

        #endregion Social Files Accept

        #region Validate file size

        public static bool ValidateFileSize(int fileSize, string extension, ref string message, ref int maxFileSize)
        {
            var _fileinfo = new FileTypeRules(new ApplicationDbContext()).GetFileTypeByExtension(extension);
            if (fileSize > _fileinfo.MaxFileSize)
            {
                maxFileSize = Convert.ToInt32(_fileinfo.MaxFileSize);
                message = "ERROR_MSG_399";
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Validate file size

        #region start and end day of the week

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 - (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(1 * diff).Date;
        }

        #endregion start and end day of the week

        #region the quarter of a date

        public static int GetQuarter(this DateTime date)
        {
            return (date.Month + 2) / 3;
        }

        #endregion the quarter of a date

        #region Income view column

        public static string getIncomeCLName(this DateTime dt, string view)
        {
            if (view == "quarterly")
            {
                return "Q" + dt.GetQuarter();
            }
            else if (view == "yearly")
            {
                return dt.ToString("yyyy");
            }
            else
            {
                return dt.ToString("MMMM yy");
            }
        }

        #endregion Income view column

        #region Invoice TaxRates HTML

        public static string InvoiceTaxRatesHtml(this decimal value, List<Models.Trader.ODS.OrderTax> taxes, CurrencySetting setting, bool isHtml = true)
        {
            if (isHtml)
                return !taxes.Any() ? ((decimal)0).ToDecimalPlace(setting) : ("<ul class=\"unstyled\">" + string.Join(Environment.NewLine, taxes.Select(s => $"<li>{(value * ((s.StaticTaxRate?.Rate ?? 0) / 100)).ToDecimalPlace(setting)} <small> &nbsp; <i>({s.StaticTaxRate?.Name ?? "Tax free"})</i></small></li>")) + $"</ul><input type=\"hidden\" value=\"{string.Join(",", taxes.Select(s => $"{s.StaticTaxRate?.Rate.ToString("N0") ?? "0"}-{s.StaticTaxRate.Name ?? "Tax free"}"))}\" class=\"txt-taxname\">");
            else
                return !taxes.Any() ? ((decimal)0).ToDecimalPlace(setting) : string.Join(Environment.NewLine, taxes.Select(s => $"{(value * (s.StaticTaxRate?.Rate ?? 0) / 100).ToDecimalPlace(setting)} ({s.StaticTaxRate?.Name ?? "Tax free"})"));
        }

        /// <summary>
        /// function return list OrderTax by html template
        /// </summary>
        /// <returns> HTML string</returns>
        public static string HtmlTaxRates(this TraderTransactionItem item, CurrencySetting setting, bool isSale = true)
        {
            if (item.Taxes == null || !item.Taxes.Any())
                return 0.ToString("N" + (int)setting.DecimalPlace);

            decimal taxValue = 0;
            decimal priceIncludeTax = 0;
            decimal priceExcludeTax = 0;

            var ulString = "<ul class=\"unstyled\">";
            var inputString = $"</ul><input type=\"hidden\" value=\"{string.Join(",", item.Taxes.Select(s => $"{(s.StaticTaxRate?.Rate ?? 0):N0}-{s.StaticTaxRate?.Name ?? ""}"))}\" class=\"txt-taxname\">";
            item.Taxes.ForEach(s =>
            {
                taxValue = s.Value * item.Quantity;
                ulString += $"<li>{taxValue.ToString("N" + (int)setting.DecimalPlace)} <small> &nbsp; <i>({s.StaticTaxRate?.Name ?? ""})</i></small></li>";
            });

            ulString += "</ul>";
            return ulString + inputString;
        }

        #endregion Invoice TaxRates HTML

        #region Fix Quote Code

        public static string FixQuoteCode(this string value)
        {
            if (!string.IsNullOrEmpty(value))
                return value.Replace("'", "\\'").Replace("\"", "&#34;");
            return "";
        }

        #endregion Fix Quote Code

        #region MediaTypeNames Image

        public static string ImageType(this string extension)
        {
            switch (extension.ToLower())
            {
                case "jpeg":
                case "jpg":
                    return MediaTypeNames.Image.Jpeg;

                case "gif":
                    return MediaTypeNames.Image.Gif;

                case "png":
                    return "image/png"; ;

                default:
                    return MediaTypeNames.Image.Jpeg;
            }
        }

        #endregion MediaTypeNames Image

        #region Trader Location to address string

        public static string TraderLocationToAddress(this Models.Trader.TraderLocation location)
        {
            StringBuilder builder = new StringBuilder();
            if (location.Address?.Phone != null)
            {
                builder.Append($"{location.Address.Phone},<br/>");
            }
            if (location.Address?.AddressLine1 != null)
            {
                builder.Append($"{location.Address.AddressLine1},<br/>");
            }
            if (location.Address?.AddressLine2 != null)
            {
                builder.Append($"{location.Address.AddressLine2},<br/>");
            }
            if (location.Address?.City != null)
            {
                builder.Append($"{location.Address.City},<br/>");
            }
            if (location.Address?.State != null)
            {
                builder.Append($"{location.Address.State},<br/>");
            }
            if (location.Address?.PostCode != null)
            {
                builder.Append($"{location.Address.PostCode},<br/>");
            }
            if (location.Address?.Country != null)
            {
                builder.Append($"{location.Address.Country.CommonName},<br/>");
            }
            return builder.ToString();
        }

        public static string ToAddressHtml(this TraderAddress address)
        {
            if (address == null) return "";
            var builder = new StringBuilder();
            if (address.AddressLine1 != null)
            {
                builder.Append($"{address.AddressLine1}<br/> ");
            }
            if (address.AddressLine2 != null)
            {
                builder.Append($"{address.AddressLine2}<br/> ");
            }
            if (address.City != null)
            {
                builder.Append($"{address.City}<br/> ");
            }
            if (address.State != null)
            {
                builder.Append($"{address.State}<br/> ");
            }
            if (address.PostCode != null)
            {
                builder.Append($"{address.PostCode}<br/> ");
            }
            if (address.Country != null)
            {
                builder.Append($"{address.Country?.CommonName}");
            }
            return builder.ToString();
        }

        public static string ToAddress(this TraderAddress address)
        {
            if (address == null) return "";
            var builder = new StringBuilder();
            if (address.AddressLine1 != null)
            {
                builder.Append($"{address.AddressLine1}, ");
            }
            if (address.AddressLine2 != null)
            {
                builder.Append($"{address.AddressLine2}, ");
            }
            if (address.City != null)
            {
                builder.Append($"{address.City}, ");
            }
            if (address.State != null)
            {
                builder.Append($"{address.State}, ");
            }
            if (address.PostCode != null)
            {
                builder.Append($"{address.PostCode}, ");
            }
            if (address.Country != null)
            {
                builder.Append($"{address.Country?.CommonName}");
            }
            return builder.ToString();
        }

        public static string ToAddress(this Models.TraderApi.Address address)
        {
            StringBuilder builder = new StringBuilder();
            if (address.AddressLine1 != null)
            {
                builder.Append($"{address.AddressLine1}, ");
            }
            if (address.AddressLine2 != null)
            {
                builder.Append($"{address.AddressLine2}, ");
            }
            if (address.City != null)
            {
                builder.Append($"{address.City}, ");
            }
            if (address.Country != null)
            {
                builder.Append($"{address.Country}");
            }
            return builder.ToString();
        }

        public static ApprovalContactInfo ToContactInfo(this TraderContact contact)
        {
            if (contact == null)
                return new ApprovalContactInfo();

            return new ApprovalContactInfo
            {
                Avatar = contact.AvatarUri.ToUriString(),
                CompanyName = contact.CompanyName,
                ContactReference = contact.ContactRef?.Reference,
                ContactId = contact.Key,
                Email = contact.Email,
                JobTitle = contact.JobTitle,
                Name = contact.Name,
                PhoneNumber = contact.PhoneNumber,
                Address = contact.Address?.ToAddress().Replace(",", Environment.NewLine)
            };
        }
        #endregion
        #region Get InStock by TraderItem

        public static decimal GetInStockByItem(this Models.Trader.TraderItem item, int locationId, Models.Trader.ProductUnit unit)
        {
            var inventoryDetail = item.InventoryDetails.FirstOrDefault(s => s.Location.Id == locationId);
            decimal instock = 0;
            if (inventoryDetail != null && unit != null)
            {
                instock = (inventoryDetail.CurrentInventoryLevel / unit.QuantityOfBaseunit);
            }
            return instock;
        }

        #endregion Get InStock by TraderItem

        #region Check has access the B2C APP

        public static bool CheckHasAccessB2C(int domainId, string currentUserId)
        {
            return new B2C.B2CRules(new ApplicationDbContext()).CheckHasAccessB2C(domainId, currentUserId);
        }

        #endregion Check has access the B2C APP

        #region Check has access the B2B APP

        public static bool CheckHasAccessB2B(int domainId, string currentUserId)
        {
            return new B2C.B2CRules(new ApplicationDbContext()).CheckHasAccessB2B(domainId, currentUserId);
        }

        #endregion Check has access the B2B APP

        #region Get BusinessProfile From Domain Id

        /// <summary>
        ///  Get BusinessProfile From Domain Id
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static Models.B2B.B2BProfile BusinesProfile(this int domainId)
        {
            return new Commerce.CommerceRules(new ApplicationDbContext()).GetB2bProfileByDomainId(domainId);
        }

        #endregion Get BusinessProfile From Domain Id

        #region Calculate Promotion Info

        /// <summary>
        /// This button is only shown if
        ///the Promotion.StartDate is before Now
        ///and
        ///the Promotion.EndDate is after Now
        ///and
        ///if the Promotion.VoucherInfo.VoucherCount != NO_MAX_VOUCHER_COUNT then
        ///the number of Vouchers associated with the Promotion is less than or equal to the number of Vouchers allowed (Promotion.VoucherInfo.VoucherCount)
        ///and
        ///if the number of Vouchers held by the current user for the Promotion<Promotion.VoucherInfo.MaxVoucherCountPerCustomer
        /// </summary>
        /// <param name="currentDate">DateTime</param>
        /// <param name="promotion">LoyaltyPromotion</param>
        /// <returns></returns>
        public static bool CheckAllowClaimNow(this LoyaltyPromotion promotion, string currentUserId, DateTime currentDate)
        {
            if (promotion.StartDate <= currentDate && promotion.EndDate > currentDate)
            {
                if (promotion.VoucherInfo.MaxVoucherCount == VoucherInfo.NO_MAX_VOUCHER_COUNT || (promotion.VoucherInfo.MaxVoucherCount > promotion.Vouchers.Count() && promotion.Vouchers.Count(s => s.ClaimedBy.Id == currentUserId) < promotion.VoucherInfo.MaxVoucherCountPerCustomer))
                    return true;
                else
                    return false;
            }
            return false;
        }

        public static string CalRemainPromotionInfo(this LoyaltyPromotion promotion, string timezone, string dateformat, DateTime currentDate)
        {
            var remainInfo = "";
            if (promotion.StartDate > currentDate)
                remainInfo = $"<span style=\"top:0;\">Promotion starts {promotion.StartDate.ConvertTimeFromUtc(timezone).ToString(dateformat + " HH:mm")}</span>";
            else if (promotion.StartDate <= currentDate && promotion.EndDate > currentDate)
            {
                var ts = (promotion.EndDate - currentDate);
                var totalDays = ts.TotalDays;
                if (promotion.VoucherInfo.MaxVoucherCount != VoucherInfo.NO_MAX_VOUCHER_COUNT && (promotion.VoucherInfo.MaxVoucherCount - promotion.Vouchers.Count()) <= 0)
                    remainInfo = "<span style=\"top:0;\">Limited offer</span>";
                else if (totalDays > 31)
                    remainInfo = "<span style=\"top:0;\">Over 31 days remaining</span>";
                else if (totalDays < 31)
                {
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        DateParseHandling = DateParseHandling.DateTimeOffset
                    };
                    var jsonSDate = JsonConvert.SerializeObject(promotion.EndDate, settings);
                    remainInfo = $"<span class=\"promotion-countdown\" style=\"top:0;\" data-timestamp={jsonSDate}>{(ts.Days > 0 ? ts.Days + "d, " : "")}{ts.Hours}h, {ts.Minutes}m, {ts.Seconds}s remaining</span>";
                }
            }
            else
                remainInfo = "<span style=\"top:0;\">Offer expired</span>";

            return remainInfo;
        }

        public static string CalRemainInfo(this LoyaltyPromotion promotion, string timezone, string dateformat, DateTime currentDate)
        {
            var description = "";
            double remaining = -1;

            if (promotion.StartDate > currentDate)
                description = $"Promotion starts {promotion.StartDate.ConvertTimeFromUtc(timezone).ToString(dateformat + " HH:mm")}";
            else if (promotion.StartDate <= currentDate && promotion.EndDate > currentDate)
            {
                var ts = (promotion.EndDate - currentDate);

                remaining = ts.TotalDays;
                if (promotion.VoucherInfo.MaxVoucherCount != VoucherInfo.NO_MAX_VOUCHER_COUNT && (promotion.VoucherInfo.MaxVoucherCount - promotion.Vouchers.Count()) <= 0)
                    description = "Limited offer";
                else if (remaining == 0)
                    description = $"{ts.Hours}h, {ts.Minutes}m, {ts.Seconds}s remaining";
                else if (remaining < 7)
                    description = $"{Convert.ToInt32(remaining)} days remaining";
                else if (remaining > 7)
                {
                    description = $"{(ts.Days > 0 ? ts.Days + "d, " : "")}{ts.Hours}h, {ts.Minutes}m, {ts.Seconds}s remaining";
                }
            }
            else
                description = "Offer expired";

            return description;
        }

        #endregion Calculate Promotion Info

        public static UserSetting ToUserSetting(this ApplicationUser user)
        {
            return new UserSetting
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                DisplayName = user.GetFullName(),
                ProfilePic = user.ProfilePic ?? ConfigManager.DefaultUserUrlGuid,
                DateFormat = (string.IsNullOrEmpty(user.DateFormat) || (user.DateFormat != "dd/MM/yyyy" && user.DateFormat != "MM/dd/yyyy")) ? "dd/MM/yyyy" : user.DateFormat,
                TimeFormat = string.IsNullOrEmpty(user.TimeFormat) ? "HH:mm" : user.TimeFormat,
                Timezone = user.Timezone ?? ConfigurationManager.AppSettings["Timezone"]
            };
        }

        public static bool CheckBusinessUserRole(int currentDomainId, string currentUserId)
        {
            return new ApplicationDbContext().DomainRole.Any(p => p.Domain.Id == currentDomainId && p.Users.Any(u => u.Id == currentUserId) && p.Name == FixedRoles.QbiclesBusinessRole);
        }

        public static DataTablesRequest ToIDataTablesRequest(this IDataTablesRequest request)
        {
            return new DataTablesRequest
            {
                Columns = request.Columns,
                Draw = request.Draw,
                Length = request.Length,
                Search = request.Search,
                Start = request.Start,
            };
        }
    }

    /// <summary>
    /// Provides atomic operations for variables that are shared by multiple threads
    /// </summary>
    public static class MultipleThreads
    {
        private static object _posOrderInstance;

        public static object PosOrderInstance
        {
            get
            {
                if (_posOrderInstance == null)
                    Interlocked.CompareExchange(ref _posOrderInstance, new object(), null);
                return _posOrderInstance;
            }
        }

        private static object _deliveryUpdateInstance;

        public static object DeliveryUpdateInstance
        {
            get
            {
                if (_deliveryUpdateInstance == null)
                    Interlocked.CompareExchange(ref _deliveryUpdateInstance, new object(), null);
                return _deliveryUpdateInstance;
            }
        }
        public static string GetRequestStatusEnumColor(this RequestStatusEnum status)
        {
            try
            {
                switch (status)
                {
                    case RequestStatusEnum.Pending:
                        return StatusLabelStyle.WarningColor;
                    case RequestStatusEnum.Reviewed:
                        return StatusLabelStyle.WarningColor;
                    case RequestStatusEnum.Approved:
                        return StatusLabelStyle.SuccessColor;
                    case RequestStatusEnum.Denied:
                        return StatusLabelStyle.DangerColor;
                    case RequestStatusEnum.Discarded:
                        return StatusLabelStyle.DangerColor;
                    default:
                        return StatusLabelStyle.PrimaryColor;
                }
            }
            catch
            {
                return StatusLabelStyle.SecondaryColor;
            }
        }

    }
    public static class CurrencyUserConfiguration
    {
        public static string ToCurrencySymbol(this decimal value, CurrencySetting setting)
        {
            if (setting.SymbolDisplay == CurrencySetting.SymbolDisplayEnum.Prefixed)
                return $"{setting.CurrencySymbol}{value.ToString("N" + (int)setting.DecimalPlace)}";
            else
                return $"{value.ToString("N" + (int)setting.DecimalPlace)}{setting.CurrencySymbol}";
        }

        public static string ToCurrencySymbol(this decimal? value, CurrencySetting setting)
        {
            var _value = value.HasValue ? value.Value : 0;
            if (setting.SymbolDisplay == CurrencySetting.SymbolDisplayEnum.Prefixed)
                return $"{setting.CurrencySymbol}{_value.ToString("N" + (int)setting.DecimalPlace)}";
            else
                return $"{_value.ToString("N" + (int)setting.DecimalPlace)}{setting.CurrencySymbol}";
        }

        public static string ToDecimalPlace(this decimal value, CurrencySetting setting)
        {
            return $"{value.ToString("N" + (int)setting.DecimalPlace)}";
        }

        public static string ToDecimalPlace(this decimal? value, CurrencySetting setting)
        {
            if (value == null) return "";
            var _value = value.HasValue ? value.Value : 0;
            return $"{_value.ToString("N" + (int)setting.DecimalPlace)}";
        }

        public static string ToDecimalPlace(this decimal value, int decimalPlace)
        {
            return $"{value.ToString("N" + decimalPlace)}";
        }

        public static string ToInputNumberFormat(this decimal value, CurrencySetting setting)
        {
            return $"{value.ToString("F" + (int)setting.DecimalPlace)}";
        }

        public static string ToInputNumberFormat(this decimal? value, CurrencySetting setting)
        {
            var _value = value.HasValue ? value.Value : 0;
            return $"{_value.ToString("F" + (int)setting.DecimalPlace)}";
        }

        public static string ToCurrencyWithoutSymbol(this decimal value, CurrencySetting setting)
        {
            return $"{value.ToString("N" + (int)setting.DecimalPlace)}";
        }
    }
}