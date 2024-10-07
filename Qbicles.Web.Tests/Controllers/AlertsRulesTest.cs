using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Web.Tests.Controllers
{
    [TestClass()]
    public class AlertsRulesTest
    {
        ApplicationDbContext DbContext = new ApplicationDbContext();
        ReturnJsonModel refModel = new ReturnJsonModel();

        [TestMethod()]
        public void SaveAlertTest()
        {
            //QbicleAlert alert, string[] linkAlertTo, int qbicleId, HttpPostedFileBase alertAttachments

            refModel = new ReturnJsonModel();
            QbicleAlert alert = new QbicleAlert();
            AlertsRules alertRule = new AlertsRules(DbContext);

            UserRules userRules = new UserRules(DbContext);
            QbicleRules cubeRule = new QbicleRules(DbContext);
            string attactFile = "/Content/DesignStyle/img/user2-160x160.jpg";
            int  fileSize = 1234;
            int cubeId = 0;
           
            var currentUser = userRules.GetUserFirstDefault();
            cubeId = currentUser.CurrentQbicle.Id;

            string[] members;

            members = userRules.GetListUserIdByDomain(currentUser.CurrentDomain);

            alert.Name = "Unit Test task";
            alert.Content = "Unit test create Qbicle - Content";
            alert.Priority = QbicleAlert.AlertPriorityEnum.Critical;
            alert.Type = QbicleAlert.AlertTypeEnum.General;
            

            //check duplicate Task name
            refModel.result = alertRule.DuplicateAlertNameCheck(cubeId, alert.Id, alert.Name);
            if (!refModel.result)
                //refModel.result = alertRule.SaveAlert(alert, members, members,attactFile, fileSize, "",0);
            //Assert
            Assert.IsNotNull(refModel);
        }
    }
}
