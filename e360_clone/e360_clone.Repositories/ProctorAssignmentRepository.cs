using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class ProctorAssignmentRepository : Repository<ProctorAssignment>, IProctorAssignmentRepository
    {
        private readonly ProctorAssignmentDAO _dao;

        public ProctorAssignmentRepository(AppDbContext context, ProctorAssignmentDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<ProctorAssignment>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<ProctorAssignment?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<ProctorAssignment> AddAsync(ProctorAssignment entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(ProctorAssignment entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(ProctorAssignment entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<ProctorAssignment, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<ProctorAssignment>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<ProctorAssignment, bool>>? filter = null,
            Func<IQueryable<ProctorAssignment>, IOrderedQueryable<ProctorAssignment>>? orderBy = null)
        {
            IQueryable<ProctorAssignment> query = _dao.Query();

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
