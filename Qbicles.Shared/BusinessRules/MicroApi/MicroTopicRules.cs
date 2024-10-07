using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System.Collections.Generic;
using System.Linq;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroTopicRules : MicroRulesBase
    {
        public MicroTopicRules(MicroContext microContext) : base(microContext)
        {
        }

        public List<MicroTopic> GetTopicByQbicleId(int qbicleId)
        {
            var topics = new TopicRules(dbContext).ListTopic(qbicleId);
            return topics.ToMicro(CurrentUser.Timezone);
        }

        public List<string> GetTopicNamesByDomainId(int domainId)
        {
            return new TopicRules(dbContext).GetTopicByDomain(domainId);
        }

        public ReturnJsonModel SaveTopic(MicroTopic microTopic, string userId)
        {
            var rules = new TopicRules(dbContext);
            var duplicate = rules.DuplicateTopicNameCheck(microTopic.QbicleId, microTopic.Id, microTopic.Name);
            if (duplicate)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_74") };

            var tp = dbContext.Topics.Find(microTopic.Id);
            if (tp != null)
            {
                if (microTopic.Name != HelperClass.GeneralName && tp.Name == HelperClass.GeneralName)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_102") };
            }

            if (microTopic.Name.Length > 50)
            {
                microTopic.Name = microTopic.Name.Substring(0, 50);
            }
            if (!string.IsNullOrEmpty(microTopic.Summary) && microTopic.Summary.Length > 250)
            {
                microTopic.Summary = microTopic.Summary.Substring(0, 250);
            }

            var topic = new Topic
            {
                Id = microTopic.Id,
                Name = microTopic.Name,
                Summary = microTopic.Summary
            };
            var saved = rules.SaveTopic(microTopic.QbicleId, topic, userId);
            if (saved != null)
                return new ReturnJsonModel { result = true };
            return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_224") };
        }

        public bool DeleteTopic(int topicMoveId, int topicDeleteId)
        {
            var qbicleId = dbContext.Topics.Find(topicDeleteId).Qbicle.Id;
            if (topicMoveId == 0)
                topicMoveId = dbContext.Topics.First(e => e.Qbicle.Id == qbicleId && e.Name == HelperClass.GeneralName).Id;
            return new TopicRules(dbContext).DeleteTopic(topicMoveId, topicDeleteId, qbicleId);
        }
    }
}
