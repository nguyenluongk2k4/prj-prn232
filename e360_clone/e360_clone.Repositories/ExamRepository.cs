using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class ExamRepository : Repository<Exam>, IExamRepository
    {
        private readonly ExamDAO _dao;

        public ExamRepository(AppDbContext context, ExamDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<Exam>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<Exam?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<Exam> AddAsync(Exam entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(Exam entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Exam entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<Exam, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<Exam>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Exam, bool>>? filter = null,
            Func<IQueryable<Exam>, IOrderedQueryable<Exam>>? orderBy = null)
        {
            IQueryable<Exam> query = _dao.Query();

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
