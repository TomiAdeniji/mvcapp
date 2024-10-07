using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Community
{
    public class HighlightCommunityPage
    {
        public int Id {get; set;}

        public ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public CommunityPage Page { get; set; }
    }
}
