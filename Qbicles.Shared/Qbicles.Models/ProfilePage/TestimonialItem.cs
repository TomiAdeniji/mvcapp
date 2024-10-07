using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.ProfilePage
{
    [Table("profile_testimonialitem")]
    public class TestimonialItem
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string PersonName { get; set; }

        [StringLength(250)]
        public string FurtherInfo { get; set; }
        public string Content { get; set; }
        [StringLength(150)]
        public string AvatarUri { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        [Required]
        [JsonIgnore]
        public virtual TestimonialList Testimonial { get; set; }
    }
}
