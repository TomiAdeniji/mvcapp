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
    public class DomainRulesTest
    {
        ApplicationDbContext DbContext = new ApplicationDbContext();
        ReturnJsonModel refModel;

        [TestMethod]
        public void ChangeDomain()
        {
            
            // Arrange
            refModel = new ReturnJsonModel();
            DomainRules dr = new DomainRules(DbContext);
            UserRules u = new UserRules(DbContext);
            int currentDomainId;
            //1. Parameter exist
            //currentDomainId = 1;
            //2. Get parameter default
            //userId = u.GetUserIdFirstDefault();
            var user = u.GetUserFirstDefault();
            refModel.Object = dr.GetDomainFirstDefault();
            if (refModel.Object !=null)
                currentDomainId = ((QbicleDomain)refModel.Object).Id;
            else
                currentDomainId = -1;
            // Act
            refModel.result = dr.UpdateCurrentDomain(currentDomainId, "");
            //Assert
            Assert.IsNotNull(refModel);
        }

        [TestMethod]
        public void OrderBy()
        {
            // Arrange
            refModel = new ReturnJsonModel();
            DomainRules dr = new DomainRules();
            UserRules u = new UserRules(DbContext);
            var userId = "";
            //BusinessRules.Enums.QbicleDisplayOrderEnum orderBy;
            //1. Parameter exist
            //userId = "931e3e3c-e315-411e-9f87-0ad284de68ca";
            //2. Get parameter default
            userId = u.GetUserIdFirstDefault();
            //orderBy = BusinessRules.Enums.QbicleDisplayOrderEnum.LatestActivity;
            // Act
            //refModel.Object = dr.GetUsersQbiclesInOrder(userId);
            //Assert
            Assert.IsNotNull(refModel);
        }
        [TestMethod]
        public void CloseQbicle()
        {
            // Arrange
            refModel = new ReturnJsonModel();
            QbicleRules qb = new QbicleRules(DbContext);
            Qbicle qbicle = new Qbicle();

            var qbicId = 0;
            //1. Parameter exist
            //userId = "931e3e3c-e315-411e-9f87-0ad284de68ca";
            //qbicId = 1;
            //2. Get parameter default
            
            refModel.Object = qb.GetQbicleFirstDefault();
            if (refModel.Object != null)
                qbicId = ((Qbicle)refModel.Object).Id;
            else
                qbicId = -1;
            // Act
            refModel.result = qb.CloseQbicle(qbicId,"");
            //Assert
            Assert.IsNotNull(refModel);
        }
    }
}
