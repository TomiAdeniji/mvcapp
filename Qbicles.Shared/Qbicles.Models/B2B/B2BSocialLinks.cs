using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    [Table("b2b_socialLinks")]
    public class B2BSocialLink
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public SocialTypeEnum Type { get; set; }
        [Required]
        public virtual B2BProfile B2BProfile { get; set; }
    }
    public enum SocialTypeEnum
    {
        Facebook=0,
        Instagram=1,
        Twitter=2,
        Youtube=3,
        LinkedIn=4
    }
}
