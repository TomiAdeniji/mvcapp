using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using static Qbicles.Models.ApprovalReq;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderContactController : BaseController
    {
        [HttpPost]
        public ActionResult SaveGroup(TraderContactGroup group)
        {
            var refModel = new ReturnJsonModel { result = true };

            try
            {
                TraderContactRules rule = new TraderContactRules(dbContext);
                group.Domain = CurrentDomain();
                if (rule.TraderContactGroupNameCheck(group))
                {
                    refModel.actionVal = 3;
                    refModel.msg = "\"" + group.Name + "\": already exists";
                    refModel.msgId = group.Id.ToString();
                    refModel.msgName = group.Name;
                    refModel.result = true;

                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }

                refModel.actionVal = 1;
                var g = rule.SaveTraderContactGroup(group, CurrentUser().Id);
                refModel.msg = "<option value='" + g.Name + "'>" + g.Name + "</option>";
                refModel.msgName = "<option value='" + g.Id + "'>" + g.Name + "</option>";
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckExistsReferenceNumber(int id, int number)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                refModel.result = new TraderContactRules(dbContext).CheckExistReferenceNumber(id, number, CurrentDomainId());

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }
            return Json(refModel.result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetNewContactRef()
        {
            var contactRef = new TraderContactRules(dbContext).CreateNewTraderContactRef(CurrentDomainId());
            return Json(new { contactRef.Id, contactRef.Reference, contactRef.ReferenceNumber }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveTraderContact(TraderContact contact, HttpPostedFileBase contactAvatar,
            string mediaObjectKey,
            int groupId, int accountId, int addressId, string CountryName, int contactReferenceId, TraderAddress address, int workgroupId, TraderContactStatusEnum contactStatus
        )
        {
            var refModel = new ReturnJsonModel { result = true };
            if (contact.Id > 0)
            {
                refModel.actionVal = 2;
            }
            else
            {
                refModel.actionVal = 1;
            }

            try
            {
                var currentUserId = CurrentUser().Id;
                if (contact?.Workgroup?.Location != null)
                {
                    if (!dbContext.WorkGroups.Any(p => p.Members.Any(i => i.Id == currentUserId) && p.Processes.Any(i => i.Name == TraderProcessesConst.Contact) && p.Location.Id == contact.Workgroup.Location.Id))
                    {
                        refModel.result = false;
                        refModel.msg = "You do not have sufficient privileges to add a contact";
                        refModel.msgId = "2";
                        return Json(refModel, JsonRequestBehavior.AllowGet);
                    }
                }

                //Validate contact name max length: forename(35) + surname(35) + 1 for space character
                var contactNameLength = contact.Name.Length;
                if (contactNameLength > 71)
                {
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_261");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }

                TraderContactRules rule = new TraderContactRules(dbContext);
                contact.ContactRef = new TraderContactRef { Id = contactReferenceId };
                contact.AvatarUri = mediaObjectKey;


                rule.SaveTraderContact(contact, groupId, accountId, addressId, CountryName, address, currentUserId, workgroupId, contactStatus);
                refModel.msg = $"<option value='{contact.Id}'>{contact.Name}</option>";
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.actionVal = 3;
                refModel.result = false;
                refModel.msg = ex.Message;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetContactById(int id)
        {
            var contact = new TraderContactRules(dbContext).GetTraderContactById(id);
            return Json(contact, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TraderContactNameCheck(TraderContact contact)
        {
            try
            {
                var refModel = new ReturnJsonModel();
                TraderContactRules rule = new TraderContactRules(dbContext);
                refModel.result = rule.TraderContactNameCheck(contact, CurrentDomainId());

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult DeleteContact(int id)
        {
            TraderContactRules rules = new TraderContactRules(dbContext);
            return Json(rules.DeleteContact(id) ? "OK" : "Fail", JsonRequestBehavior.AllowGet);
        }
        public ActionResult ContactReview(int id, bool isReload = false)
        {
            try
            {
                var contactModel = new TraderContactRules(dbContext).GetById(id) ?? new TraderContact();
                var currentDomainId = contactModel.Workgroup?.Qbicle.Domain.Id ?? 0;
                ValidateCurrentDomain(contactModel.Workgroup?.Qbicle.Domain ?? CurrentDomain(), contactModel.Workgroup?.Qbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");


               
                var isUpdate = userRoleRights.Any(e => e == RightPermissions.TraderContactUpdate);
                var isBusiness = userRoleRights.Any(e => e == RightPermissions.QbiclesBusinessAccess);

                var traderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(contactModel.ContactApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, CurrentUser().Id);
                if ( isBusiness)
                    {
                        traderApprovalRight.IsReviewer = true;
                        traderApprovalRight.IsApprover = true;
                        isUpdate = true;
                    }
                ViewBag.IsUpdate = isUpdate;
                ViewBag.TraderApprovalRight = traderApprovalRight;

                var dr = new DomainRules(dbContext);
                var user = dr.GetUser(CurrentUser().Id);

                var lstDomain = user.Domains.BusinessMapping(user.Timezone);
                var lstQbicNotNull = lstDomain?.Where(p => p.Qbicles.Count > 0).ToList();
                if (lstQbicNotNull != null && lstQbicNotNull.Any())
                {
                    lstDomain = lstDomain.Where(p => lstQbicNotNull.All(a => a.Id != p.Id)).ToList();
                    lstQbicNotNull = lstQbicNotNull.OrderBy(o => o.Qbicles.OrderBy(a => a.LastUpdated).FirstOrDefault().LastUpdated).ToList();
                    lstDomain.AddRange(lstQbicNotNull);
                }
                ViewBag.Domains = lstDomain;

                SetCurrentApprovalIdCookies(contactModel.ContactApprovalProcess?.Id ?? 0);

                int domainId = CurrentDomainId();
                ViewBag.ContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(domainId);
                ViewBag.Countries = new CountriesRules().GetAllCountries();

                ViewBag.WorkGroups = CurrentDomain().Workgroups.Where(q =>
                    q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderContactProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();

                List<Qbicles.Models.Bookkeeping.BKGroup> bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
                ViewBag.TreeView = bkGroup.Any() ? BKConfigurationRules.GenTreeView(bkGroup.ToList()) : "";
                var timeline = new ApprovalsRules(dbContext).GetApprovalStatusTimeline(contactModel.ContactApprovalProcess, user.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                if (isReload)
                {
                    return PartialView("_ContactReviewContent", contactModel);
                }
                else
                {
                    return View(contactModel);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ContactMaster(string key)
        {
            try
            {
                var id = int.Parse(key.Decrypt());

                var user = new DomainRules(dbContext).GetUser(CurrentUser().Id);
                var currentUserId = CurrentUser().Id;

                List<string> userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                TraderContact contactModel = new TraderContactRules(dbContext).GetById(id);
                var isUpdate = true;
                if (
                      (
                          contactModel.ContactGroup.saleChannelGroup == SalesChannelContactGroup.Trader
                                  &&
                          userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess)
                      )
                  ||
                      (
                          contactModel.ContactGroup.saleChannelGroup == SalesChannelContactGroup.Trader
                                  &&
                          userRoleRights.All(e => e != RightPermissions.TraderContactUpdate)

                      )
                  )
                {
                    isUpdate = false;
                }
                ViewBag.BalanceContact = new TraderContactRules(dbContext).GetBalanceContact(id);
                ViewBag.PurchaseInvBalance = new TraderContactRules(dbContext).GetPurchaseInvoiceBalanceByTraderContact(id);
                ViewBag.SaleInvBalance = new TraderContactRules(dbContext).GetSaleInvoiceBalanceByTraderContact(id);
                ViewBag.IsUpdate = isUpdate;
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                ViewBag.GoBackPage = CurrentGoBackPage();
                //this.SetCookieGoBackPage();

                int domainId = CurrentDomainId();
                
                ViewBag.ContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(domainId);
                ViewBag.Countries = new CountriesRules().GetAllCountries();

                ViewBag.WorkGroups = CurrentDomain().Workgroups.Where(q =>
                    q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderContactProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();

                List<Qbicles.Models.Bookkeeping.BKGroup> bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
                ViewBag.TreeView = bkGroup.Any() ? BKConfigurationRules.GenTreeView(bkGroup.ToList()) : "";

                var contactWorkGroup = contactModel.Workgroup;
                var isWorkGroupMem = false;
                var hasBusinessUserRole = false;
                var hasCreditNoteProcess = true;
                if (contactWorkGroup != null && contactWorkGroup.Domain != null)
                {
                    isWorkGroupMem = contactModel.Workgroup.Members.Any(p => p.Id == currentUserId);
                    hasBusinessUserRole = dbContext.DomainRole.Any(p => p.Domain.Id == contactWorkGroup.Domain.Id && p.Users.Any(u => u.Id == currentUserId) && p.Name == FixedRoles.QbiclesBusinessRole);
                    hasCreditNoteProcess = dbContext.WorkGroups.Any(p => p.Domain.Id == contactWorkGroup.Domain.Id
                                                                && p.Location.Id == contactWorkGroup.Location.Id
                                                                && p.Members.Any(i => i.Id == currentUserId)
                                                                && p.Processes.Any(i => i.Name == TraderProcessesConst.CreditNotes));
                }
                ViewBag.IsWorkGroupMem = isWorkGroupMem;
                ViewBag.HasBusinessUserRole = hasBusinessUserRole;
                ViewBag.hasCreditNoteProcess = hasCreditNoteProcess;

                return View(contactModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult PaymentsByContact([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int id)
        {
            return Json(new TraderCashBankRules(dbContext).GetByContact(id, requestModel, CurrentUser(), CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult InvoiceByContact([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int id, string type)
        {
            return Json(new TraderInvoicesRules(dbContext).GetInvoicesByContact(id, CurrentUser().Timezone, requestModel, type, CurrentUser().DateTimeFormat, CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AllocationDetail(int id)
        {
            var allocation = new TraderContactRules(dbContext).GetAllocationById(id);
            return View(allocation);
        }
        public ActionResult CreditNoteDetail(int id)
        {
            var creditnote = new TraderContactRules(dbContext).GetCreditNoteById(id);
            var timeline = new TraderContactRules(dbContext).CreditNoteApprovalStatusTimeline(creditnote.Id, CurrentUser().Timezone).OrderByDescending(q => q.LogDate).ToList();
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;
            return View(creditnote);
        }
        public ActionResult CreditNoteReview(int id)
        {
            try
            {
                var dr = new DomainRules(dbContext);
                var user = dr.GetUser(CurrentUser().Id);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var creditModel = new TraderContactRules(dbContext).GetCreditNoteById(id) ?? new CreditNote();

                ViewBag.IsUpdate = userRoleRights.Any(e => e == RightPermissions.TraderContactUpdate);
                if (creditModel.WorkGroup != null)
                {
                    ValidateCurrentDomain(creditModel.WorkGroup?.Qbicle.Domain ?? CurrentDomain(), creditModel.WorkGroup?.Qbicle.Id ?? 0);
                }

                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(creditModel.ApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, user.Id);



                var lstDomain = user.Domains.BusinessMapping(user.Timezone);
                var lstQbicNotNull = lstDomain?.Where(p => p.Qbicles.Count > 0).ToList();
                if (lstQbicNotNull != null && lstQbicNotNull.Any())
                {
                    lstDomain = lstDomain.Where(p => lstQbicNotNull.All(a => a.Id != p.Id)).ToList();
                    lstQbicNotNull = lstQbicNotNull.OrderBy(o => o.Qbicles.OrderBy(a => a.LastUpdated).FirstOrDefault().LastUpdated).ToList();
                    lstDomain.AddRange(lstQbicNotNull);
                }
                ViewBag.Domains = lstDomain;

                SetCurrentApprovalIdCookies(creditModel.ApprovalProcess?.Id ?? 0);

                int domainId = CurrentDomainId();
                ViewBag.ContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupByDomain(domainId, SalesChannelContactGroup.Trader);
                ViewBag.Countries = new CountriesRules().GetAllCountries();

                ViewBag.WorkGroups = CurrentDomain().Workgroups.Where(q =>
                    q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderContactProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();

                List<Qbicles.Models.Bookkeeping.BKGroup> bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
                ViewBag.TreeView = bkGroup.Any() ? BKConfigurationRules.GenTreeView(bkGroup.ToList()) : "";


                var timeline = new TraderContactRules(dbContext).CreditNoteApprovalStatusTimeline(creditModel.Id, user.Timezone).OrderByDescending(q => q.LogDate).ToList();
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;


                return View(creditModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult SaveAllocation(BalanceAllocation allocation)
        {
            ReturnJsonModel resultModel = new ReturnJsonModel();
            try
            {
                allocation.Reference.Domain = CurrentDomain();
                resultModel = new TraderContactRules(dbContext).SaveAllocation(allocation, CurrentUser().Id);
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            }

            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult DeleteAllocation(int id)
        {
            ReturnJsonModel resultModel = new ReturnJsonModel() { actionVal = 1, result = true };
            resultModel.result = new TraderContactRules(dbContext).DeleteAllocation(id);
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetNewAllocation(int contactid)
        {
            var allocation = new BalanceAllocation();
            allocation.Reference = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Allocation);
            allocation.ContactBalanceBefore = new TraderContactRules(dbContext).GetBalanceContact(contactid);
            allocation.Reference.Domain = null;
            return Json(allocation, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveCreditDebitNote(CreditNote creditNote)
        {
            ReturnJsonModel resultModel = new ReturnJsonModel();
            try
            {

                creditNote.Reference.Domain = CurrentDomain();
                creditNote.FinalisedDate = DateTime.UtcNow;
                resultModel = new TraderContactRules(dbContext).SaveCreditDebit(creditNote, CurrentUser().Id);
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                resultModel.actionVal = 3;
                resultModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            }

            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AllocationList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int contactid)
        {
            return Json(new TraderContactRules(dbContext).GetBalanceAllocations(contactid, requestModel, CurrentUser().Timezone, CurrentUser().DateTimeFormat, CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreditNoteList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int contactid, string type = "Credit")
        {
            return Json(new TraderContactRules(dbContext).GetCreditNotes(contactid, type, requestModel, CurrentUser().Timezone, CurrentUser().DateTimeFormat, CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeContactLogo(int id, string mediaObjectKey)
        {
            var resultModel = new ReturnJsonModel();
            try
            {

                new TraderContactRules(dbContext).UpdateContactAvatar(id, mediaObjectKey);
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            }

            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowContactContent()
        {
            List<TraderContact> traderContacts = new TraderContactRules(dbContext).GetTraderContactsByDomain(CurrentDomainId(), SalesChannelContactGroup.Trader);
            int currentLocationId = CurrentLocationManage();
            ViewBag.WorkGroups = CurrentDomain().Workgroups.Where(q =>
                 //q.Location.Id == currentLocationId &&
                 q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderContactProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();


            ViewBag.ContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(CurrentDomainId());

            return PartialView("_TraderContactContent", traderContacts);
        }

        [HttpPost]
        public ActionResult LoadContactContent(
            [Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderby,
            int[] groupId, int contactGroupId, string search, int draw, int start, int length)
        {
            var totalRecord = 0;
            List<TraderContactModel> lstResult = new TraderContactRules(dbContext).GetListTraderContacts(column, orderby, CurrentLocationManage(), groupId, contactGroupId, search, CurrentDomain(), CurrentUser().Id, start, length, ref totalRecord);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }


        // Credit note
        public ActionResult AddEditCreditDebitNote(int id = 0, TraderReferenceType type = TraderReferenceType.CreditNote, int contactId = 0)
        {
            TraderReference traderReferenceForCredit = new TraderReference();
            if (type == TraderReferenceType.Allocation)
                traderReferenceForCredit = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Allocation);
            else if (type == TraderReferenceType.CreditNote)
                traderReferenceForCredit = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.CreditNote);
            else if (type == TraderReferenceType.DebitNote)
                traderReferenceForCredit = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.DebitNote);
            var workGroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(CurrentDomainId()).Where(q =>
                            q.Location.Id == CurrentLocationManage()
                            && q.Members.Select(s => s.Id).Contains(CurrentUser().Id)
                            && q.Processes.Any(p => p.Name == TraderProcessName.CreditNotes)).OrderBy(n => n.Name).ToList();
            TraderContact contact = new TraderContactRules(dbContext).GetById(contactId);
            ViewBag.Type = type;
            ViewBag.Contact = contact;
            ViewBag.WorkGroups = workGroups;
            var creditDebit = new TraderContactRules(dbContext).GetCreditNoteById(id);
            if (id == 0)
                creditDebit.Reference = traderReferenceForCredit;
            return PartialView("_AddEditCreditDebitNote", creditDebit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="action">1-Add or Edit, 2-Approve</param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult CheckExistedApprovedContact(TraderContact contact)
        {
            var currentDomainId = CurrentDomainId();
            var checkrs = new TraderContactRules(dbContext).ValidateOnApproveContact(contact, currentDomainId);
            return Json(checkrs, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchTop10ContactContent(string keyword)
        {
            List<TraderContact> traderContacts = new TraderContactRules(dbContext).GetSearchTop10TraderContact(CurrentDomainId(), keyword);
            return PartialView("~/Views/Commerce/TradingSettings/_Top10TradingContactItems.cshtml", traderContacts);
        }
        public ActionResult TradingContactInfoContent(int contactId)
        {
            TraderContact traderContact = new TraderContactRules(dbContext).GetById(contactId);
            return PartialView("~/Views/Commerce/TradingSettings/_ContactInfo.cshtml", traderContact);
        }


        public ActionResult GetContactGroups()
        {
            var contactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(CurrentDomainId());
            var simpleDataContactGroups = contactGroups.Select(e => new {
               id =  e.Id,
               text = e.Name 
            }).ToList();
            return Json(simpleDataContactGroups, JsonRequestBehavior.AllowGet);
        }




















        public ActionResult FindSaleCredit()
        {
            return PartialView("_TraderContactMasterFindSale");
        }
        public ActionResult FindSaleCreditServerSide([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int contactId, string keyword, string dateRange)
        {
            var sales = new TraderContactRules(dbContext).FindSaleCreditServerSide(requestModel, keyword.ToLower(), contactId, dateRange, CurrentUser(), CurrentDomainId());
            if(sales != null)
            return Json(sales, JsonRequestBehavior.AllowGet);
            return Json(new DataTablesResponse(requestModel.Draw, new List<object>(), 0, 0), JsonRequestBehavior.AllowGet);
        }


        public ActionResult FindPurchaseCredit()
        {
            return PartialView("_TraderContactMasterFindPurchase");
        }        
        public ActionResult FindPurchaseCreditServerSide([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int contactId, string keyword, string dateRange)
        {
            var sales = new TraderContactRules(dbContext).FindPurchaseCreditServerSide(requestModel, keyword.ToLower(), contactId, dateRange, CurrentUser(), CurrentDomainId());
            if (sales != null)
                return Json(sales, JsonRequestBehavior.AllowGet);
            return Json(new DataTablesResponse(requestModel.Draw, new List<object>(), 0, 0), JsonRequestBehavior.AllowGet);
        }



        public ActionResult FindInvoiceContact(string type = "CreditNote")
        {
            //List<InvoiceContact> invoices = new TraderContactRules(dbContext).InvoicesByContact(contactid);
            //if (invoices.Any())
            //{
            //    if (type == "CreditNote")
            //    {
            //        invoices = invoices.Where(q => q.Status == "sale").ToList();
            //    }
            //    else if (type == "DebitNote")
            //    {
            //        invoices = invoices.Where(q => q.Status != "sale").ToList();
            //    }
            //}
            ViewBag.Mode = type;
            //ViewBag.Key = reffull;
            return PartialView("_TraderContactMasterFindInvoice");
        }

        public ActionResult FindInvoiceServerSide([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int contactId, string keyword, string dateRange, string type = "CreditNote")
        {
            var sales = new TraderContactRules(dbContext).FindInvoiceServerSide(requestModel, keyword.ToLower(), contactId, dateRange, type, CurrentUser(), CurrentDomainId());
            if (sales != null)
                return Json(sales, JsonRequestBehavior.AllowGet);
            return Json(new DataTablesResponse(requestModel.Draw, new List<object>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

    }
}