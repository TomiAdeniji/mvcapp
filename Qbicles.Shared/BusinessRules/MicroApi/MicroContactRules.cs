using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using System.Linq;
using System;

namespace Qbicles.BusinessRules.MicroApi
{
    public class MicroContactRules : MicroRulesBase
    {
        public MicroContactRules(MicroContext microContext) : base(microContext)
        {
        }

        public object GetContactById(string contactId)
        {
            var id = contactId.Decrypt2Int();
            var contact = dbContext.TraderContacts.FirstOrDefault(e => e.Id == id);

            return new
            {
                contact.Name,
                contact.CompanyName,
                contact.Email,
                contact.PhoneNumber,
                contact.JobTitle,
                contact.Address.AddressLine1,
                contact.Address.AddressLine2,
                contact.Address.City,
                Country = contact.Address.Country.CommonName,
                contact.Address.PostCode,
                contact.Address.Latitude,
                contact.Address.Longitude,
                AssigngWorkgoup = contact.Workgroup.Name,
                AssignContactGrop = contact.ContactGroup.Name,
            };
        }
    }
}
