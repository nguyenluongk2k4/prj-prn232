using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class TeachingAssignmentRepository : Repository<TeachingAssignment>, ITeachingAssignmentRepository
    {
        public TeachingAssignmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsForLecturerAndExamAsync(int lecturerId, Exam exam)
        {
            if (exam == null) return false;

            return await _dbSet.AnyAsync(x =>
                x.LecturerId == lecturerId &&
                x.SubjectId == exam.SubjectId &&
                x.ClassId == exam.ClassId &&
                (string.IsNullOrEmpty(x.AcademicYear) || x.AcademicYear == exam.AcademicYear) &&
                (string.IsNullOrEmpty(x.Semester) || x.Semester == exam.Semester) &&
                x.Status == "Active");
        }
    }
}
