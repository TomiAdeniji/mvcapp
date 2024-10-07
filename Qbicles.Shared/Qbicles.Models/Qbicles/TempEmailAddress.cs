using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models
{
    [Table("TempEmailAddress")]
    public class TempEmailAddress
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PIN { get; set; }
        [Required]
        public DateTime ExpiryTime { get; set; }
        [Required]
        public DateTime SendingTime { get; set; }
        /// <summary>
        /// Count times per day
        /// </summary>
        public int CountSentPerDay { get; set; } = 0;
        public int CountVerifyFail { get; set; } = 0;
        [Required]
        public virtual ApplicationUser User { get; set; }
    }
}
