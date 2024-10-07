using IdentityModel.Client;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Provider
{
    public class JwtProvider
    {
        private static string _tokenUri;

        //default constructor
        public JwtProvider() { }

        public static JwtProvider Create(string tokenUri)
        {
            _tokenUri = tokenUri;
            return new JwtProvider();
        }

        /// <summary>
        /// Get access token with grant_type = Password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="clientId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public async Task<string> GetTokenAsync(string username, string password, string clientId, string scope)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_tokenUri?.TrimEnd('/') + "/connect/token");
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("username", username),
                        new KeyValuePair<string, string>("password", password),
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("scope", scope),
                        new KeyValuePair<string, string>("client_id", clientId),
                    });
                var response = await client.PostAsync(string.Empty, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    //return null if unauthenticated
                    return null;
                }
            }
        }
        public async Task<string> GetServiceTokenAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = _tokenUri?.TrimEnd('/') + "/connect/token",
                        ClientId = ConfigManager.ClientId,
                        ClientSecret = ConfigManager.ClientSecret,
                        Scope = ConfigManager.ClientScope
                    });

                    if (tokenResponse.Exception != null || tokenResponse.IsError)
                    {
                        LogManager.Error(MethodBase.GetCurrentMethod(), tokenResponse.Exception, null, $"error: ${tokenResponse.Error}", ConfigManager.ClientId, ConfigManager.ClientSecret, ConfigManager.ClientScope);
                        return null;
                    }
                    //Validate and store token into Database
                    var validtoken = ValidateToken(tokenResponse?.AccessToken ?? "", true);
                    if (!validtoken.IsTokenValid)
                        return "";
                    else
                        return tokenResponse.AccessToken;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, ConfigManager.ClientId, ConfigManager.ClientSecret, ConfigManager.ClientScope);
                return null;
            }

        }

        public JObject DecodePayload(string token)
        {
            var parts = token.Split('.');
            var payload = parts[1];

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            return JObject.Parse(payloadJson);
        }

        public ClaimsIdentity CreateIdentity(bool isAuthenticated, string userName, dynamic payload, string token)
        {
            //decode the payload from token
            //in order to create a claim            
            string userId = payload.sub;

            var jwtIdentity = new ClaimsIdentity(new JwtIdentity(
                isAuthenticated, userName, DefaultAuthenticationTypes.ApplicationCookie
                    ));
            //add user id
            jwtIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            //add token
            jwtIdentity.AddClaim(new Claim(SystemDomainConst.TOKENAUTH, token));

            return jwtIdentity;
        }

        private byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
        public PosRequest ValidateTokenOnly(string iToken)
        {
            var result = new PosRequest();
            try
            {
                X509SecurityKey security = new X509SecurityKey(LoadCertificate());

                SecurityToken securityToken;

                var handler = new JwtSecurityTokenHandler();


                var token = handler.ReadToken(iToken) as JwtSecurityToken;
                var tokenExpiryDate = token.ValidTo;
                if (tokenExpiryDate == DateTime.MinValue)
                    return new PosRequest
                    {
                        Status = HttpStatusCode.Unauthorized,
                    };

                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = security,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true
                };

                handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(iToken, validationParameters, out securityToken);

                var tokenDecode = (JwtSecurityToken)securityToken;

                var client = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("client_id"));
                if (client == null)
                {
                    result.Status = HttpStatusCode.Unauthorized;
                }
                else
                {
                    if (client.Value != "web_app" && client.Value != "micro_app" && client.Value != ConfigManager.ClientId)
                    {
                        result.Status = HttpStatusCode.NotAcceptable;
                        return result;
                    }

                    result.Status = HttpStatusCode.OK;
                    result.Message = ResourcesManager._L("SUCCESSFULLY");
                }



            }
            catch
            {
                result.Status = HttpStatusCode.Forbidden;
            }

            return result;
        }
        public PosRequest ValidateToken(string tokenValid, bool storeTokenDb = false)
        {
            var result = new PosRequest
            {
                IsTokenValid = true
            };
            try
            {
                X509SecurityKey security = new X509SecurityKey(LoadCertificate());

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
                    if (client.Value != "web_app" && client.Value != ConfigManager.ClientId)
                    {
                        result.IsTokenValid = false;
                        result.Status = HttpStatusCode.NotAcceptable;
                        result.Message = "Clients NotAcceptable";
                        return result;
                    }
                    if (storeTokenDb)
                    {
                        //Decrease one minute to ensure there is no token expiration while on call
                        StoreTokenDatabase(tokenValid, tokenExpiryDate.AddMinutes(-1));
                    }
                    //result.ClientId = ClientIdType.MicroClient;
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
        public bool StoreTokenDatabase(string token, DateTime expireTime)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, token, expireTime);
                using (var dbContext = new ApplicationDbContext())
                {
                    SystemToken systemToken = dbContext.SystemTokens.FirstOrDefault();
                    if (systemToken == null)
                    {
                        systemToken = new SystemToken
                        {
                            Token = token,
                            ExpireTime = expireTime
                        };
                        dbContext.SystemTokens.Add(systemToken);
                    }
                    else
                    {
                        systemToken.Token = token;
                        systemToken.ExpireTime = expireTime;
                    }
                    dbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, token, expireTime);
                return false;
            }
        }
        public static SystemToken GetSystemToken()
        {
            try
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    return dbContext.SystemTokens.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }

        public static X509Certificate2 LoadCertificate()
        {
            var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var fileName = Path.Combine(startupPath, "Resources", "certificate.cer");
            if (!File.Exists(fileName))
                return null;
            return new X509Certificate2(fileName, "trader");
        }
    }


    public class JwtIdentity : IIdentity
    {
        private bool _isAuthenticated;
        private string _name;
        private string _authenticationType;

        public JwtIdentity() { }
        public JwtIdentity(bool isAuthenticated, string name, string authenticationType)
        {
            _isAuthenticated = isAuthenticated;
            _name = name;
            _authenticationType = authenticationType;
        }
        public string AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }

    public class TokenResultDto
    {
        public string id_token { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public SignInStatus status { get; set; }
    }
}