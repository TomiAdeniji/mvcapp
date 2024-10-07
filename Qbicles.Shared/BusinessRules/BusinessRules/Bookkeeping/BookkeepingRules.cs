using System;
using System.Linq;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Bookkeeping;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Qbicles.Models;
using Qbicles.BusinessRules.Helper;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class BookkeepingRules
    {
        ApplicationDbContext dbContext;

        public BookkeepingRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        T UnProxy<T>(ApplicationDbContext context, T proxyObject) where T : class
        {
            var proxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.ProxyCreationEnabled = false;
                T poco = context.Entry(proxyObject).CurrentValues.ToObject() as T;
                return poco;
            }
            finally
            {
                context.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
            }
        }



        public string RenderGroupAccountTree(object node, CurrencySetting setting)
        {

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Render Group Account Tree", null, null, node, setting);



                // Added By DJN
                // Work out what type of item has been passed to the method
                // We have to do this because there ia a problem (on some machines) where the type of the 
                // node is said to be 'CoANode' instead of 'BKSubGroup'
                //
                CoANode.BKCoANodeTypeEnum nodeType;

                if (node as List<BKGroup> != null)
                    nodeType = CoANode.BKCoANodeTypeEnum.GroupList;
                else
                {
                    var theNode = (CoANode)node;
                    nodeType = theNode.NodeType;
                }

                var strNode = new StringBuilder();
                switch (nodeType)
                {
                    case CoANode.BKCoANodeTypeEnum.GroupList:
                        {
                            var nodes = (List<BKGroup>)node;
                            strNode.Append("<div id='jstree_id' class='jstree'><ul>");
                            foreach (var groupList in nodes)
                            {
                                strNode.Append(RenderGroupAccountTree(groupList, setting));
                            }
                            strNode.Append("</ul></div>");
                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Group:
                        {
                            var nodeGroup = (BKGroup)node;
                            strNode.Append($"<li class='groupaccount_{nodeGroup.Id}' data-name='{nodeGroup.Name}' data-parent='{string.Join(",", new BKCoANodesRule(dbContext).GetListId(nodeGroup, new List<int>()))}' data-node='BKGroup' data-value='{nodeGroup.Id}'" +
                                           " data-jstree = '{\"icon\":\"../Content/DesignStyle/img/tree-group.png\"}','opened':false,'selected':false}'>");

                            strNode.Append($"<h5>{nodeGroup.Name}</h5>");
                            strNode.Append($"<span>{nodeGroup.AccountType}</span>");
                            strNode.Append($"<div class='promote'>{CalculateAccountAmount(nodeGroup).Balance.ToCurrencySymbol(setting)}</div>");
                            if (nodeGroup.Children != null && nodeGroup.Children.Count > 0)
                            {
                                strNode.Append("<ul>");
                                foreach (var childrenGroup in nodeGroup.Children)
                                {
                                    strNode.Append(RenderGroupAccountTree(childrenGroup, setting));
                                }
                                strNode.Append("</ul>");
                            }
                            strNode.Append("</li>");
                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.SubGroup:
                        {
                            var nodeSubGroup = (CoANode)node;
                            strNode.Append($"<li class='groupaccount_{nodeSubGroup.Id}' data-parent='{string.Join(",", new BKCoANodesRule(dbContext).GetListId(nodeSubGroup, new List<int>()))}' data-name='{nodeSubGroup.Name}' data-node='BKSubGroup' data-value='{nodeSubGroup.Id}'" +
                                           " data-jstree = '{\"icon\":\"../Content/DesignStyle/img/tree-group.png\"}','opened':false,'selected':false}'>");
                            strNode.Append($"<h5>{nodeSubGroup.Number} - {nodeSubGroup.Name}</h5>");
                            strNode.Append($"<span>{nodeSubGroup.AccountType}</span>");
                            strNode.Append($"<div class='promote'>{CalculateAccountAmount(nodeSubGroup).Balance.ToCurrencySymbol(setting)}</div>");
                            if (nodeSubGroup.Children != null && nodeSubGroup.Children.Count > 0)
                            {
                                strNode.Append("<ul>");
                                foreach (var childrenSubGroup in nodeSubGroup.Children)
                                {
                                    strNode.Append(RenderGroupAccountTree(childrenSubGroup, setting));
                                }
                                strNode.Append("</ul>");
                            }
                            strNode.Append("</li>");
                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Account:
                        {
                            var nodeAccount = (BKAccount)node;
                            strNode.Append($"<li class='groupaccount_{nodeAccount.Id}' data-parent='{string.Join(",", new BKCoANodesRule(dbContext).GetListId(nodeAccount, new List<int>()))}' data-name='{nodeAccount.Name}' data-node='BKAccount' data-value='{nodeAccount.Id}'" +
                                           " data-jstree = '{\"icon\":\"../Content/DesignStyle/img/tree-bank.png\"}','opened':false,'selected':false}'>");
                            strNode.Append($"<h5>{nodeAccount.Number} - {nodeAccount.Name}</h5>");
                            strNode.Append($"<span>{(nodeAccount.Debit ?? 0).ToDecimalPlace(setting)} -  {(nodeAccount.Credit ?? 0).ToDecimalPlace(setting)}</span>");
                            strNode.Append($"<div class='promote'>{(nodeAccount.Balance ?? 0).ToCurrencySymbol(setting)}</div>");
                            strNode.Append($"</li>");
                            break;
                        }
                }
                return strNode.ToString();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node, setting);
                return "";
            }



        }

        private List<AccountAmount> CalculateGroupAmount(object node)
        {

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Calculate Group Amount", null, null, node);


                CoANode.BKCoANodeTypeEnum nodeType;

                if (node is List<BKGroup>)
                    nodeType = CoANode.BKCoANodeTypeEnum.GroupList;
                else
                {
                    var theNode = (CoANode)node;
                    nodeType = theNode.NodeType;
                }

                var amountNode = new List<AccountAmount>();
                switch (nodeType)
                {
                    case CoANode.BKCoANodeTypeEnum.GroupList:
                        {
                            var nodes = (List<BKGroup>)node;
                            foreach (var n in nodes)
                            {
                                amountNode.AddRange(CalculateGroupAmount(n));
                            }

                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Group:
                        {
                            var nodeGroup = (BKGroup)node;

                            if (nodeGroup.Children != null && nodeGroup.Children.Count > 0)
                            {
                                foreach (var childrenGroup in nodeGroup.Children)
                                {
                                    amountNode.AddRange(CalculateGroupAmount(childrenGroup));
                                }
                            }

                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.SubGroup:
                        {
                            var nodeSubGroup = (CoANode)node;
                            if (nodeSubGroup.Children != null && nodeSubGroup.Children.Count > 0)
                            {
                                foreach (var childrenSubGroup in nodeSubGroup.Children)
                                {
                                    amountNode.AddRange(CalculateGroupAmount(childrenSubGroup));
                                }
                            }

                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Account:
                        {
                            var noteAccount = (BKAccount)node;

                            amountNode.Add(new AccountAmount
                            {
                                Credit = noteAccount.Credit ?? 0,
                                Debit = noteAccount.Debit ?? 0,
                                Balance = noteAccount.Balance ?? 0
                            });
                            break;
                        }
                }
                return amountNode;



            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return new List<AccountAmount>();
            }
        }

        public AccountAmount CalculateAccountAmount(object node)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Calculate Account Amount", null, null, node);

                var amounts = CalculateGroupAmount(node);
                return new AccountAmount()
                {
                    Credit = amounts.Select(e => e.Credit).Sum(),
                    Debit = amounts.Select(e => e.Debit).Sum(),
                    Balance = amounts.Select(e => e.Balance).Sum()
                };



            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return null;
            }

        }

        private string BreadcrumbCharOfAccount(object node)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Bread crumb Char Of Account", null, null, node);


                CoANode.BKCoANodeTypeEnum nodeType;

                if (node is List<BKGroup>)
                    nodeType = CoANode.BKCoANodeTypeEnum.GroupList;
                else
                {
                    var theNode = (CoANode)node;
                    nodeType = theNode.NodeType;
                }

                string strNode = "";

                switch (nodeType)
                {
                    case CoANode.BKCoANodeTypeEnum.GroupList:
                        {
                            var nodes = (List<BKGroup>)node;
                            foreach (var n in nodes)
                            {
                                strNode += BreadcrumbCharOfAccount(n);
                            }

                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Group:
                        {
                            var nodeGroup = (BKGroup)node;

                            if (nodeGroup.Parent != null)
                            {
                                strNode = nodeGroup.Parent.Name + "₩" + strNode;
                                strNode += BreadcrumbCharOfAccount(nodeGroup.Parent);
                            }

                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.SubGroup:
                        {
                            var nodeSubGroup = (CoANode)node;
                            if (nodeSubGroup.Parent != null)
                            {
                                strNode = nodeSubGroup.Parent.Name + "₩" + strNode;
                                strNode += BreadcrumbCharOfAccount(nodeSubGroup.Parent);
                            }

                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Account:
                        {
                            var nodeAccount = (BKAccount)node;
                            if (nodeAccount.Parent != null)
                            {
                                strNode = nodeAccount.Parent.Name + "₩" + strNode;
                                strNode += BreadcrumbCharOfAccount(nodeAccount.Parent);
                            }

                            break;
                        }
                }
                return strNode;



            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return "";
            }
        }

        public string BreadcrumbReverse(object node)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BreadcrumbReverse", null, null, node);



                var breadcrumb = BreadcrumbCharOfAccount(node);
                breadcrumb = breadcrumb.Remove(breadcrumb.Length - 1, 1);
                var text = string.Join("/", breadcrumb.Split('₩').Reverse());
                return text;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return "";
            }
        }

        public List<BKAccount> NodeAccounts(CoANode node)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Node Accounts", null, null, node);


                var nodes = new List<BKAccount>();
                foreach (var children in node.Children)
                {
                    if (children is BKAccount)
                    {
                        nodes.Add((BKAccount)children);
                    }
                }
                return nodes;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return new List<BKAccount>();
            }
        }

        public ReturnJsonModel GetListCoANodeParentId(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetListCoANodeParentId", null, null, id);


                string parents;
                var node = dbContext.BKCoANodes.Find(id);
                if (node?.Parent == null)
                    parents = id.ToString();
                else
                    parents = BreadcrumbParentId(node) + id.ToString();

                refModel.Object = parents.Split(',').ToList();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                refModel.result = false;
            }



            return refModel;
        }

        private string BreadcrumbParentId(object node)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Breadcrumb ParentId", null, null, node);



                CoANode.BKCoANodeTypeEnum nodeType;
                if (node is List<BKGroup>)
                    nodeType = CoANode.BKCoANodeTypeEnum.GroupList;
                else
                {
                    var theNode = (CoANode)node;
                    nodeType = theNode.NodeType;
                }

                string strNode = "";

                switch (nodeType)
                {
                    case CoANode.BKCoANodeTypeEnum.GroupList:
                        {
                            var nodes = (List<BKGroup>)node;
                            foreach (var n in nodes)
                            {
                                strNode += BreadcrumbParentId(n);
                            }
                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Group:
                        {
                            var nodeGroup = (BKGroup)node;
                            if (nodeGroup.Parent != null)
                            {
                                strNode = nodeGroup.Parent.Id + "," + strNode;
                                strNode += BreadcrumbParentId(nodeGroup.Parent);
                            }
                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.SubGroup:
                        {
                            var nodeSubGroup = (CoANode)node;
                            if (nodeSubGroup.Parent != null)
                            {
                                strNode = nodeSubGroup.Parent.Id + "," + strNode;
                                strNode += BreadcrumbParentId(nodeSubGroup.Parent);
                            }
                            break;
                        }

                    case CoANode.BKCoANodeTypeEnum.Account:
                        {
                            var nodeAccount = (BKAccount)node;
                            if (nodeAccount.Parent != null)
                            {
                                strNode = nodeAccount.Parent.Id + "," + strNode;
                                strNode += BreadcrumbParentId(nodeAccount.Parent);
                            }
                            break;
                        }
                }
                return strNode;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return "";
            }
        }

        public DataTable ReportIncome(int domainId, ReportIncomeConfig incomeConfig, out string start_date, out string end_date)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ReportIncome", null, null, domainId, incomeConfig);

                var tz = TimeZoneInfo.FindSystemTimeZoneById(incomeConfig.timezone);
                var startDate = incomeConfig.start_date.ConvertDateFormat(incomeConfig.date_format).ConvertTimeToUtc(tz);
                var endDate = incomeConfig.end_date.ConvertDateFormat(incomeConfig.date_format).ConvertTimeToUtc(tz);
                var lstColumns = new List<DataColumn>();
                //this week
                if (incomeConfig.period == "this-week")
                {
                    startDate = DateTime.UtcNow.StartOfWeek(DayOfWeek.Monday).Date;
                    endDate = DateTime.UtcNow.EndOfWeek(DayOfWeek.Sunday);
                    lstColumns.Add(new DataColumn(startDate.ConvertTimeFromUtc(tz).getIncomeCLName(incomeConfig.view)));
                }
                else if (incomeConfig.period == "this-month")//this Month
                {
                    startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                    endDate = startDate.AddMonths(1);
                    lstColumns.Add(new DataColumn(startDate.ConvertTimeFromUtc(tz).getIncomeCLName(incomeConfig.view)));
                }
                else if (incomeConfig.period == "this-year-to-date")//this year
                {
                    startDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
                    endDate = DateTime.UtcNow;
                    var lstMonths = Enumerable.Range(1, DateTime.UtcNow.ConvertTimeFromUtc(tz).Month).Select(i => new DataColumn(new DateTime(startDate.ConvertTimeFromUtc(tz).Year, i, 1).ToString("MMMM yy")));
                    lstColumns = lstMonths.ToList();
                }
                else
                {
                    if (incomeConfig.view == "quarterly")
                    {
                        int quarterly = startDate.ConvertTimeFromUtc(tz).GetQuarter();
                        //Last year of startDate
                        DateTime firstDay = new DateTime(startDate.ConvertTimeFromUtc(tz).Year, 1, 1);
                        DateTime daylastYear = firstDay.AddYears(1).AddTicks(-1).ConvertTimeToUtc(tz);
                        //End last year
                        if (endDate > daylastYear)
                        {
                            endDate = daylastYear;
                        }
                        //Maximun is 4 Quarterly
                        while (quarterly <= endDate.ConvertTimeFromUtc(tz).GetQuarter())
                        {
                            lstColumns.Add(new DataColumn("Q" + quarterly));
                            quarterly++;
                        }

                    }
                    else if (incomeConfig.view == "yearly")
                    {
                        int yearly = startDate.ConvertTimeFromUtc(tz).Year;
                        if ((endDate.Year - yearly) > 12)
                            endDate = endDate.AddYears(-(endDate.Year - 12));
                        //Maximun is 12 years
                        while (yearly <= endDate.ConvertTimeFromUtc(tz).Year)
                        {
                            lstColumns.Add(new DataColumn(yearly.ToString()));
                            yearly++;
                        }
                    }
                    else
                    {
                        //Last year of startDate
                        DateTime firstDay = new DateTime(startDate.ConvertTimeFromUtc(tz).Year, 1, 1);
                        DateTime daylastYear = firstDay.AddYears(1).AddTicks(-1).ConvertTimeToUtc(tz);
                        //End last year
                        if (endDate > daylastYear)
                        {
                            endDate = daylastYear;
                        }
                        //Maximun is 12 Months
                        var monthstart = startDate.ConvertTimeFromUtc(tz).Month;
                        var monthCount = endDate.ConvertTimeFromUtc(tz).Month - monthstart + 1;
                        var lstMonths = Enumerable.Range(monthstart, monthCount).Select(i => new DataColumn(new DateTime(startDate.ConvertTimeFromUtc(tz).Year, i, 1).ToString("MMMM yy")));
                        lstColumns = lstMonths.ToList();
                    }

                }
                endDate = endDate.AddDays(1);
                #region Query data transactions
                //List CoaNode
                var lstEntry = incomeConfig.incomeReportEntry.Select(s => s.id).ToArray();
                List<BKTransacsionModel> query = null;
                if (incomeConfig.dimensions != null && incomeConfig.dimensions.Any())
                {
                    query = (from trans in dbContext.BKTransactions
                             join ac in dbContext.BKAccounts on trans.Account.Id equals ac.Id
                             where ac.Domain.Id == domainId
                             && trans.Dimensions.Any(s => incomeConfig.dimensions.Any(d => d == s.Id))
                             && trans.PostedDate >= startDate
                             && trans.PostedDate < endDate
                             && trans.JournalEntry.IsApproved
                             && lstEntry.Contains(ac.Id)
                             select new BKTransacsionModel
                             {
                                 NodeId = ac.Id,
                                 Credit = trans.Credit != null ? trans.Credit.Value : 0,
                                 Debit = trans.Debit != null ? trans.Debit.Value : 0,
                                 date = trans.PostedDate
                             }).ToList();
                }
                else
                {
                    query = (from trans in dbContext.BKTransactions
                             join ac in dbContext.BKAccounts on trans.Account.Id equals ac.Id
                             where ac.Domain.Id == domainId
                             && trans.PostedDate >= startDate
                             && trans.PostedDate < endDate
                             && trans.JournalEntry.IsApproved
                             && lstEntry.Contains(ac.Id)
                             select new BKTransacsionModel
                             {
                                 NodeId = ac.Id,
                                 Credit = trans.Credit != null ? trans.Credit.Value : 0,
                                 Debit = trans.Debit != null ? trans.Debit.Value : 0,
                                 date = trans.PostedDate
                             }).ToList();
                }

                var lstTransactions = query.Select(g => new BKTransacsionsCustome
                {
                    NodeId = g.NodeId,
                    Credit = g.Credit,
                    Debit = g.Debit,
                    ColumnName = g.date.getIncomeCLName(incomeConfig.view)
                }).GroupBy(s => new { s.NodeId, s.ColumnName })
                    .Select(g => new BKTransacsionsCustome
                    {
                        ColumnName = g.Key.ColumnName,
                        NodeId = g.Key.NodeId,
                        Credit = g.Sum(x => x.Credit),
                        Debit = g.Sum(x => x.Debit),
                    }).ToList();
                #endregion
                #region Build Datatable by tree Config
                DataTable table = new DataTable();
                //Add Columns
                table.Columns.Add(new DataColumn("NodeName"));
                table.Columns.Add(new DataColumn("NodeId"));
                foreach (var item in lstColumns)
                {
                    table.Columns.Add(item);
                }
                //end
                //Add Rows
                var reportTemplate = dbContext.BKIncomeReportTemplates.FirstOrDefault(s => s.Domain.Id == domainId);
                var revenue = dbContext.BKGroups.FirstOrDefault(s => s.Domain.Id == domainId && s.AccountType == CoANode.BKAccountTypeEnum.Revenue);
                var expenses = dbContext.BKGroups.FirstOrDefault(s => s.Domain.Id == domainId && s.AccountType == CoANode.BKAccountTypeEnum.Expenses);
                var netIncomeRow = table.NewRow();
                netIncomeRow["NodeName"] = "Net income";
                netIncomeRow["NodeId"] = "subtotal_0";
                string space = "";
                #region Subgroup Revenue
                foreach (var item in reportTemplate.ReportEntries.Where(s => s.CoANode.Parent.Id == revenue.Id))
                {
                    var revrow = RowExpandedNodes(lstTransactions, item.CoANode, incomeConfig.incomeReportEntry, ref table, ref space, ref netIncomeRow, 1);
                    //Subtotal
                    #region row data InlineReportEntry
                    if (item.InlineReportEntry != null)
                    {
                        #region Row Expenes
                        var exp = item.InlineReportEntry.ExpenseReportEntry;
                        var dataExpRow = table.NewRow();
                        //NodeName
                        dataExpRow["NodeName"] = exp.CoANode.Name;
                        //NodeId
                        dataExpRow["NodeId"] = "in_" + exp.CoANode.Id;
                        //Columns dynamic
                        foreach (var cl in lstColumns)
                        {
                            //Get All child CoANode
                            var currentNode = incomeConfig.incomeReportEntry.FirstOrDefault(s => s.id == exp.CoANode.Id);
                            var children = currentNode != null ? currentNode.children : new List<int>();
                            //End
                            //ALL approved transaction related to the CoANode
                            var trans = lstTransactions.FirstOrDefault(s => children.Contains(s.NodeId) && s.ColumnName == cl.ColumnName);
                            var cellvalue = trans != null ? (-1 * (trans.Credit - trans.Debit)) : 0;
                            dataExpRow[cl] = cellvalue;
                            netIncomeRow[cl] = (netIncomeRow[cl] != DBNull.Value ? Convert.ToDecimal(netIncomeRow[cl]) : 0) - cellvalue;
                        }
                        table.Rows.Add(dataExpRow);
                        #endregion
                        #region Row Subtotal
                        var dataSubtotal = table.NewRow();
                        //NodeName
                        dataSubtotal["NodeName"] = space + "&emsp;&emsp;&emsp;" + item.InlineReportEntry.SubTotalTitle;
                        //NodeId
                        dataSubtotal["NodeId"] = "subtotal_0";
                        //Columns dynamic
                        foreach (var cl in lstColumns)
                        {
                            var celvalue_rev = revrow[cl] != DBNull.Value ? Convert.ToDecimal(revrow[cl]) : 0;
                            var celvalue_exp = dataExpRow[cl] != DBNull.Value ? Convert.ToDecimal(dataExpRow[cl]) : 0;
                            dataSubtotal[cl] = celvalue_rev - celvalue_exp;
                        }
                        table.Rows.Add(dataSubtotal);
                        #endregion
                    }
                    //Row empty
                    table.Rows.Add(table.NewRow());
                    #endregion
                    space = "";
                }
                #endregion
                #region Subgroup Expenses
                space = "";
                foreach (var item in reportTemplate.ReportEntries.Where(s => s.CoANode.Parent.Id == expenses.Id))
                {
                    RowExpandedNodes(lstTransactions, item.CoANode, incomeConfig.incomeReportEntry, ref table, ref space, ref netIncomeRow, 2);
                }
                #endregion
                //Row empty
                table.Rows.Add(table.NewRow());
                //Add net Income Row
                table.Rows.Add(netIncomeRow);
                //ref startdate and enddate
                start_date = startDate.ConvertTimeFromUtc(incomeConfig.timezone).ToString(incomeConfig.date_format);
                end_date = endDate.AddDays(-1).ConvertTimeFromUtc(incomeConfig.timezone).ToString(incomeConfig.date_format);
                return table;
                #endregion
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, incomeConfig);
                start_date = incomeConfig.start_date;
                end_date = incomeConfig.end_date;
                return new DataTable();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstTransactions">Transactions</param>
        /// <param name="node">CoANode</param>
        /// <param name="trees">TreeReportEntry</param>
        /// <param name="table">DataTable</param>
        /// <param name="space">space</param>
        /// <param name="netIncomeRow">Datarow</param>
        /// <param name="typeRef">1:Sum;2:minus;3:do nothing</param>
        /// <returns></returns>
        private DataRow RowExpandedNodes(List<BKTransacsionsCustome> lstTransactions, CoANode node, List<TreeReportEntry> trees, ref DataTable table, ref string space, ref DataRow netIncomeRow, int typeRef)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Row Expanded Nodes", null, null, lstTransactions, node, trees, typeRef);


                var row = table.NewRow();
                var columns = table.Columns.Cast<DataColumn>().ToList();
                #region row data
                var dataRow = table.NewRow();
                //NodeName
                dataRow["NodeName"] = space + node.Name;
                //NodeId
                dataRow["NodeId"] = (typeRef == 1 ? "rev_" : "") + node.Id;
                //Columns dynamic
                foreach (var cl in columns)
                {
                    if (cl.ColumnName != "NodeName" && cl.ColumnName != "NodeId")
                    {
                        //Get All child CoANode
                        var currentNode = trees.FirstOrDefault(s => s.id == node.Id);
                        var children = currentNode != null ? currentNode.children : new List<int>();
                        //End
                        //ALL approved transaction related to the CoANode
                        var celvalue = lstTransactions.Where(s => children.Contains(s.NodeId) && s.ColumnName == cl.ColumnName).Sum(s => s.Credit - s.Debit);
                        dataRow[cl] = celvalue;
                        if (typeRef == 1)
                        {
                            netIncomeRow[cl] = ((netIncomeRow[cl] != DBNull.Value ? Convert.ToDecimal(netIncomeRow[cl]) : 0) + celvalue);
                        }
                        else if (typeRef == 2)
                        {
                            dataRow[cl] = (-1 * celvalue);
                            netIncomeRow[cl] = ((netIncomeRow[cl] != DBNull.Value ? Convert.ToDecimal(netIncomeRow[cl]) : 0) - (-1 * celvalue));
                        }
                    }

                }
                table.Rows.Add(dataRow);
                #endregion
                if (trees.Any(s => s.id == node.Id && s.isExpanded) && node.Children.Any())
                {
                    var tempSpace = space;
                    space += ("&emsp;&emsp;&emsp;" + space);
                    foreach (var item in node.Children)
                    {
                        RowExpandedNodes(lstTransactions, item, trees, ref table, ref space, ref netIncomeRow, 3);
                    }
                    space = tempSpace;
                }
                return dataRow;



            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstTransactions, node, trees, typeRef);
                return null;
            }
        }

        public BKGroup GetBKGroup(int domainId, CoANode.BKAccountTypeEnum accountType)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetBKGroup", null, null, domainId, accountType);


                return dbContext.BKGroups.FirstOrDefault(s => s.Domain.Id == domainId && s.AccountType == accountType);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, accountType);
                return null;
            }
        }

        public List<TreeBalanceReportEntry> ReportBalance(int domainId, ReportBalanceConfig balanceConfig)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MESSAGE_HERE", null, null, domainId, balanceConfig);

                var tz = TimeZoneInfo.FindSystemTimeZoneById(balanceConfig.timezone);
                var startDate = balanceConfig.start_date.ConvertDateFormat(balanceConfig.date_format).ConvertTimeToUtc(tz);
                startDate = startDate.AddDays(1);
                List<BKTransacsionModel> query = (from trans in dbContext.BKTransactions
                                                  join ac in dbContext.BKAccounts on trans.Account.Id equals ac.Id
                                                  where ac.Domain.Id == domainId
                                                  && trans.PostedDate < startDate
                                                  && trans.JournalEntry.IsApproved
                                                  && balanceConfig.allNodeIds.Contains(ac.Id)
                                                  select new BKTransacsionModel
                                                  {
                                                      NodeId = ac.Id,
                                                      InitBabanceAccount = ac.InitialBalance,
                                                      Credit = trans.Credit != null ? trans.Credit.Value : 0,
                                                      Debit = trans.Debit != null ? trans.Debit.Value : 0,
                                                      date = trans.PostedDate
                                                  }).ToList();

                var lstTransactions = query.GroupBy(s => new { s.NodeId, s.InitBabanceAccount })
                .Select(g => new BKTransacsionsCustome
                {
                    NodeId = g.Key.NodeId,
                    InitBabanceAccount = g.Key.InitBabanceAccount,
                    Credit = g.Sum(x => x.Credit),
                    Debit = g.Sum(x => x.Debit),
                }).ToList();
                List<TreeBalanceReportEntry> tree = balanceConfig.incomeReportEntry;
                BindAmountReportEntry(lstTransactions, ref tree);

                return tree;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, balanceConfig);
                return new List<TreeBalanceReportEntry>();
            }
        }

        private void BindAmountReportEntry(List<BKTransacsionsCustome> bKTransacsions, ref List<TreeBalanceReportEntry> treeReports)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BindAmountReportEntry", null, null, bKTransacsions);


                foreach (var item in treeReports)
                {
                    var trans = bKTransacsions.Where(s => item.allChildIds.Contains(s.NodeId));
                    item.amount = trans.Sum(s => s.InitBabanceAccount) + trans.Sum(s => s.Credit - s.Debit);

                    //Child ANodes
                    if (item.children != null && item.children.Any())
                    {
                        List<TreeBalanceReportEntry> tree = item.children;
                        BindAmountReportEntry(bKTransacsions, ref tree);
                        item.children = tree;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, bKTransacsions);

            }
        }
    }
}