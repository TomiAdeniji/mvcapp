using Qbicles.Models.Community;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Highlight
{
    public class NewsHighlight : HighlightPost
    {
        public string NewsCitation { get; set; }
        public string NewsHyperLink { get; set; }
    }
}
