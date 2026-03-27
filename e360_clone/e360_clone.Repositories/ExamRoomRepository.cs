using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class ExamRoomRepository : Repository<ExamRoom>, IExamRoomRepository
    {
        private readonly ExamRoomDAO _dao;

        public ExamRoomRepository(AppDbContext context, ExamRoomDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<ExamRoom>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<ExamRoom?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<ExamRoom> AddAsync(ExamRoom entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(ExamRoom entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(ExamRoom entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<ExamRoom, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<ExamRoom>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<ExamRoom, bool>>? filter = null,
            Func<IQueryable<ExamRoom>, IOrderedQueryable<ExamRoom>>? orderBy = null)
        {
            IQueryable<ExamRoom> query = _dao.Query();

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

        public Task<List<ExamRoom>> GetAvailableRoomsAsync(DateTime examDate, TimeSpan startTime, TimeSpan endTime, int? excludeExamId)
        {
            return _dao.GetAvailableRoomsAsync(examDate, startTime, endTime, excludeExamId);
        }
    }
}
