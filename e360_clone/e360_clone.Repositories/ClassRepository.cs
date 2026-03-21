using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class ClassRepository : Repository<Class>, IClassRepository
    {
        private readonly ClassDAO _dao;

        public ClassRepository(AppDbContext context, ClassDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<Class>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<Class?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<Class> AddAsync(Class entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(Class entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Class entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<Class, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<Class>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Class, bool>>? filter = null,
            Func<IQueryable<Class>, IOrderedQueryable<Class>>? orderBy = null)
        {
            IQueryable<Class> query = _dao.Query();

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
    }
}
