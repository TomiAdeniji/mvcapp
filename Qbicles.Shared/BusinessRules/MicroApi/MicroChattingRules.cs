using Qbicles.BusinessRules.Micro.Model;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroChattingRules : MicroRulesBase
    {
        public MicroChattingRules(MicroContext microContext) : base(microContext)
        {
        }
        public void MicroChatting(string toUsers, bool typing, int dicussionId)
        {
            new NotificationRules(dbContext).TypingChat(CurrentUser.ToUserSetting(), toUsers, typing ? NotificationEventEnum.TypingChat : NotificationEventEnum.EndTypingChat, dicussionId);
        }
    }
}
