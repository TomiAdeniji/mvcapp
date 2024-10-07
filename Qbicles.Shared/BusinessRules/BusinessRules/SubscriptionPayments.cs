using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules
{
    public class SubscriptionPayments
    {
        private readonly ApplicationDbContext dbContext;

        public SubscriptionPayments(ApplicationDbContext context)
        {
            dbContext = context;
        }
        //At present, the details of how a Domain is payed for have not been worked out. 
        //However, a 'paypoint' is to be introduced so that the payment process can be implemented in the future.
        public async Task<ReturnJsonModel> NewDomainCreated(int domainId)
        {
            //This has NO body at present
            await Task.Delay(10);
            return new ReturnJsonModel {result=true };
        }
    }
}
