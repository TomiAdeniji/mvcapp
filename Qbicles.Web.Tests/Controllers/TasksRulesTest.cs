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
    [TestClass]
    public class TasksRulesTest
    {
        ApplicationDbContext DbContext = new ApplicationDbContext();
        ReturnJsonModel refModel = new ReturnJsonModel();

        [TestMethod]
        public void CreateTaskTest()
        {
            refModel = new ReturnJsonModel();
            QbicleTask task = new QbicleTask();
            TasksRules tkRule = new TasksRules(DbContext);

            UserRules userRules = new UserRules(DbContext);
            QbicleRules cubeRule = new QbicleRules(DbContext);
            // Act
            //using userId if know
            //var startBy = u.GetUser("931e3e3c-e315-411e-9f87-0ad284de68ca");
            //or get first defaul user here


            string dueDate = "28.03.2017 11:55";// format date time of the dtpicker control(dd.MM.yyyy HH:mm)
            string attactFile = "/Content/DesignStyle/img/user2-160x160.jpg";
            

            int cubeId = 0;
            //using here if know Qbicle Id
            //domainId = 1;
            //or get default a Qbicle for test
            //or get current Qbicle by user
            var currentUser = userRules.GetUserFirstDefault();
            cubeId = currentUser.CurrentQbicle.Id;

            string[] members;
            // members = userRules.GetListUserId();// get list member default
            //or get by Task assign logic
            //The users displayed are the users associated with the current Qbicle.  
            members = userRules.GetListUserIdByDomain(currentUser.CurrentDomain);

            task.Name = "Unit Test task";
            task.Description = "Unit test create Qbicle";
            task.Priority = QbicleTask.TaskPriorityEnum.Critical;
            task.Repeat = QbicleTask.TaskRepeatEnum.Monthly;
            int fileSize = 1232;
            string fileName = "fileName.jpg";
            string[] formId = null;
            //check duplicate Task name
            refModel.result = tkRule.DuplicateTaskNameCheck(cubeId,task.Id, task.Name);
            if (!refModel.result)
                refModel.result = tkRule.SaveTask(task, members, members, dueDate, attactFile,fileName, fileSize, formId,  cubeId, currentUser, 0);

            //Assert
            Assert.IsNotNull(refModel);
        }
    }
}
