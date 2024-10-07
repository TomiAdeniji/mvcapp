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
    public class QbicleRulesTest
    {
        ApplicationDbContext DbContext = new ApplicationDbContext();
        ReturnJsonModel refModel = new ReturnJsonModel();
        [TestMethod]
        public void CreateQbicle()
        {
            // Arrange
            refModel = new ReturnJsonModel();
            Qbicle qbicle = new Qbicle();

            UserRules u = new UserRules(DbContext);
            QbicleRules cube = new QbicleRules(DbContext);
            DomainRules dr = new DomainRules(DbContext);

            // Act
            //using userId if know
            //var startBy = u.GetUser("931e3e3c-e315-411e-9f87-0ad284de68ca");
            //or get first defaul user here
            //HelperClass.curentUserId = u.GetUserIdFirstDefault();

            string[] members = u.GetListUserId();
            string[] members2 = u.GetListUserId();

            var qbicleScope = Qbicle.QbicleScopeEnum.Public;


            int domainId = 0;
            //1. using here if know domain Id
            //domainId = 1;

            //2. Get the Domain
            var currentUser = u.GetUser(u.GetUserIdFirstDefault());
            //If the user DOES have a CurrentDomain set, then that Domain is to be set as the selected Domain.
            var domain = currentUser.CurrentDomain;
            //If the user DOES NOT have a CurrentDomain set, 
            //then the Domain is to be set as the first Domain in an alphabetical list
            //of the Domains with which the user is associated.
            if (domain == null)
                domain = dr.GetDomainFirstDefault();

            domainId = domain.Id;

            qbicle.Name = "Unit Test";
            qbicle.Description = "Unit test create Qbicle";


            //check duplicate Qbicle name
            refModel.result = cube.DuplicateQbicNameCheck(qbicle.Id, qbicle.Name, domainId);
            if (!refModel.result)
                refModel.Object = cube.SaveQbicle(qbicle, members, members2, members2, (int)qbicleScope, domainId,"");

            //Assert
            Assert.IsNotNull(refModel);
        }

        [TestMethod]
        public void GetQbicleById()
        {
            // Arrange
            refModel = new ReturnJsonModel();

            QbicleRules cube = new QbicleRules(DbContext);

            // Act
            //using userId if know
            //var startBy = u.GetUser("931e3e3c-e315-411e-9f87-0ad284de68ca");
            //or get first defaul user here


            int qbicleId = 0;
            //using here if know Qbicle Id
            //qbicleId = 1;
            //or get default a Domain for test
            refModel.Object = cube.GetQbicleFirstDefault();
            if (refModel.Object != null)
                qbicleId = ((Qbicle)refModel.Object).Id;

            refModel.Object = cube.GetQbicleById(qbicleId);

            //Assert
            Assert.IsNotNull(refModel);
        }
    }
}
