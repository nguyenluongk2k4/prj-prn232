using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class LecturerRepository : Repository<Lecturer>, ILecturerRepository
    {
        private readonly LecturerDAO _dao;

        public LecturerRepository(AppDbContext context, LecturerDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<Lecturer>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<Lecturer?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<Lecturer> AddAsync(Lecturer entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(Lecturer entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Lecturer entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<Lecturer, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<Lecturer>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Lecturer, bool>>? filter = null,
            Func<IQueryable<Lecturer>, IOrderedQueryable<Lecturer>>? orderBy = null)
        {
            IQueryable<Lecturer> query = _dao.Query();

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
