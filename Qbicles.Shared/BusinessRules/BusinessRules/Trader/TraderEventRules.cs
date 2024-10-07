using Qbicles.BusinessRules.Model;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderEventRules
    {
        private ApplicationDbContext _dbContext;

        public TraderEventRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public void GenerateStorePoinFromPaymentApproved(int paymentId)
        {
            var job = new QbicleJobParameter
            {
                Id = paymentId,
                EndPointName = "generatestorepoinfrompaymentapproved",
            };

            Task tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        public void DecreaseStoreCreditFromPaymentApproved(int paymentId)
        {
            var job = new QbicleJobParameter
            {
                Id = paymentId,
                EndPointName = "decreasestorecreditfrompaymentapproved",
            };

            Task tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

    }
}
