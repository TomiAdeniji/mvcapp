using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader;
using Qbicles.Models.UserInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using static Qbicles.BusinessRules.Enums;

namespace Qbicles.BusinessRules.Micro.Extensions
{
    public static class MappingExtensions
    {
        #region Domain & Qbicle & Activity
        public static List<BaseModel> ToMicro(this List<DomainRole> roles)
        {
            var microQbicles = new List<BaseModel>();
            roles.ForEach(q =>
            {
                microQbicles.Add(new BaseModel
                {
                    Id = q.Id,
                    Name = q.Name
                });
            });
            return microQbicles;
        }

        public static List<MicroQbicle> ToMicro(this List<Qbicle> qbicles, string timezone = "")
        {
            var microQbicles = new List<MicroQbicle>();
            var today = DateTime.UtcNow.Date;

            qbicles.ForEach(q =>
            {
                var createdDate = timezone == "" ? q.StartedDate : q.StartedDate.ConvertTimeFromUtc(timezone);
                microQbicles.Add(new MicroQbicle
                {
                    Id = q.Id,
                    ClosedBy = q.ClosedBy?.GetFullName(),
                    ClosedDate = timezone == "" ? q.ClosedDate : q.ClosedDate.ConvertTimeFromUtc(timezone),
                    Description = q.Description,
                    IsHidden = q.IsHidden,
                    IsUsingApprovals = q.IsUsingApprovals,
                    LastUpdated = timezone == "" ? q.LastUpdated : q.LastUpdated.ConvertTimeFromUtc(timezone),
                    LogoUri = q.LogoUri.ToUri(),
                    LogoKey = q.LogoUri,
                    Manager = q.Manager?.GetFullName(),
                    Name = q.Name,
                    OwnedBy = q.OwnedBy?.GetFullName(),
                    StartedBy = q.StartedBy?.GetFullName(),
                    StartedDate = createdDate == today ? "Today, " + createdDate.ToString("hh:mmtt") : createdDate.ToString("dd MMM yyyy, hh:mmtt"),
                    IsClosed = q.ClosedDate != null,
                    DomainId = q.Domain.Id
                });
            });
            return microQbicles;
        }

        public static MicroQbicle ToMicro(this Qbicle qbicle, string timezone)
        {
            var today = DateTime.UtcNow.Date;
            var createdDate = qbicle.StartedDate.ConvertTimeFromUtc(timezone);

            var microQbicle = new MicroQbicle
            {
                Id = qbicle.Id,
                DomainId = qbicle.Domain.Id,
                ClosedBy = qbicle.ClosedBy?.GetFullName(),
                ClosedDate = qbicle.ClosedDate.ConvertTimeFromUtc(timezone),
                Description = qbicle.Description,
                IsHidden = qbicle.IsHidden,
                IsUsingApprovals = qbicle.IsUsingApprovals,
                LastUpdated = qbicle.LastUpdated.ConvertTimeFromUtc(timezone),
                LogoUri = qbicle.LogoUri.ToUri(),
                LogoKey = qbicle.LogoUri,
                Name = qbicle.Name,
                OwnedBy = qbicle.OwnedBy?.GetFullName(),
                StartedBy = qbicle.StartedBy?.GetFullName(),
                StartedDate = createdDate == today ? "Today, " + createdDate.ToString("hh:mmtt") : createdDate.ToString("dd MMM yyyy, hh:mmtt"),
                Managers = new List<MicroUser>(),
                Members = new List<MicroUser>(),
                Manager = qbicle.Manager.GetFullName()
            };

            qbicle.Members.ForEach(m =>
            {
                microQbicle.Managers.Add(new MicroUser
                {
                    Id = m.Id,
                    FullName = m.GetFullName(),
                    Email = m.Email,
                    ProfilePic = m.ProfilePic.ToUri(),
                    IsQbicleManager = m.Id == qbicle.Manager?.Id
                });
            });

            qbicle.Domain.Users.ForEach(m =>
            {
                microQbicle.Members.Add(new MicroUser
                {
                    Id = m.Id,
                    FullName = m.GetFullName(),
                    Email = m.Email,
                    ProfilePic = m.ProfilePic.ToUri(),
                    IsQbicleMemeber = qbicle.Members.Any(e => e.Id == m.Id)
                });
            });

            return microQbicle;
        }

        public static List<MicroDomain> ToMicro(this List<QbicleDomain> domains, ApplicationUser user)
        {
            var microDomains = new List<MicroDomain>();
            var userId = user.Id; var timezone=user.Timezone;var dateFormat = user.DateFormat;
            domains.ForEach(d =>
            {
                var domain = new MicroDomain
                {
                    Id = d.Id,
                    DomainKey = d.Key,
                    Name = d.Name,
                    LogoUri = d.LogoUri.ToUri(),
                    Status = d.Status,
                    CreatedBy = d.CreatedBy.GetFullName(),
                    CreatedDate = d.CreatedDate.ConvertTimeFromUtc(timezone).ToOrdinalString(dateFormat),
                    OwnedBy = d.OwnedBy.GetFullName()
                };

                if (d.CreatedBy?.Id == userId)
                {
                    domain.DomainRoleType = DomainRoleEnum.MyDomain.GetId();
                    domain.DomainRole = DomainRoleEnum.MyDomain.GetDescription();
                }
                else if (d.Administrators?.Any(x => x.Id == userId) ?? false)
                {
                    domain.DomainRoleType = DomainRoleEnum.Admin.GetId();
                    domain.DomainRole = DomainRoleEnum.Admin.GetDescription();
                }
                else if (d.QbicleManagers?.Any(x => x.Id == userId) ?? false)
                {
                    domain.DomainRoleType = DomainRoleEnum.Manager.GetId();
                    domain.DomainRole = DomainRoleEnum.Manager.GetDescription();
                }
                microDomains.Add(domain);
            });
            return microDomains;
        }

        public static List<MicroUser> ToMicro(this List<ApplicationUser> users, string userId)
        {
            var microUsers = new List<MicroUser>();
            users.ForEach(u =>
            {
                var user = new MicroUser
                {
                    Id = u.Id,
                    Email = u.Email,
                    Surname = u.Surname,
                    Forename = u.Forename,
                    ProfilePic = u.ProfilePic.ToUri(),
                    FullName = u.GetFullName(userId),
                    DateFormat = u.DateFormat,
                    TimeFormat = u.TimeFormat,
                    Profile = u.Profile,
                    Timezone = u.Timezone,
                    IsOwner = u.Id == userId
                };

                microUsers.Add(user);
            });
            return microUsers;
        }

        public static List<MicroTopic> ToMicro(this List<TopicCustom> topics, string timezone)
        {
            var microTopics = new List<MicroTopic>();
            topics.ForEach(q =>
            {
                microTopics.Add(new MicroTopic
                {
                    Id = q.Id,
                    Name = q.Name,
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(timezone),
                    Creator = q.Creator.GetFullName(),
                    Summary = q.Summary,
                    Instances = q.Instances,
                    Apps = q.isTrader ? "Trader" : "",
                    CanDelete = q.Name != HelperClass.GeneralName
                });
            });
            return microTopics;
        }

        public static List<MicroMediaFolder> ToMicro(this List<MediaFolder> folders, string timezone)
        {
            var microFolders = new List<MicroMediaFolder>();
            folders.ForEach(q =>
            {
                microFolders.Add(new MicroMediaFolder
                {
                    Id = q.Id,
                    Name = q.Name,
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(timezone),
                    CreatedBy = q.CreatedBy.GetFullName()
                });
            });
            return microFolders;
        }

        public static MicroMediaFolder ToMicro(this List<QbicleMedia> medias, string timezone)
        {
            if (medias.Count == 0)
                return new MicroMediaFolder();
            var folder = medias.FirstOrDefault().MediaFolder;
            var microFolder = new MicroMediaFolder
            {
                Id = folder.Id,
                Name = folder.Name,
                CreatedBy = folder.CreatedBy.GetFullName(),
                CreatedDate = folder.CreatedDate
            };


            medias.ForEach(q =>
            {
                var version = q.VersionedFiles.OrderByDescending(f => f.UploadedDate).FirstOrDefault();

                microFolder.Medias.Add(new MicroMedia
                {
                    Id = q.Id,
                    Name = q.Name,
                    Description = q.Description,
                    Topic = q.Topic?.Name,
                    TopicId = q.Topic?.Id ?? 0,
                    CreatedDate = q.StartedDate.ConvertTimeFromUtc(timezone),
                    ActivityType = q.ActivityType.GetDescription(),
                    App = q.App.GetDescription(),
                    CreatedBy = q.StartedBy.GetFullName(),
                    FileType = new MicroFileType { Extension = q.FileType?.Extension, Type = q.FileType?.Type },
                    IsPublic = q.IsPublic,
                    MediaFolderId = q.MediaFolder.Id,
                    QbicleId = q.Qbicle?.Id ?? 0,
                    VersionedFile = new MicroVersionedFile
                    {
                        Id = version.Id,
                        FileSize = version.FileSize,
                        FileType = new MicroFileType { Extension = version.FileType?.Extension, Type = version.FileType?.Type, Size = version?.FileSize },
                        IsDeleted = version.IsDeleted,
                        MediaId = version.Media.Id,
                        UploadedBy = version.UploadedBy.GetFullName(),
                        UploadedDate = version.UploadedDate.ConvertTimeFromUtc(timezone),
                        Uri = version.Uri.ToUri()
                    }
                });
            });
            return microFolder;
        }

        public static List<MicroInvitation> ToMicro(this List<InvitationCustom> domainIvitations, string timezone)
        {
            var microInvitations = new List<MicroInvitation>();
            domainIvitations.ForEach(d =>
            {
                microInvitations.Add(new MicroInvitation
                {
                    DomainId = d.DomainId,
                    Id = d.Id,
                    CreatedDate = d.CreatedDate.ConvertTimeFromUtc(timezone),
                    DomainLogoUri = d.DomainPic.ToUri(),
                    DomainName = d.DomainName,
                    InviteBy = d.InviteBy,
                    Status = d.Status,
                    UserId = d.UserId
                });
            });
            return microInvitations;
        }

        //public static List<MicroActivitesStream> ToMicroSreams(this List<dynamic> activities, string currentTimeZone, string dateFormat, string currentUserId)
        //{
        //    var streams = new List<MicroActivitesStream>();
        //    foreach (var item in activities)
        //    {
        //        streams.Add(MicroStreamRules.GenerateMicroActivity(item, currentUserId, dateFormat, currentTimeZone));
        //    }

        //    //var microStream = new MicroQbicleStream
        //    //{
        //    //    MicroActivities = new List<MicroDatesQbicleStream>(),
        //    //    TotalCount = qbicleStreams.TotalCount
        //    //};

        //    //var today = DateTime.UtcNow;

        //    //qbicleStreams.Dates.ForEach(date =>
        //    //{
        //    //    var itemDate = date.Date.ConvertTimeFromUtc(currentTimeZone);
        //    //    var stream = new MicroDatesQbicleStream
        //    //    {
        //    //        Date = itemDate.Date == today.Date ? "Today" : itemDate.DatetimeToOrdinal(),
        //    //        Activities = new List<MicroActivitesStream>()
        //    //    };

        //    //    foreach (var item in date.Activities)
        //    //    {
        //    //        stream.Activities.Add(MicroStreamRules.GenerateActivity(item, date.Date, qbicleStreams.Pinneds, currentUserId, dateFormat, currentTimeZone, qbicleStreams.IsFilterDiscussionOrder));
        //    //    }

        //    //    microStream.MicroActivities.Add(stream);
        //    //});


        //    return streams;
        //}


        public static MicroQbicleStream ToMicro(this QbicleStreamModel qbicleStreams, string currentTimeZone, string dateFormat, string currentUserId)
        {
            var microStream = new MicroQbicleStream
            {
                MicroActivities = new List<MicroDatesQbicleStream>(),
                TotalCount = qbicleStreams.TotalCount
            };

            var today = DateTime.UtcNow;

            qbicleStreams.Dates.ForEach(date =>
            {
                var itemDate = date.Date.ConvertTimeFromUtc(currentTimeZone);
                var stream = new MicroDatesQbicleStream
                {
                    Date = itemDate.Date == today.Date ? "Today" : itemDate.DatetimeToOrdinal(),
                    Activities = new List<MicroActivitesStream>()
                };

                foreach (var item in date.Activities)
                {
                    stream.Activities.Add(MicroStreamRules.GenerateActivity(item, date.Date, qbicleStreams.Pinneds, currentUserId, dateFormat, currentTimeZone, qbicleStreams.IsFilterDiscussionOrder));
                }

                microStream.MicroActivities.Add(stream);
            });


            return microStream;
        }


        public static MicroDatesQbicleStream ToMicro(this List<QbicleActivity> qbicleStreams, List<int> pinneds, string dateFormat)
        {
            var microStream = new MicroQbicleStream
            {
                MicroActivities = new List<MicroDatesQbicleStream>(),
                TotalCount = qbicleStreams.Count
            };


            var stream = new MicroDatesQbicleStream
            {
                Activities = new List<MicroActivitesStream>()
            };

            qbicleStreams.ForEach(item =>
            {

                var streamActivity = new MicroActivitesStream();


                switch (item.ActivityType)
                {

                    case QbicleActivity.ActivityTypeEnum.ApprovalRequest:
                    case QbicleActivity.ActivityTypeEnum.ApprovalRequestApp:
                        var qbApproval = (ApprovalReq)item;
                        var manuJob = qbApproval.Manufacturingjobs.FirstOrDefault();
                        if (qbApproval.Transfer.Any())
                        {
                            streamActivity.Id = qbApproval.Transfer.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.Transfer.GetId();
                            streamActivity.StreamName = StreamType.Transfer.GetDescription();
                            streamActivity.ActivityHref = "/TraderTransfers/TransferReview?key=" + qbApproval.Transfer.FirstOrDefault().Key;
                        }
                        else if (qbApproval.Sale.Any())
                        {
                            streamActivity.Id = qbApproval.Sale.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.Sale.GetId();
                            streamActivity.StreamName = StreamType.Sale.GetDescription();
                            streamActivity.ActivityHref = "/TraderSales/SaleReview?key=" + qbApproval.Sale.FirstOrDefault().Key;
                        }
                        else if (qbApproval.Purchase.Any())
                        {
                            streamActivity.Id = qbApproval.Purchase.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.Purchase.GetId();
                            streamActivity.StreamName = StreamType.Purchase.GetDescription();
                            streamActivity.ActivityHref = "/TraderPurchases/PurchaseReview?id=" + qbApproval.Purchase.FirstOrDefault().Id;
                        }
                        else if (qbApproval.TraderContact.Any())
                        {
                            streamActivity.Id = qbApproval.TraderContact.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.TraderContact.GetId();
                            streamActivity.StreamName = StreamType.TraderContact.GetDescription();
                            streamActivity.ActivityHref = "/TraderContact/ContactReview?id=" + qbApproval.TraderContact.FirstOrDefault().Id;
                        }
                        else if (qbApproval.Invoice.Any())
                        {
                            streamActivity.Id = qbApproval.Invoice.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.Invoice.GetId();
                            streamActivity.StreamName = StreamType.Invoice.GetDescription();
                            streamActivity.ActivityHref = "/TraderInvoices/InvoiceReview?key=" + qbApproval.Invoice.FirstOrDefault().Key;
                        }
                        else if (qbApproval.Payments.Any())
                        {
                            streamActivity.Id = qbApproval.Payments.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.Payments.GetId();
                            streamActivity.StreamName = StreamType.Payments.GetDescription();
                            streamActivity.ActivityHref = "/TraderPayments/PaymentReview?id=" + qbApproval.Payments.FirstOrDefault().Id; ;
                        }
                        else if (qbApproval.SpotCounts.Any())
                        {
                            streamActivity.Id = qbApproval.SpotCounts.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.SpotCounts.GetId();
                            streamActivity.StreamName = StreamType.SpotCounts.GetDescription();
                            streamActivity.ActivityHref = "/TraderSpotCount/SpotCountReview?id=" + qbApproval.SpotCounts.FirstOrDefault().Id;
                        }
                        else if (qbApproval.WasteReports.Any())
                        {
                            streamActivity.Id = qbApproval.WasteReports.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.WasteReports.GetId();
                            streamActivity.StreamName = StreamType.WasteReports.GetDescription();
                            streamActivity.ActivityHref = "/TraderWasteReport/WasteReportReview?id=" + qbApproval.WasteReports.FirstOrDefault().Id;
                        }
                        else if (qbApproval.Manufacturingjobs.Any())
                        {
                            streamActivity.Id = qbApproval.Manufacturingjobs.FirstOrDefault().Id;
                            streamActivity.StreamId = StreamType.Manufacturingjobs.GetId();
                            streamActivity.StreamName = StreamType.Manufacturingjobs.GetDescription();
                            streamActivity.ActivityHref = "/Manufacturing/ManuJobReview?id=" + qbApproval.Manufacturingjobs.FirstOrDefault().Id;
                        }
                        switch (qbApproval.RequestStatus)
                        {
                            case ApprovalReq.RequestStatusEnum.Pending:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Pending.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Pending.GetId();
                                break;
                            case ApprovalReq.RequestStatusEnum.Reviewed:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Reviewed.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Reviewed.GetId();
                                break;
                            case ApprovalReq.RequestStatusEnum.Approved:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Approved.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Approved.GetId();
                                break;
                            case ApprovalReq.RequestStatusEnum.Denied:
                            case ApprovalReq.RequestStatusEnum.Discarded:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Discarded.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Discarded.GetId();
                                break;
                        }

                        var source = "";
                        streamActivity.Pinned = (pinneds != null && pinneds.Any(e => e == qbApproval.Id));
                        streamActivity.AppTitle = "Process " + (qbApproval.Manufacturingjobs.Count() <= 0 ? qbApproval.Name : ("Compound Item Assembly: " + (manuJob?.Reference?.FullRef ?? "") + " " + (manuJob?.Product?.Name ?? "")));
                        if (qbApproval.JournalEntries.Any() || qbApproval.BKAccounts.Any())
                            source = "Bookkeeping,";
                        else if (qbApproval.EmailPostApproval.Any() || qbApproval.CampaigPostApproval.Any())
                            source = "Sales & Marketing,";
                        else
                            source = "Trader,";
                        streamActivity.AppTitleMsg = source + qbApproval.StartedDate.ToString(dateFormat + " hh:mmtt").ToLower();
                        break;
                    case QbicleActivity.ActivityTypeEnum.TaskActivity:
                        var qbTask = (QbicleTask)item;
                        var taskStatus = 0;
                        if (!qbTask.isComplete && qbTask.ActualStart == null && qbTask.ProgrammedEnd >= DateTime.UtcNow)
                        { taskStatus = 0; }
                        else if (!qbTask.isComplete && qbTask.ActualStart != null && qbTask.ProgrammedEnd >= DateTime.UtcNow)
                        { taskStatus = 1; }
                        else if (!qbTask.isComplete && qbTask.ProgrammedEnd < DateTime.UtcNow)
                        { taskStatus = 2; }
                        else if (qbTask.isComplete)
                        { taskStatus = 3; }
                        switch (taskStatus)
                        {
                            case 0:
                                streamActivity.StatusName = "Pending";
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Pending.GetId();
                                break;
                            case 1:
                                streamActivity.StatusName = "Started";
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Reviewed.GetId();
                                break;
                            case 2:
                                streamActivity.StatusName = "Overdue";
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Approved.GetId();
                                break;
                            case 3:
                                streamActivity.StatusName = "Complete";
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Approved.GetId();
                                break;
                        }
                        streamActivity.Id = qbTask.Id;
                        streamActivity.StreamId = StreamType.Task.GetId();
                        streamActivity.StreamName = StreamType.Task.GetDescription();
                        streamActivity.Pinned = (pinneds != null && pinneds.Any(e => e == qbTask.Id));
                        streamActivity.AppTitle = "Task " + qbTask.Id + "-" + qbTask.Name;
                        streamActivity.AppTitleMsg = qbTask.ProgrammedEnd.Value.ToString(dateFormat + " hh:mmtt").ToLower();
                        break;

                    case QbicleActivity.ActivityTypeEnum.EventActivity:
                        var qbEvent = (QbicleEvent)item;
                        if (qbEvent.isComplete || qbEvent.End < DateTime.UtcNow)
                        {
                            streamActivity.StatusName = "Complete";
                            streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Approved.GetId();
                        }
                        streamActivity.Id = qbEvent.Id;
                        streamActivity.StreamId = StreamType.Event.GetId();
                        streamActivity.StreamName = StreamType.Event.GetDescription();
                        streamActivity.Pinned = (pinneds != null && pinneds.Any(e => e == qbEvent.Id));
                        streamActivity.AppTitle = "Event " + qbEvent.Id + "-" + qbEvent.Name;
                        streamActivity.AppTitleMsg = qbEvent.StartedDate.ToString(dateFormat + " hh:mmtt").ToLower();
                        break;
                    case QbicleActivity.ActivityTypeEnum.DiscussionActivity:
                        var qbDiscussion = (QbicleDiscussion)item;
                        var isActive = qbDiscussion.ExpiryDate != null && qbDiscussion.ExpiryDate <= DateTime.UtcNow;
                        if (isActive)
                        {
                            streamActivity.StatusName = "Complete";
                            streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Approved.GetId();
                        }
                        else
                        {
                            streamActivity.StatusName = "Started";
                            streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Reviewed.GetId();
                        }
                        streamActivity.Id = qbDiscussion.Id;
                        streamActivity.StreamId = StreamType.Discussion.GetId();
                        streamActivity.StreamName = StreamType.Discussion.GetDescription();
                        streamActivity.Pinned = (pinneds != null && pinneds.Any(e => e == qbDiscussion.Id));
                        streamActivity.AppTitle = "Discussion " + qbDiscussion.Id + "-" + qbDiscussion.Name;
                        streamActivity.AppTitleMsg = qbDiscussion.ActivityMembers.Count() + " people";
                        break;
                    case QbicleActivity.ActivityTypeEnum.AlertActivity:
                        var qbAlert = (QbicleAlert)item;
                        streamActivity.Id = qbAlert.Id;
                        streamActivity.StreamId = StreamType.Alert.GetId();
                        streamActivity.StreamName = StreamType.Alert.GetDescription();
                        streamActivity.Pinned = (pinneds != null && pinneds.Any(e => e == qbAlert.Id));
                        streamActivity.AppTitle = "Alert " + qbAlert.Id + "-" + qbAlert.Name;
                        streamActivity.AppTitleMsg = qbAlert.StartedDate.ToString(dateFormat + " hh:mmtt").ToLower();
                        break;
                    case QbicleActivity.ActivityTypeEnum.MediaActivity:
                        var qbMedia = (QbicleMedia)item;
                        streamActivity.Id = qbMedia.Id;
                        streamActivity.StreamId = StreamType.Medias.GetId();
                        streamActivity.StreamName = StreamType.Medias.GetDescription();
                        streamActivity.Pinned = (pinneds != null && pinneds.Any(e => e == qbMedia.Id));
                        streamActivity.AppTitle = "Media " + qbMedia.Id + "-" + qbMedia.Name;
                        var mediaLastupdate = qbMedia.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                        var fileName = mediaLastupdate != null ? $"Media name:  {mediaLastupdate.Media.Name}" : "";

                        streamActivity.AppTitleMsg = fileName + qbMedia.StartedDate.ToString(dateFormat + " hh:mmtt").ToLower();
                        break;
                    case QbicleActivity.ActivityTypeEnum.Link:
                        var qbLink = (QbicleLink)item;
                        streamActivity.Id = qbLink.Id;
                        streamActivity.StreamId = StreamType.Link.GetId();
                        streamActivity.StreamName = StreamType.Link.GetDescription();
                        streamActivity.Pinned = (pinneds != null && pinneds.Any(e => e == qbLink.Id));
                        streamActivity.AppTitle = "Link " + qbLink.Id + "-" + qbLink.Name;
                        streamActivity.AppTitleMsg = qbLink.URL;
                        break;
                }


                stream.Activities.Add(streamActivity);
            });
            return stream;
        }

        public static List<CalendarColor> ToMicro(this List<CalendarColor> dateCalendar)
        {
            return dateCalendar.Select(d => new CalendarColor
            {
                date = d.date,
                color = d.color == "dp-note-gray" ? "#4c7aa2" : "#f03e72"
            }).ToList();
        }

        public static MicroDiscussionActivity ToMicro(this QbicleDiscussion qbDiscussion, List<ApplicationUser> members, ApplicationUser user)
        {
            var dateTimeFormat = $"{user.DateFormat} {user.TimeFormat}";
            var microDiscussion = new MicroDiscussionActivity
            {
                Id = qbDiscussion.Id,
                Title = qbDiscussion.Name,
                Domain = qbDiscussion.Qbicle.Domain.Name,
                QbicleId = qbDiscussion.Qbicle.Id,
                Topic = qbDiscussion.Topic.Name,
                TopicId = qbDiscussion.Topic.Id,
                Summary = qbDiscussion.Summary,
                FeaturedImage = qbDiscussion.FeaturedImageUri?.ToUri(),
                StartedDate = qbDiscussion.StartedDate.ConvertTimeFromUtc(user.Timezone).ToString(dateTimeFormat),
                ExpireDateString = qbDiscussion.ExpiryDate.ConvertTimeFromUtc(user.Timezone)?.ToString(dateTimeFormat),
                ExpireDate = qbDiscussion.ExpiryDate.ConvertTimeFromUtc(user.Timezone),
                IsExpiry = qbDiscussion.ExpiryDate != null,
                Participants = qbDiscussion.ActivityMembers.ToUserMembers(),
                QbicleMembers = members.ToUserMembers(),
                Comments = qbDiscussion.Posts.ToActivityComments(user),
                Medias = qbDiscussion.SubActivities.ToActivityMedias(user),
                CreatedBy = qbDiscussion.StartedBy.GetFullName(),
                UserAvatar = qbDiscussion.StartedBy.ProfilePic.ToUri()
            };


            return microDiscussion;
        }

        public static MicroLinkActivity ToMicro(this QbicleLink qbLink, ApplicationUser user)
        {
            var microLink = new MicroLinkActivity
            {
                Id = qbLink.Id,
                Title = qbLink.Name,
                Domain = qbLink.Qbicle.Domain.Name,
                QbicleId = qbLink.Qbicle.Id,
                Topic = qbLink.Topic.Name,
                TopicId = qbLink.Topic.Id,
                Description = qbLink.Description,
                FeaturedImage = qbLink.FeaturedImage?.VersionedFiles.FirstOrDefault()?.Uri.ToUri(),
                Url = qbLink.URL,
                Comments = qbLink.Posts.ToActivityComments(user),
                Medias = qbLink.SubActivities.ToActivityMedias(user),
                CreatedBy = qbLink.StartedBy.GetFullName(),
                UserAvatar = qbLink.StartedBy.ProfilePic.ToUri()
            };

            return microLink;
        }

        public static MicroEventActivity ToMicro(this QbicleEvent qbEvent, List<ApplicationUser> members, ApplicationUser user, List<QbiclePeople> invites)
        {
            var eWhen = "";
            if (!qbEvent.isRecurs)
                eWhen = qbEvent.Start.FormatDatetimeOrdinal();
            else
            {
                //Update recurrence true
                var recurrence = qbEvent.AssociatedSet != null ? qbEvent.AssociatedSet.Recurrance : null;
                if (recurrence != null)
                    eWhen = "Recurring " + Utility.ShowRecurring(qbEvent.AssociatedSet.Recurrance, qbEvent.ProgrammedStart.HasValue ? qbEvent.ProgrammedStart.Value.ToString("\"at\" h:mmtt").Replace(":00", "").ToLower() : "");
            }

            //Invites

            var currentInvite = invites.FirstOrDefault(s => s.User.Id == user.Id);
            //var relates = qbEvent.AssociatedSet != null ? qbEvent.AssociatedSet.Relateds.ToList() : new List<QbicleRelated>();

            var microEvent = new MicroEventActivity
            {
                Id = qbEvent.Id,
                Title = qbEvent.Name,
                Domain = qbEvent.Qbicle.Domain.Name,
                QbicleId = qbEvent.Qbicle.Id,
                Topic = qbEvent.Topic.Name,
                TopicId = qbEvent.Topic.Id,
                Where = qbEvent.Location,
                When = eWhen,
                Description = qbEvent.Description,
                Duration = $"{qbEvent.Duration} {qbEvent.DurationUnit.GetDescription()}",
                EventType = qbEvent.EventType.GetDescription(),
                Present = currentInvite != null && currentInvite.isPresent.HasValue && !currentInvite.isPresent.Value ? "" : "I'm going",
                Attending = currentInvite?.isPresent,
                AttendingId = currentInvite?.Id,
                QbicleMembers = members.ToUserMembers(),
                Comments = qbEvent.Posts.ToActivityComments(user),
                Medias = qbEvent.SubActivities.ToActivityMedias(user),
                Attendees = invites.ToUserMembers(user.Id, qbEvent.Qbicle.Domain),
                CreatedBy = qbEvent.StartedBy.GetFullName(),
                UserAvatar = qbEvent.StartedBy.ProfilePic.ToUri()
            };


            return microEvent;
        }

        public static MicroTaskActivity ToMicro(this QbicleTask qbTask, List<ApplicationUser> members, ApplicationUser user)
        {
            //var currentInvite = invites.FirstOrDefault(s => s.User.Id == user.Id);
            //var watchers = qbTask.AssociatedSet != null ? qbTask.AssociatedSet.Peoples.Where(s => s.User.Id != qbTask.StartedBy.Id).ToList() : new List<QbiclePeople>();
            var watchers = qbTask.AssociatedSet?.Peoples.ToList() ?? new List<QbiclePeople>();


            var deadline = qbTask.ProgrammedStart;

            switch (qbTask.DurationUnit)
            {
                case QbicleTask.TaskDurationUnitEnum.Hours:
                    deadline = qbTask.ProgrammedStart.HasValue ? qbTask.ProgrammedStart.Value.AddHours(qbTask.Duration) : DateTime.UtcNow;
                    break;
                case QbicleTask.TaskDurationUnitEnum.Days:
                    deadline = qbTask.ProgrammedStart.HasValue ? qbTask.ProgrammedStart.Value.AddDays(qbTask.Duration) : DateTime.UtcNow;
                    break;
                case QbicleTask.TaskDurationUnitEnum.Weeks:
                    deadline = qbTask.ProgrammedStart.HasValue ? qbTask.ProgrammedStart.Value.AddDays(qbTask.Duration * 7) : DateTime.UtcNow;
                    break;
            }
            var duration = "";
            if (!qbTask.isComplete && qbTask.ActualStart.HasValue && qbTask.ProgrammedEnd.HasValue && qbTask.ActualStart.Value < qbTask.ProgrammedEnd.Value)
            {
                var time = qbTask.ProgrammedEnd.Value - qbTask.ActualStart.Value;
                duration = $"{time.Days}d {time.Hours}h {time.Minutes}m";
            }
            var taskAssignee = qbTask.AssociatedSet.Peoples.FirstOrDefault(m => m.Type == QbiclePeople.PeopleTypeEnum.Assignee);

            //if have step then StartProgress else mark complete
            var microTask = new MicroTaskActivity
            {
                Id = qbTask.Id,
                Title = qbTask.Name,
                Deadline = deadline.HasValue ? deadline.Value.FormatDatetimeOrdinal() : "",
                CanAddWorklog = qbTask.StartedBy.Id == user.Id || watchers.Any(s => s.Type == QbiclePeople.PeopleTypeEnum.Assignee && s.User.Id == user.Id),
                Domain = qbTask.Qbicle.Domain.Name,
                QbicleId = qbTask.Qbicle.Id,
                Topic = qbTask.Topic.Name,
                TopicId = qbTask.Topic.Id,
                Started = qbTask.ActualStart != null,
                Completed = qbTask.isComplete,
                Description = qbTask.Description,
                PriorityId = qbTask.Priority,
                PriorityName = qbTask.Priority.GetDescription(),
                TimeBegin = qbTask.ProgrammedStart?.ToString(user.DateFormat),
                Duration = $"{qbTask.Duration} {qbTask.DurationUnit.GetDescription()}",
                Status = qbTask.isComplete == true ? "Complete" : "In progress",
                Assignee = taskAssignee.User.GetFullName(),
                AssigneeId = taskAssignee.User.Id,
                QbicleMembers = members.ToUserMembers(),
                Comments = qbTask.Posts.ToActivityComments(user),
                Medias = qbTask.SubActivities.ToActivityMedias(user),
                Watchers = watchers.ToUserMembers(user.Id, qbTask.Qbicle.Domain),
                CreatedBy = qbTask.StartedBy.GetFullName(),
                UserAvatar = qbTask.StartedBy.ProfilePic.ToUri(),
                TimeLoggings = qbTask.QTimeSpents.ToList().ToTimeLoggings(qbTask.Id, user.Timezone)
            };

            if (qbTask.ActualStart == null)
            {
                microTask.Status = "Not started";
            }

            switch (microTask.Status)
            {
                case "Complete":
                    microTask.StatusColor = StatusLabelStyle.SuccessColor;
                    break;
                case "In progress":
                    microTask.StatusColor = StatusLabelStyle.WarningColor;
                    break;
                case "Not started":
                    microTask.StatusColor = StatusLabelStyle.PrimaryColor;
                    break;
            }
            return microTask;
        }

        #endregion

        #region User profile

        #region Showcase
        public static MicroUserShowcase ToUserShowcase(this Showcase showcase)
        {
            return new MicroUserShowcase
            {
                Id = showcase.Id,
                Caption = showcase.Caption,
                Name = showcase.Title,
                Title = showcase.Title,
                ImageUri = showcase.ImageUri.ToDocumentUri()
            };
        }
        public static List<MicroUserShowcase> ToUserShowcase(this List<Showcase> showcasees)
        {
            return showcasees.Select(m =>
                   new MicroUserShowcase
                   {
                       Id = m.Id,
                       Caption = m.Caption,
                       Name = m.Title,
                       Title = m.Title,
                       ImageUri = m.ImageUri.ToDocumentUri()
                   }).ToList();
        }
        #endregion

        #region Skill
        public static List<MicroUserSkill> ToUserSkill(this List<Skill> skills)
        {
            return skills.Select(m =>
                   new MicroUserSkill
                   {
                       Id = m.Id,
                       Name = m.Name,
                       Proficiency = m.Proficiency
                   }).ToList();
        }
        public static MicroUserSkill ToUserSkill(this Skill skill)
        {
            return new MicroUserSkill
            {
                Id = skill.Id,
                Name = skill.Name,
                Proficiency = skill.Proficiency
            };
        }
        #endregion

        #region Work
        public static MicroUserExperience ToUserWorkExperience(this WorkExperience work, string dateTimeFormat, string timezone)
        {
            if (work == null) return null;
            return new MicroUserExperience
            {
                Id = work.Id,
                Name = work.Summary,
                Summary = work.Summary,
                EndDate = work.EndDate?.ConvertTimeFromUtc(timezone),
                StartDate = work.StartDate.ConvertTimeFromUtc(timezone),
                EndDateString = work.EndDate == null ? "" : work.EndDate.Value.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                StartDateString = work.StartDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                Type = work.Type,
                Company = work.Company,
                Role = work.Role,
            };
        }
        public static List<MicroUserExperience> ToUserWorkExperience(this List<WorkExperience> works, string dateTimeFormat, string timezone)
        {
            if (works == null) return null;
            return works.Select(w =>
                   new MicroUserExperience
                   {
                       Id = w.Id,
                       Name = w.Summary,
                       Summary = w.Summary,
                       EndDate = w.EndDate?.ConvertTimeFromUtc(timezone),
                       StartDate = w.StartDate.ConvertTimeFromUtc(timezone),
                       EndDateString = w.EndDate == null ? "" : w.EndDate.Value.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                       StartDateString = w.StartDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                       Company = w.Company,
                       Role = w.Role,
                       Type = w.Type
                   }).ToList();
        }
        #endregion

        #region Education
        public static MicroUserExperience ToUserEducationExperience(this EducationExperience education, string dateTimeFormat, string timezone)
        {
            if (education == null) return null;
            return new MicroUserExperience
            {
                Id = education.Id,
                Name = education.Summary,
                Summary = education.Summary,
                EndDate = education.EndDate?.ConvertTimeFromUtc(timezone),
                StartDate = education.StartDate.ConvertTimeFromUtc(timezone),
                EndDateString = education.EndDate == null ? "" : education.EndDate.Value.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                StartDateString = education.StartDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                Type = education.Type,
                Course = education.Course,
                Institution = education.Institution,
            };
        }
        public static List<MicroUserExperience> ToUserEducationExperience(this List<EducationExperience> educations, string dateTimeFormat, string timezone)
        {
            if (educations == null) return null;
            return educations.Select(e =>
                   new MicroUserExperience
                   {
                       Id = e.Id,
                       Name = e.Summary,
                       Summary = e.Summary,
                       EndDate = e.EndDate?.ConvertTimeFromUtc(timezone),
                       StartDate = e.StartDate.ConvertTimeFromUtc(timezone),
                       EndDateString = e.EndDate == null ? "" : e.EndDate.Value.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                       StartDateString = e.StartDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                       Course = e.Course,
                       Institution = e.Institution,
                       Type = e.Type
                   }).ToList();
        }
        #endregion

        #region Address
        public static List<MicroUserAddress> ToUserAddress(this List<TraderAddress> addresses)
        {
            return addresses.Select(m =>
                   new MicroUserAddress
                   {
                       Id = m.Id,
                       AddressLine1 = m.AddressLine1,
                       AddressLine2 = m.AddressLine2,
                       City = m.City,
                       Country = new MicroCountry { CountryCode = m.Country?.CountryCode, CommonName = m.Country?.CommonName },
                       Email = m.Email,
                       IsDefault = m.IsDefault,
                       Latitude = m.Latitude,
                       Longitude = m.Longitude,
                       Phone = m.Phone,
                       PostCode = m.PostCode,
                       State = m.State
                   }).ToList();
        }
        public static MicroUserAddress ToUserAddress(this TraderAddress m)
        {
            return
                   new MicroUserAddress
                   {
                       Id = m.Id,
                       AddressLine1 = m.AddressLine1,
                       AddressLine2 = m.AddressLine2,
                       City = m.City,
                       Country = new MicroCountry { CountryCode = m.Country?.CountryCode, CommonName = m.Country?.CommonName },
                       Email = m.Email,
                       IsDefault = m.IsDefault,
                       Latitude = m.Latitude,
                       Longitude = m.Longitude,
                       Phone = m.Phone,
                       PostCode = m.PostCode,
                       State = m.State
                   };
        }
        #endregion

        #region Public file
        public static List<MicroUserFileShare> ToUserPublicFiles(this List<UserProfileFile> files)
        {
            return files.Select(m =>
                   new MicroUserFileShare
                   {
                       Id = m.Id,
                       Description = m.Description,
                       Name = m.StoredFileName,
                       StoredFileName = m.StoredFileName,
                       Title = m.Title,
                       FileType = new MicroFileType
                       {
                           Extension = m.FileType?.Extension,
                           Type = m.FileType?.Type
                       }
                   }).ToList();
        }
        public static MicroUserFileShare ToUserPublicFiles(this UserProfileFile file)
        {
            return
                   new MicroUserFileShare
                   {
                       Id = file.Id,
                       Description = file.Description,
                       Name = file.StoredFileName,
                       StoredFileName = file.StoredFileName,
                       Title = file.Title,
                       FileType = new MicroFileType
                       {
                           Extension = file.FileType?.Extension,
                           Type = file.FileType?.Type
                       }
                   };
        }
        #endregion


        public static List<MicroUserSharedQbicles> ToUserSharedQbicles(this List<Qbicle> qbicles, string userId)
        {
            return qbicles.Select(m =>
                   new MicroUserSharedQbicles
                   {
                       Id = m.Id,
                       Name = m.Name,
                       LogoUri = m.LogoUri.ToUri(),
                       Domain = m.Domain.Name,
                       IsMyDomain = m.Domain.OwnedBy?.Id == userId || m.Domain.Administrators.Any(u => u.Id == userId)
                   }).ToList();
        }

        public static MicroUserProfile ToUserProfile(this ApplicationUser user, string currentUserId, UserConnectStatus connectStatus)
        {
            if (user == null) return new MicroUserProfile { };
            return new MicroUserProfile
            {
                Id = user.Id,
                Email = user.Email,
                ChosenNotificationMethod = user.ChosenNotificationMethod,
                Company = user.Company,
                Connect = connectStatus.GetDescription(),
                ConnectStatus = connectStatus,
                ShowConnect = user.Id != currentUserId,
                DateFormat = user.DateFormat,
                DisplayUserName = user.DisplayUserName,
                FacebookLink = user.FacebookLink,
                UserName = user.UserName,
                Forename = user.Forename,
                InstagramLink = user.InstagramLink,
                IsAlwaysLimitMyContact = user.isAlwaysLimitMyContact ?? false,
                IsShareCompany = user.isShareCompany ?? false,
                IsShareEmail = user.isShareEmail ?? false,
                IsShareJobTitle = user.isShareJobTitle ?? false,
                PreferredDomain = new BaseModel { Id = user.PreferredDomain?.Id ?? 0, Name = user.PreferredDomain?.Name },
                PreferredQbicle = new BaseModel { Id = user.PreferredQbicle?.Id ?? 0, Name = user.PreferredQbicle?.Name },
                JobTitle = user.JobTitle,
                LinkedlnLink = user.LinkedlnLink,
                NotificationSound = user.NotificationSound,
                Profile = user.Profile,
                ProfilePic = user.ProfilePic.ToUri(),
                Surname = user.Surname,
                TagLine = user.TagLine,
                Tell = user.Tell,
                TimeFormat = user.TimeFormat,
                Timezone = user.Timezone,
                TwitterLink = user.TwitterLink,
                WhatsAppLink = user.WhatsAppLink,
                MyProfile = user.Id == currentUserId
            };
        }
        #endregion

        #region Community
        /// <summary>
        /// filter offline : bussiness -> contactType =2, invidual -> contactType=1, Pending -> Status = 1
        /// </summary>
        /// <param name="communities"></param>
        /// <param name="currentUserId"></param>
        /// <param name="timezone"></param>
        /// <returns></returns>
        public static List<MicroCommunity> ToCommunity(this List<B2CC2CModel> communities, string currentUserId, string timezone)
        {
            var microCommunities = new List<MicroCommunity>();
            communities.ForEach(c =>
            {
                var linkUser = c.LinkUsers.Where(u => u.Id != currentUserId).FirstOrDefault();
                var community = new MicroCommunity
                {
                    Id = c.Id,
                    DomainKey = c.LinkBusiness?.Domain.Key ?? "",
                    DomainSubAccountCode = c.LinkBusiness?.Domain.SubAccountCode ?? "",
                    QbicleId = c.QbicleId,
                    Status = c.Status,
                    StatusName = c.Status.GetDescription(),
                    Type = c.Type,
                    TypeName = c.Type == 2 ? "Individual" : "Business",
                    LastUpdated = c.LastUpdated.ConvertTimeFromUtc(timezone),
                    LastUpdatedString = c.LastUpdated.GetTimeRelative(),
                    NewActivities = 0//what is new?
                };
                if (c.Type == 2)
                {
                    community.CanCreateAddFile = true;
                    community.CanCreateAddTask = true;
                    community.CanCreateDiscussion = true;
                }
                if (linkUser != null)
                {
                    community.LinkId = linkUser.Id;
                    community.Image = linkUser.ProfilePic.ToUri(FileTypeEnum.Image, "T");
                    community.Name = linkUser.GetFullName();
                    community.Surname = linkUser.Surname;
                    community.ContactType = 1;
                    community.ComnunityEmail = linkUser.Email;
                }
                else if (c.LinkBusiness != null)
                {
                    community.LinkId = c.LinkBusiness.Id.ToString();
                    community.Name = c.LinkBusiness.BusinessName;
                    community.Surname = c.LinkBusiness.BusinessName;
                    community.ContactType = 2;
                    community.Image = c.LinkBusiness.LogoUri.ToUri(FileTypeEnum.Image, "T");
                    community.ComnunityEmail = c.LinkBusiness.BusinessEmail;
                }
                if (c.LikedBy.Any(u => (linkUser != null && u.Id == linkUser.Id) || (c.LinkBusiness != null && u.Id == currentUserId)))
                    community.Favourited = true;
                else
                    community.Favourited = false;

                community.Actions = new List<string>
                {
                    "ViewProfile"
                };

                if (c.Status == CommsStatus.Approved)
                {
                    if (c.LikedBy.Any(u => (linkUser != null && u.Id == linkUser.Id) || (c.LinkBusiness != null && u.Id == currentUserId)))
                    {
                        community.Actions.Add("RemoveFromFavourites");
                    }
                    else
                    {
                        community.Actions.Add("AddToFavourites");
                    }
                }

                if (c.Status == CommsStatus.Approved)
                {
                    community.Actions.Add("Block");
                }
                else if (c.Status == CommsStatus.Blocked)
                {
                    community.Actions.Add("Unblock");
                }
                //Request from other Customer
                else if (c.Status == CommsStatus.Pending && c.Type == 2 && c.SourceUser.Id != currentUserId)
                {
                    community.Actions.Add("Accept");
                    community.Actions.Add("Decline");
                }
                //Sent from current user
                else if (c.Status == CommsStatus.Pending && c.Type == 2 && c.SourceUser.Id == currentUserId)
                {
                    community.Actions.Add("CancelRequest");
                }

                community.Actions.Add("Remove");

                microCommunities.Add(community);
            });
            return microCommunities;


        }

        public static List<MicroCommunity> ToCommunity(this PaginationResponse communities)
        {
            var microCommunities = new List<MicroCommunity>();
            communities.totalNumber /= 8;

            return microCommunities;


        }

        public static List<MicroContact> ToMicroUser(this List<ApplicationUser> users)
        {
            return users.Where(u => u != null).Select(user => new MicroContact
            {
                Id = user.Id,
                AvatarUri = user.ProfilePic.ToUri(),
                Email = user.Email,
                Name = user.GetFullName(),
                Phone = user.PhoneNumber
            }).OrderBy(n => n.Name).ToList();
        }

        public static List<MicroActivitesStream> ToMicro(this List<QbicleActivity> qbicleActivities, string currentTimeZone, string dateFormat, string currentUserId)
        {
            var microStreams = new List<MicroActivitesStream>();

            var today = DateTime.UtcNow;

            foreach (var item in qbicleActivities)
            {
                microStreams.Add(MicroStreamRules.GenerateActivity(item, today.Date, null, currentUserId, dateFormat, currentTimeZone, false));
            }

            return microStreams;
        }
        #endregion

        #region Private method
        private static List<MicroUserBase> ToUserMembers(this List<ApplicationUser> users)
        {
            return users.Select(m =>
                   new MicroUserBase
                   {
                       Id = m.Id,
                       FullName = m.GetFullName(),
                       ProfilePic = m.ProfilePic.ToUri()
                   }).ToList();
        }
        private static List<MicroUserBase> ToUserMembers(this List<QbiclePeople> peoples, string userId, QbicleDomain domain)
        {
            var users = new List<MicroUserBase>();
            if (domain.Administrators.Any(a => a.Id == userId))
            {
                foreach (var invite in peoples.Select(i => i.User))
                {
                    var attend = new MicroUserBase
                    {
                        Id = invite.Id,
                        FullName = invite.GetFullName(userId),
                        ProfilePic = invite.ProfilePic.ToUri()
                    };
                    if (domain.Administrators.Any(u => u.Id == invite.Id))
                        attend.Role = AdminLevel.Administrators.GetDescription();
                    else if (domain.QbicleManagers.Any(u => u.Id == invite.Id))
                        attend.Role = AdminLevel.QbicleManagers.GetDescription();
                    else
                        attend.Role = AdminLevel.Users.GetDescription();

                    users.Add(attend);
                }
            }
            else if (domain.QbicleManagers.Any(a => a.Id == userId))
            {
                foreach (var invite in peoples.Select(i => i.User))
                {
                    if (!domain.Administrators.Any(u => u.Id == invite.Id))
                    {
                        var attend = new MicroUserBase
                        {
                            Id = invite.Id,
                            FullName = invite.GetFullName(userId),
                            ProfilePic = invite.ProfilePic.ToUri()
                        };
                        if (domain.QbicleManagers.Any(u => u.Id == invite.Id))
                            attend.Role = AdminLevel.QbicleManagers.GetDescription();
                        else
                            attend.Role = AdminLevel.Users.GetDescription();

                        users.Add(attend);
                    }

                }
            }
            return users;
        }
        private static MicroActivityComments ToActivityComments(this List<QbiclePost> comments, ApplicationUser user)
        {
            string timezone = user.Timezone;
            string timeFormat = user.TimeFormat;
            var dateTimeFormat = $"{user.DateFormat} {timeFormat}";

            return new MicroActivityComments
            {
                Comments = comments.OrderByDescending(x => x.StartedDate).Take(HelperClass.activitiesPageSize).Select(p => new MicroActivityComment
                {
                    Id = p.Id,
                    CreatedBy = p.CreatedBy.GetFullName(user.Id),
                    CreatedDate = p.StartedDate.Date == DateTime.UtcNow.Date ? "Today, " + p.StartedDate.ConvertTimeFromUtc(timezone).ToString(timeFormat) : p.StartedDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                    Image = p.CreatedBy.ProfilePic.ToUri(),
                    Message = p.Message
                }).ToList(),
                Total = (comments.Count / HelperClass.activitiesPageSize) + (comments.Count % HelperClass.activitiesPageSize == 0 ? 0 : 1),
                CommentTotal = comments.Count
            };
        }
        private static MicroActivityMedias ToActivityMedias(this List<QbicleActivity> subActivities, ApplicationUser user)
        {
            string timezone = user.Timezone;
            string timeFormat = user.TimeFormat;
            var dateTimeFormat = $"{user.DateFormat} {timeFormat}";

            var microMedia = new MicroActivityMedias
            {
                Total = (subActivities.Count / HelperClass.activitiesPageSize) + (subActivities.Count % HelperClass.activitiesPageSize == 0 ? 0 : 1)
            };
            var medias = subActivities.OrderByDescending(x => x.Id).Take(HelperClass.activitiesPageSize).ToList();
            medias.ForEach(m =>
            {
                var media = (QbicleMedia)m;
                var createdDate = media.StartedDate.Date == DateTime.UtcNow.Date ? "Today, " + media.StartedDate.ConvertTimeFromUtc(timezone).ToString(timeFormat) : media.StartedDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat);

                var mediaLastupdate = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).First();
                var lastUpdateFile = mediaLastupdate != null ? (mediaLastupdate.UploadedDate.Date == DateTime.UtcNow.Date ? "Today, " + mediaLastupdate.UploadedDate.ConvertTimeFromUtc(timezone).ToString(timeFormat) : mediaLastupdate.UploadedDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat)) : createdDate;

                microMedia.Medias.Add(new MicroActivityMedia
                {
                    Id = media.Id,
                    Key = media.Key,
                    Name = media.Name,
                    Description = media.Description,
                    CreatedBy = media.StartedBy.GetFullName(),
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

            });
            return microMedia;
        }
        private static List<TimeLogging> ToTimeLoggings(this List<QbicleTimeSpent> timeSpents, int taskId, string timezone)
        {
            return timeSpents.Select(m =>
                   new TimeLogging
                   {
                       Id = m.Id,
                       TaskId = taskId,
                       DateTime = m.DateTime.ConvertTimeFromUtc(timezone).ToString("dddd dd\"th\" MMMM yyyy"),
                       Days = m.Days,
                       Hours = m.Hours,
                       Minutes = m.Minutes
                   }).ToList();
        }
        #endregion

        #region Moniback
        public static List<MicroMonibackMyStore> ToMonibackMyStore(this List<MonibacBusinessModel> monibacks)
        {
            var myStores = new List<MicroMonibackMyStore>();

            monibacks.ForEach(moniback =>
            {
                myStores.Add(new MicroMonibackMyStore
                {
                    ContactKey = moniback.Contact?.Key,
                    LogoUri = moniback.BusinessLogoUri?.ToUri(),
                    Name = moniback.BusinessName,
                    DomainId = moniback.DomainId.Encrypt(),
                    BusinessProfileId = moniback.BusinessProfileId.Encrypt()
                });
            });

            return myStores;
        }


        public static MicroMonibackMyStoreInfo ToMonibackMyStoreInfo(this MonibacBusinessModel moniback)
        {
            var myStoreInfo = new MicroMonibackMyStoreInfo
            {
                ContactKey = moniback.Contact?.Key,
                LogoUri = new Uri(moniback.BusinessLogoUri),
                Name = moniback.BusinessName,
                DomainId = moniback.DomainId.Encrypt(),
                QbicleId = moniback.QbicleId,
                BusinessProfileId = moniback.BusinessProfileId.Encrypt(),
                AccountBalance = moniback.AccountBalance,
                AccountBalanceString = moniback.AccountBalanceString,
                Points = (int)moniback.Points,
                StoreCredit = moniback.StoreCreditBalance,
                StoreCreditString = moniback.StoreCreditBalanceString,
                ValidVouchersCount = moniback.ValidVouchersCount
            };

            return myStoreInfo;
        }
        #endregion
    }

}
