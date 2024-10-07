using Qbicles.Models;
namespace Qbicles.BusinessRules.Helper
{
    public static class QbiclesTokenHelper
    {
        public static string Create(AuthorizeModel token)
        {
            return token.ToJson().Encrypt();
        }
        public static AuthorizeModel Get(string token)
        {
            var authorize = token.Decrypt().ParseAs<AuthorizeModel>();
            authorize.PasswordHash = authorize.PasswordHash.Decrypt();
            return authorize;
        }
    }
}
