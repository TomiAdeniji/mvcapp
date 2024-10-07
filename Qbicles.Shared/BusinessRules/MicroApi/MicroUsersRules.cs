using Qbicles.BusinessRules.Micro.Model;
using static Qbicles.BusinessRules.Enums;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroUsersRules : MicroRulesBase
    {
        public MicroUsersRules(MicroContext microContext) : base(microContext)
        {
        }

        public ReturnJsonModel GetUserNavigation()
        {
            var user = new
            {
                CurrentUser.Id,
                Name = CurrentUser.GetFullName(),
                IsSound = CurrentUser.NotificationSound,
                Image = CurrentUser.ProfilePic.ToUri(FileTypeEnum.Image, "T"),
                Notifications = new NotificationRules(dbContext).GetNotificationByUser(CurrentUser.Id, false)
            };
            return new ReturnJsonModel { Object = user };
        }
    }
}
