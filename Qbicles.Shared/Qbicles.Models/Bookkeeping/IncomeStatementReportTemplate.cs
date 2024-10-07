using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{

    /// <summary>
    /// This class represents the template for the Income Statement report within a Domain.
    /// There can be only one income statement per Domain.
    /// </summary>
    [Table("bk_IncomeStatementReportTemplate")]
    public class IncomeStatementReportTemplate
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the Domain with which this template is associated.
        /// There is only ONE IncomeStatementReportTemplate per Domain
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// This is a collection of the ReportEntries that are associated with the IncomeStatement template
        /// Each of the Top SubGroups from teh chart of Accounts Revenue and Expense branches has an item in the collection.
        /// </summary>
        public virtual List<IncomeStatementReportEntry> ReportEntries { get; set; } = new List<IncomeStatementReportEntry>();





    }
}
