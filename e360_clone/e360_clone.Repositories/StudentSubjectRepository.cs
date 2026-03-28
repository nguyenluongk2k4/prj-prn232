using System.Linq.Expressions;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class StudentSubjectRepository : Repository<StudentSubject>, IStudentSubjectRepository
    {
        private readonly StudentSubjectDAO _dao;

        public StudentSubjectRepository(AppDbContext context, StudentSubjectDAO dao) : base(context)
        {
            _dao = dao;
        }

        public override async Task<IEnumerable<StudentSubject>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public override async Task<StudentSubject?> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public override async Task<StudentSubject> AddAsync(StudentSubject entity)
        {
            await _dao.AddAsync(entity);
            await _dao.SaveChangesAsync();
            return entity;
        }

        public override async Task UpdateAsync(StudentSubject entity)
        {
            _dao.Update(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task DeleteAsync(StudentSubject entity)
        {
            _dao.Remove(entity);
            await _dao.SaveChangesAsync();
        }

        public override async Task<int> CountAsync()
        {
            return await _dao.Query().CountAsync();
        }

        public override async Task<int> CountAsync(Expression<Func<StudentSubject, bool>> predicate)
        {
            return await _dao.Query().CountAsync(predicate);
        }

        public override async Task<IEnumerable<StudentSubject>> FindAsync(Expression<Func<StudentSubject, bool>> predicate)
        {
            return await _dao.Query().Where(predicate).ToListAsync();
        }

        public override async Task<StudentSubject?> FirstOrDefaultAsync(Expression<Func<StudentSubject, bool>> predicate)
        {
            return await _dao.Query().FirstOrDefaultAsync(predicate);
        }

        public override async Task<bool> AnyAsync(Expression<Func<StudentSubject, bool>> predicate)
        {
            return await _dao.Query().AnyAsync(predicate);
        }

        public override async Task<IEnumerable<StudentSubject>> GetPagedFilteredAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<StudentSubject, bool>>? filter = null,
            Func<IQueryable<StudentSubject>, IOrderedQueryable<StudentSubject>>? orderBy = null)
        {
            IQueryable<StudentSubject> query = _dao.Query();

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

        public async Task<List<int>> GetClassIdsBySubjectAsync(int subjectId)
        {
            return await _dao.GetClassIdsBySubjectAsync(subjectId);
        }

        public async Task<List<StudentSubject>> GetByStudentIdAsync(int studentId)
        {
            return await _dao.GetByStudentIdAsync(studentId);
        }

        public async Task<int> AddStudentsToSubjectAsync(int subjectId, int classId, List<int> studentIds)
        {
            var added = await _dao.AddStudentsToSubjectAsync(subjectId, classId, studentIds);
            if (added == 0)
            {
                return 0;
            }
            await _dao.SaveChangesAsync();
            return added;
        }

        public Task<string> DiagnoseAddStudentsAsync(int subjectId, int classId, List<int> studentIds)
        {
            return _dao.DiagnoseAddStudentsAsync(subjectId, classId, studentIds);
        }
    }
}
