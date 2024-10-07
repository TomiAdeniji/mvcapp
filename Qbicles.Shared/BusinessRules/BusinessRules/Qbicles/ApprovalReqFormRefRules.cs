using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class ApprovalReqFormRefRules
    {
        ApplicationDbContext _db;

        public ApprovalReqFormRefRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();


        public bool UpdateFormBuilder(TaskFormDefinitionRefCustom form, string userId, int appId)
        {
            try
            {

                var appDefinitionRef = GetApprovalReqFormRefByAppId(appId, form.Id);
                if (appDefinitionRef == null)
                {
                    var appReq = new ApprovalsRules(_db).GetApprovalById(appId);
                    var formDefinition = new FormDefinitionRules(_db).GetFormDefinitionById(form.Id);
                    var appFormRef = new ApprovalReqFormRef
                    {
                        ApprovalReq = appReq,
                        FormBuilder = form.FormBuilder,
                        FormData = form.FormData,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = DbContext.QbicleUser.Find(userId),
                        FormDefinition = formDefinition
                    };

                    _db.ApprovalReqFormRef.Add(appFormRef);
                    _db.SaveChanges();
                    return true;
                }
                else
                {

                    if (appDefinitionRef.ApprovalReq.ClosedBy == null)
                    {
                        appDefinitionRef.FormBuilder = form.FormBuilder;
                        appDefinitionRef.FormData = form.FormData;
                        appDefinitionRef.LastUpdateDate = DateTime.UtcNow;
                        appDefinitionRef.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                        var fixlazyloading = appDefinitionRef.FormDefinition;

                        if (DbContext.Entry(appDefinitionRef).State == EntityState.Detached)
                            DbContext.ApprovalReqFormRef.Attach(appDefinitionRef);
                        DbContext.Entry(appDefinitionRef).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }
        private ApprovalReqFormRef GetApprovalReqFormRefByAppId(int appId, int formDefinitionId)
        {
            try
            {
                return DbContext.ApprovalReqFormRef.FirstOrDefault(x => x.ApprovalReq.Id == appId && x.FormDefinition.Id == formDefinitionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;

            }
        }


        public List<ApprovalReqFormRef> GetApprovalDefinitionRefsByApprovalId(int approvalId)
        {
            try
            {
                return DbContext.ApprovalReqFormRef.Where(x => x.ApprovalReq.Id == approvalId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<ApprovalReqFormRef>();
            }
        }
    }
}
