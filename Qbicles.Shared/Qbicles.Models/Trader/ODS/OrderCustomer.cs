using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_OrderCustomer")]
    public class OrderCustomer
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public bool IsDefaultWalkinCustomer { get; set; } = false;
        public string CustomerName { get; set; }
        public string CustomerRef { get; set; }
        public virtual TraderAddress FullAddress { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
