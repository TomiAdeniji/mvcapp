using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroActivityCommentsRules : MicroRulesBase
    {
        public MicroActivityCommentsRules(MicroContext microContext) : base(microContext)
        {
        }

        public MicroActivityComments GetActivityComments(int activityId, int pageIndex)
        {
            string timezone = CurrentUser.Timezone; string dateFormat = CurrentUser.DateFormat; string timeFormat = CurrentUser.TimeFormat;
            var dateTimeFormat = $"{dateFormat} {timeFormat}";

            var posts = dbContext.Activities.Find(activityId)?.Posts;
            pageIndex *= HelperClass.activitiesPageSize;
            var totalComment = posts.Count;
            var totalSize = (totalComment / HelperClass.activitiesPageSize) + (totalComment % HelperClass.activitiesPageSize == 0 ? 0 : 1);
            posts = posts.OrderByDescending(d => d.StartedDate).Skip(pageIndex).Take(HelperClass.activitiesPageSize).ToList();

            return new MicroActivityComments
            {
                Comments = posts.Select(p =>
                new MicroActivityComment
                {
                    Id = p.Id,
                    CreatedBy = p.CreatedBy.GetFullName(CurrentUser.Id),
                    CreatedDate = p.StartedDate.Date == DateTime.UtcNow.Date ? "Today, " + p.StartedDate.ConvertTimeFromUtc(timezone).ToString(timeFormat) : p.StartedDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat),
                    DateCreated = p.StartedDate.Date == DateTime.UtcNow.Date ? "Today" : p.StartedDate.ConvertTimeFromUtc(timezone).ToShortDateString(),
                    Image = p.CreatedBy.ProfilePic.ToUri(),
                    Message = p.Message
                }).ToList(),
                Total = totalSize,
                CommentTotal = totalComment
            };
        }

        public ReturnJsonModel AddActivityComment(MicroPostParameter comment)
        {
            try
            {
                var post = new PostsRules(dbContext).SavePost(comment.IsCreatorTheCustomer, comment.Message, CurrentUser.Id, comment.QbicleId);

                if (post == null)
                    return new ReturnJsonModel { result = false, msg = "exception" };

                switch (comment.ActivityType)
                {
                    case StreamType.Event:
                        new EventsRules(dbContext).AddPostToEvent(comment.ActivityId, post, comment.OriginatingConnectionId);
                        break;
                    case StreamType.Task:
                        new TasksRules(dbContext).AddPostToTask(comment.ActivityId, post, comment.OriginatingConnectionId);
                        break;
                    case StreamType.Medias:
                        new MediasRules(dbContext).AddPostToMedia(comment.ActivityId, post, comment.OriginatingConnectionId);
                        break;
                    case StreamType.Discussion:
                        new DiscussionsRules(dbContext).AddComment(comment.IsCreatorTheCustomer, comment.ActivityId.Encrypt(), post, "", ApplicationPageName.Discussion,
                            false, true, comment.OriginatingConnectionId);
                        break;
                    case StreamType.DiscussionOrder:
                        new DiscussionsRules(dbContext).AddComment(comment.IsCreatorTheCustomer, comment.ActivityId.Encrypt(), post, "", ApplicationPageName.DiscussionOrder,
                            false, true, comment.OriginatingConnectionId);
                        break;
                    case StreamType.Link:
                        new DiscussionsRules(dbContext).AddComment(comment.IsCreatorTheCustomer, comment.ActivityId.Encrypt(), post, "", ApplicationPageName.Link,
                            false, true, comment.OriginatingConnectionId);
                        break;
                }
                var result = new ReturnJsonModel { result = true, actionVal = post.Id };
                if (string.IsNullOrEmpty(comment.OriginatingConnectionId)) return result;

                result.Object = MicroStreamRules.GenerateActivity(post, post.StartedDate, null,
                    CurrentUser.Id, CurrentUser.DateFormat, CurrentUser.Timezone, false, NotificationEventEnum.PostCreation);

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, comment);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public object B2COrderGetVoucher(string domainKey)
        {
            var currentDate = DateTime.UtcNow;
            var currentDomainId = domainKey.Decrypt2Int();
            var currentUserId = CurrentUser.Id;

            var queryVouchers = dbContext.Vouchers.Where(s =>
                s.ClaimedBy.Id == currentUserId &&
                s.Promotion.Domain.Id == currentDomainId
                );
            #region Filters
            //queryVouchers = queryVouchers.Where(s => !s.IsRedeemed && s.VoucherExpiryDate >= currentDate && s.Promotion.StartDate <= currentDate);

            queryVouchers = queryVouchers.Where(s => !s.IsRedeemed && !s.Promotion.IsArchived && !s.Promotion.IsHalted
                                                && s.VoucherExpiryDate >= currentDate
                                                && s.Promotion.StartDate <= currentDate && s.Promotion.EndDate >= currentDate
                                                );
            #endregion
            #region Sorting

            queryVouchers = queryVouchers.OrderByDescending(s => s.CreatedDate);
            #endregion
            #region Paging
            var list = queryVouchers.ToList();

            #endregion
            return list.Select(s => new
            {
                VourcherId = s.Id,
                s.Promotion.Name,
                Expires = "Expires on " + s.VoucherExpiryDate?.ConvertTimeFromUtc(CurrentUser.Timezone).DatetimeToOrdinalAndTime() ?? s.Promotion.EndDate.ConvertTimeFromUtc(CurrentUser.Timezone).DatetimeToOrdinalAndTime(),
                Image = s.Promotion.FeaturedImageUri.ToUri()
            }).ToList();
        }
    }
}
