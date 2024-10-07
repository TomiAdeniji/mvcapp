using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SystemDomain
{
    [Table("qb_systemtokens")]
    public class SystemToken
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public DateTime ExpireTime { get; set; }
    }
}
