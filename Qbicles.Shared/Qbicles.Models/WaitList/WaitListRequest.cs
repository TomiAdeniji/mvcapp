using Qbicles.Models.Base;
using Qbicles.Models.Qbicles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.WaitList
{
    /*
     n request - 1 right
    new request if IsApprovedForSubsDomain,IsApprovedForCustomDomain,IsRejected = false

    This will be part of the client's process.
They will know who are the users that are to be allowed create Custom Domains.

Remember, if the System Admin approves the user for creating Custom Domains,
the user is also approved for creating Subscription Domains

    It will be the sys admin's job to decide what rights the user should be given.

They can make the decision based on the history or their personal knowledge of the user

     */
    [Table("system_waitlistrequest")]
    public class WaitListRequest : DataModelBase
    {
        [Required]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// [Required]
        /// </summary>
        //public virtual Country Country { get; set; }

        /// <summary>
        /// //CommonName
        /// </summary>
        [Required]
        public string CountryName { get; set; }
        [Required]
        public CountryCode CountryCode { get; set; }

        public virtual List<BusinessCategory> BusinessCategories { get; set; }

        public NumberOfEmployees? NumberOfEmployees { get; set; }

        public DiscoveredVia? DiscoveredVia { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForSubsDomain { get; set; } = false;

        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForCustomDomain { get; set; } = false;

        [Required]
        [Column(TypeName = "bit")]
        public bool IsRejected { get; set; } = false;


        public virtual ApplicationUser ReviewedBy { get; set; } = null;

        public DateTime? ReviewedDate { get; set; } = null;
        public DateTime? LastRequesstdDate { get; set; } = null;

    }




    public enum NumberOfEmployees
    {
        [Description("1-5 employees")]
        Num_1_5 = 0,

        [Description("6-10 employees")]
        Num6_10 = 1,

        [Description("11-50 employees")]
        Num11_50 = 2,

        [Description("Over 50 employees")]
        Over50 = 3
    }

    public enum DiscoveredVia
    {
        [Description("Qbicles.com")]
        QbiclesCom = 0,

        [Description("Advertisements online")]
        AdsOnline = 1,

        [Description("Referred by a friend")]
        RefereedByFriend = 2,

        [Description("Word of mouth")]
        WordOfMount = 3,

        [Description("Other")]
        Other = 4
    }
}
