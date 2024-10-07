using CleanBooksData;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace Qbicles.Models.Bookkeeping
{
    //Bookkeeping account
    //This account is not to be confused with a CleanBooks account or the Subscription account
    [Table("bk_account")]
    public class BKAccount : CoANode
    {
        public BKAccount()
        {
            this.NodeType = BKCoANodeTypeEnum.Account;
        }

        public string Code { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal InitialCredit { get; set; }
        public decimal InitialDebit { get; set; }

        public virtual List<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

        public virtual List<BKTransaction> Transactions { get; set; } = new List<BKTransaction>();

        public virtual List<QbicleMedia> AssociatedFiles { get; set; } = new List<QbicleMedia>();

        public virtual ApprovalReq Approval { get; set; }

        public virtual BKWorkGroup WorkGroup { get; set; }

        public virtual List<Account> CBAccounts { get; set; } = new List<Account>();

    }
}