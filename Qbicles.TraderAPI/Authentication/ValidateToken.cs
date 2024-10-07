using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Provider;
using Qbicles.Models.TraderApi;
using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Qbicles.TraderAPI.Authentication
{
    public static class ValidateAccessToken
    {
        public static void ValidateHeader(HttpRequestMessage request)
        {
            var header = request.Headers;
            if (header.Authorization == null || !header.Authorization.Scheme.Equals("Bearer", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    Content = new StringContent(ResourcesManager._L("ERROR_NOTACCEPTABLE", "Authorization type")),
                    StatusCode = HttpStatusCode.NotAcceptable
                });
            }
            var accessToken = header.Authorization.Parameter;

            X509SecurityKey security = new X509SecurityKey(JwtProvider.LoadCertificate());
            SecurityToken securityToken;
            var handler = new JwtSecurityTokenHandler();

            //validation Token Expiry Date
            var token = handler.ReadToken(accessToken) as JwtSecurityToken;
            var tokenExpiryDate = token.ValidTo;
            // If there is no valid `exp` claim then `ValidTo` returns DateTime.MinValue
            if (tokenExpiryDate == DateTime.MinValue)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    Content = new StringContent("Unauthorized"),
                    StatusCode = HttpStatusCode.Unauthorized
                });

            // If the token is in the past then you can't use it
            if (tokenExpiryDate < DateTime.UtcNow)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent($"Token expired on: {tokenExpiryDate}")
                });

            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = security,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true
            };

            handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(accessToken, validationParameters, out securityToken);

            var tokenDecode = (JwtSecurityToken)securityToken;

            var client = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("client_id"));
            if (client == null)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Unauthorized")
                });
            }
            else
            {
                var clientId = ClientIdType.PosUser;

                if (client.Value == "pos_serial")
                    clientId = ClientIdType.PosSerial;
                else if (client.Value == "pos_driver")
                    clientId = ClientIdType.PosDriver;

                var serialNumber = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("serial_number"))?.Value;
                var deviceName = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("device_name"))?.Value;

                switch (clientId)
                {
                    case ClientIdType.PosUser:
                        break;
                    case ClientIdType.PosSerial:
                        if (string.IsNullOrEmpty(serialNumber))
                        {
                            throw new HttpResponseException(new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.NotAcceptable,
                                Content = new StringContent($"Client Not Acceptable")
                            });
                        }
                        break;
                    case ClientIdType.PosDriver:
                        if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(deviceName))
                        {
                            throw new HttpResponseException(new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.NotAcceptable,
                                Content = new StringContent($"Client Not Acceptable")
                            });
                        }
                        break;
                }
            }

        }


        public static PosRequest GetRequestHeaderValue(HttpRequestMessage request, ClientIdType clientAccept)
        {
            var header = request.Headers;

            X509SecurityKey security = new X509SecurityKey(JwtProvider.LoadCertificate());
            SecurityToken securityToken;
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = security,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true
            };
            handler = new JwtSecurityTokenHandler();
            try
            {
                handler.ValidateToken(header.Authorization.Parameter, validationParameters, out securityToken);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent($"Token expired on: {ex.Message}")
                });
            }

            var tokenDecode = (JwtSecurityToken)securityToken;

            var valid = new PosRequest
            {
                IsTokenValid = true,
                AccessToken = header.Authorization?.Parameter ?? "",
                Header = header.ToJson(),
                ApiControllerName = request.GetActionDescriptor().ControllerDescriptor.ControllerName,
                ApiActionName = request.GetActionDescriptor().ActionName,
                Status = HttpStatusCode.OK,
                Message = ResourcesManager._L("SUCCESSFULLY"),
                UserId = tokenDecode.Subject,
                SerialNumber = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("serial_number"))?.Value,
                DeviceName = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("device_name"))?.Value,
            };
            var client = tokenDecode.Claims.FirstOrDefault(e => e.Type.Equals("client_id"));
            valid.ClientId = ClientIdType.PosUser;

            if (client.Value == "pos_serial")
                valid.ClientId = ClientIdType.PosSerial;
            else if (client.Value == "pos_driver")
                valid.ClientId = ClientIdType.PosDriver;

            switch (clientAccept)
            {
                case ClientIdType.PosUser:
                    if (valid.ClientId != ClientIdType.PosUser)
                        throw new HttpResponseException(new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotAcceptable,
                            Content = new StringContent($"Not Acceptable Client")
                        });
                    break;
                case ClientIdType.PosSerial:
                    if (valid.ClientId != ClientIdType.PosSerial)
                        throw new HttpResponseException(new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotAcceptable,
                            Content = new StringContent($"Not Acceptable Client")
                        });
                    break;
                case ClientIdType.PosDriver:
                    if (valid.ClientId != ClientIdType.PosDriver)
                        throw new HttpResponseException(new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotAcceptable,
                            Content = new StringContent($"Not Acceptable Client")
                        });
                    break;
                default:
                    throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }
            return valid;
        }

    }
}