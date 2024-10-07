using Hangfire;
using Qbicles.BusinessRules.Model;
using System;
using System.Net;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class JobApiCatalogController : ApiController
    {
        private string JobId { get; set; }


        [Route("api/job/processcatalogquickmode")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleCreateCatalogQuickMode(CatalogJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueCatalogQuickMode(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Catalog quick mode Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/processrefreshprices")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleRefreshPrices(CatalogJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueRefreshPrices(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Refresh Prices Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/pushpricestopricingpool")]
        [AcceptVerbs("POST")]
        public QbicleJobResult SchedulePushPricesToPricingPool(CatalogJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueuePushPricesToPricingPool(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/processupdatecatalogproductsqlite")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleUpdateCatalogProductSqlite(CatalogJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueUpdateCatalogProductSqlite(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Processing Update Catalog Product Sqlite Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/clonecatalog")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleCloneCatalog(CatalogJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueCloneCatalog(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Clone Catalog Completed!",
                Status = HttpStatusCode.OK
            };
        }

        [Route("api/job/deletecatalog")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleDeleteCatalog(CatalogJobParameter job)
        {
            JobId = BackgroundJob.Schedule(() => BackgroundJobEnqueue.QueueDeleteCatalog(job), TimeSpan.FromMinutes(0));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Delete Catalog Completed!",
                Status = HttpStatusCode.OK
            };
        }



        [Route("api/job/itemsimportprocess")]
        [AcceptVerbs("POST")]
        public QbicleJobResult ScheduleItemsImportProcess(QbicleJobParameter job)
        {
            JobId = BackgroundJob.Enqueue(() => BackgroundJobEnqueue.QueueItemsImportProcess(job));
            return new QbicleJobResult
            {
                JobId = JobId,
                Message = "Hangfire Clone Catalog Completed!",
                Status = HttpStatusCode.OK
            };
        }
    }
}