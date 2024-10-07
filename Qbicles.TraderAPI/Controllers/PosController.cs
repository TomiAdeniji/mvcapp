using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.TraderApi;
using Qbicles.TraderAPI.Helper;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Qbicles.TraderAPI.Controllers
{
    [RoutePrefix("api/pos/device")]
    public class PosDeviceController : ApiController
    {
        /// <summary>
        /// Check the serial has linked to a device (Only 2 requests per minute)
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <returns></returns>
        [ThrottleApi(Count = 2, TimeUnit = TimeUnit.Minute)]
        [Route("check")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage CheckSeialLinkToDevice(string serialnumber)
        {
            var dbContext = new BusinessRules.Model.ApplicationDbContext();
            var linked = new PosRules(dbContext).CheckSeialLinkToDevice(serialnumber);
            return Request.CreateResponse(HttpStatusCode.OK, new { linked }, Configuration.Formatters.JsonFormatter);
        }
    }

    [RoutePrefix("api/pos")]
    public class PosController : BaseApiController
    {
        [Route("datetime")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDateTimeUtcServer()
        {
            var dateTime = DateTime.UtcNow;
            return Request.CreateResponse(HttpStatusCode.OK, new { DateTimeUtc = dateTime }, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// client_id: pos_serial
        /// </summary>
        /// <returns></returns>
        [Route("user")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage PosUser()
        {
            var user = new PosRules(dbContext).GetUserInformation(RequestValue(Request, ClientIdType.PosSerial).UserId);
            return user != null ?
                Request.CreateResponse(HttpStatusCode.OK, user, Configuration.Formatters.JsonFormatter) :
                Request.CreateResponse(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// client_id: pos_user
        /// </summary>
        /// <returns></returns>
        [Route("device")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DeviceForUser()
        {
            var devices = new PosRules(dbContext).GetDeviceForUser(RequestValue(Request, ClientIdType.PosUser));
            return devices != null ?
                Request.CreateResponse(HttpStatusCode.OK, devices, Configuration.Formatters.JsonFormatter) :
                Request.CreateResponse(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// pos_user
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        [Route("device")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage TabletForDevice(PosDeviceResult device)
        {
            var valid = RequestValue(Request, ClientIdType.PosUser);

            if (string.IsNullOrEmpty($"{device.Id}") || string.IsNullOrEmpty(device.SerialNumber))
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            valid.DeviceId = device.Id;
            valid.SerialNumber = device.SerialNumber;

            var result = new PosRules(dbContext).TabletForDevice(valid);

            return result.Status == HttpStatusCode.OK ?
                Request.CreateResponse(HttpStatusCode.OK) :
                Request.CreateResponse(result.Status, result.Message, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        /// pos-serial
        /// </summary>
        /// <returns></returns>
        [Route("products/version")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage ProductVersion()
        {
            try
            {
                var posRules = new PosProductRules(dbContext);
                var posDevice = posRules.GetMenuProduct(RequestValue(Request, ClientIdType.PosSerial));

                if (posDevice == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                return Request.CreateResponse(HttpStatusCode.OK, new { Version = posDevice.Menu.ProductFile }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, ex.Message, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("products")]
        [AcceptVerbs("GET")]
        public async Task<HttpResponseMessage> ProductsAsync()
        {
            var response = Request.CreateResponse();
            try
            {
                var posRules = new PosProductRules(dbContext);
                var posDevice = posRules.GetMenuProduct(RequestValue(Request, ClientIdType.PosSerial));

                if (posDevice == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(posDevice.Menu.ProductFile);
                if (s3Object.ObjectKey == null)
                {
                    //posRules.SqLiteProduct(posDevice.Menu, posDevice);
                    if (posDevice.Menu != null)
                        posDevice.Menu.IsPOSSqliteDbBeingProcessed = true;
                    dbContext.SaveChanges();
                    //new PosProductRules(dbContext).SqLiteProduct(deviceUpdate.Menu, deviceUpdate);
                    new PosMenuRules(dbContext).MoveUpdateCatalogProductSqliteToHangfire(posDevice.Menu.Id);
                    s3Object = await AzureStorageHelper.ReadObjectDataAsync(posDevice.Menu.ProductFile);
                }

                byte[] content = null;

                using (var memoryStream = new MemoryStream())
                {
                    s3Object.ObjectStream.CopyTo(memoryStream);
                    content = memoryStream.ToArray();
                }

                //Set the Response Content.
                response.Content = new ByteArrayContent(content);

                //Set the Response Content Length.
                response.Content.Headers.ContentLength = content.LongLength;

                //Set the Content Disposition Header Value and FileName.
                response.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment") { FileName = Path.GetFileName(s3Object.ObjectName) };

                //Set the File Content Type.
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                return response;
            }
            catch (Exception ex)
            {
                //Throw 404 (Not Found) exception if File not found.
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, ex.Message, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Filter contact to Pos
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [Route("contact")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage PosContact(PosTraderContact contact)
        {
            var posContacts = new PosRules(dbContext).PosContact(contact, RequestValue(Request, ClientIdType.PosSerial));

            if (posContacts.Contacts.Count == 1)
            {
                var contactId = posContacts.Contacts.FirstOrDefault()?.Id ?? 0;
                if (contactId == -1)
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, ResourcesManager._L("ERROR_MSG_5"), Configuration.Formatters.JsonFormatter);
                if (contactId == -2)
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, ResourcesManager._L("ERROR_MSG_6"), Configuration.Formatters.JsonFormatter);
            }

            return Request.CreateResponse(HttpStatusCode.OK, posContacts, Configuration.Formatters.JsonFormatter);
        }

        [Route("users")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage PosUsers()
        {
            var users = new PosRules(dbContext).PosUsersInfomation(RequestValue(Request, ClientIdType.PosSerial).SerialNumber);
            return Request.CreateResponse(HttpStatusCode.OK, users, Configuration.Formatters.JsonFormatter);
        }
    }
}