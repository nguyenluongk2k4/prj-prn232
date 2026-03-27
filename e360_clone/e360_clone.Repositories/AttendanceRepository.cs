using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using e360_clone.BusinessObjects.DTOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        private readonly AttendanceDAO _dao;

        public AttendanceRepository(AppDbContext context, AttendanceDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<Attendance?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<Attendance> AddAsync(Attendance entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(Attendance entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Attendance entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<Attendance, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<Attendance>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Attendance, bool>>? filter = null,
            Func<IQueryable<Attendance>, IOrderedQueryable<Attendance>>? orderBy = null)
        {
            IQueryable<Attendance> query = _dao.Query();

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

        public Task<List<AttendanceRosterItemDto>?> GetRosterAsync(int examId)
        {
            return _dao.GetRosterAsync(examId);
        }

        public Task<List<StudentAttendanceItemDto>> GetStudentAttendanceAsync(int studentId, DateTime? date)
        {
            return _dao.GetStudentAttendanceAsync(studentId, date);
        }

        public Task<List<AttendanceReportItemDto>> GetReportAsync(DateTime? fromDate, DateTime? toDate, int? subjectId, int? classId)
        {
            return _dao.GetReportAsync(fromDate, toDate, subjectId, classId);
        }
    }
}
