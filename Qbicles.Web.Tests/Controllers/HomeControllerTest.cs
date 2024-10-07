using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
//using System.Net.Http;
//using System.Web.Http;
//using System.Web.Http.Routing;
//using System.Web.Http.Results;
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

namespace Qbicles.Web.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        ApplicationDbContext DbContext = new ApplicationDbContext();
        [TestMethod]
        public async Task loginTest()
        {
            try
            {
                //var controller = new AccountController();
                LoginViewModel model = new LoginViewModel
                {
                    Email = "bob@qbicles.com",
                    Password = "@Password1",
                    RememberMe = false
                };
                var objAccountController = new AccountController();
                string returnUrl = @"Home/Index";
                RedirectToRouteResult result = await objAccountController.Login(model, returnUrl) as RedirectToRouteResult;
                Assert.AreEqual("Login", result.RouteValues["action"]);
                Assert.IsNull(result.RouteValues["controller"]);
                Assert.IsNotNull(result);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            // Arrange

            // Act
            //var result = controller.Login(model, "/home");
            //Assert
            //Assert.IsNotNull(result);
        }
        [TestMethod]
        public void SendEmailFromController()
        {
            // Arrange
            IdentityMessage message = new IdentityMessage();
            EmailService e = new EmailService();
            // Act
            message.Body = "body";
            message.Subject = "ForgotPassword";
            message.Destination = "qan.soft@gmail.com";
            var send = e.SendAsync(message);
            //Assert
            Assert.IsNotNull(send);
        }

        [TestMethod]
        public void SendEmailFromRules()
        {
            //// Arrange
            //IdentityMessage message = new IdentityMessage();
            //EmailRules e = new EmailRules(DbContext);
            //// Act
            //message.Body = "body";
            //message.Subject = "ForgotPassword";
            //message.Destination = "qan.soft@gmail.com";
            //var send = e.SendEmail(message);
            ////Assert
            //Assert.IsNotNull(send);
        }

    }
}
