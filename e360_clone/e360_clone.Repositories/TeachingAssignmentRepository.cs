using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class TeachingAssignmentRepository : Repository<TeachingAssignment>, ITeachingAssignmentRepository
    {
        private readonly TeachingAssignmentDAO _dao;

        public TeachingAssignmentRepository(AppDbContext context, TeachingAssignmentDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<TeachingAssignment>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<TeachingAssignment?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<TeachingAssignment> AddAsync(TeachingAssignment entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(TeachingAssignment entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(TeachingAssignment entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<TeachingAssignment, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<TeachingAssignment>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TeachingAssignment, bool>>? filter = null,
            Func<IQueryable<TeachingAssignment>, IOrderedQueryable<TeachingAssignment>>? orderBy = null)
        {
            IQueryable<TeachingAssignment> query = _dao.Query();

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

        public async Task<bool> ExistsForLecturerAndExamAsync(int lecturerId, Exam exam)
        {
            if (exam == null) return false;

            return await _dao.Query().AnyAsync(x =>
                x.LecturerId == lecturerId &&
                x.SubjectId == exam.SubjectId &&
                x.ClassId == exam.ClassId &&
                (string.IsNullOrEmpty(x.AcademicYear) || x.AcademicYear == exam.AcademicYear) &&
                (string.IsNullOrEmpty(x.Semester) || x.Semester == exam.Semester) &&
                x.Status == "Active");
        }
    }
}
