using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI.WebControls;


namespace Qbicles.BusinessRules.Micro
{
    public class MicroInventoryReportRules : MicroRulesBase
    {
        public MicroInventoryReportRules(MicroContext microContext) : base(microContext)
        {
        }

        public object GetOptionFilter(int domainId)
        {
            var Locations = dbContext.TraderLocations.Where(e => e.Domain.Id == domainId).OrderBy(e => e.Name).Select(e => new Select2CustomeModel { id = e.Id, text = e.Name }).ToList();

            return new
            {
                Locations = Locations,
                InventoryBasis = new List<SelectCustomeModel> { new SelectCustomeModel { id = "average", text = "Average cost (FIFO)" }, new SelectCustomeModel { id = "latest", text = "Latest cost" }, },
                DayToLastOperator = new List<Select2CustomeModel> {
                    new Select2CustomeModel { id = 1, text = "Last one week sales" },
                    new Select2CustomeModel { id = 2, text = "Last one month sales" },
                    new Select2CustomeModel { id = 3, text = "Custom range" }, },
            };
        }


        public object GetInventoryServerSide(InventoryReportFilterModel parameter)
        {
            var requestModel = new DataTablesRequest
            {
                Columns = null,
                Draw = 0,
                Length = parameter.pageSize,
                Search = new Search(parameter.keyword, true),
                Start = parameter.pageNumber * parameter.pageSize,
            };
            var result = new TraderInventoryRules(dbContext).GetInventoryServerSide(requestModel, parameter.LocationId, CurrentUser.ToUserSetting(), parameter.UnitsChanged,
                parameter.keyword, parameter.InventoryBasis, parameter.MaxDayToLast, parameter.Days2Last, parameter.DayToLastOperator);

            return new
            {
                Inventories = result.data,
                PageSize = result.recordsTotal / parameter.pageSize,
            };
        }


        /// <summary>
        /// return new Item value and UnitsChanged list to store localy
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public object UpdateInventoryUnit(InventoryReportFilterModel parameter)
        {
            var refModel = new TraderInventoryRules(dbContext).UpdateChangeItemUnit(CurrentUser.Id, parameter.UnitId, parameter.ItemId, parameter.LocationId, parameter.UnitsChanged,
                 parameter.InventoryBasis, parameter.MaxDayToLast, parameter.Days2Last, parameter.DayToLastOperator);

            return new
            {
                Item = refModel.Object,
                UnitsChanged = refModel.Object2
            };
            //SetDisplayUnitChangeCookies(cookieName, refModel.Object2);
        }
    }
}
