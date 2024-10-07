//using Qbicles.BusinessRules.Extensions;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models.MicroQbicleStream;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroDomainRules : MicroRulesBase
    {
        public MicroDomainRules(MicroContext microContext) : base(microContext)
        {
        }

        public List<MicroInvitation> GetDomainInvitations(string email)
        {
            var domainIvitations = new OurPeopleRules(dbContext).GetInvitationApproverByUser(email);

            return domainIvitations.ToMicro(CurrentUser.Timezone);
        }

        public ReturnJsonModel DomainInvitationUpdate(MicroApproverInvitation invitation, string userId)
        {
            var inv = new OurPeopleRules(dbContext).ApproverOrRejectInvitation(invitation.Id, invitation.Status, invitation.DomainId, invitation.Note, userId);
            return new ReturnJsonModel
            {
                actionVal = 2,
                result = inv.result,
                msg = inv.msg
            };
        }

        public List<MicroDomain> GetDomainByUserId(string userId)
        {
            var qbiclesDomains = new DomainRules(dbContext).GetDomainsByUserId(userId);

            var lstQbicNotNull = qbiclesDomains.Where(p => p.Qbicles.Count > 0).ToList();
            if (lstQbicNotNull.Any())
            {
                qbiclesDomains = qbiclesDomains.Where(p => !lstQbicNotNull.Any(a => a.Id == p.Id)).ToList();
                lstQbicNotNull = lstQbicNotNull.OrderByDescending(o => o.Qbicles?.OrderByDescending(a => a.LastUpdated).FirstOrDefault().LastUpdated).ToList();
                lstQbicNotNull.AddRange(qbiclesDomains);
                qbiclesDomains = lstQbicNotNull;
            }
            else
                qbiclesDomains = qbiclesDomains.OrderByDescending(o => o.CreatedDate).ToList();

            return qbiclesDomains.ToMicro(CurrentUser);
        }


        public List<MicroDomain> DomainsFilter(string searchText, int order, string userId)
        {
            List<Models.QbicleDomain> qbiclesDomains;
            if (!string.IsNullOrEmpty(searchText))
                qbiclesDomains = new DomainRules(dbContext).GetDomainsByUserId(userId, searchText);
            else
                qbiclesDomains = new DomainRules(dbContext).GetDomainsByUserId(userId);
            if (order == 0)
            {
                var lstQbicNotNull = qbiclesDomains.Where(p => p.Qbicles.Count > 0).ToList();
                if (lstQbicNotNull.Any())
                {
                    qbiclesDomains = qbiclesDomains.Where(p => !lstQbicNotNull.Any(a => a.Id == p.Id)).ToList();
                    lstQbicNotNull = lstQbicNotNull.OrderByDescending(o => o.Qbicles.OrderByDescending(a => a.LastUpdated).FirstOrDefault().LastUpdated).ToList();
                    lstQbicNotNull.AddRange(qbiclesDomains);
                    qbiclesDomains = lstQbicNotNull;
                }
            }
            else if (order == 1)
                qbiclesDomains = qbiclesDomains.OrderBy(o => o.Name).ToList();

            else
                qbiclesDomains = qbiclesDomains.OrderByDescending(o => o.Name).ToList();

            return qbiclesDomains.ToMicro(CurrentUser);

        }


        public string GetSubAccountCodeByDomain(int domainId)
        {
            var domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
            return domain?.SubAccountCode ?? "";
        }

        public List<MicroUser> DomainUsers(int domainId, string userId)
        {
            var users = new DomainRules(dbContext).GetUsersByDomainId(domainId);
            return users.ToMicro(userId);
        }

        public List<OurPeopleModel> DomainMembers(int domainId, string userId)
        {
            return new OurPeopleRules(dbContext).GetAllOurPeopleByDomain(domainId, CurrentUser.Timezone, userId);
        }

        public List<BaseModel> DomainRoles(int domainId)
        {
            var roles = new DomainRolesRules(dbContext).GetDomainRolesDomainId(domainId);
            return roles.ToMicro();
        }

        public bool RemoveUserFromDomain(int domainId, string userId, string removerId)
        {
            return new OurPeopleRules(dbContext).RemovedUserFromDomain(domainId, userId, removerId, 0);
        }

        public bool UpdateRoleForUser(int domainId, string userId, AdminLevel updateRoleTo, AdminLevel currentRole)
        {
            return new OurPeopleRules(dbContext).PromoteOrDemoteUser(domainId, userId, updateRoleTo, currentRole);
        }
    }
}
