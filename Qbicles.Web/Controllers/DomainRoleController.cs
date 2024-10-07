using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]

    public class DomainRoleController : BaseController
    {

        [Authorize]
        public ActionResult AddRoleDomainByName(string roleDomainName)
        {
            try
            {
                return Json(new DomainRolesRules(dbContext).AddRoleDomainByName(roleDomainName, CurrentUser().Id, CurrentDomain()));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        [Authorize]
        public ActionResult RemoveUserFromRoles(int rolesId, string userId)
        {
            try
            {
                return Json(new DomainRolesRules(dbContext).RemoveUserFromRoles(rolesId, userId, CurrentDomainId()));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        [Authorize]
        public ActionResult AddUserToRoles(List<string> listUserId, int roleId)
        {
            try
            {
                return Json(new DomainRolesRules(dbContext).AddUserToRoles(listUserId, roleId, CurrentUser().Id));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        //[Authorize]
        //public ActionResult GetUsersByRoleId(int roleId)
        //{
        //    try
        //    {
        //        var refModel = new DomainRolesRules(dbContext).GetUsersByRoleId(roleId);
        //        refModel.Object2 = CurrentDomain().Users.Select(x => new
        //        {
        //            x.Id,
        //            Name = HelperClass.GetFullNameOfUser(x)
        //        });
        //        return Json(refModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.UserSettings().Id);
        //        return View("Error");
        //    }
        //}

        public ActionResult GetDomainUsersByRoleId([ModelBinder(typeof(DataTablesBinder))]IDataTablesRequest requestModel,int roleId)
        {
            return Json(new DomainRolesRules(dbContext).GetDomainUsersByRoleId(requestModel, CurrentDomainId(),roleId, CurrentUser().Id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetUsersNotExistInRole(int roleId)
        {
            var users = new DomainRolesRules(dbContext).GetUsersNotExistInRole(CurrentDomainId(), roleId);
            return Json(users.Select(s => new Select2Option { id = s.Id, text = HelperClass.GetFullNameOfUser(s) }), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public int CountAllUserByAllRoleOfDomain()
        {
            try
            {
                return new DomainRolesRules(dbContext).CountAllUserByAllRoleOfDomain(CurrentUser().Id, CurrentDomain());
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return -9;
            }
        }
    }
}