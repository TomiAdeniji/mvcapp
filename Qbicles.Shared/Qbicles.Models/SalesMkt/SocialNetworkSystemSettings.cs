using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_SocialNetworkSystemSettings")]
    public class SocialNetworkSystemSettings
    {
        public int Id { get; set; }
        ///Social network facebook key
        public string FacebookClientId { get; set; }

        public string FacebookClientSecret { get; set; }

        ///Social network twitter key
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterUserAccessToken { get; set; }
        public string TwitterUserAccessSecret { get; set; }
    }
}
