using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Budgets;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderBudgetGroupRules
    {
        ApplicationDbContext dbContext;

        public TraderBudgetGroupRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public List<BudgetGroup> GetBudgetGroupsByLocation(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                return dbContext.BudgetGroups.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, domainId);
                return new List<BudgetGroup>();
            }
        }

        public BudgetGroup GetBudGroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.BudgetGroups.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new BudgetGroup();
            }
        }

        public List<PaymentTerms> GetPaymentTerms()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod());
                var paymentTerms = dbContext.PaymentTerms;
                return paymentTerms.Any() ? paymentTerms.ToList() : new List<PaymentTerms>();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<PaymentTerms>();
            }
        }

        public List<TraderGroup> GetTraderGroups(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var traderGroup = dbContext.TraderGroups.Where(e => e.Domain.Id == domainId).ToList();
                if (traderGroup.Any())
                    traderGroup = traderGroup.Select(q => new TraderGroup()
                    {
                        Id = q.Id,
                        Name = q.Name,
                        Items = q.Items.Any()
                            ? q.Items.Select(i => new TraderItem()
                            {
                                Id = i.Id,
                                IsBought = i.IsBought,
                                IsSold = i.IsSold,
                                Name = i.Name,
                                ImageUri = i.ImageUri,
                                SKU = i.SKU,
                                Group = new TraderGroup() { Name = i.Group.Name }
                            }).ToList()
                            : new List<TraderItem>(),
                        ExpenditureBudgetGroup = q.ExpenditureBudgetGroup,
                        RevenueBudgetGroup = q.RevenueBudgetGroup
                    }).ToList();
                else traderGroup = new List<TraderGroup>();
                return traderGroup;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderGroup>();
            }
        }

        public ReturnJsonModel SaveBudgetGroup(BudgetGroup budgetGroup, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0", result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, budgetGroup);
                var user = dbContext.QbicleUser.Find(userId);
                if (budgetGroup.ExpenditureGroups != null && budgetGroup.ExpenditureGroups.Count > 0)
                {
                    for (int i = 0; i < budgetGroup.ExpenditureGroups.Count; i++)
                    {
                        budgetGroup.ExpenditureGroups[i] = dbContext.TraderGroups.Find(budgetGroup.ExpenditureGroups[i].Id);
                    }
                }

                if (budgetGroup.RevenueGroups != null && budgetGroup.RevenueGroups.Count > 0)
                {
                    for (int i = 0; i < budgetGroup.RevenueGroups.Count; i++)
                    {
                        budgetGroup.RevenueGroups[i] = dbContext.TraderGroups.Find(budgetGroup.RevenueGroups[i].Id);
                    }
                }

                budgetGroup.LastUpdateDate = DateTime.UtcNow;
                budgetGroup.LastUpdatedBy = user;
                budgetGroup.Location = dbContext.TraderLocations.Find(budgetGroup.Location.Id);
                if (budgetGroup.PaymentTerms != null && budgetGroup.PaymentTerms.Id > 0)
                    budgetGroup.PaymentTerms = dbContext.PaymentTerms.Find(budgetGroup.PaymentTerms.Id);

                if (budgetGroup.Id == 0)
                {
                    budgetGroup.CreatedBy = user;
                    budgetGroup.CreatedDate = DateTime.UtcNow;
                    dbContext.Entry(budgetGroup).State = EntityState.Added;
                    dbContext.BudgetGroups.Add(budgetGroup);
                    dbContext.SaveChanges();

                    result.msgId = budgetGroup.Id.ToString();
                }
                else
                {
                    var ubudgetGroup = dbContext.BudgetGroups.Find(budgetGroup.Id);
                    ubudgetGroup = ClearExRevenueRelationship(ubudgetGroup);
                    ubudgetGroup.ExpenditureGroups.Clear();
                    ubudgetGroup.RevenueGroups.Clear();
                    dbContext.Entry(ubudgetGroup).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    ubudgetGroup.ExpenditureGroups = budgetGroup.ExpenditureGroups;
                    ubudgetGroup.RevenueGroups = budgetGroup.RevenueGroups;
                    ubudgetGroup.LastUpdateDate = DateTime.UtcNow;
                    ubudgetGroup.LastUpdatedBy = user;
                    ubudgetGroup.PaymentTerms = budgetGroup.PaymentTerms;
                    ubudgetGroup.Type = budgetGroup.Type;
                    ubudgetGroup.Title = budgetGroup.Title;
                    ubudgetGroup.Description = budgetGroup.Description;

                    dbContext.Entry(ubudgetGroup).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    result.msgId = ubudgetGroup.Id.ToString();
                    result.actionVal = 2;

                }

                var budgetGroupUpdate = dbContext.BudgetGroups.Find(budgetGroup.Id);
                if (budgetGroupUpdate != null)
                {
                    if (budgetGroupUpdate.ExpenditureGroups != null && budgetGroupUpdate.ExpenditureGroups.Count > 0)
                    {
                        for (int i = 0; i < budgetGroupUpdate.ExpenditureGroups.Count; i++)
                        {
                            budgetGroupUpdate.ExpenditureGroups[i].ExpenditureBudgetGroup = budgetGroupUpdate;
                        }
                    }

                    if (budgetGroupUpdate.RevenueGroups != null && budgetGroupUpdate.RevenueGroups.Count > 0)
                    {
                        for (int i = 0; i < budgetGroupUpdate.RevenueGroups.Count; i++)
                        {
                            budgetGroupUpdate.RevenueGroups[i].RevenueBudgetGroup = budgetGroupUpdate;
                        }
                    }

                    dbContext.Entry(budgetGroupUpdate).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }


            }
            catch (Exception e)
            {
                result.actionVal = 3;
                result.msg = e.Message;
                result.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, budgetGroup);
            }
            return result;
        }

        private BudgetGroup ClearExRevenueRelationship(BudgetGroup budget)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, budget);
                var budgetRelationship = dbContext.BudgetGroups.Find(budget.Id);
                if (budgetRelationship == null)
                    return null;

                if (budgetRelationship.ExpenditureGroups.Any())
                {
                    foreach (var item in budgetRelationship.ExpenditureGroups)
                    {
                        item.ExpenditureBudgetGroup = null;
                    }
                }

                if (budgetRelationship.RevenueGroups.Any())
                {
                    foreach (var item in budgetRelationship.RevenueGroups)
                    {
                        item.RevenueBudgetGroup = null;
                    }
                }

                dbContext.SaveChanges();
                return budgetRelationship;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, budget);
                return new BudgetGroup();
            }
        }

        public ReturnJsonModel DeleteBudgetGroup(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = true, actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var budgetGroup = dbContext.BudgetGroups.Find(id);
                budgetGroup = ClearExRevenueRelationship(budgetGroup);
                budgetGroup.ExpenditureGroups.Clear();
                budgetGroup.RevenueGroups.Clear();
                dbContext.Entry(budgetGroup).State = EntityState.Deleted;
                dbContext.BudgetGroups.Remove(budgetGroup);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }

            return refModel;
        }

        // Budget Scenarions

        public List<BudgetScenario> GetBudgetScenariosByLocation(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                return dbContext.BudgetScenarios.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId)
                    .ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, domainId);
                return new List<BudgetScenario>();
            }

        }

        public BudgetScenario GetBudgetScenarioById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.BudgetScenarios.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new BudgetScenario();
            }
        }

        public bool CheckExistsTitle(string title, int id, int locationId, int domainId)
        {
            var result = true; // exists
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, title, id, locationId, domainId);
                var budgetScenario = dbContext.BudgetScenarios.Where(q =>
                    q.Title.ToLower() == title.ToLower() && q.Location.Id == locationId && q.Domain.Id == domainId &&
                    q.Id != id);
                if (budgetScenario.Any())
                {
                    result = false;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, title, id, locationId, domainId);
            }
            return result;
        }

        private DateTime GetMondayDate(DateTime date, bool first = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, date, first);
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Tuesday:
                        date = first ? date.AddDays(-1) : date.AddDays(6);
                        break;
                    case DayOfWeek.Wednesday:
                        date = first ? date.AddDays(-2) : date.AddDays(5);
                        break;
                    case DayOfWeek.Thursday:
                        date = first ? date.AddDays(-3) : date.AddDays(4);
                        break;
                    case DayOfWeek.Friday:
                        date = first ? date.AddDays(-4) : date.AddDays(3);
                        break;
                    case DayOfWeek.Saturday:
                        date = first ? date.AddDays(-5) : date.AddDays(2);
                        break;
                    case DayOfWeek.Sunday:
                        date = first ? date.AddDays(-6) : date.AddDays(1);
                        break;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, date, first);
            }
            return date;
        }

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        private int GetIso8601WeekOfYear(DateTime time)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, time);
                // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
                // be the same week# as whatever Thursday, Friday or Saturday are,
                // and we always get those right
                DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
                if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                {
                    time = time.AddDays(3);
                }

                // Return the week of our adjusted day
                return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, time);
            }
            return 0;
        }

        public ReturnJsonModel SaveBudgetScenario(BudgetScenario budgetScenario, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0", result = true };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, budgetScenario);
                var user = dbContext.QbicleUser.Find(userId);
                budgetScenario.Location = dbContext.TraderLocations.Find(budgetScenario.Location.Id);
                var lstBudgetGroup = new List<BudgetGroup>();

                lstBudgetGroup.AddRange(budgetScenario.BudgetGroups);
                budgetScenario.BudgetGroups.Clear();

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                if (budgetScenario.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(budgetScenario.FeaturedImage);
                }
                else
                {
                    var mediaValid = dbContext.BudgetScenarios.Find(budgetScenario.Id);
                    if (mediaValid.FeaturedImage != budgetScenario.FeaturedImage)
                        s3Rules.ProcessingMediaS3(budgetScenario.FeaturedImage);
                }

                if (budgetScenario.Id == 0)
                {
                    budgetScenario.CreatedBy = user;
                    budgetScenario.CreatedDate = DateTime.UtcNow;
                    dbContext.Entry(budgetScenario).State = EntityState.Added;
                    dbContext.BudgetScenarios.Add(budgetScenario);
                    dbContext.SaveChanges();
                    result.msgId = budgetScenario.Id.ToString();
                }
                else
                {
                    var ubudgetScenario = dbContext.BudgetScenarios.Find(budgetScenario.Id);
                    ubudgetScenario.BudgetGroups.Clear();
                    //dbContext.ScenarioItemStartingQuantities.RemoveRange(ubudgetScenario.ScenarioItemStartingQuantities);
                    //ubudgetScenario.ScenarioItemStartingQuantities.Clear();
                    dbContext.ReportingPeriods.RemoveRange(ubudgetScenario.ReportingPeriods);
                    ubudgetScenario.ReportingPeriods.Clear();
                    dbContext.SaveChanges();
                    ubudgetScenario.Title = budgetScenario.Title;
                    ubudgetScenario.Description = budgetScenario.Description;
                    ubudgetScenario.FeaturedImage = budgetScenario.FeaturedImage;
                    ubudgetScenario.FiscalEndPeriod = budgetScenario.FiscalEndPeriod;
                    ubudgetScenario.FiscalStartPeriod = budgetScenario.FiscalStartPeriod;
                    ubudgetScenario.ReportingPeriod = budgetScenario.ReportingPeriod;
                    ubudgetScenario.LastUpdateDate = DateTime.UtcNow;
                    ubudgetScenario.LastUpdatedBy = user;
                    dbContext.Entry(ubudgetScenario).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    result.msgId = ubudgetScenario.Id.ToString();
                    result.actionVal = 2;
                }

                budgetScenario = dbContext.BudgetScenarios.Find(budgetScenario.Id);

                if (lstBudgetGroup.Any())
                {
                    for (int i = 0; i < lstBudgetGroup.Count; i++)
                    {
                        lstBudgetGroup[i] = dbContext.BudgetGroups.Find(lstBudgetGroup[i].Id);
                        if (!lstBudgetGroup[i].BudgetScenarios.Select(q => q.Id).Contains(budgetScenario.Id))
                            lstBudgetGroup[i].BudgetScenarios.Add(budgetScenario);
                    }
                }

                budgetScenario.BudgetGroups = lstBudgetGroup;
                dbContext.SaveChanges();
                // Smallest Reporting Period
                if (budgetScenario.ReportingPeriod == ReportingPeriodEnum.Monthly)
                {
                    budgetScenario.ReportingPeriods.Clear();
                    var dateCurrently = budgetScenario.FiscalStartPeriod;
                    int counter = 1;
                    for (DateTime dt = budgetScenario.FiscalStartPeriod;
                        dt < budgetScenario.FiscalEndPeriod;
                        dt = dt.AddMonths(1))
                    {
                        budgetScenario.ReportingPeriods.Add(new ReportingPeriod()
                        {
                            Id = 0,
                            BudgetScenario = budgetScenario,
                            Start = dt,
                            End = dt.AddMonths(1),
                            Name = dt.ToString("MMM") + " " + dt.Year,
                            Order = counter
                        });
                        counter++;
                    }
                }
                else if (budgetScenario.ReportingPeriod == ReportingPeriodEnum.Quarterly)
                {
                    int counter = 1;
                    for (DateTime dt = budgetScenario.FiscalStartPeriod;
                        dt < budgetScenario.FiscalEndPeriod;
                        dt = dt.AddMonths(3))
                    {
                        var name = dt.ToString("yyyy");
                        switch (dt.Month)
                        {
                            case 1:
                                name = "Q1 " + name;
                                break;
                            case 4:
                                name = "Q2 " + name;
                                break;
                            case 7:
                                name = "Q3 " + name;
                                break;
                            case 10:
                                name = "Q4 " + name;
                                break;
                        }

                        budgetScenario.ReportingPeriods.Add(new ReportingPeriod()
                        {
                            Id = 0,
                            BudgetScenario = budgetScenario,
                            Start = dt,
                            End = dt.AddMonths(3),
                            Name = name,
                            Order = counter
                        });
                        counter++;
                    }
                }
                else if (budgetScenario.ReportingPeriod == ReportingPeriodEnum.Weekly)
                {
                    var startDate = GetMondayDate(budgetScenario.FiscalStartPeriod);
                    var endDate = GetMondayDate(budgetScenario.FiscalEndPeriod);
                    int counter = 1;
                    for (DateTime dt = startDate; dt < endDate; dt = dt.AddDays(7))
                    {
                        budgetScenario.ReportingPeriods.Add(new ReportingPeriod()
                        {
                            Id = 0,
                            BudgetScenario = budgetScenario,
                            Start = dt,
                            End = dt.AddDays(7),
                            Name = dt.ToString("dd/MM/yyyy") + " (W " + GetIso8601WeekOfYear(dt) + ")",
                            Order = counter
                        });
                        counter++;
                    }
                }

                dbContext.SaveChanges();
                //Starting Quantities
                if (budgetScenario.BudgetGroups.Any())
                {
                    List<TraderItem> traderItems = new List<TraderItem>();
                    foreach (var bgGroup in budgetScenario.BudgetGroups)
                    {
                        var lstTraderGroup = new List<TraderGroup>();
                        if (bgGroup.ExpenditureGroups.Any())
                            lstTraderGroup.AddRange(bgGroup.ExpenditureGroups);
                        else lstTraderGroup.AddRange(bgGroup.RevenueGroups);
                        if (lstTraderGroup.Any())
                        {
                            List<TraderItem> items = new List<TraderItem>();
                            if (bgGroup.Type == BudgetGroupType.Expenditure)
                                items = lstTraderGroup.SelectMany(q => q.Items.Where(x =>
                                        x.IsBought && x.Locations.Select(i => i.Id)
                                            .Contains(budgetScenario.Location.Id)))
                                    .ToList();
                            else
                                items = lstTraderGroup.SelectMany(q => q.Items.Where(x =>
                                        x.IsSold && x.Locations.Select(i => i.Id).Contains(budgetScenario.Location.Id)))
                                    .ToList();
                            traderItems.AddRange(items);
                        }
                    }

                    traderItems = traderItems.Distinct().ToList();
                    var quantities = new List<ScenarioItemStartingQuantity>();
                    // delete ScenarioItems with quantities
                    foreach (var quantity in budgetScenario.ScenarioItemStartingQuantities)
                    {
                        if (!traderItems.Contains(quantity.Item))
                        {
                            if (quantity.ScenarioItems.Any())
                            {
                                dbContext.BudgetScenarioItems.RemoveRange(quantity.ScenarioItems);
                                quantity.ScenarioItems.Clear();
                                dbContext.SaveChanges();
                            }

                            quantities.Add(quantity);
                        }
                    }

                    // delete scenarioItem startingQuantiities not contains traderitems
                    if (quantities.Any())
                    {
                        dbContext.ScenarioItemStartingQuantities.RemoveRange(quantities);
                        foreach (var scenarioItemStartingQuantity in quantities)
                        {
                            budgetScenario.ScenarioItemStartingQuantities.Remove(scenarioItemStartingQuantity);
                        }

                        dbContext.SaveChanges();
                    }

                    // add quantities in traderitems and not contains quantities
                    foreach (var traderItem in traderItems)
                    {
                        if (!budgetScenario.ScenarioItemStartingQuantities.Any(q => q.Item.Id == traderItem.Id))
                        {
                            if (traderItem.Units == null || traderItem.Units.Count == 0)
                                continue;
                            budgetScenario.ScenarioItemStartingQuantities.Add(new ScenarioItemStartingQuantity()
                            {
                                Id = 0,
                                BudgetScenario = budgetScenario,
                                Item = traderItem,
                                Unit = traderItem.Units.FirstOrDefault()
                            });
                        }
                    }
                }

                dbContext.Entry(budgetScenario).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, budgetScenario);
                result.result = false;
                result.actionVal = 3;
                result.msg = e.Message;
            }
            return result;
        }

        public ReturnJsonModel SetActiveBudgetScenario(int id, int locationId, int domainId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0", result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, locationId, domainId);
                // update is active current = false;
                var currentActive =
                    dbContext.BudgetScenarios.Where(q =>
                        q.Location.Id == locationId && q.Domain.Id == domainId && q.IsActive);
                if (currentActive.Any())
                {
                    foreach (var active in currentActive)
                    {
                        active.IsActive = false;
                    }

                    dbContext.SaveChanges();
                }

                // set active 
                var budgetScenảioActive = dbContext.BudgetScenarios.Find(id);
                if (budgetScenảioActive != null)
                {
                    budgetScenảioActive.IsActive = true;
                    dbContext.Entry(budgetScenảioActive).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                result.actionVal = 3;
                result.msg = e.Message;
                result.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, locationId, domainId);
            }
            return result;
        }

        public ReturnJsonModel UpdateScenarioItemStartingUnit(ScenarioItemStartingQuantity startingQuantity)
        {
            var result = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, startingQuantity);
                var itemStarting =
                    dbContext.ScenarioItemStartingQuantities.FirstOrDefault(q => q.Id == startingQuantity.Id);
                if (itemStarting == null) return result;

                itemStarting.Unit = dbContext.ProductUnits.FirstOrDefault(e => e.Id == startingQuantity.Unit.Id);
                dbContext.Entry(itemStarting).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                result.msg = e.Message;
                result.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, startingQuantity);
            }
            return result;
        }

        public ReturnJsonModel UpdateScenarioItemStartingQuantity(ScenarioItemStartingQuantity startingQuantity)
        {
            var result = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, startingQuantity);
                var itemStarting =
                    dbContext.ScenarioItemStartingQuantities.FirstOrDefault(q => q.Id == startingQuantity.Id);
                if (itemStarting == null) return result;

                itemStarting.Quantity = startingQuantity.Quantity;
                dbContext.Entry(itemStarting).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                result.msg = e.Message;
                result.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, startingQuantity);
            }
            return result;
        }


        public List<BudgetScenarioItemGroup> GetBudgetScenarioItemGroupsByScenarioId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.BudgetScenarioItemGroups.Where(e => e.BudgetScenario.Id == id).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new List<BudgetScenarioItemGroup>();
            }
        }


        public List<BudgetPanelReport> BudgetPanelReport(int id, int domainId, int locationId)
        {
            var panelReports = new List<BudgetPanelReport>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, locationId, domainId);
                var scenario = dbContext.BudgetScenarios.Find(id);

                var reportingPeriod = scenario.ReportingPeriod;

                var dtNow = DateTime.UtcNow;
                var dtFrom = DateTime.UtcNow;
                var dtTo = DateTime.UtcNow;

                switch (reportingPeriod)
                {
                    case ReportingPeriodEnum.Weekly:
                        var day = dtNow.DayOfWeek;
                        var days = day - DayOfWeek.Monday;
                        dtFrom = dtNow.AddDays(-days).Date;
                        dtTo = dtFrom.AddDays(6).Date.AddDays(1).AddMinutes(-1);
                        break;
                    case ReportingPeriodEnum.Monthly:
                        dtFrom = new DateTime(dtNow.Year, dtNow.Month, 1).Date;
                        dtTo = dtFrom.AddMonths(1).AddDays(-1).Date.AddDays(1).AddMinutes(-1);
                        break;
                    case ReportingPeriodEnum.Quarterly:
                        var curQuarter = (dtNow.Month - 1) / 3 + 1;
                        dtFrom = new DateTime(dtNow.Year, 3 * curQuarter - 2, 1).Date;
                        dtTo = new DateTime(dtNow.Year, 3 * curQuarter + 1, 1).AddDays(-1).Date.AddDays(1).AddMinutes(-1);
                        break;
                }


                scenario.BudgetGroups.ForEach(g =>
                {
                    var panel = new BudgetPanelReport
                    {
                        BudgetScenarioId = scenario.Id,
                        BudgetGroupId = g.Id,
                        Name = g.Title,
                        ReportingPeriod = reportingPeriod.GetDescription()
                    };





                    switch (g.Type)
                    {
                        case BudgetGroupType.Revenue:
                            panel.Amount = dbContext.ItemProjections.Where(t =>
                                t.BudgetScenarioItem.BudgetScenarioItemGroup.BudgetScenario.Id == id
                                && (t.BudgetScenarioItem.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsISell ||
                                    t.BudgetScenarioItem.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuyAndSell)
                                && t.BudgetScenarioItem.Item.Group.RevenueBudgetGroup.Id == g.Id).Sum(e => e.RevenueValue);

                            panel.TotalItems = dbContext.BudgetScenarioItems.Count(e =>
                                e.Item.Group.RevenueBudgetGroup.Id == g.Id &&
                                e.BudgetScenarioItemGroup.BudgetScenario.Id == id &&
                                (e.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsISell ||
                                 e.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuyAndSell));

                            var transactionSaleItems = from sig in dbContext.BudgetScenarioItems
                                                       join item in dbContext.TraderItems on sig.Item.Id equals item.Id
                                                       join tr in dbContext.TraderSaleItems on item.Id equals tr.TraderItem.Id
                                                       where sig.BudgetScenarioItemGroup.BudgetScenario.Id == id
                                                             && sig.Item.Group.RevenueBudgetGroup.Id == g.Id &&
                                                             (sig.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsISell ||
                                                              sig.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuyAndSell)
                                                       select tr.Id;
                            var saleLogs = (from ts in dbContext.TraderSaleLogs
                                            where ts.Location.Domain.Id == domainId && ts.Location.Id == locationId
                                                                                    && ts.CreatedDate < scenario.FiscalEndPeriod &&
                                                                                    ts.CreatedDate > scenario.FiscalStartPeriod
                                                                                    && (ts.Status ==
                                                                                        TraderSaleStatusEnum.SaleApproved ||
                                                                                        ts.Status == TraderSaleStatusEnum
                                                                                            .SalesOrderedIssued)
                                                                                    && ts.SaleItems.Any(s =>
                                                                                        transactionSaleItems.Contains(s.Id))
                                            select ts).ToList();

                            panel.SinceFiscalStartDate = saleLogs.Select(e => e.SaleItems.Sum(p => p.Price)).Sum();


                            saleLogs = saleLogs.Where(d => d.CreatedDate < dtTo && d.CreatedDate > dtFrom).ToList();
                            panel.PeriodValue = saleLogs.Select(e => e.SaleItems.Sum(p => p.Price)).Sum();

                            break;
                        case BudgetGroupType.Expenditure:
                            panel.Amount = dbContext.ItemProjections.Where(t =>
                                    t.BudgetScenarioItem.BudgetScenarioItemGroup.BudgetScenario.Id == id
                                    && (t.BudgetScenarioItem.BudgetScenarioItemGroup.Type ==
                                        ItemGroupType.ItemsIBuyAndSell ||
                                        t.BudgetScenarioItem.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuy)
                                    && t.BudgetScenarioItem.Item.Group.ExpenditureBudgetGroup.Id == g.Id)
                                .Sum(e => e.ExpenditureValue);

                            panel.TotalItems = dbContext.BudgetScenarioItems.Count(e =>
                                e.Item.Group.ExpenditureBudgetGroup.Id == g.Id &&
                                e.BudgetScenarioItemGroup.BudgetScenario.Id == id &&
                                (e.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuyAndSell ||
                                 e.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuy));

                            var transactionPurchaseItems = from sig in dbContext.BudgetScenarioItems
                                                           join item in dbContext.TraderItems on sig.Item.Id equals item.Id
                                                           join tr in dbContext.TraderSaleItems on item.Id equals tr.TraderItem.Id
                                                           where sig.BudgetScenarioItemGroup.BudgetScenario.Id == id
                                                                 && sig.Item.Group.ExpenditureBudgetGroup.Id == g.Id &&
                                                                 (sig.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuyAndSell ||
                                                                  sig.BudgetScenarioItemGroup.Type == ItemGroupType.ItemsIBuy)
                                                           select tr.Id;
                            var purchaseLogs = (from ts in dbContext.TraderPurchaseLogs
                                                where ts.Location.Domain.Id == domainId && ts.Location.Id == locationId
                                                                                        && ts.CreatedDate < scenario.FiscalEndPeriod &&
                                                                                        ts.CreatedDate > scenario.FiscalStartPeriod
                                                                                        && (ts.Status == TraderPurchaseStatusEnum.PurchaseApproved ||
                                                                                            ts.Status == TraderPurchaseStatusEnum.PurchaseOrderIssued)
                                                                                        && ts.PurchaseItems.Any(s =>
                                                                                            transactionPurchaseItems.Contains(s.Id))
                                                select ts).ToList(); ;


                            panel.SinceFiscalStartDate = purchaseLogs.Select(e => e.PurchaseItems.Sum(p => p.Cost)).Sum();


                            purchaseLogs = purchaseLogs.Where(d => d.CreatedDate < dtTo && d.CreatedDate > dtFrom).ToList();
                            panel.PeriodValue = purchaseLogs.Select(e => e.PurchaseItems.Sum(p => p.Cost)).Sum();

                            break;
                    }

                    if (panel.Amount == 0)
                        panel.Percentage = 0;
                    else
                        panel.Percentage = panel.SinceFiscalStartDate / panel.Amount;

                    panelReports.Add(panel);

                });
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, domainId, locationId);
            }
            return panelReports;
        }


        public DataTablesResponse GetDataTableBudgetGroupReport(IDataTablesRequest requestModel, int domainId, int locationId, string timeZone, string dateTimeFormat,
            int budgetScenarioId, int budgetGroupId, string dimensions, int wgId, string dateRange)
        {
            try
            {
                //Get the filtered sales, get all sales not just the approved sales
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, locationId, timeZone,
                        dateTimeFormat, budgetScenarioId, budgetGroupId, dimensions, wgId, dateRange);
                var budgetGroupReports = FilteredBudgetGroupReport(domainId, locationId, timeZone, dateTimeFormat, budgetScenarioId, budgetGroupId, dateRange, dimensions, wgId);

                if (budgetGroupReports == null)
                {
                    return null;
                }

                var totalReport = budgetGroupReports.Count();


                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();

                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "FullRef":
                            budgetGroupReports = column.SortDirection == TB_Column.OrderDirection.Ascendant ?
                                budgetGroupReports.OrderBy(e => e.FullRef).ToList() : budgetGroupReports.OrderByDescending(e => e.FullRef).ToList();
                            break;
                        case "Item":

                            budgetGroupReports = column.SortDirection == TB_Column.OrderDirection.Ascendant ?
                                budgetGroupReports.OrderBy(e => e.Item).ToList() : budgetGroupReports.OrderByDescending(e => e.Item).ToList();
                            break;
                        case "Date":
                            budgetGroupReports = column.SortDirection == TB_Column.OrderDirection.Ascendant ?
                                budgetGroupReports.OrderBy(e => e.Date).ToList() : budgetGroupReports.OrderByDescending(e => e.Date).ToList();
                            break;
                        case "Unit":
                            budgetGroupReports = column.SortDirection == TB_Column.OrderDirection.Ascendant ?
                                budgetGroupReports.OrderBy(e => e.Unit).ToList() : budgetGroupReports.OrderByDescending(e => e.Unit).ToList();
                            break;
                        case "Total":
                            budgetGroupReports = column.SortDirection == TB_Column.OrderDirection.Ascendant ?
                                budgetGroupReports.OrderBy(e => e.Total).ToList() : budgetGroupReports.OrderByDescending(e => e.Total).ToList();
                            break;
                        case "Dimension":
                            budgetGroupReports = column.SortDirection == TB_Column.OrderDirection.Ascendant ?
                                budgetGroupReports.OrderBy(e => e.Dimension).ToList() : budgetGroupReports.OrderByDescending(e => e.Dimension).ToList();
                            break;
                        default:
                            budgetGroupReports = column.SortDirection == TB_Column.OrderDirection.Ascendant ?
                                budgetGroupReports.OrderBy(e => e.FullRef).ToList() : budgetGroupReports.OrderByDescending(e => e.FullRef).ToList();
                            break;
                    }
                }


                #endregion

                var totalAmount = budgetGroupReports.Sum(t => t.TotalValue);

                var reports = budgetGroupReports.Select(r => new TraderBudgetGroupReportCustom
                {
                    Id = r.Id,
                    Date = r.Date,
                    Item = r.Item,
                    Dimension = r.Dimension,
                    FullRef = r.FullRef,
                    Quantity = r.Quantity,
                    Unit = r.Unit,
                    Total = r.Total,
                    TotalAmount = totalAmount.ToString("N2")
                });

                #region Paging

                var list = reports.Skip(requestModel.Start).Take(requestModel.Length).ToList();


                #endregion

                return new DataTablesResponse(requestModel.Draw, list, totalReport, totalReport);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, timeZone, dateTimeFormat, budgetScenarioId, budgetGroupId, dimensions, wgId, dateRange);
                return null;
            }
        }


        public List<TraderBudgetGroupReportCustom> FilteredBudgetGroupReport(int domainId, int locationId, string timeZone, string dateTimeFormat,
            int budgetScenarioId, int budgetGroupId, string dateRange, string dimensions, int workGroupId)
        {
            var reports = new List<TraderBudgetGroupReportCustom>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, locationId, timeZone,
                        dateTimeFormat, budgetScenarioId, budgetGroupId, dateRange, dimensions, workGroupId);
                //Budget group
                var budgetGroup = dbContext.BudgetGroups.Find(budgetGroupId);
                if (budgetGroup == null) return new List<TraderBudgetGroupReportCustom>();

                var scenario = budgetGroup.BudgetScenarios.FirstOrDefault(e => e.Id == budgetScenarioId);

                switch (budgetGroup.Type)
                {
                    case BudgetGroupType.Revenue:

                        var transactionSaleItems = from sig in dbContext.BudgetScenarioItems
                                                   join item in dbContext.TraderItems on sig.Item.Id equals item.Id
                                                   join tr in dbContext.TraderSaleItems on item.Id equals tr.TraderItem.Id
                                                   where sig.BudgetScenarioItemGroup.BudgetScenario.Id == scenario.Id
                                                         && sig.Item.Group.RevenueBudgetGroup.Id == budgetGroup.Id
                                                   select tr.Id;

                        //var abc = transactionSaleItems.ToList();

                        var saleLogs = (from ts in dbContext.TraderSaleLogs
                                        where ts.Location.Domain.Id == domainId && ts.Location.Id == locationId
                                                                                && ts.CreatedDate < scenario.FiscalEndPeriod &&
                                                                                ts.CreatedDate > scenario.FiscalStartPeriod
                                                                                && (ts.Status ==
                                                                                    TraderSaleStatusEnum.SaleApproved ||
                                                                                    ts.Status == TraderSaleStatusEnum
                                                                                        .SalesOrderedIssued)
                                                                                && ts.SaleItems.Any(s =>
                                                                                    transactionSaleItems.Contains(s.Id))
                                        select ts).ToList();


                        saleLogs.ForEach(p =>
                        {
                            if (workGroupId > 0)
                                if (p.Workgroup.Id != workGroupId)
                                    return;


                            if (!string.IsNullOrEmpty(dateRange))
                            {
                                if (!dateRange.Contains('-'))
                                {
                                    dateRange += "-";
                                }

                                var startDate = DateTime.Parse(dateRange.Split('-')[0].Trim())
                                    .ConvertTimeToUtc(timeZone);
                                var endDate = DateTime.Parse(dateRange.Split('-')[1].Trim()).ConvertTimeToUtc(timeZone);
                                startDate = startDate.AddTicks(1);
                                endDate = endDate.AddDays(1).AddTicks(-1);
                                if (p.CreatedDate < startDate || p.CreatedDate > endDate)
                                    return;
                            }


                            p.SaleItems.ForEach(item =>
                            {
                                if (!string.IsNullOrEmpty(dimensions))
                                {
                                    var dimensionIds = dimensions.Split(',').ToArray();
                                    if (!item.Dimensions.TrueForAll(i => dimensionIds.Contains(i.Id.ToString())))
                                        return;
                                }

                                reports.Add(new TraderBudgetGroupReportCustom
                                {
                                    Id = item.Id,
                                    Date = p.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                                    Item = item.TraderItem.Name,
                                    Dimension = string.Join(", ", item.Dimensions.Select(n => n.Name)),
                                    FullRef = p.AssociatedSale.Reference.FullRef,
                                    Quantity = item.Quantity.ToString("N2"),
                                    Unit = item.Unit.Name,
                                    Total = item.Price.ToString("N2"),
                                    TotalValue = item.Price
                                });
                            });
                        });


                        break;
                    case BudgetGroupType.Expenditure:

                        var transactionPurchaseItems = from sig in dbContext.BudgetScenarioItems
                                                       join item in dbContext.TraderItems on sig.Item.Id equals item.Id
                                                       join tr in dbContext.TraderSaleItems on item.Id equals tr.TraderItem.Id
                                                       where sig.BudgetScenarioItemGroup.BudgetScenario.Id == scenario.Id
                                                             && sig.Item.Group.ExpenditureBudgetGroup.Id == budgetGroup.Id
                                                       select tr.Id;

                        var purchaseLogs = (from ts in dbContext.TraderPurchaseLogs
                                            where ts.Location.Domain.Id == domainId && ts.Location.Id == locationId
                                                                                    && ts.CreatedDate < scenario.FiscalEndPeriod &&
                                                                                    ts.CreatedDate > scenario.FiscalStartPeriod
                                                                                    && (ts.Status == TraderPurchaseStatusEnum
                                                                                            .PurchaseApproved ||
                                                                                        ts.Status == TraderPurchaseStatusEnum
                                                                                            .PurchaseOrderIssued)
                                                                                    && ts.PurchaseItems.Any(s =>
                                                                                        transactionPurchaseItems.Contains(s.Id))
                                            select ts).ToList();



                        purchaseLogs.ForEach(p =>
                        {
                            if (workGroupId > 0)
                                if (p.Workgroup.Id != workGroupId)
                                    return;


                            if (!string.IsNullOrEmpty(dateRange))
                            {
                                if (!dateRange.Contains('-'))
                                {
                                    dateRange += "-";
                                }

                                var startDate = DateTime.Parse(dateRange.Split('-')[0].Trim())
                                    .ConvertTimeToUtc(timeZone);
                                var endDate = DateTime.Parse(dateRange.Split('-')[1].Trim()).ConvertTimeToUtc(timeZone);
                                startDate = startDate.AddTicks(1);
                                endDate = endDate.AddDays(1).AddTicks(-1);
                                if (p.CreatedDate < startDate || p.CreatedDate > endDate)
                                    return;
                            }


                            p.PurchaseItems.ForEach(item =>
                            {
                                if (!string.IsNullOrEmpty(dimensions))
                                {
                                    var dimensionIds = dimensions.Split(',').ToArray();
                                    if (!item.Dimensions.TrueForAll(i => dimensionIds.Contains(i.Id.ToString())))
                                        return;
                                }

                                reports.Add(new TraderBudgetGroupReportCustom
                                {
                                    Id = item.Id,
                                    Date = p.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                                    Item = item.TraderItem.Name,
                                    Dimension = string.Join(", ", item.Dimensions.Select(n => n.Name)),
                                    FullRef = p.AssociatedPurchase.Reference.FullRef,
                                    Quantity = item.Quantity.ToString("N2"),
                                    Unit = item.Unit?.Name ?? "",
                                    Total = item.Cost.ToString("N2"),
                                    TotalValue = item.Cost
                                });
                            });
                        });
                        break;





                }

                return reports;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, locationId, timeZone, dateTimeFormat, budgetScenarioId, budgetGroupId, dateRange, dimensions, workGroupId);
                return reports;
            }
        }
    }
}

