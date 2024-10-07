using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules;
using Qbicles.Models;

namespace Qbicles.Web.Tests.Controllers
{
    [TestClass]
    public class MediasRulesTest
    {

        ApplicationDbContext DbContext = new ApplicationDbContext();
        ReturnJsonModel refModel = new ReturnJsonModel();

        [TestMethod()]
        public void SaveMediaTest()
        {
            refModel = new ReturnJsonModel();
            QbicleMedia media = new QbicleMedia();
            MediasRules mediaRule = new MediasRules(DbContext);
            media.Description = "Descript test";
            media.Name = "Media name test";
            string attactFile = "/Content/DesignStyle/img/user2-160x160.jpg";
            media.Uri = attactFile;
            media.FileSize = "5M";
            media.FileType = new FileTypeRules().GetFileTypeByExtension("img");
            
            //check duplicate Task name
            refModel.result = mediaRule.DuplicateMediaNameCheck(media.Id, media.Name);
            if (!refModel.result)
                refModel.result = mediaRule.SaveMedia(media);
            //Assert
            Assert.IsNotNull(refModel);
        }
    }
}
