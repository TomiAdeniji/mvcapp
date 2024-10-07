using CleanBooksData;
using System.Data.Entity;
using Qbicles.BusinessRules.Model;

namespace Qbicles.BusinessRules
{
    public class AuditRules
    {
        ApplicationDbContext _db;
        public AuditRules()
        {
        }

        public AuditRules(ApplicationDbContext context)
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
        public void audit_user(audit_user au)
        {
            var aus = new audit_user
            {
                changetime=au.changetime,
                changetype=au.changetype,
                id=au.id,
                Note=au.Note,
                User1_id=au.User1_id,
                User2_id=au.User2_id
            };
            _db.audit_users.Add(aus);
            _db.Entry(aus).State = EntityState.Added;
            _db.SaveChanges();
        }
        public void audit_account(audit_account au)
        {
            var aus = new audit_account
            {
                changetime = au.changetime,
                changetype = au.changetype,
                id = au.id,
                Note = au.Note,
                LastBalance = au.LastBalance,
                LastBalance_New = au.LastBalance_New,
                Name=au.Name,
                Name_New=au.Name_New,
                Number=au.Number,
                Number_New=au.Number_New,
                User_id=au.User_id
            };
            _db.audit_accounts.Add(aus);
            _db.Entry(aus).State = EntityState.Added;
            _db.SaveChanges();
        }
        public void audit_task(audit_task au)
        {
            var aus = new audit_task
            {
                changetime = au.changetime,
                changetype = au.changetype,
                id = au.id,
                Note = au.Note,
                CreatedById = au.CreatedById,
                CreatedDate = au.CreatedDate,
                Name = au.Name,
                Name_New = au.Name_New,                
                User_id = au.User_id
            };
            _db.audit_tasks.Add(aus);
            _db.Entry(aus).State = EntityState.Added;
            _db.SaveChanges();
        }
        public void audit_transaction_analysis(audit_transaction_analysis au)
        {
            var aus = new audit_transaction_analysis
            {
                changetime = au.changetime,
                changetype = au.changetype,
                id = au.id,
                Note = au.Note,
                User_id = au.User_id,
                taskId = au.taskId,
                transactionsisTaskId = au.transactionsisTaskId,
                accountId=au.accountId,
                taskInstanceId=au.taskInstanceId
            };
            _db.audit_transaction_analysiss.Add(aus);
            _db.Entry(aus).State = EntityState.Added;
            _db.SaveChanges();
        }
        public void audit_transaction_matching(audit_transaction_matching au)
        {
            var aus = new audit_transaction_matching
            {
                changetime = au.changetime,
                changetype = au.changetype,
                id = au.id,
                Note = au.Note,
                User_id = au.User_id,
                taskId = au.taskId,
                accountId = au.accountId,
                accountId2=au.accountId2,
                transactionMatchingTypeId=au.transactionMatchingTypeId,
                StartDate=au.StartDate,
                EndDate=au.EndDate,
                matchingGroupId=au.matchingGroupId
            };
            _db.audit_transaction_matchings.Add(aus);
            _db.Entry(aus).State = EntityState.Added;
            _db.SaveChanges();
        }
    }
}