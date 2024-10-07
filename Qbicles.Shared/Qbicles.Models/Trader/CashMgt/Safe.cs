using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.CashMgt
{
    [Table("trad_safe")]
    public class Safe
    {
        public int Id { set; get; }

        [Required]
        public string Name { get; set; }

        public virtual TraderCashAccount CashAndBankAccount { get; set; }

        public virtual List<Till> Tills { get; set; }

        //[Required]
        public virtual TraderLocation Location { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }

        public DateTime LastUpdatedDate { get; set; }
    }
}
