using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.AccountDTO;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<Account> _userManager;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        //private readonly AccountDTO newUser;
        public AccountRepository(UserManager<Account> userManager, IMapper mapper, ApplicationDbContext db)
        {
            _userManager = userManager;
            _mapper = mapper;
            _db = db;
            //newUser = new AccountDTO();
        }
        public async Task<IdentityResult> RegisterAsync(AccountDTO account)
        {
            if (account == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User cannot be null" });
            }
            var transaction = await _db.Database.BeginTransactionAsync();
            Account newAccount = _mapper.Map<Account>(account);
            IdentityResult result = await _userManager.CreateAsync(newAccount, account.PasswordHash);

            if (!result.Succeeded)
            {
                return result;
            }

            if (account.User != null)
            {
                account.User.AccountId = newAccount.Id;
                newAccount.User = _mapper.Map<User>(account.User);
                await _db.Users.AddAsync(newAccount.User);
            }
            else if (account.Pharmacy != null)
            {
                account.Pharmacy.AccountId = newAccount.Id;
                newAccount.Pharmacy = _mapper.Map<Pharmacy>(account.Pharmacy);
                await _db.Pharmacies.AddAsync(newAccount.Pharmacy);
            }
            
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return result;

        }
    }
}
