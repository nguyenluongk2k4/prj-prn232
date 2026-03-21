using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    /// <summary>
    /// Data Access Object for Account entity
    /// Handles direct database operations for Accounts table
    /// </summary>
    public class AccountDAO
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Account> _dbSet;

        public AccountDAO(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Account>();
        }

        /// <summary>
        /// Find account by email or username
        /// </summary>
        public async Task<Account?> FindByEmailOrUsernameAsync(string emailOrUsername)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Email == emailOrUsername || a.Username == emailOrUsername);
        }

        /// <summary>
        /// Find account by email
        /// </summary>
        public async Task<Account?> FindByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.Email == email);
        }

        /// <summary>
        /// Find account by username
        /// </summary>
        public async Task<Account?> FindByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.Username == username);
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(a => a.Email == email);
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbSet.AnyAsync(a => a.Username == username);
        }

        /// <summary>
        /// Get accounts by role
        /// </summary>
        public async Task<IEnumerable<Account>> GetByRoleAsync(string role)
        {
            return await _dbSet.Where(a => a.Role == role).ToListAsync();
        }

        /// <summary>
        /// Get active accounts only
        /// </summary>
        public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
        {
            return await _dbSet.Where(a => a.Status == "Active").ToListAsync();
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Add new account
        /// </summary>
        public async Task<Account> AddAsync(Account account)
        {
            await _dbSet.AddAsync(account);
            return account;
        }

        /// <summary>
        /// Update account
        /// </summary>
        public void Update(Account account)
        {
            _dbSet.Update(account);
        }

        /// <summary>
        /// Delete account
        /// </summary>
        public void Remove(Account account)
        {
            _dbSet.Remove(account);
        }

        /// <summary>
        /// Save changes
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update last login timestamp
        /// </summary>
        public async Task UpdateLastLoginAsync(int accountId)
        {
            var account = await GetByIdAsync(accountId);
            if (account != null)
            {
                account.LastLoginAt = DateTime.UtcNow;  // Use UTC instead of Local
                Update(account);
                await SaveChangesAsync();
            }
        }

        /// <summary>
        /// Change password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int accountId, string newPasswordHash)
        {
            var account = await GetByIdAsync(accountId);
            if (account != null)
            {
                account.PasswordHash = newPasswordHash;
                account.UpdatedAt = DateTime.Now;
                Update(account);
                await SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lock account
        /// </summary>
        public async Task LockAccountAsync(int accountId)
        {
            var account = await GetByIdAsync(accountId);
            if (account != null)
            {
                account.Status = "Locked";
                account.UpdatedAt = DateTime.Now;
                Update(account);
                await SaveChangesAsync();
            }
        }

        /// <summary>
        /// Unlock account
        /// </summary>
        public async Task UnlockAccountAsync(int accountId)
        {
            var account = await GetByIdAsync(accountId);
            if (account != null)
            {
                account.Status = "Active";
                account.UpdatedAt = DateTime.Now;
                Update(account);
                await SaveChangesAsync();
            }
        }
    }
}
