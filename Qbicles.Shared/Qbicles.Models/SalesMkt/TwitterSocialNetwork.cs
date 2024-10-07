using Qbicles.Models.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_twitterccount")]
    public class TwitterAccount : SocialNetworkAccount
    {

        /// <summary>
        /// This is the Social Network ID required to connect to the social network
        /// </summary>
        [Required]
        public long TwitterId { get; set; }

        /// <summary>
        /// This is the user name required to connect to the social network
        /// </summary>
        [Required]
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        /// <summary>
        /// This is the Token required to connect to the social network
        /// </summary>
        [Required]
        public string Token { get; set; }
        [Required]
        public string TokenSecret { get; set; }
    }
}
