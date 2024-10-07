using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules
{
    public class QbicleFileRules
    {
        ApplicationDbContext dbContext;

        public QbicleFileRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
    }
}
