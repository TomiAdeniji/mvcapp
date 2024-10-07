using System.Collections.Generic;
using Newtonsoft.Json;

namespace Qbicles.Models.TraderApi
{

    public class IPosResult
    {
        public bool IsTokenValid { get; set; }
        public System.Net.HttpStatusCode Status { get; set; }
        public string Message { get; set; }
        public string TraderId { get; set; }

    }


    public class UserInformation
    {
        public string TraderId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public string Profile { get; set; }
        public string Timezone { get; set; }
        public string DisplayUserName { get; set; }
    }

    public class PosUserInfomation: UserInformation
    {
        public int? PIN { get; set; }
        public bool TillManager { get; set; }
        public bool TillCashier { get; set; }
        public bool TillSupervisor { get; set; }
        public bool TillUser { get; set; }
    }
    public class PosDeviceForUser
    {
        public List<PosDeviceResult> Devices { get; set; }
    }

    public class PosDeviceResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Menu { get; set; }
        public string SerialNumber { get; set; }
        public string Location { get; set; }
        public string Domain { get; set; }
    }

    public class PosContacts
    {
        public List<PosTraderContact> Contacts { get; set; }
    }

    public class PosTraderContact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarUri { get; set; }
        public string ContactRef { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public PosContactAddress Address { get; set; }
    }

    public class PosContactAddress
    {
        [JsonIgnore]
        public string Street { get; set; }
        public string City { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }

    }
}
