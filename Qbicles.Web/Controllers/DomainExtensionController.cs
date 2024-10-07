using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Qbicles.Models;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;

namespace Qbicles.Web.Controllers
{
    public class DomainExtensionController : BaseController
    {
        public ActionResult ChangeExtensionRequestStatus(int requestId, string domainKey, ExtensionRequestType type, ExtensionRequestStatus status, string note)
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey))
            {
                domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
            }

            var extensionRules = new DomainExtensionRules(dbContext);
            var request = extensionRules.GetRequestById(requestId);
            var creatorId = request?.CreatedBy?.Id ?? "";
            var currentUserId = CurrentUser().Id;
            var updateResult = extensionRules.UpdateExtensionRequestStatus(requestId, status, domainId, creatorId, currentUserId, type, note);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateMultipleExtensionRequestStatus(List<int> lstRequestIds, ExtensionRequestStatus status, string note) {
            var extensionRules = new DomainExtensionRules(dbContext);
            var currentUserId = CurrentUser().Id;
            foreach(var requestId in lstRequestIds)
            {
                var request = extensionRules.GetRequestById(requestId);
                if(request != null)
                {
                    var domainId = request.Domain.Id;
                    var creatorId = request.CreatedBy?.Id ?? "";
                    var type = request.Type;
                    extensionRules.UpdateExtensionRequestStatus(requestId, status, domainId, creatorId, currentUserId, type, note);
                }
            }
            return Json(new ReturnJsonModel() { result = true });
        }

        public ActionResult LoadExtensionRequestTableContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, 
            string keySearch, string dateRange, string createdUserIdSearch, ExtensionRequestType extensionTypeSearch,
            List<ExtensionRequestStatus> lstRequestStatusSearch, string domainKey, int start, int length, int draw)
        {
            try
            {
                var userSettings = CurrentUser();
                var dateTimeFormat = userSettings.DateTimeFormat;
                var dateFormat = userSettings.DateFormat;
                var timeZone = userSettings.Timezone;
                var totalRecord = 0;
                var domainId = 0;
                if (!string.IsNullOrEmpty(domainKey.Trim()))
                {
                    domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
                }
                List<DomainExtensionRequestCustomModel> lstResult = new DomainExtensionRules(dbContext).GetListExtensionRequestPagination(requestModel, keySearch, dateRange, createdUserIdSearch,
                    extensionTypeSearch, lstRequestStatusSearch, domainId, dateTimeFormat, dateFormat, timeZone, ref totalRecord, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<DomainExtensionRequestCustomModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetExtensionRequestById(int requestId)
        {
            var request = new DomainExtensionRules(dbContext).GetRequestById(requestId);
            if(request != null)
            {
                return Json(new ReturnJsonModel() { result = true, Object = new DomainExtensionRequestCustomModel { DomainName = request.Domain.Name, TypeName = request.Type.ToString()} }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new ReturnJsonModel { result = false, msg = "Get Extension Request information failed" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}