using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class SubjectRepository : Repository<Subject>, ISubjectRepository
    {
        private readonly SubjectDAO _dao;

        public SubjectRepository(AppDbContext context, SubjectDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<Subject>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<Subject?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<Subject> AddAsync(Subject entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(Subject entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Subject entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<Subject, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<Subject>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Subject, bool>>? filter = null,
            Func<IQueryable<Subject>, IOrderedQueryable<Subject>>? orderBy = null)
        {
            IQueryable<Subject> query = _dao.Query();

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
