using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models.Trader.PoS;

namespace Qbicles.Web.Controllers
{
    public class PosTableController : BaseController
    {
        // GET: POSTable
        public ActionResult ShowAddEditTableModal(int posTableId)
        {
            var posTable = new PosTableRules(dbContext).GetPosTableById(posTableId);
            return PartialView("~/Views/PointOfSale/_AddEditTable.cshtml", posTable);
        }
        public ActionResult AddEditPosTable(POSTable posTable)
        {
            var saveResult = new PosTableRules(dbContext).SavePosTable(posTable, CurrentLocationManage(), CurrentUser().Id);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePosTable(int posTableId)
        {
            var deleteResult = new PosTableRules(dbContext).DeletePosTable(posTableId);
            return Json(deleteResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SavePosTableLayout(int locationId, S3ObjectUploadModel uploadModel = null)
        {
            var saveResult = new PosTableRules(dbContext).SavePosTableLayout(locationId, CurrentUser().Id, uploadModel);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeletePosTableLayout(int locationId)
        {
            var deleteResult = new PosTableRules(dbContext).DeletePosTableLayout(locationId);
            return Json(deleteResult, JsonRequestBehavior.AllowGet);
        }
    }
}