using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace e360_clone.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        #region Basic CRUD

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
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Query

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        #endregion

        #region Paged

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PagedResult<T>> GetPagedResultAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalRecords = await query.CountAsync();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion

        #region Search

        public virtual async Task<IEnumerable<T>> SearchAsync(string searchTerm, params string[] searchProperties)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchProperties.Length == 0)
            {
                return await GetAllAsync();
            }

            var parameter = Expression.Parameter(typeof(T), "e");
            var expressions = searchProperties
                .Select(propName => CreateContainsExpression(parameter, propName, searchTerm))
                .OfType<Expression<Func<T, bool>>>();

            var combinedExpression = expressions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(
                null,
                (current, expr) => current == null ? expr : current.Or(expr));

            return combinedExpression != null
                ? await _dbSet.Where(combinedExpression).ToListAsync()
                : await GetAllAsync();
        }

        private static Expression<Func<T, bool>>? CreateContainsExpression(
            ParameterExpression parameter,
            string propertyName,
            string searchTerm)
        {
            var property = Expression.Property(parameter, propertyName);
            if (property.Type != typeof(string))
            {
                return null;
            }

            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            if (containsMethod == null)
            {
                return null;
            }

            var searchTermConstant = Expression.Constant(searchTerm.ToLower());
            var propertyLower = Expression.Call(property, nameof(string.ToLower), null);
            var containsExpression = Expression.Call(propertyLower, containsMethod, searchTermConstant);

            return Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        }

        #endregion

        #region Bulk Operations

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        #endregion
    }

    #region Expression Extensions

    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = first.Parameters[0];
            var visitor = new ParameterReplacer(parameter);

            var firstBody = visitor.Visit(first.Body);
            var secondBody = visitor.Visit(second.Body);

            var orExpression = Expression.OrElse(firstBody, secondBody);
            return Expression.Lambda<Func<T, bool>>(orExpression, parameter);
        }

        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = first.Parameters[0];
            var visitor = new ParameterReplacer(parameter);

            var firstBody = visitor.Visit(first.Body);
            var secondBody = visitor.Visit(second.Body);

            var andExpression = Expression.AndAlso(firstBody, secondBody);
            return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            public ParameterReplacer(ParameterExpression parameter)
            {
                _parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return base.VisitParameter(_parameter);
            }
        }
    }

    #endregion
}
