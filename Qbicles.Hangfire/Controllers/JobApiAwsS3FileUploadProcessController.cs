using Hangfire;
using Qbicles.BusinessRules.Model;
using System.Net;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class JobApiAwsS3FileUploadProcessController : ApiController
    {
        [Route("api/job/awss3fileuploadprocess")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleAwsS3FileUploadProcess(QbicleJobParameter job)
        {
            var jobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueAwsS3FileUploadProcess(job));

            return new QbicleJobResult
            {
                JobId = jobId,
                Message = "Hangfire Schedule awss3 file upload process!",
                Status = HttpStatusCode.OK
            };
        }
    }
}