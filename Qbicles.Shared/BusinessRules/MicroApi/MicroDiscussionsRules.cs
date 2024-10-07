using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using System;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroDiscussionsRules : MicroRulesBase
    {
        public MicroDiscussionsRules(MicroContext microContext) : base(microContext)
        {
        }

        public ReturnJsonModel CreateQbicleDiscussion(DiscussionQbicleModel discussion, bool isCreatorTheCustomer)
        {
            var rules = new DiscussionsRules(dbContext);


            var isDiscussionExist = rules.DuplicateDiscussionNameCheck(discussion.QbicleId, discussion.Id, discussion.Title);
            if (isDiscussionExist)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_393")
                };

            var user = CurrentUser;
            discussion.CurrentUser = user;
            discussion.CurrentTimezone = user.Timezone;

            discussion.Media = new MediaModel
            {
                IsPublic = false,
                UrlGuid = discussion.UploadKey
            };
            var userSetting = new UserSetting
            {
                Id = user.Id,
                TimeFormat= user.TimeFormat,
                DateFormat= user.DateFormat,
                Timezone= user.Timezone
            };
            return rules.SaveDiscussionForQbicle(discussion, userSetting, isCreatorTheCustomer,discussion.OriginatingConnectionId);
        }

        public MicroDiscussionActivity GetQbicleDiscussion(int id)
        {
            var discussion = new DiscussionsRules(dbContext).GetDiscussionById(id);
            var members = new QbicleRules(dbContext).GetUsersByQbicleId(discussion.Qbicle.Id);
            members = members.Except(discussion.ActivityMembers).ToList();
            return discussion.ToMicro(members, CurrentUser);
        }

        /// <summary>
        /// Add a Participant to the discussion
        /// </summary>
        /// <param name="discussion"></param>
        /// <param name="isAdd">true - add, false - remove</param>
        /// <returns></returns>
        public ReturnJsonModel QbicleDiscussionParticipant(DiscussionQbicleModel discussion, bool isAdd,bool isCreatorTheCustomer)
        {
            try
            {
                var discussionDb = dbContext.Discussions.Find(discussion.Id);
                discussionDb.IsCreatorTheCustomer = isCreatorTheCustomer;
                var rules = new UserRules(dbContext);
                foreach (var item in discussion.Assignee)
                {
                    var user = rules.GetUser(item, 0);
                    if (user != null)
                    {
                        if(isAdd)
                            discussionDb.ActivityMembers.Add(user);
                        else
                            discussionDb.ActivityMembers.Remove(user);
                    }
                }
                dbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = "",
                    Id = discussion.Id,
                    EventNotify = NotificationEventEnum.AddUserParticipants,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = CurrentUser.Id,
                    CreatedByName = CurrentUser.DisplayUserName,
                    ReminderMinutes = 0
                };
                if (isAdd == false)
                    activityNotification.EventNotify = NotificationEventEnum.RemoveUserParticipants;

                new NotificationRules(dbContext).Notification2Activity(activityNotification);

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussion, isAdd);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }

        }



    }
}
