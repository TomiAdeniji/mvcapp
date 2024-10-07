using System;
using System.Transactions;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.IdentityModel.Logging;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.Identity;
using System.Web;
using Qbicles.BusinessRules.Model;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Qbicles.BusinessRules.Provider;
using Hangfire.Storage.MySql;

[assembly: OwinStartupAttribute(typeof(Qbicles.Hangfire.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]

namespace Qbicles.Hangfire
{
    public partial class Startup
    {
        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            X509SecurityKey security = new X509SecurityKey(JwtProvider.LoadCertificate());
            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = security,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true
                    },
                    Provider = new TokenOAuthBearerProvider()
                });
        }

        [Obsolete]
        public void Configuration(IAppBuilder app)
        {
            //Ensure that log4net is configured
            log4net.Config.XmlConfigurator.Configure();

            ConfigureAuth(app);
            ConfigureOAuthTokenConsumption(app);
            IdentityModelEventSource.ShowPII = true;

            var connection = System.Configuration.ConfigurationManager.ConnectionStrings["hangfire"].ConnectionString;

            GlobalConfiguration.Configuration.UseStorage(
                    new MySqlStorage(
                        connection,
                        new MySqlStorageOptions
                        {
                            TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                            QueuePollInterval = TimeSpan.FromSeconds(1),
                            JobExpirationCheckInterval = TimeSpan.FromHours(1),
                            CountersAggregateInterval = TimeSpan.FromMinutes(5),
                            PrepareSchemaIfNecessary = true,
                            DashboardJobListLimit = 50000,
                            TransactionTimeout = TimeSpan.FromMinutes(1),
                            TablesPrefix = "qbicles_"
                        }))
                .UseFilter(new NLogHangFireAttribute());
            //.UseConsole();

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "chatting", "itemsimportjob", "qbiclesjob", "campaignjob", "catalogjob", "loyaltyjob", "processmedia", "default" },
                ServerName = "Qbicle"
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "pos", "driver", "pds", "dds" },
                ServerName = "POS Job"
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "orderstatus" },
                ServerName = "Trade Order Status Job"//Logging, update status
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "inventoryjob", "tradermovementjob" },
                WorkerCount = 1,
                ServerName = "Inventory Job"
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "processorder" },
                WorkerCount = 1,
                ServerName = "Process Order Job"
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "domainsubscriptionjob" },
                WorkerCount = 1,
                ServerName = "Process the Domain subscription notifications"
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "eventtasknotificationpoints" },
                WorkerCount = 1,
                ServerName = "Event & Task notification points"
            });

            app.UseHangfireDashboard("/Dashboard", new DashboardOptions
            {
                IsReadOnlyFunc = (DashboardContext context) => false,
                Authorization = new[] { new QbiclesAuthorizationFilter() },
                AppPath = VirtualPathUtility.ToAbsolute("/Home/Index")
            });
        }

        public class QbiclesAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                var dbContext = new ApplicationDbContext();

                var userIdAuth = HttpContext.Current.User.Identity.GetUserId();

                if (string.IsNullOrEmpty(userIdAuth))
                    return false;
                var sysAdmin = new BusinessRules.QbicleRules(dbContext).SystemRoleValidation(userIdAuth, BusinessRules.SystemRoles.SystemAdministrator);

                if (!sysAdmin)
                    return false;

                var user = dbContext.QbicleUser.Find(userIdAuth);
                if (user == null)
                    return false;

                return true;

                //// In case you need an OWIN context, use the next line, `OwinContext` class
                //// is the part of the `Microsoft.Owin` package.
                //var owinContext = new OwinContext(context.GetOwinEnvironment());
                ////return HttpContext.Current.User.Identity.Name.Equals("SystemAdmin");
                //// Allow all authenticated users to see the Dashboard (potentially dangerous).
                //return owinContext.Authentication.User.Identity.IsAuthenticated;
            }
        }
    }
}