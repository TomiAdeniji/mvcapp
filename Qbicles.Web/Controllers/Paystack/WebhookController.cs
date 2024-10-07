using Qbicles.BusinessRules.BusinessRules.PayStack;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Model.PaystackWebhook;
using Qbicles.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers.Paystack
{
    [AllowAnonymous]
    [AuthorizeIPAddress]
    public class WebhookController : Controller
    {
        public ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Webhook
        public ActionResult InvoiceUpdatedWebhook(WebhookInvoiceUpdatedRequestModel data)
        {
            // Track the data of payment for the subscription
            // Save the payment information
            var isSaved = new PayStackRules(dbContext).SavePaystackPaymentFromWebhook(data);

            // If the payment is not successful, need to notify the creator of the domain
            if (isSaved)
            {
                if(data.data.transaction.status != "success")
                {
                    // Do something to notify the domain creator or admins
                }
            }
            

            return new HttpStatusCodeResult(200);
        }
    }
}