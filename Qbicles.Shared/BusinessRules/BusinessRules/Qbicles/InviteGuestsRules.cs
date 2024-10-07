using Qbicles.BusinessRules.Model;

namespace Qbicles.BusinessRules
{
    class InviteGuestsRules
    {

        ApplicationDbContext _db;
        public InviteGuestsRules()
        {

        }
        public InviteGuestsRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }
        

    }
}
