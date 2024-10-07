using Qbicles.Models.Trader;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qbicles.Models.Catalogs;

namespace Qbicles.Models.B2B
{
    [Table("b2b_tradingitems")]
    public class B2BTradingItem
    {
        public int Id { get; set; }

        [Required]
        public virtual TradeOrderB2B RelatedOrder { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy {get;set;}

        [Required]
        public DateTime CreatedDate { get; set; }


        public virtual TraderItem ConsumerDomainItem { get; set; }

        public virtual ProductUnit ConsumerUnit { get; set; }


        /// <summary>
        /// This is the item in the provider Product Menu that is being sold
        /// It identifies the TraderItem and Unit  in the Provider's Domain
        /// </summary>
        public virtual Variant Variant { get; set; }
    }
}
