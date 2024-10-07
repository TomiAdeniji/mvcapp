using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Qbicles.Web.Models
{
    public class InstargramValueCollection
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
        public string redirect_uri { get; set; }
        public string code { get; set; }
    }

    public class InstargramDataToken
    {
        public string access_token { get; set; }
        public InstargramUser user { get; set; }
    }

    public class InstargramUser
    {
        public long id { get; set; }
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string full_name { get; set; }
    }

    public class AuthTwitter
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string UserAccessToken { get; set; }
        public string UserAccessSecret { get; set; }
    }

    public class AuthFacebook
    {
        public int FacebookType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}