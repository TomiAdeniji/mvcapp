using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;

namespace Qbicles.Models.Trader
{

    [Table("trad_reference")]
    public class TraderReference : DataModelBase
    {
        //Use is foreign key with QbicleDomain model
        public int Domain_Id { get; set; }
        /// <summary>
        /// The Domain with which the Reference is associated
        /// </summary>
        [Required]
        [ForeignKey("Domain_Id")]
        public virtual QbicleDomain Domain { get; set; }



        /// <summary>
        /// The FUllRef is constructed from the other parts of the class as follows
        /// 
        /// <Prefix><Delimeter><NumericPart.ToString><Delimeter><Suffix>
        /// </summary>
        [StringLength(50)]
        public string FullRef { get; set; }


        public string Prefix { get; set; }


        public string Suffix { get; set; }


        public string Delimeter { get; set; }


        public int NumericPart { get; set; }


        public TraderReferenceType Type { get; set; }

    }

    public enum TraderReferenceType
    {
        Sale = 1,
        SalesOrder = 2,
        Purchase = 3,
        PurchaseOrder = 4,
        Invoice = 5,
        Bill = 6,
        Allocation = 7,
        CreditNote = 8,
        DebitNote = 9,
        SaleReturn = 10,
        ManuJob = 11,
        Transfer = 12,
        Reorder = 13,
        Order = 14,
        Payment = 15,
        Delivery = 16,
        AlertGroup = 17,
        AlertReport = 18,
    }
}
