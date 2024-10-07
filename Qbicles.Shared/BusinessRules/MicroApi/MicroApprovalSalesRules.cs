using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using static Qbicles.BusinessRules.Enums;
using static Qbicles.Models.ApprovalReq;

namespace Qbicles.BusinessRules.MicroApi
{
    public class MicroApprovalSalesRules : MicroRulesBase
    {
        public MicroApprovalSalesRules(MicroContext microContext) : base(microContext)
        {
        }

        public ApprovalOverviewModel GetSaleReviewOverview(string key)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var currentDomainId = saleModel?.Workgroup.Qbicle.Domain.Id ?? 0;

            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return new ApprovalOverviewModel { ActivityId = -1 };

            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);

            var approvalOverview = new ApprovalOverviewModel
            {
                ActivityId = saleModel.SaleApprovalProcess.Id,
                ApprovalKey = saleModel.SaleApprovalProcess?.Key ?? "",
                Reference = saleModel.Reference?.FullRef,
                Status = saleModel.SaleApprovalProcess?.RequestStatus.GetDescription(),
                StatusColor = saleModel.SaleApprovalProcess?.RequestStatus.GetRequestStatusEnumColor(),
                CreatedBy = saleModel.CreatedBy.GetFullName(),
                CreatedDate = saleModel.CreatedDate.FormatDateTimeByUser(userSetting.DateFormat, userSetting.TimeFormat),
                CreatedById = saleModel.CreatedBy.Id,
                Total = saleModel.SaleTotal.ToCurrencySymbol(currencySettings),
                Type = saleModel.DeliveryMethod.GetDescription(),
                ContactInfo = saleModel.Purchaser.ToContactInfo(),
                InQbicle = saleModel.Workgroup?.Qbicle.Name,
                DeliveryAddress = saleModel.DeliveryAddress?.ToAddress().Replace(",", Environment.NewLine),
                VoucherType = saleModel.Voucher?.Promotion.VoucherInfo.Type.GetDescription(),
                VoucherName = saleModel.Voucher?.Promotion.Name,
                VoucherDescription = saleModel.Voucher == null ? "" : $"Voucher {saleModel.Voucher?.Code} applied by customer",
            };
            return approvalOverview;
        }

        public object GetSaleReviewStatus(string key)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(key.Decrypt2Int());

            var status = saleModel.SaleApprovalProcess?.RequestStatus ?? RequestStatusEnum.Pending;

            return new { Code = status.GetId(), Name = status.GetDescription() };
        }

        public object GetSaleReviewApprovalRight(string key)
        {
            var userSetting = CurrentUser;
            var saleModel = new TraderSaleRules(dbContext).GetById(key.Decrypt2Int());

            var traderAppRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(saleModel.SaleApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, userSetting.Id);

            var userRight = "none";
            //o1
            if (!traderAppRight.IsInitiators && !traderAppRight.IsReviewer && !traderAppRight.IsApprover)
            { userRight = "none"; }//hiden approval, no edit
                                   //o2
            else if (traderAppRight.IsInitiators && !traderAppRight.IsReviewer && !traderAppRight.IsApprover)
            { userRight = "initiator"; } // readonly approval, no edit
                                         //o3
            else if (traderAppRight.IsReviewer && saleModel.SaleApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Pending)
            {
                userRight = "reviewer";
            }//If the user is an ApprovalReq.ApprovalRequestDefinition.Reviewer and the Status of the ApprovalReq is Pending, the drop down displays the items Discard, or Send to Approval.
             //o4
            else if (traderAppRight.IsApprover && saleModel.SaleApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Reviewed)
            {
                userRight = "approver";
            }
            //o4.1
            else if (traderAppRight.IsApprover && saleModel.SaleApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Pending)
            {
                userRight = "initiator";
            }
            //o5
            else if (saleModel.SaleApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Approved
                     || saleModel.SaleApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Denied
                     || saleModel.SaleApprovalProcess?.RequestStatus == ApprovalReq.RequestStatusEnum.Discarded)
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

            if (confirm && saleModel.SaleApprovalProcess != null && userRight != "none")
            {
                var approvalKey = saleModel.SaleApprovalProcess.Id.Encrypt();
                switch (userRight)
                {
                    case "initiator":
                    case "none":
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = saleModel.SaleApprovalProcess.RequestStatus, Name = saleModel.SaleApprovalProcess.RequestStatus.GetDescription() });
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
                        approvalRight.Add(new MicroApprovalModel { ApprovalKey = approvalKey, Status = saleModel.SaleApprovalProcess.RequestStatus, Name = saleModel.SaleApprovalProcess.RequestStatus.GetDescription() });
                        break;
                }
            }
            return approvalRight;
        }

        public async Task<ReturnJsonModel> MicroSaleReviewApproval(MicroApprovalModel approval)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(approval.ApprovalKey.Decrypt2Int());
            var approvalKey = saleModel.SaleApprovalProcess?.Key ?? "";
            return await new ApprovalsRules(dbContext).SetRequestStatusForApprovalRequest(approvalKey, approval.Status, CurrentUser.Id);
        }

        public List<ApprovalItemModel> GetSaleReviewItems(string key)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var currentDomainId = saleModel?.Workgroup.Qbicle.Domain.Id ?? 0;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);
            //Items
            var items = saleModel.SaleItems.Select(item => new ApprovalItemModel
            {
                Img = item.TraderItem.ImageUri.ToUriString(),
                Name = item.TraderItem.Name,
                Unit = item.Unit?.Name,
                Quantity = item.Quantity.ToDecimalPlace(currencySettings),
                UnitPrice = item.SalePricePerUnit.ToDecimalPlace(currencySettings),
                Discount = item.Discount.ToDecimalPlace(currencySettings) + "%",
                Taxes = item.StringTaxRates(currencySettings),
                Total = item.Price.ToDecimalPlace(currencySettings)
            }).ToList();
            return items;
        }

        public List<ApprovalTeamModel> GetSaleReviewTeams(string key)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(key.Decrypt2Int());
            //Team
            var approvalProcesss = new ApprovalRequestDefinition();
            if (saleModel.SaleApprovalProcess != null && saleModel.SaleApprovalProcess.ApprovalRequestDefinition != null)
            {
                approvalProcesss = saleModel.SaleApprovalProcess.ApprovalRequestDefinition;
            }
            var approvers = saleModel.Workgroup.Approvers;
            approvers.AddRange(saleModel.Workgroup.Members.Where(q => approvalProcesss.Approvers.Any(a => a.Id == q.Id)).ToList());
            approvers = approvers.Distinct().ToList();

            var reviewers = saleModel.Workgroup.Reviewers;
            reviewers.AddRange(saleModel.Workgroup.Members.Where(q => approvalProcesss.Reviewers.Any(a => a.Id == q.Id)).ToList());
            reviewers = reviewers.Distinct().ToList();

            var initiators = saleModel.Workgroup.Members.Where(q => !(approvers.Any(a => a.Id == q.Id) || reviewers.Any(r => r.Id == q.Id))).ToList();
            initiators.AddRange(saleModel.Workgroup.Members.Where(q => approvalProcesss.Initiators.Any(a => a.Id == q.Id)).ToList());
            initiators = initiators.Distinct().ToList();

            var teams = new List<ApprovalTeamModel>();
            foreach (var applicationUser in initiators)
            {
                var roleInit = "Initiator";
                roleInit += approvalProcesss.Reviewers.Any(q => q.Id == applicationUser.Id) ? ", Reviewer" : "";
                roleInit += approvalProcesss.Approvers.Any(q => q.Id == applicationUser.Id) ? ", Approver" : "";

                var detailInfo = new MicroCommunity
                {
                    DomainKey = saleModel.Workgroup?.Domain.Key ?? "",
                    DomainSubAccountCode = saleModel.Workgroup?.Domain.SubAccountCode ?? "",
                    LinkId = applicationUser.Id,
                    Image = applicationUser.ProfilePic.ToUri(FileTypeEnum.Image, "T"),
                    Name = applicationUser.GetFullName(),
                    Surname = applicationUser.Surname,
                    ComnunityEmail = applicationUser.Email,
                    QbicleId = saleModel.Workgroup?.Qbicle.Id ?? 0,
                };

                teams.Add(new ApprovalTeamModel
                {
                    Type = "Initiator",
                    Name = applicationUser.GetFullName(),
                    Img = applicationUser.ProfilePic.ToUriString(),
                    Roles = roleInit,
                    DetailInfo = detailInfo
                });
            }
            foreach (var applicationUser in reviewers)
            {
                var roleReview = "Reviewer";
                roleReview = approvalProcesss.Initiators.Any(q => q.Id == applicationUser.Id) ? "Initiator, " + roleReview : roleReview;
                roleReview += approvalProcesss.Approvers.Any(q => q.Id == applicationUser.Id) ? ", Approver" : "";
                var detailInfo = new MicroCommunity
                {
                    DomainKey = saleModel.Workgroup?.Domain.Key ?? "",
                    DomainSubAccountCode = saleModel.Workgroup?.Domain.SubAccountCode ?? "",
                    QbicleId = saleModel.Workgroup?.Qbicle.Id ?? 0,
                    LinkId = applicationUser.Id,
                    Image = applicationUser.ProfilePic.ToUri(FileTypeEnum.Image, "T"),
                    Name = applicationUser.GetFullName(),
                    Surname = applicationUser.Surname,
                    ComnunityEmail = applicationUser.Email,
                };
                teams.Add(new ApprovalTeamModel
                {
                    Type = "Reviewer",
                    Name = applicationUser.GetFullName(),
                    Img = applicationUser.ProfilePic.ToUriString(),
                    Roles = roleReview,
                    DetailInfo = detailInfo
                });
            }
            foreach (var applicationUser in approvers)
            {
                var roleApprover = "";
                roleApprover += approvalProcesss.Initiators.Any(q => q.Id == applicationUser.Id) ? "Initiator" : roleApprover;
                roleApprover += approvalProcesss.Reviewers.Any(q => q.Id == applicationUser.Id) ? string.IsNullOrEmpty(roleApprover) ? "Reviewer" : ", Reviewer" : roleApprover;
                roleApprover += (string.IsNullOrEmpty(roleApprover) ? "Approver" : ", Approver");
                var detailInfo = new MicroCommunity
                {
                    DomainKey = saleModel.Workgroup?.Domain.Key ?? "",
                    DomainSubAccountCode = saleModel.Workgroup?.Domain.SubAccountCode ?? "",
                    QbicleId = saleModel.Workgroup?.Qbicle.Id ?? 0,
                    LinkId = applicationUser.Id,
                    Image = applicationUser.ProfilePic.ToUri(FileTypeEnum.Image, "T"),
                    Name = applicationUser.GetFullName(),
                    Surname = applicationUser.Surname,
                    ComnunityEmail = applicationUser.Email,
                };
                teams.Add(new ApprovalTeamModel
                {
                    Type = "Approver",
                    Name = applicationUser.GetFullName(),
                    Img = applicationUser.ProfilePic.ToUriString(),
                    Roles = roleApprover,
                    DetailInfo = detailInfo
                });
            }

            return teams;
        }

        public List<ApprovalTimelineModel> GetSaleReviewTimelines(string key)
        {
            var rule = new TraderSaleRules(dbContext);
            var saleModel = rule.GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;

            var timelineDateDetail = rule.SaleApprovalStatusTimeline(saleModel?.Id ?? 0, userSetting.Timezone);
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

        public List<ApprovalCommentModel> GetSaleReviewComments(string key)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var comments = new List<ApprovalCommentModel>();
            string timezone = CurrentUser.Timezone; string dateFormat = CurrentUser.DateFormat; string timeFormat = CurrentUser.TimeFormat;
            var dateTimeFormat = $"{dateFormat} {timeFormat}";

            if (saleModel.SaleApprovalProcess != null)
            {
                comments = saleModel.SaleApprovalProcess.Posts.OrderByDescending(x => x.StartedDate).Select(p => new ApprovalCommentModel
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

        public List<ApprovalFileModel> GetSaleReviewFiles(string key)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var files = new List<ApprovalFileModel>();
            if (saleModel.SaleApprovalProcess != null)
            {
                if (saleModel.SaleApprovalProcess.SubActivities.Count > 0)
                {
                    foreach (var item in saleModel.SaleApprovalProcess.SubActivities.OrderByDescending(x => x.Id))
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