using Qbicles.Models.Bookkeeping;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_cashaccount")]
    public class TraderCashAccount
    {
        public int Id { get; set; }

        public string Name { get; set;}

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual QbicleDomain Domain { get; set; }

        public virtual BKAccount AssociatedBKAccount { get; set; }

        [Required(ErrorMessage = "Item image is required")]
        [DataType(DataType.Text)]
        public string ImageUri { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public BankmateAccountType BankmateType { get; set; } = BankmateAccountType.NotInBankMate;
    }
    public enum BankmateAccountType
    {
        NotInBankMate = 0,
        Domain = 1,
        ExternalBank = 2,
        Driver = 3
    }
}
