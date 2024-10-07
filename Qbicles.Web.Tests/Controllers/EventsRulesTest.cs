using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules;
using Qbicles.Models;

namespace Qbicles.Web.Tests.Controllers
{
    [TestClass]
    public class EventsRulesTest
    {
        ApplicationDbContext DbContext = new ApplicationDbContext();
        ReturnJsonModel refModel = new ReturnJsonModel();

        [TestMethod]
        public void SaveEventTest()
        {
            refModel = new ReturnJsonModel();
            QbicleEvent qEvent = new QbicleEvent();
            EventsRules eventRule = new EventsRules(DbContext);

            UserRules userRules = new UserRules(DbContext);
            QbicleRules cubeRule = new QbicleRules(DbContext);
            string attactFile = "/Content/DesignStyle/img/user2-160x160.jpg";
            int cubeId = 0;
            var a = DbContext.QbicleUser.Find("QbicleUser");
            var currentUser = userRules.GetUserFirstDefault();
            cubeId = currentUser.CurrentQbicle.Id;

            string[] members;

            members = userRules.GetListUserIdByDomain(currentUser.CurrentDomain);

            qEvent.Name = "Unit Test task";
            qEvent.Start = DateTime.Now.AddDays(-1);
            qEvent.End = DateTime.Now;
            qEvent.Location = "Location test";
            qEvent.EventType = QbicleEvent.EventTypeEnum.Holiday;
            int fileSize = 4511112;


            //check duplicate event name
            refModel.result = eventRule.DuplicateEventNameCheck(cubeId, qEvent.Id, qEvent.Name);
            if (!refModel.result)
                refModel.result = eventRule.SaveEvent(qEvent, qEvent.Start.ToString(), qEvent.End.ToString(), members,
                    members,attactFile, fileSize,"",0);
            //Assert
            Assert.IsNotNull(refModel);
        }
    }
}
