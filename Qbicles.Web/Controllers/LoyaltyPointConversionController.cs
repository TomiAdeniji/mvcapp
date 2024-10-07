using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Loyalty;

namespace Qbicles.Web.Controllers
{
    public class LoyaltyPointConversionController : BaseController
    {
        // GET: LoyaltyPointConversion
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddConversion(PaymentConversion conversion)
        {
            var domainId = CurrentDomainId();
            var currentUserId = CurrentUser().Id;
            var addResult = new StorePointConversionRules(dbContext).AddConversion(domainId, currentUserId, conversion);
            return Json(addResult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LoyaltyPaymentConversionDTContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, OrderToPointsConversionType conversiontye, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                var userSettings = CurrentUser();
                var currentDomainId = CurrentDomainId();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);
                var lstResult = new StorePointConversionRules(dbContext).GetPaymentConversionsPagination(userSettings.DateTimeFormat, userSettings.Timezone, currencySettings, conversiontye, ref totalRecord, requestModel, currentDomainId, true);
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
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<LoyaltyConversionCustomModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddLoyaltySystemSetting(SystemSettings sysSettings)
        {
            var currentUserId = CurrentUser().Id;
            var addResult = new StorePointConversionRules(dbContext).AddLoyaltySystemSettings(currentUserId, sysSettings);
            return Json(addResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDomainLoyaltySetting(int workGroupId = 0, bool isDebitPaymentAllowed = false)
        {
            var currentDomainId = CurrentDomainId();
            var updateRs = new StorePointConversionRules(dbContext).UpdateDomainLoyaltySettings(workGroupId, isDebitPaymentAllowed, currentDomainId);
            return Json(updateRs, JsonRequestBehavior.AllowGet);
        }

    }
}