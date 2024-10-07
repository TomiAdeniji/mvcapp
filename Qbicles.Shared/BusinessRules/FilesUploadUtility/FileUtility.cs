using System.Collections.Generic;

namespace Qbicles.BusinessRules.FilesUploadUtility
{
    /// <summary>
    /// File name and extension
    /// </summary>
    public class FileUtility
    {
        public string fileName { get; set; }
        public string fileExtension { get; set; }
    }
    /// <summary>
    /// sheet id,name
    /// </summary>
    public class SheetUtility
    {
        public string sheetId { get; set; }
        public string sheetName { get; set; }
    }

    public class transactionStructure
    {
        public string colKey { get; set; }
        public string colValue { get; set; }
    }

    public class transactionCompare
    {
        public string Date { get; set; }
        public string Description { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string Balance { get; set; }
    }
    public static class hardColumn
    {
        /// <summary>
        /// fix columns is require have selected
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> headColRequireDebit()
        {
            return new HashSet<string> {
                HelperClass.Date,
                HelperClass.Description,
                HelperClass.Debit
            };
        }

        public static HashSet<string> headColRequireCredit()
        {
            return new HashSet<string> {
                HelperClass.Date,
                HelperClass.Description,
                HelperClass.Credit
            };
        }

        public static HashSet<string> headColRequire { get; set; }
    }

};
