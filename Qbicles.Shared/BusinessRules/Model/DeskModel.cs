using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Model
{
    public class DiscussionModel
    {
        public bool Status { get; set;}
        public QbicleDiscussion Discussion { get; set; }
    }

    public class QbicleEventModel
    {
        public bool Status { get; set; }
        public QbicleEvent Event { get; set; }
    }

    public class QbicleProcessModel
    {
        public bool Status { get; set; }
        public ApprovalReq Approval { get; set; }
    }
}
