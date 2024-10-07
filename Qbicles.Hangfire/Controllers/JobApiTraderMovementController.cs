using Hangfire;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader.Movement;
using System;
using System.Net;
using System.Web.Http;

namespace Qbicles.Hangfire.Controllers
{
    [Authorize]
    public class JobApiTraderMovementController : ApiController
    {
        [Route("api/job/schedulecheckingnomovementalert")]
        public QbicleJobResult ScheduleOnCheckingNoMovement(TraderMovementJobParamenter job)
        {
            var dbContext = new ApplicationDbContext();
            var alertConstraint = dbContext.AlertConstraints.Find(job.AlertConstraintId);
            if (alertConstraint == null)
            {
                return null;
            }
            else
            {
                if (alertConstraint.CheckEvent == CheckEvent.Daily)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueNoMovementCheck(job), "0 59 23 * * *");
                }
                else if (alertConstraint.CheckEvent == CheckEvent.Weekly)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueNoMovementCheck(job), "59 23 * * FRI");
                } else if (alertConstraint.CheckEvent == CheckEvent.Month)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueNoMovementCheck(job), "0 59 23 L * *");
                }
                //RecurringJob.AddOrUpdate("hh",  () => Console.Write("Easy!"), Cron.Daily);

                return new QbicleJobResult
                {
                    JobId = job.AlertConstraintId.ToString(),
                    Message = "Hangfire Schedule Activity Completed!",
                    Status = HttpStatusCode.OK
                };
            }
        }

        [Route("api/job/schedulecheckingminmaxalert")]
        public QbicleJobResult ScheduleOnCheckingMinMaxAlert(TraderMovementJobParamenter job)
        {
            var dbContext = new ApplicationDbContext();
            var alertConstraint = dbContext.AlertConstraints.Find(job.AlertConstraintId);
            if (alertConstraint == null)
            {
                return null;
            }
            else
            {
                if (alertConstraint.CheckEvent == CheckEvent.Daily)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueMinMaxCheck(job), "0 59 23 * * *");
                } else if (alertConstraint.CheckEvent == CheckEvent.Weekly)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueMinMaxCheck(job), "59 23 * * FRI");
                } else if (alertConstraint.CheckEvent == CheckEvent.Month)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueMinMaxCheck(job), "0 59 23 L * *");
                }

                return new QbicleJobResult
                {
                    JobId = job.AlertConstraintId.ToString(),
                    Message = "Hangfire Schedule Activity Completed!",
                    Status = HttpStatusCode.OK
                };
            }
        }

        [Route("api/job/schedulecheckingaccumulationalert")]
        public QbicleJobResult ScheduleOnCheckingAccumulationAlert(TraderMovementJobParamenter job)
        {
            var dbContext = new ApplicationDbContext();
            var alertConstraint = dbContext.AlertConstraints.Find(job.AlertConstraintId);
            if (alertConstraint == null)
            {
                return null;
            }
            else
            {
                if(alertConstraint.CheckEvent == CheckEvent.Daily)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueAccumulationCheck(job), "0 59 23 * * *");
                }
                else if (alertConstraint.CheckEvent == CheckEvent.Weekly)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueAccumulationCheck(job), "59 23 * * FRI");
                } else if (alertConstraint.CheckEvent == CheckEvent.Month)
                {
                    RecurringJob.AddOrUpdate(job.AlertConstraintId.ToString(), () => BackgroundJobEnqueue.QueueAccumulationCheck(job), "0 59 23 L * *");
                }

                return new QbicleJobResult
                {
                    JobId = job.AlertConstraintId.ToString(),
                    Message = "Hangfire Schedule Activity Completed!",
                    Status = HttpStatusCode.OK
                };
            }
        }

        [Route("api/job/removeschedulejob")]
        public QbicleJobResult RemoveScheduleJob(TraderMovementJobParamenter job)
        {
            try
            {
                RecurringJob.RemoveIfExists(job.AlertConstraintId.ToString());
                var result = new QbicleJobResult();
                result.JobId = job.AlertConstraintId.ToString();
                result.Message = "Hangfire Schedule Removed!";
                result.Status = HttpStatusCode.OK;
                return result;
            } catch(Exception)
            {
                return null;
            }
        }
    }
}