namespace Qbicles.Web.Tests.Controllers
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.AspNet.Identity;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
    using System.Net;
    using System.IO;
    using System.Web;
    using System.Web.Hosting;
    using System.Reflection;
    using Qbicles.Web.Controllers;
    using Qbicles.Web.Models;
    using System.Web.Mvc;
    using System.Threading.Tasks;
    using Qbicles.BusinessRules;
    using Qbicles.BusinessRules.Model;
    using Qbicles.Models;
    using System.Linq;

    [TestClass]
    public class DiscussionsRulesTest
    {
        ApplicationDbContext DbContext = new ApplicationDbContext();
        ReturnJsonModel refModel = new ReturnJsonModel();

        [TestMethod]
        public void CreateDiscussionTest()
        {
            refModel = new ReturnJsonModel();
            QbicleDiscussion discussion = new QbicleDiscussion();
            DiscussionsRules tkRule = new DiscussionsRules(DbContext);

            UserRules userRules = new UserRules(DbContext);
            QbicleRules cubeRule = new QbicleRules(DbContext);
            // Act
            //using userId if know
            //var startBy = u.GetUser("931e3e3c-e315-411e-9f87-0ad284de68ca");
            //or get first defaul user here
            

            string expDate = "30.03.2017 11:55";// format date time of the dtpicker control(dd.MM.yyyy HH:mm)
            string attactFile = "/Content/DesignStyle/img/user2-160x160.jpg";


            int cubeId = 0;
            //using here if know Qbicle Id
            //domainId = 1;
            //or get current Qbicle by user
            var currentUser = userRules.GetUserFirstDefault();
            cubeId = currentUser.CurrentQbicle.Id;
            
            //init data for Qbicle
            discussion.Name = "Unit Test Discussion 1";
            string[] members;
           // members = userRules.GetListUserId();// get list member default
           //or get by Qbicle member logic
            switch (currentUser.CurrentQbicle.Scope)
            {
                case Qbicle.QbicleScopeEnum.Public://In the case of a public Qbicle this is a list of all the users in the Domain.
                    members = userRules.GetListUserIdByDomain(currentUser.CurrentDomain);
                    break;
                case Qbicle.QbicleScopeEnum.Private://The list of users displayed is the list of users associated with the Qbicle.
                    members = userRules.GetListUserIdByQbicle(currentUser.CurrentQbicle);
                    break;
                default:
                    members = userRules.GetListUserId();
                    break;
            }
            int filesize = 1234;
            //check duplicate Task name
            refModel.result = tkRule.DuplicateDiscussionNameCheck(cubeId, discussion.Id, discussion.Name);
            if (!refModel.result)
                refModel.result = tkRule.SaveDiscussion(discussion, members, members, expDate, attactFile, filesize, cubeId,"");

            //Assert
            Assert.IsNotNull(refModel);
        }
    }
}
