using MySql.Data.MySqlClient;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using System.Collections.Generic;
using System.Data.Entity;
using System;
using System.Linq;
using Qbicles.Models.TraderApi;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroHomeRules : MicroRulesBase
    {
        public MicroHomeRules(MicroContext microContext) : base(microContext)
        {
        }
        /// <summary>
        /// if wizard < 3 continue setting user
        /// https://atomsinteractive.atlassian.net/browse/QBIC-3606
        /// </summary>
        /// <returns></returns>
        public object MicroFirstLaunchedValidation()
        {
            if (CurrentUser.IsImportWizardMicro == null || CurrentUser.IsImportWizardMicro < 1)
                return new
                {
                    Wizard = MicroFirstLaunched.ImportWizard,
                    Key = ""
                };

            if (CurrentUser.IsUserProfileWizardRunMicro != true)
                return new
                {
                    Wizard = MicroFirstLaunched.UserProfileWizard,
                    Key = ""
                };

            return new
            {
                Wizard = MicroFirstLaunched.Splash,
                Key = ""
            };
        }

        public LoginSplashInterstitial GetLoginSplashInterstitial()
        {
            //if (CurrentUser.IsUserProfileWizardRunMicro != true)
            //    return new LoginSplashInterstitial { DomainAction = LoginSplashInterstitialAction.UserProfileWizard };
            var response = new LoginSplashInterstitial
            {
                Avatar = CurrentUser.ProfilePic.ToUri(),
                Forename = CurrentUser.Forename,
            };

            if (CurrentUser.Domains.Count > 1)
            {
                response.DomainAction = LoginSplashInterstitialAction.GoToListDomain;
                response.DomainTitle = "Manage my businesses";
            }
            else if (CurrentUser.Domains.Count == 1)
            {
                response.DomainAction = LoginSplashInterstitialAction.GoToQbicle;
                var domain = CurrentUser.Domains.FirstOrDefault();
                response.Domain = new Models.MicroQbicleStream.BaseModelImage
                {
                    Id = domain.Id,
                    Name = domain.Name,
                    ImageUri = domain.LogoUri.ToUri(),
                };
                response.DomainTitle = "Go to my business";
            }

            else if (CurrentUser.Domains.Count == 0)
            {
                response.DomainAction = LoginSplashInterstitialAction.AddDomain;
                response.DomainTitle = "Add my business";
            }

            int allNum = 0;
            int favouriteNum = 0;
            int requestNum = 0;
            int sentNum = 0;
            int blockedNum = 0;


            var sContactType = dbContext.UiSettings.FirstOrDefault(s => s.CurrentPage == SystemPageConst.C2C
                                                    && s.CurrentUser.Id == CurrentUser.Id
                                                    && s.Key == C2CStoreUiSettingsConst.CONTACTTYPE)?.Value ?? "0";

            new C2CRules(dbContext).GetCommunityTalkNumByType(CurrentUser.Id, "", int.Parse(sContactType), ref allNum, ref favouriteNum, ref requestNum, ref sentNum, ref blockedNum);

            response.NewMessage = requestNum;
            return response;
        }

        public List<RoutesModel> Update(RoutesModel ds)
        {
            using (var context = new ApplicationDbContext())
            {
                string sql = "INSERT INTO trad_routes (name, routes, createddate) VALUES (@name, @routes, @createddate);";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@name", ds.Name),
                    new MySqlParameter("@routes", ds.Routes),
                    new MySqlParameter("@createddate", DateTime.UtcNow)
                };
                int rowsAffected = context.Database.ExecuteSqlCommand(sql, parameters);
            }

            var sqlQuery = "SELECT DISTINCT p1.name, p1.routes FROM trad_routes p1 INNER JOIN " +
                $"(SELECT name, MAX(createddate) AS max_updated FROM trad_routes where  name != '{ds.Name}' GROUP BY name) p2 ON p1.name = p2.name AND p1.createddate = p2.max_updated " +
                "ORDER BY p1.createddate DESC LIMIT 3;";

            var resultList = dbContext.Database.SqlQuery<RoutesModel>(sqlQuery);
            var routes = new List<RoutesModel>();
            foreach (var route in resultList.Distinct())
            {
                routes.Add(new RoutesModel { Name = route.Name, Routes = route.Routes });
            }

            return routes;
        }


        public bool Delete(RoutesModel ds)
        {
            using (var db = new ApplicationDbContext())
            {
                var sql = $"DELETE FROM trad_routes WHERE name = '{ds.Name}' AND id NOT IN (SELECT id FROM (SELECT id FROM trad_routes WHERE name = '{ds.Name}' ORDER BY createddate DESC LIMIT 1) t)";
                dbContext.Database.ExecuteSqlCommand(sql);
            }

            return true;
        }
        public bool DeleteAll()
        {
            using (var db = new ApplicationDbContext())
            {
                var sql = $"DELETE FROM trad_routes";
                dbContext.Database.ExecuteSqlCommand(sql);
            }

            return true;
        }
    }
}
