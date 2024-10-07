using Qbicles.BusinessRules.Micro.Model;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroPostRules : MicroRulesBase
    {
        public MicroPostRules(MicroContext microContext) : base(microContext)
        {
        }

        public ReturnJsonModel CreatePostOnStream(MicroPostParameter post)
        {
            return new PostsRules(dbContext).SavePostTopic(post.IsCreatorTheCustomer, post.Message, post.TopicName, CurrentUser.Id, post.QbicleId, post.OriginatingConnectionId, Models.AppType.Micro);
        }
    }
}
