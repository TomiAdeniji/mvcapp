using Qbicles.Models.Community;
using Qbicles.Models.Qbicles;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Highlight
{
    public class KnowledgeHighlight : HighlightPost
    {
        public virtual Country Country { get; set; }
        public string KnowledgeCitation { get; set; }
        public string KnowledgeHyperlink { get; set; }
    }
}
