using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Highlight
{
    public class JobListingHighlight : ListingHighlight
    {
        public string Salary { get; set; }
        public DateTime ClosingDate { get; set; }
        public string SkillRequired { get; set; }
    }
}
