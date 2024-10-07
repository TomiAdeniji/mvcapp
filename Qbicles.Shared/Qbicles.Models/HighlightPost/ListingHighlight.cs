using Qbicles.Models.Qbicles;
using System.Collections.Generic;
using System.ComponentModel;

namespace Qbicles.Models.Highlight
{
    public class ListingHighlight : HighlightPost
    {
        public virtual Country Country { get; set; }
        public ListingType ListingHighlightType { get; set; }
        public string Reference { get; set; }
        public virtual List<ApplicationUser> FlaggedBy { get; set; } = new List<ApplicationUser>();
        public virtual HighlightLocation ListingLocation { get; set; }
    }

    public enum ListingType
    {
        [Description("None")]
        None = 0,
        [Description("Event")]
        Event = 1,
        [Description("Job")]
        Job = 2,
        [Description("Real Estate")]
        RealEstate = 3
    }
}
