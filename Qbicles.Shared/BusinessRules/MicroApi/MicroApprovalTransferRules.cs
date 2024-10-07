using PayStackDotNetSDK.Models.Transfers;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Tweetinvi.Logic.Model;
using static Qbicles.Models.ApprovalReq;

namespace Qbicles.BusinessRules.MicroApi
{
    public class MicroApprovalTransfersRules : MicroRulesBase
    {
        public MicroApprovalTransfersRules(MicroContext microContext) : base(microContext)
        {
        }

        public ApprovalOverviewModel GetTransferReviewOverview(string key)
        {
            var transferModel = new TraderTransfersRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var currentDomainId = transferModel?.Workgroup.Qbicle.Domain.Id ?? 0;

            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return new ApprovalOverviewModel { ActivityId = -1 };

            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);

            var approvalOverview = new ApprovalOverviewModel
            {
                ActivityId = transferModel.TransferApprovalProcess.Id,
                ApprovalKey = transferModel.TransferApprovalProcess?.Key ?? "",
                Reference = transferModel.Reference?.FullRef,
                Status = transferModel.TransferApprovalProcess?.RequestStatus.GetDescription(),
                StatusColor = transferModel.TransferApprovalProcess?.RequestStatus.GetRequestStatusEnumColor(),
                CreatedById = transferModel.CreatedBy.Id,
                CreatedBy = transferModel.CreatedBy.GetFullName(),
                CreatedDate = transferModel.CreatedDate.FormatDateTimeByUser(userSetting.DateFormat, userSetting.TimeFormat),
                //Total = transferModel.transferTotal.ToCurrencySymbol(currencySettings),
                //Type = transferModel.DeliveryMethod.GetDescription(),
                //ContactName = transferModel.Purchaser.Name,
                //ContactImg = transferModel.Purchaser.AvatarUri.ToUriString(),
                //InQbicle = transferModel.Workgroup?.Qbicle.Name,
                //ContactAddress = transferModel.DeliveryAddress?.ToAddress().Replace(",", Environment.NewLine),
                //VoucherType = transferModel.Voucher?.Promotion.VoucherInfo.Type.GetDescription(),
                //VoucherName = transferModel.Voucher?.Promotion.Name,
                //VoucherDescription = transferModel.Voucher == null ? "" : $"Voucher {transferModel.Voucher?.Code} applied by customer",
            };
            return approvalOverview;
        }

        public object GetTransferReviewStatus(string key)
        {
            var transferModel = new TraderTransfersRules(dbContext).GetById(key.Decrypt2Int());

            var status = transferModel.TransferApprovalProcess?.RequestStatus ?? RequestStatusEnum.Pending;

            return new { Code = status.GetId(), Name = status.GetDescription() };
        }

        public async Task<ReturnJsonModel> MicroTransferReviewApproval(MicroApprovalModel approval)
        {
            return await new ApprovalsRules(dbContext).SetRequestStatusForApprovalRequest(approval.ApprovalKey, approval.Status, CurrentUser.Id);
        }

        public List<ApprovalItemModel> GetTransferReviewItems(string key)
        {
            var transferModel = new TraderTransfersRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var currentDomainId = transferModel?.Workgroup.Qbicle.Domain.Id ?? 0;
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomainId);
            //Items
            var items = transferModel.TransferItems.Select(item => new ApprovalItemModel
            {
                Img = item.TraderItem.ImageUri.ToUriString(),
                Name = item.TraderItem.Name,
                Unit = item.Unit?.Name,
                //Quantity = item.Quantity.ToDecimalPlace(currencySettings),
                //UnitPrice = item.transferPricePerUnit.ToDecimalPlace(currencySettings),
                //Discount = item.Discount.ToDecimalPlace(currencySettings) + "%",
                //Taxes = item.StringTaxRates(currencySettings),
                //Total = item.Price.ToDecimalPlace(currencySettings)
            }).ToList();
            return items;
        }

        public List<ApprovalTeamModel> GetTransferReviewTeams(string key)
        {
            var transferModel = new TraderTransfersRules(dbContext).GetById(key.Decrypt2Int());
            //Team
            var approvalProcesss = new ApprovalRequestDefinition();
            if (transferModel.TransferApprovalProcess != null && transferModel.TransferApprovalProcess.ApprovalRequestDefinition != null)
            {
                approvalProcesss = transferModel.TransferApprovalProcess.ApprovalRequestDefinition;
            }
            var approvers = transferModel.Workgroup.Approvers;
            approvers.AddRange(transferModel.Workgroup.Members.Where(q => approvalProcesss.Approvers.Any(a => a.Id == q.Id)).ToList());
            approvers = approvers.Distinct().ToList();

            var reviewers = transferModel.Workgroup.Reviewers;
            reviewers.AddRange(transferModel.Workgroup.Members.Where(q => approvalProcesss.Reviewers.Any(a => a.Id == q.Id)).ToList());
            reviewers = reviewers.Distinct().ToList();

            var initiators = transferModel.Workgroup.Members.Where(q => !(approvers.Any(a => a.Id == q.Id) || reviewers.Any(r => r.Id == q.Id))).ToList();
            initiators.AddRange(transferModel.Workgroup.Members.Where(q => approvalProcesss.Initiators.Any(a => a.Id == q.Id)).ToList());
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

        public List<ApprovalTimelineModel> GettransferReviewTimelines(string key)
        {
            var rule = new TraderTransfersRules(dbContext);
            var transferModel = rule.GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;

            var timelineDateDetail = rule.TransferApprovalStatusTimeline(transferModel?.Id ?? 0, userSetting.Timezone);
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

        public List<ApprovalCommentModel> GettransferReviewComments(string key)
        {
            var transferModel = new TraderTransfersRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var comments = new List<ApprovalCommentModel>();
            string timezone = CurrentUser.Timezone; string dateFormat = CurrentUser.DateFormat; string timeFormat = CurrentUser.TimeFormat;
            var dateTimeFormat = $"{dateFormat} {timeFormat}";
            if (transferModel.TransferApprovalProcess != null)
            {
                comments = transferModel.TransferApprovalProcess.Posts.OrderByDescending(x => x.StartedDate).Select(p => new ApprovalCommentModel
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

        public List<ApprovalFileModel> GettransferReviewFiles(string key)
        {
            var transferModel = new TraderTransfersRules(dbContext).GetById(key.Decrypt2Int());
            var userSetting = CurrentUser;
            var files = new List<ApprovalFileModel>();
            if (transferModel.TransferApprovalProcess != null)
            {
                if (transferModel.TransferApprovalProcess.SubActivities.Count > 0)
                {
                    foreach (var item in transferModel.TransferApprovalProcess.SubActivities.OrderByDescending(x => x.Id))
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