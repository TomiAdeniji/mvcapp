using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    /// <summary>
    /// This class represents an EXPENSE ReportEntry that is associated with a Revenue ReportEntry
    /// for the purposes of displaying an Inline Summary Total.
    /// The base class provides the database table for this class
    /// </summary>
    [Table("bk_InlineReportEntry")]
    public class InlineReportEntry
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the inline title for the sub total
        /// </summary>
        public string SubTotalTitle { get; set; }



        /// <summary>
        /// This is the IncomeStatementReportEntry for the Expense
        /// </summary>
        public virtual IncomeStatementReportEntry ExpenseReportEntry { get; set; }


        /// <summary>
        /// This is the ReportEntry with which this inline report entry is associted.
        /// It MUST be a ReportEntry that refers to a Revenue SubGroup
        /// </summary>
        public virtual IncomeStatementReportEntry RevenueReportEntry { get; set; }
    }
}
