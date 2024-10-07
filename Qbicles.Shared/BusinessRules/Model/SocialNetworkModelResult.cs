
namespace Qbicles.BusinessRules.Model
{
    public class FacebookModelResult
    {
        public dynamic pages { get; set; }
        public dynamic groups { get; set; }
        public string access_token { get; set; }
    }

    public class AccountSocialNetwork
    {
        public long SocialNetworkId { get; set; }
        public long NetworkId { get; set; }
        public string AccountName { get; set; }
        public string AvatarUrl { get; set; }
        public string NetworkType { get; set; }
        public bool? IsDisabled { get; set; }
    }

}
