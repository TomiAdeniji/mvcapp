using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    /// <summary>
    /// This class represents each of the top level nodes associated with the Chart of Accounts branches Revenue & Expense
    /// </summary>

    [Table("bk_IncomeStatementReportEntry")]
    public class IncomeStatementReportEntry
    {

        public int Id { get; set; }


        /// <summary>
        /// This property points to the element from the ChartOfAccounts to which this entry refers         
        /// </summary>
        public virtual CoANode CoANode { get; set;}


        /// <summary>
        /// If this ReportEntry is associated with Revenue
        /// (i.e. this.TopGroup.Parent.AccountType = Revenue)
        /// then it is POSSIBLE that the ReportEntry will have an InlineReportEntry that refers to an associated Expenses ReportEntry
        /// </summary>
        public virtual InlineReportEntry InlineReportEntry { get ; set; }
    }



}
