using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Form;
using Qbicles.Models.Operator.Compliance;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorFormRules
    {
        ApplicationDbContext dbContext;
        public OperatorFormRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public FormDefinition GetFormDefinitionById(int id)
        {
            try
            {
                return dbContext.FormDefinition.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new FormDefinition();
            }
        }
        public List<FormDefinition> GetFormDefinitionsAll(int domainId)
        {
            try
            {
                return dbContext.FormDefinition.Where(s => s.ComplianceForm.Domain.Id == domainId && s.IsHide == false && s.IsDraft == false).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<FormDefinition>();
            }
        }
        public ReturnJsonModel SaveForm(OperatorFormModel formModel, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var formDefinition = dbContext.FormDefinition.Find(formModel.Id);
                if (formDefinition == null)
                {
                    var complianceForm = dbContext.OperatorComplianceForms.FirstOrDefault(s => s.Domain.Id == formModel.DomainId);
                    if (complianceForm == null)
                    {
                        complianceForm = new ComplianceForms();
                        complianceForm.Domain = dbContext.Domains.Find(formModel.DomainId);
                        dbContext.OperatorComplianceForms.Add(complianceForm);
                        dbContext.Entry(complianceForm).State = EntityState.Added;
                    }
                    var user = dbContext.QbicleUser.Find(userId);
                    formDefinition = new FormDefinition
                    {
                        Title = formModel.Title,
                        Description = formModel.Description,
                        EstimatedTime = formModel.EstimatedTime,
                        ComplianceForm = complianceForm,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        IsDraft = formModel.IsDraft,
                        IsHide = false
                    };
                    foreach (var tagId in formModel.Tags)
                    {
                        var tag = dbContext.OperatorTags.Find(tagId);
                        if (tag != null)
                            formDefinition.Tags.Add(tag);
                    }
                    foreach (var item in formModel.FormElements)
                    {
                        if (!string.IsNullOrEmpty(item.Label) && (item.AllowPhotos || item.AllowDocs || item.AllowNotes || item.AllowScore))
                        {
                            var element = new FormElement
                            {
                                CreatedBy = user,
                                CreatedDate = DateTime.UtcNow,
                                Label = item.Label,
                                DisplayOrder = item.DisplayOrder,
                                AllowPhotos = item.AllowPhotos,
                                AllowDocs = item.AllowDocs,
                                AllowNotes = item.AllowNotes,
                                AllowScore = item.AllowScore
                            };
                            if (element.AllowScore)
                            {
                                var measure = dbContext.OperatorMeasures.Find(item.AssociatedMeasureId);
                                if (measure != null)
                                {
                                    element.AssociatedMeasure = measure;
                                }

                            }
                            else
                                element.AssociatedMeasure = null;
                            element.FormDefintion = formDefinition;
                            element.Type = item.Type;
                            dbContext.FormElement.Add(element);
                            dbContext.Entry(element).State = EntityState.Added;
                            formDefinition.Elements.Add(element);
                        }
                    }
                }
                else
                {
                    formDefinition.Title = formModel.Title;
                    formDefinition.Description = formModel.Description;
                    formDefinition.EstimatedTime = formModel.EstimatedTime;
                    if (formDefinition.IsDraft)
                    {
                        formDefinition.IsDraft = formModel.IsDraft;
                    }
                    formDefinition.Tags.Clear();
                    foreach (var tagId in formModel.Tags)
                    {
                        var tag = dbContext.OperatorTags.Find(tagId);
                        if (tag != null)
                            formDefinition.Tags.Add(tag);
                    }
                    var lstFormElementRemove = new List<FormElement>();
                    foreach (var item in formDefinition.Elements)
                    {
                        if (!formModel.FormElements.Any(s => s.Id == item.Id))
                            lstFormElementRemove.Add(item);
                    }
                    #region Delete FormElements
                    foreach (var item in lstFormElementRemove)
                    {
                        if (item.FormElementDatas.Any())
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_709", item.Label);
                            return returnJson;
                        }
                        else
                        {
                            dbContext.FormElement.Remove(item);
                        }
                    }
                    #endregion
                    foreach (var item in formModel.FormElements)
                    {
                        if (!string.IsNullOrEmpty(item.Label) && (item.AllowPhotos || item.AllowDocs || item.AllowNotes || item.AllowScore))
                        {
                            var fe = dbContext.FormElement.Find(item.Id);
                            if (fe != null)
                            {
                                fe.Label = item.Label;
                                fe.AllowPhotos = item.AllowPhotos;
                                fe.AllowDocs = item.AllowDocs;
                                fe.AllowNotes = item.AllowNotes;
                                fe.AllowScore = item.AllowScore;
                                if (fe.AllowScore)
                                {
                                    var measure = dbContext.OperatorMeasures.Find(item.AssociatedMeasureId);
                                    if (measure != null)
                                    {
                                        fe.AssociatedMeasure = measure;
                                    }

                                }
                                else
                                {
                                    fe.AssociatedMeasure = null;
                                }
                                if (dbContext.Entry(fe).State == EntityState.Detached)
                                    dbContext.FormElement.Attach(fe);
                                dbContext.Entry(fe).State = EntityState.Modified;
                            }
                            else
                            {
                                FormElement element = new FormElement();
                                element.CreatedBy = formModel.CurrentUser;
                                element.CreatedDate = DateTime.UtcNow;
                                element.Label = item.Label;
                                element.DisplayOrder = item.DisplayOrder;
                                element.AllowPhotos = item.AllowPhotos;
                                element.AllowDocs = item.AllowDocs;
                                element.AllowNotes = item.AllowNotes;
                                element.AllowScore = item.AllowScore;
                                if (element.AllowScore)
                                {
                                    var measure = dbContext.OperatorMeasures.Find(item.AssociatedMeasureId);
                                    if (measure != null)
                                    {
                                        element.AssociatedMeasure = measure;
                                    }

                                }
                                else
                                {
                                    fe.AssociatedMeasure = null;
                                }
                                element.FormDefintion = formDefinition;
                                element.Type = item.Type;
                                dbContext.FormElement.Add(element);
                                dbContext.Entry(element).State = EntityState.Added;
                                formDefinition.Elements.Add(element);
                            }
                        }
                    }
                }

                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, formModel.CurrentUser.Id, formModel);
            }
            return returnJson;
        }
        public ReturnJsonModel removeForm(int id)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var form = dbContext.FormDefinition.Find(id);
                if (form != null)
                {
                    form.IsHide = true;
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnJson;
        }
        public DataTablesResponse SearchForms([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, OperatorSearchFormModel filter)
        {
            try
            {
                var query = dbContext.FormDefinition.Where(t => t.ComplianceForm.Domain.Id == filter.domainId && t.IsHide == false).AsQueryable();
                int totalcount = 0;
                #region Filter
                if (!string.IsNullOrEmpty(filter.keyword))
                    query = query.Where(q => q.Title.Contains(filter.keyword) || q.Description.Contains(filter.keyword));
                if (filter.tag > 0)
                {
                    query = query.Where(q => q.Tags.Any(s => filter.tag == s.Id));
                }
                totalcount = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Description":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Description" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "EstimatedTime":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "EstimatedTime" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "IsDraft":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "IsDraft" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Title asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.Description,
                    EstimatedTime = q.EstimatedTime + "m",
                    IsDraft = !q.IsDraft ? "<span class=\"label label-lg label-success\">Active</span>" : "<span class=\"label label-lg label-primary\">Draft</span>",
                    Tags = string.Join(" ", q.Tags.Select(s => $"<span class=\"label label-lg label-primary\">{s.Name}</span>").ToList())
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public ReturnJsonModel OperatorFormSubmissions(OperatorFormSubmissionsModel model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    //TODO: Hangfire process
                    var currentUser = dbContext.QbicleUser.Find(userId);
                    var formInstance = dbContext.FormInstances.Find(model.FormInstanceId);
                    if (formInstance == null)
                    {
                        formInstance = new FormInstance
                        {
                            ComplianceTaskInstance = dbContext.OperatorTaskInstances.Find(model.TaskInstanceId),
                            ParentDefinition = dbContext.FormDefinition.Find(model.FormDefinitionId),
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            IsSubmitted = true
                        };
                        dbContext.Entry(formInstance).State = EntityState.Added;
                        dbContext.FormInstances.Add(formInstance);
                    }
                    else
                    {
                        formInstance.IsSubmitted = true;
                        if (dbContext.Entry(formInstance).State == EntityState.Detached)
                            dbContext.FormInstances.Attach(formInstance);
                        dbContext.Entry(formInstance).State = EntityState.Modified;
                    }
                    foreach (var item in model.Elements)
                    {
                        var elm = dbContext.FormElement.Find(item.FormElementId);
                        if (elm != null)
                        {
                            var elementData = new FormElementData
                            {
                                Value = item.Value,
                                ParentElement = elm,
                                ParentInstance = formInstance,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.UtcNow
                            };
                            if (elm.AllowScore)
                                elementData.Score = item.Score;
                            else
                                elementData.Score = 0;
                            if (elm.AllowPhotos && item.ImageFileResponse != null)
                            {
                                s3Rules.ProcessingMediaS3(item.ImageKey);
                                var image = new Image
                                {
                                    ParentElementData = elementData,
                                    ImageName = item.ImageName,
                                    ImageUri = item.ImageFileResponse?.UrlGuid
                                };
                                dbContext.Entry(image).State = EntityState.Added;
                                dbContext.FormImages.Add(image);
                                elementData.Attachments.Add(image);
                            }
                            if (elm.AllowDocs && item.DocFileResponse != null)
                            {
                                s3Rules.ProcessingMediaS3(item.DocKey);
                                var document = new Document
                                {
                                    ParentElementData = elementData,
                                    DocumentName = item.DocName,
                                    DocumentUri = item.DocFileResponse?.UrlGuid
                                };
                                dbContext.Entry(document).State = EntityState.Added;
                                dbContext.FormDocuments.Add(document);
                                elementData.Attachments.Add(document);
                            }
                            if (elm.AllowNotes && !string.IsNullOrEmpty(item.Note))
                            {
                                Note note = new Note();
                                note.ParentElementData = elementData;
                                note.Text = item.Note;
                                dbContext.Entry(note).State = EntityState.Added;
                                dbContext.FormNotes.Add(note);
                                elementData.Attachments.Add(note);
                            }
                            dbContext.Entry(elementData).State = EntityState.Added;
                            dbContext.FormElementData.Add(elementData);
                        }
                    }
                    #region Check if submited all form then update task is complete
                    var lstFormsSubmitted = formInstance.ComplianceTaskInstance != null && formInstance.ComplianceTaskInstance.FormInstances != null ? formInstance.ComplianceTaskInstance.FormInstances : null;
                    var totalForms = formInstance.ComplianceTaskInstance.ParentComplianceTask.OrderedForms?.Count ?? 0;
                    var totalFormsSubmitted = lstFormsSubmitted != null ? lstFormsSubmitted.Where(s => s.IsSubmitted).Count() : 0;
                    var task = formInstance.ComplianceTaskInstance.AssociatedQbicleTask;
                    if (task != null && totalForms == totalFormsSubmitted)
                    {
                        task.isComplete = true;
                        task.ActualEnd = DateTime.UtcNow;
                        if (totalFormsSubmitted == 1)
                            task.ActualStart = DateTime.UtcNow;
                    }
                    else if (task != null && totalFormsSubmitted == 1)
                    {
                        task.ActualStart = DateTime.UtcNow;
                    }

                    #endregion

                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }
    }
}
