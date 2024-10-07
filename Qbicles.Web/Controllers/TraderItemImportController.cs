using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderItemImportController : BaseController
    {
        public ActionResult GetItemImports([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string datetime, int locationId)
        {
            var domainId = CurrentDomainId();
            var itemImports = new TraderItemImportRules(dbContext).GetItemImports(requestModel, keyword, datetime, domainId, locationId, CurrentUser());
            if (itemImports != null)
                return Json(itemImports, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult FileNameAndExtensionVerify(string fileName, int locationId)
        {
            var refModel = new TraderItemImportRules(dbContext).FileNameAndExtensionVerify(fileName, CurrentDomainId(), locationId);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveItemImport(TraderItemImport itemImport)
        {

            itemImport.Domain.Id = CurrentDomainId();
            itemImport.CreatedBy = new Qbicles.Models.ApplicationUser { Id = CurrentUser().Id };

            var refModel = new TraderItemImportRules(dbContext).SaveItemImport(itemImport);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteItemImport(string key)
        {
            var refModel = new TraderItemImportRules(dbContext).DeleteItemImport(key);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadImportedFile(FileModel file)
        {
            try
            {
                var currentUser = CurrentUser();

                var imported = dbContext.TraderItemImports.FirstOrDefault(e => e.SpreadsheetKey == file.Uri);
                var fileName = "";
                if (file.Name == "error")
                {
                    fileName = imported.SpreadsheetErrorsKey;
                    file.Uri = fileName;
                }
                else
                {
                    var files = imported.Spreadsheet.Split('.');
                    fileName = $"{files[0]}-{imported.CreatedDate.ToString("dd-MM-yyyy-hh-MM-ss")}.{files[1]}";
                }
                var fileString = AzureStorageHelper.SignedUrl(file.Uri, fileName);
                return Json(fileString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return null;
            }
        }
    }
}