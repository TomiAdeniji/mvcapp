using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Qbicles.Models.TraderApi;
using Qbicles.BusinessRules.AWS;
using System.Threading.Tasks;
using Qbicles.BusinessRules.Azure;

namespace Qbicles.TraderAPI.Controllers
{
    [System.Web.Http.RoutePrefix("api/pos")]
    public class MediaRetrieverApiController : BaseApiController
    {
        [System.Web.Http.Route("image")]
        [System.Web.Http.AcceptVerbs("GET")]
        public async Task<HttpResponseMessage> ImageAsync(string file)
        {
            var storageFile = dbContext.StorageFiles.Find(file);

            if (storageFile == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = "The requested resource was not found."
                }, Configuration.Formatters.JsonFormatter);

            var fileDetail = dbContext.StorageFileDetails.AsNoTracking().FirstOrDefault(e => e.StorageFile == file && e.Extension == "S");
            if (fileDetail == null)
            {
                //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(file);
                await new AzureStorageRules(dbContext).ImageProcessAsync(s3Object, storageFile);

                fileDetail = dbContext.StorageFileDetails.AsNoTracking().FirstOrDefault(e => e.StorageFile == file && e.Extension == "S");
                if (fileDetail == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, new IPosResult
                    {
                        IsTokenValid = true,
                        Status = HttpStatusCode.NotFound,
                        Message = "The requested resource was not found."
                    }, Configuration.Formatters.JsonFormatter);
            }

            file = fileDetail.Path;

            var s3ObjectUrl = AzureStorageHelper.SignedUrl(file);
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(s3ObjectUrl);
            return response;
        }

        [System.Web.Http.Route("document")]
        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage Document(string file)
        {
            var storageFile = dbContext.StorageFiles.AsNoTracking().FirstOrDefault(e => e.Id == file);

            if (storageFile == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = "The requested resource was not found."
                }, Configuration.Formatters.JsonFormatter);

            var s3ObjectUrl = AzureStorageHelper.SignedUrl(storageFile.Path);
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(s3ObjectUrl);
            return response;
        }

        [System.Web.Http.Route("video")]
        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage Video(string file, string type)
        {
            var storageFile = dbContext.StorageFileDetails.AsNoTracking().FirstOrDefault(e => e.StorageFile == file && e.Extension == type);

            if (storageFile == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = "The requested resource was not found."
                }, Configuration.Formatters.JsonFormatter);

            var s3ObjectUrl = AzureStorageHelper.SignedUrl(storageFile.Path);
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(s3ObjectUrl);
            return response;
        }
    }
}