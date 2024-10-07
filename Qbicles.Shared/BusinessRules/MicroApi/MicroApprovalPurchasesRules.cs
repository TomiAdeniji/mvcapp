using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using static Qbicles.Models.ApprovalReq;
using System.Collections.Generic;
using System.Linq;
using System;
using Qbicles.Models.MicroQbicleStream;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.MicroApi
{
    public class MicroApprovalPurchasesRules : MicroRulesBase
    {
        public MicroApprovalPurchasesRules(MicroContext microContext) : base(microContext)
        {
        }

        public ApprovalOverviewModel GetPurchaseReviewOverview(string key)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var currentDomainId = purchaseModel?.Workgroup.Qbicle.Domain.Id ?? 0;

            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return new ApprovalOverviewModel { ActivityId = -1 };

            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);

            var approvalOverview = new ApprovalOverviewModel
            {
                ActivityId = purchaseModel.PurchaseApprovalProcess.Id,
                ApprovalKey = purchaseModel.PurchaseApprovalProcess?.Key ?? "",
                Reference = purchaseModel.Reference?.FullRef,
                Status = purchaseModel.PurchaseApprovalProcess?.RequestStatus.GetDescription(),
                StatusColor = purchaseModel.PurchaseApprovalProcess?.RequestStatus.GetRequestStatusEnumColor(),
                CreatedById = purchaseModel.CreatedBy.Id,
                CreatedBy = purchaseModel.CreatedBy.GetFullName(),
                CreatedDate = purchaseModel.CreatedDate.FormatDateTimeByUser(userSetting.DateFormat, userSetting.TimeFormat),
                Total = purchaseModel.PurchaseTotal.ToCurrencySymbol(currencySettings),
                Type = purchaseModel.DeliveryMethod.GetDescription(),
                ContactInfo = purchaseModel.Vendor.ToContactInfo(),
                InQbicle = purchaseModel.Workgroup?.Qbicle.Name,
            };
            return approvalOverview;
        }

        public object GetPurchaseReviewStatus(string key)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(key.Decrypt2Int());

            var status = purchaseModel.PurchaseApprovalProcess?.RequestStatus ?? RequestStatusEnum.Pending;

            return new { Code = status.GetId(), Name = status.GetDescription() };
        }

        public object GetPurchaseReviewApprovalRight(string key)
        {
            var userSetting = CurrentUser;
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(key.Decrypt2Int());

            var traderAppRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(purchaseModel.PurchaseApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, userSetting.Id);

            var userRight = "none";
            //o1
            if (!traderAppRight.IsInitiators && !traderAppRight.IsReviewer && !traderAppRight.IsApprover)
            { userRight = "none"; }//hiden approval, no edit
                                   //o2
            else if (traderAppRight.IsInitiators && !traderAppRight.IsReviewer && !traderAppRight.IsApprover)
            { userRight = "initiator"; } // readonly approval, no edit
                                         //o3
            else if (traderAppRight.IsReviewer && purchaseModel.PurchaseApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Pending)
            {
                userRight = "reviewer";
            }//If the user is an ApprovalReq.ApprovalRequestDefinition.Reviewer and the Status of the ApprovalReq is Pending, the drop down displays the items Discard, or Send to Approval.
             //o4
            else if (traderAppRight.IsApprover && purchaseModel.PurchaseApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Reviewed)
            {
                userRight = "approver";
            }
            //o4.1
            else if (traderAppRight.IsApprover && purchaseModel.PurchaseApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Pending)
            {
                userRight = "initiator";
            }
            //o5
            else if (purchaseModel.PurchaseApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Approved
                     || purchaseModel.PurchaseApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Denied
                     || purchaseModel.PurchaseApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Discarded)
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

            if (confirm && purchaseModel.PurchaseApprovalProcess != null && userRight != "none")
            {
                var approvalKey = purchaseModel.PurchaseApprovalProcess.Id.Encrypt();
                switch (userRight)
                {
                    case "initiator":
                    case "none":
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = purchaseModel.PurchaseApprovalProcess.RequestStatus, Name = purchaseModel.PurchaseApprovalProcess.RequestStatus.GetDescription() });
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
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = purchaseModel.PurchaseApprovalProcess.RequestStatus, Name = purchaseModel.PurchaseApprovalProcess.RequestStatus.GetDescription() });
                        break;
                }
            }
            return approvalRight;
        }

        public async Task<ReturnJsonModel> MicroPurchaseReviewApproval(MicroApprovalModel approval)
        {
            return await new ApprovalsRules(dbContext).SetRequestStatusForApprovalRequest(approval.ApprovalKey, approval.Status, CurrentUser.Id);
        }

        public List<ApprovalItemModel> GetPurchaseReviewItems(string key)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var currentDomainId = purchaseModel?.Workgroup.Qbicle.Domain.Id ?? 0;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);
            //Items
            var items = purchaseModel.PurchaseItems.Select(item => new ApprovalItemModel
            {
                Img = item.TraderItem.ImageUri.ToUriString(),
                Name = item.TraderItem.Name,
                Unit = item.Unit?.Name,
                Quantity = item.Quantity.ToDecimalPlace(currencySettings),
                UnitPrice = item.CostPerUnit.ToDecimalPlace(currencySettings),
                Discount = item.Discount.ToDecimalPlace(currencySettings) + "%",
                Taxes = item.StringTaxRates(currencySettings),
                Total = item.Cost.ToDecimalPlace(currencySettings)
            }).ToList();
            return items;
        }

        public List<ApprovalTeamModel> GetPurchaseReviewTeams(string key)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(key.Decrypt2Int());
            //Team
            var approvalProcesss = new ApprovalRequestDefinition();
            if (purchaseModel.PurchaseApprovalProcess != null && purchaseModel.PurchaseApprovalProcess.ApprovalRequestDefinition != null)
            {
                approvalProcesss = purchaseModel.PurchaseApprovalProcess.ApprovalRequestDefinition;
            }
            var approvers = purchaseModel.Workgroup.Approvers;
            approvers.AddRange(purchaseModel.Workgroup.Members.Where(q => approvalProcesss.Approvers.Any(a => a.Id == q.Id)).ToList());
            approvers = approvers.Distinct().ToList();

            var reviewers = purchaseModel.Workgroup.Reviewers;
            reviewers.AddRange(purchaseModel.Workgroup.Members.Where(q => approvalProcesss.Reviewers.Any(a => a.Id == q.Id)).ToList());
            reviewers = reviewers.Distinct().ToList();

            var initiators = purchaseModel.Workgroup.Members.Where(q => !(approvers.Any(a => a.Id == q.Id) || reviewers.Any(r => r.Id == q.Id))).ToList();
            initiators.AddRange(purchaseModel.Workgroup.Members.Where(q => approvalProcesss.Initiators.Any(a => a.Id == q.Id)).ToList());
            initiators = initiators.Distinct().ToList();

            var teams = new List<ApprovalTeamModel>();
            foreach (var applicationUser in initiators)
            {
                var roleInit = "Initiator";
                roleInit += approvalProcesss.Reviewers.Any(q => q.Id == applicationUser.Id) ? ", Reviewer" : "";
                roleInit += approvalProcesss.Approvers.Any(q => q.Id == applicationUser.Id) ? ", Approver" : "";
                teams.Add(new ApprovalTeamModel
                {
                    Type = "Initiator",
                    Name = applicationUser.GetFullName(),
                    Img = applicationUser.ProfilePic.ToUriString(),
                    Roles = roleInit
                });
            }
            foreach (var applicationUser in reviewers)
            {
                var roleReview = "Reviewer";
                roleReview = approvalProcesss.Initiators.Any(q => q.Id == applicationUser.Id) ? "Initiator, " + roleReview : roleReview;
                roleReview += approvalProcesss.Approvers.Any(q => q.Id == applicationUser.Id) ? ", Approver" : "";
                teams.Add(new ApprovalTeamModel
                {
                    Type = "Reviewer",
                    Name = applicationUser.GetFullName(),
                    Img = applicationUser.ProfilePic.ToUriString(),
                    Roles = roleReview
                });
            }
            foreach (var applicationUser in approvers)
            {
                var roleApprover = "";
                roleApprover += approvalProcesss.Initiators.Any(q => q.Id == applicationUser.Id) ? "Initiator" : roleApprover;
                roleApprover += approvalProcesss.Reviewers.Any(q => q.Id == applicationUser.Id) ? string.IsNullOrEmpty(roleApprover) ? "Reviewer" : ", Reviewer" : roleApprover;
                roleApprover += (string.IsNullOrEmpty(roleApprover) ? "Approver" : ", Approver");
                teams.Add(new ApprovalTeamModel
                {
                    Type = "Approver",
                    Name = applicationUser.GetFullName(),
                    Img = applicationUser.ProfilePic.ToUriString(),
                    Roles = roleApprover
                });
            }

            return teams;
        }

        public List<ApprovalTimelineModel> GetPurchaseReviewTimelines(string key)
        {
            var rule = new TraderPurchaseRules(dbContext);
            var purchaseModel = rule.GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;

            var timelineDateDetail = rule.PurchaseApprovalStatusTimeline(purchaseModel?.Id ?? 0, userSetting.Timezone);
            var timelineDate = timelineDateDetail?.Select(d => d.LogDate.Date).Distinct().OrderByDescending(e => e).ToList();

            var timelines = new List<ApprovalTimelineModel>();
            foreach (var date in timelineDate)
            {
                var dateStart = date.ConvertTimeFromUtc(userSetting.Timezone).DatetimeToOrdinal();
                var dateStr = date.Date == DateTime.UtcNow.Date ? "Today" : dateStart;
                var timeline = new ApprovalTimelineModel { Date = dateStr };

                foreach (var tl in timelineDateDetail.Where(d => d.LogDate.Date == date.Date))
                {
                    timeline.Timelines.Add(new ApprovalTimelineDetailModel
                    {
                        Img = tl.UserAvatar.ToUriString(),
                        Description = tl.Status,
                        Time = tl.Time
                    });
                }
                timelines.Add(timeline);
            }

            return timelines;
        }

        public List<ApprovalCommentModel> GetPurchaseReviewComments(string key)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var comments = new List<ApprovalCommentModel>();
            string timezone = CurrentUser.Timezone; string dateFormat = CurrentUser.DateFormat; string timeFormat = CurrentUser.TimeFormat;
            var dateTimeFormat = $"{dateFormat} {timeFormat}";
            if (purchaseModel.PurchaseApprovalProcess != null)
            {
                comments = purchaseModel.PurchaseApprovalProcess.Posts.OrderByDescending(x => x.StartedDate).Select(p => new ApprovalCommentModel
                {
                    Id = p.Id,
                    CreatedBy = p.CreatedBy.GetFullName(CurrentUser.Id),
                    CreatedDate = p.StartedDate.Date == DateTime.UtcNow.Date ? "Today, " + p.StartedDate.ConvertTimeFromUtc(timezone).ToString(timeFormat) : p.StartedDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                    DateCreated = p.StartedDate.Date == DateTime.UtcNow.Date ? "Today" : p.StartedDate.ConvertTimeFromUtc(timezone).ToShortDateString(),
                    Image = p.CreatedBy.ProfilePic.ToUri(),
                    Message = p.Message
                }).ToList();
            }
            return comments;
        }

        public List<ApprovalFileModel> GetPurchaseReviewFiles(string key)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var files = new List<ApprovalFileModel>();
            if (purchaseModel.PurchaseApprovalProcess != null)
            {
                if (purchaseModel.PurchaseApprovalProcess.SubActivities.Count > 0)
                {
                    foreach (var item in purchaseModel.PurchaseApprovalProcess.SubActivities.OrderByDescending(x => x.Id))
                    {
                        var media = (QbicleMedia)item;
                        var createdDate = media.StartedDate.Date == DateTime.UtcNow.Date ? "Today, " + media.StartedDate.ConvertTimeFromUtc(userSetting.Timezone).ToString("hh:mmtt").ToLower() : media.StartedDate.ConvertTimeFromUtc(userSetting.Timezone).ToString(userSetting.DateFormat + " hh:mmtt").ToLower();
                        var mediaLastupdate = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).First() ?? null;
                        var lastUpdateFile = mediaLastupdate != null ? (mediaLastupdate.UploadedDate.Date == DateTime.UtcNow.Date ? "Today, " + mediaLastupdate.UploadedDate.ConvertTimeFromUtc(userSetting.Timezone).ToString("hh:mmtt").ToLower() : mediaLastupdate.UploadedDate.ConvertTimeFromUtc(userSetting.Timezone).ToString(userSetting.DateFormat + " hh:mmtt").ToLower()) : createdDate;
                        files.Add(new ApprovalFileModel
                        {
                            Id = media.Id,
                            Key = media.Key,
                            Name = media.Name,
                            Description = media.Description,
                            CreatedBy = media.StartedBy.GetFullName(userSetting.Id),
                            CreatedDate = createdDate,
                            CreatedByImage = media.StartedBy.ProfilePic.ToUri(),
                            LastUpdate = $"{media.FileType?.Type} | Update {lastUpdateFile}",
                            FileType = new MicroFileType
                            {
                                Extension = media.FileType?.Extension,
                                Type = media.FileType?.Type
                            },
                            MediaUri = mediaLastupdate?.Uri.ToUri((media.FileType?.Type ?? "").GetFileTypeEnum()),
                            Topic = media.Topic?.Name,
                            Folder = media.MediaFolder?.Name
                        });
                    }
                }
            }

            return files;
        }

        public ApprovalCommentModel AddCommentToApproval(MicroCommentApprovalModel comment)
        {
            var approvalId = comment.ApprovalKey.Decrypt2Int();

            var appRules = new ApprovalsRules(dbContext);
            appRules.AddPostToApproval(false, comment.Message, approvalId, CurrentUser.Id);
            var userSetting = CurrentUser;
            return new ApprovalCommentModel
            {
                CreatedBy = userSetting.GetFullName(userSetting.Id),
                CreatedDate = DateTime.UtcNow.ConvertTimeFromUtc(userSetting.Timezone).ToString(userSetting.DateFormat + ", hh:mmtt").ToLower(),
                Image = userSetting.ProfilePic.ToUri(),
                Message = comment.Message
            };
        }
    }
}