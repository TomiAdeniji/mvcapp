using System;
using System.Linq;
using System.Collections.Generic;
using CleanBooksData;
using Qbicles.BusinessRules.Model;

namespace Qbicles.BusinessRules
{
    public class CBProcessRules
    {
        private readonly ApplicationDbContext _db;

        public CBProcessRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();

        public List<CBProcess> GetAll()
        {
            return DbContext.CBProcesses.ToList();
        }

        public CBProcess GetById(int id)
        {
            return DbContext.CBProcesses.Find(id);
        }

    }
}