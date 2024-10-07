using Qbicles.BusinessRules;
using System;
using System.Reflection;

namespace Qbicles.Web.Controllers
{
    
    public class FormManagerController : BaseController
    {
        public bool InsertOrDeleteManageTaskFormsPermissionByChecked(bool isChecked, string userId, int domainId) {
            try
            {
                bool result = new FormManagerRules(dbContext).InsertOrDeleteManageTaskPermissionFormsByChecked(isChecked, userId, domainId);
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return false;
            }
        }
        public bool InsertOrDeleteQueryOrReportPermissionByChecked(bool isChecked, string userId, int domainId)
        {
            try
            {
                bool result = new FormManagerRules(dbContext).InsertOrDeleteQueryOrReportPermissionByChecked(isChecked, userId, domainId);
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return false;
            }
            
        }


        
    }
}