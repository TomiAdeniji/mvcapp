using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models
{
    /// <summary>
    /// Model for an user profile.
    /// 
    /// </summary>
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public virtual List<QbicleProfile> QbicleProfiles { get; set; } = new List<QbicleProfile>();
    }
}
