using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Returns
{
    [Table("trad_returnlog")]
    public class ReturnLog
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the Return with which this ReturnLog is associated 
        /// </summary>
        [Required]
        public virtual TraderReturn ParentReturn {get; set;}

        /// <summary>
        /// The location at which this return takes place
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// The Sale with which the Return is associated
        /// </summary>
        [Required]
        public virtual TraderSale Sale { get; set; }


        /// <summary>
        /// The collection of Return Items associated with this return
        /// Each ReturnItem is associated with one TraderTransactionItem from the associated Sale
        /// </summary>
        public virtual List<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();


        /// <summary>
        /// The UTC date and time on which the Return LOG was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The Application user who created the Return LOG
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        /// <summary>
        /// The date on which the Return was last updated
        /// </summary>
        [Required]
        public DateTime LastUpdatedDate { get; set; }


        /// <summary>
        /// The Application user who last updated the return
        /// </summary>
        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// The WorkGroup with which the Return is associated
        /// </summary>
        [Required]
        public virtual WorkGroup Workgroup { get; set; }


        /// <summary>
        /// This is the status of the Return
        /// </summary>
        [Required]
        public TraderReturnStatusEnum Status { get; set; }
    }
}
