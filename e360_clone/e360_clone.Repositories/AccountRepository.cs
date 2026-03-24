using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;

namespace e360_clone.Repositories
{
    /// <summary>
    /// Repository for Account operations
    /// Uses AccountDAO for data access
    /// </summary>
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        private readonly AccountDAO _accountDao;

        public AccountRepository(AppDbContext context, AccountDAO accountDao) : base(context)
        {
            _accountDao = accountDao;
        }

        public async Task<Account?> FindByEmailOrUsernameAsync(string emailOrUsername)
        {
            return await _accountDao.FindByEmailOrUsernameAsync(emailOrUsername);
        }

        public async Task<Account?> FindByEmailAsync(string email)
        {
            return await _accountDao.FindByEmailAsync(email);
        }

        public async Task<Account?> FindByUsernameAsync(string username)
        {
            return await _accountDao.FindByUsernameAsync(username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _accountDao.EmailExistsAsync(email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _accountDao.UsernameExistsAsync(username);
        }

        public async Task<IEnumerable<Account>> GetByRoleAsync(string role)
        {
            return await _accountDao.GetByRoleAsync(role);
        }

        public async Task<IEnumerable<Account>> GetByStudentIdsAsync(IEnumerable<int> studentIds)
        {
            return await _accountDao.GetByStudentIdsAsync(studentIds);
        }

        public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
        {
            return await _accountDao.GetActiveAccountsAsync();
        }

        public async Task UpdateLastLoginAsync(int accountId)
        {
            await _accountDao.UpdateLastLoginAsync(accountId);
        }

        public async Task<bool> ChangePasswordAsync(int accountId, string newPasswordHash)
        {
            return await _accountDao.ChangePasswordAsync(accountId, newPasswordHash);
        }

        public async Task LockAccountAsync(int accountId)
        {
            await _accountDao.LockAccountAsync(accountId);
        }

        public async Task UnlockAccountAsync(int accountId)
        {
            await _accountDao.UnlockAccountAsync(accountId);
        }
    }
}
