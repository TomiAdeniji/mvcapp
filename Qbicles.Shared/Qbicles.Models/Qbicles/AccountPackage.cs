using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Qbicles.Models
{
    [Table("qb_AccountPackage")]
    public class AccountPackage
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string AccessLevel { get; set; }
        [Required]
        public decimal Cost { get; set; }

        public int? NumberOfDomains { get; set; }

        public int? NumberOfUsers { get; set; }

        public int? NumberOfQbicles { get; set; }

        public int? NumberOfGuests { get; set; }
        [Required]
        public PerTimeEnum PerTimes { get; set; }

        public enum PerTimeEnum
        {
            [Description("Free")]
            Free = 1,
            [Description("Month")]
            Month = 2,
            [Description("Year")]
            Year = 3,
        }
        public AccountPackage()
        {
            this.PerTimes = PerTimeEnum.Free;
        }

        public enum LimitedTypeEnum
        {
            [Description("Packge Ready")]
            PackgeReady = 1,
            [Description("Qbicles Limited")]
            QbiclesLimited = 2,
            [Description("Members Limited")]
            MembersLimited = 3,
            [Description("Guests Limited")]
            GuestsLimited = 4
        }
        public enum PackgeTypeEnum
        {
            [Description("Qbicles And Guests")]
            QbiclesAndGuest = 1,
            [Description("Members")]
            Members = 2,
            [Description("Activity Guests")]
            ActivityGuests = 3
        }
    }
}
