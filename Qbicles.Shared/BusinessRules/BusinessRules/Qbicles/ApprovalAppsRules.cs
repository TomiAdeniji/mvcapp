using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Form;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qbicles.BusinessRules
{
    public class ApprovalAppsRules
    {
        private readonly ApplicationDbContext _db;

        public ApprovalAppsRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();

        public ApprovalGroup GetApprovalAppsGroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval apps group by id", null, null, id);

                return DbContext.ApprovalAppsGroup.First(e => e.Id == id);
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
            
        }

        private List<ApprovalGroup> GetApprovalAppsGroupByAppId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval apps group by app id", null, null, id);

                return DbContext.ApprovalAppsGroup.Where(e => e.AppInstance.Id == id).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new List<ApprovalGroup>();
            }
            
        }

        /// <summary>
        ///     check duplicate name of group process
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="title"></param>
        /// <param name="currentDomain"></param>
        /// <returns></returns>
        public bool DuplicateApprovalAppsGroupNameCheck(int groupId, string title, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check dupplicate apps group name", null, null, groupId, title, domainId);

                var currentDomain = DbContext.Domains.Find(domainId);
                var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                if (ai != null)
                {
                    if (groupId > 0)
                        return DbContext.ApprovalAppsGroup.Any(x =>
                            x.Id != groupId && x.AppInstance.Id == ai.Id && x.Name == title);
                    return DbContext.ApprovalAppsGroup.Any(x => x.AppInstance.Id == ai.Id && x.Name == title);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupId, title, domainId);
                return true;
            }
        }

        public ApprovalRequestDefinition GetApprovalAppById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval app by id", null, null, id);

                return DbContext.ApprovalAppsRequestDefinition.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
            
        }

        public List<ApprovalGroupAppsModel> SearchApprovalAppsByText(string textSearch, List<ApprovalGroup> theGroups)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Search approval apps by text", null, null, textSearch, theGroups);


                return theGroups.Select(c => new ApprovalGroupAppsModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Approvals = GetApprovals(c, textSearch.ToUpper())
                }).Where(g => g.Approvals.Count > 0).OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, textSearch, theGroups);
                return new List<ApprovalGroupAppsModel>() ;
            }
        }

        private List<ApprovalRequestDefinition> GetApprovals(ApprovalGroup group, string textSearch)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approvals", null, null, group, textSearch);

                return group.Approvals.Where(p =>
                    p.Title.ToUpper().Contains(textSearch) || p.Description.ToUpper().Contains(textSearch)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, group, textSearch);
                return new List<ApprovalRequestDefinition>();
            }

        }

        public ReturnJsonModel SaveApprovalAppGroup(int groupId, string groupName, string userId, int domainId)
        {
            var currentDomain = DbContext.Domains.Find(domainId);
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save approval app group", null, null, groupId, groupName, userId, domainId);
                
                var user = DbContext.QbicleUser.Find(userId);
                var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                if (ai != null)
                {
                    if (groupId == 0)
                    {
                        var approvalGroup = new ApprovalGroup
                        {
                            Name = groupName,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            AppInstance = ai
                        };
                        DbContext.ApprovalAppsGroup.Add(approvalGroup);
                        DbContext.Entry(approvalGroup).State = EntityState.Added;
                        DbContext.SaveChanges();
                        refModel.msgId = approvalGroup.Id.ToString();
                    }
                    else
                    {
                        refModel.msgId = groupId.ToString();
                        var approvalGroup = GetApprovalAppsGroupById(groupId);
                        approvalGroup.Name = groupName;
                        if (DbContext.Entry(approvalGroup).State == EntityState.Detached)
                            DbContext.ApprovalAppsGroup.Attach(approvalGroup);
                        DbContext.Entry(approvalGroup).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }

                    // generate process group html to append
                    refModel.Object = new ApprovalGroup
                    {
                        Name = groupName,
                        Id = int.Parse(refModel.msgId)
                    };
                    refModel.msgName = groupName;
                    refModel.msg = "";
                }
                else
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_676", null));
                    refModel.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupId, groupName, userId, domainId);
                refModel.result = false;
            }

            return refModel;
        }


        public ReturnJsonModel DuplicateApprovalAppNameCheck(ApprovalAppModel theApprovalApp, int domainId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate approval app name", null, null, theApprovalApp, domainId);

                var currentDomain = DbContext.Domains.Find(domainId);
                var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                if (ai != null)
                {
                    if (theApprovalApp.Id > 0)
                        refModel.result = DbContext.ApprovalAppsRequestDefinition.Any(x =>
                            x.Id != theApprovalApp.Id && x.Group.AppInstance.Id == ai.Id &&
                            x.Title == theApprovalApp.Name);
                    else
                        refModel.result = DbContext.ApprovalAppsRequestDefinition.Any(x =>
                            x.Group.AppInstance.Id == ai.Id && x.Title == theApprovalApp.Name);
                }
                else
                {
                    refModel.result = true;
                }

                if (refModel.result)
                    refModel.msg = ResourcesManager._L("ERROR_MSG_307");
            }
            catch (Exception ex)
            {
                refModel.result = true;
                refModel.msg = ex.ToString();
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, theApprovalApp, domainId);
            }

            return refModel;
        }


        public List<FormDefinition> GetFormsRelatedByApprovalGroupId(int approvalGroupId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get forms related by approval group id", null, null, approvalGroupId);

                var result = new List<FormDefinition>();
                var approvalAppsRequestDefinition =
                    DbContext.ApprovalAppsRequestDefinition.FirstOrDefault(x => x.Group.Id == approvalGroupId);
                if (approvalAppsRequestDefinition != null) result = approvalAppsRequestDefinition.Forms;
                return result;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approvalGroupId);
                return new List<FormDefinition>();
            }
           
        }

        public ReturnJsonModel SaveApprovalApp(ApprovalAppModel theApprovalApp, string documents,
            string userId, List<int> formRelatedIds)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save approval app", userId, null, theApprovalApp, documents, formRelatedIds);
                
                var userRoles = new UserRules(DbContext);
                var proRelated = JsonConvert.DeserializeObject<List<ProcessDocumentModel>>(documents);
                var initiates = JsonConvert.DeserializeObject<List<string>>(theApprovalApp.Initiate);
                var approvers = JsonConvert.DeserializeObject<List<string>>(theApprovalApp.Approval);
                var reviewers = JsonConvert.DeserializeObject<List<string>>(theApprovalApp.Reviewer);
                var theGroup = GetApprovalAppsGroupById(theApprovalApp.GroupId);
                var user = DbContext.QbicleUser.Find(userId);
                if (theApprovalApp.Id == 0)
                {
                    var apptovalApp = new ApprovalRequestDefinition
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Description = theApprovalApp.Description,
                        Title = theApprovalApp.Name,
                        ApprovalImage = theApprovalApp.ProcessImage,
                        Group = theGroup,
                        Type = theApprovalApp.Type
                    };

                    DbContext.ApprovalAppsRequestDefinition.Add(apptovalApp);
                    foreach (var formId in formRelatedIds)
                    {
                        var form = new FormDefinitionRules(DbContext).GetFormDefinitionById(formId);
                        apptovalApp.Forms.Add(form);
                    }

                    if (initiates != null)
                        foreach (var u in initiates)
                            apptovalApp.Initiators.Add(userRoles.GetUser(u, 0));
                    if (reviewers != null)
                        foreach (var u in reviewers)
                            apptovalApp.Reviewers.Add(userRoles.GetUser(u, 0));
                    if (approvers != null)
                        foreach (var u in approvers)
                            apptovalApp.Approvers.Add(userRoles.GetUser(u, 0));

                    if (proRelated != null)
                        foreach (var rel in proRelated)
                        {
                            var thedoc = new ApprovalDocument
                            {
                                AppDocumentImage = rel.DocumentImage,
                                CreatedBy = user,
                                CreatedDate = DateTime.UtcNow,
                                Document = rel.Document,
                                Approval = apptovalApp,
                                FileType = new FileTypeRules(DbContext).GetFileTypeById(rel.FileTypeId)
                            };
                            DbContext.ApprovalDocument.Add(thedoc);
                        }

                    DbContext.SaveChanges();

                    var approvelAppItem = new StringBuilder();
                    approvelAppItem.AppendFormat(
                        "<a onclick='EditProcessApp({0},{1})' class='asset_group_item' data-toggle='modal' data-target='#create-approval-type'>",
                        apptovalApp.Id, apptovalApp.Group.Id);
                    approvelAppItem.AppendFormat("<img id='image-approval-app-{0}' src='{1}' class='img-responsive'>",
                        theApprovalApp.Id, theApprovalApp.ProcessImage);
                    approvelAppItem.AppendFormat(
                        "<div class='detail'><h5><span id='title-approval-app-{0}'>{0}</span></h5></div>",
                        theApprovalApp.Name);
                    approvelAppItem.Append("</a>");
                    refModel.msg = approvelAppItem.ToString();
                }
                else
                {
                    var approvalApp = GetApprovalAppById(theApprovalApp.Id);

                    approvalApp.Description = theApprovalApp.Description;
                    approvalApp.Title = theApprovalApp.Name;
                    approvalApp.Type = theApprovalApp.Type;
                    if (!string.IsNullOrEmpty(theApprovalApp.ProcessImage))
                        approvalApp.ApprovalImage = theApprovalApp.ProcessImage;

                    approvalApp.Group = theGroup;

                    var removeds = approvalApp.Forms.Select(x => x.Id).Except(formRelatedIds);
                    var addNews = formRelatedIds.Except(approvalApp.Forms.Select(x => x.Id));
                    foreach (var formId in removeds)
                        approvalApp.Forms.Remove(approvalApp.Forms.FirstOrDefault(x => x.Id == formId));

                    foreach (var formId in addNews)
                    {
                        var form = new FormDefinitionRules(DbContext).GetFormDefinitionById(formId);
                        approvalApp.Forms.Add(form);
                    }

                    if (DbContext.Entry(approvalApp).State == EntityState.Detached)
                        DbContext.ApprovalAppsRequestDefinition.Attach(approvalApp);
                    DbContext.Entry(approvalApp).State = EntityState.Modified;

                    DbContext.ApprovalDocument.RemoveRange(approvalApp.Documents);


                    approvalApp.Reviewers.Clear();
                    approvalApp.Initiators.Clear();
                    approvalApp.Approvers.Clear();

                    foreach (var u in initiates)
                        approvalApp.Initiators.Add(userRoles.GetUser(u, 0));
                    foreach (var u in reviewers)
                        approvalApp.Reviewers.Add(userRoles.GetUser(u, 0));
                    foreach (var u in approvers)
                        approvalApp.Approvers.Add(userRoles.GetUser(u, 0));

                    foreach (var rel in proRelated)
                    {
                        var thedoc = new ApprovalDocument
                        {
                            AppDocumentImage = rel.DocumentImage,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            Document = rel.Document,
                            Approval = approvalApp,
                            FileType = new FileTypeRules(DbContext).GetFileTypeById(rel.FileTypeId)
                        };
                        DbContext.ApprovalDocument.Add(thedoc);
                    }

                    DbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, theApprovalApp, documents, formRelatedIds);
                refModel.result = false;
            }

            return refModel;
        }

        public ApprovalAppModel MapApprovalAppToModal(ApprovalRequestDefinition app, List<string> appRolesRight)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Map approval app to modal", null, null, app, appRolesRight);

                var approvalTypes = HelperClass.EnumModel.GetEnumValuesAndDescriptions<ApprovalRequestDefinition.RequestTypeEnum>();
                var approval = new ApprovalAppModel
                {
                    Id = app.Id,
                    Approval = string.Join(",", app.Approvers.Select(x => x.Id).ToArray()),
                    ApprovalName = string.Join(", ", app.Approvers.Select(x => HelperClass.GetFullNameOfUser(x)).ToArray()),
                    Description = app.Description,
                    GroupId = app.Group.Id,
                    GroupName = app.Group.Name,
                    Initiate = string.Join(",", app.Initiators.Select(x => x.Id).ToArray()),
                    InitiateName =
                        string.Join(", ", app.Initiators.Select(x => HelperClass.GetFullNameOfUser(x)).ToArray()),
                    Name = app.Title,
                    ProcessImage =  app.ApprovalImage.ToUriString(),
                    Reviewer = string.Join(",", app.Reviewers.Select(x => x.Id).ToArray()),
                    ReviewerName = string.Join(", ", app.Reviewers.Select(x => HelperClass.GetFullNameOfUser(x)).ToArray()),
                    Type = app.Type,
                    TypeName = approvalTypes.FirstOrDefault(e => e.Key == (int)app.Type)?.Value ?? "",
                    DocumentHtml = DocumentToHtml(app.Documents, appRolesRight)
                    //Forms = app.Forms.Select(x => new KeyValuePair<string, int>(x.Name, x.Id)).OrderByDescending(f => f.Key)
                    //    .ToList()
                };

                return approval;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, app, appRolesRight);
                return null;
            }
           
        }

        private string DocumentToHtml(List<ApprovalDocument> documents, List<string> appRolesRight)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Convert document to html", null, null, documents, appRolesRight);

                var html = new StringBuilder();
                var docId = Guid.NewGuid();
                var disabled = appRolesRight.Any(r => r == RightPermissions.EditContent) == false ? "disabled" : "";
                foreach (var doc in documents)
                {
                    html.AppendFormat("<tr class='pro-document-tr' id='{0}'>", docId);
                    html.AppendFormat("<td id='document-{0}' width='60%'>{1}</td>", docId, doc.Document);
                    html.AppendFormat("<td id='documentImage-{0}' data-url='{1}' width='20%' class='table_file_image'>",
                        docId, doc.FileType != null ? doc.FileType.IconPath : "");
                    html.AppendFormat("<img src='{0}' style='width:32px;'>",
                        doc.FileType != null ? doc.FileType.IconPath : "");
                    html.Append("</td>");
                    html.AppendFormat("<td id='filetypeId-{0}' style='display:none;'>{1}</td>", docId,
                        doc.FileType != null ? doc.FileType.Extension : "");
                    html.Append("<td width='20%'>");
                    html.AppendFormat("<a class='btn btn-success' download href='{0}'><i class='fa fa-download'></i></a>",
                        doc.AppDocumentImage);
                    html.AppendFormat(
                        "<a {1} onclick=\"RemoveDocument('{0}');\" class='btn-remove-document btn btn-danger'><i class='fa fa-remove'></i></a>",
                        docId, disabled);
                    html.Append("</td></tr>");
                    docId = Guid.NewGuid();
                }

                return html.ToString();
            
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, documents, appRolesRight);
                return "";
            }
        }

        /// <summary>
        ///     get current  group approval include approvals
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApprovalGroup> GetCurrentApprovalAppsGroup(string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get current approval apps group", null, null, userId, domainId);

                var currentDomain = DbContext.Domains.Find(domainId);
                var theGroups = new List<ApprovalGroup>();

                var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                if (ai != null)
                    theGroups.AddRange(GetApprovalAppsGroupByAppId(ai.Id));
                return theGroups.OrderBy(n => n.Name);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return new List<ApprovalGroup>();
            }
        }

        /// <summary>
        ///     New Approval request menu shown for any user that has been set as an Initiator in an Approval Request Definition
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public bool UserAsInitiators(string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Users as initiators", null, null, userId, domainId);

                var currentDomain = DbContext.Domains.Find(domainId);
                if (currentDomain != null)
                {
                    var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                    if (ai != null)
                    {
                        var groups = GetApprovalAppsGroupByAppId(ai.Id);
                        return groups.Any(a => a.Approvals.Any(u => u.Initiators.Any(uId => uId.Id == userId)));
                    }

                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return false;
            }
        }

        /// <summary>
        ///     Get all current approvals
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApprovalRequestDefinition> GetCurrentApprovalApps(string userId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get current approval apps", null, null, userId, domainId);

                var currentDomain = DbContext.Domains.Find(domainId);
                var theGroups = new List<ApprovalGroup>();
                var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                if (ai != null) theGroups.AddRange(GetApprovalAppsGroupByAppId(ai.Id));

                var approvals = theGroups.SelectMany(ap => ap.Approvals);

                return approvals;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return null;
            }
        }

        /// <summary>
        ///     Get all user as reviewer and approver in the Approval application
        /// </summary>
        /// <param name="approvalId"></param>
        /// <returns></returns>
        public ApprovalUsersAssociatedModel GetApprovalUsersAssociated(int approvalId)
        {
            try{
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval users associated", null, null, approvalId);

                var associated = new ApprovalUsersAssociatedModel();
                var approvalApplication = GetApprovalAppById(approvalId);
                associated.Reviewers = approvalApplication.Reviewers;
                associated.Approvers = approvalApplication.Approvers;
                return associated;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approvalId);
                return null;
            }
        }


        public IsReviewerAndApproverModel GetIsReviewerAndApprover(int approvalId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get is reviewer and approvar", null, null, approvalId, userId);


                var approval = GetApprovalAppById(approvalId);
                var result = new IsReviewerAndApproverModel
                {
                    IsReviewer = approval?.Reviewers.Any(u => u.Id == userId) ?? false,
                    IsApprover = approval?.Approvers.Any(u => u.Id == userId) ?? false
                };

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approvalId, userId);
                return null;
            }

        }

        public IsReviewerAndApproverModel GetTraderApprovalRight(int approvalId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get trader approval right", null, null, approvalId, userId);

                var approval = GetApprovalAppById(approvalId) ?? new ApprovalRequestDefinition();
                var result = new IsReviewerAndApproverModel
                {
                    IsReviewer = approval.Reviewers?.Any(u => u.Id == userId) ?? false,
                    IsApprover = approval.Approvers?.Any(u => u.Id == userId) ?? false,
                    IsInitiators = approval.Initiators?.Any(u => u.Id == userId) ?? false
                };
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approvalId, userId);
                return new IsReviewerAndApproverModel();
            }
        }

        public ReturnJsonModel ChangeAvailableApps(string userId, int roleId, int domainId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Change available apps", null, null, userId, roleId, domainId);

                var currentDomain = DbContext.Domains.Find(domainId);

                bool appReged;
                string check;
                var appsByAccount = currentDomain.SubscribedApps.Where(e=> !e.IsCore);
                var roleRights = DbContext.RoleRightAppXref.Where(x =>
                    x.AppInstance.Domain.Id == currentDomain.Id && x.Role.Id == roleId);

                var html = new StringBuilder();
                html.Append(
                    "<div class='modal-dialog' role='document'><div class='modal-content'><div class='modal-header'>");
                html.Append(
                    "<button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>");
                html.Append(
                    "<h5 class='modal-title'>Manage Available Apps</h5></div><div class='modal-body'><div class='well tan'>");
                foreach (var item in appsByAccount)
                {
                    string _onchange = "";
                    if (item.Name == HelperClass.appMyBankMate)
                        _onchange = "onchange='checkBankmateAccountSetup(this)'";
                    if (roleRights.Any(x => x.AppInstance.QbicleApplication.Id == item.Id))
                    {
                        check = "checked";
                        appReged = true;
                    }
                    else
                    {
                        check = "";
                        appReged = false;
                    }

                    html.Append("<div class='checkbox toggle'><label>");
                    html.AppendFormat(
                        "<input data-toggle='toggle' class='apps-account' appRed='{2}' id='app-account-{0}' value='{0}' data-onstyle='success' type='checkbox' {1} {3}>",
                        item.Id, check, appReged, _onchange);
                    html.Append(item.Name);
                    html.Append("</label></div>");
                }

                html.Append("</div>");
                html.Append(
                    "<button type='button' onclick='updateAppsForDomain()' class='btn btn-success'><i class='fa fa-check'></i> &nbsp; Update</button>&nbsp;");
                html.Append(
                    "<button class='btn btn-danger' data-dismiss='modal'><i class='fa fa-remove'></i> &nbsp; Cancel</button>");
                html.Append("</div></div></div>");


                refModel.msg = html.ToString();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, roleId, domainId);
            }

            return refModel;
        }

        public bool DeleteApprovalRequest(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete approval request", null, null, id);

                var approvalRequest = DbContext.ApprovalAppsRequestDefinition.Find(id);
                DbContext.ApprovalAppsRequestDefinition.Attach(approvalRequest ?? throw new InvalidOperationException());
                DbContext.ApprovalAppsRequestDefinition.Remove(approvalRequest);

                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public ApprovalGroup GetApprovalAppsGroupByName(string name, QbicleDomain domain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval apps group by name", null, null, name, domain);

                var ai = domain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                return ai != null
                    ? DbContext.ApprovalAppsGroup.FirstOrDefault(x => x.AppInstance.Id == ai.Id && x.Name == name)
                    : null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, domain);
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="groupName"></param>
        /// <param name="user">always send from Rules</param>
        /// <param name="currentDomain"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveApprovalAppGroup(int groupId, string groupName, ApplicationUser user, QbicleDomain currentDomain)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save approval app group", user.Id, null, groupId, groupName, user, currentDomain);

                var ai = currentDomain.AssociatedApps.FirstOrDefault(q => q.QbicleApplication.Name == HelperClass.appTypeApprovals);
                if (ai != null)
                {
                    if (groupId == 0)
                    {
                        var approvalGroup = new ApprovalGroup
                        {
                            Name = groupName,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            AppInstance = ai
                        };
                        DbContext.ApprovalAppsGroup.Add(approvalGroup);
                        DbContext.Entry(approvalGroup).State = EntityState.Added;
                        DbContext.SaveChanges();
                        refModel.msgId = approvalGroup.Id.ToString();
                    }
                    else
                    {
                        refModel.msgId = groupId.ToString();
                        var approvalGroup = GetApprovalAppsGroupById(groupId);
                        approvalGroup.Name = groupName;
                        if (DbContext.Entry(approvalGroup).State == EntityState.Detached)
                            DbContext.ApprovalAppsGroup.Attach(approvalGroup);
                        DbContext.Entry(approvalGroup).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }

                    // generate process group html to append
                    refModel.Object = new ApprovalGroup
                    {
                        Name = groupName,
                        Id = int.Parse(refModel.msgId)
                    };
                    refModel.msgName = groupName;
                    refModel.msg = "";
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_306");
                    refModel.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, groupId, groupName, user, currentDomain);
                refModel.result = false;
            }

            return refModel;
        }
    }
}