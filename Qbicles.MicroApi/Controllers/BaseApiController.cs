using System.Net;
using System.Net.Http.Headers;
using System.Web.Http;
using Qbicles.BusinessRules.Model;
using Qbicles.MicroApi.Authentication;
using Qbicles.Models.TraderApi;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.PoS;
using System.Net.Http;
using Qbicles.BusinessRules.Micro.Model;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public class BaseApiController : ApiController
    {
        //public ApplicationDbContext dbContext = new ApplicationDbContext();
        /// <summary>
        ///
        /// </summary>
        public IPosResult _authorizationInformation;

        /// <summary>
        ///
        /// </summary>
        public MicroContext _microContext { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="HttpResponseException"></exception>
        [ApiExplorerSettings(IgnoreApi = true)]
        public void HeaderInformation(HttpRequestMessage request)
        {
            var headerInformation = Validate(request.Headers);

            if (headerInformation.Status != HttpStatusCode.OK)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    Content = new StringContent(headerInformation.Message),
                    StatusCode = headerInformation.Status
                });

            _authorizationInformation = StatusCodeValidation(headerInformation);
            headerInformation.ApiControllerName = request.RequestUri.ToString();
            var dbContext = new ApplicationDbContext();
            _microContext = new MicroContext
            {
                Context = dbContext,
                UserId = headerInformation.UserId
            };

            new PosRequestRules(dbContext).HangfirePosRequestLog(headerInformation, "Pos Api Request Log Manage", System.Reflection.MethodBase.GetCurrentMethod().Name);

            if (_authorizationInformation.Status != HttpStatusCode.OK)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    Content = new StringContent("Unauthorized"),
                    StatusCode = _authorizationInformation.Status
                });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns> 
        [ApiExplorerSettings(IgnoreApi = true)]
        private PosRequest Validate(HttpRequestHeaders header)
        {
            return ValidateAccessToken.ValidateHeader(header);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tokenValid"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        private IPosResult StatusCodeValidation(PosRequest tokenValid)
        {
            if (!tokenValid.IsTokenValid)
                return new IPosResult
                {
                    Status = tokenValid.Status,
                    Message = tokenValid.Message,
                    IsTokenValid = tokenValid.IsTokenValid
                };

            if (tokenValid.ClientId != ClientIdType.MicroClient)
                return new IPosResult
                {
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_NOTACCEPTABLE", "Client type"),
                    IsTokenValid = false
                };
            return new IPosResult
            {
                Status = HttpStatusCode.OK
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public string GenerateCallbackUrl()
        {
            try
            {
                return $"{ConfigManager.AuthHost}/Account/Login";
            }
            catch
            {
                return "";
            }
        }
    }
}