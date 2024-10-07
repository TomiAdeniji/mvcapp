using Qbicles.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_BusinessDomainLevel")]
    public class BusinessDomainLevel : DataModelBase
    {
        public decimal? Cost { get; set; }
        public decimal? CostPerAdditionalUser { get; set; }
        public string Name { get; set; }
        public int NumberOfUsers { get; set; }
        public BusinessDomainLevelEnum Level { get; set; }
        public string Currency { get; set; } = "NGN";

        [Column(TypeName = "bit")]
        public bool IsVisible { set; get; }
        public decimal? VerifyAmount { set; get; }
        public int? NumberOfFreeTrialDays { get; set; }
        public string Description { get; set; }
    }

    public enum BusinessDomainLevelEnum
    {
        [Description("Free package")]
        Free = 1,
        [Description("Starter package")]
        Starter = 2,
        [Description("Standard package")]
        Standard = 3,
        [Description("Expert package")]
        Expert = 4,
        [Description("Existing package")]
        Existing = 5,
    }
}
