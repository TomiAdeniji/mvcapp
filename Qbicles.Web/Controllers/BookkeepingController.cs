using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using Qbicles.Web.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static Qbicles.Models.ApprovalReq;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class BookkeepingController : BaseController
    {
        // GET: Bookkeeping 

        public ActionResult BKApps()
        {
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");

            ClearAllCurrentActivities();
            ViewBag.UserRoleRights = userRoleRights;

            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), CurrentUser().Id, "");

            ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.JournalEntry ||
                e.Name == BookkeepingProcessName.ViewJournalEntries));
            ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
            ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));


            ViewBag.rightCreateJournalEntry = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.JournalEntry));
            if (!ViewBag.rightShowAccountTab)
                if (ViewBag.rightShowJournalTab)
                    return RedirectToAction("JournalEntries", "Bookkeeping");
                else if (userRoleRights.Any(r => r == RightPermissions.BKManageAppSettings))
                    return RedirectToAction("BKConfiguration", "Bookkeeping");

            ViewBag.CloseBook = new BKCoANodesRule(dbContext).CloseBook(CurrentDomainId())?.ClosureDate.ToString(CurrentUser().DateTimeFormat) ?? "N/A";

            return !ViewBag.rightShowAccountTab ? View("Error") : View();
        }
        public ActionResult BKReports()
        {
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");

            ClearAllCurrentActivities();
            ViewBag.UserRoleRights = userRoleRights;
            var domainId = CurrentDomainId();
            ViewBag.Expenses = new BookkeepingRules(dbContext).GetBKGroup(domainId, CoANode.BKAccountTypeEnum.Expenses);
            ViewBag.Revenue = new BookkeepingRules(dbContext).GetBKGroup(domainId, CoANode.BKAccountTypeEnum.Revenue);
            ViewBag.Assets = new BookkeepingRules(dbContext).GetBKGroup(domainId, CoANode.BKAccountTypeEnum.Assets);
            ViewBag.Liabilities = new BookkeepingRules(dbContext).GetBKGroup(domainId, CoANode.BKAccountTypeEnum.Liabilities);
            ViewBag.Equity = new BookkeepingRules(dbContext).GetBKGroup(domainId, CoANode.BKAccountTypeEnum.Equity);
            ViewBag.dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domainId);
            ViewBag.UserRoleRights = userRoleRights;

            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(domainId, CurrentUser().Id, "");

            ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.JournalEntry ||
                e.Name == BookkeepingProcessName.ViewJournalEntries));
            ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
            ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));

            if (!ViewBag.rightShowAccountTab)
                if (ViewBag.rightShowJournalTab)
                    return RedirectToAction("JournalEntries", "Bookkeeping");
                else if (userRoleRights.Any(r => r == RightPermissions.BKManageAppSettings))
                    return RedirectToAction("BKConfiguration", "Bookkeeping");
            return View();
        }
        public ActionResult BKReportsIncome(ReportIncomeConfig reportIncome, string incomeReportEntry)
        {
            if (string.IsNullOrEmpty(incomeReportEntry))
                return RedirectToAction("BKReports");
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");

            ClearAllCurrentActivities();
            ViewBag.UserRoleRights = userRoleRights;

            ViewBag.dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domainId);
            reportIncome.timezone = CurrentUser().Timezone;
            reportIncome.date_format = CurrentUser().DateFormat;
            reportIncome.incomeReportEntry = new JavaScriptSerializer().Deserialize<List<TreeReportEntry>>(incomeReportEntry);

            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(domainId, CurrentUser().Id, "");

            ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.JournalEntry ||
                e.Name == BookkeepingProcessName.ViewJournalEntries));
            ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
            ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));

            if (!ViewBag.rightShowAccountTab)
                if (ViewBag.rightShowJournalTab)
                    return RedirectToAction("JournalEntries", "Bookkeeping");
                else if (userRoleRights.Any(r => r == RightPermissions.BKManageAppSettings))
                    return RedirectToAction("BKConfiguration", "Bookkeeping");
            string start_date = "";
            string end_date = "";
            var table = new BookkeepingRules(dbContext).ReportIncome(domainId, reportIncome, out start_date, out end_date);
            reportIncome.start_date = start_date;
            reportIncome.end_date = end_date;
            ViewBag.reportConfig = reportIncome;
            ViewBag.DomainLogoBase64 = GetDocumentBase64(CurrentDomain().LogoUri);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ViewBag.treeConfig = serializer.Serialize(reportIncome.incomeReportEntry);
            return View(table);
        }
        public ActionResult BKReportsBalance(ReportBalanceConfig reportBalance, string balanceReportEntry, string allNodeIds)
        {
            if (string.IsNullOrEmpty(balanceReportEntry))
                return RedirectToAction("BKReports");
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");

            ClearAllCurrentActivities();
            ViewBag.UserRoleRights = userRoleRights;

            reportBalance.timezone = CurrentUser().Timezone;
            reportBalance.date_format = CurrentUser().DateFormat;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            reportBalance.incomeReportEntry = serializer.Deserialize<List<TreeBalanceReportEntry>>(balanceReportEntry);
            reportBalance.allNodeIds = serializer.Deserialize<List<int>>(allNodeIds);

            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(domainId, CurrentUser().Id, "");

            ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.JournalEntry ||
                e.Name == BookkeepingProcessName.ViewJournalEntries));
            ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
            ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));

            if (!ViewBag.rightShowAccountTab)
                if (ViewBag.rightShowJournalTab)
                    return RedirectToAction("JournalEntries", "Bookkeeping");
                else if (userRoleRights.Any(r => r == RightPermissions.BKManageAppSettings))
                    return RedirectToAction("BKConfiguration", "Bookkeeping");
            var reportData = new BookkeepingRules(dbContext).ReportBalance(domainId, reportBalance);
            ViewBag.reportConfig = reportBalance;
            ViewBag.DomainLogoBase64 = GetDocumentBase64(CurrentDomain().LogoUri);

            ViewBag.treeConfig = serializer.Serialize(reportBalance.incomeReportEntry);
            return View(reportData);
        }
        public ActionResult FilterReportBalance(ReportBalanceConfig reportBalance)
        {
            reportBalance.timezone = CurrentUser().Timezone;
            reportBalance.date_format = CurrentUser().DateFormat;
            var reportData = new BookkeepingRules(dbContext).ReportBalance(CurrentDomainId(), reportBalance);
            return PartialView("_BalanceReport", reportData);
        }
        public ActionResult FilterReport(FilterIncomeReport filterIncome)
        {
            int domainId = CurrentDomainId();
            var date = filterIncome.date.Split('-');
            var treeconfig = new JavaScriptSerializer().Deserialize<List<TreeReportEntry>>(filterIncome.treeConfig);
            ReportIncomeConfig incomeConfig = new ReportIncomeConfig();
            incomeConfig.timezone = CurrentUser().Timezone;
            incomeConfig.date_format = CurrentUser().DateFormat;
            incomeConfig.period = "Custom date range";
            incomeConfig.start_date = date[0].Trim();
            incomeConfig.end_date = date[1].Trim();
            incomeConfig.view = filterIncome.view;
            incomeConfig.incomeReportEntry = treeconfig;
            incomeConfig.dimensions = filterIncome.dimensions;
            string startDate = "";
            string endDate = "";
            var table = new BookkeepingRules(dbContext).ReportIncome(domainId, incomeConfig, out startDate, out endDate);
            ViewBag.start_date = startDate;
            ViewBag.end_date = endDate;
            return PartialView("_IncomeReport", table);
        }
        public ActionResult DownloadReport(string data)
        {
            try
            {
                var sbyteCapture = data.Replace("data:image/png;base64,", "").Replace("data:image/octet-stream;base64,", "");
                var filePath = Server.MapPath($"~/App_Data/income-report");
                var pdfPath = filePath + Guid.NewGuid();

                var imageBytes = Convert.FromBase64String(sbyteCapture);
                var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                ms.Write(imageBytes, 0, imageBytes.Length);
                var image = System.Drawing.Image.FromStream(ms, true);
                image.Save(pdfPath + ".png", System.Drawing.Imaging.ImageFormat.Png);
                PdfHelper.Instance.SaveImageAsPdf(pdfPath + ".png", pdfPath + ".pdf", 1000, true);
                var fullFileName = Path.Combine(pdfPath + ".pdf");

                var fileDownload = HelperClass.GetBase64StringFromLocalImage(fullFileName);
                System.IO.File.Delete(pdfPath + ".pdf");
                return Json(fileDownload, JsonRequestBehavior.AllowGet); ;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return new EmptyResult();
            }
        }
        public ActionResult ListTransactions(int templateId)
        {
            var template = new JournalEntryTemplateRules(dbContext).GetById(templateId);

            ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            return PartialView("_ListTransactionsPartial", template);
        }
        public ActionResult TreeViewGroupChartPartial(bool callback = false)
        {
            var domainId = CurrentDomainId();
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            var nodeRule = new BKCoANodesRule(dbContext);
            var bkGroup = nodeRule.GetBKGroupByDomain(domainId);
            if (bkGroup == null || !bkGroup.Any())
            {
                bkGroup = nodeRule.InitBKGroups(CurrentDomain(), CurrentUser().Id);
            }
            ViewBag.GroupAccountTree = new BookkeepingRules(dbContext).RenderGroupAccountTree(bkGroup, currencySettings);
            ViewBag.CallBack = callback;
            return PartialView("TreeViewGroupChartPartial");
        }

        public ActionResult GetNumber(int id)
        {
            var refModel = new ReturnJsonModel();
            refModel.actionVal = 1;
            try
            {
                refModel.result = true;
                var gr = new BKCoANodesRule(dbContext).GetBKCoANodeById(id);
                if (gr.Children.Any())
                {
                    var lstNumber = gr.Children.Select(q => (q.Number ?? "").Replace(gr.Number + ".", "")).ToList();
                    while (lstNumber.Contains(refModel.actionVal.ToString()))
                    {
                        refModel.actionVal++;
                    }
                    refModel.Object = lstNumber;
                }
                else
                {
                    refModel.Object = new List<string>();
                }
                refModel.msgId = gr.Number;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, id);
                refModel.result = false;
                refModel.actionVal = 2;
                refModel.msgId = "";
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGroupById(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = true;
                var gr = new BKCoANodesRule(dbContext).GetBKCoANodeById(id);
                var lstChildNodes = new BKCoANodesRule(dbContext).GetChildBKCoANodeByParentId(gr.Parent.Id);
                if (lstChildNodes != null && lstChildNodes.Any())
                {
                    var lstNumber = lstChildNodes.Select(q => (q.Number ?? "")).ToList();
                    lstNumber.Remove(gr.Number);
                    lstNumber = lstNumber.Select(q => q.Replace(gr.Parent.Number + ".", "")).ToList();
                    refModel.Object2 = lstNumber;
                    if (string.IsNullOrEmpty(gr.Number) || string.IsNullOrEmpty(gr.Number.Substring(0, gr.Number.LastIndexOf('.'))))
                    {
                        var sugest = 1;
                        while (lstNumber.Contains(sugest.ToString()))
                        {
                            sugest++;
                        }
                        gr.Number = gr.Parent.Number + "." + sugest.ToString();
                    }
                }
                refModel.Object = new { gr.Name, gr.Number };
                refModel.actionVal = gr.Children.Any() ? 1 : 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, id);
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TreeViewAccountPartial(bool callback = false, int linkedId = 0)
        {
            var bkgroupRules = new BKCoANodesRule(dbContext);
            var bkGroup = bkgroupRules.GetBKGroupByDomain(CurrentDomainId());
            if (bkGroup.Any()) ViewBag.AccountTreeView = BKConfigurationRules.GenTreeView(bkGroup, linkedId);
            else
                ViewBag.AccountTreeView = "";
            ViewBag.CallBack = callback;
            return PartialView("TreeViewAccountPartial");
        }

        public ActionResult TreeViewAccountByNodeIdPartial(int id, int number, int linkedId = 0)
        {
            var bkgroupRules = new BKCoANodesRule(dbContext);
            if (id > 0)
            {
                var bkGroup = bkgroupRules.GetBkGroupById(id);
                if (bkGroup.Any()) ViewBag.AccountTreeView = BKConfigurationRules.GenTreeView(bkGroup, linkedId);
                else
                    ViewBag.AccountTreeView = "";
            }
            else
            {
                var bkGroup = bkgroupRules.GetBKGroupByDomain(CurrentDomainId());
                if (bkGroup.Any()) ViewBag.AccountTreeView = BKConfigurationRules.GenTreeView(bkGroup, linkedId);
                else
                    ViewBag.AccountTreeView = "";
            }
            ViewBag.CallBack = false;
            return PartialView("TreeViewAccountPartial");
        }
        public ActionResult BKChartOfAccountContent(int value)
        {
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                return View("ErrorAccessPage");
            //ViewBag.UserRoleRights = userRoleRights;

            ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), CurrentUser().Id, "");

            ViewBag.rightManagerAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Accounts));
            ViewBag.rightAccount = new BKCoANodesRule(dbContext).AlReadyAccount(value);


            var bkRules = new BookkeepingRules(dbContext);
            //var bkgroupRules = new BKCoANodesRule(dbContext);
            var node = new BKCoANodesRule(dbContext).GetBKCoANodeById(value);


            ViewBag.BreadcrumbName = $"{node.Number}-{node.Name}";
            ViewBag.AccountAmount = bkRules.CalculateAccountAmount(node);

            if (node is BKGroup)
            {
                //var theGroup = (BKGroup)node;
                // No Parent only children

                ViewBag.Breadcrumb = "Chart of Account";
                ViewBag.NodeAccounts = bkRules.NodeAccounts(node);
                ViewBag.ModelNode = node;

                return PartialView("_BKGroupChart");
            }

            if (node is BKSubGroup)
            {
                //var theSubGroup = (BKSubGroup)node;
                // Parent and children
                ViewBag.Breadcrumb = bkRules.BreadcrumbReverse(node);
                ViewBag.NodeAccounts = bkRules.NodeAccounts(node);
                ViewBag.ModelNode = node;

                return PartialView("_BKGroupChart");
            }

            if (node is BKAccount)
            {
                var theAccount = (BKAccount)node;
                // Parent only, the 'children' of an Accout are the account's transactions

                ViewBag.Breadcrumb = bkRules.BreadcrumbReverse(node);
                //ViewBag.CurrentUser = new UserRules(dbContext).GetUserOnly(CurrentUser().Id);
                ViewBag.AccountAmount = bkRules.CalculateAccountAmount(node);
                ViewBag.AccountNode = theAccount;
                var refModel = new BKCoANodesRule(dbContext).GetAccountAssociatedDetail(theAccount.Id, CurrentDomainId());
                var accountRef = (AccountAssociated)refModel.Object;
                var rightDelete = !(theAccount.JournalEntries.Any() ||
                                     theAccount.Transactions.Any() ||
                                     accountRef.CashAccount.Any() ||
                                     accountRef.ContactAccount.Any() ||
                                     accountRef.InventoryAccount.Any() ||
                                     accountRef.PurchaseAccounts.Any() ||
                                     accountRef.SaleAccounts.Any() ||
                                     accountRef.TaxAccounts.Any()
                                     );

                ViewBag.RightDelete = rightDelete;

                return PartialView("_BKAccountChart");
            }
            return PartialView("_BKGroupChart");

        }
        public ActionResult BKAccountAddPartial(int id = 0)
        {
            ViewBag.Account = null;
            if (id > 0)
            {
                ViewBag.Account = new BKCoANodesRule(dbContext).GetAccountById(id);
            }
            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), CurrentUser().Id, "");
            ViewBag.BkWorkGroups = bkWorkGroups.Where(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.Accounts ||
                e.Name == BookkeepingProcessName.ViewChartOfAccounts)).ToList();


            return PartialView("_BKAccountAddPartial");
        }
        public ActionResult SaveSubGroup(BKSubGroup node)
        {
            node.Domain = CurrentDomain();
            var refModel = new BKCoANodesRule(dbContext).SaveSubGroup(node, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BKAccountEditPartial(int id = 0)
        {
            if (id > 0)
            {
                var acc = new BKCoANodesRule(dbContext).GetAccountById(id);
                var grParent = acc.Parent;
                var lstNumber = grParent.Children.Select(q => (q.Number ?? "")).ToList();
                lstNumber.Remove("");
                if (string.IsNullOrEmpty(acc.Number))
                {
                    acc.Number = string.Empty;
                    var lst = new List<string>();
                    foreach (var item in lstNumber)
                    {
                        if (item.Contains('.'))
                        {
                            lst.Add(item.Split('.')[item.Split('.').Length - 1]);
                        }
                        else lst.Add(item);
                    }
                    lstNumber = lst;
                }
                else
                {
                    lstNumber.Remove(acc.Number);
                    lstNumber = lstNumber.Select(q => q.Replace(grParent.Number + ".", "")).ToList();
                }
                var suggestion = 1;
                if (string.IsNullOrEmpty(acc.Number))
                {
                    while (lstNumber.Contains(suggestion.ToString()))
                    {
                        suggestion++;
                    }
                    ViewBag.Suggestion = suggestion;
                }
                else
                {
                    if (acc.Number.Contains('.')) ViewBag.Suggestion = acc.Number.Split('.')[acc.Number.Split('.').Length - 1];
                    else ViewBag.Suggestion = acc.Number;
                }


                ViewBag.ListNumber = lstNumber;
                ViewBag.Account = acc;
            }
            else ViewBag.Account = null;

            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), CurrentUser().Id, "");
            ViewBag.BkWorkGroups = bkWorkGroups.Where(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.Accounts ||
                e.Name == BookkeepingProcessName.ViewChartOfAccounts)).ToList();


            return PartialView("_BKAccountEditPartial");
        }
        [HttpGet]
        public ActionResult GetAccountById(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                var acc = new BKCoANodesRule(dbContext).GetAccountById(id);
                var account = new BKAccount()
                {
                    Number = acc.Number,
                    Id = acc.Id,
                    Name = acc.Name,
                    Balance = acc.Balance,
                    Credit = acc.Credit,
                    Debit = acc.Debit,
                    Code = acc.Code,
                    AssociatedFiles = acc.AssociatedFiles.Select(q => new QbicleMedia() { Id = q.Id, Name = q.Name, Description = q.Description, FileType = q.FileType }).ToList(),
                    Description = acc.Description
                };
                refModel.result = true;
                refModel.actionVal = 1;
                refModel.Object = account;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        // BKConfiguration
        public ActionResult BKConfiguration()
        {
            try
            {

                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.BKManageAppSettings))
                    return View("ErrorAccessPage");

                ClearAllCurrentActivities();
                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
                var domain = CurrentDomain();

                var bkAppSetting = domain.BKAppSettings.FirstOrDefault();
                ViewBag.BKAppSettings = bkAppSetting;
                ViewBag.QbicelAttachment = domain.Qbicles;
                ViewBag.DefaultAttachmentTopics = new TopicRules(dbContext).GetTopicByQbicle(bkAppSetting?.AttachmentQbicle?.Id ?? 0) ?? new List<Topic>();
                ViewBag.Currency = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);

                var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(domain.Id, CurrentUser().Id, "");

                ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.JournalEntry || e.Name == BookkeepingProcessName.ViewJournalEntries));
                ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
                ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));


                ViewBag.rightCreateJournalEntry = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.JournalEntry));

                ViewBag.UserRoleRights = userRoleRights;

                return userRoleRights.All(r => r != RightPermissions.BKManageAppSettings) ? View("ErrorAccessPage") : View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }

        }
        [HttpGet]
        public ActionResult GetTopicsByQbicleId(int qbicleId)
        {
            var topics = new TopicRules(dbContext).GetTopicOnlyByQbicle(qbicleId);

            return Json(topics, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult DefaultSaveSetting(BKAppSettings bkappsetting)
        {

            bkappsetting.Domain = CurrentDomain();

            var bkappsettingRule = new BKConfigurationRules(dbContext);
            var refModel = bkappsettingRule.SaveDefaultBKAppSetting(bkappsetting);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        // Workgroups
        public ActionResult WorkGroupPartial()
        {
            var domainId = CurrentDomainId();
            ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
            ViewBag.Process = new BKWorkGroupsRules(dbContext).GetKBWorkGroupProcesss();
            ViewBag.DomainRoles = new DomainRolesRules(dbContext).GetDomainRolesDomainId(domainId);

            var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroups(domainId);
            return PartialView("WorkGroupPartial", bkWorkGroups);
        }



        // Dimension
        public ActionResult DimensionPartial()
        {
            var dimensions = new TransactionDimensionRules(dbContext).GetTransactionDimension2TraderReportingFilters(CurrentDomainId());
            return PartialView("DimensionPartial", dimensions);
        }

        [HttpGet]
        public ActionResult GetEditDimensionById(int id)
        {
            var dimension = new TransactionDimensionRules(dbContext).GetOnlyById(id, CurrentUser().DateFormat);
            return Json(dimension, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult SaveDimension(TransactionDimension transactionDimension)
        {

            var refModel = new TransactionDimensionRules(dbContext).SaveDimension(transactionDimension, CurrentUser().Id, CurrentDomain());
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteDimension(int id)
        {
            var transactionDimension = new TransactionDimensionRules(dbContext);
            if (transactionDimension.DeleteDimension(id))
            {
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Fail", JsonRequestBehavior.AllowGet);
            }
        }

        // Journal Group
        public ActionResult JournalGroupPartial()
        {
            ViewBag.JournalGroups = new JournalGroupRules(dbContext).GetByDomainId(CurrentDomainId());
            return PartialView("_JournalGroupPartial");
        }
        [HttpPost]
        public ActionResult SaveJournalGroup(JournalGroup journalGroup)
        {
            var refModel = new JournalGroupRules(dbContext).SaveJournalGroup(journalGroup, CurrentUser().Id, CurrentDomain());

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetEditJournalGroupById(int id)
        {
            var journalGroup = new JournalGroupRules(dbContext).GetJournalGroupOnlyById(id);
            return Json(journalGroup, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteJournalGroup(int id)
        {
            var journalGroup = new JournalGroupRules(dbContext);
            if (journalGroup.DeleteJournalGroup(id))
            {
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Fail", JsonRequestBehavior.AllowGet);
            }
        }


        // Tax Rate 
        public ActionResult TaxRatePartial()
        {
            ViewBag.TaxRates = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId());
            int domainId = CurrentDomainId();
            var bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
            if (bkGroup.Any()) ViewBag.TreeView = BKConfigurationRules.GenTreeView(bkGroup.ToList());
            else
                ViewBag.TreeView = "";
            return PartialView("TaxRatePartial");
        }
        [HttpPost]
        public ActionResult SaveTaxRate(TaxRate taxRate)
        {

            var refModel = new TaxRateRules(dbContext).SaveTaxRate(taxRate, CurrentDomain());

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetEditTaxRateById(int id)
        {
            var taxRate = new TaxRateRules(dbContext).GetItemTaxRateItemById(id);
            return Json(taxRate, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteTaxRate(int id)
        {
            var taxRate = new TaxRateRules(dbContext);
            if (taxRate.DeleteTaxRate(id))
            {
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Fail", JsonRequestBehavior.AllowGet);
            }
        }

        // Template
        public ActionResult ConfigReportIncome()
        {
            var revId = 0;
            var expId = 0;
            var coANodes = new JournalEntryTemplateRules(dbContext).GetManageTemplate(CurrentDomainId(), out revId, out expId);
            ViewBag.revId = revId;
            ViewBag.expId = expId;
            return PartialView("_ConfigReportContent", coANodes);

        }
        public ActionResult SaveConfigReportIncome(List<ItemManagerTemplate> items)
        {
            var refModel = new JournalEntryTemplateRules(dbContext).SaveConfigReportIncome(CurrentDomainId(), items);

            return Json(refModel);
        }
        [HttpPost]
        public ActionResult SaveTemplate(JournalEntryTemplate template)
        {
            var refModel = new JournalEntryTemplateRules(dbContext).UpdateJournalEntryTemplate(template, CurrentDomain());

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetEditTemplateById(int id)
        {
            var template = new JournalEntryTemplateRules(dbContext).GetTemplateItemById(id);
            return Json(template, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteTemplate(int id)
        {
            var template = new JournalEntryTemplateRules(dbContext);
            if (template.DeleteJournalEntryTemplate(id))
            {
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Fail", JsonRequestBehavior.AllowGet);
            }
        }



        // Journal Entries
        public ActionResult JournalEntries()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                    return View("ErrorAccessPage");
                ClearAllCurrentActivities();
                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
                ViewBag.JournalGroup = new JournalGroupRules(dbContext).GetByDomainId(CurrentDomainId()).ToList();
                ViewBag.JournalStatus = Enum.GetValues(typeof(RequestStatusEnum)).Cast<RequestStatusEnum>().ToList();
                ViewBag.BkAccounts = new BKCoANodesRule(dbContext).MapingAccountGroup(CurrentDomainId());

                var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), CurrentUser().Id, "");

                ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                    e.Name == BookkeepingProcessName.JournalEntry ||
                    e.Name == BookkeepingProcessName.ViewJournalEntries));
                ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                    e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
                ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));

                if (!ViewBag.rightShowJournalTab)
                    if (ViewBag.rightShowAccountTab)
                        return RedirectToAction("BKApps", "Bookkeeping");
                    else if (userRoleRights.Any(r => r == RightPermissions.BKManageAppSettings))
                        return RedirectToAction("BKConfiguration", "Bookkeeping");

                ViewBag.rightCreateJournalEntry = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.JournalEntry));
                ViewBag.UserRoleRights = userRoleRights;

                return !ViewBag.rightShowJournalTab ? View("Error") : View();
            }
            catch (Exception ex)
            {

                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult BKJournalEntriesContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            string accounts = "", string status = "", string groups = "", string dates = "")
        {
            var result = new JournalEntryRules(dbContext).JournalEntriesFilter(requestModel, accounts, status, groups, dates, CurrentDomainId(), CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateJournalEntry()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                    return View("ErrorAccessPage");
                ClearAllCurrentActivities();
                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
                var domainId = CurrentDomainId();
                var jEntry = new JournalEntry()
                {
                    CreatedDate = DateTime.UtcNow,
                    PostedDate = DateTime.UtcNow
                };
                ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domainId);
                ViewBag.JournalGroups = new JournalGroupRules(dbContext).GetByDomainId(domainId).ToList();
                ViewBag.BKAppSetting = new BKConfigurationRules(dbContext).GetBKAppSettingByDomain(domainId);
                ViewBag.JournalTemplates = new JournalEntryTemplateRules(dbContext).GetByDomainId(domainId).ToList();

                var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), CurrentUser().Id, "");

                ViewBag.BkWorkGroups = bkWorkGroups.Where(p => p.Processes.Any(e =>
                    e.Name == BookkeepingProcessName.JournalEntry ||
                    e.Name == BookkeepingProcessName.ViewJournalEntries)).ToList();
                ViewBag.CloseBookDate = new BKCoANodesRule(dbContext).CloseBook(CurrentDomainId())?.ClosureDate.ToString("MM/dd/yyyy HH:mm");

                return View(jEntry);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult JournalEntry(int id)
        {
            var currentUserId = CurrentUser().Id;
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                    return View("ErrorAccessPage");

                SetCurrentJournalEntryIdCookies(id);
                
                ViewBag.CurrentPage = "bookkeeping";
                SetCurrentPage("bookkeeping");
                var model = new JournalEntryRules(dbContext).GetById(id);
                if (model == null)
                    model = new JournalEntryRules(dbContext).GetByApprovalId(id);

                ViewBag.rightCreateJournalEntry = (model.WorkGroup?.Members.Any(u => u.Id == currentUserId) ?? false)
                                                  && (bool)model.WorkGroup?.Processes.Any(p => p.Name == BookkeepingProcessName.JournalEntry);

                ViewBag.UserRoleRights = userRoleRights;
                var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), currentUserId, "");
                ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                e.Name == BookkeepingProcessName.JournalEntry ||
                e.Name == BookkeepingProcessName.ViewJournalEntries));
                ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e =>
                    e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
                ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));
                ViewBag.CurrentUserId = currentUserId;
                return View(model);
            }
            catch (Exception ex)
            {

                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, id);
                return View("Error");
            }

        }

        public ActionResult ApprovalBookkeeping()
        {
            try
            {
                var userSetting = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
                if (userRoleRights != null && userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                    return View("ErrorAccessPage");

                var approval = new ApprovalsRules(dbContext).GetApprovalById(CurrentApprovalId()).BusinessMapping(userSetting.Timezone);
                ViewBag.Approval = approval;
                ViewBag.CurrentReviewerAndApprover = new ApprovalAppsRules(dbContext).GetIsReviewerAndApprover(approval.ApprovalRequestDefinition != null ? approval.ApprovalRequestDefinition.Id : 0, userSetting.Id);

                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("Approval");
                ViewBag.JournalEntry = approval.JournalEntries.FirstOrDefault();

                var timeline = new ApprovalsRules(dbContext).GetApprovalStatusTimeline(approval, userSetting.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult EditJournalEntry(int id)
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                    return View("ErrorAccessPage");

                var currentId = CurrentUser().Id;
                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
                SetCurrentJournalEntryIdCookies(id);
                var domainId = CurrentDomainId();
                var jEntry = new JournalEntryRules(dbContext).GetById(id);

                if (jEntry.WorkGroup?.Members.All(e => e.Id != currentId) ?? false)
                {
                    return View("Error");
                }
                if (jEntry.Approval?.RequestStatus == RequestStatusEnum.Approved || jEntry.Approval?.RequestStatus == RequestStatusEnum.Discarded || jEntry.Approval?.RequestStatus == RequestStatusEnum.Denied)
                {
                    return View("Error");
                }

                ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domainId);
                ViewBag.JournalGroups = new JournalGroupRules(dbContext).GetByDomainId(domainId).ToList();
                ViewBag.BKAppSetting = new BKConfigurationRules(dbContext).GetBKAppSettingByDomain(domainId);


                var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), currentId, "");
                ViewBag.BkWorkGroups = bkWorkGroups.Where(p => p.Processes.Any(e =>
                    e.Name == BookkeepingProcessName.JournalEntry ||
                    e.Name == BookkeepingProcessName.ViewJournalEntries)).ToList();

                ViewBag.CloseBookDate = new BKCoANodesRule(dbContext).CloseBook(CurrentDomainId())?.ClosureDate.ToString("MM/dd/yyyy HH:mm");
                //Comment/ Media
                return View(jEntry);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, id);
                return View("Error");
            }
        }

        public ActionResult GetWorkGroup(int id)
        {
            var result = new ReturnJsonModel();
            try
            {
                var wg = new BKWorkGroupsRules(dbContext).GetById(id);
                result.Object =
                    new
                    {
                        Process = string.Join(", ", wg.Processes.Select(e => e.Name)),
                        Qbicle = wg.Qbicle.Name,
                        Members = wg.Members.Count,
                        wg.Id
                    };

                var reviewTable = "<table class='table app_specific valigntop'>";
                reviewTable += "<thead><tr>";
                reviewTable += "<th>Approval process</th><th>Associated Qbicle</th><th>Reviewers</th><th>Approvers</th>";
                reviewTable += "</tr></thead>";
                reviewTable += "<tbody><tr>";
                var processName = "";
                wg.Processes.ForEach(p =>
                {
                    processName += $"<li>{p.Name}</li>";
                });
                reviewTable += $"<td><ul>{processName}</ul></td>";
                reviewTable += $"<td>{wg.Qbicle.Name}</td>";


                var reviewverName = "";
                wg.Reviewers.ForEach(p =>
                {
                    reviewverName += $"<li>{HelperClass.GetFullNameOfUser(p)}</li>";
                });
                reviewTable += $"<td><ul>{reviewverName}</ul></td>";

                var approverName = "";
                wg.Approvers.ForEach(p =>
                {
                    approverName += $"<li>{HelperClass.GetFullNameOfUser(p)}</li>";
                });
                reviewTable += $"<td><ul>{approverName}</ul></td>";



                reviewTable += "</tr></tbody>";
                reviewTable += "</table>";
                reviewTable += "<button onclick='SendJournalEntryToReview()' class='btn btn-success'><i class='fa fa-check'></i> &nbsp; Send to review</button>";
                reviewTable += " <button class='btn btn-danger' data-dismiss='modal'><i class='fa fa-remove'></i> &nbsp; Cancel</button>";
                result.msg = reviewTable;
                result.result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, id);
                result.result = false;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetListMergeAccount(int accountMasterId)
        {
            var accounts = new BKCoANodesRule(dbContext).GetListMergeAccount(CurrentDomainId(), accountMasterId);
            return PartialView("_ListMergeAccountSelect", accounts);
        }

        public ActionResult BKMergeAccount(int accountMergeId, int accountMasterId)
        {
            var refModel = new ReturnJsonModel { result = false };
            var accountMergeDetail = TempData[$"BkAccount-{accountMergeId}"];
            if (accountMergeDetail == null)
            {
                refModel.msg = ResourcesManager._L("ERROR_MSG_453");
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            refModel = new BKCoANodesRule(dbContext).BKMergeAccount(accountMergeId, accountMasterId, accountMergeDetail);

            if (refModel.result)
                TempData.Remove($"BkAccount-{accountMergeId}");

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BKDeleteAccount(int accountId)
        {
            var refModel = new BKCoANodesRule(dbContext).BKDeleteAccount(accountId);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BKDeleteSubGroup(int groupId)
        {
            var refModel = new BKCoANodesRule(dbContext).BKDeleteSubGroup(groupId);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAccountAssociatedDetail(int accountId)
        {
            var refModel = new BKCoANodesRule(dbContext).GetAccountAssociatedDetail(accountId, CurrentDomainId());
            TempData[$"BkAccount-{accountId}"] = refModel.Object;
            TempData.Keep($"BkAccount-{accountId}");
            refModel.Object = null;

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowBkTransactionDetail(int id)
        {
            var bkTransaction = new BKTransactionRules(dbContext).GetById(id);

            return PartialView("_BkTransactionDetail", bkTransaction);
        }


        public ActionResult CloseBook()
        {
            var closeBook = new BKCoANodesRule(dbContext).CloseBook(CurrentDomainId());

            return PartialView("_CloseBook", closeBook);
        }


        public ActionResult CloseBookSave(string closureDate)
        {
            var refModel = new BKCoANodesRule(dbContext).CloseBookSave(CurrentUser().Id, CurrentDomainId(), closureDate, CurrentUser().DateTimeFormat);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VerifyJournalPostedDate(string journalPostedDate, string bkPostedDates)
        {
            var refModel = new BKCoANodesRule(dbContext).VerifyJournalPostedDate(journalPostedDate, bkPostedDates, CurrentUser().DateTimeFormat);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="bkAccount">BK Account data</param>
        /// <param name="bkAccountAssociatedFiles"> Associated files of bk account - to update name</param>
        /// <param name="bkAccountAttachments">New attachments files</param>
        /// <returns></returns>
        public ActionResult SaveBKAccount(BKAccount bkAccount, BKAccount bkAccountAssociatedFiles, List<MediaModel> bkAccountAttachments)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                var currentDomain = CurrentDomain();

                refModel = new BKCoANodesRule(dbContext).SaveBKAccount(bkAccount, currentDomain, CurrentUser().Id);

                var mediaRules = new MediasRules(dbContext);

                if (bkAccountAssociatedFiles?.AssociatedFiles?.Count > 0)
                    mediaRules.UpdateAttachmentsBkAccount(bkAccountAssociatedFiles);

                if (bkAccountAttachments?.Count > 0)
                    mediaRules.SaveNewAttachmentsBkAccount((BKAccount)refModel.Object2, bkAccountAttachments, CurrentUser().Id);
                refModel.Object2 = null;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

    }
}