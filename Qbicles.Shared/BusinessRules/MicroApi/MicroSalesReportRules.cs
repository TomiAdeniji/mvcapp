using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroSalesReportRules : MicroRulesBase
    {
        public MicroSalesReportRules(MicroContext microContext) : base(microContext)
        {
        }

        public object GetOptionFilter(int domainId)
        {
            var SalesChannel = Enum.GetNames(typeof(SalesChannelEnum)).Where(e => e != "Trader").Select(e => new SelectCustomeModel { id = e, text = e }).ToList();
            var Contacts = dbContext.TraderContacts.Where(d => d.ContactGroup.Domain.Id == domainId).OrderBy(n => n.Name).Select(e => new Select2CustomeModel { id = e.Id, text = e.Name }).ToList();
            var Locations = dbContext.TraderLocations.Where(e => e.Domain.Id == domainId).OrderBy(e => e.Name).Select(e => new Select2CustomeModel { id = e.Id, text = e.Name }).ToList();

            return new
            {
                Locations = Locations.Prepend(new Select2CustomeModel { id = 0, text = "Show all" }),
                SalesChannel = SalesChannel.Prepend(new SelectCustomeModel { id = "", text = "Show all" }),
                Contacts = Contacts.Prepend(new Select2CustomeModel { id = 0, text = "Show all" }),
            };
        }

        public object GetSummaries(SalesReportFilterParameter filter)
        {
            var dateTimeFormat = new UserSetting
            {
                DateFormat = CurrentUser.DateFormat,
                TimeFormat = CurrentUser.TimeFormat,
                Timezone = CurrentUser.Timezone
            };
            var result = new TraderSaleRules(dbContext).GetDataDashBoard(filter.DomainId, filter.LocationId, dateTimeFormat, "", 0, filter.DateRange, filter.SaleChanel, filter.ContactId);

            var currentcy = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(filter.DomainId);
            var summaries = new
            {
                summary = new
                {
                    Revenue = $"{currentcy.CurrencySymbol}{result.TotalSaleValue}",//.ToCurrencySymbol(currentcy);
                    Transactions = result.TotalApproved,
                },
                TopSellers = result.TopSells.Take(10).Select(e => new TopValueSale
                {
                    GroupId = e.GroupId,
                    GroupName = e.GroupName,
                    Percent = $"{e.Percent}%",
                    TraderItemIds = string.Join("-", e.TraderItemIds.Split('-').ToList().Select(s => int.Parse(s)).Distinct()),
                    Value = $"{currentcy.CurrencySymbol}{e.Value}"
                }),
                TopMargins = result.TopMargin.Take(10).Select(e => new TopValueSale
                {
                    GroupId = e.GroupId,
                    GroupName = e.GroupName,
                    Percent = $"{e.Percent}%",
                    TraderItemIds = string.Join("-", e.TraderItemIds.Split('-').ToList().Select(s => int.Parse(s)).Distinct()),
                    Value = "",//$"{currentcy.CurrencySymbol} {e.Value}"
                }),
                TopGrossMargions = result.TopGrossMargion.Take(10).Select(e => new TopValueSale
                {
                    GroupId = e.GroupId,
                    GroupName = e.GroupName,
                    Percent = "",// $"{e.Percent}%",
                    TraderItemIds = string.Join("-", e.TraderItemIds.Split('-').ToList().Select(s => int.Parse(s)).Distinct()),
                    Value = $"{currentcy.CurrencySymbol}{e.Value}"
                })
            };
            return summaries;
        }

        public object ProductGroupDetail(SalesReportFilterParameter filter)
        {
            var currentcy = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(filter.DomainId);
            var lstId = new List<int>();
            if (!string.IsNullOrEmpty(filter.TraderItemIds))
            {
                lstId = filter.TraderItemIds.Split('-').ToList().Select(s => int.Parse(s)).Distinct().ToList();
            }
            var dateTimeFormat = new UserSetting
            {
                DateFormat = CurrentUser.DateFormat,
                TimeFormat = CurrentUser.TimeFormat,
                Timezone = CurrentUser.Timezone
            };
            var result = new TraderSaleRules(dbContext).GetItemsByGroupId(lstId, filter.DomainId, filter.LocationId, dateTimeFormat, filter.DateRange, CurrentUser.Timezone, true);

            return result.Select(q => new
            {
                Image = q.TraderItem.ImageUri.ToUri(),
                q.TraderItem.Name,
                q.TraderItem.SKU,
                q.TraderItem.Barcode,
                q.TraderItem.Description,
                Price = $"{currentcy.CurrencySymbol}{q.Price}",
                Cost = $"{currentcy.CurrencySymbol}{q.Cost}",
                Quantity = $"{q.Quantity}",
                Margin = $"{q.Margin}",

            });
        }
    }
}
