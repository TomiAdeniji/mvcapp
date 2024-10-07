using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System.Collections.Generic;
using Qbicles.BusinessRules.Micro.Extensions;
using System.Linq;
using System;
using Qbicles.BusinessRules.Helper;
using System.Net.Http;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroQbiclesRules : MicroRulesBase
    {
        public MicroQbiclesRules(MicroContext microContext) : base(microContext)
        {
        }

        public List<MicroUser> QbicleMemebers(int qbicleId, string userId)
        {
            var users = new QbicleRules(dbContext).GetUsersByQbicleId(qbicleId);
            return users.ToMicro(userId);
        }

        public List<MicroQbicle> GetQbiclesByDomainId(int domainId)
        {

            var domain = dbContext.Domains.Find(domainId);
            var qbicles = domain.Administrators.Any(u => u.Id == CurrentUser.Id) ?
                    new QbicleRules(dbContext).GetQbicleByDomainId(domainId) :
                    new QbicleRules(dbContext).GetQbicleByUserId(domainId, CurrentUser.Id);           
            
            return qbicles.ToMicro(CurrentUser.Timezone);
        }


        public MicroQbicle GetQbicleById(int id)
        {
            var qbicle = new QbicleRules(dbContext).GetQbicleById(id);
            return qbicle.ToMicro(CurrentUser.Timezone);
        }

        public List<MicroQbicle> FilterQbilce(QbicleSearchParameter qbicleParameters)
        {
            var qbicles = new QbicleRules(dbContext).FilterQbicle(qbicleParameters);
            return qbicles.ToMicro();
        }

        public ReturnJsonModel CreateUpdateQbicle(MicroQbicle qbicle, string userId)
        {
            var qbiclesRule = new QbicleRules(dbContext);
            var checkName = qbiclesRule.DuplicateQbicleNameCheck(qbicle.Id, qbicle.Name, qbicle.DomainId);
            if (checkName)
                return new ReturnJsonModel
                {
                    result = false,
                    actionVal = 1
                };

            var newQbicle = new Qbicle
            {
                Id = qbicle.Id,
                Description = qbicle.Description,
                LogoUri = qbicle.LogoKey,
                Name = qbicle.Name
            };
            
            return qbiclesRule.SaveQbicle(newQbicle, qbicle.QbicleUsers, null, qbicle.DomainId, userId, qbicle.Manager ?? qbicle.OwnedBy);

        }

        public bool ShowOrHideQbicle(int qbicleId, bool isHidden)
        {
            return new QbicleRules(dbContext).ShowOrHideQbicle(qbicleId, isHidden);
        }


        public void SednEmailInvite(ApplicationUser guest, int activityId, QbicleActivity.ActivityTypeEnum type)
        {
            var requestUri = new Uri($"{ConfigManager.QbiclesUrl}/qbicles/ReSendInvited?tokenToUserId={guest.Id}&tokenToEmail={guest.Email}&activityId={activityId}&type={type}&sendByEmail={CurrentUser.Email}");
            var client = new HttpClient();
            client.GetAsync(requestUri);
        }
    }
}
