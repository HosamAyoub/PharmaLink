using Microsoft.EntityFrameworkCore.Storage;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Repository
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        private readonly ApplicationDbContext _db;

        public AccountRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public Task<IDbContextTransaction> StartTransactionAsync()
        {
            return _db.Database.BeginTransactionAsync();
        }
        public Task EndTransactionAsync()
        {
            return _db.Database.CommitTransactionAsync();
        }
    }
}
