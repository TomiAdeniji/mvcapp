using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Bookkeeping.CoANode;

namespace Qbicles.BusinessRules
{
    public class JournalEntryTemplateRules
    {
        ApplicationDbContext dbContext;

        public JournalEntryTemplateRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        private bool CheckExistName(JournalEntryTemplate template)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Journal Entry Template Check Exist Name", null, null, template);


                var templates = dbContext.JournalEntryTemplates.Where(q => q.Name == template.Name && q.Domain.Id == template.Domain.Id);
                if (template.Id > 0 && templates.Any())
                    templates = templates.Where(q => q.Id != template.Id);
                if (templates.Any()) return true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, template);
            }
            return false;

        }
        public IQueryable<JournalEntryTemplate> GetByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "JournalEntryTemplate GetByDomainId", null, null, domainId);

                var templates = dbContext.JournalEntryTemplates.Where(q => q.Domain != null && q.Domain.Id == domainId);
                return templates;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }

        }
        public JournalEntryTemplate GetById(int templateId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "JournalEntryTemplate GetById", null, null, templateId);


                return dbContext.JournalEntryTemplates.Find(templateId);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, templateId);
                return null;
            }
        }
        public JournalEntryTemplateItem GetTemplateItemById(int templateId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "JournalEntryTemplateItem GetTemplateItemById", null, null, templateId);


                var template = dbContext.JournalEntryTemplates.Find(templateId);

                return new JournalEntryTemplateItem() { Id = template.Id, Name = template.Name, Description = template.Description };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, templateId);

            }

            return null;
        }
        public ReturnJsonModel UpdateJournalEntryTemplate(JournalEntryTemplate journalEntryTemplate, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateJournalEntry Template", null, null, journalEntryTemplate);



                if (journalEntryTemplate != null && !string.IsNullOrEmpty(journalEntryTemplate.Name))
                {
                    if (CheckExistName(journalEntryTemplate))
                    {
                        refModel.actionVal = 3;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_632", journalEntryTemplate.Name);
                        refModel.msgId = journalEntryTemplate.Id.ToString();
                        refModel.msgName = journalEntryTemplate.Name;
                        refModel.result = true;
                        return refModel;
                    }

                    journalEntryTemplate.Domain = domain;

                    if (journalEntryTemplate.Id > 0)
                    {

                        var jeTemplateUpdate = dbContext.JournalEntryTemplates.Find(journalEntryTemplate.Id);
                        jeTemplateUpdate.Name = journalEntryTemplate.Name;
                        jeTemplateUpdate.Description = journalEntryTemplate.Description;
                        if (dbContext.Entry(jeTemplateUpdate).State == EntityState.Detached)
                            dbContext.JournalEntryTemplates.Attach(jeTemplateUpdate);
                        dbContext.Entry(jeTemplateUpdate).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        refModel.actionVal = 2;
                        refModel.msgId = jeTemplateUpdate.Id.ToString();
                        refModel.msgName = jeTemplateUpdate.Name;
                    }
                    else
                    {
                        dbContext.JournalEntryTemplates.Add(journalEntryTemplate);
                        dbContext.Entry(journalEntryTemplate).State = EntityState.Added;
                        dbContext.SaveChanges();
                        refModel.actionVal = 1;
                        //append to select group
                        refModel.msgId = journalEntryTemplate.Id.ToString();
                        refModel.msgName = journalEntryTemplate.Name;
                    }
                    refModel.result = true;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalEntryTemplate);
                refModel.msg = new ErrorMessageModel("ERROR_MSG_5", null).ToJson();
            }

            return refModel;
        }
        public bool DeleteJournalEntryTemplate(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete JournalEntry Template", null, null, id);

                var template = dbContext.JournalEntryTemplates.Find(id);
                var tempRow = template.TemplateRows;//lazy load
                dbContext.JournalEntryTemplates.Remove(template);
                dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }

        }
        /// <summary>
        /// Events that create the IncomeStatementReportEntry
        /// </summary>
        /// <param name="domainId">Domain Id</param>
        /// <returns>List<IncomeStatementReportEntry></returns>
        public List<IncomeStatementReportEntry> GetManageTemplate(int domainId, out int revId, out int expId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "List<IncomeStatementReportEntry> GetManageTemplate", null, null, domainId);


                var reportTemplate = dbContext.BKIncomeReportTemplates.FirstOrDefault(s => s.Domain.Id == domainId);
                if (reportTemplate == null)
                {
                    reportTemplate = new IncomeStatementReportTemplate
                    {
                        Domain = dbContext.Domains.Find(domainId)
                    };
                    dbContext.Entry(reportTemplate).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                var revenueId = dbContext.BKGroups.FirstOrDefault(s => s.Domain.Id == domainId && s.AccountType == BKAccountTypeEnum.Revenue)?.Id ?? 0;
                var expensesId = dbContext.BKGroups.FirstOrDefault(s => s.Domain.Id == domainId && s.AccountType == BKAccountTypeEnum.Expenses)?.Id ?? 0;
                var bkGroups = dbContext.BKCoANodes.Where(s => s.Domain.Id == domainId
                && (s.AccountType == BKAccountTypeEnum.Revenue || s.AccountType == BKAccountTypeEnum.Expenses || s.AccountType == 0)
                && (s.Parent.Id == revenueId || s.Parent.Id == expensesId)
                ).ToList();
                List<CoANode> lst_BKSubGroups = new List<CoANode>();
                //Add top-level BKSubGroups to List
                foreach (var item in bkGroups)
                {
                    lst_BKSubGroups.Add(item);
                }
                //End
                var reportEntires = reportTemplate.ReportEntries;
                //If any are missing, then a new IncomeStatementReportEntry is created for each missing BKSubGroup
                foreach (var item in lst_BKSubGroups)
                {
                    if (!reportEntires.Any(s => s.CoANode.Id == item.Id || s?.InlineReportEntry?.ExpenseReportEntry?.CoANode.Id == item.Id))
                    {
                        IncomeStatementReportEntry reportEntry = new IncomeStatementReportEntry
                        {
                            CoANode = item
                        };
                        reportTemplate.ReportEntries.Add(reportEntry);
                        dbContext.Entry(reportEntry).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }
                }
                //end
                //If there are any IncomeStatementReportEntry that do not have a valid BKSubGroup (The BkSubGroup might no longer exist)
                //, the IncomeStatementReportEntry is deleted and InlineReportEntry associated with the IncomeStatementReportEntry is also deleted.
                List<IncomeStatementReportEntry> lstIncomeRemove = new List<IncomeStatementReportEntry>();
                List<InlineReportEntry> lstInlineRemove = new List<InlineReportEntry>();
                foreach (var item in reportEntires)
                {
                    if (!lst_BKSubGroups.Any(s => s.Id == item.CoANode.Id || s.Id == item?.InlineReportEntry?.ExpenseReportEntry?.CoANode.Id))
                    {
                        if (item.InlineReportEntry != null)
                            lstInlineRemove.Add(item.InlineReportEntry);
                        lstIncomeRemove.Add(item);
                    }
                }
                if (lstInlineRemove.Any())
                    dbContext.BKInlineReportEntries.RemoveRange(lstInlineRemove);
                if (lstIncomeRemove.Any())
                    dbContext.BKIncomeReportEntries.RemoveRange(lstIncomeRemove);
                //End
                dbContext.SaveChanges();
                revId = revenueId;
                expId = expensesId;
                return reportEntires;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                throw ex;
            }
        }
        public ReturnJsonModel SaveConfigReportIncome(int domainId, List<ItemManagerTemplate> items)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save ConfigReport Income", null, null, domainId, items);



                var reportTemplate = dbContext.BKIncomeReportTemplates.FirstOrDefault(s => s.Domain.Id == domainId);
                if (reportTemplate != null)
                {
                    foreach (var item in items)
                    {
                        if (item.action == "Add")
                        {
                            #region Add an inline subtotal 
                            var expense = dbContext.BKIncomeReportEntries.Find(item.expId);
                            //move expense to revenue
                            var revenue = dbContext.BKIncomeReportEntries.Find(item.revId);
                            if (revenue.InlineReportEntry != null)
                            {
                                var inline = revenue.InlineReportEntry;
                                inline.ExpenseReportEntry = expense;
                                inline.SubTotalTitle = item.title;
                                if (dbContext.Entry(inline).State == EntityState.Detached)
                                    dbContext.BKInlineReportEntries.Attach(inline);
                                dbContext.Entry(inline).State = EntityState.Modified;
                            }
                            else
                            {
                                InlineReportEntry inlineReport = new InlineReportEntry();
                                inlineReport.RevenueReportEntry = revenue;
                                inlineReport.ExpenseReportEntry = expense;
                                inlineReport.SubTotalTitle = item.title;
                                dbContext.Entry(inlineReport).State = EntityState.Added;
                                revenue.InlineReportEntry = inlineReport;
                                //remove expense ReportEntries
                                reportTemplate.ReportEntries.Remove(expense);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Reset inline subtotal
                            var expense = dbContext.BKIncomeReportEntries.Find(item.expId);
                            //move expense to revenue
                            var revenue = dbContext.BKIncomeReportEntries.Find(item.revId);
                            if (revenue.InlineReportEntry != null)
                                dbContext.BKInlineReportEntries.Remove(revenue.InlineReportEntry);
                            revenue.InlineReportEntry = null;
                            if (dbContext.Entry(revenue).State == EntityState.Detached)
                                dbContext.BKIncomeReportEntries.Attach(revenue);
                            dbContext.Entry(revenue).State = EntityState.Modified;
                            //remove expense ReportEntries
                            reportTemplate.ReportEntries.Add(expense);
                            #endregion
                        }
                    }

                    refModel.result = dbContext.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, items);
                refModel.result = false;
                throw ex;
            }
            return refModel;
        }
    }
}
