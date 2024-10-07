using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroRulesBase
    {
        public ApplicationDbContext dbContext;
        public ApplicationUser CurrentUser;

        public MicroRulesBase(MicroContext microContext)
        {
            dbContext = microContext.Context;
            if (!string.IsNullOrEmpty(microContext.UserId))
                CurrentUser = MicroCurrentUser(microContext.UserId);
        }

        private ApplicationUser MicroCurrentUser(string userId)
        {
            return dbContext.QbicleUser.Find(userId);
            //var cacheKey = $"current-user-{userId}";
            //return QbiclesCache.Instance.Get(cacheKey, () =>
            //{
            //    return dbContext.QbicleUser.Find(userId);
            //});
        }

        public void InvalidUserCache(string userId)
        {
            if (MemoryCache.Default.Contains(userId))
            {
                MemoryCache.Default.Remove(userId);
            }
        }

        public bool CanAccessBusiness(int domainId, string userId)
        {
            var isDomainAdmin = dbContext.Domains.FirstOrDefault(d => d.Id == domainId)?.Administrators.Any(p => p.Id == userId) ?? false;
            var rightApps = new AppRightRules(dbContext).UserRoleRights(userId, domainId);

            if (!isDomainAdmin && !rightApps.Any(r => r == RightPermissions.QbiclesBusinessAccess))
                return false;
            return true;
        }

        public object GettransferReviewApprovalRight(string key)
        {
            var userSetting = CurrentUser;
            var transferModel = new TraderTransfersRules(dbContext).GetById(key.Decrypt2Int());

            var traderAppRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(transferModel.TransferApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, userSetting.Id);

            var userRight = "none";
            //o1
            if (!traderAppRight.IsInitiators && !traderAppRight.IsReviewer && !traderAppRight.IsApprover)
            { userRight = "none"; }//hiden approval, no edit
                                   //o2
            else if (traderAppRight.IsInitiators && !traderAppRight.IsReviewer && !traderAppRight.IsApprover)
            { userRight = "initiator"; } // readonly approval, no edit
                                         //o3
            else if (traderAppRight.IsReviewer && transferModel.TransferApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Pending)
            {
                userRight = "reviewer";
            }//If the user is an ApprovalReq.ApprovalRequestDefinition.Reviewer and the Status of the ApprovalReq is Pending, the drop down displays the items Discard, or Send to Approval.
             //o4
            else if (traderAppRight.IsApprover && transferModel.TransferApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Reviewed)
            {
                userRight = "approver";
            }
            //o4.1
            else if (traderAppRight.IsApprover && transferModel.TransferApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Pending)
            {
                userRight = "initiator";
            }
            //o5
            else if (transferModel.TransferApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Approved
                     || transferModel.TransferApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Denied
                     || transferModel.TransferApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Discarded)
            {
                userRight = "completed";
            }

            var confirm = true;
            if (userRight == "initiator" || userRight == "completed")
            {
                confirm = false;
            }

            if (userRight == "none")
            {
                confirm = false;
            }

            var approvalRight = new List<MicroApprovalModel>();

            if (confirm && transferModel.TransferApprovalProcess != null && userRight != "none")
            {
                var approvalKey = transferModel.TransferApprovalProcess.Id.Encrypt();
                switch (userRight)
                {
                    case "initiator":
                    case "none":
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = transferModel.TransferApprovalProcess.RequestStatus, Name = transferModel.TransferApprovalProcess.RequestStatus.GetDescription() });
                        break;

                    case "reviewer":
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = ApprovalReq.RequestStatusEnum.Reviewed, Name = "Send to Approval" });
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = ApprovalReq.RequestStatusEnum.Discarded, Name = "Discard" });
                        break;

                    case "approver":
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = ApprovalReq.RequestStatusEnum.Approved, Name = "Approve" });
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = ApprovalReq.RequestStatusEnum.Pending, Name = "Send back to Review" });
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = ApprovalReq.RequestStatusEnum.Denied, Name = "Deny" });
                        break;

                    case "completed":
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = transferModel.TransferApprovalProcess.RequestStatus, Name = transferModel.TransferApprovalProcess.RequestStatus.GetDescription() });
                        break;
                }
            }
            return approvalRight;
        }
    }
}