using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    /// <summary>
    /// Interface for Account repository
    /// Extends IRepository with account-specific methods
    /// </summary>
    public interface IAccountRepository : IRepository<Account>
    {
        /// <summary>
        /// Find account by email or username
        /// </summary>
        Task<Account?> FindByEmailOrUsernameAsync(string emailOrUsername);

        /// <summary>
        /// Find account by email
        /// </summary>
        Task<Account?> FindByEmailAsync(string email);

        /// <summary>
        /// Find account by username
        /// </summary>
        Task<Account?> FindByUsernameAsync(string username);

        /// <summary>
        /// Check if email exists
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Check if username exists
        /// </summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Get accounts by role
        /// </summary>
        Task<IEnumerable<Account>> GetByRoleAsync(string role);

        /// <summary>
        /// Get accounts by student IDs
        /// </summary>
        Task<IEnumerable<Account>> GetByStudentIdsAsync(IEnumerable<int> studentIds);

        /// <summary>
        /// Get active accounts only
        /// </summary>
        Task<IEnumerable<Account>> GetActiveAccountsAsync();

        /// <summary>
        /// Update last login timestamp
        /// </summary>
        Task UpdateLastLoginAsync(int accountId);

        /// <summary>
        /// Change password
        /// </summary>
        Task<bool> ChangePasswordAsync(int accountId, string newPasswordHash);

        /// <summary>
        /// Lock account
        /// </summary>
        Task LockAccountAsync(int accountId);

        /// <summary>
        /// Unlock account
        /// </summary>
        Task UnlockAccountAsync(int accountId);
    }
}
