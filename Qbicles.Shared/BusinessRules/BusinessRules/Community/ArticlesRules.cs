using System.Linq;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Community;

namespace Qbicles.BusinessRules.BusinessRules.Community
{
    public class ArticlesRules
    {
        private ApplicationDbContext _db;

        public ArticlesRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }

        public Article GetArticlesById(int id)
        {
            return DbContext.Articles.FirstOrDefault(d => d.Id == id);
        }
    }
}