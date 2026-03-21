using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class GradeRepository : Repository<Grade>, IGradeRepository
    {
        private readonly GradeDAO _dao;

        public GradeRepository(AppDbContext context, GradeDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<Grade>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<Grade?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<Grade> AddAsync(Grade entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(Grade entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Grade entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<Grade, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<Grade>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Grade, bool>>? filter = null,
            Func<IQueryable<Grade>, IOrderedQueryable<Grade>>? orderBy = null)
        {
            IQueryable<Grade> query = _dao.Query();

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
