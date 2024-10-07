using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroB2CCommsRules : MicroRulesBase
    {
        public MicroB2CCommsRules(MicroContext microContext) : base(microContext)
        {
        }

        /// <summary>
        /// Get access righ to B2C menu
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool GetB2CMenu(string domainKey)
        {
            var userId = CurrentUser.Id;
            var domainId = int.Parse(domainKey.Decrypt());

            if (!dbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == domainId)?.IsDisplayedInB2CListings ?? false)
                return false;

            if (!CanAccessBusiness(domainId, userId) && !new B2CRules(dbContext).CheckHasAccessB2C(domainId, userId))
                return false;
            return true;
        }

        /// <summary>
        /// Get All, New, blocked community
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public object GetB2CSubnavigation(string domainKey)
        {
            var userId = CurrentUser.Id;
            var domainId = int.Parse(domainKey.Decrypt());

            var isDomainAdmin = dbContext.Domains.FirstOrDefault(d => d.Id == domainId)?.Administrators.Any(p => p.Id == userId) ?? false;

            var query = (from b2c in dbContext.B2CQbicles
                         where !b2c.IsHidden
                         && b2c.Business.Id == domainId
                         && (isDomainAdmin || b2c.Members.Any(s => s.Id == userId))
                         select b2c);//.ToList();

            var connections = 0;
            query.ToList().Where(p => p.Status != CommsStatus.Blocked).ForEach(q =>
            {
                if (q.RemovedForUsers.Count == 0)
                    connections++;
                else if (q.RemovedForUsers.Any(r => r.Id != q.Customer.Id))
                    connections++;
            });

            return new { All = connections, New = query.Where(p => p.IsNewContact == true).Count(), Blocked = query.Where(p => p.Status == CommsStatus.Blocked).Count() };
        }
        /// <summary>
        /// Get all c2c qbicle by current user
        /// </summary>
        /// <param name="keyword">Search contacts...</param>
        /// <param name="orderby">
        /// 0: Order by latest activity(Default)
        /// 1: Order by forename A-Z
        /// 2: Order by forename Z-A
        /// 3: Order by surname A-Z
        /// 4: Order by surname Z-A
        /// </param>
        /// <param name="includeBlocked">Include blocked users</param>
        /// <param name="b2cQbiceActiveId">This is the Id of b2cqbicle set selected. It default is 0 </param>
        /// <param name="typeShown">1 - show all - excluded blocked connections, 2 - show new connections, 3 - show blocked connections</param>
        /// <returns>PartialView _B2CQbiclesContent</returns>
        public List<MicroB2CCommList> B2CCommunicate(B2CSearchParameter search)
        {
            var userId = CurrentUser.Id;
            var domainId = int.Parse(search.DomainKey.Decrypt());
            if (!CanAccessBusiness(domainId, userId) && !new B2CRules(dbContext).CheckHasAccessB2C(domainId, userId))
                throw new Exception("Sorry, you are not allowed to access this page");

            var qbicles = new B2CRules(dbContext).GetB2CQbiclesData(domainId, userId, search.ContactName, search.Orderby, search.ShownType).ToList();

            var b2cQbicles = new List<MicroB2CCommList>();
            qbicles.ForEach(item =>
            {
                var connectedDate = item.StartedDate.ConvertTimeFromUtc(CurrentUser.Timezone);
                var comm = new MicroB2CCommList
                {
                    Id = item.Id,
                    Key = item.Key,
                    ProfileKey = item.Customer.Id.Encrypt(),
                    Image = item.Customer.ProfilePic.ToDocumentUri(Enums.FileTypeEnum.Image, "T"),
                    Name = item.Customer.GetFullName(),
                    ConnectedString = connectedDate.GetTimeRelative(),
                    Connected = connectedDate,
                    IsNew = item.IsNewContact.HasValue && item.IsNewContact.Value,
                    IsBlocked = item.Status == CommsStatus.Blocked,
                    IsUnseen = !item.BusinessViewed ?? false
                };
                b2cQbicles.Add(comm);
            });

            return b2cQbicles;
        }

        public ReturnJsonModel B2CCommUnblock(string key, string domainKey, CommsStatus status)
        {
            var qId = int.Parse(key.Decrypt());
            var domainId = int.Parse(domainKey.Decrypt());
            return new B2CRules(dbContext).SetStatusByBusiness(qId, domainId, CurrentUser.Id, status);
        }

        public ReturnJsonModel B2CCommRemove(string key)
        {
            var id = int.Parse(key.Decrypt());
            return new B2CRules(dbContext).RemoveB2CQbicleById(id, CurrentUser.Id);
        }


        /// <summary>
        /// We’ll handle this in the following way:
        /// Check if defaults have already been set for all of the above If defaults are present,
        /// bypass this and process the order 
        /// If one or more defaults are not set, display the dialog select workgroup
        /// </summary>
        /// <param name="tradeOrderId"></param>
        /// <returns></returns>
        public object InitCompleteB2COrder(int tradeOrderId)
        {
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(tradeOrderId);

            var location = discussion.TradeOrder.ProductMenu.Location;
            var b2cqbicle = discussion.Qbicle as B2CQbicle;

            var orderDefaultSettings = new CommerceRules(dbContext).GetB2COrderSettingDefault(b2cqbicle.Business.Id, CurrentUser.Id, location.Id);

            //if all setting saved then process order
            if (orderDefaultSettings.SaveSettings
                && orderDefaultSettings.DefaultSaleWorkGroupId > 0 && orderDefaultSettings.DefaultInvoiceWorkGroupId > 0
                && orderDefaultSettings.DefaultPaymentWorkGroupId > 0 && orderDefaultSettings.DefaultTransferWorkGroupId > 0
                && orderDefaultSettings.DefaultPaymentAccountId > 0
                )
            {
                //process order
                var complete = CompleteB2COrder(new B2BSubmitProposal
                {
                    tradeOrderId = tradeOrderId,
                    invoiceWGId = orderDefaultSettings.DefaultInvoiceWorkGroupId,
                    saleWGId = orderDefaultSettings.DefaultSaleWorkGroupId,
                    transferWGId = orderDefaultSettings.DefaultTransferWorkGroupId,
                    paymentWGId = orderDefaultSettings.DefaultPaymentWorkGroupId,
                    paymentAccId = orderDefaultSettings.DefaultPaymentAccountId,
                    SaveSettings = true
                });

                //process done
                if (complete.result)
                    return new
                    {
                        OrderProcessed = true,
                        Workgroups = new WorkgroupProposal(),
                        Message = "Order completed. In processing..."
                    };
                //process false
                return new
                {
                    OrderProcessed = false,
                    Workgroups = new WorkgroupProposal(),
                    Message = $"Error {complete.msg}"
                };

            }


            var listWorkgroup = dbContext.WorkGroups.Where(p => p.Location.Id == location.Id).OrderBy(n => n.Name);

            var SaleWorkgroups = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == TraderProcessName.TraderSaleProcessName))
                .Select(e => new Select2Model { Id = e.Id, Text = e.Name, Selected = e.Id == orderDefaultSettings.DefaultSaleWorkGroupId }).ToList();
            var InvoiceWorkgroups = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == TraderProcessName.TraderInvoiceProcessName))
                .Select(e => new Select2Model { Id = e.Id, Text = e.Name, Selected = e.Id == orderDefaultSettings.DefaultInvoiceWorkGroupId }).ToList();
            var PaymentWorkgroups = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == TraderProcessName.TraderPaymentProcessName))
                .Select(e => new Select2Model { Id = e.Id, Text = e.Name, Selected = e.Id == orderDefaultSettings.DefaultPaymentWorkGroupId }).ToList();
            var TransferWorkgroups = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == TraderProcessName.TraderTransferProcessName))
                .Select(e => new Select2Model { Id = e.Id, Text = e.Name, Selected = e.Id == orderDefaultSettings.DefaultTransferWorkGroupId }).ToList();

            var cashBankAccounts = dbContext.TraderCashAccounts.Where(d => d.Domain.Id == b2cqbicle.Business.Id)
                .Select(e => new Select2Model { Id = e.Id, Text = e.Name, Selected = e.Id == orderDefaultSettings.DefaultPaymentAccountId }).ToList();

            return new
            {
                OrderProcessed = false,
                Workgroups = new WorkgroupProposal { Sales = SaleWorkgroups, Invoices = InvoiceWorkgroups, Payments = PaymentWorkgroups, Transfers = TransferWorkgroups, CashBankAccounts = cashBankAccounts },
                Message = ""
            };

        }

        /// <summary>
        /// Submit b2c order by management
        /// </summary>
        /// <param name="proposal">
        /// tradeOrderId and workgroups
        /// </param>
        public ReturnJsonModel CompleteB2COrder(B2BSubmitProposal proposal)
        {
            proposal.CurrentUserId = CurrentUser.Id;
            //var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(proposal.tradeOrderId);
            var discussion = dbContext.B2COrderCreations.FirstOrDefault(s => s.TradeOrder.Id == proposal.tradeOrderId);
            var qbicle = discussion.Qbicle as B2CQbicle;

            var returnJson = new B2CRules(dbContext).ProcessB2COrderMicro(
                proposal.tradeOrderId, proposal.paymentAccId, proposal.saleWGId, proposal.invoiceWGId, proposal.paymentWGId, proposal.transferWGId,
                qbicle.Customer?.Id ?? discussion.TradeOrder.Customer?.Id, proposal.CurrentUserId, discussion.Id,qbicle.Id, proposal.CurrentUserId);

            if (returnJson.result)
            {
                //new B2CRules(dbContext).B2CDicussionOrderSendMessage(false, ResourcesManager._L("B2C_PROCESSED_ORDER"), discussion.Id, CurrentUser.Id, discussion.Qbicle.Id, "", true);

                new CommerceRules(dbContext).SaveB2CConfig(new B2cConfigModel
                {
                    LocationId = discussion.TradeOrder.ProductMenu.Location.Id,
                    CurrentUser = dbContext.QbicleUser.Find(CurrentUser.Id),
                    DefaultSaleWorkGroupId = proposal.saleWGId,
                    DefaultInvoiceWorkGroupId = proposal.invoiceWGId,
                    DefaultPaymentWorkGroupId = proposal.paymentWGId,
                    DefaultTransferWorkGroupId = proposal.transferWGId,
                    DefaultPaymentAccountId = proposal.paymentAccId,
                    SaveSettings = proposal.SaveSettings
                }, CurrentUser.Id);

            }
            return returnJson;
        }
    }
}
