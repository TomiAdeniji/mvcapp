using Qbicles.BusinessRules;
using Qbicles.BusinessRules.CMs;
using Qbicles.Models.Trader.CashMgt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class ShiftManagementController : BaseController
    {
        //---------------- Methods to show sub Tabs ----------------
        public ActionResult SMCashManagementTabContent(bool isSafe, bool isTill, string key)
        {
            try
            {
                
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);

                var CMsRules = new CMsRules(dbContext);
                var currentLocationId = CurrentLocationManage();
                var safe = CMsRules.GetSafeByLocation(currentLocationId);
                var tills = CMsRules.GetTillsByLocation(currentLocationId);

                if (key != null && !String.IsNullOrEmpty(key.Trim()))
                {
                    tills = tills.Where(t => t.Name.ToLower().Contains(key.ToLower())).ToList();
                    if (!safe.Name.ToLower().Contains(key.ToLower()))
                        safe = null;
                }

                ViewBag.ShowSafe = isSafe;
                ViewBag.ShowTill = isTill;
                ViewBag.Key = String.IsNullOrEmpty(key) ? "" : key;

                if (safe == null)
                    ViewBag.SafeBalance = 0;
                else
                {
                    var _safeBalance = new CMsRules(dbContext).getSafeBalance(safe.Id, CurrentUser(), timezone, CurrentDomainId());
                    ViewBag.SafeBalance = _safeBalance;
                }

                if (isSafe && isTill)
                {
                    ViewBag.Safe = safe;
                    ViewBag.Tills = tills;
                }
                else if (isSafe && !isTill)
                {
                    ViewBag.Tills = new List<Till>();
                    ViewBag.Safe = safe;
                }
                else if (isTill && !isSafe)
                {
                    ViewBag.Tills = tills;
                    ViewBag.Safe = null;
                }

                return PartialView("_SMCashManagement");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SMVoidReturnTabContent()
        {
            try
            {
                // Currently, Shift Management page on qbicles.talisman is empty
                return PartialView("_SMVoidReturn");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SMTillPaymentTabContent(int tillId, string type)
        {
            try
            {
                var currentLocationId = CurrentLocationManage();
                var currentUserId = CurrentUser().Id;
                var workGroups = dbContext.WorkGroups.Where(p =>
                    p.Location.Id == currentLocationId
                    && p.Processes.Any(m => m.Name.Equals(TraderProcessName.TraderCashManagement))
                    && p.Members.Select(n => n.Id).Contains(currentUserId)).OrderBy(n => n.Name).OrderBy(n => n.Name).ToList();

                var rules = new CMsRules(dbContext);

                var till = rules.GetTillByIdAsNoTracking(tillId);
                var safe = rules.GetSafeByLocationAsNoTracking(currentLocationId);

                if (till == null || safe == null || workGroups == null || workGroups.Count == 0)
                {
                    return View("Error");
                }

                ViewBag.Till = till;
                ViewBag.Safe = safe;
                ViewBag.WorkGroups = workGroups;
                
                if (type == "payin")
                {
                    return PartialView("_SMTillPayIn");
                }
                else
                {
                    ViewBag.LastBalance = rules.GetTillBalance(tillId);
                    return PartialView("_SMTillPayOut");
                }
            } catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult TillCheckpointAddTabContent(int tillId)
        {
            try
            {
                ViewBag.userDateTimeFormat = CurrentUser();
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                ViewBag.TimeZone = timezone;

                var associatedTill = dbContext.Tills.FirstOrDefault(t => t.Id == tillId);
                ViewBag.Till = associatedTill;

                var currentUserId = CurrentUser().Id;
                var workGroups = dbContext.WorkGroups.Where(p =>
                    p.Location.Id == associatedTill.Location.Id
                    && p.Processes.Any(m => m.Name.Equals(TraderProcessName.TraderCashManagement))
                    && p.Members.Select(n => n.Id).Contains(currentUserId)).OrderBy(n => n.Name).ToList();

                ViewBag.WorkGroups = workGroups;

                return PartialView("_CashManagementTillCheckpointAdd");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SafeCheckpointAddTabContent(int safeId)
        {
            try
            {
                ViewBag.userDateTimeFormat = CurrentUser();
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                ViewBag.TimeZone = timezone;

                var associatedSafe = dbContext.Safes.FirstOrDefault(t => t.Id == safeId);
                ViewBag.Safe = associatedSafe;

                var currentUserId = CurrentUser().Id;
                var workGroups = dbContext.WorkGroups.Where(p =>
                    p.Location.Id == associatedSafe.Location.Id
                    && p.Processes.Any(m => m.Name.Equals(TraderProcessName.TraderCashManagement))
                    && p.Members.Select(n => n.Id).Contains(currentUserId)).OrderBy(n => n.Name).ToList();

                ViewBag.WorkGroups = workGroups;

                return PartialView("_CashManagementSafeCheckpointAdd");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        //public ActionResult SMTillPayOutTabContent()
        //{
        //    try
        //    {


        //        CurrentLocationManage();
        //        return PartialView("_SMTillPayOut");
        //    } catch (Exception ex)
        //    {
        //        LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, UserSettings().Id);
        //        return View("Error");
        //    }
        //}
    }
}