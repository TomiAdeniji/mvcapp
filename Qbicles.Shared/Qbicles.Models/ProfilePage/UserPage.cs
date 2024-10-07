using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.ProfilePage
{

    public class UserPage : ProfilePage
    {
        public UserPage()
        {
            this.Type = ProfilePageType.User;
        }
        [Required]
        public virtual ApplicationUser User { get; set; }

        public UserPageStatusEnum Status { get; set; }
    }
    public enum UserPageStatusEnum
    {
        [Description("Draft")]
        IsDraft = 0,
        [Description("Active")]
        IsActive = 1,
        [Description("Inactive")]
        IsInActive = 2
    }


}
