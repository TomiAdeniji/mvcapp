using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Web.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class BkJournalEntriesController : BaseController
    {
        [HttpGet]
        public ActionResult AddNewTableRow(int index = 1, string rowNewId = "")
        {
            try
            {
                var bkTransaction = new BKTransaction
                {
                    PostedDate = DateTime.UtcNow
                };
                ViewBag.index = index;
                ViewBag.AddNew = true;
                ViewBag.RowNewId = rowNewId;
                ViewBag.dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
                return PartialView("_TransactionRowPartial", bkTransaction);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                throw;
            }
        }


        [HttpGet]
        public ActionResult AttachmentViewPartial(int index = 1, string rowNewId = "")
        {
            var bkTransaction = new BKTransaction();
            ViewBag.index = index;
            ViewBag.RowNewId = rowNewId;
            ViewBag.dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            ViewBag.taxRates = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId()).ToList();
            ViewBag.index = index;
            return PartialView("_AttachmentViewPartial", bkTransaction);
        }


        [HttpGet]
        public ActionResult GetByAccountId(int id)
        {
            var acc = new BKCoANodesRule(dbContext).GetAccountById(id);
            return Json(new { acc.Id, acc.Name }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetDimensionByAccountId(int id)
        {
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTransaction(BKTransaction bkTransaction)
        {
            try
            {
                if (bkTransaction != null)
                    return PartialView("_TransactionRowPartial", bkTransaction);
                else bkTransaction = new BKTransaction();
                ViewBag.dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
                ViewBag.taxRates = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId()).ToList();
                ViewBag.SelectedTaxRate = "isSelectedTaxRate";

                bkTransaction.Account = new BKCoANodesRule(dbContext).GetAccountById(bkTransaction.Account.Id);
                if (bkTransaction.Debit.HasValue)
                {
                    bkTransaction.Credit = 0;
                }
                else if (bkTransaction.Credit.HasValue)
                {
                    bkTransaction.Debit = 0;
                }

                if (bkTransaction.Dimensions != null && bkTransaction.Dimensions.Count > 0)
                {
                    var lstIdDimension = bkTransaction.Dimensions.Select(q => q.Id).ToList();
                    bkTransaction.Dimensions = dbContext.TransactionDimensions
                        .Where(q => lstIdDimension.Contains(q.Id))
                        .ToList();
                }

                ViewBag.index = bkTransaction.Id;
                // new row 2
                if (bkTransaction.Id < 0)
                {
                    ViewBag.index = bkTransaction.Id * -1;
                    bkTransaction.Id = 0;
                    bkTransaction.Account.Name =
                        bkTransaction.Account.Name;
                    var tran = bkTransaction.Debit;
                    bkTransaction.Debit = bkTransaction.Credit;
                    bkTransaction.Credit = tran;
                }
                else
                {
                    bkTransaction.Id = 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, bkTransaction);
            }

            return PartialView("_TransactionRowPartial", bkTransaction);
        }

        [HttpGet]
        public ActionResult GetTaxRate(int id)
        {
            var taxRate = new TaxRateRules(dbContext).GetById(id);
            if (taxRate == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(
                new
                {
                    taxRate.Id,
                    taxRate.Rate,
                    taxRate.Name,
                    Account = new
                    {
                        taxRate.AssociatedAccount.Id,
                        Name = taxRate.AssociatedAccount.Name + "-VAT" + taxRate.Rate + " %"
                    }
                }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetTaxRates(SearchAdvance search)
        {
            var taxRates = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId())
                .Where(q => search.q != "" && q.Name.Contains(search.q)).ToList();
            return Json(taxRates.Select(q => new { q.Id, q.Rate, q.Name }).ToList(), JsonRequestBehavior.AllowGet);
        }




        public ActionResult GetListCoANodeParentId(int id)
        {
            var refModel = new BookkeepingRules(dbContext).GetListCoANodeParentId(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowTransactionComment(int id)
        {
            var comments = new JournalEntryRules(dbContext).ShowTransactionComment(id);
            ViewBag.TransactionId = id;
            return PartialView("_TransactionComments", comments);
        }

        public ActionResult ShowTransactionAttachments(int id)
        {
            var attachments = new JournalEntryRules(dbContext).ShowTransactionAttachments(id);
            ViewBag.TransactionId = id;
            return PartialView("_TransactionAttachments", attachments);
        }



        //// Load more older Activity

        public ActionResult LoadMoreJournalEntryMedias(int activityId, int size)
        {
            var endOfOlder = false;
            var medias = new MediasRules(dbContext).GetJournalEntryMedias(activityId);
            medias = medias.OrderByDescending(d => d.TimeLineDate).Skip(size).Take(HelperClass.activitiesPageSize)
                .ToList();
            if (medias.Count < size) endOfOlder = true;
            ViewBag.EndOfOlder = endOfOlder;

            return PartialView("_JournalEntryMedias", medias);

        }

        public ActionResult LoadMoreJournalEntryPosts(int activityId, int size)
        {
            try
            {
                var endOfOlder = false;
                var posts = new PostsRules(dbContext).GetJournalEntryPosts(activityId);
                posts = posts.OrderByDescending(d => d.TimeLineDate).Skip(size).Take(HelperClass.activitiesPageSize)
                    .ToList();
                if (posts.Count < size) endOfOlder = true;
                ViewBag.EndOfOlder = endOfOlder;
                if (posts.Count > 0)
                    return PartialView("_ActivityPosts", posts);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }

        [HttpPost]
        //public ActionResult SaveJournalEntry(JournalEntry jEntry)
        //{
        //    var refModel = new JournalEntryRules(dbContext).SaveJournalEntry(jEntry, CurrentUser(), CurrentDomainId());
        //    return Json(refModel, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult SaveJournalEntry(JournalEntry jEntry, List<BKTransactionCustom> bKTransactions)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (jEntry.Id == 0)
                    refModel = new JournalEntryRules(dbContext).CreateJournalEntry(jEntry, bKTransactions, CurrentUser(), CurrentDomainId());
                else
                    refModel = new JournalEntryRules(dbContext).UpdateJournalEntry(jEntry, bKTransactions, CurrentUser());

            }
            catch (Exception ex)
            {
                refModel.actionVal = 0;
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveJournalEntryTemplate(JournalEntry jEntry, JournalEntryTemplate template)
        {
            var refModel = new JournalEntryRules(dbContext).SaveJournalEntryTemplate(jEntry, template, CurrentDomain());
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveBKTransactionMedia(int id, BKTransaction bKTransactionAssociatedFiles, List<MediaModel> bkTransactionAttachments)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (id == 0)
                {
                    refModel.msg = "BK Transaction Id is required!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                var transaction = new BKTransactionRules(dbContext).GetById(id);

                var mediaRules = new MediasRules(dbContext);

                if (bKTransactionAssociatedFiles?.AssociatedFiles?.Count > 0)
                    mediaRules.UpdateOnReviewAttachmentsBkTransaction(bKTransactionAssociatedFiles, transaction);

                if (bkTransactionAttachments?.Count > 0)
                    mediaRules.SaveOnReviewNewAttachmentsBKTransaction(transaction, bkTransactionAttachments, CurrentUser().Id);

                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.msg = ex.Message;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
    }
}