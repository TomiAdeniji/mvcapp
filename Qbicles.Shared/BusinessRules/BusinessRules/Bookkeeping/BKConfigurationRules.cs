using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Bookkeeping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qbicles.BusinessRules
{

    public class BKConfigurationRules
    {
        ApplicationDbContext dbContext;
       
        public BKConfigurationRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ReturnJsonModel SaveDefaultBKAppSetting(BKAppSettings bkappsetting)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDefaultBKAppSetting", null, null, bkappsetting);



                if (bkappsetting != null)
                {
                    if (bkappsetting.Id > 0)
                    {
                        var domain = bkappsetting.Domain;

                        var bk_appsetting = dbContext.BKAppSettings.Where(q => q.Id == bkappsetting.Id).FirstOrDefault();
                        //bk_appsetting.ApprovalQbicle = domain.Qbicles.FirstOrDefault(q => q.Id == bkappsetting.ApprovalQbicle.Id);
                        bk_appsetting.AttachmentQbicle = domain.Qbicles.FirstOrDefault(q => q.Id == bkappsetting.AttachmentQbicle.Id);
                        bk_appsetting.AttachmentDefaultTopic = dbContext.Topics.FirstOrDefault(q => q.Id == bkappsetting.AttachmentDefaultTopic.Id);
                        //bk_appsetting.DefaultTopic = DbContext.Topics.FirstOrDefault(q => q.Id == bkappsetting.DefaultTopic.Id);
                        //bk_appsetting.JournalEntryApprovalProcess = DbContext.ApprovalAppsRequestDefinition.Find(bkappsetting.JournalEntryApprovalProcess.Id);




                        if (dbContext.Entry(bk_appsetting).State == EntityState.Detached)
                            dbContext.BKAppSettings.Attach(bk_appsetting);
                        dbContext.Entry(bk_appsetting).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        refModel.actionVal = 2;
                        refModel.msg = bk_appsetting.Domain.Name;
                        refModel.msgId = bk_appsetting.Id.ToString();
                        refModel.msgName = bk_appsetting.Domain.Name;
                    }
                    else
                    {
                        bkappsetting.AttachmentQbicle = dbContext.Qbicles.FirstOrDefault(q => q.Id == bkappsetting.AttachmentQbicle.Id);
                        bkappsetting.AttachmentDefaultTopic = dbContext.Topics.FirstOrDefault(q => q.Id == bkappsetting.AttachmentDefaultTopic.Id);
                        dbContext.BKAppSettings.Add(bkappsetting);
                        dbContext.Entry(bkappsetting).State = EntityState.Added;
                        dbContext.SaveChanges();
                        refModel.actionVal = 1;
                        //append to select group
                        refModel.msgId = bkappsetting.Id.ToString();
                        refModel.msgName = bkappsetting.Domain.Name;
                    }
                    refModel.result = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, bkappsetting);
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
            }
            return refModel;
        }

        public static string GenTreeView(object node, int linkedId = 0)
        {

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "string GenTreeView", null, null, node);


                CoANode.BKCoANodeTypeEnum type;

                if (node as List<BKGroup> != null)
                    type = CoANode.BKCoANodeTypeEnum.GroupList;
                else
                {
                    var theNode = (CoANode)node;
                    type = theNode.NodeType;
                }
                var strNode = new StringBuilder();
                if (type == CoANode.BKCoANodeTypeEnum.GroupList)
                {
                    var nodes = (List<BKGroup>)node;
                    strNode.Append("<ul>\r\n");
                    foreach (var n in nodes)
                    {
                        strNode.Append(GenTreeView((n as CoANode), linkedId));
                    }
                    strNode.Append("</ul>");
                }
                else if (type == CoANode.BKCoANodeTypeEnum.Group)
                {
                    var nodeBKGroup = (BKGroup)node;
                    strNode.Append("<li class=\"groupaccount_" + nodeBKGroup.Id + "\" onclick=\"initSelectedAccount()\" data-name=\"" + nodeBKGroup.Name + "\" data-node=\"BKGroup\" data-value=\"" + nodeBKGroup.Id + "\" data-jstree = '{\"icon\":\"../Content/DesignStyle/img/tree-group.png\"}'>\r\n");
                    strNode.Append("<h5> " + nodeBKGroup.Name + " </h5>\r\n");
                    strNode.Append("<span> " + nodeBKGroup.AccountType + " </span>\r\n");
                    if (nodeBKGroup.Children != null && nodeBKGroup.Children.Count > 0)
                    {
                        strNode.Append("<ul>\r\n");
                        foreach (var c in nodeBKGroup.Children)
                        {
                            strNode.Append(GenTreeView((c as CoANode), linkedId));
                        }
                        strNode.Append("</ul>\r\n");
                    }
                    strNode.Append("</li>\r\n");
                }
                else if (type == CoANode.BKCoANodeTypeEnum.SubGroup)
                {
                    var nodeBKSubGroup = (CoANode)node;
                    strNode.Append("<li class=\"groupaccount_" + nodeBKSubGroup.Id + "\" onclick=\"initSelectedAccount()\" data-name=\"" + nodeBKSubGroup.Name + "\" data-node=\"BKSubGroup\" data-value=\"" + nodeBKSubGroup.Id + "\" data-jstree = '{\"icon\":\"../Content/DesignStyle/img/tree-group.png\"}'>\r\n");
                    strNode.Append($"<h5>{nodeBKSubGroup.Number} - {nodeBKSubGroup.Name}</h5>\r\n");
                    strNode.Append("<span> " + nodeBKSubGroup.AccountType + " </span>\r\n");
                    if (nodeBKSubGroup.Children != null && nodeBKSubGroup.Children.Count > 0)
                    {
                        strNode.Append("<ul>\r\n");
                        foreach (var c in nodeBKSubGroup.Children)
                        {
                            strNode.Append(GenTreeView((c as CoANode), linkedId));
                        }
                        strNode.Append("</ul>\r\n");
                    }
                    strNode.Append("</li>\r\n");

                }
                else if (type == CoANode.BKCoANodeTypeEnum.Account)
                {
                    var nodeBKAccount = (BKAccount)node;
                    if (nodeBKAccount.Id == linkedId)
                    {
                        strNode.Append("<li id=\"selected-account\" class=\"groupaccount_" + nodeBKAccount.Id + " accountid-" + nodeBKAccount.Id + "\"  data-name=\"" + nodeBKAccount.Name + "\" data-node=\"BKAccount\" data-value=\"" + nodeBKAccount.Id + "\" data-jstree = '{\"icon\":\"../Content/DesignStyle/img/tree-bank.png\"}'>\r\n");
                    }
                    else
                    {
                        strNode.Append("<li class=\"groupaccount_" + nodeBKAccount.Id + " accountid-" + nodeBKAccount.Id + "\"  data-name=\"" + nodeBKAccount.Name + "\" data-node=\"BKAccount\" data-value=\"" + nodeBKAccount.Id + "\" data-jstree = '{\"icon\":\"../Content/DesignStyle/img/tree-bank.png\"}'>\r\n");
                    }
                    strNode.Append("<a href = \"\">\r\n");
                    strNode.Append($"<h5>{nodeBKAccount.Number} - {nodeBKAccount.Name}</h5>\r\n");
                    strNode.Append("<span> " + nodeBKAccount.Code + " </span>\r\n");
                    //strNode.Append($"<button class='btn btn-success' onclick=\"buttonsSelectAccount({nodeBKAccount.Id},'{nodeBKAccount.Name}')\"><i class='fa fa-check'></i></button>\r\n");
                    if(nodeBKAccount.Id == linkedId)
                    {
                        strNode.Append("<button class=\"btn btn-info\" onclick=\"selectAccount(this," + nodeBKAccount.Id + ");\"><i class=\"fa fa-thumbs-up\"></i></button>\r\n");
                    }
                    else
                    {
                        strNode.Append("<button class=\"btn btn-success\" onclick=\"selectAccount(this," + nodeBKAccount.Id + ");\"><i class=\"fa fa-check\"></i></button>\r\n");
                    }
                    strNode.Append("</a>\r\n");
                    strNode.Append("</li>\r\n");
                }
                return strNode.ToString();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return "";
            }
        }

        public BKAppSettings GetBKAppSettingByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BKAppSettings GetBKAppSettingByDomain", null, null, domainId);

                return dbContext.BKAppSettings.FirstOrDefault(e => e.Domain.Id == domainId);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }
    }
}
