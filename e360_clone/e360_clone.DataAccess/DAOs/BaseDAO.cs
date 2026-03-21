using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    /// <summary>
    /// Base interface for all DAOs
    /// </summary>
    public interface IBaseDAO<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<int> SaveChangesAsync();
    }

    /// <summary>
    /// Base implementation for all DAOs
    /// </summary>
    public abstract class BaseDAO<T> : IBaseDAO<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseDAO(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
