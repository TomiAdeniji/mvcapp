//using System.Threading.Tasks;
//using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using NLog;

namespace Qbicles.Hangfire
{
    public class NLogHangFireAttribute : JobFilterAttribute,
        IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private static readonly ILog NLogger = LogProvider.GetCurrentClassLogger();


        public NLogHangFireAttribute()
        {
            //Log.Logger = new LoggerConfiguration()
            //    .Enrich.FromLogContext()
            //    .MinimumLevel.Debug()
            //    .WriteTo.Sink(new HangfireConsoleSink())
            //    .CreateLogger();
        }

        
        public void OnCreating(CreatingContext context)
        {
            NLogger.InfoFormat("Creating a job based on method `{0}`...", context.Job.Method.Name);
        }

        
        public void OnCreated(CreatedContext context)
        {
            NLogger.InfoFormat(
                "Job that is based on method `{0}` has been created with id `{1}`",
                context.Job.Method.Name,
                context.BackgroundJob?.Id);
        }


        public void OnPerforming(PerformingContext context)
        {
            NLogger.InfoFormat("Starting to perform job `{0}`", context.BackgroundJob.Id);
        }

        public void OnPerformed(PerformedContext context)
        {
            NLogger.InfoFormat("Job `{0}` has been performed", context.BackgroundJob.Id);
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                NLogger.WarnFormat(
                    "Job `{0}` has been failed due to an exception `{1}`",
                    context.BackgroundJob.Id,
                    failedState.Exception);
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            NLogger.InfoFormat(
                "Job `{0}` state was changed from `{1}` to `{2}`",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            NLogger.InfoFormat(
                "Job `{0}` state `{1}` was unapplied.",
                context.BackgroundJob.Id,
                context.OldStateName);
        }
    }
}