using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Bookkeeping;

namespace Qbicles.Models.Trader
{
    [Table("bk_taxrate")]
    public class TaxRate
    {
        public int Id { get; set; }

        public decimal Rate { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// This property indicates whether this tax rate is used for a Purchase (TRUE) or Sale (FALSE) 
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsPurchaseTax { get; set; }


        /// <summary>
        /// This property indicates whether or not the Tax, calculated based on this tax rate, 
        /// is accounted for separately in the case of a Journal Entry
        /// i.e. is a separate transaction created for the Tax when it is included in a Journal Entry
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsAccounted { get; set; }


        /// <summary>
        /// This property is only of importance if
        /// * the IsAccounted is true 
        /// If the value is true then it means that the Tax value calculated is added to the
        /// associated Account  as a CREDIT,
        /// if false it is added as a DEBIT
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsCreditToTaxAccount { get; set; }

        public virtual BKAccount AssociatedAccount { get; set; }

        public virtual QbicleDomain Domain { get; set; }

        public virtual List<TraderItem> TraderItems { get; set; }
        public bool IsStatic { get; set; } = false;
    } 
}
