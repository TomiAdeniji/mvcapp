using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class QbicleCommentRules
    {
        ApplicationDbContext dbContext;

        public QbicleCommentRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        
    }
}
