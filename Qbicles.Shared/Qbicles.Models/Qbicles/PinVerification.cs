using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Qbicles
{
    [Table("qb_pinverification")]
    public class PinVerification
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Object type UserVerification encrypt
        /// </summary>
        [Required]
        public string UserVerification { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(6)]
        public string PIN { get; set; }
        /// <summary>
        /// Encrypt string, decrypt when use
        /// </summary>
        [Required]
        public string CallbackUrl { get; set; }
    }
}
