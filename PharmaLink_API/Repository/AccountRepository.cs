using Microsoft.EntityFrameworkCore.Storage;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Repository
{
    // Repository for managing Account entities and related database operations
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        private readonly ApplicationDbContext _db;

        // Constructor injects the application's DbContext
        public AccountRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Starts a new database transaction for account-related operations.
        /// </summary>
        /// <returns>IDbContextTransaction object for controlling the transaction.</returns>
        public Task<IDbContextTransaction> StartTransactionAsync()
        {
            return _db.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        /// <returns>A task representing the commit operation.</returns>
        public Task EndTransactionAsync()
        {
            return _db.Database.CommitTransactionAsync();
        }
    }
}
