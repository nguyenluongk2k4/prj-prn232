using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        private readonly StudentDAO _dao;

        public StudentRepository(AppDbContext context, StudentDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<Student?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<Student> AddAsync(Student entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(Student entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Student entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<Student, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<Student>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Student, bool>>? filter = null,
            Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderBy = null)
        {
            IQueryable<Student> query = _dao.Query();

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
