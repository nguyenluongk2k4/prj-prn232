using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class StudentSubjectDAO : BaseDAO<StudentSubject>
    {
        public StudentSubjectDAO(AppDbContext context) : base(context)
        {
        }

        public async Task<List<int>> GetClassIdsBySubjectAsync(int subjectId)
        {
            return await _dbSet
                .Where(x => x.SubjectId == subjectId && x.ClassId.HasValue)
                .Select(x => x.ClassId!.Value)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<StudentSubject>> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .Include(x => x.Subject)
                .Include(x => x.Class)
                .Where(x => x.StudentId == studentId)
                .OrderBy(x => x.Subject != null ? x.Subject.SubjectCode : string.Empty)
                .ToListAsync();
        }
    }
}
