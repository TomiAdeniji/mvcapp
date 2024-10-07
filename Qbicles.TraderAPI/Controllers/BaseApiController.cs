using Qbicles.BusinessRules.Model;
using Qbicles.Models.TraderApi;
using Qbicles.TraderAPI.Authentication;
using Qbicles.TraderAPI.Helper;
using System.Net.Http;
using System.Web.Http;

namespace Qbicles.TraderAPI.Controllers
{
    [ApiRequestAuthorization]
    public class BaseApiController : ApiController
    {
        public ApplicationDbContext dbContext = new ApplicationDbContext();

        /// <summary>
        /// Get value in reuqest header
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// PosRequest{UserId, DeviceId, SerialNumber, DeviceName}
        /// </returns>
        public PosRequest RequestValue(HttpRequestMessage request, ClientIdType clientAccept)
        {
            return ValidateAccessToken.GetRequestHeaderValue(request, clientAccept);
        }
    }
}
