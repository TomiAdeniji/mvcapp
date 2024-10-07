using System;
using System.Linq;
using System.Net;
using Qbicles.Models.TraderApi;
using System.Net.Http.Headers;
using Qbicles.BusinessRules.Helper;
using System.IdentityModel.Tokens;
using Qbicles.BusinessRules.Provider;

namespace Qbicles.MicroApi.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public static class ValidateAccessToken
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenValid"></param>
        /// <returns></returns>
        public static PosRequest ValidateToken(string tokenValid)
        {
            var result = new PosRequest
            {
                IsTokenValid = true
            };
            try
            {
                X509SecurityKey security = new X509SecurityKey(JwtProvider.LoadCertificate());

                SecurityToken securityToken;

                var handler = new JwtSecurityTokenHandler();


                var token = handler.ReadToken(tokenValid) as JwtSecurityToken;
                var tokenExpiryDate = token.ValidTo;
                if (tokenExpiryDate == DateTime.MinValue)
                    return new PosRequest
                    {
                        IsTokenValid = false,
                        Status = HttpStatusCode.Unauthorized,
                        Message = "Unauthorized"
                    };

                if (tokenExpiryDate < DateTime.UtcNow) return new PosRequest
                {
                    IsTokenValid = false,
                    Status = HttpStatusCode.Unauthorized,
                    Message = $"Token expired on: {tokenExpiryDate}"
                };

                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = security,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true
                };

                handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(tokenValid, validationParameters, out securityToken);

                var tokenDecode = (JwtSecurityToken)securityToken;

                var client = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("client_id"));
                if (client == null)
                {
                    result.Status = HttpStatusCode.Unauthorized;
                    result.Message = HttpStatusCode.Unauthorized.ToString();
                }
                else
                {
                    if (client.Value != "micro_app")
                    {
                        result.IsTokenValid = false;
                        result.Status = HttpStatusCode.NotAcceptable;
                        result.Message = "Clients NotAcceptable";
                        return result;
                    }    

                    result.ClientId = ClientIdType.MicroClient;
                    result.IsTokenValid = true;
                    result.Status = HttpStatusCode.OK;
                    result.Message = ResourcesManager._L("SUCCESSFULLY");
                    result.UserId = tokenDecode.Subject;
                    //TODO: provide result extend here
                    //result.xxx = xxx;
                }



            }
            catch (Exception e)
            {
                result.IsTokenValid = false;
                result.Message = e.Message;
                result.Status = HttpStatusCode.Forbidden;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static PosRequest ValidateHeader(HttpRequestHeaders header)
        {

            if (header.Authorization == null || !header.Authorization.Scheme.Equals("Bearer", StringComparison.CurrentCultureIgnoreCase))
            {
                return new PosRequest
                {
                    AccessToken = header.Authorization?.Parameter ?? "",
                    Header = header.ToJson(),
                    IsTokenValid = false,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_NOTACCEPTABLE", "Authorization type")
                };
            }
            var valid = ValidateToken(header.Authorization.Parameter);
            valid.AccessToken = header.Authorization?.Parameter ?? "";
            valid.Header = header.ToJson();
            return valid;
        }

    }

}