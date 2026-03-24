using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class StudentSubjectRepository : Repository<StudentSubject>, IStudentSubjectRepository
    {
        public StudentSubjectRepository(AppDbContext context) : base(context)
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
