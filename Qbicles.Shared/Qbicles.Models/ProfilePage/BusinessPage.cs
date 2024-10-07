using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.ProfilePage
{

    public class BusinessPage: ProfilePage
    {
        public BusinessPage()
        {
            this.Type = ProfilePageType.Business;
        }
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        public BusinessPageStatusEnum Status { get; set; }
    }
    public enum BusinessPageStatusEnum
    {
        [Description("Draft")]
        IsDraft = 0,
        [Description("Active")]
        IsActive = 1,
        [Description("Inactive")]
        IsInActive = 2
    }


}
