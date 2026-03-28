using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class StudentSubjectDAO : BaseDAO<StudentSubject>
    {
        public StudentSubjectDAO(AppDbContext context) : base(context)
        {
        }

        public async Task<int> GetCurrentTermIdAsync()
        {
            return await _context.Terms
                .Where(t => t.IsCurrent)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(int subjectId, int studentId, int termId)
        {
            return await _dbSet.AnyAsync(x =>
                x.SubjectId == subjectId &&
                x.StudentId == studentId &&
                x.TermId == termId);
        }

        public async Task<int> AddStudentsToSubjectAsync(int subjectId, int classId, List<int> studentIds)
        {
            var termId = await GetCurrentTermIdAsync();
            if (termId <= 0)
            {
                return 0;
            }

            var isAssigned = await _context.TeachingAssignments.AnyAsync(t =>
                t.SubjectId == subjectId &&
                t.ClassId == classId &&
                t.Status == "Active");
            if (!isAssigned)
            {
                return 0;
            }

            var studentsToAssign = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();

            if (studentsToAssign.Count == 0)
            {
                return 0;
            }

            foreach (var student in studentsToAssign)
            {
                if (student.ClassId != classId)
                {
                    student.ClassId = classId;
                    student.UpdatedAt = DateTime.UtcNow;
                }
            }

            var validStudents = studentsToAssign.Select(s => s.Id).ToList();

            if (validStudents.Count == 0)
            {
                return 0;
            }

            var existing = await _dbSet
                .Where(x => x.SubjectId == subjectId && x.TermId == termId && validStudents.Contains(x.StudentId))
                .Select(x => x.StudentId)
                .ToListAsync();

            var toAdd = validStudents.Except(existing).ToList();
            if (toAdd.Count == 0)
            {
                return 0;
            }

            var entities = toAdd.Select(studentId => new StudentSubject
            {
                StudentId = studentId,
                SubjectId = subjectId,
                ClassId = classId,
                TermId = termId,
                Status = "Enrolled",
                AcademicYear = string.Empty,
                Semester = 0,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _dbSet.AddRangeAsync(entities);
            return entities.Count;
        }

        public async Task<string> DiagnoseAddStudentsAsync(int subjectId, int classId, List<int> studentIds)
        {
            var termId = await GetCurrentTermIdAsync();
            if (termId <= 0)
            {
                return "Chưa có kỳ hiện tại.";
            }

            var isAssigned = await _context.TeachingAssignments.AnyAsync(t =>
                t.SubjectId == subjectId &&
                t.ClassId == classId &&
                t.Status == "Active");
            if (!isAssigned)
            {
                return "Lớp chưa được gán môn.";
            }

            var validStudents = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();
            if (validStudents.Count == 0)
            {
                return "Không có sinh viên hợp lệ.";
            }

            var existing = await _dbSet
                .Where(x => x.SubjectId == subjectId && x.TermId == termId && validStudents.Contains(x.StudentId))
                .Select(x => x.StudentId)
                .ToListAsync();

            if (existing.Count == validStudents.Count)
            {
                return "Sinh viên đã có trong môn.";
            }

            return "Không thể thêm sinh viên.";
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
